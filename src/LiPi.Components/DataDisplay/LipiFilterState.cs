// SPEC: docs/00-COMPONENTS — Phase 2.8 Data Display / filtering (PR5a shared filter model).
// PR5a — the shared, mutable filter model bound BY REFERENCE into LipiTable and (PR5b) LipiSlicer.
// It owns the committed FilterDescriptor set plus a lightweight per-column accessor+type registry
// so any holder can evaluate the filters — e.g. a slicer computing FACETED counts (apply every
// other column's filter, then group its own). Every mutation raises Changed(current, previous,
// reason) so all bound surfaces re-render. When the dev supplies no instance, LipiTable creates
// its own internal one, so existing single-table usages behave identically (back-compat).
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiPi.Components.DataDisplay;

public sealed class LipiFilterState<TItem>
{
    private readonly List<FilterDescriptor> _filters = new();
    private readonly Dictionary<string, Binding> _columns = new();
    private sealed record Binding(Func<TItem, object?> Accessor, ColumnType Type, string Header, bool Filterable);
    private readonly List<string> _columnOrder = new();   // registration order (candidate order)

    /// <summary>Evaluation options, configured by the table (sensible defaults otherwise).</summary>
    public bool CaseSensitive { get; private set; }
    public TimeZoneInfo? TimeZone { get; private set; }
    public DayOfWeek WeekStart { get; private set; } = DayOfWeek.Sunday;

    /// <summary>Raised on every mutation. The context carries (current, previous, reason).</summary>
    public event Action<FilterChangedContext>? Changed;

    public IReadOnlyList<FilterDescriptor> Filters => _filters;
    public bool Active => _filters.Count > 0;
    public int ActiveColumnCount => _filters.Select(f => f.ColumnKey).Distinct().Count();
    public FilterDescriptor? FilterFor(string columnKey) => _filters.FirstOrDefault(f => f.ColumnKey == columnKey);

    /// <summary>Whether a column's value+type is known to the registry (drives a slicer's column-type form).</summary>
    public bool Knows(string columnKey) => _columns.ContainsKey(columnKey);
    public ColumnType? TypeOf(string columnKey) => _columns.TryGetValue(columnKey, out var b) ? b.Type : null;

    public void Configure(bool caseSensitive, TimeZoneInfo? timeZone, DayOfWeek weekStart)
    {
        CaseSensitive = caseSensitive;
        TimeZone = timeZone;
        WeekStart = weekStart;
    }

    /// <summary>Register/replace a column's value accessor + type (and, for the slicer panel's
    /// auto-derive + column picker, its header label and filterable flag) for evaluation/faceting.</summary>
    public void RegisterColumn(string columnKey, Func<TItem, object?> accessor, ColumnType type,
                               string header = "", bool filterable = true)
    {
        if (!_columns.ContainsKey(columnKey)) _columnOrder.Add(columnKey);
        _columns[columnKey] = new Binding(accessor, type, header, filterable);
    }

    public void UnregisterColumn(string columnKey)
    {
        if (_columns.Remove(columnKey)) _columnOrder.Remove(columnKey);
    }

    /// <summary>Filterable columns known to the state, in registration order — the auto-derive
    /// candidate set for a LipiSlicerPanel with no explicit options. DefaultVisible is true.</summary>
    public IReadOnlyList<SlicerColumnSpec<TItem>> Candidates()
    {
        var list = new List<SlicerColumnSpec<TItem>>();
        foreach (var k in _columnOrder)
            if (_columns.TryGetValue(k, out var b) && b.Filterable)
                list.Add(new SlicerColumnSpec<TItem>(
                    k, string.IsNullOrEmpty(b.Header) ? k : b.Header, b.Type, b.Accessor, true));
        return list;
    }

    /// <summary>Replace all descriptors for one column (0..n). Empty clears the column.</summary>
    public void SetColumn(string columnKey, FilterChangeReason reason, params FilterDescriptor[] descriptors)
    {
        var prev = _filters.ToArray();
        _filters.RemoveAll(f => f.ColumnKey == columnKey);
        if (descriptors.Length > 0) _filters.AddRange(descriptors);
        Changed?.Invoke(new FilterChangedContext(_filters.ToArray(), prev, reason));
    }

    public void RemoveColumn(string columnKey, FilterChangeReason reason)
    {
        if (_filters.All(f => f.ColumnKey != columnKey)) return;
        var prev = _filters.ToArray();
        _filters.RemoveAll(f => f.ColumnKey == columnKey);
        Changed?.Invoke(new FilterChangedContext(_filters.ToArray(), prev, reason));
    }

    public void Clear(FilterChangeReason reason)
    {
        if (_filters.Count == 0) return;
        var prev = _filters.ToArray();
        _filters.Clear();
        Changed?.Invoke(new FilterChangedContext(_filters.ToArray(), prev, reason));
    }

    /// <summary>Apply all active filters to <paramref name="items"/>, optionally skipping one
    /// column (a slicer excludes its own column to compute faceted counts). Columns with no
    /// registered accessor are ignored (treated as pass-through).</summary>
    public IEnumerable<TItem> Apply(IEnumerable<TItem> items, string? excludeColumnKey = null)
    {
        var active = _filters.Where(f => f.ColumnKey != excludeColumnKey).ToList();
        if (active.Count == 0) return items;
        return items.Where(row => active.All(f =>
        {
            if (!_columns.TryGetValue(f.ColumnKey, out var b)) return true;
            return LipiFilterEvaluator.Matches(b.Accessor(row), f, CaseSensitive, TimeZone, WeekStart);
        }));
    }
}

/// <summary>One slicer candidate column — emitted by a declarative LipiSlicerOption (curated) or
/// by LipiFilterState.Candidates() (auto-derive). Type-erased over TValue at the Accessor boundary
/// so a TItem-only panel can render a slicer for it without knowing TValue.</summary>
public sealed record SlicerColumnSpec<TItem>(
    string Key, string Header, ColumnType Type, Func<TItem, object?> Accessor, bool DefaultVisible = true);

/// <summary>Where a LipiSlicerPanel docks / how it lays its slicers out: Top/Bottom render a
/// horizontal bar; Left/Right render a vertical stack (the dev positions the panel beside the table).</summary>
public enum SlicerPlacement { Top, Bottom, Left, Right }
