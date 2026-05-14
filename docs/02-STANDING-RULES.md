# LiPi HIS — Standing Rules

All rules below are MANDATORY and apply to every page and every component.

---

## UX rules (system-wide)

### Confirmation dialogs (MANDATORY)

Confirmation dialog before every destructive or irreversible action:
- Delete
- Lock
- Suspend
- Reset Password
- Status changes
- Role removal

Cancel button must be prominent. Destructive button must be red. **No single-click actions.**

### Table alignment

- First column = **left aligned**
- If first column is S.No / Date = **center**, second column = **left**
- All other columns = **center**
- Action buttons = **right (last column only)**

Applies to all tables and list-grids system-wide.

### Form rules

1. Validate on `oninput` (after first blur) AND `onblur`
2. After save: `fieldset disabled="@saved"` wraps entire form
   - Save button → disabled, "✓ Saved"
   - Cancel button → "Close"
3. Success / error messages near the Save button, **not top of page**
4. Email + Mobile mandatory in all Clinic / Org / User forms

---

## Form accessibility (MANDATORY)

- Every input / select / textarea needs:
  - `id="prefix-field"`
  - `name="camelCase"`
  - `autocomplete="valid-value"`
- Labels need `for="fieldId"`

### Page prefixes
- `un-` UsersNew
- `ue-` UsersEdit
- `cn-` ClinicsNew
- `ce-` ClinicsEdit
- `on-` OrgsNew
- `oe-` OrgsEdit

### Invalid autocomplete values
- `country-name` → use `country`
- `organization-title` / `organization-type` → use `off`

---

## Pre-delivery checklist (MANDATORY)

Run before delivering ANY file to deploy script:

1. Razor tag balance
2. C# braces balance
3. `@using` directives present
4. Cache version stamps consistent across files
5. Deploy script (`deploy-downloads.ps1`) entries present
6. Inheritance shadowing audit
7. Family CSS rules in shared CSS not scoped `.razor.css`
8. No redeclaring base members (HelperText / HasHelperContent etc on LipiInputBase)
9. Component-ref dicts use `int` key not `TValue` (CS8714 error otherwise)
10. ValueExpression on child InputBase = simple private field — never method call or base-class property

---

## Phase 2.5 standing rules

A. No inherited-member redeclarations in subclasses
B. Generic component-ref dicts use `int` key not `TValue` (CS8714)
C. `Dictionary<TValue>` triggers CS8714 when `TValue` unconstrained
D. `LipiInputDefaults` extended with `CheckboxGroupDensity`, `RadioGroupDensity`, `RadioGroupAllowClear`
E. `--r-2xs`, `--tr-toggle`, `--sh-thumb` tokens added to baseline

---

## Phase 2.5.5 standing rules

A. Fieldset cannot be CSS Grid container for legend — use `position:relative` + padding + absolute legend instead
B. Always-rendered popovers outside `lipi-input-body` need `position:fixed` in base CSS
C. Scoped CSS `height:auto` must override shared CSS `height:Npx` for multiline fields
D. `LabelPosition` + `AriaLabel` + filter ship together per component — never any subset
E. `--lipi-label-w` default is **180px** (confirmed by real label fitting during build, NOT 130px)

---

## Strategic-chat verification rule

Before locking specs that extend conventions, reference paths, or invoke tokens / files:
**`project_knowledge_search` FIRST.**

Six Phase 2.5 audit failures caught by build chat:
- Wrong CSS layer
- Wrong dark formula
- Invented `Validation/` folder
- Wrong enum convention
- Three identical enums
- Undefined token

Build chat catches via codebase inspection. **Upfront search is cheaper than reversal round-trip.**

---

## GitHub Actions rules

- SQL validation must strip dollar-quoted blocks (`$$...$$`) and SQL comments before checking quote balance or parens — single quotes inside `$$` function bodies are literal not delimiters
- Always use `@v4` for all GitHub Actions — `@v3` is deprecated and will hard-fail
- Check all action versions on every workflow change
- No external dependencies in CI workflows (no Slack, no third-party notifications) unless deliberately designed and discussed
