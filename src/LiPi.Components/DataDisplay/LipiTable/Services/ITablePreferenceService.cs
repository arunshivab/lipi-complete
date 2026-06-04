// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §21.4.1 (ITablePreferenceService surface)
//   §21.4.2 (Scoped lifetime — one instance per Blazor circuit)
//   §21.5   (Silent error handling — persistence is infrastructure)
//   §25.3.4 (Consuming-app-provides-implementation contract — but see A38)
// PHASE: 2.8 Data Display — Stage 1B (services foundation)
// COMPONENT: LipiTable persistence
//
// High-level persistence service that LipiTable consumes. Operates in terms of the rich
// TablePreferences record (typed columns/sort/filter/density/page-size shape). The default
// implementation (TablePreferenceService) ships in LiPi.Components and handles JSON
// serialization, per-circuit in-memory caching, debounced writes, and error logging
// internally. Consumers implement only the two lower-level abstractions:
// IUserTablePreferenceStore (raw CRUD) and ICurrentUserAccessor (auth resolution).
//
// Architecture: Option C (store abstraction) decided in CHANGE-LOG A38. This split mirrors
// ASP.NET Identity's UserManager<T> + IUserStore<T> pattern — the library owns the
// high-level concerns; the consuming app owns the storage + auth specifics.
//
// LipiTable consumes this via [Inject] in its scoped CSS-isolated context. The single
// Save flow uses fire-and-forget semantics with internal debouncing — LipiTable never
// awaits Save completion to keep the UI responsive.

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// High-level table-preference persistence. LipiTable consumes this interface; consumers
/// register the default <c>TablePreferenceService</c> in DI and supply implementations of
/// <see cref="IUserTablePreferenceStore"/> and <see cref="ICurrentUserAccessor"/>.
/// </summary>
public interface ITablePreferenceService
{
    /// <summary>
    /// Load preferences for the current user + given tableId. Returns null when no
    /// preferences are stored OR when the user is unauthenticated OR when load fails
    /// (failures are logged silently per §21.5; consumers should not surface to the UI).
    /// </summary>
    Task<TablePreferences?> GetAsync(string tableId, CancellationToken ct = default);

    /// <summary>
    /// Save preferences for the current user + given tableId. The default implementation
    /// returns immediately and writes asynchronously via a 300ms debounce window — rapid
    /// successive calls coalesce into a single store write. Disposal of the service
    /// flushes pending writes synchronously.
    /// </summary>
    Task SaveAsync(string tableId, TablePreferences prefs, CancellationToken ct = default);

    /// <summary>
    /// Reset preferences for the current user + given tableId (deletes the stored row).
    /// Cancels any pending debounced write. Errors are logged silently.
    /// </summary>
    Task ResetAsync(string tableId, CancellationToken ct = default);

    /// <summary>
    /// List all tableIds for which the current user has stored preferences. Returns an
    /// empty list if the user is unauthenticated or the list cannot be retrieved.
    /// </summary>
    Task<IReadOnlyList<string>> ListTableIdsAsync(CancellationToken ct = default);

    /// <summary>
    /// Rename a stored tableId for the current user (used for spec migrations when a
    /// developer renames a TableId across releases). Cancels any pending debounced write
    /// for the old id. No-op if old and new ids are equal or either is whitespace.
    /// </summary>
    Task RenameTableIdAsync(string oldId, string newId, CancellationToken ct = default);
}
