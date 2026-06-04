// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §23 (Events)
//   §23.3 (Selection events — SelectionChangedContext)
//   §23.4 (Sort, filter, search events — Sort/Filter/QuickSearchChangedContext)
//   §23.5 (Pagination events — Page/PageSizeChangedContext)
//   §23.6 (Group events — GroupChanged/Expanded/CollapsedContext)
//   §23.7 (Tree events — TreeNodeExpanded/CollapsedContext)
//   §23.8 (Master-detail events — RowDetailExpanded/CollapsedContext)
//   §23.9 (Inline edit events — RowEdit/CellEdit/DirtyStateChangedContext)
//   §23.10.2 (DensityChangedContext)
//   §13.2.8 / §13.3.7 / §13.4.9 / §23.10.3 (ColumnResized/Reordered/Pinned/VisibilityChangedContext)
//   §23.10.5 (TableErrorContext + TableErrorSource)
//   §23.10.6 (QueryCompleteContext + QueryCompleteReason)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiTable
//
// All non-export event context records and their associated *Reason enums. Generic
// contexts (over TItem) preserve the row type; non-generic contexts (sort, filter, page,
// density, column ops) are plain records.
//
// EXCLUDED from this file:
//   • Export contexts (BeforeExportContext, AfterExportContext, ExportProgress) →
//     ExportTypes.cs (co-located with ExportFormat / ExportScope / ExportOptions).
//   • Persistence contexts (PersistedContext, PersistedTrigger) → deferred to Stage 1B's
//     TablePreferences.cs (references TablePreferences which lives there).

using System;
using System.Collections.Generic;

namespace LiPi.Components.DataDisplay;

// ════════════════════════════════════════════════════════════════════════
//  Selection — §23.3
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// Selection change event payload. Includes the full SelectedItems snapshot plus the
/// AddedItems / RemovedItems diff for callers that want either model. Per §23.3.1.
/// </summary>
public sealed record SelectionChangedContext<TItem>(
    IReadOnlyList<TItem> SelectedItems,
    IReadOnlyList<TItem> AddedItems,
    IReadOnlyList<TItem> RemovedItems,
    SelectionChangeReason Reason);

public enum SelectionChangeReason
{
    UserClickCheckbox,
    UserKeyboardSpace,
    UserShiftClick,
    UserCtrlClick,
    UserSelectAllOnPage,
    UserSelectAllAcrossPages,
    UserClearSelection,
    Programmatic,
    DataChange
}

// ════════════════════════════════════════════════════════════════════════
//  Sort, filter, quick search — §23.4
// ════════════════════════════════════════════════════════════════════════

/// <summary>Sort change event payload. Per §23.4.1.</summary>
public sealed record SortChangedContext(
    IReadOnlyList<SortDescriptor> NewSort,
    IReadOnlyList<SortDescriptor> PreviousSort,
    SortChangeReason Reason);

public enum SortChangeReason
{
    UserClickHeader,
    UserShiftClickHeader,
    UserContextMenu,
    UserClearAll,
    Programmatic,
    PersistenceRestore,
    ResetToDefault
}

/// <summary>Filter change event payload. Per §23.4.2.</summary>
public sealed record FilterChangedContext(
    IReadOnlyList<FilterDescriptor> NewFilters,
    IReadOnlyList<FilterDescriptor> PreviousFilters,
    FilterChangeReason Reason);

public enum FilterChangeReason
{
    UserApplyPopover,
    UserApplyDrawer,
    UserRemoveChip,
    UserClearAll,
    Programmatic,
    PersistenceRestore,
    ResetToDefault
}

/// <summary>Quick-search change event payload (debounced 300ms). Per §23.4.3.</summary>
public sealed record QuickSearchChangedContext(string? NewValue, string? PreviousValue);

// ════════════════════════════════════════════════════════════════════════
//  Pagination — §23.5
// ════════════════════════════════════════════════════════════════════════

/// <summary>Page-change event payload. PreviousPage = 0 on initial mount. Per §23.5.1.</summary>
public sealed record PageChangedContext(
    int NewPage,
    int PreviousPage,
    int PageSize,
    PageChangeReason Reason);

public enum PageChangeReason
{
    UserClickPager,
    UserJumpToPage,
    UserKeyboard,
    InfiniteScrollLoadMore,
    LoadMoreButton,
    Programmatic,
    DataChange,
    ResetAfterFilter,
    PageSnap          // Stage 7 / LP-24: out-of-range bounds clamp. LP-19 focus = treat as navigation
                      // (focus lands on first row of snapped page), distinct from DataChange (preserve focused row identity).
}

/// <summary>Payload for the header "select all on page" event (Stage 7 / LP-17). The composing
/// layer (Stage 4c) subscribes to show the across-pages banner. <paramref name="AllOnPageSelected"/>
/// is true when the action selected the page, false when it deselected. Counts let the banner read
/// "All N on this page selected. Select all <see cref="TotalCount"/>?".</summary>
public sealed record AllOnPageSelectedContext(
    bool AllOnPageSelected,
    int PageSelectableCount,
    int TotalCount);

/// <summary>Page-size change event payload. Per §23.5.2.</summary>
public sealed record PageSizeChangedContext(
    int NewSize,
    int PreviousSize,
    PageSizeChangeReason Reason);

public enum PageSizeChangeReason
{
    UserSelectSize,
    UserAllOption,
    Programmatic,
    PersistenceRestore,
    ResetToDefault
}

// ════════════════════════════════════════════════════════════════════════
//  Grouping — §23.6
// ════════════════════════════════════════════════════════════════════════

/// <summary>Group dimension change event payload. Per §23.6.1.</summary>
public sealed record GroupChangedContext(
    IReadOnlyList<string> NewGroupBy,
    IReadOnlyList<string> PreviousGroupBy,
    GroupChangeReason Reason);

public enum GroupChangeReason
{
    UserDragToGroupBar,
    UserRemoveFromGroupBar,
    UserReorderGroupBar,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

/// <summary>
/// Group expand event payload. RequiresContentFetch=true signals the lazy-load
/// trigger — caller's handler should fetch the group's children. Per §23.6.2 / §9.9.3.
/// </summary>
public sealed record GroupExpandedContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    bool RequiresContentFetch,
    GroupExpandReason Reason);

/// <summary>Group collapse event payload. Per §23.6.2.</summary>
public sealed record GroupCollapsedContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    GroupCollapseReason Reason);

public enum GroupExpandReason
{
    UserClickChevron,
    Programmatic,
    DefaultExpanded
}

public enum GroupCollapseReason
{
    UserClickChevron,
    Programmatic,
    DefaultCollapsed
}

// ════════════════════════════════════════════════════════════════════════
//  Tree — §23.7
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// Tree node expand event payload. RequiresContentFetch=true signals the lazy-load
/// trigger — caller's handler should fetch the node's children. Per §23.7 / §10.10.2.
/// </summary>
public sealed record TreeNodeExpandedContext<TItem>(
    TItem Row,
    int Depth,
    bool RequiresContentFetch,
    TreeNodeExpandReason Reason);

/// <summary>Tree node collapse event payload. Per §23.7.</summary>
public sealed record TreeNodeCollapsedContext<TItem>(
    TItem Row,
    int Depth,
    TreeNodeCollapseReason Reason);

public enum TreeNodeExpandReason
{
    UserClickChevron,
    UserKeyboard,
    Programmatic,
    DefaultExpanded,
    AutoExpandedForFilter
}

public enum TreeNodeCollapseReason
{
    UserClickChevron,
    UserKeyboard,
    Programmatic,
    DefaultCollapsed,
    FilterCleared
}

// ════════════════════════════════════════════════════════════════════════
//  Master-detail — §23.8
// ════════════════════════════════════════════════════════════════════════

/// <summary>Master-detail row expand event payload. Per §23.8.</summary>
public sealed record RowDetailExpandedContext<TItem>(
    TItem Row,
    DetailExpandReason Reason);

/// <summary>Master-detail row collapse event payload. Per §23.8.</summary>
public sealed record RowDetailCollapsedContext<TItem>(
    TItem Row,
    DetailCollapseReason Reason);

public enum DetailExpandReason
{
    UserClickChevron,
    UserKeyboard,
    Programmatic,
    DefaultExpanded
}

public enum DetailCollapseReason
{
    UserClickChevron,
    UserKeyboard,
    Programmatic,
    AccordionAutoCollapse
}

// ════════════════════════════════════════════════════════════════════════
//  Inline edit lifecycle — §23.9
// ════════════════════════════════════════════════════════════════════════

/// <summary>Row edit start event payload. IsAddNew=true when entering edit on a new add-row. Per §23.9.1.</summary>
public sealed record RowEditStartContext<TItem>(TItem Row, bool IsAddNew);

/// <summary>Row edit end event payload. Per §23.9.1.</summary>
public sealed record RowEditEndContext<TItem>(TItem Row, RowEditEndReason Reason, bool WasAddNew);

public enum RowEditEndReason
{
    UserSaved,
    UserCancelled,
    UserDiscarded,
    Programmatic,
    SaveFailed,
    ConflictResolved
}

/// <summary>Cell edit start event payload. Per §23.9.2.</summary>
public sealed record CellEditStartContext<TItem>(TItem Row, string ColumnKey);

/// <summary>Cell edit end event payload. Per §23.9.2.</summary>
public sealed record CellEditEndContext<TItem>(TItem Row, string ColumnKey, CellEditEndReason Reason);

public enum CellEditEndReason
{
    UserSaved,
    UserCancelled,
    UserMovedToNextCell,
    UserFocusLoss,
    Programmatic
}

/// <summary>Dirty state change event payload. ChangedFieldKeys lists every dirty field. Per §23.9.3.</summary>
public sealed record DirtyStateChangedContext<TItem>(
    TItem Row,
    bool IsDirty,
    IReadOnlyList<string> ChangedFieldKeys);

// ════════════════════════════════════════════════════════════════════════
//  Density — §23.10.2
// ════════════════════════════════════════════════════════════════════════

/// <summary>Density change event payload. Per §23.10.2.</summary>
public sealed record DensityChangedContext(
    TableDensity NewDensity,
    TableDensity PreviousDensity,
    DensityChangeReason Reason);

public enum DensityChangeReason
{
    UserToggle,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

// ════════════════════════════════════════════════════════════════════════
//  Column ops — §13.2.8, §13.3.7, §13.4.9, §23.10.3
// ════════════════════════════════════════════════════════════════════════

/// <summary>Column resize event payload. Per §13.2.8.</summary>
public sealed record ColumnResizedContext(
    string ColumnKey,
    string OldWidth,
    string NewWidth,
    ColumnResizeReason Reason);

public enum ColumnResizeReason
{
    UserDrag,
    UserAutoFit,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

/// <summary>
/// Column reorder event payload. OldPin / NewPin are included because reorder
/// can implicitly change a column's pin state per §13.3.3. Per §13.3.7.
/// </summary>
public sealed record ColumnReorderedContext(
    string ColumnKey,
    int OldIndex,
    int NewIndex,
    ColumnPin OldPin,
    ColumnPin NewPin,
    ColumnReorderReason Reason);

public enum ColumnReorderReason
{
    UserDrag,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

/// <summary>Column pin event payload. Per §13.4.9.</summary>
public sealed record ColumnPinnedContext(
    string ColumnKey,
    ColumnPin OldPin,
    ColumnPin NewPin,
    ColumnPinReason Reason);

public enum ColumnPinReason
{
    UserContextMenu,
    UserDrag,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

/// <summary>Column visibility change event payload. Per §23.10.3.</summary>
public sealed record ColumnVisibilityChangedContext(
    string ColumnKey,
    bool IsVisible,
    ColumnVisibilityChangeReason Reason);

public enum ColumnVisibilityChangeReason
{
    UserColumnPicker,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

// ════════════════════════════════════════════════════════════════════════
//  Error + query complete — §23.10.5, §23.10.6
// ════════════════════════════════════════════════════════════════════════

/// <summary>
/// Error event payload. Exception is null when the error originated from the LoadError
/// parameter (no exception to surface). Per §23.10.5.
/// </summary>
public sealed record TableErrorContext(
    string Message,
    Exception? Exception,
    TableErrorSource Source);

public enum TableErrorSource
{
    DataSourceException,
    LoadErrorParameter,
    OnRowSaveException,
    OnBeforeExportException,
    GenericInternal
}

/// <summary>
/// Query complete event payload. Fires after every DataSource invocation (server-side)
/// or every client-side recomputation. Request / Response are null for client-side mode.
/// Per §23.10.6.
/// </summary>
public sealed record QueryCompleteContext<TItem>(
    long ItemCount,
    long TotalCount,
    TimeSpan Duration,
    QueryCompleteReason Reason,
    TableQueryRequest? Request,
    TableQueryResponse<TItem>? Response);

public enum QueryCompleteReason
{
    InitialLoad,
    Refresh,
    SortChange,
    FilterChange,
    PageChange,
    PageSizeChange,
    GroupChange,
    QuickSearchChange
}
