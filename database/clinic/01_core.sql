-- =====================================================================
-- LiPi :: clinic :: 01_core
-- Per-clinic foundational schema: reference data, medical codesets,
-- facility topology, person/patient/staff master, consent.
-- Every other clinic schema FK's into this.
-- Prereq: 00_common/* applied.
-- =====================================================================

-- core schema was already created by 002_uuid_v7.sql; ensure it exists
CREATE SCHEMA IF NOT EXISTS core;
SET search_path = core, public;

-- =====================================================================
-- 1. GEOGRAPHY & ADDRESS REFERENCE
-- =====================================================================
CREATE TABLE core.countries (
    code              char(2)     PRIMARY KEY,
    name              text        NOT NULL,
    iso3              char(3)     NOT NULL,
    dial_code         text        NOT NULL
);

CREATE TABLE core.states (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    country_code      char(2)     NOT NULL REFERENCES core.countries(code),
    name              text        NOT NULL,
    code              text        NOT NULL,
    CONSTRAINT uq_states_country_code UNIQUE (country_code, code)
);

CREATE TABLE core.cities (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    state_id          uuid        NOT NULL REFERENCES core.states(id),
    name              text        NOT NULL,
    latitude          numeric(9,6),
    longitude         numeric(9,6)
);
CREATE INDEX ix_cities_state_name ON core.cities(state_id, name);

CREATE TABLE core.pincodes (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    city_id           uuid        NOT NULL REFERENCES core.cities(id),
    pincode           core.d_pincode_in NOT NULL,
    area              text,
    CONSTRAINT uq_pincodes_city_pin UNIQUE (city_id, pincode)
);
CREATE INDEX ix_pincodes_pin ON core.pincodes(pincode);

-- Reusable structured address (patients, staff, organizations)
CREATE TABLE core.addresses (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    use_type          text        NOT NULL CHECK (use_type IN ('home','work','temp','old','billing','emergency')),
    line1             text        NOT NULL,
    line2             text,
    landmark          text,
    city_id           uuid        REFERENCES core.cities(id),
    state_id          uuid        REFERENCES core.states(id),
    pincode           core.d_pincode_in,
    country_code      char(2)     NOT NULL DEFAULT 'IN' REFERENCES core.countries(code),
    latitude          numeric(9,6),
    longitude         numeric(9,6),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_addresses_clinic ON core.addresses(clinic_id);

-- =====================================================================
-- 2. MEDICAL CODESETS (populated from authoritative sources — seed separately)
-- =====================================================================
CREATE TABLE core.icd10_codes (
    code              text        PRIMARY KEY,
    description       text        NOT NULL,
    chapter           text,
    category          text,
    is_billable       boolean     NOT NULL DEFAULT true
);
CREATE INDEX ix_icd10_description_trgm ON core.icd10_codes USING gin (description gin_trgm_ops);

CREATE TABLE core.snomed_codes (
    code              text        PRIMARY KEY,
    fully_specified_name text     NOT NULL,
    preferred_term    text        NOT NULL,
    semantic_tag      text,
    is_active         boolean     NOT NULL DEFAULT true
);
CREATE INDEX ix_snomed_term_trgm ON core.snomed_codes USING gin (preferred_term gin_trgm_ops);

CREATE TABLE core.loinc_codes (
    code              text        PRIMARY KEY,
    long_name         text        NOT NULL,
    short_name        text,
    component         text,
    property          text,
    scale_type        text,
    method_type       text
);

CREATE TABLE core.rxnorm_codes (
    rxcui             text        PRIMARY KEY,
    name              text        NOT NULL,
    tty               text        NOT NULL,                -- term type (SCD, SBD, IN, BN, ...)
    is_active         boolean     NOT NULL DEFAULT true
);
CREATE INDEX ix_rxnorm_name_trgm ON core.rxnorm_codes USING gin (name gin_trgm_ops);

-- CDSCO Medical Device Classification (India-specific)
CREATE TABLE core.cdsco_device_classes (
    code              text        PRIMARY KEY,             -- A / B / C / D
    description       text        NOT NULL,
    risk_level        text        NOT NULL
);

-- =====================================================================
-- 3. FACILITY TOPOLOGY (buildings → wings → floors → wards → rooms → beds)
-- =====================================================================
CREATE TABLE core.facility (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    name              text        NOT NULL,
    code              text        NOT NULL,
    address_id        uuid        REFERENCES core.addresses(id),
    contact           jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,
    CONSTRAINT uq_facility_clinic_code UNIQUE (clinic_id, code)
);

CREATE TABLE core.departments (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    facility_id       uuid        REFERENCES core.facility(id),
    code              text        NOT NULL,                -- 'ONCO', 'RAD', 'PATH'
    name              text        NOT NULL,
    department_type   text        NOT NULL CHECK (department_type IN
                                   ('clinical','diagnostic','support','admin','emergency','oncology_rt','oncology_med','oncology_surg','nuclear_med')),
    head_staff_id     uuid,                                 -- FK added later via ALTER
    cost_centre_code  text,
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_dept_clinic_code UNIQUE (clinic_id, code)
);
CREATE INDEX ix_departments_clinic_type ON core.departments(clinic_id, department_type);

CREATE TABLE core.specialties (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    code              text        NOT NULL UNIQUE,         -- e.g. 'oncology.medical', 'radiology.interventional'
    name              text        NOT NULL,
    parent_id         uuid        REFERENCES core.specialties(id)
);

CREATE TABLE core.wards (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    department_id     uuid        NOT NULL REFERENCES core.departments(id),
    code              text        NOT NULL,
    name              text        NOT NULL,
    ward_type         text        NOT NULL CHECK (ward_type IN
                                   ('general','semi_private','private','deluxe','icu','ccu','nicu','picu','hdu','isolation','oncology','post_op')),
    floor             text,
    bed_capacity      int         NOT NULL DEFAULT 0,
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_wards_clinic_code UNIQUE (clinic_id, code)
);

CREATE TABLE core.rooms (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    ward_id           uuid        NOT NULL REFERENCES core.wards(id),
    room_number       text        NOT NULL,
    room_type         text        NOT NULL CHECK (room_type IN
                                   ('standard','ot','linac','ct','mri','nm','console','consult','procedure','recovery','storage')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_rooms_ward_number UNIQUE (ward_id, room_number)
);

CREATE TABLE core.beds (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    room_id           uuid        NOT NULL REFERENCES core.rooms(id),
    bed_number        text        NOT NULL,
    bed_type          text        NOT NULL CHECK (bed_type IN
                                   ('standard','icu','isolation','paediatric','bariatric','oncology_day','recovery')),
    status            text        NOT NULL DEFAULT 'available'
                                    CHECK (status IN ('available','occupied','reserved','blocked','cleaning','maintenance')),
    current_admission_id uuid,                             -- FK added in 02_ipd.sql (deferred)
    is_active         boolean     NOT NULL DEFAULT true,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_beds_room_number UNIQUE (room_id, bed_number)
);
CREATE INDEX ix_beds_status ON core.beds(clinic_id, status);

-- =====================================================================
-- 4. PERSON (shared base — patients, staff, kin — single identity)
-- =====================================================================
CREATE TABLE core.persons (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    title             text        CHECK (title IN ('Mr','Mrs','Ms','Dr','Prof','Mx','Rev')),
    given_name        text        NOT NULL,
    middle_name       text,
    family_name       text,
    display_name      text        GENERATED ALWAYS AS (
                                    trim(both ' ' from
                                         coalesce(given_name,'') || ' ' ||
                                         coalesce(middle_name||' ','') ||
                                         coalesce(family_name,''))) STORED,
    gender            text        NOT NULL CHECK (gender IN ('male','female','other','unknown')),
    date_of_birth     date,
    blood_group       text        CHECK (blood_group IN ('A+','A-','B+','B-','AB+','AB-','O+','O-','unknown')),
    marital_status    text        CHECK (marital_status IN ('single','married','divorced','widowed','separated','unknown')),
    nationality_code  char(2)     DEFAULT 'IN' REFERENCES core.countries(code),
    preferred_language text       DEFAULT 'en',
    photo_s3_key      text,
    extension_data    jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    updated_by        uuid,
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_persons_clinic_name_trgm ON core.persons USING gin (display_name gin_trgm_ops);
CREATE INDEX ix_persons_dob               ON core.persons(clinic_id, date_of_birth);

-- Multiple contact points (email/phone/fax) per person
CREATE TABLE core.contact_points (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    person_id         uuid        NOT NULL REFERENCES core.persons(id) ON DELETE CASCADE,
    system            text        NOT NULL CHECK (system IN ('phone','email','fax','url','sms','whatsapp')),
    value             text        NOT NULL,
    use_type          text        CHECK (use_type IN ('home','work','mobile','temp','old')),
    is_primary        boolean     NOT NULL DEFAULT false,
    is_verified       boolean     NOT NULL DEFAULT false,
    verified_at       timestamptz,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_contact_points_person ON core.contact_points(person_id);
CREATE INDEX ix_contact_points_value  ON core.contact_points(value);

-- Address linking (a person may have multiple addresses)
CREATE TABLE core.person_addresses (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    person_id         uuid        NOT NULL REFERENCES core.persons(id) ON DELETE CASCADE,
    address_id        uuid        NOT NULL REFERENCES core.addresses(id),
    is_primary        boolean     NOT NULL DEFAULT false,
    valid_from        date,
    valid_to          date,
    CONSTRAINT uq_person_addresses UNIQUE (person_id, address_id)
);

-- =====================================================================
-- 5. PATIENT (extends person)
-- =====================================================================
CREATE TABLE core.patients (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    person_id         uuid        NOT NULL UNIQUE REFERENCES core.persons(id),
    mrn               text        NOT NULL,                -- Medical Record Number, clinic-local
    uhid              text,                                -- Universal Health ID if assigned at org level
    registration_date timestamptz NOT NULL DEFAULT now(),
    registered_by     uuid,                                 -- staff_id FK added later
    vip_flag          boolean     NOT NULL DEFAULT false,
    deceased          boolean     NOT NULL DEFAULT false,
    deceased_datetime timestamptz,
    death_cause_icd10 text        REFERENCES core.icd10_codes(code),
    organ_donor       boolean     NOT NULL DEFAULT false,
    primary_doctor_id uuid,                                 -- staff_id FK added later
    occupation        text,
    referral_source   text,
    patient_type      text        NOT NULL DEFAULT 'general'
                                    CHECK (patient_type IN ('general','vip','international','staff','cgs','ecs','charity','research')),
    active_alerts     jsonb       NOT NULL DEFAULT '[]'::jsonb,   -- allergies summary, isolation, etc.
    extension_data    jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    updated_by        uuid,
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_patients_clinic_mrn UNIQUE (clinic_id, mrn)
);
CREATE INDEX ix_patients_clinic   ON core.patients(clinic_id) WHERE deleted_at IS NULL;
CREATE INDEX ix_patients_uhid     ON core.patients(uhid)      WHERE uhid IS NOT NULL;

-- All external identifiers (ABHA, Aadhaar, passport, PAN, insurance IDs, foreign IDs)
-- Kept in one table so we can search any identifier fast.
CREATE TABLE core.patient_identifiers (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_id        uuid        NOT NULL REFERENCES core.patients(id) ON DELETE CASCADE,
    identifier_type   text        NOT NULL CHECK (identifier_type IN
                                   ('abha_number','abha_address','aadhaar','pan','passport',
                                    'voter_id','driving_licence','insurance','tpa','employee_id',
                                    'student_id','foreign_national_id','other')),
    identifier_value  text        NOT NULL,
    issuing_authority text,
    valid_from        date,
    valid_to          date,
    is_verified       boolean     NOT NULL DEFAULT false,
    verified_at       timestamptz,
    verification_ref  text,                                 -- UIDAI txn id, ABDM linkToken, etc.
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_patient_identifier UNIQUE (patient_id, identifier_type, identifier_value)
);
CREATE INDEX ix_patient_identifiers_value ON core.patient_identifiers(identifier_type, identifier_value);
CREATE INDEX ix_patient_identifiers_patient ON core.patient_identifiers(patient_id);

CREATE TABLE core.emergency_contacts (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_id        uuid        NOT NULL REFERENCES core.patients(id) ON DELETE CASCADE,
    name              text        NOT NULL,
    relationship      text        NOT NULL CHECK (relationship IN
                                   ('spouse','parent','child','sibling','guardian','friend','other')),
    phone             core.d_phone_in NOT NULL,
    email             core.d_email,
    is_primary        boolean     NOT NULL DEFAULT false,
    address           jsonb,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_emergency_contacts_patient ON core.emergency_contacts(patient_id);

-- =====================================================================
-- 6. STAFF (extends person)
-- =====================================================================
CREATE TABLE core.staff (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    person_id         uuid        NOT NULL UNIQUE REFERENCES core.persons(id),
    employee_code     text        NOT NULL,                -- clinic-local staff ID
    staff_type        text        NOT NULL CHECK (staff_type IN
                                   ('doctor','nurse','technician','pharmacist','radiographer',
                                    'medical_physicist','dosimetrist','therapist','admin','support','manager','lab_tech')),
    primary_specialty_id uuid     REFERENCES core.specialties(id),
    primary_department_id uuid    REFERENCES core.departments(id),
    qualification     text,                                 -- e.g. 'MBBS, MD Radiation Oncology'
    mci_registration_no text,                               -- NMC / MCI / state council number
    hpr_id            text,                                 -- ABDM Healthcare Professional Registry
    joining_date      date,
    exit_date         date,
    reporting_to_id   uuid        REFERENCES core.staff(id),
    on_call           boolean     NOT NULL DEFAULT false,
    is_active         boolean     NOT NULL DEFAULT true,
    extension_data    jsonb       NOT NULL DEFAULT '{}'::jsonb,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    deleted_at        timestamptz,
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_staff_clinic_code UNIQUE (clinic_id, employee_code)
);
CREATE INDEX ix_staff_clinic_type  ON core.staff(clinic_id, staff_type)    WHERE deleted_at IS NULL;
CREATE INDEX ix_staff_department   ON core.staff(primary_department_id)   WHERE deleted_at IS NULL;
CREATE INDEX ix_staff_hpr          ON core.staff(hpr_id)                   WHERE hpr_id IS NOT NULL;

-- Staff can belong to multiple departments (e.g. shared Rad-Onco + Nuclear-Med physicist)
CREATE TABLE core.staff_departments (
    staff_id          uuid        NOT NULL REFERENCES core.staff(id) ON DELETE CASCADE,
    department_id     uuid        NOT NULL REFERENCES core.departments(id) ON DELETE CASCADE,
    role_in_dept      text,
    is_primary        boolean     NOT NULL DEFAULT false,
    PRIMARY KEY (staff_id, department_id)
);

-- Now that staff exists, wire up the deferred FKs
ALTER TABLE core.departments
    ADD CONSTRAINT fk_departments_head FOREIGN KEY (head_staff_id) REFERENCES core.staff(id);

ALTER TABLE core.patients
    ADD CONSTRAINT fk_patients_registered_by FOREIGN KEY (registered_by) REFERENCES core.staff(id),
    ADD CONSTRAINT fk_patients_primary_doctor FOREIGN KEY (primary_doctor_id) REFERENCES core.staff(id);

-- =====================================================================
-- 7. CONSENT (DPDP Act 2023 + HIPAA — patient consent artefacts)
-- =====================================================================
CREATE TABLE core.consents (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    patient_id        uuid        NOT NULL REFERENCES core.patients(id),
    consent_type      text        NOT NULL CHECK (consent_type IN
                                   ('treatment','surgery','anaesthesia','chemotherapy','radiation',
                                    'research','data_sharing','abdm_link','hie_share','telemedicine',
                                    'photography','autopsy','organ_donation','dpdp_processing')),
    scope             jsonb       NOT NULL DEFAULT '{}'::jsonb,    -- FHIR-style provisions
    granted           boolean     NOT NULL,
    granted_at        timestamptz NOT NULL DEFAULT now(),
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    revoked_at        timestamptz,
    revocation_reason text,
    witness_staff_id  uuid        REFERENCES core.staff(id),
    signature_s3_key  text,                                 -- scanned / e-signed artefact
    language_used     text        DEFAULT 'en',
    abdm_consent_artefact_id text,                         -- ref to abdm.consent_artefacts
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    created_by        uuid,
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_consents_patient_type ON core.consents(patient_id, consent_type, granted);
CREATE INDEX ix_consents_valid        ON core.consents(valid_to) WHERE granted AND revoked_at IS NULL;

-- =====================================================================
-- 8. ROW-LEVEL SECURITY — belt & braces (DB is clinic-isolated, but RLS catches bugs)
-- =====================================================================
DO $$
DECLARE
    t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables
             WHERE schemaname = 'core'
               AND tablename NOT IN (
                   -- reference / lookup tables (no clinic_id)
                   'countries','states','cities','pincodes',
                   'icd10_codes','snomed_codes','loinc_codes',
                   'rxnorm_codes','cdsco_device_classes','specialties',
                   -- junction tables without their own clinic_id column
                   'staff_departments','person_addresses',
                   'patient_allergies','patient_diagnoses',
                   'patient_procedures','patient_medications'
               )
    LOOP
        EXECUTE format('ALTER TABLE core.%I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format(
          'CREATE POLICY p_%1$s_tenant ON core.%1$I
           USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
           WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
    END LOOP;
END $$;

-- =====================================================================
-- 9. STANDARD TRIGGERS
-- =====================================================================
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname = 'core'
             AND tablename NOT IN ('countries','states','cities','pincodes',
                                   'icd10_codes','snomed_codes','loinc_codes',
                                   'rxnorm_codes','cdsco_device_classes','specialties',
                                   'staff_departments','person_addresses')
    LOOP
        PERFORM core.attach_standard_triggers('core', t);
    END LOOP;
END $$;
