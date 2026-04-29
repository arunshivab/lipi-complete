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
-- =====================================================================
-- LiPi :: 00_common :: 003_audit_triggers
-- Generic triggers for created_at / updated_at / row_version.
-- Per-table audit-event emission is defined in clinic/04_audit.sql
-- because it references audit.audit_events (not yet created here).
-- =====================================================================

CREATE OR REPLACE FUNCTION core.fn_set_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at := now();
    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
    RETURN NEW;
END;
$$;

COMMENT ON FUNCTION core.fn_set_updated_at() IS
  'Attach as BEFORE UPDATE trigger on every mutable table. Bumps updated_at and row_version.';

-- Convenience helper: attach the standard triggers to a table in one call


-- =====================================================================
-- LiPi :: master :: 001_schema_master
-- The Master Registry DB — single source of truth for:
--   • tenants (organizations, clinics, clinic groups)
--   • subscriptions & billing
--   • global (cross-clinic) user identity & AD federation
--   • tenant provisioning metadata
--   • master-level audit
-- A region has exactly one master DB. Clinics never write here.
-- Prereq: 00_common/* applied to this database.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS master;
SET search_path = master, core, public;

-- ---------------------------------------------------------------------
-- ORGANIZATIONS  (the legal entity — clinic chain, single hospital, or group)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.organizations (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    legal_name        text        NOT NULL,
    trading_name      text,
    org_type          text        NOT NULL CHECK (org_type IN
                                   ('single_clinic','hospital_chain','clinic_group','government','academic')),
    country_code      char(2)     NOT NULL DEFAULT 'IN',
    pan               core.d_pan,
    gstin             core.d_gstin,
    cin               text,                       -- Corporate Identification Number (MCA)
    registered_address jsonb       NOT NULL,
    primary_contact   jsonb       NOT NULL,        -- {name, email, phone}
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('active','suspended','terminated','onboarding')),
    onboarded_at      timestamptz,
    extension_data    jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    updated_by        uuid,
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_organizations_pan   ON master.organizations(pan)   WHERE pan IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_organizations_gstin ON master.organizations(gstin) WHERE gstin IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_organizations_status       ON master.organizations(status) WHERE deleted_at IS NULL;

-- ---------------------------------------------------------------------
-- CLINICS (individual facility — each has its own OLTP database)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.clinics (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    organization_id   uuid        NOT NULL REFERENCES master.organizations(id),
    code              text        NOT NULL,                -- human-friendly short code e.g. 'APL-DEL-01'
    name              text        NOT NULL,
    clinic_type       text        NOT NULL CHECK (clinic_type IN
                                   ('hospital','cancer_centre','diagnostic','day_care','clinic','polyclinic')),
    city              text        NOT NULL,
    state             text        NOT NULL,
    country_code      char(2)     NOT NULL DEFAULT 'IN',
    address           jsonb       NOT NULL,
    timezone          text        NOT NULL DEFAULT 'Asia/Kolkata',
    bed_count         int         CHECK (bed_count >= 0),
    nabh_accredited   boolean     NOT NULL DEFAULT false,
    nabl_accredited   boolean     NOT NULL DEFAULT false,
    has_oncology      boolean     NOT NULL DEFAULT false,
    has_rt            boolean     NOT NULL DEFAULT false,   -- radiation therapy → triggers AERB + RT-QA modules
    has_nuclear_med   boolean     NOT NULL DEFAULT false,
    status            text        NOT NULL DEFAULT 'provisioning'
                                    CHECK (status IN ('provisioning','active','suspended','terminated')),
    go_live_at        timestamptz,
    extension_data    jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    updated_by        uuid,
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_clinics_code UNIQUE (organization_id, code)
);

CREATE INDEX IF NOT EXISTS ix_clinics_org        ON master.clinics(organization_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_clinics_status     ON master.clinics(status)          WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_clinics_oncology   ON master.clinics(organization_id) WHERE has_oncology AND deleted_at IS NULL;

-- ---------------------------------------------------------------------
-- CLINIC DATABASES (provisioning metadata — where each clinic's OLTP DB lives)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.clinic_databases (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL UNIQUE REFERENCES master.clinics(id),
    db_host           text        NOT NULL,
    db_port           int         NOT NULL DEFAULT 5432,
    db_name           text        NOT NULL,
    db_schema_version text        NOT NULL,                -- e.g. '1.4.2' — which migration is applied
    read_replica_host text,
    backup_policy     jsonb       NOT NULL DEFAULT '{}'::jsonb,
    encryption_key_arn text       NOT NULL,                -- AWS KMS key for TDE
    fdw_linked        boolean     NOT NULL DEFAULT false,  -- wired to warehouse?
    last_health_check timestamptz,
    health_status     text        CHECK (health_status IN ('green','yellow','red')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_clinic_databases_status ON master.clinic_databases(health_status);

-- ---------------------------------------------------------------------
-- CLINIC GROUPS (logical grouping for combined dashboards — many-to-many)
-- Example: "Apollo North India Oncology Group" = Delhi + Noida + Gurgaon clinics
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.clinic_groups (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    organization_id   uuid        NOT NULL REFERENCES master.organizations(id),
    name              text        NOT NULL,
    description       text,
    group_type        text        NOT NULL CHECK (group_type IN
                                   ('regional','clinical','financial','reporting','custom')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_clinic_groups_name UNIQUE (organization_id, name)
);

CREATE TABLE IF NOT EXISTS master.clinic_group_members (
    clinic_group_id   uuid        NOT NULL REFERENCES master.clinic_groups(id) ON DELETE CASCADE,
    clinic_id         uuid        NOT NULL REFERENCES master.clinics(id) ON DELETE CASCADE,
    added_at          timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (clinic_group_id, clinic_id)
);

-- ---------------------------------------------------------------------
-- SUBSCRIPTION PLANS & SUBSCRIPTIONS
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.subscription_plans (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    code              text        NOT NULL UNIQUE,
    name              text        NOT NULL,
    tier              text        NOT NULL CHECK (tier IN ('starter','professional','enterprise','custom')),
    modules_included  text[]      NOT NULL,                -- array of module codes: ['opd','ipd','lab',...]
    max_users         int,
    max_beds          int,
    max_patients      int,
    price_inr_monthly numeric(12,2),
    price_inr_annual  numeric(12,2),
    features          jsonb       NOT NULL DEFAULT '{}'::jsonb,  -- e.g. {"pacs": true, "esapi": false}
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS master.subscriptions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    organization_id   uuid        NOT NULL REFERENCES master.organizations(id),
    clinic_id         uuid        REFERENCES master.clinics(id),    -- NULL ⇒ applies to whole org
    plan_id           uuid        NOT NULL REFERENCES master.subscription_plans(id),
    starts_on         date        NOT NULL,
    ends_on           date        NOT NULL,
    billing_cycle     text        NOT NULL CHECK (billing_cycle IN ('monthly','quarterly','annual')),
    auto_renew        boolean     NOT NULL DEFAULT true,
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('trial','active','past_due','cancelled','expired')),
    addon_modules     text[]      NOT NULL DEFAULT '{}',
    price_override_inr numeric(12,2),
    po_reference      text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT ck_subscription_dates CHECK (ends_on > starts_on)
);

CREATE INDEX IF NOT EXISTS ix_subscriptions_org     ON master.subscriptions(organization_id);
CREATE INDEX IF NOT EXISTS ix_subscriptions_clinic  ON master.subscriptions(clinic_id);
CREATE INDEX IF NOT EXISTS ix_subscriptions_active  ON master.subscriptions(status, ends_on);

CREATE TABLE IF NOT EXISTS master.invoices (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    subscription_id   uuid        NOT NULL REFERENCES master.subscriptions(id),
    invoice_number    text        NOT NULL UNIQUE,
    period_start      date        NOT NULL,
    period_end        date        NOT NULL,
    amount_inr        numeric(12,2) NOT NULL,
    gst_amount_inr    numeric(12,2) NOT NULL DEFAULT 0,
    total_inr         numeric(12,2) NOT NULL,
    currency          core.d_iso_ccy NOT NULL DEFAULT 'INR',
    status            text        NOT NULL DEFAULT 'issued'
                                    CHECK (status IN ('draft','issued','paid','overdue','void')),
    issued_at         timestamptz NOT NULL DEFAULT now(),
    paid_at           timestamptz,
    payment_reference text,
    line_items        jsonb       NOT NULL DEFAULT '[]'::jsonb
);

CREATE INDEX IF NOT EXISTS ix_invoices_subscription ON master.invoices(subscription_id);
CREATE INDEX IF NOT EXISTS ix_invoices_status       ON master.invoices(status);

-- ---------------------------------------------------------------------
-- GLOBAL USERS (cross-clinic: directors, group admins, LiPi platform staff)
-- Clinic-scoped users live in the per-clinic DB's identity schema.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.global_users (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    email             core.d_email NOT NULL UNIQUE,
    full_name         text        NOT NULL,
    phone             core.d_phone_in,
    user_type         text        NOT NULL CHECK (user_type IN
                                   ('platform_admin','platform_support','org_director','group_admin','auditor')),
    organization_id   uuid        REFERENCES master.organizations(id),   -- NULL for platform staff
    password_hash     text,                                              -- Argon2id; NULL if only-SSO
    is_mfa_enabled    boolean     NOT NULL DEFAULT true,
    last_login_at     timestamptz,
    failed_login_count int        NOT NULL DEFAULT 0,
    locked_until      timestamptz,
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('invited','active','suspended','terminated')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_global_users_org ON master.global_users(organization_id) WHERE deleted_at IS NULL;

-- Which clinics can each global user access? (group-head scope)
CREATE TABLE IF NOT EXISTS master.global_user_clinic_access (
    global_user_id    uuid        NOT NULL REFERENCES master.global_users(id) ON DELETE CASCADE,
    clinic_id         uuid        NOT NULL REFERENCES master.clinics(id) ON DELETE CASCADE,
    access_level      text        NOT NULL CHECK (access_level IN ('read','report','admin')),
    granted_at        timestamptz NOT NULL DEFAULT now(),
    granted_by        uuid,
    PRIMARY KEY (global_user_id, clinic_id)
);

-- ---------------------------------------------------------------------
-- ACTIVE DIRECTORY FEDERATION
-- Each organization can federate with one or more AD/LDAP/SAML/OIDC providers.
-- Clinic-level identity then uses these claims + local role mappings.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.identity_providers (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    organization_id   uuid        NOT NULL REFERENCES master.organizations(id),
    idp_type          text        NOT NULL CHECK (idp_type IN ('ad_ldap','azure_ad','okta','saml','oidc','keycloak')),
    display_name      text        NOT NULL,
    domain            text,                                 -- e.g. 'apollo.in'
    config            jsonb       NOT NULL,                 -- provider-specific (metadata URL, LDAP base DN, etc.)
    is_default        boolean     NOT NULL DEFAULT false,
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT uq_idp_org_name UNIQUE (organization_id, display_name)
);

-- AD group → LiPi role mapping (applied during login)
CREATE TABLE IF NOT EXISTS master.idp_group_role_mappings (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    identity_provider_id uuid     NOT NULL REFERENCES master.identity_providers(id) ON DELETE CASCADE,
    ad_group_dn       text        NOT NULL,                 -- e.g. 'CN=Oncologists,OU=Doctors,DC=apollo,DC=in'
    role_code         text        NOT NULL,                 -- e.g. 'oncologist', 'nurse', 'radiographer'
    clinic_id         uuid        REFERENCES master.clinics(id),  -- NULL = applies to all clinics in the org
    created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_idp_group_role_idp ON master.idp_group_role_mappings(identity_provider_id);

-- ---------------------------------------------------------------------
-- FEATURE FLAGS (platform-wide + per-org overrides)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.feature_flags (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    flag_key          text        NOT NULL UNIQUE,
    description       text,
    default_enabled   boolean     NOT NULL DEFAULT false,
    rollout_rules     jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS master.feature_flag_overrides (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    feature_flag_id   uuid        NOT NULL REFERENCES master.feature_flags(id) ON DELETE CASCADE,
    scope_type        text        NOT NULL CHECK (scope_type IN ('organization','clinic','user')),
    scope_id          uuid        NOT NULL,
    enabled           boolean     NOT NULL,
    reason            text,
    created_at        timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT uq_feature_override UNIQUE (feature_flag_id, scope_type, scope_id)
);

-- ---------------------------------------------------------------------
-- MASTER AUDIT LOG (platform-level actions only — clinic data has its own)
-- Hash-chained for tamper evidence.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS master.audit_events (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    event_ts          timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (id, event_ts),
    actor_user_id     uuid        REFERENCES master.global_users(id),
    actor_ip          inet,
    actor_user_agent  text,
    action            text        NOT NULL,                 -- e.g. 'clinic.provisioned', 'subscription.renewed'
    entity_type       text        NOT NULL,
    entity_id         uuid,
    organization_id   uuid,
    clinic_id         uuid,
    before_state      jsonb,
    after_state       jsonb,
    request_id        uuid,
    previous_hash     bytea,
    current_hash      bytea       NOT NULL
) PARTITION BY RANGE (event_ts);

CREATE INDEX IF NOT EXISTS ix_master_audit_entity ON master.audit_events(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS ix_master_audit_actor  ON master.audit_events(actor_user_id, event_ts DESC);
CREATE INDEX IF NOT EXISTS ix_master_audit_clinic ON master.audit_events(clinic_id, event_ts DESC);

-- First monthly partition (provisioner creates new ones ahead of time via cron)
CREATE TABLE IF NOT EXISTS master.audit_events_2026_04 PARTITION OF master.audit_events
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE IF NOT EXISTS master.audit_events_2026_05 PARTITION OF master.audit_events
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- ---------------------------------------------------------------------
-- Standard triggers
-- ---------------------------------------------------------------------

-- ================================================================
-- platform_users — ALL user identity (replaces identity.users)
-- Single source of truth for auth across all clinics
-- ================================================================
CREATE TABLE IF NOT EXISTS master.platform_users (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    username            TEXT        NOT NULL,
    password_hash       TEXT,
    first_name          TEXT        NOT NULL DEFAULT '',
    middle_name         TEXT,
    last_name           TEXT        NOT NULL DEFAULT '',
    display_name        TEXT        NOT NULL DEFAULT '',
    title               TEXT,
    gender              TEXT,
    date_of_birth       TEXT,
    blood_group         TEXT,
    nationality         TEXT        DEFAULT 'Indian',
    email               TEXT        NOT NULL DEFAULT '',
    phone               TEXT,
    phone_country_code  TEXT        DEFAULT '+91',
    nmc_reg_number      TEXT,
    aerb_rp_number      TEXT,
    extension_data      JSONB       NOT NULL DEFAULT '{}',
    user_type           TEXT        NOT NULL DEFAULT 'staff',
    status              TEXT        NOT NULL DEFAULT 'active',
    must_change_password BOOLEAN    NOT NULL DEFAULT true,
    is_mfa_enforced     BOOLEAN     NOT NULL DEFAULT false,
    failed_login_count  INT         NOT NULL DEFAULT 0,
    locked_until        TIMESTAMP,
    last_login_at       TIMESTAMP,
    created_at          TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at          TIMESTAMP   NOT NULL DEFAULT now(),
    deleted_at          TIMESTAMP,
    created_by          UUID,
    row_version         INT         NOT NULL DEFAULT 1
);

CREATE UNIQUE INDEX ux_platform_users_username
    ON master.platform_users(username) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_platform_users_email
    ON master.platform_users(email) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_platform_users_type
    ON master.platform_users(user_type) WHERE deleted_at IS NULL;

-- ================================================================
-- clinic_memberships — which users belong to which clinic
-- ================================================================
CREATE TABLE IF NOT EXISTS master.clinic_memberships (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    platform_user_id UUID        NOT NULL REFERENCES master.platform_users(id) ON DELETE CASCADE,
    clinic_id        UUID        NOT NULL REFERENCES master.clinics(id) ON DELETE CASCADE,
    status           TEXT        NOT NULL DEFAULT 'active',
    created_at       TIMESTAMP   NOT NULL DEFAULT now(),
    created_by       UUID,
    CONSTRAINT uq_clinic_membership UNIQUE (platform_user_id, clinic_id)
);

CREATE INDEX IF NOT EXISTS ix_clinic_memberships_clinic
    ON master.clinic_memberships(clinic_id);
