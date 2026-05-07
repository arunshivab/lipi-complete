# Phase 2.1 Sub-step Closing — Handoff to Spec Doc Chat (2026-05-03)

> Sub-step 2.1 (LipiButton) is **VISUALLY APPROVED IN BROWSER** and ready
> for spec doc generation. This document gives you the deployed state so
> you can write `01.1-Buttons.md` against reality, not stale project
> knowledge index.

---

## STATUS

**Sub-step 2.1: LipiButton component — VISUALLY APPROVED, READY FOR SPEC DOC**

- ✅ All 5 component files saved + deployed
- ✅ StyleGuide.razor merged with Buttons section showcase
- ✅ Light/Dark mode toggle works for buttons
- ✅ Visual review approved (5 variants × 3 sizes = 15 buttons render correctly)
- ✅ Two color refinements approved post-visual-review (logged as A11)
- 🔧 Code commit pending (user handling separately)
- 🔧 Spec doc 01.1-Buttons.md NEEDED FROM YOU
- 🔧 CHANGE-LOG.md update NEEDED FROM YOU (A11 + Phase 2.1 milestone)
- 🔧 Final commit + tag `phase-2-sub-2-1-complete-may3` after spec doc approval

---

## DEPLOYED STATE — 8 FILES (all attached separately)

These are the FINAL deployed versions. Treat as authoritative — your
project knowledge index is stale (still shows pre-Phase-2.0 placeholders
and missing Phase 2.1 work entirely).

### Component files — `src/LiPi.Web/Components/Shared/`
1. `LucideIcon.razor` — name → SVG resolver, 29 starter icons
2. `LipiButtonSpinner.razor` — size-matched circular spinner
3. `LipiButtonTypes.cs` — `ButtonSize` (Small/Medium/Large), `ButtonVariant` (Primary/Secondary/Danger/Ghost/Link) enums
4. `LipiButton.razor` — main component, 13 parameters
5. `LipiButton.razor.css` — Blazor CSS Isolation styles, all variants × sizes × states

### Page files — `src/LiPi.Web/Pages/`
6. `StyleGuide.razor` — Foundation showcase + new Buttons section (480 lines)
7. `StyleGuide.razor.css` — Foundation styles + new `.sg-button-*` styles (447 lines)

### Theme files — `src/LiPi.Web/wwwroot/themes/`
8. `mode-light.css` — Phase 2.0 baseline + A11 refinements (steel hover, calmer red ladder)

### Files NOT changed in Phase 2.1 (don't regenerate, but you can reference via project_knowledge_search)
- `mode-dark.css` — unchanged since Phase 2.0; user deferred dark mode danger update after seeing light fix in browser, then approved as-is
- `App.razor` — unchanged since Phase 2.0 (CSS Isolation bundle link already added in A9)
- All component files except those listed above

---

## API SURFACE LOCKED IN

13 parameters on LipiButton.razor:

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | `null` | Button label. Null = icon-only mode (requires Icon + AriaLabel). |
| `Size` | `ButtonSize` | `Medium` | Small (28px), Medium (32px), Large (40px) |
| `Variant` | `ButtonVariant` | `Primary` | Non-nullable. 5 values: Primary, Secondary, Danger, Ghost, Link |
| `Icon` | `string?` | `null` | Lucide icon name for left position. v1.0: Lucide only. |
| `IconRight` | `string?` | `null` | Lucide icon name for right position. Ignored when icon-only. |
| `OnClick` | `EventCallback<MouseEventArgs>` | — | Fires after optional confirm. No-op when Disabled or IsLoading. |
| `Disabled` | `bool` | `false` | opacity 0.5, cursor not-allowed, no interaction |
| `IsLoading` | `bool` | `false` | Spinner replaces left icon. Text + right icon remain visible (no layout shift). Parent-controlled. |
| `ConfirmMessage` | `string?` | `null` | Shows confirm dialog before OnClick. v1.0 = window.confirm(); v1.1+ = LipiConfirmDialog (API stable). |
| `FullWidth` | `bool` | `false` | width: 100% for form layouts |
| `Type` | `string` | `"button"` | "button" / "submit" / "reset" |
| `AriaLabel` | `string?` | `null` | REQUIRED for icon-only (throws ArgumentException). WCAG 2.1.4.6 compliance. |
| `Class` | `string?` | `null` | APPEND-ONLY layout utilities. NEVER for visual styling. |

Derived behavior:
- `IsIconOnly` = `ChildContent == null && !string.IsNullOrEmpty(Icon)`
- Icon-only mode: square sizing (width = height = 28/32/40px), min-width waived
- `EffectiveDisabled` = `Disabled || IsLoading`
- IconSize: 14/16/18px matching Size
- Spinner: matches IconSize, 0.6s linear infinite, currentColor

---

## DESIGN PACKAGE FINAL VALUES

### Sizes
| Size | Height | H-padding | Font | Min-width | Icon | Gap |
|---|---|---|---|---|---|---|
| Small | 28px | 12px | 12px | 64px | 14px | 6px |
| Medium | 32px | 16px | 13px | 80px | 16px | 8px |
| Large | 40px | 20px | 14px | 96px | 18px | 10px |

### Variants (light mode reference colors after A11)
| Variant | Bg | Text | Border | Shadow base | Shadow hover |
|---|---|---|---|---|---|
| Primary | `--color-primary` | `--color-text-inverse` | none | `--sh-sm` | `--sh-md` |
| Secondary | transparent | `--color-text-primary` | 1px `--color-border-default` | none | none |
| Danger | `--color-danger` (now red-600) | `--color-text-inverse` | none | `--sh-sm` | `--sh-md` |
| Ghost | transparent | `--color-text-primary` | none | none | none |
| Link | transparent | `--color-primary` | none | none | none |

### Universal
- Border radius: `var(--r-sm)` (6px)
- Active state: `inset 0 1px 2px rgba(0, 0, 0, 0.10)` (literal, not tokenized)
- Focus: `box-shadow: 0 0 0 3px var(--color-primary-pale)` on `:focus-visible` only
- Hover transition: 120ms ease (4 props)
- Disabled: opacity 0.5 + cursor not-allowed
- Font weight: 500
- Text overflow: ellipsis with white-space: nowrap

---

## A11 AMENDMENT — VISUAL REVIEW COLOR REFINEMENTS

After Phase 2.1 deployment, user reviewed buttons in browser. Two
adjustments approved:

### Change 1: Primary hover refined cobalt → steel
- Was: `#1B4DA0` (admin.css `--cobalt`)
- Now: `#134E8C` (steel blue)
- Reason: cobalt felt too vibrant; steel blue gives professional/clinical feel
  appropriate for HIS context. Side-by-side comparison shown to user;
  6 options compared (A/B/E/D/F/G); G chosen.

### Change 2: Danger ladder shifted one notch deeper
- Was: red-500/600/700 (`#EF4444` / `#DC2626` / `#B91C1C`)
- Now: red-600/700/800 (`#DC2626` / `#B91C1C` / `#991B1B`)
- Reason: red-500 felt "too loud for the eyes" given clinical staff sees
  destructive actions throughout shifts. Calmer red-600 base is industry
  standard for enterprise products. Side-by-side comparison + realistic
  context preview shown; Option B chosen.

### Dark mode unchanged
User said dark mode was "good" earlier. After light mode change, user
confirmed dark mode still acceptable. Dark mode danger ladder
(`#F85149` / `#FF6B63` / `#E03B33`) preserved.

### File affected
- `wwwroot/themes/mode-light.css` — 4 token values changed, A11
  amendment block added to header comment

---

## PROPOSED CHANGE-LOG.md ADDITIONS

Add under "v1.0 — AMENDMENTS (May 3, 2026)" section, AFTER A10:

### A11 — LipiButton visual review color refinements

**Changed**: Two color tokens in `mode-light.css` after Phase 2.1 LipiButton
deployment + browser visual review.

**Tokens refined**:

| Token | Phase 2.0 deployed | Phase 2.1 refined |
|---|---|---|
| `--color-primary-hover` | `#1B4DA0` (admin.css cobalt) | `#134E8C` (steel blue) |
| `--color-danger` | `#EF4444` (red-500) | `#DC2626` (red-600) |
| `--color-danger-hover` | `#DC2626` (red-600) | `#B91C1C` (red-700) |
| `--color-danger-active` | `#B91C1C` (red-700) | `#991B1B` (red-800) |

**Reasoning**:

*Primary hover*: Original cobalt (`#1B4DA0`) inherited from admin.css worked
well in admin pages but felt overly vibrant when applied to interactive
button hover state. Steel blue (`#134E8C`) provides the same hue family with
a more professional/clinical tone appropriate for healthcare software.

*Danger ladder*: Tailwind red-500 (`#EF4444`) is industry standard for
attention-grabbing alerts but tested as visually fatiguing in clinical
contexts where staff encounter destructive action buttons throughout shifts.
Shifting the entire ladder one notch deeper (red-600/700/800) preserves the
"warning" semantic while reducing eye strain. Tailwind red-600 (`#DC2626`)
is what enterprise products like Stripe and Linear use as base danger.

*Dark mode unchanged*: Visual review confirmed dark mode danger ladder
(`#F85149` base) remains appropriate. Decision deferred to v1.1+ if needed.

**Files**:
- `src/LiPi.Web/wwwroot/themes/mode-light.css` — 4 token values changed,
  A11 documented in file header

---

### Phase 2.1 milestone entry

Also propose adding (could go under Production-Ready Modules or as new
Phase 2 status section):

```markdown
### Phase 2 Sub-step Status (May 3, 2026)
- ✅ Sub-step 2.0: Foundation tokens + Style Guide bootstrap
- ✅ Sub-step 2.1: LipiButton component (5 variants × 3 sizes,
  29-icon Lucide library, spinner, full Style Guide showcase)
- ⏳ Sub-step 2.2: TextInputs (LipiTextBox, LipiTextArea, LipiNumberInput)
- ⏳ Sub-step 2.3: Selectors (LipiSelect, LipiCombobox, LipiCheckbox, LipiRadio)
- ⏳ Sub-steps 2.4–2.5: Remaining foundational components
```

---

## SPEC DOC TARGETS — `docs/00-COMPONENTS/01.1-Buttons.md`

Document what was BUILT, not what was theoretical.

### Must-have sections (suggested structure)

1. **Overview** — what LipiButton is, decision reference (#12), version (v1.0)
2. **Visual specifications** — exact px values for sizes, padding, font, gaps, icons
3. **API parameter table** — all 13 parameters with types, defaults, notes
4. **Variants** — all 5 with descriptions, when to use, example markup
5. **States** — default, hover, active, focus, disabled, loading (with token references)
6. **Icon system** — LucideIcon component, 29 starter icons listed,
   how to add new icons (dictionary pattern), missing-icon fallback
7. **Accessibility** — AriaLabel requirement (with throw rule), focus-visible
   pattern, aria-busy on loading, WCAG references
8. **Code examples** — 5–7 realistic usages (Save+Cancel pair, async loading,
   confirm flow, icon-only, full-width form submit)
9. **Theme integration** — list all `--color-*` tokens consumed; note light
   vs dark mode differences for hover/active
10. **Future considerations** — LipiConfirmDialog v1.1+ migration, Lucide
    library expansion path, RenderFragment icon override possibility
11. **References** — 00-baseline.css, mode-light/dark.css, related decisions,
    CHANGE-LOG entries (A2, A11)

### Style guidance
- Match `00.2-THEMING-ARCHITECTURE.md` format
- Use markdown tables for API/states/sizes/variants
- Code blocks for all example usages
- Reference paths use forward slashes (`docs/00-COMPONENTS/...`)
- Cite specific Phase 2.0/2.1 amendments where relevant (A2, A8, A9, A10, A11)

### What NOT to do
- Don't document features that weren't built (no v1.1+ aspirations dressed as v1.0)
- Don't document the cobalt/red-500 colors — those are pre-A11 historical
- Don't claim icon system supports custom RenderFragments — string-only in v1.0
- Don't claim ConfirmMessage uses LipiConfirmDialog — uses window.confirm() in v1.0

---

## DELIVERY EXPECTATIONS

1. Show me the spec doc + proposed CHANGE-LOG entries BEFORE saving anywhere
2. I (audit chat) will verify accuracy against deployed state
3. After my approval → save final files
4. User does final commit + tag

---

## LESSONS CARRIED FORWARD (FOR PHASE 2.2+)

For when we kick off TextInputs:

1. **Read existing analogs first** — Before designing LipiTextBox API,
   read LipiButton.razor to match patterns (ChildContent, Variant enum,
   Class append-only rule, etc.)

2. **JS interop is set** — `lipiTheme.apply(brand, mode)` and
   `lipiTheme.getCurrentTheme()` are stable. Don't reinvent.

3. **CSS Isolation works** — drop `LipiTextBox.razor.css` and it
   auto-bundles. No App.razor changes needed.

4. **Auth pattern** — components don't need auth (rendered inside
   already-authorized pages). Only protected pages like StyleGuide use
   `[Authorize]` + claim check + `forceLoad: true`.

5. **Token system stable** — all `--color-*`, `--font-*`, `--sp-*`,
   `--r-*`, `--sh-*`, `--ts-*` tokens deployed and verified. New
   components consume; don't redefine.

6. **Variant nullable was rejected** — explicit defaults preferred over
   "magic" auto-defaults that depend on other parameters.

7. **RenderFragment vs string** — for parameters that are typically
   simple values (icon names, labels), prefer string. Use RenderFragment
   only when complex/formatted content is the norm (ChildContent).

8. **Loading state contract** — width preserved, text visible, only the
   "primary action indicator" (left icon) gets replaced by spinner.

9. **AriaLabel hard validation** — accessibility is non-negotiable in
   HIPAA context. Throw on violation, don't warn.

10. **Class param is layout-only** — never for visual styling. Document
    this strongly in component XML docs.
