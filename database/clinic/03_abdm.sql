-- =====================================================================
-- LiPi :: clinic :: 03_abdm
-- ABDM (Ayushman Bharat Digital Mission) — ABHA, consent, HIP/HIU flows,
-- Aadhaar eKYC, HFR (Health Facility Registry) + HPR (Healthcare Professional Registry) linkage.
-- Prereq: 00_common + 01_core applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS abdm;
SET search_path = abdm, core, public;

-- ---------------------------------------------------------------------
-- HFR / HPR LINKAGE — clinic's own presence in ABDM national registries
-- ---------------------------------------------------------------------
CREATE TABLE abdm.facility_registry (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL UNIQUE,
    facility_id       uuid        NOT NULL REFERENCES core.facility(id),
    hfr_facility_id   text        NOT NULL UNIQUE,                -- ABDM HFR id
    hfr_facility_name text        NOT NULL,
    ownership         text        CHECK (ownership IN ('government','private','trust','corporate','public_private_partnership')),
    hfr_status        text        NOT NULL CHECK (hfr_status IN ('pending','verified','suspended','delisted')),
    registered_at     timestamptz,
    last_sync_at      timestamptz,
    registry_payload  jsonb       NOT NULL DEFAULT '{}'::jsonb,    -- full HFR response cache
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);

CREATE TABLE abdm.professional_registry (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL UNIQUE REFERENCES core.staff(id),
    hpr_id            text        NOT NULL UNIQUE,                -- ABDM HPR id
    hpr_name          text        NOT NULL,
    council_name      text,                                        -- e.g. 'Medical Council of India'
    council_reg_no    text,
    qualification     text,
    hpr_status        text        NOT NULL CHECK (hpr_status IN ('pending','verified','suspended','revoked')),
    verified_at       timestamptz,
    last_sync_at      timestamptz,
    registry_payload  jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);

-- ---------------------------------------------------------------------
-- ABHA PROFILES — one per patient who has linked their ABHA
-- ---------------------------------------------------------------------
CREATE TABLE abdm.abha_profiles (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_id        uuid        NOT NULL REFERENCES core.patients(id),
    abha_number       core.d_abha NOT NULL,
    abha_address      text        NOT NULL,                        -- user@abdm
    name              text        NOT NULL,
    gender            text,
    date_of_birth     date,
    mobile_verified   boolean     NOT NULL DEFAULT false,
    email_verified    boolean     NOT NULL DEFAULT false,
    kyc_verified      boolean     NOT NULL DEFAULT false,
    kyc_method        text        CHECK (kyc_method IN ('aadhaar_otp','aadhaar_biometric','driving_licence','mobile','demographic')),
    abha_qr_s3_key    text,
    profile_payload   jsonb       NOT NULL DEFAULT '{}'::jsonb,
    linked_at         timestamptz NOT NULL DEFAULT now(),
    revoked_at        timestamptz,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_abha_patient UNIQUE (clinic_id, patient_id, abha_number)
);
CREATE INDEX ix_abha_profiles_number ON abdm.abha_profiles(abha_number);

-- ---------------------------------------------------------------------
-- LINK REQUESTS — linking patient records as care-contexts under ABHA
-- ---------------------------------------------------------------------
CREATE TABLE abdm.link_requests (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    abha_profile_id   uuid        NOT NULL REFERENCES abdm.abha_profiles(id),
    patient_id        uuid        NOT NULL REFERENCES core.patients(id),
    link_token        text        NOT NULL UNIQUE,
    link_type         text        NOT NULL CHECK (link_type IN ('user_initiated','hip_initiated')),
    auth_mode         text        NOT NULL CHECK (auth_mode IN ('demographic','mobile_otp','aadhaar_otp','password','direct')),
    status            text        NOT NULL CHECK (status IN ('requested','otp_sent','verified','linked','failed','cancelled','expired')),
    requested_at      timestamptz NOT NULL DEFAULT now(),
    verified_at       timestamptz,
    linked_at         timestamptz,
    error_code        text,
    error_detail      text
);
CREATE INDEX ix_link_requests_patient ON abdm.link_requests(patient_id);

-- Care contexts = episodes of care (OPD visit, admission, diagnostic report) exposed to ABDM
CREATE TABLE abdm.care_contexts (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    abha_profile_id   uuid        NOT NULL REFERENCES abdm.abha_profiles(id),
    reference_id      text        NOT NULL,                        -- e.g. encounter_id, report_id
    context_type      text        NOT NULL CHECK (context_type IN
                                   ('opd_visit','ipd_admission','day_care','discharge_summary','prescription',
                                    'diagnostic_report','immunization','wellness_record','health_document')),
    display           text        NOT NULL,
    linked_at         timestamptz NOT NULL DEFAULT now(),
    unlinked_at       timestamptz,
    visible           boolean     NOT NULL DEFAULT true,

    CONSTRAINT uq_care_context UNIQUE (abha_profile_id, reference_id, context_type)
);
CREATE INDEX ix_care_contexts_abha ON abdm.care_contexts(abha_profile_id) WHERE unlinked_at IS NULL;

-- ---------------------------------------------------------------------
-- CONSENT ARTEFACTS (ABDM Consent Manager issued)
-- ---------------------------------------------------------------------
CREATE TABLE abdm.consent_requests (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    abha_profile_id   uuid        NOT NULL REFERENCES abdm.abha_profiles(id),
    requester_type    text        NOT NULL CHECK (requester_type IN ('hip','hiu')),
    consent_manager_request_id text,
    purpose_code      text        NOT NULL,                        -- CAREMGT, PATRQT, PUBHLTH, ...
    purpose_text      text,
    hi_types          text[]      NOT NULL,                        -- DiagnosticReport, Prescription, ...
    date_range_from   date        NOT NULL,
    date_range_to     date        NOT NULL,
    expires_at        timestamptz NOT NULL,
    fetch_mode        text        NOT NULL CHECK (fetch_mode IN ('VIEW','STORE')),
    frequency_unit    text        CHECK (frequency_unit IN ('HOUR','DAY','WEEK','MONTH','YEAR')),
    frequency_value   int,
    status            text        NOT NULL CHECK (status IN ('requested','granted','denied','revoked','expired','error')),
    raised_at         timestamptz NOT NULL DEFAULT now(),
    granted_at        timestamptz,
    denied_at         timestamptz,
    revoked_at        timestamptz,
    consent_payload   jsonb       NOT NULL DEFAULT '{}'::jsonb
);
CREATE INDEX ix_consent_requests_abha ON abdm.consent_requests(abha_profile_id);
CREATE INDEX ix_consent_requests_status ON abdm.consent_requests(clinic_id, status);

CREATE TABLE abdm.consent_artefacts (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    consent_request_id uuid       NOT NULL REFERENCES abdm.consent_requests(id),
    artefact_id       text        NOT NULL UNIQUE,                -- issued by Consent Manager
    signature         text        NOT NULL,                        -- CM signature
    schema_version    text        NOT NULL,
    artefact_payload  jsonb       NOT NULL,                        -- full signed artefact
    issued_at         timestamptz NOT NULL,
    valid_from        timestamptz NOT NULL,
    valid_to          timestamptz NOT NULL,
    revoked_at        timestamptz,
    revocation_reason text
);

-- ---------------------------------------------------------------------
-- HEALTH INFORMATION EXCHANGE (HIP responses + HIU fetches)
-- ---------------------------------------------------------------------
CREATE TABLE abdm.hi_exchange_sessions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    consent_artefact_id uuid      NOT NULL REFERENCES abdm.consent_artefacts(id),
    role              text        NOT NULL CHECK (role IN ('HIP','HIU')),
    transaction_id    text        NOT NULL UNIQUE,
    key_material      jsonb       NOT NULL,                        -- ECDH public key + nonce (never the private key)
    requested_at      timestamptz NOT NULL DEFAULT now(),
    completed_at      timestamptz,
    status            text        NOT NULL CHECK (status IN ('initiated','ciphered','delivered','acknowledged','failed')),
    error_code        text,
    error_detail      text
);

CREATE TABLE abdm.hi_care_bundle_log (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    hi_exchange_session_id uuid   NOT NULL REFERENCES abdm.hi_exchange_sessions(id) ON DELETE CASCADE,
    care_context_ref  text        NOT NULL,
    bundle_sha256     bytea       NOT NULL,                        -- hash only; raw FHIR bundle streamed to S3
    bundle_s3_key     text        NOT NULL,
    sent_at           timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- ABDM GATEWAY REQUEST LOG (every outbound call — for audit + retry)
-- ---------------------------------------------------------------------
CREATE TABLE abdm.gateway_requests (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    requested_at      timestamptz NOT NULL DEFAULT now(),
    api_path          text        NOT NULL,                        -- e.g. '/v0.5/users/auth/init'
    http_method       text        NOT NULL,
    correlation_id    uuid        NOT NULL,
    request_body      jsonb,
    response_status   int,
    response_body     jsonb,
    duration_ms       int,
    outcome           text        NOT NULL CHECK (outcome IN ('success','gateway_error','timeout','validation_error','auth_error','retried')),
    PRIMARY KEY (id, requested_at)
) PARTITION BY RANGE (requested_at);

CREATE INDEX ix_gw_correlation ON abdm.gateway_requests(correlation_id);
CREATE INDEX ix_gw_outcome_clinic ON abdm.gateway_requests(clinic_id, outcome, requested_at DESC);

CREATE TABLE abdm.gateway_requests_2026_04 PARTITION OF abdm.gateway_requests
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE abdm.gateway_requests_2026_05 PARTITION OF abdm.gateway_requests
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- ---------------------------------------------------------------------
-- AADHAAR eKYC (via UIDAI) — stored under strict retention
-- We store ONLY the eKYC reference, NEVER the Aadhaar number in plaintext in ad-hoc columns.
-- ---------------------------------------------------------------------
CREATE TABLE abdm.aadhaar_ekyc_requests (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_id        uuid        REFERENCES core.patients(id),
    staff_id          uuid        REFERENCES core.staff(id),
    aadhaar_reference_hash bytea  NOT NULL,                        -- HMAC(Aadhaar, tenant_salt) — never raw
    txn_id            text        NOT NULL UNIQUE,                -- UIDAI transaction id
    auth_mode         text        NOT NULL CHECK (auth_mode IN ('otp','biometric_fp','biometric_iris','demographic')),
    status            text        NOT NULL CHECK (status IN ('initiated','otp_sent','verified','failed','expired')),
    requested_at      timestamptz NOT NULL DEFAULT now(),
    verified_at       timestamptz,
    ekyc_payload_encrypted bytea,                                  -- KMS envelope-encrypted UIDAI response
    error_code        text,
    error_detail      text,

    CONSTRAINT ck_ekyc_subject CHECK (patient_id IS NOT NULL OR staff_id IS NOT NULL)
);
CREATE INDEX ix_ekyc_patient ON abdm.aadhaar_ekyc_requests(patient_id);
CREATE INDEX ix_ekyc_staff   ON abdm.aadhaar_ekyc_requests(staff_id);

-- ---------------------------------------------------------------------
-- SUBSCRIPTION CALLBACKS (HIU subscribes to a HIP's new care contexts)
-- ---------------------------------------------------------------------
CREATE TABLE abdm.subscription_callbacks (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    abha_profile_id   uuid        NOT NULL REFERENCES abdm.abha_profiles(id),
    subscription_id   text        NOT NULL UNIQUE,                -- ABDM subscription id
    subscriber_hiu_id text        NOT NULL,
    hi_types          text[]      NOT NULL,
    status            text        NOT NULL CHECK (status IN ('active','paused','revoked','expired')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='abdm'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
    LOOP
        EXECUTE format('ALTER TABLE abdm.%I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format(
          'CREATE POLICY p_%1$s_tenant ON abdm.%1$I
           USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
           WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
    END LOOP;
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='abdm'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
    LOOP
        PERFORM core.attach_standard_triggers('abdm', t);
    END LOOP;
END $$;
