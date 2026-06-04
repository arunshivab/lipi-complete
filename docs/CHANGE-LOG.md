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

---

## v1.0 — AMENDMENTS (May 15, 2026)

> Phase 2.7 — Feedback Components family. Single consolidated amendment (A35)
> covering all six components per the locked "one batch, six components" plan.
> No mid-phase recoveries or follow-up bug sweeps — the family was built
> sequentially in strict handout order (tokens → Spinner → Badge → Pill →
> Skeleton → ValidationSummary → Toast → wiring → StyleGuide → deploy).

### A35 — Phase 2.7 Feedback Components family (LipiSpinner + LipiBadge + LipiPill + LipiSkeleton + LipiValidationSummary + LipiToast)

**Phase**: 2 Sub-step 2.7 — Feedback Components family
**Date**: 2026-05-15
**Status**: ✅ Shipped — Phase 2.7 closed in one batch

**Scope.** Six components covering loading states, status indicators,
placeholders, and notifications. Zero external libraries. Built on the token
system + LipiAlert (Phase 2.6.1) + LipiOverlayHost pattern (Phase 2.6.2). No
new infrastructure services beyond `ILipiToastService` itself.

**Components.**

1. **LipiSpinner** — general-purpose loading indicator. Distinct from
   `LipiButtonSpinner` (button-internal, small only). 4 sizes × 4 intents +
   custom `SizePx` / `Color` overrides + 4 label positions (reuses Phase 2.5.5
   `InputLabelPosition`). Pure-render SVG, no JS, no events. Reduced-motion
   slows rotation from 1.0s to 2.5s (slower-but-still-rotating preserves the
   "still loading" signal vs static which is ambiguous).

2. **LipiBadge** — attached count/dot indicator (parent must be
   `position: relative`). 6 colors × 4 corner positions + inline mode +
   `Max` clamping ("N+" when Count > Max, default Max=99) + `Dot="true"`
   pure indicator + `ShowZero` toggle. Default `Color="BadgeColor.Danger"`
   matches the universal notification convention. `box-shadow` outline ring
   in surface color creates the "punched out" look on icon hosts.

3. **LipiPill** — standalone label/tag/chip. 7 intents × 3 variants (Filled,
   Outlined, Subtle) × 3 sizes + `Icon` prefix (Lucide) + `Dismissible` ×
   button. Sibling pattern to LipiBadge — Badge attaches to a parent, Pill
   stands alone in text flow. `Subtle` variant uses the `*-text-strong`
   token family added in A12 for accessible text-on-pale-bg contrast.

4. **LipiSkeleton** — three single-file pure-render primitives
   (`LipiSkeletonLine`, `LipiSkeletonCircle`, `LipiSkeletonRect`) composed
   by consumers to match the shape of loading content. No per-primitive
   scoped CSS — family shares `lipi-skeleton.css` for the shimmer keyframe
   + 3 shape classes + reduced-motion handling (animation off, opacity 0.5).
   Shimmer uses translateX(±100%) overlay (GPU-accelerated, works on any
   width) rather than background-position animation.

5. **LipiValidationSummary** — form-level error summary. Auto-discovers errors
   from cascading `EditContext` (subscribes to `OnValidationStateChanged` +
   `OnFieldChanged`), resolves field labels via `[Display(Name)]` attribute
   reflection (fallback to PascalCase-to-space conversion), and provides
   click-to-field navigation via JS interop (`window.lipiValidation.scrollToField`).
   1.5s amber flash ring on the focused field. Built on `LipiAlert` (no new
   visual styling); adds intelligence layer only. Two modes: `Detailed`
   (bulleted list, default) and `Compact` (single-line count summary for
   very long forms). `Placement` is a `[Flags]` enum supporting Top, Bottom,
   or Both (consumer places the component twice for Both pattern).

6. **LipiToast** — service-driven transient notifications. `ILipiToastService`
   (Scoped per circuit, registered in `Program.cs`). 4 severities × 6 corner
   positions + persistent flag + dedup by `Id` + queue beyond `MaxVisible=4`
   + single inline action button (max 12 chars) + Promise-style morph
   (Loading → Success/Error in place, same Id). **Errors are persistent by
   default** — clinical safety decision (must be acknowledged manually).
   Toast NEVER steals focus (informational only — keyboard users navigate
   via Tab from page content). `role="alert"` + `aria-live="assertive"` for
   Error, `role="status"` + `aria-live="polite"` for the other three. Host
   (`LipiToastHost`) renders into `TopNavLayout` at z-index 900 (above
   modals + drawers at 800/700).

**Token additions (mode-light.css + mode-dark.css).** A35 introduces 7 new
tokens, one of which fills a long-standing semantic gap (danger text-strong).

Light mode:
- `--color-danger-text-strong: #991B1B` (red-800) — completes the `*-text-strong`
  ladder introduced in A12. Used by LipiValidationSummary error links, LipiToast
  error icons, and LipiPill outlined-danger variant. Hex collides with
  `--color-danger-active` (same hex, different role: text-on-tinted-bg vs
  button-active-fill — the same value serving two distinct semantic roles is
  intentional and documented in the mode file).
- `--lipi-skeleton-bg: var(--color-bg-subtle)` — base block color (one shade
  off the typical bg-surface card).
- `--lipi-skeleton-highlight: rgba(255, 255, 255, 0.50)` — shimmer sweep alpha.
- `--lipi-spinner-duration: 1.0s` — base rotation period (overridable per parent).
- `--lipi-spinner-track: rgba(15, 45, 94, 0.10)` — navy-tinted faint backing ring.
- `--lipi-toast-bg: var(--color-bg-surface)` — elevated toast surface.
- `--lipi-toast-border: var(--color-border-default)` — toast outline (subtle;
  the colored 4px left bar carries severity, not the border).
- `--lipi-toast-shadow: 0 8px 24px rgba(15, 45, 94, 0.16), 0 2px 6px rgba(15, 45, 94, 0.08)`
  — heavier than `--sh-md` so toast reads as floating.
- `--lipi-badge-z: 10` — stacking context default (above icon hosts, below
  dropdown menus).

Dark mode tokens follow the existing convention — `--color-danger-text-strong:
#FCA5A5` (red-300, matching the "100-200 brighter than base" pattern of
success/warning/info text-strong); `--lipi-skeleton-bg: var(--color-bg-elevated)`
(one shade lighter than bg-surface so skeleton reads slightly raised);
`--lipi-skeleton-highlight: rgba(255, 255, 255, 0.10)` (lower alpha because
white-on-dark sweep is intrinsically more visible than white-on-light);
toast shadow uses higher alpha (`0 8px 24px rgba(0, 0, 0, 0.50)`).

**Spec → deployed token name mapping (audit findings).** Phase 2.7 specs use
some token names that diverge from the deployed token set. All Phase 2.7
component CSS files use the deployed names; the spec-to-deployed mapping is
documented in each CSS file header:

| Spec name | Deployed name |
|-----------|---------------|
| `--color-success-fill` | `--color-success` |
| `--color-danger-fill`  | `--color-danger` |
| `--color-warning-fill` | `--color-warning` |
| `--color-info-fill`    | `--color-info` |
| `--color-bg-strong`    | `--color-border-strong` (neutral filled bg) |
| `--color-bg-muted`     | `--color-bg-subtle` (Skeleton base) |
| `--color-text-muted`   | `--color-text-tertiary` (Spinner subtle intent) |
| `--color-success-text` | `--color-success-text-strong` (A12) |
| `--color-success-subtle` | `--color-success-pale` |

Spec docs left unchanged — the deviation is captured in code comments and in
this amendment. If a future audit decides to rename the deployed tokens,
both the specs and the component CSS need updating together; currently the
deployed names are authoritative.

**Locked design decisions surfaced during build.**

1. **Errors persistent by default** (LipiToast). Clinical safety decision —
   an error toast that auto-dismisses while a nurse is reaching for the
   keyboard is a silent failure mode. Consumer can override via
   `ToastOptions.DurationMs > 0` for genuinely transient errors. Documented
   in `ILipiToastService` XML doc + spec §7.

2. **Toast does NOT steal focus** (LipiToast). Spec §10. Toast is
   informational; focus stealing would interrupt typing, form filling, and
   keyboard navigation. The action button (if present) is keyboard-reachable
   via Tab from page content. No autofocus.

3. **`InputLabelPosition` reused** (LipiSpinner). Rather than introducing
   `SpinnerLabelPosition`, Spinner consumes the Phase 2.5.5 cross-family
   `InputLabelPosition` enum. Same 4 directions (Top/Right/Bottom/Left).
   Future label-position-bearing components (e.g., LipiCheckbox future
   variants) reuse the same enum.

4. **Skeleton primitives are single-file pure-render** (LipiSkeleton).
   Deviation from the general handout literal of split `.razor` +
   `.razor.cs`. Each primitive is so small (4 parameters, 1 derived class
   string, inline-style emission) that a partial class adds ceremony
   without benefit. Documented inline in each primitive file header.

5. **Validation summary's object-level errors out of scope for v1.0
   auto-discovery** (LipiValidationSummary). EditContext's object-level
   FieldIdentifier isn't cleanly reachable via the public API without
   reflection that risks breaking on framework upgrades. Consumer with
   object-level rules supplies `Errors` explicitly (with `FieldName = "*"`).
   Documented limitation in `RefreshErrorsFromContext` inline comment.

6. **ShouldRender_Custom naming on LipiBadge** (LipiBadge). The derived
   "should this render anything at all" property is named with the
   `_Custom` suffix to avoid colliding with Blazor's protected
   `bool ShouldRender()` override (different signature, different purpose).
   The latter is used for component-level render skipping; the former
   gates visibility based on Count + ShowZero. Suffix documented in the
   `.razor` file's `@code` block.

**Integration points.**

- `App.razor` — cache version bumped 20260525 → 20260526 across all 23 stamps
  (CSS + JS) + theme-switcher.js. New `<link>` for `lipi-skeleton.css` and
  `lipi-validation.css` added to the cascade order (positions 15 and 16, after
  the Phase 2.6.2 overlay CSS, before the isolation bundle). New `<script>`
  for `lipi-validation.js` added before `blazor.web.js` (same pattern as
  `lipi-overlay-interop.js` from Phase 2.6.2). Version-history table updated
  with new 20260526 row.

- `TopNavLayout.razor` — `<LipiToastHost />` mounted inside `.tn-shell` after
  the existing `<LipiOverlayHost />`. Single host per circuit; renders ALL
  active toasts at any of 6 corner positions at z-index 900 (above overlay
  surfaces). MainLayout (unauthenticated) doesn't get toast rendering in
  v1.0 — no requirement surfaced for it yet.

- `Program.cs` — `builder.Services.AddScoped<ILipiToastService, LipiToastService>()`
  registered in the new Phase 2.7 services block. Note: legacy
  `builder.Services.AddServerSideBlazor().AddCircuitOptions(o => o.DetailedErrors = true)`
  retained as-is per the locked roadmap — its migration is a Phase 2.10
  Infrastructure Audit item, NOT a Phase 2.7 deliverable. Removing it without
  the migration breaks Blazor events (documented critical bug fix).

- `StyleGuide.razor` — new "Phase 2.7" sidebar nav group with a single
  consolidated link to `/admin/style-guide/feedback`. Surgical 10-line
  insertion between the existing Phase 2.6 group and the closing `</aside>`.
  Same `data-enhance-nav="false"` rationale as the Phase 2.6 cross-links.

- `StyleGuideFeedback.razor` (new) — single consolidated showcase covering
  all 6 components, ~20 demos total: Spinner (4 demos), Badge (4), Pill (5),
  Skeleton (3), ValidationSummary (2), Toast (6). Mirrors the
  StyleGuideOverlays.razor pattern (sidebar nav + sg-section + sg-demo-num).

**Architectural divergences (documented for Phase 2.10 audit).**

1. `--color-danger-text-strong` light-mode hex (`#991B1B`) collides with
   `--color-danger-active`. Two different semantic roles share one value.
   This is fine for v1.0 but if either role's value changes, the other token
   must be reconsidered. Not a bug — a documented dependency.

2. Spec → deployed token name mapping (table above). Phase 2.10 audit
   should decide whether to rename deployed tokens to match specs (cheaper
   for new component authors but breaks all existing CSS) or rewrite specs
   to use deployed names (cheaper for existing code but every spec needs
   updating).

3. Toast's `LipiToastHost` corner-positioning CSS is inlined in the
   component's `.razor` file (`<style>` block) rather than a separate
   `.razor.css` or wwwroot file. Decision rationale: the host is a
   singleton in TopNavLayout, the positions are short and well-defined,
   and a separate file would add a deploy-script entry for ~30 lines of
   layout CSS. Phase 2.10 may consolidate if the inline-style approach
   becomes a pattern elsewhere.

**Files (32 new + 8 modified = 40 total deploy artifacts + 6 spec docs).**

New components (`src/LiPi.Web/Components/Shared/`):
- `LipiSpinnerTypes.cs`, `LipiSpinner.razor`, `LipiSpinner.razor.cs`, `LipiSpinner.razor.css`
- `LipiBadgeTypes.cs`, `LipiBadge.razor`, `LipiBadge.razor.cs`, `LipiBadge.razor.css`
- `LipiPillTypes.cs`, `LipiPill.razor`, `LipiPill.razor.cs`, `LipiPill.razor.css`
- `LipiSkeletonLine.razor`, `LipiSkeletonCircle.razor`, `LipiSkeletonRect.razor`
- `LipiValidationSummaryTypes.cs`, `LipiValidationSummary.razor`, `LipiValidationSummary.razor.cs`, `LipiValidationSummary.razor.css`
- `LipiToastTypes.cs`, `LipiToast.razor`, `LipiToast.razor.cs`, `LipiToast.razor.css`
- `LipiToastHost.razor`, `LipiToastHost.razor.cs`

New services (`src/LiPi.Web/Services/`):
- `ILipiToastService.cs`, `LipiToastService.cs`

New wwwroot (`src/LiPi.Web/wwwroot/`):
- `css/lipi-skeleton.css`
- `css/lipi-validation.css`
- `js/lipi-validation.js`

New StyleGuide page (`src/LiPi.Web/Pages/`):
- `StyleGuideFeedback.razor`, `StyleGuideFeedback.razor.css`

Modified:
- `src/LiPi.Web/wwwroot/themes/mode-light.css` — A35 token additions
- `src/LiPi.Web/wwwroot/themes/mode-dark.css` — A35 token additions
- `src/LiPi.Web/App.razor` — cache 20260525→20260526, new `<link>`/`<script>` entries, version-history row
- `src/LiPi.Web/Components/Layouts/TopNavLayout.razor` — `<LipiToastHost />` mount
- `src/LiPi.Web/Program.cs` — `ILipiToastService` registration
- `src/LiPi.Web/Pages/StyleGuide.razor` — Phase 2.7 sidebar nav group
- `deploy-downloads.ps1` — Phase 2.7 component block + StyleGuideFeedback entries + Phase 2.7 spec doc entries
- `docs/CHANGE-LOG.md` — this amendment (A35)

Spec docs (`docs/00-COMPONENTS/2.7/`):
- `00-Phase2.7-Overview.md`
- `01-LipiSpinner-Spec.md`
- `02-LipiBadge-Pill-Spec.md`
- `03-LipiSkeleton-Spec.md`
- `04-LipiValidationSummary-Spec.md`
- `05-LipiToast-Spec.md`

**Phase 2.7 — CLOSED.** All 6 Feedback components shipped in one batch per the
locked plan. Component library, showcases, specs, and deploy mappings are
consistent. Next: Phase 2.8 — Data Display (LipiTable + supporting), per the
locked roadmap.

---

### A36 — Phase 2.7 audit-log consolidation: dedicated 2.10 audit checklist + two bug-fix learnings captured

**Phase**: 2 Sub-step 2.7 — close-out doc cleanup
**Date**: 2026-05-15
**Status**: ✅ Shipped — doc-only, no source code change

**Trigger.** A35's "Architectural divergences" section documented three Phase
2.7 audit items inline in the changelog (token hex collision, spec→deployed
token naming, toast host inline CSS) plus a paragraph reference to the
`AddServerSideBlazor()` retention. Two additional learnings from Phase 2.7
mid-batch bug fixes — Blazor CSS Isolation scope boundaries and host-component
rendermode requirement — lived only in build-chat conversation responses and
in inline source comments, not in any deployed tracking doc. Once the
conversation context is gone, those learnings disappear.

A36 closes the gap. Three changes, all doc-only:

**1. New consolidated audit checklist** —
`docs/00-COMPONENTS/2.10-Audit-Checklist.md`. Single living doc for every
Phase 2.10 audit item, regardless of source. Sixteen items inventoried at
creation: four from the locked roadmap (cross-page nav smoke test, component
package isolation, `AddServerSideBlazor` migration, PHI/HIPAA audit coverage),
five queued from earlier audits (MailKit, JWT Bearer, blockchain anchoring,
AWS RDS docs, test-automation-guide), and seven from Phase 2.7. One Phase 2.7
item (C4) is a duplicate of the locked roadmap item A3 and collapses into it
during audit work, so effective count is fifteen. Each item carries severity,
source phase, v1.0 state, audit action, files affected, and rationale.

**2. Two bug-fix learnings now in the deployed CHANGE-LOG.** Both surfaced
during Phase 2.7 mid-batch builds and were fixed inside the A35 batch, but
the architectural lessons weren't recorded in the deployed docs.

- **C6 — Blazor CSS Isolation: every new `StyleGuide*.razor` page needs a
  standalone `sg-*` base block in its scoped CSS.** Blazor's CSS Isolation
  appends a per-component scope attribute to selectors and elements; rules
  from another component's `.razor.css` do NOT match elements rendered by
  this one, even with identical class names. The initial Phase 2.7
  `StyleGuideFeedback.razor.css` assumed inheritance from
  `StyleGuide.razor.css` and shipped with the sidebar collapsed into a
  horizontal block and demo numbers running into demo titles. Fixed by
  porting the full base `sg-*` rule block (matching the
  `StyleGuideOverlays.razor.css` pattern). Pattern lesson: convention
  documentation in `00.2-THEMING-ARCHITECTURE.md` would prevent the next
  StyleGuide page from hitting the same bug; alternatively the base block
  can be extracted to `wwwroot/css/lipi-styleguide.css` and imported by
  every StyleGuide page.

- **C7 — Host-component rendermode: every singleton service-driven renderer
  mounted in a layout (not a page) MUST have `@rendermode InteractiveServer`
  at the top.** `LipiToastHost.razor` initially shipped without the
  directive. The host rendered SSR-only on first paint, subscribed to
  `ILipiToastService.OnChanged` in the SSR pass, and that subscription was
  dead by the time the interactive circuit attached. Pages called
  `_toast.Success(...)`, the service fired `OnChanged`, but nothing was
  listening — silent failure with zero console errors and zero server-log
  output. Fixed by adding `@rendermode InteractiveServer`.
  `LipiOverlayHost.razor` had the directive from the start, which is why
  modals always worked. Convention lesson: any component that subscribes to
  a Scoped service event in `OnInitialized` AND is mounted in a layout
  (rather than a page) needs the explicit rendermode directive.

**3. One existing locked-decision promoted to explicit Phase 2.10 item.**
LipiValidationSummary's object-level EditContext errors were captured in A35
as a "locked design decision" (auto-discovery field-level only; consumers
supply object-level errors explicitly). A36 retags this as Phase 2.10 audit
item C5 — the audit decides whether to keep the v1.0 limitation permanent or
implement reflection-based discovery with framework-version guards.

**Files**:
- `docs/00-COMPONENTS/2.10-Audit-Checklist.md` — new, ~430 lines
- `docs/CHANGE-LOG.md` — this amendment (A36)
- `deploy-downloads.ps1` — new entry under Docs section mapping the audit
  checklist file

**Phase 2.7 — RE-CLOSED.** All audit-log debt from the Phase 2.7 build is
now in deployed docs. Build chat work for Phase 2.7 is complete. Strategic
chat owns Phase 2.8 (Data Display) design discussion; build chat picks up on
handoff per the locked 2.8 → 2.9 → 2.10 → page redesign order.

---

### A37 — Phase 2.8 Stage 1A: LipiTable type foundation + new LiPi.Components csproj

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 1A (Foundation — types only)
**Date**: 2026-05-16
**Status**: ✅ Shipped — type-only foundation; no behavior; new csproj registered

**Trigger.** Phase 2.8 strategic-chat handoff dropped 7 spec docs and a 9-stage
build plan into build chat. Stage 1 of that plan is "Foundation: types,
services, DB migration, shared CSS, JS interop" with the verification gate
"project compiles; migration runs; services register in DI". Stage 1 is large
enough (≈21 deliverables) to warrant sub-staging in the established 2.6.2
precedent (1A / 1B / 1C). Stage 1A delivers types only — pure declarations,
no behavior, no runtime dependencies on Phase 2.7 or earlier components.

A37 ships 10 files: 1 new csproj plus 9 type-only `.cs` files covering the
public API surface of all four Phase 2.8 components (LipiTable, LipiList,
LipiPagination, LipiEmptyState) plus the shared status taxonomy. Compiling
this stage is self-contained — the new `LiPi.Components.csproj` builds
standalone via `dotnet build src\LiPi.Components\LiPi.Components.csproj`
without any project-reference wire-up to `LiPi.Web` (that wire-up is Stage 1C).

**1. New csproj: `LiPi.Components`.** First Phase 2.8 file. Per spec §25.2.2
(component isolation contract — locked), all new Phase 2.8 component code
lives in a separate `LiPi.Components.*` namespace, isolated from `LiPi.Web.*`
HIS application code. The new project targets .NET 10, uses
`Microsoft.NET.Sdk.Razor` (so Razor components added Stage 2+ compile
without csproj changes), and has zero NuGet package references — only the
ASP.NET Core shared framework via `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
This satisfies Phase 2.8's zero-external-functional-library posture (§1.3).

Existing Phase 2.1–2.7 components stay where they are in
`src\LiPi.Web\Components\Shared\`. The Phase 2.10 audit migrates them into
`LiPi.Components` later. Phase 2.8 components are the first to ship in the
new redistributable layout.

**2. Type surface organization (9 .cs files).** Files split per spec §27.3.3
file taxonomy:

- `LipiTableTypes.cs` — 19 table-level enums covering selection, edit,
  pagination, density, filter, loading, virtualization, grouping, tree,
  master-detail, conflict resolution, new-row placement, row-status placement.
- `LipiColumnTypes.cs` — 8 column-level enums: `ColumnType` (the 14 built-in
  generic types), `ColumnPin`, `ColumnAlign`, `CopyTarget`, `StatusVariant`,
  `LipiAggregate`, `SortCycleMode`, `NullSortOrder`.
- `SortDescriptor.cs` — `SortDescriptor` / `FilterDescriptor` /
  `GroupDescriptor` records + `SortDirection` + `FilterOperator` (28 ops
  covering text, numeric, date-relative, boolean, set, and universal).
- `TableQueryRequest.cs` — `TableQueryRequest` record (server-side request
  envelope; `Extra` defaults to `ImmutableDictionary<string, object?>.Empty`).
- `TableQueryResponse.cs` — `TableQueryResponse<TItem>` + `GroupBucket<TItem>`
  + `ConflictInfo` (per §4.3 and §12.10.2).
- `SaveResult.cs` — `SaveResult` + `SaveOutcome` + `ValidationResult` +
  `ValidationError` + `ValidationSeverity` (per §12.6.2 and §12.9.4).
- `Contexts.cs` — 21 non-export event context records + 16 reason enums
  covering selection, sort, filter, quick-search, pagination, grouping,
  tree, master-detail, inline edit (row + cell + dirty state), density,
  column ops (resize / reorder / pin / visibility), error, query complete.
- `Export\ExportTypes.cs` — `ExportFormat` (Csv / Pdf / Print) + `ExportScope`
  (View / Filtered / All / Selected) + `ExportOptions` + `ExportProgress` +
  `BeforeExportContext` + `AfterExportContext`. PDF stays declared in the
  enum surface but the actual exporter is stubbed until Phase 2.10 lands
  the in-house LiPi PDF library.
- `Shared\LipiStatus.cs` — 17 status string constants (Active, Pending,
  Inactive, Suspended, Locked, Archived, Draft, Published, InProgress,
  Completed, Failed, Cancelled, Warning, Error, Info, Success, Unknown).
  Status values are strings (not C# enum) at consumption points so consuming
  apps can extend the taxonomy without modifying the LiPi component library.

**3. Three deviations from Phase 2.8 spec, all recorded here and surfaced
to strategic chat.**

- *Deviation 1 — `Export\ExportTypes.cs` added to Stage 1A.* Spec §28.4
  deploy registry doesn't list this file in Stage 1. Build chat added it
  because `BeforeExportContext` and `AfterExportContext` (event-context
  records consumed via `OnBeforeExport` / `OnAfterExport`) carry
  `ExportFormat`. Placing the export contexts in `Contexts.cs` without
  `ExportFormat` in scope would create a Stage-6 forward-declaration
  coupling. Co-locating the full export type family in
  `Export\ExportTypes.cs` at Stage 1A matches §27.7 file taxonomy and
  keeps `Contexts.cs` free of export-specific references.

- *Deviation 2 — `LiPi.Components.Shared` realized as a subfolder inside
  `LiPi.Components.csproj`, not a separate csproj.* Spec §27.2 diagram
  shows `src/LiPi.Components/` and `src/LiPi.Components.Shared/` as
  parallel top-level project folders, implying two csproj. But §28.4
  deploy registry paths (`src\LiPi.Components\Shared\LipiStatus.cs`)
  place `Shared\` as a subfolder inside `LiPi.Components`. Build chat
  followed §28.4 (deploy paths are the concrete contract). Result: one
  csproj (`LiPi.Components.csproj`) with two top-level subfolders:
  `DataDisplay\` and `Shared\`. The two-csproj split can revisit in
  Phase 2.10 audit if a real consumer needs `LiPi.Components.Shared`
  shipped independent of `LiPi.Components`.

- *Deviation 3 — `PersistedContext` + `PersistedTrigger` deferred to
  Stage 1B.* §23.10.7 places these in `Contexts.cs`, but the record
  references `TablePreferences` — a Stage 1B service type that doesn't
  exist yet. Build chat relocated them to ship in Stage 1B alongside
  `TablePreferences.cs` in the services folder. No spec change required;
  just a file-location refinement within the same component family.

**4. One divergence from spec §28.3 file-naming convention.** Spec §28.3
mandates that files dropped in `Downloads\LiPi\` use `2.8-types-LipiTableTypes.cs`
prefixed naming. Reality on the ground: the existing `deploy-downloads.ps1`
uses bare filenames as hashtable keys (`"LipiTableTypes.cs" = "src\..."`).
The naming convention in spec §28.3 was written without seeing the actual
deploy script convention; project convention wins. All 10 Stage 1A files use
bare filenames. Phase 2.10 audit can revisit the convention if a future
phase produces filename collisions across phases.

**5. Locked decisions honored at Stage 1A.**

- Component isolation contract (§25.2.1, §25.2.2, §25.2.3): all new code
  under `LiPi.Components.*` namespace; zero `using LiPi.Web.*` or
  `using LiPi.HIS.*`; CSS prefix `lipi-*` (no CSS in this stage but
  reflected in any string literals); zero clinical / HIS-specific type
  names in the public surface (`ColumnType` includes generic types only —
  Text, Number, Currency, Status, etc.; clinical types are caller's
  `Type=Custom` + `<CellTemplate>` responsibility).
- Zero new NuGet packages (Phase 2.8 §1.3 / §8).
- SPEC comment header on every file (§27.19 template).
- LP-0 (LipiPagination variant naming) deferred to Stage 4 — no
  LipiPagination code in this stage.
- PDF deferred to Phase 2.10 — `ExportFormat.Pdf` exists in the enum surface,
  the actual exporter stubs in Stage 6 throwing "PDF library pending Phase
  2.10 integration" until the in-house LiPi PDF library ships.
- Edit + concurrency types align with the row-version pattern (Q3.5 locked):
  `RowVersionSelector` not part of Stage 1A (parameter on LipiTable, lands
  in Stage 2), but `ConflictInfo.ServerRowVersion` and the conflict UX
  enums are in place.

**6. What's NOT in Stage 1A (handled in later sub-stages).**

- Stage 1B: `ITablePreferenceService` + `TablePreferenceService` +
  `TablePreferences` + `ColumnPreference` + `PersistedContext` +
  `PersistedTrigger`; `UserTablePreference` EF entity + configuration;
  DB migration up/down SQL.
- Stage 1C: `lipi-table-tokens.css` + `lipi-table-print.css` +
  `lipi-status-tokens.css`; `lipi-table-interop.js`; `App.razor`
  cache-version bump + new `<link>` and `<script>` tags; `Program.cs`
  DI registration of `ITablePreferenceService`; `LiPi.sln` solution-folder
  entry + `LiPi.Web.csproj` `<ProjectReference>` to `LiPi.Components`.
- Stage 2 onward: Razor components, sub-components, services beyond
  preferences, full feature implementation.
- Stage 8: Phase 2.8 spec docs (`00-Phase2.8-Overview.md`,
  `01-LipiTable-Spec.md`, `02-LipiList-Spec.md`, `03-LipiPagination-Spec.md`,
  `04-LipiEmptyState-Spec.md`) deployed alongside StyleGuide demo pages,
  matching the A35 / A36 pattern.

**7. Verification gate (run after deploy).**

```
cd C:\Users\aruns\Documents\lipi-complete\lipi-complete
dotnet build src\LiPi.Components\LiPi.Components.csproj
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

Additional verification:

- `grep -r "LiPi.Web" src\LiPi.Components\` returns no matches (component
  isolation invariant).
- `grep -rE "Patient|Doctor|Clinic|Encounter|Aadhaar|Uhid|Mrn" src\LiPi.Components\`
  returns matches only inside SPEC comment blocks and docstrings —
  never inside type names, parameter names, or enum values.
- Every `.cs` file has a `// SPEC:` header pointing to a §-anchor (§27.19
  contract).
- The full-solution `dotnet build` from project root will NOT include
  `LiPi.Components` yet — the project is intentionally not in
  `LiPi.sln` and not referenced by `LiPi.Web.csproj` at this stage.
  That wire-up is Stage 1C.

**Files**:

- `src\LiPi.Components\LiPi.Components.csproj` — new csproj, redistributable
  component library
- `src\LiPi.Components\DataDisplay\LipiTable\LipiTableTypes.cs` — 19 table-level enums
- `src\LiPi.Components\DataDisplay\LipiTable\LipiColumnTypes.cs` — 8 column-level enums
- `src\LiPi.Components\DataDisplay\LipiTable\SortDescriptor.cs` — sort / filter / group descriptors
- `src\LiPi.Components\DataDisplay\LipiTable\TableQueryRequest.cs` — server request envelope
- `src\LiPi.Components\DataDisplay\LipiTable\TableQueryResponse.cs` — server response + ConflictInfo
- `src\LiPi.Components\DataDisplay\LipiTable\SaveResult.cs` — save outcomes + validation result
- `src\LiPi.Components\DataDisplay\LipiTable\Contexts.cs` — 21 event context records + reason enums
- `src\LiPi.Components\DataDisplay\LipiTable\Export\ExportTypes.cs` — export-related types
- `src\LiPi.Components\Shared\LipiStatus.cs` — shared status taxonomy (17 constants)
- `deploy-downloads.ps1` — appended Phase 2.8 Stage 1A block with 10 hashtable entries
- `docs\CHANGE-LOG.md` — this amendment (A37)

---

### A38 — Phase 2.8 Stage 1B: persistence service abstractions + EF entity + clinic-DB migration

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 1B (Foundation — services + DB)
**Date**: 2026-05-16
**Status**: ✅ Shipped — abstractions + default impl + entity + migration SQL; wire-up in Stage 1C

**Trigger.** Stage 1A landed the type foundation. Stage 1B adds the persistence
service surface (interfaces + default implementation), the EF entity for the
storage table, and the SQL migration that creates the table. Two architectural
decisions were resolved with Arun before code was written; both are captured
here so the rationale survives the build-chat context loss.

**1. Decision: per-clinic persistence, not master.**

`user_table_preferences` lives in each clinic database, not in master alongside
`identity.users`. Five reasons, in declining weight:

- *Locality of reference.* Preferences describe a clinic-scoped table; storing
  them with the data they describe is straightforward architecture. Backup of
  a clinic captures its user-behavior metadata; restore brings it back.
- *TableId namespace collision eliminated.* If preferences were master-scoped,
  `TableId="patients-list"` from Clinic A would collide with the same id from
  Clinic B. Per-clinic storage naturally namespaces by DB.
- *Tenant isolation (HIPAA + DPDP).* User-behavior metadata derived from
  clinic PHI belongs in that clinic's DB, not in a central directory.
- *Industry pattern matches.* Salesforce per-org, Notion per-workspace, Linear
  per-workspace, Microsoft Dynamics per-org. The reverse pattern (cross-tenant
  global prefs) is rare and usually causes problems.
- *Backup/restore granularity.* Per-clinic restore brings prefs back; central
  restore would need separate coordination.

**Cross-DB FK consequence.** PostgreSQL cannot enforce a foreign-key constraint
across separate databases. `user_table_preferences.user_id` references
`master.identity.users(id)` but the constraint is app-layer only (validated via
auth context). This is safe because LiPi never hard-deletes users —
clinic-user-deletion is access revocation, not row removal (confirmed with
Arun: *"Clinic user deletion means they lose access to that clinic. No user is
hard deleted."*). So orphan preference rows never accumulate; no cascade is
needed; no soft-delete tombstone state needs to be tracked.

**2. Decision: Option C — store abstraction architecture.**

Spec §28.4 placed the default `TablePreferenceService` implementation inside
`LiPi.Components`, but the spec's reference implementation depends on
`IdentityDbContext` (a consumer-app type), which directly violates §25.3.2's
isolation contract. The contradiction was discussed with Arun across three
options:

- *Option A* — Pure consumer-provides. `LiPi.Components` defines the interface
  only; `LiPi.Web` provides the full EF-backed implementation. Simplest but
  every future consumer reinvents JSON serialization, caching, and debouncing.
- *Option B* — Spec literal. Implementation in `LiPi.Components`. Breaks
  isolation; Phase 2.10 audit would block deployment. Rejected.
- *Option C* — Store abstraction. `LiPi.Components` defines BOTH the
  high-level `ITablePreferenceService` AND lower-level
  `IUserTablePreferenceStore` + `ICurrentUserAccessor`. `LiPi.Components`
  also ships the default `TablePreferenceService` (composes the two
  abstractions, owns JSON / cache / debounce / error handling). Consumers
  implement only the two low-level interfaces against their storage and auth.

Option C selected. Pattern mirrors ASP.NET Identity's `UserManager<TUser>` +
`IUserStore<TUser>` split: library owns the high-level behavior, consumer owns
storage and auth specifics. This is the textbook .NET-library shape for
configurable, consumer-overridable services.

The five new `LiPi.Components` files (interface + records + default impl +
two abstractions) compile standalone — no dependency on consumer code.
`LiPi.Web` in Stage 1C will deliver the two concrete implementations
(`EfUserTablePreferenceStore` + `BlazorCurrentUserAccessor`).

**3. Files added.**

**LiPi.Components — five new files in `src\LiPi.Components\DataDisplay\LipiTable\Services\`:**

- `ITablePreferenceService.cs` — high-level interface (5 methods: Get, Save,
  Reset, ListTableIds, RenameTableId). Async throughout. Returns null /
  empty collections on auth-missing or error (silent error handling per §21.5).
- `IUserTablePreferenceStore.cs` — low-level CRUD interface (5 methods over
  `(Guid userId, string tableId, string prefsJson)`). Storage-agnostic.
- `ICurrentUserAccessor.cs` — single-method auth abstraction.
  `ValueTask<Guid?> GetUserIdAsync(CancellationToken)`. ValueTask because
  most implementations resolve synchronously from cached claims.
- `TablePreferences.cs` — top-level `TablePreferences` class with versioned
  schema, `ColumnPreference` record, plus `PersistedContext` record and
  `PersistedTrigger` enum (deferred from Stage 1A per A37 because both
  reference `TablePreferences` which lives here).
- `TablePreferenceService.cs` — default implementation. Implements
  `ITablePreferenceService` + `IAsyncDisposable`. Owns:
    * JSON serialization (System.Text.Json, camelCase, ignore-null-on-write)
    * Per-circuit `ConcurrentDictionary` cache (avoids re-reads inside the
      same circuit; cache holds `null` for "no row exists")
    * 300ms debounced writes per `tableId` (rapid SaveAsync calls coalesce
      into a single store write; superseded writes cancel cleanly)
    * Silent error handling — every store exception is logged via
      `ILogger<TablePreferenceService>` at Warning level and swallowed;
      `OperationCanceledException` re-throws as expected
    * `DisposeAsync` snapshots pending writes, cancels their debounce
      timers, and flushes synchronously with a 2-second timeout cap so
      slow stores don't block circuit teardown
    * Malformed JSON tolerated — `JsonException` on read returns null and
      logs; the user effectively gets a fresh-defaults experience rather
      than a hard error
    * Argument null checks in constructor (defensive; DI catches mis-wiring early)

**LiPi.Clinic.Core — one new file in `database\efcore\LiPi.Clinic.Core\Entities\`:**

- `UserTablePreference.cs` — EF entity. `[Table("user_table_preferences", Schema = "identity")]`.
  Composite PK via `[PrimaryKey(nameof(UserId), nameof(TableId))]` (EF Core 7+
  attribute). `UserId` is UUID, `TableId` varchar(200), `PrefsJson` jsonb,
  `UpdatedAt` timestamptz. Data-annotation-based config — no separate
  `Configurations\` file, matching existing project precedent (`UserPreference`,
  `UserRole`, `AdSyncLog` all use data annotations).

**database\migrations — two new SQL files:**

- `2026-05-16-phase-2.8-user-table-prefs-up.sql` — creates
  `identity.user_table_preferences` with composite PK, secondary index on
  `user_id`, and table/column comments documenting the cross-DB FK app-layer
  enforcement. Wrapped in BEGIN/COMMIT. Idempotent via
  `CREATE SCHEMA IF NOT EXISTS` + `CREATE TABLE IF NOT EXISTS` +
  `CREATE INDEX IF NOT EXISTS`.
- `2026-05-16-phase-2.8-user-table-prefs-down.sql` — reverses the up
  migration. `DROP INDEX IF EXISTS` + `DROP TABLE IF EXISTS`. Wrapped in
  BEGIN/COMMIT.

**Migration deployment**: apply PART B (each clinic DB) only. Master DB does
not get this migration. Existing project deploy convention supports the
PART A / PART B split (see prior A1 / Decision-12 theming migration which
notes the same pattern). Apply via pgAdmin or psql manually — the deploy
script copies the SQL to `database\migrations\` but does not execute it.

**4. Three convention divergences from spec §28.4, all recorded.**

- *Divergence 1 — Entity project.* Spec said `LiPi.Identity.Core` (project
  doesn't exist in this codebase). Reality is `LiPi.Clinic.Core` because the
  entity lives in clinic DB per A38 §1. Adjusting the deploy path was
  unavoidable.
- *Divergence 2 — Migration SQL path.* Spec said
  `database\migrations\identity\2026-05-15_AddUserTablePreferences.sql` with
  an `identity\` subfolder. Existing project convention (per the A1
  decision-12-theming migration) uses flat `database\migrations\` with
  date-prefixed-up/down filename pairs. Followed project convention.
- *Divergence 3 — EF configuration.* Spec said separate
  `Configurations\UserTablePreferenceConfiguration.cs` fluent-API file.
  Existing project precedent (`UserPreference`, `UserRole`, `AdSyncLog`)
  uses data annotations directly on the entity. Followed project convention
  to stay consistent with the rest of `LiPi.Clinic.Core`. If a future
  refactor standardizes on fluent-API config, this entity migrates with the
  rest of the project in a single sweep (queued as a Phase 2.10 audit item
  candidate, not yet committed to the checklist).

**5. What's NOT in Stage 1B (handled in Stage 1C).**

- `EfUserTablePreferenceStore.cs` — concrete `IUserTablePreferenceStore` impl
  against the clinic-side `DbContext`. Will inject `ClinicDbContext` (or
  whatever the clinic-side DbContext is named — Arun to share in Stage 1C)
  and translate the five store methods into EF queries.
- `BlazorCurrentUserAccessor.cs` — concrete `ICurrentUserAccessor` impl.
  Injects `AuthenticationStateProvider`; reads the `sub` claim or
  `ClaimTypes.NameIdentifier`; returns `Guid?`.
- `DbSet<UserTablePreference>` registration on the clinic-side DbContext.
  Requires the current DbContext file from Arun to deliver the full updated
  version (per the no-snippets rule).
- `Program.cs` DI registration — three `AddScoped` calls:
  `ITablePreferenceService → TablePreferenceService`,
  `IUserTablePreferenceStore → EfUserTablePreferenceStore`,
  `ICurrentUserAccessor → BlazorCurrentUserAccessor`.
- `LiPi.sln` + `LiPi.Web.csproj` wire-up — add `LiPi.Components` to the
  solution and add `<ProjectReference>` from `LiPi.Web` to `LiPi.Components`.
  Until this lands, `LiPi.Web` cannot resolve any `LiPi.Components` types.
- Migration application — the SQL deploys to `database\migrations\` but is
  not auto-applied; manual `psql` or pgAdmin run is the deployment step.

**6. Verification gate.**

```
cd C:\Users\aruns\Documents\lipi-complete\lipi-complete
dotnet build src\LiPi.Components\LiPi.Components.csproj
dotnet build database\efcore\LiPi.Clinic.Core\LiPi.Clinic.Core.csproj
```

Both should report `Build succeeded. 0 Warning(s). 0 Error(s).` The migration
SQL files exist in `database\migrations\` and are syntactically valid (test
with `psql --dry-run` if available, or visual inspection — they're short).

Solution-wide `dotnet build` from the project root still does not include
`LiPi.Components` — that wire-up is Stage 1C.

The migration files are NOT applied to any database in Stage 1B; that's a
Stage 1C deployment step. The `.sql` files just sit at
`database\migrations\` ready to apply.

**Files**:

- `src\LiPi.Components\DataDisplay\LipiTable\Services\ITablePreferenceService.cs` — high-level interface
- `src\LiPi.Components\DataDisplay\LipiTable\Services\IUserTablePreferenceStore.cs` — low-level store interface
- `src\LiPi.Components\DataDisplay\LipiTable\Services\ICurrentUserAccessor.cs` — auth abstraction
- `src\LiPi.Components\DataDisplay\LipiTable\Services\TablePreferences.cs` — records + PersistedContext + PersistedTrigger
- `src\LiPi.Components\DataDisplay\LipiTable\Services\TablePreferenceService.cs` — default impl (cache + debounce + JSON)
- `database\efcore\LiPi.Clinic.Core\Entities\UserTablePreference.cs` — EF entity
- `database\migrations\2026-05-16-phase-2.8-user-table-prefs-up.sql` — create table + index
- `database\migrations\2026-05-16-phase-2.8-user-table-prefs-down.sql` — drop table + index
- `deploy-downloads.ps1` — appended Phase 2.8 Stage 1B block with 8 hashtable entries
- `docs\CHANGE-LOG.md` — this amendment (A38)

---

### A39 — Phase 2.8 Stage 1C: persistence wire-up + entity moved to LiPi.Clinic.Identity

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 1C (Foundation — service wire-up)
**Date**: 2026-05-16
**Status**: ✅ Shipped — end-to-end persistence compiles; migration apply is a manual step

**Trigger.** Stage 1B shipped the persistence abstractions, default service, EF entity,
and migration SQL. Stage 1C connects them: the consuming app (LiPi.Web) supplies the
two low-level implementations, registers all three services, references the component
library, and the entity is relocated to its correct home. After this stage the whole
app compiles and table preferences round-trip through the per-clinic database.

**1. Correction: entity moved LiPi.Clinic.Core → LiPi.Clinic.Identity.**

Stage 1B (A38, divergence #1) placed `UserTablePreference` in `LiPi.Clinic.Core`
because the spec referenced a nonexistent `LiPi.Identity.Core` project and Core seemed
the closest real match. Reviewing the actual codebase for Stage 1C revealed the true
sibling: `UserPreference` (theme prefs, Decision #12) lives in `LiPi.Clinic.Identity`
→ `IdentityDbContext.UserPreferences` → `identity.user_preferences`, inside each clinic
DB. `UserTablePreference` is the same kind of thing (per-user, per-clinic-DB preference
row in the identity schema), so it belongs beside `UserPreference`.

Decided with Arun: move to `LiPi.Clinic.Identity`. Benefits:

- `LiPi.Web` already references `LiPi.Clinic.Identity` — no new project reference for
  the entity.
- `ClinicDbFactory.CreateForClinicAsync(clinicId)` already returns an
  `IdentityDbContext` pointed at the clinic DB — exactly what the store needs. No new
  factory method, no hand-built context.
- Schema is `identity` (set by `IdentityDbContext.HasDefaultSchema("identity")`),
  which matches the migration SQL shipped in Stage 1B unchanged.
- Consistent with the established `user_preferences` pattern.

The migration SQL (`identity.user_table_preferences`, applied per clinic DB) was
already correct and is unchanged. Only the entity's C# project/namespace and the
DbContext registration target changed.

**2. Entity re-issued as a plain POCO.**

Stage 1B's entity used data annotations (`[Table]`, `[PrimaryKey]`, `[Column]`).
`IdentityDbContext` configures every entity fluently and applies schema + snake_case
naming via its `OnModelCreating` loop, and `UserPreference` is a plain POCO. To be a
clean sibling, `UserTablePreference` is re-issued annotation-free in namespace
`LiPi.Clinic.Identity.Entities`. The snake_case loop produces `user_id` / `table_id` /
`prefs_json` / `updated_at`, matching the migration. Composite PK, jsonb type, and the
200-char `table_id` cap are configured fluently next to the `UserPreference` block.

`updated_at` is deliberately NOT configured as store-generated: the store sets a UTC
value on every write, so EF must send it. The SQL `DEFAULT NOW()` remains a safety net
for any non-EF insert path.

**3. New consumer implementations in LiPi.Web\Services.**

- `BlazorCurrentUserAccessor` — implements `ICurrentUserAccessor`. Resolves the user
  GUID from the `NameIdentifier` claim via `AuthenticationStateProvider` (same claim
  `ClaimsHelper.UserId` reads). Memoized per circuit.
- `EfUserTablePreferenceStore` — implements `IUserTablePreferenceStore`. Resolves the
  current clinic GUID from the `clinicId` claim (same claim `ClaimsHelper.ClinicId`
  reads), calls `ClinicDbFactory.CreateForClinicAsync(clinicId)` to get the per-clinic
  `IdentityDbContext`, and performs raw CRUD against `UserTablePreferences`. The store
  does not catch exceptions — `TablePreferenceService` already wraps every store call
  in try/catch + Warning log (store = raw CRUD, service = resilience). `RenameAsync`
  removes + re-inserts because `TableId` is part of the PK and can't be mutated in
  place; it overwrites any existing new-id row per the interface's documented semantics.

**4. Background-write safety via memoization.**

`TablePreferenceService` (Stage 1B) performs debounced writes on a background `Task`.
Reading `AuthenticationStateProvider` from a background thread in Blazor Server is
fragile. Rather than edit the working Stage 1B service, both consumer implementations
memoize their resolved id on first call — which always happens on the circuit thread
during the table's mount-time `GetAsync`. The later background write reads the cached
GUID and never touches `AuthenticationStateProvider` off-circuit. A circuit serves one
user + one clinic for its lifetime, so caching is correct (a clinic switch means
re-login → new principal → new circuit).

**5. Service registration (Program.cs) — three Scoped services.**

```
builder.Services.AddScoped<ICurrentUserAccessor, BlazorCurrentUserAccessor>();
builder.Services.AddScoped<IUserTablePreferenceStore, EfUserTablePreferenceStore>();
builder.Services.AddScoped<ITablePreferenceService, TablePreferenceService>();
```

Added after the Phase 2.7 toast registration, with `using LiPi.Components.DataDisplay;`.
All Scoped (one per circuit), which keeps the id-memoization and the service's
per-circuit cache safe.

**6. Project reference (LiPi.Web.csproj).**

Added `<ProjectReference Include="..\LiPi.Components\LiPi.Components.csproj" />` so
LiPi.Web can resolve the interfaces and the default `TablePreferenceService`. No .sln
exists in this codebase (Arun builds/runs via `dotnet run` on LiPi.Web), so this
ProjectReference is the entire wire-up — there is no solution file to update.

**7. Files modified vs new (deploy routing).**

- `Program.cs` — MODIFIED. Existing deploy entry routes it (no new key).
- `IdentityDbContext.cs` — MODIFIED (DbSet + fluent config). Existing deploy entry
  routes it (no new key).
- `UserTablePreference.cs` — MODIFIED + RELOCATED. Stage 1B deploy entry REPOINTED
  from `LiPi.Clinic.Core\Entities\` to `LiPi.Clinic.Identity\Entities\`.
- `LiPi.Web.csproj` — MODIFIED. NEW deploy entry.
- `EfUserTablePreferenceStore.cs` — NEW. NEW deploy entry.
- `BlazorCurrentUserAccessor.cs` — NEW. NEW deploy entry.

**8. Two manual steps after deploy.**

- Delete the orphaned Stage 1B entity that was deployed to the old Core location:
  `Remove-Item "database\efcore\LiPi.Clinic.Core\Entities\UserTablePreference.cs"`.
  Leaving it is harmless dead code in a different namespace, but delete it to avoid
  confusion (the LiPi.Clinic.Identity copy is the live one).
- Apply the migration to each clinic DB (it was shipped in Stage 1B but never
  auto-applied):
  `psql -d <clinic_db> -f database\migrations\2026-05-16-phase-2.8-user-table-prefs-up.sql`.
  The table must exist before the first `SaveChanges`. For dev, apply to the Armoki
  clinic DB.

**9. One dependency to verify at build.**

`EfUserTablePreferenceStore` calls `ClinicDbFactory.CreateForClinicAsync(Guid)` →
`IdentityDbContext?`, mirroring `UserPreferenceService`'s usage. If the current
`ClinicDbFactory` exposes that method under a different name or signature, the store
won't compile — surface it and the store adjusts. This is the only assumed external
API in the stage.

**10. Verification gate.**

```
cd C:\Users\aruns\Documents\lipi-complete\lipi-complete
dotnet build src\LiPi.Web\LiPi.Web.csproj
```

This now transitively builds `LiPi.Components` (new ProjectReference). Expected:
`Build succeeded`. Because there is no .sln, `dotnet build` on the web csproj is the
whole-app build. After applying the migration to a clinic DB, run the app
(`dotnet run --project src\LiPi.Web`), sign into a clinic, and the persistence path is
live (full round-trip is observable once Stage 2 ships a real LipiTable that calls
`ITablePreferenceService`).

**11. What's NOT in Stage 1C (moved to Stage 1D, just before Stage 2).**

Shared CSS (`lipi-table-tokens.css`, `lipi-table-print.css`, `lipi-status-tokens.css`),
the JS interop (`lipi-table-interop.js`), and the `App.razor` cache-version bump +
`<link>` / `<script>` tags. Nothing renders until Stage 2 components exist, so these
are deferred to a focused Stage 1D rather than shipping unused assets now.

**Files**:

- `database\efcore\LiPi.Clinic.Identity\Entities\UserTablePreference.cs` — relocated POCO
- `database\efcore\LiPi.Clinic.Identity\IdentityDbContext.cs` — +DbSet +fluent config
- `src\LiPi.Web\Services\EfUserTablePreferenceStore.cs` — store impl (per-clinic IdentityDbContext)
- `src\LiPi.Web\Services\BlazorCurrentUserAccessor.cs` — auth accessor impl
- `src\LiPi.Web\Program.cs` — +using +3 Scoped registrations
- `src\LiPi.Web\LiPi.Web.csproj` — +ProjectReference to LiPi.Components
- `deploy-downloads.ps1` — repointed UserTablePreference.cs entry + Stage 1C block (3 new entries)
- `docs\CHANGE-LOG.md` — this amendment (A39)

---

### A40 — Phase 2.8 Stage 1D: Lipicons integration (vendored LiPicons.Blazor + <LipiIcon>)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 1D (icon foundation)
**Date**: 2026-05-30
**Status**: ✅ Shipped — Lipicons rendering live in the HIS; data-display components can compose `<LipiIcon>`

**Roadmap amendment.** The locked roadmap put Lipicons at Phase 3.0 ("Lucide icons
stay for v1.0"). That lock is amended: **Lipicons enters at Phase 2.8.** Trigger — the
first data-display component (LipiEmptyState) needs to render icons inside the
redistributable `LiPi.Components` library, and `LucideIcon` lives in `LiPi.Web`
(referencing it would break the isolation contract). The icon question, parked for 3.0,
came due at 2.8. Arun had `LiPicons.Blazor` built in the separate icons chat, so the
clean resolution is to consume the real icon package now rather than copy LucideIcon
forward as throwaway.

**Why a standalone package (not source inside LiPi.Components).** Three imagiQa projects
consume Lipicons, not just the HIS. A dependency shared by 3+ projects must be a package,
not source copied into one consumer — otherwise the other two projects would have to
depend on the entire HIS component library just to render an icon. So `LiPicons.Blazor`
is its own NuGet package (built + owned by the icons chat), and `LiPi.Components` consumes
it like any generic dependency. This is isolation-clean: a HIS-agnostic icon package is an
allowed dependency under §25.3.1, same category as `Microsoft.AspNetCore.Components`.

**Vendored, not yet a feed package.** `LiPicons.Blazor` is built and on `main` in the
lipicons repo but is not yet published to a NuGet feed. To keep the HIS building
standalone (no sibling-checkout dependency), the package is **vendored** into the HIS repo
at `libs\LiPicons.Blazor\` (7 files: 6 source + `icons.json`). `libs\` signals
third-party/vendored to the Phase 2.10 isolation audit. When the package publishes, the
two ProjectReferences (from `LiPi.Components` and `LiPi.Web`) swap to a single
`PackageReference Include="LiPicons.Blazor" Version="x.y.z"` — one-line change, zero
component churn.

The vendored copy differs from upstream in exactly one line: the `EmbeddedResource` path
for `icons.json` is repointed from `../../dist/json/icons.json` (upstream repo layout) to
the local `icons.json` copied alongside the project. Everything else is byte-identical to
upstream v1.0.2.

**Package verification (done before integrating).** Re-cloned the repo and verified
against the §1 contract rather than trusting the handback checklist: enum
`LipiconVariant { Regular, Bold, Light, Thin, Fill, Duotone }` exact; `LipiIcon`
parameters `Name`/`Variant`/`Size`/`Color`/`Class`/`Title` with correct types and
defaults; namespace `LiPicons.Blazor`; unknown name renders nothing without throwing;
`currentColor` default; square sizing with preserved 24×24 viewBox; manifest embedded as
an assembly resource (offline, no wwwroot, no CDN); sole dependency
`Microsoft.AspNetCore.Components.Web`; no HIS coupling. All confirmed.

**The `<LipiIcon>` API contract (locked, both chats build to this verbatim).**
`<LipiIcon Name Variant Size Color Class Title />`. `Name` string required (manifest key);
`Variant` defaults `Regular`; `Size` int px default 24; `Color` null → `currentColor`;
`Class` appended; `Title` null → decorative `aria-hidden`, set → `role=img` + `aria-label`.
Data-display default variant = `Regular`; per-component overrides decided as we go.

**§5 icon naming decisions (from the icons chat, recorded for HIS mapping).** Lipicons
uses native names, not Lucide names. The three HIS needs without an exact match were
resolved as temporary mappings for v1.0:
- FilteredEmpty → `search` (no dedicated `no-results`/`search-off` glyph yet)
- Error → `warning` (no dedicated `error`/octagon glyph yet; warning and error share a
  glyph in v1.0, differentiated by color tone — error gets danger tint, warning gets
  warning tint)
- Sortable-unsorted → `sort` (genuine up/down double-arrow; `arrow-up`/`arrow-down` for
  active asc/desc)

**Accepted as a known v1.0 compromise.** When the dedicated `no-results` and `error`
glyphs are added to Lipicons, they're additive — HIS code keeps working, and the two
mappings switch whenever desired (one-line change in `LipiEmptyState.razor.cs` and any
future consumer).

**Token reconciliation (carried into A41).** The first CSS bundle reviewed
(`00-baseline.css`, `app.css`) suggested no semantic color tokens existed, which would
have blocked the phase. Investigation found the real Layer-3 token architecture in
`themes/mode-light.css` + `themes/mode-dark.css` (Decision #12 §12.4): the full
`--color-*` contract (text-primary/secondary/tertiary, danger/success/warning/info +
pale + text-strong ladders) resolving per `[data-mode]`. The LipiEmptyState spec
referenced three token names that don't exist in this codebase; bound to the real tokens:
`--color-text-faint` → `--color-text-tertiary`, `--color-danger-alpha-04` →
`--color-danger-pale`, `--color-success-alpha-04` → `--color-success-pale`. Detail in A41.

**Files** (Stage 1D):

- `libs\LiPicons.Blazor\LiPicons.Blazor.csproj` — vendored, EmbeddedResource repointed to local icons.json
- `libs\LiPicons.Blazor\LipiIcon.razor` — vendored (byte-identical to upstream)
- `libs\LiPicons.Blazor\LipiconVariant.cs` — vendored
- `libs\LiPicons.Blazor\LipiconRenderer.cs` — vendored
- `libs\LiPicons.Blazor\IconManifest.cs` — vendored
- `libs\LiPicons.Blazor\LipiconName.cs` — vendored (1,149 typed name constants)
- `libs\LiPicons.Blazor\icons.json` — vendored full manifest (1,149 icons × 6 variants, 436KB)
- `src\LiPi.Components\LiPi.Components.csproj` — +ProjectReference to vendored LiPicons.Blazor
- `src\LiPi.Web\LiPi.Web.csproj` — +ProjectReference to vendored LiPicons.Blazor (explicit, for demo `_Imports`)
- `deploy-downloads.ps1` — Stage 1D/2 block (vendored + component + token + demo entries)
- `docs\CHANGE-LOG.md` — this amendment (A40)

---

### A41 — Phase 2.8 Stage 2: LipiEmptyState (first Data Display component)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 2 (LipiEmptyState)
**Date**: 2026-05-30
**Status**: ✅ Shipped — first rendered Data Display component; composes `<LipiIcon>`

**What shipped.** `LipiEmptyState` — the zero-state primitive used by LipiTable / LipiList
empty slots, card empties, and full-page empty states. Stateless, presentational,
token-driven. Per the locked build order (EmptyState first, not §27.17's Stage 7), this is
the first rendered component of Phase 2.8 and the first real consumer of `<LipiIcon>`.

**Component surface.** `LiPi.Components.DataDisplay` namespace, in
`src\LiPi.Components\DataDisplay\LipiEmptyState\`:

- `LipiEmptyStateTypes.cs` — `EmptyStateVariant { Default, Empty, FilteredEmpty, Error,
  Success, Coming }` + `EmptyStateSize { Inline, Card, Page }`.
- `LipiEmptyState.razor` — markup: `role="region"` + `aria-label` (title), icon slot
  (3-way resolution), dynamic heading element, body `<p>`, collapsing CTA container.
- `LipiEmptyState.razor.cs` — code-behind: parameters, icon resolution, heading-level
  resolution, env-gated Title validation.
- `LipiEmptyState.razor.css` — scoped layout/structure; consumes only `--lipi-empty-*` +
  structure tokens (`--sp-*`, `--r-*`).

**Parameters.** `Title` (required), `Body`, `Icon`, `IconTemplate`, `Size` (default
`Inline`), `Variant` (default `Default`), `PrimaryCta`, `SecondaryCta`, `Class`, `Style`,
`TitleHeadingLevel`.

**Icon resolution order (§3.3).** (1) `IconTemplate` fragment wins; (2) explicit `Icon`
string — empty string `""` is an explicit opt-out (no icon); (3) variant default via the
A40 §5 mapping; (4) else no icon. Variant defaults (Lipicons-native names): Default/Empty
→ `empty-state`, FilteredEmpty → `search`, Error → `warning`, Success → `check-circle`,
Coming → `clock`. Icons referenced via the typed `LipiconName` constants for compile-time
safety. Icon variant is `Regular` (data-display default); icon size scales with component
size (Inline 32 / Card 48 / Page 64).

**Env-gated Title validation (A14 pattern).** Title is required. In Development, an empty
Title throws `InvalidOperationException` (fail-fast for the developer). In Production, it
logs a Warning and omits the title element (degrade gracefully — never crash a clinical
screen). `IHostEnvironment` + `ILogger` are injected as OPTIONAL (`[Inject]` on properties
that may be null) so the component still works in isolation tests / consumers that don't
register hosting; when absent, behavior defaults to the non-throwing production path. This
mirrors LipiButton's A14 hard-throw divergence and stays queued for the v1.1 retrofit that
will standardize env-gated validation across components.

**Heading level (§4.2).** Page → `h2`, Inline/Card → `h3`. `TitleHeadingLevel` (1–6)
overrides; out-of-range falls back to the size default. Rendered via a switch over the six
heading elements (no dynamic-tag trickery — explicit and SSR-safe).

**Token file — the single seam.** `src\LiPi.Web\wwwroot\css\lipi-empty-tokens.css` defines
the `--lipi-empty-*` tokens (dimensions, type sizes, gaps, variant icon tints, variant
container backgrounds) and binds the color-bearing ones to the real `--color-*` contract.
The component's scoped CSS consumes only `--lipi-empty-*`, so the component stays
redistributable (never touches `app.css`'s flat `--primary`/`--bg`, never touches
`--color-*` directly). The token file is the one place where HIS palette → generic
contract; when the full theming system lands in 2.10, this file is absorbed/expanded.

Token reconciliation applied (per A40): `--color-text-faint` → `--color-text-tertiary`;
`--color-danger-alpha-04` → `--color-danger-pale`; `--color-success-alpha-04` →
`--color-success-pale`. All bound tokens resolve in both light and dark via the
`[data-mode]` cascade — no mode-specific block needed in the token file. Verified each
referenced `--color-*` exists in `themes/mode-light.css` and `themes/mode-dark.css`.

**App.razor wiring.** `lipi-empty-tokens.css` linked AFTER `themes/mode-*.css` (it consumes
their tokens), grouped with the other `lipi-*.css` files before the Blazor isolation
bundle. All cache versions bumped `20260526` → `20260530`; cache-history row added. No JS —
EmptyState has no interop.

**Demo page.** `src\LiPi.Web\Pages\StyleGuideDataDisplay.razor` (+ scoped CSS) at
route `/admin/style-guide/data-display`. Named and routed to match the existing cross-page
demos (`StyleGuideFeedback`, `StyleGuideOverlays`): explicit
`@layout LiPi.Web.Components.Layouts.TopNavLayout`, and reached from the main StyleGuide via
a `data-enhance-nav="false"` link (the enhanced-nav scoped-CSS workaround — a Phase 2.10
smoke-test item). Shows all six variants (Inline), three sizes (Empty), and custom-icon
cases (explicit `Icon`, `IconTemplate`, opt-out `Icon=""`), plus accessibility notes. The
demo is self-styled (its own `sg-*` scoped CSS) so it renders cleanly without depending on
global StyleGuide styles, and uses plain `<button>` (no LipiButton dependency). Demo lives
in `LiPi.Web` (not redistributable) per the packaging rule.

**StyleGuide nav registration.** `StyleGuide.razor` gains a new "Phase 2.8" nav group with
a "Data Display →" link to `/admin/style-guide/data-display`, modeled on the Phase 2.6/2.7
groups (same `data-enhance-nav="false"` rationale). LipiTable / LipiList / LipiPagination
links will be added to this same group/page as those components ship in later 2.8 stages.

**One integration note for the build.** The demo and nav link use
`data-enhance-nav="false"` (full document load on click) — the established StyleGuide
cross-page pattern that avoids the enhanced-nav scoped-CSS reattachment bug. This is the
same workaround used by the Phase 2.6/2.7 demo links and is flagged for the Phase 2.10
cross-page-nav smoke test.

**Files** (Stage 2):

- `src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyStateTypes.cs` — enums
- `src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor` — markup
- `src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.cs` — code-behind
- `src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.css` — scoped styles
- `src\LiPi.Web\wwwroot\css\lipi-empty-tokens.css` — token seam (binds --lipi-empty-* to --color-*)
- `src\LiPi.Web\Pages\StyleGuideDataDisplay.razor` — StyleGuide demo (route /admin/style-guide/data-display)
- `src\LiPi.Web\Pages\StyleGuideDataDisplay.razor.css` — demo scoped styles
- `src\LiPi.Web\Pages\StyleGuide.razor` — +Phase 2.8 nav group ("Data Display →" link)
- `src\LiPi.Web\App.razor` — cache bump 20260526→20260530 + lipi-empty-tokens.css link
- `deploy-downloads.ps1` — Stage 1D/2 block (shared with A40)
- `docs\CHANGE-LOG.md` — this amendment (A41)

---

### A42 — Phase 2.8 Stage 2 core shell: LipiTable bare chassis (generic table renders static data)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 2 core shell (the "rolling chassis")
**Date**: 2026-05-31
**Status**: ✅ Shipped — generic LipiTable renders static columns of data; foundation for all later table stages

**What this is.** The first LipiTable slice — deliberately the thinnest table that renders.
Per the agreed step-by-step approach ("get the skeleton standing and rendering correctly
before adding sort, filter, selection, decoration"), this slice proves the hardest
architectural piece — the generic type cascade — with ~10 files instead of discovering a
problem after building 95. You write `<LipiTable Items=...>` with `<LipiColumn>` children
and a styled table appears: header row + one row per item, per-type cell rendering, body
states, light/dark aware. Nothing else works yet, by design.

**Scope — what renders vs. what's deferred.**

In this slice: declarative columns, CSS-Grid layout, the 14 ColumnType default renderers
(common types fully; flagged types simplified — see below), `Items` data path, and the
Loading / Error / Empty / Normal body states delegating to LipiEmptyState (built in A41).

Explicitly NOT in this slice (later stages, by agreement — kept the verification gate to
"does it render?"): sort, filter, quick-search, selection/checkboxes, pagination, inline
edit, grouping, tree, master-detail, column resize/reorder/pin, density toggle, toolbar,
header band, filter chips, bulk bar, group bar, aggregate footer row, virtualization, and
server-side `DataSource` mode. The corresponding `LipiColumn` / `LipiTable` parameters are
**declared but inert** so the public API shape is final now and pages won't face
breaking-change churn as those stages land.

**Architecture (locked by spec, confirmed by read).**

- `LipiTable<TItem>` with `[CascadingTypeParameter(nameof(TItem))]`;
  `LipiColumn<TItem, TValue>` children obtain the parent via `[CascadingParameter]` and
  register themselves during `OnInitialized`. The cascade is `IsFixed="true"` and the
  column markup sits in a `display:none` holder purely to trigger each column's
  initialization — `LipiColumn` renders no DOM of its own.
- **Type erasure at the registration boundary.** `LipiColumn<TItem, TValue>` builds a
  `ColumnDefinition<TItem>` (generic only over TItem) exposing a boxed
  `Func<TItem, object?> GetValue`. This lets `LipiTable<TItem>` hold a homogeneous
  `List<ColumnDefinition<TItem>>` and keep its render loop free of TValue — no reflection
  over open generics. Type-specific behavior that genuinely needs TValue (sort comparers,
  filter templates) is captured as erased delegates when those stages land.
- **CSS Grid backing** (§3.6.1): each column contributes one grid track from its
  `Width` / type-default. The header row and each body row are independent
  `display:grid` containers sharing the same inline `grid-template-columns`, which keeps
  columns aligned without a parent grid that would fight sticky header positioning.
- **`role="grid"`** semantics (§19): `role="row"` / `role="columnheader"` / `role="gridcell"`.
- **Identity via `KeySelector`** (`Func<TItem, object>`), used for `@key` on rows now and
  selection/edit-tracking/persistence later. Required — env-gated validation (throws in
  Development, logs + degrades in Production), matching the A14/A41 pattern.

**Files added — LiPi.Components (`DataDisplay/LipiTable/`):**

- `ColumnDefinition.cs` — type-erased per-column descriptor (key, header, type, align,
  grid track, format, boxed value accessor, optional cell/header templates).
- `CellFormatter.cs` — boxed-value → display-string for the common types (Number,
  Currency, Date, DateTime, Time, Boolean tokens), honoring an explicit `Format` string.
- `LipiColumn.razor` + `.razor.cs` — declarative column. Resolves ColumnKey (explicit >
  dotted Field path > fallback), Header (template > Header > humanized leaf member > key),
  Align (explicit > type default), and GridTrack (explicit Width > type default).
  Registers/unregisters with the parent. Field expression compiled once for the value
  accessor; `ValueSelector` supported as the no-metadata alternative.
- `LipiTable.razor` + `.razor.cs` + `.razor.css` — the generic component: column registry
  (keyed, re-registration replaces not duplicates), Items/DataSource/KeySelector
  validation, body-state resolution, grid template assembly, header + body render, and
  per-type cell rendering with the flagged simplifications.

**Files added — LiPi.Components (`Shared/Internal/`):**

- `IdentifierHumanizer.cs` — `"DateOfBirth"` → `"Date Of Birth"`, acronym-aware
  (`"patientUHID"` → `"Patient UHID"`), handles snake_case / kebab / letter-digit
  boundaries. Internal utility, reused by LipiList field headers later.

**Files added — LiPi.Web (`wwwroot/css/`):**

- `lipi-status-tokens.css` — **new shared infrastructure** (Phase 2.8 Overview §2.1/§2.3),
  created here because it didn't exist yet and the spec's deferred-list comment in the
  Stage 1C deploy block had only named it. Defines `--color-status-*` (the LipiStatus
  taxonomy bound to the real semantic palette), `.lipi-status-strip-{left,right,top}`
  utilities, and the `.lipi-table-status` chip + `[data-status]` color mapping. First
  consumer is LipiTable's Status cells; future consumers per the overview (LipiList,
  LipiCard/Alert retrofit, LipiBadge/Pill). Token reconciliation: the spec's example
  referenced `--color-success-600` / `--color-neutral-400` which don't exist; bound to the
  real `--color-success` / `--color-warning` / `--color-danger` / `--color-info` (+ `-pale`
  tiers, `+ -text-strong` for chip text) and `--color-text-tertiary` for neutral. Resolves
  per `[data-mode]`.
- `lipi-table-tokens.css` — the `--lipi-table-*` seam bound to the real `--color-*`
  contract (same pattern as `lipi-empty-tokens.css`). Component scoped CSS consumes only
  `--lipi-table-*` + structure tokens, never `--color-*` directly, so the component stays
  redistributable.

**Files modified (full files, routed by existing deploy entries):**

- `App.razor` — cache bump `20260530` → `20260531`; added `<link>` for
  `lipi-status-tokens.css` and `lipi-table-tokens.css` (after the theme mode files,
  grouped with the other `lipi-*.css` token files); cache-history row added. The
  historical `20260530` row was preserved (not bumped).
- `StyleGuideDataDisplay.razor` — added a "LipiTable — static data (bare chassis)" section
  with a 7-column staff table (Name, Mono code, Department, Date, Currency w/ Sum
  aggregate declared, Boolean, Status) over 6 demo rows, plus an empty-state table. Demo
  `DemoStaff` record + data added to `@code`. Domain-neutral demo data (staff, not
  clinical PHI).
- `StyleGuideDataDisplay.razor.css` — added `sg-note-p` / `sg-subhead` helper classes.

**Flagged simplifications (deliberate; each completed in its proper later stage).**

- **Status** → rendered as a simple `.lipi-table-status` chip styled by
  `lipi-status-tokens.css` via `data-status`. Full `LipiBadge` / `LipiPill` composition
  waits until those Phase 2.7 components migrate into `LiPi.Components` (Phase 2.10) or a
  Components-local badge is built — using the `LiPi.Web` copies now would break the
  isolation contract.
- **Date / DateTime / Time** → formatted via the column `Format` string or invariant
  culture `ToString`. Culture-aware formatting via `IDateFormatService` (a `LiPi.Web`
  service) is deferred until an isolation-clean abstraction exists; injecting the Web
  service into a redistributable component would break isolation.
- **Avatar** → initials-circle placeholder. Full image/initials-with-hash-color rendering
  is Stage 3 (rows/cells).
- **Actions** → renders the caller's `CellTemplate` only (no auto-injected edit/save
  buttons — those arrive with inline edit in Stage 5).
- **Currency** → culture currency format; `CurrencyCode` override deferred.

**Isolation verified.** `LipiTable` / `LipiColumn` / `ColumnDefinition` / `CellFormatter` /
`IdentifierHumanizer` reference no `LiPi.Web` / `LiPi.HIS` / `LiPi.Clinic` / `LiPi.Master`
type, and carry no clinical names (Patient / Doctor / Encounter / Aadhaar / UHID / MRN) in
the public surface. Scoped CSS references only `--lipi-table-*` + structure tokens.

**Verification gate.**

```
cd C:\Users\aruns\Documents\lipi-complete\lipi-complete
dotnet build src\LiPi.Web\LiPi.Web.csproj
dotnet run --project src\LiPi.Web
```

Sign in → `/admin/style-guide` → Phase 2.8 → Data Display → the new "LipiTable — static
data" section shows a styled 7-column table (date formatted, currency with ₹ + thousands,
mono code, boolean check glyph, status chips), striped rows, sticky header, correct
per-type alignment, and an empty-state table below — all light/dark aware. No F12 console
errors, no server errors.

**Files**:

- `src\LiPi.Components\DataDisplay\LipiTable\ColumnDefinition.cs` — type-erased descriptor
- `src\LiPi.Components\DataDisplay\LipiTable\CellFormatter.cs` — per-type value formatting
- `src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor` — declarative column (no DOM)
- `src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor.cs` — column code-behind
- `src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor` — table markup (grid + cells)
- `src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor.cs` — table code-behind
- `src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor.css` — scoped grid styles
- `src\LiPi.Components\Shared\Internal\IdentifierHumanizer.cs` — header humanizer
- `src\LiPi.Web\wwwroot\css\lipi-status-tokens.css` — shared status taxonomy (new infra)
- `src\LiPi.Web\wwwroot\css\lipi-table-tokens.css` — table token seam
- `src\LiPi.Web\App.razor` — cache bump + 2 CSS links
- `src\LiPi.Web\Pages\StyleGuideDataDisplay.razor` — +LipiTable demo section + data
- `src\LiPi.Web\Pages\StyleGuideDataDisplay.razor.css` — +demo helper classes
- `deploy-downloads.ps1` — Stage 2 core-shell block (8 component + 2 CSS entries)
- `docs\CHANGE-LOG.md` — this amendment (A42)

---

### A42.1 — Phase 2.8 Stage 2 core shell: LipiTable header weight tweak (visual review)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 2 core shell (post-render visual review)
**Date**: 2026-05-31
**Status**: ✅ Shipped — token-only tweak; no component/logic change

**Trigger.** First-render visual review of the bare-chassis LipiTable (A42). The table
rendered correctly (dates, ₹ currency with lakh grouping, mono codes, boolean glyphs,
status chips, alignment all correct), but the column header row read faint: 600 weight
in the muted secondary grey at the 10px uppercase header size receded more than intended.

**Change.** Two tokens in `lipi-table-tokens.css`:
- `--lipi-table-header-weight`: `var(--fw-semi)` (600) → `var(--fw-bold)` (700).
- `--lipi-table-header-text`: `var(--color-text-secondary)` → `var(--color-text-primary)`.

Weight alone wouldn't have fixed the faintness — the secondary grey was doing most of the
recession at that size. Bumping to primary text color + 700 makes the header read as a
clear, deliberate band without being shouty. Standards note: most data tables (Material,
AG-Grid) use 600 for headers because they pair it with darker header text; this change
aligns the weight/color pairing rather than over-weighting on a light color.

**Cache.** `App.razor` cache version bumped `20260531` → `20260532` so the updated CSS
loads without a manual hard-refresh.

**Files**:

- `src\LiPi.Web\wwwroot\css\lipi-table-tokens.css` — header weight 700 + header text primary
- `src\LiPi.Web\App.razor` — cache bump 20260531 → 20260532
- `docs\CHANGE-LOG.md` — this amendment (A42.1)

---

### A43 — Phase 2.8 Migration Stage 1: input/selection family → LiPi.Components

**Phase**: 2 Sub-step 2.8 — component-library migration (Phase 2.10 item #5, pulled forward)
**Date**: 2026-05-31
**Status**: ✅ Shipped — input/selection family now lives in the redistributable LiPi.Components package

**Why now (roadmap pull-forward).** Stage 3 of LipiTable needs the selection checkbox column,
which must render the real `LipiCheckbox`. The locked direction is "all components move into
LiPi.Components; LipiCheckbox only, no inert/native placeholder." Rather than fake a checkbox
or break the isolation contract by referencing LiPi.Web from the redistributable table, the
input/selection family is migrated now. This is the first slice of the Phase 2.10 item-#5
batch migration ("migrate Phase 2.1–2.7 components from LiPi.Web.Components.Shared into
LiPi.Components"), done early and in dependency order.

**Scope decision — input/selection family only.** The full Shared folder is 113 files across
three tiers: (A) clean leaf/foundation components with no LiPi.Web reach-back, (B) components
coupled to LiPi.Web services (Modal, Drawer, Toast, DynamicTabs, date/time pickers — inject
IFocusTrapService, ILipiModalService, IDateFormatService, IClinicTimezoneService, etc.), and
(C) HIS-specific files that must never migrate (PatientFab, samples, LucideIcon). Migrating all
113 at once would break the build (Tier B injections failing across the boundary) and pollute
the package (Tier C). So the migration proceeds tier-by-tier. This amendment is **Migration
Stage 1**: the input/selection family (all Tier A, verified no LiPi.Web reach-back). Later
slices handle the other leaves (Button/Badge/Pill/Card/Alert/Spinner/Skeleton/Tabs/Validation)
and — last, with deliberate service-architecture decisions — the Tier B service-coupled set.

**What moved (37 files) — namespace LiPi.Web.Components.Shared → LiPi.Components,
location src\LiPi.Web\Components\Shared\ → src\LiPi.Components\Forms\:**

- Bases/config/types: `LipiInputBase.cs`, `LipiInputDefaults.cs`, `LipiSelectionTypes.cs`,
  `LipiTextInputTypes.cs`, `LipiSelectBase.cs`, `LipiMultiSelectBase.cs`, `LipiContainerBase.cs`,
  `ICompoundSegment.cs`, `AutocompleteValidator.cs`, `MustBeTrueAttribute.cs`
- Selection family: `LipiCheckbox` (+css), `LipiCheckboxGroup` (+css), `LipiCheckboxGroupContext.cs`,
  `LipiRadio` (+css), `LipiRadioGroup` (+css), `LipiRadioGroupContext.cs`, `LipiToggle` (+css)
- Text/select family: `LipiTextBox` (+css), `LipiTextArea` (+css), `LipiNumberInput` (+css),
  `LipiSelect` (+css), `LipiCombobox` (+css), `LipiMultiSelect`, `LipiMultiCombobox`
- Compound: `LipiCompoundField`, `SelectSegment`, `TextSegment`

**Verified clean before moving.** Every one of the 37 references only framework types
(InputBase<T>, IJSRuntime, IOptions, IWebHostEnvironment, ILogger) + its own Lipi types — NO
LiPi.Web service, NO LiPi.Clinic/Master, NO HIS specifics. `IWebHostEnvironment`/`ILogger`
resolve from LiPi.Components's existing FrameworkReference Microsoft.AspNetCore.App (already
proven — LipiEmptyState and LipiTable inject the same). `LipiInputBase : InputBase<TValue>`
needs Microsoft.AspNetCore.Components.Forms (shared framework — present). No new package.

**Namespace mechanics.** The .razor files had no @namespace directive (folder-derived
namespace), so moving the folder would have computed LiPi.Components.Forms — a mismatch with
the .cs files' explicit `namespace LiPi.Components;`. Fix: every migrated .razor got an explicit
`@namespace LiPi.Components` directive (decouples namespace from folder, standard library
practice). The .cs files' `namespace LiPi.Web.Components.Shared;` → `namespace LiPi.Components;`.
Internal fully-qualified references (the `LiPi.Web.Components.Shared.RequiredVisualStyle.ApricotTint`
collision-avoidance pattern from LipiInputDefaults, used to disambiguate the property/enum name
clash) rewritten to `LiPi.Components.RequiredVisualStyle...` — still disambiguates correctly.

**Consumer edits (kept everything compiling — minimal blast radius).** The whole app resolved
the family via a single global `@using LiPi.Web.Components.Shared` in the webroot _Imports.razor.
The redirect is therefore one line:

- `src\LiPi.Web\_Imports.razor` — `+@using LiPi.Components` (after the existing Shared using).
  This covers EVERY .razor consumer: all HIS pages, and the four still-in-Web date/time pickers
  (`LipiDatePicker`, `LipiTimePicker`, `LipiDateTimePicker`, `LipiDateRangePicker`) that
  `@inherits LipiInputBase<T>` — they now inherit the moved base across the project boundary
  (LiPi.Web → LiPi.Components reference already exists). The other two _Imports.razor (Components
  subfolder, Pages) don't declare the Shared using and need no change — the webroot one is
  hierarchically above them.
- `src\LiPi.Web\Components\Shared\LipiSpinner.razor.cs` — `+using LiPi.Components;` (the one .cs
  code-behind referencing a moved type, `InputLabelPosition`; .cs files don't read _Imports).
- `src\LiPi.Web\Program.cs` — `+using LiPi.Components;` so `Configure<LipiInputDefaults>`
  resolves from the new namespace. (DI registration unchanged; the assembly was already
  referenced via `using LiPi.Components.DataDisplay;`.)

Verified no type-name collision between the migrated set and the remaining Shared files, so the
dual `@using` (old + new) in _Imports is unambiguous.

**Global CSS unchanged.** `lipi-inputs.css` and `lipi-selection-family.css` STAY in
src\LiPi.Web\wwwroot\css\ (global utility CSS, already linked in App.razor, consumed by the
migrated components' scoped CSS via the documented `.lipi-input-*` class contract). Per the
token-seam pattern, redistributable components depend on the documented class contract; moving
global CSS would be unnecessary churn. App.razor unchanged — no new links, no cache bump.

**⚠ MANUAL DELETION STEP (required, sequence matters).** The deploy script is copy-only (no
delete). The 37 OLD copies in src\LiPi.Web\Components\Shared\ MUST be removed BEFORE deploying
the new ones, or the build sees duplicate type definitions in two projects (ambiguous reference
errors). The exact `Remove-Item` block is documented at the top of deploy-downloads.ps1.
Sequence: (1) run the Remove-Item block → (2) run deploy → (3) dotnet build.

**Gate.** After delete-old → deploy-new → `dotnet build src\LiPi.Web\LiPi.Web.csproj`: solution
builds green; existing HIS forms still render (date pickers inherit the moved base via the
_Imports redirect); `LipiCheckbox` is now resolvable from LiPi.Components for the LipiTable
selection column (unblocks Stage 3 checkbox work).

**Files**:

- 37 component files → `src\LiPi.Components\Forms\*` (namespace LiPi.Components)
- `src\LiPi.Web\_Imports.razor` — +@using LiPi.Components
- `src\LiPi.Web\Components\Shared\LipiSpinner.razor.cs` — +using LiPi.Components
- `src\LiPi.Web\Program.cs` — +using LiPi.Components
- `deploy-downloads.ps1` — 37 entries repointed to Forms\ + manual-deletion block
- `docs\CHANGE-LOG.md` — this amendment (A43)

---

### A44 — Phase 2.8 Migration Stage 1 fix-up: LiPi.Components build errors + LiPicons v1.0.4 icon cutover (input family)

**Phase**: 2 Sub-step 2.8 — Migration Stage 1 follow-up (build fix + icon cutover)
**Date**: 2026-05-31
**Status**: ✅ Shipped — resolves the 26 build errors from the A43 migration; input family now renders LiPicons (not Lucide)

**Trigger.** The A43 migration moved the input/selection family into LiPi.Components. First
build surfaced 26 errors + 57 warnings, all tracing to three root causes — none a logic bug,
all consequences of the cross-project move. Fixed here, and per the locked icon direction the
LucideIcon dependency is resolved by cutting the family over to LiPicons (not by vendoring
LucideIcon forward).

**Root cause 1 — missing framework usings in LiPi.Components (≈22 errors).**
`IsDevelopment()` (Microsoft.Extensions.Hosting) and the `ILogger` `Log*` methods
(Microsoft.Extensions.Logging) are extension methods. They resolved in LiPi.Web via that
project's imports; in LiPi.Components they were unqualified. Fixes:
- **New `src\LiPi.Components\_Imports.razor`** — project-wide Razor usings (System, Components,
  Forms, Web, JSInterop, Extensions.Hosting, Extensions.Logging, Extensions.Options). This
  resolves every `.razor` component's errors at once (TextBox, TextArea, NumberInput, Select,
  Combobox, MultiSelect, MultiCombobox, SelectSegment, TextSegment).
- **The four base `.cs` files** (`.cs` don't read `_Imports`) got explicit usings:
  `LipiInputBase.cs` + `LipiContainerBase.cs` → +`using Microsoft.Extensions.Hosting;` (they
  had AspNetCore.Hosting for the IWebHostEnvironment type, but the modern `IsDevelopment()`
  extension lives in Extensions.Hosting). `LipiSelectBase.cs` + `LipiMultiSelectBase.cs` → had
  neither Hosting nor Logging; added AspNetCore.Hosting + Extensions.Hosting + Extensions.Logging.

**Root cause 2 — LucideIcon unresolvable → full LiPicons cutover for the input family
(50+ RZ10012 warnings).** The migrated components rendered `<LucideIcon>`, which stays in
LiPi.Web (Tier C, being retired). A redistributable component can't reference back into
LiPi.Web. Per the locked "LiPicons everywhere, retire Lucide" direction, the fix is the icon
cutover (Path A), not vendoring LucideIcon. To do it cleanly the LiPicons package was first
updated to **v1.0.4 (1,164 icons)** — which added exactly the glyphs the family needed
(`minus`, `square`, `check-square`, `circle-x`), making the Lucide→LiPicons map fully 1:1 with
zero substitutions.

In 10 component files (`LipiCombobox`, `LipiCompoundField`, `LipiMultiCombobox`,
`LipiMultiSelect`, `LipiNumberInput`, `LipiSelect`, `LipiTextArea`, `LipiTextBox`, `LipiToggle`,
`SelectSegment`):
- `<LucideIcon>` → `<LipiIcon>`; attribute `CssClass` → `Class` (LipiIcon's API).
- `+@using LiPicons.Blazor` after each `@namespace LiPi.Components`.
- Name map: `x` → `close`, `alert-triangle` → `warning` (both markup and the
  `EffectiveTrailingIcon` C# switch defaults). All other names unchanged — `check`, `plus`,
  `search`, `info`, `minus`, `square`, `check-square`, `circle-x`, `chevron-up`, `chevron-down`
  exist natively in v1.0.4. Every icon name in the family verified against the v1.0.4 manifest.
- LipiCheckbox / LipiRadio draw their indicator via inline SVG + CSS (no icon dependency) — not
  touched by the cutover.

**Root cause 3 — LipiColumn.razor.cs `ColumnAlign.Inherit` (1 error).** The Stage-2 fix made
earlier this session (the enum has no `Inherit` member; null Align falls through to the type
default) had never actually deployed. Corrected file shipped.

**Warnings also fixed (CS1570 malformed XML doc comments).**
- `LipiSelectBase.cs` — duplicate `/// <summary>` opener collapsed.
- `LipiMultiSelectBase.cs` — `InputBase<List<TValue>>` in a `<summary>` escaped to
  `InputBase&lt;List&lt;TValue&gt;&gt;`.
- `LipiCheckboxGroup.razor` — `Required && IsEmpty` in a doc comment escaped to `&amp;&amp;`.

**LiPicons v1.0.4 re-vendor.** `libs\LiPicons.Blazor\` updated from v1.0.2 → v1.0.4: `icons.json`
(1,149 → 1,164 icons), regenerated `LipiconName.cs`, version-bumped csproj. Same filenames, same
deploy entries (overwrite in place) — no repoint, no deletion needed. The EmbeddedResource
repoint to local `icons.json` preserved.

**Naming note.** The icon system is **LiPicons** (one word); the Blazor package/namespace is
**`LiPicons.Blazor`**; the component written in markup is **`<LipiIcon>`** (Lipi-prefixed, like
all components). All code uses these forms; verified no `LiPiIcons` / `LiPi Icons` variants.

**Deploy note — `_Imports.razor` filename collision.** LiPi.Web already has an `_Imports.razor`
deploy key. The deploy script matches by source filename in Downloads\LiPi\, so two files named
`_Imports.razor` can't coexist there. The LiPi.Components one is delivered as source filename
`_Imports.Components.razor` with a deploy entry mapping it to `src\LiPi.Components\_Imports.razor`
(Copy-Item names the destination from the target path, so it lands correctly named).

**Gate.** `dotnet build src\LiPi.Web\LiPi.Web.csproj` → green (was 26 errors). Input components
render LiPicons glyphs; LipiTable's selection column can use the real LipiCheckbox; all existing
HIS forms render unchanged (icons swapped like-for-like).

**Files**:

- `libs\LiPicons.Blazor\*` — re-vendored v1.0.4 (csproj, LipiIcon.razor, LipiconVariant.cs, LipiconRenderer.cs, IconManifest.cs, LipiconName.cs, icons.json)
- `src\LiPi.Components\_Imports.razor` — NEW (framework usings; source file _Imports.Components.razor)
- `src\LiPi.Components\Forms\LipiInputBase.cs`, `LipiSelectBase.cs`, `LipiMultiSelectBase.cs`, `LipiContainerBase.cs` — +Hosting/Logging usings (+XML fixes in SelectBase/MultiSelectBase)
- `src\LiPi.Components\Forms\LipiCombobox.razor`, `LipiCompoundField.razor`, `LipiMultiCombobox.razor`, `LipiMultiSelect.razor`, `LipiNumberInput.razor`, `LipiSelect.razor`, `LipiTextArea.razor`, `LipiTextBox.razor`, `LipiToggle.razor`, `SelectSegment.razor` — LucideIcon→LipiIcon cutover
- `src\LiPi.Components\Forms\LipiCheckboxGroup.razor` — XML doc-comment escape
- `src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor.cs` — ColumnAlign.Inherit fix
- `deploy-downloads.ps1` — +_Imports.Components.razor entry
- `docs\CHANGE-LOG.md` — this amendment (A44)

---

### A45 — Phase 2.8 Stage 3: LipiTable rows/cells (density, selection column, avatar, copy, row hooks)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 3 (rows/cells refinement)
**Date**: 2026-05-31
**Status**: ✅ Shipped — six rows/cells features land on the LipiTable bare chassis (A42); all rendering, no selection behavior yet (that is Stage 4).

**Scope.** Stage 3 turns the bare chassis into a presentable table. Six features, all in one stage per the locked plan, built against the existing type scaffolding (the `TableDensity` / `SelectionMode` / `RowStatusPlacement` enums already existed from Stage 1A):

1. **Density (§14.2/§14.3).** New `Density` parameter (`TableDensity`, default `Comfortable`) → `data-density="compact|comfortable|spacious"` on the table root. `lipi-table-tokens.css` restructured: the measurement tokens (row height 32/40/52, body font 11/13/14, cell padding, selection-column width, avatar diameter 24/32/40, row border 0.5/1/1px, hover transition 80/120/160ms) now switch per `[data-density]` block. The header font stays constant at 12px (`--ts-label`) across all densities per §14.3 — header is structural scaffolding, not content. Comfortable is defined on both `:root` (fallback when no attribute is present) and its own `[data-density="comfortable"]` block.

2. **Selection column (§5.6) — structure only, inert.** New `SelectionMode` parameter (default `None`). When not `None`, a leading select column renders a **pure-visual checkbox placeholder** (`<span class="lipi-table-select-box">`, density-sized, `role="checkbox" aria-disabled="true"`) — header (Multi mode only) + per-row. The grid template gains a leading `var(--lipi-table-select-w)` track. The placeholder is visibly present but **does nothing**.

   **Design note (changed during build).** The original plan rendered the *real* migrated `LipiCheckbox` here (read-only/inert). That failed at runtime: `LipiCheckbox<bool>` derives from Blazor `InputBase<T>`, which throws `InvalidOperationException: requires a value for the 'ValueExpression' parameter` whenever it has no two-way binding — a hard requirement, not EditContext-gated. Rather than satisfy it with a dummy `ValueExpression`/no-op `ValueChanged` (a false construct), we switched to a pure-visual placeholder. This also matches how mature data grids (AG-Grid, MUI DataGrid) render selection cells — a lightweight visual the grid controls centrally, not a bound form-input per row. Stage 4's selection model will own checkbox state centrally and render the real interactive control then.

   ⚠️ **STAGE 4 RE-TEST OBLIGATION:** the selection column is a *visual placeholder only* this stage. When Stage 4 wires real selection behavior (toggle, select-all, indeterminate, cross-page banner), the placeholder is replaced by the real interactive control and the **full selection column must be re-tested** — render, keyboard (Space toggle, Ctrl/Cmd+A), indeterminate state, `RowDisabled` interaction, and the `var(--lipi-table-select-w)` track sizing across all three densities. Do not assume the Stage 3 visual proves Stage 4 behavior.

3. **Avatar (§3.3.2).** `ColumnType.Avatar` now renders a real avatar: a circular `<img>` when the value looks like a URL (http/https/`/`/`data:image`) and the column is not `IsInitials`; otherwise an initials circle with a deterministic 8-color hash background (same name → same color, stable across renders via an FNV-style hash). New `IsInitials` column parameter forces the initials treatment. Diameter follows density.

4. **Cell ellipsis + copy affordance (§3.8).** Text-like cells get a native `title` tooltip for truncated content (ellipsis was already in the chassis CSS). `Copyable="true"` columns show a hover-reveal copy button (LiPicons `copy` glyph) that copies the cell value to the clipboard; on success the icon swaps to `check` and an inline **"Copied!"** flash shows for ~1.4s, then clears. Copy is **isolation-clean** — it uses a component-local JS file (see below) and an optional `OnCopy` `EventCallback<string>` for hosts that want to raise their own toast. No dependency on `LipiToast` (which is still in `LiPi.Web`, unmigrated).

5. **Row formatting hooks (§22.2–22.5).** `RowStatus` (`Func<TItem,string?>`) → applies `lipi-status-strip-{left|right|top}` + `data-status` on the row; placement via new `RowStatusPlacement` parameter. `RowClass` (`Func<TItem,string?>`, appended after the standard row classes) and `RowStyle` (`Func<TItem,string?>`, inline) are caller escape hatches. `RowDisabled` (`Func<TItem,bool>`) → `lipi-table-row--disabled` (opacity 0.5, `pointer-events:none`).

6. **Status strip taxonomy.** `lipi-status-tokens.css` strip-color rules extended from 3 values (active/pending/locked) to the **full 16-value taxonomy** (§22.2.2) — success/completed/published (green), warning/in-progress/suspended/draft (amber), failed/error/cancelled (red), info (blue), inactive/archived (neutral). The chip rules already covered all 16; this brings the strip rules to parity.

**Component-local JS (Option B — self-contained package).** New `src\LiPi.Components\wwwroot\lipi-table.js` exposes `window.lipiTable.copy(text)` (async Clipboard API + `execCommand` fallback, returns bool). Because `LiPi.Components` is a `Microsoft.NET.Sdk.Razor` project with `IsPackable=true`, the file is served at `_content/LiPi.Components/lipi-table.js` and packs into the NuGet automatically — so the table ships its own JS, no host wiring required by an external consumer. This is the first static web asset in the package's `wwwroot`; the deploy auto-creates the directory. (The token CSS seam remains in `LiPi.Web\wwwroot` for now; consolidating all component static assets under `_content/` is queued for the Phase 2.10 isolation audit rather than done piecemeal.)

**Files.**
- Component (LiPi.Components): `LipiTable.razor` (+selection column, avatar img/initials, copy affordance, row hooks, density attr), `LipiTable.razor.cs` (+7 parameters, avatar hash, copy JS interop, row helpers), `LipiTable.razor.css` (+Stage 3 rules), `LipiColumn.razor.cs` (+`IsInitials`, `CopyTarget` params), `ColumnDefinition.cs` (+`IsInitials`, `CopyTarget`), **new** `wwwroot\lipi-table.js`.
- Shared CSS (LiPi.Web wwwroot): `lipi-table-tokens.css` (density restructure), `lipi-status-tokens.css` (16-value strip coverage).
- Demo (LiPi.Web): `StyleGuideDataDisplay.razor` (+3 sections: density toggle, avatar/selection/copy, row status & formatting) + `.razor.css` (+`sg-density-toggle`).
- `App.razor`: +`_content/LiPi.Components/lipi-table.js` script (before `blazor.web.js`); all cache versions bumped 20260532 → **20260533** (token CSS changed).

**Not in scope (deferred).** Selection *behavior* (toggle, select-all, banner) → Stage 4. Sort / filter / pin / group / tree / edit → their respective later stages. `CopyTarget=Href/Url` is plumbed on the column but only `Value` is wired this stage (Link/File columns are bare-chassis text).

**Build-fix follow-ups (same A45 batch).**
- *Selection checkbox → pure-visual placeholder* (see design note above): the real `LipiCheckbox` threw `ValueExpression` at runtime; replaced with `<span class="lipi-table-select-box">`.
- *Copy button scoped-CSS fix (two attempts)*: the copy button was initially emitted via `RenderTreeBuilder` (C#), so Blazor's per-component scope attribute (`[b-xxxxx]`) never reached it — the scoped `.lipi-table-cell-copy` rules in `LipiTable.razor.css` didn't match, and the button rendered with default browser chrome (always-visible boxed button instead of the hover-reveal icon). Confirmed via DevTools (only user-agent + global resets applied to the button). First attempt moved the button to **markup** in `LipiTable.razor` — but DevTools showed the `<button>` *still* had no scope attribute while sibling `<div>`s did, so the scoped rules still didn't apply. Final fix: the `.lipi-table-cell-copy*` and `.lipi-table-select-box*` rules were moved **out of the scoped `LipiTable.razor.css` into the global `lipi-table-tokens.css`**. Global CSS matches on class name alone (scope attribute is additive), so `.lipi-table-row:hover .lipi-table-cell-copy` resolves correctly. The structural cell rules (`.lipi-table-cell` position context, `.lipi-table-cell-select`) stay scoped — they sit on `<div>`s that do receive the scope attribute. Cache bumped 20260533 → **20260534** (token CSS changed again). **Lesson: scoped Blazor CSS is unreliable for elements rendered conditionally/dynamically; component-global rules belong in the `lipi-*-tokens.css` seam file, which is the documented home for the table's non-scoped styling anyway.**
- *Class strings* for both elements built via `CopyButtonClass`/`SelectBoxClass` helpers to avoid literal+ternary concatenation in markup attributes (Razor rule 3).

**Verification gate.** `dotnet build src\LiPi.Web\LiPi.Web.csproj` green; demo page `/admin/style-guide/data-display`: density toggle visibly changes row height/font; avatar column shows initials with stable hash colors; selection placeholder boxes render (inert — clicking does nothing, by design); Code column shows hover-copy with "Copied!" flash + `OnCopy` echo; row status strips + high-CTC RowStyle tint render. The table now has **zero dependency on unmigrated Tier-B components** — no `LipiToast` (copy uses inline flash + `OnCopy`), no `LipiCheckbox` (selection uses the pure-visual placeholder), keeping `LiPi.Components` isolation-clean.

---

### A46 — Phase 2.8 Stage 4a: LipiTable selection (core)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 4a (selection — core)
**Date**: 2026-05-31
**Status**: ✅ Shipped — real interactive single/multi selection with bound state. Power interactions (4b) and scale features (4c) deferred.

**Scope.** Stage 4 (selection) is sliced 4a/4b/4c. This is **4a — core selection**: the 80% that makes selection work on a single page, eye-verifiable. It replaces the Stage 3 inert placeholder with a real, bound `LipiCheckbox`, discharging the Stage 3 re-test obligation.

**Selection state model (§5.2.2).** Internal `HashSet<object> _selectedKeys` keyed via `KeySelector` — the spec-mandated key-set (not `TItem` references), so cross-page persistence (4c) drops in without rework. Single-page this slice.

**Parameters added (§5.7).**
- `@bind-SelectedItems` (`IReadOnlyList<TItem>` + `SelectedItemsChanged`) — never null, empty list when none; reflects visible selected rows.
- `@bind-SelectedKeys` (`IReadOnlyCollection<object>?` + `SelectedKeysChanged`) — raw key set; wired now (cheap, completes the binding surface, 4c builds on it).
- `OnSelectionChanged` (`EventCallback<SelectionChangedContext<TItem>>`) — fires after every change.
- `AllowDeselectInSingleMode` (default true) — §5.1.4.
- `SelectionPlacement` (Left default) — §5.6.1; **Left honored, Right declared but visually deferred** (see "not in scope").

**`SelectionChangedContext` signature note.** The on-disk `Contexts.cs` record (from Stage 1 scaffolding) is `(SelectedItems, AddedItems, RemovedItems, Reason)` — an added/removed diff model — which differs from the §5.7.3 spec text `(SelectedKeys, SelectedItemsVisible, IsSelectAllAcrossPagesActive, Reason)`. **On-disk wins** (strict file rule; the record predates this stage and is the richer API). `SelectionChangeReason` uses the on-disk enum values (`UserClickCheckbox`, `UserSelectAllOnPage`, `UserClearSelection`, `Programmatic`, `DataChange`, plus the 4b/4c reasons already declared). Spec doc to be reconciled to match the code in the Phase 2.10 audit.

**Rendering (§5.6) — real LipiCheckbox, indeterminate resolved.**
- **The earlier indeterminate concern was a false alarm.** `LipiCheckbox` already supports tri-state natively via `TValue=bool?` (null → CSS dash + `aria-checked="mixed"`). No `Indeterminate` parameter exists or is needed; `LipiCheckboxGroup` has no select-all of its own. So the header select-all is simply a `bool?` checkbox — no Forms-family change, no table-local visual, no deferral.
- **Header select-all** (`Multi` only): `LipiCheckbox TValue="bool?"` — `HeaderTriValue` maps to true (all visible selected) / false (none) / null (some → indeterminate dash). `:set` ignores the incoming value and routes to `ToggleSelectAllOnPageAsync` (decides add vs remove from current state). §5.6.2.
- **Row checkbox**: `LipiCheckbox TValue="bool"` — `:get` = `IsRowSelected`, `:set` routes to `ToggleRowAsync`. Disabled when the row is `RowDisabled`. §5.6.3.
- **`@bind-Value:get/:set` binding form** used deliberately: it makes the Razor compiler emit a non-null `ValueExpression`, which is what the Stage 3 inert checkbox lacked (that threw `InvalidOperationException: requires a value for ValueExpression`). Explicit `Value`+`ValueChanged` without `:get/:set` does not guarantee `ValueExpression` and risks the same throw. **Lesson: bind LipiCheckbox via `@bind-Value:get/:set`, never bare `Value`+`ValueChanged`.**
- **`lipi-table-row--selected`** modifier (subtle `--color-primary-pale` tint, wins over striping) added to selected rows.

**Behavior (4a = click-to-toggle).**
- Multi: row checkbox toggles that key; header toggles all visible rows (page-level, §5.3.1 step 1 — no across-pages banner, that's 4c).
- Single: selecting a row replaces any prior selection; `AllowDeselectInSingleMode` controls whether re-clicking clears it.
- Disabled rows are non-interactive (checkbox disabled).
- Every change emits `SelectedItemsChanged` + `SelectedKeysChanged` + `OnSelectionChanged` (with added/removed diff + reason), then re-renders.

**Public programmatic API (via `@ref`, §5.7.4 — 4a subset).** `SelectAsync` / `DeselectAsync` / `ToggleSelectionAsync` / `SelectKeysAsync` / `DeselectKeysAsync` / `SelectAllOnPageAsync` / `ClearSelectionAsync` / `GetSelectedKeys` / `GetSelectedVisibleItems` / `IsSelected` / `IsKeySelected`.

**Files.**
- Component (LiPi.Components): `LipiTable.razor` (real checkboxes + row-selected class), `LipiTable.razor.cs` (selection state model + params + 11-method ref API), `LipiTable.razor.css` (`lipi-table-row--selected`), `LipiTableTypes.cs` (+`SelectionPlacement` enum). `Contexts.cs` unchanged (record already existed).
- Shared CSS (LiPi.Web wwwroot): `lipi-table-tokens.css` — dead `.lipi-table-select-box` placeholder rules removed (real checkbox replaces them); `--lipi-table-row-bg-selected` / `--lipi-table-row-selected-accent` tokens added.
- Demo (LiPi.Web): `StyleGuideDataDisplay.razor` (+ interactive Multi-select demo with live `@bind-SelectedItems` count, + Single-select demo) — the Stage 3 "selection placeholder" note updated to point at the real demo.
- `App.razor`: cache bump 20260534 → **20260535** (token CSS changed). **deploy-downloads.ps1 unchanged** — every touched file already has an entry; `LipiCheckbox` was deployed in A43.

**Not in scope (deferred).**
- **4b (power interactions):** shift-click range + anchor, Ctrl/Cmd-click, keyboard (Space / Shift+Space / Ctrl-Cmd+A / Shift+Arrow / Escape), `OnRowClick` modifier interplay, platform tooltip.
- **4c (scale):** two-step across-pages banner + `IsSelectAllAcrossPagesActive` + `SelectAllAcrossPagesAsync`, bulk action bar (region ③), `PreserveSelectionOnDataChange`, server-side key persistence, `SelectAllBannerTemplate`.
- **`SelectionPlacement.Right`** — parameter accepted but the checkbox always renders leading in 4a; Right-placement layout is a small follow-up (grid track + cell order), deferred to avoid grid-template churn this slice.

**Runtime-fix follow-ups (same A46 batch).**
- *FieldIdentifier error → `LipiCheckboxGroup` binding pattern.* `@bind-Value:get/:set` produced a `ValueExpression` of `() => IsRowSelected(item)` — a method call, which `FieldIdentifier.ParseAccessor` rejects ("only supports simple member accessors"). Switched to the proven pattern from `LipiCheckboxGroup`: explicit `Value` (computed) + `ValueChanged` (`EventCallback.Factory.Create<T>`) + `ValueExpression="@(() => _dummyBackingField)"` where the dummy is a real field (parses cleanly, never read). Two dummy fields added (`_rowCheckboxBacking`, `_headerCheckboxBacking`) with `#pragma warning disable CS0649`, mirroring the group's `_childCheckboxBacking`.
- *Double-click-to-toggle → value-applying handlers.* The first row handler blind-toggled current state, which desynced from `LipiCheckbox`'s internal `CurrentValue` (it sets its own value in `HandleChange` then fires `ValueChanged`), so the first click appeared to do nothing. Fixed by **applying the value the checkbox reports** (`SetRowSelectedAsync(item, checkedState)` — add if checked, remove if not; idempotent), exactly as `LipiCheckboxGroup.OnItemToggleAsync` does. Header handler (`OnHeaderToggleAsync(bool? reported)`) likewise uses the reported tri-state value (true→select all visible, false→deselect). **Lesson: bind LipiCheckbox with a value-applying handler, never a blind toggle — the component owns its own CurrentValue.**
- *Checkbox clipped at left border.* The selection `LipiCheckbox` has no visible label (AriaLabel only), but its wrapper kept the default right-label layout — an empty label span + gap widened the wrapper past the narrow select track, pushing the indicator left into the table's `overflow:hidden` rounded border. Fixed with a global rule (`.lipi-table-cell-select .lipi-checkbox-wrapper { gap:0 }` + hide the empty `.lipi-checkbox-label-text`). Cache bumped 20260535 → **20260536**.

- *Select-all included disabled rows.* `OnHeaderToggleAsync` / `SelectAllOnPageAsync` selected every visible row including `RowDisabled` ones, and `HeaderState` counted them — so a locked row could be force-selected and the header could never reach "checked." Fixed: select-all and the header tri-state now operate on **selectable (non-disabled) rows only** (`Items.Where(r => !IsRowDisabled(r))`). §5.6.3.
- *Checkbox not centred in select column.* Root cause (found via DevTools box-model + element inspection): the `LipiCheckbox` renders an empty **helper-text slot** (`lipi-input-helper`, `aria-live`) as a second flex item beside the indicator in the cell, plus an empty label-text span — together they widened the cell content and pushed the visible indicator left of centre. Fixed with global rules hiding `.lipi-checkbox-label-text` and `.lipi-input-helper` within `.lipi-table-cell-select` (no label or helper text ever shows in a selection column; the checkbox's `AriaLabel` carries the accessible name). Cache bumped to **20260537**.

**Verification gate.** `dotnet build src\LiPi.Web\LiPi.Web.csproj` green; demo `/admin/style-guide/data-display`: Multi-select — row checkboxes toggle, selected rows tint, header shows unchecked/indeterminate-dash/checked as selection count crosses 0 / partial / all, live "N selected: …" updates, locked row's checkbox disabled. Single-select — picking a row moves the selection, "Chosen:" updates. **Discharges the Stage 3 re-test obligation** (placeholder → real interactive control).

---

### A47 — Phase 2.8 Stage 4b: LipiTable selection power interactions (mouse)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 4b (selection — mouse modifiers + OnRowClick)
**Date**: 2026-05-31
**Status**: ✅ Shipped — shift-click range, Ctrl/Cmd-click, OnRowClick interplay. Keyboard deferred to 4d; across-pages range to 4c.

**Scope.** Stage 4b adds the desktop power-selection mouse patterns (§5.4) on top of 4a's core. Keyboard selection (§5.5/§19.3) was sliced out as **Stage 4d** (bundled with the §19.3.1 grid-navigation model, which selection-keyboard depends on). Cross-page shift-click range (§5.4.3) defers to 4c (needs pagination).

**Architecture — cell owns the click (Option B).** `LipiCheckbox`'s `ValueChanged` only carries a `bool`, not the modifier keys. So selection clicks are captured on the **selection cell's `@onclick`** (which receives `MouseEventArgs` with `ShiftKey`/`CtrlKey`/`MetaKey`), and the body row checkbox is made **display-only** via `pointer-events:none` (scoped to `.lipi-table-cell.lipi-table-cell-select` so the header select-all checkbox keeps handling its own click). The checkbox still reflects state through `Value`. This avoids a fragile onclick-vs-onchange ordering dance and gives one clean modifier-aware path.

**Mouse interactions (§5.4).**
- *Shift-click range (§5.4.1):* on the selection cell or a row body cell, selects/deselects the range from the anchor to the clicked row in display order, inclusive; the anchor's current selected-state sets the direction (anchor selected → range selected; anchor unselected → range cleared). Disabled rows in the range are skipped. Multi-mode only; falls back to a plain toggle if no anchor. The anchor is **not** moved by a shift-click (§5.4.2).
- *Anchor tracking (§5.4.2):* `_anchorKey` updates on every plain/Ctrl-click (the clicked row becomes the anchor); resets on `ClearSelectionAsync`, and on `OnParametersSet` if the anchor row's key left the data.
- *Ctrl/Cmd-click (§5.4.4):* toggles only the clicked row (honors both `ctrlKey` and `metaKey` — `IsToggleModifier`; platform-specific tooltip deferred). On a row body, Ctrl/Cmd-click **selects** instead of firing `OnRowClick`.
- *`OnRowClick` (§23.2.1, pulled into 4b):* new `[Parameter] EventCallback<TItem> OnRowClick`. Plain click on a row **body** fires it (e.g. open-detail); Ctrl/Cmd-click selects without firing; Shift-click extends the range. Disabled rows still fire `OnRowClick` (§22.5.5) but never select. Clickable rows get a `lipi-table-row--clickable` pointer cursor.
- *Copy-button propagation fix:* the in-cell copy button now stops click propagation (`AddEventStopPropagationAttribute`) so copying doesn't also trigger `OnRowClick`.

**Files.**
- `LipiTable.razor` — selection cell + body cells get `@onclick` modifier handlers; copy button stop-propagation.
- `LipiTable.razor.cs` — `_anchorKey`, `OnSelectCellClickAsync`, `OnRowBodyClickAsync`, `ToggleRowAndSetAnchorAsync`, `ApplyShiftRangeAsync`, `IsToggleModifier`, `OnRowClick` param, `lipi-table-row--clickable` in `RowCssClass`, anchor resets. `using Microsoft.AspNetCore.Components.Web` added (MouseEventArgs).
- `LipiTable.razor.css` — `lipi-table-row--clickable .lipi-table-cell { cursor:pointer }`.
- `lipi-table-tokens.css` — body select cell `cursor:pointer` + checkbox `pointer-events:none` (scoped to body so header select-all still clicks). Cache bump 20260537 → **20260538**.
- `StyleGuideDataDisplay.razor` — selection note updated for 4b; new "Row click + modifier-select" demo (`OnRowClick` records last-clicked name, Ctrl/Shift modifiers live).
- **deploy-downloads.ps1 unchanged** — every touched file already has an entry.

**Not in scope.** Keyboard selection (4d, with §19.3.1 nav model). Cross-page shift-click range (4c). Two-step across-pages banner + bulk bar (4c). `OnRowDoubleClick` (§23.2.2 — later events stage). Platform-specific modifier tooltip (§5.4.5).

**Verification gate.** `dotnet build` green; demo `/admin/style-guide/data-display`: shift-click selects a contiguous range; Ctrl/Cmd-click toggles single rows; in the Row-click demo a plain click records the name while Ctrl/Shift-click drive selection; disabled rows never get range-selected; copy button doesn't trigger row-click.

---

### A48 — Phase 2.8 Stage 4d: LipiTable keyboard navigation + selection

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 4d (keyboard — §19.3.1 nav + §19.3.2 selection)
**Date**: 2026-05-31
**Status**: ✅ Shipped — roving-tabindex grid navigation + keyboard selection. Pagination/sort/filter/edit-dependent keys deferred to their stages.

**Scope.** Stage 4d adds the keyboard layer that 4b's slice deferred. Built as one shot per decision. In: §19.3.1 cell-by-cell navigation (arrows, Home/End, Ctrl+Home/End) + §19.3.2 selection keys (Space, Shift+Space, Ctrl/Cmd+Space, Ctrl/Cmd+A, Shift+Arrow, Escape). Deferred by dependency: PageUp/Down + Ctrl+PageUp/Down (pagination/St7), Ctrl/Cmd+Shift+A across-pages (4c), §19.3.3 header sort/filter keys (later stages), §19.3.4 inline-edit keys (later stage), Tab-out to other regions (toolbar/footer don't exist yet).

**Focus model — roving tabindex, FocusAsync (Option A, no JS for focus).** Exactly one body cell is tab-focusable (`tabindex=0`); all others `tabindex=-1`. Focused coords tracked in `_focusRow`/`_focusCol` (default to first cell so Tab can enter). Arrow keys update the coords and set `_pendingFocus`; `OnAfterRenderAsync` then calls `FocusAsync()` on the newly-focused cell. The focused cell's `ElementReference` is captured via an `@ref` **Action lambda** — `@ref="@(el => { if (IsFocusedCell(r,c)) _focusedCellRef = el; })"` — so every cell registers but only the focused one stores into the field (no last-wins, no markup duplication). `role="grid"` cell-by-cell model per §19.3.1.

**Keyboard scroll-guard — the one place pure Blazor can't reach (JS).** Blazor Server decides `preventDefault` client-side *before* the .NET handler runs, so per-key conditional preventDefault is impossible in C#. The combobox avoided this by focusing an `<input>` (arrow keys move the text cursor, not the page) — but grid cells are `<div>`s, where arrows scroll. So a single delegated capture-phase `keydown` listener (`window.lipiTable.initKeyboardGuard`, added to the existing `lipi-table.js`) calls `preventDefault()` for Arrow/Home/End/Space **only when focus is inside a LipiTable body cell**, and **never** for Tab/Escape (those must pass through so focus can leave the grid) or inside an input/textarea. Idempotent; installed once on first render via `OnAfterRenderAsync`. Focus movement itself stays pure Blazor — JS only suppresses the scroll.

**Keyboard map (in scope).**
- *Navigation:* Arrow Up/Down/Left/Right (cell-by-cell), Home/End (row ends), Ctrl/Cmd+Home/End (grid corners). Coords clamp to grid bounds.
- *Selection:* Space (toggle focused row + set anchor), Shift+Space (range from anchor), Ctrl/Cmd+Space (toggle without moving anchor — `ToggleRowKeepAnchorAsync`), Ctrl/Cmd+A (select all on page, Multi only), Shift+Arrow Up/Down (extend by one — `ExtendSelectionByOneAsync`, skips disabled), Escape (clear when count > 0). Reuses 4b's `ToggleRowAndSetAnchorAsync` / `ApplyShiftRangeAsync`.

**Focus ring.** `.lipi-table-cell:focus-visible` → 2px inset primary outline (`--lipi-table-focus-ring` token). `:focus-visible` (not `:focus`) so the ring shows on keyboard focus, not mouse-click; programmatic `FocusAsync` after an arrow key counts as keyboard. Inset offset keeps it inside the rounded table border.

**Files.**
- `LipiTable.razor` — body converted to indexed `@for` loops (need row/col indices); cells get `tabindex`, `@ref` focus-capture lambda, `@onkeydown`.
- `LipiTable.razor.cs` — focus state (`_focusRow/_focusCol/_pendingFocus/_focusedCellRef`), `CellTabIndex`, `IsFocusedCell`, `OnCellKeyDownAsync` (full key map), `MoveFocus`, `ToggleRowKeepAnchorAsync`, `ExtendSelectionByOneAsync`, `OnAfterRenderAsync` (guard init + FocusAsync), `TotalColumns`/`RowCount`.
- `LipiTable.razor.css` — `.lipi-table-cell:focus-visible` ring.
- `lipi-table-tokens.css` — `--lipi-table-focus-ring` token.
- `lipi-table.js` — `initKeyboardGuard` capture-phase scroll-suppressor.
- `StyleGuideDataDisplay.razor` — selection note + keyboard hint.
- `App.razor` — cache bump 20260538 → **20260539** (tokens + js changed).
- **deploy-downloads.ps1 unchanged** — every touched file already has an entry.

**Runtime-fix follow-ups (A48).**
- *Disabled rows were keyboard-selectable.* Mouse paths guarded `IsRowDisabled`, but keyboard Space called `ToggleRowAndSetAnchorAsync` directly with no guard. Fixed by guarding disabled at the top of `ToggleRowAndSetAnchorAsync` (defense-in-depth — protects every caller) plus an explicit early-break in the Space branch. Now Space/Shift+Space/Shift+Arrow all refuse disabled rows, matching mouse.
- *Shift+Space with no anchor did nothing.* Now degrades to a plain toggle + set-anchor (matches how shift-click degrades when no anchor exists, §5.4.2). With an anchor, it extends the range as before.

**Verification gate.** `dotnet build` green; demo `/admin/style-guide/data-display`: click a cell → arrow keys move a visible focus ring cell-by-cell without scrolling the page; Home/End/Ctrl+Home/End jump correctly; Space toggles the focused row; Shift+Space and Shift+Arrow extend the range (skipping the locked row); Ctrl/Cmd+A selects the 5 selectable rows; Esc clears; Tab still exits the grid to the rest of the page.

---

### A49 — Stage 7 prerequisite: LipiButton migrated to LiPi.Components (package-clean)

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 7 prep (component migration pulled forward)
**Date**: 2026-06-01
**Status**: ✅ Migrated + compile-verified. LipiButton now lives in the redistributable package so LipiPagination (and future components) can compose it.

**Why now.** Stage 7's LipiPagination composes LipiButton + LipiSelect per the all-LiPi distributable-package directive (no native `<button>`/`<select>` — the package presents a unified LiPi surface to consuming apps). LipiSelect was already migrated (Stage 1, A43/A44) and is package-clean. LipiButton was NOT — it still lived in `LiPi.Web.Components.Shared`. A `LiPi.Components` component cannot reference a `LiPi.Web` component (that would make the package depend on the app and break distribution), so LipiButton had to move first. This is the "clean leaves" migration tier (2.10 audit item 5), pulled forward because pagination needs it.

**The one real coupling — LucideIcon.** An on-disk audit found LipiButton's only blocker was `<LucideIcon>` (a `LiPi.Web` component on the never-migrate list). The `IWebHostEnvironment` / `ILogger` / `IJSRuntime` injections are fine in the package — the csproj has `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, and LipiSelectBase already uses `IWebHostEnvironment` in the package and compiles. (An earlier read incorrectly flagged Env as a blocker; the csproj corrected that.) Fix: swap `<LucideIcon Name Size>` → `<LipiIcon Name Size>` (LiPicons.Blazor, already a package dependency; identical Name/Size API).

**Icon-name verification (the de-risking step).** Lucide and LiPicons use different icon names, so every name LipiButton callers pass was checked against `icons.json` (LiPicons v1.0.4, 1164 icons) before the swap. LipiButton call sites use: `save, check, arrow-right, chevron-down, x, settings, trash, calendar, user`. All exist in LiPicons EXCEPT `x` → remapped to `close` (LiPicons' name; keywords include "x/cancel/dismiss/exit"). Only one call site used `Icon="x"` (the StyleGuide demo close button); the 9 Shared composers use only `calendar` (LipiModal), which exists. (Separately noted: 16 of 26 app-wide Lucide names are absent from LiPicons — a latent LucideIcon→LiPicons cutover for the wider app, tracked for 3.0, NOT addressed here.)

**Compile-verified here.** Built in a minimal `LiPi.Components` harness (FrameworkReference + LipiIcon stub) with the .NET 10 SDK. Two real bugs caught that `LiPi.Web`'s global usings had been masking: `MouseEventArgs` (needs `Microsoft.AspNetCore.Components.Web`) and `Env.IsDevelopment()` (needs `Microsoft.Extensions.Hosting` for the extension method). Both added as explicit `@using` in LipiButton.razor. Final build: 0 warnings, 0 errors. (The real package `_Imports.razor` already provides these project-wide; the inline usings are redundant-but-harmless and make the file self-documenting, matching LipiSelect's pattern.)

**Changes.**
- MOVED → `src\LiPi.Components\Shared\`: `LipiButton.razor`, `LipiButton.razor.css`, `LipiButtonSpinner.razor`, `LipiButtonTypes.cs`.
- `LipiButton.razor`: `@namespace LiPi.Components` + `@using LiPicons.Blazor` + `Components.Web` + `Extensions.Hosting`; both `<LucideIcon>` → `<LipiIcon>`.
- `LipiButtonSpinner.razor`: `@namespace LiPi.Components` (self-contained inline SVG — no icon dependency).
- `LipiButtonTypes.cs`: `namespace LiPi.Web.Components.Shared` → `LiPi.Components` (`ButtonVariant`, `ButtonSize`).
- `StyleGuide.razor`: `Icon="x"` → `Icon="close"` (the one demo close button).
- `deploy-downloads.ps1`: 4 LipiButton entries repointed from `LiPi.Web\Components\Shared\` → `LiPi.Components\Shared\`.
- NO `_Imports.razor` edit: `LiPi.Web`'s root `_Imports.razor` already has `@using LiPi.Components`, which cascades to `Components\Shared\` (the 9 composers) — so they resolve the moved LipiButton automatically.
- **Manual delete (Arun):** remove the old `src\LiPi.Web\Components\Shared\LipiButton.razor`, `.razor.css`, `LipiButtonSpinner.razor`, `LipiButtonTypes.cs` after deploy (the deploy script now writes them to the package location; the stale Web copies must go or the type exists in two namespaces).

**Verification gate.** Deploy → delete old Web copies → `dotnet run`. Expect clean build. Check: StyleGuide buttons render with icons (the close button shows the close glyph via `Icon="close"`); modals/drawers/dialogs still show their buttons; no "LipiButton not found" in any Shared composer.

---

### A50 — Phase 2.8 Stage 7: LipiPagination standalone component

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 7 (LipiPagination)
**Date**: 2026-06-01
**Status**: ✅ Shipped + runtime-verified (light + dark). Backfilled log entry — the code shipped referencing A50; this entry records it.

**What shipped.** The standalone `LipiPagination` component (`LiPi.Components.DataDisplay`), redistributable and package-clean. Three layout variants collapsed into one component per FLAGS LP-0: `Full` (page-size + row count + page-number buttons + first/last + jump-to-page), `Compact` (page-size + prev/next + "Page X of M"), `Minimal` (prev/next chevrons only). Stateless — render is a pure function of params; only local state is the jump-to-page typed value.

**Composition (LP-12-REVERSED).** The pager composes `LipiSelect<PageSizeOption>` for page-size and `LipiButton` for nav chevrons rather than native `<select>`/`<button>`. Rationale recorded at ship time: keeps the all-LiPi redistributable surface, inherits LipiSelect's keyboard/a11y/theming, and `PageSizeOption.ToString()` lets the dropdown render "All" for the `int.MaxValue` sentinel (a native select can't do that label trick). This deviation from the spec's native-control guidance was deliberate and logged.

**Pure helper.** Page-number windowing (`ComputePageSlots`) + total-page math live in `PaginationMath.cs` under `LiPi.Components.Internal.Algorithms` (LP-14 pattern). Sibling sub-components: `LipiPaginationPageSize`, `LipiPaginationCountDisplay` ("Showing X-Y of Z"), `LipiPaginationLoadMore` (a distinct append-on-demand paradigm, NOT a variant).

**Defaults.** Page-size ladder starts at 5 (5/10/15/25/50/100/200/All) per project-owner deviation from spec §3.3 (clinical lists are often short — finer low-end control). `Siblings`/`BoundaryCount` default 1 (LP-14).

---

### A50.1 — LipiPagination visual-style axes (three independent enums) + demo polish

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 7 (LipiPagination visual refinement)
**Date**: 2026-06-01
**Status**: ✅ Shipped + runtime-verified (light + dark). Backfilled log entry.

**The decision.** Pagination became properly themeable via three INDEPENDENT style axes (any combination composes), all token-overridable:
- `PaginationCellStyle` { Bordered, Borderless } — page-number cell affordance (axis 1).
- `PaginationActiveStyle` { Solid, Tint } — how the active page is signalled (axis 2).
- `PaginationChevronStyle` { Auto, Bordered, Ghost } — nav chevron border treatment (axis 3); `Auto` follows CellStyle.

Zero-config default = **Bordered + Solid + Auto** (general-purpose library default). The LiPi HIS in-app look = **Borderless + Tint** (opt-in). Tokens remain the deeper override; deeper tint uses `color-mix(in srgb, primary-pale 60%, primary 40%)` (mode-aware).

**Structural dependency (FLAG LP-A50.1).** The chevron-style axis is BUILT ON the LipiButton composition from A50 — `ChevronsBordered → ButtonVariant.Secondary vs Ghost` is the implementation mechanism. Removing LipiButton from the chevrons would discard the axis. This is structural, not incidental.

**Demo.** StyleGuidePagination gained a "Visual styles" section; the demo's dark-mode washout was fixed by painting `.sg-page { background: var(--color-bg-base); }` (it was the only StyleGuide page with no painted surface, exposing TopNavLayout's non-theming `.tn-content`). A global `.tn-content` fix is deferred to Phase 2.10.

**CSS lesson (standing rule).** A `*/`-sequence inside a CSS comment (e.g. glob text `lipi-*/` or `--color-*/--r-*`) terminates the comment early and silently discards subsequent rules. Found + fixed in StyleGuidePagination.razor.css and lipi-pagination-tokens.css. Every CSS file is now comment-balance + brace-balance linted before delivery.

---

### A51 — Phase 2.8 Stage 7: LipiTable ↔ LipiPagination wiring (client-mode) + status-strip package-ization

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 7 (LipiTable embeds LipiPagination)
**Date**: 2026-06-02
**Status**: ✅ Client-mode paging shipped + runtime-verified on a 480-row test page (page nav, page-size, jump-to-page, selection across pages, header tri-state per page, status strips, style axes, light + dark). Server-mode (DataSource), Left/Right side-pager rendering, and keyboard nav remain deferred (see below).

**Roadmap-state correction (important).** An on-disk audit found LipiTable is the **Stage 2/3 chassis only**: it renders `Items` directly with selection (4a/4b/4d) + column/cell/density/preference plumbing. Sorting, filtering, search, tree, grouping, master-detail, and inline-edit are **declared as types/enums/contexts but NOT implemented** (verified: `OrderBy`=0, `Skip`/`Take`=0 files). The Stage-7 handoff's "Stages 4–6 DONE" was inaccurate — only Stage 4 *selection* shipped. Pagination therefore wires directly over `Items`; when sort/filter are built later they compose BEFORE the slice (`Items → filter → sort → page`), changing only what feeds `PagedItems`.

**Step 1 — types + params foundation.**
- `LipiPaginationTypes.cs`: +`PaginationOrientation` { Horizontal, Vertical }; +`PaginationRange` (5 members: Start, End, Total, CurrentPage, PageSize — LP-21, all included now to avoid a breaking add later); +`PageSizeOptions` static helper (`.All`, `.Of`, `.FromInts`, `.Default` — LP-20 consumer ergonomics without weakening the type).
- `LipiTableTypes.cs`: `PaginationPlacement` expanded 4 → 6 (Bottom default, Top, Both, **Left**, **Right**, None); +`PaginationOptions` record (all-null defaults; `PageSizeOptions` is `IReadOnlyList<PageSizeOption>?` per LP-20, NOT `int`); +`PaginationOptionsPresets.LipiHisDefault`.
- `LipiTable.razor.cs`: +4 params — `Placement`, `Variant`, `PaginationOptions`, `PaginationSideWidth`.
- `LipiPagination.razor.cs`: +`Orientation` param only. **No reason callback** (LP-23 Option B).

**Step 2+3 — client-mode slicing + bottom/top pager (merged).** `_currentPage`/`_pageSize` state; `PagedItems` slices `Items` by the current page (`int.MaxValue` size = "All" = whole set); default page size **10** (table convention, independent of the pager's 5-start ladder; consumer overrides via `PaginationOptions.DefaultPageSize`). Render loop iterates `PagedItems`; `aria-rowcount` reports the **total**. Pager renders in Bottom/Top/Both via a reusable `RenderPager()` (RenderTreeBuilder) wiring all resolved params + two-way binds. `OnParametersSet` clamps the current page into range. Page-size change resets to page 1.

**Step 4 — selection × pagination.**
- **LP-16**: `HeaderState` (tri-state) and `OnHeaderToggleAsync` now operate on `PagedItems` (current page), not all `Items` — fixes the latent bug where the header checkbox would select all rows in the dataset.
- **LP-17**: +`OnAllOnPageSelected` EventCallback firing `AllOnPageSelectedContext(AllOnPageSelected, PageSelectableCount, TotalCount)` after a header select/deselect — the SEAM for the Stage 4c across-pages banner ("Select all N?"). Stage 7 fires it; the banner UI itself is Stage 4c (NOT built here).
- **Cross-page persistence**: verified — pagination handlers touch only `_currentPage`, never `_selectedKeys`, so keyed selections survive page navigation by construction.

**`PageChangeReason` reconciliation (LP-22-RETIRED / LP-23 / LP-24).** A build error (CS0101) revealed `PageChangeReason` ALREADY EXISTS in `Contexts.cs` as part of the table's 20+ Context/Reason event-taxonomy family. LP-22's proposed standalone 6-value enum was **retired** (it partially duplicated the existing 8-value enum + the separate `PageSizeChangeReason`). The shipped enum is authoritative; +`PageSnap` added as a 9th value (LP-24, non-breaking) because LP-19 focus logic must distinguish a bounds-snap (focus lands on first row of snapped page, treat as navigation) from `DataChange` (preserve focused-row identity). LP-23: `PageChangeReason`/`PageChangedContext` belong to LipiTable's `Contexts.cs`; the redistributable `LipiPagination` exposes ZERO table vocabulary and fires only its existing `CurrentPageChanged(int)` — the composing table infers reason (it wired the handlers). `AllOnPageSelectedContext` likewise lives in `Contexts.cs` (table taxonomy).

**Status strip → package feature + header alignment fix (Path 1 + option b).** The row status strip is now OWNED by LipiTable (package feature) instead of split between the component and app-side `lipi-status-tokens.css`. The `.lipi-status-strip-{left,right,top}` rendering + `[data-status]` colors + `--lipi-status-strip-width` moved INTO `LipiTable.razor.css`, bound DIRECTLY to the foundation semantic palette (`--color-success/warning/danger/info/text-tertiary`) with hard hex fallbacks (A22/A26) — zero dependency on the app-side status file. The header row now carries a matching transparent strip of the same width (`HeaderStripClass`), so header content aligns with body content (fixes the header-shift when `RowStatus` is set). `lipi-status-tokens.css` retains only the shared `--color-status-*` token taxonomy + `.lipi-table-status` chip.

**Pager row-shift fix.** The "Showing X-Y of Z" readout (`.lipi-pagination-count`) had no reserved width, so its text gaining digits across pages (81-90 → 91-100 → 101-110) widened it and shifted the nav strip right/left. Fixed with `min-width: var(--lipi-pagination-count-min-w, 24ch)` + `text-align: left` + `flex-shrink: 0` — stable width for the common range, token-overridable, degrades gracefully for pathological totals.

**Two Razor traps hit + fixed (now on the manual pre-delivery checklist).** (1) `@{ }` cannot nest inside a `@switch`/`@for` case body (already code context) → RZ1010; moved the `pageRows` local to the top legal `@{ }` block. (2) `@member` appended to literal attribute text (`class="lipi-table-header-row@HeaderStripClass"`) does NOT evaluate — it renders literally, breaking the header's grid (collapsed to `display:block`, cells stacked); fixed by wrapping the whole value as `class="@($"lipi-table-header-row{HeaderStripClass}")"`. Brace/comment lint doesn't catch these — both are now manual `.razor` checks.

**Test page.** `StyleGuideTablePaging.razor` (+`.razor.css`) at `/admin/style-guide/data-display/table-paging` — a deterministic 480-row dataset with 5 sections (Default/Full/Bottom + multi-select + jump, HIS Borderless+Tint, Top placement size 25, Both placement, Compact variant) and a live cross-page selection readout. Linked from the StyleGuide hub Phase 2.8 group (along with the A50.1 Pagination link, which the on-disk hub was missing).

**Flags recorded this stage.** LP-12.1 (scope carve-out: LP-12/LP-13 native-control guidance applies to NEW table-internal chrome from Stage 7 onward, NOT the shipped A50 pager); LP-20 (PaginationOptions.PageSizeOptions = `IReadOnlyList<PageSizeOption>?`); LP-21 (PaginationRange 5 members); LP-22-RETIRED; LP-23; LP-24 (PageSnap); LP-A50.1 (chevron axis depends on LipiButton); PROCESS-1 (strategic chat verifies shipped surface before authoring amendments that name existing types — three documented re-derivation errors: LP-12/13, PageSizeOptions, LP-22).

**Changes.**
- `LipiPaginationTypes.cs`: +PaginationOrientation, +PaginationRange, +PageSizeOptions helper (PageChangeReason NOT here — it's in Contexts.cs, LP-23).
- `LipiPagination.razor.cs`: +Orientation param (no reason callback).
- `LipiTableTypes.cs`: PaginationPlacement 6 values; +PaginationOptions record; +PaginationOptionsPresets.
- `LipiTable.razor.cs`: +Placement/Variant/PaginationOptions/PaginationSideWidth params; pagination state + PagedItems slice + handlers + ClampCurrentPage + resolution helpers; LP-16 page-aware HeaderState/OnHeaderToggleAsync; +OnAllOnPageSelected (LP-17); +HeaderStripClass; Left/Right Dev-warning (vertical rendering not yet implemented).
- `LipiTable.razor`: iterate PagedItems; outer pagination wrap + top/bottom pager slots + RenderPager(); header class as interpolated expression; @HeaderStripClass on header row.
- `LipiTable.razor.css`: pager wrapper layout; status-strip rendering moved in (foundation-token-bound, +header gutter).
- `Contexts.cs`: +PageSnap (9th PageChangeReason value, LP-24); +AllOnPageSelectedContext record (LP-17).
- `lipi-status-tokens.css`: strip rendering + width token REMOVED (moved to LipiTable); retains --color-status-* tokens + .lipi-table-status chip.
- `LipiPaginationCountDisplay.razor.css`: +min-width/text-align/flex-shrink (row-shift fix).
- `StyleGuideTablePaging.razor` (+.razor.css): NEW 480-row test page.
- `StyleGuide.razor`: +Pagination and +Table Paging links (Phase 2.8 group).
- `deploy-downloads.ps1`: +2 test-page entries (396 keys total, no duplicates).

**Verification gate.** Deploy the Stage-7 set → `dotnet run` → open the Table Paging page. Confirm: row stays still across 9→10→11→back; header tri-state reflects current page only; selection persists across pages (readout count holds); status-strip header aligns with body; light + dark both correct. Server-mode, Left/Right vertical pager, and keyboard nav (Alt+PageUp/Down, Ctrl+Home/End) are DEFERRED — not in this entry.

---

### A52 — Phase 2.8 Stage 7: side-pager, keyboard nav, empty-state height, pager alignment

**Phase**: 2 Sub-step 2.8 — Data Display, Stage 7 (continuation of A51)
**Date**: 2026-06-02
**Status**: ✅ All client-side; runtime-verified on the 480-row test page (light + dark). Server-mode (DataSource) remains the only deferred Stage-7 item.

**Scope.** Everything built after the A51 client-mode wiring: the Left/Right vertical side-pager, pagination keyboard nav, the empty-table behavior (disabled pager + EmptyRows reserved height + compact mode), and the PaginationAlign zone-alignment option. Several latent bugs found and fixed along the way (RowCount, the alignment flexbox mechanics). All on `LiPi.Components` (redistributable) + the demo/test page in `LiPi.Web`.

**Left/Right side-pager — vertical rendering (Option A).** Placement=Left/Right now renders (previously resolution-only with a Dev-warning). The vertical pager is a narrow column beside the grid: chevron-up (prev) / stacked "current / total" / chevron-down (next), in bordered boxes, vertically centered, with a filled surface (`--color-bg-subtle`) and a divider against the grid. Full auto-downgrades to Compact for side placement (locked rule). Design decisions:
- Mockup options A/B/C were compared; Option A (rotated Compact strip) chosen — B (vertical number rail) is an anti-pattern, C (rotated horizontal) makes text sideways.
- Icons are oriented in code-behind (`PrevIcon`/`NextIcon` → chevron-up/down when vertical). **Icon-verification (A49 discipline):** `chevron-up`/`chevron-down` exist in LiPicons v1.0.4; `chevrons-up`/`chevrons-down` do NOT — so `FirstIcon`/`LastIcon` stay horizontal (`chevrons-left/right`) and vertical-Full is intentionally unsupported (side placement is Compact anyway). The vertical indicator shows stacked `current / total` (not "Page X of M") to match the mockup; full text preserved as the indicator's `aria-label`.
- New: `PaginationOrientation` consumed (root `--vertical` class), `CompactChevronVariant` (bordered when vertical), the side-layout CSS (`lipi-table-pagination-wrap--side` → row layout, grid flex-grows with `min-width:0`, side column fixed width from `PaginationSideWidth`).

**Keyboard navigation (Step 7, whole-table scope).** Pagination shortcuts added to the A48 cell-keydown handler (`OnCellKeyDownAsync`): **Alt+PageDown** (next page), **Alt+PageUp** (prev page), **Ctrl+Alt+Home** (first page), **Ctrl+Alt+End** (last page). After a page change, focus lands on the first cell of the new page (LP-19 `UserKeyboard`). Chosen chord set (option a) deliberately AVOIDS Ctrl+Home/End, which A48 already uses for grid-corner focus — no regression. The capture-phase JS scroll-guard (`lipi-table.js`) gained Alt-gated PageUp/PageDown suppression (plain PageUp/Down still scrolls a focused table). `NavigateToPageByKeyboard` + `PaginationTotalPages` helpers added.

**Latent bug fixed — RowCount.** A48's `RowCount` was `Items.Count` (all rows) but only `PagedItems.Count` render per page; arrow-key focus could target rows not on the current page. Fixed to `PagedItems.Count` so keyboard focus clamps to the current page.

**Empty table — disabled pager (UX decision).** When the table has zero rows, the pager still RENDERS but is `Disabled` (greyed chevrons, inert page-size) and the count reads "No items". Rationale (per project owner): a vanished pager is ambiguous (no data? load failure? still loading?); a persistent disabled pager is an affirmative "loaded, zero results" signal and prevents layout reflow when data arrives. Wired as `Disabled = IsLoading || PaginationTotalCount == 0`.

**EmptyRows — row-count-based reserved height (+ compact mode).** New `EmptyRows` (int?) reserves height for the non-data states (empty/loading/error) expressed as a ROW COUNT, not a raw length — the table multiplies by its own density row-height (`--lipi-table-row-min-h`), so it's density-correct and can't be an odd CSS value. Two modes (root class):
- **EmptyRows 1–2 → compact:** EXACT N-row height, clipped, with a small inline text (no big illustration) — for a genuinely short empty strip. Fixes the initial bug where 1 and 6 looked identical (the Card-size `LipiEmptyState` illustration is ~200px and dominated any min-height floor).
- **EmptyRows 3+ → reserve:** FLOOR of N rows (min-height); the full illustration shows and may grow taller (never clipped).
- **unset → content-driven** (unchanged). Threshold is a const (`CompactEmptyMaxRows = 2`). `min-height` floor design rejected in favor of this because a raw px value is arbitrary, isn't density-aware, and invites odd values.

**PaginationAlign — pager zone alignment.** New `PaginationAlign` { SpaceBetween (default), Start, Center, End } on `PaginationOptions`. SpaceBetween (count left, nav right) is the data-grid standard and the new out-of-box look (previously left-packed/drifty). On `LipiPagination`: `Align` param + `AlignClass`; on LipiTable: `ResolvedAlign` wired into the embedded pager. Reachability concern (wide tables pushing nav off-screen) checked and dismissed — the table is `overflow:hidden` with `minmax()` grid tracks, so it never h-scrolls; the pager always spans the visible container.

**The alignment flexbox fix (record this so it isn't reintroduced).** Getting `justify-content` to actually move the cluster required THREE things, found via a diagnostic build (temporary colored outlines proved the CSS was live, isolating it as a layout problem):
1. `flex-wrap: nowrap` on `.lipi-pagination` — with `wrap`, a wide row-count pushed the nav onto its own full-width line, defeating alignment.
2. `.lipi-pagination-main { flex: 0 0 auto }` — the left group (size+count, wrapped into one `.lipi-pagination-main` div) must not grow, or it eats the free space.
3. **`.lipi-pagination--horizontal { width: 100% }`** — THE key fix: a shrink-wrapped flex container has zero free space, so `justify-content` does nothing. The horizontal pager must fill its parent. (Vertical pager stays content-width.)
Markup restructured: page-size + count wrapped in `.lipi-pagination-main`, so alignment positions the two GROUPS (left cluster + nav), making the count's width irrelevant to alignment while preserving the A51 row-shift fix.

**Changes.**
- `LipiPagination.razor.cs`: +Orientation consumption (`IsVertical`/`OrientationClass`/oriented icons), +`Align` param + `AlignClass`, +`CompactChevronVariant`. (Also: the `Orientation` PARAM had to be (re)added — an earlier deploy gap left it referenced-but-undeclared; CS0103/CS0117 fixed.)
- `LipiPagination.razor`: root nav gets `@OrientationClass @AlignClass`; oriented chevron icons; vertical stacked indicator; size+count wrapped in `.lipi-pagination-main`.
- `LipiPagination.razor.css`: vertical-orientation rules; stacked-indicator styling; `.lipi-pagination--horizontal { width:100% }`; `nowrap`; `.lipi-pagination-main` group + the four align modes.
- `LipiPaginationTypes.cs`: +`PaginationAlign` enum.
- `LipiTableTypes.cs`: `PaginationOptions` +`Align`.
- `LipiTable.razor.cs`: +`EmptyRows` param + compact/reserve helpers (`IsCompactEmpty`/`EmptyRowsModeClass`/`CompactStateText`/`EmptyRowsStyleVar`); +`ResolvedAlign`; +`NavigateToPageByKeyboard`/`PaginationTotalPages`; RowCount→PagedItems fix; keyboard chords in `OnCellKeyDownAsync`; side-placement resolution (removed the old Dev-warning).
- `LipiTable.razor`: side-pager slots (Left/Right) + wrap modifier; pager `Disabled` when empty; EmptyRows mode class + style var on root; compact-state branches in empty/loading/error; `Align` passed to RenderPager.
- `LipiTable.razor.css`: side-placement row layout; status-strip header gutter (A51, retained); EmptyRows compact/reserve `.lipi-table-statewrap` heights + `.lipi-table-state-compact` text.
- `lipi-table.js`: Alt-gated PageUp/PageDown scroll-guard.
- `StyleGuideTablePaging.razor`: +sections — alignment (Start/Center/End), side-pager (Left/Right), empty tables (EmptyRows 1/2/3/6).

**Process notes.**
- **Staleness (recurring):** several uploaded files this stage were stale (pre-Step-1 `LipiPagination.razor.cs` missing `Orientation`; A48-era `LipiTable.razor` missing pager wiring; a scratch CSS missing side-pager rules). Each was caught by grepping the file for the members an edit depended on, and rebasing onto the current `/outputs` copies. **The `/outputs` folder is the reliable source of truth for these files.**
- **Layout-debug lesson:** for CSS layout bugs, request computed `width`/`flex`/`justify-content` first; the shrink-wrap container cause would have been obvious from one computed-width reading rather than several deploy rounds. The diagnostic-outline trick (prove the rule is live before theorizing) is the right first step.

**Verification gate.** Deploy the Stage-7 set → `dotnet run` → Table Paging page. Confirm: side-pagers render (Left/Right, stacked indicator); Alt+PageUp/Down + Ctrl+Alt+Home/End navigate pages with focus landing on the new page; empty tables show disabled "No items" pager; EmptyRows 1/2 are thin compact strips, 3/6 show the full illustration; alignment Start/Center/End/Between are visually distinct. Server-mode (DataSource) is NOT in this entry.

---

---

### A53 — Phase 2.8 Stage 7→shared query layer: sort (S1), quick search (S2), column filters S3a

**Phase**: 2 Sub-step 2.8 — Data Display. Shared query layer for LipiTable (reused by LipiList later).
**Date**: 2026-06-03
**Status**: ✅ All client-side; runtime-verified on the 480-row test page (light + dark). S3b/S3c pending.

**Roadmap reorder (locked).** The shared query layer (sort + quick search + column filters) is built on LipiTable BEFORE LipiList, because LipiList reuses the same query machinery (descriptors, pipeline, operator engine). Build-once-reuse-twice. Pipeline order is fixed: `Items → quick-search → filters → sort → page`.

**S1 — Sort (single + multi-column).** Header-click cycles a column unsorted→asc→desc (`SortCycleMode.ThreeState` default; `TwoState` available). Shift-click adds the column to a multi-column chain with visible priority numbers. Indicator: faint stacked up/down chevrons at rest (opacity 0.3, discoverable), solid chevron-up/down when active. Auto-disabled for Actions/Avatar/template-only columns; per-column `Sortable="false"` opts out. `NullSortOrder` is direction-aware. Public surface: `OnSortChanged`/`SortChangedContext`, `ClearSortAsync()`, `CurrentSort`. ColumnDefinition gained `Sortable`; the old `SortComparer` was erased to `Comparison<object?>?` so the table sorts boxed values without knowing TValue. Header-vs-data alignment fix: the sort button's `justify-content` rules must target the REAL emitted align classes `.lipi-table-cell--right`/`--center` (not `header-cell--*`), or right/center headers drift from their data.

**S2 — Quick search.** `ShowQuickSearch` (default false) renders a minimal toolbar with a right-aligned box (placeholder "Quick search…", search icon, clear ✕). **Semicolon `;`-separated terms** (comma rejected — commas appear constantly in real cell data e.g. "Gupta, Aarav" / "₹ 65,000.00"); spaces are literal WITHIN a term; case-insensitive Ordinal `Contains` across all non-opted-out columns; a row matches when EVERY term matches SOME column (AND across terms, OR across columns). Per-column `Searchable="false"` opt-out; template-only columns auto-excluded. 300ms C# debounce (CancellationTokenSource, each keystroke cancels the prior pending apply). `HighlightMatches` (default false) wraps matched substrings in `<mark>` — **built-in TEXT cells only** (the table cannot reach into caller `<CellTemplate>` markup; documented limitation). Searching matches the RAW value (`40000`), not the formatted display (`₹ 40,000.00`). `QuickSearchChangedContext`; resets to page 1. ColumnDefinition gained `Searchable`. Toolbar surface joined to the table as one card (shared bg/border, top-rounded, flush via negative margin; table top corners squared via `.lipi-table-has-toolbar`); search-box border bumped to `--color-border-strong` for contrast on the card.

**S3a — Column filters (HeaderIcon + text/universal operators).** `FilterMode.HeaderIcon` (default) shows a funnel per filterable header — faint outline at rest, brighter on header-hover, and the **Fill icon variant + primary color when active**. Click opens a popover: operator dropdown + value input (hidden for Empty/NotEmpty) + Clear/Apply.
- **`FilterApplyMode { Apply (default), Live }`** — Apply shows an Apply button (explicit commit); Live filters as operator/value change (300ms debounced), no Apply button.
- **`FilterChipsPlacement { Separate (default), Inline }`** — Separate = chips in their own strip below the toolbar, joined to the card and wrapping; Inline = chips ride inside the toolbar beside the search box and scroll horizontally in one line (so the toolbar never grows); Inline falls back to a separate strip when there's no toolbar (`ShowQuickSearch=false`).
- **`FilterCaseSensitive`** (default false = OrdinalIgnoreCase; true = Ordinal).
- **Operators (S3a):** text + universal — Contains, NotContains, Equals, NotEquals, StartsWith, EndsWith, Empty, NotEmpty. The `MatchesFilter(value, descriptor)` switch is the extension point; S3b adds numeric/date/boolean/In/relative cases. `OperatorsFor(def)` (S3a returns the text+universal set for all columns; S3b refines by ColumnType) and `OperatorLabel(op)` already carry labels for the FULL operator set.
- **Active-filter chips:** `Column: operator "value"` (value omitted for Empty/NotEmpty), per-chip ✕, Clear all.
- Public surface: `OnFilterChanged`/`FilterChangedContext`, `ClearAllFiltersAsync()`, `CurrentFilters`. `Filterable` auto-disabled for Actions/Avatar/template-only; `Filterable="false"` opts out. ColumnDefinition gained `Filterable`. Pipeline complete: `Items → quick-search → filters → sort → page`; filter changes reset to page 1; `PaginationTotalCount` reads the filtered set.
- **Popover positioning (the fiddly part).** Popover is `position:fixed` so it escapes the table's `overflow:hidden` and never clips (even a 1-row filtered table). Anchored to the funnel button's viewport rect via new JS `lipiTable.getRect(el)`. **Measurement is done in `OnAfterRenderAsync`, NOT the click handler** (A48 rule: an ElementReference is only reliably resolvable after the DOM commits — measuring in the click handler returned a null rect → popover flew to the 0,0 corner). A `_pendingPopoverAnchor` flag defers it; the popover stays `visibility:hidden` until measured (no corner-flash); flips ABOVE the funnel when within ~280px of the viewport bottom.
- **Dismissal (three ways):** backdrop click, **page scroll**, and **Esc** — all via one JS listener pair (`lipiTable.onScrollClose`/`offScrollClose`, capture-phase scroll + keydown) calling `[JSInvokable] ClosePopoverFromJs()`. The table now implements `IDisposable` (holds a `DotNetObjectReference<LipiTable<TItem>>`, deregisters listeners + disposes the ref on teardown).

**Spec-vs-reality fixes (record so they aren't reintroduced).**
- Created `FilterApplyMode` + `FilterChipsPlacement` enums in `LipiTableTypes.cs` — they did NOT previously exist.
- Real `FilterChangeReason` values (from `Contexts.cs`): `UserApplyPopover`, `UserApplyDrawer`, `UserRemoveChip`, `UserClearAll`, `Programmatic`, `PersistenceRestore`, `ResetToDefault`. Used UserApplyPopover/UserRemoveChip/UserClearAll (NOT the invented UserApply/UserClear).
- `FilterDescriptor(string ColumnKey, FilterOperator Operator, object? Value, object? ValueEnd)` — `ValueEnd` reserved for Between (S3b).
- **Two icon APIs (must keep straight):** `LipiButton.Icon` takes a kebab STRING (`"chevron-left"`); `LipiIcon.Name` takes the `LipiconName` ENUM. In Razor markup the enum needs the `@` prefix: `Name="@LipiconName.Search"` — without it the literal string `"LipiconName.Search"` is passed → runtime "unknown icon name" + nothing renders.
- **Icon-exists-in-json ≠ enum-member-exists.** `x` is in icons.json but `LipiconName.X` does NOT exist (single-letter names dropped by the enum generator) → use `LipiconName.Close`. Verified-valid members used this arc (all compiled): Check, CheckCircle, Copy, ChevronUp, ChevronDown, Close, Search, Filter. Active funnel uses `LipiconVariant.Fill`.
- CS8602 in the sort fallback: `a`/`b` are provably non-null after the null-check block, but flow analysis doesn't track it through `object?` → null-forgiving `a!.ToString() ?? string.Empty`.

**Changes.**
- `ColumnDefinition.cs`: +`Sortable`, +`Searchable`, +`Filterable`; `SortComparer` erased to `Comparison<object?>?`.
- `LipiColumn.razor.cs`: +`Searchable`/`Filterable` params + `ResolveSortable`/`ResolveFilterable` (auto-disable Actions/Avatar/template-only) + erased-comparer builder.
- `LipiTableTypes.cs`: +`FilterApplyMode`, +`FilterChipsPlacement` enums.
- `LipiTable.razor.cs`: sort engine (`SortedItems`, `CompareRows`, `OnHeaderSortAsync`, `ClearSortAsync`); quick-search engine (`SearchedItems`, semicolon term parse, debounce, `HighlightSegments`); filter engine (`FilteredItems`, `MatchesFilter`, `OperatorsFor`, `OperatorLabel`, draft state, Apply/Live commit, chip helpers, `ClearAllFiltersAsync`); popover anchor (`getRect` via OnAfterRender, flip, hide-until-measured); `IDisposable` + `DotNetObjectReference` + `[JSInvokable] ClosePopoverFromJs`; `FilterCaseSensitive`; pipeline repointed `Items→search→filters→sort→page`; `PaginationTotalCount` reads filtered set.
- `LipiTable.razor`: sort button + indicators in headers; quick-search toolbar (joined card); funnel per filterable header (`@ref` dict, Fill-when-active) + inline popover (operator select, value input, Clear/Apply) + backdrop; chips (inline-in-toolbar + separate strip) with per-chip ✕ + Clear all; `RenderMaybeHighlighted` cell emitter; `has-toolbar` covers toolbar OR separate chips strip.
- `LipiTable.razor.css`: sort button/indicator + right/center justify on real cell-align classes; quick-search toolbar + joined-card surface + match `<mark>`; funnel button (idle/hover/active) + fixed-position popover + chips (separate joined-strip + inline horizontal-scroll); header cell `position:relative` anchor.
- `lipi-table.js`: +`getRect(el)` (viewport rect for fixed anchoring); +`onScrollClose`/`offScrollClose` (capture-phase scroll + Esc keydown → `ClosePopoverFromJs`).
- `StyleGuideTablePaging.razor`: first demo table opted into `ShowQuickSearch` + `HighlightMatches` (HeaderIcon filters appear by default since `FilterMode` defaults to HeaderIcon).

**Queued (design items, NOT locked features).**
- **Better filtering UX exploration** — after the baseline (S3a + S3b + S3c Drawer) ships, revisit whether the funnel→popover→operator-dropdown pattern can be made easier for clinical users. Project owner finds the conventional pattern rudimentary; wants an easier method, including multi-value like "Radiation OR Surgical Oncology". To be a DESIGN DISCUSSION informed by the working baseline, then decided — not pre-committed. Whatever emerges rides on the same FilterDescriptor engine + pipeline (a new presentation, like Drawer), not a rebuild.
- **`In` operator (multi-value)** — partially addresses the OR-across-values need; lands in S3b (Boolean+In sub-slice) with a distinct-value multi-select editor.

**Verification gate.** Deploy the set → `dotnet run` → Table Paging page (first table). Sort: click headers (tri-state), shift-click (multi-column priority). Quick search: `oncology`, then `gupta; oncology` (cross-column AND), clear. Filters: funnel → contains "onc" → Apply → rows filter + chip + Fill funnel; popover anchors to funnel, flips above near bottom (1-row table), closes on backdrop/scroll/Esc; try `FilterApplyMode=Live` and `FilterChipsPlacement=Inline`. S3b (numeric/date/boolean/In/relative editors) and S3c (Drawer) are NOT in this entry.

---

### A54 — DateTime picker family migrated to LiPi.Components.Forms + capability redesign

**Date:** 2026-06-03
**Scope:** Independent PR. Migrate the Date/Time picker family from `LiPi.Web.Components.Shared`
into the redistributable `LiPi.Components.Forms`; absorb `IDateFormatService` +
`IClinicTimezoneService` mechanics as parameter-driven static helpers; add a time-source model,
an AM/PM segmented toggle, and a Direction×Span date-range preset system.

### Isolation / namespace
- All four pickers + `LipiDateTimeTypes.cs` moved to `namespace LiPi.Components.Forms`
  (physically `src\LiPi.Components\Forms\`). Base classes `LipiInputBase<T>`/`LipiContainerBase`
  were already package-side — PR did not widen to the input-base layer.
- Pickers no longer inject HIS services. The two `@inject`s and `@using LiPi.Web.Services`
  removed. Each picker carries a direct `@using LiPicons.Blazor`.
- **LucideIcon → LipiIcon** in all migrated pickers (LucideIcon is in `LiPi.Web`, unusable from
  the package). Verified enum members: x→Close, calendar→Calendar, clock→Clock,
  chevron-left/right→ChevronLeft/ChevronRight.

### New package code
- `LipiTimeResolver.cs` — `LipiTimeSource { Server, Utc, Client, SpecificZone }` (default
  Server, SaaS-safe); `LipiTimeZones.IndiaIST` (Asia/Kolkata + ICU-less fixed-offset fallback,
  ported from ClinicTimezoneService); static `LipiTimeResolver` (ResolveNow/ResolveToday/
  ToZone/Compose — clinic-free ports of GetClinicLocalNow/ToClinicLocal/ToUtc).
- `LipiDateFormat.cs` — DateFormatService engine ported verbatim (token translation, ISO +
  forgiving parse, 12h/24h cross-format parse, segment-order regex); takes explicit format.
- `lipi-input.js` — appended `window.lipiInput.getClientNow()` (browser local parts + tz
  offset) for the `Client` time source.

### LipiInputDefaults (extended)
+5 properties (India defaults): `DefaultDateFormat="DD/MM/YYYY"`, `DefaultTimeFormat="24h"`,
`DefaultWeekStart=Sunday`, `DefaultTimeSource=Server`, `DefaultTimeZone=null`. App configures
in Program.cs; per-component params override; a HIS feeds these from clinic config.

### Per-component params added
- DatePicker/RangePicker: `TimeSource`, `TimeZone`, `WeekStart` + `ResolvedToday` (today honors
  the source). RangePicker injects `IOptions<LipiInputDefaults>` directly (not a LipiInputBase
  subclass).
- TimePicker: `TimeSource`, `TimeZone`. Now button → `LipiTimeResolver.ResolveNow`.
- DateTimePicker: `TimeSource`, `TimeZone`, `DisplayZone` + `WorkingZone` computed
  (`DisplayZone ?? TimeZone ?? DefaultTimeZone ?? IndiaIST`) — preserves pre-migration
  clinic-IST behavior. Kept `DateTimeOffset?` binding; zoneless `DateTime?` mode deferred.

### AM/PM segmented toggle (TimePicker)
Native `<select>` period segment replaced with a themed two-button segmented toggle
(`role=radiogroup`, roving tabindex, arrow-key switching, active option filled `--color-primary`,
`lipi-*` CSS tokens only). No LipiSelect dependency — respects the LP-12 chrome-vs-action
principle. Resolves the unstyleable native-dropdown issue.

### Direction × Span date-range presets (LipiDateTimeTypes.cs + RangePicker)
- `PresetDirection { Past, Future, Both }` × `PresetSpan { Day, Week, Month, Quarter, Year }`
  (cumulative). `FiscalYearStart` (int month, default 4=April; 1=calendar quarters). Quarters
  are FISCAL (aligned to FiscalYearStart).
- `LipiDateRangePresets.Build(direction, span, today, weekStart, fyStartMonth)` generates the
  list. RangePicker gains `PresetDirection`/`PresetSpan`/`FiscalYearStart` + `EffectivePresets`
  (explicit `Presets` wins; else builds; else no panel). Existing bundles retained.
- 19 presets; no future-FY/year. Date math validated against a fixed-date reference table
  before C# implementation.

### Deploy / demo
- `deploy-downloads.ps1`: added the 2 helpers; relocated 4 pickers + 6 CSS + types file
  Shared→Forms. `_Imports.razor` (Web) += LiPi.Components.Forms; `_Imports.Components.razor`
  += LiPi.Components.Forms + LiPicons.Blazor.
- `StyleGuide.razor`: interactive Direction×Span presets demo (live Direction/Span/FY
  dropdowns) added to the Date & Time section; existing 12h demo now shows the AM/PM toggle.
- **Manual step required at deploy:** delete old-location picker files from
  `src\LiPi.Web\Components\Shared\` (deploy copies but can't delete → duplicate-component
  errors otherwise).

### Deferred (out of scope, recorded)
Zoneless `DateTime?` DateTimePicker mode; cleanup/removal of the old HIS date/time services;
S3b-ii table date filter (will reuse migrated pickers + carry a zone param for instant windows).

---

### A55 — Phase 2.8 column filters S3b: typed operators + editors (numeric / date / boolean / In)

**Phase**: 2 Sub-step 2.8 — Data Display (LipiTable filtering; continuation of A53/S3a).
**Date**: 2026-06-04
**Status**: ✅ Deployed + runtime-confirmed on the table-filters demo. Logged after the fact — A54 (DateTime PR) took the prior number; this is the S3a→S3b continuation.

**Scope.** Extends A53/S3a (text + universal operators) to FULL typed filtering, all on the same `FilterDescriptor` engine + `Items→quick-search→filters→sort→page` pipeline — new operator/editor coverage, not new architecture.

**Typed operators (`MatchesFilter`).** Numeric: `=, ≠, <, ≤, >, ≥, Between`. Date: `On / Before / After / Between` + a relative set. Boolean: `IsTrue / IsFalse`. `In`: set membership. (text + universal carried from S3a.)

**`OperatorsFor(ColumnType)`** now returns the operator subset per column type (numeric / date / boolean / string), refining S3a's "text+universal for all." **`EditorFor` → `FilterEditor { None, Text, Number, NumberRange, Date, DateRange, RelativeN, Multi }`** picks the popover editor from operator+type (Between→NumberRange/DateRange; relative→RelativeN; In→Multi).

**Date columns — option (c) (locked).** The funnel opens a themed `LipiDateRangePicker` directly (PresetDirection=Both, PresetSpan=Year → Today / This month / Last quarter presets *replace* a relative-operator dropdown); commits a Between window, or open-ended After/Before. **Quarters here are CALENDAR quarters** (filter context) — deliberately distinct from the picker's FISCAL quarters in A54: a filter window is a literal date range, not a fiscal-report boundary. **`In` editor** = distinct-value checkbox checklist (the partial multi-value answer flagged in A53's queued items).

**Zone/week params ("the filter carries the zone, not the picker").** `FilterTimeZone` (`TimeZoneInfo?`) anchors date-window + relative-"today" boundaries for `DateTimeOffset` columns; `FilterWeekStart` (default Sunday) sets week boundaries for relative week math. Both resolved in the engine, not the picker.

**Polish round.** In-chip text joins the selected values (was rendering the `List<>` type name); Between chip shows both bounds; header funnel absolutely-positioned so header text still aligns by `col.Align` (text→left, numeric→right, date/bool/status→center) with the funnel overlaid; active-filter underline affordance via `:has()`; CS1574 cref fix in `LipiDateTimeTypes.cs` (qualified `CommonReports`/`CommonScheduling`).

**Changes** (draft state was still scalar at this point — unified next by A56/PR1).
- `LipiTable.razor.cs`: typed `MatchesFilter` cases; `OperatorsFor(ColumnType)`; `EditorFor` + `FilterEditor` enum; relative-date resolution via `FilterTimeZone`; week math via `FilterWeekStart`; `FilterTimeZone`/`FilterWeekStart` params.
- `LipiTable.razor`: per-type popover editors (number, number-range, date-range-picker option-c, In-checklist); chip-text fixes.
- `LipiTable.razor.css`: funnel absolute-position + align-by-`Align`; active-filter `:has()` underline.
- `LipiDateTimeTypes.cs`: CS1574 cref qualification.
- `StyleGuideTableFilters.razor` (+ `.razor.css`): typed-filter demo at `/admin/style-guide/data-display/table-filters` (hub Phase 2.8 group, `data-enhance-nav="false"`, `DemoStaff` 10-row model).

**Verification gate.** Table-filters demo: numeric Between; date option-c picker (presets); boolean IsTrue/IsFalse; In checklist; chips render values (not type names); funnel stays aligned per column type.

---

### A56 — Phase 2.8 S3b→S3c prep: LipiTable filter draft model unified (per-column `ColumnDraft`)

**Phase**: 2 Sub-step 2.8 — Data Display. Prep for the S3c filter Drawer (PR3).
**Date**: 2026-06-04
**Status**: ✅ Deployed + popover-regression confirmed (text / number-range / In-checklist / date-option-c editors identical to A55/S3b — no regression).

**Scope.** Pure refactor — no behavior change, no engine change. Replaces LipiTable's 7 scalar draft fields (`_draftOperator/_draftValue/_draftValueEnd/_draftMulti/_draftDateStart/_draftDateEnd/_draftColumnKey`) with a single keyed `Dictionary<string, ColumnDraft>`.

**Why.** The header popover edits one column at a time, but the S3c filter Drawer shows EVERY filterable column at once — scalar fields can't hold multiple columns' in-progress edits simultaneously. Both presentations now drive the same per-column draft store. The filter engine (`MatchesFilter`/`OperatorsFor`/`EditorFor`/`CoerceDraft`/chips, A53+A55) is **untouched**.

**Changes.**
- `LipiTable.razor.cs`: new private `class ColumnDraft { Operator, Value, ValueEnd, Multi (HashSet), DateStart, DateEnd }`; `Dictionary<string, ColumnDraft> _drafts`; `EnsureDraft(def)` (lazy, render-safe) + `SeedDraft(def)` (re-seed on popover open); the 6 draft handlers (`OnDraftOperatorChanged/ValueChanged/ValueEndChanged/DateStartChanged/DateEndChanged/ToggleDraftMulti`) + `CommitDraftAsync` all keyed by `ColumnKey`.
- `LipiTable.razor`: popover markup rebound via a hoisted `var d = EnsureDraft(col);` at the top of the open-popover block; date callbacks via `EventCallback.Factory.Create<DateOnly?>`.
- `.css` untouched.

**Deploy note.** This PR was delivered earlier but never reached disk (the on-disk `LipiTable.razor.cs` remained the S3b scalar-draft version); re-shipped and re-verified against that base before logging.

**Verification gate (the "PR1 popover check").** Table-filters demo popover: text / number-range / In-checklist / date(option-c) editors behave IDENTICALLY to A55/S3b — open, edit, Apply, chip, clear; no regression.

**Deferred.** S3c filter Drawer (PR3) composes `<LipiDrawer Open= OnClose=>` declaratively over this unified draft.

---

### A57 — Phase 2.8 overlay cluster → `LiPi.Components.Overlays` (+ LipiSpinner → `LiPi.Components.Feedback`)

**Phase**: 2 Sub-step 2.8 — independent PR. Pulled the 2.10 service-coupled migration forward so S3c's filter Drawer can compose a package `LipiDrawer`.
**Date**: 2026-06-04
**Status**: ✅ Build clean, 0 warnings (`dotnet build src\LiPi.Web`).

**Isolation / namespace.** 38 files (Modal + Drawer + Toast + DynamicTabs components, 4 dialogs, 2 hosts, 12 services/interfaces, type files) → `namespace LiPi.Components.Overlays` (physically `src\LiPi.Components\Overlays\`). **Zero `LiPi.Web.*` refs — the build-enforced §25.3 grep passes.** Sub-component renames `ModalBody/ModalFooter/DrawerBody/DrawerFooter → Lipi*` (a type-NAME change, so consumer markup was rewritten, not just re-namespaced).

**Stays in LiPi.Web (established seam — same as `lipi-inputs.css`).** `lipi-overlays.css` (keeps its valid `.tn-content` pin-shift) and `lipi-overlay-interop.js` are NOT moved → no `App.razor` change; scoped `.razor.css` auto-bundles via `LiPi.Web.styles.css`.

**LipiSpinner.** Migrated to a new `LiPi.Components.Feedback` namespace (collision-free vs `Forms`, correct family per the 2.10 clean-leaves plan) because `LipiToast` depends on it; old LiPi.Web copy orphan-deleted; both `_Imports` + `GlobalUsings` `+= Feedback`.

**Icons (LucideIcon → LipiIcon).** Static names mapped (`chevron-left/right→ChevronLeft/Right`, `x→Close`). `Icon` params kept `string?` — direct passthrough, because **`LipiIcon.Name` is a `string`** and **`LipiconName` is a const-string class, NOT an enum** (the const *is* the kebab key); unknown name → renders nothing, never throws. `SeverityIcon` remapped to real keys (`check`/`warning`/`close`/`info`). **Deferred:** caller-side Lucide-name→LiPicon-key cutover (2.10/3.0).

**Consumer repoint.** LiPi.Web `_Imports += @using LiPi.Components.Overlays` (+ Feedback); new `GlobalUsings.cs` covers `.cs` consumers. *Deploy:* 38 keys repointed (+4 spinner→Feedback), 4 renamed keys swapped, GlobalUsings key added, no dupes. **Manual orphan-deletion required** (deploy copies, can't delete).

**Deferred (build-safe, flagged).** `LipiDrawer.razor.css` 5 hardcoded icon-cell hex → token scrub; packaging `lipi-overlays.css` + interop JS into `_content/` for true redistributability (project-wide seam, 2.10).

**Record so they aren't reintroduced.**
- Isolation audits must grep component **tags**, not just `LiPi.Web.*` namespaces — the namespace grep was clean while `<LucideIcon>`/`<LipiSpinner>` deps survived (surfaced only at build as RZ10012).
- Orphan deletion is mandatory + manual; leaving old copies → `CS0104` / `RZ9985` for every moved type.
- `Get-ChildItem -Recurse -Include *.razor` enumerates nothing without a wildcard path — use `-Filter` (the rename sweep matched 0 files first pass).
- Deploy copies whatever sits in `Downloads\LiPi`; stale older files there silently re-overwrite fresh edits.
- `@rendermode InteractiveServer` needs `@using static …RenderMode` in the package `_Imports`.
- **Pre-existing bug found:** `StyleGuide.razor` nav link is `/admin/style-guide/overlays`; the real route is `/admin/style-guide-overlays` (hyphen). One-char fix, left untouched.

**Verification gate.** Build 0 warnings; `/admin/style-guide-overlays` — modals/drawers/toasts open, focus-trap + scroll-lock work, severity icons render.

---

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
