// SPEC:  docs/02-LipiDrawer-Spec.md §5, §6
// PHASE: 2.6.2 — Overlay Surfaces

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using LiPi.Web.Components.Shared;

namespace LiPi.Web.Services;

/// <summary>
/// <see cref="ILipiDrawerService"/> implementation.
/// Tracks one drawer per side (max 4 total — one per placement).
/// </summary>
public sealed class LipiDrawerService : ILipiDrawerService
{
    private readonly ILogger<LipiDrawerService> _log;
    private readonly Dictionary<DrawerPlacement, DrawerRequest> _active = new();
    private readonly object _lock = new();

    public LipiDrawerService(ILogger<LipiDrawerService> log) => _log = log;

    public event Action? OnStateChanged;

    public bool IsOpen(DrawerPlacement placement)
    {
        lock (_lock) return _active.ContainsKey(placement);
    }

    internal IReadOnlyDictionary<DrawerPlacement, DrawerRequest> GetActive()
    {
        lock (_lock) return new Dictionary<DrawerPlacement, DrawerRequest>(_active);
    }

    public async Task<TResult?> ShowAsync<TComponent, TResult>(
        DrawerPlacement              placement  = DrawerPlacement.Right,
        Dictionary<string, object?>? parameters = null,
        DrawerSize                   size       = DrawerSize.Standard,
        string?                      title      = null)
        where TComponent : ComponentBase
    {
        // Close existing drawer on same side (rule: max 1 per side)
        DrawerRequest? existing;
        lock (_lock) _active.TryGetValue(placement, out existing);
        if (existing is not null)
        {
            _log.LogWarning(
                "[LipiDrawer] Replacing existing {Placement} drawer with new one.",
                placement);
            existing.Tcs.TrySetResult(null);
            // Give UI time to animate out before new one animates in
            await Task.Delay(50);
        }

        var tcs       = new TaskCompletionSource<object?>();
        var allParams = parameters ?? new Dictionary<string, object?>();
        allParams["OnResult"] = Microsoft.AspNetCore.Components.EventCallback.Factory.Create<TResult>(
            this, (TResult result) => tcs.TrySetResult(result));

        var sideIndex = (int)placement;
        var req = new DrawerRequest
        {
            Title         = title ?? string.Empty,
            Placement     = placement,
            Size          = size,
            ComponentType = typeof(TComponent),
            Parameters    = allParams,
            Tcs           = tcs,
            ZIndex        = 700 + sideIndex * 5
        };

        lock (_lock) _active[placement] = req;
        OnStateChanged?.Invoke();

        try
        {
            var result = await tcs.Task;
            if (result is TResult typed) return typed;
            return default;
        }
        finally
        {
            lock (_lock) _active.Remove(placement);
            OnStateChanged?.Invoke();
        }
    }

    public Task CloseAsync(DrawerPlacement placement)
    {
        DrawerRequest? req;
        lock (_lock) _active.TryGetValue(placement, out req);
        req?.Tcs.TrySetResult(null);
        return Task.CompletedTask;
    }

    public Task CloseAllAsync()
    {
        List<DrawerRequest> all;
        lock (_lock) all = _active.Values.ToList();
        foreach (var req in all)
            req.Tcs.TrySetResult(null);
        return Task.CompletedTask;
    }
}
