# Data Dictionary — clinic :: `abdm`

Scope: ABDM (Ayushman Bharat Digital Mission) integration — ABHA profiles, consent management, health information exchange, Aadhaar eKYC, and ABDM gateway request logging.

---

## ABDM Registries

### abdm.facility_registry
Clinic's presence in ABDM Health Facility Registry (HFR). One per clinic.

**Key:** `clinic_id` UNIQUE. **Fields:** `hfr_facility_id` (ABDM-assigned), `hfr_status` (pending/verified/suspended/delisted), `registry_payload` (JSONB: full HFR response).

### abdm.professional_registry
Staff member's presence in ABDM Healthcare Professional Registry (HPR). One per staff per clinic.

**Key:** `staff_id` UNIQUE. **Fields:** `hpr_id` (ABDM-assigned, mandatory for doctors in ABDM workflows), `hpr_status`, `council_name`, `council_reg_no` (NMC/state-council registration).

---

## ABHA & Linking

### abdm.abha_profiles
One ABHA (Ayushman Bharat Health Account) per patient who has linked. ABHA is the patient's national health identifier in ABDM.

**Key fields:**
- `patient_id` — FK to core.patients
- `abha_number` (UNQ) — `XX-XXXX-XXXX-XXXX` format
- `abha_address` (UNQ) — `user@abdm` ABHA address
- `name`, `gender`, `date_of_birth` — demographic snapshot
- `mobile_verified`, `email_verified`, `kyc_verified` — verification status
- `kyc_method` — aadhaar_otp, aadhaar_biometric, driving_licence, demographic
- `abha_qr_s3_key` — QR code for patient scanning
- `linked_at`, `revoked_at` — lifecycle

**Index:** `(abha_number)` for lookups; `(patient_id, linked_at DESC)` for patient history.

### abdm.link_requests
Linking workflow state: initiation, OTP, verification, final link.

**Key fields:**
- `abha_profile_id` — FK
- `link_token` (UNQ) — ABDM-issued linking token
- `link_type` — user_initiated or hip_initiated
- `auth_mode` — demographic, mobile_otp, aadhaar_otp, password, direct
- `status` — requested → otp_sent → verified → linked (or failed/expired)

### abdm.care_contexts
Episodes of care exposed under a linked ABHA. Each context is a reference to a local encounter/report that the patient can share.

**Key fields:**
- `reference_id` — encounter_id, report_id, etc. (clinic-local, not ABDM)
- `context_type` — opd_visit, ipd_admission, discharge_summary, prescription, diagnostic_report, immunization
- `display` — human-readable name shown to patient
- `visible` — whether patient has consented to share this context
- `unlinked_at` — patient revoked context sharing

**Uniqueness:** `(abha_profile_id, reference_id, context_type)`.

---

## Consent & HI Exchange

### abdm.consent_requests
Request for patient health information sharing via ABDM Consent Manager.

**Key fields:**
- `requester_type` — HIP (Health Information Provider) or HIU (Health Information User)
- `consent_manager_request_id` — request ID from CM
- `purpose_code` — CAREMGT, PATRQT, PUBHLTH (FHIR Purpose of Use)
- `hi_types` — array of health info types (DiagnosticReport, Prescription, Immunization, etc.)
- `date_range_from`, `date_range_to` — date range of data access
- `fetch_mode` — VIEW (read-only) or STORE (copy data)
- `frequency_unit`, `frequency_value` — recurring access pattern (e.g., MONTH=1 for monthly)
- `status` — requested → granted/denied; can be revoked/expired

### abdm.consent_artefacts
Signed consent artefact issued by ABDM Consent Manager. Cryptographically signed.

**Key fields:**
- `consent_request_id` — FK to consent_requests
- `artefact_id` (UNQ) — CM-issued identifier
- `signature` — CM digital signature
- `schema_version` — consent schema version
- `artefact_payload` (JSONB) — full signed artefact (FHIR Consent resource)
- `issued_at`, `valid_from`, `valid_to` — lifecycle
- `revoked_at`, `revocation_reason` — patient revocation

### abdm.hi_exchange_sessions
Health information exchange session (HIP sending to HIU or self-serving to patient).

**Key fields:**
- `consent_artefact_id` — FK
- `role` — HIP (provider) or HIU (consumer)
- `transaction_id` (UNQ) — ABDM transaction identifier
- `key_material` (JSONB) — ECDH public key + nonce (never private key)
- `status` — initiated → ciphered → delivered → acknowledged (or failed)
- `requested_at`, `completed_at` — timeline

### abdm.hi_care_bundle_log
Log of FHIR bundles sent in an HI exchange session. Raw bundle streamed to S3; only hash stored.

**Key fields:**
- `care_context_ref` — reference to which care context was bundled
- `bundle_sha256` — hash of FHIR bundle (integrity check)
- `bundle_s3_key` — S3 location of actual bundle
- `sent_at` — when bundle was delivered

---

## ABDM Gateway

### abdm.gateway_requests
Every outbound call to ABDM Gateway API. Partitioned monthly for audit/retry.

**Key fields:**
- `api_path` — e.g., `/v0.5/users/auth/init`
- `http_method` — GET, POST, etc.
- `correlation_id` — ABDM correlation ID for tracing
- `request_body`, `response_body` (JSONB) — full request/response
- `response_status` — HTTP status
- `duration_ms` — latency
- `outcome` — success, gateway_error, timeout, validation_error, auth_error, retried

**Indexes:** `(correlation_id)` for request tracing; `(clinic_id, outcome, requested_at DESC)` for monitoring.

---

## Aadhaar eKYC

### abdm.aadhaar_ekyc_requests
Aadhaar verification via UIDAI eKYC API (used for patient/staff KYC in ABDM workflows).

**Security note:** Never stores Aadhaar number in plaintext. Instead:
- `aadhaar_reference_hash` — HMAC(aadhaar, tenant_salt) — one-way hash
- `ekyc_payload_encrypted` — KMS envelope-encrypted UIDAI response

**Key fields:**
- `patient_id`, `staff_id` — one or the other (CONSTRAINT)
- `txn_id` (UNQ) — UIDAI transaction ID
- `auth_mode` — otp, biometric_fp, biometric_iris, demographic
- `status` — initiated → otp_sent → verified (or failed/expired)
- `requested_at`, `verified_at` — timeline

---

## Subscriptions

### abdm.subscription_callbacks
HIU subscribes to a HIP's new care contexts (push notification capability).

**Key fields:**
- `abha_profile_id` — FK
- `subscription_id` (UNQ) — ABDM subscription ID
- `subscriber_hiu_id` — HIU's ABDM ID
- `hi_types` — array of health info types subscribed to
- `status` — active, paused, revoked, expired

---

## Notes

- **FHIR Compliance:** Consent artefacts and HI bundles follow FHIR R4 standards.
- **Encryption:** eKYC payloads encrypted via AWS KMS; key_material in HI exchanges is public key only.
- **Privacy:** Aadhaar never stored as plaintext; only HMAC hash + encrypted payload.
- **Partitioning:** gateway_requests partitioned monthly for efficient audit query.
- **Audit trail:** All ABDM interactions logged for DPDP compliance and incident investigation.
