// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §3.2 (column model), §3.3 (types),
//       §3.6 (width), §3.3.3 (alignment)
// PHASE: 2.8 Data Display — Stage 2 core shell
// COMPONENT: LipiTable internal — type-erased column descriptor
//
// LipiColumn<TItem, TValue> is generic over TValue, but LipiTable<TItem> holds a list of
// columns of mixed TValue. To avoid the table reflecting into open generics, each
// LipiColumn registers a type-erased ColumnDefinition<TItem> with the parent. The
// definition exposes a GetValue(TItem) -> object? accessor (TValue boxed) plus all the
// presentation metadata the table needs to render header + cell.
//
// This keeps the table's render loop free of TValue: it calls def.GetValue(row) and
// formats per def.Type / def.Format. Type-specific behavior (sort comparers, filter
// templates) that genuinely needs TValue is deferred to later stages and captured as
// erased delegates when those stages land.

using System;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Type-erased descriptor for one column, registered by a LipiColumn&lt;TItem, TValue&gt;
/// with its parent LipiTable&lt;TItem&gt;. Generic only over TItem; TValue is erased to
/// object? at the GetValue boundary so the table can hold a homogeneous column list.
/// </summary>
public sealed class ColumnDefinition<TItem>
{
    /// <summary>Stable per-column identity (explicit ColumnKey or derived from Field path).</summary>
    public required string ColumnKey { get; init; }

    /// <summary>Resolved header text (template &gt; Header &gt; humanized field &gt; key).</summary>
    public required string Header { get; init; }

    /// <summary>True when a HeaderTemplate fragment is supplied (table renders it instead of text).</summary>
    public bool HasHeaderTemplate { get; init; }

    /// <summary>Built-in render type. Custom signals "use CellTemplate".</summary>
    public ColumnType Type { get; init; } = ColumnType.Text;

    /// <summary>Resolved horizontal alignment (explicit Align or type/position default).</summary>
    public ColumnAlign Align { get; init; } = ColumnAlign.Left;

    /// <summary>CSS grid track size for this column (explicit Width or type default).</summary>
    public required string GridTrack { get; init; }

    /// <summary>Format string passed to string.Format as "{0:Format}". Null = type default.</summary>
    public string? Format { get; init; }

    /// <summary>True when the column has no value source (template-only / Actions).</summary>
    public bool IsTemplateOnly { get; init; }

    /// <summary>True when Copyable opt-in is set (cell shows hover-copy affordance — §3.8).</summary>
    public bool Copyable { get; init; }

    /// <summary>What the hover-copy affordance copies. Value (the cell value) is default. Per §3.8.</summary>
    public CopyTarget CopyTarget { get; init; } = CopyTarget.Value;

    /// <summary>
    /// For Avatar columns: when true, the value is treated as a name string and rendered as an
    /// initials circle (deterministic hash color). When false (default), a value that looks like a
    /// URL renders as a circular image; otherwise falls back to initials. Per §3.3.2 Avatar.
    /// </summary>
    public bool IsInitials { get; init; }

    /// <summary>True when the column is currently visible (false = skip in layout).</summary>
    public bool Visible { get; init; } = true;

    /// <summary>
    /// Boxed value accessor. Returns the cell value for a row as object? (TValue boxed),
    /// or null for template-only columns. The table formats the result per Type/Format.
    /// </summary>
    public required Func<TItem, object?> GetValue { get; init; }

    /// <summary>
    /// Renders the caller's CellTemplate for a row, or null when no template is set.
    /// Captured type-erased so the table's render loop stays TValue-free.
    /// </summary>
    public Func<TItem, Microsoft.AspNetCore.Components.RenderFragment?>? CellTemplate { get; init; }

    /// <summary>Renders the caller's HeaderTemplate, or null when not set.</summary>
    public Microsoft.AspNetCore.Components.RenderFragment? HeaderTemplate { get; init; }

    // ── Sort (Stage S1) ──────────────────────────────────────────────────
    /// <summary>True when this column can be sorted (resolved: caller's Sortable AND a
    /// sortable type AND has a value source). The table only renders a sort affordance
    /// and accepts header clicks when true.</summary>
    public bool Sortable { get; init; }

    /// <summary>
    /// Type-erased comparer for two boxed cell values (the TValue comparer captured as
    /// Comparison&lt;object?&gt;). Null = the table uses its default comparison (IComparable,
    /// with null handled per NullSortOrder). Non-null = the caller's SortComparer&lt;TValue&gt;,
    /// erased so the table stays TValue-free.
    /// </summary>
    public Comparison<object?>? SortComparer { get; init; }

    // ── Quick search (Stage S2) ──────────────────────────────────────────
    /// <summary>True when this column participates in quick search (resolved: caller's
    /// Searchable AND has a value source). The table matches search terms against this
    /// column's GetValue(item)?.ToString() when true. Per-column opt-out via Searchable=false.</summary>
    public bool Searchable { get; init; }

    // ── Column filter (Stage S3) ─────────────────────────────────────────
    /// <summary>True when this column can be filtered (resolved: caller's Filterable AND a
    /// filterable type AND has a value source). The table shows a header funnel + filter
    /// popover when true. Per-column opt-out via Filterable=false.</summary>
    public bool Filterable { get; init; }
}
