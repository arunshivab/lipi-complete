# LiPi HIS — CHANGE LOG

> **Purpose**: Track all design/spec changes after v1.0 launch.  
> **Rule**: v1.0 decisions in `00-PROJECT-BASELINE.md` are LOCKED. New changes go HERE.

---

## v1.0 — BASE (May 2, 2026) 🔒 LOCKED

All 12 design decisions finalized. See `00-PROJECT-BASELINE.md`.

### Locked Decisions
1. CSS Architecture: 00-baseline.css + per-module CSS + theme files
2. CLAUDE.md: Extracted to 00-PROJECT-BASELINE.md, deprecated
3. Database Docs: Hybrid (database/ + docs/00-DATABASE/)
4. Module Scope: All 25 modules
5. DOB Confidence: SysAdmin + SiteAdmin can override Verified
6. Duplicate Detection: Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile
7. Record Merge Cooling: 24h/7d/30d/30d+ by role
8. Scheduling Coordinator: Standalone role, multiple per clinic
9. Teleconsult: PARKED (revisit after 10+ modules)
10. Public Calling Board: BASE FEATURE + per-module customization, multi-screen
11. Waitlist Confirmation: Manual
12. **Component Library + Multi-Theme** (added May 2, 2026):
    - Custom Razor components with `Lipi` prefix
    - 41 components total for v1.0 (P0: 15, P1: 15, P2: 11)
    - Two-dimensional theme system (brand × mode)
    - v1.0 ships with: 1 brand (`lipi-default`) × 2 modes (light + dark)
    - Default mode for new users: Light
    - Apple HIG density baseline
    - `/admin/style-guide` page as living showcase

### Production-Ready Modules (v1.0)
- ✅ Admin (Users, Clinics, Orgs, Settings)
- ✅ Authentication (Login, OTP, Password Reset)
- 🟡 Patient Registration (80%)
- 🟡 Appointments (60%)

### Component Library Status (v1.0)
- 📋 Architecture Specs: In progress (Decision #12)
- 📋 41 Razor Components: Build phase
- 📋 Style Guide Page: Build phase
- 📋 Theme System: Build phase
- 📋 Existing pages refactored to use Lipi components: Pending

---

## v1.0 — AMENDMENTS (May 3, 2026)

> **Scope**: Phase 2 Sub-step 2.0 — Theme foundation populated, style guide bootstrapped.
> Amendments below adjust v1.0 deliverables but do NOT alter any of the 12 locked decisions.

### A1 — Border radius scale shifted to admin.css production values

**Changed**: `--r-sm`, `--r-md`, `--r-lg`, `--r-xl` in `00-baseline.css` and Token Naming Convention.

| Token | Phase 0 draft | Phase 1 reconciled (current) |
|-------|---------------|-------------------------------|
| `--r-sm` | 4px | **6px** |
| `--r-md` | 7px | **10px** |
| `--r-lg` | 10px | **14px** |
| `--r-xl` | 16px | 16px (unchanged) |

**Reasoning**: Matches PatientNew patient card (14px) and `reg-*` form fields (legacy 7px → standardized 6px). admin.css `--r-sm: 6px / --r-md: 10px / --r-lg: 14px` is the production-deployed baseline; Phase 0 spec drafts diverged. Reconciliation aligns the spec with what is already shipping.

**Status**: This was already applied in Phase 1 reconciliation (May 2, 2026). Logged here as a formal amendment record so the radius shift is traceable from the change log rather than only from inline comments in the spec.

**Files**:
- `wwwroot/css/00-baseline.css` (already deployed with reconciled values)
- `docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md` §Token Naming Convention (inline `(was 4px in draft)` comments preserved)

---

### A2 — Added `--color-danger-hover` and `--color-danger-active` (both modes)

**Added**: Two new semantic tokens to fill the missing hover/active ladder for the danger color.

| Mode  | `--color-danger` (base) | `--color-danger-hover` (NEW) | `--color-danger-active` (NEW) |
|-------|-------------------------|------------------------------|-------------------------------|
| Light | `#EF4444`               | `#DC2626`                    | `#B91C1C`                     |
| Dark  | `#F85149`               | `#FF6B63`                    | `#E03B33`                     |

**Reasoning**: The Phase 2.1 LipiButton component design package requires distinct hover and active states for destructive actions (Delete, Lock, Suspend, Reset Password). Phase 1 reconciliation defined only `--color-danger` and `--color-danger-pale`, leaving no tokens for interactive states. Without these, every destructive button would use the same base color across rest/hover/active, breaking visual feedback for destructive actions — a UX rule called out in `00-PROJECT-BASELINE.md` §UX Rules.

Light-mode ladder follows the natural Tailwind red-500 → red-600 → red-700 progression (darker on interaction). Dark-mode ladder follows the convention used elsewhere in dark mode (primary, accent): hover lighter for visibility, active darker for pressed feel.

**Files**:
- `wwwroot/themes/mode-light.css` — added two tokens
- `wwwroot/themes/mode-dark.css` — added two tokens
- `docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md` §Token Naming Convention (added entries with "added Phase 2.0 (A2)" comments)
- `docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md` §Mode: Light + §Mode: Dark (added concrete values inline)

---

### A5 — Foundation theme files materialized

**Changed**: Phase 1 placeholders in `wwwroot/themes/mode-light.css` and `wwwroot/themes/mode-dark.css` populated with full token values from the Phase 1 reconciled spec.

**Reasoning**: Phase 1 deployed empty-shell CSS files with `[data-mode="..."] { /* Full token values — added in Phase 3 */ }` placeholders, with the actual values documented only in `00.2-THEMING-ARCHITECTURE.md` §Mode: Light and §Mode: Dark. Phase 2.0 brings the runtime CSS in line with the spec so:

1. The `data-mode` attribute on `<html>` actually drives visible theme switching.
2. Components built in Phase 2.1+ can consume `var(--color-*)` tokens immediately without waiting on Phase 3.
3. The StyleGuide page (A6) has real tokens to showcase.

The original Phase 0 plan deferred token population to Phase 3. Phase 2.0 advances this work because (a) component build (Phase 2.1+) needs tokens to consume, and (b) Phase 1 reconciliation already finalized the values. Deferring further would block Phase 2.1 and add no benefit.

**Files**:
- `wwwroot/themes/mode-light.css` — full Phase 1 reconciled values + A2 additions
- `wwwroot/themes/mode-dark.css` — full Phase 1 reconciled values + A2 additions
- `wwwroot/css/00-baseline.css` — populated with structural tokens (radius A1 already reconciled)
- `wwwroot/themes/brand-lipi.css` — populated with brand identity tokens

**Coordination note**: `admin.css` continues to be loaded through Phase 1-2 per the Modified Big Bang migration strategy. It is removed in Phase 3 after theme tokens are verified against the deployed UI.

---

### A6 — StyleGuide page bootstrapped

**Added**: New page at `/admin/style-guide` (sub-decision 12.5) as the living Foundation showcase.

- Route: `/admin/style-guide`
- File: `src/LiPi.Web/Pages/Admin/StyleGuide.razor` + `StyleGuide.razor.css` (Blazor CSS Isolation)
- Namespace: `LiPi.Web.Pages.AdminPages` (avoids collision with existing `Pages/Admin.razor`)
- Auth: SiteAdmin / SysAdmin / GlobalAdmin only
- Layout: `LiPi.Web.Components.Layouts.TopNavLayout`
- Content: Foundation showcase — color tokens, typography, spacing scale, radius scale, shadow ramp. Mode toggle uses `window.lipiTheme.set` / `.get` JS interop.

**Reasoning**: Sub-decision 12.5 commits to a living component showcase. Phase 2.0 builds the Foundation half (tokens) so Phase 2.1+ can incrementally append the Components section as each LipiButton, LipiTextBox, etc. is built. Bootstrapping it now also gives the user a visible verification surface for A5 — every token defined in the mode files appears on the page.

**Files**:
- `src/LiPi.Web/Pages/Admin/StyleGuide.razor`
- `src/LiPi.Web/Pages/Admin/StyleGuide.razor.css`

---

### A8 — `00-baseline.css` link added to App.razor

**Added**: `<link rel="stylesheet" href="css/00-baseline.css?v=20260503" />` to App.razor `<head>`, positioned FIRST in the cascade order (before `app.css` and `dashboard.css`).

**Reasoning**: `00-baseline.css` was generated and deployed in Phase 2.0 Sub-step 2.0 but App.razor never had a `<link>` tag for it. Phase 1 only included theme files (`brand-lipi.css`, `mode-light.css`, `mode-dark.css`) and Phase 0 module files (`app.css`, `dashboard.css`, `admin.css`) in the cascade. The structural foundation file was orphaned at deployment — defined and shipping but not loaded by the browser, so its tokens (`--r-sm`, `--sp-md`, `--ts-body`, etc.) were never resolvable in the runtime.

Discovered when StyleGuide.razor failed to render correctly during Phase 2.0 deployment verification. Network tab in F12 confirmed all theme files loading but no `00-baseline.css` request. All CSS version strings simultaneously bumped `20260502 → 20260503` for cache busting.

**Files**:
- `src/LiPi.Web/App.razor` — added structural CSS link as first entry in cascade

---

### A9 — Blazor CSS Isolation bundle linked in App.razor

**Added**: `<link rel="stylesheet" href="LiPi.Web.styles.css" />` to App.razor `<head>`, positioned LAST in the CSS cascade (after `admin.css`).

**Reasoning**: Project never had this `<link>` tag — silent omission since project inception. Blazor auto-generates the CSS Isolation bundle at `obj/Debug/net10.0/scopedcss/bundle/LiPi.Web.styles.css` from all `*.razor.css` files in the project, but the framework does NOT auto-inject the link tag — that is the developer's responsibility.

The omission was invisible until StyleGuide.razor (Phase 2.0) became the first page in the project to use Blazor CSS Isolation, exposing the missing infrastructure. Symptom was the silent killer: mode toggle worked (data-mode flipped on `<html>`, cookies persisted, scrollbar darkened via browser hint), but the page itself didn't change colors because StyleGuide.razor.css was never reaching the browser.

This is a project-wide infrastructure fix. Every Phase 2.1+ component that ships with a `*.razor.css` companion file (LipiButton, LipiTextBox, etc.) will now auto-load via this bundle. No further App.razor changes are needed for component CSS going forward.

Loaded LAST in cascade — isolation rules use scope attributes (`[b-abc123]`) for natural specificity, but final-position guarantees they beat any unscoped `admin.css` rules during the Phase 1-2 Modified Big Bang transition.

**Files**:
- `src/LiPi.Web/App.razor` — added isolation bundle link as last entry in cascade

---

### A10 — StyleGuide.razor file path, auth pattern, and JS interop corrected

**Changed**: Three corrections to the deployed StyleGuide.razor that diverged from project conventions.

**1. File path moved**: `Pages/Admin/StyleGuide.razor` → `Pages/StyleGuide.razor`.
The `Pages/Admin/` subfolder collided with the existing `Pages/Admin.razor` file (a class file in the `Pages` namespace), causing a `CS0101` namespace collision. The original Phase 2.0 draft attempted to work around this by setting `@namespace LiPi.Web.Pages.AdminPages` on the page, but the cleaner solution is to place admin-style pages directly in `Pages/` root. This pattern matches `Settings.razor`, `Users.razor`, `Clinics.razor`, etc.

**2. Auth pattern corrected**: `[Authorize(Roles="SiteAdmin,SysAdmin,GlobalAdmin")]` → `[Authorize]` + imperative claim check in `OnInitializedAsync`.
Two-layer bug: (a) PascalCase role names in the `Roles` parameter don't match this project's snake_case claim values (`global_admin`, `sys_admin`, `site_admin`, `staff`); (b) imperative redirect in `OnInitializedAsync` without `forceLoad: true` fires during SSR pre-render before role claims load, causing GlobalAdmin to be redirected to `/dashboard?error=unauthorized` even when authorized.

Production-proven pattern (matches `Settings.razor`, `Users.razor`):
```razor
@attribute [Authorize]
@code {
    [CascadingParameter] private Task<AuthenticationState> AuthState { get; set; } = default!;
    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState;
        var role = auth.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (role is not ("global_admin" or "sys_admin" or "site_admin"))
        { Nav.NavigateTo("/dashboard", forceLoad: true); return; }
    }
}
```

**3. JS interop function names corrected**: `lipiTheme.set(mode)` / `lipiTheme.get()` → `lipiTheme.apply(brand, mode)` / `lipiTheme.getCurrentTheme()`.
The original draft called functions that don't exist on `window.lipiTheme`. The actual API exposed by `theme-switcher.js`:
- `apply(brand, mode)` — sets `data-brand` + `data-mode` on `<html>` and writes both cookies
- `getCurrentTheme()` — returns `{ brand, mode }` object (not a string)
- `init()` — re-reads cookies and reapplies

StyleGuide now deserializes `getCurrentTheme()` into a `ThemeState(string Brand, string Mode)` record and passes both `_brand` + `_mode` to `apply()` on every toggle (preserving the brand while only mode changes).

**Reasoning**: Each of the three issues above traces to the same root cause: synthesizing the page from generic Blazor patterns instead of reading existing production pages and the actual JS helper before generating the file. Lesson logged for Phase 2.1+ kickoff: when introducing a new page or first-of-its-kind component, audit at least one production-proven analog (`Settings.razor` for auth, `theme-switcher.js` for interop) before writing the new file.

A10 also implicitly supersedes the inaccurate descriptions in A6 above — A6 was drafted before deployment debugging surfaced these issues, so it documents the *intended* path and JS interop rather than what actually shipped. A6 is preserved as historical record; A10 is the authoritative current state.

**Files**:
- `src/LiPi.Web/Pages/StyleGuide.razor` — path moved, auth pattern rewritten, JS interop corrected
- `deploy-downloads.ps1` — entry updated to `src\LiPi.Web\Pages\StyleGuide.razor` (no `Admin\` segment)

---

### A11 — LipiButton visual review color refinements

**Phase**: 2.1 (LipiButton component sub-step)
**Date**: 2026-05-03

**Changed**: Two color decisions in `mode-light.css` after Phase 2.1 LipiButton deployment + browser visual review.

**Tokens refined**:

| Token | Phase 2.0 deployed | Phase 2.1 refined |
|---|---|---|
| `--color-primary-hover` | `#1B4DA0` (admin.css cobalt) | `#134E8C` (steel blue) |
| `--color-danger` | `#EF4444` (red-500) | `#DC2626` (red-600) |
| `--color-danger-hover` | `#DC2626` (red-600) | `#B91C1C` (red-700) |
| `--color-danger-active` | `#B91C1C` (red-700) | `#991B1B` (red-800) |

**Reasoning**:

*Primary hover*: Original cobalt (`#1B4DA0`) inherited from admin.css worked well in admin pages but felt overly vibrant when applied to interactive button hover state. Steel blue (`#134E8C`) provides the same hue family with a more professional/clinical tone appropriate for healthcare software. Six options were compared side-by-side; option G (steel blue) chosen.

*Danger ladder*: Tailwind red-500 (`#EF4444`) is industry standard for attention-grabbing alerts but tested as visually fatiguing in clinical contexts where staff encounter destructive action buttons throughout shifts. Shifting the entire ladder one notch deeper (red-600/700/800) preserves the "warning" semantic while reducing eye strain. Tailwind red-600 (`#DC2626`) is what enterprise products like Stripe and Linear use as base danger.

*Dark mode unchanged*: Visual review confirmed dark mode danger ladder (`#F85149` base) remains appropriate. No change required for dark mode.

**Note on A2 supersession**: A11's danger-ladder shift supersedes A2's light-mode values (which set the baseline at red-500/600/700). A2 is preserved as historical record; A11 is the authoritative current state for light-mode danger tokens. Dark-mode danger values from A2 remain authoritative — A11 does not touch them.

**Files**:
- `src/LiPi.Web/wwwroot/themes/mode-light.css` — 4 token values changed, A11 documented in file header

---

## v1.0 — AMENDMENTS (May 4, 2026)

> **Scope**: Phase 2 Sub-step 2.2 — TextInput component family kickoff.
> Token foundation laid before component code so LipiTextBox, LipiTextArea,
> LipiNumberInput, and LipiSelect can consume `var(--color-*)` tokens immediately.
> Amendments below adjust v1.0 deliverables but do NOT alter any of the 12 locked decisions.

### A12 — Phase 2.2 token additions (form labels, required tint, badge text-strong, confidence pills)

**Phase**: 2.2 (TextInputs component sub-step) — kickoff (tokens-only batch)
**Date**: 2026-05-04

**Added**: Token foundation for the four Phase 2.2 components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect). Five token groups added across both light and dark modes.

**Group 1 — Form label text color (1 token × 2 modes)**

| Token | Light | Dark |
|---|---|---|
| `--color-text-strong-secondary` | `#334155` (slate-700) | `#CBD5E1` (slate-300) |

Form labels (top-stacked, weight 500, sentence case) need a stronger tone than `--color-text-secondary` (used for muted body text) but lighter than `--color-text-primary` (reserved for primary content/headers). One notch up the contrast ladder from secondary in both modes.

**Group 2 — Required field treatment (2 tokens × 2 modes)**

| Token | Light | Dark |
|---|---|---|
| `--color-required-bg` | `#FFF7ED` (orange-50) | `rgba(254, 215, 170, 0.06)` (faint apricot wash) |
| `--color-required-border` | `#FED7AA` (orange-200) | `rgba(254, 215, 170, 0.40)` (visible but not glowing) |

Apricot tint on empty required fields. Filled required fields revert to surface bg + standard border (calmer at scale — see UX rationale below). Dark mode uses alpha values rather than solid hex so the apricot reads as a faint warm hint over dark surfaces rather than a solid orange box.

State precedence (highest priority wins, override required tint): Error > Success > Warning > Required-tint > Default.

**Group 3 — Semantic text-strong variants (3 tokens × 2 modes)**

| Token | Light | Dark |
|---|---|---|
| `--color-success-text-strong` | `#047857` (emerald-700) | `#34D399` (emerald-400) |
| `--color-warning-text-strong` | `#92400E` (amber-800) | `#FCD34D` (amber-300) |
| `--color-info-text-strong` | `#1E40AF` (blue-700) | `#60A5FA` (blue-400) |

These fill a gap in the semantic color ladder: existing `--color-success` (etc.) tokens are tuned for buttons/banners (filled-bg use), but badges/tags/alerts/pills need *darker text on tinted bg* — a different role. Light mode uses 700-level (darker) for contrast against pale backgrounds; dark mode uses 300/400-level (brighter) for contrast against deep dark surfaces.

Pattern follows Material Design 3's "on-{color}-container" convention. Future LipiBadge, LipiTag, LipiBanner, status indicators all benefit from these tokens.

No `--color-danger-text-strong` is added. The danger-pale background (`#FEF2F2` light / `#2c0f0f` dark) is read with the existing `--color-danger` value, which is already at 600-level in light and 500-level in dark — both have sufficient contrast against their respective pale backgrounds.

**Group 4 — Confidence pill tokens (8 tokens × 2 modes — composed)**

| Token | Composes from |
|---|---|
| `--color-confidence-verified-bg` | `var(--color-success-pale)` |
| `--color-confidence-verified-text` | `var(--color-success-text-strong)` |
| `--color-confidence-self-bg` | `var(--color-info-pale)` |
| `--color-confidence-self-text` | `var(--color-info-text-strong)` |
| `--color-confidence-estimated-bg` | `var(--color-warning-pale)` |
| `--color-confidence-estimated-text` | `var(--color-warning-text-strong)` |
| `--color-confidence-unknown-bg` | `var(--color-bg-subtle)` |
| `--color-confidence-unknown-text` | `var(--color-text-secondary)` |

Composition over hardcoding so future theme changes propagate automatically. Light/dark differ only because the underlying tokens differ — the chain itself is identical in both files. The "unknown" pill has no semantic anchor (no danger, no success, neutral) — it composes from neutral surface tokens.

Used by DOB confidence badges (Verified / Self-reported / Estimated / Unknown) per Decision #5, identity verification status (Aadhaar, ABHA), and any future confidence-tier UI.

**Reasoning — why composition not hardcoding (the option C decision)**:

Three options were considered: (A) compose from existing semantic `-pale` and base tokens — but the existing semantic base tokens are tuned for button fills, not badge text, so contrast on pale backgrounds was insufficient. (B) hardcode the spec hex values directly into confidence-* tokens — works but creates a parallel "confidence" color family disconnected from the rest of the semantic system, with no reuse for future badges/tags/alerts. (C) extend the semantic ladder with `-text-strong` variants and compose confidence-* from them — adds 6 tokens but every future "darker text on tinted bg" need is solved.

Option C chosen. Mirrors Material Design 3's "on-{color}-container" pattern. Same precedent as A2 (which added `-hover`/`-active` to danger when LipiButton needed them). The extra 6 tokens are a small cost for a reusable foundation.

**Reasoning — why apricot tint on required fields (industry-non-standard but defensible for HIS)**:

Industry norm for required field marking is red asterisk only (Material UI, Ant Design, Carbon, Chakra, MudBlazor, Telerik, Syncfusion, Salesforce Lightning, GitHub Primer, HIG all use this). Field-background tinting is essentially absent from major systems. The closest precedents are GOV.UK (faint border emphasis, not fill) and some Japanese form patterns (yellow tint).

For HIS context the trade-off is different than general SaaS: reception staff scan 30-field patient registration forms under time pressure with high error cost. The apricot tint provides cognitive scaffolding ("what's still missing at a glance") that pairs with asterisk + ARIA so accessibility is solid (color is not the only signal). Fade-to-white-on-fill addresses the "scale calmness" concern — the form de-emphasizes after each field is filled.

This is shipped as default with an opt-out path: a `RequiredVisualStyle` parameter on each input component (defaulting to `ApricotTint`, alternative `AsteriskOnly`) plus a global `LipiInputDefaults` DI configuration so the app-wide default can be flipped in one place if production review indicates the tint feels heavy. We will reassess after PatientNew migration sees it on a real form at scale.

**Files**:
- `src/LiPi.Web/wwwroot/themes/mode-light.css` — 14 tokens added (1 label + 2 required + 3 text-strong + 8 confidence). Unknown-pill tokens compose from existing `--color-bg-subtle` and `--color-text-secondary` but still get their own `--color-confidence-unknown-{bg,text}` aliases for API consistency with the rest of the confidence family.
- `src/LiPi.Web/wwwroot/themes/mode-dark.css` — 14 tokens added (same structure)
- File headers in both updated to reference A12 entry
- `deploy-downloads.ps1` — no change needed for this batch (both `mode-light.css` and `mode-dark.css` already mapped from Phase 1; `CHANGE-LOG.md` already mapped under Docs section). Will need updating in Batch 2 with LipiTextBox component file mappings.

**Coordination note**: Component code (LipiTextBox + companions) ships in subsequent batches. This entry covers the token-only foundation. The B-entry for the Phase 2.2 components themselves will be filed after the components ship and pass visual review (mirroring how A11 was filed after Phase 2.1 LipiButton visual review, not at component kickoff).

---

## v1.0 — AMENDMENTS (May 5, 2026)

> **Scope**: Phase 2 Sub-step 2.2 — TextInput component family completion.
> A13 documents the input-family base infrastructure (shared CSS + JS).
> A15 documents the LipiNumberInput, LipiSelect, and LipiCombobox components
> shipping on top of that base. Together with A12 (May 4 token foundation),
> these three amendments close out Phase 2.2.
>
> A14 (originally reserved for the LipiButton env-gated retrofit) was held in
> sequence between A13 and A15 even though it shipped a day later than A15.
> The amendment number reflects the logical ordering — A14 was always the
> LipiButton retrofit slot, regardless of when it landed.

### A13 — Phase 2.2 input-family base (LipiTextArea + shared lipi-inputs.css + lipi-input.js)

**Phase**: 2.2 (TextInputs component sub-step) — Batch 3
**Date**: 2026-05-05

**Added**: Shared base infrastructure for the input component family. Pre-existing LipiTextBox (shipped Batch 2, May 4) was sliced into a thin component-deltas-only file with the bulk of its styling extracted into a shared `lipi-inputs.css` consumable by LipiTextArea (this batch) and the upcoming LipiNumberInput / LipiSelect / LipiCombobox (A15).

**The extraction (lipi-inputs.css)**

Pre-Batch-3 state: LipiTextBox.razor.css contained ~310 lines covering the input-family base styling (wrapper grid, label, field, control, helper, state cascade, sizes, icons, confidence pill, required tint). All of these would have to be duplicated across every new input component if not extracted.

Post-Batch-3 state: shared `wwwroot/css/lipi-inputs.css` (~290 lines) holds the family base. LipiTextBox.razor.css slimmed to ~19 lines of LipiTextBox-specific overrides. New components (LipiTextArea, and later A15's components) consume the shared file with minimal per-component scoped CSS.

**LipiTextArea component (multi-line text)**

InputBase<string?>-derived component for clinical notes, history fields, addresses. Supports `MinRows` (default 3) and `MaxRows` (default 8) parameters. Top-aligned leading/trailing icons (vs LipiTextBox's center-aligned) so they don't drift down with content.

**Autogrow infrastructure (lipi-input.js)**

Created `wwwroot/js/lipi-input.js` exposing `window.lipiInput` global with three functions: `autogrow(textarea)` (measure scrollHeight, set inline height up to MaxRows × line-height, switch to scrollable beyond), `attachAutogrow(textarea)` (wire up oninput listener), `detachAutogrow(textarea)` (cleanup). Detach is critical for SignalR circuit disposal — without it, listener references leak across page navigations.

The autogrow approach was chosen over CSS `field-sizing: content` because the CSS property is Chrome 123+/Safari 17.4+/Firefox-flagged. HIS deployments include older clinical workstations on pinned browsers — JS-based autogrow is the portable v1.0 solution. The JS file is structured to allow future migration to `field-sizing: content` as a feature-detected fast path.

**App.razor cache bumping**

App.razor cache version (the `?v=YYYYMMDD` query string used to bust browser caches on deploy) bumped 20260503 → 20260504 to invalidate cached LipiTextBox.razor.css from clients that loaded it pre-extraction. New `lipi-inputs.css` and `lipi-input.js` references added to App.razor's stylesheet/script blocks with the same versioned query string. This pattern repeats for every CSS or JS change in subsequent batches.

**Reasoning — why a single shared CSS/JS file (not per-component)**

Considered: (a) full duplication per component (no shared base) — rejected, ~300 lines × 4 components = ~1200 lines of inevitable drift. (b) one CSS file per concern (e.g., separate `lipi-inputs-state.css`, `lipi-inputs-helper.css`, etc.) — rejected, breaks Blazor's CSS isolation flow and adds bundle complexity for marginal benefit. (c) single shared base + per-component scoped CSS — chosen. Component scoped CSS handles the small unique deltas (e.g., LipiTextArea's top-aligned icons), shared base handles the family rules.

Same logic for JS: single `lipi-input.js` consolidates input-family JS helpers. As the family grows (A15 adds dropdown positioning, scroll-reposition, value-sync helpers), they all live in this one file rather than spawning per-component JS.

**Files**:
- `src/LiPi.Web/wwwroot/css/lipi-inputs.css` — NEW. Shared input-family base (~290 lines).
- `src/LiPi.Web/wwwroot/js/lipi-input.js` — NEW. Shared input-family JS helpers.
- `src/LiPi.Web/Components/Shared/LipiTextArea.razor` — NEW. Multi-line text component.
- `src/LiPi.Web/Components/Shared/LipiTextArea.razor.css` — NEW. Component-specific deltas.
- `src/LiPi.Web/Components/Shared/LipiTextBox.razor.css` — slimmed from ~310 lines to ~19 lines (deltas only).
- `src/LiPi.Web/App.razor` — cache version 20260503 → 20260504; added `<link>` for lipi-inputs.css and `<script>` for lipi-input.js.
- `src/LiPi.Web/Pages/Test/TextareaTest.razor` — verification scaffold for the autogrow + states.
- `deploy-downloads.ps1` — added entries for new files.

**Coordination note**: A15 builds on this base. Every component A15 adds (LipiNumberInput, LipiSelect, LipiCombobox) consumes lipi-inputs.css and extends lipi-input.js with additional helper functions. The base file architecture established here is the foundation for all subsequent input-family work.

---

### A14 — LipiButton env-gated parameter validation retrofit

**Phase**: 2.1 (LipiButton component sub-step) — A14 reservation backlog
**Date**: 2026-05-06 (numbering preserved between A13 and A15 per reservation)

**Changed**: `LipiButton.razor` parameter validation — replaces the always-throw `OnParametersSet` (originally shipped in Phase 2.1) with the env-gated pattern proven across all five Phase 2.2 components and consolidated into `LipiInputBase.ValidateOrFallback` in A16.

**Before A14:**

```csharp
protected override void OnParametersSet()
{
    if (IsIconOnly && string.IsNullOrWhiteSpace(AriaLabel))
    {
        throw new ArgumentException(
            "LipiButton: AriaLabel is required when in icon-only mode " +
            "(ChildContent null + Icon set). WCAG 2.1.4.6 / SC 4.1.2 compliance.");
    }
}
```

A `throw` in OnParametersSet crashes the page on any render, including production patient-care screens. For Phase 2.1 this was an acceptable trade-off (icon-only-without-AriaLabel is a clear coding error and the rendering surface was admin-only). For production HIS workloads it's unsafe — a missed AriaLabel on a settings panel button shouldn't blank the patient registration screen.

**After A14:**

```csharp
private string? _resolvedAriaLabel;

protected override void OnParametersSet()
{
    _resolvedAriaLabel = AriaLabel;

    if (IsIconOnly && string.IsNullOrWhiteSpace(AriaLabel))
    {
        var message =
            "LipiButton: AriaLabel is required when in icon-only mode " +
            $"(ChildContent null + Icon='{Icon}'). WCAG 2.1.4.6 / SC 4.1.2 compliance.";

        if (Env.IsDevelopment())
        {
            throw new InvalidOperationException(message);
        }
        else
        {
            Log.LogError("[LipiButton] {Message} Falling back to '{Fallback} button'.",
                message, Icon);
            _resolvedAriaLabel = $"{Icon} button";
        }
    }
}
```

The markup binds `aria-label="@_resolvedAriaLabel"` (was `@AriaLabel`). Production fallback derives the accessible name from the icon name itself — `Icon="save"` + missing AriaLabel renders `aria-label="save button"`. Reasonable for screen readers; developers still see the dev-time exception and learn to set AriaLabel explicitly. The exception type is `InvalidOperationException` rather than `ArgumentException` because the parameter values arrived correctly from Blazor — the issue is a programming-time mistake about which combination is valid, not a malformed argument.

**Injected services added:**
- `IWebHostEnvironment Env` — for the `Env.IsDevelopment()` branch
- `ILogger<LipiButton> Log` — for the production warning

Same services and same names as the five Phase 2.2 components, so the pattern is immediately recognizable across the family.

**Why a `_resolvedAriaLabel` field instead of mutating the [Parameter] directly:** Blazor re-sets `[Parameter]` properties on every render, so any direct mutation in OnParametersSet would survive only one render cycle. The field is recomputed each OnParametersSet from the current AriaLabel value, ensuring the fallback flows through to the DOM consistently. This matches the pattern in LipiInputBase (`_resolvedName`, `_resolvedLabel`, `_resolvedAutocomplete`).

**Files**:
- `src/LiPi.Web/Components/Shared/LipiButton.razor` — file header `TODO (A14)` block replaced with brief A14-shipped reference; `@using` block extended with `Microsoft.AspNetCore.Hosting` and `Microsoft.Extensions.Logging`; `@inject` block extended with `IWebHostEnvironment Env` and `ILogger<LipiButton> Log`; markup `aria-label` binding switched to `_resolvedAriaLabel`; `OnParametersSet` rewritten with env-gated pattern; `_resolvedAriaLabel` private field added; AriaLabel parameter docstring updated to describe the new env-gated behavior.

**No other files changed.** No CSS, no JS, no test page updates, no deploy script changes (LipiButton.razor already mapped from Phase 2.1). No cache bump (Razor recompile only).

**Coordination note**: A14 closes the documented divergence flagged at A12 (token foundation). The Known Divergences section below is updated to mark A14 resolved (entry retained as historical record). The v1.1 PLANNED list is updated to remove the A14 line item, since the retrofit has now shipped rather than waiting for v1.1.

**Verification**: Manual smoke test on `/styleguide` confirmed:
- Icon-only LipiButton with explicit AriaLabel renders correctly (no behavior change)
- Icon-only LipiButton without AriaLabel: in Development build → throws on render with diagnostic; in Release build → renders with `aria-label="{Icon} button"` and logs the error
- Non-icon-only buttons (text + optional icon): no change, AriaLabel optional and passes through

---

### A15 — Phase 2.2 component completion (LipiNumberInput, LipiSelect, LipiCombobox)

**Phase**: 2.2 (TextInputs component sub-step) — Batches 4 + 4.1 + 4.2 + 4.3 + 5 + 5.1 + 5.2 + 5.3 + 5.4
**Date**: 2026-05-05

**Added**: Three remaining components in the Phase 2.2 family — LipiNumberInput<TValue> (locale-aware numeric), LipiSelect<TValue> (identity-selector dropdown), LipiCombobox<TValue, TItem> (templated dropdown). With these shipped, Phase 2.2 closes.

**Group 1 — LipiNumberInput<TValue> (Batches 4 + 4.1 + 4.2 + 4.3)**

Generic-typed numeric input supporting 22 numeric TValue types (signed/unsigned int/long/short/byte/sbyte/uint/ulong/ushort/float/double/decimal and their `Nullable<>` variants). Type validation runs at OnInitialized via a `SupportedTypes` whitelist; non-whitelisted TValue triggers env-gated throw (Dev) / log+fallback (Prod).

| Sub-batch | Scope |
|-----------|-------|
| Batch 4 (May 5) | Component itself: locale-aware formatting (en-IN default, 1,23,456 grouping), Min/Max bounds with `Comparer<TValue>.Default`, optional steppers with at-bound disable, focus/blur display switching (raw on focus, formatted on blur), App.razor cache 20260505 → 20260506. |
| Batch 4.1 | Arrow keys (↑/↓ to increment/decrement), EditForm test page integration with DataAnnotationsValidator. |
| Batch 4.2 | `DisableArrowKeys` parameter, `BlockNonNumericInput` C# input filter (locale-aware via `EffectiveCulture`, signed/unsigned auto-detect from TValue, single-decimal collapse, first-char-only minus). |
| Batch 4.3 | JS DOM value-sync (`lipiInput.setValue`) — fixes a render-diff edge case where filtered values weren't reaching the DOM. Cursor position preserved via `setSelectionRange`. App.razor cache 20260506 → 20260507. |

The `dynamic` arithmetic dispatch in `AddStep(...)` deserves a note: rather than maintaining a 22-entry switch table for stepper increments, the component uses `dynamic dc = c; dynamic ds = s; return (TValue)(dc - ds);`. Performance impact is negligible (steppers are click-driven, not hot-path), and the alternative (explicit casts per type) was rejected as verbose-and-error-prone.

**Group 2 — LipiSelect<TValue> + LipiCombobox<TValue, TItem> (Batches 5 + 5.1 + 5.2 + 5.3 + 5.4)**

Two concrete dropdown components sharing `LipiSelectBase<TValue, TItem>` abstract base (~750 lines) which holds the state machine: search filter, dropdown open/close, keyboard navigation, virtualization, JS interop for positioning + outside-click + scroll-reposition.

LipiSelect<TValue> is identity-selector (item IS the value, label = ToString() with culture-aware formatting). For enums, primitive lists, simple string lists. LipiCombobox<TValue, TItem> is templated (caller provides `ValueSelector` + `LabelSelector` + optional `ItemTemplate` / `SelectedAnchorTemplate`). For Country with flag+name+ISD, Doctor with specialty+photo, Insurance scheme with provider+plan code.

| Sub-batch | Scope |
|-----------|-------|
| Batch 5 (May 5) | LipiSelectBase abstract + LipiSelect + LipiCombobox concrete. lipi-input.js extended with `positionDropdown`, `attachSelectHandlers`, `detachSelectHandlers` (RAF-throttled scroll, capture-phase listeners, `window._lipiSelectState` namespacing). lipi-inputs.css extended with ~150 lines of select/combobox styling. App.razor cache 20260507 → 20260508. |
| Batch 5.1 | Build-error fix: `GetFilteredOptions` return type changed from `IReadOnlyList<TItem>` to `List<TItem>` for `<Virtualize>` ICollection compatibility and IndexOf access. LipiNumberInput nullability annotations cleaned up. |
| Batch 5.2 | Keyboard architecture refinement: consolidated `HandleAnchorKeyDown` + `HandleDropdownKeyDown` into single `HandleKeyDown` branching on `_isOpen`. Added `_wasOpen` field + `OnAfterRenderAsync` hook for focus-on-open transition. Search input gets `form="lipi-select-orphan"` (HTML5 form-orphan technique to disassociate from any `<EditForm>` ancestor — Enter has no submission target). lipi-inputs.css `.lipi-select-display { line-height: 1 }` for vertical alignment with LipiTextBox. App.razor cache 20260508 → 20260509. |
| Batch 5.3 | Non-searchable focus fix: explicit `_anchorRef.FocusAsync()` for Searchable=false case (was relying on click-to-focus, which is browser-and-render-timing dependent). SelectTest.razor gained Section 7 alignment-comparison row. |
| Batch 5.4 | CSS selector bug fix: `.lipi-input-medium .lipi-input-select .lipi-input-field` (descendant, with space) → `.lipi-input-medium.lipi-input-select .lipi-input-field` (compound, no space). Both classes were on the same wrapper element, so the descendant form never matched. Pre-fix LipiSelect field rendered at ~17.6px (natural content height); post-fix renders at the intended 28/32/40px per size. Same fix applied to disabled/readonly cursor rules. App.razor cache 20260509 → 20260510. |

**The architectural decisions captured for future reference**

A15 introduces several engineering calls worth recording for sibling-component decisions later:

1. **Reposition-don't-close on scroll** (HIS-specific UX) — when a dropdown is open and user scrolls the page or any scrollable ancestor, the dropdown REPOSITIONS to follow the anchor instead of closing. Material UI does this. Ant closes. Carbon repositions. We chose reposition because clinical workstations may have tracking pointers, accessibility scroll modes, or scrollable modal panels — closing-on-scroll surprises the user mid-selection.

2. **`scroll` listener with `capture: true`** — scroll events don't bubble, but they DO propagate during the capture phase. Single document-level listener catches scroll on any scrollable ancestor (no need to attach per-ancestor).

3. **RAF-throttled reposition** — momentum scrolls fire scroll events at 60fps. Without `requestAnimationFrame` throttling, `positionDropdown` would run 60×/second flooding the SignalR roundtrip.

4. **`window._lipiSelectState[dropdownId]` namespacing** — multiple concurrent dropdowns must coexist without leaking listeners. Per-dropdown state (anchor ref, listeners, RAF id) keyed by the dropdown's element ID.

5. **Defensive detach-before-attach** — `attachSelectHandlers` calls `detachSelectHandlers` first. Protects against re-render double-attach during fast user interactions.

6. **Pinned list-based sort preservation** — `PinnedValues` items render in caller-passed order (NOT alphabetical). Caller intent ("recently used", "common countries with India first") shouldn't be overridden by alphabetical sort.

7. **`TryParseValueFromString` triple cascade** — label match → value-string match → `AllowFreeText` fallback. Most user input is label-match; value-string covers paste-the-underlying-value edge cases; free-text covers "Other / freeform" use cases.

8. **Form-orphan technique for search input Enter** — `<input form="lipi-select-orphan">` (HTML5 standard) disassociates the search input from any ancestor form. Without this, Enter would submit the form before our async C# handler runs (Blazor Server's SignalR roundtrip is too slow to preventDefault retroactively from C#).

9. **Full ARIA combobox/listbox/option pattern** — the select family implements WAI-ARIA 1.2's combobox role pattern (anchor `role="combobox"` + `aria-expanded` + `aria-controls`; dropdown `role="listbox"`; options `role="option"` + `aria-selected`). Genuinely accessible to screen readers, not just visually styled.

10. **`EqualityComparer<TValue>.Default` for value comparison** — type-agnostic, handles nullable structs correctly, respects custom `IEquatable<T>` on caller types.

11. **`_dotNetRef.Dispose()` in async disposal** — prevents `DotNetObjectReference` leak across Blazor circuits. Always dispose async refs explicitly via `IAsyncDisposable`.

**Reasoning — why two concrete subclasses, not one component with optional templates**

Considered: a single `LipiSelect<TValue>` with optional `ValueSelector`/`LabelSelector`/`ItemTemplate` parameters that fall back to identity when null. Rejected because:
- The type signature `LipiSelect<TValue, TItem>` always exposes both type params even for the simple-string-list case where TItem = TValue. Verbose at every call site.
- The "is this Combobox-style or Select-style?" branch lives at every selector access (`ItemValue(item)`, `ItemLabel(item)`) — runtime overhead and code complexity.
- Two clean public APIs (`LipiSelect<TValue>` for simple, `LipiCombobox<TValue, TItem>` for rich) reads better.

Trade-off: shared logic must live in `LipiSelectBase<TValue, TItem>` abstract class. The 80%+ code reuse via abstract base + 20% concrete-class-specific selectors is the right factoring. Mirrors Material UI's Select-vs-Autocomplete split.

**Known limitations** (formally captured for v1.1+ tracking)

| # | Limitation | Component | Mitigation |
|---|------------|-----------|------------|
| 1 | Unsigned TValue underflow without explicit Min | LipiNumberInput | Pass `Min="0"`; at-bound logic disables the "−" button. |
| 2 | Multi-decimal-point silent stripping | LipiNumberInput | Filter handles only first decimal separator; subsequent ones stripped. Acceptable for clinical-numeric inputs. |
| 3 | Range selection collapses to cursor on filter | LipiNumberInput (setValue) | Native browser range-replace behavior lost; non-trivial JS fix deferred. |
| 4 | Brief letter-flash on Blazor Server (~20-60ms LAN) | LipiNumberInput | C# filter runs after SignalR roundtrip; user sees character flash before strip. WAN deployments more visible. Acceptable for v1.0; revisit if PatientNew migration shows it's disruptive. |
| 5 | Composition events / IME / non-Latin numerals | LipiNumberInput | Devanagari/Arabic numerals stripped by filter. Workaround: `BlockNonNumericInput="false"` + culture-aware `BindConverter` for non-ASCII numeric deployments. |
| 6 | CSS compound-vs-descendant selector trap | LipiSelect (Batch 5.4 lesson) | When two state-driven classes apply to the same element, target with compound (no-space) selectors. Documented in 01.2-TextInputs.md §9.11 for future component CSS authors. |

**A14 trigger condition met**: The env-gated throw pattern (`IWebHostEnvironment.IsDevelopment()` → throw; production → `ILogger.LogError` + fallback) shipped successfully across all five Phase 2.2 components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox). The pattern is proven. A14's queued LipiButton retrofit (which awaited "Phase 2.2 components shipping the env-gated pattern proven") subsequently shipped 2026-05-06 — see A14 entry above.

**Files** (across all sub-batches):

Components:
- `src/LiPi.Web/Components/Shared/LipiNumberInput.razor` — NEW. ~860 lines. Generic numeric input with locale-aware formatting, steppers, bounds, filter modes.
- `src/LiPi.Web/Components/Shared/LipiNumberInput.razor.css` — NEW. ~40 lines. Component-specific deltas.
- `src/LiPi.Web/Components/Shared/LipiSelectBase.cs` — NEW. ~750 lines. Abstract base for select family.
- `src/LiPi.Web/Components/Shared/LipiSelect.razor` — NEW. ~240 lines. Identity-selector concrete subclass.
- `src/LiPi.Web/Components/Shared/LipiSelect.razor.css` — NEW. Minimal placeholder.
- `src/LiPi.Web/Components/Shared/LipiCombobox.razor` — NEW. ~310 lines. Templated concrete subclass.
- `src/LiPi.Web/Components/Shared/LipiCombobox.razor.css` — NEW. Anchor template content alignment.

Shared infrastructure (extended from A13 base):
- `src/LiPi.Web/wwwroot/css/lipi-inputs.css` — extended ~150 lines (select/combobox styling, search row, options list, empty state, open-state focus rings).
- `src/LiPi.Web/wwwroot/js/lipi-input.js` — extended ~120 lines (`selectAll`, `setValue`, `positionDropdown`, `attachSelectHandlers`, `detachSelectHandlers`).

Test scaffolds:
- `src/LiPi.Web/Pages/Test/NumberInputTest.razor` — NEW. 6-section verification scaffold.
- `src/LiPi.Web/Pages/Test/SelectTest.razor` — NEW. 7-section verification scaffold including alignment-comparison row.

App-level:
- `src/LiPi.Web/App.razor` — cache version bumped 20260504 → 20260505 → 20260506 → 20260507 → 20260508 → 20260509 → 20260510 across 5 sub-batches.
- `deploy-downloads.ps1` — added entries for all new components and test scaffolds.

**Coordination note**: With A13 + A15 shipping, all five Phase 2.2 components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox) are in production. Spec doc `docs/00-COMPONENTS/01.2-TextInputs.md` (shipping in Batch 6 alongside this entry) captures the architecture, parameters, state precedence, and architectural decisions. StyleGuide showcase (`/admin/style-guide`) extended with new sections covering all five components plus a §7 alignment-comparison row.

Forward references:
- **Phase 2.2.5** — `EditContext.OnValidationStateChanged` auto-population across all 5 components (removes the need for explicit `<ValidationMessage For>`). Prerequisite for full PatientNew migration.
- **Batch 7** — Build hygiene sweep (warnings → 0; pre-existing Clinics CS8601, PatientNew CS0649/CS0414; MailKit → System.Net.Mail.SmtpClient; SharpZipLib resolution).
- **A14** — LipiButton env-gated retrofit (✅ shipped 2026-05-06; see A14 entry above).
- **Phase 2.3** — `LipiSelectTextCompound`, `LipiMultiSelect`, `LipiCheckbox`, `LipiRadio`, `LipiToggle`.

---

### Phase 2 Sub-step Status (May 5, 2026)

- ✅ Sub-step 2.0: Foundation tokens + Style Guide bootstrap
  - `mode-light.css` + `mode-dark.css` populated with full token set
  - `brand-lipi.css`, `00-baseline.css` (structural tokens)
  - `/admin/style-guide` Foundation showcase live
- ✅ Sub-step 2.1: LipiButton component
  - 5 variants × 3 sizes (Primary, Secondary, Danger, Ghost, Link)
  - 29-icon Lucide starter library (`LucideIcon` component)
  - Size-matched spinner (`LipiButtonSpinner`)
  - Full Style Guide showcase: variant grid, states demo, icon patterns, real-world use cases
  - Spec doc: `docs/00-COMPONENTS/01.1-Buttons.md`
- ✅ Sub-step 2.2: TextInputs (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox)
  - ✅ Batch 1 (May 4): Token foundation (A12) — apricot tint, label color, badge text-strong, confidence pills
  - ✅ Batch 2 (May 4): LipiTextBox + companions (LucideIcon, LipiInputDefaults, AutocompleteValidator, LipiTextInputTypes)
  - ✅ Batch 3 (May 5): LipiTextArea + lipi-inputs.css extraction + lipi-input.js created (A13)
  - ✅ Batch 4 (May 5) + 4.1 + 4.2 + 4.3: LipiNumberInput<TValue> generic component (A15)
  - ✅ Batch 5 (May 5) + 5.1 + 5.2 + 5.3 + 5.4: LipiSelect + LipiCombobox + LipiSelectBase (A15)
  - ✅ Batch 6 (May 5): StyleGuide showcase additions + `01.2-TextInputs.md` spec doc + A13/A15 changelog entries + deploy-downloads.ps1 finalized
- ⏳ Sub-step 2.2.5: `EditContext.OnValidationStateChanged` auto-population across all 5 Phase 2.2 components (removes explicit `<ValidationMessage For>`). Prerequisite for full PatientNew migration.
- ⏳ Sub-step 2.3: Selectors (LipiSelectTextCompound, LipiMultiSelect, LipiCheckbox, LipiRadio, LipiToggle). LipiCombobox shipped in 2.2 ahead of original 2.3 plan.
- ⏳ Sub-steps 2.4–2.5: Remaining foundational components

---

### Known divergences from deployed code (defer to v1.1)

The amendments above bring the spec into sync with deployed Phase 2.0 work. The following divergences between spec and deployed code are **acknowledged but not addressed in Phase 2.0** to keep scope contained:

- **A7 (planned)** — `00.2-THEMING-ARCHITECTURE.md` §Theme Provider Component: spec section shows the older `IUserPreferenceService` + `IClinicContextService` + `eval()` pattern. Deployed code uses `IThemeContextService` + `lipiTheme.apply` (post-D6 architecture refinement during Phase 1 Deliverable 6).
  - Defer reconciliation to a focused v1.1 spec-update session (call it A7) so the spec accurately documents production code.
  - Until then, treat the deployed code as authoritative and the spec section as historical reference.

- **A14 (✅ shipped 2026-05-06)** — Phase 2.1 LipiButton previously used unconditional `throw new ArgumentException` in `OnParametersSet` for missing `AriaLabel` on icon-only buttons. Phase 2.2 components shipped with an env-gated pattern: `IWebHostEnvironment.IsDevelopment()` → throw; production → `ILogger.LogError` + render with auto-generated fallback. This protected production from parameter-validation crashes (high stakes for a hospital app) while keeping dev-time strictness.
  - **Resolution**: A14 retrofit landed 2026-05-06 (see A14 entry above). LipiButton now uses the same env-gated pattern as the Phase 2.2 components. The `TODO (A14)` comment in `LipiButton.razor` was replaced with a brief A14-shipped reference.
  - **Historical record retained**: This entry kept in the divergences section as a record of the resolved drift between Phase 2.1 and Phase 2.2 patterns. Future Known-Divergences scans should treat it as closed.

---

## v1.0 — AMENDMENTS (May 6, 2026)

> A16 documents Phase 2.2.5 — the EditContext auto-population migration. Three
> batches (8a, 8b, 8c) executed sequentially with verification gates between
> each. The five Phase 2.2 components now derive from a shared
> `LipiInputBase<TValue>` abstract class that owns common parameters, services,
> EditContext subscription, and the touched-state validation pattern.

### A16 — Phase 2.2.5 EditContext auto-population (LipiInputBase + 5-component migration)

**Trigger.** Phase 2.2 components shipped with explicit `<ValidationMessage For>` markup required adjacent to every input — verbose for callers and inconsistent with how production form libraries (Material UI, Carbon, Ant Design) integrate validation. The fix: each component subscribes to `EditContext.OnValidationStateChanged` internally and populates its own helper slot with the current validation message. Caller markup becomes `<LipiTextBox Name="email" @bind-Value="model.Email" />` with no separate ValidationMessage tag.

**Batches & sequencing.**

- **Batch 8a** (May 5 → May 6): LipiInputBase class created. LipiTextBox migrated to inherit from it. Two-step verification: (Step 1) ship with comparison-baseline `<ValidationMessage>` visible alongside auto-populated helper slot; user verifies texts match; (Step 2) remove comparison baseline, leaving pure E2 form. Closed clean across 5 sub-iterations (8a, 8a.1, 8a.2, 8a.3, 8a.4) covering build issues caught by tightening checks.
- **Batch 8b** (May 6): LipiTextArea + LipiNumberInput migrated. ~400 lines of duplicated code eliminated across the two components. LipiNumberInput's raw → formatted display switch on blur uses the new `OnFieldBlurred` virtual hook on the base.
- **Batch 8c** (May 6): LipiSelectBase migrated to a 3-level hierarchy (`InputBase<TValue>` → `LipiInputBase<TValue>` → `LipiSelectBase<TValue, TItem>` → `LipiSelect<TValue>` / `LipiCombobox<TValue, TItem>`). LipiSelectBase implements both IDisposable (inherited) AND IAsyncDisposable (own JS handler cleanup). Touched-state pattern wired into three trigger points: `CloseDropdownAsync`, `SelectItem`, and a new `HandleAnchorBlur` for tab-past handling.

**LipiInputBase architecture.**

- Abstract class, derives from `InputBase<TValue>`, implements `IDisposable`. Microsoft's `InputBase<TValue>` already provides the disposal pattern; we extend rather than hide it. EditContext detach is genuinely synchronous (`-=` on an event handler), so IAsyncDisposable would be ceremony without functional benefit. Components needing async cleanup (LipiSelectBase) layer IAsyncDisposable on top.
- 18 common `[Parameter]` declarations: Name, Label, Placeholder, Helper, Error, Warning, Success, Required, Disabled, ReadOnly, Autocomplete, Size, Icon, AriaDescribedBy, RequiredVisualStyle, LabelConfidence, Cols, Class.
- 3 injected services: `IOptions<LipiInputDefaults>`, `IWebHostEnvironment`, `ILogger<LipiInputBase<TValue>>`. Component-specific injections (`IJSRuntime` for LipiTextArea/NumberInput/SelectBase) layer on top.
- `EditContext.OnValidationStateChanged` subscription via `OnInitialized`, with swap-handling in `OnParametersSet` (`ReferenceEquals` guard against the same `EditContext` instance, manual detach + reattach when EditContext is replaced).
- `_editContextError` field + `EffectiveError` property: `Error ?? _editContextError`. Caller-set `Error` wins display, but does NOT fail `EditContext.Validate()` (intentional — see §10.10 of 01.2-TextInputs.md). For submission-blocking server-side errors, callers handle in `OnValidSubmit` or use a custom validator with `EditContext.GetMessageStore()`.
- State precedence cascade (Disabled > ReadOnly > Error > Success > Warning > RequiredEmpty > Default) reads `EffectiveError` so EditContext-driven errors elevate state correctly.
- Touched-state pattern: `_isTouched` bool, defaults false. `HandleValidationStateChanged` gates `_editContextError` update on `_isTouched` — silent until first blur, then live updates per keystroke. Matches Material UI / Carbon / Ant Design industry standard.
- `HandleBlur(FocusEventArgs)` method bound via `@onblur` in concrete components. Marks `_isTouched=true`, calls `EditContext.NotifyFieldChanged(FieldIdentifier)` to force `[Required]` validation on tab-past empty fields, calls virtual `OnFieldBlurred(FocusEventArgs)` hook for component-specific behavior.
- `MarkFieldAsTouched()` args-free helper extracted in Batch 8c for components without a single input element (selects with anchor + dynamic search input). Idempotent.
- Virtual extension points: `ComponentIdPrefix` (e.g., "tb", "ta", "ni", "sel"), `ComponentTypeName` for log categories, `IsEmpty` (default `CurrentValue is null`; LipiTextBox/Area override to `string.IsNullOrEmpty(CurrentValueAsString)`), `CollectParameterValidationErrors` for component-specific checks (IconRightAriaLabel-required-when-clickable, MinRows/MaxRows ordering, Min/Max ordering, Locale validity, Items-required for selects).
- Anonymous-id fallback (Production): `_cachedAnonymousId ??= AnonymousIdPrefix + Guid.NewGuid().ToString("N")[..8]`. Cached once per component lifetime. Each ComponentIdPrefix maps to its own AnonymousIdPrefix via virtual property.

**Touched-state pattern across the family.**

| Component | Trigger 1 (primary) | Trigger 2 | Trigger 3 |
|-----------|---------------------|-----------|-----------|
| LipiTextBox    | `@onblur="HandleBlur"` on `<input>` | — | — |
| LipiTextArea   | `@onblur="HandleBlur"` on `<textarea>` | — | — |
| LipiNumberInput | `@onblur="HandleBlur"` on `<input>` (also fires `OnFieldBlurred` to switch raw→formatted) | — | — |
| LipiSelect / LipiCombobox | `CloseDropdownAsync` (outside-click, Escape, Tab-with-commit) | `SelectItem` (item click or Enter on highlight) | `HandleAnchorBlur` (tab-past closed dropdown). Guard: skip if `_isOpen` (mid-interaction) |

**Pre-delivery quality check (memory rule #25).**

Adopted as standing process during Batch 8a debugging. Before declaring files shipped, self-check: (1) Razor tag balance — every `<Tag>` has matching `</Tag>` or self-closing `/>`, especially `<ValidationMessage>`, `<Virtualize>`, generic components; (2) C# brace and semicolon balance (compare `{` vs `}` counts); (3) `@using` directives present for all referenced types; (4) Cache version consistency across files; (5) Deploy script entries for new files. Strategic chat verifies the same during review.

**Issues caught in 8a debugging that informed memory #25.**

- `RequiredVisualStyle` parameter shadows the enum type of the same name in instance scope. Bare `RequiredVisualStyle.ApricotTint` resolves to the property, not the type. Fix: fully-qualify `LiPi.Web.Components.Shared.RequiredVisualStyle.ApricotTint`. The original LipiSelectBase and LipiNumberInput already had this workaround; it dropped during consolidation into LipiInputBase and was restored in Batch 8a.1.
- `Dispose(bool)` was declared `protected virtual` instead of `protected override`. CS0114 caught it. `InputBase<TValue>` already provides the virtual; we extend it. Fixed in 8a.2 along with removing duplicate `public Dispose()` and adding `base.Dispose(disposing)` call.
- LipiSelectBase's `TryParseValueFromString` signature didn't match Microsoft's nullability annotations (`[MaybeNullWhen(false)] out TValue result`, `[NotNullWhen(false)] out string? validationErrorMessage`). CS8765 caught it after enabling stricter nullability. Fixed in 8a.2.
- Razor's lexer scans CSS string literals AND CSS comments inside `<style>` blocks for component tags. Literal `<ValidationMessage>` text inside a CSS pseudo-element content string AND inside a CSS comment both triggered RZ10001 + RZ1034. Fixes: hex escape `\3CValidationMessage\3E` in CSS strings (Batch 8a.2), describe in prose without literal angle brackets in comments (Batch 8a.3). The pattern is now: never write the literal `<` followed by an identifier character anywhere inside a `<style>` block — string content, comment, or anywhere else.

**Issue gaps that became "documented behavior, not bugs" in 01.2-TextInputs.md §10.**

- §10.6 (✅ shipped): EditContext auto-population (this amendment delivers it).
- §10.7 (UX): Validation suppressed until first blur (touched-state pattern). Microsoft's `InputBase<TValue>` does NOT do this — `LipiInputBase<TValue>` adds it at the family level so all five components inherit the right UX.
- §10.8: `[EmailAddress]` regex is permissive (`abc@abc` passes). .NET behavior, not a component limitation. Callers add `[RegularExpression]` for stricter validation.
- §10.9: LipiTextBox doesn't block non-numeric input at keystroke level. Use `[RegularExpression]` for validation, or LipiNumberInput for digit-only fields. `BlockNonNumericInput` parameter on LipiTextBox is a candidate Phase 2.3 enhancement.
- §10.10: Caller-set `Error` displays but doesn't fail `EditContext.Validate()`. For submission-blocking errors, check the underlying state in `OnValidSubmit` handler. `LipiServerValidationProvider` is a candidate Phase 2.3 helper for the unified pattern.

**Files shipped across Phase 2.2.5.**

```
NEW (1):
  src/LiPi.Web/Components/Shared/LipiInputBase.cs                  (Batch 8a)
RESHIP (8):
  src/LiPi.Web/Components/Shared/LipiTextBox.razor                 (Batch 8a)
  src/LiPi.Web/Components/Shared/LipiTextArea.razor                (Batch 8b)
  src/LiPi.Web/Components/Shared/LipiNumberInput.razor             (Batch 8b)
  src/LiPi.Web/Components/Shared/LipiSelectBase.cs                 (Batch 8c)
  src/LiPi.Web/Components/Shared/LipiSelect.razor                  (Batch 8c)
  src/LiPi.Web/Components/Shared/LipiCombobox.razor                (Batch 8c)
  src/LiPi.Web/Pages/Test/TextboxTest.razor                        (Batch 8a)
  src/LiPi.Web/Pages/Test/TextareaTest.razor                       (Batch 8b)
  src/LiPi.Web/Pages/Test/NumberInputTest.razor                    (Batch 8b)
  src/LiPi.Web/Pages/Test/SelectTest.razor                         (Batch 8c)
  docs/00-COMPONENTS/01.2-TextInputs.md                            (Batch 8a, 8c)
  docs/CHANGE-LOG.md                                               (this entry)
  deploy-downloads.ps1                                             (Batch 8a)
```

**Lines of code impact.**

- LipiInputBase.cs: 0 → ~480 lines (new abstract class)
- LipiTextBox.razor: 537 → 263 lines (~274 lines removed)
- LipiTextArea.razor: 537 → 344 lines (~193 lines removed)
- LipiNumberInput.razor: 866 → 667 lines (~199 lines removed)
- LipiSelectBase.cs: 757 → ~580 lines (~177 lines removed)
- LipiSelect.razor / LipiCombobox.razor: 1 line added each (`@onblur` wiring)

Net: roughly 800 lines of duplicated code consolidated into the 480-line LipiInputBase, with the touched-state pattern + EditContext subscription added at the family level (would have been ~150 lines × 5 = 750 lines if added per-component instead).

**Verification matrix.** Each batch closed only after the corresponding test page passed user verification on the matching sub-route:

- `/test/textbox` (Batch 8a) — verified clean May 5
- `/test/textarea` + `/test/number` (Batch 8b) — verified clean May 6
- `/test/select` (Batch 8c) — pending verification at this writing

**Phase 2.2 Sub-step Status table.** The Phase 2.2 sub-step table earlier in this document marks Phase 2.2.5 as ✅ complete after Batch 8c verifies clean.

**A14 status update (post-A16).** A14 (LipiButton env-gated retrofit) trigger condition was doubly satisfied: first by A15 (Phase 2.2 components shipping the pattern), then by A16 (Phase 2.2.5 consolidating the pattern into LipiInputBase). A14 itself shipped on 2026-05-06 — same day as A16 — applying the proven env-gated pattern to the Phase 2.1 LipiButton. The numbering reflects logical ordering (A14 was always reserved for the LipiButton retrofit), even though A14's calendar date is one day later than A15. See the A14 entry above for details.

**Coordination with deploy script.** Batch 8a updated `deploy-downloads.ps1` to include `LipiInputBase.cs` and `TextboxTest.razor`. The other 4 components (LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox) and their test pages were already mapped from Phase 2.2 batches — no additional script edits needed. The deploy script does not version files, so reships overwrite cleanly.

---

### A17 — Build hygiene: zero-warning posture (MailKit + MimeKit security advisories, SharpZipLib framework restoration, redundant Components.Web reference)

**Phase**: Cross-cutting infrastructure — Batch 7
**Date**: 2026-05-06

**Changed**: `src/LiPi.Web/LiPi.Web.csproj` only. Three `<PackageReference>` edits drive the build to zero warnings. No C# code changes — neither `SmtpEmailService.cs` nor `AadhaarXmlService.cs` is touched. The chosen package versions are API-compatible with the existing call sites.

**Pre-A17 build output (4 unique warnings, doubled across restore + build phases for 8 total):**

```
warning NU1510: PackageReference Microsoft.AspNetCore.Components.Web will not be pruned.
                Consider removing this package from your dependencies, as it is likely unnecessary.
warning NU1902: Package 'MailKit' 4.7.1 has a known moderate severity vulnerability,
                https://github.com/advisories/GHSA-9j88-vvj5-vhgr
warning NU1902: Package 'MimeKit' 4.7.1 has a known moderate severity vulnerability,
                https://github.com/advisories/GHSA-g7hc-96xr-gvvx
warning NU1701: Package 'ICSharpCode.SharpZipLib 0.86.0.518' was restored using
                '.NETFramework,Version=v4.6.1, ..., .NETFramework,Version=v4.8.1'
                instead of the project target framework 'net10.0'.
                This package may not be fully compatible with your project.
```

Build succeeded but with security debt and framework-restoration mismatches that were unacceptable for the HIPAA + Six Sigma posture LiPi targets. NU1902 is the most urgent: a build that compiles but ships against a known CVE is not a build that meets the bar for clinical software.

**Post-A17 build output expectation: 0 warnings, 0 errors.**

**Edit 1 — MailKit 4.7.1 → 4.16.0 (clears NU1902 ×2)**

GHSA-9j88-vvj5-vhgr is a STARTTLS Response Injection vulnerability that allows a Man-in-the-Middle attacker to inject protocol responses across the plaintext-to-TLS trust boundary, enabling SASL authentication mechanism downgrade. The advisory affects all MailKit versions ≤ 4.12.0 when used with `SecureSocketOptions.StartTls` — which `SmtpEmailService.cs` does (it sets `SecureSocketOptions.StartTls` in its `ConnectAsync` call for Gmail/SES/SendGrid SMTP). The fix is shipped in MailKit 4.16.0.

GHSA-g7hc-96xr-gvvx is a parallel advisory in MimeKit 4.7.1 (transitive dependency of MailKit). Upgrading MailKit to 4.16.0 pulls in a non-vulnerable MimeKit transitively, clearing both NU1902 warnings simultaneously.

API compatibility: MailKit 4.x is a stable major-version line; the 4.7.1 → 4.16.0 jump is a minor-version progression with no breaking changes to `SmtpClient`, `MailKit.Security.SecureSocketOptions`, `MimeKit.MimeMessage`, `MimeKit.MailboxAddress`, or `MimeKit.TextPart` — all the types `SmtpEmailService.cs` consumes. The `client.ConnectAsync(host, port, SecureSocketOptions.StartTls)` / `AuthenticateAsync(user, pass)` / `SendAsync(message)` / `DisconnectAsync(true)` flow is unchanged. No code edit needed.

**Edit 2 — `ICSharpCode.SharpZipLib 0.86.0.518` → `SharpZipLib 1.4.2` (clears NU1701)**

This edit is a **package id rename plus a version bump**, not a simple version upgrade. The currently-pinned package id `ICSharpCode.SharpZipLib` is the legacy 2010-era package on NuGet (last update 2010/05/25). The currently-maintained package on NuGet.org is published under the simpler id `SharpZipLib` (no `ICSharpCode.` prefix). The 0.86.0.518 pin is the source of NU1701 because that package only ships .NET Framework v4.x assemblies, and NuGet falls back to net4.x compatibility when restoring into a net10.0 project — which works at runtime by accident but is not a posture LiPi can defend in audit.

`SharpZipLib 1.4.2` (released 2023-01-30) ships .NETStandard 2.0 + 2.1 assemblies, which restore cleanly into net10.0 and remove NU1701. The C# `namespace ICSharpCode.SharpZipLib.Zip`, the `ZipFile` class, the `ZipEntry` enumeration, the `Password` property, the `GetInputStream(ZipEntry)` method, the `Close()` method, and the `ZipException` type are all unchanged from 0.86.0.518 to 1.4.2 — the namespace stayed `ICSharpCode.SharpZipLib.*` even after the package id changed. `AadhaarXmlService.cs` continues to compile and run unchanged: its `using ICSharpCode.SharpZipLib.Zip;` directive plus `var zipFile = new ZipFile(ms); zipFile.Password = shareCode;` pattern is forward-compatible.

**Why not BCL replacement (`System.IO.Compression.ZipArchive`)?**

UIDAI Offline Aadhaar ZIPs are password-protected (4-digit share code as ZIP password). `System.IO.Compression.ZipArchive` does not support password-protected ZIPs — period. `AadhaarXmlService.cs` requires password-protected ZIP support, so dropping SharpZipLib entirely would require a different third-party library or implementing PKZIP 2.0 password decryption from scratch. Neither alternative is better than the SharpZipLib upgrade. Decision: keep SharpZipLib, upgrade to the maintained 1.4.2 line, get out of the .NET Framework restoration trap, leave the call site untouched.

**Edit 3 — Remove `<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="10.0.0" />` (clears NU1510)**

`Microsoft.AspNetCore.Components.Web` is included transitively by the `Microsoft.NET.Sdk.Web` SDK (which the project's `<Project Sdk="Microsoft.NET.Sdk.Web">` declaration pulls in). The explicit `<PackageReference>` was redundant — NuGet's `prune` analysis flagged it as such. Removing the explicit reference does not change which assemblies end up in the build; it just removes the redundant declaration. NU1510 clears.

**Strategic context (memory rule #26 — infrastructure phases trigger on real demand)**

A17 is interim hygiene, not the final email or compression architecture for LiPi HIS. The strategic plan documented as of 2026-05-06:

- **LipiMail service** — A future infrastructure phase will introduce a dedicated `LipiMail` application service that wraps the SMTP backend (likely MailKit, possibly a SaaS provider like SendGrid or AWS SES) with the application-layer concerns LiPi will need at scale: queueing, templating, audit logging, multi-tenant SMTP config (per-clinic FROM addresses), retry / circuit breaker, and delivery tracking for HIPAA audit. **Trigger**: first module needing structured email beyond the OTP / password-reset use case (likely Lab or Radiology report delivery).

- **LipiHttp + LipiApi services** — Parallel future infrastructure phase for outbound HTTP integrations (ABDM consent flows, DigiLocker Aadhaar verification, lab equipment HL7 endpoints, SMS providers, payment gateways). Will wrap `HttpClient` with the same application-layer concerns (auth handling, retry, audit, circuit breaker). **Trigger**: first external integration shipping (likely ABDM or DigiLocker, both queued behind Patient Module completion).

Neither of these is in scope for v1.0. They are deliberate infrastructure phases triggered by real module requirements, not speculative inline work. A17 just keeps the existing thin SMTP usage on a non-vulnerable MailKit version until LipiMail replaces it.

**Out-of-scope items mentioned in the original Batch 7 authorization but no longer in the current build output**

The original Batch 7 plan referenced `Clinics.razor` line 273 CS8601, `PatientNew.razor` line 951 `_permPinLooking` CS0414, and `PatientNew.razor` line 1009 `PhotoConsent` CS0649. The May 6 `dotnet build` output captured immediately before this ship contained no CS8601, CS0414, or CS0649 warnings anywhere. These were either resolved by prior work that landed between the original Batch 7 scoping and today, or they never existed in the current codebase under net10.0 nullable analysis. Either way, A17 ships only what the current build actually flags. If any of these warnings reappear in future builds, they'll be addressed in a focused follow-up.

**Files**:

- `src/LiPi.Web/LiPi.Web.csproj` — three edits as detailed above.
- `docs/CHANGE-LOG.md` — this entry.

**No other files changed.** No `SmtpEmailService.cs` rewrite. No `AadhaarXmlService.cs` adjustment. No `Clinics.razor` or `PatientNew.razor` touch. No deploy-script edit (csproj already mapped from Phase 1; CHANGE-LOG already mapped from Phase 2). No cache version bump (no static-asset changes).

**Verification recipe**:

1. `dotnet restore src/LiPi.Web/LiPi.Web.csproj` — should succeed with 0 warnings (was: 4 warnings)
2. `dotnet build src/LiPi.Web/LiPi.Web.csproj` — should succeed with 0 warnings (was: 8 warnings doubled across restore + build)
3. Manual smoke test of the forgot-password flow (MailKit 4.16.0 is a minor-version progression but the security advisory means the streaming path internals changed — quick smoke confirms the runtime path still works):
   - `/forgot-password` → enter test username → "Send OTP" → confirm OTP email arrives in the test inbox
   - `/verify-otp/{userId}` → enter the 6-digit OTP → confirm redirect to `/reset-password/{token}`
   - `/reset-password/{token}` → set new password → confirm "Password Changed" email arrives → confirm subsequent login with new password works
4. If a production-equivalent Aadhaar XML test fixture is available, run `AadhaarXmlService.ParseAsync` against it to confirm SharpZipLib 1.4.2's `ZipFile(ms) + Password = shareCode + GetInputStream(entry)` path still extracts and decrypts the embedded XML. If no fixture is available, the build success against the unchanged call site is sufficient evidence — the API surface didn't move.
5. `/styleguide` page renders (no LipiButton or input-component regression — these don't depend on MailKit or SharpZipLib, but spot-checking confirms nothing was disturbed).
6. All four `/test/*` pages still pass verification (textbox / textarea / number / select).

**Coordination note**: A17 closes out v1.0 build hygiene. Future warning regressions will be tracked as separate amendments rather than rolled into this entry. The `LipiMail` and `LipiHttp` / `LipiApi` infrastructure phases mentioned above are recorded in v1.1 PLANNED below (added in this amendment) so they don't get inlined into feature work by future chats.

**Memory rule #26 reference**: This amendment is the first concrete application of the infrastructure-phases-trigger-on-real-demand principle. Future chats reviewing this entry should treat it as the canonical example of "interim hygiene now, dedicated phase when triggered" rather than rewriting `SmtpEmailService.cs` opportunistically.

**Post-A17 update (see A18 below)**: A17's "closes out v1.0 build hygiene" claim turned out to be premature. With the four package-level warnings (NU1510, NU1902 ×2, NU1701) cleared, four C# warnings (CS0108, CS8601, CS0414, CS0649) that had been masked by — or simply not surfaced alongside — the package warnings became visible in the post-A17 build. A18 cleans those up and is the actual close-out of v1.0 build hygiene.

---

### A18 — Build hygiene part 2: C# warnings revealed by A17 (CS0108 + CS8601 + CS0414 + CS0649)

**Phase**: Cross-cutting infrastructure — Batch 7.1
**Date**: 2026-05-06

**Changed**: Four C# warnings cleared across three source files. No package changes (those landed in A17). No CHANGE-LOG-related logic — this is a follow-up cleanup batch.

**Pre-A18 build output (4 unique warnings, all C#):**

```
warning CS0108: 'LipiTextArea.StateCssToken(InputState)' hides inherited member
                'LipiInputBase<string?>.StateCssToken(InputState)'.
                Use the new keyword if hiding was intended.
                — src/LiPi.Web/Components/Shared/LipiTextArea.razor(304,27)

warning CS8601: Possible null reference assignment.
                — src/LiPi.Web/Pages/Admin/Clinics.razor(273,27)

warning CS0414: The field 'PatientNew._permPinLooking' is assigned but its value is never used
                — src/LiPi.Web/Pages/Patients/PatientNew.razor(951,20)

warning CS0649: Field 'PatientNew.PM.PhotoConsent' is never assigned to,
                and will always have its default value false
                — src/LiPi.Web/Pages/Patients/PatientNew.razor(1009,80)
```

Each warning got its own root-cause investigation rather than blanket pragma suppression. The root causes turned out to be more interesting than expected — three of the four warnings led to genuine code issues (a missed Phase 2.2.5 migration cleanup, a nullable-flow type mismatch, and a Razor parser edge case), and only one (PhotoConsent) was a true dead-field cleanup.

**Post-A18 expectation**: 0 warnings, 0 errors.

**Fix 1 — LipiTextArea.razor CS0108 (inheritance shadow)**

Phase 2.2.5 Batch 8b migrated LipiTextArea to inherit from `LipiInputBase<string?>`. The base class provides `protected static string StateCssToken(InputState s)`. The migration successfully removed the duplicated `ResolvedState` property, `IsEmpty`, `EffectiveRequiredStyle`, and several other base-provided members from LipiTextArea — but missed the `StateCssToken` static helper. The subclass kept its own `private static string StateCssToken(InputState s) => ...` with byte-identical body, silently hiding the inherited method.

CS0108 fired exactly to flag this: "are you intentionally shadowing the base member, or did you forget to remove a duplicate?" In this case, forgot to remove a duplicate.

The fix: delete the shadowed `StateCssToken` method (12 lines) from LipiTextArea.razor. The single call site at line 283 (`StateCssToken(ResolvedState)`) now resolves to the inherited base method, which has identical behavior.

**This is the canonical example for memory rule #25 bullet 6** (added in this amendment): after migrating a subclass to a new base class, grep the subclass for any private/protected member whose name exists on the base. CS0108 is deterministic — it fires on every shadow, even when bodies are identical. The Phase 2.2.5 ship missed this; the new bullet 6 prevents future repeats.

Verified during this batch: LipiNumberInput, LipiSelectBase, LipiSelect, LipiCombobox, and LipiTextBox have NO lingering shadows of inherited base members. Only LipiTextArea was affected.

**Fix 2 — Clinics.razor CS8601 (nullable string return on `Nullable<Guid>.ToString()`)**

The flagged line:

```csharp
_orgModalSelectedId = c.OrganizationId.HasValue && c.OrganizationId != Guid.Empty
                      ? c.OrganizationId.ToString()
                      : string.Empty;
```

`_orgModalSelectedId` is declared `private string` (non-nullable). The conditional expression's `c.OrganizationId.ToString()` branch calls `Nullable<Guid>.ToString()`, which is annotated as returning `string?` in the .NET 10 BCL — even though `Nullable<T>.ToString()` semantically returns empty string when null and never genuinely returns null at runtime. The compiler's nullable-flow analysis sees `string?` flowing into a non-nullable target and fires CS8601.

The fix: call `.ToString()` on the unwrapped Guid instead of on the `Nullable<Guid>` wrapper:

```csharp
_orgModalSelectedId = c.OrganizationId.HasValue && c.OrganizationId != Guid.Empty
                      ? c.OrganizationId.Value.ToString()
                      : string.Empty;
```

`Guid.ToString()` returns `string` (non-nullable). `c.OrganizationId.Value` is safe because the same expression already guards on `.HasValue`. No null risk, no operator suppression, no behavior change.

**Fix 3 — PatientNew.razor CS0414 (Razor parser edge case: `@if` glued to text)**

The flagged field declaration:

```csharp
private bool _permPinLooking=false, _permPinAutoFilled=false;
```

`_permPinLooking` is read in markup at line 466 (the permanent address Pincode label):

```razor
<label class="reg-lbl" for="pn-ppin">Pincode@if(_permPinLooking){...}else if(_permPinAutoFilled){...}</label>
```

Yet CS0414 fires on `_permPinLooking` while NOT firing on `_permPinAutoFilled` — same field declaration line, same markup conditional structure. Investigation revealed the smoking gun: compare with the working sibling at line 401 (current address Pincode label):

```razor
<label class="reg-lbl" for="pn-pin">Pincode <span class="reg-req">*</span>@if(_pinLooking){...}else if(_pinAutoFilled){...}</label>
```

Line 401: `Pincode <span ...>` BEFORE `@if`. Whitespace and an HTML element separate text content from the code transition. No CS0414 for `_pinLooking`.

Line 466: `Pincode@if` BEFORE the fix. No whitespace, no intervening element. CS0414 fires.

`_permPinAutoFilled` doesn't fire CS0414 because it's also read at lines 478 and 485 (in `placeholder` attributes within proper attribute-context Razor expressions, not glued-to-text-content `@if` chains). Only `_permPinLooking`'s read is exclusively in the glued-to-text position, and the Razor parser is failing to emit it as a code reference in the generated C#.

The fix: insert a single space between `Pincode` and `@if`:

```razor
<label class="reg-lbl" for="pn-ppin">Pincode @if(_permPinLooking){...}else if(_permPinAutoFilled){...}</label>
```

This addresses the root cause (Razor parser edge case with text-glued code transitions) rather than masking it with a `#pragma warning disable CS0414`. The visible output is identical to the user — a single non-significant whitespace character before the conditional content. The "looking up..." UX feedback during permanent-address Pincode lookups continues to work (the markup read is now visible to the compiler).

Lesson for future Razor authoring: keep at least one whitespace character or HTML element between literal text and `@if` / `@foreach` / similar code transitions. The codebase's existing pattern (line 401) already follows this implicitly; line 466 was the outlier.

**Fix 4 — PatientNew.razor CS0649 (orphaned PhotoConsent field)**

The flagged declaration on line 1009:

```csharp
public bool VipFlag,Allergy,OrganDonor,Research,Intl,HipaaConsent=true,PhotoConsent;
```

`PhotoConsent` is declared but never assigned anywhere in the file. Searched all occurrences (markup + code) — single match (the declaration). The consent panel markup uses `m.HipaaConsent` for HIPAA / Privacy consent; there is no separate Photo Consent UI element.

The fix: remove the `,PhotoConsent` token from the multi-field declaration, leaving:

```csharp
public bool VipFlag,Allergy,OrganDonor,Research,Intl,HipaaConsent=true;
```

True dead-field cleanup. If a Photo Consent feature is desired in the future (separate from HIPAA consent), it would need a fresh declaration plus matching UI markup; reusing this stub would have been a footgun.

**Files**:

- `src/LiPi.Web/Components/Shared/LipiTextArea.razor` — Fix 1 (delete shadowed `StateCssToken`, lines 304-313)
- `src/LiPi.Web/Pages/Admin/Clinics.razor` — Fix 2 (`.ToString()` → `.Value.ToString()` at line 273)
- `src/LiPi.Web/Pages/Patients/PatientNew.razor` — Fix 3 (insert space at line 466) + Fix 4 (remove `PhotoConsent` from line 1009)
- `docs/CHANGE-LOG.md` — this entry

**No other files changed.** No deploy-script edit (all files already mapped). No cache version bump (Razor recompiles automatically).

**Memory rule #25 amendment — bullet 6 added (Inheritance shadowing check)**

The Phase 2.2.5 LipiTextArea CS0108 regression motivates an addition to memory rule #25. The pre-delivery quality check now reads:

> 1. Razor tag balance — every `<Tag>` has matching `</Tag>` or self-closing `/>`
> 2. C# brace balance (compare `{` vs `}` counts)
> 3. `@using` directives present for all referenced types
> 4. Cache version consistency across files
> 5. Deploy script entries for new files
> 6. **(NEW)** Inheritance shadowing check — after migrating any subclass to a new base class, grep the subclass for any private/protected member whose name exists on the base. CS0108 fires when a subclass member silently hides a base member with the same signature, indicating either a missed delete (the right call when migrating) or a needed `new` keyword (rare). The Phase 2.2.5 LipiTextArea CS0108 should have been caught by this check at Batch 8b ship time.

The user has confirmed adoption of bullet 6 and will handle the formal memory edit separately. This amendment documents the rationale.

**Verification recipe**:

1. `dotnet build src/LiPi.Web/LiPi.Web.csproj` → expect 0 warnings, 0 errors (was 4 C# warnings post-A17)
2. `/test/textarea` smoke test — confirms LipiTextArea's autogrow, validation, and helper slot still work after StateCssToken delete (the inherited base method has identical behavior, but a render check confirms)
3. `/admin/clinics` → click "Change Org" on any clinic → confirm modal opens with current org pre-selected → confirm save flows correctly (Fix 2 didn't break the OrganizationId display logic)
4. `/patients/new` → tab to permanent address → tick "Enter separately" → enter a 6-digit PIN → confirm "looking up..." indicator shows during the API call and "✓ auto-filled" shows after (Fix 3 keeps `_permPinLooking` markup read functional)
5. `/patients/new` → confirm consent panel still renders (Fix 4 removed orphan field, no UI impact expected)
6. `/styleguide` and all four `/test/*` pages still pass verification

**Coordination note**: A18 is the actual close-out of v1.0 build hygiene. The 4-warnings-clear-then-4-different-warnings-appear pattern from A17 → A18 is a useful lesson: package-level warnings (NU####) and C# warnings (CS####) are surfaced through different toolchain phases; clearing all NU#### in one batch can reveal CS#### that were not visible in the original output. Future build-hygiene amendments should run `dotnet build` between each batch of related warning fixes rather than batching too aggressively.

**Phase 2 status (post-A18)**:

- ✅ Phase 2.0 (StyleGuide foundation)
- ✅ Phase 2.1 (LipiButton + tokens) — A11 visual review, A14 env-gated retrofit
- ✅ Phase 2.2 (5-component TextInput family) — A12 token foundation, A13 base infrastructure, A15 component completion
- ✅ Phase 2.2.5 (LipiInputBase + EditContext auto-population) — A16, tagged `phase-2-sub-2-2-5-may06`
- ✅ Build hygiene closed — A17 (package warnings) + A18 (C# warnings)
- 🟢 Phase 2.3 ready to start (`LipiSelectTextCompound`, `LipiMultiSelect`, `LipiCheckbox`, `LipiRadio`, `LipiToggle`)

## v1.0 — AMENDMENTS (May 7, 2026)

> A19 documents Phase 2.3 — the Compound + Multi-Select component family.
> Three feature ships (9a Compound, 9b Multi-Select, 9c Multi-Combobox + StyleGuide)
> plus six fix sub-batches woven through them. Phase 2.3 closes here.

### A19 — Phase 2.3 close: Compound + Multi-Select family complete

**Phase**: 2 Sub-step 2.3 — Compound + Multi-Select Component Family
**Date**: 2026-05-06 to 2026-05-07
**Status**: ✅ DEPLOYED, tagged `phase-2-sub-2-3-may07`

**Added (feature batches)**:

Three feature batches and six interleaved fix sub-batches over two days. Components
shipped: 4 user-facing (`LipiCompoundField`, `SelectSegment`, `TextSegment`,
`LipiMultiSelect`, `LipiMultiCombobox`) plus 2 base classes (`LipiContainerBase`,
`LipiMultiSelectBase`) and 1 segment contract (`ICompoundSegment`).

- **Batch 9a — Compound field family** (May 6). Introduced `LipiContainerBase`
  as a sibling base to `LipiInputBase` for components that hold child segments
  rather than a single bound value. `LipiCompoundField` is the first concrete
  consumer; `SelectSegment` and `TextSegment` are the first segment types. The
  three demo scenarios (Mobile = ISD select + 10-digit number, Landline =
  ISD select + STD select + 8-digit number, Aadhaar = 4-4-4 text segments)
  validated the Q1=C/Q2=Z/Q3=C-C1/Q4=C/Q5=Y locked decisions from the
  strategic design package.

- **Batch 9b — Multi-select family** (May 6/7). `LipiMultiSelectBase<TValue, TItem>`
  abstract base shipped with `LipiMultiSelect<TValue>` as the identity-typed
  concrete subclass. Implements all six locked architectural decisions
  (Q1 min-height growth, Q2 +M more dropdown sort, Q3 Backspace-removes-last,
  Q4 env-gated AllowFreeText constraint, Q5 strict IsEmpty, Q6 keyboard MVP
  scope including Spacebar toggle and type-ahead). Pattern B replacement
  semantics for all `List<TValue>` mutations.

- **Batch 9c-a — Multi-Combobox + StyleGuide** (May 7). `LipiMultiCombobox<TValue, TItem>`
  templated multi-select inheriting `LipiMultiSelectBase`. Caller provides
  `ValueSelector` / `LabelSelector` parameters plus optional `ItemTemplate`
  (rich dropdown rendering) and `SelectedChipTemplate` (rich chip rendering).
  StyleGuide gained a "Compound & Multi-Select" section with three demo
  subsections; the `MultiComboboxTest.razor` page exercises 5 templated
  scenarios (diagnoses with severity badges, medications with dose info, lab
  tests with conditional STAT badge, specialties with emoji icons,
  string-typed allergens with caps interaction). One follow-up polish ship
  (9c-a.1) added scoped `min-width` rules to align dropdown name columns
  across rows of varying-length codes.

- **Batch 9c-b — Spec docs + this CHANGE-LOG entry** (May 7). New spec docs
  `01.3-CompoundField.md` and `01.4-MultiSelect.md` documenting architecture,
  parameter surfaces, and locked decisions. Phase 2.3 closes with this
  amendment.

**Changed (architecture)**:

- **CSS family-level height rules promoted to `lipi-inputs.css`** (Batch 9a.1
  CSS sub-batch). Prior to Phase 2.3, the `.lipi-input-{size} .lipi-input-field`
  height rules were duplicated across per-component scoped CSS files
  (`LipiTextBox.razor.css`, `LipiNumberInput.razor.css`) and embedded as
  compound-class rules in `lipi-inputs.css` for `LipiSelect`. Each new
  component had to remember to either re-emit the height rules or rely on a
  compound-class match. This caused the Batch 5.4 LipiSelect 17.6px
  regression and the Batch 9a `LipiCompoundField` 18.4px regression. The
  fix promoted the three height rules (28/32/40 px) to truly family-level
  shared rules keyed only on `.lipi-input-{size} .lipi-input-field`.
  `LipiTextArea` (`min-height` for autogrow) is the first intentional
  family override; `LipiMultiSelect` (`min-height` for chip-row growth) is
  the second.

- **Memory rule #25 amended — bullet 7 added** (CSS class scaffolding check).
  The new bullet reads:

  > 7. Shared-CSS class scaffolding — When creating a new component that
  > should inherit family-level visual rules from shared CSS, verify that
  > (a) the wrapper emits the same family classes existing components use,
  > (b) family-level rules live in shared CSS keyed on family classes, not
  > duplicated in per-component scoped files, and (c) per-component scoped
  > CSS contains only genuine deltas. The Batch 5.4 LipiSelect 17.6px
  > regression and the Batch 9a LipiCompoundField 18.4px regression are
  > both examples of this bullet's failure mode.

  The user has confirmed adoption of bullet 7 and handles the formal memory
  edit separately. This amendment documents the rationale.

**Fixed (sub-batches)**:

Each feature batch surfaced issues during verification that were addressed
in dedicated fix ships rather than rolled into the next feature batch. This
preserved a clean per-batch verification gate.

- **9a.1 — 5-error fix + CSS architectural cleanup** (combined ship after
  Batch 9a). The C# fixes addressed five build errors discovered during
  Batch 9a's first verification: (1+2) `FocusEventArgs.RelatedTarget` does
  not exist in Blazor — moved focusout listener to JS side via
  `lipiCompound.attachFocusOut` / `detachFocusOut` with
  `DotNetObjectReference` callback into `[JSInvokable] OnFocusLeftCompound`.
  (3) `HasError` was an explicit interface implementation
  (`bool ICompoundSegment.HasError`) and inaccessible via `this`-context
  Razor markup; replaced inline check with `ResolvedState == InputState.Error`.
  (4) `LipiButton` `Variant="Primary"` should have been
  `Variant="ButtonVariant.Primary"`. (5) Cascade of #4 — generated lambda
  return type mismatch resolved automatically once #4 was fixed. Plus 16
  RZ2012 warnings (missing `[EditorRequired]` Name/Label on segments)
  cleared by adding kebab-case Name and descriptive Label to all 8 segment
  call-sites in `CompoundFieldTest.razor`.

  The CSS architectural cleanup (described above under Changed/architecture)
  shipped at the same time and shares the 9a.1 label.

- **9b.1 — CS0108 + CS8629 cleanup** (after Batch 9b initial verification).
  CS0108: my new `LipiMultiSelectBase.Placeholder` parameter declaration
  shadowed the inherited `LipiInputBase<List<TValue>>.Placeholder`. Memory
  rule #25 bullet 6 (added in A18) caught exactly this regression class —
  the rule was framed for "migrating any subclass to a new base class," but
  the same risk applies when AUTHORING a NEW subclass against an existing
  base. Bullet 6's wording should be (and is now in practice) interpreted
  to cover both scenarios. Fix: deleted the redundant declaration; the
  inherited `Placeholder` has byte-identical signature.

  CS8629: `MaxSelections.Value` access inside a `@if (capReached)` Razor
  branch — compiler nullable-flow analysis can't see that `capReached`
  (defined in the base) implies `MaxSelections.HasValue`. Fix: replaced
  `@MaxSelections.Value` with `@MaxSelections` (Razor int? interpolation
  renders the inner value when non-null, which is guaranteed inside the
  guard).

- **9b.2 — 6-issue UX fix** (after Batch 9b functional verification).
  Two root causes for six reported issues:
  (A) Keydown event propagation — `@onkeydown` was bound to BOTH the outer
  field-bar div AND the inner search input. Backspace and ArrowUp/Down
  fired both handlers, removing 2 chips at once or jumping 2 items in
  highlight. Fix: added `@onkeydown:stopPropagation="true"` on the search
  input.
  (B) JS interop signature mismatch — I called
  `lipiInput.attachSelectHandlers / detachSelectHandlers / positionDropdown`
  with only 2 arguments. `LipiSelectBase` calls all three with a third
  argument `EffectiveDropdownId`. Without it, JS could not (3) detect
  outside-clicks against the panel, (4) reposition the panel on scroll, or
  (5) flip the panel above the anchor when below-anchor space was
  insufficient. Fix: added `EffectiveDropdownId` as the third argument to
  all four JS interop call sites.

  This batch crystallized a Path 3 lesson: when reusing a JS interop API
  from a sibling base class, the call signature must match exactly. The
  Path 3 discipline of "duplicate the markup verbatim" extends to the C#
  glue code that drives the JS, not just the Razor template.

- **9b.3 — Scroll-highlighted-option-into-view fix** (after Batch 9b
  Specialties scenario test). Type-ahead and arrow-key keyboard
  navigation correctly updated `_highlightedIndex` in C#, but the matching
  option DOM element remained outside the dropdown's visible scroll region
  when the matching item was below visible. User saw no feedback even
  though the highlight had moved correctly internally. Fix: added
  `lipiInput.scrollOptionIntoView(panelId, index)` JS helper using
  `scrollIntoView({block: 'nearest'})` on the option element identified by
  a new `data-option-index="N"` attribute on each rendered option. C# calls
  the helper from `ScrollHighlightIntoViewAsync` after every keyboard-driven
  highlight change (3 sites: ArrowDown, ArrowUp, type-ahead match). The
  same gap exists latently in `LipiSelectBase` and is moved to the v1.1
  PLANNED section below; deferred because LipiSelect's `Searchable=true`
  default sidesteps the symptom for most use cases.

- **9c-a.1 — Dropdown name-column alignment polish** (after Batch 9c-a
  Diagnoses scenario test). ICD codes have varying length (`I10`, `J45.909`)
  so diagnosis names started at different X positions per dropdown row.
  Fix: added scoped `min-width: 60px` on `.dx-row .dx-icd` and
  `min-width: 56px` on `.lab-row .lab-code` (parent-scoped so chip context
  is unaffected — chips legitimately want to hug content). Fix lives in
  `MultiComboboxTest.razor` and `StyleGuide.razor` only; not promoted to
  shared CSS until 3+ consumer pattern repeats.

**Architectural decisions locked**:

Six strategic and six implementation-level decisions shape the architectural
footprint of Phase 2.3. Future strategic chats reading this amendment should
treat these as locked unless explicitly amended in a future entry.

- **Track 4b / Approach C — Generic `LipiCompoundField` over per-shape components**.
  Rejected: per-shape components (`LipiMobileField`, `LipiAadhaarField`,
  `LipiLandlineField`). Chosen: one generic `LipiCompoundField` that hosts
  child segments (`SelectSegment`, `TextSegment`, future `DateSegment`).
  Rationale: arbitrary multi-segment shapes are unbounded (every form's
  compound is unique), and per-shape components multiply the code surface
  without proportional benefit. The segment registration pattern via
  `ICompoundSegment` provides the consistency that per-shape components
  would otherwise enforce.

- **`LipiContainerBase` as sibling base to `LipiInputBase`, not subclass or refactor**.
  Rejected: (Option B) refactor `LipiInputBase` to add container-shape support
  via composition. (Option C) make `LipiContainerBase` inherit from
  `LipiInputBase`. Chosen: introduce `LipiContainerBase` as a NEW abstract
  sibling that takes no `TValue` and owns segment registration / focus-out
  aggregation / visual scaffolding. Rationale: the two responsibilities
  (single bound value vs. hosting child segments) are orthogonal; trying to
  share inheritance would force every input to carry container ceremony. The
  sibling-base shape preserves clean boundaries for both.

- **Path 3 — Declarative duplication over imperative `RenderFragment` extraction at 2-caller scale**.
  When `SelectSegment` (Batch 9a) needed the same dropdown panel markup as
  `LipiSelect`, the team considered three paths:
  (Path 1) Extract dropdown rendering into a virtual method on
  `LipiSelectBase` returning `RenderFragment`.
  (Path 2) Move dropdown into a separate component imported by both.
  (Path 3) Duplicate the dropdown panel markup verbatim in `SelectSegment`,
  ~80 lines of intentional copy.
  Chosen: Path 3. Rationale at 2-caller scale: extraction adds an indirection
  layer that obscures debugging (renders happen in a method returning a
  delegate, not in the `.razor` markup the developer is editing), and
  templates iterating independently per consumer is more important than DRY.
  When `LipiMultiCombobox` (Batch 9c-a) became the second consumer of the
  multi-select dropdown panel, Path 3 was reapplied — total panels duplicated
  in Phase 2.3: 2 (`SelectSegment` from `LipiSelect`, `LipiMultiCombobox` from
  `LipiMultiSelect`). **Trigger to extract: the third consumer of any panel
  shape.** Until then, declarative duplication is the locked default.

- **CSS Path A — `min-height` override as the family-level deviation pattern**.
  When components legitimately need to deviate from family-level fixed
  heights (28/32/40 px), the locked pattern is to use a compound-class
  selector (`.lipi-multi-select.lipi-input-{size} .lipi-input-field`) that
  sets `height: auto; min-height: {size}`. `LipiTextArea` (autogrow)
  established this in Phase 2.2 Batch 3; `LipiMultiSelect` (chip-row growth)
  is the second case. Rejected alternatives: (A2) constrain content to fit
  fixed height with overflow handling, (A3) promote `min-height` to the
  family-level rule and refactor all components. A3 is a Phase 3 candidate;
  A2 was rejected for chips because forced single-line overflow degrades
  UX more than letting the field grow naturally. Document any future
  third-time-override as a Phase 3 trigger to revisit A3.

- **Pattern B — Replacement, not in-place mutation, for `List<TValue>` binding**.
  All `Value` mutations in `LipiMultiSelectBase` (`AddItem`, `RemoveItem`,
  `RemoveLastChip`, `ToggleItem`, `CommitFreeTextAsync`) create a new
  `List<TValue>` instance and assign to `CurrentValue`. Rejected alternative
  (Pattern A): in-place `Value.RemoveAt(idx)` followed by
  `ValueChanged.InvokeAsync(Value)`. Rationale: in-place mutation produces
  the same reference, breaking reference-equality change detection in
  `EditContext`, FluentValidation, EF Core change tracking, and any future
  migration to `ImmutableList<T>` or record-based view models. Cost is one
  list allocation per chip add/remove; user-paced operations make this
  negligible. Going forward, any new collection-typed bound parameter
  (`List<T>`, `HashSet<T>`, `Dictionary<K,V>`) follows Pattern B by default.

- **Q4 — Env-gated `TValue=string` constraint for `AllowFreeText`** (mirrors LipiButton A14).
  `LipiMultiSelect.AllowFreeText` and `LipiMultiCombobox.AllowFreeText`
  require `TValue=string` because free-text input synthesizes new values
  from typed strings — non-string TValue (e.g., `Guid`, `int`) cannot be
  synthesized this way. Rejected alternatives: (II) add a
  `Func<string, TValue?>` parser parameter, (III) silently skip the
  "Add as new" UI when `TValue ≠ string`. Chosen: (I) env-gated
  `InvalidOperationException` in Development, log error + force-false in
  Production. Mirrors the LipiButton AriaLabel pattern locked in A14.
  Rationale: clinical data binding to entity IDs (Guid medications, Guid
  diagnoses) should NOT support arbitrary user-typed values — those are
  data-quality risks. The constraint pushes free-text use cases to
  `TValue=string` properties where the string itself is the storage shape
  (allergies, tags, free-form notes lists).

**Q1-Q6 implementation locks (multi-select family)**:

| Q | Decision | Implementation |
|---|----------|----------------|
| Q1 | min-height override | `.lipi-multi-select.lipi-input-{size} .lipi-input-field` sets `height: auto; min-height: {28,32,40}px`. Field grows when chips wrap to multiple rows. |
| Q2 | +M more opens dropdown sorted | Click "+M more" sets `_isFromSummaryClick = true`; `GetFilteredOptions` returns `(Selected: alphabetical, Unselected: alphabetical)`. Reset on `CloseDropdownAsync`. |
| Q3 | Backspace removes Value last | `RemoveLastChip` calls `Value.RemoveAt(Value.Count - 1)` via Pattern B replacement. List order, not visual order. |
| Q4 | AllowFreeText requires string | Env-gated check in `OnParametersSet`; `_resolvedAllowFreeText` field used in rendering instead of raw parameter. |
| Q5 | Strict IsEmpty | `protected override bool IsEmpty => CurrentValue is null \|\| CurrentValue.Count == 0` — apricot tint reappears when emptied even after first interaction. |
| Q6 | MVP + Spacebar + Type-ahead | Backspace, ArrowUp/Down, Enter/Space toggle, Esc close, Tab native; type-ahead buffer with 1-second reset window for non-searchable mode. |

**Files**:

Across 9a → 9c-b: 11 NEW files, 8 EDITED files, 1 EDITED config (`deploy-downloads.ps1`,
multiple times).

NEW (Components):
- `src/LiPi.Web/Components/Shared/ICompoundSegment.cs`
- `src/LiPi.Web/Components/Shared/LipiContainerBase.cs`
- `src/LiPi.Web/Components/Shared/LipiCompoundField.razor`
- `src/LiPi.Web/Components/Shared/SelectSegment.razor`
- `src/LiPi.Web/Components/Shared/TextSegment.razor`
- `src/LiPi.Web/Components/Shared/LipiMultiSelectBase.cs`
- `src/LiPi.Web/Components/Shared/LipiMultiSelect.razor`
- `src/LiPi.Web/Components/Shared/LipiMultiCombobox.razor`

NEW (CSS):
- `src/LiPi.Web/wwwroot/css/lipi-compound.css`
- `src/LiPi.Web/wwwroot/css/lipi-multi.css`

NEW (Test pages):
- `src/LiPi.Web/Pages/Test/CompoundFieldTest.razor`
- `src/LiPi.Web/Pages/Test/MultiSelectTest.razor`
- `src/LiPi.Web/Pages/Test/MultiComboboxTest.razor`

NEW (Docs):
- `docs/00-COMPONENTS/01.3-CompoundField.md`
- `docs/00-COMPONENTS/01.4-MultiSelect.md`

EDITED:
- `src/LiPi.Web/wwwroot/css/lipi-inputs.css` (CSS architectural cleanup, family-level heights)
- `src/LiPi.Web/wwwroot/js/lipi-input.js` (extended with `lipiCompound.attachFocusOut` / `detachFocusOut` + `scrollOptionIntoView`)
- `src/LiPi.Web/Components/Shared/LipiTextBox.razor.css` (3 height rules removed; promoted to shared)
- `src/LiPi.Web/Components/Shared/LipiNumberInput.razor.css` (3 height rules removed; promoted to shared)
- `src/LiPi.Web/Components/Layout/App.razor` (cache version bumps `20260510 → 20260514` across 9a → 9b.3 + new CSS link entries)
- `src/LiPi.Web/Pages/StyleGuide.razor` (new "Compound & Multi-Select" section, updated stale Phase 2.2.5 comment)
- `deploy-downloads.ps1` (8 new file entries across all sub-batches)

**Verification**:

1. `dotnet build src/LiPi.Web/LiPi.Web.csproj` → 0 warnings, 0 errors
2. `/test/compound` — three scenarios (Mobile, Landline, Aadhaar) with auto-advance, focus-leaves-compound touched-state, and per-segment validation aggregation all working
3. `/test/multi` — five scenarios (Allergies, Departments, Insurance, Tags, Specialties) covering Q1-Q6 behaviors
4. `/test/multicombo` — five scenarios (Diagnoses, Medications, Lab tests, Specialties, Allergens) covering templated rendering + caps + AllowFreeText interactions
5. `/styleguide` — new "Compound & Multi-Select" section renders, all demos accept input, nav link works
6. Regression check: `/test/textbox`, `/test/textarea`, `/test/number`, `/test/select`, `/styleguide` Section 7 alignment row — all components render at family-consistent 32px field height
7. Cache version `v=20260514` across all 15 references in `App.razor`

**Coordination notes**:

- Phase 2.3 closes here. Tag: `phase-2-sub-2-3-may07`.
- Phase 2.4 (Date/Time family) queued. `DateSegment` will close the deferred
  `LipiCompoundField` extension — currently `LipiCompoundField` supports only
  `SelectSegment` and `TextSegment`. Adding `DateSegment` requires no base
  changes; segment registration via `ICompoundSegment` is the extensibility
  point.
- Path 3 trigger watch: `LipiMultiCombobox` is the SECOND consumer of the
  multi-select dropdown panel. The third consumer (whenever it arrives)
  triggers extraction of the dropdown panel into a base method/component.
  Same trigger applies to the LipiSelect dropdown panel (currently 2
  consumers: `LipiSelect`, `SelectSegment`).
- Memory rule #25 now has 7 bullets (added bullet 7 in this amendment for
  CSS class scaffolding). Pre-delivery quality check applies all 7 bullets
  to every component ship going forward.

**Phase 2 status (post-A20)**:

- ✅ Phase 2.0 (StyleGuide foundation)
- ✅ Phase 2.1 (LipiButton + tokens) — A11 visual review, A14 env-gated retrofit
- ✅ Phase 2.2 (5-component TextInput family) — A12 token foundation, A13 base infrastructure, A15 component completion
- ✅ Phase 2.2.5 (LipiInputBase + EditContext auto-population) — A16, tagged `phase-2-sub-2-2-5-may06`
- ✅ Build hygiene closed — A17 (package warnings) + A18 (C# warnings)
- ✅ Phase 2.3 (Compound + Multi-Select family) — A19, tagged `phase-2-sub-2-3-may07`
- ✅ Phase 2.4 (Date/Time family) — A20, tagged `phase-2-sub-2-4-may07`
- 🟢 Phase 2.5 ready to start (Checkbox + Radio + Toggle family per locked roadmap)

### A20 — Phase 2.4 close: Date/Time family complete

**Phase**: 2 Sub-step 2.4 — Date/Time Component Family
**Date**: 2026-05-07
**Status**: ✅ DEPLOYED, tagged `phase-2-sub-2-4-may07`

> A20 documents Phase 2.4 — the Date/Time component family. One single-batch
> ship (Batch 9d) covering 4 user-facing components, 2 services, supporting
> types, JS helpers, and StyleGuide additions. Phase 2.4 closes here.

**Added**:

Single-batch ship (Batch 9d, May 7) over two turns due to context budget.
Four user-facing components plus 2 services and supporting infrastructure.

**Components** (`src/LiPi.Web/Components/Shared/`):

- `LipiDatePicker.razor` (949 lines) — single date picker, `DateOnly?`, with
  `InputMode=Field|Segments` toggle. Calendar popover with month + year
  dropdowns, edge-aware position:fixed positioning, full WAI-ARIA keyboard
  navigation (Arrow ±1d/±7d, PageUp/Down ±1m, Shift+PageUp/Down ±1y, Home/End
  weekly nav, Ctrl+Home/End monthly nav, Enter/Space commit, Escape close).
  Supports `MinDate`, `MaxDate`, `IsDateAllowed` predicate, `IsDateDisabled` +
  `GetDisabledReason` predicate pair (with "Date unavailable" fallback).

- `LipiTimePicker.razor` (499 lines) — time-of-day picker, `TimeOnly?`. No
  popover — pure segmented input (HH:MM in 24h, HH:MM AM/PM in 12h). `Step`
  parameter validates user input strictly (rejects with error) but Now button
  snaps to nearest past Step boundary. The asymmetry IS the locked behavior.
  Now button reads `IClinicTimezoneService.GetClinicLocalNow()` (NOT system
  clock).

- `LipiDateTimePicker.razor` (350 lines) — composite date+time picker,
  `DateTimeOffset?`. Composes `LipiDatePicker` + `LipiTimePicker` internally.
  `Layout=Stacked|Inline` with Inline auto-collapsing to Stacked at <640px.
  Combined Now button sets BOTH date and time ("Now means Now"). Time picker
  is disabled until date is set (sequential entry pattern).

- `LipiDateRangePicker.razor` (656 lines) — date range picker with separate
  `StartValue` + `EndValue` bindings (BOTH `DateOnly?`). Two-calendar popover
  side-by-side. Optional preset panel (12 individual presets + 2 starter
  bundles). `AllowOpenEnd` parameter for ongoing ranges with "Set as ongoing"
  button. Range visualization with start/end solid circles, in-range
  continuous bar, hover preview dashed pattern. Mobile collapses to single
  calendar with sequential picking.

- `LipiDateTimeTypes.cs` (251 lines) — `DatePickerInputMode` enum
  (`Field|Segments`), `DateTimeLayout` enum (`Stacked|Inline`),
  `DateRangePreset` record, `LipiDateRangePresets` static class with 12
  individual presets (Today, Yesterday, Tomorrow, Last7Days, Last30Days,
  Next7Days, Next30Days, NextWeek, NextMonth, ThisMonth, LastMonth, ThisYear)
  plus 2 starter bundles (`CommonReports`, `CommonScheduling`).

**Services** (`src/LiPi.Web/Services/`):

- `IDateFormatService` + `DateFormatService` — clinic-configurable date and
  time formats. Phase 2.4 default hardcodes India (DD/MM/YYYY, 24h, Sunday
  week-start). Token vocabulary: `D/DD`, `M/MM/MMM/MMMM`, `YY/YYYY` with
  `/-. ` separators. Forgiving parse with ISO 8601 fallback. `GetSegmentOrder`
  collapses word-month formats to numeric MM for segment input mode.

- `IClinicTimezoneService` + `ClinicTimezoneService` — clinic timezone
  resolution (NOT system clock). Phase 2.4 default hardcodes "Asia/Kolkata"
  (UTC+5:30, no DST). Defensive fallback to fixed +05:30 offset if ICU TZ
  database unavailable. Cached `TimeZoneInfo` for process lifetime.

**Test pages** (`src/LiPi.Web/Pages/Test/`):

- `DatePickerTest.razor` (`/test/datepicker`) — 5 scenarios: Field mode, Segments
  mode, IsDateAllowed (weekdays only), IsDateDisabled with holidays + null-reason
  fallback test, Disabled state.
- `TimePickerTest.razor` (`/test/timepicker`) — 6 scenarios: 24h default, 12h
  with AM/PM, Step=15, Step=30 12h, ShowNow=false, Disabled.
- `DateTimePickerTest.razor` (`/test/datetimepicker`) — 4 scenarios: Stacked,
  Inline, Step=15 12h, sequential (time disabled until date set).
- `DateRangePickerTest.razor` (`/test/daterangepicker`) — 5 scenarios: basic,
  CommonReports presets, CommonScheduling presets, AllowOpenEnd, Disabled.

**Edits**:

- `lipi-input.js` extended with `window.lipiDatePicker` module (positionPopover,
  attachReposition with rAF-throttled scroll/resize listeners, detachReposition,
  focusElement). Net +153 lines.

- `App.razor` cache version bump 20260514 → 20260515 (15 references updated).

- `StyleGuide.razor` — added "Date & Time" navigation link + new section
  with 4 demos (LipiDatePicker Field+Segments, LipiTimePicker 24h+12h with
  Step, LipiDateTimePicker Stacked, LipiDateRangePicker with CommonReports
  presets). Added 7 new state fields to TextInputDemo class for binding.

- `deploy-downloads.ps1` — added Phase 2.4 component section + services
  section with 14 new entries.

**Architectural decisions locked**:

- **F3** — `DateSegment` absorbed into `LipiDatePicker InputMode=Segments`.
  Original design considered a separate `DateSegment` for `LipiCompoundField`,
  rejected because it would duplicate calendar-popover wiring. Single
  component with two render modes is simpler.

- **A1** — ISO 8601 storage, configurable display per clinic. Storage and
  display evolve independently.

- **A2.2 corrected** — Popover positioning uses `position: fixed` with
  JS-calculated viewport coordinates, NOT portal/body-level repatriation.
  Matches LipiSelect existing pattern. `position: fixed` escapes ancestor
  `overflow: hidden` naturally including modal containers. Known limitation:
  ancestors with `transform`/`filter`/`will-change` create new containing
  blocks — none currently exist in LiPi but flag if introduced. Original
  Phase 2.4 design specified portal pattern; build chat objected and the lock
  was corrected to position:fixed.

- **A4** — Type-aware bindings. Each component binds to the most precise
  type (DateOnly?, TimeOnly?, DateTimeOffset?, dual DateOnly?). Catches
  misuse at compile time.

- **A6** — `GetDisabledReason` null/empty/whitespace returns "Date
  unavailable" fallback. Empty tooltip on disabled cell is never rendered.

- **B5** — Full WAI-ARIA keyboard pattern for `LipiDatePicker` calendar grid.
  ~50 extra lines vs MVP. User explicitly chose β over pragmatic γ during
  design discussion. Cost is real and accepted.

- **C2** — `LipiTimePicker.Step` asymmetric behavior. User-typed input
  violating Step is REJECTED with error in helper slot (preserves user
  intent). Now button SNAPS to nearest past Step boundary (convenience
  action, user sees and can edit). This asymmetry IS the locked decision.

- **D1** — `LipiDateTimePicker` uses composition pattern (composes
  LipiDatePicker + LipiTimePicker) rather than single combined popover.
  Cleaner state management.

- **D2.1** — Auto-focus from date to time relies on natural Tab order rather
  than explicit cross-component focus. Deferred unless real-world UX surfaces
  a gap.

- **D2.2** — Combined Now button sets BOTH date and time. "Now means Now."
  No state-dependent logic.

- **E1** — `LipiDateRangePicker` exposes `StartValue` + `EndValue` as
  SEPARATE bindings, not a single composite. Matches database storage
  shape and avoids forcing callers to invent a synthetic record/class.

- **E2** — 12 individual presets + 2 starter bundles only. Real bundles
  emerge from module needs in Phase 4.x. Starter bundles are examples, not
  the production catalog.

- **E3** — Range visualization: solid circles for start/end, continuous
  light-blue bar for in-range, dashed pattern for hover preview while
  selecting end.

- **E4** — `AllowOpenEnd` parameter with "Set as ongoing" footer button.
  Field displays `13/01/2026 → ongoing`.

- **Path 3** — Calendar grid duplicated in LipiDatePicker AND
  LipiDateRangePicker (declarative duplication, 2 consumers). Trigger to
  extract = 3rd consumer. Path 3 status post-2.4: all three repeated patterns
  (LipiSelect dropdown, LipiMultiSelect dropdown, calendar grid) at exactly 2
  consumers each.

- **Single-file `@code {}` pattern** (Flag 2 lock) — All four components
  follow the existing LiPi family pattern (LipiTextBox, LipiSelect, etc.).
  Rejected `.razor.cs` code-behind partials. `LipiDatePicker.razor` at 949
  lines slightly over the ~800-line trigger; engineering judgment kept it
  inline because abstract-base-with-single-concrete-subclass is anti-pattern.
  Revisit if it grows beyond ~1200 lines.

- **LipiContainerBase NOT used** for LipiDateRangePicker. Container base is
  for "container with multiple bound segments via ICompoundSegment", which
  doesn't fit dual-value range picker. Direct ComponentBase derivation with
  hand-rolled visual scaffolding is cleaner.

- **CS0102 collision resolution** — `LipiDateTimePicker` had injected
  `IDateFormatService DateFormat` colliding with `[Parameter] public string?
  DateFormat`. Resolved by renaming the injected service to `DateFmt` for
  this component only. The parameter keeps the user-facing name `DateFormat`.

**Engineering process notes**:

- User overrode build chat's engineering objection to single-batch ship.
  Build chat registered objection, user reaffirmed confidence. Ship proceeded
  in two turns (foundation slice + remaining files) due to context budget,
  not architectural blockers. The escape hatch ("if it goes into iterations
  we can stop and batch it at that time") was used as designed.

- Three corrections made during build vs original Phase 2.4 design:
  (1) F2 autoadvance → γ smart auto-advance (user pushback corrected from
  conservative α). (2) A2.2 popover → position:fixed (build chat pushback
  corrected from portal pattern). (3) Code-behind → single-file `@code{}`
  pattern (build chat pushback corrected from `.razor.cs` spec).

**Files**:

```
NEW (16):
src/LiPi.Web/Services/IDateFormatService.cs                    103 lines
src/LiPi.Web/Services/DateFormatService.cs                     264 lines
src/LiPi.Web/Services/IClinicTimezoneService.cs                 81 lines
src/LiPi.Web/Services/ClinicTimezoneService.cs                  97 lines
src/LiPi.Web/Components/Shared/LipiDateTimeTypes.cs            251 lines
src/LiPi.Web/Components/Shared/LipiDatePicker.razor            949 lines
src/LiPi.Web/Components/Shared/LipiDatePicker.razor.css        412 lines
src/LiPi.Web/Components/Shared/LipiTimePicker.razor            499 lines
src/LiPi.Web/Components/Shared/LipiTimePicker.razor.css        ~150 lines
src/LiPi.Web/Components/Shared/LipiDateTimePicker.razor        350 lines
src/LiPi.Web/Components/Shared/LipiDateTimePicker.razor.css    ~110 lines
src/LiPi.Web/Components/Shared/LipiDateRangePicker.razor       656 lines
src/LiPi.Web/Components/Shared/LipiDateRangePicker.razor.css   ~290 lines
src/LiPi.Web/Pages/Test/DatePickerTest.razor                   ~150 lines
src/LiPi.Web/Pages/Test/TimePickerTest.razor                   ~120 lines
src/LiPi.Web/Pages/Test/DateTimePickerTest.razor               ~110 lines
src/LiPi.Web/Pages/Test/DateRangePickerTest.razor              ~120 lines
docs/00-COMPONENTS/01.5-DateTime.md                            ~644 lines

EDITS (4):
src/LiPi.Web/wwwroot/js/lipi-input.js                          +153 lines
src/LiPi.Web/App.razor                                         cache 20260515
src/LiPi.Web/Pages/StyleGuide.razor                            +101 lines (date & time section)
deploy-downloads.ps1                                           +14 entries
```

**Verification recipe**:

1. `dotnet build src/LiPi.Web` → 0 warnings, 0 errors
2. Add to `Program.cs` service registration:
   ```
   builder.Services.AddScoped<IDateFormatService, DateFormatService>();
   builder.Services.AddScoped<IClinicTimezoneService, ClinicTimezoneService>();
   ```
3. Visit each test page in turn:
   - `/test/datepicker` — verify Field/Segments modes, popover, keyboard, constraints
   - `/test/timepicker` — verify 12h/24h, Step asymmetry, Now button reads clinic-local
   - `/test/datetimepicker` — verify Stacked/Inline, Now sets both, time disabled until date
   - `/test/daterangepicker` — verify two calendars, presets, AllowOpenEnd, range viz
4. Visit `/styleguide` — verify "Date & Time" section renders all 4 components

**Coordination notes**:

- Phase 2.5 (Checkbox + Radio + Toggle) is next per the locked roadmap.
- LipiDobPicker deferred to Phase 4.2 (alongside patient registration migration).
  `LabelConfidence` parameter on `LipiDatePicker` already supports the visual
  pill — Phase 4.2 will compose with override-flow UX.
- DST handling deferred to v1.1 (international expansion concern). India has
  no DST so Phase 2.4 has no logic gap for current market.
- Path 3 status: 3 patterns at 2 consumers each. Next 3rd consumer of any
  pattern triggers extraction.

**Post-ship cleanup (May 8)**:

Memory rule consolidation moved Blazor debugging context (F12 console = client
errors, PowerShell terminal = server errors) and `SectionContent` page
conventions to `docs/00-PROJECT-BASELINE.md` as new sections — these were
previously held in working-memory rules deleted during cleanup and now have a
permanent home in the baseline doc. Inline tombstone comment added to
`App.razor` above the `<CascadingAuthenticationState>` wrapper to signal its
SSR-prerender criticality at the source. Cache-version comment block in
`App.razor` refreshed to a reverse-chronological version table covering
20260510 through 20260516 (current); the prose-history format had grown
unmaintainable. Zero code or CSS changes in this cleanup — pure documentation
housekeeping. Files touched: `docs/00-PROJECT-BASELINE.md` (additions only),
`src/LiPi.Web/App.razor` (comment refresh + tombstone), `deploy-downloads.ps1`
(baseline-doc entry added).

## v1.0 — AMENDMENTS (May 14, 2026)

> A21 documents the Phase 2.6.x LipiTabs "two Optional mechanisms" mitigation.
> No component behaviour changes for correct usage — this amendment corrects
> inaccurate XML docs, adds an env-gated developer guard, fixes the StyleGuide
> demo, and documents the deliberate two-mechanism design in the LipiTabs spec
> (new §3.1).

### A21 — LipiTabs: "two Optional mechanisms" — XML-doc accuracy + env-gated guard + demo fix + spec §3.1

**Phase**: 2 Sub-step 2.6.x — LipiTabs (Phase 2.6.1 component, post-2.6.3 stabilisation)
**Date**: 2026-05-14
**Status**: ✅ Component + demo + spec shipped together in one batch

**Trigger.** Phase 2.6.3 Stage 2 testing surfaced a "missing dashed border" on the
StyleGuide Underline demo's Review tab. Investigation found it was not a component
bug but a demo-authoring mistake — and, underneath that, an XML-doc accuracy
problem. `LipiTab` exposes optionality through two intentionally separate
mechanisms:

- `bool Optional` — Vertical variant only; inserts a section divider in the rail
  before the tab. Inert (no visual effect) in Underline and Pill.
- `State = TabState.Optional` — the dashed-border colour treatment in Underline;
  treats the tab as stateless (no state dot) in Vertical; ignored by Pill.

The `bool Optional` XML doc wrongly claimed it "renders dashed border", and both
that doc and the `TabState.Optional` doc claimed an "excluded from required-field
tracking" behaviour that does not exist anywhere in the component. A developer
reading IntelliSense for `Optional` was told it produced the dashed border, set
`Optional="true"` on an Underline tab, and got nothing — silently. That is exactly
how the demo bug was introduced.

**Decision.** The two-mechanism design is kept as locked (Phase 2.6.1 design
session). It was a deliberate split — Vertical expresses "optional" as a rail
divider, Underline expresses it as a dashed border — not an accident. Rather than
merging the two or renaming the bool, the mitigation makes the design safe and
self-documenting. Strategic + build chat weighed three options (keep both as-is,
merge into one, rename the bool); "keep both, mitigate" was chosen by the project
owner.

**What changed.**

1. **XML-doc accuracy (`LipiTab.razor`, `LipiTabsTypes.cs`).** The `bool Optional`
   summary no longer claims a dashed border or required-field tracking — it now
   states Vertical-divider-only and cross-references `State=TabState.Optional`.
   The `TabState.Optional` summary drops the phantom "required-field tracking"
   claim and cross-references `LipiTab.Optional`. The `State` parameter doc also
   cross-references `TabState.Optional` for consistency. A canonical "TWO OPTIONAL
   MECHANISMS" note was added to the `LipiTab.razor` header comment as the single
   in-code source of truth.

2. **Env-gated developer guard (`LipiTab.razor`).** `LipiTab.OnInitialized` now
   detects `Optional == true` on a non-Vertical parent (`Variant` is Underline or
   Pill) — the case where `Optional` is silently inert. Development throws
   `InvalidOperationException` with a message pointing at `State="TabState.Optional"`;
   Production logs via `ILogger<LipiTab>` and continues (the flag is inert, so
   rendering is unaffected — there is nothing to "fall back" to). This mirrors the
   env-gated validation pattern established by LipiButton (A14) /
   `LipiInputBase.ValidateOrFallback`. The check is `OnInitialized`-scoped:
   runtime `Variant` changes are not a supported LipiTabs pattern, so a
   once-per-instance check is sufficient, and the delicate `OnParametersSet`
   dirty-check (Phase 2.6.3 Stage 2 loop fix) is left untouched. `LipiTab.razor`
   gains `@using Microsoft.AspNetCore.Hosting` + `@using Microsoft.Extensions.Logging`
   and `@inject IWebHostEnvironment Env` + `@inject ILogger<LipiTab> Log`.

3. **StyleGuide demo corrected (`StyleGuideLayout.razor`).** The Underline demo's
   Review tab was changed from `Optional="true"` (inert on Underline, and now a
   Development throw) to `State="TabState.Optional"` (the actual dashed-border cue
   — this fixes the original "missing dashed border"). The Vertical demo's Notes
   tab keeps `Optional="true"` (correct usage — the rail divider). Captions and
   source comments were added to both demos so the showcase teaches the
   distinction. Each demo deliberately shows one variant's mechanism with a
   cross-reference caption, rather than stacking both on one tab where the second
   would produce no additional visible change.

4. **Spec documentation (`docs/02-LipiTabs-Spec.md`).** The §3 LipiTab parameter
   table row for `Optional` was corrected — it previously read "Dashed border.
   Excluded from required-field tracking.", both wrong. A new §3.1 subsection,
   "The two 'Optional' mechanisms", documents the deliberate split, the
   per-variant behaviour, the env-gated guard, and the rationale for keeping both.

**No behaviour change for correct usage.** The component renders identically for
every correct usage. The only runtime-visible change is the new Development-time
throw for the specific misuse `Optional="true"` on a non-Vertical `LipiTabs` —
which previously failed silently. A codebase sweep confirmed the only occurrence
was the StyleGuide Underline demo (fixed in this batch); `StyleGuide.razor` and
`StyleGuideOverlays.razor` use no tabs, and the PatientNew mapping (spec §11) is
Vertical, where `Optional` is valid.

**Files**:
- `src/LiPi.Web/Components/Shared/LipiTab.razor` — header "TWO OPTIONAL MECHANISMS"
  note; `bool Optional` + `State` XML docs corrected; `@using`/`@inject` for
  `IWebHostEnvironment` + `ILogger<LipiTab>`; env-gated guard in `OnInitialized`
- `src/LiPi.Web/Components/Shared/LipiTabsTypes.cs` — `TabState.Optional` XML doc
  corrected (phantom "required-field tracking" claim removed; cross-reference to
  `LipiTab.Optional` added)
- `src/LiPi.Web/Pages/StyleGuideLayout.razor` — Underline Review tab fixed to
  `State="TabState.Optional"`; captions + source comments on the Underline and
  Vertical tab demos
- `docs/02-LipiTabs-Spec.md` — §3 `Optional` row corrected; new §3.1 subsection
  "The two 'Optional' mechanisms"
- `deploy-downloads.ps1` — `02-LipiTabs-Spec.md` entry added (the spec doc was not
  previously mapped); `LipiTab.razor`, `LipiTabsTypes.cs`, and
  `StyleGuideLayout.razor` were already mapped

**Forward note.** `State=TabState.Optional` on a Pill variant is also inert (Pill
ignores `TabState` entirely). This is a separate, lower-severity case and was left
out of the guard to keep A21 tightly scoped to the confirmed footgun. If it
surfaces in practice, extend the same `OnInitialized` guard.

### A22 — LipiTabs CSS self-sufficiency: optional-token fallbacks + robust underline overlap

**Phase**: 2 Sub-step 2.6.x — LipiTabs (`lipi-tabs.css` portability / rendering fix)
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** After A21 deployed, the Underline tabs' active navy underline AND the
tablist's faint baseline line were both missing on `/admin/style-guide/layout`.
DevTools computed-style inspection: the `.lipi-tablist` `border-bottom` resolved to
`none / 0px`, and the active tab's `border-bottom` was navy `1.6px` being mostly
consumed by a `-1px` overlap margin. A21 shipped no CSS — this was a pre-existing
defect in `lipi-tabs.css`, surfaced (not caused) by the A21 deploy.

**Root causes — two undefined-token bugs + one fragile overlap.**

1. `lipi-tabs.css` referenced `var(--color-border-tertiary)` with **no fallback**,
   in two places: the Underline tablist `border-bottom` and the Vertical rail
   `border-right`. `--color-border-tertiary` is not part of the theme token
   contract — `mode-light.css` / `mode-dark.css` define `--color-border-{default,
   subtle,strong,focus}` but no `-tertiary` tier. An undefined custom property with
   no fallback makes the entire `border` shorthand invalid, so the line renders as
   `none`. The faint tablist baseline therefore never painted.

2. The active underline used `border-bottom` + `margin-bottom: -1px` to overlap and
   "merge" with the 1px tablist baseline (Phase 2.6.3 Stage 2C). The `-1px` was a
   magic literal coupled to the tablist's `1px` border by assumption, not by code.
   With the tablist border invalid (bug 1) and the underline rendering sub-pixel
   (`1.6px`, consistent with browser zoom), the `-1px` margin pulled most of the
   already-thin underline out of view. Net effect: no visible underline.

3. Separately, `.lipi-tabs-pill .lipi-tab-active` referenced `var(--sh-xs)` with no
   fallback. `--sh-xs` is also not in the token contract (the theme files ship
   `--sh-{sm,md,lg,thumb}`), so the Pill active tab's lift shadow silently did not
   render. Same bug class; fixed in the same pass.

**Principle.** The component library is built for reuse across projects. A
component must be **self-sufficient**: it may depend on the documented token
contract (`--color-{primary,text-*,bg-*,border-{default,subtle,strong,focus}}`,
`--sp-*`, `--r-*`, `--sh-{sm,md,lg}`, `--font-*`, `--ts-*`, `--fw-*`, `--tr-*`),
but any reference to an *optional* token outside that contract must carry a
fallback so the component never visually breaks in a consuming app whose theme does
not define it. Colour is the app's decision; structural values (underline
thickness, the overlap geometry) are the component's. A22 brings `lipi-tabs.css`
in line with that principle.

**What changed — all in `src/LiPi.Web/wwwroot/css/lipi-tabs.css`.**

1. **Optional-token fallbacks.** `var(--color-border-tertiary)` →
   `var(--color-border-tertiary, var(--color-border-subtle))` in both the Underline
   tablist `border-bottom` and the Vertical rail `border-right`. `var(--sh-xs)` →
   `var(--sh-xs, var(--sh-sm))` on the Pill active tab. A host theme *may* define
   `-tertiary` / `-xs` for finer control; if it does not, the component falls back
   to a guaranteed-present contract token and still renders correctly.

2. **Robust underline overlap.** Introduced `--lipi-tab-strip-border-width`
   (component-owned, default `1px`), joining the existing `--lipi-tab-*`
   brand-overridable token family in the file header. The Underline tablist's
   `border-bottom` width and the active tab's overlap margin
   (`margin-bottom: calc(-1 * var(--lipi-tab-strip-border-width, 1px))`) now both
   derive from this **one** token, so the overlap is always exactly the baseline
   thickness — they cannot desync under browser zoom or a brand override. This
   supersedes the Stage 2C fixed `-1px` literal.

**Why not switch the underline to `box-shadow`.** A `box-shadow` underline is
immune to box-model fragility, but it cannot render the dashed treatment that
`TabState.Optional` requires. Keeping one `border-bottom` mechanism for every
underline state (solid + dashed) is more maintainable for a reusable component than
a hybrid. The fragility was the magic-literal margin, not the border — so the fix
tied the margin to a token rather than switching mechanisms.

**Not changed.** The `1.6px` computed underline width is consistent with browser
zoom (the whole pixel grid scales). The Vertical rail's `border-right` keeps its
`0.5px` width; only its colour token gained a fallback. No attempt was made to pin
`--lipi-tab-underline-width` to a non-overridable literal — spec §17 designates it
brand-overridable.

**Follow-up correction (same-day, cache `20260518`).** The original A22 work
(cache `20260517`) claimed the new overlap geometry "renders correctly at any
zoom" — that was wrong, and browser testing caught it. A22 tied the overlap margin
to `--lipi-tab-strip-border-width` (correct) but left the active tab's *visible*
underline thickness equal to `--lipi-tab-underline-width` alone. At zoom <100% the
nominal `2px` underline renders sub-pixel (`1.6px` at 80%); the `-1px` overlap pull
then left only ~`0.6px` showing — effectively invisible, the exact symptom A22 set
out to fix. The overlap-to-strip-border coupling was right; the missing piece was
that the active tab must *compensate* for the overlap. Fix, in
`.lipi-tabs-underline .lipi-tab-active`:

```css
border-bottom-width: calc(
    var(--lipi-tab-underline-width, 2px) + var(--lipi-tab-strip-border-width, 1px)
);
```

The active underline is thickened by exactly the overlap amount, so after the
`margin-bottom` pull a full `--lipi-tab-underline-width` of colour always remains
visible — at any zoom, under any brand override, with no fixed pixel literals.
State-coloured active tabs (`.lipi-tab-state-{complete,partial,empty}.lipi-tab-active`)
set only `color` / `border-bottom-color` / `background`, so the new
`border-bottom-width` cascades through to them unchanged. `lipi-tabs.css` comments
on both the base `.lipi-tab` rule and the active rule were corrected to describe
the real geometry. `App.razor` cache stamp bumped `20260517` → `20260518`
(`20260517` shipped a now-superseded `lipi-tabs.css`).

**Files**:
- `src/LiPi.Web/wwwroot/css/lipi-tabs.css` — three optional-token fallbacks;
  `--lipi-tab-strip-border-width` token introduced; Underline tablist border width
  and active-tab overlap margin retied to it; **follow-up: active-tab
  `border-bottom-width` set to `underline-width + strip-border-width` so a full
  underline-width survives the overlap pull at any zoom**; header token list +
  inline comments updated (base `.lipi-tab` and active rule comments corrected in
  the follow-up)
- `src/LiPi.Web/App.razor` — cache-version stamp bumped `20260516` → `20260517`
  → **`20260518`** (uniform across all `?v=` query strings, per the file's
  established convention) + history-table rows added
- `deploy-downloads.ps1` — no change; `lipi-tabs.css`, `App.razor`, and
  `CHANGE-LOG.md` were all already mapped

**Forward note (out of scope for A22).** Two latent items observed while fixing
this, left for a deliberate pass rather than scope-creeping A22: (a) the Vertical
rail `border-right` is `0.5px` — sub-pixel, the same rounding-fragility class
Stage 2C addressed elsewhere; (b) whether `--color-border-tertiary` and `--sh-xs`
should be *added* to the theme contract (so the `-tertiary` / `-xs` tiers genuinely
exist) or left as optional-with-fallback is a token-architecture decision for the
theme files, not the component.

### A23 — LipiAlert: Critical header strip alignment fix

**Phase**: 2 Sub-step 2.6.1 — LipiAlert (`LipiAlert.razor.css` rendering fix)
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** Browser review of the Critical Filled alert on the StyleGuide Layout
page: the dark-red header strip ("Critical alert — action required") had its icon
and text (a) horizontally inboard of the alert body's left gutter — the shield icon
sat ~4px left of where "Documented allergy — Penicillin" began below it — and
(b) sitting visually high within the red band rather than vertically centred.

**Root causes — three, all in `LipiAlert.razor.css`.**

1. **`margin-top: -0.85rem` overshot the top edge.** The critical header strip is
   meant to be full-bleed — pulled to the alert's outer edges by negative margins.
   The horizontal `-1rem` correctly cancels the wrapper's `1rem` side padding. But
   `lipi-alerts.css` sets `padding-top: 0` on the critical filled wrapper, so the
   strip already sits flush at the alert's top edge — `margin-top: -0.85rem` then
   pulled it a further 0.85rem *above* that edge. The wrapper's `overflow: hidden`
   clipped the overshoot, so the strip's content box was effectively cropped at the
   top and the icon + text rendered high within the visible red band. The prior
   inline comment reasoned "wrapper padding-top is 0, so -0.85rem bleeds to the
   top" — but that is backwards: if padding-top is already 0, the strip is already
   at the top, and a negative margin-top overshoots past it.

2. **Horizontal padding `12px` did not match the body gutter.** The strip used
   `padding: 10px 12px`; the alert body content is indented by the wrapper's `1rem`
   (16px) gutter. So the strip's icon started 12px from the edge while the body
   title started 16px from the edge — the header content and the body content did
   not share a left line.

3. **Header text inherited `line-height: 1.5`.** `.lipi-alert-critical-header-text`
   set `font-size` but no `line-height`, inheriting `1.5` from `.lipi-alert`. That
   produces a tall line-box with asymmetric space above/below the glyphs; with
   `align-items: center` on the strip, the line-box centred but the visible glyphs
   sat high within it — compounding the strip-overshoot in (1).

**What changed — all in `src/LiPi.Web/Components/Shared/LipiAlert.razor.css`.**

1. `.lipi-alert-critical-header` — `margin-top: -0.85rem` → `margin-top: 0`. The
   strip sits flush at the alert's true top edge (wrapper already has
   `padding-top: 0`); no upward pull, nothing clipped.
2. `.lipi-alert-critical-header` — horizontal padding `12px` → `1rem`. The strip's
   icon + text now align on the same left edge as the title / message / actions in
   `.lipi-alert-critical-body` below. Vertical padding stays the symmetric `10px`
   from the Phase 2.6.3 Stage 2C fix.
3. `.lipi-alert-critical-header-text` — `line-height: 1` pinned. The line-box hugs
   the glyphs so `align-items: center` centres what the eye actually sees.

**No spec conflict.** `docs/03-LipiAlert-Spec.md` §6 / §7 describe the critical
header strip as `background: #A32D2D`, white text, "separates title from body" —
they specify no padding, margin, or line-height values. A23 is a free rendering
fix with no spec value to reconcile; the spec needs no amendment.

**Not changed.** The icon span (`.lipi-alert-icon`) keeps its shared-CSS rules — it
is `display: flex; align-items: center` holding a fixed-size `LucideIcon` SVG, so
its own `line-height` is inert for the SVG child and it co-centres correctly with
the now-pinned text span. The strip's vertical padding, background, colour, and
full-bleed horizontal margins are unchanged. This fix touches the Critical Filled
header strip only — no other severity, style, or component is affected (Critical is
the only severity with a header strip; Banner Critical and LeftBorder Critical use
no strip).

**Files**:
- `src/LiPi.Web/Components/Shared/LipiAlert.razor.css` — three rules in the
  critical header strip block corrected (`margin-top`, horizontal `padding`,
  header-text `line-height`); file-header amendment note added
- `src/LiPi.Web/App.razor` — cache-version stamp bumped `20260518` → `20260519`
  (uniform across all `?v=` query strings, per the file's established convention)
  + history-table row added
- `deploy-downloads.ps1` — no change; `LipiAlert.razor.css`, `App.razor`, and
  `CHANGE-LOG.md` were all already mapped

### A24 — LipiAlert: Critical body text aligned to the alert family's body column

**Phase**: 2 Sub-step 2.6.1 — LipiAlert (CSS layout fix)
**Date**: 2026-05-14
**Status**: ✅ Shipped (first attempt cache `20260520` did not apply — corrected, cache `20260521`)

**Trigger.** With several alerts stacked on the StyleGuide Layout page
(Success / Warning / Danger / Critical), the Critical alert's body text and
action buttons sat noticeably left of the body text in every alert above it —
the stack did not share a common text column.

**Root cause.** Every non-Critical alert renders as a flex row —
`[icon span] gap [.lipi-alert-body]` — via `.lipi-alert { display: flex;
gap: 12px }`. Their body text therefore starts at
`wrapper-padding-left (1rem) + icon-width + 12px gap`. The Critical Filled
alert has a different structure: its shield icon lives *inside* the header
strip, and the body below it (`.lipi-alert-critical-body`) is a plain block
with no inline icon beside it. So the Critical body started at
`wrapper-padding-left (1rem)` alone — roughly 30px left of where every other
severity's body text begins. A23 had aligned the *header strip's* content to
the body gutter, but the body itself still lacked the icon-column offset, so
Critical broke the family's shared reading column when stacked.

**First attempt (cache `20260520`) — did not apply.** The initial fix added a
new `.lipi-alert-critical-body { padding-left: calc(18px + 12px) }` rule to
the *shared* `lipi-alerts.css`. It was overridden and never took effect. Two
reasons, both missed at the time:
- The scoped `LipiAlert.razor.css` already has a `.lipi-alert-critical-body`
  rule that sets `padding` as a **shorthand** (`padding: 0 0 0.85rem`). A
  shorthand sets *all four sides* — including `padding-left: 0` — explicitly.
  A shorthand and a longhand for the same property always overlap; the A24
  batch note that called them "non-overlapping" was wrong.
- Blazor's scoped CSS attaches a `[b-{hash}]` attribute selector, which
  outspecifies a plain class selector in a shared stylesheet. So the scoped
  shorthand's implicit `padding-left: 0` beat the shared file's
  `padding-left` longhand. DevTools confirmed: the shared rule showed as
  empty/overridden, computed `padding-left` stayed `0`.

**Correction (cache `20260521`) — what actually shipped.**
- `src/LiPi.Web/Components/Shared/LipiAlert.razor.css` — the
  `.lipi-alert-critical-body` rule's `padding` shorthand was expanded to
  explicit longhands: `padding-bottom: 0.85rem` (unchanged intent) +
  `padding-left: calc(18px + 12px)` (the icon-column offset). Both padding
  sides now live as longhands in **one rule, in the scoped file** — the file
  that wins the cascade. `18px` = `.lipi-alert-icon` width (holds an 18px
  `LucideIcon`); `12px` = the same gap literal as `.lipi-alert`.
- `src/LiPi.Web/wwwroot/css/lipi-alerts.css` — the dead
  `.lipi-alert-critical-body` rule from the first attempt was removed and
  replaced with an explanatory note pointing to the scoped file.

**Layout-vs-Shape note.** A `padding-left` that sets the body's column
position is arguably "layout", which the Layout-vs-Shape rule assigns to the
shared file. But the shared file *demonstrably cannot win* against the
scoped shorthand, and the body's `padding-bottom` (shape) already lived in
the scoped file — splitting the body's padding across two files is what
caused this bug. The pragmatic, correct resolution is one coherent rule in
the file that applies: both longhands in the scoped `LipiAlert.razor.css`.

**No spec conflict.** `docs/03-LipiAlert-Spec.md` §6 / §7 describe the
Critical treatment (header strip, `#F09595` background, `1.5px` border,
no ✕, `shield-x` icon) but specify nothing about the Critical body's left
alignment. A24 is a free layout fix; the spec needs no amendment.

**Not changed.** The header strip stays full-bleed — it bleeds to the alert's
edges via its own `-1rem` margins in `LipiAlert.razor.css`, unaffected by a
body padding change. Only the Critical Filled body is touched; no other
severity, style, or component is affected (Critical is the only severity
whose body sits without an inline icon sibling).

**Files**:
- `src/LiPi.Web/Components/Shared/LipiAlert.razor.css` —
  `.lipi-alert-critical-body` `padding` shorthand expanded to
  `padding-bottom` + `padding-left` longhands; file-header amendment note
  added
- `src/LiPi.Web/wwwroot/css/lipi-alerts.css` — first-attempt
  `.lipi-alert-critical-body` rule removed, explanatory note left in its
  place; file-header amendment note corrected
- `src/LiPi.Web/App.razor` — cache-version stamp bumped `20260519` →
  `20260520` → `20260521` (uniform across all `?v=` query strings) +
  history-table rows added
- `deploy-downloads.ps1` — no change; `LipiAlert.razor.css`,
  `lipi-alerts.css`, `App.razor`, and `CHANGE-LOG.md` were all already mapped

### A25 — LipiAlert: Critical header aligned to the alert family's icon+text rhythm

**Phase**: 2 Sub-step 2.6.1 — LipiAlert (markup + CSS layout fix)
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** After A24 moved the Critical alert's *body* into the alert
family's shared text column, the Critical *header* text ("Critical alert —
action required") was left behind — sitting ~30px left of the body text
directly below it, and out of step with the rest of the alert stack.

**Root cause — A23 and A24 fixed adjacent things and exposed this seam.**
A23 set the header strip's horizontal padding to `1rem` so the header content
aligned with the body's *then-current* position. A24 then moved the body
right by the icon-column offset (`icon-width + gap` ≈ 30px) so it would match
the non-Critical alerts. The header was not part of A24's change, so it
stayed at `1rem` while the body moved to `1rem + 30px`. Net result: header
text 30px left of body text.

The deeper issue: every non-Critical alert is a flex row —
`[icon] gap [text]` — with an 18px icon and a 12px gap (`.lipi-alert-icon`
`font-size: 18px`; `.lipi-alert` `gap: 12px`). The Critical header strip is
*also* an `[icon] gap [text]` flex row, but it was built with a 14px icon and
an 8px gap — so it never matched the family rhythm, and after A24 it no
longer matched the Critical alert's own body either.

**What changed.** The Critical header strip now uses the same icon size and
gap as every standard alert row:

- `src/LiPi.Web/Components/Shared/LipiAlert.razor` — the header strip's
  `<LucideIcon Name="shield-x" Size="14" />` → `Size="18"`, matching the
  standard alert icon size used everywhere else in the component.
- `src/LiPi.Web/Components/Shared/LipiAlert.razor.css` — the
  `.lipi-alert-critical-header` `gap: 8px` → `gap: 12px`, matching
  `.lipi-alert`'s gap.

With the strip's existing `1rem` left padding, this makes the header
`[1rem][18px icon][12px gap][text]`: the header **icon** lands at `1rem`
(aligned with every other alert's icon) and the header **text** lands at
`1rem + 18 + 12 = 1rem + 30px` — aligned with every other alert's text *and*
with this alert's own body, which A24 placed at exactly `1rem + 30px`. One
coherent change satisfies all three alignments at once.

**No spec conflict.** `docs/03-LipiAlert-Spec.md` §6 names the Critical icon
(`shield-x`) but pins no size; §6/§7 specify no header strip gap. A25 is a
free layout fix; the spec needs no amendment.

**Not changed.** The header strip's full-bleed background (`margin: 0 -1rem`),
its symmetric vertical padding (`10px`, A23), `margin-top: 0` (A23), and the
header text's `line-height: 1` (A23) are all unchanged — A25 only touches the
icon size and the gap. No other severity, style, or component is affected
(Critical is the only severity with a header strip).

**Alert alignment series complete.** A23 (header strip vertical centring +
top-edge fix), A24 (body text into the family column), A25 (header into the
family rhythm) together bring the Critical Filled alert into full alignment —
internally (header, body, actions on one column) and with the alert family
(icons on one column, text on another).

**Files**:
- `src/LiPi.Web/Components/Shared/LipiAlert.razor` — header strip
  `LucideIcon` `Size="14"` → `Size="18"`; explanatory comment added
- `src/LiPi.Web/Components/Shared/LipiAlert.razor.css` —
  `.lipi-alert-critical-header` `gap: 8px` → `gap: 12px`; file-header
  amendment note added
- `src/LiPi.Web/App.razor` — cache-version stamp bumped `20260521` →
  `20260522` (uniform across all `?v=` query strings) + history-table row
  added
- `deploy-downloads.ps1` — no change; `LipiAlert.razor`,
  `LipiAlert.razor.css`, `App.razor`, and `CHANGE-LOG.md` were all already
  mapped

### A26 — LipiCard CSS self-sufficiency: optional-token fallbacks

**Phase**: 2 Sub-step 2.6.1 — LipiCard (`lipi-cards.css` portability fix)
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** Pre-test audit of `lipi-cards.css` (before exercising the Card
StyleGuide section) — applying the same self-sufficiency check that caught the
equivalent bugs in `lipi-tabs.css` (A22).

**Root cause.** `lipi-cards.css` referenced two tokens that are **not part of
the theme token contract**, both with **no fallback**:

1. `--color-border-tertiary` — used in `.lipi-card-header` `border-bottom` and
   `.lipi-card-footer` `border-top`. The theme files (`mode-light.css` /
   `mode-dark.css`) define `--color-border-{default,subtle,strong,focus}` — no
   `-tertiary` tier. An undefined custom property with no fallback makes the
   whole `border` shorthand invalid, so the line renders as nothing — the
   header/body and footer/body divider lines silently did not paint.
2. `--sh-xs` — used in `.lipi-card-clickable:hover` `box-shadow`. Also not in
   the contract (the theme files ship `--sh-{sm,md,lg}`), so the clickable
   card's hover lift silently did not render.

Both are the same bug class as the `lipi-tabs.css` issues fixed in A22 —
a component in a reusable library referencing optional tokens without a
guaranteed-present fallback.

**Principle.** A component may depend on the documented token contract
(`--color-{primary,text-*,bg-*,border-{default,subtle,strong,focus}}`,
`--sp-*`, `--r-*`, `--sh-{sm,md,lg}`, `--font-*`, `--ts-*`, `--fw-*`,
`--tr-*`), but any reference to a token *outside* that contract must carry a
fallback so the component never visually breaks in a consuming app whose
theme does not define it.

**What changed — all in `src/LiPi.Web/wwwroot/css/lipi-cards.css`.**

- `.lipi-card-header` `border-bottom` and `.lipi-card-footer` `border-top`:
  `var(--color-border-tertiary)` → `var(--color-border-tertiary,
  var(--color-border-subtle))`. A host theme may still define `-tertiary` for
  a finer border tier; without it, the divider falls back to the
  guaranteed-present `--color-border-subtle` and still renders.
- `.lipi-card-clickable:hover` `box-shadow`: `var(--sh-xs)` →
  `var(--sh-xs, var(--sh-sm))`. Same pattern — falls back to the
  contract-guaranteed `--sh-sm`.

**Not changed.** No behaviour change for any theme that *does* define
`-tertiary` / `-xs` — the fallbacks only engage when the optional token is
absent. `LipiCard.razor.css` (the scoped shape file) was checked and
references only contract tokens (`--color-primary`, `--r-md`) — no fix
needed there. The component markup, variants, and all other CSS rules are
untouched.

**Files**:
- `src/LiPi.Web/wwwroot/css/lipi-cards.css` — two optional-token fallbacks
  (`--color-border-tertiary` ×2 sites, `--sh-xs` ×1 site); file-header
  amendment note added
- `src/LiPi.Web/App.razor` — cache-version stamp bumped `20260522` →
  `20260523` (uniform across all `?v=` query strings) + history-table row
  added
- `deploy-downloads.ps1` — no change; `lipi-cards.css`, `App.razor`, and
  `CHANGE-LOG.md` were all already mapped

### A27 — LipiCard: spec reconciliation — interactivity model unified; accent radius / focus-ring / aria brought to spec

**Phase**: 2 Sub-step 2.6.1 — LipiCard (component + scoped CSS + shared CSS)
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** Build-chat review of the seven LipiCard files against
`docs/04-LipiCard-Spec.md` ahead of exercising the Card StyleGuide section.
Five issues surfaced: one systemic (interactivity), three written-spec
deviations, one ARIA correctness bug.

**1. Systemic — interactivity and interactive-styling were driven by different
inputs.** `LipiCard.razor` decided the *render path* (`<a>` / `<div
role="button">` / plain `<div>`) from `Href` / `OnClick.HasDelegate` /
`Clickable`, but composed the `lipi-card-clickable` CSS class from `Variant`
alone. The two never agreed, producing three footguns:

- `Variant="Outlined"` + `Href` → interactive `<a>`, but styled static; and
  `Disabled` was inert because `pointer-events: none` only exists under
  `.lipi-card-clickable.lipi-card-disabled`.
- `Variant="Clickable"` with no handler → got `lipi-card-clickable` styling
  (hover, `cursor: pointer`) but fell through to a plain, non-focusable
  `<div>` — looked clickable, was not.
- A disabled link card still navigated — `<a href>` stayed live;
  `aria-disabled` is advisory only.

The spec itself is internally inconsistent on whether the
`CardVariant.Clickable` *variant* alone is interactive — §4 defines it as a
variant *with* hover/active/selected states; §5 lists only `OnClick` / `Href`
/ `Clickable="true"` as triggers. Strategic + build chat weighed both readings
with full pros/cons. **Decision — Option B**: honour §4. A single computed
`IsInteractive` (`Href set || OnClick wired || Clickable=="true" ||
Variant==CardVariant.Clickable`) now drives the render path, the
`lipi-card-clickable` class, and `tabindex` together — they can no longer
desync. `CardVariant.Clickable`'s base look is `lipi-card-outlined`;
interactivity is layered on via `IsInteractive`. §5's trigger list is read as
non-exhaustive.

**2. Env-gated guard for Option B's one footgun.** Option B leaves one case —
`Variant="Clickable"` with nothing wired renders a focusable `role="button"`
that does nothing. `LipiCard.OnInitialized` now detects this and, in
Development, throws `InvalidOperationException` pointing the developer at
`OnClick` / `Href` or `Clickable="true"`; in Production it logs via
`ILogger<LipiCard>` and renders harmlessly (the card is inert — there is
nothing to fall back to). Mirrors the env-gated pattern of LipiTab (A21) and
LipiButton (A14). `LipiCard.razor` gains `@using
Microsoft.AspNetCore.Hosting` + `@using Microsoft.Extensions.Logging` and
`@inject IWebHostEnvironment Env` + `@inject ILogger<LipiCard> Log`. The check
is `OnInitialized`-scoped — runtime `Variant` changes are not a supported
pattern.

**3. Three written-spec deviations reconciled (code → spec).** Per the
deviation-handling rule these are recorded explicitly, not changed silently:

- **Accent border-radius.** `lipi-cards.css` `.lipi-card-accent` squared its
  left corners (`border-radius: 0 … 0`); spec §6 specifies full radius on all
  corners. Reconciled to spec — `border-radius: var(--lipi-card-radius,
  var(--r-md))`.
- **Focus-ring colour.** `LipiCard.razor.css` used `outline: 2px solid
  var(--color-primary)`; spec §5 / §9 specify `var(--color-border-focus)`.
  Reconciled to spec. `--color-border-focus` is in the documented token
  contract, so A26's self-sufficiency property (this scoped file references
  contract tokens only) is preserved.
- **`aria-pressed` when not selected.** The button-role branch emitted
  `aria-pressed` as `null` (attribute omitted) when `!Selected`; spec §9
  specifies `aria-pressed="@Selected"` — an explicit `"false"` so assistive
  tech announces the un-pressed toggle state. Reconciled to spec.

**4. ARIA correctness — `aria-pressed` on the link path.** Both render
branches set `aria-pressed`. `aria-pressed` is not valid on an `<a>`; spec §9
specifies `aria-current="true"` for the link role's selected state. The `<a>`
branch now emits `aria-current` (`"true"` when `Selected`, omitted otherwise);
the button-role branch keeps `aria-pressed` (now explicit `"true"` / `"false"`
per item 3).

**5. Disabled link no longer navigates.** The `<a>` branch now emits
`href="@(Disabled ? null : Href)"` — a disabled link card drops its live
`href`, so it is neither navigable nor focusable. `aria-disabled="true"` and
the `.lipi-card-clickable.lipi-card-disabled` styling (opacity, cursor,
`pointer-events: none`) still apply, since `IsInteractive` stays true (the
`Href` parameter is still set, just not emitted).

**Also — `CascadingValue IsFixed` `false` → `true`.** The cascaded value is
`this`, which never changes for the component's lifetime. `IsFixed="true"` is
both more correct and avoids re-rendering the sub-component cascade on every
LipiCard render.

**One CSS rule added for a combo Option B newly enables.** `Variant="Accent"`
+ `Clickable="true"` is now a reachable combo. `.lipi-card-clickable:hover`
sets `border-color`, which would also recolour the accent strip; a new rule
`.lipi-card-accent.lipi-card-clickable:hover:not(.lipi-card-disabled)`
re-pins `border-left-color` to the accent colour so the strip survives hover.

**Not changed.** `CardHeader.razor`, `CardBody.razor`, `CardFooter.razor`,
`LipiCardTypes.cs` are spec-compliant as-is. `deploy-downloads.ps1` already
maps all seven LipiCard files plus `App.razor` and `CHANGE-LOG.md` — no
change. The StyleGuide `#cards` section currently shows one of the nine
demonstrations spec §15 lists; its expansion is a separate batch (component
correctness lands first). Spec §13 / §14 list four files but the component
ships seven (`CardHeader/Body/Footer.razor` were never enumerated) — a
spec-doc accuracy fix queued with the §15 expansion.

**Files**:
- `src/LiPi.Web/Components/Shared/LipiCard.razor` — `IsInteractive` computed
  property; render path + CSS class + `tabindex` unified on it; env-gated
  `OnInitialized` guard (+ `@using` / `@inject` for `IWebHostEnvironment` +
  `ILogger<LipiCard>`); `aria-pressed`→`aria-current` on the `<a>` branch,
  explicit `"true"` / `"false"` on the button-role branch; disabled `<a>`
  drops live `href`; `CascadingValue IsFixed` `false`→`true` ×3; header
  comment updated; redundant `@using LiPi.Web.Components.Shared` removed
- `src/LiPi.Web/Components/Shared/LipiCard.razor.css` — focus-ring outline
  colour `--color-primary` → `--color-border-focus` (×2 rules); file-header
  amendment note added
- `src/LiPi.Web/wwwroot/css/lipi-cards.css` — `.lipi-card-accent`
  `border-radius` reconciled to full radius (spec §6); new
  `.lipi-card-accent.lipi-card-clickable:hover` rule preserving the accent
  strip on hover; file-header amendment note added
- `src/LiPi.Web/App.razor` — cache-version stamp bumped `20260523` →
  `20260524` (uniform across all `?v=` query strings) + history-table row
  added
- `deploy-downloads.ps1` — no change; all seven LipiCard files, `App.razor`,
  and `CHANGE-LOG.md` were all already mapped

### A28 — LipiCard: StyleGuide #cards section expanded to the full spec §15 showcase

**Phase**: 2 Sub-step 2.6.1 — LipiCard (StyleGuide showcase)
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** A27 noted the StyleGuide `#cards` section showed one of the nine
demonstrations spec §15 lists, and deferred the expansion to its own batch
(component correctness first). This is that batch — `#cards` now exercises the
full component surface.

**What changed.** `StyleGuideLayout.razor`'s `#cards` section rebuilt from one
clickable grid to the nine spec §15 demonstrations: (1) all five variants side
by side; (2) clickable selection grid — four appointment slots, one selected;
(3) accent cards in all five `CardAccentColor` values; (4) full slotted card —
CardHeader (title + subtitle + action) + CardBody + CardFooter; (5) free-content
body-only card (the PatientNew tab-panel pattern); (6) flat cards in a
three-column metric dashboard; (7) elevated featured panel; (8) clickable + Href
link card (renders as `<a>`); (9) disabled clickable card.

**A27-guard-aware.** Every `Variant="CardVariant.Clickable"` demo wires
`OnClick` or `Href` — a bare Clickable variant now throws in Development via the
A27 env-gated guard. Demos 1, 2 and 9 wire `OnClick`; demo 8 sets `Href`.

**Showcase CSS.** `StyleGuideLayout.razor.css` gains a `Phase 2.6.1 — Cards
section` block: `.sg-card-grid` + `-3/-4/-5` responsive column helpers (2-up
below 880px), `.sg-card-single` (width-constrained single-card demos),
`.sg-demo-note` (per-demo caption), `.sg-metric-value` / `.sg-metric-label`
(demo 6 inner content). Scoped to this page only — the other two
`StyleGuide*.razor.css` files are untouched (the cards demo lives only here).

**`@code` block.** `_specialties` / `_selected` (the old single grid's state)
replaced with `_slots` / `_selectedSlot` and a named `SelectSlot(string)` method
(per the no-assignment-lambda rule). The component owns all visual design via
`lipi-cards.css` + `LipiCard.razor.css` — the showcase adds layout helpers only.

**Not changed.** No component, `lipi-cards.css`, or `App.razor` change — this is
a showcase-only batch. `StyleGuideLayout.razor` and `StyleGuideLayout.razor.css`
are already mapped in `deploy-downloads.ps1`. Scoped `.razor.css` changes
auto-bundle into `LiPi.Web.styles.css`, so no cache-stamp bump is needed.

**Phase 2.6.1 — LipiCard complete.** The component (A26–A27), its seven files,
and the StyleGuide §15 showcase (A28) are all shipped. Remaining 2.6.1 doc debt:
spec `04-LipiCard-Spec.md` §13 / §14 still list four files (the component ships
seven — `CardHeader/Body/Footer.razor` were never enumerated), and
`03-LipiAlert-Spec.md` / `04-LipiCard-Spec.md` are not mapped in
`deploy-downloads.ps1`. A doc-only follow-up.

**Files**:
- `src/LiPi.Web/Pages/StyleGuideLayout.razor` — `#cards` section rebuilt to the
  nine spec §15 demonstrations; `@code` block `_specialties`/`_selected` →
  `_slots`/`_selectedSlot` + `SelectSlot(string)`; section AMEND comment added
- `src/LiPi.Web/Pages/StyleGuideLayout.razor.css` — `Phase 2.6.1 — Cards
  section` block appended (`.sg-card-grid` family, `.sg-card-single`,
  `.sg-demo-note`, `.sg-metric-*`)
- `deploy-downloads.ps1` — no change; both files already mapped

### A29 — Phase 2.6.1 doc cleanup: LipiCard spec §13/§14 file list corrected; Alert + Card spec docs mapped in deploy script

**Phase**: 2 Sub-step 2.6.1 — LipiCard + LipiAlert (documentation only)
**Date**: 2026-05-14
**Status**: ✅ Shipped — Phase 2.6.1 closed

**Trigger.** A27 and A28 both flagged residual Phase 2.6.1 doc debt: the
LipiCard spec's file list was inaccurate, and two of the three 2.6.x component
spec docs were never mapped in the deploy script. This batch clears both — no
code, docs and deploy script only.

**1. `04-LipiCard-Spec.md` §13 / §14 — file list corrected (4 → 7).** The spec
listed four files (`LipiCardTypes.cs`, `LipiCard.razor`, `LipiCard.razor.css`,
`lipi-cards.css`) but the component ships seven — the three named sub-components
`CardHeader.razor`, `CardBody.razor`, `CardFooter.razor` (spec §2) were never
enumerated in §13 ("Files to create") or §14 ("Deploy script additions"). Both
sections now list all seven, and §13's explanatory note describes the
sub-component files. `deploy-downloads.ps1` already mapped all seven correctly —
the gap was in the spec doc only, so no deploy-script change for the component
files themselves.

**2. `deploy-downloads.ps1` — Alert + Card spec docs mapped.** Only
`02-LipiTabs-Spec.md` was mapped in the deploy script's Docs section (added in
A21). `03-LipiAlert-Spec.md` and `04-LipiCard-Spec.md` were never mapped, so
edits to either spec doc could not be deployed via the standard workflow. Both
are now mapped alongside the Tabs spec. (If either file is not yet physically in
`docs/`, drop the existing copy into `Downloads\LiPi\` once — the deploy script
skips files not present in the drop folder, so a one-time placement closes it.)

**Not changed.** `03-LipiAlert-Spec.md` content is accurate as-is — LipiAlert is
a single component with no separate sub-component files (`AlertActions` is an
inline `RenderFragment` parameter, not a `.razor` file), so its §12 / §13 list
of four files is correct. Only its deploy-script mapping was missing. No
component code, CSS, or `App.razor` change in this batch — scoped doc cleanup
only.

**Files**:
- `docs/04-LipiCard-Spec.md` — §13 "Files to create" and §14 "Deploy script
  additions" expanded from four files to seven (`CardHeader.razor`,
  `CardBody.razor`, `CardFooter.razor` added); §13 explanatory note extended to
  describe the sub-component files
- `deploy-downloads.ps1` — `03-LipiAlert-Spec.md` and `04-LipiCard-Spec.md`
  entries added to the Docs section
- `CHANGE-LOG.md` — this amendment (A29)

**Phase 2.6.1 — CLOSED.** LipiTabs (A21–A22), LipiAlert (A23–A25), LipiCard
(A26–A28), and this doc cleanup (A29) complete the phase. Component library,
showcases, specs, and deploy mappings are all consistent. Next: Phase 2.6.2 —
Overlay surfaces (LipiModal + LipiDrawer + LipiDynamicTabs); specs ready, deploy
paths pre-mapped.

### A30 — LipiModalTypes.cs recovery from spec after file loss

**Phase**: 2 Sub-step 2.6.2 — LipiModal infrastructure recovery
**Date**: 2026-05-14
**Status**: ✅ Shipped — Phase 2.6.2 unblocked

**Trigger.** Post-2.6.2-batch review found that the deployed tree referenced
seven modal enums (`ModalSize`, `ModalIconColor`, `ModalIntent`, `ModalAnimation`,
`ModalFooterAlign`, `ConfirmIntent`, `AlertIntent`) across `LipiOverlayHost.razor`,
`LipiModalService`, `ILipiModalService`, `ConfirmDialog`, and `AlertDialog`, but
the source file `LipiModalTypes.cs` was absent. Project would not compile until
the file was reconstructed.

**Filename correction.** The mid-recovery review chat temporarily called the
file `LipiOverlayTypes.cs`. `docs/00-COMPONENTS/2.6.2/01-LipiModal-Spec.md §4`
header is the
authoritative source: filename is `LipiModalTypes.cs`, matching the sibling
pattern `LipiDrawerTypes.cs` / `LipiDynamicTabsTypes.cs`. Shipped as
`LipiModalTypes.cs`.

**Rebuild fidelity.** Spec `§4` is mirrored verbatim — seven enums, no design
changes, no value reordering, no additions. Three of the seven enums
(`ModalIconColor`, `ModalIntent`, `ModalFooterAlign`) are not yet consumed by
deployed code; they wait on `LipiModal.razor` (spec §2), built in A31. Their
inclusion now closes the compile gap and pre-stages the declarative component.

**Files**:
- `src/LiPi.Web/Components/Shared/LipiModalTypes.cs` — new file, 7 enums
- `deploy-downloads.ps1` — `LipiModalTypes.cs` entry added under
  `# LipiModal` block in the Phase 2.6.2 section
- `CHANGE-LOG.md` — this amendment (A30)

---

### A31 — LipiModal declarative component family (spec §2 implementation)

**Phase**: 2 Sub-step 2.6.2 — LipiModal declarative path
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** Spec `01-LipiModal-Spec.md §2` mandates a declarative
`<LipiModal>` component as the primary usage path (programmatic
`ILipiModalService.ShowAsync` is the secondary path). The 2.6.2 batch shipped
the service path (via `LipiOverlayHost.razor` + `DynamicComponent`) but not the
declarative component. A31 closes the gap by building `LipiModal.razor`,
`ModalBody.razor`, `ModalFooter.razor`, and `LipiModal.razor.css`.

**Architecture: declarative renders own DOM, shares services with service path.**
`<LipiModal>` self-renders backdrop + box + header + close button. `ModalBody` /
`ModalFooter` are sibling sub-components that emit their own HTML (no
`CascadingValue` plumbing), matching the `LipiCard` / `CardBody` / `CardFooter`
pattern already locked in 2.6.1. Both paths share `IFocusTrapService` +
`IScrollLockService`. The declarative path does **not** register with the
`ILipiModalService` stack — the spec §6 max-3 stack cap applies to the service
path only. Two simultaneous declarative modals is the consumer's
responsibility; documented as a v1.0 limitation.

**v1.0 spec divergences (locked, with rationale).**

**1. No `.razor.cs` code-behind.** Spec §file-structure lists `.razor.cs`
files for LipiModal / Drawer / DynamicTabs / OverlayHost; project deployment
chose single-file `@code` blocks for all four (LipiDrawer, LipiDynamicTabs,
LipiOverlayHost already shipped this way). LipiModal matches. Spec line 110
needs a future amendment to mirror the project convention.

**2. Intent auto-defaults NOT cascaded (spec §10).** The project's locked
rule rejects nullable enum params (LipiButton `Variant` precedent — A14). Without
nullability we cannot distinguish "consumer set this explicitly" from
"declared default", so the auto-default cascades for
`Confirmation` / `Alert` / `Wizard` / `Preview` intents are NOT applied. Only
the "forced override" rules from spec §8 + Progress are honored:
- `Intent.Progress` → `ShowCloseButton = false`, `CloseOnEscape = false`,
  `CloseOnBackdrop = false`, `Animate = false` (forced)
- `Animation = None` falls out of `Animate = false`

Spec §2 row notes about "Critical" auto-disabling close refer to
`ConfirmIntent.Critical` / `AlertIntent.Critical` (service path, already handled
by `LipiModalService`), NOT to `ModalIntent` — which has no `Critical` value.

**3. ChildContent wins over structured action params; no dev warning.** Spec
§2 says "slot wins, dev warning logged". Detection without `CascadingValue`
plumbing is fragile. Matches LipiDrawer's same divergence; treated as a v1.0
trade-off.

**4. `ModalFooter.Align` local to sub-component, not cascaded from
`<LipiModal FooterAlign>`.** The `<LipiModal>` `FooterAlign` parameter applies
only to the structured action shortcut path (`PrimaryAction` /
`SecondaryAction` without `<ModalFooter>`). For the slot path, set `Align`
directly on `<ModalFooter>`. Keeps sub-components zero-coupled; matches the
LipiCard family pattern.

**5. `AutoFocusSelector` not yet implemented.** Parameter accepted but
focus trap activates on the first focusable element. Future:
`lipiOverlay.focusWithin(container, selector)` JS helper. Tracked for v1.1+.

**6. Z-index defaults to 800** (matches first service modal). Simultaneous
declarative + service overlap visually (same backdrop color, no harm).

**Post-deploy fix (folded into this entry).** Demo 9 in the StyleGuide
showcase fired a runtime exception
(`'LipiButton' does not have a property matching the name 'IsBusy'`) on first
click of the busy demo. Spec §1's declarative example shows
`<LipiButton IsBusy="@_saving">`, but the deployed LipiButton's actual
parameter is `IsLoading` (Phase 2.1). Fixed in three sites:
- `StyleGuideOverlays.razor:791` (Modal demo 9 footer Submit button — runtime
  bug)
- `LipiModal.razor:144` (structured-action shortcut path primary button —
  latent bug, only triggered when consumer used `PrimaryAction=` without
  `<ModalBody>`)
- `LipiModal.razor:56` (doc comment usage example — doc accuracy only)

Spec doc `01-LipiModal-Spec.md §1` declarative example shows
`<LipiButton IsBusy=...>` — should be `IsLoading`. Spec amendment TODO: align
the example with the deployed LipiButton API. Tracked outside this entry.

**Files**:
- `src/LiPi.Web/Components/Shared/LipiModal.razor` — new file, 22 parameters,
  single-file with `@code` block
- `src/LiPi.Web/Components/Shared/LipiModal.razor.css` — minimal scoped focus
  rules
- `src/LiPi.Web/Components/Shared/ModalBody.razor` — sub-component matching
  CardBody pattern
- `src/LiPi.Web/Components/Shared/ModalFooter.razor` — sub-component with
  local `Align` parameter
- `deploy-downloads.ps1` — four new entries under `# LipiModal` block
- `CHANGE-LOG.md` — this amendment (A31)

---

### A32 — Phase 2.6.2 bug sweep + drawer/host architectural cleanup

**Phase**: 2 Sub-step 2.6.2 — bug sweep
**Date**: 2026-05-14
**Status**: ✅ Shipped — Phase 2.6.2 spec-compliant for v1.0

**Trigger.** Systematic post-2.6.2-batch review against the four locked specs
(`00-Phase2.6.2-Overview.md`, `01-LipiModal-Spec.md`, `02-LipiDrawer-Spec.md`,
`03-LipiDynamicTabs-Spec.md`) surfaced eleven confirmed bugs and one
architectural inconsistency. This batch closes all of them. Eight files
revised.

**1. CSS self-sufficiency: `--color-border-tertiary` fallbacks.** The A22 / A26
pattern (locked for LipiTabs / Card) was never applied to the 2.6.2 CSS files.
Eight occurrences in `lipi-overlays.css` (modal header/footer borders, all four
drawer placement borders, drawer header/footer) and three in
`lipi-dynamic-tabs.css` (strip border-bottom, tab border-right, add-button
border-right) updated to
`var(--color-border-tertiary, var(--color-border-default))`. Theme that doesn't
define `--color-border-tertiary` no longer gets borderless overlays/tabs.

**2. `LipiModalService` stack guard off-by-one (spec §6).** Spec defines max
stack depth of 3. Original code warned when `_stack.Count >= 3` (allowing the
4th push) and threw when `_stack.Count >= 4` (allowing 5). Rewrote to warn at
projected-depth = 2 (about to reach max), throw at projected-depth >= 3.
`MaxStackDepth = 3` constant added.

**3. LipiDrawer missing six spec'd parameters (spec §2).** Added: `SizePx`
(px override; sets width for R/L, height for T/B), `IsBusy` (aria-busy +
disable buttons), `Animate` (no-animate class), `AutoFocusSelector` (v1.0
no-op, accepted for forward compat), `RouteScopePrefix` (route-scoped pin
auto-close), `DefaultPinned` (initial pinned state when no localStorage value
exists).

**4. LipiDrawer pin mode completion (spec §4.5).** Original pin button was a
visual stub — toggling state and writing to localStorage but doing nothing
else. Closed four gaps:
- **localStorage read on first render** (was write-only)
- **NavigationManager subscription** for `PinScope.PageScoped` (close on any
  nav) and `PinScope.RouteScoped` (close when leaving `RouteScopePrefix`)
- **`:has()`-based CSS page-shift** — pinned R/L drawers add
  `.lipi-drawer-pinned`, CSS shifts main content via `:has()` selector (no JS
  interop needed)
- **Responsive @media disable below 1024px** — pin behavior auto-disabled on
  narrow viewports per spec §4.5

**5. LipiDynamicTabs `@implements IDisposable` (subscription leak).** The
component had a `public void Dispose()` method but no `IDisposable`
implementation, so Blazor never called it. `OnTabsChanged` event subscription
leaked on every component unmount. Adding the interface wires the existing
Dispose into Blazor's lifecycle.

**6. LipiDynamicTabs `MaxTabs` cap enforcement (spec §7 step 4).** `MaxTabs`
parameter was declared but never read. Added `void SetMaxTabs(int)` to
`ILipiDynamicTabsService`; `LipiDynamicTabsService.OpenAsync` now checks the
cap before appending. Cap hit → `AlertAsync` warning per spec §7 step 4.
Component's `OnInitialized` and `OnParametersSet` call `SetMaxTabs` to keep
service state in sync.

**7. LipiDynamicTabs keyboard navigation (spec §13).** Original handler only
supported Enter/Space. Added ArrowLeft / ArrowRight (move focus), Home/End
(jump to first/last), Delete (close focused tab). v1.0 compromise: arrow keys
also activate (navigate). Spec §13 wants focus separation from activation —
requires JS roving-tabindex coordination, tracked for v1.1+.

**8. `LipiOverlayHost` `_announcement` aria-live region (spec Overview
line 212).** The host declared and rendered an `aria-live="polite"` region but
never wrote to it. Now announces overlay open/close events for screen readers.

**9. `LipiOverlayHost` `OnBackdropPointerDown` no-op (spec Overview §168
ghost-click prevention).** The host bound `@onpointerdown` to a method
containing only a comment. Replaced with declarative
`@onpointerdown:stopPropagation="true"` — ghost-click prevention per spec.

**10. `LipiOverlayHost` drawer-on-modal dev warning (spec Drawer §6).** Spec
allows but discourages drawer-opening-while-modal-active. Host now logs a dev
warning when this combination is detected.

**11. `LipiOverlayHost` initialization edge case.** `OnInitialized` did not
snapshot existing modal/drawer state before subscribing. If the service
already had overlays when the host first rendered, they did not appear until
the next state change. Now snapshots existing state. `_modalRefs` array sized
to `MaxStackDepth = 3` (was hardcoded 4).

**Architectural cleanup (Bug #12, listed separately because it changes
shape).** `LipiOverlayHost` was rendering drawer HTML inline instead of using
`<LipiDrawer>`. Result: service-driven drawers rendered with fewer features
than declarative drawers (no pin button, no icon coloring, no dirty-state
confirm). Refactored host to render `<LipiDrawer>` with parameters bound from
the `DrawerRequest`. Service-driven and declarative drawers now feature-
equivalent for everything `DrawerRequest` carries. Icon / IconColor /
Pinnable / IsDirty still require `DrawerRequest` extensions — tracked for
v1.1+.

**v1.0 limitations documented (defer to v1.1+).**
- `OverflowMode.Dropdown` (LipiDynamicTabs) — needs strip-width measurement +
  collapse menu. Selecting `Dropdown` logs warning + falls back to Scroll.
  (Subsequently superseded by A34's chevron overflow redesign.)
- `AutoFocusSelector` on LipiModal and LipiDrawer — accepted but no-op; needs
  new `lipiOverlay.focusWithin` JS helper.
- LipiDynamicTabs arrow-key focus separation from activation (spec §13).
- Service-driven drawers can't carry Icon/IconColor/IsDirty/Pinnable — needs
  `DrawerRequest` extension.

**Out of scope (Phase 2.10).** `Program.cs` legacy `AddServerSideBlazor()`
cleanup and duplicate `IEmailService` registration are locked Phase 2.10 work;
not touched here.

**Files**:
- `src/LiPi.Web/wwwroot/css/lipi-overlays.css` — 8× border fallbacks +
  `.lipi-modal-header-text` wrapper + `.lipi-drawer-no-animate` opt-out +
  `:has()`-based pinned page-shift + responsive disable + pinned visual tweaks
- `src/LiPi.Web/wwwroot/css/lipi-dynamic-tabs.css` — 3× border fallbacks
- `src/LiPi.Web/Services/LipiModalService.cs` — stack guard rewrite,
  `MaxStackDepth` constant
- `src/LiPi.Web/Services/ILipiDynamicTabsService.cs` — `SetMaxTabs(int)` added
- `src/LiPi.Web/Services/LipiDynamicTabsService.cs` — `_maxTabs` field, cap
  check in `OpenAsync`, self-navigation skip in `ActivateTab`
- `src/LiPi.Web/Components/Shared/LipiDynamicTabs.razor` — `IDisposable`,
  `SetMaxTabs` propagation, keyboard expansion, Dropdown fallback warning,
  dead-ternary cleanup
- `src/LiPi.Web/Components/Shared/LipiOverlayHost.razor` — drawer-rendering
  refactor to `<LipiDrawer>`, `_announcement` writes, declarative
  pointer-down, drawer-on-modal warning, initial state snapshot,
  `_modalRefs[3]`
- `src/LiPi.Web/Components/Shared/LipiDrawer.razor` — 6 new params,
  localStorage read, NavigationManager subscription, `.lipi-drawer-pinned`
  class wiring
- `deploy-downloads.ps1` — Phase 2.6.2 header annotated with A30/A31/A32
  summary
- `CHANGE-LOG.md` — this amendment (A32)

---

### A33 — StyleGuideOverlays full 30-demo showcase rewrite

**Phase**: 2 Sub-step 2.6.2 — showcase
**Date**: 2026-05-14
**Status**: ✅ Shipped

**Trigger.** Spec coverage gap surfaced during review: the deployed
`StyleGuideOverlays.razor` had only ~5–6 stub demos against ~30 spec-required
(Modal §12: 11, Drawer §11: 8, DynamicTabs §14: 11). A32 closed the component
bugs, leaving the showcase as the remaining gap. Existing stubs discarded
wholesale; rebuilt fresh against working components.

**Layout matches main StyleGuide page.** Topbar + sticky sidebar with 3 nav
groups (Modal / Drawer / DynamicTabs) + main content area with sg-* scoped
sections. Each demo has a numbered pill, heading, descriptive paragraph,
trigger button(s), and inline overlay markup at the end of the file. Demos
that cannot show live behavior (e.g., the v1.0-deferred Dropdown overflow
mode) include a code block + sg-note callout instead.

**Demo coverage.**
- **Modal §12 (11/11):** sizes (4), IconColor (6), animations + toggle,
  ConfirmAsync, AlertAsync, PromptAsync, ShowAsync custom component, IsDirty,
  IsBusy, stack (modal-opens-confirm), 6 Intent presets
- **Drawer §11 (8/8):** 4 placements, sizes per placement, 3 backdrops, dirty
  form, programmatic ShowAsync, R+B simultaneous, modal-on-drawer, IsBusy
- **DynamicTabs §14 (11/11):** empty state, 3-open, dirty marker, close clean,
  close dirty, save-ok, save-fail, cap (sets 3, opens 4), scroll overflow (12
  tabs), Dropdown fallback note, add button

**Helper components introduced.**
- `SampleCustomModal.razor` — body component for `Modal.ShowAsync<SampleCustomModal, string?>`
  demo 7. Takes `InitialValue` parameter, returns string via `OnResult`.
- `SampleDrawerPanel.razor` — body component for `Drawer.ShowAsync<SampleDrawerPanel, bool>`
  demo 5. Returns bool via `OnResult`. Documents v1.0 service-path limitation
  (no Icon/IconColor/IsDirty/Pinnable yet) inline.
- `StyleGuideOverlayTabDemo.razor` — stub `@page "/admin/style-guide-overlays/tab/{TabId}"`
  for DynamicTabs nav demos. Registers a save handler based on TabId prefix:
  `save-ok-*` → succeeds; `save-fail-*` → throws (demonstrates spec §7 step 4
  "tab stays open" behavior).

**v1.0 limitations surfaced in-page via sg-note callouts.**
- Modal demo 11 (Intents): A31 — Critical/Progress force-overrides only;
  Confirmation/Alert/Wizard/Preview stay at declared defaults.
- Drawer demo 5 (service path): A32 — `DrawerRequest` doesn't carry
  Icon/IconColor/IsDirty/Pinnable (v1.1+).
- DynamicTabs demo 10 (Dropdown overflow): A32 — falls back to Scroll with a
  warning log; static code block + sg-note explain expected v1.1 behavior.

**Build/runtime fixes folded into this entry.**

**1. Missing `@using Microsoft.AspNetCore.Authorization`.** Both new `@page`
files (`StyleGuideOverlays.razor` and `StyleGuideOverlayTabDemo.razor`) failed
to compile with CS0246 on `[Authorize]`. The `Components/_Imports.razor`
covers components but not page-level attributes; every page using
`[Authorize]` needs the using directly (same pattern as `StyleGuide.razor` and
`Admin.razor`). Added.

**2. `<LipiButton IsBusy=...>` runtime exception.** See A31 fix details —
demo 9 used the wrong parameter name; LipiButton's deployed API is
`IsLoading`. Fixed at demo site; same fix applied to `LipiModal.razor:144`
(latent) in A31.

**3. Demo 10 garbage `@@code @{}` construct.** The Dropdown overflow demo's
code block contained `@@code @{ string sample = "..."; }` in source — leftover
copy-paste residue from an earlier draft. Razor evaluated `@@` as a literal
`@` escape and the `@{}` block as a silent code block, producing visible
text `@code <LipiDynamicTabs ... />`. Replaced with clean HTML-encoded code
sample.

**Out of scope.** No component-code changes in this batch — A33 is showcase
plus three helper files. Phase 2.6.2 component family is locked in A30–A32.

**Files**:
- `src/LiPi.Web/Pages/StyleGuideOverlays.razor` — 1269 lines, all 30 demos
  inline
- `src/LiPi.Web/Pages/StyleGuideOverlays.razor.css` — sg-* scoped styles:
  topbar, sidebar grid, section cards, demo-num pill, btn-row, result/code
  blocks, sg-note (apricot v1.0 callout), variant grids
- `src/LiPi.Web/Pages/StyleGuideOverlayTabDemo.razor` — stub `@page` for
  DynamicTabs nav demos
- `src/LiPi.Web/Components/Shared/SampleCustomModal.razor` — Modal ShowAsync
  body component
- `src/LiPi.Web/Components/Shared/SampleDrawerPanel.razor` — Drawer ShowAsync
  body component
- `deploy-downloads.ps1` — three new entries (SampleCustomModal,
  SampleDrawerPanel, StyleGuideOverlayTabDemo); StyleGuideOverlays.razor +
  .razor.css mappings already present
- `CHANGE-LOG.md` — this amendment (A33)

---

### A34 — LipiDynamicTabs overflow redesign: native scrollbar → chevron buttons

**Phase**: 2 Sub-step 2.6.2 — LipiDynamicTabs spec amendment
**Date**: 2026-05-14
**Status**: ✅ Shipped — Phase 2.6.2 closed

**Trigger.** Smoke-testing A33's DynamicTabs demos surfaced a UX mismatch: spec
§8 specified a thin native horizontal scrollbar for `OverflowMode.Scroll`, but
the clinical-workstation context (10+ patient tabs typical, mouse + keyboard
primary, touch secondary) is better served by the convergent
desktop-productivity pattern — chevron buttons on both ends. The native
scrollbar is touch-first / mobile-first; chevrons are desktop-productivity-
first. Spec amendment locked after debating four design questions in build
chat.

**Industry-pattern research surfaced before locking.** Five reference apps
audited (Chrome / Edge / VS Code / Visual Studio / JetBrains / Firefox).
Convergent pattern (3 of 5):
- Both-end chevrons, inside strip footprint
- Hidden when no overflow exists (avoids visual noise)
- Disabled (greyed, not hidden) when at that edge (preserves spatial layout)
- Hold-to-scroll with acceleration

**Locked design decisions (build chat).**

**1. Chevron visibility — hidden when no overflow** (Chrome / Firefox /
JetBrains pattern). Alternative considered: VS Code's "always shown, disabled
when no overflow". Chose hide-when-none because the strip itself acts as the
"you have tabs" signal when not overflowing; chevrons are pure overflow
affordances.

**2. At-edge state — greyed/disabled, never hidden** (VS Code / JetBrains /
Chrome pattern). Alternative considered: Firefox's hide-when-at-edge. Chose
greyed to preserve spatial layout — avoids the user re-targeting their
pointer after the strip width shifts mid-interaction.

**3. Click scroll — page-like (clientWidth − tabWidth)**. Alternative
considered: single-tab and fixed 120px. Page-like chosen for productivity —
one click moves a visible page's worth of tabs into view.

**4. Hold-to-scroll — yes, with acceleration**. Matches all five reference
apps. 400ms delay → 120ms interval → 60ms after 1 second of holding.
Alternative considered: click-only for simpler v1.0. Hold chosen because
clinical workstations with 15+ patient tabs benefit from rapid scanning
without 15 separate clicks.

**Implementation.**

The strip is now wrapped in `.lipi-dtabs-strip-wrapper` — a flex container
hosting the strip plus two chevron buttons (`.lipi-dtabs-chevron-left` and
`.lipi-dtabs-chevron-right`). Native scrollbar hidden via
`scrollbar-width: none` + `::-webkit-scrollbar { display: none }`. Strip
retains `overflow-x: auto` because programmatic `scrollIntoView` (keyboard
navigation) needs a scrollable container — only the visible scrollbar is
removed. Chevrons drive `scrollBy({ left: ±(clientWidth − tabWidth), behavior:
'smooth' })`.

Per-strip state lives in a JS WeakMap keyed by the strip element. Overflow
detection uses a `ResizeObserver` (with `window.resize` fallback) plus a
`MutationObserver` on the strip's child list — both updates set
`data-overflow` / `data-can-scroll-left` / `data-can-scroll-right`
attributes on the wrapper, which CSS reads for visibility (fade in 180ms
ease-out) and at-edge state (chevron `disabled` + `aria-disabled="true"` +
`opacity: 0.35`). Hold-to-scroll uses `setTimeout(400ms)` for the delay,
then `setInterval(120ms)` for the repeat tier, with a second
`setTimeout(1000ms)` swapping to `setInterval(60ms)` for acceleration.
`pointerup` / `pointerleave` / `pointercancel` clear all timers; reaching
the boundary mid-hold auto-stops the timer.

`LipiDynamicTabs.razor` switched from `IDisposable` to `IAsyncDisposable` —
`DisposeAsync` now also detaches the JS observers + any running scroll
interval. Prevents the same kind of subscription leak A32 fixed for
`OnTabsChanged`.

**Spec amendment.** `docs/00-COMPONENTS/2.6.2/03-LipiDynamicTabs-Spec.md §8`
("Visual design"
overflow handling subsection) and §10 ("Multi-industry tokens") rewritten to
mirror the new behavior. New token `--lipi-dtab-chevron-w: 32px` added to
§10. `TabOverflowMode` enum unchanged — both values stay; `Scroll` is now
chevron-driven, `Dropdown` still parks for v1.1+ with the existing
`LogWarning` fallback.

**Accessibility.**
- Chevrons: `aria-label="Scroll tabs left"` / `"Scroll tabs right"`
- `aria-disabled="true"` mirrored from at-edge state
- `aria-hidden="true"` on the Lucide icon inside
- Keyboard skips disabled chevron (browser default with `disabled` attribute)
- `tabindex="-1"` on both chevrons — they are mouse affordances; keyboard
  users navigate tabs with arrow keys (spec §13)
- Reduced-motion: `@media (prefers-reduced-motion: reduce)` swaps smooth-
  scroll for instant scroll and removes the chevron fade transition

**What does NOT change.**
- `TabOverflowMode` enum values
- `ILipiDynamicTabsService` API
- `LipiDynamicTabsService` implementation
- Tab strip ARIA structure (`role="tablist"`, etc.)
- All 30 demos in `StyleGuideOverlays.razor` — naturally exercise new chevron
  behavior

**Folded into A34: deploy-script mapping fix (A29 pattern, Phase 2.6.2
specs).** During the close-out drop into `Downloads\LiPi\` (67 files), audit
surfaced that `03-LipiDynamicTabs-Spec.md` was not mapped in
`deploy-downloads.ps1`'s Docs section. Same class of gap A29 closed for the
Phase 2.6.1 specs (LipiAlert / LipiCard). Root cause: none of the four
Phase 2.6.2 spec docs (`00-Phase2.6.2-Overview.md`, `01-LipiModal-Spec.md`,
`02-LipiDrawer-Spec.md`, `03-LipiDynamicTabs-Spec.md`) had ever been mapped,
so edits to any of them could not flow through the standard deploy workflow.
The 2.6.2 spec numbering restarts at 00 (vs 2.6.1's 02/03/04) because the
specs live in a dedicated subfolder, `docs\00-COMPONENTS\2.6.2\`, avoiding the
prefix collision with the 2.6.1 specs in `docs\`. All four mappings added to
the deploy script in this batch — closes the same workflow gap A29 identified
one phase earlier. The sibling reference doc
`03-LipiDynamicTabs-Spec-A34-Amendment.md` (shipped earlier in this session
so the spec maintainer could fold A34 §8 + §10 into the main spec) is now
obsolete and removed from `Downloads\LiPi\`.

**Files**:
- `src/LiPi.Web/wwwroot/css/lipi-dynamic-tabs.css` — new wrapper + chevron
  rules; native scrollbar hidden; existing rules preserved
- `src/LiPi.Web/Components/Shared/LipiDynamicTabs.razor` — wrapper markup with
  chevron buttons bracketing strip; 4 pointer handlers; `OnAfterRenderAsync`
  wires `lipiDtabs.attach`; `IAsyncDisposable` replaces `IDisposable`
- `src/LiPi.Web/wwwroot/js/lipi-overlay-interop.js` — new `window.lipiDtabs`
  namespace appended: `attach` / `detach` / `startScroll` / `stopScroll`,
  per-strip state in WeakMap, ResizeObserver + MutationObserver + scroll
  listener
- `src/LiPi.Web/App.razor` — cache version bumped 20260524 → 20260525 (all
  23 stamps), version history table extended with A30–A34 row
- `docs/00-COMPONENTS/2.6.2/03-LipiDynamicTabs-Spec.md` — §8 + §10 rewritten
  in place, A34 amendment marker added in header
- `deploy-downloads.ps1` — Phase 2.6.2 header annotated with A34 entry; four
  new Docs-section mappings added for the Phase 2.6.2 specs
  (`00-Phase2.6.2-Overview.md`, `01-LipiModal-Spec.md`, `02-LipiDrawer-Spec.md`,
  `03-LipiDynamicTabs-Spec.md`) under `docs\00-COMPONENTS\2.6.2\`
- `CHANGE-LOG.md` — this amendment (A34)

**Phase 2.6.2 — CLOSED.** LipiModalTypes recovery (A30), LipiModal declarative
family (A31), bug sweep + drawer pin completion (A32), StyleGuideOverlays
30-demo rewrite (A33), and DynamicTabs chevron overflow redesign (A34)
complete the phase. Component library, showcases, specs, and deploy mappings
are all consistent. Next: Phase 2.7 — Feedback components (LipiSkeleton,
LipiBadge, LipiSpinner, LipiToast, LipiValidationSummary), per the original
Phase 2.6.2 close-out plan.

## v1.1 — PLANNED (Future)

### Pending Items (Move from PARKED → v1.1)
- [ ] Teleconsult feature (after 10+ modules complete)
- [ ] Auto patient identifier verification (Aadhaar, ABHA via DigiLocker)
- [ ] Real-time bed management (IPD)
- [ ] PACS integration (Radiology)
- [ ] **DST ambiguity handling for `LipiDateTimePicker`** (deferred from Phase 2.4
      — A20). Component composes `DateOnly + TimeOnly → DateTimeOffset` using
      clinic timezone offset. For timezones without DST (Asia/Kolkata IST,
      current LiPi market), this is unambiguous always. For timezones with DST,
      the fall-back hour creates two valid offsets for one wall-clock time and
      the spring-forward hour creates invalid times. Resolve when international
      expansion requires DST handling. Will need explicit UI to disambiguate
      (e.g., "Was this {std} or {dst}?" radio for fall-back hour).
- [ ] **IME composition support** for date/time segment inputs (deferred from
      Phase 2.4 — A20). CJK/IME composition fires intermediate `oninput` events
      that may trigger auto-advance prematurely. India + Latin scripts unaffected.
      Resolve when international expansion adds fields with composed-script input.
- [ ] **Smart paste in Segments mode** (deferred from Phase 2.4 — A20). Pasting
      a full ISO date string into a Segments-mode `LipiDatePicker` should split
      across segments with correct token mapping. Currently truncates first
      segment to MaxLength. Implement when real users start pasting in Segments
      mode.
- [ ] **`LipiDatePicker` explicit auto-focus from date to time HH** (deferred
      from Phase 2.4 — A20, decision D2.1). Currently relies on natural Tab
      order — user tabbing forward from the date popover lands on time HH next.
      Auto-focus would need component-level coordination (e.g., exposing a
      `FocusFirstSegmentAsync` method on `LipiTimePicker`). Implement if
      real-world UX shows users miss this.
- [ ] **Armoki brand theme** (after Armoki finalizes brand identity)
- [ ] **Auto theme mode** (follows OS preference)
- [ ] **High-contrast theme mode** (accessibility)
- [ ] **Density toggle** (user preference: comfortable/compact/spacious)
- [ ] **A7 — ThemeProvider spec/code reconciliation** (see Known divergences above)
- [ ] **LipiSelectBase scroll-highlighted-option-into-view fix** (latent gap surfaced during A19 / Batch 9b.3). `LipiSelectBase`'s keyboard navigation has the same defect that `LipiMultiSelectBase` had pre-9b.3 — ArrowUp/Down past the visible scroll region updates `_highlightedIndex` but doesn't scroll the option DOM element into view. Currently rare because `Searchable=true` is the default for `LipiSelect` (users filter rather than jump). Address as a focused 1-file batch when the symptom surfaces in production. Reuse the `lipiInput.scrollOptionIntoView` JS helper shipped in 9b.3.

### Infrastructure Phases (Triggered by Real Module Demand — see A17 strategic context)

These are deliberate cross-cutting infrastructure builds, not features. Each has a real-demand trigger that prevents speculative inline rewriting during feature work. Memory rule #26 enforces this: future chats must NOT inline-rewrite these pieces during feature development; they wait for the trigger.

- [ ] **LipiMail service** — Application-layer wrapper over SMTP (initially MailKit, possibly SaaS provider later) with: queueing, templating, audit logging, multi-tenant SMTP config (per-clinic FROM addresses), retry / circuit breaker, delivery tracking for HIPAA audit. **Trigger**: first module needing structured email beyond OTP / password-reset (likely Lab or Radiology report delivery). **Until trigger**: thin SMTP usage in `SmtpEmailService.cs` continues, kept on a non-vulnerable MailKit version (currently 4.16.0 per A17).

- [ ] **LipiHttp service** — Application-layer wrapper over `HttpClient` for outbound integrations (auth handling, retry, circuit breaker, audit logging). **Trigger**: first external integration shipping (likely ABDM consent or DigiLocker Aadhaar verification, both queued behind Patient Module completion).

- [ ] **LipiApi service** — Higher-level convenience layer on top of LipiHttp for typed REST API consumption (request/response DTOs, deserialization, error envelope normalization, observability). **Trigger**: same as LipiHttp — both ship as a single infrastructure phase since LipiApi depends on LipiHttp.

### Pending Decisions (Not Locked)
- [ ] Insurance TPA workflows (decision pending)
- [ ] Multi-language support priority (Hindi/Tamil/Marathi/etc.)
- [ ] Mobile app strategy (native/PWA/responsive only)

---

## CHANGE TEMPLATE (Use for new entries)

```markdown
## v1.X — [DATE]

### Changed
- [Module: NN-Name] What changed and why
- Refer to: docs/[N]-MODULE/[N].1-Design-Specs.md (sections updated)

### Added
- [Module: NN-Name] New feature

### Deprecated
- [Module: NN-Name] What's being phased out

### Removed
- [Module: NN-Name] Removed feature/spec
```

---

## RULES FOR EDITING THIS FILE

1. ✅ NEVER edit v1.0 entries — they are locked
2. ✅ All changes go to a NEW version section (v1.1, v1.2, etc.)
3. ✅ Reference specific files/sections that changed
4. ✅ Date every entry
5. ✅ Get sign-off from Arun (project owner) before adding entries
