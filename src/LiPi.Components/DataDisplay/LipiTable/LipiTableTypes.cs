// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §3.1 (LipiTable<TItem> signature — parameters reference these enums)
//   §5.1 (SelectionMode), §7.1 (FilterMode)
//   §8.4 (PaginationPlacement), §8.5 (PaginationMode)
//   §9.5 (DefaultGroupState)
//   §10.4.2 (DefaultTreeState), §10.5.2 (TreeFilterMode), §10.7.1 (TreeSelectionCascade)
//   §11.6 (DefaultDetailState)
//   §12.1 (TableEditMode), §12.2.5 (EditButtonPlacement)
//   §12.9 (OptimisticUpdate), §12.11 (ConflictResolutionMode), §12.12 (NewRowPlacement)
//   §14.2 (TableDensity), §14.4 (DensityToggleStyle)
//   §18.3 (LoadingStrategy)
//   §20.2 (VirtualizeMode)
//   §22.2 (RowStatusPlacement)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiTable
//
// Table-level enums for LipiTable<TItem>. These drive the table's overall behavior
// (selection mode, edit mode, pagination, density, filter mode, etc.) and are referenced
// by parameters on LipiTable itself, not by column declarations. Column-specific enums
// live in LipiColumnTypes.cs.

namespace LiPi.Components.DataDisplay;

/// <summary>Selection model for LipiTable rows. None is the default. Per §5.1.</summary>
public enum SelectionMode
{
    None,
    Single,
    Multi
}

/// <summary>
/// Where the selection checkbox column sits. Left is the default (leftmost column);
/// Right places it at the rightmost position before any pinned-right columns. Per §5.6.1.
/// </summary>
public enum SelectionPlacement
{
    Left,
    Right
}

/// <summary>Inline-edit model for LipiTable. None is the default (read-only). Per §12.1.1.</summary>
public enum TableEditMode
{
    None,
    Row,
    Cell
}

/// <summary>Placement of Save / Cancel buttons during row edit. InlineRowRight is default. Per §12.2.5.</summary>
public enum EditButtonPlacement
{
    InlineRowRight,
    StickyBottomBar
}

/// <summary>Save flow for inline edits. Pessimistic is default per Q3.4 (locked). Per §12.9.1.</summary>
public enum OptimisticUpdate
{
    Pessimistic,
    Optimistic
}

/// <summary>Concurrency conflict resolution UX mode. Banner is default. Per §12.11.1.</summary>
public enum ConflictResolutionMode
{
    Banner,
    Modal,
    Custom
}

/// <summary>Placement of the inline add-new pseudo-row. Top is default per Q3.x (locked). Per §12.12.</summary>
public enum NewRowPlacement
{
    Top,
    Bottom,
    AfterFocused
}

/// <summary>Pagination model. Paginated is default. Per §8.5.1.</summary>
public enum PaginationMode
{
    Paginated,
    InfiniteScroll,
    LoadMore
}

/// <summary>Pagination control placement around the table. Bottom is default. Per §8.4.1 + LP-15 Q1
/// amendment (Left/Right side placement). Both = top+bottom pair (NOT including Left/Right).
/// Left/Right render a vertical side pager (LipiTable sets Orientation=Vertical automatically).</summary>
public enum PaginationPlacement
{
    Bottom,   // default — region ⑧
    Top,      // region ④
    Both,     // top + bottom pair
    Left,     // side-anchored vertical pager (LP-15 Q1)
    Right,    // side-anchored vertical pager, mirrored (LP-15 Q1)
    None      // escape hatch; consumer composes externally via @bind
}

/// <summary>Density mode affecting row height, font size, and padding. Comfortable is default. Per §14.2.</summary>
public enum TableDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>Density toggle control style in the toolbar. Segmented is default. Per §14.4.2.</summary>
public enum DensityToggleStyle
{
    Segmented,
    IconWithLabel,
    IconDropdown
}

/// <summary>Virtualization activation mode. Auto engages at 100-row threshold. Per §20.2.1.</summary>
public enum VirtualizeMode
{
    Auto,
    Always,
    Never
}

/// <summary>Filter UI mode. HeaderIcon is default. Per §7.1.</summary>
public enum FilterMode
{
    HeaderIcon,
    Drawer,
    None
}

/// <summary>
/// Which control opens the filter drawer when <c>FilterMode=Drawer</c> (Stage S3c / PR3).
/// <see cref="ToolbarButton"/> (default) = a labeled "Filters" button in the toolbar carrying an
/// active-filter-column count badge; <see cref="HeaderFunnel"/> = a compact funnel-icon button (same
/// toolbar slot, icon-only). Both open the same drawer.
/// </summary>
public enum FilterDrawerTrigger
{
    ToolbarButton,
    HeaderFunnel
}

/// <summary>
/// Side the filter drawer slides in from (Stage S3c / PR3). <see cref="Right"/> is default.
/// DataDisplay-local (maps to the Overlays <c>DrawerPlacement</c> internally) so the table API
/// stays self-contained. For <see cref="Top"/>/<see cref="Bottom"/>, <c>FilterDrawerSizePx</c>
/// controls the drawer's height instead of its width.
/// </summary>
public enum FilterDrawerSide
{
    Right,
    Left,
    Top,
    Bottom
}

/// <summary>When a column filter commits (Stage S3). Apply = explicit Apply button (default);
/// Live = filters as the operator/value change, debounced. Configurable per table.</summary>
public enum FilterApplyMode
{
    Apply,
    Live
}

/// <summary>Where active-filter chips render (Stage S3). Separate = own strip below the toolbar
/// (default); Inline = within the toolbar row beside the quick-search box. Falls back to a
/// separate strip when the toolbar is absent (ShowQuickSearch=false).</summary>
public enum FilterChipsPlacement
{
    Separate,
    Inline
}

/// <summary>
/// Which relative-date operator buckets a date column offers (Stage S3b+). A <c>[Flags]</c>
/// set so a column — or the table default — can enable any combination. Resolution cascade:
/// column <c>RelativeDateSpans</c> ?? table <c>RelativeDateSpans</c> ?? <see cref="All"/>, so the
/// unspecified default preserves the full relative set (no breaking change). This gates ONLY the
/// relative operators; the absolute operators (On/Before/After/Between, Empty/NotEmpty) and the
/// date-range picker are unaffected. Buckets: <see cref="Day"/> = Today/Yesterday/Tomorrow plus
/// Last-N/Next-N-days; <see cref="Week"/>/<see cref="Month"/>/<see cref="Quarter"/>/<see cref="Year"/>
/// = the This/Last/Next relatives at that grain. A month-only clinic sets
/// <c>RelativeDateSpans="DateSpan.Month"</c> and the week/quarter/year relatives simply don't appear.
/// </summary>
[Flags]
public enum DateSpan
{
    None    = 0,
    Day     = 1 << 0,
    Week    = 1 << 1,
    Month   = 1 << 2,
    Quarter = 1 << 3,
    Year    = 1 << 4,
    All     = Day | Week | Month | Quarter | Year
}

/// <summary>Loading-state rendering strategy. AlwaysSkeleton is default. Per §18.3.4.</summary>
public enum LoadingStrategy
{
    AlwaysSkeleton,
    SkeletonOnInitial,
    OverlayAlways
}

/// <summary>Placement of the row status strip. Left is default. Per §22.2.</summary>
public enum RowStatusPlacement
{
    Left,
    Right,
    Top
}

/// <summary>Default expand/collapse state for groups on mount. Expanded is default per Q4.3. Per §9.5.</summary>
public enum DefaultGroupState
{
    Expanded,
    Collapsed
}

/// <summary>Default expand/collapse state for tree rows on mount. Collapsed is default. Per §10.4.2.</summary>
public enum DefaultTreeState
{
    Collapsed,
    Expanded,
    FirstLevelExpanded
}

/// <summary>Default expand/collapse state for master-detail rows on mount. Collapsed is default. Per §11.6.</summary>
public enum DefaultDetailState
{
    Collapsed,
    Expanded
}

/// <summary>How tree filtering treats matched vs unmatched nodes. PreserveAncestors is default. Per §10.5.2.</summary>
public enum TreeFilterMode
{
    PreserveAncestors,
    MatchOnly
}

/// <summary>Cascade behavior when a tree parent is selected. None is default (per §10.7.1).</summary>
public enum TreeSelectionCascade
{
    None,
    Descendants,
    Manual
}


/// <summary>
/// Optional style/behavior overrides for the LipiPagination that LipiTable embeds (Stage 7 / LP-15 Q6).
/// A record bag with all-null defaults: each null delegates to LipiPagination's own default, so a
/// consumer overrides only what they care about. PageSizeOptions is <see cref="PageSizeOption"/> list
/// (LP-20) — matches the shipped LipiPagination surface and preserves the "All" label; build lists via
/// the <c>PageSizeOptions</c> helper (e.g. <c>PageSizeOptions.FromInts(10,25,50)</c>).
/// </summary>
public sealed record PaginationOptions(
    PaginationCellStyle? CellStyle = null,
    PaginationActiveStyle? ActiveStyle = null,
    PaginationChevronStyle? ChevronStyle = null,
    int? Siblings = null,
    int? BoundaryCount = null,
    IReadOnlyList<PageSizeOption>? PageSizeOptions = null,
    int? DefaultPageSize = null,
    bool? ShowFirstLast = null,
    bool? ShowJumpToPage = null,
    bool? ShowPageSize = null,             // auto-overridden to false in Left/Right placement
    bool? ShowRowCount = null,
    Func<PaginationRange, string>? RowCountTemplate = null,
    PaginationAlign? Align = null
);

/// <summary>
/// Ready-made <see cref="PaginationOptions"/> presets (Stage 7 / LP-15 Q6). The embedded default
/// (when PaginationOptions is null) is LipiPagination's own standalone default — Bordered + Solid +
/// Auto — because LipiTable is redistributable (§25) and must not bake HIS-specific visuals. The HIS
/// app opts into the clinical look explicitly via <see cref="LipiHisDefault"/>.
/// </summary>
public static class PaginationOptionsPresets
{
    /// <summary>The LiPi HIS in-app look: borderless numbers + soft-tint active. Opt-in, not the library default.</summary>
    public static readonly PaginationOptions LipiHisDefault = new(
        CellStyle: PaginationCellStyle.Borderless,
        ActiveStyle: PaginationActiveStyle.Tint,
        ChevronStyle: PaginationChevronStyle.Auto);
}
