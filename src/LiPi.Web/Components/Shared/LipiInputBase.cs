// SPEC: docs/00-COMPONENTS/01.2-TextInputs.md §1.2 (InputBase derivation rationale)
// PHASE: Phase 2 Sub-step 2.2.5 — EditContext.OnValidationStateChanged auto-population
// AMEND: docs/CHANGE-LOG.md A16 (pending — Phase 2.2.5 wrap-up)
//
// LipiInputBase consolidates four concerns shared across all five Phase 2.2 input
// components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect, LipiCombobox):
//
//   1. EditContext subscription — when DataAnnotationsValidator fires inside an
//      <EditForm>, error messages auto-populate _editContextError. Components
//      read EffectiveError (caller Error wins; falls back to _editContextError).
//
//   2. Common parameters — the 18 parameters that are identical across all five
//      components (Name, Label, Placeholder, Helper, Error, Warning, Success,
//      Required, Disabled, ReadOnly, Autocomplete, Size, Icon, AriaDescribedBy,
//      RequiredVisualStyle, LabelConfidence, Cols, Class). Concrete components
//      inherit these and add only their component-specific parameters.
//
//   3. State cascade — ResolvedState / EffectiveTrailingIcon / HelperText /
//      HelperStateClass / AriaInvalidAttr — the state precedence machine that
//      computes Disabled > ReadOnly > Error > Success > Warning > RequiredEmpty
//      > Default. Reads EffectiveError so EditContext-driven validation flows
//      through automatically.
//
//   4. Anonymous-id fallback + env-gated parameter validation — when Name or
//      Label is missing, Development throws; Production logs and falls back to
//      a stable cached anonymous id and "(unlabeled)" placeholder. Concrete
//      components override AnonymousIdPrefix / ComponentIdPrefix for namespacing.
//
// Hierarchy:
//   InputBase<TValue>        (Microsoft framework)
//     └── LipiInputBase<TValue>     (this file — IDisposable)
//           ├── LipiTextBox<string?>
//           ├── LipiTextArea<string?>
//           ├── LipiNumberInput<TValue>
//           └── LipiSelectBase<TValue, TItem>     (IAsyncDisposable)
//                 ├── LipiSelect<TValue>
//                 └── LipiCombobox<TValue, TItem>
//
// IDisposable (NOT IAsyncDisposable): EditContext detach is genuinely synchronous
// (-= on an event). Microsoft's InputBase<TValue> uses IDisposable; matching that
// pattern is more valuable than internal consistency with LipiSelectBase. Components
// that need async disposal (LipiSelectBase) layer IAsyncDisposable on top while
// inheriting LipiInputBase's IDisposable.

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiPi.Web.Components.Shared;

public abstract class LipiInputBase<TValue> : InputBase<TValue>, IDisposable
{
    // ==========================================================================
    // INJECTED SERVICES (consolidated from per-component duplication)
    // ==========================================================================

    [Inject] protected IOptions<LipiInputDefaults> Defaults { get; set; } = default!;
    [Inject] protected IWebHostEnvironment Env { get; set; } = default!;
    [Inject] protected ILogger<LipiInputBase<TValue>> Log { get; set; } = default!;

    // ==========================================================================
    // COMMON PARAMETERS (the 18 shared across all Phase 2.2 components)
    // ──────────────────────────────────────────────────────────────────────────
    // Inherited from InputBase<TValue> — DO NOT redeclare in subclasses:
    //   [Parameter] Value, ValueChanged, ValueExpression, DisplayName
    //   protected CurrentValue, CurrentValueAsString, EditContext, FieldIdentifier
    // ==========================================================================

    /// <summary>HTML name attribute. REQUIRED. Drives id (via ComponentIdPrefix-Name).
    /// Throws in Development if missing/empty. Logs and falls back to a cached anonymous
    /// id (via AnonymousIdPrefix) in Production.</summary>
    [Parameter, EditorRequired] public string Name { get; set; } = string.Empty;

    /// <summary>Field label rendered above the input. REQUIRED. Visible text for sighted
    /// users and accessible name for assistive tech. Throws in Development if missing/empty;
    /// falls back to "(unlabeled)" in Production.</summary>
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;

    /// <summary>Placeholder text shown when value is empty.</summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>Helper text shown in the helper slot when no error/warning/success present.</summary>
    [Parameter] public string? Helper { get; set; }

    /// <summary>Caller-set error message. When non-empty, takes precedence over EditContext-
    /// driven errors (EffectiveError = Error ?? _editContextError). Use to display server-side
    /// validation errors or to override EditContext temporarily.</summary>
    [Parameter] public string? Error { get; set; }

    /// <summary>Warning message — non-empty triggers Warning state. Never auto-populated
    /// from EditContext (DataAnnotationsValidator only produces errors).</summary>
    [Parameter] public string? Warning { get; set; }

    /// <summary>Success message — non-empty triggers Success state.</summary>
    [Parameter] public string? Success { get; set; }

    /// <summary>Marks field as required. Pairs with RequiredVisualStyle for visual treatment.</summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>Disabled state — terminal, no interaction.</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Read-only state — selectable but not editable.</summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>HTML5 autocomplete token. Validated against WHATWG list at OnParametersSet.</summary>
    [Parameter] public string? Autocomplete { get; set; }

    /// <summary>Size variant. Default Medium (32px).</summary>
    [Parameter] public InputSize Size { get; set; } = InputSize.Medium;

    /// <summary>Lucide icon name rendered to the LEFT of input text (decorative).</summary>
    [Parameter] public string? Icon { get; set; }

    /// <summary>External aria-describedby reference (in addition to the auto-wired helper id).</summary>
    [Parameter] public string? AriaDescribedBy { get; set; }

    /// <summary>Per-component override of required-field visual treatment. Null = inherit
    /// from <see cref="LipiInputDefaults.RequiredVisualStyle"/>.</summary>
    [Parameter] public RequiredVisualStyle? RequiredVisualStyle { get; set; }

    /// <summary>Confidence pill in label (Verified / Self-reported / Estimated / Unknown).
    /// Used for DOB and identity verification fields per Decision #5.</summary>
    [Parameter] public ConfidenceLevel? LabelConfidence { get; set; }

    /// <summary>Optional grid column span. Sets style="grid-column: span @Cols".</summary>
    [Parameter] public int? Cols { get; set; }

    /// <summary>Append-only CSS class. LAYOUT UTILITIES ONLY (margin, alignment).</summary>
    [Parameter] public string? Class { get; set; }

    // ==========================================================================
    // EDITCONTEXT SUBSCRIPTION (Phase 2.2.5 NEW)
    // ──────────────────────────────────────────────────────────────────────────
    // When the component lives inside an <EditForm>, EditContext fires
    // OnValidationStateChanged whenever DataAnnotationsValidator (or any registered
    // validator) revalidates. We subscribe to this event, read the validation
    // messages for THIS field, and surface the first one via _editContextError.
    //
    // Caller-set Error parameter wins over EditContext (see EffectiveError).
    //
    // Edge case: EditContext can swap if the component is moved between EditForms
    // (rare, but Microsoft's InputBase handles it). OnParametersSet checks for
    // ReferenceEquals change and re-subscribes appropriately.
    // ==========================================================================

    private EditContext? _previousEditContext;
    private string? _editContextError;

    /// <summary>
    /// Touched-state flag for the "validate on blur, then on input" UX pattern.
    /// False until the user blurs the field for the first time. Once true, stays true
    /// for the lifetime of the component instance — subsequent keystrokes can update
    /// validation immediately without waiting for another blur.
    ///
    /// This pattern matches Material UI / Carbon / Ant Design / every major form library.
    /// Microsoft's InputBase does NOT do this — per-keystroke validation noise is the
    /// #1 reason hand-written Blazor forms get rearchitected. LipiInputBase fixes it
    /// at the family level so all five Phase 2.2 components inherit the right UX.
    /// </summary>
    protected bool _isTouched;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (EditContext is not null)
        {
            EditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
        _previousEditContext = EditContext;
    }

    /// <summary>
    /// Re-subscribes the validation-state listener if the component has been moved
    /// between EditForms (EditContext swap). Subclasses overriding OnParametersSet
    /// MUST call base.OnParametersSet() FIRST to preserve subscription correctness.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(EditContext, _previousEditContext))
        {
            if (_previousEditContext is not null)
                _previousEditContext.OnValidationStateChanged -= HandleValidationStateChanged;

            if (EditContext is not null)
                EditContext.OnValidationStateChanged += HandleValidationStateChanged;

            _previousEditContext = EditContext;
        }

        // Snapshot caller-provided values into resolved fields. We never mutate
        // [Parameter] properties — Blazor re-sets them on every render and our
        // mutations would be wiped.
        _resolvedName         = Name;
        _resolvedLabel        = Label;
        _resolvedAutocomplete = Autocomplete;

        // Run common parameter validation. Subclasses can extend by overriding
        // CollectParameterValidationErrors and adding component-specific checks.
        var errors = CollectParameterValidationErrors();
        if (errors.Count > 0)
        {
            ValidateOrFallback(errors);
        }
    }

    /// <summary>
    /// EditContext.OnValidationStateChanged fires for ALL field validation changes,
    /// not just THIS field's. We filter by reading just our FieldIdentifier's messages,
    /// then short-circuit re-render when the first message hasn't changed.
    ///
    /// CRITICAL: only surfaces the error when _isTouched is true. Until the user has
    /// blurred this field at least once, we suppress validation display — the user is
    /// mid-typing and yelling at them per-keystroke is bad UX. Once blurred, every
    /// subsequent keystroke updates validation immediately.
    /// </summary>
    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        if (EditContext is null) return;

        string? first = null;
        foreach (var msg in EditContext.GetValidationMessages(FieldIdentifier))
        {
            first = msg;
            break;
        }

        // Touched-state gate: silent until first blur. After first blur, keep updating
        // on every validation event (including per-keystroke from TryParseValueFromString).
        var effectiveFirst = _isTouched ? first : null;

        if (!string.Equals(effectiveFirst, _editContextError, System.StringComparison.Ordinal))
        {
            _editContextError = effectiveFirst;
            // Defensive InvokeAsync: some custom validators run on non-UI threads.
            InvokeAsync(StateHasChanged);
        }
    }

    // ==========================================================================
    // BLUR HANDLING — touched-state transition + validation force-fire
    // ==========================================================================

    /// <summary>
    /// Concrete components wire @onblur="HandleBlur" on their input element. This:
    /// (1) marks the field as touched so subsequent validation events surface in the
    /// helper slot; (2) calls EditContext.NotifyFieldChanged to force DataAnnotations
    /// to evaluate this field even if the value didn't change (e.g., empty Required
    /// field that the user tabbed past without typing); (3) triggers the validation
    /// state-changed event which then displays the error if any; (4) calls the virtual
    /// OnFieldBlurred hook for component-specific blur logic (e.g., LipiNumberInput's
    /// raw → formatted display switch).
    ///
    /// For components without a single input element (LipiSelect / LipiCombobox have
    /// an anchor + a dynamically-rendered search input), call MarkFieldAsTouched()
    /// directly from internal close/commit paths. HandleBlur itself is the
    /// FocusEventArgs-bound shape the markup needs.
    /// </summary>
    protected void HandleBlur(FocusEventArgs e)
    {
        MarkFieldAsTouched();
        OnFieldBlurred(e);
    }

    /// <summary>
    /// Args-free entry point for marking the field as touched and firing validation.
    /// Used internally by HandleBlur and externally by components like LipiSelectBase
    /// where the trigger is not a DOM blur event but a logical close-of-interaction
    /// (dropdown close, item selection, escape key, outside click). Idempotent —
    /// calling multiple times is safe; only the first call triggers the catch-up
    /// validation read.
    /// </summary>
    protected void MarkFieldAsTouched()
    {
        var firstBlur = !_isTouched;
        _isTouched = true;

        // Force validation evaluation for fields the user tabbed past or interacted
        // with without changing value. Without this, [Required] validation never
        // fires on never-touched fields.
        if (EditContext is not null)
        {
            EditContext.NotifyFieldChanged(FieldIdentifier);
            // If this is the first touch AND validation has already evaluated this field
            // (e.g., during a prior form submit attempt), the OnValidationStateChanged
            // event may not re-fire. Manually pull the current state and update.
            if (firstBlur)
            {
                string? first = null;
                foreach (var msg in EditContext.GetValidationMessages(FieldIdentifier))
                {
                    first = msg;
                    break;
                }
                if (!string.Equals(first, _editContextError, System.StringComparison.Ordinal))
                {
                    _editContextError = first;
                    StateHasChanged();
                }
            }
        }
    }

    /// <summary>
    /// Virtual hook for concrete components to add component-specific blur logic
    /// (e.g., LipiNumberInput switches from raw display to formatted display on blur,
    /// LipiTextArea may collapse autogrow). Default implementation is empty.
    /// </summary>
    protected virtual void OnFieldBlurred(FocusEventArgs e) { }

    // ==========================================================================
    // DISPOSAL
    // ==========================================================================

    // Note: we do NOT declare a public Dispose() — InputBase<TValue> already implements
    // IDisposable.Dispose() and routes to the virtual Dispose(bool) override below.
    // Declaring our own would either duplicate (CS0114) or "new"-hide the base.

    /// <summary>
    /// Standard IDisposable pattern — subclasses override to add their own cleanup
    /// (e.g., LipiSelectBase's IAsyncDisposable.DisposeAsync calls into this).
    /// Marked `override` because Microsoft's InputBase&lt;TValue&gt; already declares
    /// a virtual Dispose(bool); we extend it rather than hide it. Calls
    /// base.Dispose(disposing) to preserve InputBase's own cleanup (it detaches
    /// its internal EditContext.OnFieldChanged subscription).
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_previousEditContext is not null)
            {
                _previousEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
                _previousEditContext = null;
            }
        }
        base.Dispose(disposing);
    }

    // ==========================================================================
    // ANONYMOUS-ID FALLBACK + ENV-GATED PARAMETER VALIDATION
    // ──────────────────────────────────────────────────────────────────────────
    // Subclasses override:
    //   ComponentIdPrefix  — short id namespace (e.g., "tb" for LipiTextBox)
    //   AnonymousIdPrefix  — fallback prefix when Name is missing in Production
    //                        (defaults to {ComponentIdPrefix}-anon-)
    //   ComponentTypeName  — for log messages (e.g., "LipiTextBox")
    //   CollectParameterValidationErrors — extend with component-specific checks
    // ==========================================================================

    protected string _resolvedName = string.Empty;
    protected string _resolvedLabel = string.Empty;
    protected string? _resolvedAutocomplete;

    private string? _cachedAnonymousId;

    /// <summary>Short id namespace prefix used for the rendered element id (e.g., "tb"
    /// produces id="tb-{name}"). Override per concrete component.</summary>
    protected virtual string ComponentIdPrefix => "lipi";

    /// <summary>Fallback id prefix used in Production when Name parameter is missing/empty.
    /// Default derives from ComponentIdPrefix as "{ComponentIdPrefix}-anon-".</summary>
    protected virtual string AnonymousIdPrefix => $"{ComponentIdPrefix}-anon-";

    /// <summary>Type name surfaced in log messages. Override for cleaner production logs
    /// (e.g., "LipiTextBox" instead of "LipiInputBase`1[System.String]").</summary>
    protected virtual string ComponentTypeName => GetType().Name;

    /// <summary>
    /// Default common-parameter checks. Subclasses override to add component-specific
    /// validation; call <c>base.CollectParameterValidationErrors()</c> first to retain
    /// the common checks, then append.
    /// </summary>
    protected virtual List<string> CollectParameterValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name parameter is required (used to generate id, name, and a11y attributes)");

        if (string.IsNullOrWhiteSpace(Label))
            errors.Add("Label parameter is required (visible text for sighted users + accessible name)");

        if (!string.IsNullOrWhiteSpace(Autocomplete) && !AutocompleteValidator.IsValid(Autocomplete))
            errors.Add(AutocompleteValidator.FormatInvalidMessage(
                ComponentTypeName, "Autocomplete", Autocomplete));

        return errors;
    }

    /// <summary>
    /// Env-gated parameter-error reporter. In Development, throws to surface mistakes
    /// immediately. In Production, logs and applies safe fallback values to keep the
    /// component renderable (never crash a hospital app over a missing label).
    /// </summary>
    protected void ValidateOrFallback(List<string> errors)
    {
        if (errors.Count == 0) return;

        var message = $"{ComponentTypeName} parameter validation failed: " +
                      string.Join("; ", errors);

        if (Env.IsDevelopment())
        {
            throw new System.InvalidOperationException(message);
        }

        Log.LogError("[{Component}] {Message}. Component will render with fallback values.",
            ComponentTypeName, message);

        if (string.IsNullOrWhiteSpace(_resolvedName))
        {
            // Cache once across the component's lifetime so the rendered id is stable
            // between renders. Without caching, every render generates a new guid →
            // id changes → label `for=` no longer matches → broken a11y in the exact
            // case we're already failing.
            _cachedAnonymousId ??= AnonymousIdPrefix + System.Guid.NewGuid().ToString("N")[..8];
            _resolvedName = _cachedAnonymousId;
        }
        if (string.IsNullOrWhiteSpace(_resolvedLabel))
        {
            _resolvedLabel = "(unlabeled)";
        }
        if (!string.IsNullOrWhiteSpace(_resolvedAutocomplete) && !AutocompleteValidator.IsValid(_resolvedAutocomplete))
        {
            _resolvedAutocomplete = "off";
        }
    }

    // ==========================================================================
    // EFFECTIVE ERROR (caller wins; EditContext fills the gap)
    // ==========================================================================

    /// <summary>
    /// The error message used by the state cascade and helper-text source. When the
    /// caller passes Error explicitly, that wins (lets callers override EditContext
    /// with server-side messages or temporarily suppress validation). Otherwise
    /// returns the latest validation message from EditContext (or null if none).
    /// </summary>
    protected string? EffectiveError =>
        !string.IsNullOrWhiteSpace(Error) ? Error : _editContextError;

    // ==========================================================================
    // STATE CASCADE
    // ──────────────────────────────────────────────────────────────────────────
    // ResolvedState reads EffectiveError so EditContext-driven validation flows
    // through automatically. Other state inputs (Disabled, ReadOnly, Warning,
    // Success, Required+empty) remain caller-controlled.
    //
    // IsEmpty is virtual: LipiTextBox uses string.IsNullOrEmpty(CurrentValueAsString),
    // selects use CurrentValue is null. Default implementation works for nullable
    // reference and value types.
    // ==========================================================================

    protected virtual bool IsEmpty => CurrentValue is null;

    protected RequiredVisualStyle EffectiveRequiredStyle =>
        RequiredVisualStyle ?? Defaults.Value.RequiredVisualStyle;

    protected InputState ResolvedState
    {
        get
        {
            if (Disabled)                                    return InputState.Disabled;
            if (ReadOnly)                                    return InputState.ReadOnly;
            if (!string.IsNullOrEmpty(EffectiveError))       return InputState.Error;
            if (!string.IsNullOrEmpty(Success))              return InputState.Success;
            if (!string.IsNullOrEmpty(Warning))              return InputState.Warning;
            // Naming collision note: the [Parameter] property `RequiredVisualStyle` shadows
            // the enum type of the same name in instance member scope. C# resolves the bare
            // identifier to the property, not the type. Fully qualifying the enum reference
            // (LiPi.Web.Components.Shared.RequiredVisualStyle.ApricotTint) avoids the
            // collision. The parameter name stays as-is to preserve the public API surface.
            if (Required && IsEmpty
                && EffectiveRequiredStyle == LiPi.Web.Components.Shared.RequiredVisualStyle.ApricotTint)
                                                             return InputState.RequiredEmpty;
            return InputState.Default;
        }
    }

    protected static string StateCssToken(InputState s) => s switch
    {
        InputState.Disabled       => "disabled",
        InputState.ReadOnly       => "readonly",
        InputState.Error          => "error",
        InputState.Warning        => "warning",
        InputState.Success        => "success",
        InputState.RequiredEmpty  => "required-empty",
        _                         => "default"
    };

    // ==========================================================================
    // RESOLVED IDS + ARIA
    // ==========================================================================

    protected string EffectiveId => $"{ComponentIdPrefix}-{_resolvedName}";
    protected string HelperId => $"{EffectiveId}-helper";

    protected int IconPx => Size switch
    {
        InputSize.Small => 14,
        InputSize.Large => 18,
        _               => 16
    };

    protected string? InlineStyle => Cols.HasValue ? $"grid-column: span {Cols.Value};" : null;

    protected string? AutocompleteAttr => string.IsNullOrWhiteSpace(_resolvedAutocomplete)
        ? null
        : _resolvedAutocomplete;

    // ==========================================================================
    // HELPER TEXT (resolved per state)
    // ──────────────────────────────────────────────────────────────────────────
    // Helper slot resolution priority matches state precedence. Note that
    // Error state reads EffectiveError (caller Error or EditContext message).
    // ==========================================================================

    protected string? HelperText => ResolvedState switch
    {
        InputState.Error   => EffectiveError,
        InputState.Warning => Warning,
        InputState.Success => Success,
        _                  => Helper
    };

    protected string HelperStateClass => ResolvedState switch
    {
        InputState.Error   => "lipi-input-helper-error",
        InputState.Warning => "lipi-input-helper-warning",
        InputState.Success => "lipi-input-helper-success",
        _                  => "lipi-input-helper-default"
    };

    protected bool HasHelperContent => !string.IsNullOrEmpty(HelperText);

    protected string? HelperRole       => ResolvedState == InputState.Error ? "alert" : null;
    protected string  HelperAriaLive   => ResolvedState == InputState.Error ? "assertive" : "polite";
    protected string? AriaInvalidAttr  => ResolvedState == InputState.Error ? "true" : null;

    protected string? AriaDescribedByAttr
    {
        get
        {
            var ids = new List<string>();
            if (HasHelperContent || Defaults.Value.AlwaysReserveHelperSlot) ids.Add(HelperId);
            if (!string.IsNullOrWhiteSpace(AriaDescribedBy)) ids.Add(AriaDescribedBy);
            return ids.Count == 0 ? null : string.Join(" ", ids);
        }
    }

    // ==========================================================================
    // CONFIDENCE PILL
    // ==========================================================================

    protected string? ConfidenceCssClass => LabelConfidence switch
    {
        ConfidenceLevel.Verified     => "verified",
        ConfidenceLevel.SelfReported => "self",
        ConfidenceLevel.Estimated    => "estimated",
        ConfidenceLevel.Unknown      => "unknown",
        _                            => null
    };

    protected string ConfidenceLabelText => LabelConfidence switch
    {
        ConfidenceLevel.Verified     => "Verified",
        ConfidenceLevel.SelfReported => "Self-reported",
        ConfidenceLevel.Estimated    => "Estimated",
        ConfidenceLevel.Unknown      => "Unknown",
        _                            => string.Empty
    };

    protected string? ConfidenceAriaLabel =>
        LabelConfidence.HasValue ? $"Data confidence: {ConfidenceLabelText}" : null;
}
