# LiPi HIS — Project Baseline (v1.0)

> **Last Updated**: May 2, 2026 (Decision #12 added)  
> **Status**: LOCKED — All 12 design decisions finalized  
> **Next Changes**: v1.1+ decisions isolated in CHANGE-LOG.md

---

## PROJECT OVERVIEW

**LiPi HIS** is a comprehensive Hospital Information System modelled after Varian ARIA, built for Indian cancer hospitals. **LiPi** is a SaaS HIS product by **imagiQa**. **Armoki** is the first expected enterprise client and serves as the design reference for v1.0 features.

- **Product**: LiPi HIS
- **Brand Owner**: imagiQa
- **First Expected Client**: Armoki (cancer hospital)
- **Stack**: .NET 10, Blazor Web App (InteractiveServer only), PostgreSQL + EF Core 10 + Npgsql
- **Auth**: Cookie-based, Argon2id hashing
- **Design**: Apple HIG density, Navy (#0B2545) + Gold (#C49A22) brand, multi-theme (light/dark)
- **Compliance**: HIPAA + Six Sigma quality standards
- **Developer Env**: Visual Studio (Windows), pgAdmin 4, PowerShell

> **Important distinction**: The v1.0 baseline represents the **core LiPi product** (owned by imagiQa), not Armoki-specific customizations. Future clients may have client-specific configurations, but the locked decisions in this document apply to the LiPi product itself.

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

## 🔒 12 LOCKED DESIGN DECISIONS (v1.0)

| # | Decision | Answer | Details |
|---|----------|--------|---------|
| 1 | CSS Architecture | **B) 00-baseline.css + per-module CSS + theme files** | Modular, theme-aware |
| 2 | CLAUDE.md Handling | **Extract → 00-PROJECT-BASELINE.md + Deprecate** | Specs as source of truth |
| 3 | Database Docs | **Hybrid: Keep SQL/ER in database/, specs in docs/00-DATABASE/** | Separation: technical vs spec |
| 4 | Module Scope | **All 25 modules** | Complete roadmap |
| 5 | DOB Confidence Tagging | **SysAdmin + SiteAdmin can override Verified DOB** | Mark "Overridden by [User] on [Date]" |
| 6 | Duplicate Detection | **Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile** | Confidence cascade |
| 7 | Record Merge Cooling | **24h / 7d / 30d / 30d+** | By role level |
| 8 | Scheduling Coordinator | **Standalone role, SiteAdmin-assigned, multiple per clinic** | New role |
| 9 | Teleconsult Feature | **PARKED → v1.1** | Revisit after 10+ modules |
| 10 | Public Calling Board | **BASE FEATURE + Per-module customization** | Multi-screen support |
| 11 | Waitlist Confirmation | **Manual confirmation** | Staff clicks "CONFIRM ARRIVAL" |
| 12 | **Component Library + Multi-Theme** ⭐ NEW | **Custom Razor components + 2 themes** | See Decision #12 detail below |

---

## 🎨 DECISION #12 — COMPONENT LIBRARY + MULTI-THEME (LOCKED v1.0)

### Sub-Decision 12.1: CSS Approach
**Custom CSS** (NOT MudBlazor or any 3rd-party library)

**Rationale**:
- MudBlazor's Material Design doesn't fit medical/clinical UI density needs
- MudBlazor has known compatibility issues with InteractiveServer + authentication (as of late 2025)
- Existing `reg-*`, `uf-*`, `cf-*` CSS systems represent significant investment
- Custom CSS gives total control for multi-theme support
- 5-10 year medical product lifespan benefits from owning the design system

### Sub-Decision 12.2: Design Density
**Apple HIG (Human Interface Guidelines) style** — balanced density

**Specifications**:
- Form field height: 32px (admin forms), 34px (PatientNew `reg-*` system)
- Label font: 11px uppercase, letter-spacing 0.3-0.5px
- Section headers: 14-15px, weight 500
- Border: 0.5-1px solid `#E2E8F0` to `#CBD5E1`
- Border radius: 6-8px standard, 10-12px for cards
- Generous-but-not-wasteful spacing

### Sub-Decision 12.3: Component Library
**Build custom Razor component library** with `Lipi` prefix.

**Examples**: `LipiButton`, `LipiTextBox`, `LipiCombo`, `LipiCard`, `LipiTable`, `LipiModal`, `LipiDatePicker`

**Location**: `src/LiPi.Web/Components/Lipi/`

**Scope for v1.0**: All 41 components (P0: 15 critical, P1: 15 important, P2: 11 specialized)

**See**: `docs/00-COMPONENTS/00.1-COMPONENT-INVENTORY.md` for full list

### Sub-Decision 12.4: Multi-Theme System

**Two-dimensional theme model**:
1. **Brand Theme** (set by clinic admin): Determines primary/accent colors, logos
2. **User Mode** (set by individual user): Light / Dark

**Combined theme**: `[brand]-[mode]` (e.g., `lipi-default-light`, `lipi-default-dark`)

**v1.0 ships with**:
- 1 brand theme: `lipi-default` (Navy + Gold)
- 2 modes: `light` (default) + `dark`

**v1.1+ may add**:
- `armoki` brand theme (when Armoki finalizes brand)
- Additional client themes
- Auto-mode (follows OS)
- High-contrast mode

**Default for new users**: **Light mode**

**Implementation**:
- CSS custom properties (variables) keyed by `data-brand` + `data-mode` attributes on `<body>`
- Theme switching is instant (no page reload)
- Per-user preference stored in `clinic.identity.user_preferences.theme_mode`
- Per-clinic brand stored in `master.clinics.brand_theme_id`

**See**: `docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md` for full architecture

### Sub-Decision 12.5: Style Guide Page
**Build `/admin/style-guide` page** as living component showcase.

Renders every component with:
- Visual examples (all variants)
- Code snippets
- Light/Dark mode toggle
- Brand switcher (when more brands added)

Becomes:
- Developer onboarding tool
- CSS refactoring testbed
- Demo material for prospects (e.g., Armoki)

---

## DESIGN SYSTEM

### Colors (Brand: lipi-default)

**Light Mode**:
- Primary: Navy `#0B2545`
- Accent: Gold `#C49A22`
- Cobalt (info): `#1565C0`
- Background: `#F4F7FB`
- Surface (cards): `#FFFFFF`
- Text primary: `#1a1a1a`
- Text secondary: `#5F5E5A`

**Dark Mode**:
- Primary: `#4A9BD4` (lighter navy for contrast)
- Accent: `#E5B847` (brighter gold)
- Background: `#0d1117`
- Surface (cards): `#161b22`
- Text primary: `#e6edf3`
- Text secondary: `#8b949e`

**Status colors** (both modes):
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

### Type Scale (CSS Variables — theme-agnostic)
- `--ts-page-title`: 18px / 600 (page H1)
- `--ts-section`: 15px / 600 (card titles)
- `--ts-body`: 13px / 400 (general text)
- `--ts-label`: 12px / 500 (form labels)
- `--ts-small`: 11px / 400 (helper text)
- `--ts-th`: 10px / 600 (table headers — UPPERCASE)
- `--ts-badge`: 10px / 500 (badges, pills)
- `--ts-micro`: 9px / 400 (captions, footnotes)
- `--ts-mono-data`: 12px / 400 (DM Mono: IDs, dates)

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

### Patient Identifiers
- Aadhaar (12 digits), ABHA (optional), Passport, Driver's License (optional)

### Patient Addresses
- Current + Permanent (separate table rows)
- Immutable versioning: `valid_to IS NULL` = current
- `is_aspirational` denormalized
- Plain text city/district/state (no FK to geodata)

---

## APPOINTMENT MODULE (60% COMPLETE)

### Core Rules
- Template-required, per-doctor slot duration, overbooking default ON
- Recurring: max 99 or rolling 8-week
- Manual waitlist confirmation
- Queue modes: FIFO / Appointment-first / Blended

### Public Calling Board (BASE + Customization)
- Core service, multi-screen, per-module custom fields
- Configurable display, font, colors, rotation
- Timezone-aware

---

## SECURITY & AUDIT (PRODUCTION-GRADE)

- Hash-chained audit log (cryptographically linked, tamper-detectable)
- Blockchain anchoring (Merkle roots, external verification)
- PHI access log (separate, minimum-necessary tracking)
- Standard `AuditActions` constants (never raw strings)
- Always via `IAuditService` (injectable)

---

## DEPLOYMENT & VERSIONING

### CSS Strategy (Decision #12 LOCKED)
```
wwwroot/
├── css/
│   ├── 00-baseline.css       (structure, variables, NO colors)
│   ├── 01-user.css           (per-module styles)
│   ├── 02-clinic.css
│   ├── ...25-pacs.css
│   └── components/           (shared component CSS)
└── themes/
    ├── brand-lipi.css         (LiPi brand colors)
    ├── mode-light.css         (light mode tokens)
    └── mode-dark.css          (dark mode tokens)
```

### Component Library Location
```
src/LiPi.Web/Components/Lipi/
├── LipiButton.razor
├── LipiTextBox.razor
├── LipiCombo.razor
├── ...41 components total
└── Theme/
    └── ThemeProvider.razor
```

### Module Build Order
1. ✅ User Registration
2. ✅ Clinic Registration
3. ✅ Organization Registration
4. ✅ Patient Registration (80%)
5. ✅ Appointments (60%)
6. **Component Library + Multi-Theme** (NEXT — Decision #12)
7. Audit Log Infrastructure
8-25. Future modules

### Version Management
- **v1.0**: All 12 decisions locked in this file
- **v1.1+**: Changes isolated in CHANGE-LOG.md
- Never edit this file after launch

---

## EXTERNAL INTEGRATIONS (STATUS)

| API | Status | Notes |
|---|---|---|
| NMC India (doctor verify) | Manual | nmr.nmc.org.in |
| AERB (radiation licence) | Manual | elora.aerb.gov.in |
| India Post PIN | Planned | For patient address auto-fill |
| GSTIN verification | Planned | For clinic registration |
| ABDM/ABHA | Planned | Phase 2 |
| DigiLocker | Planned | Phase 2 |

---

## SUPPORT & REFERENCES

- **Schema Docs**: `docs/00-DATABASE/00.2-Clinic-Core-Schema.md`
- **Component Library**: `docs/00-COMPONENTS/00.0-MASTER-PLAN.md`
- **Component Inventory**: `docs/00-COMPONENTS/00.1-COMPONENT-INVENTORY.md`
- **Theming Architecture**: `docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md`
- **Module Specs**: `docs/[NN]-MODULE-NAME/[NN].1-Design-Specs.md`
- **System Prompt**: `system-prompt.md`
- **Test Automation**: `test-automation-guide.md`
- **CSS Refactoring**: `css-refactoring-roadmap.md`

---

**This file is LOCKED. No changes except v1.1+ in CHANGE-LOG.md.**

Last Updated: **May 2, 2026** by Arun Shiva (Single Developer, LiPi HIS Project)
