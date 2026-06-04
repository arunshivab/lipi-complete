// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §25.3.4 (consumer-provides auth)
// CROSS-REF: CHANGE-LOG.md A38 (Option C architecture), A39 (Stage 1C wire-up)
// PHASE: 2.8 Data Display — Stage 1C (consumer implementation)
//
// Concrete ICurrentUserAccessor for LiPi HIS. Resolves the current user's GUID from the
// authenticated principal's NameIdentifier claim — the same claim ClaimsHelper.UserId
// reads everywhere else in the app.
//
// Memoization: a Blazor Server circuit serves exactly one user for its lifetime, so the
// resolved GUID is cached after the first call. This matters because
// TablePreferenceService performs debounced writes on a background Task — caching the
// id on the first (circuit-thread) call means the background path never has to touch
// AuthenticationStateProvider off-circuit.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LiPi.Components.DataDisplay;
using Microsoft.AspNetCore.Components.Authorization;

namespace LiPi.Web.Services;

/// <summary>
/// Resolves the current user's GUID from the authenticated principal (NameIdentifier
/// claim). Scoped per circuit; memoizes after first resolution. Returns null when no
/// user is authenticated.
/// </summary>
public sealed class BlazorCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly AuthenticationStateProvider _authProvider;

    private Guid? _cachedUserId;
    private bool _resolved;

    public BlazorCurrentUserAccessor(AuthenticationStateProvider authProvider)
        => _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));

    public async ValueTask<Guid?> GetUserIdAsync(CancellationToken ct = default)
    {
        if (_resolved) return _cachedUserId;

        var state = await _authProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
        var raw = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _cachedUserId = Guid.TryParse(raw, out var g) ? g : null;
        _resolved = true;
        return _cachedUserId;
    }
}
