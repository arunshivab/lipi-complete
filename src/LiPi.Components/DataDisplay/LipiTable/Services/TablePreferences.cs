// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §21.2   (TablePreferences shape)
//   §21.3   (ColumnPreference shape)
//   §21.7   (Versioning — Version field for forward-compat migration)
//   §23.10.7 (PersistedContext + PersistedTrigger — deferred from Stage 1A per A37)
// PHASE: 2.8 Data Display — Stage 1B (services foundation)
// COMPONENT: LipiTable persistence
//
// The serialized shape of per-user-per-table preferences. JSON-friendly:
//   • Nullable fields throughout — unset means "no override, use table defaults"
//   • Strings for enum-shaped values (ColumnPreference.Pin) so JSON stays version-portable
//   • Version field on the outer record for future migration logic
//
// PersistedContext + PersistedTrigger live here (not in Contexts.cs) because they
// reference TablePreferences. Stage 1A's A37 documented this deferral; A38 ships them.
//
// JSON wire format (camelCase via TablePreferenceService's JsonSerializerOptions):
//   {
//     "version": 1,
//     "columns": { "patientName": { "width": "200px", "order": 0, "pin": "Left", "visible": true } },
//     "sort":    [ { "columnKey": "createdAt", "direction": "Descending", "priority": 0 } ],
//     "filters": [ { "columnKey": "status", "operator": "Equals", "value": "Active" } ],
//     "quickSearch": "smith",
//     "groupBy": [ "department" ],
//     "density": "Comfortable",
//     "pageSize": 50
//   }

using System.Collections.Generic;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Top-level preferences record. All fields are optional — unset means "no override,
/// use table defaults". Version drives forward-compat migration logic when the shape
/// evolves in later LiPi versions.
/// </summary>
public sealed class TablePreferences
{
    /// <summary>Schema version. Increment when the wire format changes. v1.0 = 1.</summary>
    public int Version { get; init; } = 1;

    /// <summary>Per-column overrides keyed by column key. Null = use column defaults.</summary>
    public IReadOnlyDictionary<string, ColumnPreference>? Columns { get; init; }

    /// <summary>Active sort chain. Null = use table default sort.</summary>
    public IReadOnlyList<SortDescriptor>? Sort { get; init; }

    /// <summary>Active filters. Null = no filters.</summary>
    public IReadOnlyList<FilterDescriptor>? Filters { get; init; }

    /// <summary>Quick-search text. Null = no quick search active.</summary>
    public string? QuickSearch { get; init; }

    /// <summary>Active group dimensions, outermost-first. Null = no grouping.</summary>
    public IReadOnlyList<string>? GroupBy { get; init; }

    /// <summary>
    /// Density override (string form of TableDensity enum: "Compact" | "Comfortable" |
    /// "Spacious"). Stored as string for JSON portability across enum-renames.
    /// </summary>
    public string? Density { get; init; }

    /// <summary>Active page size. Null = use table default.</summary>
    public int? PageSize { get; init; }

    /// <summary>LipiSlicerPanel UI state (collapsed flag + visible column keys). Null = use panel
    /// defaults. Additive (PR5c); absent in older stored JSON, which deserializes to null.</summary>
    public SlicerPanelPreference? SlicerPanel { get; init; }
}

/// <summary>
/// Persisted state for a LipiSlicerPanel: whether the pane is collapsed and which candidate
/// columns the user has shown. Stored under the panel's own PreferenceKey (kept distinct from any
/// table TableId). Null VisibleColumns is treated as empty.
/// </summary>
public sealed record SlicerPanelPreference(bool Collapsed, IReadOnlyList<string> VisibleColumns);

/// <summary>
/// Per-column override. All fields participate in persistence except Order which is
/// always required (column order is fundamental to layout). Pin is stored as string
/// for JSON portability (matches the rest of TablePreferences).
/// </summary>
public sealed record ColumnPreference(
    string? Width,
    int Order,
    string Pin,        // "None" | "Left" | "Right"
    bool Visible);

/// <summary>
/// Fired by LipiTable's OnPreferencesPersisted event whenever preferences successfully
/// persist. Consuming pages can subscribe for telemetry / audit purposes. The new prefs
/// snapshot lets the consumer compute diffs without re-reading from storage.
/// </summary>
public sealed record PersistedContext(
    string TableId,
    TablePreferences NewPrefs,
    PersistedTrigger Trigger);

/// <summary>
/// What change caused preferences to persist. Used for diagnostic logging and audit
/// trails — not for control flow inside LipiTable itself.
/// </summary>
public enum PersistedTrigger
{
    ColumnLayoutChange,
    SortChange,
    FilterChange,
    DensityChange,
    PageSizeChange,
    GroupChange,
    Reset
}
