-- =====================================================================
-- LiPi :: clinic :: 05_security
-- Envelope encryption keys, DPDP Act 2023 consent & rights, security events,
-- rate limiting, break-glass controls, data classification.
-- Prereq: 00_common + 01_core + 02_identity applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS security;
SET search_path = security, core, public;

-- ---------------------------------------------------------------------
-- ENCRYPTION KEYS — envelope encryption via AWS KMS
-- Data is encrypted client-side using a Data Encryption Key (DEK).
-- The DEK itself is stored here encrypted by the KMS Customer Master Key.
-- ---------------------------------------------------------------------
CREATE TABLE security.encryption_keys (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    key_purpose       text        NOT NULL CHECK (key_purpose IN
                                   ('phi_field','attachment','dicom','audit_log','mfa_secret','abdm_token','backup')),
    key_version       int         NOT NULL,
    wrapped_dek       bytea       NOT NULL,                          -- KMS-encrypted DEK
    kms_cmk_arn       text        NOT NULL,
    algorithm         text        NOT NULL DEFAULT 'AES-256-GCM',
    is_active         boolean     NOT NULL DEFAULT true,
    activated_at      timestamptz NOT NULL DEFAULT now(),
    retired_at        timestamptz,
    rotation_due_at   timestamptz NOT NULL,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_enc_key_purpose_version UNIQUE (clinic_id, key_purpose, key_version)
);
CREATE INDEX ix_enc_keys_active ON security.encryption_keys(clinic_id, key_purpose) WHERE is_active;

-- ---------------------------------------------------------------------
-- DATA CLASSIFICATION (reference)
-- ---------------------------------------------------------------------
CREATE TABLE security.data_classifications (
    code              text        PRIMARY KEY,                      -- 'public','internal','confidential','restricted','phi','spi'
    name              text        NOT NULL,
    description       text,
    dpdp_category     text        CHECK (dpdp_category IN ('personal','sensitive_personal','critical_personal','non_personal')),
    retention_days    int,                                           -- recommended default retention
    encryption_required boolean   NOT NULL DEFAULT true,
    audit_required    boolean     NOT NULL DEFAULT true
);

-- ---------------------------------------------------------------------
-- DPDP CONSENTS (platform-wide data-processing consents, distinct from clinical consent)
-- ---------------------------------------------------------------------
CREATE TABLE security.dpdp_consents (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    data_principal_type text      NOT NULL CHECK (data_principal_type IN ('patient','staff','visitor','vendor')),
    data_principal_id uuid        NOT NULL,                          -- patients.id or staff.id depending on type
    purpose_code      text        NOT NULL,                          -- 'treatment','billing','research','marketing',...
    purpose_text      text        NOT NULL,
    lawful_basis      text        NOT NULL CHECK (lawful_basis IN
                                   ('consent','legitimate_use','legal_obligation','medical_emergency','employment','public_interest')),
    data_categories   text[]      NOT NULL,                          -- 'identity','contact','health','biometric','financial','location'
    granted           boolean     NOT NULL,
    granted_at        timestamptz NOT NULL DEFAULT now(),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    withdrawal_channel text       CHECK (withdrawal_channel IN ('app','email','sms','paper','in_person')),
    withdrawn_at      timestamptz,
    language_used     text        DEFAULT 'en',
    proof_s3_key      text,                                           -- signed/verified consent artefact
    notice_version    text        NOT NULL,                           -- which version of the notice was shown
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_dpdp_consents_subject ON security.dpdp_consents(data_principal_type, data_principal_id);
CREATE INDEX ix_dpdp_consents_valid   ON security.dpdp_consents(valid_to) WHERE granted AND withdrawn_at IS NULL;

-- ---------------------------------------------------------------------
-- DPDP DATA SUBJECT REQUESTS (access, correction, erasure, portability, grievance)
-- ---------------------------------------------------------------------
CREATE TABLE security.dpdp_subject_requests (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    request_type      text        NOT NULL CHECK (request_type IN
                                   ('access','correction','erasure','portability','nominee','grievance','consent_withdrawal')),
    data_principal_type text      NOT NULL,
    data_principal_id uuid        NOT NULL,
    request_channel   text        NOT NULL CHECK (request_channel IN ('app','email','paper','in_person','phone')),
    requested_at      timestamptz NOT NULL DEFAULT now(),
    sla_deadline      timestamptz NOT NULL,                           -- 30 days from request per DPDP
    status            text        NOT NULL DEFAULT 'received'
                                    CHECK (status IN ('received','identity_verified','in_progress','awaiting_consent','fulfilled','partial','rejected','escalated')),
    assigned_to       uuid,                                            -- identity.users.id (DPO or delegate)
    identity_verification_method text,
    identity_verified_at timestamptz,
    fulfilled_at      timestamptz,
    response_artefact_s3_key text,
    rejection_reason  text,
    dpo_notes         text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_dpdp_requests_status ON security.dpdp_subject_requests(clinic_id, status, sla_deadline);
CREATE INDEX ix_dpdp_requests_subject ON security.dpdp_subject_requests(data_principal_type, data_principal_id);

-- Erasure requests need special handling — track per-system completion
CREATE TABLE security.dpdp_erasure_tasks (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    dpdp_subject_request_id uuid  NOT NULL REFERENCES security.dpdp_subject_requests(id) ON DELETE CASCADE,
    target_system     text        NOT NULL,                          -- 'oltp','warehouse','s3_attachments','dicom','backup','abdm_hie'
    status            text        NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','running','completed','failed','skipped_legal_hold')),
    rows_affected     bigint,
    executed_at       timestamptz,
    error_detail      text,
    legal_hold_reference text                                          -- if retention-obligated, cite the legal basis
);

-- ---------------------------------------------------------------------
-- SECURITY EVENTS — SIEM feed
-- ---------------------------------------------------------------------
CREATE TABLE security.security_events (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    occurred_at       timestamptz NOT NULL DEFAULT now(),
    event_category    text        NOT NULL CHECK (event_category IN
                                   ('auth_anomaly','privilege_escalation','data_exfiltration','malware','policy_violation','config_drift','dlp','ids_ips','break_glass','key_rotation')),
    severity          text        NOT NULL CHECK (severity IN ('info','low','medium','high','critical')),
    actor_user_id     uuid,
    actor_ip          inet,
    source_host       text,
    signature         text,                                            -- rule/IOC id that matched
    detail            jsonb       NOT NULL DEFAULT '{}'::jsonb,
    status            text        NOT NULL DEFAULT 'open'
                                    CHECK (status IN ('open','investigating','resolved','false_positive','suppressed')),
    triaged_by        uuid,
    triaged_at        timestamptz,
    resolution_notes  text
);
CREATE INDEX ix_secevt_severity ON security.security_events(clinic_id, severity, occurred_at DESC);
CREATE INDEX ix_secevt_status   ON security.security_events(clinic_id, status, occurred_at DESC);

-- ---------------------------------------------------------------------
-- BREAK-GLASS (emergency PHI access override, with mandatory justification)
-- ---------------------------------------------------------------------
CREATE TABLE security.break_glass_sessions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    user_id           uuid        NOT NULL,
    patient_id        uuid        NOT NULL,
    activated_at      timestamptz NOT NULL DEFAULT now(),
    expires_at        timestamptz NOT NULL,                            -- max 4 hours typical
    reason_code       text        NOT NULL CHECK (reason_code IN ('life_threatening','unconscious_patient','no_consenting_kin','statutory','other')),
    justification     text        NOT NULL,
    approved_by       uuid,
    approval_at       timestamptz,
    reviewed_by       uuid,
    reviewed_at       timestamptz,
    review_outcome    text        CHECK (review_outcome IN ('appropriate','inappropriate','training_required','disciplinary'))
);
CREATE INDEX ix_break_glass_patient ON security.break_glass_sessions(patient_id, activated_at DESC);

-- ---------------------------------------------------------------------
-- IP ALLOWLIST (administrative endpoints)
-- ---------------------------------------------------------------------
CREATE TABLE security.ip_allowlist (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    cidr              cidr        NOT NULL,
    scope             text        NOT NULL CHECK (scope IN ('admin_console','api','abdm_ingress','dicom_ingress','hl7_ingress')),
    description       text,
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    expires_at        timestamptz
);

-- ---------------------------------------------------------------------
-- RATE LIMIT STATE (per api_key / user / ip; short-retention)
-- ---------------------------------------------------------------------
CREATE TABLE security.rate_limit_buckets (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    bucket_key        text        NOT NULL,                          -- 'user:<uuid>' | 'apikey:<id>' | 'ip:<cidr>'
    window_start      timestamptz NOT NULL,
    request_count     int         NOT NULL DEFAULT 0,
    rejected_count    int         NOT NULL DEFAULT 0,
    CONSTRAINT uq_rlb_key_window UNIQUE (bucket_key, window_start)
);

-- ---------------------------------------------------------------------
-- DATA LOCALIZATION POLICIES — record that Indian data stays in India
-- Satisfies DPDP Act 2023 s.16 + future notified restrictions.
-- ---------------------------------------------------------------------
CREATE TABLE security.data_localization_policies (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    data_category     text        NOT NULL,                           -- 'phi','financial','employee','imaging','backup'
    storage_region    text        NOT NULL,                           -- 'ap-south-1','ap-south-2',...
    replication_regions text[]    NOT NULL DEFAULT '{}',
    cross_border_allowed boolean  NOT NULL DEFAULT false,
    cross_border_countries char(2)[] NOT NULL DEFAULT '{}',
    policy_version    text        NOT NULL,
    effective_from    date        NOT NULL,
    effective_to      date,
    approved_by       uuid,
    approved_at       timestamptz,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='security'
             AND tablename NOT IN ('data_classifications')
    LOOP
        EXECUTE format('ALTER TABLE security.%I ENABLE ROW LEVEL SECURITY', t);
        BEGIN
            EXECUTE format(
              'CREATE POLICY p_%1$s_tenant ON security.%1$I
               USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
               WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
        EXCEPTION WHEN undefined_column THEN NULL;
        END;
    END LOOP;
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='security'
             AND tablename NOT IN ('rate_limit_buckets','data_classifications')
    LOOP
        PERFORM core.attach_standard_triggers('security', t);
    END LOOP;
END $$;
