-- =====================================================================
-- LiPi :: clinic :: 02_identity
-- Clinic-scoped user identity, RBAC, sessions, MFA, API keys, AD sync.
-- Platform (cross-clinic) users live in master.global_users.
-- Prereq: 00_common + clinic/01_core applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS identity;
SET search_path = identity, core, public;

-- ---------------------------------------------------------------------
-- USERS — every clinic user (doctor, nurse, admin) has a row here.
-- May be bound to a staff member (staff_id) OR a system-principal (service_accounts).
-- ---------------------------------------------------------------------
CREATE TABLE identity.users (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    username          citext      NOT NULL,                     -- LDAP sAMAccountName or email local-part
    email             core.d_email,
    phone             core.d_phone_in,
    staff_id          uuid        UNIQUE REFERENCES core.staff(id),  -- NULL for service accts
    password_hash     text,                                      -- Argon2id; NULL if SSO-only
    password_set_at   timestamptz,
    must_change_password boolean  NOT NULL DEFAULT false,
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('invited','active','suspended','locked','terminated')),
    is_ad_managed     boolean     NOT NULL DEFAULT false,        -- true → account sourced from AD/LDAP, don't allow local password change
    ad_object_guid    text,                                      -- AD objectGUID for re-sync stability
    ad_dn             text,                                      -- distinguished name
    last_login_at     timestamptz,
    last_login_ip     inet,
    failed_login_count int        NOT NULL DEFAULT 0,
    locked_until      timestamptz,
    is_mfa_enforced   boolean     NOT NULL DEFAULT true,
    preferred_locale  text        DEFAULT 'en-IN',
    timezone          text        DEFAULT 'Asia/Kolkata',
    extension_data    jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    updated_by        uuid,
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_users_clinic_username UNIQUE (clinic_id, username)
);

CREATE UNIQUE INDEX uq_users_clinic_email ON identity.users(clinic_id, email) WHERE email IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX ix_users_status             ON identity.users(clinic_id, status) WHERE deleted_at IS NULL;
CREATE INDEX ix_users_ad_guid            ON identity.users(ad_object_guid)    WHERE ad_object_guid IS NOT NULL;
CREATE INDEX ix_users_staff              ON identity.users(staff_id)          WHERE staff_id IS NOT NULL;

-- ---------------------------------------------------------------------
-- ROLES & PERMISSIONS
-- ---------------------------------------------------------------------
CREATE TABLE identity.roles (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    code              text        NOT NULL,                      -- e.g. 'oncologist','rt_physicist','opd_nurse'
    name              text        NOT NULL,
    description       text,
    is_system_role    boolean     NOT NULL DEFAULT false,         -- shipped by LiPi, cannot be renamed
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_roles_clinic_code UNIQUE (clinic_id, code)
);

-- Permissions are namespaced by module. Granted to roles.
CREATE TABLE identity.permissions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    permission_code   text        NOT NULL UNIQUE,                -- e.g. 'opd.encounter.create', 'rtqa.approve', 'compliance.license.edit'
    module            text        NOT NULL,                       -- 'opd' | 'ipd' | 'rtqa' | 'compliance' | ...
    action            text        NOT NULL,                       -- 'read' | 'create' | 'update' | 'delete' | 'approve' | 'sign' | 'export'
    description       text,
    is_phi_sensitive  boolean     NOT NULL DEFAULT false          -- if true, emits phi_access_log entry on use
);

CREATE TABLE identity.role_permissions (
    role_id           uuid        NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    permission_id     uuid        NOT NULL REFERENCES identity.permissions(id) ON DELETE CASCADE,
    granted_at        timestamptz NOT NULL DEFAULT now(),
    granted_by        uuid,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE identity.user_roles (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    user_id           uuid        NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    role_id           uuid        NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    scope_department_id uuid      REFERENCES core.departments(id),    -- optional: role scoped to a dept
    assigned_at       timestamptz NOT NULL DEFAULT now(),
    assigned_by       uuid,
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz
);
-- Prevent duplicate (user,role) when no department scope
CREATE UNIQUE INDEX uq_user_roles_global ON identity.user_roles(user_id, role_id) WHERE scope_department_id IS NULL;
-- Prevent duplicate (user,role,dept) when scoped
CREATE UNIQUE INDEX uq_user_roles_scoped ON identity.user_roles(user_id, role_id, scope_department_id) WHERE scope_department_id IS NOT NULL;

-- ---------------------------------------------------------------------
-- SESSIONS — short-lived JWT + refresh-token state
-- ---------------------------------------------------------------------
CREATE TABLE identity.sessions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    user_id           uuid        NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    refresh_token_hash bytea      NOT NULL,                       -- SHA-256 of refresh token (never store raw)
    jwt_jti           uuid        NOT NULL,                       -- JWT id — used for revocation check
    client_ip         inet,
    user_agent        text,
    device_fingerprint text,
    issued_at         timestamptz NOT NULL DEFAULT now(),
    expires_at        timestamptz NOT NULL,
    last_active_at    timestamptz NOT NULL DEFAULT now(),
    revoked_at        timestamptz,
    revocation_reason text
);
CREATE INDEX ix_sessions_user        ON identity.sessions(user_id) WHERE revoked_at IS NULL;
CREATE INDEX ix_sessions_jti         ON identity.sessions(jwt_jti);
CREATE INDEX ix_sessions_expires     ON identity.sessions(expires_at) WHERE revoked_at IS NULL;

-- ---------------------------------------------------------------------
-- MFA METHODS — TOTP, WebAuthn, SMS OTP, email OTP
-- ---------------------------------------------------------------------
CREATE TABLE identity.mfa_methods (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    user_id           uuid        NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    method_type       text        NOT NULL CHECK (method_type IN ('totp','webauthn','sms_otp','email_otp','backup_codes')),
    label             text,                                       -- e.g. 'iPhone 15', 'YubiKey 5C'
    secret_encrypted  bytea,                                      -- envelope-encrypted via KMS
    webauthn_credential_id bytea,
    webauthn_public_key bytea,
    webauthn_counter  bigint,
    is_primary        boolean     NOT NULL DEFAULT false,
    is_verified       boolean     NOT NULL DEFAULT false,
    created_at        timestamptz NOT NULL DEFAULT now(),
    last_used_at      timestamptz,
    revoked_at        timestamptz
);
CREATE INDEX ix_mfa_user ON identity.mfa_methods(user_id) WHERE revoked_at IS NULL;

CREATE TABLE identity.password_history (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    user_id           uuid        NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    password_hash     text        NOT NULL,
    set_at            timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_pwd_history_user ON identity.password_history(user_id, set_at DESC);

-- ---------------------------------------------------------------------
-- LOGIN ATTEMPTS (successful + failed — feeds anomaly detection)
-- ---------------------------------------------------------------------
CREATE TABLE identity.login_attempts (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    attempted_at      timestamptz NOT NULL DEFAULT now(),
    username          citext,
    user_id           uuid        REFERENCES identity.users(id),
    client_ip         inet,
    user_agent        text,
    auth_method       text        CHECK (auth_method IN ('password','ad','saml','oidc','api_key','service','mfa')),
    outcome           text        NOT NULL CHECK (outcome IN ('success','bad_password','unknown_user','mfa_failed','locked','disabled','expired','rate_limited','suspicious')),
    failure_detail    text,
    geo_country       char(2),
    geo_city          text,
    PRIMARY KEY (id, attempted_at)
) PARTITION BY RANGE (attempted_at);

CREATE INDEX ix_login_attempts_user     ON identity.login_attempts(user_id, attempted_at DESC);
CREATE INDEX ix_login_attempts_ip       ON identity.login_attempts(client_ip, attempted_at DESC);
CREATE INDEX ix_login_attempts_outcome  ON identity.login_attempts(clinic_id, outcome, attempted_at DESC);

CREATE TABLE identity.login_attempts_2026_04 PARTITION OF identity.login_attempts
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE identity.login_attempts_2026_05 PARTITION OF identity.login_attempts
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- ---------------------------------------------------------------------
-- API KEYS & SERVICE ACCOUNTS (for integrations: lab analyser, modality bridge, etc.)
-- ---------------------------------------------------------------------
CREATE TABLE identity.service_accounts (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    name              text        NOT NULL,
    description       text,
    user_id           uuid        UNIQUE REFERENCES identity.users(id),  -- service acct is backed by a user row
    purpose           text        NOT NULL CHECK (purpose IN
                                   ('modality_bridge','lab_analyser','esapi_sync','abdm_gateway','hl7_ingest',
                                    'fhir_ingest','sms_gateway','email_gateway','billing_gateway','dicom_router','etl','other')),
    allowed_ip_cidrs  inet[]      NOT NULL DEFAULT '{}',
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,

    CONSTRAINT uq_service_accounts_name UNIQUE (clinic_id, name)
);

CREATE TABLE identity.api_keys (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    service_account_id uuid       NOT NULL REFERENCES identity.service_accounts(id) ON DELETE CASCADE,
    key_prefix        text        NOT NULL,                       -- first 8 chars for display / lookup
    key_hash          bytea       NOT NULL,                       -- SHA-256(key)
    scopes            text[]      NOT NULL DEFAULT '{}',          -- list of permission_codes
    expires_at        timestamptz,
    last_used_at      timestamptz,
    last_used_ip      inet,
    revoked_at        timestamptz,
    revocation_reason text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,

    CONSTRAINT uq_api_key_prefix UNIQUE (key_prefix)
);
CREATE INDEX ix_api_keys_service ON identity.api_keys(service_account_id) WHERE revoked_at IS NULL;

-- ---------------------------------------------------------------------
-- AD / LDAP SYNC STATE
-- ---------------------------------------------------------------------
CREATE TABLE identity.ad_sync_runs (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    identity_provider_id uuid     NOT NULL,                       -- FK to master.identity_providers (cross-db — enforced by app)
    started_at        timestamptz NOT NULL DEFAULT now(),
    finished_at       timestamptz,
    status            text        NOT NULL CHECK (status IN ('running','success','failed','partial')),
    users_created     int         NOT NULL DEFAULT 0,
    users_updated     int         NOT NULL DEFAULT 0,
    users_disabled    int         NOT NULL DEFAULT 0,
    users_unchanged   int         NOT NULL DEFAULT 0,
    error_count       int         NOT NULL DEFAULT 0,
    error_details     jsonb       NOT NULL DEFAULT '[]'::jsonb,
    high_water_mark   text                                         -- last processed AD uSNChanged
);
CREATE INDEX ix_ad_sync_runs_idp ON identity.ad_sync_runs(identity_provider_id, started_at DESC);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='identity'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('permissions')
    LOOP
        EXECUTE format('ALTER TABLE identity.%I ENABLE ROW LEVEL SECURITY', t);
        BEGIN
            EXECUTE format(
              'CREATE POLICY p_%1$s_tenant ON identity.%1$I
               USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
               WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
        EXCEPTION WHEN undefined_column THEN NULL;    -- tables without clinic_id: skip
        END;
    END LOOP;
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='identity'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('password_history','login_attempts','role_permissions','user_roles')
    LOOP
        PERFORM core.attach_standard_triggers('identity', t);
    END LOOP;
END $$;
