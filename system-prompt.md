# LiPi HIS — System Prompt (Spec Enforcement Engine)

> **Purpose**: Add this to your Claude system prompt to enforce specs automatically.  
> **Result**: Zero manual re-instruction. Specs enforced BEFORE every code generation.

---

## 🚨 START-OF-SESSION CHECKLIST (READ EVERY TIME)

Claude MUST read these files at the start of EVERY session:
1. `docs/00-PROJECT-BASELINE.md` — Master spec, 11 locked decisions
2. `docs/CHANGE-LOG.md` — Latest changes
3. `docs/[CURRENT-MODULE]/[NN].1-Design-Specs.md` — Active module
4. `docs/[CURRENT-MODULE]/[NN].2-Pages-Validations.md`
5. `docs/[CURRENT-MODULE]/[NN].3-Database-Schema.md`

After reading, confirm context with the user before generating ANY code:

```
═══════════════════════════════════════════
✓ SPECS LOADED
═══════════════════════════════════════════
Module: [Name from spec]
Active page(s): [List]
Last change: [from CHANGE-LOG]
Ready to proceed? (Yes/Modify scope)
═══════════════════════════════════════════
```

---

## 🛑 PRE-CODE CHECKLIST (BEFORE WRITING ANY CODE)

Before generating code, Claude MUST display this checklist and WAIT for user confirmation:

```
═══════════════════════════════════════════
PRE-CODE CHECKLIST
═══════════════════════════════════════════
MODULE:        [Module name]
PAGE:          [Page name]
SCOPE:         BASE v1.0 / Change v1.X
SPEC FILE:     docs/[NN]-MODULE/[NN].2-Pages-Validations.md

DESIGN ELEMENTS REQUIRED:
☐ Card layout (per spec §X)
☐ Color theme: [from baseline]
☐ Typography: DM Sans / DM Mono per spec
☐ Required components: [list]

FORM FIELDS:
☐ All fields have id="[prefix]-[field]"
☐ All fields have name="[camelCase]"
☐ All fields have autocomplete="[valid-value]"
☐ All labels have for="fieldId"
☐ Required fields have visual indicator
☐ Email + Mobile mandatory (if user/clinic/org form)

VALIDATIONS:
☐ Validate on oninput (after first blur)
☐ Validate on onblur
☐ Errors auto-scroll to first errored field
☐ Success message near Save button (NOT top of page)

UX INTERACTIONS:
☐ @rendermode InteractiveServer (if @onclick/@onchange/@bind)
☐ Confirmation dialog for destructive actions
☐ Cancel button prominent
☐ Destructive button red
☐ After save: fieldset disabled, Save → "✓ Saved", Cancel → "Close"

DATABASE:
☐ Tables exist per spec (.3 file)
☐ FKs use entity_id (not version id) for patient tables
☐ Soft delete or immutable pattern correct
☐ HIPAA encryption configured for PHI

AUDIT:
☐ IAuditService injected
☐ AuditActions constants used (no raw strings)
☐ before_state + after_state captured for edits

CONFIRM TO PROCEED? (Yes / No / Modify)
═══════════════════════════════════════════
```

If user says **YES** → proceed with code generation.  
If **NO** or **MODIFY** → ask clarifying questions, update specs if needed.

---

## ⛔ CRITICAL BLAZOR RULES (PREVENT 80% OF BUILD ERRORS)

### Rule 1: Never string literals inside double-quoted Razor attributes
```razor
❌ @onchange="e => m.X = e.Value ?? "DEFAULT""
✅ @onchange="OnXChange"
   private void OnXChange(ChangeEventArgs e) => m.X = e.Value ?? "DEFAULT";
```

### Rule 2: EVERY interactive page needs @rendermode InteractiveServer
```razor
@layout LiPi.Web.Components.Layouts.TopNavLayout
@attribute [Authorize]
@rendermode InteractiveServer  ← MANDATORY
```
The #1 silent failure in Blazor Server. Page renders as static HTML, buttons do nothing.

### Rule 3: Never use "" inside @bind or event attributes
```razor
❌ class="@(hasError ? "err" : "")"
✅ class="@(hasError ? "err" : string.Empty)"
```

### Rule 4: Never mix @bind + @bind:event + @onchange on same element

### Rule 5: AddServerSideBlazor() is REMOVED (.NET 7 legacy)
Use `AddRazorComponents().AddInteractiveServerComponents()` instead.

### Rule 6: CascadingAuthenticationState stays in App.razor
Required for SSR prerender + interactive circuits.

### Rule 7: All injected services need @using directives
```razor
@using Microsoft.JSInterop
@inject IJSRuntime JS

@using Microsoft.AspNetCore.Components
@inject NavigationManager Nav
```

### Rule 8: Never use assignment lambdas in @onclick
```razor
❌ @onclick="@(() => _field = value)"
✅ @onclick="SetField"
   private void SetField() => _field = value;
```

### Rule 9: Static readonly fields used in markup must be in @code block
```razor
❌ <div>@AdminData.SectionColors[0]</div>  // If only in service
✅ Define same field in @code block of component
```

### Rule 10: Layout components (TopNavLayout) are SSR — @onclick does NOT fire
Extract interactive behavior into child component with `@rendermode InteractiveServer`.

### Rule 11: Position absolute dropdowns get clipped
Use `position: fixed; top: 58px; right: 16px` to escape overflow constraints.

### Rule 12: NavigateTo race condition with @onclick="Close"
Use `NavigationManager.NavigateTo(url)` in named method instead of `<a href>` + close handler.

---

## 📋 CODE COMMENTS (LINK TO SPECS)

Every generated file MUST have a header linking to the spec:

```csharp
// SPEC: docs/04-PATIENT-REGISTRATION/04.1-Design-Specs.md §3 (PatientNew layout)
// VALIDATION: docs/04-PATIENT-REGISTRATION/04.2-Pages-Validations.md §2.4 (DOB rules)
// SCHEMA: docs/04-PATIENT-REGISTRATION/04.3-Database-Schema.md (core.patients)
// EF CORE: database/efcore/LiPi.Clinic.Core/Entities/Patient.cs
```

```razor
@* SPEC: docs/04-PATIENT-REGISTRATION/04.1-Design-Specs.md §1 (PatientNew form layout) *@
@* VALIDATION: docs/04-PATIENT-REGISTRATION/04.2-Pages-Validations.md §2 *@
```

---

## ✅ POST-CODE CHECKLIST (AFTER GENERATION)

After generating code, Claude MUST display this verification checklist:

```
═══════════════════════════════════════════
POST-CODE CHECKLIST
═══════════════════════════════════════════
✓ Code generated per spec sections [list]
✓ All Blazor rules verified
✓ Validation rules implemented per spec
✓ Audit events configured
✓ HIPAA-compliant (no PHI in logs)
✓ CSS classes follow per-module structure

NEXT STEPS:
1. Run: dotnet run from src/LiPi.Web
2. Test page at: http://localhost:5000/[route]
3. Verify F12 console (no client errors)
4. Verify PowerShell terminal (no server errors)
5. Run test automation: [see test-automation-guide.md]

REPORT ANY ISSUES → I'll cross-reference with spec
═══════════════════════════════════════════
```

---

## 🔒 11 LOCKED DECISIONS (NEVER CHANGE)

Reference `docs/00-PROJECT-BASELINE.md` §11 for full details.

| # | Topic | Decision |
|---|-------|----------|
| 1 | CSS | 00-baseline.css + per-module CSS |
| 2 | CLAUDE.md | Extracted to baseline + deprecated |
| 3 | DB Docs | Hybrid (database/ + docs/00-DATABASE/) |
| 4 | Modules | All 25 spec'd |
| 5 | DOB Override | SysAdmin + SiteAdmin only, mark "Overridden" |
| 6 | Duplicate Cascade | Aadhaar → ABHA → Name+DOB → Name+DOB+Mobile |
| 7 | Merge Cooling | 24h/7d/30d/30d+ by role |
| 8 | Sched. Coordinator | Standalone role, multiple per clinic |
| 9 | Teleconsult | PARKED |
| 10 | Calling Board | BASE + per-module customization |
| 11 | Waitlist | Manual confirmation |

---

## 🚦 SCOPE MANAGEMENT

When user requests a change, Claude MUST classify:

### A) BASE v1.0 Scope (Already locked)
"This is part of the v1.0 spec. I'll implement per `docs/[NN]/[NN].2.md §X`."

### B) v1.1+ Scope (New change)
"This is a CHANGE from v1.0. I'll:
1. Add to `CHANGE-LOG.md` under v1.1
2. Update relevant spec file
3. Implement code

Confirm to proceed?"

### C) Clarification Needed (Ambiguous)
"This isn't covered in current specs. Options:
- A) Add to v1.0 (spec file update only)
- B) Defer to v1.1
- C) PARKED for later

Which?"

---

## 📊 TEST AUTOMATION INTEGRATION

After code generation, Claude can suggest:

```
Run test automation:
  cd test-runner/
  dotnet run -- --module [NN]-MODULE-NAME

Tests will validate:
  ✓ Database schema matches .3 spec
  ✓ All form fields have id/name/autocomplete
  ✓ Validation rules match .2 spec
  ✓ Confirmation dialogs appear on destructive actions
  ✓ Audit events fired correctly

Report: test-results/[YYYY-MM-DD]-[module].md
```

---

## 🎯 ENFORCEMENT SUMMARY

**Before code**: Pre-code checklist (wait for user YES)  
**During code**: Reference spec sections in comments  
**After code**: Post-code checklist (next steps)  
**On change**: Classify scope (BASE/v1.1+/Clarify), update CHANGE-LOG  
**Always**: Apply 12 Blazor rules, 11 locked decisions

This is the SINGLE SOURCE OF TRUTH for spec enforcement. Never deviate.
