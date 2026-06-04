// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §25.3.4 (Consuming-app-provides-implementation contract)
// CROSS-REF: CHANGE-LOG.md A38 (Option C architecture — store abstraction)
// PHASE: 2.8 Data Display — Stage 1B (services foundation)
// COMPONENT: LipiTable persistence
//
// Low-level abstraction for the actual storage operations. Operates on raw JSON strings
// and user GUIDs — knows nothing about TablePreferences shape, JSON serialization,
// caching, or debouncing (all owned by TablePreferenceService).
//
// Consumers implement this against their storage of choice:
//   • EF Core + PostgreSQL (LiPi HIS — Stage 1C delivers EfUserTablePreferenceStore)
//   • Browser localStorage (would require JS interop wrapper)
//   • Redis, Azure Cosmos DB, file system, anything
//
// Method semantics:
//   • ReadAsync   — returns the stored JSON blob, or null if not stored
//   • WriteAsync  — upserts (insert or update on conflict of user_id + table_id)
//   • DeleteAsync — removes the row if present; no-op if absent
//   • ListTableIdsAsync — returns all tableIds for the given user (empty if none)
//   • RenameAsync — atomically renames a tableId for the given user

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Storage abstraction for table preferences. Consumers implement this against their
/// chosen storage backend; the LiPi component library's TablePreferenceService composes
/// implementations of this interface with ICurrentUserAccessor into the full
/// ITablePreferenceService surface.
/// </summary>
public interface IUserTablePreferenceStore
{
    /// <summary>Read the JSON blob for (userId, tableId). Returns null when no row exists.</summary>
    Task<string?> ReadAsync(Guid userId, string tableId, CancellationToken ct = default);

    /// <summary>
    /// Upsert the JSON blob for (userId, tableId). Implementations must atomically
    /// insert-or-update and refresh any updated-at timestamp the storage backend tracks.
    /// </summary>
    Task WriteAsync(Guid userId, string tableId, string prefsJson, CancellationToken ct = default);

    /// <summary>Delete the row for (userId, tableId). No-op when no row exists.</summary>
    Task DeleteAsync(Guid userId, string tableId, CancellationToken ct = default);

    /// <summary>List all tableIds for which the given user has stored preferences.</summary>
    Task<IReadOnlyList<string>> ListTableIdsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Rename a tableId for the given user. Implementations should be atomic — either
    /// the rename succeeds in full or the existing row remains untouched. No-op if
    /// oldId does not exist for this user. Behavior when newId already exists for
    /// this user is implementation-defined; the default LiPi.Web EF implementation
    /// (Stage 1C) overwrites the existing newId row.
    /// </summary>
    Task RenameAsync(Guid userId, string oldId, string newId, CancellationToken ct = default);
}
