// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §21.4.2 (Default implementation behavior)
//   §21.4.3 (300ms debounce window — locked)
//   §21.4.4 (Per-circuit in-memory cache)
//   §21.5   (Silent error handling — log warning, never throw, never surface)
//   §21.6   (Dispose flushes pending writes synchronously with timeout)
// CROSS-REF: CHANGE-LOG.md A38 (Option C architecture — this impl ships in LiPi.Components
//            because it depends only on abstractions, not on consuming-app types)
// PHASE: 2.8 Data Display — Stage 1B (services foundation)
// COMPONENT: LipiTable persistence
//
// Default implementation of ITablePreferenceService. Composes IUserTablePreferenceStore +
// ICurrentUserAccessor into the rich-shape API LipiTable consumes. Concerns owned here:
//   • JSON serialization (System.Text.Json) of TablePreferences to/from store JSON blob
//   • Per-circuit in-memory cache (avoids re-reads within the same circuit)
//   • Debounced writes (300ms) — consecutive SaveAsync calls within the window coalesce
//     into a single store write. Flush on DisposeAsync forces pending writes to commit
//     synchronously with a 2-second timeout cap.
//   • Silent error handling — persistence is infrastructure; failures are logged but
//     don't surface to the user (per §21.5)
//   • Cancellation-aware (caller's ct on synchronous API; internal CTS for debounce)
//
// Registered as Scoped (one instance per Blazor circuit) per §21.4.2. The Scoped
// registration is what makes the cache and pending-writes dictionaries safe — Blazor
// circuits are single-user.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Default implementation of <see cref="ITablePreferenceService"/>. Ships in
/// LiPi.Components — depends only on abstractions (<see cref="IUserTablePreferenceStore"/>
/// + <see cref="ICurrentUserAccessor"/>) so it remains consumer-agnostic. Register as
/// Scoped in DI.
/// </summary>
public sealed class TablePreferenceService : ITablePreferenceService, IAsyncDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Debounce window for consecutive writes to the same tableId.</summary>
    private const int DebounceMs = 300;

    /// <summary>Max time DisposeAsync waits for pending writes to flush.</summary>
    private static readonly TimeSpan DisposeFlushTimeout = TimeSpan.FromSeconds(2);

    private readonly IUserTablePreferenceStore _store;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<TablePreferenceService> _logger;

    /// <summary>Per-tableId cache. Stores the last-known value, including null for "no row".</summary>
    private readonly ConcurrentDictionary<string, TablePreferences?> _cache = new();

    /// <summary>Pending debounced writes. Keyed by tableId.</summary>
    private readonly ConcurrentDictionary<string, PendingWrite> _pendingWrites = new();

    /// <summary>Disposed flag — set true once DisposeAsync starts, blocks new scheduled writes.</summary>
    private volatile bool _disposed;

    public TablePreferenceService(
        IUserTablePreferenceStore store,
        ICurrentUserAccessor currentUser,
        ILogger<TablePreferenceService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TablePreferences?> GetAsync(string tableId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tableId)) return null;
        if (_cache.TryGetValue(tableId, out var cached)) return cached;

        var userId = await _currentUser.GetUserIdAsync(ct).ConfigureAwait(false);
        if (userId is null)
        {
            _cache[tableId] = null;
            return null;
        }

        try
        {
            var json = await _store.ReadAsync(userId.Value, tableId, ct).ConfigureAwait(false);
            if (json is null)
            {
                _cache[tableId] = null;
                return null;
            }

            TablePreferences? prefs;
            try
            {
                prefs = JsonSerializer.Deserialize<TablePreferences>(json, _jsonOptions);
            }
            catch (JsonException jex)
            {
                _logger.LogWarning(jex,
                    "Malformed TablePreferences JSON for tableId {TableId}; ignoring stored value.",
                    tableId);
                _cache[tableId] = null;
                return null;
            }

            _cache[tableId] = prefs;
            return prefs;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read TablePreferences for tableId {TableId}", tableId);
            _cache[tableId] = null;
            return null;
        }
    }

    public Task SaveAsync(string tableId, TablePreferences prefs, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tableId)) return Task.CompletedTask;
        if (prefs is null) return Task.CompletedTask;
        if (_disposed) return Task.CompletedTask;

        // Update cache immediately so subsequent GetAsync returns the new value without
        // waiting for the debounced write to land in storage.
        _cache[tableId] = prefs;

        ScheduleDebouncedWrite(tableId, prefs);
        return Task.CompletedTask;
    }

    public async Task ResetAsync(string tableId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tableId)) return;

        // Cancel any pending debounced write for this tableId — we're about to delete.
        if (_pendingWrites.TryRemove(tableId, out var pending))
        {
            pending.Cts.Cancel();
            pending.Cts.Dispose();
        }

        _cache[tableId] = null;

        var userId = await _currentUser.GetUserIdAsync(ct).ConfigureAwait(false);
        if (userId is null) return;

        try
        {
            await _store.DeleteAsync(userId.Value, tableId, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to delete TablePreferences for tableId {TableId}", tableId);
        }
    }

    public async Task<IReadOnlyList<string>> ListTableIdsAsync(CancellationToken ct = default)
    {
        var userId = await _currentUser.GetUserIdAsync(ct).ConfigureAwait(false);
        if (userId is null) return Array.Empty<string>();

        try
        {
            return await _store.ListTableIdsAsync(userId.Value, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to list TablePreferences table ids");
            return Array.Empty<string>();
        }
    }

    public async Task RenameTableIdAsync(string oldId, string newId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(oldId) || string.IsNullOrWhiteSpace(newId)) return;
        if (string.Equals(oldId, newId, StringComparison.Ordinal)) return;

        var userId = await _currentUser.GetUserIdAsync(ct).ConfigureAwait(false);
        if (userId is null) return;

        // Cancel pending writes for the old id; clear both old and new cache entries.
        if (_pendingWrites.TryRemove(oldId, out var pending))
        {
            pending.Cts.Cancel();
            pending.Cts.Dispose();
        }
        _cache.TryRemove(oldId, out _);
        _cache.TryRemove(newId, out _);

        try
        {
            await _store.RenameAsync(userId.Value, oldId, newId, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to rename TablePreferences from {OldId} to {NewId}", oldId, newId);
        }
    }

    // ─── Debounced write internals ───────────────────────────────────────

    private void ScheduleDebouncedWrite(string tableId, TablePreferences prefs)
    {
        // Replace any pending write for this tableId with a new debounce window.
        var cts = new CancellationTokenSource();
        var pending = new PendingWrite(prefs, cts);

        if (_pendingWrites.TryRemove(tableId, out var existing))
        {
            existing.Cts.Cancel();
            existing.Cts.Dispose();
        }

        _pendingWrites[tableId] = pending;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceMs, cts.Token).ConfigureAwait(false);
                await FlushSingleAsync(tableId, prefs, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer SaveAsync, or service disposed.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Background save of TablePreferences for {TableId} failed", tableId);
            }
        }, cts.Token);
    }

    private async Task FlushSingleAsync(string tableId, TablePreferences prefs, CancellationToken ct)
    {
        var userId = await _currentUser.GetUserIdAsync(ct).ConfigureAwait(false);
        if (userId is null)
        {
            if (_pendingWrites.TryRemove(tableId, out var stale))
                stale.Cts.Dispose();
            return;
        }

        string json;
        try
        {
            json = JsonSerializer.Serialize(prefs, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to serialize TablePreferences for tableId {TableId}", tableId);
            if (_pendingWrites.TryRemove(tableId, out var stale))
                stale.Cts.Dispose();
            return;
        }

        try
        {
            await _store.WriteAsync(userId.Value, tableId, json, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to write TablePreferences for tableId {TableId}", tableId);
        }
        finally
        {
            if (_pendingWrites.TryRemove(tableId, out var pair))
                pair.Cts.Dispose();
        }
    }

    /// <summary>
    /// Flush all pending debounced writes synchronously. Called on circuit teardown.
    /// Bounded by <see cref="DisposeFlushTimeout"/> — slow store implementations don't
    /// hold up circuit shutdown indefinitely.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        var snapshot = _pendingWrites.ToArray();
        _pendingWrites.Clear();

        if (snapshot.Length == 0) return;

        using var flushCts = new CancellationTokenSource(DisposeFlushTimeout);

        var flushTasks = snapshot.Select(async kvp =>
        {
            var (tableId, pair) = kvp;
            // Stop the debounce timer; flush immediately under the dispose timeout.
            pair.Cts.Cancel();
            try
            {
                await FlushImmediateAsync(tableId, pair.Prefs, flushCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to flush pending TablePreferences for {TableId} on dispose", tableId);
            }
            finally
            {
                pair.Cts.Dispose();
            }
        });

        try
        {
            await Task.WhenAll(flushTasks).ConfigureAwait(false);
        }
        catch
        {
            // Per-task errors already logged above; swallow here so dispose completes.
        }
    }

    private async Task FlushImmediateAsync(string tableId, TablePreferences prefs, CancellationToken ct)
    {
        var userId = await _currentUser.GetUserIdAsync(ct).ConfigureAwait(false);
        if (userId is null) return;

        var json = JsonSerializer.Serialize(prefs, _jsonOptions);
        await _store.WriteAsync(userId.Value, tableId, json, ct).ConfigureAwait(false);
    }

    private sealed record PendingWrite(TablePreferences Prefs, CancellationTokenSource Cts);
}
