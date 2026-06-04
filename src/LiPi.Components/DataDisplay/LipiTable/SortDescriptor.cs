// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §4.2.2 (SortDescriptor + SortDirection)
//   §4.2.3 (FilterDescriptor + FilterOperator)
//   §4.2.4 (GroupDescriptor)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiTable
//
// Sort, filter, and group descriptor records used by both the public API (programmatic
// table state) and the wire format (TableQueryRequest sent to server-side DataSource
// handlers). Records are immutable; LipiTable replaces the full list to mutate state.

namespace LiPi.Components.DataDisplay;

/// <summary>Direction of a sort. Per §4.2.2.</summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// One sort dimension. Multi-column sort uses a list of descriptors ordered by Priority
/// (0 = primary). Single-column sort always has exactly one descriptor with Priority 0.
/// Per §4.2.2.
/// </summary>
public sealed record SortDescriptor(
    string ColumnKey,
    SortDirection Direction,
    int Priority);

/// <summary>
/// One filter dimension. Value and ValueEnd shapes vary by Operator:
/// <list type="bullet">
///   <item>Between: Value = lower bound, ValueEnd = upper bound</item>
///   <item>In: Value is an IReadOnlyList&lt;object&gt;</item>
///   <item>LastNDays / NextNDays: Value is the int count</item>
///   <item>Empty / NotEmpty / IsTrue / IsFalse / Today / Yesterday / etc.: Value is null</item>
///   <item>All others (Contains, Equals, GreaterThan, etc.): Value is the comparand</item>
/// </list>
/// Per §4.2.3.
/// </summary>
public sealed record FilterDescriptor(
    string ColumnKey,
    FilterOperator Operator,
    object? Value,
    object? ValueEnd);

/// <summary>
/// One group dimension. Multi-level grouping uses a list, outermost-first. Direction
/// controls the order of group headers within their parent group. Per §4.2.4.
/// </summary>
public sealed record GroupDescriptor(
    string ColumnKey,
    SortDirection Direction = SortDirection.Ascending);

/// <summary>
/// Filter operator set. Per-ColumnType subsets apply (text columns use the text set,
/// number columns use the number set, etc.). Server-side handlers cast Value based on
/// the column's declared type. Per §4.2.3.
/// </summary>
public enum FilterOperator
{
    // Text operators
    Contains,
    NotContains,
    Equals,
    NotEquals,
    StartsWith,
    EndsWith,

    // Number / date numeric operators
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Between,

    // Date relative operators (per §7.5)
    Today,
    Tomorrow,
    Yesterday,
    ThisWeek,
    LastWeek,
    NextWeek,
    ThisMonth,
    LastMonth,
    NextMonth,
    ThisQuarter,
    LastQuarter,
    ThisYear,
    LastYear,
    LastNDays,
    NextNDays,

    // Boolean operators
    IsTrue,
    IsFalse,

    // Set operator (Value is IReadOnlyList<object>)
    In,

    // Universal operators
    Empty,
    NotEmpty
}
