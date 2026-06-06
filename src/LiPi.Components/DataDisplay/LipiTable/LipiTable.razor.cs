// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §3.1 (LipiTable<TItem> signature, required params), §3.1.3 (KeySelector + Items/DataSource),
//   §2.1 (region order + body-state priority), §18.1 (body-state priority),
//   §3.6.1 (CSS Grid backing), §19 (role="grid")
// PHASE: 2.8 Data Display — Stage 2 core shell ("rolling chassis")
// COMPONENT: LipiTable<TItem>
//
// The bare chassis: declarative <LipiColumn> children register here; the table validates
// KeySelector + a single data source (Items only this stage), computes the CSS Grid track
// template, and renders the sticky header row + one body row per item, delegating empty/
// loading/error body states to LipiEmptyState.
//
// SCOPE NOTE (Stage 2 bare chassis):
//   • Items path ONLY. Server-side DataSource is declared (API stability) but inert —
//     supplying it throws a dev-mode NotSupported note this stage; wired in a later stage.
//   • No toolbar, no header band, no filter chips, no bulk bar, no group bar, no
//     pagination, no selection, no sort, no edit. Those are 2a-excluded by agreement.
//   • Body states: Loading / Error / Empty / Normal. FilteredEmpty is identical to Empty
//     this stage (no filters exist yet to distinguish).
//   • Validation is non-throwing-friendly: KeySelector missing throws in Development
//     (env-gated), logs + renders fallback in Production, per the established pattern.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using LiPi.Components.Overlays;   // DrawerPlacement (filter drawer, PR3)

namespace LiPi.Components.DataDisplay;

[CascadingTypeParameter(nameof(TItem))]
public partial class LipiTable<TItem> : ComponentBase, IDisposable
{
    // ── Required data params (§3.1.3) ────────────────────────────────────
    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }
    [Parameter] public Func<TableQueryRequest, Task<TableQueryResponse<TItem>>>? DataSource { get; set; }
    [Parameter] public Func<TItem, object>? KeySelector { get; set; }
    [Parameter] public string? TableId { get; set; }

    // ── Body-state params (subset active this stage) ─────────────────────
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string? LoadError { get; set; }

    // ── Empty-state copy (forwarded to LipiEmptyState) ───────────────────
    [Parameter] public string EmptyTitle { get; set; } = "No data";
    [Parameter] public string? EmptyBody { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }
    [Parameter] public RenderFragment? ErrorTemplate { get; set; }

    // ── Visual (subset active this stage) ────────────────────────────────
    [Parameter] public bool StripedRows { get; set; } = true;
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // ── Density (§14.2) ──────────────────────────────────────────────────
    [Parameter] public TableDensity Density { get; set; } = TableDensity.Comfortable;

    // ── Selection (§5.1) — Stage 3 renders the column structure (visible);
    //    toggle behavior (select-all, per-row state) is Stage 4. ───────────
    [Parameter] public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    // ── Row status strip (§22.2) ─────────────────────────────────────────
    [Parameter] public Func<TItem, string?>? RowStatus { get; set; }
    [Parameter] public RowStatusPlacement RowStatusPlacement { get; set; } = RowStatusPlacement.Left;

    // ── Row formatting hooks (§22.3–22.5) ────────────────────────────────
    [Parameter] public Func<TItem, string?>? RowClass { get; set; }
    [Parameter] public Func<TItem, string?>? RowStyle { get; set; }
    [Parameter] public Func<TItem, bool>? RowDisabled { get; set; }

    // ── Copy affordance callback (§3.8; optional — host may toast) ────────
    [Parameter] public EventCallback<string> OnCopy { get; set; }

    // ── Column children ──────────────────────────────────────────────────
    // ── Selection (Stage 4a) ─────────────────────────────────────────────
    // Two-way bound visible items (never null; empty list when nothing selected). §5.7.1
    [Parameter] public IReadOnlyList<TItem> SelectedItems { get; set; } = Array.Empty<TItem>();
    [Parameter] public EventCallback<IReadOnlyList<TItem>> SelectedItemsChanged { get; set; }
    // Two-way bound raw key set (includes off-page keys at scale; single-page this slice). §5.7.2
    [Parameter] public IReadOnlyCollection<object>? SelectedKeys { get; set; }
    [Parameter] public EventCallback<IReadOnlyCollection<object>> SelectedKeysChanged { get; set; }
    // Rich change event with added/removed diff + reason. §5.7.3
    [Parameter] public EventCallback<SelectionChangedContext<TItem>> OnSelectionChanged { get; set; }

    // LP-17 (Stage 7→4c seam): fires when the header checkbox selects/deselects all rows on the
    // CURRENT page. Stage 4c subscribes to show the across-pages banner ("Select all N?"). Stage 7
    // only DEFINES + fires it; it renders no banner. Payload in Contexts.cs (table taxonomy, LP-23).
    [Parameter] public EventCallback<AllOnPageSelectedContext> OnAllOnPageSelected { get; set; }

    // ── Sort (Stage S1) ──────────────────────────────────────────────────
    /// <summary>Fired after the sort chain changes (header click, shift-click, clear). </summary>
    [Parameter] public EventCallback<SortChangedContext> OnSortChanged { get; set; }

    /// <summary>Header sort cycle. ThreeState (unsorted→asc→desc→unsorted) default. Per §6.1.2.</summary>
    [Parameter] public SortCycleMode SortCycleMode { get; set; } = SortCycleMode.ThreeState;

    /// <summary>Null ordering relative to non-null. AlwaysLast default. Per §6.4.3.</summary>
    [Parameter] public NullSortOrder NullSortOrder { get; set; } = NullSortOrder.AlwaysLast;

    /// <summary>When true, shift-clicking a header adds to the sort chain (multi-column).
    /// When false, any header click replaces the chain (single-column only). Default true.</summary>
    [Parameter] public bool MultiColumnSort { get; set; } = true;

    // ── Quick search (Stage S2) ──────────────────────────────────────────
    /// <summary>Show the quick-search box in the toolbar. Default false (opt-in).</summary>
    [Parameter] public bool ShowQuickSearch { get; set; }

    /// <summary>Placeholder text for the quick-search box.</summary>
    [Parameter] public string QuickSearchPlaceholder { get; set; } = "Quick search…";

    /// <summary>Highlight matched substrings in built-in (table-rendered) cells. Default false.
    /// Custom CellTemplate cells are NOT highlighted (the table can't reach into caller markup).</summary>
    [Parameter] public bool HighlightMatches { get; set; }

    /// <summary>Fired (debounced 300ms) after the quick-search text changes.</summary>
    [Parameter] public EventCallback<QuickSearchChangedContext> OnQuickSearchChanged { get; set; }

    // ── Column filters (Stage S3) ────────────────────────────────────────
    /// <summary>Filter UI mode. HeaderIcon (funnel per column) default; Drawer (S3c); None disables.</summary>
    [Parameter] public FilterMode FilterMode { get; set; } = FilterMode.HeaderIcon;

    /// <summary>When a column filter commits. Apply (explicit button) default; Live (debounced).</summary>
    [Parameter] public FilterApplyMode FilterApplyMode { get; set; } = FilterApplyMode.Apply;

    /// <summary>Case-sensitive column filtering (Stage S3). Default false (OrdinalIgnoreCase).</summary>
    [Parameter] public bool FilterCaseSensitive { get; set; }

    /// <summary>Zone that anchors relative/explicit date-filter window boundaries for
    /// DateTimeOffset/DateTime columns (the BP-search resolution — the FILTER carries the
    /// zone, not the picker). Null = server-local "today". (S3b)</summary>
    [Parameter] public TimeZoneInfo? FilterTimeZone { get; set; }

    /// <summary>First day of week for relative week filters (ThisWeek/LastWeek/NextWeek). (S3b)</summary>
    [Parameter] public DayOfWeek FilterWeekStart { get; set; } = DayOfWeek.Sunday;

    /// <summary>Forwarded to the date-range filter editor's <c>IndependentMonths</c> (popover + drawer):
    /// when true the two calendars navigate independently (right ≥ left). Default false (linked).</summary>
    [Parameter] public bool DateFilterIndependentMonths { get; set; }

    /// <summary>Default relative-date operator buckets offered by date columns (cascade default;
    /// a column's own <c>RelativeDateSpans</c> overrides). Null = all buckets (the full relative set).
    /// A month-only clinic sets <c>RelativeDateSpans="DateSpan.Month"</c> here once. Gates only the
    /// relative operators; absolute ops (On/Before/After/Between) and the date-range picker always
    /// remain. (S3b+)</summary>
    [Parameter] public DateSpan? RelativeDateSpans { get; set; }

    /// <summary>Where active-filter chips render. Separate strip (default) or inline in the toolbar.</summary>
    [Parameter] public FilterChipsPlacement FilterChipsPlacement { get; set; } = FilterChipsPlacement.Separate;

    /// <summary>Fired after the filter set changes (apply, clear, clear-all).</summary>
    [Parameter] public EventCallback<FilterChangedContext> OnFilterChanged { get; set; }

    // ── Filter drawer (Stage S3c / PR3) — active only when FilterMode=Drawer ───
    /// <summary>Which control opens the filter drawer when <c>FilterMode=Drawer</c>.
    /// ToolbarButton (labeled, with active-count badge) default; HeaderFunnel (funnel icon).</summary>
    [Parameter] public FilterDrawerTrigger FilterDrawerTrigger { get; set; } = FilterDrawerTrigger.ToolbarButton;

    /// <summary>Side the filter drawer slides from. Right default; Left supported.
    /// Maps internally to the Overlays <c>DrawerPlacement</c>.</summary>
    [Parameter] public FilterDrawerSide FilterDrawerSide { get; set; } = FilterDrawerSide.Right;

    /// <summary>Filter drawer size in px (passed to <c>LipiDrawer.SizePx</c>). Default 420.
    /// Width for Left/Right sides; height for Top/Bottom. Null delegates to LipiDrawer's Size default.</summary>
    [Parameter] public int? FilterDrawerSizePx { get; set; } = 420;

    /// <summary>Filter drawer header title. Default "Filters".</summary>
    [Parameter] public string FilterDrawerTitle { get; set; } = "Filters";
    // Single-mode: allow clicking the selected row to clear it (default true). §5.1.4
    [Parameter] public bool AllowDeselectInSingleMode { get; set; } = true;
    // Checkbox column placement (Left default; Right deferred visual in 4a). §5.6.1
    [Parameter] public SelectionPlacement SelectionPlacement { get; set; } = SelectionPlacement.Left;

    // Fires on plain click of a row's BODY (not the checkbox). Ctrl/Cmd-click and Shift-click
    // on the body drive selection instead of firing this. §5.4.4 / §23.2.1
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }

    // ── Pagination (Stage 7 / LP-15) — params only; rendering wired in later steps ──
    // Placement: Bottom default; Top/Both/Left/Right/None per LP-15 Q1. LipiTable owns rendering
    // (None = escape hatch, consumer composes externally via @bind). §8.4.
    [Parameter] public PaginationPlacement Placement { get; set; } = PaginationPlacement.Bottom;

    // Variant of the embedded pager (Full default). In Left/Right placement, Full auto-downgrades
    // to Compact with a Dev-mode warning (handled in the rendering step, not here). §8.5 / LP-15 Q1.
    [Parameter] public PaginationVariant Variant { get; set; } = PaginationVariant.Full;

    // Style/behavior overrides for the embedded pager (null = LipiPagination's own defaults).
    // Embedded default stays Bordered+Solid+Auto (isolation §25); HIS opts into LipiHisDefault. LP-15 Q6 / LP-20.
    [Parameter] public PaginationOptions? PaginationOptions { get; set; }

    // Side-pager column width for Left/Right placement (fits ~36px square buttons + padding). LP-15 Q1.
    [Parameter] public string PaginationSideWidth { get; set; } = "48px";

    // Reserve height for the non-data body states (empty / loading / error) expressed as a
    // ROW COUNT, not a raw length: the table multiplies by its own density-driven row height
    // (--lipi-table-row-min-h), so the reserved space matches real-row rhythm at any density and
    // can't be an odd CSS value. Null/0 = content-driven (no reserved height). Set near the page
    // size to keep the table's footprint stable across full <-> empty <-> loading (no layout jump).
    [Parameter] public int? EmptyRows { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    // ── JS interop for clipboard (component-local lipi-table.js) ──────────
    [Inject] public Microsoft.JSInterop.IJSRuntime? JS { get; set; }

    // ── Optional DI (env detection only) ─────────────────────────────────
    [Inject] public IHostEnvironment? HostEnvironment { get; set; }
    [Inject] public ILogger<LipiTable<TItem>>? Logger { get; set; }

    // ── Internal column registry ─────────────────────────────────────────
    // Insertion-ordered, keyed by ColumnKey so re-registration (param change) replaces
    // rather than duplicates. LipiColumn registers during its OnInitialized.
    private readonly List<ColumnDefinition<TItem>> _columns = new();

    // ══════════════════════════════════════════════════════════════════════
    //  Column filters — Stage S3 (HeaderIcon + text/universal operators).
    //  Pipeline: Items → quicksearch → FILTERS → sort → page. One descriptor per filtered
    //  column. MatchesFilter is the extension point — S3b adds numeric/date/boolean/In cases.
    // ══════════════════════════════════════════════════════════════════════
    private readonly List<FilterDescriptor> _filters = new();
    private string? _openFilterKey;        // column key whose popover is open (null = none)
    private readonly Dictionary<string, ElementReference> _filterBtnRefs = new();
    // Fixed-position coordinates for the open popover (escapes the table's overflow:hidden).
    private double _popoverTop, _popoverBottom, _popoverRight;
    private bool _popoverAbove;   // open upward when near the viewport bottom
    private DotNetObjectReference<LipiTable<TItem>>? _selfRef;
    private System.Threading.CancellationTokenSource? _filterLiveCts;

    /// <summary>Active filters (read-only view for consumers / persistence).</summary>
    public IReadOnlyList<FilterDescriptor> CurrentFilters => _filters.ToArray();

    private bool FiltersActive => _filters.Count > 0;

    // Toolbar shows when quick search is on, OR when inline chips need a home and filters are active.
    private bool ChipsInline => FilterChipsPlacement == FilterChipsPlacement.Inline;
    // Inline chips need a toolbar to sit in; that requires ShowQuickSearch (the toolbar's reason
    // to exist). Inline placement without a toolbar falls back to the separate strip.
    private bool ShowInlineChips   => FiltersActive && ChipsInline && ShowQuickSearch;
    private bool ShowSeparateChips => FiltersActive && !ShowInlineChips;
    // Toolbar also renders in Drawer mode so the filter trigger button has a home (PR3).
    private bool ShowToolbar       => ShowQuickSearch || FilterMode == FilterMode.Drawer;
    private FilterDescriptor? FilterFor(string columnKey) =>
        _filters.FirstOrDefault(f => f.ColumnKey == columnKey);

    // The filtered view (applied AFTER quick search, BEFORE sort). No filters → unchanged.
    private IReadOnlyList<TItem> FilteredItems
    {
        get
        {
            var searched = SearchedItems;
            if (searched.Count == 0 || _filters.Count == 0) return searched;
            return searched.Where(row =>
                _filters.All(f =>
                {
                    var def = _columns.FirstOrDefault(c => c.ColumnKey == f.ColumnKey);
                    if (def is null || !def.Filterable) return true;   // unknown col → ignore filter
                    return MatchesFilter(def.GetValue(row), f);
                })).ToList();
        }
    }

    // Operator dispatch. S3a: text + universal. S3b extends (numeric/date/boolean/In).
    private bool MatchesFilter(object? value, FilterDescriptor f)
    {
        var sc = FilterCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        // ── Universal (value-independent) ──
        switch (f.Operator)
        {
            case FilterOperator.Empty:
                return value is null || string.IsNullOrEmpty(value.ToString());
            case FilterOperator.NotEmpty:
                return value is not null && !string.IsNullOrEmpty(value.ToString());
            case FilterOperator.IsTrue:
                return value is bool bt && bt;
            case FilterOperator.IsFalse:
                return value is bool bf && !bf;
        }

        // ── Set membership (In): Value is IReadOnlyList<object> of selected tokens ──
        if (f.Operator == FilterOperator.In)
        {
            if (f.Value is not System.Collections.IEnumerable items) return true;
            var sv = value?.ToString();
            foreach (var it in items)
                if (string.Equals(it?.ToString(), sv, sc)) return true;
            return false;
        }

        // ── Numeric operators ──
        if (IsNumericOperator(f.Operator) && TryToDouble(value, out var nv))
        {
            if (f.Operator == FilterOperator.Between)
            {
                if (!TryToDouble(f.Value, out var lo) || !TryToDouble(f.ValueEnd, out var hi)) return true;
                if (lo > hi) (lo, hi) = (hi, lo);
                return nv >= lo && nv <= hi;
            }
            if (!TryToDouble(f.Value, out var cmp)) return true;
            return f.Operator switch
            {
                FilterOperator.Equals             => nv == cmp,
                FilterOperator.NotEquals          => nv != cmp,
                FilterOperator.GreaterThan        => nv > cmp,
                FilterOperator.GreaterThanOrEqual => nv >= cmp,
                FilterOperator.LessThan           => nv < cmp,
                FilterOperator.LessThanOrEqual    => nv <= cmp,
                _ => true
            };
        }

        // ── Date operators (explicit On/Before/After/Between + relative set) ──
        if (TryToDate(value, out var dv))
        {
            // Relative operators resolve a window from "filter today".
            if (IsRelativeDateOperator(f.Operator))
            {
                var (rs, re) = RelativeWindow(f.Operator, f.Value);
                return dv >= rs && dv <= re;
            }
            if (IsExplicitDateOperator(f.Operator))
            {
                if (f.Operator == FilterOperator.Between)
                {
                    if (!TryToDate(f.Value, out var ds) || !TryToDate(f.ValueEnd, out var de)) return true;
                    if (ds > de) (ds, de) = (de, ds);
                    return dv >= ds && dv <= de;
                }
                if (!TryToDate(f.Value, out var d1)) return true;
                return f.Operator switch
                {
                    FilterOperator.Equals       => dv == d1,
                    FilterOperator.NotEquals    => dv != d1,
                    FilterOperator.GreaterThan  => dv > d1,   // After
                    FilterOperator.LessThan     => dv < d1,   // Before
                    FilterOperator.GreaterThanOrEqual => dv >= d1,
                    FilterOperator.LessThanOrEqual    => dv <= d1,
                    _ => true
                };
            }
        }

        // ── Text operators (S3a behavior) ──
        var s = value?.ToString();
        var term = f.Value?.ToString();
        if (string.IsNullOrEmpty(term)) return true;   // no constraint
        if (s is null) return false;
        return f.Operator switch
        {
            FilterOperator.Contains    => s.IndexOf(term, sc) >= 0,
            FilterOperator.NotContains => s.IndexOf(term, sc) < 0,
            FilterOperator.Equals      => string.Equals(s, term, sc),
            FilterOperator.NotEquals   => !string.Equals(s, term, sc),
            FilterOperator.StartsWith  => s.StartsWith(term, sc),
            FilterOperator.EndsWith    => s.EndsWith(term, sc),
            _ => true
        };
    }

    // ── S3b typed-coercion + relative-date helpers ────────────────────────
    private static bool TryToDouble(object? v, out double d)
    {
        switch (v)
        {
            case null: d = 0; return false;
            case double dd: d = dd; return true;
            case float f:   d = f;  return true;
            case decimal m: d = (double)m; return true;
            case int i:     d = i;  return true;
            case long l:    d = l;  return true;
            case short s:   d = s;  return true;
            case byte b:    d = b;  return true;
        }
        return double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d)
            || double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
    }

    private static bool TryToDate(object? v, out DateOnly d)
    {
        switch (v)
        {
            case DateOnly db:        d = db; return true;
            case DateTime dt:        d = DateOnly.FromDateTime(dt); return true;
            case DateTimeOffset dto: d = DateOnly.FromDateTime(dto.DateTime); return true;
            case null:               d = default; return false;
        }
        // string fallback: try ISO then invariant
        if (DateOnly.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return true;
        if (DateTime.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
        { d = DateOnly.FromDateTime(dt2); return true; }
        return false;
    }

    private static bool IsNumericOperator(FilterOperator op) => op is
        FilterOperator.Equals or FilterOperator.NotEquals or
        FilterOperator.GreaterThan or FilterOperator.GreaterThanOrEqual or
        FilterOperator.LessThan or FilterOperator.LessThanOrEqual or FilterOperator.Between;

    private static bool IsExplicitDateOperator(FilterOperator op) => op is
        FilterOperator.Equals or FilterOperator.NotEquals or
        FilterOperator.GreaterThan or FilterOperator.LessThan or
        FilterOperator.GreaterThanOrEqual or FilterOperator.LessThanOrEqual or
        FilterOperator.Between;

    private static bool IsRelativeDateOperator(FilterOperator op) => op is
        FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow or
        FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek or
        FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth or
        FilterOperator.ThisQuarter or FilterOperator.LastQuarter or
        FilterOperator.ThisYear or FilterOperator.LastYear or
        FilterOperator.LastNDays or FilterOperator.NextNDays;

    // Relative date operators in display order — gated per-column by RelativeDateSpans (S3b+).
    private static readonly FilterOperator[] RelativeOperatorsInOrder =
    {
        FilterOperator.Today, FilterOperator.Yesterday, FilterOperator.Tomorrow,
        FilterOperator.ThisWeek, FilterOperator.LastWeek, FilterOperator.NextWeek,
        FilterOperator.ThisMonth, FilterOperator.LastMonth, FilterOperator.NextMonth,
        FilterOperator.ThisQuarter, FilterOperator.LastQuarter,
        FilterOperator.ThisYear, FilterOperator.LastYear,
        FilterOperator.LastNDays, FilterOperator.NextNDays,
    };

    // Which DateSpan bucket a relative operator belongs to (Day groups the day-grain + Last-N/Next-N).
    private static DateSpan SpanOf(FilterOperator op) => op switch
    {
        FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow
            or FilterOperator.LastNDays or FilterOperator.NextNDays => DateSpan.Day,
        FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek => DateSpan.Week,
        FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth => DateSpan.Month,
        FilterOperator.ThisQuarter or FilterOperator.LastQuarter => DateSpan.Quarter,
        FilterOperator.ThisYear or FilterOperator.LastYear => DateSpan.Year,
        _ => DateSpan.None
    };

    // Resolve a [start,end] DateOnly window for a relative operator, anchored at FilterToday.
    // Week boundaries use FilterWeekStart; quarters are calendar quarters here (filter context).
    private (DateOnly start, DateOnly end) RelativeWindow(FilterOperator op, object? nValue)
    {
        var today = FilterToday;
        DateOnly Eom(int y, int m) => new(y, m, DateTime.DaysInMonth(y, m));
        DateOnly WkStart(DateOnly d) { int delta = ((int)d.DayOfWeek - (int)FilterWeekStart + 7) % 7; return d.AddDays(-delta); }
        int n() { if (nValue is int i) return i; int.TryParse(nValue?.ToString(), out var k); return k; }

        switch (op)
        {
            case FilterOperator.Today:     return (today, today);
            case FilterOperator.Yesterday: return (today.AddDays(-1), today.AddDays(-1));
            case FilterOperator.Tomorrow:  return (today.AddDays(1), today.AddDays(1));
            case FilterOperator.ThisWeek:  { var s = WkStart(today); return (s, s.AddDays(6)); }
            case FilterOperator.LastWeek:  { var s = WkStart(today).AddDays(-7); return (s, s.AddDays(6)); }
            case FilterOperator.NextWeek:  { var s = WkStart(today).AddDays(7); return (s, s.AddDays(6)); }
            case FilterOperator.ThisMonth: { var s = new DateOnly(today.Year, today.Month, 1); return (s, Eom(s.Year, s.Month)); }
            case FilterOperator.LastMonth: { var s = new DateOnly(today.Year, today.Month, 1).AddMonths(-1); return (s, Eom(s.Year, s.Month)); }
            case FilterOperator.NextMonth: { var s = new DateOnly(today.Year, today.Month, 1).AddMonths(1); return (s, Eom(s.Year, s.Month)); }
            case FilterOperator.ThisQuarter: { int q = (today.Month - 1) / 3; var s = new DateOnly(today.Year, q * 3 + 1, 1); var e = s.AddMonths(3).AddDays(-1); return (s, e); }
            case FilterOperator.LastQuarter: { int q = (today.Month - 1) / 3; var ts = new DateOnly(today.Year, q * 3 + 1, 1); var s = ts.AddMonths(-3); return (s, ts.AddDays(-1)); }
            case FilterOperator.ThisYear:  return (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31));
            case FilterOperator.LastYear:  return (new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31));
            case FilterOperator.LastNDays: { int k = Math.Max(0, n()); return (today.AddDays(-k), today); }
            case FilterOperator.NextNDays: { int k = Math.Max(0, n()); return (today, today.AddDays(k)); }
            default: return (today, today);
        }
    }

    // "Today" for relative date filters. Resolved from the table's FilterTimeZone if set
    // (the zone that anchors DateTimeOffset date-window boundaries — the BP-search resolution:
    // the FILTER carries the zone, not the picker), else server-local today.
    private DateOnly FilterToday =>
        FilterTimeZone is { } tz
            ? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz))
            : DateOnly.FromDateTime(DateTime.Now);

    // Operators offered for a column type (S3a: text columns get the text+universal set).
    // S3b returns numeric/date/boolean/status sets. Centralized so the popover + chips agree.
    private IReadOnlyList<FilterOperator> OperatorsFor(ColumnDefinition<TItem> def)
    {
        switch (def.Type)
        {
            case ColumnType.Number:
            case ColumnType.Currency:
                return new[]
                {
                    FilterOperator.Equals, FilterOperator.NotEquals,
                    FilterOperator.GreaterThan, FilterOperator.GreaterThanOrEqual,
                    FilterOperator.LessThan, FilterOperator.LessThanOrEqual,
                    FilterOperator.Between, FilterOperator.Empty, FilterOperator.NotEmpty
                };

            case ColumnType.Date:
            case ColumnType.DateTime:
            {
                // Absolute operators first (always offered), then the relative operators
                // gated by the column's resolved RelativeDateSpans (cascade), then Empty/NotEmpty.
                var ops = new List<FilterOperator>
                {
                    FilterOperator.Equals,            // On
                    FilterOperator.LessThan,          // Before
                    FilterOperator.GreaterThan,       // After
                    FilterOperator.Between,
                };
                var spans = def.RelativeDateSpans;
                foreach (var op in RelativeOperatorsInOrder)
                    if ((spans & SpanOf(op)) != 0)
                        ops.Add(op);
                ops.Add(FilterOperator.Empty);
                ops.Add(FilterOperator.NotEmpty);
                return ops;
            }

            case ColumnType.Boolean:
                return new[] { FilterOperator.IsTrue, FilterOperator.IsFalse };

            case ColumnType.Status:
                // Status is a constrained string set → membership + equality.
                return new[]
                {
                    FilterOperator.In, FilterOperator.Equals, FilterOperator.NotEquals,
                    FilterOperator.Empty, FilterOperator.NotEmpty
                };

            default: // Text, Mono, Link, etc.
                return new[]
                {
                    FilterOperator.Contains, FilterOperator.NotContains,
                    FilterOperator.Equals, FilterOperator.NotEquals,
                    FilterOperator.StartsWith, FilterOperator.EndsWith,
                    FilterOperator.In,
                    FilterOperator.Empty, FilterOperator.NotEmpty
                };
        }
    }

    // Which editor a given draft operator needs (drives the popover body).
    private enum FilterEditor { None, Text, Number, NumberRange, Date, DateRange, RelativeN, Multi }

    private FilterEditor EditorFor(ColumnDefinition<TItem> def, FilterOperator op)
    {
        if (op is FilterOperator.Empty or FilterOperator.NotEmpty
               or FilterOperator.IsTrue or FilterOperator.IsFalse) return FilterEditor.None;
        if (IsRelativeDateOperator(op))
            return op is FilterOperator.LastNDays or FilterOperator.NextNDays
                ? FilterEditor.RelativeN : FilterEditor.None;
        if (op == FilterOperator.In) return FilterEditor.Multi;
        if (op == FilterOperator.Between)
            return def.Type is ColumnType.Date or ColumnType.DateTime
                ? FilterEditor.DateRange : FilterEditor.NumberRange;
        if (def.Type is ColumnType.Date or ColumnType.DateTime) return FilterEditor.Date;
        if (def.Type is ColumnType.Number or ColumnType.Currency) return FilterEditor.Number;
        return FilterEditor.Text;
    }

    // Does an operator need a value input? Empty/NotEmpty don't.
    private static bool OperatorNeedsValue(FilterOperator op)
    {
        if (op is FilterOperator.Empty or FilterOperator.NotEmpty
               or FilterOperator.IsTrue or FilterOperator.IsFalse) return false;
        // Relative date operators carry their own window — only LastNDays/NextNDays need an N.
        if (op is FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow
               or FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek
               or FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth
               or FilterOperator.ThisQuarter or FilterOperator.LastQuarter
               or FilterOperator.ThisYear or FilterOperator.LastYear) return false;
        return true;
    }

    // ── Popover open/close (one at a time; backdrop dismisses) ────────────
    private void ToggleFilterPopover(string columnKey)
        => _openFilterKey = _openFilterKey == columnKey ? null : columnKey;
    private void CloseFilterPopover()
    {
        _openFilterKey = null;
        _pendingPopoverAnchor = false;
        _popoverMeasured = false;
        if (JS is not null) { try { _ = JS.InvokeVoidAsync("lipiTable.offScrollClose"); } catch { } }
    }

    /// <summary>Invoked from JS when the page scrolls while a filter popover is open — close it.</summary>
    [Microsoft.JSInterop.JSInvokable]
    public void ClosePopoverFromJs()
    {
        if (_openFilterKey is null) return;
        CloseFilterPopover();
        StateHasChanged();
    }

    // ── Unified per-column draft model (PR1) ─────────────────────────────
    // One draft entry per column key, holding the operator + value(s) being
    // edited before Apply. Replaces the former single scalar draft set so BOTH
    // the header popover (one column open at a time) AND the upcoming filter
    // Drawer (every filterable column on screen at once) drive the same storage.
    // The popover binds to the entry for _openFilterKey; the drawer (PR3) will
    // bind each section to its own column entry. The filter ENGINE (MatchesFilter,
    // OperatorsFor, EditorFor, CoerceDraft, chips) is unchanged.
    private sealed class ColumnDraft
    {
        public FilterOperator Operator = FilterOperator.Contains;
        public string Value = string.Empty;
        public string ValueEnd = string.Empty;            // Between upper bound (S3b)
        public readonly HashSet<string> Multi = new();    // In multi-select selections (S3b)
        public DateOnly? DateStart;                        // date-range editor (option c, S3b)
        public DateOnly? DateEnd;
    }

    // Keyed by ColumnKey. Entries are ephemeral working copies — (re)seeded from
    // the applied filter when a popover/section (re)opens, so a stale entry is
    // harmless. Seeded lazily via EnsureDraft / forcibly via SeedDraft.
    private readonly Dictionary<string, ColumnDraft> _drafts = new();

    // Return the existing draft for a column, or seed one if absent. Safe to call
    // from the render path (markup) without wiping in-progress edits.
    private ColumnDraft EnsureDraft(ColumnDefinition<TItem> def)
        => _drafts.TryGetValue(def.ColumnKey, out var d) ? d : SeedDraft(def);

    // (Re)seed a column's draft from its applied filter (or the operator default
    // when unfiltered), overwriting any prior draft. Called when a popover opens
    // so the editor reflects the live filter. PR1.
    private ColumnDraft SeedDraft(ColumnDefinition<TItem> def)
    {
        var d = new ColumnDraft();
        var existing = FilterFor(def.ColumnKey);
        var ops = OperatorsFor(def);
        d.Operator = existing?.Operator ?? ops[0];
        d.ValueEnd = existing?.ValueEnd?.ToString() ?? string.Empty;
        if (existing?.Operator == FilterOperator.In && existing.Value is System.Collections.IEnumerable en)
            foreach (var it in en) { var t = it?.ToString(); if (t is not null) d.Multi.Add(t); }
        d.Value = existing?.Operator == FilterOperator.In
            ? string.Empty
            : existing?.Value?.ToString() ?? string.Empty;
        // Boolean tri-state: map an existing IsTrue/IsFalse back to the editor value; no filter → (Any).
        if (def.Type == ColumnType.Boolean)
        {
            d.Value = existing?.Operator switch
            {
                FilterOperator.IsTrue  => "true",
                FilterOperator.IsFalse => "false",
                _                      => string.Empty
            };
        }
        // Date-range editor (option c): seed start/end from an existing Between filter.
        if (def.Type is ColumnType.Date or ColumnType.DateTime)
        {
            if (existing?.Value is DateOnly ds) d.DateStart = ds;
            if (existing?.ValueEnd is DateOnly de) d.DateEnd = de;
        }
        _drafts[def.ColumnKey] = d;
        return d;
    }

    // Seed the draft when a popover opens from an existing filter (or defaults).
    private bool _pendingPopoverAnchor;   // measure the funnel rect in OnAfterRender (A48 pattern)
    private bool _popoverMeasured;        // hide the popover until measured to avoid a corner flash

    private void OpenFilterPopover(ColumnDefinition<TItem> def)
    {
        // Toggle off if already open on this column.
        if (_openFilterKey == def.ColumnKey) { _openFilterKey = null; return; }

        _openFilterKey = def.ColumnKey;
        SeedDraft(def);   // fresh draft from the applied filter (PR1 unified draft model)

        // ElementReference is only reliably usable in OnAfterRenderAsync (A48). Defer the
        // getRect measurement there; until measured, the popover stays hidden (no corner flash).
        _popoverMeasured = false;
        _pendingPopoverAnchor = true;
    }

    // Minimal DTO matching lipiTable.getRect's return shape.
    private sealed class PopoverRect
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Bottom { get; set; }
        public double Right { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double ViewportH { get; set; }
        public double ViewportW { get; set; }
    }

    private string PopoverFixedStyle
    {
        get
        {
            if (!_popoverMeasured) return "position:fixed;visibility:hidden;top:0;right:0;";
            return _popoverAbove
                ? $"position:fixed;bottom:{_popoverBottom:0}px;right:{_popoverRight:0}px;top:auto;"
                : $"position:fixed;top:{_popoverTop:0}px;right:{_popoverRight:0}px;";
        }
    }

    private async Task OnDraftOperatorChanged(string key, ChangeEventArgs e)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        if (Enum.TryParse<FilterOperator>(e.Value?.ToString(), out var op))
        {
            d.Operator = op;
            if (FilterApplyMode == FilterApplyMode.Live) await CommitDraftAsync(key);
        }
    }

    private async Task OnDraftValueChanged(string key, ChangeEventArgs e)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        d.Value = e.Value?.ToString() ?? string.Empty;
        if (FilterApplyMode == FilterApplyMode.Live) await DebouncedLiveCommitAsync(key);
    }

    private async Task DebouncedLiveCommitAsync(string key)
    {
        _filterLiveCts?.Cancel();
        var cts = new System.Threading.CancellationTokenSource();
        _filterLiveCts = cts;
        try { await Task.Delay(300, cts.Token); }
        catch (TaskCanceledException) { return; }
        if (cts.IsCancellationRequested) return;
        await CommitDraftAsync(key);
    }

    // Commit the draft into the filter set (Apply button, or Live on change).
    private async Task ApplyDraftAsync(ColumnDefinition<TItem> def)
    {
        await CommitDraftAsync(def.ColumnKey);
        if (FilterApplyMode == FilterApplyMode.Apply) CloseFilterPopover();
    }

    private async Task CommitDraftAsync(string key)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        var previous = _filters.ToArray();
        _filters.RemoveAll(f => f.ColumnKey == key);

        bool needsValue = OperatorNeedsValue(d.Operator);
        var def = _columns.FirstOrDefault(c => c.ColumnKey == key);
        var editor = def is not null ? EditorFor(def, d.Operator) : FilterEditor.Text;

        // Option (c): date columns filter via the LipiDateRangePicker → a Between window.
        // Either bound may be open (AllowOpenEnd) → fall back to After/Before when one side is null.
        if (def is not null && def.Type is ColumnType.Date or ColumnType.DateTime)
        {
            if (d.DateStart is { } s && d.DateEnd is { } e)
                _filters.Add(new FilterDescriptor(key, FilterOperator.Between, s, e));
            else if (d.DateStart is { } s2)
                _filters.Add(new FilterDescriptor(key, FilterOperator.GreaterThanOrEqual, s2, null));
            else if (d.DateEnd is { } e2)
                _filters.Add(new FilterDescriptor(key, FilterOperator.LessThanOrEqual, e2, null));
            // else: no dates picked → no filter (removed above)
            _currentPage = 1;
            await FireFilterChanged(previous, FilterChangeReason.UserApplyPopover);
            StateHasChanged();
            return;
        }

        // Boolean tri-state: "true"/"false" → IsTrue/IsFalse; "" → (Any) → no filter (left removed).
        if (def is not null && def.Type == ColumnType.Boolean)
        {
            if (d.Value == "true")
                _filters.Add(new FilterDescriptor(key, FilterOperator.IsTrue, null, null));
            else if (d.Value == "false")
                _filters.Add(new FilterDescriptor(key, FilterOperator.IsFalse, null, null));
            _currentPage = 1;
            await FireFilterChanged(previous, FilterChangeReason.UserApplyPopover);
            StateHasChanged();
            return;
        }

        if (!needsValue)
        {
            // Value-independent (Empty/NotEmpty/IsTrue/IsFalse) or self-contained relative window.
            _filters.Add(new FilterDescriptor(key, d.Operator, null, null));
        }
        else if (editor == FilterEditor.Multi)
        {
            if (d.Multi.Count > 0)
                _filters.Add(new FilterDescriptor(key, d.Operator,
                    (IReadOnlyList<object>)d.Multi.Cast<object>().ToList(), null));
        }
        else if (editor is FilterEditor.NumberRange or FilterEditor.DateRange)
        {
            if (!string.IsNullOrEmpty(d.Value) && !string.IsNullOrEmpty(d.ValueEnd))
                _filters.Add(new FilterDescriptor(key, d.Operator,
                    CoerceDraft(editor, d.Value), CoerceDraft(editor, d.ValueEnd)));
        }
        else if (!string.IsNullOrEmpty(d.Value))
        {
            _filters.Add(new FilterDescriptor(key, d.Operator,
                CoerceDraft(editor, d.Value), null));
        }
        _currentPage = 1;   // filter change resets to page 1
        await FireFilterChanged(previous, FilterChangeReason.UserApplyPopover);
        StateHasChanged();
    }

    // Distinct non-null string values for a column (drives the In multi-select editor).
    // Computed off the pre-filter Items so the option set is stable as filters apply.
    private IReadOnlyList<string> DistinctValuesFor(ColumnDefinition<TItem> def)
    {
        if (Items is null) return Array.Empty<string>();
        var set = new SortedSet<string>(StringComparer.CurrentCulture);
        foreach (var row in Items)
        {
            var v = def.GetValue(row)?.ToString();
            if (!string.IsNullOrEmpty(v)) set.Add(v);
        }
        return set.ToList();
    }

    private void ToggleDraftMulti(string key, string token)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        if (!d.Multi.Remove(token)) d.Multi.Add(token);
    }

    // Draft-end change handler (Between upper bound).
    private async Task OnDraftValueEndChanged(string key, ChangeEventArgs e)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        d.ValueEnd = e.Value?.ToString() ?? string.Empty;
        if (FilterApplyMode == FilterApplyMode.Live) await DebouncedLiveCommitAsync(key);
    }

    private async Task OnDraftDateStartChanged(string key, DateOnly? value)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        d.DateStart = value;
        if (FilterApplyMode == FilterApplyMode.Live) await DebouncedLiveCommitAsync(key);
    }

    private async Task OnDraftDateEndChanged(string key, DateOnly? value)
    {
        if (!_drafts.TryGetValue(key, out var d)) return;
        d.DateEnd = value;
        if (FilterApplyMode == FilterApplyMode.Live) await DebouncedLiveCommitAsync(key);
    }

    // Coerce a draft string into the typed value the engine compares against.
    private object CoerceDraft(FilterEditor editor, string raw)
    {
        switch (editor)
        {
            case FilterEditor.Number:
            case FilterEditor.NumberRange:
                if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
                return raw;
            case FilterEditor.Date:
            case FilterEditor.DateRange:
                if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
                return raw;
            case FilterEditor.RelativeN:
                if (int.TryParse(raw, out var n)) return n;
                return 0;
            default:
                return raw;
        }
    }

    // Remove one column's filter (chip ✕ or popover Clear).
    private async Task ClearColumnFilterAsync(string columnKey)
    {
        if (_filters.All(f => f.ColumnKey != columnKey)) { CloseFilterPopover(); return; }
        var previous = _filters.ToArray();
        _filters.RemoveAll(f => f.ColumnKey == columnKey);
        _drafts.Remove(columnKey);   // drop the working draft for this column (PR1)
        _currentPage = 1;
        await FireFilterChanged(previous, FilterChangeReason.UserRemoveChip);
        StateHasChanged();
    }

    /// <summary>Clear every active filter.</summary>
    public async Task ClearAllFiltersAsync()
    {
        if (_filters.Count == 0) return;
        var previous = _filters.ToArray();
        _filters.Clear();
        _currentPage = 1;
        await FireFilterChanged(previous, FilterChangeReason.UserClearAll);
        StateHasChanged();
    }

    private async Task FireFilterChanged(IReadOnlyList<FilterDescriptor> previous, FilterChangeReason reason)
    {
        if (OnFilterChanged.HasDelegate)
            await OnFilterChanged.InvokeAsync(new FilterChangedContext(_filters.ToArray(), previous, reason));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Filter drawer — Stage S3c / PR3. Declarative <LipiDrawer> over the SAME
    //  per-column ColumnDraft model (PR1). The drawer never calls a drawer
    //  service: the parent (this table) owns _filterDrawerOpen and mounts the
    //  drawer; LipiDrawer invokes OnClose, which un-mounts it. The filter ENGINE
    //  (OperatorsFor / EditorFor / SeedDraft / CommitDraftAsync / ClearAll) is
    //  reused verbatim — A58 relative-date gating is inherited via OperatorsFor.
    // ══════════════════════════════════════════════════════════════════════
    private bool _filterDrawerOpen;

    // Distinct columns currently carrying a filter (drives the trigger badge).
    private int ActiveFilterColumnCount => _filters.Select(f => f.ColumnKey).Distinct().Count();

    // DataDisplay FilterDrawerSide → Overlays DrawerPlacement.
    private DrawerPlacement MapDrawerSide(FilterDrawerSide side) => side switch
    {
        FilterDrawerSide.Left   => DrawerPlacement.Left,
        FilterDrawerSide.Top    => DrawerPlacement.Top,
        FilterDrawerSide.Bottom => DrawerPlacement.Bottom,
        _                       => DrawerPlacement.Right
    };

    // Open: seed a working draft for every filterable column from its committed filter,
    // so the drawer editors reflect live state, then mount the drawer.
    private void OpenFilterDrawer()
    {
        foreach (var def in _columns.Where(c => c.Filterable))
            SeedDraft(def);
        _filterDrawerOpen = true;
    }

    private void CloseFilterDrawer() => _filterDrawerOpen = false;

    // Apply: commit every filterable column's draft at once, then close. (CommitDraftAsync is
    // the locked per-column engine path; in Items-only v1.0 it fires OnFilterChanged per changed
    // column — a single batched event is a possible future refinement, noted in CHANGE-LOG.)
    private async Task ApplyAllInDrawerAsync()
    {
        foreach (var def in _columns.Where(c => c.Filterable))
            await CommitDraftAsync(def.ColumnKey);
        _filterDrawerOpen = false;
    }

    // Clear all: drop every active filter, then re-seed drafts to operator defaults so the
    // still-open drawer editors visibly reset. Drawer stays open (build a fresh set immediately).
    private async Task ClearAllInDrawerAsync()
    {
        await ClearAllFiltersAsync();
        foreach (var def in _columns.Where(c => c.Filterable))
            SeedDraft(def);
        StateHasChanged();
    }

    // ── Chip rendering helpers ────────────────────────────────────────────
    private string FilterChipText(FilterDescriptor f)
    {
        var def = _columns.FirstOrDefault(c => c.ColumnKey == f.ColumnKey);
        var col = def?.Header ?? f.ColumnKey;
        var op = def is not null ? OperatorLabelFor(def, f.Operator) : OperatorLabel(f.Operator);

        // In: join the selected values rather than ToString() the list (which prints the type name).
        if (f.Operator == FilterOperator.In && f.Value is System.Collections.IEnumerable en && f.Value is not string)
        {
            var vals = en.Cast<object?>().Select(v => v?.ToString()).Where(s => !string.IsNullOrEmpty(s));
            return $"{col}: {op} {string.Join(", ", vals)}";
        }

        // Between: show both bounds.
        if (f.Operator == FilterOperator.Between)
            return $"{col}: {op} {ChipVal(f.Value)} – {ChipVal(f.ValueEnd)}";

        return OperatorNeedsValue(f.Operator)
            ? $"{col}: {op} \"{ChipVal(f.Value)}\""
            : $"{col}: {op}";
    }

    // Render a chip value: dates as dd/MM/yyyy, else ToString.
    private static string ChipVal(object? v) => v switch
    {
        null => string.Empty,
        DateOnly d => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        _ => v.ToString() ?? string.Empty
    };

    // Date columns show On/Before/After instead of equals/less-than/greater-than.
    private string OperatorLabelFor(ColumnDefinition<TItem> def, FilterOperator op)
    {
        if (def.Type is ColumnType.Date or ColumnType.DateTime)
        {
            switch (op)
            {
                case FilterOperator.Equals:      return "on";
                case FilterOperator.LessThan:    return "before";
                case FilterOperator.GreaterThan: return "after";
            }
        }
        return OperatorLabel(op);
    }

    private static string OperatorLabel(FilterOperator op) => op switch
    {
        FilterOperator.Contains    => "contains",
        FilterOperator.NotContains => "does not contain",
        FilterOperator.Equals      => "equals",
        FilterOperator.NotEquals   => "not equals",
        FilterOperator.StartsWith  => "starts with",
        FilterOperator.EndsWith    => "ends with",
        FilterOperator.Empty       => "is empty",
        FilterOperator.NotEmpty    => "is not empty",
        FilterOperator.GreaterThan => "greater than",
        FilterOperator.GreaterThanOrEqual => "≥",
        FilterOperator.LessThan    => "less than",
        FilterOperator.LessThanOrEqual    => "≤",
        FilterOperator.Between     => "between",
        FilterOperator.In          => "in",
        FilterOperator.IsTrue      => "is true",
        FilterOperator.IsFalse     => "is false",
        FilterOperator.Today       => "is today",
        FilterOperator.Yesterday   => "is yesterday",
        FilterOperator.Tomorrow    => "is tomorrow",
        FilterOperator.ThisWeek    => "this week",
        FilterOperator.LastWeek    => "last week",
        FilterOperator.NextWeek    => "next week",
        FilterOperator.ThisMonth   => "this month",
        FilterOperator.LastMonth   => "last month",
        FilterOperator.NextMonth   => "next month",
        FilterOperator.ThisQuarter => "this quarter",
        FilterOperator.LastQuarter => "last quarter",
        FilterOperator.ThisYear    => "this year",
        FilterOperator.LastYear    => "last year",
        FilterOperator.LastNDays   => "in the last N days",
        FilterOperator.NextNDays   => "in the next N days",
        _ => op.ToString().ToLowerInvariant()
    };

    // ══════════════════════════════════════════════════════════════════════
    //  Quick search — Stage S2. First pipeline stage:
    //  Items → quicksearch → sort → page. Semicolon-separated terms; each term is a
    //  literal case-insensitive substring (spaces preserved); a row matches when EVERY
    //  term matches SOME searchable column. 300ms debounce via CancellationTokenSource.
    // ══════════════════════════════════════════════════════════════════════
    private string _quickSearch = string.Empty;       // applied (debounced) value
    private System.Threading.CancellationTokenSource? _searchDebounceCts;
    private const int QuickSearchDebounceMs = 300;

    /// <summary>The applied quick-search text (read-only view).</summary>
    public string CurrentQuickSearch => _quickSearch;

    // Parsed terms: split on ';', trim, drop empties. Empty list = no filter.
    private IReadOnlyList<string> QuickSearchTerms =>
        string.IsNullOrWhiteSpace(_quickSearch)
            ? Array.Empty<string>()
            : _quickSearch.Split(';')
                          .Select(t => t.Trim())
                          .Where(t => t.Length > 0)
                          .ToArray();

    // The searched view of Items. No terms → Items unchanged. Otherwise keep rows where
    // EVERY term matches SOME searchable column's stringified value (AND across terms).
    private IReadOnlyList<TItem> SearchedItems
    {
        get
        {
            if (Items is null || Items.Count == 0) return Array.Empty<TItem>();
            var terms = QuickSearchTerms;
            if (terms.Count == 0) return Items;

            var searchCols = _columns.Where(c => c.Searchable).ToList();
            if (searchCols.Count == 0) return Items;   // nothing to search → no filtering

            return Items.Where(row => terms.All(term => RowMatchesTerm(row, term, searchCols))).ToList();
        }
    }

    private bool RowMatchesTerm(TItem row, string term, List<ColumnDefinition<TItem>> cols)
    {
        foreach (var c in cols)
        {
            var v = c.GetValue(row);
            if (v is null) continue;
            var text = v.ToString();
            if (!string.IsNullOrEmpty(text)
                && text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    // Debounced input handler wired to the toolbar box. Each keystroke cancels the prior
    // pending apply and schedules a new one 300ms out; the apply fires QuickSearchChanged.
    private async Task OnQuickSearchInput(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        var typed = e.Value?.ToString() ?? string.Empty;
        _searchDebounceCts?.Cancel();
        var cts = new System.Threading.CancellationTokenSource();
        _searchDebounceCts = cts;
        try
        {
            await Task.Delay(QuickSearchDebounceMs, cts.Token);
        }
        catch (TaskCanceledException) { return; }   // superseded by a newer keystroke
        if (cts.IsCancellationRequested) return;

        await ApplyQuickSearchAsync(typed);
    }

    private async Task ApplyQuickSearchAsync(string newValue)
    {
        if (newValue == _quickSearch) return;
        var previous = _quickSearch;
        _quickSearch = newValue;
        _currentPage = 1;   // search change resets to page 1
        if (OnQuickSearchChanged.HasDelegate)
            await OnQuickSearchChanged.InvokeAsync(new QuickSearchChangedContext(newValue, previous));
        StateHasChanged();
    }

    // Clear button.
    private Task ClearQuickSearchAsync() => ApplyQuickSearchAsync(string.Empty);

    // Highlight helper for built-in cells (Stage S2). Splits the cell text into matched/unmatched
    // segments for the active terms; the markup wraps matches in <mark>. Only called when
    // HighlightMatches is true AND quick search is active. Custom templates bypass this.
    private bool ShouldHighlight => HighlightMatches && QuickSearchTerms.Count > 0;

    private IReadOnlyList<(string Text, bool Match)> HighlightSegments(string? cellText)
    {
        if (string.IsNullOrEmpty(cellText)) return Array.Empty<(string, bool)>();
        var terms = QuickSearchTerms;
        if (terms.Count == 0) return new[] { (cellText, false) };

        // Mark every character covered by any term match.
        var covered = new bool[cellText.Length];
        foreach (var term in terms)
        {
            int idx = 0;
            while ((idx = cellText.IndexOf(term, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                for (int i = idx; i < idx + term.Length && i < covered.Length; i++) covered[i] = true;
                idx += term.Length;
            }
        }
        // Coalesce contiguous runs of same-coverage into segments.
        var segs = new List<(string, bool)>();
        int start = 0;
        for (int i = 1; i <= cellText.Length; i++)
        {
            if (i == cellText.Length || covered[i] != covered[start])
            {
                segs.Add((cellText.Substring(start, i - start), covered[start]));
                start = i;
            }
        }
        return segs;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Sort — Stage S1 (single + multi-column). Pipeline stage BEFORE paging:
    //  Items → (filter S3) → (search S2) → sort → page. Client mode only here.
    // ══════════════════════════════════════════════════════════════════════
    private readonly List<SortDescriptor> _sort = new();   // ordered by Priority (0 = primary)

    /// <summary>Current sort chain (read-only view for consumers / persistence).</summary>
    public IReadOnlyList<SortDescriptor> CurrentSort => _sort.ToArray();

    // The sorted view of Items. When no sort is active, returns Items unchanged (stable,
    // no allocation beyond the cast). Multi-column: OrderBy primary, ThenBy the rest.
    private IReadOnlyList<TItem> SortedItems
    {
        get
        {
            var src0 = FilteredItems;                 // Items → quicksearch → filters (→ sort below)
            if (src0.Count == 0) return Array.Empty<TItem>();
            if (_sort.Count == 0) return src0;

            IOrderedEnumerable<TItem>? ordered = null;
            foreach (var d in _sort.OrderBy(s => s.Priority))
            {
                var def = _columns.FirstOrDefault(c => c.ColumnKey == d.ColumnKey);
                if (def is null || !def.Sortable) continue;
                var dir = d.Direction;
                // Direction-aware compare (handles nulls per NullSortOrder for THIS direction,
                // and inverts non-null comparison for descending). No argument-swap, so
                // AlwaysFirst/Last stay absolute while FirstOnAsc/LastOnAsc flip with direction.
                var keyCmp = Comparer<TItem>.Create(
                    (x, y) => CompareRows(def.GetValue(x), def.GetValue(y), def, dir));
                ordered = ordered is null
                    ? src0.OrderBy(x => x, keyCmp)
                    : ordered.ThenBy(x => x, keyCmp);
            }
            return ordered is null ? src0 : ordered.ToList();
        }
    }

    // Direction-aware row comparison honoring the erased comparer (if any) and NullSortOrder.
    // Returns the final comparison already adjusted for direction, so OrderBy uses it directly.
    private int CompareRows(object? a, object? b, ColumnDefinition<TItem> def, SortDirection dir)
    {
        bool an = a is null, bn = b is null;
        if (an && bn) return 0;
        if (an || bn)
        {
            // Where does null go for THIS direction? AlwaysFirst/Last are absolute (ignore dir).
            // FirstOnAsc → nulls first when ascending, last when descending (and vice-versa).
            bool nullsFirst = NullSortOrder switch
            {
                NullSortOrder.AlwaysFirst => true,
                NullSortOrder.AlwaysLast  => false,
                NullSortOrder.FirstOnAsc  => dir == SortDirection.Ascending,
                NullSortOrder.LastOnAsc   => dir != SortDirection.Ascending,
                _ => false
            };
            // a null and nulls-first → a comes first (-1).
            int r = an ? -1 : 1;
            return nullsFirst ? r : -r;
        }

        int cmp = def.SortComparer is not null
            ? def.SortComparer(a, b)
            : (a is IComparable c ? c.CompareTo(b) : string.CompareOrdinal(a!.ToString() ?? string.Empty, b!.ToString() ?? string.Empty));
        return dir == SortDirection.Ascending ? cmp : -cmp;
    }

    // Header-click sort handler. Plain click cycles this column (replacing the chain unless
    // shift is held for multi-column); shift-click adds/cycles within the chain.
    private async Task OnHeaderSortAsync(ColumnDefinition<TItem> def, bool shiftKey)
    {
        if (!def.Sortable) return;
        var previous = _sort.ToArray();
        bool additive = shiftKey && MultiColumnSort;

        var existing = _sort.FirstOrDefault(s => s.ColumnKey == def.ColumnKey);

        if (!additive)
        {
            // Single-column: clear others, cycle this one.
            if (existing is null)
            {
                _sort.Clear();
                _sort.Add(new SortDescriptor(def.ColumnKey, SortDirection.Ascending, 0));
            }
            else
            {
                var next = NextDirection(existing.Direction, isActive: true);
                _sort.Clear();
                if (next is { } dir) _sort.Add(new SortDescriptor(def.ColumnKey, dir, 0));
            }
        }
        else
        {
            // Multi-column: cycle this column within the chain, keep the rest.
            if (existing is null)
            {
                int nextPriority = _sort.Count == 0 ? 0 : _sort.Max(s => s.Priority) + 1;
                _sort.Add(new SortDescriptor(def.ColumnKey, SortDirection.Ascending, nextPriority));
            }
            else
            {
                var next = NextDirection(existing.Direction, isActive: true);
                _sort.Remove(existing);
                if (next is { } dir)
                    _sort.Add(new SortDescriptor(def.ColumnKey, dir, existing.Priority));
                else
                    Renumber();   // removed → close the priority gap
            }
        }

        _currentPage = 1;   // sort change resets to page 1
        if (OnSortChanged.HasDelegate)
            await OnSortChanged.InvokeAsync(new SortChangedContext(
                _sort.ToArray(), previous,
                shiftKey ? SortChangeReason.UserShiftClickHeader : SortChangeReason.UserClickHeader));
        StateHasChanged();
    }

    // ThreeState: asc → desc → (unsorted=null). TwoState: asc → desc → asc (never unsorted).
    private SortDirection? NextDirection(SortDirection current, bool isActive)
    {
        if (SortCycleMode == SortCycleMode.TwoState)
            return current == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        // ThreeState
        return current == SortDirection.Ascending ? SortDirection.Descending : (SortDirection?)null;
    }

    private void Renumber()
    {
        var reordered = _sort.OrderBy(s => s.Priority).ToList();
        _sort.Clear();
        for (int i = 0; i < reordered.Count; i++)
            _sort.Add(reordered[i] with { Priority = i });
    }

    /// <summary>Programmatically clear all sort.</summary>
    public async Task ClearSortAsync()
    {
        if (_sort.Count == 0) return;
        var previous = _sort.ToArray();
        _sort.Clear();
        if (OnSortChanged.HasDelegate)
            await OnSortChanged.InvokeAsync(new SortChangedContext(
                Array.Empty<SortDescriptor>(), previous, SortChangeReason.UserClearAll));
        StateHasChanged();
    }

    // Header indicator helpers (consumed by the markup).
    private SortDescriptor? SortFor(string columnKey) => _sort.FirstOrDefault(s => s.ColumnKey == columnKey);
    private bool IsMultiSort => _sort.Count > 1;

    // ══════════════════════════════════════════════════════════════════════
    //  Pagination — Stage 7 client-mode slicing + embedded pager (Step 2+3)
    //  Client mode only here (Items path). Server (DataSource) is Step 5.
    //  Seam note: when sort/filter are built later, they compose BEFORE the
    //  slice — feed a filtered+sorted sequence into PagedItems, the loop is
    //  unchanged. Pipeline order will be: Items → filter → sort → page.
    // ══════════════════════════════════════════════════════════════════════
    private int _currentPage = 1;          // 1-based
    private int? _pageSize;                 // null until resolved from options (see ResolvedPageSize)

    // Table's own default page size (independent of LipiPagination's 5-start ladder).
    // 10 is the table convention (AntDesign). Consumer overrides via PaginationOptions.DefaultPageSize.
    private const int DefaultTablePageSize = 10;

    /// <summary>The effective page size: explicit local state → options default → table default.</summary>
    private int ResolvedPageSize =>
        _pageSize ?? PaginationOptions?.DefaultPageSize ?? DefaultTablePageSize;

    /// <summary>Total row count in client mode (server mode supplies its own — Step 5).</summary>
    private int PaginationTotalCount => FilteredItems.Count;

    /// <summary>Is pagination active at all? (Has a placement other than None and data to page.)</summary>
    private bool PaginationEnabled => Placement != PaginationPlacement.None;

    /// <summary>
    /// The rows to actually render: in client mode, the current page's slice of Items.
    /// "All" (int.MaxValue) page size returns the whole set. When pagination is disabled
    /// (Placement=None with no external binding) we still slice so @bind-driven external
    /// pagers work; an unsliced view would ignore the consumer's page state.
    /// </summary>
    private IReadOnlyList<TItem> PagedItems
    {
        get
        {
            var source = SortedItems;                               // Items → sort (→ filter/search later)
            if (source.Count == 0) return Array.Empty<TItem>();
            int size = ResolvedPageSize;
            if (size == int.MaxValue) return source;                // "All"
            int skip = (_currentPage - 1) * size;
            if (skip < 0) skip = 0;
            return source.Skip(skip).Take(size).ToList();
        }
    }

    // Handlers wired to the embedded LipiPagination's two-way binds.
    private void OnPaginationPageChanged(int newPage)
    {
        if (newPage == _currentPage) return;
        _currentPage = newPage;
        StateHasChanged();
    }

    // Total pages for the current data + page size (1 when "All" or empty). Used by keyboard nav.
    private int PaginationTotalPages
    {
        get
        {
            int size = ResolvedPageSize;
            int total = PaginationTotalCount;
            if (size == int.MaxValue || total == 0) return 1;
            return (int)Math.Ceiling(total / (double)size);
        }
    }

    // Keyboard page navigation (Stage 7, §19.3.1). Changes the page and lands focus on the
    // first cell of the new page (LP-19: UserKeyboard reason → focus moves to first row).
    // Returns true if the page actually changed (so the caller suppresses the key).
    private bool NavigateToPageByKeyboard(int targetPage)
    {
        if (Placement == PaginationPlacement.None) return false;     // no embedded pager to drive
        int clamped = Math.Clamp(targetPage, 1, PaginationTotalPages);
        if (clamped == _currentPage) return false;
        _currentPage = clamped;
        // Land focus on the first cell of the new page (col 0 keeps the selection column if present).
        _focusRow = 0;
        _focusCol = 0;
        _pendingFocus = true;
        return true;
    }

    private void OnPaginationPageSizeChanged(int newSize)
    {
        if (newSize == ResolvedPageSize) return;
        _pageSize = newSize;
        _currentPage = 1;          // LP-18: page-size change always resets to page 1
        StateHasChanged();
    }

    // ── Embedded-pager resolution (LP-15 Q1/Q6) ───────────────────────────
    private bool IsSidePlacement =>
        Placement is PaginationPlacement.Left or PaginationPlacement.Right;

    /// <summary>Orientation for the embedded pager: Vertical for Left/Right, else Horizontal.</summary>
    private PaginationOrientation ResolvedOrientation =>
        IsSidePlacement ? PaginationOrientation.Vertical : PaginationOrientation.Horizontal;

    /// <summary>Effective variant: Full auto-downgrades to Compact in side placement (LP-15 Q1).
    /// Dev-mode warns once per render path; production downgrades silently (A14 env-gated pattern).</summary>
    private PaginationVariant ResolvedVariant
    {
        get
        {
            if (IsSidePlacement && Variant == PaginationVariant.Full)
            {
                if (IsDevelopment)
                    Logger?.LogWarning(
                        "LipiTable: Variant=Full is not compatible with Placement=Left|Right. " +
                        "Auto-downgrading to Compact. To suppress this warning, set Variant=Compact " +
                        "or Variant=Minimal explicitly.");
                return PaginationVariant.Compact;
            }
            return Variant;
        }
    }

    // Resolved per-axis style (PaginationOptions null members fall back to LipiPagination's own defaults).
    private PaginationCellStyle ResolvedCellStyle =>
        PaginationOptions?.CellStyle ?? PaginationCellStyle.Bordered;
    private PaginationActiveStyle ResolvedActiveStyle =>
        PaginationOptions?.ActiveStyle ?? PaginationActiveStyle.Solid;
    private PaginationChevronStyle ResolvedChevronStyle =>
        PaginationOptions?.ChevronStyle ?? PaginationChevronStyle.Auto;

    // Show flags: side placement forces ShowPageSize off (dropdown too wide for a narrow column).
    private bool ResolvedShowPageSize =>
        IsSidePlacement ? false : (PaginationOptions?.ShowPageSize ?? true);
    private bool ResolvedShowRowCount => PaginationOptions?.ShowRowCount ?? true;
    private bool ResolvedShowFirstLast => PaginationOptions?.ShowFirstLast ?? true;
    private bool ResolvedShowJumpToPage => PaginationOptions?.ShowJumpToPage ?? false;
    private int ResolvedSiblings => PaginationOptions?.Siblings ?? 1;
    private int ResolvedBoundaryCount => PaginationOptions?.BoundaryCount ?? 1;
    private IReadOnlyList<PageSizeOption>? ResolvedPageSizeOptions => PaginationOptions?.PageSizeOptions;
    private PaginationAlign ResolvedAlign => PaginationOptions?.Align ?? PaginationAlign.SpaceBetween;

    /// <summary>True when the embedded pager should render at the top (Top or Both).</summary>
    private bool ShowTopPager => PaginationEnabled && Placement is PaginationPlacement.Top or PaginationPlacement.Both;
    /// <summary>True when the embedded pager should render at the bottom (Bottom or Both).</summary>
    private bool ShowBottomPager => PaginationEnabled && Placement is PaginationPlacement.Bottom or PaginationPlacement.Both;
    /// <summary>True when the embedded pager is a side column (Left or Right).</summary>
    private bool ShowSidePager => PaginationEnabled && IsSidePlacement;

    /// <summary>Wrap layout modifier: side placement switches the wrap to a horizontal row
    /// (grid + side pager), left/right ordering handled by the slot order in markup.</summary>
    // EmptyRows behavior (Stage 7):
    //  • unset/0  → content-driven (no reserved height, full illustration).
    //  • 1–2      → COMPACT: exact N-row height + small inline text (no big illustration);
    //               overflow clipped so it stays a thin strip. For "small 1-row empty table".
    //  • 3+       → reserve a FLOOR of N rows (min-height) and show the full illustration,
    //               which can grow taller if it needs to (never clipped).
    private const int CompactEmptyMaxRows = 2;
    private bool HasEmptyRows => EmptyRows is int n && n > 0;
    private bool IsCompactEmpty => EmptyRows is int n && n > 0 && n <= CompactEmptyMaxRows;

    // CSS var carrying the row count for the statewrap height calc (emitted only when set).
    private string? EmptyRowsStyleVar =>
        HasEmptyRows ? $"--lipi-table-empty-rows:{EmptyRows};" : null;

    // Mode class on the table root so CSS picks exact-clip (compact) vs floor (full) height.
    private string EmptyRowsModeClass =>
        !HasEmptyRows ? string.Empty
        : IsCompactEmpty ? " lipi-table--empty-compact"
        : " lipi-table--empty-reserve";

    // Compact one-line text per state (used only in compact mode).
    private string CompactStateText => ResolveBodyState() switch
    {
        BodyState.Loading => "Loading...",
        BodyState.Error   => string.IsNullOrWhiteSpace(LoadError) ? "Couldn't load" : LoadError!,
        _                 => EmptyTitle
    };

    private string PaginationWrapModifier => IsSidePlacement
        ? " lipi-table-pagination-wrap--side"
        : string.Empty;

    private bool IsDevelopment =>
        HostEnvironment?.EnvironmentName is { } env &&
        string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);

    // Called by LipiColumn during its OnInitialized.
    internal void RegisterColumn(ColumnDefinition<TItem> def)
    {
        var existing = _columns.FindIndex(c => c.ColumnKey == def.ColumnKey);
        if (existing >= 0) _columns[existing] = def;
        else _columns.Add(def);
        StateHasChanged();
    }

    internal void UnregisterColumn(string columnKey)
    {
        var idx = _columns.FindIndex(c => c.ColumnKey == columnKey);
        if (idx >= 0)
        {
            _columns.RemoveAt(idx);
            StateHasChanged();
        }
    }

    // Visible columns in registration order (the only order this stage — reorder is later).
    private IReadOnlyList<ColumnDefinition<TItem>> VisibleColumns =>
        _columns.Where(c => c.Visible).ToList();

    // ── Validation (§3.1.3) ──────────────────────────────────────────────
    protected override void OnParametersSet()
    {
        // Exactly one of Items / DataSource.
        if (Items is not null && DataSource is not null)
        {
            Fail("LipiTable: specify exactly one of Items or DataSource, not both.");
        }

        // Server-side DataSource not active in the bare chassis.
        if (DataSource is not null && Items is null)
        {
            if (IsDevelopment)
                throw new NotSupportedException(
                    "LipiTable: server-side DataSource is not yet implemented in this build " +
                    "stage (bare chassis). Use the Items parameter for now.");
            Logger?.LogWarning(
                "LipiTable: DataSource supplied but server-side mode is not implemented in this build stage; rendering empty.");
        }

        // KeySelector required.
        if (KeySelector is null)
        {
            Fail("LipiTable requires a KeySelector (Func<TItem, object>) — used for row identity, " +
                 "selection, edit-tracking, and persistence.");
        }

        // Selection: if the caller two-way-binds SelectedKeys, sync the inbound value into the
        // internal set on first set / external change. We compare by content so caller-driven
        // updates flow in, but our own outbound writes (which set _lastPushedKeys) don't loop.
        SyncIncomingSelectedKeys();

        // Anchor reset (§5.4.2): if the anchor row's key is no longer in the data, drop it.
        if (_anchorKey is not null && Items is not null
            && !Items.Any(r => Equals(RowKey(r), _anchorKey)))
        {
            _anchorKey = null;
        }

        // Pagination: clamp current page into range when data/size changes (the embedded
        // LipiPagination also self-clamps, but the table keeps its own state consistent).
        ClampCurrentPage();
    }

    private void ClampCurrentPage()
    {
        int size = ResolvedPageSize;
        int total = PaginationTotalCount;
        int totalPages = (size == int.MaxValue || total == 0)
            ? 1
            : (int)Math.Ceiling(total / (double)size);
        if (_currentPage > totalPages) _currentPage = totalPages;
        if (_currentPage < 1) _currentPage = 1;
    }

    private void Fail(string message)
    {
        if (IsDevelopment) throw new InvalidOperationException(message);
        Logger?.LogWarning("{Message}", message);
    }

    // ── Body-state resolution (§18.1 priority) ───────────────────────────
    private enum BodyState { Loading, Error, Empty, Normal }

    private BodyState ResolveBodyState()
    {
        if (IsLoading) return BodyState.Loading;
        if (!string.IsNullOrEmpty(LoadError)) return BodyState.Error;
        if (Items is null || Items.Count == 0) return BodyState.Empty;
        return BodyState.Normal;
    }

    // ── CSS Grid track template (§3.6.1) ─────────────────────────────────
    private string GridTemplateColumns =>
        VisibleColumns.Count == 0
            ? "1fr"
            : string.Join(" ", VisibleColumns.Select(c => c.GridTrack));

    // ── Row identity ─────────────────────────────────────────────────────
    private object RowKey(TItem item) => KeySelector?.Invoke(item) ?? item!;

    // ── Cell rendering helpers (used by markup) ──────────────────────────
    private static string AlignClass(ColumnAlign a) => a switch
    {
        ColumnAlign.Right => "lipi-table-cell--right",
        ColumnAlign.Center => "lipi-table-cell--center",
        _ => "lipi-table-cell--left"
    };

    private string RootClass
    {
        get
        {
            var cls = "lipi-table";
            if (StripedRows) cls += " lipi-table--striped";
            if (SelectionMode != SelectionMode.None) cls += " lipi-table--selectable";
            if (!string.IsNullOrWhiteSpace(Class)) cls += $" {Class}";
            return cls;
        }
    }

    // ── Density (§14.3) → data-density attribute on root ─────────────────
    private string DensityAttr => Density switch
    {
        TableDensity.Compact => "compact",
        TableDensity.Spacious => "spacious",
        _ => "comfortable"
    };

    // ── Selection column visible? (Stage 3: structure only, inert) ───────
    private bool HasSelectionColumn => SelectionMode != SelectionMode.None;

    // The grid template gains a leading selection track when a selection column shows.
    private string EffectiveGridTemplate
    {
        get
        {
            var cols = GridTemplateColumns;
            return HasSelectionColumn
                ? $"var(--lipi-table-select-w) {cols}"
                : cols;
        }
    }

    // ── Row formatting hooks (§22.3–22.5) ────────────────────────────────
    private string RowCssClass(TItem item)
    {
        var cls = "lipi-table-row";
        if (RowStatus is not null) cls += $" lipi-status-strip-{StatusPlacementClass}";
        if (IsRowSelected(item)) cls += " lipi-table-row--selected";
        if (OnRowClick.HasDelegate && !IsRowDisabled(item)) cls += " lipi-table-row--clickable";
        if (RowDisabled?.Invoke(item) == true) cls += " lipi-table-row--disabled";
        var caller = RowClass?.Invoke(item);
        if (!string.IsNullOrWhiteSpace(caller)) cls += $" {caller}";
        return cls;
    }

    private string StatusPlacementClass => RowStatusPlacement switch
    {
        RowStatusPlacement.Right => "right",
        RowStatusPlacement.Top => "top",
        _ => "left"
    };

    // Header gets a transparent strip of the SAME placement/width as body rows, so header
    // content starts at the same x-offset (fixes the header-shift when RowStatus is in play).
    // Empty when no RowStatus — header sits flush like the body rows do.
    private string HeaderStripClass =>
        RowStatus is not null ? $" lipi-status-strip-{StatusPlacementClass}" : string.Empty;

    private string? RowDataStatus(TItem item)
    {
        var s = RowStatus?.Invoke(item);
        return string.IsNullOrWhiteSpace(s) ? null : s!.ToLowerInvariant();
    }

    private string? RowInlineStyle(TItem item)
    {
        var grid = $"grid-template-columns:{EffectiveGridTemplate}";
        var caller = RowStyle?.Invoke(item);
        return string.IsNullOrWhiteSpace(caller) ? grid : $"{grid};{caller}";
    }

    private bool IsRowDisabled(TItem item) => RowDisabled?.Invoke(item) == true;

    // ── Avatar initials + deterministic hash color (§3.3.2) ──────────────
    // 8-color muted palette; same name → same color (stable across renders/sessions).
    private static readonly string[] AvatarPalette =
    {
        "#5B8DEF", "#9B6DD6", "#3FB9A8", "#E0894A",
        "#5C6BC0", "#26A69A", "#EC6F8E", "#7E9B3F"
    };

    private static int AvatarColorIndex(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        // Simple stable hash (FNV-ish); modulo palette length.
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in s)
            {
                hash ^= ch;
                hash *= 16777619;
            }
            return (int)(hash % (uint)AvatarPalette.Length);
        }
    }

    private static string AvatarColor(string s) => AvatarPalette[AvatarColorIndex(s)];

    private static string Initials(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "?";
        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
        return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpperInvariant();
    }

    private static bool LooksLikeUrl(string s) =>
        s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
        s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
        s.StartsWith("/", StringComparison.Ordinal) ||
        s.StartsWith("data:image", StringComparison.OrdinalIgnoreCase);

    // ── Copy affordance (§3.8) ───────────────────────────────────────────
    // Tracks which cell most recently copied, to drive the brief "Copied!" flash.
    private string? _copiedCellKey;

    private string CellKey(object rowKey, string columnKey) => $"{rowKey}|{columnKey}";

    private async Task CopyCellAsync(object? value, object rowKey, string columnKey)
    {
        var text = value?.ToString();
        if (string.IsNullOrEmpty(text)) return;

        var ok = false;
        if (JS is not null)
        {
            try { ok = await JS.InvokeAsync<bool>("lipiTable.copy", text); }
            catch { ok = false; }
        }

        if (ok)
        {
            _copiedCellKey = CellKey(rowKey, columnKey);
            if (OnCopy.HasDelegate) await OnCopy.InvokeAsync(text);
            StateHasChanged();

            // Clear the flash after a short delay (fire-and-forget; no throw on dispose).
            var keyAtCopy = _copiedCellKey;
            _ = Task.Delay(1400).ContinueWith(_ =>
            {
                if (_copiedCellKey == keyAtCopy)
                {
                    _copiedCellKey = null;
                    InvokeAsync(StateHasChanged);
                }
            });
        }
    }

    private bool IsCellCopied(object rowKey, string columnKey) =>
        _copiedCellKey == CellKey(rowKey, columnKey);

    // ════════════════════════════════════════════════════════════════════
    //  Selection (Stage 4a) — §5
    //  State is a set of row keys (per §5.2.2). Single-page this slice; the
    //  key-set design already supports cross-page (4c) without rework.
    // ════════════════════════════════════════════════════════════════════

    private readonly HashSet<object> _selectedKeys = new();
    // Snapshot of the last key set we pushed outward, to avoid re-ingesting our own writes.
    private IReadOnlyCollection<object>? _lastPushedKeys;
    // Shift-click range anchor (§5.4.2): the last row clicked without Shift. Null until first click.
    private object? _anchorKey;

    // Dummy backing fields used ONLY to satisfy InputBase<T>'s ValueExpression requirement
    // (FieldIdentifier needs a simple member accessor; our selection state is computed, not a
    // per-row field). Same pattern as LipiCheckboxGroup's _childCheckboxBacking. Never read for
    // display — Value drives that; ValueChanged drives mutation.
    // CS0649 suppressed intentionally — these are compile-time-only artifacts.
#pragma warning disable CS0649
    private bool _rowCheckboxBacking;
    private bool? _headerCheckboxBacking;
#pragma warning restore CS0649

    private void SyncIncomingSelectedKeys()
    {
        if (SelectionMode == SelectionMode.None) return;
        if (SelectedKeys is null) return;
        // Only ingest when the caller's set differs from what we last pushed (external change).
        if (_lastPushedKeys is not null && SetEquals(SelectedKeys, _lastPushedKeys)) return;
        if (SetEquals(SelectedKeys, _selectedKeys)) return;

        _selectedKeys.Clear();
        foreach (var k in SelectedKeys) _selectedKeys.Add(k);
        _lastPushedKeys = SelectedKeys;
    }

    private static bool SetEquals(IReadOnlyCollection<object> a, IReadOnlyCollection<object> b)
    {
        if (a.Count != b.Count) return false;
        var set = new HashSet<object>(b);
        foreach (var x in a) if (!set.Contains(x)) return false;
        return true;
    }

    // ── Query helpers (used by markup) ───────────────────────────────────
    private bool IsKeySelectedInternal(object key) => _selectedKeys.Contains(key);

    private bool IsRowSelected(TItem item) => _selectedKeys.Contains(RowKey(item));

    // Header select-all tri-state over the CURRENTLY VISIBLE rows (§5.6.2).
    private enum HeaderSelectState { Unchecked, Indeterminate, Checked }

    private HeaderSelectState HeaderState
    {
        get
        {
            // LP-16: header checkbox reflects the CURRENT PAGE only (PagedItems), not all Items.
            var rows = PagedItems;
            if (rows is null || rows.Count == 0) return HeaderSelectState.Unchecked;
            // Disabled rows can't be selected, so they don't count toward the header state —
            // otherwise "all selectable selected" would never reach Checked when a locked row
            // is present. Tri-state is computed over selectable rows only.
            int selectable = 0, selected = 0;
            foreach (var r in rows)
            {
                if (IsRowDisabled(r)) continue;
                selectable++;
                if (_selectedKeys.Contains(RowKey(r))) selected++;
            }
            if (selectable == 0 || selected == 0) return HeaderSelectState.Unchecked;
            if (selected == selectable) return HeaderSelectState.Checked;
            return HeaderSelectState.Indeterminate;
        }
    }

    // Header checkbox tri-value: true=all visible selected, false=none, null=indeterminate.
    // Drives LipiCheckbox TValue=bool? (native dash for null). §5.6.2
    private bool? HeaderTriValue => HeaderState switch
    {
        HeaderSelectState.Checked => true,
        HeaderSelectState.Indeterminate => null,
        _ => false
    };

    // Header click handler. Uses the value the bool? checkbox REPORTS (keeps its CurrentValue
    // in sync — same anti-double-click reasoning as rows). From the checkbox's tri-state click
    // resolution: unchecked→true and indeterminate→true both report true (→ select all visible);
    // checked→false reports false (→ deselect all visible). §5.3.1 step 1
    private async Task OnHeaderToggleAsync(bool? reported)
    {
        // LP-16: select/deselect applies to the CURRENT PAGE's selectable rows only.
        var page = PagedItems;
        if (SelectionMode != SelectionMode.Multi || page.Count == 0) return;
        // Only selectable (non-disabled) rows participate — disabled rows can't be selected. §5.6.3
        var selectableKeys = page.Where(r => !IsRowDisabled(r)).Select(RowKey).ToArray();
        if (selectableKeys.Length == 0) return;
        bool selectAll = reported == true;
        if (selectAll)
            await ApplySelectionChangeAsync(selectableKeys, Array.Empty<object>(),
                SelectionChangeReason.UserSelectAllOnPage);
        else
            await ApplySelectionChangeAsync(Array.Empty<object>(), selectableKeys,
                SelectionChangeReason.UserSelectAllOnPage);

        // LP-17: seam for Stage 4c (across-pages banner). Fire AFTER the page selection applies,
        // reporting whether the whole page is now selected + the page/total counts the banner needs.
        if (OnAllOnPageSelected.HasDelegate)
            await OnAllOnPageSelected.InvokeAsync(new AllOnPageSelectedContext(
                AllOnPageSelected: selectAll,
                PageSelectableCount: selectableKeys.Length,
                TotalCount: PaginationTotalCount));
    }

    // Density → checkbox size (compact=Small, comfortable/spacious=Medium). §14.3
    private LiPi.Components.InputSize CheckboxSize =>
        Density == TableDensity.Compact
            ? LiPi.Components.InputSize.Small
            : LiPi.Components.InputSize.Medium;

    // Checkbox Name attributes (REQUIRED by LipiInputBase). Built in C# to avoid nested-quote
    // interpolation in a double-quoted Razor attribute (Blazor rule 1/3).
    private string TableIdOrDefault => string.IsNullOrWhiteSpace(TableId) ? "lipi" : TableId!;
    private string SelectAllName => $"{TableIdOrDefault}-selectall";
    private string RowSelectName(object rowKey) => $"{TableIdOrDefault}-sel-{rowKey}";

    // ── Mutation core ────────────────────────────────────────────────────
    // Applies a change, then emits outward bindings + OnSelectionChanged with a diff.
    private async Task ApplySelectionChangeAsync(
        IEnumerable<object> addKeys,
        IEnumerable<object> removeKeys,
        SelectionChangeReason reason)
    {
        var added = new List<object>();
        var removed = new List<object>();

        foreach (var k in removeKeys)
            if (_selectedKeys.Remove(k)) removed.Add(k);
        foreach (var k in addKeys)
            if (_selectedKeys.Add(k)) added.Add(k);

        if (added.Count == 0 && removed.Count == 0) return;

        await EmitSelectionAsync(added, removed, reason);
    }

    private async Task EmitSelectionAsync(
        List<object> addedKeys, List<object> removedKeys, SelectionChangeReason reason)
    {
        // Resolve visible items for the bound SelectedItems + the diff payload.
        var visibleItems = ResolveVisibleSelectedItems();
        var addedItems = ResolveItemsForKeys(addedKeys);
        var removedItems = ResolveItemsForKeys(removedKeys);
        var keysSnapshot = (IReadOnlyCollection<object>)_selectedKeys.ToArray();

        _lastPushedKeys = keysSnapshot;

        if (SelectedItemsChanged.HasDelegate)
            await SelectedItemsChanged.InvokeAsync(visibleItems);
        if (SelectedKeysChanged.HasDelegate)
            await SelectedKeysChanged.InvokeAsync(keysSnapshot);
        if (OnSelectionChanged.HasDelegate)
            await OnSelectionChanged.InvokeAsync(
                new SelectionChangedContext<TItem>(visibleItems, addedItems, removedItems, reason));

        StateHasChanged();
    }

    private IReadOnlyList<TItem> ResolveVisibleSelectedItems()
    {
        if (Items is null || Items.Count == 0) return Array.Empty<TItem>();
        var list = new List<TItem>();
        foreach (var r in Items)
            if (_selectedKeys.Contains(RowKey(r))) list.Add(r);
        return list;
    }

    private IReadOnlyList<TItem> ResolveItemsForKeys(List<object> keys)
    {
        if (keys.Count == 0 || Items is null) return Array.Empty<TItem>();
        var keySet = new HashSet<object>(keys);
        var list = new List<TItem>();
        foreach (var r in Items)
            if (keySet.Contains(RowKey(r))) list.Add(r);
        return list;
    }

    // ── Row checkbox handler (markup calls this) ─────────────────────────
    // Applies the value the checkbox REPORTS (checkedState), not a blind toggle. This keeps
    // LipiCheckbox's internal CurrentValue in sync with our state — a blind toggle desyncs and
    // causes the "click twice" bug. Mirrors LipiCheckboxGroup.OnItemToggleAsync. §5.6.3
    private async Task SetRowSelectedAsync(TItem item, bool checkedState)
    {
        if (IsRowDisabled(item)) return;
        var key = RowKey(item);
        var isSelected = _selectedKeys.Contains(key);

        if (SelectionMode == SelectionMode.Single)
        {
            if (checkedState)
            {
                // Select this row, clearing any prior single selection.
                var toRemove = _selectedKeys.Where(k => !Equals(k, key)).ToArray();
                await ApplySelectionChangeAsync(new[] { key }, toRemove,
                    SelectionChangeReason.UserClickCheckbox);
            }
            else
            {
                // Deselect — only if allowed.
                if (AllowDeselectInSingleMode && isSelected)
                    await ApplySelectionChangeAsync(Array.Empty<object>(), new[] { key },
                        SelectionChangeReason.UserClickCheckbox);
            }
            return;
        }

        // Multi: apply the reported state (idempotent add/remove).
        if (checkedState && !isSelected)
            await ApplySelectionChangeAsync(new[] { key }, Array.Empty<object>(),
                SelectionChangeReason.UserClickCheckbox);
        else if (!checkedState && isSelected)
            await ApplySelectionChangeAsync(Array.Empty<object>(), new[] { key },
                SelectionChangeReason.UserClickCheckbox);
    }

    // ════════════════════════════════════════════════════════════════════
    //  Mouse modifier interactions (Stage 4b) — §5.4
    //  The selection CELL owns the click (not the checkbox) so it can read
    //  Shift/Ctrl/Cmd from MouseEventArgs. The checkbox is display-only.
    // ════════════════════════════════════════════════════════════════════

    // True when the platform "toggle without range" modifier is held. We honor both ctrlKey
    // (Win/Linux) and metaKey (macOS Cmd) — §5.4.4/§5.4.5. Platform-specific tooltip is 4b-deferred.
    private static bool IsToggleModifier(MouseEventArgs e) => e.CtrlKey || e.MetaKey;

    // Click on the selection cell. Plain → toggle + set anchor. Shift → range from anchor.
    // Ctrl/Cmd → toggle this row only, set anchor (same as plain for the checkbox per §5.4.4).
    private async Task OnSelectCellClickAsync(TItem item, MouseEventArgs e)
    {
        if (SelectionMode == SelectionMode.None) return;
        if (IsRowDisabled(item)) return;
        var key = RowKey(item);

        // Shift-click range (Multi only; Single has no range). Falls back to plain toggle if no anchor.
        if (e.ShiftKey && SelectionMode == SelectionMode.Multi && _anchorKey is not null)
        {
            await ApplyShiftRangeAsync(key);
            return;   // anchor is NOT moved by a shift-click (§5.4.2)
        }

        // Plain or Ctrl/Cmd click: toggle this row, then this row becomes the anchor.
        await ToggleRowAndSetAnchorAsync(item, IsToggleModifier(e)
            ? SelectionChangeReason.UserCtrlClick
            : SelectionChangeReason.UserClickCheckbox);
    }

    // Click on a row's BODY cell (not the selection cell).
    //   • plain click  → fire OnRowClick (navigation etc.); selection untouched
    //   • Ctrl/Cmd     → select this row WITHOUT firing OnRowClick (§5.4.4)
    //   • Shift        → extend selection range from anchor (Multi)
    private async Task OnRowBodyClickAsync(TItem item, MouseEventArgs e)
    {
        if (IsRowDisabled(item))
        {
            // Disabled rows still fire OnRowClick per §22.5.5; caller decides.
            if (!e.ShiftKey && !IsToggleModifier(e) && OnRowClick.HasDelegate)
                await OnRowClick.InvokeAsync(item);
            return;
        }

        if (SelectionMode == SelectionMode.Multi && e.ShiftKey && _anchorKey is not null)
        {
            await ApplyShiftRangeAsync(RowKey(item));
            return;
        }

        if (SelectionMode != SelectionMode.None && IsToggleModifier(e))
        {
            // Ctrl/Cmd-click on the body selects without navigating.
            await ToggleRowAndSetAnchorAsync(item, SelectionChangeReason.UserCtrlClick);
            return;
        }

        // Plain click on the body → caller's row-click handler.
        if (OnRowClick.HasDelegate)
            await OnRowClick.InvokeAsync(item);
    }

    // Toggle a single row and make it the new anchor. Honors Single/Multi + AllowDeselect.
    private async Task ToggleRowAndSetAnchorAsync(TItem item, SelectionChangeReason reason)
    {
        if (IsRowDisabled(item)) return;   // disabled rows are never selectable (keyboard or mouse)
        var key = RowKey(item);
        var isSelected = _selectedKeys.Contains(key);

        if (SelectionMode == SelectionMode.Single)
        {
            if (isSelected)
            {
                if (AllowDeselectInSingleMode)
                    await ApplySelectionChangeAsync(Array.Empty<object>(), new[] { key }, reason);
            }
            else
            {
                var toRemove = _selectedKeys.Where(k => !Equals(k, key)).ToArray();
                await ApplySelectionChangeAsync(new[] { key }, toRemove, reason);
            }
            _anchorKey = key;
            return;
        }

        // Multi: plain toggle.
        if (isSelected)
            await ApplySelectionChangeAsync(Array.Empty<object>(), new[] { key }, reason);
        else
            await ApplySelectionChangeAsync(new[] { key }, Array.Empty<object>(), reason);
        _anchorKey = key;
    }

    // Shift-click range (§5.4.1): take the rows between the anchor and the clicked row in display
    // order (inclusive), and apply the CLICKED row's resulting state to all of them. Disabled rows
    // in the range are skipped. The anchor's current state decides the direction:
    //   anchor selected   → range becomes selected
    //   anchor unselected → range becomes unselected
    private async Task ApplyShiftRangeAsync(object clickedKey)
    {
        if (Items is null || Items.Count == 0 || _anchorKey is null) return;

        int anchorIdx = -1, clickedIdx = -1;
        for (int i = 0; i < Items.Count; i++)
        {
            var k = RowKey(Items[i]);
            if (Equals(k, _anchorKey)) anchorIdx = i;
            if (Equals(k, clickedKey)) clickedIdx = i;
        }
        // Anchor not on this page (cross-page range is 4c) or clicked not found → plain toggle.
        if (anchorIdx < 0 || clickedIdx < 0)
        {
            var clickedItem = Items.FirstOrDefault(r => Equals(RowKey(r), clickedKey));
            if (clickedItem is not null) await ToggleRowAndSetAnchorAsync(clickedItem,
                SelectionChangeReason.UserClickCheckbox);
            return;
        }

        int lo = Math.Min(anchorIdx, clickedIdx);
        int hi = Math.Max(anchorIdx, clickedIdx);

        // Direction: extend the anchor's current selected-state across the range.
        bool select = _selectedKeys.Contains(_anchorKey);

        var affected = new List<object>();
        for (int i = lo; i <= hi; i++)
        {
            var r = Items[i];
            if (IsRowDisabled(r)) continue;          // never select disabled rows
            affected.Add(RowKey(r));
        }
        if (affected.Count == 0) return;

        if (select)
            await ApplySelectionChangeAsync(affected, Array.Empty<object>(),
                SelectionChangeReason.UserShiftClick);
        else
            await ApplySelectionChangeAsync(Array.Empty<object>(), affected,
                SelectionChangeReason.UserShiftClick);
        // Anchor stays put on a shift-click (§5.4.2).
    }

    // ════════════════════════════════════════════════════════════════════
    //  Keyboard navigation + selection (Stage 4d) — §19.3.1 + §19.3.2
    //  Roving tabindex: exactly one body cell is tab-focusable (tabindex=0);
    //  arrows move focus cell-by-cell and call FocusAsync (no JS — Option A).
    // ════════════════════════════════════════════════════════════════════

    // Focused cell coords. (-1,-1) = nothing focused yet; first cell becomes tabbable on mount.
    private int _focusRow = -1;
    private int _focusCol = -1;
    private bool _pendingFocus;                 // set when a key moved focus → focus in OnAfterRender
    // ElementReference per body cell, keyed by row*1000+col. @ref into a dictionary indexer is a
    // legal lvalue (compiles to _cellRefs[key] = __ref); a single field would last-wins across cells
    // and an @ref lambda isn't an lvalue (CS0131). We focus the entry for the focused coords.
    private readonly Dictionary<int, ElementReference> _cellRefs = new();
    private static int FocusCellKey(int r, int c) => r * 1000 + c;

    private int TotalColumns => VisibleColumns.Count + (HasSelectionColumn ? 1 : 0);
    // Rendered rows on the current page (focus nav clamps to THIS, not all Items — Stage 7 fix).
    private int RowCount => PagedItems.Count;

    // The cell that should be tab-focusable. Default to the first cell (0,0) when nothing is
    // focused yet, so Tab can enter the grid and land somewhere sensible.
    private bool IsFocusedCell(int r, int c)
    {
        if (_focusRow < 0) return r == 0 && c == 0;   // initial tab-stop
        return r == _focusRow && c == _focusCol;
    }

    private string CellTabIndex(int r, int c) => IsFocusedCell(r, c) ? "0" : "-1";

    private async Task OnCellKeyDownAsync(KeyboardEventArgs e, TItem item, int r, int c)
    {
        // Establish current focus coords (first interaction may start from the default tab-stop).
        if (_focusRow < 0) { _focusRow = 0; _focusCol = 0; }

        var key = e.Key;
        var toggleMod = e.CtrlKey || e.MetaKey;

        // ── Pagination shortcuts (Stage 7, whole-table scope) ─────────────
        // Checked BEFORE the switch so Ctrl+Alt+Home/End is caught as first/last PAGE before
        // the plain/Ctrl Home/End grid-corner nav below. Distinct chords (option a) so grid-corner
        // (Ctrl+Home/End) is preserved. After a page change, focus lands on the new page's first cell.
        if (e.AltKey && key == "PageDown")
        {
            if (NavigateToPageByKeyboard(_currentPage + 1)) { StateHasChanged(); }
            return;
        }
        if (e.AltKey && key == "PageUp")
        {
            if (NavigateToPageByKeyboard(_currentPage - 1)) { StateHasChanged(); }
            return;
        }
        if (e.CtrlKey && e.AltKey && key == "Home")
        {
            if (NavigateToPageByKeyboard(1)) { StateHasChanged(); }
            return;
        }
        if (e.CtrlKey && e.AltKey && key == "End")
        {
            if (NavigateToPageByKeyboard(PaginationTotalPages)) { StateHasChanged(); }
            return;
        }

        switch (key)
        {
            // ── Navigation (§19.3.1) ──────────────────────────────────────
            case "ArrowDown":
                if (e.ShiftKey && SelectionMode == SelectionMode.Multi)
                    await ExtendSelectionByOneAsync(r, +1);
                MoveFocus(r + 1, c); break;
            case "ArrowUp":
                if (e.ShiftKey && SelectionMode == SelectionMode.Multi)
                    await ExtendSelectionByOneAsync(r, -1);
                MoveFocus(r - 1, c); break;
            case "ArrowRight": MoveFocus(r, c + 1); break;
            case "ArrowLeft":  MoveFocus(r, c - 1); break;
            case "Home":
                if (toggleMod) MoveFocus(0, 0); else MoveFocus(r, 0); break;
            case "End":
                if (toggleMod) MoveFocus(RowCount - 1, TotalColumns - 1);
                else MoveFocus(r, TotalColumns - 1); break;

            // ── Selection (§19.3.2) ───────────────────────────────────────
            case " ":            // Space
            case "Spacebar":     // legacy key name
                if (SelectionMode == SelectionMode.None) return;
                if (IsRowDisabled(item)) break;   // disabled rows: swallow the key, no toggle
                if (e.ShiftKey && SelectionMode == SelectionMode.Multi && _anchorKey is not null)
                    await ApplyShiftRangeAsync(RowKey(item));
                else if (toggleMod)
                    await ToggleRowKeepAnchorAsync(item);     // Ctrl/Cmd+Space: toggle, keep anchor
                else
                    // Plain Space, or Shift+Space with no anchor yet → toggle + set anchor.
                    await ToggleRowAndSetAnchorAsync(item, SelectionChangeReason.UserKeyboardSpace);
                break;

            case "a":
            case "A":
                if (toggleMod && SelectionMode == SelectionMode.Multi)
                {
                    // Ctrl/Cmd+A → select all on page. (Ctrl/Cmd+Shift+A across-pages = 4c.)
                    await SelectAllOnPageAsync();
                }
                else return;     // not a shortcut → let it through
                break;

            case "Escape":
                if (_selectedKeys.Count > 0) await ClearSelectionAsync();
                else return;
                break;

            default:
                return;          // unhandled key → don't preventDefault
        }

        // We handled the key: keep focus on the active cell and suppress default scroll/typeahead.
        _pendingFocus = true;
        StateHasChanged();
    }

    // Move focus to (r,c), clamped to the grid. Sets _pendingFocus so OnAfterRender focuses it.
    private void MoveFocus(int r, int c)
    {
        if (RowCount == 0 || TotalColumns == 0) return;
        _focusRow = Math.Clamp(r, 0, RowCount - 1);
        _focusCol = Math.Clamp(c, 0, TotalColumns - 1);
        _pendingFocus = true;
    }

    // Ctrl/Cmd+Space: toggle this row's selection WITHOUT moving the anchor (§19.3.2).
    private async Task ToggleRowKeepAnchorAsync(TItem item)
    {
        var saved = _anchorKey;
        await ToggleRowAndSetAnchorAsync(item, SelectionChangeReason.UserKeyboardSpace);
        _anchorKey = saved;
    }

    // Shift+Arrow: extend selection by one row in the arrow direction (§19.3.2).
    private async Task ExtendSelectionByOneAsync(int fromRow, int delta)
    {
        if (Items is null) return;
        int target = fromRow + delta;
        if (target < 0 || target >= Items.Count) return;
        var targetItem = Items[target];
        if (IsRowDisabled(targetItem)) return;
        // Establish an anchor if none, then add the target to the selection.
        if (_anchorKey is null) _anchorKey = RowKey(Items[fromRow]);
        await ApplySelectionChangeAsync(new[] { RowKey(targetItem) }, Array.Empty<object>(),
            SelectionChangeReason.UserShiftClick);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Stage S3a — anchor the filter popover to its funnel once the DOM is committed.
        if (_pendingPopoverAnchor && _openFilterKey is not null && JS is not null
            && _filterBtnRefs.TryGetValue(_openFilterKey, out var fbtn))
        {
            _pendingPopoverAnchor = false;
            try
            {
                var rect = await JS.InvokeAsync<PopoverRect?>("lipiTable.getRect", fbtn);
                if (rect is not null)
                {
                    const double popoverH = 280;   // conservative: operator+value+buttons+chrome
                    _popoverAbove  = rect.Bottom + popoverH > rect.ViewportH && rect.Top > popoverH;
                    _popoverTop    = rect.Bottom + 4;
                    _popoverBottom = rect.ViewportH - rect.Top + 4;
                    _popoverRight  = rect.ViewportW - rect.Right;
                    _popoverMeasured = true;
                    _selfRef ??= DotNetObjectReference.Create(this);
                    try { await JS.InvokeVoidAsync("lipiTable.onScrollClose", _selfRef); } catch { }
                    StateHasChanged();
                }
            }
            catch { /* JS unavailable — reveal unanchored rather than hide forever */ _popoverMeasured = true; StateHasChanged(); }
        }
        if (firstRender && JS is not null)
        {
            // One-time global scroll-guard for grid nav keys (idempotent in JS). Stage 4d.
            try { await JS.InvokeVoidAsync("lipiTable.initKeyboardGuard"); }
            catch { /* prerender / JS not ready — guard installs on a later render path */ }
        }

        if (_pendingFocus)
        {
            _pendingFocus = false;
            if (_cellRefs.TryGetValue(FocusCellKey(_focusRow, _focusCol), out var cellRef))
            {
                try { await cellRef.FocusAsync(); }
                catch { /* element may not be in DOM (e.g., row removed); ignore */ }
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  Public programmatic API (via @ref) — §5.7.4 (4a subset)
    // ════════════════════════════════════════════════════════════════════

    public async ValueTask SelectAsync(TItem item) =>
        await ApplySelectionChangeAsync(new[] { RowKey(item) }, Array.Empty<object>(),
            SelectionChangeReason.Programmatic);

    public async ValueTask DeselectAsync(TItem item) =>
        await ApplySelectionChangeAsync(Array.Empty<object>(), new[] { RowKey(item) },
            SelectionChangeReason.Programmatic);

    public async ValueTask ToggleSelectionAsync(TItem item)
    {
        var key = RowKey(item);
        if (_selectedKeys.Contains(key)) await DeselectAsync(item);
        else await SelectAsync(item);
    }

    public async ValueTask SelectKeysAsync(IEnumerable<object> keys) =>
        await ApplySelectionChangeAsync(keys, Array.Empty<object>(),
            SelectionChangeReason.Programmatic);

    public async ValueTask DeselectKeysAsync(IEnumerable<object> keys) =>
        await ApplySelectionChangeAsync(Array.Empty<object>(), keys,
            SelectionChangeReason.Programmatic);

    public async ValueTask SelectAllOnPageAsync()
    {
        if (Items is null || Items.Count == 0) return;
        var selectableKeys = Items.Where(r => !IsRowDisabled(r)).Select(RowKey).ToArray();
        await ApplySelectionChangeAsync(selectableKeys, Array.Empty<object>(),
            SelectionChangeReason.UserSelectAllOnPage);
    }

    public async ValueTask ClearSelectionAsync()
    {
        _anchorKey = null;   // §5.4.2 — clearing selection resets the range anchor
        if (_selectedKeys.Count == 0) return;
        var all = _selectedKeys.ToArray();
        await ApplySelectionChangeAsync(Array.Empty<object>(), all,
            SelectionChangeReason.UserClearSelection);
    }

    public IReadOnlyCollection<object> GetSelectedKeys() => _selectedKeys.ToArray();

    public IReadOnlyList<TItem> GetSelectedVisibleItems() => ResolveVisibleSelectedItems();

    public bool IsSelected(TItem item) => _selectedKeys.Contains(RowKey(item));

    public bool IsKeySelected(object key) => _selectedKeys.Contains(key);

    void IDisposable.Dispose()
    {
        if (JS is not null) { try { _ = JS.InvokeVoidAsync("lipiTable.offScrollClose"); } catch { } }
        _selfRef?.Dispose();
    }
}

