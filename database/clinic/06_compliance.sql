-- =====================================================================
-- LiPi :: clinic :: 06_compliance
-- Regulatory licences (Fire, CGWA, MCA, RoC, CDSCO, AERB, NABH, etc),
-- responsibility matrix, physical-copy custody + audit log, renewal tracking, alerts.
-- Prereq: 00_common + 01_core + 02_identity applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS compliance;
SET search_path = compliance, core, public;

-- ---------------------------------------------------------------------
-- LICENCE TYPE CATALOGUE (reference)
-- Authority, document class, renewal cycle, mandatory for which departments
-- ---------------------------------------------------------------------
CREATE TABLE compliance.licence_types (
    code              text        PRIMARY KEY,                      -- 'fire_noc','cgwa_consent','mca','roc_cert','cdsco_import','aerb','nabh'
    name              text        NOT NULL,
    issuing_authority text        NOT NULL,                         -- 'Fire Department','CGWA','Registrar','CDSCO','AERB','NABH','...'
    document_class    text        NOT NULL CHECK (document_class IN ('permission','certificate','licence','consent','registration','accreditation')),
    renewal_cycle_months int,                                        -- NULL if perpetual / no renewal
    mandatory_for_departments text[],                               -- array of department_type codes; NULL if org-level
    compliance_standard text,                                       -- 'fire_safety','water_mgmt','mca_act','company_act','medical_device','nuclear','accreditation'
    audit_required    boolean     NOT NULL DEFAULT true,
    description       text
);

-- ---------------------------------------------------------------------
-- LICENCES — one row per issued licence document (may renew, creating new row)
-- Status tracks: applied, approved, suspended, expired, revoked, renewal_pending
-- ---------------------------------------------------------------------
CREATE TABLE compliance.licences (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    licence_type_code text        NOT NULL REFERENCES compliance.licence_types(code),
    department_id     uuid        REFERENCES core.departments(id),  -- NULL if org-level
    licence_number    text        NOT NULL,
    issued_date       date        NOT NULL,
    valid_from        date        NOT NULL,
    valid_to          date,                                          -- NULL if perpetual
    issued_by         text,                                          -- authority name
    authority_contact text,                                          -- email or phone for renewal inquiry
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('applied','approved','active','suspended','expired','revoked','renewal_pending')),
    status_reason     text,                                          -- e.g. 'default','lapsed','voluntary'
    conditions_text   text,                                          -- specific clauses / restrictions
    physical_copy_s3_key text,                                       -- scanned copy location
    digital_copy_url  text,                                          -- hyperlink to authority portal (if any)
    renewal_applied_at timestamptz,
    renewal_approved_at timestamptz,
    previous_licence_id uuid       REFERENCES compliance.licences(id),  -- links renewal chain
    is_critical       boolean     NOT NULL DEFAULT false,            -- requires board notification if lapsed
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,
    deleted_at        timestamptz,

    CONSTRAINT uq_licence_number UNIQUE (clinic_id, licence_type_code, licence_number)
);
CREATE INDEX ix_licences_dept ON compliance.licences(department_id) WHERE deleted_at IS NULL;
CREATE INDEX ix_licences_status ON compliance.licences(clinic_id, status, valid_to);
CREATE INDEX ix_licences_type ON compliance.licences(licence_type_code, clinic_id) WHERE status IN ('active','renewal_pending');

-- Track which staff member is legally responsible for each licence
CREATE TABLE compliance.licence_responsibility (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    licence_id        uuid        NOT NULL REFERENCES compliance.licences(id) ON DELETE CASCADE,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    responsibility_role text       NOT NULL CHECK (responsibility_role IN
                                   ('primary_holder','custodian','renewal_owner','signatory','witness')),
    assigned_at       timestamptz NOT NULL DEFAULT now(),
    assigned_by       uuid,
    valid_from        timestamptz NOT NULL DEFAULT now(),
    valid_to          timestamptz,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_lic_resp_staff ON compliance.licence_responsibility(staff_id, clinic_id);
CREATE INDEX ix_lic_resp_licence ON compliance.licence_responsibility(licence_id);

-- Physical copy custody — who accessed the printed/original document, when, for what
CREATE TABLE compliance.physical_copy_access_log (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    licence_id        uuid        NOT NULL REFERENCES compliance.licences(id) ON DELETE CASCADE,
    accessed_at       timestamptz NOT NULL DEFAULT now(),
    actor_user_id     uuid,                                          -- identity.users.id
    actor_action      text        NOT NULL CHECK (actor_action IN ('checked_out','checked_in','inspected','scanned','photographed')),
    purpose           text        NOT NULL,                          -- 'regulatory_inspection','renewal','audit','training',...
    location_stored_at text,                                         -- 'safe_A2','office_cabinet_B1',...
    custody_duration_mins int,                                       -- if checked_out: when checked back in
    notes             text
);
CREATE INDEX ix_phys_access_licence ON compliance.physical_copy_access_log(licence_id, accessed_at DESC);
CREATE INDEX ix_phys_access_actor ON compliance.physical_copy_access_log(actor_user_id, accessed_at DESC);

-- Renewal tracking & deadlines
CREATE TABLE compliance.renewal_schedule (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    licence_id        uuid        NOT NULL REFERENCES compliance.licences(id) ON DELETE CASCADE,
    days_before_expiry int        NOT NULL DEFAULT 90,              -- alert threshold
    due_date          date        NOT NULL,                         -- calculated: valid_to - days_before_expiry
    renewal_submitted_at timestamptz,
    renewal_approved_at timestamptz,
    new_licence_id    uuid        REFERENCES compliance.licences(id),  -- upon approval, link new licence
    status            text        NOT NULL DEFAULT 'pending'
                                    CHECK (status IN ('pending','submitted','approved','lapsed')),
    assigned_to       uuid        REFERENCES identity.users(id),    -- person responsible for renewal action
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_renewal_clinic_due ON compliance.renewal_schedule(clinic_id, due_date) WHERE status='pending';
CREATE INDEX ix_renewal_licence ON compliance.renewal_schedule(licence_id);

-- Compliance alerts & board notifications
CREATE TABLE compliance.compliance_alerts (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    alert_type        text        NOT NULL CHECK (alert_type IN
                                   ('licence_expiry_imminent','licence_expired','licence_lapsed','critical_licence_missing',
                                    'capa_overdue','inspection_due','training_required','policy_review_required')),
    licence_id        uuid        REFERENCES compliance.licences(id),
    severity          text        NOT NULL CHECK (severity IN ('low','medium','high','critical')),
    raised_at         timestamptz NOT NULL DEFAULT now(),
    target_audience   text        NOT NULL CHECK (target_audience IN ('clinic_admin','dpo','medical_director','board','compliance_officer','ceo')),
    message           text        NOT NULL,
    action_required   text,
    due_date          date,
    status            text        NOT NULL DEFAULT 'open'
                                    CHECK (status IN ('open','acknowledged','in_progress','resolved','escalated')),
    acknowledged_by   uuid,
    acknowledged_at   timestamptz,
    resolved_at       timestamptz,
    resolution_notes  text
);
CREATE INDEX ix_alerts_clinic_severity ON compliance.compliance_alerts(clinic_id, severity, raised_at DESC);
CREATE INDEX ix_alerts_status ON compliance.compliance_alerts(clinic_id, status, due_date);

-- Inspection records (internal audits, regulatory visits, accreditation assessments)
CREATE TABLE compliance.inspection_records (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    inspection_type   text        NOT NULL CHECK (inspection_type IN
                                   ('internal_audit','regulatory_inspection','accreditation_visit','surprise_inspection','followup_audit')),
    inspection_date   date        NOT NULL,
    conducted_by      text        NOT NULL,                         -- auditor/inspector name + org
    scope_description text,
    findings_count    int,
    critical_findings int,
    major_findings    int,
    minor_findings    int,
    capa_required     boolean     NOT NULL DEFAULT false,
    report_s3_key     text,                                          -- scanned report location
    scheduled_followup_date date,
    followup_completed_at timestamptz,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_inspections_clinic_date ON compliance.inspection_records(clinic_id, inspection_date DESC);

-- CAPA (Corrective And Preventive Actions) tracking
CREATE TABLE compliance.capa_items (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    inspection_id     uuid        NOT NULL REFERENCES compliance.inspection_records(id) ON DELETE CASCADE,
    finding_code      text,                                          -- reference to specific finding
    description       text        NOT NULL,
    root_cause_analysis text,
    action_planned    text        NOT NULL,
    assigned_to       uuid        REFERENCES identity.users(id),
    due_date          date        NOT NULL,
    status            text        NOT NULL DEFAULT 'open'
                                    CHECK (status IN ('open','in_progress','completed','verified_effective','closed','deferred')),
    completion_date   date,
    effectiveness_review_date date,
    verified_by       uuid,
    verified_at       timestamptz,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_capa_clinic_status ON compliance.capa_items(clinic_id, status, due_date) WHERE status IN ('open','in_progress');
CREATE INDEX ix_capa_inspection ON compliance.capa_items(inspection_id);

-- Policy & procedure versioning
CREATE TABLE compliance.policies (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    policy_code       text        NOT NULL,                         -- e.g. 'FIRE_SAFETY_2026','BIOMEDICAL_WASTE_2025'
    policy_title      text        NOT NULL,
    description       text,
    version           int         NOT NULL,
    effective_from    date        NOT NULL,
    effective_to      date,
    document_s3_key   text,
    approval_date     date,
    approved_by       uuid,
    published_by      uuid,
    compliance_mapped_to text[],                                     -- array of regulation codes
    review_cycle_months int,
    next_review_due   date,
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('draft','approved','active','superseded','archived')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_policy_code_version UNIQUE (clinic_id, policy_code, version)
);
CREATE INDEX ix_policies_clinic ON compliance.policies(clinic_id, effective_from DESC);
CREATE INDEX ix_policies_review_due ON compliance.policies(clinic_id, next_review_due) WHERE status='active';

-- Staff training records (mandatory certifications, inductions, refreshers)
CREATE TABLE compliance.training_completions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    staff_id          uuid        NOT NULL REFERENCES core.staff(id),
    training_name     text        NOT NULL,                         -- 'Fire Safety Induction','Biomedical Waste Handling','HIPAA Training',...
    training_category text        NOT NULL CHECK (training_category IN
                                   ('induction','mandatory_annual','regulatory_required','cme','skill_development','incident_response')),
    training_date     date        NOT NULL,
    certificate_s3_key text,
    completion_hours  numeric(5,2),
    score             numeric(5,2),
    passed            boolean     NOT NULL DEFAULT true,
    valid_until       date,                                          -- renewal date
    conducted_by      text,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_training_staff_category ON compliance.training_completions(staff_id, training_category, training_date DESC);
CREATE INDEX ix_training_expiry ON compliance.training_completions(clinic_id, valid_until) WHERE valid_until IS NOT NULL;

-- Incident & near-miss tracking for compliance analytics
CREATE TABLE compliance.incidents (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    incident_date     date        NOT NULL,
    incident_type     text        NOT NULL CHECK (incident_type IN
                                   ('safety_hazard','medication_error','needle_stick','fire','electrical','chemical_spill',
                                    'infection_control_breach','data_breach','near_miss','adverse_event')),
    severity          text        NOT NULL CHECK (severity IN ('minor','moderate','serious','critical')),
    reported_by       uuid,
    description       text        NOT NULL,
    immediate_action  text,
    capa_raised       boolean     NOT NULL DEFAULT false,
    capa_id           uuid        REFERENCES compliance.capa_items(id),
    status            text        NOT NULL DEFAULT 'reported'
                                    CHECK (status IN ('reported','investigating','closed','escalated')),
    investigation_notes text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_incidents_clinic_date ON compliance.incidents(clinic_id, incident_date DESC);
CREATE INDEX ix_incidents_type ON compliance.incidents(incident_type, clinic_id);

-- Audit of all compliance state changes for regulatory proof
CREATE TABLE compliance.compliance_audit_log (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    logged_at         timestamptz NOT NULL DEFAULT now(),
    entity_type       text        NOT NULL,                         -- 'licence','inspection','capa','alert','policy',...
    entity_id         uuid        NOT NULL,
    action            text        NOT NULL CHECK (action IN ('created','updated','status_changed','approved','resolved')),
    actor_user_id     uuid,
    old_state         jsonb,
    new_state         jsonb,
    reason            text,
    PRIMARY KEY (id, logged_at)
) PARTITION BY RANGE (logged_at);

CREATE TABLE compliance.compliance_audit_log_2026_04 PARTITION OF compliance.compliance_audit_log
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE compliance.compliance_audit_log_2026_05 PARTITION OF compliance.compliance_audit_log
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

CREATE INDEX ix_comp_audit_entity ON compliance.compliance_audit_log(entity_type, entity_id);
CREATE INDEX ix_comp_audit_actor ON compliance.compliance_audit_log(actor_user_id, logged_at DESC);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='compliance'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('licence_types')
    LOOP
        EXECUTE format('ALTER TABLE compliance.%I ENABLE ROW LEVEL SECURITY', t);
        BEGIN
            EXECUTE format(
              'CREATE POLICY p_%1$s_tenant ON compliance.%1$I
               USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
               WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
        EXCEPTION WHEN undefined_column THEN NULL;
        END;
    END LOOP;
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='compliance'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('licence_types')
    LOOP
        PERFORM core.attach_standard_triggers('compliance', t);
    END LOOP;
END $$;
