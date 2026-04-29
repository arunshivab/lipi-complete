-- =====================================================================
-- LiPi :: clinic :: 04_audit
-- Tamper-evident, hash-chained audit trail for a clinic.
-- Every write to a clinical entity, every PHI read, every export/print
-- lands here. Partitioned monthly. Optional Hyperledger anchoring.
-- Prereq: 00_common + 01_core + 02_identity applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS audit;
SET search_path = audit, core, public;

-- ---------------------------------------------------------------------
-- EVENT TYPE CATALOGUE (reference data)
-- ---------------------------------------------------------------------
CREATE TABLE audit.event_types (
    code              text        PRIMARY KEY,                     -- e.g. 'patient.created', 'prescription.signed'
    category          text        NOT NULL CHECK (category IN
                                   ('authentication','authorization','data_read','data_write','data_export',
                                    'data_print','config_change','admin_action','integration','security','clinical')),
    severity          text        NOT NULL CHECK (severity IN ('info','notice','warning','alert','critical')),
    is_phi_access     boolean     NOT NULL DEFAULT false,
    description       text,
    nabh_required     boolean     NOT NULL DEFAULT false,           -- flagged as NABH/HIPAA-mandated
    dpdp_required     boolean     NOT NULL DEFAULT false
);

-- ---------------------------------------------------------------------
-- AUDIT EVENTS — append-only, hash-chained, partitioned monthly
--
-- Hash chain:
--   current_hash = sha256( previous_hash ||
--                          id || event_ts || action || entity_type || entity_id ||
--                          coalesce(before_state,'') || coalesce(after_state,'') )
-- The previous_hash for the first row of a partition is the last hash of
-- the previous partition; blockchain_anchors stores periodic batch hashes.
-- ---------------------------------------------------------------------
CREATE TABLE audit.audit_events (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    event_ts          timestamptz NOT NULL DEFAULT now(),
    action            text        NOT NULL REFERENCES audit.event_types(code),
    actor_user_id     uuid,                                          -- identity.users.id (no FK — survives user deletion)
    actor_username    citext,                                        -- denormalised snapshot
    actor_role_codes  text[],                                        -- roles held at moment of action
    actor_ip          inet,
    actor_user_agent  text,
    actor_session_id  uuid,
    actor_service_acct_id uuid,
    entity_type       text        NOT NULL,                          -- 'core.patient', 'opd.encounter', ...
    entity_id         uuid,
    entity_parent_id  uuid,                                          -- e.g. patient for an encounter
    action_detail     text,
    before_state      jsonb,
    after_state       jsonb,
    change_reason     text,                                          -- required for 'break-glass' operations
    request_id        uuid,
    correlation_id    uuid,
    outcome           text        NOT NULL DEFAULT 'success'
                                    CHECK (outcome IN ('success','failure','denied','error')),
    previous_hash     bytea,
    current_hash      bytea       NOT NULL,
    PRIMARY KEY (id, event_ts)
) PARTITION BY RANGE (event_ts);

CREATE INDEX ix_audit_clinic_ts     ON audit.audit_events(clinic_id, event_ts DESC);
CREATE INDEX ix_audit_entity        ON audit.audit_events(entity_type, entity_id);
CREATE INDEX ix_audit_actor         ON audit.audit_events(actor_user_id, event_ts DESC);
CREATE INDEX ix_audit_action        ON audit.audit_events(action, event_ts DESC);
CREATE INDEX ix_audit_correlation   ON audit.audit_events(correlation_id);

-- First partitions; cron creates future ones
CREATE TABLE audit.audit_events_2026_04 PARTITION OF audit.audit_events
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE audit.audit_events_2026_05 PARTITION OF audit.audit_events
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- Function to compute hash for a new row given the previous tip hash
CREATE OR REPLACE FUNCTION audit.fn_compute_hash(
    p_prev bytea, p_id uuid, p_ts timestamptz, p_action text,
    p_entity_type text, p_entity_id uuid,
    p_before jsonb, p_after jsonb
) RETURNS bytea
LANGUAGE sql IMMUTABLE AS $$
    SELECT digest(
        coalesce(p_prev, '\x00'::bytea) ||
        convert_to(p_id::text || '|' || p_ts::text || '|' || p_action || '|' ||
                   p_entity_type || '|' || coalesce(p_entity_id::text,'') || '|' ||
                   coalesce(p_before::text,'') || '|' ||
                   coalesce(p_after::text,''), 'UTF8'),
        'sha256');
$$;

-- Tip hash tracker (one row per clinic — rolling tip of the chain)
CREATE TABLE audit.hash_chain_tip (
    clinic_id         uuid        PRIMARY KEY,
    tip_hash          bytea       NOT NULL,
    tip_event_id      uuid        NOT NULL,
    tip_event_ts      timestamptz NOT NULL,
    updated_at        timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- PHI ACCESS LOG — every read of PHI by a user (HIPAA § 164.312(b) + DPDP)
-- ---------------------------------------------------------------------
CREATE TABLE audit.phi_access_log (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    accessed_at       timestamptz NOT NULL DEFAULT now(),
    actor_user_id     uuid,
    actor_role_codes  text[],
    actor_ip          inet,
    patient_id        uuid        NOT NULL,
    mrn_snapshot      text,
    access_context    text        NOT NULL CHECK (access_context IN
                                   ('direct_care','referral','billing','research','quality','legal','break_glass','admin','self_access','public_health')),
    resource_type     text        NOT NULL,                          -- 'encounter','lab_report','dicom','prescription','note'
    resource_id       uuid,
    purpose_of_use    text,
    justification     text,                                           -- free-text; required when context='break_glass'
    minimum_necessary_principle_ok boolean NOT NULL DEFAULT true,
    consent_ref       text,
    request_id        uuid,
    PRIMARY KEY (id, accessed_at)
) PARTITION BY RANGE (accessed_at);

CREATE INDEX ix_phi_patient   ON audit.phi_access_log(patient_id, accessed_at DESC);
CREATE INDEX ix_phi_actor     ON audit.phi_access_log(actor_user_id, accessed_at DESC);
CREATE INDEX ix_phi_context   ON audit.phi_access_log(clinic_id, access_context, accessed_at DESC);

CREATE TABLE audit.phi_access_log_2026_04 PARTITION OF audit.phi_access_log
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE audit.phi_access_log_2026_05 PARTITION OF audit.phi_access_log
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

-- ---------------------------------------------------------------------
-- EXPORT / PRINT LOG — bulk data egress + printing
-- ---------------------------------------------------------------------
CREATE TABLE audit.export_log (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    exported_at       timestamptz NOT NULL DEFAULT now(),
    actor_user_id     uuid,
    actor_ip          inet,
    export_type       text        NOT NULL CHECK (export_type IN
                                   ('patient_summary','mis_report','financial_report','dicom','lab_csv','fhir_bundle','backup','research_dataset')),
    export_format     text        NOT NULL,                          -- 'pdf','xlsx','csv','dicom','fhir_json','parquet'
    record_count      int,
    patient_ids       uuid[],                                         -- populated if patient-specific
    query_parameters  jsonb,
    destination       text        NOT NULL CHECK (destination IN ('download','email','sftp','s3','api')),
    destination_detail text,
    file_sha256       bytea,
    file_size_bytes   bigint,
    dlp_scan_result   text        CHECK (dlp_scan_result IN ('clean','warning','blocked','skipped')),
    consent_ref       text,
    justification     text
);
CREATE INDEX ix_export_log_actor ON audit.export_log(actor_user_id, exported_at DESC);

CREATE TABLE audit.print_log (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    printed_at        timestamptz NOT NULL DEFAULT now(),
    actor_user_id     uuid,
    printer_name      text,
    document_type     text        NOT NULL,                          -- 'discharge_summary','prescription','lab_report',...
    document_ref_id   uuid,
    patient_id        uuid,
    copies            int         NOT NULL DEFAULT 1,
    watermark_applied boolean     NOT NULL DEFAULT true,
    client_ip         inet
);
CREATE INDEX ix_print_log_patient ON audit.print_log(patient_id, printed_at DESC);

-- ---------------------------------------------------------------------
-- BLOCKCHAIN ANCHORS — periodic batch hashes written to Hyperledger (optional)
-- ---------------------------------------------------------------------
CREATE TABLE audit.blockchain_anchors (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    anchor_type       text        NOT NULL CHECK (anchor_type IN ('audit_events','phi_access','export_log')),
    window_from       timestamptz NOT NULL,
    window_to         timestamptz NOT NULL,
    row_count         bigint      NOT NULL,
    batch_merkle_root bytea       NOT NULL,
    ledger_tx_id      text,
    ledger_block_no   bigint,
    anchored_at       timestamptz NOT NULL DEFAULT now(),
    verified_at       timestamptz,
    status            text        NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','anchored','verified','mismatch'))
);
CREATE INDEX ix_anchors_window ON audit.blockchain_anchors(clinic_id, anchor_type, window_to DESC);

-- ---------------------------------------------------------------------
-- HASH-CHAIN VERIFICATION RUNS — cron job re-walks chain & flags breaks
-- ---------------------------------------------------------------------
CREATE TABLE audit.hash_chain_verification_runs (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    started_at        timestamptz NOT NULL DEFAULT now(),
    finished_at       timestamptz,
    window_from       timestamptz NOT NULL,
    window_to         timestamptz NOT NULL,
    rows_checked      bigint      NOT NULL DEFAULT 0,
    breaks_found      int         NOT NULL DEFAULT 0,
    first_break_id    uuid,
    outcome           text        NOT NULL CHECK (outcome IN ('running','clean','break_detected','error'))
);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='audit'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
             AND tablename NOT IN ('event_types')
    LOOP
        EXECUTE format('ALTER TABLE audit.%I ENABLE ROW LEVEL SECURITY', t);
        EXECUTE format(
          'CREATE POLICY p_%1$s_tenant ON audit.%1$I
           USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
           WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
    END LOOP;
END $$;

-- Audit tables are append-only — disallow UPDATE/DELETE for everyone but superuser
-- (implemented via GRANT / REVOKE at deploy time; policy stub here)
CREATE POLICY p_audit_events_no_modify ON audit.audit_events
    AS RESTRICTIVE FOR UPDATE USING (false);
CREATE POLICY p_audit_events_no_delete ON audit.audit_events
    AS RESTRICTIVE FOR DELETE USING (false);
CREATE POLICY p_phi_log_no_modify ON audit.phi_access_log
    AS RESTRICTIVE FOR UPDATE USING (false);
CREATE POLICY p_phi_log_no_delete ON audit.phi_access_log
    AS RESTRICTIVE FOR DELETE USING (false);
