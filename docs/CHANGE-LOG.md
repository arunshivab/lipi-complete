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

### Phase 2 Sub-step Status (May 3, 2026)

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
- ⏳ Sub-step 2.2: TextInputs (LipiTextBox, LipiTextArea, LipiNumberInput)
- ⏳ Sub-step 2.3: Selectors (LipiSelect, LipiCombobox, LipiCheckbox, LipiRadio)
- ⏳ Sub-steps 2.4–2.5: Remaining foundational components

---

### Known divergences from deployed code (defer to v1.1)

The amendments above bring the spec into sync with deployed Phase 2.0 work. The following divergences between spec and deployed code are **acknowledged but not addressed in Phase 2.0** to keep scope contained:

- **A7 (planned)** — `00.2-THEMING-ARCHITECTURE.md` §Theme Provider Component: spec section shows the older `IUserPreferenceService` + `IClinicContextService` + `eval()` pattern. Deployed code uses `IThemeContextService` + `lipiTheme.apply` (post-D6 architecture refinement during Phase 1 Deliverable 6).
  - Defer reconciliation to a focused v1.1 spec-update session (call it A7) so the spec accurately documents production code.
  - Until then, treat the deployed code as authoritative and the spec section as historical reference.

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
