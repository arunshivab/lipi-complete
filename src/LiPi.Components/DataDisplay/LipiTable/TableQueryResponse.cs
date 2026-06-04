// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §4.3 (TableQueryResponse definition)
//   §4.3.3 (TotalCount = -1 means "unknown total")
//   §4.3.4 (Aggregates contract)
//   §4.3.5 (PreGrouped + GroupBucket)
//   §4.3.6 (TreeChildren lazy-load contract)
//   §4.3.7 (FilteredCount vs TotalCount)
//   §4.3.8 (CapBanner)
//   §4.3.9 (ServerExtra round-trip)
//   §12.10.2 (ConflictInfo descriptor)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiTable
//
// Server-side data source response payload, plus the concurrency conflict descriptor.
// ConflictInfo lives here (rather than in SaveResult.cs) because it describes the
// server-to-client direction of the data contract — same conceptual group as
// TableQueryResponse. SaveResult.Conflict references ConflictInfo via this file.

using System;
using System.Collections.Generic;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Server-side data source response payload. Carries the page of rows plus optional
/// aggregates, pre-grouped buckets, tree children, filtered counts, and cap banner.
/// Generic over TItem so the rows preserve their declared type through the round-trip.
/// Per §4.3.
/// </summary>
public sealed record TableQueryResponse<TItem>(
    IReadOnlyList<TItem> Rows,
    long TotalCount)
{
    /// <summary>
    /// Column-key → aggregate value. Returned when TableQueryRequest.AggregateColumns
    /// is non-empty. If the table has aggregate columns but this is null, LipiTable
    /// renders em-dash in those footer cells and logs a developer-mode warning. Per §4.3.4.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Aggregates { get; init; }

    /// <summary>
    /// Pre-grouped row buckets. Server returns this for performance when grouping is
    /// active. When null but Groups are present, LipiTable groups the flat Rows
    /// client-side (only valid when the rows include all data for the current page).
    /// Per §4.3.5.
    /// </summary>
    public IReadOnlyList<GroupBucket<TItem>>? PreGrouped { get; init; }

    /// <summary>
    /// Parent-key → children map. Set when responding to a tree expansion request
    /// (TableQueryRequest.ExpandedParentKey was non-null). Per §4.3.6.
    /// </summary>
    public IReadOnlyDictionary<object, IReadOnlyList<TItem>>? TreeChildren { get; init; }

    /// <summary>
    /// Filtered row count when distinct from TotalCount. Set only when the consuming
    /// page wants the "47 of 4,872" filtered-of-total display. Per §4.3.7.
    /// </summary>
    public long? FilteredCount { get; init; }

    /// <summary>
    /// Custom cap banner text when the server caps an "All" request to ServerSideAllCap.
    /// If null and a cap was applied, LipiTable renders a default banner using TotalCount
    /// and Rows.Count. Per §4.3.8.
    /// </summary>
    public string? CapBanner { get; init; }

    /// <summary>
    /// Round-trip server-supplied side payload. Surfaced to the consuming page via the
    /// OnQueryComplete event for side-channel use (server statistics, warnings, etc.).
    /// LipiTable itself does not interpret this. Per §4.3.9.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ServerExtra { get; init; }
}

/// <summary>
/// A group of rows returned by the server, optionally with nested sub-groups for
/// multi-level grouping. Items contains the rows in this group's leaf level when
/// SubGroups is null; SubGroups contains nested buckets when grouping is multi-level.
/// Per §4.3.5.
/// </summary>
public sealed record GroupBucket<TItem>(
    string ColumnKey,
    object? GroupValue,
    long ItemCount,
    IReadOnlyList<TItem> Items,
    IReadOnlyDictionary<string, object?>? Aggregates,
    IReadOnlyList<GroupBucket<TItem>>? SubGroups);

/// <summary>
/// Concurrency conflict descriptor returned in SaveResult.Conflict when the caller's
/// OnRowSave handler detects that the row changed on the server since the user started
/// editing. LipiTable renders the conflict UX (banner, modal, or custom template) per
/// the table's ConflictResolutionMode. Per §12.10.2.
///
/// ServerRow is typed as object? because SaveResult is non-generic — the consuming
/// page boxes the row at construction and unboxes in its conflict UI handler.
/// </summary>
public sealed class ConflictInfo
{
    /// <summary>The current server-side state of the row (boxed TItem).</summary>
    public object? ServerRow { get; init; }

    /// <summary>The server-side row version (rowversion, etag, integer, guid, etc.).</summary>
    public object? ServerRowVersion { get; init; }

    /// <summary>Optional display name of the user who made the conflicting change.</summary>
    public string? ConflictingUserDisplay { get; init; }

    /// <summary>Optional timestamp of the conflicting change.</summary>
    public DateTime? ConflictedAt { get; init; }
}
