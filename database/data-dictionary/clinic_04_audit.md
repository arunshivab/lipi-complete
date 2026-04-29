# Data Dictionary — clinic :: `audit`

Scope: Tamper-evident, hash-chained audit trail. Every write to clinical data, every PHI read, and every export/print lands here. Partitioned monthly.

---

## Event Catalog & Audit Trail

### audit.event_types
Reference table: allowed audit event types.

**Fields:** `code` (PK), `category` (authentication, authorization, data_read, data_write, data_export, admin_action, clinical), `severity` (info/notice/warning/alert/critical), `is_phi_access` (PHI-triggering), `nabh_required`, `dpdp_required`.

### audit.audit_events
Core append-only table: every action recorded here. Partitioned monthly by `event_ts`.

**Key fields:**
- `id`, `event_ts` (composite PK) — UUIDv7 + timestamp
- `clinic_id` (FK, RLS) — tenant
- `action` (FK) — event_types.code
- `actor_user_id`, `actor_username`, `actor_role_codes` — WHO (denormalized snapshot)
- `actor_ip`, `actor_user_agent`, `actor_session_id` — context
- `entity_type`, `entity_id`, `entity_parent_id` — WHAT (e.g., 'core.patient', patient UUID)
- `action_detail`, `before_state`, `after_state` (JSONB) — HOW
- `change_reason` — required for break-glass operations
- `request_id`, `correlation_id` — traceability
- `outcome` — success, failure, denied, error
- `previous_hash`, `current_hash` (bytea) — hash-chain link

**Hash-chaining:** `current_hash = SHA-256(previous_hash || event_ts || action || entity_type || entity_id || before_state || after_state)`.

**Restrictive policies:** UPDATE and DELETE are forbidden for all except superuser, enforced via RLS.

### audit.hash_chain_tip
One row per clinic: the current tip of the hash chain (latest hash).

**Fields:** `clinic_id` (PK), `tip_hash`, `tip_event_id`, `tip_event_ts`, `updated_at`.

### audit.hash_chain_verification_runs
Periodic jobs that re-walk the entire audit chain to detect tampering.

**Fields:** `started_at`, `finished_at`, `window_from`, `window_to`, `rows_checked`, `breaks_found`, `first_break_id`, `outcome` (running/clean/break_detected/error).

---

## PHI Access & Data Egress

### audit.phi_access_log
HIPAA § 164.312(b) + DPDP compliance: every PHI read by a user. Partitioned monthly.

**Key fields:**
- `accessed_at` (FK) — when PHI was accessed
- `actor_user_id`, `actor_role_codes`, `actor_ip` — WHO
- `patient_id` (FK) — WHICH patient
- `mrn_snapshot` — MRN at access time
- `access_context` — direct_care, referral, billing, research, break_glass, self_access, public_health
- `resource_type`, `resource_id` — WHAT (encounter, lab_report, prescription, etc.)
- `purpose_of_use` — stated purpose
- `justification` — required if context=break_glass
- `minimum_necessary_principle_ok` — compliance marker
- `consent_ref` — reference to active consent

### audit.export_log
Bulk data egress: patient summaries, reports, backups, research datasets, etc.

**Key fields:**
- `export_type` — patient_summary, mis_report, dicom, research_dataset
- `export_format` — pdf, xlsx, csv, fhir_json
- `record_count` — # of records exported
- `patient_ids` (UUID array) — if patient-specific
- `query_parameters` (JSONB) — filter criteria
- `destination` — download, email, sftp, s3, api
- `destination_detail` — email address, SFTP path, etc.
- `file_sha256`, `file_size_bytes` — integrity & size
- `dlp_scan_result` — clean, warning, blocked, skipped
- `consent_ref`, `justification` — data sharing basis

### audit.print_log
Printing of sensitive documents (discharge summaries, prescriptions, lab reports).

**Fields:** `printer_name`, `document_type`, `document_ref_id`, `patient_id`, `copies`, `watermark_applied`, `client_ip`.

---

## Blockchain Anchoring (Optional)

### audit.blockchain_anchors
Periodic batch hashing of audit events, written to Hyperledger or other ledger for immutability proof.

**Fields:** `anchor_type` (audit_events/phi_access/export_log), `window_from`, `window_to`, `row_count`, `batch_merkle_root`, `ledger_tx_id`, `ledger_block_no`, `status` (pending/anchored/verified/mismatch).

---

## Notes

- **Append-only:** No UPDATE/DELETE allowed (RLS restrictive policies).
- **Hash-chaining:** Detects tampering: any row modified breaks the chain.
- **Partitioning:** Monthly by event_ts / accessed_at for efficient queries and archival.
- **FHIR:** `before_state` and `after_state` can be FHIR resources for clinical events.
- **Retention:** Per DPDP Act 2023, audit logs retained per data classification (typically 5+ years).
