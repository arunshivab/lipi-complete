# Data Dictionary — Master Registry

Scope: `master` schema of the regional Master Registry database. Canonical reference for every column.

---

## master.organizations
Legal entity that owns one or more clinics (a single hospital, a chain, or a clinic group).

| Column | Type | Null | Default | Notes |
|---|---|---|---|---|
| id | uuid | NN | `core.uuid_v7()` | PK |
| legal_name | text | NN | | Name on PAN/GSTIN/MCA records |
| trading_name | text | Y | | Name used on signage / invoices |
| org_type | text | NN | | `single_clinic` \| `hospital_chain` \| `clinic_group` \| `government` \| `academic` |
| country_code | char(2) | NN | `IN` | ISO-3166 alpha-2 |
| pan | text | Y | | Indian Permanent Account Number (unique when present) |
| gstin | text | Y | | Indian GST Identification Number (unique when present) |
| cin | text | Y | | Corporate Identification Number (MCA) |
| registered_address | jsonb | NN | `{}` | Full postal address as FHIR-style object |
| primary_contact | jsonb | NN | `{}` | `{name, email, phone, designation}` |
| status | text | NN | `active` | `onboarding` \| `active` \| `suspended` \| `terminated` |
| onboarded_at | timestamptz | Y | | Date first clinic went live |
| extension_data | jsonb | NN | `{}` | FHIR-style extensions |
| created_at / updated_at / created_by / updated_by / deleted_at / row_version | audit columns |

---

## master.clinics
Individual clinic/hospital facility. Each clinic has its own dedicated OLTP database — see `master.clinic_databases`.

| Column | Type | Null | Notes |
|---|---|---|---|
| id | uuid | NN | PK |
| organization_id | uuid | NN | FK → organizations |
| code | text | NN | Human-friendly short code e.g. `APL-DEL-01` (unique within org) |
| name | text | NN | Display name |
| clinic_type | text | NN | `hospital` \| `cancer_centre` \| `diagnostic` \| `day_care` \| `clinic` \| `polyclinic` |
| city / state | text | NN | Denormalised for group-dashboard filtering |
| address | jsonb | NN | Structured address |
| timezone | text | NN | Default `Asia/Kolkata`; drives appointment scheduling |
| bed_count | int | Y | Licensed capacity |
| nabh_accredited | bool | NN | Drives NABH compliance tracking in clinic DB |
| nabl_accredited | bool | NN | Drives NABL lab compliance |
| has_oncology | bool | NN | Enables Oncology Suite modules |
| has_rt | bool | NN | Enables AERB radiation-therapy licences + RT-QA module |
| has_nuclear_med | bool | NN | Enables AERB radioactive-material licences + NM module |
| status | text | NN | `provisioning` \| `active` \| `suspended` \| `terminated` |
| go_live_at | timestamptz | Y | When `status` first became `active` |

---

## master.clinic_databases
Provisioning metadata — where each clinic's OLTP DB physically lives.

| Column | Notes |
|---|---|
| clinic_id | UNIQUE — 1:1 with clinic |
| db_host, db_port, db_name | Connection parameters |
| db_schema_version | Which clinic DDL migration is currently applied (e.g. `1.4.2`) |
| read_replica_host | Read-replica endpoint for BI queries |
| backup_policy | `{rpo_minutes, rto_minutes, retention_days}` |
| encryption_key_arn | AWS KMS CMK for per-tenant TDE |
| fdw_linked | `true` once `postgres_fdw` server is wired to the warehouse |
| health_status | `green` \| `yellow` \| `red` — set by health-check worker |

---

## master.clinic_groups / clinic_group_members
Logical grouping for combined dashboards. A clinic may belong to multiple groups. Group types: `regional` (North India), `clinical` (Oncology Group), `financial`, `reporting`, `custom`.

---

## master.subscription_plans / subscriptions / invoices
SaaS billing. `subscriptions.clinic_id` NULL means the subscription applies org-wide. `addon_modules` allows a-la-carte module purchases on top of the plan (e.g. plan = `professional`, addon = `esapi`).

---

## master.global_users
Cross-clinic users: LiPi platform admins/support, organization directors, group admins, auditors. **Does not include** clinic-scoped users (doctors, nurses, technicians) — those live in each per-clinic DB's `identity.users` table.

| Column | Notes |
|---|---|
| user_type | `platform_admin` \| `platform_support` \| `org_director` \| `group_admin` \| `auditor` |
| organization_id | NULL for LiPi platform staff |
| password_hash | Argon2id; NULL when user is SSO-only |
| is_mfa_enabled | Default true; strongly enforced for directors |
| locked_until | Set when `failed_login_count` exceeds threshold |

### master.global_user_clinic_access
Which clinics each global user can access, and at what level (`read` \| `report` \| `admin`). Required for directors of multi-clinic groups.

---

## master.identity_providers / idp_group_role_mappings
AD/Azure-AD/Okta/SAML/OIDC/Keycloak federation config per organization. `config` JSON holds provider-specific metadata (LDAP base DN, SAML metadata URL, OIDC issuer, etc).

`idp_group_role_mappings` is applied at login: user's IdP group memberships are resolved to LiPi role codes (e.g. `CN=Oncologists,...` → `oncologist`). `clinic_id` NULL means the mapping applies to every clinic in the org.

---

## master.feature_flags / feature_flag_overrides
Platform-wide flags with default on/off plus per-org / per-clinic / per-user overrides. Used for staged module rollouts.

---

## master.audit_events
Platform-level audit trail (tenant provisioning, subscription changes, login from platform users). **Not** clinical audit — each clinic has its own `audit.audit_events`. Hash-chained via `previous_hash`/`current_hash` for tamper-evidence; batches are anchored to Hyperledger by a background job (optional).

Partitioned monthly by `event_ts`.
