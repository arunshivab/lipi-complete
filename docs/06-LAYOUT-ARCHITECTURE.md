# LiPi HIS — Layout Architecture (LOCKED)

This document consolidates the locked layout decisions for LiPi's clinical workstation.
It is the source-of-truth when Phase 4 OPD design opens (which will produce pixel-level mockups).

**Status:** Architecture LOCKED. Pixel-level visual design deferred to Phase 4 mockups.

---

## 1. Shell architecture

```
┌──────────────────────────────────────────────────────────────────┐
│  TopNav  (44px, navy #0B2545)                                    │
├──────────────────────────────────────────────────────────────────┤
│  Workspace Tab Strip  (LipiDynamicTabs, hidden when empty)       │
├──────────────────────────────────────────────────────────────────┤
│  Patient Banner  (when patient open, two-row structure)          │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Content Area  (page content + optional pinned drawer)           │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│  BottomNav  (70px, dark navy)                                    │
└──────────────────────────────────────────────────────────────────┘
```

**Order rationale (LOCKED):**
- Tab strip sits BETWEEN TopNav and Patient Banner
- Patient Banner is part of the active tab's content (banner content changes with tab)
- Avoids "banner-content mismatch" risk where banner shows Patient A but page shows Patient B
- Tab strip hidden when no tabs open (one-time layout shift acceptable on first tab open)

**Locked decisions:**
- **TopNav + BottomNav shell**, no side nav (validated on clinical-ergonomic grounds)
- 50/50 desktop / tablet usage
- Horizontal screen real estate is sacred (THX / Z-pattern reading)
- One-desk-one-role workflow — rare navigation, dense content
- No clinic switcher in chrome — multi-clinic users logout + login
- Tab strip empty-state: **hidden when empty, slides in on first tab open**
- Tab strip position: **above Patient Banner** (banner belongs to active tab)

---

## 2. TopNav (44px)

### Layout zones

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ [logo] Armoki HIS · by LiPi  │  Patient Registration  │  🔍 🔔3 💬2 📥7 ✋1 ❓ Dr.Reddy ▾ │
└─────────────────────────────────────────────────────────────────────────────┘
  ↑ Identity                     ↑ Page title           ↑ Status + Identity
```

### Left zone — Identity
- Clinic logo (28×28px, uploaded by clinic in Settings)
- Clinic name (e.g., "Armoki HIS")
- "by LiPi" attribution (subtle, hideable for white-label deployments)

### Center zone — Page context
- Page title via `<SectionContent SectionName="page-title">`
- No search bar here (search is in right zone as icon)

### Right zone — Status + Identity (left to right)
- 🔍 **Search icon** — opens Spotlight-style centered overlay (~600px wide) on click / `GS` / `/`
- 🔔 **Notifications** — count badge, opens Notifications drawer (RightScoped, Global scope)
- 💬 **Messages** — count badge, opens Messages drawer (Global scope)
- 📥 **Worklist / Inbox** — role-aware, count badge — visible only for users with worklist queues (lab tech, radiologist, attending)
- ✋ **Approvals** — role-aware, count badge — visible only for approvers (attending, supervisor, SiteAdmin)
- ❓ **Help** — neutral color (NOT red), opens Help panel (Help module deferred to its own phase)
- 👤 **User chip** — name + dropdown menu (Profile / Tasks / Recent activity / Active session info / Theme / Break-glass / Logout)

### Removed from current LiPi TopNav
- Greeting message ("Good evening, Dr. ...") — dropped
- Date / time — moved to BottomNav right zone

### Background and typography
- Background: navy `#0B2545`
- Text: white with hierarchy (clinic name brightest, "by LiPi" subtle)

---

## 3. BottomNav (70px)

### Layout zones

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ 🔍  [Module buttons — Tier-based]                  │ 🔄 ● 18:32 · Tue 12-May │
└─────────────────────────────────────────────────────────────────────────────┘
  ↑                                                    ↑
  Search icon                                          Status zone
```

### Left — Search icon (40px)
- Opens Spotlight-style search overlay (same as TopNav search icon)
- Always at left end of BottomNav

### Center — Module buttons (Tier-based)

Module display is determined by user's granted-module count:

#### Compact tier (2-6 modules)
All granted modules shown as circular buttons. No favorites, no categories.

```
[🔍] [PT][OP][IP][RT]  │  [● online][18:32]
```

Typical user: doctor (3-4 modules), nurse (2-4), receptionist (4), lab tech (2), pharmacist (2).

#### Power tier (7+ modules)
4 user-customisable favorites + up to 5 categories with popup-on-click.

```
[🔍] [Fav1][Fav2][Fav3][Fav4]  │  [Clinical ▸][Diag ▸][Ops ▸][Business ▸][Admin ▸]  │  [● online][18:32]
```

Typical user: SiteAdmin, SysAdmin, GlobalAdmin, CEO/COO/Director, Department Heads, multi-role users.

### Module button specifications

- **Visual:** circular button, color-coded per module, label below
- **Visible size:** ~36px circle
- **Touch target:** invisible 44×44px area (WCAG / iOS HIG compliance)
- **Component:** `LipiModuleButton` + `LipiBottomNav` — built in Phase 2.9 Navigation
- **Active state:** module color filled, ring more prominent, label bold
- **Hover state:** slight glow, label brighter

### Category popup behavior
- Click category button → popup appears ABOVE BottomNav
- Contains all modules belonging to that category, color-coded
- Click-outside or Escape closes
- Ghost click prevention (mandatory — see Standing Rules)

### Right — Status zone
- 🔄 **Sync status** — visible ONLY when not "all synced"
- ● **Connection indicator** — always visible, color-coded (green = online, amber = degraded, red = offline)
- 🕐 **Date / time** — always visible, format per clinic via `IDateFormatService`

### Removed from BottomNav scope
- ❌ **Autosave indicator** — deferred to Clinical Notes module (Phase 5/6)
- ❌ **Print queue indicator** — deferred to Prescription / Pharmacy module
- ❌ **More overflow button** — replaced by Tier-based Compact/Power model

### Deferred decisions (Phase 4 + role permission design)
- Category names and module-to-category mapping
- Default favorites per role
- User favorites preference UI in Settings

---

## 4. Patient Banner — three-layer architecture

This is the most clinically sensitive part of the layout.

### The core principle

**Three locations for clinical information, each with a distinct job:**

| Layer | Question it answers | Trigger |
|---|---|---|
| **Banner** | "What about this patient should I know right now?" | Always visible (when patient open) |
| **Drawer** | "Tell me everything about this person — history, lifestyle, prior visits" | User clicks (RouteScoped to patient routes) |
| **Page CDS** | "Am I about to do something that conflicts with this patient's state?" | The clinical action triggers the check |

### Layer 1 — Banner

Single banner with internal two-row structure (NOT two separate banners).

```
┌─┬──────────────────────────────────────────────────────────────────────┐
│█│ [photo] Rajesh Kumar V. · PT-2026-004821 · M · 47y · OPD #12 · Dr.R │
│█│         ⚠ Penicillin · Sulfa · Latex · Shellfish  [DNR]  ⚠ Fall risk│
│█│ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  │
│█│ Comorbidities: T2DM · HTN · CKD-3        Vitals: BP 138/86 · ECOG 1 │
└─┴──────────────────────────────────────────────────────────────────────┘
 ↑ Isolation precaution band (when active, 8-12px wide colored vertical)
```

### Banner content tiers

**TIER 1 — System mandatory (always shown, cannot be turned off):**
- Patient identity (photo + Name + UHID + Age + Sex)
- **Encounter type + location** (see Encounter Display below)
- **All allergies** (fully listed inline — first 4 shown, "+N more" link if 5+)
- DNR / Code status
- Active critical alerts (fall risk, suicide watch, anticoagulation, pregnancy)

**TIER 2 — Clinic mandatory (SiteAdmin configures per clinic):**
- Examples for cancer hospital: ANC count, ECOG performance status, treatment day flag
- Examples for cardiac hospital: ECG result, BP, anticoagulation status
- Individual clinicians cannot remove these

**TIER 3 — User-customisable (up to 3 additional sections):**
- Comorbidities (active problem list)
- Latest vitals
- Active medications
- Recent labs (abnormal flagged)
- Care team
- Previous diagnoses
- Other reference items

User picks max 3 in Settings > My Preferences > Patient Banner.

**CONTEXT-AWARE STRIPS (automatic, when relevant):**
- Pre-chemo today — appears when chemo order due today, shows pre-chemo checks (ANC, LFT, Wt)
- Pre-surgery today — appears for OT scheduled today, shows NPO status, consent status
- Active admission — appears for IPD, shows admission day count, attending of record
- Day-care active — appears for chemo/dialysis bays

These auto-appear and auto-disappear. User cannot control.

### Encounter type display in identity line

Identity line always shows encounter type + location:

| Encounter | Format |
|---|---|
| OPD | `OPD Visit #12 · Dr. Reddy · Token 47` |
| IPD | `IPD Day 3 · Ward C / Bed 12 · Dr. Reddy` |
| Day-care | `Day-care · Chemo Bay 4 · Cycle 3/6` |
| Discharged | `Discharged 10 May 2026 [Read-only view]` |

### Active flags vs Everything Always — user preference

```
Settings > My Preferences > Patient Banner > Comorbidity display
  ◯ Active flags only (show comorbidities only when uncontrolled)
  ◉ Everything always (show all comorbidities regardless of status)
```

Default: **Everything always** (matches majority Indian clinical practice).

### Allergy display rule
- 1-4 allergies: all names shown inline
- 5+ allergies: first 4 shown + "+N more" link (opens drawer)
- **NOT** a count chip — allergies are critical, always listed

### Isolation precaution band (left edge of banner)

**Critical NABH compliance note:** The band represents **action required**, NOT **patient infection status**.

| Color | Precaution type | When to show |
|---|---|---|
| **Yellow** | Droplet precautions | Flu, meningitis, pertussis (active) |
| **Pink/Magenta** | Contact precautions | MRSA active infection, VRE, C. diff |
| **Blue** | Airborne precautions | TB, measles, COVID active |
| **Red** | Multiple/special precautions | Ebola, highest-isolation |
| **Green** | Protective isolation | Immunocompromised (chemo, BMT) |
| (none) | No active isolation needed | Patient with HIV/HBV/HCV but no active complicating infection |

**NABH + HIV Act 2017 compliance:** HIV/HBV/HCV-positive status alone does NOT trigger a band. Universal precautions apply to all patients. The band is for action-required precautions, not stigma labels. A HIV-positive patient with no active opportunistic infection has NO band. Their HIV status is in the chart for clinical care (ART meds, monitoring) but is NOT a visible flag.

Tooltip on hover shows precaution type. Full details (start date, reason, expected duration, last culture) in drawer.

---

### Layer 2 — Patient Summary Drawer

Opens when user clicks "Patient Summary →" link on banner. Slides in from right.

**Architecture:**
- LipiDrawer with `PinScope = RouteScoped` (scope = `/patients/*`)
- All sections always present
- User can reorder sections and choose expansion defaults (Settings)
- Clinic SiteAdmin can mandate some sections default-expanded

**Sections (all always present):**

1. **Demographics** — full identity, contact, family contact
2. **Lifestyle** — smoking, alcohol, occupation, dietary preferences, exercise pattern
3. **Allergies** — full list (severity, reaction, date verified, reporter)
4. **Active problems** — current comorbidities + active diagnoses
5. **Past problems** — resolved conditions, prior diagnoses
6. **Medications** — current prescriptions + previously prescribed (history)
7. **Vitals** — latest snapshot + access to 90-day trend
8. **Recent labs** — last 30 days, abnormal flagged
9. **Imaging** — recent scans with date
10. **Visits** — last 10-20 encounters (date, doctor, brief reason)
11. **Care team** — primary doctor, specialists, primary nurse
12. **Insurance** — active policy status, validity (no financial flags)
13. **Documents** — uploaded files (consent forms, outside reports)
14. **Personal & Family History** — including family cancer history (important in oncology)

**User customisation scope:**
- Reorder sections (drag/drop in Settings)
- Choose which sections are expanded by default vs. collapsed
- Hide sections they never use (with "Show hidden" toggle to recover)
- **Cannot remove sections from data — only from display**

**Clinic-level customisation:**
- SiteAdmin can mandate certain sections default-expanded
- Example: Armoki mandates Allergies + Vitals + Comorbidities default-expanded

**Drawer hiding never reduces safety** — CDS alerts (Layer 3) fire regardless of drawer state.

---

### Layer 3 — Page Clinical Decision Support (CDS)

**Principle:** Page-level safety alerts fire AT THE MOMENT OF ACTION. Not in banner, not in drawer.

**Examples:**

| Action being taken | Alert triggered by | Severity |
|---|---|---|
| Prescribe Penicillin | Match in patient's allergy list | Hard stop — must acknowledge with reason |
| Prescribe Aspirin | Patient on Warfarin (drug interaction) | Warning — review before continuing |
| Order CT with contrast | Patient eGFR < 30 | Warning — alternative suggested |
| Schedule chemo | Last ANC < 1.5 | Hard stop — repeat CBC first |
| Schedule surgery | Patient on anticoagulation | Warning — bridge plan needed |
| Discharge patient | Pending lab results not reviewed | Warning — review or sign-off |
| Prescribe NSAID | CKD-3 + on ACE inhibitor | Warning — renal risk |
| Order radiation | Pregnancy possible | Hard stop — confirm |

**Alert specifications:**
- **Specific, not generic** (avoid "Patient has medications" — show "Patient on Warfarin — Aspirin increases bleeding risk")
- **Severity-graded** (hard stop vs. warning vs. advisory)
- **Override requires documented reason** for hard stops (HIPAA audit)
- **Alert fatigue prevention** — only fire when actionable, not for general context

**CDS infrastructure phase:** Build alongside first prescription/order module (early Phase 5). Each subsequent module plugs its rules into the engine. Not a standalone phase.

---

## 5. Financial / Insurance flags — NOT on clinical surfaces

**Indian regulatory + ethical position:**
- DPDP Act 2023 — minimum necessary disclosure (clinicians don't need financial info to treat)
- NMC ethics — treatment decisions should not be influenced by ability to pay
- Showing "insurance pending" to a doctor creates implicit pressure to bias treatment

**Locked rules:**
- Financial info → billing module only (NOT in patient banner, NOT in drawer, NOT in clinical pages)
- Insurance status → billing module only
- Payment pending / refund issues → billing module only

**Exception — workflow-specific flags:**
- "Authorisation pending for surgery" — shown on **surgery scheduling page only**, not on clinical pages
- "Insurance pre-auth required for chemo" — shown on **chemo scheduling page only**

These are workflow info, not financial pressure info. They live on workflow-specific surfaces.

---

## 6. LipiDynamicTabs strip

Per Phase 2.6.2 spec — placed between Patient Banner and content area.

- Hidden when no tabs open
- Renders when first tab opens (one-time layout shift acceptable)
- Routing-integrated (URL changes per tab)
- Destroy on close (memory + clinical safety)
- Max 10 tabs soft cap
- 3-option close confirmation (Cancel / Discard / Save & close)

---

## 7. LipiDrawer pin scope policies

Per Phase 2.6.2 spec — pin scope is per-drawer-type:

| Drawer | Pin scope | Behavior |
|---|---|---|
| Patient Summary | RouteScoped (`/patients/*`) | Pinned across patient navigation; auto-closes on non-patient routes |
| Notifications | Global | Pinned across all routes |
| Messages | Global | Pinned across all routes |
| Filter / Sort | PageScoped | Pinned only on current page |
| Activity log | PageScoped | Pinned only on entity's page |

Pin mode disabled below 1024px viewport (auto-switches to overlay).

---

## 8. Status overlay layer (full-screen behaviors)

Activated when needed, not chrome. Each overlay has distinct visual treatment and z-index.

### 8.1 Code calls (e.g., Code Blue) — full-width banner ABOVE TopNav

**Trigger:** Any clinical staff calls a code via dedicated UI action (selects code type + location, confirmation modal prevents accidental activation).

**Visual:** Red banner (`#A32D2D`), white text, pulsing dot, timer counting up since call. Two action buttons (Respond / Details).

**Behavior:**
- z-index 10000 — highest (above session lockout)
- Cannot be dismissed during active code
- Auto-clears when code is cancelled by initiator or supervisor
- Cancellation requires reason (false alarm / patient stable / patient deceased) — logged for audit + Six Sigma analytics
- Audible alert (configurable per role: code team yes, billing/front desk no — SiteAdmin maps role→audio)

**Initiation flow:**
1. Staff clicks "Call code" button
2. Selects code type (Blue / Pink / Red / Purple / Yellow / Black per clinic config)
3. Selects location (auto-fills from patient context if open)
4. Confirmation modal: "Confirm Code Blue at Ward C / Bed 12?"
5. After confirm, code is active for all staff in facility

**Phase:** Phase 8 IPD or Phase 9 emergency module (when ward/bed exists)

### 8.2 Critical labs — toast upper-right + Notifications badge increment

**Trigger:** Lab result outside critical range reported for a patient in user's care relationship.

**Visual:** Toast in upper-right corner of screen, persistent (no auto-dismiss). Patient name + critical value + reported time. Two actions (View patient / Acknowledge).

**Behavior:**
- z-index 3000 — above content, below banners
- Notifications badge increments simultaneously (coexists with TopNav notification system)
- Click "View patient" → opens patient chart in new dynamic tab
- Click "Acknowledge" → marks lab as ack'd, timestamp logged, toast dismisses
- Optional audio chime (configurable per role)

**Who receives the toast (care-relationship filter):**
- Ordering doctor — always
- Currently-attending doctor on the patient — yes
- Nurse assigned to bed (IPD) — yes (escalation tier)
- Lab tech — yes (their own reports)
- All other users — no

**Escalation chain (LOCKED):**
- T+0: Ordering doctor notified
- T+5 min (no ack): escalate to attending physician + nurse
- T+15 min (no ack): escalate to supervisor / charge nurse
- T+30 min (no ack): escalate to SiteAdmin
- All escalation events logged for NABH audit + Six Sigma
- Timing configurable per clinic by SiteAdmin

**Phase:** Phase 7 Lab module

### 8.3 Session security — warning then lockout

**Trigger:** User inactivity (no mouse/keyboard activity) for configured threshold.

**Stage 1 — Warning (default at 13 min idle):**
- Centered modal, dark backdrop
- Title: "Session about to expire"
- 2-minute countdown timer (mono font)
- Single button: "Continue working" (extends session)
- Any mouse/keyboard activity also extends session (silent — no modal needed)
- Low-priority audio chime

**Stage 2 — Lockout (default at 15 min idle):**
- Full-screen overlay (no other UI accessible)
- z-index 9000 — second-highest (only code calls above)
- Background blurred (PHI not visible to anyone walking past)
- Identity shown (user knows it's still their session)
- Password input to unlock (future: biometric)
- Two buttons: "Unlock" / "Logout"

**Work preservation across lockout:**
- Drafts persist in DB (Phase 4 Draft + Tab pattern)
- Tabs persist (auto-restored on unlock)
- On unlock → user returns to exact state they left

**Configuration per clinic (LOCKED):**
- Idle timeout (default 15 min)
- Warning time (default 13 min — 2 min warning)
- Re-auth method (password / biometric / both)

**Phase:** Phase 2.10 (Infrastructure Audit) — HIPAA-required, ships in v1.0

### 8.4 System outages — dismissible banner BELOW TopNav

**Trigger:** Service degraded / offline (manual SiteAdmin notice OR auto-detected via health checks).

**Visual:** Amber banner below TopNav, above patient banner. Icon + message + Details link + dismiss X.

**Behavior:**
- z-index 4000 — below break-glass, above content
- Dismissible per session — but reappears on each page navigation while outage active
- Click "Details" → opens panel with affected modules + ETA + manual workaround
- Multiple outages stack vertically

**Severity gradient (color-coded):**
- Yellow — Planned maintenance (scheduled, low urgency)
- Amber — Service degraded (slow but working)
- Orange — Service offline (module unavailable, alternative workflow active)
- Red — Critical failure (affects patient safety, escalation required)

**Creation paths (BOTH supported, LOCKED):**
- Manual: SiteAdmin / SysAdmin creates outage notice
- Auto: System creates from heartbeat / database / external integration health checks
- Cleared by SiteAdmin manually OR automatically when service restored

**Phase:** Phase 2.10 (Infrastructure Audit) — basic admin-controlled banner

### 8.5 Break-glass active — persistent red banner BELOW TopNav

**Trigger:** User invokes break-glass to access patient outside normal authorization (emergency, covering for colleague, etc.).

**Visual:** Red banner (`#791F1F`), white text, pulsing amber dot. Shows patient name + reason + expiry countdown. End-access button.

**Behavior:**
- z-index 5000 — above outage banner, below modals
- CANNOT be dismissed by user
- Every action while banner active is **specially audited** (separate flag in audit log)
- Banner clears when:
  - User clicks "End access"
  - User navigates away from the patient
  - Session auto-expires

**Configuration per clinic (LOCKED):**
- Default expiry: 60 min
- Extendable with new reason (clinic configurable max extension count)
- All extension events logged

**Why so visible?**
- HIPAA requires clear awareness of "you're accessing PHI outside normal authorization"
- Prevents users from "forgetting" they invoked break-glass and accumulating extended audit-flagged sessions
- Defense in deposition: "Yes, the system clearly showed break-glass was active"

**Phase:** Phase 4-5 — alongside first module that needs break-glass (Phase 4 OPD when patient access controls activate)

### 8.6 Z-index hierarchy (LOCKED)

```
10000  Code call banner       (life-safety, always on top)
 9000  Session lockout overlay (compliance-critical, blocks all)
 8000  Session warning modal   (gentle)
 5000  Break-glass banner      (HIPAA awareness)
 4000  System outage banner    (informational)
 3000  Critical lab toast      (can coexist with banners — different position)
  800-830  LipiModal stack    (standard modal infrastructure)
  700-720  LipiDrawer per side (standard drawer infrastructure)
```

**Coexistence rules:**
- Code call + Critical lab: both visible (different positions on screen)
- Code call + Session lockout: lockout still appears, code call visible above
- Break-glass + System outage: both stack (break-glass above)
- Modal opens on top of any banner — banner stays visible but inert

---

## 9. Settings UI structure (for user preferences)

```
Settings
├── My Profile
├── My Preferences
│   ├── Patient Banner Layout
│   │   ├── Always shown (read-only): identity, allergies, DNR, isolation, alerts
│   │   ├── Required by [Clinic Name]: ANC, ECOG (clinic-mandated)
│   │   └── Your customisations (up to 3): checkboxes
│   ├── Patient Summary Drawer
│   │   ├── Section order (drag to reorder)
│   │   ├── Expanded-by-default toggles per section
│   │   └── Hide sections (with "Show hidden" recovery)
│   ├── Quick Modules (Power tier only)
│   │   └── 4 favorite modules selection
│   ├── Theme (light / dark / auto)
│   ├── Language
│   └── Notification preferences
└── My Activity
```

---

## 10. What ships when (release strategy)

**Locked release policy:**

**Nothing ships to client until all modules are ready.** No half-shipping. No "Phase 4 only" partial release. The hospital gets the complete LiPi suite when it's complete.

This means:
- Phase 4 OPD development can take its time
- No pressure to release before Phases 5-8 complete
- Internal testing / beta with Armoki is internal, not a "release"
- Public / commercial release = complete system ready

---

## 11. Module phase sequencing (locked for cancer-hospital priorities)

This sequencing reflects clinical-revenue priority and user domain expertise (radiology + RT before orders/pharmacy/lab — unusual for HIS but right for Armoki):

| Phase | Modules | Notes |
|---|---|---|
| **Phase 4** | OPD + Billing | Insurance joins if needed, else defers |
| **Phase 5** | Radiology + Radiotherapy + Medical Physics | Order: Rad → RT → MP (RT needs Rad imaging for planning) |
| **Phase 6** | Compliance | Standalone |
| **Phase 7** | Orders + Pharmacy + Lab | The order-and-result loop |
| **Phase 8** | IPD | After pharmacy/lab support exists |
| **Phase 9+** | Chemo, MO, Sx, OT, CSSD, Cath, Dent, Dialysis | Specialty clinical modules |
| **Phase 10+** | Purchase, Asset, Tickets, PACS | Operational + advanced |

**Patient banner pixel-level mockups produced as part of Phase 4 OPD design** (because banner renders inside every patient page — needs real page context).

---

## 12. Decisions deferred to module phases

These are intentionally NOT locked here. They get decided when relevant module opens:

- Module-to-category mapping (Phase 4+ role permission design)
- Default favorite modules per role (Phase 4+)
- Specific CDS rules and severity grading (Phase 5+ alongside prescription module)
- Mockup pixel design for banner / drawer (Phase 4 OPD)
- DNR / advance directives documentation flow (Phase 4 OPD)
- Goals of care documentation (Phase 4 OPD)
- Patient communication preferences (Phase 4 OPD)
- Social work / behavioral flags placement (Phase 6 Compliance or later)
- Multi-language interface (Phase 2.10 i18n infrastructure)
- Draft + Tab pattern detailed implementation (Phase 4 — see `03-DEFERRED-ITEMS.md`)

---

## 12.5 Prescription module — three-layer ship plan

Prescription functionality ships across multiple phases, with each layer building on the previous:

### Layer 1 — Phase 4 OPD (basic Rx)

Documentation + print only:
- Drug name (typed or from drug list)
- Dose, frequency, duration, route, instructions
- Save to patient record + print paper
- No CDS, no inventory, no dispensing tracking

**Sufficient for clinical use** — patient takes paper Rx to any pharmacy (in-house or outside).

### Layer 2 — Phase 5 (CDS-enhanced Rx)

Adds clinical decision support at prescribe time:
- Drug picker with structured formulary (reduces typo errors)
- Allergy alert (hard stop on match)
- Drug interaction alert (warning, severity-graded)
- Renal / hepatic dose adjustment prompts
- Dose range validation
- Pregnancy / pediatric contraindication checks

Still prints paper. CDS infrastructure (alert engine, severity grading, override audit trail) first ships here.

### Layer 3 — Phase 7 Pharmacy (full integration)

Full Pharmacy module:
- Electronic routing to in-house pharmacy queue
- Inventory tracking + decrement on dispense
- Dispensing status (pending / dispensed / partial / cancelled)
- Medication administration record (MAR) for IPD
- Controlled substance handover + audit
- Reorder alerts on low stock
- Drug expiry management
- Automatic pharmacy billing integration

### Paper Rx availability — always

**Locked rule:** Paper Rx printing remains available at every phase, even after Phase 7 ships full Pharmacy.

Reasons:
- Outside pharmacy referrals (patient preference)
- Mixed inventory (in-house doesn't stock everything)
- Walkout cases (patient leaves before collecting meds)
- Discharge Rx (IPD patient takes paper home for post-discharge meds)
- Regulatory — Indian patient right to physical prescription

Paper is parallel, not deprecated.

---

## 13. Critical clinical-ethics standing rules

1. **Universal precautions** — isolation band represents action required, not patient infection status. HIV/HBV/HCV alone never triggers a band (NABH + HIV Act 2017).

2. **Minimum necessary disclosure (DPDP Act 2023)** — financial info excluded from clinical surfaces.

3. **No flagging by sensitive attribute** — religion, caste, sexual orientation, mental health diagnosis, HIV status, etc. never used as visible flags. Used only when clinically relevant for current care.

4. **Drawer hiding never reduces safety** — CDS alerts fire regardless of drawer display state. Drawer is reference; CDS is safety.

5. **Clinical chart documentation never relies on banner** — notes must contain full context. Banner is for the doctor doing the looking, not for documentation. (Training/policy point.)

---

## 14. Open items (not yet locked in this spec)

- Settings UI mockup — to design alongside Phase 4
- Detailed pixel design of TopNav / BottomNav / Banner / Drawer — Phase 4 OPD mockups

---

*Layout architecture LOCKED at structural level. Pixel-level mockups produced as part of Phase 4 OPD design.*

*Last updated: this design session.*
