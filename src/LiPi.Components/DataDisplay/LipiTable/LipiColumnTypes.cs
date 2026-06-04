// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §3.3 (ColumnType — 14 generic types), §3.7.1 (LipiAggregate)
//   §3.9 (ColumnPin, ColumnAlign, CopyTarget, StatusVariant)
//   §6.1.2 (SortCycleMode), §6.4.3 (NullSortOrder)
//   §13.4.1 (ColumnPin cross-reference)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiColumn
//
// Column-level enums for LipiColumn<TItem, TValue>. These drive how a single column
// renders (Type), sorts (SortCycleMode / NullSortOrder), pins (ColumnPin), aligns
// (ColumnAlign), aggregates (LipiAggregate), and exposes copy affordances (CopyTarget).
// StatusVariant is consulted only when Type == Status. Table-wide enums live in
// LipiTableTypes.cs.

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Built-in column types with default renderers. Per §3.3. None of these types know about
/// clinical concepts — domain-specific rendering is the caller's responsibility via
/// Type=Custom + &lt;CellTemplate&gt; (component isolation contract §25.2.1).
/// </summary>
public enum ColumnType
{
    Text,
    Number,
    Currency,
    Date,
    DateTime,
    Time,
    Boolean,
    Mono,
    Status,
    Avatar,
    Link,
    File,
    Actions,
    Custom
}

/// <summary>Column pin position. None means the column scrolls with the body. Per §13.4.1.</summary>
public enum ColumnPin
{
    None,
    Left,
    Right
}

/// <summary>
/// Horizontal cell alignment. When null on the column, alignment is derived from ColumnType
/// per §3.3.3 (first column left; numeric right; Date / Boolean / Status / Avatar center;
/// Actions right).
/// </summary>
public enum ColumnAlign
{
    Left,
    Center,
    Right
}

/// <summary>Target for the hover-copy affordance on a cell. Value is default. Per §3.8.</summary>
public enum CopyTarget
{
    Value,
    Href,
    Url
}

/// <summary>Status cell rendering style for ColumnType.Status. Badge is default. Per §3.8.</summary>
public enum StatusVariant
{
    Badge,
    Pill
}

/// <summary>
/// Built-in column aggregate functions. None disables aggregation for the column (the column
/// renders no footer aggregate). Per §3.7.1.
/// </summary>
public enum LipiAggregate
{
    None,
    Sum,
    Avg,
    Count,
    CountNonNull,
    CountDistinct,
    Min,
    Max,
    First,
    Last
}

/// <summary>
/// Header sort cycle mode. ThreeState (unsorted → asc → desc → unsorted) is the default and
/// matches Excel / Google Sheets / AG-Grid. TwoState keeps sort active once started (asc → desc → asc).
/// Per §6.1.2.
/// </summary>
public enum SortCycleMode
{
    ThreeState,
    TwoState
}

/// <summary>
/// How null values sort relative to non-null values. AlwaysLast is default. Per §6.4.3.
/// FirstOnAsc/LastOnAsc make null position direction-dependent.
/// </summary>
public enum NullSortOrder
{
    AlwaysFirst,
    AlwaysLast,
    FirstOnAsc,
    LastOnAsc
}
