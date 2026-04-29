-- =====================================================================
-- LiPi :: 00_common :: 001_extensions
-- Run FIRST on every database (master + per-clinic).
-- =====================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";      -- gen_random_uuid, digest(), hmac()
CREATE EXTENSION IF NOT EXISTS "btree_gist";    -- range exclusion constraints (appointment overlap, bed occupancy)
CREATE EXTENSION IF NOT EXISTS "pg_trgm";       -- fuzzy name / MRN search
CREATE EXTENSION IF NOT EXISTS "unaccent";      -- accent-insensitive search for Indian names
CREATE EXTENSION IF NOT EXISTS "citext";        -- case-insensitive TEXT (emails, usernames)
CREATE EXTENSION IF NOT EXISTS "postgres_fdw";  -- clinic-to-warehouse FDW links

-- =====================================================================
-- LiPi :: 00_common :: 002_uuid_v7
-- UUIDv7 generator (48-bit Unix-ms timestamp + 74 random bits + version/variant).
-- Time-ordered UUIDs keep B-tree indexes compact and reduce WAL churn on high-volume inserts.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS core;

CREATE OR REPLACE FUNCTION core.uuid_v7()
RETURNS uuid
LANGUAGE plpgsql
VOLATILE
AS $$
DECLARE
    v_time_ms  bigint;
    v_rand     bytea;
    v_bytes    bytea;
BEGIN
    v_time_ms := (extract(epoch from clock_timestamp()) * 1000)::bigint;
    v_rand    := gen_random_bytes(10);

    -- 6 bytes timestamp (big-endian ms) || 10 bytes random
    v_bytes := set_byte(
                set_byte(
                 set_byte(
                  set_byte(
                   set_byte(
                    set_byte(v_rand, 0, ((v_time_ms >> 40) & 255)::int),
                            0, ((v_time_ms >> 40) & 255)::int),
                           0, ((v_time_ms >> 40) & 255)::int),
                          0, ((v_time_ms >> 40) & 255)::int),
                         0, ((v_time_ms >> 40) & 255)::int),
                        0, ((v_time_ms >> 40) & 255)::int);

    -- clearer re-build: allocate 16 bytes explicitly
    v_bytes := decode(lpad(to_hex(v_time_ms), 12, '0') || encode(v_rand, 'hex'), 'hex');

    -- version = 7  (top 4 bits of byte 6)
    v_bytes := set_byte(v_bytes, 6, ((get_byte(v_bytes, 6) & 15) | 112));
    -- variant = RFC 4122 (top 2 bits of byte 8 = 10)
    v_bytes := set_byte(v_bytes, 8, ((get_byte(v_bytes, 8) & 63) | 128));

    RETURN encode(v_bytes, 'hex')::uuid;
END;
$$;

COMMENT ON FUNCTION core.uuid_v7() IS
  'RFC 9562 UUIDv7. Time-ordered; use as DEFAULT for every primary key across LiPi.';

-- =====================================================================
-- LiPi :: 00_common :: 004_reference_domains
-- Shared DOMAIN types for common constrained values. Used across schemas.
-- =====================================================================

CREATE DOMAIN core.d_email        AS citext  CHECK (VALUE ~ '^[^@\s]+@[^@\s]+\.[^@\s]+$');
CREATE DOMAIN core.d_phone_in     AS text    CHECK (VALUE ~ '^\+?[0-9][0-9\-\s]{5,19}$');
CREATE DOMAIN core.d_aadhaar      AS text    CHECK (VALUE ~ '^[0-9]{12}$');
CREATE DOMAIN core.d_abha         AS text    CHECK (VALUE ~ '^[0-9]{2}-[0-9]{4}-[0-9]{4}-[0-9]{4}$');
CREATE DOMAIN core.d_pan          AS text    CHECK (VALUE ~ '^[A-Z]{5}[0-9]{4}[A-Z]$');
CREATE DOMAIN core.d_gstin        AS text    CHECK (VALUE ~ '^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][0-9][A-Z][0-9A-Z]$');
CREATE DOMAIN core.d_pincode_in   AS text    CHECK (VALUE ~ '^[1-9][0-9]{5}$');
CREATE DOMAIN core.d_iso_ccy      AS text    CHECK (VALUE ~ '^[A-Z]{3}$');
CREATE DOMAIN core.d_sigma_level  AS numeric(4,2)  CHECK (VALUE >= 0 AND VALUE <= 6.5);
CREATE DOMAIN core.d_percent      AS numeric(5,2)  CHECK (VALUE >= 0 AND VALUE <= 100);

-- ================================================================
-- LiPi HIS — Training clinic identity schema
-- Run against lipi_training database
-- ================================================================

CREATE SCHEMA IF NOT EXISTS identity;
SET search_path = identity, core, public;

-- ROLES & PERMISSIONS
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.roles (
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
CREATE TABLE IF NOT EXISTS identity.permissions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    permission_code   text        NOT NULL UNIQUE,                -- e.g. 'opd.encounter.create', 'rtqa.approve', 'compliance.license.edit'
    module            text        NOT NULL,                       -- 'opd' | 'ipd' | 'rtqa' | 'compliance' | ...
    action            text        NOT NULL,                       -- 'read' | 'create' | 'update' | 'delete' | 'approve' | 'sign' | 'export'
    description       text,
    is_phi_sensitive  boolean     NOT NULL DEFAULT false          -- if true, emits phi_access_log entry on use
);

CREATE TABLE IF NOT EXISTS identity.role_permissions (
    role_id           uuid        NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    permission_id     uuid        NOT NULL REFERENCES identity.permissions(id) ON DELETE CASCADE,
    granted_at        timestamptz NOT NULL DEFAULT now(),
    granted_by        uuid,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE IF NOT EXISTS identity.user_roles (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    user_id           uuid        NOT NULL, -- references master.platform_users(id) — no FK (cross-DB)
    role_id           uuid        NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    scope_department_id uuid,    -- optional: role scoped to a dept
    assigned_at       timestamptz NOT NULL DEFAULT now(),
    assigned_by       uuid,
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz
);
-- Prevent duplicate (user,role) when no department scope
CREATE UNIQUE INDEX IF NOT EXISTS uq_user_roles_global ON identity.user_roles(user_id, role_id) WHERE scope_department_id IS NULL;
-- Prevent duplicate (user,role,dept) when scoped
CREATE UNIQUE INDEX IF NOT EXISTS uq_user_roles_scoped ON identity.user_roles(user_id, role_id, scope_department_id) WHERE scope_department_id IS NOT NULL;

-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.sessions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    user_id           uuid        NOT NULL, -- references master.platform_users(id) — no FK (cross-DB)
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
CREATE INDEX IF NOT EXISTS ix_sessions_user        ON identity.sessions(user_id) WHERE revoked_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_sessions_jti         ON identity.sessions(jwt_jti);
CREATE INDEX IF NOT EXISTS ix_sessions_expires     ON identity.sessions(expires_at) WHERE revoked_at IS NULL;

-- ---------------------------------------------------------------------
-- MFA METHODS — TOTP, WebAuthn, SMS OTP, email OTP
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.mfa_methods (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    user_id           uuid        NOT NULL, -- references master.platform_users(id) — no FK (cross-DB)
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
CREATE INDEX IF NOT EXISTS ix_mfa_user ON identity.mfa_methods(user_id) WHERE revoked_at IS NULL;

CREATE TABLE IF NOT EXISTS identity.password_history (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    user_id           uuid        NOT NULL, -- references master.platform_users(id) — no FK (cross-DB)
    password_hash     text        NOT NULL,
    set_at            timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_pwd_history_user ON identity.password_history(user_id, set_at DESC);

-- ---------------------------------------------------------------------
-- LOGIN ATTEMPTS (successful + failed — feeds anomaly detection)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.login_attempts (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    attempted_at      timestamptz NOT NULL DEFAULT now(),
    username          citext,
    user_id           uuid, -- references master.platform_users(id) — no FK (cross-DB)
    client_ip         inet,
    user_agent        text,
    auth_method       text        CHECK (auth_method IN ('password','ad','saml','oidc','api_key','service','mfa')),
    outcome           text        NOT NULL CHECK (outcome IN ('success','bad_password','unknown_user','mfa_failed','locked','disabled','expired','rate_limited','suspicious')),
    failure_detail    text,
    geo_country       char(2),
    geo_city          text,
    PRIMARY KEY (id, attempted_at)
) PARTITION BY RANGE (attempted_at);

CREATE INDEX IF NOT EXISTS ix_login_attempts_user     ON identity.login_attempts(user_id, attempted_at DESC);
CREATE INDEX IF NOT EXISTS ix_login_attempts_ip       ON identity.login_attempts(client_ip, attempted_at DESC);
CREATE INDEX IF NOT EXISTS ix_login_attempts_outcome  ON identity.login_attempts(clinic_id, outcome, attempted_at DESC);

CREATE TABLE IF NOT EXISTS identity.login_attempts_2026_04 PARTITION OF identity.login_attempts
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE IF NOT EXISTS identity.login_attempts_2026_05 PARTITION OF identity.login_attempts
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- ---------------------------------------------------------------------
-- API KEYS & SERVICE ACCOUNTS (for integrations: lab analyser, modality bridge, etc.)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.mfa_methods (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    user_id           uuid        NOT NULL, -- references master.platform_users(id) — no FK (cross-DB)
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
CREATE INDEX IF NOT EXISTS ix_mfa_user ON identity.mfa_methods(user_id) WHERE revoked_at IS NULL;

CREATE TABLE IF NOT EXISTS identity.password_history (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    user_id           uuid        NOT NULL, -- references master.platform_users(id) — no FK (cross-DB)
    password_hash     text        NOT NULL,
    set_at            timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_pwd_history_user ON identity.password_history(user_id, set_at DESC);

-- ---------------------------------------------------------------------
-- LOGIN ATTEMPTS (successful + failed — feeds anomaly detection)
-- ---------------------------------------------------------------------

-- ================================================================
-- clinic_profiles — per-clinic user profile (designation, dept etc.)
-- platform_user_id references master.platform_users.id (no FK - different DB)
-- ================================================================
CREATE TABLE IF NOT EXISTS identity.clinic_profiles (
    id               UUID        PRIMARY KEY DEFAULT core.uuid_v7(),
    platform_user_id UUID        NOT NULL,
    clinic_id        UUID        NOT NULL,
    designation      TEXT,
    department       TEXT,
    staff_type       TEXT,
    employee_code    TEXT,
    joining_date     TEXT,
    status           TEXT        NOT NULL DEFAULT 'active',
    extension_data   JSONB       NOT NULL DEFAULT '{}'::jsonb,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at       TIMESTAMPTZ,
    created_by       UUID,
    CONSTRAINT uq_clinic_profiles_user UNIQUE (platform_user_id, clinic_id)
);

CREATE INDEX IF NOT EXISTS ix_clinic_profiles_user ON identity.clinic_profiles(platform_user_id);
CREATE INDEX IF NOT EXISTS ix_clinic_profiles_clinic ON identity.clinic_profiles(clinic_id);

