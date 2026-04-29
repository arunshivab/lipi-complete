# Data Dictionary — clinic :: `sigma`

Scope: Six Sigma and process improvement: process definitions, defect tracking, statistical process control (SPC), control charts, FMEA (Failure Mode & Effects Analysis), DMAIC/DMADV projects, Kaizen suggestions.

---

## Process Definitions

### sigma.process_definitions
Hierarchical process decomposition (L1→L2→L3→L4): departments → sub-processes → steps → tasks.

**Key fields:**
- `process_code` (UNQ clinic+code), `process_name`
- `process_level` — 1 (department), 2 (sub-process), 3 (step), 4 (task)
- `parent_process_id` (FK, self-referencing) — for hierarchy
- `department_id` (FK, optional) — if L1 is department-specific
- `process_owner_id` (FK) — staff member responsible (champion)
- `process_objective` — intended outcome/benefit
- `kpi_code`, `kpi_target`, `kpi_unit` — primary KPI tracked (e.g., turnaround_time in hours)
- `upper_spec_limit` (USL), `lower_spec_limit` (LSL), `target_value` (nominal) — control limits
- `critical_step` — true if one non-conformance = critical failure
- `documentation_s3_key` — flowchart, FMEA, risk assessment
- `effective_from`, `effective_to` — versioning
- `status` — draft, active, under_review, archived

### sigma.process_steps
Detailed step sequence within a process.

**Key fields:**
- `process_id`, `step_sequence` (UNQ with process)
- `step_name`, `step_description`, `responsible_role_code` — execution details
- `estimated_duration_mins`, `success_criteria` — standards
- `risk_level` — low, medium, high
- `mitigating_control` — if high risk, what control prevents failure

---

## Defect & Non-Conformance

### sigma.defects
Individual defect / non-conformance records.

**Key fields:**
- `process_id` (FK), `defect_date`
- `defect_code` — tracking ID
- `defect_type` — documentation_error, patient_safety, billing_error, communication_gap, procedural_deviation, equipment_failure, data_entry
- `severity` — critical, major, minor, observation
- `description`, `root_cause` — analysis
- `detected_by`, `detected_stage` — during_process, by_qa, by_customer, external_audit
- `immediate_action` — temporary containment
- `capa_required` — if true, CAPA item raised
- `capa_id` (FK) — link to compliance.capa_items
- `defect_cost` — $ impact if applicable
- `status` — reported → investigating → capa_raised → resolved → closed
- `trend_flag` — if true, part of a trend (consecutive defects)

**Index:** `(process_id, defect_type) WHERE trend_flag` to identify trends.

---

## Statistical Process Control (SPC)

### sigma.spc_observations
Individual measurements: one row per sample.

**Key fields:**
- `process_id` (FK), `observation_date` (partitioned monthly)
- `observation_time`, `observation_batch` — e.g., morning_shift
- `observation_value` — the measured KPI
- `subgroup_id` (FK, optional) — for rational subgrouping (same patient batch, same time slot)
- `is_outlier` — flagged by SPC statistical rules

### sigma.spc_rollups
Aggregated statistics: daily, weekly, monthly rollup.

**Key fields:**
- `process_id`, `rollup_date` (UNQ with process+period)
- `rollup_period` — daily, weekly, monthly
- `sample_count` — # of observations
- `mean_value`, `std_dev`, `min_value`, `max_value`, `median_value` — descriptive stats
- `q1_value`, `q3_value`, `iqr_value` — quartiles
- `cp_index`, `cpk_index` — process capability (overall vs. one-sided)
- `pp_index`, `ppk_index` — performance indices (shorter-term vs. longer-term)
- `process_yield` — % of output within spec
- `defects_count` — defects in this period
- `out_of_control` — true if SPC rule violated
- `control_rule_violated` — which rule (rule_1: beyond 3σ; rule_6: trend; rule_8: point > 1σ beyond centerline)

### sigma.control_chart_points
Points plotted on SPC control charts: X-bar, I-MR, p-chart, u-chart.

**Key fields:**
- `process_id`, `point_date`
- `chart_type` — x_bar, individual, range, moving_range, p_chart, np_chart, u_chart, c_chart
- `point_value` — the plotted value
- `ucl`, `lcl`, `center_line` — control limits and process mean
- `usl`, `lsl` — spec limits (inherited from process definition)
- `is_out_of_control`, `spc_rule_violated` — alert status
- `alert_level` — normal, warning, critical

---

## FMEA

### sigma.fmea_items
Failure Mode and Effects Analysis: systematic risk assessment.

**Key fields:**
- `process_id`, `process_step_id` (optional FK)
- `failure_mode` — what could go wrong (e.g., patient misidentified)
- `potential_effects` — consequences (patient harm, billing error, etc.)
- `potential_causes` — root cause categories
- `severity_score`, `occurrence_score`, `detection_score` — 1-10 scales
- `rpn_value` — Risk Priority Number = severity × occurrence × detection
- `current_controls` — existing preventive/detective controls
- `recommended_action`, `action_owner`, `action_deadline` — corrective action
- `action_taken`, `action_effective` — verification
- `revised_severity`, `revised_occurrence`, `revised_detection`, `revised_rpn` — post-action indices
- `notes`

**Index:** `(clinic_id, rpn_value DESC)` to prioritize high-risk items.

---

## DMAIC/DMADV Projects

### sigma.projects
Six Sigma project register: DMAIC (improve existing) or DMADV (design new).

**Key fields:**
- `project_code` (UNQ clinic+code), `project_name`, `project_type` — dmaic, dmadv, kaizen, other
- `related_process_id` (FK, optional) — which process being improved
- `sponsor_id` (FK) — executive sponsor
- `project_lead_id` (FK) — black belt / project lead
- `team_members` (UUID array) — green belts, team
- `charter_s3_key` — project charter document
- `problem_statement`, `goal_statement`, `scope`
- `baseline_metric`, `target_metric`, `metric_unit` — before/after goals
- `project_start_date`, `target_completion_date`, `actual_completion_date`
- `status` — initiated → define → measure → analyze → improve → control → completed (or on_hold/cancelled)
- `phase_*_complete` (boolean) — gate tracking (define, measure, analyze, improve, control)
- `financial_benefit` — projected or realized savings ($)

---

## Continuous Improvement

### sigma.kaizen_suggestions
Bottom-up improvement ideas: employees propose quick wins.

**Key fields:**
- `process_id` (FK), `suggested_by` (FK staff)
- `suggestion_date`, `suggestion_title`, `suggestion_detail`
- `expected_benefit`, `estimated_effort` — low, medium, high
- `status` — submitted → under_review → approved → implemented (or rejected/deferred)
- `reviewed_by`, `reviewed_date`, `review_comments` — review process
- `implementation_owner`, `implementation_date` — execution
- `verified_benefit` — post-implementation validation

---

## Trend & Alert Summary

### sigma.process_trend_summary
Rolling process health snapshot: defect trends, capability trends, alert levels.

**Key fields:**
- `process_id`, `summary_date` (UNQ with process)
- `defect_count_30d`, `defect_count_90d`, `defect_count_365d` — trend denominator
- `trend_direction` — improving, stable, declining
- `cpk_current`, `cpk_trend` — current capability and direction
- `last_out_of_control_date` — when last SPC violation detected
- `process_alert_level` — green (all good), yellow (watch), red (action required)
- `updated_at` — last refresh

---

## Notes

- **SPC rules:** Control rules applied:
  - Rule 1: Beyond 3σ
  - Rule 6: Trend of 6+ points increasing/decreasing
  - Rule 8: 8+ points on same side of center line
  
- **Partitioning:** spc_observations partitioned monthly for efficient data retention and archival.

- **FMEA RPN:** High-risk items (RPN > threshold, e.g., 100) escalate to DMAIC projects or immediate corrective action.

- **Projects:** Dashboard tracks DMAIC funnel: # in each phase; financial benefits realized; cycle time from initiation to control.

- **Kaizen:** Empowers staff to improve; lightweight, fast implementation tracks organizational improvement culture.

- **Integration:** Defects link to compliance.capa_items if high-impact; FMEA findings drive process redesign in DMADV projects.
