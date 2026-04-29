# Data Dictionary — clinic :: `certs`

Scope: Professional certifications, board registrations, CME/CPD credits, competency assessments, training records, and credential verification.

---

## Credentials & Registrations

### certs.credential_types
Catalogue of professional credentials: medical degrees (MBBS, MD, MS, DNB), specialist boards, support credentials.

**Fields:** `code` (PK: mbbs, md, ms, dnb, frcs, fams, bls), `credential_level` (basic/intermediate/advanced/specialist), `category` (medical_degree/nursing_degree/specialist_board/support_services/technical), `issuing_authority`, `issuing_country` (ISO), `validity_years` (NULL if perpetual), `mandatory_for_roles` (array of staff_type codes), `mci_recognised`, `international_equiv` (ISO equivalence).

### certs.staff_credentials
Individual credentials held by staff. One row per unique (staff, credential_type) pair.

**Key fields:**
- `staff_id` (FK), `credential_type_code` (FK UK)
- `credential_number` (UNQ) — authority-issued ID
- `issued_date`, `issued_by`, `valid_from`, `valid_to` — lifecycle
- `country_issued` (ISO) — country of issuance
- `discipline_specialisation` — e.g., Diagnostic Radiology
- `sub_specialisation` — e.g., Interventional Radiology
- `verification_status` — unverified, verification_pending, verified, expired, revoked, suspended
- `verification_method` — manual_review, online_portal, issuing_authority_confirmation, third_party_api
- `verified_at`, `verified_by` — verification details
- `certificate_s3_key`, `portal_url` — proof links
- `renewal_required`, `renewal_due_date` — if renewable credential
- `is_primary` — primary qualification for role
- `scope_limitations` — restrictions (e.g., limited_to_ct_mri)

**Index:** `(staff_id, clinic_id)`; `(clinic_id, renewal_due_date) WHERE renewal_required`.

### certs.board_registrations
Board/council registrations: NMC doctor, NMC nurse, INC, etc.

**Key fields:**
- `staff_id` (FK), `board_code` (FK UK) — e.g., nmc_doctor
- `board_name`, `registration_number` (UNQ)
- `registration_state` — state-level registration (India)
- `registered_date`, `valid_from`, `valid_to`, `renewal_date`
- `registration_status` — active, lapsed, renewed, suspended, revoked, cancelled
- `suspension_reason`, `suspension_from`, `suspension_to` — suspension details
- `certificate_s3_key`, `online_verify_url` — proof links
- `is_primary` — primary board registration

**Index:** `(staff_id, clinic_id)`; `(clinic_id, renewal_date) WHERE registration_status='active'`.

---

## CME & CPD Tracking

### certs.cme_credits
Individual continuing medical education credits earned.

**Key fields:**
- `staff_id` (FK)
- `credit_type` — conference, workshop, online_course, journal_club, grand_rounds, publication, teaching, research
- `credit_provider` — IMA, NMC, ASCO, European Board
- `credit_points` — hours or credit points awarded
- `credit_date`, `activity_title`, `activity_location`
- `certificate_s3_key`, `credit_identifier` — proof
- `approved`, `approved_by` — optional approval workflow
- `notes`

### certs.cme_requirements
Rolling CME/CPD targets by year and requirement type.

**Key fields:**
- `staff_id`, `year` (UNQ with staff+requirement_type)
- `requirement_type` — cme, cpd, specialty_maintenance, board_recertification
- `required_credits`, `earned_credits` — progress tracking
- `deadline_date`, `status` — in_progress, completed, partial, overdue, exempted
- `exemption_reason`, `exempted_by`, `exempted_at` — if exempted

---

## Competency & Training

### certs.competency_assessments
Clinical competency assessments: skills, procedures, communication, teaching effectiveness.

**Key fields:**
- `staff_id` (FK), `assessment_date`
- `assessment_type` — clinical_skills, procedural_competency, communication, teaching_effectiveness, research_output, patient_safety, team_work
- `assessor_id` (FK) — supervising physician/senior staff
- `competency_level` — novice, beginner, competent, proficient, expert, needs_remediation
- `score`, `max_score`, `comments`
- `evidence_documents` (array) — s3 keys of supporting docs
- `action_plan` — if needs_remediation
- `followup_date` — for remediation tracking

### certs.training_records
Structured training completion: induction, onboarding, mandatory annual, safety drills, incident response.

**Key fields:**
- `staff_id` (FK)
- `training_code` — e.g., INDUCTION_2026, FIRE_DRILL_2026_Q1
- `training_name`, `training_category` — induction, onboarding, mandatory_annual, role_specific, regulatory, safety_drill, incident_response, skills_upgrade
- `start_date`, `end_date`, `duration_hours`
- `trainer_name`, `trainer_org`, `training_mode` — in_person, online, hybrid, self_paced, on_job
- `completion_status` — in_progress, completed, cancelled, deferred
- `test_required`, `test_score`, `test_passed` — if knowledge test
- `certificate_s3_key`, `competency_verified`, `verified_by`

---

## Credential Holds & Disciplinary

### certs.credential_holds
Temporary hold on credentials due to investigation, malpractice suit, or integrity concern.

**Key fields:**
- `staff_id` (FK)
- `hold_reason` — criminal_investigation, malpractice_suit, credential_verification_pending, integrity_concern, patient_complaint, regulatory_action
- `held_from`, `held_until` — hold period
- `hold_details`, `imposed_by` — details
- `appeal_filed`, `appeal_date`, `appeal_outcome` — appeal process
- `lifted_at`, `lifted_by` — removal

---

## Academic Output

### certs.publications
Publication and research output tracking (for CV, appraisal, academic standing).

**Key fields:**
- `staff_id` (FK), `publication_date`
- `publication_type` — peer_reviewed_journal, conference_paper, book_chapter, editorial, case_report, letter
- `journal_name`, `conference_name`, `title`
- `authors` (comma-separated), `author_position` — author list
- `doi`, `pmid`, `url` — identifiers
- `impact_factor`, `keywords`, `abstract`
- `is_corresponding_author`, `citation_count` — tracking

### certs.awards
Honours and recognitions.

**Key fields:**
- `staff_id`, `award_date`
- `award_name`, `awarding_organization`, `award_category` — clinical_excellence, research, teaching, patient_care, innovation
- `award_level` — departmental, institutional, state, national, international
- `certificate_s3_key`

---

## Verification Checklist

### certs.credential_verification_checklist
Pre-hire or privileging verification checklist.

**Fields:** `staff_id`, `checklist_date`, `medical_degree_verified`, `board_registration_verified`, `liability_insurance_verified`, `background_check_verified`, `references_verified`, `previous_employment_verified`, `disciplinary_history_checked`, `all_verified` (boolean), `verified_by`, `notes`.

---

## Notes

- **Renewals:** `renewal_due_date` drives alerts in compliance module; renewal_required credentials must be resubmitted before `valid_to`.
- **Verification:** Portal URLs allow real-time confirmation with issuing authorities (NMC, INC, boards).
- **Competency:** Links to certs; trainer observations feed appraisal and promotion workflows.
- **CV generation:** Publications, awards, credentials exported for institutional CVs and grant applications.
