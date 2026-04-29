# Data Dictionary — clinic :: `core`

Scope: `core` schema of each per-clinic OLTP database. Foundational reference data, facility topology, person/patient/staff master, consent.

Every other clinic schema (opd, ipd, lab, ...) FKs into tables here.

---

## Reference / geography

### core.countries / states / cities / pincodes
Populated at DB-provisioning time from authoritative data (ISO-3166, India Post). `pincodes` carries the PIN code + area name.

### core.addresses
Reusable structured address (one row per physical address) — referenced by persons, organizations, facilities, emergency contacts, etc. via `person_addresses` link table.

---

## Medical codesets

Populated from authoritative external catalogues during DB provisioning, updated by a central sync job:

| Table | Source | Use |
|---|---|---|
| core.icd10_codes | WHO ICD-10 2019 + CM modifications | Diagnoses, discharge summary, mortality |
| core.snomed_codes | SNOMED-CT International | Clinical concepts, problems, procedures |
| core.loinc_codes | LOINC | Lab & observation terminology |
| core.rxnorm_codes | RxNorm / India drug index | Medications |
| core.cdsco_device_classes | CDSCO India | Medical device classification |

All code tables have trigram GIN indexes for fuzzy search.

---

## Facility topology

```
facility → departments → wards → rooms → beds
```

### core.facility
A physical building. Multi-facility clinics (main hospital + day-care annexe) have multiple rows.

### core.departments
Functional units. `department_type` drives which modules / compliance requirements apply:
- `oncology_rt` → AERB licences, RT-QA module
- `oncology_med` → chemo protocols module
- `oncology_surg` → OT scheduling
- `nuclear_med` → radioactive-material licences

### core.specialties
Hierarchical list (ontology) used to tag staff and book appointments. `parent_id` allows sub-specialties (e.g. `radiology.interventional` under `radiology`).

### core.wards / rooms / beds
`beds.status` cycle: `available` → `reserved` → `occupied` → `cleaning` → `available`. Triggers in `02_ipd.sql` keep `current_admission_id` in sync.

---

## Identity master

### core.persons
Single base identity for every human in the clinic — patient, staff, kin. 1:1 join with `patients` OR `staff` (a person might be both; each relationship is optional).

`display_name` is a **GENERATED STORED** column — do not write to it.

Names are stored with Unicode support so transliterated Indic names survive round-trips.

### core.contact_points
FHIR-aligned multi-system contact info (phone / email / fax / whatsapp). `is_verified` flips true after OTP or link confirmation.

### core.patients
Extends `persons` with clinic-local MRN plus optional org-level `uhid`. Key business fields:
- `active_alerts` — summary array (allergies, isolation flags) cached for every screen
- `patient_type` — drives billing tariff selection (`cgs` = Central Government Scheme, `ecs` = Ex-Servicemen Contributory Scheme)
- `death_cause_icd10` — required for mortality reporting

### core.patient_identifiers
All external IDs in one table — easy to search by any value. Supported types:
| identifier_type | format | source |
|---|---|---|
| abha_number | `XX-XXXX-XXXX-XXXX` | ABDM |
| abha_address | `user@abdm` | ABDM |
| aadhaar | 12 digits | UIDAI |
| pan / passport / voter_id / driving_licence | various | Government |
| insurance / tpa | string | Private insurers |
| employee_id / student_id | clinic-local | HR / institutional |
| foreign_national_id | string | International patients |

`verification_ref` stores the UIDAI txn-id or ABDM linkToken for audit.

### core.emergency_contacts
Minimum one primary contact per patient recommended; enforced in application layer.

### core.staff
Clinic-scoped employees. `hpr_id` is the ABDM Healthcare Professional Registry number — mandatory for doctors in ABDM-linked workflows. `mci_registration_no` = NMC/state-council number.

`staff_type` drives UI menu rendering and RT-QA role assignments (e.g. `medical_physicist` can sign machine-QA forms).

### core.staff_departments
A physicist can be shared between Rad-Onco and Nuclear Medicine — many-to-many table captures this; exactly one `is_primary = true` per staff (enforced by app layer + deferrable constraint planned).

---

## Consent

### core.consents
DPDP Act 2023 + HIPAA consent artefacts. `consent_type` covers clinical (surgery, anaesthesia), data-sharing (ABDM link, HIE), and research/admin (photography, autopsy, organ donation).

`scope` JSONB is FHIR-Consent-compatible — `{provisions: [{purpose, actor, data_type, ...}]}` — so that the Compliance & Audit schemas can reconstruct purpose-limitation.

`abdm_consent_artefact_id` links to the ABDM Consent Manager artefact for any sharing action.

Revocation: never delete; set `revoked_at` and keep the row. Downstream data-access checks evaluate both `granted=true` and `revoked_at IS NULL`.

---

## Row-Level Security

Every tenant-scoped table has a policy:
```sql
USING (clinic_id = current_setting('lipi.clinic_id')::uuid)
WITH CHECK (clinic_id = current_setting('lipi.clinic_id')::uuid)
```
The EF Core connection interceptor issues `SET LOCAL lipi.clinic_id = '...'` from the JWT `clinic_id` claim on every connection checkout. Even though the physical DB is clinic-isolated, this catches any app-layer bug that would leak one clinic's data into another's context.

Reference-only tables (countries, states, cities, pincodes, icd10_codes, snomed_codes, loinc_codes, rxnorm_codes, cdsco_device_classes, specialties) are **not** RLS-protected — they are global reference data.

---

## Naming & audit conventions

- Every mutable table carries `created_at`, `updated_at`, `created_by`, `updated_by`, `deleted_at`, `row_version` — populated by triggers or EF Core save interceptors.
- Soft-delete (`deleted_at`) is the default. Hard-delete only on explicit DPDP erasure request (see `security.dpdp_erasure_requests`, Wave 1).
- All UUIDs generated by `core.uuid_v7()` for B-tree locality at scale.
