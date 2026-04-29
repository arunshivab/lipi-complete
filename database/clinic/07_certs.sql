-- =====================================================================
-- LiPi :: clinic :: 07_certs
-- Professional certifications, board registrations, mandatory training,
-- CME credits, continuing education tracking, competency assessments.
-- Prereq: 00_common + 01_core + 02_identity applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS certs;
SET search_path = certs, core, public;

-- ---------------------------------------------------------------------
-- CREDENTIAL TYPES & ISSUING AUTHORITIES
-- Medical degrees (MBBS, MD, MS, DNB), board registrations, specialist certs
-- ---------------------------------------------------------------------
CREATE TABLE certs.credential_types (
    code              text        PRIMARY KEY,                      -- 'mbbs','md','ms','dnb','frcs','fams','nrf','bls','acls'
    credential_name   text        NOT NULL,
    credential_level  text        NOT NULL CHECK (credential_level IN ('basic','intermediate','advanced','specialist','super_specialist')),
    category          text        NOT NULL CHECK (category IN ('medical_degree','surgical_degree','nursing_degree','allied_health','specialist_board','support_services','technical','non_clinical')),
    issuing_authority text        NOT NULL,                         -- 'MCI','NMC','INC','IBPS','Royal College',... + issuing_country
    issuing_country   char(2)     NOT NULL,                         -- ISO 3166-1 alpha-2
    validity_years    int,                                           -- NULL if perpetual (degree); years for renewal-required certs
    mandatory_for_roles text[],                                      -- e.g. ARRAY['medical_doctor','surgeon']
    mci_recognised    boolean     NOT NULL DEFAULT false,
    international_equiv text,                                        -- ISO/IEC equivalence (for mutual recognition)
    description       text
);

-- Individual credentials held by staff
CREATE TABLE certs.staff_credentials (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    credential_type_code text      NOT NULL REFERENCES certs.credential_types(code),
    credential_number text        NOT NULL,                         -- e.g. MCI reg #, board cert #
    issued_date       date        NOT NULL,
    issued_by         text,                                          -- authority that issued it
    valid_from        date        NOT NULL,
    valid_to          date,                                          -- NULL if perpetual
    country_issued    char(2),                                       -- ISO country code
    discipline_specialisation text,                                  -- e.g. 'Diagnostic Radiology', 'Pediatric Oncology'
    sub_specialisation text,                                         -- e.g. 'Interventional Radiology'
    verification_status text       NOT NULL DEFAULT 'unverified'
                                    CHECK (verification_status IN ('unverified','verification_pending','verified','expired','revoked','suspended')),
    verification_method text       CHECK (verification_method IN ('manual_review','online_portal','issuing_authority_confirmation','third_party_api')),
    verified_at       timestamptz,
    verified_by       uuid,
    certificate_s3_key text,                                         -- scanned copy
    portal_url        text,                                          -- online verification link
    renewal_required  boolean     NOT NULL DEFAULT false,
    renewal_due_date  date,
    is_primary        boolean     NOT NULL DEFAULT false,            -- primary qualification for role
    scope_limitations text,                                          -- restrictions (e.g. 'limited_to_ct_mri')
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_staff_credential UNIQUE (clinic_id, staff_id, credential_type_code)
);
CREATE INDEX ix_credentials_staff ON certs.staff_credentials(staff_id, clinic_id);
CREATE INDEX ix_credentials_renewal ON certs.staff_credentials(clinic_id, renewal_due_date) WHERE renewal_required;
CREATE INDEX ix_credentials_verified ON certs.staff_credentials(clinic_id, verification_status);

-- Board registrations (NMC, NURSING COUNCIL, INC, etc.)
CREATE TABLE certs.board_registrations (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    board_code        text        NOT NULL,                         -- 'nmc_doctor','nmc_nurse','icc_acupuncturist','icmr_technician'
    board_name        text        NOT NULL,
    registration_number text       NOT NULL,
    registration_state text,                                        -- state where registered
    registered_date   date        NOT NULL,
    valid_from        date        NOT NULL,
    valid_to          date,
    renewal_date      date,
    registration_status text       NOT NULL DEFAULT 'active'
                                    CHECK (registration_status IN ('active','lapsed','renewed','suspended','revoked','cancelled')),
    suspension_reason text,
    suspension_from   date,
    suspension_to     date,
    certificate_s3_key text,
    online_verify_url text,
    is_primary        boolean     NOT NULL DEFAULT false,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_board_reg UNIQUE (clinic_id, staff_id, board_code)
);
CREATE INDEX ix_board_regs_staff ON certs.board_registrations(staff_id, clinic_id);
CREATE INDEX ix_board_regs_renewal ON certs.board_registrations(clinic_id, renewal_date) WHERE registration_status='active';

-- CME (Continuing Medical Education) & CPD (Continuing Professional Development) credits
CREATE TABLE certs.cme_credits (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    credit_type       text        NOT NULL CHECK (credit_type IN ('conference','workshop','online_course','journal_club','grand_rounds','publication','teaching','research')),
    credit_provider   text        NOT NULL,                         -- 'IMA','NMC','ASCO','European Board',...
    credit_points     numeric(6,2) NOT NULL,                        -- hours or points awarded
    credit_date       date        NOT NULL,
    activity_title    text        NOT NULL,
    activity_location text,
    certificate_s3_key text,
    credit_identifier text,                                          -- unique ID from provider
    approved          boolean     NOT NULL DEFAULT false,
    approved_by       uuid,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_cme_staff_date ON certs.cme_credits(staff_id, credit_date DESC);
CREATE INDEX ix_cme_provider ON certs.cme_credits(credit_provider, credit_date DESC);

-- CME/CPD rolling targets & tracking (by year/period)
CREATE TABLE certs.cme_requirements (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    year              int         NOT NULL,
    requirement_type  text        NOT NULL CHECK (requirement_type IN ('cme','cpd','specialty_maintenance','board_recertification')),
    required_credits  numeric(6,2) NOT NULL,
    earned_credits    numeric(6,2) NOT NULL DEFAULT 0,
    deadline_date     date        NOT NULL,
    status            text        NOT NULL DEFAULT 'in_progress'
                                    CHECK (status IN ('in_progress','completed','partial','overdue','exempted')),
    exemption_reason  text,
    exempted_by       uuid,
    exempted_at       timestamptz,
    comments          text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_cme_req UNIQUE (clinic_id, staff_id, year, requirement_type)
);
CREATE INDEX ix_cme_req_deadline ON certs.cme_requirements(clinic_id, deadline_date) WHERE status IN ('in_progress','partial','overdue');

-- Competency assessments & evaluations (clinical, technical, teaching, research)
CREATE TABLE certs.competency_assessments (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    assessment_date   date        NOT NULL,
    assessment_type   text        NOT NULL CHECK (assessment_type IN
                                   ('clinical_skills','procedural_competency','communication','teaching_effectiveness','research_output','patient_safety','team_work','knowledge_test')),
    assessor_id       uuid        NOT NULL REFERENCES core.staff(id),  -- supervising physician/senior
    competency_level  text        NOT NULL CHECK (competency_level IN ('novice','beginner','competent','proficient','expert','needs_remediation')),
    score             numeric(5,2),
    max_score         numeric(5,2),
    comments          text,
    evidence_documents text[],                                       -- s3 keys of supporting docs
    action_plan       text,                                          -- if needs_remediation
    followup_date     date,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_competency_staff_date ON certs.competency_assessments(staff_id, assessment_date DESC);
CREATE INDEX ix_competency_type ON certs.competency_assessments(assessment_type, clinic_id, assessment_date DESC);

-- Training & development records (induction, onboarding, role-specific, incident response drills)
CREATE TABLE certs.training_records (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    training_code     text        NOT NULL,                         -- e.g. 'INDUCTION_2026','FIRE_DRILL_2026_Q1','RT_QA_ANNUAL'
    training_name     text        NOT NULL,
    training_category text        NOT NULL CHECK (training_category IN
                                   ('induction','onboarding','mandatory_annual','role_specific','regulatory','safety_drill','incident_response','skills_upgrade','other')),
    start_date        date        NOT NULL,
    end_date          date,
    duration_hours    numeric(6,2),
    trainer_name      text,
    trainer_org       text,
    training_mode     text        NOT NULL CHECK (training_mode IN ('in_person','online','hybrid','self_paced','on_job')),
    completion_status text        NOT NULL DEFAULT 'in_progress'
                                    CHECK (completion_status IN ('in_progress','completed','cancelled','deferred')),
    test_score        numeric(5,2),
    test_required     boolean     NOT NULL DEFAULT false,
    test_passed       boolean,
    certificate_s3_key text,
    competency_verified boolean    NOT NULL DEFAULT false,
    verified_by       uuid,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_training_staff_category ON certs.training_records(staff_id, training_category, end_date DESC);
CREATE INDEX ix_training_completion ON certs.training_records(clinic_id, completion_status, end_date);

-- Licence hold / disciplinary actions affecting credentials
CREATE TABLE certs.credential_holds (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    hold_reason       text        NOT NULL CHECK (hold_reason IN
                                   ('criminal_investigation','malpractice_suit','credential_verification_pending','integrity_concern','patient_complaint','regulatory_action')),
    held_from         timestamptz NOT NULL DEFAULT now(),
    held_until        timestamptz,
    hold_details      text,
    imposed_by        uuid,
    appeal_filed      boolean     NOT NULL DEFAULT false,
    appeal_date       date,
    appeal_outcome    text,
    lifted_at         timestamptz,
    lifted_by         uuid,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_holds_staff ON certs.credential_holds(staff_id, held_from DESC);
CREATE INDEX ix_holds_active ON certs.credential_holds(clinic_id, held_until) WHERE held_until IS NULL;

-- Publication & research output tracking (for CV, appraisal, academic standing)
CREATE TABLE certs.publications (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    publication_date  date        NOT NULL,
    publication_type  text        NOT NULL CHECK (publication_type IN ('peer_reviewed_journal','conference_paper','book_chapter','editorial','case_report','letter')),
    journal_name      text,
    conference_name   text,
    title             text        NOT NULL,
    authors           text,                                          -- comma-separated author list
    author_position   int,                                           -- position in author list
    doi               text,
    pmid              text,
    url               text,
    impact_factor     numeric(6,3),                                  -- journal IF if applicable
    keywords          text[],
    abstract          text,
    is_corresponding_author boolean NOT NULL DEFAULT false,
    citation_count    int,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_publications_staff_date ON certs.publications(staff_id, publication_date DESC);
CREATE INDEX ix_publications_type ON certs.publications(publication_type, publication_date DESC);

-- Awards, honours, recognitions
CREATE TABLE certs.awards (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    award_date        date        NOT NULL,
    award_name        text        NOT NULL,
    awarding_organization text     NOT NULL,
    award_category    text,                                          -- 'clinical_excellence','research','teaching','patient_care','innovation',...
    award_level       text,                                          -- 'departmental','institutional','state','national','international'
    certificate_s3_key text,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_awards_staff_date ON certs.awards(staff_id, award_date DESC);

-- Audit of credential & certification state changes
CREATE TABLE certs.credential_audit_log (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    logged_at         timestamptz NOT NULL DEFAULT now(),
    entity_type       text        NOT NULL,                         -- 'credential','board_reg','cme','training',...
    entity_id         uuid        NOT NULL,
    staff_id          uuid,
    action            text        NOT NULL,                         -- 'created','updated','verified','renewed','expired','revoked',...
    actor_user_id     uuid,
    old_state         jsonb,
    new_state         jsonb,
    PRIMARY KEY (id, logged_at)
) PARTITION BY RANGE (logged_at);

CREATE TABLE certs.credential_audit_log_2026_04 PARTITION OF certs.credential_audit_log
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE certs.credential_audit_log_2026_05 PARTITION OF certs.credential_audit_log
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

CREATE INDEX ix_cred_audit_entity ON certs.credential_audit_log(entity_type, entity_id);
CREATE INDEX ix_cred_audit_staff ON certs.credential_audit_log(staff_id, logged_at DESC);

-- Credential verification checklist (before hiring/privileging)
CREATE TABLE certs.credential_verification_checklist (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    checklist_date    date        NOT NULL,
    medical_degree_verified boolean NOT NULL DEFAULT false,
    board_registration_verified boolean NOT NULL DEFAULT false,
    liability_insurance_verified boolean NOT NULL DEFAULT false,
    background_check_verified boolean NOT NULL DEFAULT false,
    references_verified boolean    NOT NULL DEFAULT false,
    previous_employment_verified boolean NOT NULL DEFAULT false,
    disciplinary_history_checked boolean NOT NULL DEFAULT false,
    all_verified      boolean     NOT NULL DEFAULT false,
    verified_by       uuid,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_checklist_staff ON certs.credential_verification_checklist(staff_id, checklist_date DESC);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='certs'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('credential_types')
    LOOP
        EXECUTE format('ALTER TABLE certs.%I ENABLE ROW LEVEL SECURITY', t);
        BEGIN
            EXECUTE format(
              'CREATE POLICY p_%1$s_tenant ON certs.%1$I
               USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
               WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
        EXCEPTION WHEN undefined_column THEN NULL;
        END;
    END LOOP;
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='certs'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('credential_types')
    LOOP
        PERFORM core.attach_standard_triggers('certs', t);
    END LOOP;
END $$;
