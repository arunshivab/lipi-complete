// SPEC:     docs/00-COMPONENTS/2.7/04-LipiValidationSummary-Spec.md
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiValidationSummary partial class — the heaviest single component in Phase 2.7.
//
// Responsibilities:
//   1. Auto-discover errors from cascading EditContext when Errors is not supplied.
//   2. Subscribe to OnValidationStateChanged + OnFieldChanged for live updates.
//   3. Resolve [Display(Name = "...")] attribute per field for user-friendly labels.
//   4. Convert PascalCase to spaced text as fallback (FirstName → "First name").
//   5. Resolve field HTML id from the FieldIdPrefix parameter + property name.
//   6. Dispose subscriptions cleanly.
//   7. JS interop call to scroll + focus + flash the field on error click.
//
// Threading note: OnValidationStateChanged fires from the EditForm thread; we
// must InvokeAsync(StateHasChanged) to marshal the UI update back onto the
// circuit's renderer.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LiPi.Web.Components.Shared;

public partial class LipiValidationSummary
{
    // ==========================================================================
    // PARAMETERS (spec §3 — table of params)
    // ==========================================================================

    /// <summary>
    /// Explicit error list. When supplied, the EditContext auto-discovery path
    /// is skipped — useful for server-returned errors after a save attempt.
    /// When null (default), errors are pulled live from the cascading EditContext.
    /// </summary>
    [Parameter] public IEnumerable<ValidationError>? Errors { get; set; }

    /// <summary>Display mode. Default <see cref="ValidationSummaryMode.Detailed"/>.</summary>
    [Parameter] public ValidationSummaryMode Mode { get; set; } = ValidationSummaryMode.Detailed;

    /// <summary>Placement bit-flags. Default <see cref="ValidationSummaryPlacement.Top"/>.
    /// When set to <see cref="ValidationSummaryPlacement.Both"/>, this component
    /// instance still renders ONCE per placement value — consumers wanting both
    /// top and bottom typically place the component twice (once at top, once at
    /// bottom, each with appropriate Placement). Bit-flag value is preserved
    /// on the wrapper class so callers can target CSS.
    /// </summary>
    [Parameter] public ValidationSummaryPlacement Placement { get; set; } = ValidationSummaryPlacement.Top;

    /// <summary>When true (default), clicking an error scrolls and focuses
    /// the corresponding field, with a brief highlight flash.</summary>
    [Parameter] public bool ScrollToFieldOnClick { get; set; } = true;

    /// <summary>Optional custom header template. Receives the error count.</summary>
    [Parameter] public RenderFragment<int>? HeaderTemplate { get; set; }

    /// <summary>
    /// Prefix for HTML field IDs. Forms use a page prefix (e.g., "pn-" for
    /// PatientNew, "un-" for UsersNew) — supply the prefix here so the summary
    /// can build "pn-firstName" from a "FirstName" property. When null, the
    /// summary builds IDs from just the property name (lowercased first letter).
    /// </summary>
    [Parameter] public string? FieldIdPrefix { get; set; }

    /// <summary>Append-only layout utility classes.</summary>
    [Parameter] public string? Class { get; set; }

    // ==========================================================================
    // CASCADING + INJECTED
    // ==========================================================================

    /// <summary>Cascaded from EditForm. Null is OK — component then requires
    /// explicit Errors. We don't throw on null because the component might be
    /// placed in a context (test page, demo) where EditForm isn't relevant.</summary>
    [CascadingParameter] public EditContext? CurrentEditContext { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;


    // ==========================================================================
    // STATE
    // ==========================================================================

    private List<ValidationError> _errorsList = new();
    private EditContext? _subscribed;             // tracks which EditContext we're listening to
    private Type? _modelType;                     // tracks model type for cached property lookups

    private bool HasErrors => _errorsList.Count > 0;
    private bool RenderTop => Placement.HasFlag(ValidationSummaryPlacement.Top);
    private bool RenderBottom => Placement.HasFlag(ValidationSummaryPlacement.Bottom);

    private string SummaryTitle =>
        Mode == ValidationSummaryMode.Compact
            ? $"{_errorsList.Count} {(_errorsList.Count == 1 ? "error" : "errors")}. Please fix them before saving."
            : $"{_errorsList.Count} {(_errorsList.Count == 1 ? "error" : "errors")}. Please fix:";

    private string WrapperCssClasses
    {
        get
        {
            var classes = new List<string> { "lipi-validation-summary" };
            if (Placement == ValidationSummaryPlacement.Bottom) classes.Add("lipi-validation-summary-bottom");
            if (Placement == ValidationSummaryPlacement.Both)   classes.Add("lipi-validation-summary-both");
            if (Mode == ValidationSummaryMode.Compact)          classes.Add("lipi-validation-summary-compact");
            if (!string.IsNullOrWhiteSpace(Class))              classes.Add(Class);
            return string.Join(" ", classes);
        }
    }


    // ==========================================================================
    // LIFECYCLE
    // ==========================================================================

    protected override void OnParametersSet()
    {
        // Subscribe / re-subscribe to EditContext as needed. If the consumer
        // wraps an EditForm in a state that re-creates EditContext, we'll see
        // a new instance here and need to swap subscriptions.
        if (Errors != null)
        {
            // Explicit-errors path — unsubscribe any prior EditContext and
            // use the supplied list verbatim.
            UnsubscribeFromEditContext();
            _errorsList = Errors.ToList();
            return;
        }

        if (CurrentEditContext == null)
        {
            // No EditContext and no explicit errors — render nothing.
            _errorsList = new List<ValidationError>();
            return;
        }

        if (!ReferenceEquals(CurrentEditContext, _subscribed))
        {
            UnsubscribeFromEditContext();
            CurrentEditContext.OnValidationStateChanged += HandleValidationChanged;
            CurrentEditContext.OnFieldChanged          += HandleFieldChanged;
            _subscribed = CurrentEditContext;
            _modelType  = CurrentEditContext.Model?.GetType();
        }

        // Initial population — pick up any errors already present on the context.
        RefreshErrorsFromContext();
    }


    // ==========================================================================
    // EDITCONTEXT HANDLERS
    // ==========================================================================

    private void HandleValidationChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        // OnValidationStateChanged can fire from any thread. Marshal to the
        // circuit's renderer before touching state + StateHasChanged.
        _ = InvokeAsync(() =>
        {
            RefreshErrorsFromContext();
            StateHasChanged();
        });
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        // Field-level changes also trigger a refresh — keeps the summary live
        // as the user types (after first blur, per LipiInputBase touch protocol).
        _ = InvokeAsync(() =>
        {
            RefreshErrorsFromContext();
            StateHasChanged();
        });
    }

    private void RefreshErrorsFromContext()
    {
        if (CurrentEditContext == null)
        {
            _errorsList = new List<ValidationError>();
            return;
        }

        // Walk every model property and ask the EditContext for messages on that
        // field. This preserves the field <-> message association we need for
        // click-to-field navigation. Properties without messages contribute
        // nothing to the result.
        //
        // Object-level (top-level) validation messages are out of scope for the
        // auto-discovery path in v1.0 — consumer with object-level rules should
        // supply Errors explicitly (with FieldName = "*"). Documented limitation.
        var result = new List<ValidationError>();

        if (_modelType != null && CurrentEditContext.Model != null)
        {
            foreach (var prop in _modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Only walk public read-write or read-only properties (the things
                // a model exposes as "fields"). Indexers and write-only oddities
                // skipped — calling FieldIdentifier on them throws.
                if (prop.GetIndexParameters().Length > 0) continue;
                if (!prop.CanRead) continue;

                FieldIdentifier fieldId;
                try
                {
                    fieldId = new FieldIdentifier(CurrentEditContext.Model, prop.Name);
                }
                catch
                {
                    // FieldIdentifier construction can throw for properties not
                    // representable as field paths — skip them silently.
                    continue;
                }

                foreach (var msg in CurrentEditContext.GetValidationMessages(fieldId))
                {
                    result.Add(BuildValidationError(prop, msg));
                }
            }
        }

        _errorsList = result;
    }


    // ==========================================================================
    // FIELD-NAME RESOLUTION
    // ==========================================================================

    /// <summary>
    /// Build a <see cref="ValidationError"/> from a model property and a
    /// validation message. Resolves Display(Name) attribute, falls back to
    /// PascalCase-to-space conversion, and builds the HTML id from prefix + name.
    /// </summary>
    private ValidationError BuildValidationError(PropertyInfo prop, string message)
    {
        var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
        var displayName = displayAttr?.Name ?? PascalCaseToSpace(prop.Name);
        var fieldId     = BuildFieldId(prop.Name);

        return new ValidationError
        {
            FieldName   = prop.Name,
            Message     = message,
            DisplayName = displayName,
            FieldId     = fieldId
        };
    }

    /// <summary>"FirstName" → "First name". The first letter stays capitalised;
    /// subsequent capitals get a leading space and lowercase.</summary>
    private static string PascalCaseToSpace(string pascal)
    {
        if (string.IsNullOrEmpty(pascal)) return pascal;
        var sb = new StringBuilder(pascal.Length + 4);
        sb.Append(pascal[0]);
        for (int i = 1; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c))
            {
                sb.Append(' ');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>Build the HTML id from prefix + lowercased-first-letter property name.
    /// "FirstName" with prefix "pn-" → "pn-firstName".
    /// "FirstName" with no prefix → "firstName".</summary>
    private string BuildFieldId(string propName)
    {
        if (string.IsNullOrEmpty(propName)) return string.Empty;
        var camel = char.ToLowerInvariant(propName[0]) + propName.Substring(1);
        return string.IsNullOrWhiteSpace(FieldIdPrefix)
            ? camel
            : $"{FieldIdPrefix}{camel}";
    }


    // ==========================================================================
    // CLICK-TO-FIELD NAVIGATION
    // ==========================================================================

    private async Task OnErrorClick(string fieldId)
    {
        try
        {
            await JS.InvokeVoidAsync("lipiValidation.scrollToField", fieldId);
        }
        catch (JSException)             { /* JS not loaded — silent failure is fine */ }
        catch (JSDisconnectedException) { /* circuit disconnecting */ }
        catch (TaskCanceledException)   { /* component unmounting */ }
    }


    // ==========================================================================
    // DISPOSE
    // ==========================================================================

    public void Dispose()
    {
        UnsubscribeFromEditContext();
    }

    private void UnsubscribeFromEditContext()
    {
        if (_subscribed != null)
        {
            _subscribed.OnValidationStateChanged -= HandleValidationChanged;
            _subscribed.OnFieldChanged           -= HandleFieldChanged;
            _subscribed = null;
            _modelType  = null;
        }
    }
}
