# Phase 2.7 — LipiValidationSummary Specification

**Status:** LOCKED
**Component:** LipiValidationSummary
**Built on:** LipiAlert (Phase 2.6.1)

---

## 1. Purpose

Form-level error summary. Displays all validation errors in one place,
typically at the top of a form. WCAG 2.1 Level AA requirement for accessible
forms with multiple errors.

**Why this matters for LiPi:**

Patient registration has ~25 fields. When user clicks Save and 6 are invalid,
they need to see:
1. There are errors (overall state)
2. Which fields are wrong (list)
3. Where they are on the form (click to navigate)

---

## 2. Usage examples

### Basic usage (auto-discovers EditContext)

```razor
<EditForm Model="_patient" OnSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <LipiValidationSummary />   <!-- top of form -->
    
    <LipiTextBox Field="@(() => _patient.FirstName)" />
    <LipiTextBox Field="@(() => _patient.Mobile)" />
    <!-- ... more fields ... -->
    
    <LipiButton Type="submit">Save Patient</LipiButton>
</EditForm>
```

### Both placements (top + above submit)

```razor
<EditForm Model="_patient" OnSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <LipiValidationSummary Placement="ValidationSummaryPlacement.Both" />
    
    <!-- fields -->
    
    <LipiButton Type="submit">Save Patient</LipiButton>
</EditForm>
```

### Compact mode for very long forms

```razor
<LipiValidationSummary Mode="ValidationSummaryMode.Compact" />
<!-- Shows: ⚠ 12 errors. Please fix them before saving. -->
```

### Explicit errors (server-returned, custom validation)

```razor
<LipiValidationSummary Errors="@_serverErrors" />

@code {
    private List<ValidationError> _serverErrors = new()
    {
        new("FirstName", "Already exists in clinic"),
        new("Mobile", "Already linked to another patient")
    };
}
```

---

## 3. Parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Errors` | `IEnumerable<ValidationError>?` | null | Explicit errors override. If null, auto-discover from EditContext. |
| `Mode` | `ValidationSummaryMode` | `Detailed` | Compact / Detailed |
| `Placement` | `ValidationSummaryPlacement` | `Top` | Top / Bottom / Both (flags enum) |
| `ScrollToFieldOnClick` | `bool` | `true` | Click error → scroll + focus field |
| `HeaderTemplate` | `RenderFragment<int>?` | null | Custom header — receives error count |
| `Class` | `string?` | null | Append-only |

---

## 4. Enums (LipiValidationSummaryTypes.cs)

```csharp
public enum ValidationSummaryMode
{
    Compact,    // "⚠ 6 errors. Please fix before saving."
    Detailed    // Full bulleted list of errors (default)
}

[Flags]
public enum ValidationSummaryPlacement
{
    Top    = 1,   // Above form fields (default)
    Bottom = 2,   // Above submit button
    Both   = Top | Bottom
}

public class ValidationError
{
    public string FieldName { get; set; }      // e.g., "FirstName" or "*"
    public string Message { get; set; }        // e.g., "is required"
    public string? DisplayName { get; set; }   // e.g., "First name" (from [Display] attr)
    public string? FieldId { get; set; }       // HTML id of the field, for scroll-to
}
```

---

## 5. Integration model

### EditContext auto-discovery

When `Errors` is not provided, the component reads from the cascading EditContext:

```csharp
[CascadingParameter] public EditContext? CurrentEditContext { get; set; }

protected override void OnInitialized()
{
    if (CurrentEditContext != null && Errors == null)
    {
        CurrentEditContext.OnValidationStateChanged += HandleValidationChanged;
    }
}

private void HandleValidationChanged(object? sender, ValidationStateChangedEventArgs e)
{
    _errors = CurrentEditContext.GetValidationMessages()
        .Select(BuildValidationError)
        .ToList();
    StateHasChanged();
}
```

### Field name resolution

For each validation message, resolve user-friendly field name:

1. If model property has `[Display(Name = "First name")]`, use that
2. Otherwise convert `PascalCase` → `Space Separated` (e.g., `FirstName` → `First name`)
3. If property cannot be identified, fall back to raw message

**Standing convention (added to docs):** Every form model property should have
`[Display(Name = "...")]` for proper LipiValidationSummary display.

```csharp
public class PatientRegistrationModel
{
    [Display(Name = "First name")]
    [Required(ErrorMessage = "is required")]
    public string FirstName { get; set; }
    
    [Display(Name = "Mobile")]
    [Required(ErrorMessage = "is required")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "must be 10 digits")]
    public string Mobile { get; set; }
}
```

Resulting summary item: "First name is required"

---

## 6. Click-to-field navigation

When `ScrollToFieldOnClick=true` (default), clicking an error in the list:

1. Looks up the field's HTML id (matches `[Display(Name)]` to component id via convention or explicit mapping)
2. Scrolls the field into view smoothly
3. Focuses the input
4. Briefly highlights the field (CSS ring/glow for 1.5s)

```javascript
// Implementation requires JS interop
window.lipiValidation = {
    scrollToField: function(fieldId) {
        const el = document.getElementById(fieldId);
        if (!el) return;
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        setTimeout(() => el.focus(), 300);
        el.classList.add('lipi-field-flash');
        setTimeout(() => el.classList.remove('lipi-field-flash'), 1500);
    }
};
```

Field ID convention (already enforced via Phase 2.5.5 accessibility rules):
`id="prefix-fieldName"` where prefix matches the page (e.g., `pn-firstName` for PatientNew).

The summary uses field IDs derived from page prefix + property name.

---

## 7. Visual design

### Uses LipiAlert internally

LipiValidationSummary is a thin wrapper that delegates visual rendering to
LipiAlert with `Severity="AlertSeverity.Danger"`. No new CSS needed for
the box, border, color — reuses Phase 2.6.1 work.

### Detailed mode

```
┌─────────────────────────────────────────────┐
│ ⚠  6 errors. Please fix:                    │
│                                              │
│    • First name is required                  │
│    • Mobile must be 10 digits                │
│    • Email format is invalid                 │
│    • Aadhaar must be 12 digits               │
│    • Date of birth is required               │
│    • Sex must be selected                    │
└─────────────────────────────────────────────┘
```

### Compact mode

```
┌─────────────────────────────────────────────┐
│ ⚠  6 errors. Please fix them before saving. │
└─────────────────────────────────────────────┘
```

### CSS

```css
.lipi-validation-summary {
    margin-bottom: 16px;
}

.lipi-validation-summary-list {
    list-style: none;
    padding: 0;
    margin: 8px 0 0 0;
}

.lipi-validation-summary-item {
    padding: 4px 0;
    font-size: 12px;
}

.lipi-validation-summary-item a {
    color: var(--color-danger-text-strong);
    text-decoration: none;
    cursor: pointer;
}

.lipi-validation-summary-item a:hover {
    text-decoration: underline;
}

/* Field flash on click-to-navigate */
@keyframes lipi-field-flash {
    0%, 100% { box-shadow: 0 0 0 0 transparent; }
    50%      { box-shadow: 0 0 0 4px var(--color-warning-subtle); }
}

.lipi-field-flash {
    animation: lipi-field-flash 1.5s ease-out;
}
```

---

## 8. Accessibility

- `role="alert"` on the summary wrapper
- `aria-live="polite"` (announces appearance without interrupting)
- Each error item is keyboard-tabbable when clickable
- Error count announced first ("6 errors")
- Then each error message announced

Example screen-reader behavior:
> "Alert. 6 errors. First name is required. Mobile must be 10 digits. Email format is invalid. ..."

---

## 9. Placement behavior

| Placement | Position | Use case |
|---|---|---|
| `Top` | Above first form field (default) | Standard pattern, errors visible before user scrolls |
| `Bottom` | Above submit button area | User scrolls to submit, sees errors right where they look |
| `Both` | Both positions render same summary | Long forms (e.g., 25+ field patient registration) |

In `Both` mode, the component renders twice — top + bottom. Both update simultaneously when validation changes.

---

## 10. StyleGuide section

Add `#validation-summary` section with:
1. Detailed mode with 6 errors
2. Compact mode with 6 errors
3. Top placement (above form)
4. Bottom placement (above submit)
5. Both placement (long form example)
6. Click-to-field navigation demo
7. Explicit errors mode (server-returned errors)
8. Empty state (no errors — should render nothing)

---

## 11. Hides when no errors

When `Errors` is empty OR `EditContext` has no validation messages, the component
renders nothing (no empty alert, no "0 errors" message). Disappears entirely.

---

*Built on LipiAlert internally. Adds intelligence for EditContext integration,
field name resolution, and click-to-field navigation. Reuses Phase 2.6.1 visual work.*
