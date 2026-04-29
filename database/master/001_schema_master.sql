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
CREATE TABLE master.organizations (
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

CREATE UNIQUE INDEX uq_organizations_pan   ON master.organizations(pan)   WHERE pan IS NOT NULL;
CREATE UNIQUE INDEX uq_organizations_gstin ON master.organizations(gstin) WHERE gstin IS NOT NULL;
CREATE INDEX ix_organizations_status       ON master.organizations(status) WHERE deleted_at IS NULL;

-- ---------------------------------------------------------------------
-- CLINICS (individual facility — each has its own OLTP database)
-- ---------------------------------------------------------------------
CREATE TABLE master.clinics (
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

CREATE INDEX ix_clinics_org        ON master.clinics(organization_id) WHERE deleted_at IS NULL;
CREATE INDEX ix_clinics_status     ON master.clinics(status)          WHERE deleted_at IS NULL;
CREATE INDEX ix_clinics_oncology   ON master.clinics(organization_id) WHERE has_oncology AND deleted_at IS NULL;

-- ---------------------------------------------------------------------
-- CLINIC DATABASES (provisioning metadata — where each clinic's OLTP DB lives)
-- ---------------------------------------------------------------------
CREATE TABLE master.clinic_databases (
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

CREATE INDEX ix_clinic_databases_status ON master.clinic_databases(health_status);

-- ---------------------------------------------------------------------
-- CLINIC GROUPS (logical grouping for combined dashboards — many-to-many)
-- Example: "Apollo North India Oncology Group" = Delhi + Noida + Gurgaon clinics
-- ---------------------------------------------------------------------
CREATE TABLE master.clinic_groups (
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

CREATE TABLE master.clinic_group_members (
    clinic_group_id   uuid        NOT NULL REFERENCES master.clinic_groups(id) ON DELETE CASCADE,
    clinic_id         uuid        NOT NULL REFERENCES master.clinics(id) ON DELETE CASCADE,
    added_at          timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (clinic_group_id, clinic_id)
);

-- ---------------------------------------------------------------------
-- SUBSCRIPTION PLANS & SUBSCRIPTIONS
-- ---------------------------------------------------------------------
CREATE TABLE master.subscription_plans (
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

CREATE TABLE master.subscriptions (
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

CREATE INDEX ix_subscriptions_org     ON master.subscriptions(organization_id);
CREATE INDEX ix_subscriptions_clinic  ON master.subscriptions(clinic_id);
CREATE INDEX ix_subscriptions_active  ON master.subscriptions(status, ends_on);

CREATE TABLE master.invoices (
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

CREATE INDEX ix_invoices_subscription ON master.invoices(subscription_id);
CREATE INDEX ix_invoices_status       ON master.invoices(status);

-- ---------------------------------------------------------------------
-- GLOBAL USERS (cross-clinic: directors, group admins, LiPi platform staff)
-- Clinic-scoped users live in the per-clinic DB's identity schema.
-- ---------------------------------------------------------------------
CREATE TABLE master.global_users (
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

CREATE INDEX ix_global_users_org ON master.global_users(organization_id) WHERE deleted_at IS NULL;

-- Which clinics can each global user access? (group-head scope)
CREATE TABLE master.global_user_clinic_access (
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
CREATE TABLE master.identity_providers (
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
CREATE TABLE master.idp_group_role_mappings (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    identity_provider_id uuid     NOT NULL REFERENCES master.identity_providers(id) ON DELETE CASCADE,
    ad_group_dn       text        NOT NULL,                 -- e.g. 'CN=Oncologists,OU=Doctors,DC=apollo,DC=in'
    role_code         text        NOT NULL,                 -- e.g. 'oncologist', 'nurse', 'radiographer'
    clinic_id         uuid        REFERENCES master.clinics(id),  -- NULL = applies to all clinics in the org
    created_at        timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_idp_group_role_idp ON master.idp_group_role_mappings(identity_provider_id);

-- ---------------------------------------------------------------------
-- FEATURE FLAGS (platform-wide + per-org overrides)
-- ---------------------------------------------------------------------
CREATE TABLE master.feature_flags (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    flag_key          text        NOT NULL UNIQUE,
    description       text,
    default_enabled   boolean     NOT NULL DEFAULT false,
    rollout_rules     jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE master.feature_flag_overrides (
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
CREATE TABLE master.audit_events (
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

CREATE INDEX ix_master_audit_entity ON master.audit_events(entity_type, entity_id);
CREATE INDEX ix_master_audit_actor  ON master.audit_events(actor_user_id, event_ts DESC);
CREATE INDEX ix_master_audit_clinic ON master.audit_events(clinic_id, event_ts DESC);

-- First monthly partition (provisioner creates new ones ahead of time via cron)
CREATE TABLE master.audit_events_2026_04 PARTITION OF master.audit_events
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE master.audit_events_2026_05 PARTITION OF master.audit_events
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- ---------------------------------------------------------------------
-- Standard triggers
-- ---------------------------------------------------------------------
SELECT core.attach_standard_triggers('master','organizations');
SELECT core.attach_standard_triggers('master','clinics');
SELECT core.attach_standard_triggers('master','clinic_databases');
SELECT core.attach_standard_triggers('master','clinic_groups');
SELECT core.attach_standard_triggers('master','subscription_plans');
SELECT core.attach_standard_triggers('master','subscriptions');
SELECT core.attach_standard_triggers('master','global_users');
SELECT core.attach_standard_triggers('master','identity_providers');
SELECT core.attach_standard_triggers('master','feature_flags');
