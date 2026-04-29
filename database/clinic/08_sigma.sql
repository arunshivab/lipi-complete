-- =====================================================================
-- LiPi :: clinic :: 08_sigma
-- Six Sigma & process improvement: process definitions, defects,
-- SPC (Statistical Process Control), control charts, capability indices,
-- CAPA linkage to compliance.capa_items.
-- Prereq: 00_common + 01_core + compliance/06_compliance applied.
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS sigma;
SET search_path = sigma, core, public;

-- Cross-schema reference to compliance
CREATE TABLE sigma.capa_link (LIKE compliance.capa_items INCLUDING ALL);
-- Placeholder for documentation; actual FK is logical only

-- ---------------------------------------------------------------------
-- PROCESS DEFINITIONS — hierarchical (level 1, 2, 3, 4)
-- L1: Department processes; L2: Sub-processes; L3: Steps; L4: Tasks
-- ---------------------------------------------------------------------
CREATE TABLE sigma.process_definitions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_code      text        NOT NULL,                         -- e.g. 'PROC_OPD_REGISTRATION','PROC_LAB_SAMPLE_COLLECTION'
    process_name      text        NOT NULL,
    process_level     int         NOT NULL CHECK (process_level IN (1,2,3,4)),
    parent_process_id uuid        REFERENCES sigma.process_definitions(id),  -- for hierarchical decomposition
    department_id     uuid        REFERENCES core.departments(id),
    process_owner_id  uuid        NOT NULL REFERENCES core.staff(id), -- process champion / owner
    process_objective text,                                         -- intended outcome, benefit
    kpi_code          text,                                          -- primary KPI tracked (e.g. 'turnaround_time', 'error_rate')
    kpi_target        numeric(10,4),                                 -- numeric target (e.g. 4.5 for 4.5 hrs turnaround)
    kpi_unit          text,                                          -- 'hours','minutes','percent','count','ppm'
    upper_spec_limit  numeric(10,4),                                 -- USL for control limits
    lower_spec_limit  numeric(10,4),                                 -- LSL for control limits
    target_value      numeric(10,4),                                 -- nominal target
    critical_step     boolean     NOT NULL DEFAULT false,            -- if one non-conformance = critical
    documentation_s3_key text,                                        -- process flowchart, FMEA
    effective_from    date        NOT NULL DEFAULT CURRENT_DATE,
    effective_to      date,
    status            text        NOT NULL DEFAULT 'active'
                                    CHECK (status IN ('draft','active','under_review','archived')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_process_code UNIQUE (clinic_id, process_code)
);
CREATE INDEX ix_processes_dept ON sigma.process_definitions(department_id) WHERE status='active';
CREATE INDEX ix_processes_owner ON sigma.process_definitions(process_owner_id);
CREATE INDEX ix_processes_parent ON sigma.process_definitions(parent_process_id);

-- Process step / workflow task
CREATE TABLE sigma.process_steps (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id) ON DELETE CASCADE,
    step_sequence     int         NOT NULL,
    step_name         text        NOT NULL,
    step_description  text,
    responsible_role_code text,                                      -- role performing this step
    estimated_duration_mins int,
    success_criteria  text,                                          -- what makes this step 'done'
    risk_level        text        CHECK (risk_level IN ('low','medium','high')),
    mitigating_control text,                                         -- if high risk, what prevents failure
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_process_step UNIQUE (clinic_id, process_id, step_sequence)
);
CREATE INDEX ix_steps_process ON sigma.process_steps(process_id);

-- Defect / non-conformance register
CREATE TABLE sigma.defects (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    defect_date       date        NOT NULL,
    defect_code       text,                                          -- unique tracking code
    defect_type       text        NOT NULL CHECK (defect_type IN
                                   ('documentation_error','patient_safety','billing_error','communication_gap','procedural_deviation','equipment_failure','data_entry','other')),
    severity          text        NOT NULL CHECK (severity IN ('critical','major','minor','observation')),
    description       text        NOT NULL,
    root_cause        text,
    detected_by       uuid,
    detected_stage    text,                                          -- 'during_process','by_qa','by_customer','external_audit'
    immediate_action  text,
    capa_required     boolean     NOT NULL DEFAULT false,
    capa_id           uuid        REFERENCES compliance.capa_items(id),  -- link to compliance CAPA
    defect_cost       numeric(12,2),                                 -- $ cost if applicable
    status            text        NOT NULL DEFAULT 'reported'
                                    CHECK (status IN ('reported','investigating','capa_raised','resolved','closed')),
    closed_at         timestamptz,
    trend_flag        boolean     NOT NULL DEFAULT false,            -- if part of trend
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_defects_process_date ON sigma.defects(process_id, defect_date DESC);
CREATE INDEX ix_defects_severity ON sigma.defects(clinic_id, severity, status) WHERE status IN ('reported','investigating');
CREATE INDEX ix_defects_trend ON sigma.defects(process_id, defect_type) WHERE trend_flag;

-- Observation data points (individual measurements for SPC)
-- e.g. patient wait time (mins), lab TAT (hours), medication error count per shift
CREATE TABLE sigma.spc_observations (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    observation_date  date        NOT NULL,
    observation_time  time,                                          -- time of day when sample was taken
    observation_batch text,                                          -- e.g. 'morning_shift','afternoon','evening'
    observation_value numeric(12,4) NOT NULL,                        -- the actual measured value
    observation_unit  text,                                          -- inherited from process.kpi_unit
    subgroup_id       uuid,                                          -- for rational subgrouping (e.g. same patient/procedure batch)
    is_outlier        boolean     NOT NULL DEFAULT false,            -- flagged by SPC logic
    created_at        timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (id, observation_date)
) PARTITION BY RANGE (observation_date);

CREATE TABLE sigma.spc_observations_2026_04 PARTITION OF sigma.spc_observations
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE sigma.spc_observations_2026_05 PARTITION OF sigma.spc_observations
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

CREATE INDEX ix_spc_obs_process_date ON sigma.spc_observations(process_id, observation_date DESC);
CREATE INDEX ix_spc_obs_outlier ON sigma.spc_observations(process_id, is_outlier) WHERE is_outlier;

-- SPC Roll-ups: daily/weekly/monthly statistics
CREATE TABLE sigma.spc_rollups (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    rollup_date       date        NOT NULL,                         -- the day/week start/month start being summarized
    rollup_period     text        NOT NULL CHECK (rollup_period IN ('daily','weekly','monthly')),
    sample_count      int         NOT NULL,
    mean_value        numeric(12,4),
    std_dev           numeric(12,4),                                 -- standard deviation
    min_value         numeric(12,4),
    max_value         numeric(12,4),
    median_value      numeric(12,4),
    q1_value          numeric(12,4),                                 -- 25th percentile
    q3_value          numeric(12,4),                                 -- 75th percentile
    iqr_value         numeric(12,4),                                 -- inter-quartile range
    cp_index          numeric(6,3),                                  -- process capability (overall)
    cpk_index         numeric(6,3),                                  -- capability index (min of Cpu/Cpl)
    pp_index          numeric(6,3),                                  -- performance index
    ppk_index         numeric(6,3),                                  -- performance index (min)
    process_yield     numeric(5,2),                                  -- % within spec
    defects_count     int,                                           -- defects observed in this period
    out_of_control    boolean     NOT NULL DEFAULT false,            -- SPC rule violation detected
    control_rule_violated text,                                      -- which rule (e.g. 'rule_1', 'rule_6')
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_spc_rollup UNIQUE (clinic_id, process_id, rollup_date, rollup_period)
);
CREATE INDEX ix_spc_rollups_process ON sigma.spc_rollups(process_id, rollup_date DESC);
CREATE INDEX ix_spc_rollups_ooc ON sigma.spc_rollups(clinic_id, out_of_control) WHERE out_of_control;

-- Control chart points (x-bar chart, I-MR chart, p-chart, np-chart, u-chart)
CREATE TABLE sigma.control_chart_points (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    chart_type        text        NOT NULL CHECK (chart_type IN ('x_bar','individual','range','moving_range','p_chart','np_chart','u_chart','c_chart')),
    point_date        date        NOT NULL,
    point_value       numeric(12,4) NOT NULL,
    ucl               numeric(12,4),                                 -- upper control limit
    lcl               numeric(12,4),                                 -- lower control limit
    center_line       numeric(12,4),                                 -- process mean
    usl               numeric(12,4),                                 -- upper spec limit (from process def)
    lsl               numeric(12,4),                                 -- lower spec limit
    is_out_of_control boolean     NOT NULL DEFAULT false,
    spc_rule_violated text,                                          -- which control rule
    alert_level       text        CHECK (alert_level IN ('normal','warning','critical')),
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_control_chart_process ON sigma.control_chart_points(process_id, point_date DESC);
CREATE INDEX ix_control_chart_ooc ON sigma.control_chart_points(process_id, is_out_of_control) WHERE is_out_of_control;

-- FMEA (Failure Mode & Effects Analysis) register
CREATE TABLE sigma.fmea_items (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    process_step_id   uuid        REFERENCES sigma.process_steps(id),
    failure_mode      text        NOT NULL,                         -- what could go wrong (e.g. 'patient misidentified')
    potential_effects text        NOT NULL,                         -- consequences
    potential_causes  text,
    severity_score    int         NOT NULL CHECK (severity_score >= 1 AND severity_score <= 10),
    occurrence_score  int         NOT NULL CHECK (occurrence_score >= 1 AND occurrence_score <= 10),
    detection_score   int         NOT NULL CHECK (detection_score >= 1 AND detection_score <= 10),
    rpn_value         int         NOT NULL,                          -- Risk Priority Number (S*O*D)
    current_controls  text,                                          -- existing preventive measures
    recommended_action text,
    action_owner      uuid        REFERENCES core.staff(id),
    action_deadline   date,
    action_taken      text,
    action_effective  boolean,
    revised_severity  int,
    revised_occurrence int,
    revised_detection int,
    revised_rpn       int,
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_fmea_process ON sigma.fmea_items(process_id);
CREATE INDEX ix_fmea_rpn ON sigma.fmea_items(clinic_id, rpn_value DESC);

-- Six Sigma project register (DMAIC, DMADV)
CREATE TABLE sigma.projects (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    project_code      text        NOT NULL,
    project_name      text        NOT NULL,
    project_type      text        NOT NULL CHECK (project_type IN ('dmaic','dmadv','kaizen','other')),
    related_process_id uuid        REFERENCES sigma.process_definitions(id),
    sponsor_id        uuid        NOT NULL REFERENCES core.staff(id),  -- executive sponsor
    project_lead_id   uuid        NOT NULL REFERENCES core.staff(id),  -- black belt / project lead
    team_members      uuid[],                                        -- green belts, team
    charter_s3_key    text,                                          -- project charter document
    problem_statement text,
    goal_statement    text,
    scope             text,
    baseline_metric   numeric(12,4),                                 -- starting performance
    target_metric     numeric(12,4),                                 -- desired performance
    metric_unit       text,
    project_start_date date       NOT NULL,
    target_completion_date date,
    actual_completion_date date,
    status            text        NOT NULL DEFAULT 'initiated'
                                    CHECK (status IN ('initiated','define','measure','analyze','improve','control','completed','on_hold','cancelled')),
    phase_define_complete boolean  NOT NULL DEFAULT false,
    phase_measure_complete boolean NOT NULL DEFAULT false,
    phase_analyze_complete boolean NOT NULL DEFAULT false,
    phase_improve_complete boolean NOT NULL DEFAULT false,
    phase_control_complete boolean NOT NULL DEFAULT false,
    financial_benefit numeric(14,2),                                 -- projected or realized savings
    notes             text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    updated_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0,

    CONSTRAINT uq_project_code UNIQUE (clinic_id, project_code)
);
CREATE INDEX ix_projects_status ON sigma.projects(clinic_id, status);
CREATE INDEX ix_projects_process ON sigma.projects(related_process_id);

-- Kaizen / continuous improvement suggestions
CREATE TABLE sigma.kaizen_suggestions (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    suggested_by      uuid        NOT NULL REFERENCES core.staff(id),
    suggested_date    date        NOT NULL DEFAULT CURRENT_DATE,
    suggestion_title  text        NOT NULL,
    suggestion_detail text        NOT NULL,
    expected_benefit  text,
    estimated_effort  text,                                          -- 'low','medium','high'
    status            text        NOT NULL DEFAULT 'submitted'
                                    CHECK (status IN ('submitted','under_review','approved','implemented','rejected','deferred')),
    reviewed_by       uuid,
    reviewed_date     date,
    review_comments   text,
    implementation_owner uuid      REFERENCES core.staff(id),
    implementation_date date,
    verified_benefit  text,
    created_at        timestamptz NOT NULL DEFAULT now(),
    row_version       int         NOT NULL DEFAULT 0
);
CREATE INDEX ix_kaizen_process_date ON sigma.kaizen_suggestions(process_id, suggested_date DESC);
CREATE INDEX ix_kaizen_status ON sigma.kaizen_suggestions(clinic_id, status);

-- Trend analysis & alert summary (for dashboard)
CREATE TABLE sigma.process_trend_summary (
    id                uuid        PRIMARY KEY DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    process_id        uuid        NOT NULL REFERENCES sigma.process_definitions(id),
    summary_date      date        NOT NULL,
    defect_count_30d  int,
    defect_count_90d  int,
    defect_count_365d int,
    trend_direction   text        CHECK (trend_direction IN ('improving','stable','declining')),
    cpk_current       numeric(6,3),
    cpk_trend         text        CHECK (cpk_trend IN ('improving','stable','declining')),
    last_out_of_control_date date,
    process_alert_level text       CHECK (process_alert_level IN ('green','yellow','red')),
    notes             text,
    updated_at        timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT uq_trend_summary UNIQUE (clinic_id, process_id, summary_date)
);
CREATE INDEX ix_trend_summary_alert ON sigma.process_trend_summary(clinic_id, process_alert_level);

-- Audit log for sigma operations
CREATE TABLE sigma.sigma_audit_log (
    id                uuid        NOT NULL DEFAULT core.uuid_v7(),
    clinic_id         uuid        NOT NULL,
    logged_at         timestamptz NOT NULL DEFAULT now(),
    entity_type       text        NOT NULL,                         -- 'process','defect','project','spc_calculation',...
    entity_id         uuid        NOT NULL,
    action            text        NOT NULL,                         -- 'created','updated','analysis_run','control_rules_evaluated'
    actor_user_id     uuid,
    details           jsonb       NOT NULL DEFAULT '{}'::jsonb,     -- calculation results, alerts, etc.
    PRIMARY KEY (id, logged_at)
) PARTITION BY RANGE (logged_at);

CREATE TABLE sigma.sigma_audit_log_2026_04 PARTITION OF sigma.sigma_audit_log
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE sigma.sigma_audit_log_2026_05 PARTITION OF sigma.sigma_audit_log
    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');

CREATE INDEX ix_sigma_audit_entity ON sigma.sigma_audit_log(entity_type, entity_id);
CREATE INDEX ix_sigma_audit_actor ON sigma.sigma_audit_log(actor_user_id, logged_at DESC);

-- ---------------------------------------------------------------------
-- RLS + TRIGGERS
-- ---------------------------------------------------------------------
DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='sigma'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
    LOOP
        EXECUTE format('ALTER TABLE sigma.%I ENABLE ROW LEVEL SECURITY', t);
        BEGIN
            EXECUTE format(
              'CREATE POLICY p_%1$s_tenant ON sigma.%1$I
               USING (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)
               WITH CHECK (clinic_id = current_setting(''lipi.clinic_id'', true)::uuid)', t);
        EXCEPTION WHEN undefined_column THEN NULL;
        END;
    END LOOP;
END $$;

DO $$
DECLARE t text;
BEGIN
    FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='sigma'
             AND tablename NOT LIKE '%\_20%' ESCAPE '\'
    LOOP
        PERFORM core.attach_standard_triggers('sigma', t);
    END LOOP;
END $$;
