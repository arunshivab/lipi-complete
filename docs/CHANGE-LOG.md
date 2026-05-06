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
> Note: A14 is reserved for the LipiButton env-gated retrofit (see Known
> divergences below). Numbering jumps A13 → A15 to preserve A14's reservation.
> The trigger condition for A14 (Phase 2.2 components shipping the env-gated
> pattern proven) is satisfied as of A15 — A14 retrofit is now ready to schedule.

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

**A14 trigger condition met**: The env-gated throw pattern (`IWebHostEnvironment.IsDevelopment()` → throw; production → `ILogger.LogError` + fallback) shipped successfully across all five Phase 2.2 components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox). The pattern is proven. A14's queued LipiButton retrofit (which awaited "Phase 2.2 components shipping the env-gated pattern proven") is now actionable and ready to schedule.

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
- **A14** — LipiButton env-gated retrofit (trigger condition met as of A15; ready to schedule).
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

- **A14 (queued — trigger met)** — Phase 2.1 LipiButton uses unconditional `throw new ArgumentException` in `OnParametersSet` for missing `AriaLabel` on icon-only buttons (LipiButton.razor lines 122–130). Phase 2.2 components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox) ship with an env-gated pattern: `IWebHostEnvironment.IsDevelopment()` → throw; production → `ILogger.LogError` + render with auto-generated fallback. This protects production from parameter-validation crashes (high stakes for a hospital app) while keeping dev-time strictness.
  - LipiButton.razor will be retrofitted to the env-gated pattern as a focused v1.1 amendment (A14) once Phase 2.2 components ship and the pattern is proven.
  - A `TODO (A14)` comment is added to `LipiButton.razor` so the inconsistency is not forgotten.
  - Until A14 ships: Phase 2.1 LipiButton hard-throws, Phase 2.2 components warn-and-fallback. Documented divergence, scheduled for closure.
  - **Status update (2026-05-05, post-A15)**: Trigger condition met. The env-gated throw pattern shipped successfully across all 5 Phase 2.2 components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox). Pattern proven. LipiButton retrofit is now ready to schedule.

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

**A14 status update (post-A16).** A14 (LipiButton env-gated retrofit) trigger condition has been doubly satisfied: first by A15 (Phase 2.2 components shipping the pattern), now by A16 (Phase 2.2.5 consolidating the pattern into LipiInputBase). Retrofit pattern: extend the LipiButton parameter validation to use the env-gated `Env.IsDevelopment() ? throw : log + fallback` pattern, removing the current always-throw. A14 still scheduled for v1.1 sprint planning.

**Coordination with deploy script.** Batch 8a updated `deploy-downloads.ps1` to include `LipiInputBase.cs` and `TextboxTest.razor`. The other 4 components (LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox) and their test pages were already mapped from Phase 2.2 batches — no additional script edits needed. The deploy script does not version files, so reships overwrite cleanly.

---

## v1.1 — PLANNED (Future)

### Pending Items (Move from PARKED → v1.1)
- [ ] Teleconsult feature (after 10+ modules complete)
- [ ] Auto patient identifier verification (Aadhaar, ABHA via DigiLocker)
- [ ] Real-time bed management (IPD)
- [ ] PACS integration (Radiology)
- [ ] **Armoki brand theme** (after Armoki finalizes brand identity)
- [ ] **Auto theme mode** (follows OS preference)
- [ ] **High-contrast theme mode** (accessibility)
- [ ] **Density toggle** (user preference: comfortable/compact/spacious)
- [ ] **A7 — ThemeProvider spec/code reconciliation** (see Known divergences above)
- [ ] **A14 — LipiButton env-gated throw retrofit** (trigger condition met as of A15; ready to schedule)

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
