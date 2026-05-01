-- =====================================================================
-- LiPi HIS — Clinic Core Schema v3
-- Full redesign: persons+patients collapsed, append-only immutable records,
-- separate address table with plain text city/district/state,
-- customisable flag system, payer table
-- Run against each clinic DB after 00_common scripts
-- =====================================================================

-- Drop old schema cleanly before recreating
-- Safe in dev — only test data exists
DROP SCHEMA IF EXISTS core CASCADE;

CREATE SCHEMA IF NOT EXISTS core;
CREATE OR REPLACE FUNCTION core.uuid_v7()
RETURNS uuid LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_time_ms  bigint;
    v_rand     bytea;
    v_bytes    bytea;
BEGIN
    v_time_ms := (extract(epoch from clock_timestamp()) * 1000)::bigint;
    v_rand    := gen_random_bytes(10);
    v_bytes   := decode(lpad(to_hex(v_time_ms), 12, '0') || encode(v_rand, 'hex'), 'hex');
    v_bytes   := set_byte(v_bytes, 6, ((get_byte(v_bytes, 6) & 15) | 112));
    v_bytes   := set_byte(v_bytes, 8, ((get_byte(v_bytes, 8) & 63) | 128));
    RETURN encode(v_bytes, 'hex')::uuid;
END; $$;

-- Recreate trigger helpers (also dropped with schema)
CREATE OR REPLACE FUNCTION core.fn_set_updated_at()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at := now();
    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
    RETURN NEW;
END; $$;

CREATE OR REPLACE FUNCTION core.attach_standard_triggers(p_schema text, p_table text)
RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    EXECUTE format(
      'CREATE TRIGGER trg_%1$s_set_updated_at BEFORE UPDATE ON %2$I.%1$I
       FOR EACH ROW EXECUTE FUNCTION core.fn_set_updated_at();',
      p_table, p_schema);
END; $$;
SET search_path = core, public;

-- =====================================================================
-- 1. REFERENCE TABLES (mutable — not patient PHI)
-- =====================================================================

CREATE TABLE IF NOT EXISTS core.countries (
    code            char(2)     PRIMARY KEY,
    name            text        NOT NULL,
    iso3            char(3)     NOT NULL,
    dial_code       text        NOT NULL,
    is_active       boolean     NOT NULL DEFAULT true
);

CREATE TABLE IF NOT EXISTS core.states (
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    name            text        NOT NULL,
    code            text        NOT NULL,                -- ISO 3166-2:IN  e.g. IN-MH
    is_ut           boolean     NOT NULL DEFAULT false,
    is_active       boolean     NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_states_code UNIQUE (code)
);
CREATE INDEX ix_states_name ON core.states(name);

CREATE TABLE IF NOT EXISTS core.districts (
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    state_id        uuid        NOT NULL REFERENCES core.states(id),
    name            text        NOT NULL,
    is_aspirational boolean     NOT NULL DEFAULT false,  -- NITI Aayog ADP
    is_active       boolean     NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_districts_state_name UNIQUE (state_id, name)
);
CREATE INDEX ix_districts_state         ON core.districts(state_id);
CREATE INDEX ix_districts_aspirational  ON core.districts(is_aspirational) WHERE is_aspirational = true;
CREATE INDEX ix_districts_name_trgm     ON core.districts USING gin(name gin_trgm_ops);

CREATE TABLE IF NOT EXISTS core.cities (
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    district_id     uuid        NOT NULL REFERENCES core.districts(id),
    state_id        uuid        NOT NULL REFERENCES core.states(id),
    name            text        NOT NULL,
    is_district_hq  boolean     NOT NULL DEFAULT false,
    is_active       boolean     NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_cities_district_name UNIQUE (district_id, name)
);
CREATE INDEX ix_cities_district  ON core.cities(district_id);
CREATE INDEX ix_cities_name_trgm ON core.cities USING gin(name gin_trgm_ops);

-- =====================================================================
-- 2. MEDICAL CODESETS
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.icd10_codes (
    code            text        PRIMARY KEY,
    description     text        NOT NULL,
    chapter         text,
    is_active       boolean     NOT NULL DEFAULT true
);

-- =====================================================================
-- 3. FLAG DEFINITIONS (clinic-configurable — not PHI)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.flag_definitions (
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id       uuid        NOT NULL,
    code            text        NOT NULL,
    label           text        NOT NULL,
    color_hex       char(7)     NOT NULL DEFAULT '#F59E0B',
    icon            text,
    description     text,
    requires_note   boolean     NOT NULL DEFAULT false,
    is_active       boolean     NOT NULL DEFAULT true,
    sort_order      int         NOT NULL DEFAULT 0,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    created_by      uuid,
    CONSTRAINT uq_flag_def_clinic_code UNIQUE (clinic_id, code)
);
CREATE INDEX ix_flag_def_clinic ON core.flag_definitions(clinic_id) WHERE is_active = true;

-- =====================================================================
-- 4. PATIENTS — IMMUTABLE / APPEND-ONLY
--
-- RULES:
--   • No row is ever UPDATE'd or DELETE'd
--   • Every change INSERT's a new row with new id
--   • entity_id is the STABLE patient identity — used in all FK references
--   • previous_id links the new version to the version it supersedes
--   • current version  = WHERE valid_to IS NULL
--   • history          = WHERE entity_id = X ORDER BY valid_from
--   • "deleted" record = new version with change_reason = 'deleted'
--                        and valid_to set on the prior version by the app
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.patients (
    -- Version identity
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    entity_id       uuid        NOT NULL,
    previous_id     uuid        REFERENCES core.patients(id),
    valid_from      timestamptz NOT NULL DEFAULT now(),
    valid_to        timestamptz,
    changed_by      uuid,
    change_reason   text,

    -- Identity
    clinic_id       uuid        NOT NULL,
    title           text        CHECK (title IN (
                                    'Mr','Mrs','Ms','Miss','Dr','Prof',
                                    'Mx','Rev','Baby','Master')),
    first_name      text        NOT NULL,
    middle_name     text,
    last_name       text        NOT NULL,
    display_name    text        GENERATED ALWAYS AS (
                                    trim(both ' ' from
                                        coalesce(title || ' ', '') ||
                                        coalesce(first_name, '') || ' ' ||
                                        coalesce(middle_name || ' ', '') ||
                                        coalesce(last_name, ''))) STORED,
    gender          text        NOT NULL CHECK (gender IN (
                                    'male','female','other','unknown')),
    date_of_birth   date        NOT NULL,
    dob_confidence  text        NOT NULL DEFAULT 'self_reported'
                                CHECK (dob_confidence IN (
                                    'verified','self_reported','estimated','unknown')),
    blood_group     text        CHECK (blood_group IN (
                                    'A+','A-','B+','B-','AB+','AB-','O+','O-','unknown')),
    marital_status  text        CHECK (marital_status IN (
                                    'single','married','divorced','widowed','separated','unknown')),
    nationality_code char(2)    DEFAULT 'IN' REFERENCES core.countries(code),
    preferred_language text     DEFAULT 'en',
    photo_s3_key    text,

    -- Clinical registration
    uhid            text        NOT NULL,
    mrn             text,
    registration_date timestamptz NOT NULL DEFAULT now(),
    patient_type    text        NOT NULL DEFAULT 'general' CHECK (patient_type IN (
                                    'general','vip','international','staff',
                                    'cgs','ecs','charity','research')),
    deceased        boolean     NOT NULL DEFAULT false,
    deceased_at     timestamptz,
    death_cause_icd10 text      REFERENCES core.icd10_codes(code),
    occupation      text,
    referral_source text        CHECK (referral_source IN (
                                    'doctor','self','emergency','camp','transfer','online','other')),
    referral_channel text       CHECK (referral_channel IN (
                                    'google','friend','social_media','advertisement',
                                    'camp','hospital_website','other')),
    referred_by     text,
    registered_by   uuid,

    -- Socioeconomic overflow
    extension_data  jsonb       NOT NULL DEFAULT '{}'::jsonb,

    -- Audit
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now()
);

-- One current version per patient
CREATE UNIQUE INDEX uq_patient_entity_current
    ON core.patients(entity_id) WHERE valid_to IS NULL;
-- UHID unique across active patients
CREATE UNIQUE INDEX uq_patient_uhid_current
    ON core.patients(uhid) WHERE valid_to IS NULL;
-- MRN unique per clinic when assigned
CREATE UNIQUE INDEX uq_patient_mrn_clinic
    ON core.patients(clinic_id, mrn)
    WHERE mrn IS NOT NULL AND valid_to IS NULL;

CREATE INDEX ix_patients_clinic       ON core.patients(clinic_id) WHERE valid_to IS NULL;
CREATE INDEX ix_patients_name_trgm    ON core.patients USING gin(display_name gin_trgm_ops) WHERE valid_to IS NULL;
CREATE INDEX ix_patients_dob          ON core.patients(clinic_id, date_of_birth) WHERE valid_to IS NULL;
CREATE INDEX ix_patients_entity       ON core.patients(entity_id);
CREATE INDEX ix_patients_history      ON core.patients(entity_id, valid_from DESC);

-- =====================================================================
-- 5. CONTACT POINTS (immutable)
-- patient_entity_id references core.patients(entity_id) — app enforced
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.contact_points (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    entity_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    previous_id       uuid        REFERENCES core.contact_points(id),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    changed_by        uuid,
    change_reason     text,

    clinic_id         uuid        NOT NULL,
    system            text        NOT NULL CHECK (system IN (
                                      'phone','email','fax','url','sms','whatsapp')),
    value             text        NOT NULL,
    use_type          text        CHECK (use_type IN ('home','work','mobile','temp','old')),
    is_primary        boolean     NOT NULL DEFAULT false,
    is_verified       boolean     NOT NULL DEFAULT false,
    verified_at       timestamptz,
    created_at        timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX uq_contact_entity_current ON core.contact_points(entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_contact_patient   ON core.contact_points(patient_entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_contact_value     ON core.contact_points(value) WHERE valid_to IS NULL;

-- =====================================================================
-- 6. ADDRESSES (immutable)
-- Plain text city/state/district — no UUID FKs
-- is_aspirational set at save time from master.aspirational_districts
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.addresses (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    entity_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    previous_id       uuid        REFERENCES core.addresses(id),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    changed_by        uuid,
    change_reason     text,

    clinic_id         uuid        NOT NULL,
    address_type      text        NOT NULL DEFAULT 'current'
                                  CHECK (address_type IN ('current','permanent','work','temporary')),
    line1             text        NOT NULL,
    line2             text,
    district          text,
    city              text        NOT NULL,
    state             text        NOT NULL,
    pincode           text        CHECK (pincode ~ '^[1-9][0-9]{5}$'),
    country_code      char(2)     NOT NULL DEFAULT 'IN' REFERENCES core.countries(code),
    is_aspirational   boolean     NOT NULL DEFAULT false,
    created_at        timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX uq_address_entity_current ON core.addresses(entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_address_patient       ON core.addresses(patient_entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_address_district      ON core.addresses(district) WHERE valid_to IS NULL;
CREATE INDEX ix_address_aspirational  ON core.addresses(is_aspirational)
                                       WHERE is_aspirational = true AND valid_to IS NULL;
CREATE INDEX ix_address_pincode       ON core.addresses(pincode) WHERE valid_to IS NULL;

-- =====================================================================
-- 7. PATIENT IDENTIFIERS (immutable)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.patient_identifiers (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    entity_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    previous_id       uuid        REFERENCES core.patient_identifiers(id),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    changed_by        uuid,
    change_reason     text,

    clinic_id         uuid        NOT NULL,
    identifier_type   text        NOT NULL CHECK (identifier_type IN (
                                      'abha_number','abha_address','aadhaar','pan','passport',
                                      'voter_id','driving_licence','insurance','tpa',
                                      'employee_id','student_id','foreign_national_id','other')),
    identifier_value  text        NOT NULL,
    issuing_authority text,
    valid_from_date   date,
    valid_to_date     date,
    is_verified       boolean     NOT NULL DEFAULT false,
    verified_at       timestamptz,
    verification_ref  text,
    created_at        timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX uq_patientid_entity_current ON core.patient_identifiers(entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_patientid_patient ON core.patient_identifiers(patient_entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_patientid_value   ON core.patient_identifiers(identifier_type, identifier_value);

-- =====================================================================
-- 8. PATIENT PAYERS (immutable)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.patient_payers (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    entity_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    previous_id       uuid        REFERENCES core.patient_payers(id),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    changed_by        uuid,
    change_reason     text,

    clinic_id         uuid        NOT NULL,
    payer_type        text        NOT NULL DEFAULT 'self' CHECK (payer_type IN (
                                      'self','insurance','tpa','corporate','govt_scheme','other')),
    payer_name        text,
    policy_number     text,
    scheme_name       text,
    coverage_start    date,
    coverage_end      date,
    is_primary        boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid
);
CREATE UNIQUE INDEX uq_payer_entity_current ON core.patient_payers(entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_payer_patient ON core.patient_payers(patient_entity_id) WHERE valid_to IS NULL;

-- =====================================================================
-- 9. EMERGENCY CONTACTS (immutable)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.emergency_contacts (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    entity_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    previous_id       uuid        REFERENCES core.emergency_contacts(id),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    changed_by        uuid,
    change_reason     text,

    clinic_id         uuid        NOT NULL,
    name              text        NOT NULL,
    relationship      text        NOT NULL CHECK (relationship IN (
                                      'spouse','parent','child','sibling',
                                      'guardian','friend','other')),
    phone             text        NOT NULL,
    email             text,
    is_primary        boolean     NOT NULL DEFAULT false,
    address_json      jsonb,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX uq_emgcontact_entity_current ON core.emergency_contacts(entity_id) WHERE valid_to IS NULL;
CREATE INDEX ix_emgcontact_patient ON core.emergency_contacts(patient_entity_id) WHERE valid_to IS NULL;

-- =====================================================================
-- 10. PATIENT FLAGS (append-only — cleared, never deleted)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.patient_flags (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    flag_id           uuid        NOT NULL REFERENCES core.flag_definitions(id),
    note              text,
    flagged_by        uuid        NOT NULL,
    flagged_at        timestamptz NOT NULL DEFAULT now(),
    cleared_by        uuid,
    cleared_at        timestamptz,
    cleared_reason    text
);
CREATE UNIQUE INDEX uq_patient_flag_active
    ON core.patient_flags(patient_entity_id, flag_id) WHERE cleared_at IS NULL;
CREATE INDEX ix_patient_flags_patient ON core.patient_flags(patient_entity_id) WHERE cleared_at IS NULL;

-- =====================================================================
-- 11. CONSENTS (append-only)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.consents (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_entity_id uuid        NOT NULL,
    consent_type      text        NOT NULL CHECK (consent_type IN (
                                      'treatment','surgery','anaesthesia','chemotherapy',
                                      'radiation','research','data_sharing','abdm_link',
                                      'hie_share','telemedicine','photography','autopsy',
                                      'organ_donation','dpdp_processing')),
    granted           boolean     NOT NULL,
    granted_at        timestamptz NOT NULL DEFAULT now(),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    revoked_at        timestamptz,
    revocation_reason text,
    scope             jsonb       NOT NULL DEFAULT '{}'::jsonb,
    language_used     text        DEFAULT 'en',
    witness_name      text,
    signature_s3_key  text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid
);
CREATE INDEX ix_consents_patient ON core.consents(patient_entity_id, consent_type) WHERE revoked_at IS NULL;

-- =====================================================================
-- 12. DEPARTMENTS & SPECIALTIES (clinic-defined — standard mutable)
-- =====================================================================
CREATE TABLE IF NOT EXISTS core.departments (
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id       uuid        NOT NULL,
    code            text        NOT NULL,
    name            text        NOT NULL,
    department_type text        NOT NULL CHECK (department_type IN (
                                    'clinical','diagnostic','support','admin','emergency',
                                    'oncology_rt','oncology_med','oncology_surg','nuclear_med')),
    cost_centre_code text,
    is_active       boolean     NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_department_clinic_code UNIQUE (clinic_id, code)
);

CREATE TABLE IF NOT EXISTS core.specialties (
    id              uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id       uuid        NOT NULL,
    code            text        NOT NULL,
    name            text        NOT NULL,
    is_active       boolean     NOT NULL DEFAULT true,
    created_at      timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_specialty_clinic_code UNIQUE (clinic_id, code)
);

-- =====================================================================
-- COMMENTS
-- =====================================================================
COMMENT ON TABLE core.patients IS
  'Append-only patient record. No UPDATE or DELETE. Each change = new row with new id. '
  'entity_id is stable patient identity. previous_id chains versions. '
  'valid_to IS NULL = current version.';

COMMENT ON TABLE core.addresses IS
  'Append-only patient addresses. Plain text city/district/state — no UUID FKs. '
  'is_aspirational is denormalised from master.aspirational_districts at save time.';

COMMENT ON COLUMN core.patients.entity_id IS
  'Stable patient UUID across all versions. Used in FK references from all dependent tables.';

COMMENT ON COLUMN core.patients.previous_id IS
  'id of the version this row supersedes. NULL on first registration. '
  'Chain: current → previous → previous → ... → first_registration';

COMMENT ON COLUMN core.addresses.is_aspirational IS
  'Denormalised flag: true if district is in NITI Aayog Aspirational Districts Programme. '
  'Set at address save time. Background job refreshes when master.aspirational_districts changes.';
