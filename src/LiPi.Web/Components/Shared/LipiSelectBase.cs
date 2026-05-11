// SPEC: Phase 2.2 Sub-step Batch 5 — abstract base for LipiSelect / LipiCombobox.
// Holds the shared state machine: search, dropdown open/close, keyboard navigation,
// virtualization toggle, pinned-options sorting, JS positioning interop.
//
// Phase 2.2.5 Batch 8c: derives from LipiInputBase<TValue> instead of InputBase<TValue>.
// 3-level hierarchy:
//   InputBase<TValue>                    (Microsoft framework, IDisposable)
//     └── LipiInputBase<TValue>          (Phase 2.2.5 base — IDisposable, EditContext +
//                                         18 common params + state cascade + touched-state
//                                         pattern)
//           └── LipiSelectBase<TValue, TItem>  (this class — also IAsyncDisposable for
//                                               JS handler cleanup; inherits IDisposable
//                                               from LipiInputBase for EditContext detach)
//                 ├── LipiSelect<TValue>
//                 └── LipiCombobox<TValue, TItem>
//
// LipiSelect<TValue>           : LipiSelectBase<TValue, TValue>     — identity selectors
// LipiCombobox<TValue, TItem>  : LipiSelectBase<TValue, TItem>      — caller-provided selectors
//
// The component templates (.razor files) own the markup and call into protected members
// of this base for state changes. Public API surface is on the concrete subclasses.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace LiPi.Web.Components.Shared;

public abstract class LipiSelectBase<TValue, TItem> : LipiInputBase<TValue>, IAsyncDisposable
{
    // ==========================================================================
    // INJECTED SERVICES
    // ──────────────────────────────────────────────────────────────────────────
    // Inherited from LipiInputBase<TValue>:
    //   IOptions<LipiInputDefaults> Defaults, IWebHostEnvironment Env,
    //   ILogger<LipiInputBase<TValue>> Log
    // Component-specific:
    //   IJSRuntime JS — for select positioning, keyboard handlers, click-outside detection
    // ==========================================================================

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // ==========================================================================
    // CALLER PARAMETERS — component-specific only (common ones inherited from base)
    // ──────────────────────────────────────────────────────────────────────────
    // Inherited from LipiInputBase<TValue>:
    //   Name, Label, Placeholder, Helper, Error, Warning, Success, Required,
    //   Disabled, ReadOnly, Autocomplete, Size, Icon, AriaDescribedBy,
    //   RequiredVisualStyle, LabelConfidence, Cols, Class
    // Inherited from InputBase<TValue>:
    //   Value, ValueChanged, ValueExpression, DisplayName
    // ==========================================================================

    /// <summary>The full list of options. IReadOnlyList (not IEnumerable) so .Count is O(1)
    /// for virtualization auto-detection. Caller materializes upstream.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    /// <summary>Whether to render the search input box at the top of the dropdown.
    /// Default true. Set to false for short fixed lists (Gender, Marital Status) where
    /// search is unnecessary UI weight. Dropdown remains scrollable either way.</summary>
    [Parameter] public bool Searchable { get; set; } = true;

    /// <summary>Search algorithm. Default Contains (substring match anywhere in label).
    /// StartsWith is appropriate for ordered lists where users naturally type the prefix
    /// (e.g., country names, district names).</summary>
    [Parameter] public SelectSearchMode SearchMode { get; set; } = SelectSearchMode.Contains;

    /// <summary>Values that should appear pinned at the top of the dropdown above a divider.
    /// Order is preserved from this collection (not sorted alphabetically). Items not in
    /// this list appear below the divider in their original Items order.</summary>
    [Parameter] public IEnumerable<TValue>? PinnedValues { get; set; }

    /// <summary>Heading text shown above the pinned section (e.g., "Recently used").
    /// Null = no heading, just the divider line.</summary>
    [Parameter] public string? PinnedHeading { get; set; }

    /// <summary>Heading text shown above the unpinned section (e.g., "All countries").
    /// Null = no heading. Only rendered when PinnedValues is non-empty AND there are
    /// unpinned items to show.</summary>
    [Parameter] public string? UnpinnedHeading { get; set; }

    /// <summary>When true, search text that doesn't match any option is accepted as the
    /// value verbatim. Use for "Other" or freeform fields. Default false.</summary>
    [Parameter] public bool AllowFreeText { get; set; }

    /// <summary>Override virtualization behavior. Null = auto (virtualize when
    /// Items.Count > VirtualizationThreshold = 50). True/false forces explicit choice.</summary>
    [Parameter] public bool? UseVirtualization { get; set; }

    // ── Phase 2.5.5 — LabelPosition cross-family retrofit ─────────────────────────────────

    /// <summary>Position of the label relative to the input field.
    /// Top (default) — label above. Left — label left, saves vertical space.
    /// Right — label right (RTL scenarios). Bottom — label below (caption style).
    /// When Left or Right, use AriaLabel or ensure Label has content.</summary>
    [Parameter] public InputLabelPosition LabelPosition { get; set; }
        = InputLabelPosition.Top;

    /// <summary>Accessible name when Label is empty (e.g., inline-left layout).
    /// Required when Label="" to satisfy WCAG 2.1 SC 4.1.2.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    // ==========================================================================
    // ABSTRACT — implemented by LipiSelect (identity) and LipiCombobox (caller-provided)
    // ==========================================================================

    /// <summary>Extract the bound value from an option item. For LipiSelect this is
    /// identity (item IS the value). For LipiCombobox the caller provides ValueSelector.</summary>
    protected abstract TValue? ItemValue(TItem item);

    /// <summary>Extract the display label from an option item. For LipiSelect this is
    /// item.ToString() (or culture-aware formatting for primitives). For LipiCombobox
    /// the caller provides LabelSelector.</summary>
    protected abstract string ItemLabel(TItem item);

    /// <summary>Render an option row's content. LipiSelect uses default text rendering.
    /// LipiCombobox uses the caller-provided ItemTemplate when supplied, otherwise falls
    /// back to ItemLabel.</summary>
    protected abstract RenderFragment<TItem> ItemContent { get; }

    // ==========================================================================
    // PHASE 2.5.5 — resolved label position class (protected for subclass markup)
    // ==========================================================================

    /// <summary>CSS class emitted on the wrapper for non-Top LabelPosition values.
    /// Protected so LipiSelect.razor and LipiCombobox.razor markup can bind to it.
    /// Computed in OnParametersSet from LabelPosition each render.</summary>
    protected string _resolvedLabelPositionClass = string.Empty;

    // ==========================================================================
    // STATE MACHINE
    // ==========================================================================

    /// <summary>Below this many items, render all options without Virtualize. Above,
    /// virtualize. Tunable based on field testing — clinical dropdowns rarely exceed
    /// 200 items (countries) or 700 (districts). Most are under 50.</summary>
    protected const int VirtualizationThreshold = 50;

    // _resolvedName, _resolvedLabel, _resolvedAutocomplete, _cachedAnonymousId now
    // inherited from LipiInputBase. Anonymous-id prefix override below.

    protected bool _isOpen;
    protected bool _wasOpen; // tracks transition for focus-on-open in OnAfterRenderAsync
    protected string _searchText = string.Empty;
    protected int _highlightedIndex = -1;
    protected ElementReference _anchorRef;
    protected ElementReference _searchInputRef;
    protected DotNetObjectReference<LipiSelectBase<TValue, TItem>>? _dotNetRef;
    protected bool _jsHandlersAttached;

    // ==========================================================================
    // INPUTBASE OVERRIDE
    // ==========================================================================

    /// <summary>
    /// <summary>
    /// Parses the user's typed search/free-text into a TValue. For LipiSelect/LipiCombobox,
    /// successful parsing means: (a) the text matches an option's label (or value), or
    /// (b) AllowFreeText is true and BindConverter can parse the text into TValue.
    /// </summary>
    /// <remarks>
    /// Signature matches Microsoft's InputBase&lt;TValue&gt;.TryParseValueFromString exactly,
    /// including the [MaybeNullWhen(false)] / [NotNullWhen(false)] attributes. Without these,
    /// CS8765 fires (nullability mismatch with overridden member).
    /// </remarks>
    protected override bool TryParseValueFromString(
        string? value,
        [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue result,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default!;
            validationErrorMessage = null;
            return true; // empty = clear (valid for nullable; non-nullable parse error happens at FormatValueAsString)
        }

        // Try to match an existing option by label
        foreach (var item in Items)
        {
            if (string.Equals(ItemLabel(item), value, StringComparison.OrdinalIgnoreCase))
            {
                // ItemValue returns TValue? (the abstract sig allows null values for
                // nullable TValue contexts). The override contract is [MaybeNullWhen(false)]
                // which means non-null when returning true — but for LipiSelect<TValue?>
                // a null option IS valid input, so suppress the warning.
                result = ItemValue(item)!;
                validationErrorMessage = null;
                return true;
            }
        }

        // Try to match by value's string representation (for cases where user types the value directly)
        foreach (var item in Items)
        {
            var itemValue = ItemValue(item);
            if (itemValue?.ToString() == value)
            {
                result = itemValue!;
                validationErrorMessage = null;
                return true;
            }
        }

        // No match — accept as free text if allowed
        if (AllowFreeText && BindConverter.TryConvertTo<TValue>(value, CultureInfo.CurrentCulture, out var freeText))
        {
            result = freeText;
            validationErrorMessage = null;
            return true;
        }

        result = default!;
        validationErrorMessage = $"\"{value}\" is not a valid option for {DisplayName ?? FieldIdentifier.FieldName}.";
        return false;
    }

    /// <summary>
    /// Formats the bound value into the display string shown in the closed-state input.
    /// Looks up the matching option's label; falls back to the value's ToString() if no
    /// match (e.g., AllowFreeText was used to set a value not in the list).
    /// </summary>
    protected override string? FormatValueAsString(TValue? value)
    {
        if (value is null) return string.Empty;

        foreach (var item in Items)
        {
            if (EqualityComparer<TValue?>.Default.Equals(ItemValue(item), value))
            {
                return ItemLabel(item);
            }
        }

        // Free-text or stale value — display the raw string representation
        return value.ToString() ?? string.Empty;
    }

    // ==========================================================================
    // BASE CLASS OVERRIDES
    // ==========================================================================

    /// <summary>Component id namespace — produces id="sel-{Name}" so legacy stylesheets
    /// targeting #sel-* selectors continue to match. Anonymous-id prefix follows the
    /// same convention via AnonymousIdPrefix override on the base.</summary>
    protected override string ComponentIdPrefix => "sel";

    /// <summary>Type name for log messages — uses runtime GetType().Name so subclasses
    /// (LipiSelect / LipiCombobox) get accurate component identification in logs without
    /// each having to override.</summary>
    protected override string ComponentTypeName => GetType().Name;

    /// <summary>Append the LipiSelectBase-specific Items-required check on top of base
    /// validation (Name + Label + Autocomplete).</summary>
    protected override List<string> CollectParameterValidationErrors()
    {
        var errors = base.CollectParameterValidationErrors();

        // Phase 2.5.5 — Label/AriaLabel filter (same pattern as Phase 2.5 selection family).
        // When AriaLabel provides the accessible name, suppress base class "Label parameter
        // is required" error. Gating rule: LabelPosition + AriaLabel ship together.
        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            errors.RemoveAll(e => e.StartsWith("Label parameter is required"));
        }

        if (string.IsNullOrWhiteSpace(_resolvedLabel) && string.IsNullOrWhiteSpace(AriaLabel))
        {
            errors.Add(
                "AriaLabel is required when Label is empty " +
                "(accessible name for assistive tech).");
        }

        if (Items is null)
        {
            errors.Add("Items parameter is required (the options to render in the dropdown)");
        }

        return errors;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Phase 2.5.5 — resolve label position CSS class each render.
        _resolvedLabelPositionClass = LabelPosition switch
        {
            InputLabelPosition.Left   => "lipi-input-label-left",
            InputLabelPosition.Right  => "lipi-input-label-right",
            InputLabelPosition.Bottom => "lipi-input-label-bottom",
            _                         => string.Empty
        };
    }

    // ==========================================================================
    // FILTERED + SORTED OPTIONS
    // ==========================================================================

    /// <summary>
    /// Computed property returning the option list after applying search filter and pin
    /// ordering. Pinned items (in PinnedValues order) come first, followed by unpinned items
    /// in original Items order. Both sections are filtered by search text.
    ///
    /// Returns concrete <c>List&lt;TItem&gt;</c> (not <c>IReadOnlyList</c>) for two reasons:
    /// (1) Blazor's &lt;Virtualize&gt; requires <c>ICollection&lt;TItem&gt;</c> for its
    /// Items parameter, which IReadOnlyList does not extend; (2) the markup calls
    /// <c>.IndexOf(item)</c> which is on List/IList but not IReadOnlyList.
    /// </summary>
    protected (List<TItem> Pinned, List<TItem> Unpinned) GetFilteredOptions()
    {
        var pinnedSet = PinnedValues?.ToHashSet(EqualityComparer<TValue>.Default!);
        var pinned = new List<TItem>();
        var unpinned = new List<TItem>();

        foreach (var item in Items)
        {
            if (!MatchesSearch(item)) continue;
            var v = ItemValue(item);
            if (pinnedSet is not null && v is not null && pinnedSet.Contains(v))
            {
                pinned.Add(item);
            }
            else
            {
                unpinned.Add(item);
            }
        }

        // Reorder pinned list to match PinnedValues order
        if (pinnedSet is not null && pinned.Count > 1 && PinnedValues is not null)
        {
            var pinnedOrder = PinnedValues.ToList();
            pinned.Sort((a, b) =>
            {
                var ia = pinnedOrder.IndexOf(ItemValue(a)!);
                var ib = pinnedOrder.IndexOf(ItemValue(b)!);
                return ia.CompareTo(ib);
            });
        }

        return (pinned, unpinned);
    }

    private bool MatchesSearch(TItem item)
    {
        if (string.IsNullOrEmpty(_searchText)) return true;
        var label = ItemLabel(item);
        return SearchMode switch
        {
            SelectSearchMode.StartsWith => label.StartsWith(_searchText, StringComparison.OrdinalIgnoreCase),
            _                           => label.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
        };
    }

    /// <summary>Whether to use Blazor's &lt;Virtualize&gt; for the dropdown rendering.
    /// Honors UseVirtualization parameter; falls back to threshold-based auto-detect.</summary>
    protected bool ShouldVirtualize =>
        UseVirtualization ?? (Items.Count > VirtualizationThreshold);

    // ==========================================================================
    // STATE TRANSITIONS — called from .razor markup events
    // ==========================================================================

    protected async Task OpenDropdownAsync()
    {
        if (Disabled || ReadOnly || _isOpen) return;
        _isOpen = true;
        _searchText = string.Empty;
        _highlightedIndex = -1;
        await EnsureJsHandlersAsync();
        await PositionDropdownAsync();
    }

    protected async Task CloseDropdownAsync(bool commitSelection = false)
    {
        if (!_isOpen) return;
        _isOpen = false;

        if (commitSelection && _highlightedIndex >= 0)
        {
            var (pinned, unpinned) = GetFilteredOptions();
            var combined = pinned.Concat(unpinned).ToList();
            if (_highlightedIndex < combined.Count)
            {
                SelectItem(combined[_highlightedIndex]);
            }
        }

        _searchText = string.Empty;
        _highlightedIndex = -1;
        await DetachJsHandlersAsync();

        // Closing the dropdown counts as the user finishing their interaction with this
        // field — equivalent to a blur on a text input. Mark touched so validation
        // surfaces in the helper slot. Idempotent; safe to call on every close.
        MarkFieldAsTouched();
    }

    protected void SelectItem(TItem item)
    {
        var v = ItemValue(item);
        CurrentValue = v;
        _isOpen = false;
        _searchText = string.Empty;
        _highlightedIndex = -1;
        _ = DetachJsHandlersAsync();

        // Selecting an item is also a definitive interaction — touched.
        MarkFieldAsTouched();
    }

    /// <summary>
    /// Anchor blur handler — wired via @onblur on the anchor div in LipiSelect.razor /
    /// LipiCombobox.razor. Handles the tab-past case: user tabs through a closed,
    /// never-opened select. The other touched-state triggers (CloseDropdownAsync,
    /// SelectItem) won't fire because the dropdown was never opened.
    ///
    /// Critical guard: if _isOpen is true, the blur is moving focus into the search
    /// input or option list (which are children of the dropdown panel, not the anchor).
    /// That's mid-interaction, NOT a leave-the-field event. Skip marking touched.
    /// CloseDropdownAsync (triggered by outside-click or Escape) will handle the
    /// touched-state when the user actually finishes interacting.
    /// </summary>
    protected void HandleAnchorBlur(FocusEventArgs e)
    {
        if (_isOpen) return; // mid-interaction — let close paths handle touched-state
        MarkFieldAsTouched();
    }

    protected void HandleSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _highlightedIndex = string.IsNullOrEmpty(_searchText) ? -1 : 0;
    }

    /// <summary>
    /// Single keyboard handler bound to BOTH the anchor (via tabindex=0) AND the search input
    /// (when Searchable). Routes by _isOpen state:
    ///   - Closed: ArrowDown/ArrowUp/Enter/Space open the dropdown.
    ///   - Open: ArrowDown/ArrowUp move highlight; Enter selects highlighted (or commits free-text);
    ///           Escape closes; Tab closes (commits highlighted if any, lets browser move focus).
    ///
    /// Why a single handler: focus moves between anchor (closed state) and search input (open state).
    /// Each element has @onkeydown="HandleKeyDown" — only the focused element's handler fires.
    /// The internal _isOpen branch ensures correct behavior regardless of which element fired the event.
    ///
    /// No preventDefault needed:
    ///   - Arrow keys on a focused input/anchor don't scroll the page (browser default for focused
    ///     interactive elements is no-op or cursor movement).
    ///   - Enter would submit the parent EditForm, but the search input uses form="lipi-select-orphan"
    ///     to disassociate from any form (HTML5 standard). Enter has no default action.
    ///   - Tab's natural focus movement is preserved.
    /// </summary>
    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled || ReadOnly) return;

        if (!_isOpen)
        {
            // Closed-state keys: open the dropdown
            switch (e.Key)
            {
                case "ArrowDown":
                case "ArrowUp":
                case "Enter":
                case " ":
                    await OpenDropdownAsync();
                    break;
                // Other keys ignored when closed
            }
            return;
        }

        // Open-state keys: navigate, select, close
        switch (e.Key)
        {
            case "ArrowDown":
                MoveHighlight(1);
                break;
            case "ArrowUp":
                MoveHighlight(-1);
                break;
            case "Enter":
                if (_highlightedIndex >= 0)
                {
                    var (pinned, unpinned) = GetFilteredOptions();
                    var combined = pinned.Concat(unpinned).ToList();
                    if (_highlightedIndex < combined.Count)
                    {
                        SelectItem(combined[_highlightedIndex]);
                    }
                }
                else if (AllowFreeText && !string.IsNullOrWhiteSpace(_searchText))
                {
                    // No highlighted option but search has text and free-text is allowed: commit it
                    await CommitFreeTextAsync();
                }
                break;
            case "Escape":
                await CloseDropdownAsync();
                break;
            case "Tab":
                // Commit highlighted (if any) but DON'T preventDefault — let browser move focus naturally
                await CloseDropdownAsync(commitSelection: _highlightedIndex >= 0);
                break;
        }
    }

    /// <summary>
    /// On dropdown-open transition (closed → open), focus the keyboard target so arrow keys
    /// route to HandleKeyDown via the focused element rather than reaching the page body
    /// (which would trigger page scroll). For searchable selects, focus the search input.
    /// For non-searchable selects, focus the anchor explicitly — relying on click-to-focus
    /// for the anchor div is browser-specific (most modern browsers do focus tabindex=0 divs
    /// on click, but Blazor's async render cycle can cause focus drift between click and
    /// render-completion). Explicit FocusAsync guarantees the keyboard handler receives
    /// subsequent arrow-key events.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Call base implementation FIRST so any LipiInputBase OnAfterRender work runs.
        // Currently no-op in base, but reserved for future use (e.g., auto-focus-on-error).
        await base.OnAfterRenderAsync(firstRender);

        if (_isOpen && !_wasOpen)
        {
            _wasOpen = true;
            try
            {
                if (Searchable)
                {
                    await _searchInputRef.FocusAsync();
                }
                else
                {
                    // Explicit anchor focus — defensive against browser click-to-focus
                    // variations and Blazor render-cycle focus drift.
                    await _anchorRef.FocusAsync();
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "[{Component}] FocusAsync on dropdown open failed",
                    GetType().Name);
            }
        }
        else if (!_isOpen && _wasOpen)
        {
            _wasOpen = false;
        }
    }

    private void MoveHighlight(int delta)
    {
        var (pinned, unpinned) = GetFilteredOptions();
        var total = pinned.Count + unpinned.Count;
        if (total == 0) { _highlightedIndex = -1; return; }
        _highlightedIndex = ((_highlightedIndex + delta) % total + total) % total;
    }

    /// <summary>Commit search text as free-text value (called from "Add as new" button).</summary>
    protected async Task CommitFreeTextAsync()
    {
        if (!AllowFreeText || string.IsNullOrWhiteSpace(_searchText)) return;
        CurrentValueAsString = _searchText;
        await CloseDropdownAsync();
    }

    // ==========================================================================
    // JS INTEROP — dropdown positioning + outside-click + scroll-reposition
    // ==========================================================================

    private async Task EnsureJsHandlersAsync()
    {
        if (_jsHandlersAttached) return;
        _dotNetRef ??= DotNetObjectReference.Create(this);
        try
        {
            await JS.InvokeVoidAsync("lipiInput.attachSelectHandlers", _anchorRef, _dotNetRef, EffectiveDropdownId);
            _jsHandlersAttached = true;
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "[{Component}] attachSelectHandlers failed (id={Id})",
                GetType().Name, EffectiveId);
        }
    }

    private async Task DetachJsHandlersAsync()
    {
        if (!_jsHandlersAttached) return;
        try
        {
            await JS.InvokeVoidAsync("lipiInput.detachSelectHandlers", EffectiveDropdownId);
        }
        catch
        {
            // Circuit may be closing; best-effort cleanup
        }
        _jsHandlersAttached = false;
    }

    protected async Task PositionDropdownAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("lipiInput.positionDropdown", _anchorRef, EffectiveDropdownId);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "[{Component}] positionDropdown failed (id={Id})",
                GetType().Name, EffectiveId);
        }
    }

    /// <summary>Called from JS when user clicks outside the anchor or dropdown.</summary>
    [JSInvokable]
    public async Task OnOutsideClick()
    {
        if (_isOpen)
        {
            await CloseDropdownAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>Called from JS on viewport/ancestor scroll. We reposition rather than close,
    /// per Material UI / Carbon convention. Closing on scroll is jarring for HIS contexts
    /// where users may be scrolling within scrollable modals or accessibility scroll modes.</summary>
    [JSInvokable]
    public async Task OnAncestorScroll()
    {
        if (_isOpen)
        {
            await PositionDropdownAsync();
        }
    }

    /// <summary>
    /// IAsyncDisposable.DisposeAsync — async JS handler cleanup, then synchronous
    /// EditContext detach via base.Dispose(true). The 3-level disposal chain:
    ///   LipiSelectBase.DisposeAsync (this)
    ///     → DetachJsHandlersAsync (async JS interop)
    ///     → _dotNetRef.Dispose (sync)
    ///     → base.Dispose(true)
    ///         → LipiInputBase.Dispose(bool) (EditContext.OnValidationStateChanged detach)
    ///         → InputBase.Dispose(bool) (Microsoft's internal cleanup)
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DetachJsHandlersAsync();
        _dotNetRef?.Dispose();
        // Synchronous part of disposal: EditContext detach via LipiInputBase.Dispose(bool).
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // ==========================================================================
    // RESOLVED PROPERTIES — component-specific only (common ones inherited from base)
    // ==========================================================================

    // EffectiveId is inherited from LipiInputBase as $"sel-{_resolvedName}" via the
    // ComponentIdPrefix => "sel" override above. Re-derived here just for the
    // dropdown's child id (which subclasses' .razor templates reference).
    protected string EffectiveDropdownId => $"{EffectiveId}-dropdown";

    // IsEmpty inherited from LipiInputBase (default: CurrentValue is null) — correct for selects.
    // ResolvedState, StateCssToken, EffectiveRequiredStyle, IconPx, InlineStyle, HelperText,
    // HelperStateClass, HasHelperContent, HelperRole, HelperAriaLive, AriaInvalidAttr,
    // AutocompleteAttr, AriaDescribedByAttr, ConfidenceCssClass, ConfidenceLabelText,
    // ConfidenceAriaLabel — all inherited from base.

    /// <summary>Component-specific CSS class assembly. Adds lipi-input-select and the
    /// open-state class on top of the base's size + state tokens.</summary>
    protected string CssClasses
    {
        get
        {
            var classes = new List<string>
            {
                $"lipi-input-{Size.ToString().ToLowerInvariant()}",
                $"lipi-input-state-{StateCssToken(ResolvedState)}",
                "lipi-input-select"
            };
            if (IsEmpty)                       classes.Add("lipi-input-empty");
            if (_isOpen)                       classes.Add("lipi-input-select-open");
            if (!string.IsNullOrEmpty(Icon))   classes.Add("lipi-input-has-leading-icon");
            classes.Add("lipi-input-has-trailing-icon"); // chevron always renders
            return string.Join(" ", classes);
        }
    }

    /// <summary>Trailing-icon mapping for selects: validation-state icons (Error → circle-x,
    /// Warning → alert-triangle, Success → check) win over the open/closed chevron.
    /// Each component family defines its own; LipiTextBox/Area use IconRight as default,
    /// LipiNumberInput uses ResolvedState. Selects use chevron-up when open, chevron-down when closed.</summary>
    protected string? EffectiveTrailingIcon => ResolvedState switch
    {
        InputState.Error   => "circle-x",
        InputState.Warning => "alert-triangle",
        InputState.Success => "check",
        _                  => _isOpen ? "chevron-up" : "chevron-down"
    };

    /// <summary>Display text shown in the closed-state anchor button. Falls back to
    /// Placeholder when no value is selected.</summary>
    protected string DisplayText
    {
        get
        {
            var formatted = FormatValueAsString(CurrentValue);
            if (!string.IsNullOrEmpty(formatted)) return formatted;
            return Placeholder ?? string.Empty;
        }
    }

    protected bool ShowPlaceholder => string.IsNullOrEmpty(FormatValueAsString(CurrentValue));
}

/// <summary>Search algorithm for LipiSelect / LipiCombobox option matching.</summary>
public enum SelectSearchMode
{
    /// <summary>Substring match anywhere in the option's label (default).</summary>
    Contains,
    /// <summary>Match only when the label starts with the search text.</summary>
    StartsWith
}
