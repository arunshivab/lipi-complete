// SPEC: docs/00-COMPONENTS/01.3-CompoundField.md (shipping in Batch 9c)
// PHASE: 2.3 (Batch 9a — Phase 2.3 introduction)
// AMEND: docs/CHANGE-LOG.md A19 (pending)
//
// LipiContainerBase is a SIBLING base class to LipiInputBase. It owns the visual
// scaffolding (label rendering, helper slot, asterisk, focus ring CSS hooks,
// state cascade-by-aggregation, _isTouched tracking, segment registration).
//
// Why sibling, not child of LipiInputBase:
//   LipiCompoundField is NOT an InputBase<TValue>. It has no single value to bind
//   to EditContext, no FieldIdentifier, no FormatValueAsString. Lying about that
//   inheritance to reuse the visual primitives would mean overriding EVERY core
//   LipiInputBase method (IsEmpty, ResolvedState's read of EffectiveError,
//   OnInitialized's EditContext subscription, etc.). At that point the IS-A
//   relationship is a fiction.
//
//   Instead: LipiContainerBase has no EditContext, no TValue, no nullable-typed
//   value field. It accepts the same VISUAL parameters (Label, Helper, Required,
//   Disabled, ReadOnly, Size, Class, Cols, RequiredVisualStyle) so the resulting
//   markup is consistent with LipiInputBase descendants, but it computes state
//   by aggregating from registered ICompoundSegment children rather than from
//   EditContext.
//
// Trigger for refactoring this into a shared abstract root with LipiInputBase:
//   when 4+ Lipi*Base classes exist (current: 2). Until then, intentional and
//   bounded duplication of visual primitives.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Base for compound (multi-segment) input components. Owns visual scaffolding
/// and segment lifecycle; defers value semantics entirely to the registered
/// child segments. Each child segment is independently bound to its own model
/// property via @bind-Value on the segment, so segments handle their own
/// EditContext interaction. The parent's job is to render a unified visual
/// envelope (single label, single helper slot, single field-level focus ring,
/// single field-level error state) by aggregating segment state.
/// </summary>
public abstract class LipiContainerBase : ComponentBase, IAsyncDisposable
{
    // ==========================================================================
    // INJECTED SERVICES — same DI shape as LipiInputBase for visual consistency.
    // (Defaults provides AlwaysReserveHelperSlot etc.; Env + Log power
    // env-gated parameter validation via ValidateOrFallback below.)
    // ==========================================================================

    [Inject] protected IOptions<LipiInputDefaults> Defaults { get; set; } = default!;
    [Inject] protected IWebHostEnvironment Env { get; set; } = default!;
    [Inject] protected ILogger<LipiContainerBase> Log { get; set; } = default!;

    /// <summary>Injected so subclasses can use the lipiCompound.isInsideElement
    /// helper for the focusout relatedTarget check (Q2 = Z implementation).</summary>
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // ==========================================================================
    // VISUAL PARAMETERS — mirror LipiInputBase's parameter surface so the
    // resulting markup is consistent. Subclasses don't need to redeclare these.
    // ==========================================================================

    /// <summary>Field name used for id generation. Conventional: kebab-case.
    /// EffectiveId becomes "{ComponentIdPrefix}-{Name}".</summary>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>Visible label text and accessible name for the entire compound
    /// field. Each segment also has its own implicit label via aria-label or
    /// the segment's internal labeling.</summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    /// <summary>Static helper text. Surfaces in the helper slot when no segment
    /// has a pending error to display.</summary>
    [Parameter] public string? Helper { get; set; }

    /// <summary>Caller-set error. When non-empty, wins over per-segment error
    /// aggregation in the helper slot. Use for cross-segment validation errors
    /// the segments themselves can't express (e.g., "Mobile and WhatsApp must
    /// not be the same").</summary>
    [Parameter] public string? Error { get; set; }

    /// <summary>Caller-set success message.</summary>
    [Parameter] public string? Success { get; set; }

    /// <summary>Caller-set warning message.</summary>
    [Parameter] public string? Warning { get; set; }

    /// <summary>True if the field is required overall. Note: individual segments
    /// also carry their own Required parameters. The field-level asterisk
    /// renders if Required is true OR if any registered segment is required —
    /// whichever convention the caller prefers. Locked Q-decision: asterisk
    /// + ARIA + tooltip ALWAYS render when Required at field level.</summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>Disable the entire compound (propagates to all segments via
    /// the parent reference; segments can also have their own Disabled).</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Read-only the entire compound (propagates similarly).</summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>Field size — applies to all segments uniformly.</summary>
    [Parameter] public InputSize Size { get; set; } = InputSize.Medium;

    /// <summary>Append-only CSS class. LAYOUT UTILITIES ONLY.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Optional grid column span (for grid-layout forms).</summary>
    [Parameter] public int? Cols { get; set; }

    /// <summary>Per-instance override of the Required visual style.</summary>
    [Parameter] public RequiredVisualStyle? RequiredVisualStyle { get; set; }

    /// <summary>Optional confidence level pill on the label (matches
    /// LipiInputBase pattern).</summary>
    [Parameter] public ConfidenceLevel? LabelConfidence { get; set; }

    /// <summary>External aria-describedby IDs to merge with the helper slot's id.</summary>
    [Parameter] public string? AriaDescribedBy { get; set; }

    /// <summary>Optional leading icon at the field level.</summary>
    [Parameter] public string? Icon { get; set; }

    /// <summary>Child segments are rendered inside the .lipi-compound-field-bar.
    /// Caller composes via &lt;SelectSegment&gt; / &lt;TextSegment&gt; child elements.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // ==========================================================================
    // SEGMENT REGISTRATION — segments call RegisterSegment on init, and
    // UnregisterSegment on dispose. Registration order is preserved (List<>),
    // matching DOM tab order, used for AdvanceFocusFromAsync (Q4 auto-advance).
    // ==========================================================================

    private readonly List<ICompoundSegment> _segments = new();

    /// <summary>Read-only view of registered segments in DOM order.</summary>
    protected IReadOnlyList<ICompoundSegment> Segments => _segments;

    public void RegisterSegment(ICompoundSegment segment)
    {
        if (segment is null) return;
        if (!_segments.Contains(segment))
        {
            _segments.Add(segment);
        }
    }

    public void UnregisterSegment(ICompoundSegment segment)
    {
        if (segment is null) return;
        _segments.Remove(segment);
    }

    /// <summary>Called by segments when their state changes (touched, value,
    /// validation). Triggers a parent re-render so the aggregated CSS classes
    /// (focus ring, field-level error outline) and helper slot stay current.</summary>
    public void NotifyStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    /// <summary>Auto-advance focus to the segment after the given segment.
    /// If the given segment is the last one, no-op (Q4 locked: don't try to
    /// programmatically move to the next form field — Tab handles that).</summary>
    public async ValueTask AdvanceFocusFromAsync(ICompoundSegment from)
    {
        var idx = _segments.IndexOf(from);
        if (idx < 0 || idx >= _segments.Count - 1) return;
        var next = _segments[idx + 1];
        await next.FocusAsync();
    }

    // ==========================================================================
    // TOUCHED-STATE — Q2 locked = Z (focus-leaves-compound). Set true when
    // focus actually exits the compound's outer wrapper, computed via JS
    // helper lipiCompound.isInsideElement against e.RelatedTarget.
    // ==========================================================================

    protected bool _isTouched;
    protected ElementReference _wrapperRef;
    private DotNetObjectReference<LipiContainerBase>? _dotNetRef;
    private bool _focusOutHandlerAttached;

    /// <summary>
    /// JSInvokable callback fired by the JS-side focusout listener ONLY when focus
    /// has actually left the compound's wrapper element (i.e., relatedTarget is not
    /// a descendant of _wrapperRef). The JS side handles the DOM contains-check
    /// because Blazor's FocusEventArgs does NOT surface RelatedTarget — that's a
    /// DOM web-API property, not exposed in the .NET event args.
    ///
    /// This implements the Q2=Z touched-state semantic: validation surfaces
    /// only when focus leaves the entire compound, not when it moves between
    /// segments inside the compound.
    /// </summary>
    [JSInvokable]
    public void OnFocusLeftCompound()
    {
        if (_isTouched) return;

        _isTouched = true;
        // Tell each segment to also mark touched — once focus leaves the whole
        // compound, every segment should validate its current value even if user
        // never visited it (Q2=Z semantics: visit-or-skip doesn't matter, leaving-
        // the-field does).
        foreach (var seg in _segments)
        {
            if (seg is LipiInputBaseTouchable t) t.MarkTouchedFromContainer();
        }
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Attach the JS-side focusout listener. Called from OnAfterRenderAsync(firstRender:true)
    /// in the concrete component (LipiCompoundField). Idempotent — safe to call repeatedly,
    /// JS side defends against double-attachment.
    /// </summary>
    protected async Task AttachFocusOutHandlerAsync()
    {
        if (_focusOutHandlerAttached) return;
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("lipiCompound.attachFocusOut", _wrapperRef, _dotNetRef);
            _focusOutHandlerAttached = true;
        }
        catch (Exception ex)
        {
            // JS interop may fail during prerender — log and proceed without
            // touched-state aggregation. The segments still have their own
            // per-segment touched-state from their LipiInputBase HandleBlur.
            Log.LogWarning(ex, "[LipiContainerBase] lipiCompound.attachFocusOut failed.");
        }
    }

    /// <summary>Detach the JS-side focusout listener. Called from DisposeAsync.</summary>
    protected async Task DetachFocusOutHandlerAsync()
    {
        if (!_focusOutHandlerAttached) return;
        try
        {
            await JS.InvokeVoidAsync("lipiCompound.detachFocusOut", _wrapperRef);
        }
        catch
        {
            // Circuit may be closing; best-effort cleanup.
        }
        _focusOutHandlerAttached = false;
    }

    // ==========================================================================
    // STATE AGGREGATION — what the field-level visuals reflect.
    // ==========================================================================

    /// <summary>True if any registered segment is touched OR the compound's
    /// own focus-leaves event has fired. Used to gate validation message
    /// display in the helper slot.</summary>
    protected bool AnyTouched => _isTouched || _segments.Any(s => s.IsTouched);

    /// <summary>True if any registered segment has an error AND we're past the
    /// touched threshold. Drives field-level red outline.</summary>
    protected bool AnyErrored => AnyTouched && _segments.Any(s => s.HasError);

    /// <summary>The first errored segment's message, in registration order
    /// (left-to-right DOM order). Surfaces in the parent's single helper slot.
    /// Falls back to caller-set Error / Warning / Success / Helper per the
    /// standard cascade.</summary>
    protected string? AggregatedHelperText
    {
        get
        {
            // Caller-set Error always wins (allows cross-segment validation
            // messages from the form layer).
            if (!string.IsNullOrEmpty(Error)) return Error;
            // First failing segment.
            if (AnyTouched)
            {
                var firstError = _segments.FirstOrDefault(s => s.HasError);
                if (firstError is not null && !string.IsNullOrEmpty(firstError.ErrorMessage))
                    return firstError.ErrorMessage;
            }
            if (!string.IsNullOrEmpty(Warning)) return Warning;
            if (!string.IsNullOrEmpty(Success)) return Success;
            return Helper;
        }
    }

    protected InputState ResolvedState
    {
        get
        {
            if (Disabled)                                   return InputState.Disabled;
            if (ReadOnly)                                   return InputState.ReadOnly;
            if (!string.IsNullOrEmpty(Error) || AnyErrored) return InputState.Error;
            if (!string.IsNullOrEmpty(Success))             return InputState.Success;
            if (!string.IsNullOrEmpty(Warning))             return InputState.Warning;
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

    protected RequiredVisualStyle EffectiveRequiredStyle =>
        RequiredVisualStyle ?? Defaults.Value.RequiredVisualStyle;

    // ==========================================================================
    // RESOLVED ID + LABEL HELPERS (mirror LipiInputBase pattern)
    // ==========================================================================

    protected virtual string ComponentIdPrefix => "cf";  // "compound field"
    protected virtual string AnonymousIdPrefix => $"{ComponentIdPrefix}-anon-";

    protected string EffectiveId   => $"{ComponentIdPrefix}-{_resolvedName}";
    protected string HelperId      => $"{EffectiveId}-helper";

    protected string  _resolvedName  = string.Empty;
    protected string  _resolvedLabel = string.Empty;
    private string?   _cachedAnonymousId;

    protected int IconPx => Size switch
    {
        InputSize.Small => 14,
        InputSize.Large => 18,
        _               => 16
    };

    protected string? InlineStyle => Cols.HasValue ? $"grid-column: span {Cols.Value};" : null;

    protected string CssClasses
    {
        get
        {
            var classes = new List<string>
            {
                "lipi-compound-field",
                $"lipi-input-{Size.ToString().ToLowerInvariant()}",
                $"lipi-input-state-{StateCssToken(ResolvedState)}"
            };
            if (Required && _segments.All(s => !s.IsTouched) && !AnyErrored
                && EffectiveRequiredStyle == LiPi.Web.Components.Shared.RequiredVisualStyle.ApricotTint
                && ResolvedState == InputState.Default)
            {
                // Apricot tint at field level when overall Required and nothing
                // touched yet. Per-segment apricot tint is also driven by each
                // segment's own LipiInputBase ResolvedState — this is just the
                // field-wide hint.
                classes.Add("lipi-input-state-required-empty");
            }
            return string.Join(" ", classes);
        }
    }

    // Helper slot rendering helpers — same shape as LipiInputBase's exposes.
    protected string HelperStateClass => ResolvedState switch
    {
        InputState.Error   => "lipi-input-helper-error",
        InputState.Warning => "lipi-input-helper-warning",
        InputState.Success => "lipi-input-helper-success",
        _                  => "lipi-input-helper-default"
    };

    protected bool HasHelperContent      => !string.IsNullOrEmpty(AggregatedHelperText);
    protected string? HelperRole         => ResolvedState == InputState.Error ? "alert" : null;
    protected string  HelperAriaLive     => ResolvedState == InputState.Error ? "assertive" : "polite";
    protected string? AriaInvalidAttr    => ResolvedState == InputState.Error ? "true"   : null;

    protected string? AriaDescribedByAttr
    {
        get
        {
            var ids = new List<string>();
            if (HasHelperContent || Defaults.Value.AlwaysReserveHelperSlot) ids.Add(HelperId);
            if (!string.IsNullOrWhiteSpace(AriaDescribedBy))                 ids.Add(AriaDescribedBy);
            return ids.Count == 0 ? null : string.Join(" ", ids);
        }
    }

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

    // ==========================================================================
    // PARAMETER VALIDATION (env-gated, mirrors LipiInputBase pattern)
    // ==========================================================================

    protected override void OnParametersSet()
    {
        _resolvedName  = Name;
        _resolvedLabel = Label;

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name parameter is required (used for id generation).");
        if (string.IsNullOrWhiteSpace(Label))
            errors.Add("Label parameter is required (visible field label and aria-label).");

        if (errors.Count == 0) return;

        var message = $"{GetType().Name} parameter validation failed: " +
                      string.Join("; ", errors);

        if (Env.IsDevelopment())
        {
            throw new InvalidOperationException(message);
        }
        else
        {
            Log.LogError("[{Component}] {Message} Component renders with fallback values.",
                GetType().Name, message);

            if (string.IsNullOrWhiteSpace(_resolvedName))
            {
                _cachedAnonymousId ??= AnonymousIdPrefix + Guid.NewGuid().ToString("N")[..8];
                _resolvedName = _cachedAnonymousId;
            }
            if (string.IsNullOrWhiteSpace(_resolvedLabel))
            {
                _resolvedLabel = "(unlabeled)";
            }
        }
    }

    // ==========================================================================
    // DISPOSAL
    // ==========================================================================

    public async ValueTask DisposeAsync()
    {
        await DetachFocusOutHandlerAsync();
        _dotNetRef?.Dispose();
        _segments.Clear();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Optional contract for segments that derive from LipiInputBase and want to
/// receive a "container said focus left, please mark touched" signal. Allows
/// the container's OnFocusLeftCompound JS callback to force-validate every
/// segment, not just the one that was focused last. Implementing this is
/// opt-in; segments without it just keep their own per-segment touched state.
/// </summary>
public interface LipiInputBaseTouchable
{
    void MarkTouchedFromContainer();
}
