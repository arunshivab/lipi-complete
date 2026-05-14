# LiPi HIS — Project Facts (LOCKED)

This document consolidates project-level facts that are locked decisions. Read once,
treat as ground truth. These don't change without explicit re-discussion.

---

## Tech stack

- **.NET 10** — default for all projects. No .NET 7/8/9 patterns unless explicitly asked
- **Blazor Web App** — InteractiveServer only (no WASM)
- **PostgreSQL** + **EF Core 10** + **Npgsql**
- **HIPAA compliance** + **Six Sigma** required on every feature including dashboards

## Multi-tenancy

- **One PostgreSQL database per clinic** (completely separate)
- One central **master** database
- **GlobalAdmin** and **SysAdmin** have global rights across all databases
- Each clinic DB has a default **SiteAdmin**
- All three admin users (GlobalAdmin, SysAdmin, SiteAdmin) are **undeletable by design**

## User architecture

- Staff = `master.platform_users` only
- Clinic DB staff refs are plain UUIDs resolved from master at display time
- Departments / Specialties = clinic DB
- Certs = master DB
- `identity.users` REMOVED
- `identity.ad_sync_runs` REMOVED

## Roles (15 mutually exclusive groups)

clinician, nursing, physicist, dosimetrist, rtt, rad_tech, lab_tech, nm_tech,
ot_tech, cssd_tech, billing, pharmacy, dietician, physiotherapist, counsellor.

**Operations + RSO** are add-ons OR standalone primary.
**Visiting Consultant** = no Break Glass.
Unique constraint: `uq_user_roles_global WHERE scope_department_id IS NULL AND valid_to IS NULL`.

## User status flow

`active → suspend → reactivate → active`
`active/suspended → terminate → reactivate`

- No hard delete anywhere
- Terminated users hidden from list by default, shown only with "Terminated" toggle
- `OrganizationId` on Clinic is `Guid?` nullable (clinic doesn't require parent org)

---

## Patient module architecture

- **UHID** = clinic-specific, customisable format (prefix + optional middle segment + number)
- Max 20 chars, **Luhn check digit default ON** (SysAdmin can disable)
- Minimum digits auto-calculated from projected volume (2.5M / 10yr)
- **Provisional UHID** = internal only; patient sees APT reference
- **Check-in always requires physical verification**
- All patient data in clinic DB only
- **Inter-clinic sharing** only via explicit patient consent + encrypted FHIR R4 bundle
- **MRN** = nullable
- **UHID format** = `PT-YYYY-XXXXXX`
- Patient flags customisable per clinic via `flag_definitions`

## Patient safety

- **Duplicate detection** = Soundex + DOB confidence cascade (Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile)
  - Side-by-side UI
  - Reception can override with reason
- **Record merge** = SiteAdmin minimum, cooling-off:
  - SiteAdmin: 24h
  - SysAdmin: 7d
  - GlobalAdmin: 30d
  - locked: 30d+
- **Luhn check digit** on UHID + APT references (transparent to users)
- **DOB confidence tags**: Verified / Self-reported / Estimated / Unknown
- Verified DOB locked, only SiteAdmin can override

## Aspirational Districts

- `master.aspirational_districts` = authority list (112 NITI Aayog districts)
- Admin page: `/admin/aspirational-districts` (GlobalAdmin / SysAdmin)
- PatientNew loads active districts from master DB on init into HashSet (non-fatal if fails)
- `core.addresses.is_aspirational` = denormalised boolean set at save time
- Geodata seed: 36 states, 708 districts, 131 aspirational, 1167 cities
- File: `02_geodata_seed.sql` with deterministic UUIDs

## PatientNew (Session 19 design LOCKED)

- `reg-*` CSS classes for all form fields
- `reg-split` for compound fields (border on wrapper, children border:none height:100%)
- Single `admin.css`, no appending
- Address field order: **Pincode → City → District → State**
- Referral source values match DB constraint: `doctor / self / emergency / camp / transfer / online / other`

## Appointment module

- **Templates required**
- **Scheduling Coordinator** = standalone role, multiple per clinic, SiteAdmin assigns
- Slot duration per doctor
- Overbooking default ON, per-doctor flag
- Recurring max 99; indefinite = rolling 8wk
- Waitlist = manual staff confirm
- Queue: FIFO / Appt-first / Blended (configurable)
- Calling board = public + staff
- Cancel window default 2h, configurable
- **Teleconsult parked**

---

## Clinic DB Schema v3 (LOCKED)

- `core.patients` = collapsed persons + patients
- Immutable / append-only
- `id` = version UUID
- `entity_id` = stable patient UUID
- `previous_id` = prior version
- `valid_to IS NULL` = current
- Address plain text (no UUID FKs)
- `is_aspirational` boolean denormalised
- Staff / Facility / MedicalCodes NOT in `ClinicCoreDbContext` — future modules only
- Files: `01_core_v3.sql`, `02_geodata_seed.sql`, `02_identity.sql`

## EF Core rules

1. **Nothing added to `ClinicCoreDbContext`** unless current sprint needs it. EF validates ALL entities at startup.
2. **Shadow FK bug**: nav property name + PK name = generated column name (e.g. `DeathCauseCode` + `Code` = `DeathCauseCodeCode`). Always explicitly configure non-conventional FKs.
3. `Patient.DisplayName` is `GENERATED STORED` — `ValueGeneratedOnAddOrUpdate()`, never write it.
4. `ClinicDbFactory` returns null (not IdentityConnection fallback) when clinic DB is missing.

---

## Date / time format

- DB stores ISO 8601 always
- Display format configurable per clinic (`master.clinics.date_format`)
- Default: `DD/MM/YYYY` for India
- Format library: `D/DD`, `M/MM/MMM/MMMM`, `YY/YYYY` with `/`-`. ` space separators
- `IDateFormatService` for parse/format
- `LipiDatePicker` reads clinic format via DI with per-component `Format` override
- DateSegment supports numeric formats only (word-month formats are display-only, calendar click required for input)

---

## Layout architecture (LOCKED)

- **TopNav + BottomNav shell** — validated on clinical-ergonomic grounds
- 50/50 desktop + tablet split
- Horizontal screen real estate is sacred (THX / Z-pattern reading)
- Navigation goes bottom not left (rare-click clinical context, mostly one-desk-one-role)
- Circular module buttons retained
- **No clinic switcher in chrome** — multi-clinic users logout + login (clinical safety)
- **Universal search** confirmed needed (Ctrl+K, "Search Well" pattern)

Full layout architecture spec in `06-LAYOUT-ARCHITECTURE.md`.

---

## Release policy (LOCKED)

**Nothing ships to client until all modules ready.** No half-shipping. No partial release. The hospital receives the complete LiPi suite when complete.

- Internal testing / beta with Armoki is internal, not a release
- Public / commercial release = complete system ready
- Calm pace, no half-finished modules in client hands

---

## Module phase sequencing (LOCKED for cancer-hospital priorities)

Unusual order from HIS-software convention — sequenced by clinical-revenue priority and user domain expertise (RT + Rad before Pharmacy/Lab because user has domain depth in RT/Rad):

| Phase | Modules |
|---|---|
| Phase 4 | OPD + Billing (Insurance joins or defers) |
| Phase 5 | Radiotherapy + Medical Physics + Radiology |
| Phase 6 | Compliance |
| Phase 7 | Orders + Pharmacy + Lab |
| Phase 8 | IPD |
| Phase 9+ | Specialty modules (Chemo, MO, Sx, OT, CSSD, Cath, Dent, Dialysis) |
| Phase 10+ | Operational modules (Purchase, Asset, Tickets, PACS) |

---

## Known HIPAA gaps

- `phi_access_log` and `audit_events` exist in schema but code does not yet write to them
- To be closed in Phase 2.10 Infrastructure Audit
