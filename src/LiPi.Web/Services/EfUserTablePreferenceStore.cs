// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §25.3.4 (consumer-provides storage)
// CROSS-REF: CHANGE-LOG.md A38 (Option C architecture, per-clinic persistence),
//            A39 (Stage 1C wire-up, Core→Identity move)
// PHASE: 2.8 Data Display — Stage 1C (consumer implementation)
//
// Concrete IUserTablePreferenceStore for LiPi HIS. Persists rows in the per-clinic DB
// via ClinicDbFactory.CreateForClinicAsync(clinicId), which returns an IdentityDbContext
// pointed at the current clinic's database. The table is identity.user_table_preferences
// (sibling of identity.user_preferences).
//
// Clinic resolution: the store reads the current clinicId from the authenticated
// principal's "clinicId" claim — the same claim ClaimsHelper.ClinicId reads. Memoized
// per circuit (one clinic per circuit) so the background debounced write path never
// touches AuthenticationStateProvider off-circuit.
//
// Error handling: this store does NOT catch — it lets exceptions propagate.
// TablePreferenceService (the high-level service in LiPi.Components) wraps every store
// call in try/catch and logs at Warning level (persistence is silent infrastructure
// per §21.5). Keeping the store catch-free preserves the separation: store = raw CRUD,
// service = resilience.
//
// RenameAsync: TableId is part of the composite PK, so a rename can't mutate the key in
// place — it removes the old row and re-inserts under the new id (overwriting any
// existing new-id row, per the interface's documented semantics).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiPi.Clinic.Identity.Entities;
using LiPi.Components.DataDisplay;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

/// <summary>
/// EF Core implementation of <see cref="IUserTablePreferenceStore"/> backed by the
/// per-clinic <c>identity.user_table_preferences</c> table. Scoped per circuit.
/// </summary>
public sealed class EfUserTablePreferenceStore : IUserTablePreferenceStore
{
    private readonly ClinicDbFactory _clinicDbFactory;
    private readonly AuthenticationStateProvider _authProvider;

    private Guid? _cachedClinicId;
    private bool _clinicResolved;

    public EfUserTablePreferenceStore(
        ClinicDbFactory clinicDbFactory,
        AuthenticationStateProvider authProvider)
    {
        _clinicDbFactory = clinicDbFactory ?? throw new ArgumentNullException(nameof(clinicDbFactory));
        _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
    }

    public async Task<string?> ReadAsync(Guid userId, string tableId, CancellationToken ct = default)
    {
        var clinicId = await ResolveClinicIdAsync().ConfigureAwait(false);
        if (clinicId is null) return null;

        await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId.Value).ConfigureAwait(false);
        if (db is null) return null;

        var row = await db.UserTablePreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TableId == tableId, ct)
            .ConfigureAwait(false);

        return row?.PrefsJson;
    }

    public async Task WriteAsync(Guid userId, string tableId, string prefsJson, CancellationToken ct = default)
    {
        var clinicId = await ResolveClinicIdAsync().ConfigureAwait(false);
        if (clinicId is null) return;

        await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId.Value).ConfigureAwait(false);
        if (db is null) return;

        var existing = await db.UserTablePreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TableId == tableId, ct)
            .ConfigureAwait(false);

        if (existing is null)
        {
            db.UserTablePreferences.Add(new UserTablePreference
            {
                UserId = userId,
                TableId = tableId,
                PrefsJson = prefsJson,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.PrefsJson = prefsJson;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid userId, string tableId, CancellationToken ct = default)
    {
        var clinicId = await ResolveClinicIdAsync().ConfigureAwait(false);
        if (clinicId is null) return;

        await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId.Value).ConfigureAwait(false);
        if (db is null) return;

        var existing = await db.UserTablePreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TableId == tableId, ct)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            db.UserTablePreferences.Remove(existing);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyList<string>> ListTableIdsAsync(Guid userId, CancellationToken ct = default)
    {
        var clinicId = await ResolveClinicIdAsync().ConfigureAwait(false);
        if (clinicId is null) return Array.Empty<string>();

        await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId.Value).ConfigureAwait(false);
        if (db is null) return Array.Empty<string>();

        return await db.UserTablePreferences
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.TableId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task RenameAsync(Guid userId, string oldId, string newId, CancellationToken ct = default)
    {
        var clinicId = await ResolveClinicIdAsync().ConfigureAwait(false);
        if (clinicId is null) return;

        await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId.Value).ConfigureAwait(false);
        if (db is null) return;

        var old = await db.UserTablePreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TableId == oldId, ct)
            .ConfigureAwait(false);
        if (old is null) return;

        // TableId is part of the PK — cannot mutate in place. Remove + re-add.
        // Overwrite any existing new-id row (documented interface semantics).
        var target = await db.UserTablePreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TableId == newId, ct)
            .ConfigureAwait(false);
        if (target is not null)
            db.UserTablePreferences.Remove(target);

        db.UserTablePreferences.Remove(old);
        db.UserTablePreferences.Add(new UserTablePreference
        {
            UserId = userId,
            TableId = newId,
            PrefsJson = old.PrefsJson,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolve and memoize the current clinic GUID from the "clinicId" claim. One clinic
    /// per circuit, so caching is correct and keeps the background write path off
    /// AuthenticationStateProvider.
    /// </summary>
    private async ValueTask<Guid?> ResolveClinicIdAsync()
    {
        if (_clinicResolved) return _cachedClinicId;

        var state = await _authProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
        var raw = state.User.FindFirst("clinicId")?.Value;
        _cachedClinicId = Guid.TryParse(raw, out var g) ? g : null;
        _clinicResolved = true;
        return _cachedClinicId;
    }
}
