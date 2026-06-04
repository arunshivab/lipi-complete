// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §4.2 (TableQueryRequest definition)
//   §4.2.5 (idempotency contract)
//   §4.2.6 (Extra payload)
//   §5.2.6 (SelectAllAcrossPages intent)
//   §8.2.2 (RequestAllRows / ServerSideAllCap contract)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiTable
//
// The shape sent to server-side DataSource callbacks. Immutable record. The callback
// MUST be idempotent — the same request with the same data should produce the same
// response — per §4.2.5. LipiTable may invoke the callback multiple times for the same
// state during reconciliation; the server must not have side effects.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Server-side data source request payload. The query envelope LipiTable sends to a
/// DataSource handler each time the underlying dataset needs to be re-queried (initial
/// mount, refresh, sort change, filter change, page change, group change, quick-search
/// change, or tree expansion). Per §4.2.
/// </summary>
public sealed record TableQueryRequest
{
    // ─── Pagination ──────────────────────────────────────────────────────

    /// <summary>1-indexed current page. Per §4.2.1.</summary>
    public int Page { get; init; }

    /// <summary>Current page size. May be int.MaxValue when RequestAllRows is true.</summary>
    public int PageSize { get; init; }

    /// <summary>
    /// True when the user selected "All" page size. Server should honor ServerSideAllCap
    /// (default 1000) per §8.2.2 — return capped result with a CapBanner.
    /// </summary>
    public bool RequestAllRows { get; init; }

    // ─── Sort ────────────────────────────────────────────────────────────

    /// <summary>Active sort chain, ordered by Priority. Empty when no sort applied.</summary>
    public IReadOnlyList<SortDescriptor> Sort { get; init; } = Array.Empty<SortDescriptor>();

    // ─── Filters ─────────────────────────────────────────────────────────

    /// <summary>Active column filters. Empty when no filters applied.</summary>
    public IReadOnlyList<FilterDescriptor> Filters { get; init; } = Array.Empty<FilterDescriptor>();

    /// <summary>Quick search text for free-text search across string columns. Per §7.10.</summary>
    public string? QuickSearch { get; init; }

    // ─── Grouping ────────────────────────────────────────────────────────

    /// <summary>Active group dimensions, outermost-first. Empty when not grouped.</summary>
    public IReadOnlyList<GroupDescriptor> Groups { get; init; } = Array.Empty<GroupDescriptor>();

    // ─── Tree ────────────────────────────────────────────────────────────

    /// <summary>
    /// When lazy-loading a tree node's children, the parent's KeySelector value.
    /// Null for root-level fetches. Per §4.3.6.
    /// </summary>
    public object? ExpandedParentKey { get; init; }

    /// <summary>Tree depth of the expanded parent. Null for root-level fetches.</summary>
    public int? TreeDepth { get; init; }

    // ─── Selection intent ────────────────────────────────────────────────

    /// <summary>
    /// True when the user activated the two-step "select all across pages" banner.
    /// The consuming page combines this with the current Filters to bulk-act on
    /// the matching set without enumerating keys. Per §5.2.6.
    /// </summary>
    public bool SelectAllAcrossPages { get; init; }

    // ─── Aggregates ──────────────────────────────────────────────────────

    /// <summary>
    /// Column keys for which aggregates are requested. The server returns values
    /// in TableQueryResponse.Aggregates keyed by column key.
    /// </summary>
    public IReadOnlyList<string> AggregateColumns { get; init; } = Array.Empty<string>();

    // ─── Locale / culture ────────────────────────────────────────────────

    /// <summary>Optional culture hint for server-side formatting. Rarely used.</summary>
    public string? CultureName { get; init; }

    // ─── Caller-extensible payload ───────────────────────────────────────

    /// <summary>
    /// Caller-extensible payload round-tripped opaquely through LipiTable to the
    /// DataSource handler. Used by consuming pages to pass page-specific state
    /// (e.g., a department filter wired outside LipiTable's filter UI). Per §4.2.6.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Extra { get; init; }
        = ImmutableDictionary<string, object?>.Empty;
}
