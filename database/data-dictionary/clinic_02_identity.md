# Data Dictionary — clinic :: `identity`

Scope: `identity` schema of each per-clinic OLTP database. User identity, authentication, authorization (RBAC), sessions, multi-factor authentication, API keys, and directory synchronization.

---

## Users & Authentication

### identity.users
Clinic-scoped user identity (doctor, nurse, admin, or system service account). Each user belongs to exactly one clinic but can access multiple departments via scoped roles.

**Key fields:**
- `username` (citext, UNQ per clinic) — LDAP sAMAccountName or email local part
- `password_hash` — Argon2id; NULL if SSO-only
- `staff_id` — FK to core.staff; NULL for service accounts
- `is_ad_managed` — true if synced from Active Directory/LDAP; prevents local password change
- `ad_object_guid` — GUID for stable re-sync across AD updates
- `status` — invited, active, suspended, locked, terminated
- `is_mfa_enforced` — mandatory MFA requirement (default true)
- `failed_login_count`, `locked_until` — account lockout tracking

**RLS:** Clinic-scoped. **Soft-delete:** `deleted_at`.

---

### identity.sessions
Short-lived JWT + refresh token state. One session per active user login.

**Key fields:**
- `refresh_token_hash` — SHA-256 of refresh token (never raw)
- `jwt_jti` — JWT id, used for revocation checks
- `client_ip`, `user_agent`, `device_fingerprint` — client context
- `issued_at`, `expires_at`, `last_active_at` — lifecycle
- `revoked_at`, `revocation_reason` — early termination

**Index strategy:** `(user_id) WHERE revoked_at IS NULL` for active session lookups.

---

## Authorization (RBAC)

### identity.roles
Role definition (e.g., "oncologist", "opd_nurse"). Clinic-scoped.

**Key fields:**
- `code` (UNQ per clinic) — short identifier
- `is_system_role` — shipped by LiPi, cannot be renamed
- `is_active` — allows soft-disable without cascade delete

---

### identity.permissions
System-wide permission catalog (not clinic-scoped). Examples: `opd.encounter.create`, `rtqa.approve`, `compliance.license.edit`.

**Key fields:**
- `permission_code` (UNIQUE) — fully qualified permission name
- `module` — opd, ipd, rtqa, compliance, etc.
- `action` — read, create, update, delete, approve, sign, export
- `is_phi_sensitive` — true triggers `phi_access_log` audit entry

---

### identity.role_permissions
Junction table: many roles can have many permissions.

---

### identity.user_roles
User-role assignment, optionally scoped to a single department. Allows the same user to hold the same role in multiple departments with different scope.

**Key fields:**
- `scope_department_id` — NULL = global role; FK to core.departments otherwise
- `valid_from`, `valid_to` — time-bound role assignment

**Constraint:** Partial unique index `(user_id, role_id) WHERE scope_department_id IS NULL` to prevent duplicate unscoped assignments.

---

## MFA (Multi-Factor Authentication)

### identity.mfa_methods
MFA enrollment: TOTP, WebAuthn, SMS OTP, email OTP, backup codes.

**Key fields:**
- `method_type` — totp, webauthn, sms_otp, email_otp, backup_codes
- `label` — user-friendly name (e.g., "iPhone 15")
- `secret_encrypted` — TOTP secret, KMS envelope-encrypted
- `webauthn_credential_id`, `webauthn_public_key`, `webauthn_counter` — WebAuthn state
- `is_primary` — which method is used by default
- `is_verified` — confirmed functional
- `revoked_at` — logical revocation

---

## Authentication Audit

### identity.login_attempts
Record of every login attempt (successful and failed).

**Key fields:**
- `username`, `user_id`, `client_ip`, `user_agent` — request context
- `auth_method` — password, ad, saml, oidc, api_key, service, mfa
- `outcome` — success, bad_password, unknown_user, mfa_failed, locked, disabled, rate_limited, suspicious
- `failure_detail` — reason for failure
- `geo_country`, `geo_city` — optional geolocation

**Partitioning:** Monthly range on `attempted_at`.

---

### identity.password_history
Historical password hashes (not plaintext). Enforces no-reuse policies.

---

## Service Accounts & API Keys

### identity.service_accounts
Automation account for integrations (modality bridge, lab analyser, ESAPI sync, ABDM gateway, HL7/FHIR ingest, billing, etc.).

**Key fields:**
- `user_id` — backed by a user row (allows sessions, audit logging)
- `purpose` — integration type (driven by capabilities)
- `allowed_ip_cidrs` — whitelist of source IPs
- `is_active` — soft-disable

---

### identity.api_keys
Per-service-account API keys for stateless client authentication.

**Key fields:**
- `key_prefix` — first 8 characters for display/lookup; UNQ
- `key_hash` — SHA-256(actual key); never store plaintext
- `scopes` — array of permission codes this key can exercise
- `expires_at` — optional expiration
- `last_used_at`, `last_used_ip` — usage tracking
- `revoked_at` — logical revocation

---

## Directory Integration

### identity.ad_sync_runs
Track Active Directory / LDAP synchronization jobs.

**Key fields:**
- `identity_provider_id` — FK to master.identity_providers
- `status` — running, success, failed, partial
- `users_created`, `users_updated`, `users_disabled`, `users_unchanged` — sync results
- `error_count`, `error_details` (JSONB) — failures per user
- `high_water_mark` — last processed AD uSNChanged (incremental sync anchor)

---

## Notes

- **Clinic isolation:** All tables carry `clinic_id` RLS constraint. EF Core connection interceptor sets `lipi.clinic_id` on every checkout.
- **Soft-delete:** `deleted_at` on users allows audit trail recovery. Hard-delete only via DPDP erasure request.
- **Concurrency:** `row_version` on users for optimistic locking.
- **Password hashing:** Argon2id (configured in app security layer).
- **Session revocation:** Checking JWT `jti` against sessions table allows real-time revocation without token expiry.
