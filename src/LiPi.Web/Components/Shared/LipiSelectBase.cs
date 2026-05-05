// SPEC: Phase 2.2 Sub-step Batch 5 — abstract base for LipiSelect / LipiCombobox.
// Holds the shared state machine: search, dropdown open/close, keyboard navigation,
// virtualization toggle, pinned-options sorting, JS positioning interop.
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LiPi.Web.Components.Shared;

public abstract class LipiSelectBase<TValue, TItem> : InputBase<TValue>, IAsyncDisposable
{
    // ==========================================================================
    // INJECTED SERVICES
    // ==========================================================================

    [Inject] protected IOptions<LipiInputDefaults> Defaults { get; set; } = default!;
    [Inject] protected IWebHostEnvironment Env { get; set; } = default!;
    [Inject] protected ILogger<LipiSelectBase<TValue, TItem>> Log { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // ==========================================================================
    // CALLER PARAMETERS (shared across LipiSelect and LipiCombobox)
    // ──────────────────────────────────────────────────────────────────────────
    // Inherited from InputBase<TValue> — DO NOT redeclare:
    //   [Parameter] Value, ValueChanged, ValueExpression, DisplayName
    //   protected CurrentValue, CurrentValueAsString, EditContext, FieldIdentifier
    // ==========================================================================

    [Parameter, EditorRequired] public string Name { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;

    /// <summary>The full list of options. IReadOnlyList (not IEnumerable) so .Count is O(1)
    /// for virtualization auto-detection. Caller materializes upstream.</summary>
    [Parameter, EditorRequired] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? Helper { get; set; }
    [Parameter] public string? Error { get; set; }
    [Parameter] public string? Warning { get; set; }
    [Parameter] public string? Success { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public InputSize Size { get; set; } = InputSize.Medium;
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? AriaDescribedBy { get; set; }
    [Parameter] public RequiredVisualStyle? RequiredVisualStyle { get; set; }
    [Parameter] public ConfidenceLevel? LabelConfidence { get; set; }
    [Parameter] public int? Cols { get; set; }
    [Parameter] public string? Class { get; set; }

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
    // STATE MACHINE
    // ==========================================================================

    /// <summary>Below this many items, render all options without Virtualize. Above,
    /// virtualize. Tunable based on field testing — clinical dropdowns rarely exceed
    /// 200 items (countries) or 700 (districts). Most are under 50.</summary>
    protected const int VirtualizationThreshold = 50;

    protected const string AnonymousIdPrefix = "sel-anon-";

    protected string? _cachedAnonymousId;
    protected string _resolvedName = string.Empty;
    protected string _resolvedLabel = string.Empty;
    protected string? _resolvedAutocomplete;

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
    /// Parses the user's typed search/free-text into a TValue. For LipiSelect/LipiCombobox,
    /// successful parsing means: (a) the text matches an option's label (or value), or
    /// (b) AllowFreeText is true and BindConverter can parse the text into TValue.
    /// </summary>
    protected override bool TryParseValueFromString(
        string? value,
        out TValue? result,
        out string? validationErrorMessage)
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
                result = ItemValue(item);
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
                result = itemValue;
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
    // PARAMETER VALIDATION (env-gated, mirrors LipiTextBox/Area/NumberInput)
    // ==========================================================================

    protected override void OnParametersSet()
    {
        _resolvedName         = Name;
        _resolvedLabel        = Label;
        _resolvedAutocomplete = Autocomplete;

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name parameter is required (used to generate id, name, and a11y attributes)");

        if (string.IsNullOrWhiteSpace(Label))
            errors.Add("Label parameter is required (visible text for sighted users + accessible name)");

        if (Items is null)
            errors.Add("Items parameter is required (the options to render in the dropdown)");

        if (!string.IsNullOrWhiteSpace(Autocomplete) && !AutocompleteValidator.IsValid(Autocomplete))
            errors.Add(AutocompleteValidator.FormatInvalidMessage(GetType().Name, "Autocomplete", Autocomplete));

        if (errors.Count == 0) return;

        var message = $"{GetType().Name} parameter validation failed: {string.Join("; ", errors)}";

        if (Env.IsDevelopment())
        {
            throw new InvalidOperationException(message);
        }
        else
        {
            Log.LogError("[{Component}] {Message}. Component will render with fallback values.",
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
            if (!string.IsNullOrWhiteSpace(_resolvedAutocomplete) && !AutocompleteValidator.IsValid(_resolvedAutocomplete))
            {
                _resolvedAutocomplete = "off";
            }
        }
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
    }

    protected void SelectItem(TItem item)
    {
        var v = ItemValue(item);
        CurrentValue = v;
        _isOpen = false;
        _searchText = string.Empty;
        _highlightedIndex = -1;
        _ = DetachJsHandlersAsync();
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

    public async ValueTask DisposeAsync()
    {
        await DetachJsHandlersAsync();
        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    // ==========================================================================
    // RESOLVED PROPERTIES (used by .razor templates)
    // ==========================================================================

    protected string EffectiveId         => $"sel-{_resolvedName}";
    protected string EffectiveDropdownId => $"{EffectiveId}-dropdown";
    protected string HelperId            => $"{EffectiveId}-helper";

    protected RequiredVisualStyle EffectiveRequiredStyle =>
        RequiredVisualStyle ?? Defaults.Value.RequiredVisualStyle;

    protected bool IsEmpty => CurrentValue is null;

    // State precedence: Disabled > ReadOnly > Error > Success > Warning > RequiredEmpty > Default
    protected InputState ResolvedState
    {
        get
        {
            if (Disabled)                       return InputState.Disabled;
            if (ReadOnly)                       return InputState.ReadOnly;
            if (!string.IsNullOrEmpty(Error))   return InputState.Error;
            if (!string.IsNullOrEmpty(Success)) return InputState.Success;
            if (!string.IsNullOrEmpty(Warning)) return InputState.Warning;
            if (Required && IsEmpty
                && EffectiveRequiredStyle == LiPi.Web.Components.Shared.RequiredVisualStyle.ApricotTint)
                                                return InputState.RequiredEmpty;
            return InputState.Default;
        }
    }

    protected int IconPx => Size switch
    {
        InputSize.Small => 14,
        InputSize.Large => 18,
        _               => 16
    };

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

    private static string StateCssToken(InputState s) => s switch
    {
        InputState.Disabled       => "disabled",
        InputState.ReadOnly       => "readonly",
        InputState.Error          => "error",
        InputState.Warning        => "warning",
        InputState.Success        => "success",
        InputState.RequiredEmpty  => "required-empty",
        _                         => "default"
    };

    protected string? InlineStyle => Cols.HasValue ? $"grid-column: span {Cols.Value};" : null;

    protected string? EffectiveTrailingIcon => ResolvedState switch
    {
        InputState.Error   => "circle-x",
        InputState.Warning => "alert-triangle",
        InputState.Success => "check",
        _                  => _isOpen ? "chevron-up" : "chevron-down"
    };

    protected string? HelperText => ResolvedState switch
    {
        InputState.Error   => Error,
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

    protected string? AutocompleteAttr => string.IsNullOrWhiteSpace(_resolvedAutocomplete) ? null : _resolvedAutocomplete;

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
