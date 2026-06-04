// SPEC: docs/00-COMPONENTS/01.4-MultiSelect.md (shipping in Batch 9c)
// PHASE: 2.3 (Batch 9b)
// AMEND: docs/CHANGE-LOG.md A19 (pending — Phase 2.3 close-out)
//
// LipiMultiSelectBase — abstract base for chip-style multi-selection components.
// Concrete subclasses ship in 9b (LipiMultiSelect — TValue identity) and 9c
// (LipiMultiCombobox — TValue+TItem with ValueSelector / LabelSelector).
//
// Architectural lineage:
//   ComponentBase
//   └── InputBase<List<TValue>>  (Microsoft, Blazor binding)
//       └── LipiInputBase<List<TValue>>  (Phase 2.2.5 — visual scaffolding,
//                                          touched-state, EditContext wiring,
//                                          env-gated parameter validation)
//           └── LipiMultiSelectBase<TValue, TItem>  (this class —
//                                                    multi-value semantics,
//                                                    chip rendering helpers,
//                                                    dropdown state machine)
//
// PARALLELS LipiSelectBase but for List<TValue> binding. Differences:
//   - Value is List<TValue>, not TValue?
//   - Selection is TOGGLE (add or remove), not SET
//   - Dropdown stays OPEN after selection (multi-add UX)
//   - Search input clears on each selection (ready for next)
//   - Backspace on empty search input removes the LAST chip (Q3 = a)
//   - +M more summary chip when MaxVisibleChips set + Value.Count > MaxVisibleChips (Q2 = α)
//   - All Value mutations use REPLACEMENT (new List), not in-place mutation (Pattern B)
//
// Path 3 lesson from 9a: dropdown panel rendering is duplicated declaratively
// across subclasses (LipiMultiSelect, LipiMultiCombobox in 9c) rather than
// extracted into a method-form RenderFragment on the base. Trigger to extract:
// when 3+ subclasses or a non-multi consumer also wants the same panel shape.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiPi.Components;

/// <summary>
/// Generic abstract base for multi-selection input components. Subclasses provide
/// ItemValue / ItemLabel / ItemContent to map between the TItem dropdown shape
/// and the TValue binding shape. The base owns:
/// <list type="bullet">
///   <item>Value mutation helpers (AddItem, RemoveItem, RemoveLastChip, ToggleItem) — all using replacement (Pattern B)</item>
///   <item>Dropdown state machine (_isOpen, _searchText, _highlightedIndex)</item>
///   <item>Filtering + free-text handling (_resolvedAllowFreeText after env-gated check)</item>
///   <item>MaxSelections cap enforcement before AddItem</item>
///   <item>+M more summary handling — _isFromSummaryClick flag triggers selected-first sort</item>
///   <item>Type-ahead buffer for non-searchable mode</item>
///   <item>Keyboard handlers (Backspace, Arrow, Enter, Space, Escape, Type-ahead)</item>
///   <item>JS interop for outside-click / anchor positioning (mirrors LipiSelectBase)</item>
/// </list>
/// </summary>
public abstract class LipiMultiSelectBase<TValue, TItem> : LipiInputBase<List<TValue>>, IAsyncDisposable
{
    // ==========================================================================
    // INJECTED SERVICES
    // ==========================================================================

    [Inject] protected IJSRuntime JS { get; set; } = default!;

    // ==========================================================================
    // PARAMETERS
    // ==========================================================================

    /// <summary>The full set of items available for selection. Filtered/searched
    /// in the dropdown, but the backing list is this Items collection.</summary>
    [Parameter, EditorRequired] public IEnumerable<TItem> Items { get; set; } = Array.Empty<TItem>();

    /// <summary>True (default): renders an embedded search input inside the dropdown
    /// so the user can filter as they type. False: dropdown opens with full item
    /// list and supports type-ahead via keyboard buffer.</summary>
    [Parameter] public bool Searchable { get; set; } = true;

    /// <summary>Free-text capability — when true (and Searchable=true), the dropdown
    /// shows "Add 'X' as new" when the search text doesn't match any item. Selecting
    /// it adds the typed string as a new value.
    /// REQUIRES TValue=string (or string?). Validated env-gated in OnParametersSet
    /// per memory rule A14 pattern. See <c>_resolvedAllowFreeText</c> for the
    /// runtime-resolved value used in rendering.</summary>
    [Parameter] public bool AllowFreeText { get; set; }

    /// <summary>Hard cap on total selections. When Value.Count == MaxSelections,
    /// dropdown items not yet selected are disabled and a "Maximum reached"
    /// message is shown. null = unlimited (default).</summary>
    [Parameter] public int? MaxSelections { get; set; }

    /// <summary>Cap on number of chips visually rendered in the field bar.
    /// When set and Value.Count > MaxVisibleChips, only the first
    /// MaxVisibleChips chips render, plus a "+M more" summary chip.
    /// null = render all chips (default).</summary>
    [Parameter] public int? MaxVisibleChips { get; set; }

    /// <summary>Section heading for the "selected items" group when dropdown is
    /// opened from a +M more summary chip click (Q2 = α: dropdown opens with
    /// selected items sorted to top alphabetically).</summary>
    [Parameter] public string SelectedHeading { get; set; } = "Selected";

    /// <summary>Section heading for the "available items" group in the same
    /// summary-click scenario.</summary>
    [Parameter] public string AvailableHeading { get; set; } = "Available";

    /// <summary>Optional virtualization override. Default heuristic kicks in
    /// at >50 items.</summary>
    [Parameter] public bool? UseVirtualization { get; set; }

    // ── Phase 2.5.5 — LabelPosition cross-family retrofit ─────────────────────────────────

    /// <summary>Position of the label relative to the input field.
    /// Top (default) — label above. Left — saves vertical space. Right — RTL. Bottom — caption.</summary>
    [Parameter] public InputLabelPosition LabelPosition { get; set; }
        = InputLabelPosition.Top;

    /// <summary>Accessible name when Label is empty. Required when Label="" (WCAG 2.1 SC 4.1.2).</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>Resolved CSS class for label position. Protected so concrete razor markup can bind.
    /// Computed in OnParametersSet.</summary>
    protected string _resolvedLabelPositionClass = string.Empty;

    // Note: Placeholder is inherited from LipiInputBase — no need to redeclare here.
    // (Memory rule #25 bullet 6 — inheritance shadowing check caught a duplicate
    // Placeholder declaration in this slot during the Batch 9b CS0108 cleanup.)

    // ==========================================================================
    // ABSTRACT HOOKS — same signatures as LipiSelectBase
    // ==========================================================================

    protected abstract TValue? ItemValue(TItem item);
    protected abstract string ItemLabel(TItem item);
    protected abstract RenderFragment<TItem> ItemContent { get; }

    // ==========================================================================
    // CONSTANTS
    // ==========================================================================

    protected const int VirtualizationThreshold = 50;
    private static readonly TimeSpan _typeAheadResetWindow = TimeSpan.FromSeconds(1);

    // ==========================================================================
    // STATE
    // ==========================================================================

    protected bool _isOpen;
    protected bool _wasOpen;
    protected string _searchText = string.Empty;
    protected int _highlightedIndex = -1;

    /// <summary>True when the dropdown was opened by clicking the +M more
    /// summary chip. Triggers the selected-first alphabetical sort in
    /// GetFilteredOptions. Reset to false in CloseDropdownAsync.</summary>
    protected bool _isFromSummaryClick;

    /// <summary>Resolved AllowFreeText value after env-gated TValue=string check.
    /// In Development, AllowFreeText=true with non-string TValue throws. In
    /// Production, it's logged and forced to false. Renderer reads this, not
    /// the raw AllowFreeText parameter.</summary>
    protected bool _resolvedAllowFreeText;

    /// <summary>Type-ahead buffer for non-searchable mode. Accumulates typed
    /// keys with a 1-second reset window.</summary>
    private string _typeAheadBuffer = string.Empty;
    private DateTime _typeAheadLastKeyAt = DateTime.MinValue;

    protected ElementReference _anchorRef;
    protected ElementReference _searchInputRef;
    protected DotNetObjectReference<LipiMultiSelectBase<TValue, TItem>>? _dotNetRef;
    protected bool _jsHandlersAttached;

    // ==========================================================================
    // INPUTBASE OVERRIDES
    // ==========================================================================

    /// <summary>InputBase&lt;List&lt;TValue&gt;&gt; requires this. We don't parse — Value
    /// is set programmatically via AddItem / RemoveItem / etc. The string
    /// representation here exists only because InputBase requires it.</summary>
    protected override bool TryParseValueFromString(
        string? value, out List<TValue> result, out string validationErrorMessage)
    {
        result = CurrentValue ?? new List<TValue>();
        validationErrorMessage = string.Empty;
        return true;
    }

    /// <summary>Q5 — strict IsEmpty: tint applies whenever Value is empty,
    /// regardless of interaction history. Matches LipiTextBox / LipiSelect
    /// family behavior.</summary>
    protected override bool IsEmpty => CurrentValue is null || CurrentValue.Count == 0;

    protected override string ComponentIdPrefix => "msel";
    protected override string ComponentTypeName => GetType().Name;

    /// <summary>Mirrors LipiSelectBase.CollectParameterValidationErrors. Adds
    /// the AllowFreeText + TValue=string env-gated validation (Q4 = Option I).</summary>
    protected override List<string> CollectParameterValidationErrors()
    {
        var errors = base.CollectParameterValidationErrors();

        // Phase 2.5.5 — Label/AriaLabel filter (same pattern as SelectBase).
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
            errors.Add("Items parameter is required.");

        if (MaxSelections.HasValue && MaxSelections.Value <= 0)
            errors.Add($"MaxSelections must be > 0 when set. Got {MaxSelections.Value}.");

        if (MaxVisibleChips.HasValue && MaxVisibleChips.Value <= 0)
            errors.Add($"MaxVisibleChips must be > 0 when set. Got {MaxVisibleChips.Value}.");

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

        // Q4 — env-gated AllowFreeText validation. AllowFreeText creates new
        // values from typed strings, which only makes sense for TValue=string.
        // Develeopment: throw. Production: log + force-false.
        _resolvedAllowFreeText = AllowFreeText;
        if (AllowFreeText && typeof(TValue) != typeof(string))
        {
            var msg = $"LipiMultiSelect: AllowFreeText=true requires TValue=string (or string?). " +
                      $"Got TValue={typeof(TValue).Name}. Free-text input creates new values that " +
                      $"cannot be synthesized for non-string types.";
            if (Env.IsDevelopment())
            {
                throw new InvalidOperationException(msg);
            }
            else
            {
                Log.LogError("[LipiMultiSelect] {Msg} Falling back to AllowFreeText=false.", msg);
                _resolvedAllowFreeText = false;
            }
        }

        // Defensive: ensure CurrentValue is always a non-null List for chip
        // rendering loops. Caller can bind to a null property; we treat that
        // as empty.
        if (CurrentValue is null)
        {
            CurrentValue = new List<TValue>();
        }
    }

    // ==========================================================================
    // VALUE MUTATIONS — Pattern B (replacement). All mutations create a new
    // List<TValue> instance and assign to CurrentValue. The InputBase setter
    // pipeline handles ValueChanged + EditContext.NotifyFieldChanged.
    //
    // Why Pattern B: in-place mutation produces the same reference, which
    // breaks reference-equality change detection in EditContext, FluentValidation,
    // EF Core change tracking, and any future migration to ImmutableList<T>.
    // Per chip-add/remove cost: one List<T> allocation. User-paced operations,
    // negligible runtime impact.
    // ==========================================================================

    protected bool IsSelected(TValue? value)
    {
        if (CurrentValue is null || value is null) return false;
        return CurrentValue.Contains(value, EqualityComparer<TValue>.Default);
    }

    /// <summary>Returns true if MaxSelections cap would prevent another add.</summary>
    protected bool AtMaxSelections =>
        MaxSelections.HasValue && (CurrentValue?.Count ?? 0) >= MaxSelections.Value;

    /// <summary>Adds an item if not already selected and not at cap. Returns
    /// true if the item was added; false if already present or capped.</summary>
    protected bool AddItem(TValue value)
    {
        if (value is null) return false;
        var current = CurrentValue ?? new List<TValue>();
        if (current.Contains(value, EqualityComparer<TValue>.Default)) return false;
        if (MaxSelections.HasValue && current.Count >= MaxSelections.Value) return false;

        var newList = new List<TValue>(current) { value };
        CurrentValue = newList;
        return true;
    }

    /// <summary>Removes the specific item from selection (chip × click).
    /// Returns true if item was present and removed; false otherwise.</summary>
    protected bool RemoveItem(TValue value)
    {
        if (value is null || CurrentValue is null || CurrentValue.Count == 0) return false;
        var newList = new List<TValue>(CurrentValue);
        var removed = newList.Remove(value);
        if (!removed) return false;
        CurrentValue = newList;
        return true;
    }

    /// <summary>Toggles selection of an item — adds if absent, removes if present.
    /// Used by dropdown option clicks (Enter/Space/Click). Returns the new
    /// presence state (true = item is now selected, false = item is now unselected).</summary>
    protected bool ToggleItem(TValue value)
    {
        if (IsSelected(value))
        {
            RemoveItem(value);
            return false;
        }
        return AddItem(value);
    }

    /// <summary>Q3 = a — Backspace handler. Removes Value[Value.Count - 1]
    /// (last in list order, which is also "most recently added"). Pattern B
    /// replacement. No-op if Value is empty.</summary>
    protected void RemoveLastChip()
    {
        if (CurrentValue is null || CurrentValue.Count == 0) return;
        var newList = new List<TValue>(CurrentValue);
        newList.RemoveAt(newList.Count - 1);
        CurrentValue = newList;
    }

    // ==========================================================================
    // FILTERING + FREE-TEXT
    // ==========================================================================

    /// <summary>Returns dropdown items filtered by current search text. When
    /// _isFromSummaryClick is true (dropdown opened by +M more click), items
    /// are split into Selected (alphabetical) + Unselected (alphabetical) groups
    /// per Q2 locked decision.</summary>
    protected (List<TItem> Selected, List<TItem> Unselected) GetFilteredOptions()
    {
        var allItems = (Items ?? Array.Empty<TItem>()).ToList();

        // Apply search filter (case-insensitive contains-match on label)
        var filtered = string.IsNullOrWhiteSpace(_searchText)
            ? allItems
            : allItems.Where(MatchesSearch).ToList();

        if (!_isFromSummaryClick)
        {
            // Normal open — return everything in unselected bucket, no split.
            return (Selected: new List<TItem>(), Unselected: filtered);
        }

        // Summary-click open — partition into Selected vs Unselected, sort each
        // alphabetically by ItemLabel.
        var selected = filtered
            .Where(item => { var v = ItemValue(item); return v is not null && IsSelected(v); })
            .OrderBy(ItemLabel, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var unselected = filtered
            .Where(item => { var v = ItemValue(item); return v is null || !IsSelected(v); })
            .OrderBy(ItemLabel, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return (Selected: selected, Unselected: unselected);
    }

    private bool MatchesSearch(TItem item)
    {
        var label = ItemLabel(item);
        if (string.IsNullOrEmpty(label)) return false;
        return label.IndexOf(_searchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    protected bool ShouldVirtualize =>
        UseVirtualization ?? ((Items?.Count() ?? 0) > VirtualizationThreshold);

    /// <summary>Commits the current _searchText as a new free-text value (when
    /// AllowFreeText resolved true). Adds via AddItem, clears search, keeps
    /// dropdown open for next entry.</summary>
    protected async Task CommitFreeTextAsync()
    {
        if (!_resolvedAllowFreeText) return;
        if (string.IsNullOrWhiteSpace(_searchText)) return;

        // Safe cast — _resolvedAllowFreeText is only true when TValue==string.
        // The cast pattern goes through object to satisfy the compiler's
        // generic-type-erasure analysis.
        var asValue = (TValue)(object)_searchText.Trim();
        AddItem(asValue);

        _searchText = string.Empty;
        _highlightedIndex = -1;
        // Keep dropdown open for next selection — multi-select stays open
        StateHasChanged();
        await Task.CompletedTask;
    }

    // ==========================================================================
    // DROPDOWN OPEN / CLOSE
    // ==========================================================================

    protected async Task OpenDropdownAsync(bool fromSummaryClick = false)
    {
        if (Disabled || ReadOnly) return;
        _isOpen = true;
        _isFromSummaryClick = fromSummaryClick;
        _searchText = string.Empty;
        _highlightedIndex = -1;
        await EnsureJsHandlersAsync();
        StateHasChanged();
    }

    protected async Task CloseDropdownAsync()
    {
        _isOpen = false;
        _isFromSummaryClick = false;  // reset per memo refinement — otherwise
                                      // a subsequent normal open would carry
                                      // the flag and incorrectly sort
        _searchText = string.Empty;
        _highlightedIndex = -1;
        _typeAheadBuffer = string.Empty;
        _typeAheadLastKeyAt = DateTime.MinValue;
        await DetachJsHandlersAsync();
        StateHasChanged();
    }

    /// <summary>Click-on-option handler. Toggles selection (multi-select toggle
    /// semantics, not single-select set), keeps dropdown open, clears search,
    /// returns focus to search input for next add.
    ///
    /// Internally checks MaxSelections cap — when at cap and the clicked item
    /// is NOT already selected, the click is ignored (UI shows the option as
    /// disabled per CssClasses, but the click handler defends against
    /// keyboard-driven Enter/Space arriving here when cap is in effect).</summary>
    protected async Task SelectAndContinueAsync(TItem item)
    {
        var v = ItemValue(item);
        if (v is null) return;

        // Cap defense — if at cap and item not currently selected, ignore.
        // (When item IS selected, this is a "remove" operation, which we
        // always allow regardless of cap.)
        if (AtMaxSelections && !IsSelected(v)) return;

        ToggleItem(v);

        // Clear search so user can type next filter; dropdown stays open.
        _searchText = string.Empty;
        _highlightedIndex = -1;
        StateHasChanged();

        // Focus stays in search input (if Searchable) so user can type next.
        // Don't reposition focus to anchor — that would close the dropdown
        // on next render via blur.
        if (Searchable)
        {
            try { await _searchInputRef.FocusAsync(); }
            catch { /* not yet rendered, ignore */ }
        }
    }

    // ==========================================================================
    // EVENT HANDLERS
    // ==========================================================================

    /// <summary>Anchor click — opens or closes the dropdown.</summary>
    protected async Task HandleAnchorClick(MouseEventArgs e)
    {
        if (Disabled || ReadOnly) return;
        if (_isOpen) await CloseDropdownAsync();
        else         await OpenDropdownAsync(fromSummaryClick: false);
    }

    /// <summary>+M more chip click — opens dropdown with Q2 sort behavior.</summary>
    protected async Task HandleSummaryChipClick(MouseEventArgs e)
    {
        if (Disabled || ReadOnly) return;
        await OpenDropdownAsync(fromSummaryClick: true);
    }

    /// <summary>Chip × click — removes specific chip.</summary>
    protected void HandleChipRemove(TValue value)
    {
        if (Disabled || ReadOnly) return;
        RemoveItem(value);
        MarkFieldAsTouched();
    }

    /// <summary>Search input change — updates filter, resets highlight, keeps
    /// dropdown open.</summary>
    protected void HandleSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _highlightedIndex = string.IsNullOrEmpty(_searchText) ? -1 : 0;
        if (!_isOpen) _isOpen = true;
        StateHasChanged();
    }

    /// <summary>Anchor blur (when dropdown is closed) — marks touched per the
    /// LipiInputBase blur-touch convention. Note: anchor blur does NOT close
    /// the dropdown — that's handled by JS outside-click handler. This blur
    /// fires when user tabs away, not when they click into the search input.</summary>
    protected void HandleAnchorBlur(FocusEventArgs e)
    {
        if (!_isOpen)
        {
            MarkFieldAsTouched();
        }
    }

    /// <summary>Q6 keyboard handlers. Each branch documents its specific
    /// keyboard scope — see locked decision Q6 in CHANGE-LOG A19.</summary>
    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled || ReadOnly) return;

        switch (e.Key)
        {
            case "Backspace":
                // Q3 — Backspace on empty search input removes last chip.
                // If search has any text, the input itself handles the
                // backspace (delete last char) — we don't intercept.
                if (string.IsNullOrEmpty(_searchText) && CurrentValue is { Count: > 0 })
                {
                    RemoveLastChip();
                    MarkFieldAsTouched();
                    StateHasChanged();
                }
                break;

            case "ArrowDown":
                if (!_isOpen)
                {
                    await OpenDropdownAsync();
                }
                else
                {
                    MoveHighlight(+1);
                    StateHasChanged();
                    await ScrollHighlightIntoViewAsync();
                }
                break;

            case "ArrowUp":
                if (_isOpen)
                {
                    MoveHighlight(-1);
                    StateHasChanged();
                    await ScrollHighlightIntoViewAsync();
                }
                break;

            case "Enter":
            case " ":  // Spacebar — Q6 WAI-ARIA toggle pattern
                if (!_isOpen)
                {
                    await OpenDropdownAsync();
                    return;
                }
                // Commit highlighted item (toggle semantics) — keep dropdown open
                var (selected, unselected) = GetFilteredOptions();
                var combined = selected.Concat(unselected).ToList();
                if (_highlightedIndex >= 0 && _highlightedIndex < combined.Count)
                {
                    var item = combined[_highlightedIndex];
                    var v = ItemValue(item);
                    if (v is not null) ToggleItem(v);
                    _searchText = string.Empty;
                    _highlightedIndex = -1;
                    StateHasChanged();
                }
                else if (_resolvedAllowFreeText && !string.IsNullOrWhiteSpace(_searchText))
                {
                    // Enter on empty highlight + free-text mode + non-empty
                    // search = commit free-text entry.
                    await CommitFreeTextAsync();
                }
                break;

            case "Escape":
                if (_isOpen)
                {
                    await CloseDropdownAsync();
                }
                break;

            default:
                // Type-ahead for non-searchable mode. Single-character keys
                // accumulate into a buffer with 1-second reset window. Buffer
                // matches against ItemLabel prefix.
                if (!Searchable && _isOpen && e.Key.Length == 1)
                {
                    var now = DateTime.UtcNow;
                    if (now - _typeAheadLastKeyAt > _typeAheadResetWindow)
                    {
                        _typeAheadBuffer = string.Empty;
                    }
                    _typeAheadBuffer += e.Key;
                    _typeAheadLastKeyAt = now;

                    var allItems = (Items ?? Array.Empty<TItem>()).ToList();
                    var matchIdx = allItems.FindIndex(item =>
                        ItemLabel(item).StartsWith(
                            _typeAheadBuffer, StringComparison.CurrentCultureIgnoreCase));
                    if (matchIdx >= 0)
                    {
                        _highlightedIndex = matchIdx;
                        StateHasChanged();
                        await ScrollHighlightIntoViewAsync();
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Scrolls the currently-highlighted option into the dropdown panel's
    /// visible scroll region. Called after every keyboard-driven update to
    /// _highlightedIndex (ArrowUp/Down/Type-ahead). Uses
    /// scrollIntoView({block:'nearest'}) on JS side — only scrolls as much
    /// as needed; no-op if already visible.
    ///
    /// Bug history (Batch 9b.3): without this, type-ahead and arrow-key
    /// navigation past the visible region updated _highlightedIndex
    /// correctly but the user saw no visual feedback because the highlighted
    /// option remained outside the dropdown's scroll viewport.
    /// </summary>
    protected async Task ScrollHighlightIntoViewAsync()
    {
        if (_highlightedIndex < 0) return;
        try
        {
            await JS.InvokeVoidAsync("lipiInput.scrollOptionIntoView",
                EffectiveDropdownId, _highlightedIndex);
        }
        catch
        {
            // JS interop unavailable — keyboard navigation still works,
            // user can scroll dropdown manually. Silent.
        }
    }

    private void MoveHighlight(int delta)
    {
        var (selected, unselected) = GetFilteredOptions();
        var totalCount = selected.Count + unselected.Count;
        if (totalCount == 0)
        {
            _highlightedIndex = -1;
            return;
        }
        var next = _highlightedIndex + delta;
        if (next < 0) next = totalCount - 1;
        if (next >= totalCount) next = 0;
        _highlightedIndex = next;
    }

    // ==========================================================================
    // CHIP RENDERING HELPERS — used by subclass templates
    // ==========================================================================

    /// <summary>Returns (visibleChips, hiddenChipCount) per MaxVisibleChips rules.
    /// hiddenChipCount > 0 means a +M more summary chip should render.</summary>
    protected (List<TValue> Visible, int HiddenCount) GetChipsToRender()
    {
        if (CurrentValue is null || CurrentValue.Count == 0)
        {
            return (Visible: new List<TValue>(), HiddenCount: 0);
        }

        if (!MaxVisibleChips.HasValue || CurrentValue.Count <= MaxVisibleChips.Value)
        {
            return (Visible: CurrentValue, HiddenCount: 0);
        }

        var max = MaxVisibleChips.Value;
        return (Visible: CurrentValue.Take(max).ToList(),
                HiddenCount: CurrentValue.Count - max);
    }

    /// <summary>Looks up the human-readable label for a TValue from the Items
    /// list. Used by chip rendering. If TValue isn't found in Items (e.g.,
    /// free-text entry where TValue=string), falls back to value.ToString().</summary>
    protected string GetLabelForValue(TValue value)
    {
        if (Items is null) return value?.ToString() ?? string.Empty;

        foreach (var item in Items)
        {
            var iv = ItemValue(item);
            if (iv is not null && EqualityComparer<TValue>.Default.Equals(iv, value))
            {
                return ItemLabel(item);
            }
        }
        // Fallback: free-text or stale TValue not in Items
        return value?.ToString() ?? string.Empty;
    }

    // ==========================================================================
    // JS INTEROP — outside-click + dropdown positioning. Mirrors LipiSelectBase.
    // ==========================================================================

    private async Task EnsureJsHandlersAsync()
    {
        if (_jsHandlersAttached) return;
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("lipiInput.attachSelectHandlers", _anchorRef, _dotNetRef, EffectiveDropdownId);
            _jsHandlersAttached = true;
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "[LipiMultiSelect] attachSelectHandlers failed.");
        }
    }

    private async Task DetachJsHandlersAsync()
    {
        if (!_jsHandlersAttached) return;
        try
        {
            await JS.InvokeVoidAsync("lipiInput.detachSelectHandlers", EffectiveDropdownId);
        }
        catch { /* circuit may be closing */ }
        _jsHandlersAttached = false;
    }

    [JSInvokable]
    public async Task OnOutsideClick()
    {
        if (_isOpen)
        {
            await CloseDropdownAsync();
        }
    }

    [JSInvokable]
    public async Task OnAncestorScroll()
    {
        if (_isOpen)
        {
            try { await JS.InvokeVoidAsync("lipiInput.positionDropdown", _anchorRef, EffectiveDropdownId); }
            catch { /* ignore */ }
        }
    }

    // ==========================================================================
    // OnAfterRenderAsync — focus management + dropdown positioning
    // ==========================================================================

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_isOpen && !_wasOpen)
        {
            // Just opened — position dropdown + focus search input
            try
            {
                await JS.InvokeVoidAsync("lipiInput.positionDropdown", _anchorRef, EffectiveDropdownId);
                if (Searchable)
                {
                    await _searchInputRef.FocusAsync();
                }
            }
            catch { /* ignore */ }
        }

        _wasOpen = _isOpen;
    }

    // ==========================================================================
    // DISPOSAL
    // ==========================================================================

    public async ValueTask DisposeAsync()
    {
        await DetachJsHandlersAsync();
        _dotNetRef?.Dispose();
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // ==========================================================================
    // RENDERING HELPERS
    // ==========================================================================

    protected string EffectiveDropdownId => $"{EffectiveId}-dropdown";

    protected string CssClasses
    {
        get
        {
            var classes = new List<string>
            {
                "lipi-input-wrapper",
                "lipi-multi-select",  // Q1 — drives the .lipi-multi-select compound
                                      // CSS rules (min-height instead of fixed height)
                $"lipi-input-{Size.ToString().ToLowerInvariant()}",
                $"lipi-input-state-{StateCssToken(ResolvedState)}"
            };
            if (Required && IsEmpty && ResolvedState == InputState.Default
                && EffectiveRequiredStyle == LiPi.Components.RequiredVisualStyle.ApricotTint)
            {
                classes.Add("lipi-input-state-required-empty");
            }
            return string.Join(" ", classes);
        }
    }

    protected bool ShowPlaceholder =>
        (CurrentValue is null || CurrentValue.Count == 0) && string.IsNullOrEmpty(_searchText);
}
