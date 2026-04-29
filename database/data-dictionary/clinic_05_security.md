# Data Dictionary — clinic :: `security`

Scope: Cryptographic key management (envelope encryption), DPDP Act 2023 consent & rights, security events, rate limiting, data classification, and data localization.

---

## Encryption & Key Management

### security.encryption_keys
Data Encryption Keys (DEK) wrapped by AWS KMS Customer Master Key (CMK). One per purpose per version per clinic.

**Key fields:**
- `key_purpose` — phi_field, attachment, dicom, audit_log, mfa_secret, abdm_token, backup
- `key_version` — version number for rotation tracking
- `wrapped_dek` (bytea) — KMS-encrypted DEK
- `kms_cmk_arn` — AWS KMS CMK ARN
- `algorithm` — AES-256-GCM
- `is_active` — only one active per purpose
- `activated_at`, `retired_at` — lifecycle
- `rotation_due_at` — schedule for next rotation

**Uniqueness:** `(clinic_id, key_purpose, key_version)`.

### security.data_classifications
Reference table: how data should be treated based on sensitivity.

**Fields:** `code` (PK: public, internal, confidential, restricted, phi, spi), `dpdp_category` (personal, sensitive_personal, critical_personal, non_personal), `retention_days`, `encryption_required`, `audit_required`.

---

## DPDP Act 2023: Consent & Rights

### security.dpdp_consents
Platform-wide data-processing consents (distinct from clinical consent).

**Key fields:**
- `data_principal_type` — patient, staff, visitor, vendor
- `data_principal_id` — patients.id or staff.id
- `purpose_code` — treatment, billing, research, marketing, etc.
- `lawful_basis` — consent, legitimate_use, legal_obligation, medical_emergency, employment, public_interest
- `data_categories` — identity, contact, health, biometric, financial, location
- `granted`, `granted_at` — consent given and timestamp
- `valid_from`, `valid_to` — time-bound consent
- `withdrawal_channel` — app, email, sms, paper, in_person
- `withdrawn_at` — revocation timestamp
- `proof_s3_key` — scanned/verified consent artefact
- `notice_version` — which notice was shown to principal

**Index:** `(data_principal_type, data_principal_id, valid_to) WHERE granted AND withdrawn_at IS NULL` for active consents.

### security.dpdp_subject_requests
Data subject rights requests: access, correction, erasure, portability, grievance, consent withdrawal.

**Key fields:**
- `request_type` — access, correction, erasure, portability, nominee, grievance, consent_withdrawal
- `data_principal_type`, `data_principal_id` — WHO is requesting
- `request_channel` — app, email, paper, in_person, phone
- `requested_at`, `sla_deadline` — 30 days from request per DPDP Act 2023
- `status` — received → identity_verified → in_progress → fulfilled (or partial/rejected/escalated)
- `assigned_to` — DPO or delegate handling request
- `identity_verification_method`, `identity_verified_at` — KYC for request legitimacy
- `fulfilled_at` — when response delivered
- `response_artefact_s3_key` — download link for access/portability requests
- `rejection_reason`, `dpo_notes` — notes if denied

**SLA tracking:** Dashboard alerts on `sla_deadline`.

### security.dpdp_erasure_tasks
Decomposition of an erasure request across target systems (OLTP, data warehouse, S3, DICOM, backups, ABDM HIE).

**Key fields:**
- `dpdp_subject_request_id` (FK) — parent request
- `target_system` — oltp, warehouse, s3_attachments, dicom, backup, abdm_hie
- `status` — pending, running, completed, failed, skipped_legal_hold
- `rows_affected`, `executed_at` — execution tracking
- `legal_hold_reference` — if retention-obligated (statutory requirement), cite the basis

---

## Security Events

### security.security_events
SIEM feed: anomalies, privilege escalation, data exfiltration, malware, policy violations, key rotations, DLP triggers.

**Key fields:**
- `event_category` — auth_anomaly, privilege_escalation, data_exfiltration, malware, policy_violation, config_drift, dlp, ids_ips, break_glass, key_rotation
- `severity` — info, low, medium, high, critical
- `actor_user_id`, `actor_ip`, `source_host` — source
- `signature` — rule/IOC id that matched
- `detail` (JSONB) — enriched context (IPs, domains, file hashes, etc.)
- `status` — open, investigating, resolved, false_positive, suppressed
- `triaged_by`, `triaged_at` — SOC analyst review
- `resolution_notes` — how it was resolved

### security.break_glass_sessions
Emergency PHI access override (life-threatening emergency, unconscious patient, no consenting kin, statutory requirement).

**Key fields:**
- `user_id`, `patient_id` — WHO accessed WHICH patient
- `activated_at`, `expires_at` — max 4 hours typical
- `reason_code` — life_threatening, unconscious_patient, no_consenting_kin, statutory, other
- `justification` — mandatory free-text explanation
- `approved_by`, `approval_at` — initial approval (post-access)
- `reviewed_by`, `reviewed_at` — auditor review (24-48 hours post-access)
- `review_outcome` — appropriate, inappropriate, training_required, disciplinary

---

## Access Control

### security.ip_allowlist
Whitelist of IP CIDRs for administrative endpoints.

**Fields:** `cidr`, `scope` (admin_console, api, abdm_ingress, dicom_ingress, hl7_ingress), `is_active`, `expires_at`.

### security.rate_limit_buckets
Per-user, per-API-key, per-IP rate limiting state (sliding window).

**Fields:** `bucket_key` (user:<uuid>/apikey:<id>/ip:<cidr>), `window_start`, `request_count`, `rejected_count`.

---

## Data Residency

### security.data_localization_policies
Ensures Indian data stays in India per DPDP Act 2023 § 16.

**Key fields:**
- `data_category` — phi, financial, employee, imaging, backup
- `storage_region` — ap-south-1, ap-south-2 (AWS India regions)
- `replication_regions` — where copies allowed
- `cross_border_allowed` — if false, export forbidden
- `cross_border_countries` — if allowed, which countries
- `policy_version`, `effective_from`, `effective_to` — versioning
- `approved_by`, `approved_at` — board approval

---

## Notes

- **Key rotation:** Automatic via `rotation_due_at` alerts; old keys retired but retained for decryption of old ciphertext.
- **DPDP compliance:** 30-day SLA on all data subject requests; escalation alerts at day 25.
- **Break-glass audit:** Post-access review mandatory; outcome feeds training/discipline workflows.
- **Data localization:** Infrastructure as code enforces region constraints; cross-border attempts blocked at DB/API layer.
