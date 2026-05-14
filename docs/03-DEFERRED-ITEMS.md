# LiPi HIS — Deferred Items

Every parked decision, deferred infrastructure phase, and "discuss in Phase 2.10" item.
This is the master list for Phase 2.10 Infrastructure Audit & Decisions.

---

## Phase 2.10 — Infrastructure Audit & Decisions

Discuss each of these in dedicated design sessions. All items added without explicit
discussion get resolved here before they ship to production.

### NuGet packages to review

| Package | Purpose | Status |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT auth for API consumers | Not discussed — keep or remove? |
| `MailKit` | SMTP email (OTP, password reset) | Not discussed — discuss email mechanism (email vs SMS vs both) |

### Services to discuss

- `SmtpEmailService` + `IEmailService` — OTP delivery mechanism never discussed at architecture level
- `AadhaarXmlService` — implied by Aadhaar offline XML requirement, confirm scope

### CI/CD & test automation

- `database-deploy.yml` — was added without discussion (now cleaned: gated to manual trigger, Slack removed)
- `spec-validation.yml` — referenced in docs but doesn't exist yet
- `test-automation-guide.md` — speculative Selenium + xUnit + Npgsql framework. `test-runner/` project doesn't exist. Decide: build it, or remove the doc?

### Audit schema features

- Hash-chained audit log — implied by HIPAA, confirm scope
- **Blockchain anchoring** — Merkle roots on external ledger — likely overengineering, propose to remove
- PHI access log + Export / Print logs — required by HIPAA, keep, **must wire writes to it in this phase**

### Hosting infrastructure

- AWS RDS references in `PROVISIONING.md` — hosting was never decided
- Production hosting plan: AWS? Azure? Self-hosted? On-prem for hospitals? Decide.

### Future infrastructure phases (queued)

- **LipiMail service** — full email infrastructure (queuing, templates, multi-tenant SMTP, audit). Replaces thin `SmtpEmailService` when first structured-email module ships (Lab / Radiology reports)
- **LipiHttp + LipiApi** — wrapper over `HttpClient` for ABDM, DigiLocker, SMS, payments. Build when first external integration arrives
- **i18n / multi-language** — LiPi Sans covers 10 Indian scripts. Interface translation never designed. Hindi first.
- **Full custom LiPi typeface** — currently LiPi Sans = Inter + Noto. Full custom typeface after Armoki pilot.
- **Unit test infrastructure** — framework choice (bUnit vs plain unit), assertion library, CI integration. Test cases for shipped components / attributes accumulating in spec docs. First tracked: `MustBeTrueAttribute` (3 cases).
- **Draft + Tab pattern (interruption recovery subsystem)** — ships with Phase 4 patient registration as reference implementation. See dedicated section below.

### Deferred chrome features (status indicators)

- **Autosave / Draft indicator** — clinical notes module (Phase 5/6). Industry pattern: silent autosave of drafts with explicit "Sign to commit" required. Indicator shows save state on long-form note pages. Drop from v1.0 chrome until clinical notes module designs the draft pattern.
- **Print queue indicator** — Prescription / Pharmacy module phase. Three industry patterns (OS mirror / batch print queue / pending-confirmation queue) — pick when implementing. Drop from v1.0 chrome until print queue subsystem ships.

### Draft + Tab pattern (interruption recovery subsystem) — Phase 4 deliverable

**Purpose:** Handle real-world workflow interruptions (patient walks away mid-registration, doctor called to emergency mid-prescription, oncologist drafts chemo order before review).

**Hybrid architecture: LipiDynamicTabs (in-memory speed) + DB autosave (persistence).**

**How it works:**
1. User opens an interruptible form (e.g., "+ New patient") → new dynamic tab opens
2. As fields fill → tab is visible with dirty marker (*) + DB autosaves draft silently
3. User can open additional tabs in parallel (register another patient, etc.)
4. Disaster recovery: power outage / crash / logout → drafts persisted in DB
5. User logs back in → system detects unfinished drafts owned by them → **automatically reopens them as tabs**
6. On final save → draft converts to real record, draft row deleted

**Architecture decisions (LOCKED):**

| Decision | Value |
|---|---|
| Visibility default | **Role-shared** (any user with same role can see/resume drafts) |
| Visibility override | **Configurable per clinic** by SiteAdmin (can switch to owner-only) |
| Expiry default | **24 hours** |
| Expiry override | **Configurable per clinic** by SiteAdmin |
| Auto-restore | **Yes** — drafts owned by user auto-open as tabs on next login |
| Storage | DB-backed draft table in clinic DB |
| Autosave trigger | Debounced on field blur, plus periodic interval (e.g., every 10-15s while form has focus) |

**Modules that will use this pattern:**
- Patient registration (Phase 4 — **reference implementation**)
- Prescription writing (Phase 4 Layer 1 / Phase 5 Layer 2)
- Chemo order (Phase 9)
- Discharge summary (Phase 8)
- Lab order set (Phase 7)
- Op note (Phase 9)
- Admission notes (Phase 8)
- Any long-form clinical note (Phase 5/6 clinical notes module)

**Implementation order:**
- Build subsystem with Phase 4 patient registration
- Patient registration is the reference implementation
- Future modules plug into the same service

**Open questions for Phase 4 build:**
- Draft table schema (one table per form type, or single polymorphic table?)
- Conflict resolution if two users edit same draft simultaneously
- Audit trail format
- "Expiring soon" notice timing (e.g., final 24h warning before expiry)
- Settings UI for SiteAdmin to configure visibility + expiry per role

### Component implementation rules (must apply to every popup / dropdown / overlay)

- **Ghost click prevention** — every popup, dropdown, dropdown menu, category popup, search overlay, autocomplete dropdown, and context menu MUST handle:
  - Click outside closes (handled at host level, not per popup)
  - Mousedown on backdrop captured to prevent click-through to underlying elements
  - Touch tap handled separately from click (tablet) — use pointerdown not click for primary interaction
  - During popup-open: underlying buttons get `pointer-events: none` until popup closes + 50ms guard
  - Escape key closes popup before any other key handler fires
  - Tab order trapped within popup when open
  - Re-render race: if popup is closing while user clicks a new control, ignore the late click event
  - Popup open animation must NOT delay backdrop event handler attachment — backdrop is interactive instantly
- This rule applies to LipiCategoryPopup, LipiSearchSpotlight, LipiDropdown, LipiContextMenu, LipiAutocomplete, and any future component that overlays content. Add to pre-delivery checklist as item #11.

---

## Phase 3.0 — Brand identity

- **Custom LiPi Icons** — in development separately, replaces Lucide
- **Custom LiPi Spinner** — replaces generic App.razor spinner

---

## Component-specific deferred items

### LipiFormContext (Tier 3 of OnEditContextReset)

A wrapper around `EditContext` exposing an explicit `OnReset` event for clean
programmatic form reset.

- **Tier 1 + 2 shipped** in `LipiInputBase`:
  - **Tier 1**: EditContext swap resets `_isTouched` on `OnParametersSet`
  - **Tier 2**: `OnValidationStateChanged` heuristic detects `MarkAsUnmodified` + `NotifyValidationStateChanged` protocol
- **Tier 3 deferred** to when wizard forms are first built
- Own design session needed: framework choice, callbacks, multi-step navigation

### Component features deferred from Phase 2.6.1

- **LipiSkeleton** — placeholder during async load (Phase 2.7)
- **Media slot** on LipiCard — image zones for radiology thumbnails (Phase 5/6 domain components)
- **Loading state** on LipiCard — `Loading` boolean prop (deferred in favour of standalone LipiSkeleton)
- **Dynamic card actions** (context menu ⋮) — Phase 2.6.2+ alongside Modal

### Patient module deferred

- Duplicate detection UI rebuild using LipiCard (Phase 5)
- Record merge flow UI (Phase 5)
- Inter-clinic data sharing via FHIR R4 bundle (Phase 6+)
- Teleconsult — parked indefinitely

### Other deferred features

- Persistent dynamic tab state across sessions (localStorage / server / DB) — Phase 2.10+
- "Recently closed tabs" recovery list — based on real usage data
- Wizard step indicator (LipiStepIndicator) — Phase 2.9
- Multi-patient workstation persistence — Phase 2.10+
