# Data Dictionary — clinic :: `compliance`

Scope: Regulatory licences (Fire, CGWA, MCA, RoC, CDSCO, AERB, NABH, etc.), responsibility assignment, physical-copy custody, renewal tracking, and compliance alerts.

---

## Licence Management

### compliance.licence_types
Catalogue of licence types and their regulatory requirements.

**Fields:** `code` (PK: fire_noc, cgwa_consent, cdsco_import, aerb, nabh), `issuing_authority`, `document_class` (permission/certificate/licence/consent), `renewal_cycle_months`, `mandatory_for_departments` (array of dept types requiring this licence), `compliance_standard` (fire_safety, water_mgmt, medical_device, nuclear), `audit_required`.

### compliance.licences
Individual issued licences. State machine: issued → approved → active → renewal_pending → renewed.

**Key fields:**
- `licence_type_code` (FK) — type of licence
- `department_id` (FK, optional) — NULL if org-level
- `licence_number` (UNQ clinic+type+number) — authority-issued ID
- `issued_date`, `valid_from`, `valid_to` — lifecycle
- `issued_by`, `authority_contact` — issuing authority details
- `status` — applied, approved, active, suspended, expired, revoked, renewal_pending
- `conditions_text` — specific clauses/restrictions
- `physical_copy_s3_key` — scanned licence
- `digital_copy_url` — authority portal link (if any)
- `renewal_applied_at`, `renewal_approved_at` — renewal workflow
- `previous_licence_id` (FK) — link to prior licence (renewal chain)
- `is_critical` — true = board must be notified if lapsed

**Indexes:** `(clinic_id, status, valid_to)` for expiry tracking; `(licence_type_code, clinic_id) WHERE status='active'`.

### compliance.licence_responsibility
Who is legally responsible for each licence.

**Key fields:**
- `licence_id` (FK), `staff_id` (FK)
- `responsibility_role` — primary_holder, custodian, renewal_owner, signatory, witness
- `assigned_at`, `valid_from`, `valid_to` — assignment lifecycle

### compliance.physical_copy_access_log
Audit trail for physical licence documents: who accessed, when, for what reason.

**Key fields:**
- `actor_action` — checked_out, checked_in, inspected, scanned, photographed
- `purpose` — regulatory_inspection, renewal, audit, training, etc.
- `location_stored_at` — safe_A2, office_cabinet_B1, etc.
- `custody_duration_mins` — if checked out, how long held
- `accessed_at` — timestamp

---

## Renewal Tracking

### compliance.renewal_schedule
Renewal deadline tracking and workflow.

**Key fields:**
- `licence_id` (FK)
- `days_before_expiry` — alert threshold (default 90)
- `due_date` — calculated: valid_to - days_before_expiry
- `renewal_submitted_at`, `renewal_approved_at` — workflow
- `new_licence_id` (FK) — upon approval, link new licence
- `status` — pending, submitted, approved, lapsed
- `assigned_to` — person responsible for renewal action

**Index:** `(clinic_id, due_date) WHERE status='pending'` for alert dashboard.

---

## Alerts & CAPA

### compliance.compliance_alerts
Compliance status notifications: expirations, lapses, CAPA overdue, inspections due, training required, etc.

**Key fields:**
- `alert_type` — licence_expiry_imminent, licence_expired, critical_licence_missing, capa_overdue, inspection_due, training_required
- `severity` — low, medium, high, critical
- `target_audience` — clinic_admin, dpo, medical_director, board, compliance_officer, ceo
- `message`, `action_required`, `due_date` — details
- `status` — open, acknowledged, in_progress, resolved, escalated
- `acknowledged_by`, `acknowledged_at`, `resolved_at` — tracking

### compliance.inspection_records
Internal audits, regulatory inspections, accreditation visits.

**Key fields:**
- `inspection_type` — internal_audit, regulatory_inspection, accreditation_visit, surprise_inspection, followup_audit
- `inspection_date`, `conducted_by` — when/who
- `scope_description`, `findings_count`, `critical_findings`, `major_findings`, `minor_findings`
- `capa_required` — if true, CAPA items must be raised
- `report_s3_key` — scanned inspection report
- `scheduled_followup_date`, `followup_completed_at` — follow-up scheduling

### compliance.capa_items
Corrective and Preventive Actions: findings → root cause → action plan → verification.

**Key fields:**
- `inspection_id` (FK) — from which inspection
- `finding_code` — reference to specific finding
- `description`, `root_cause_analysis` — analysis
- `action_planned`, `assigned_to`, `due_date` — action details
- `status` — open, in_progress, completed, verified_effective, closed, deferred
- `completion_date`, `effectiveness_review_date` — verification
- `verified_by`, `verified_at` — who verified effectiveness

---

## Policies & Training

### compliance.policies
Policy and procedure versioning.

**Key fields:**
- `policy_code` (UNQ clinic+code+version) — e.g., FIRE_SAFETY_2026
- `policy_title`, `description`
- `version`, `effective_from`, `effective_to` — versioning
- `document_s3_key` — policy document
- `approval_date`, `approved_by` — approval workflow
- `compliance_mapped_to` (array) — regulations it satisfies
- `review_cycle_months`, `next_review_due` — review scheduling
- `status` — draft, approved, active, superseded, archived

### compliance.training_completions
Mandatory training: fire safety, biomedical waste handling, HIPAA/DPDP, etc.

**Key fields:**
- `staff_id` (FK)
- `training_name`, `training_category` — induction, mandatory_annual, regulatory_required, cme, skill_development
- `training_date`, `completion_hours`, `score`, `passed`
- `valid_until` — renewal due date
- `conducted_by`, `certificate_s3_key`

---

## Incident Tracking

### compliance.incidents
Safety, medication error, needle stick, fire, electrical, chemical spill, infection control breach, data breach incidents.

**Key fields:**
- `incident_date`, `incident_type`, `severity`
- `reported_by`, `description`, `immediate_action`
- `capa_raised` — if true, CAPA created
- `status` — reported, investigating, closed, escalated
- `investigation_notes`

---

## Notes

- **Licence renewal chain:** `previous_licence_id` allows audit history of all licence iterations.
- **Physical custody:** Every access logged; enables audit trail if document goes missing.
- **CAPA closure:** Effectiveness review mandatory; loop back to inspection if remediation fails.
- **Board visibility:** `is_critical` flags drive executive dashboards; board minutes track resolution.
