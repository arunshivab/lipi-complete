# LiPi HIS — Project Baseline (v1.0)

> **Last Updated**: May 2, 2026  
> **Status**: LOCKED — All 11 design decisions finalized  
> **Next Changes**: v1.1+ decisions isolated in CHANGE-LOG.md

---

## PROJECT OVERVIEW

**LiPi HIS** is a comprehensive Hospital Information System modelled after Varian ARIA, built for Indian cancer hospitals. Brand: **LiPi** by Armoki.

- **Stack**: .NET 10, Blazor Web App (InteractiveServer only), PostgreSQL + EF Core 10 + Npgsql
- **Auth**: Cookie-based, Argon2id hashing
- **Design**: Navy (#0B2545) + Gold (#C49A22), white cards on soft blue-gray background
- **Compliance**: HIPAA + Six Sigma quality standards
- **Developer Env**: Visual Studio (Windows), pgAdmin 4, PowerShell

---

## ARCHITECTURE (LOCKED)

### Multi-Tenancy
- **Completely separate PostgreSQL database per clinic** — zero shared tables between clinics
- Master DB (`lipi_master`) tracks organizations, clinics, and platform users only
- Each clinic DB has isolated identity schema, clinical data, and audit logs
- Connection strings: `IdentityConnection` (clinic), `MasterConnection` (master)

### Admin Hierarchy (Undeletable)
| Level | Default Username | Access | Can Be Deleted? |
|---|---|---|---|
| Global Admin | `Admin` | All databases, all clinics — unrestricted | ❌ NEVER |
| Sys Admin | `SysAdmin` | All databases by default; Global Admin can restrict | ❌ NEVER |
| Site Admin | `SiteAdmin` | Assigned clinic(s) only | ❌ NEVER |

All three seeded with `must_change_password = true`. Only GlobalAdmin can promote to SysAdmin.

### Database Schema (LOCKED v3.0)
- Core schema: `01_core_v3.sql` (collapsed persons + patients, immutable/append-only)
- Geodata: `02_geodata_seed.sql` (36 states, 708 districts, 131 aspirational, 1,167 cities)
- Identity: `02_identity.sql` (users, roles, sessions, MFA, audit)
- ABDM/ABHA: `03_abdm.sql`
- Audit: `04_audit.sql` (hash-chained, blockchain anchored)
- Security: `05_security.sql`
- Compliance: `06_compliance.sql`
- Certs: `07_certs.sql`
- Six Sigma: `08_sigma.sql`

All patient tables: **immutable**. No UPDATE/DELETE. New INSERT on every change with `previous_id` FK, `valid_to` timestamp.

---

## COMPLIANCE REQUIREMENTS (NON-NEGOTIABLE)

### HIPAA
- ✅ No PHI in logs — use IDs only
- ✅ Audit log for every read/write of PHI (who, when, what, where)
- ✅ MFA enforced for clinical staff accessing PHI
- ✅ Session: 30 min timeout, non-sliding, HttpOnly + SameSite=Strict cookies
- ✅ Password: 8+ chars, upper+lower+digit+symbol, 90-day expiry
- ✅ Account lockout: 5 failures → 30 min lock
- ✅ PHI field-level encryption (AES-256-GCM)
- ✅ Break-glass access with mandatory reason codes + audit
- ✅ Data retention: audit logs 6 years, patient records 6 years from last encounter
- ✅ HTTPS enforced in production

### Six Sigma
- ✅ Dashboard includes quality metrics
- ✅ Track defect rates per process
- ✅ DPMO visible to Directors+
- ✅ Control charts, run charts, Cpk on clinical workflows
- ✅ Sigma level on module dashboards

---

## UX RULES (MANDATORY EVERY PAGE)

### Confirmation Dialogs
**EVERY destructive action MUST show confirmation BEFORE executing.**
- Includes: Delete, Lock, Suspend, Reset Password, Reactivate, status changes, role removal
- Dialog names the affected record
- Describes exactly what will happen
- Cancel = default / prominent button
- Destructive button = red
- Minimum 2 steps (never single-click)

### Edit Interactions
- **Inline panels** for: Contact edit, Manage roles
- **Modal overlays** ONLY for: Confirmation dialogs
- Inline panel slides open inside table row, pushes rows below down

### Form Rules
- ✅ Validate on `oninput` (after first blur) AND `onblur`
- ✅ After save: wrap entire form in `<fieldset disabled="@saved">`, Save button → disabled "✓ Saved", Cancel → "Close"
- ✅ Success/error messages near Save button (NOT top of page)
- ✅ Validation errors auto-scroll, focus first errored field
- ✅ Email + Mobile mandatory in ALL Clinic/Org/User forms

### Accessibility (MANDATORY)
Every input/select/textarea needs:
- `id="prefix-field"` (e.g., `un-email` for UsersNew)
- `name="camelCase"`
- `autocomplete="valid-value"`
- Label with `for="fieldId"`

**Prefixes**: `un-` (UsersNew), `ue-` (UsersEdit), `cn-` (ClinicsNew), `ce-` (ClinicsEdit), `on-` (OrgsNew), `oe-` (OrgsEdit), `pn-` (PatientNew), `ps-` (PatientSearch)

---

## ROLE SYSTEM (LOCKED)

### 15 Primary Role Groups (Mutually Exclusive)
`clinician`, `nursing`, `physicist`, `dosimetrist`, `rtt`, `rad_tech`, `lab_tech`, `nm_tech`, `ot_tech`, `cssd_tech`, `billing`, `pharmacy`, `dietician`, `physiotherapist`, `counsellor`

### Add-ons (Standalone OR with Primary Role)
- `operations` (facility ops, scheduling, logistics)
- `rso` (Radiation Safety Officer)
- `visiting_consultant` (no Break Glass access)

### Constraints
- Unique constraint: `uq_user_roles_global WHERE scope_department_id IS NULL AND valid_to IS NULL`
- No user can have 2 primary roles
- Operations/RSO are add-ons, not primary

---

## 🔒 11 LOCKED DESIGN DECISIONS (v1.0)

| # | Decision | Answer | Details |
|---|----------|--------|---------|
| 1 | CSS Architecture | **B) Refactor to 00-baseline.css + per-module CSS** | Modular, extractable, scales to 25 modules |
| 2 | CLAUDE.md Handling | **Extract → 00-PROJECT-BASELINE.md + Deprecate** | Clean break, specs as source of truth |
| 3 | Database Docs | **Hybrid: Keep SQL/ER in database/, specs in docs/00-DATABASE/** | Separation: technical vs spec-focused |
| 4 | Module Scope | **All 25 modules** | Complete roadmap, even if some empty templates |
| 5 | DOB Confidence Tagging | **SysAdmin + SiteAdmin can override Verified DOB** | Mark overrides: "Overridden by [User] on [Date]" |
| 6 | Duplicate Detection | **Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile** | Order correct, confidence cascade locked |
| 7 | Record Merge Cooling | **24h (basic) / 7d (staff) / 30d (SiteAdmin) / 30d+ (SysAdmin)** | Cooling-off periods by role level |
| 8 | Scheduling Coordinator | **Standalone role, SiteAdmin-assigned, multiple per clinic** | New role for appointment coordination |
| 9 | Teleconsult Feature | **PARKED** | Revisit after 10+ modules, likely v1.1 |
| 10 | Public Calling Board | **BASE FEATURE + Per-module customization** | Core service, custom fields per module, multi-screen support |
| 11 | Waitlist Confirmation | **Manual confirmation** | Staff clicks "CONFIRM ARRIVAL" to move patient to in-progress |

---

## DESIGN SYSTEM

### Colors
- **Primary**: Navy `#0B2545`, Gold `#C49A22`
- **Background**: Soft blue-gray
- **Cards**: White with colored left borders (4px)
- **Status strips** (left border on table rows):
  - Active: `#4CAF50` green
  - Suspended: `#FF9800` orange
  - Locked: `#F44336` red
  - Invited: `#2196F3` blue
  - Terminated: `#9E9E9E` grey

### Typography (TWO FONTS ONLY)
| Font | Usage |
|------|-------|
| **DM Sans** | ALL UI — headings, labels, buttons, nav, badges, forms |
| **DM Mono** | ONLY: IDs, timestamps, dates, codes, clinical data |

### Type Scale (CSS Variables)
- `--ts-page-title`: 18px / 600 (page H1)
- `--ts-section`: 15px / 600 (card titles)
- `--ts-body`: 13px / 400 (general text)
- `--ts-label`: 12px / 500 (form labels)
- `--ts-small`: 11px / 400 (helper text)
- `--ts-th`: 10px / 600 (table headers — UPPERCASE)
- `--ts-badge`: 10px / 500 (badges, pills)
- `--ts-micro`: 9px / 400 (captions, footnotes)
- `--ts-mono-data`: 12px / 400 (DM Mono: IDs, dates)

### Module Dock Colors (3 groups)
**Clinical**: OP (#4A9BD4), IP (#3b82f6), RO (#a855f7), MO (#f97316), Sx (#ef4444), CV (#0ea5e9), OT (#f43f5e)  
**Diagnostics**: Ph (#22c55e), Lab (#eab308), Rad (#67e8f9), NM (#f59e0b), Dia (#06b6d4), Den (#10b981)  
**Services**: Nu (#ec4899), CS (#6366f1), Ad (gold)

---

## CRITICAL BLAZOR RULES (PREVENT 80% OF BUILD ERRORS)

### ⛔ RULE 1: NEVER string literals inside double-quoted Razor attributes
```razor
❌ @onchange="e => m.X = e.Value ?? "DEFAULT""
✅ @onchange="OnXChange"  // Extract to named method
   private void OnXChange(ChangeEventArgs e) => m.X = e.Value ?? "DEFAULT";
```

### ⛔ RULE 2: EVERY interactive page needs @rendermode InteractiveServer
```razor
@layout LiPi.Web.Components.Layouts.TopNavLayout
@attribute [Authorize]
@rendermode InteractiveServer  ← MANDATORY if page has @onclick/@oninput/@onchange/@bind
```

### ⛔ RULE 3: Never use "" inside @bind or event attributes
```razor
❌ @bind-Value="@(value ?? "")"
✅ @bind-Value="@(value ?? string.Empty)"

❌ class="@(hasError ? "err" : "")"
✅ class="@(hasError ? "err" : string.Empty)"
```

### ⛔ RULE 4: Never mix @bind + @bind:event + @onchange on same element

### ⛔ RULE 5: AddServerSideBlazor() is REMOVED
.NET 7 legacy API. Use `AddRazorComponents().AddInteractiveServerComponents()` instead.

### ✅ RULE 6: CascadingAuthenticationState stays in App.razor
Required for SSR prerender + interactive circuits.

### ✅ RULE 7: All services injected need @using directives
```razor
@using Microsoft.JSInterop
@inject IJSRuntime JS

@using Microsoft.AspNetCore.Components
@inject NavigationManager Nav
```

---

## CRITICAL BUG FIXES (DO NOT REVERT)

| Issue | Fix |
|---|---|
| ExtensionData column not found | Changed from `Dictionary<string,object>` to `string`; explicit `.HasColumnName("extension_data").HasColumnType("jsonb")` in IdentityDbContext |
| Blazor events dead | Removed `AddServerSideBlazor()` — conflicts with .NET 10's InteractiveServer |
| Auth state crash on prerender | `CascadingAuthenticationState` in App.razor + `AddCascadingAuthenticationState()` in Program.cs |
| Dictionary jsonb error | Npgsql 8+ requires opt-in; use `string` instead |
| Escaped quotes in lambdas | Never use `\"` in Razor attributes; use named methods |

---

## PATIENT MODULE (80% COMPLETE)

### UHID System (LOCKED)
- Format: `PT-YYYY-XXXXXX` (clinic prefix + middle + number + optional Luhn check digit)
- Customizable per clinic (prefix, middle, number range, Luhn default ON)
- Provisional UHID: for internal use only, not patient-facing
- Max 20 chars total
- Luhn check digit: SysAdmin can disable

### Duplicate Detection (CASCADE)
Confidence order: **Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile**
- Side-by-side UI shows potential duplicates
- Reception can override with reason
- Marked in audit trail

### Patient Identifiers
- Aadhaar (12 digits)
- ABHA (optional, for ABDM)
- Passport (optional)
- Driver's License (optional)

### Patient Addresses
- Current + Permanent (separate table rows)
- Immutable versioning: `valid_to IS NULL` = current
- `is_aspirational` denormalized from master.aspirational_districts
- Plain text city/district/state (no FK to geodata)

### Contact Points
- Multiple phones per patient (home, mobile, work)
- Multiple emails per patient
- Immutable versioning

### Consents
- Append-only (treatment, research, communication)
- Dated, signed, version-tracked
- Used in HIPAA break-glass checks

---

## APPOINTMENT MODULE (60% COMPLETE)

### Core Rules
- **Template-required**: Every appointment must use a template
- **Per-doctor slot duration**: Configurable (15/30/45/60 min)
- **Overbooking**: Default ON, configurable per clinic
- **Recurring**: Max 99 or rolling 8-week indefinite
- **Manual waitlist**: Confirmation required (patient calls to confirm)
- **Queue modes**: FIFO / Appointment-first / Blended (configurable)

### Public Calling Board (BASE FEATURE + Customization)
- Core service (Appointment module)
- Multi-screen support per location
- Per-module custom fields (OPD: doctor+room, Radiology: modality+tech, Lab: test+priority)
- Configurable display mode, font size, colors, rotation speed
- Timezone-aware

### Waitlist Confirmation (Manual)
- Staff clicks "CONFIRM ARRIVAL"
- Patient moves from "WAITING" to "IN PROGRESS"
- Prevents no-shows blocking queue

### Teleconsult (PARKED)
- Not in v1.0
- Revisit after 10+ modules
- Likely v1.1 feature

---

## SECURITY & AUDIT (PRODUCTION-GRADE, DO NOT DUPLICATE)

### Hash-Chained Audit Log
- Every `AuditEvent` has `PreviousHash` + `CurrentHash`
- Cryptographically linked records
- Tampering breaks chain instantly (detectable)

### Blockchain Anchoring
- Merkle roots anchored to external ledger
- Independent tamper-proof verification

### PHI Access Log (Separate)
- Every read of patient data logged independently
- Minimum-necessary-principle tracking
- Consent references

### Standard Action Codes
- Use `AuditActions` static constants
- Never raw strings
- Always via `IAuditService` (injectable)

---

## DEPLOYMENT & VERSIONING

### CSS Strategy (LOCKED: Option B)
- `00-baseline.css` (extracted common, reusable)
- Per-module CSS files: `01-user.css`, `02-clinic.css`, ... `25-pacs.css`
- Extract existing `admin.css` into baseline + module files

### Module Build Order
1. ✅ User Registration
2. ✅ Clinic Registration
3. ✅ Organization Registration
4. ✅ Patient Registration (80%)
5. ✅ Appointments (60%)
6. Audit Log Infrastructure
7-25. (Future modules follow template)

### Version Management
- **v1.0**: All 11 decisions locked in this file
- **v1.1+**: Changes isolated in CHANGE-LOG.md
- Never edit this file after launch
- All v1.0 decisions are BASE

---

## EXTERNAL INTEGRATIONS (STATUS)

| API | Status | Notes |
|---|---|---|
| NMC India (doctor verify) | Manual | elora.aerb.gov.in |
| AERB (radiation licence) | Manual | elora.aerb.gov.in |
| India Post PIN | Planned | For patient address auto-fill |
| GSTIN verification | Planned | For clinic registration |
| ABDM/ABHA | Planned | Phase 2 |
| DigiLocker | Planned | Phase 2 |

---

## SUPPORT & REFERENCES

- **Schema Docs**: `docs/00-DATABASE/00.2-Clinic-Core-Schema.md`
- **Module Specs**: `docs/[NN]-MODULE-NAME/[NN].1-Design-Specs.md`
- **System Prompt**: `system-prompt.md`
- **Test Automation**: `test-automation-guide.md`
- **CSS Refactoring**: `css-refactoring-roadmap.md`

---

**This file is LOCKED. No changes except v1.1+ in CHANGE-LOG.md.**

Last Updated: **May 2, 2026** by Arun Shiva (Single Developer, LiPi HIS Project)
