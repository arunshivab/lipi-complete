// SPEC:  docs/00-Phase2.6.2-Overview.md — Shared infrastructure
// PHASE: 2.6.2 — Overlay Surfaces
// JS:    wwwroot/js/lipi-overlay-interop.js (window.lipiOverlay.lockBodyScroll)

using Microsoft.JSInterop;

namespace LiPi.Components.Overlays;

/// <summary>
/// Reference-counted <see cref="IScrollLockService"/> via JS interop.
/// Scoped — one instance per Blazor circuit.
/// </summary>
public sealed class ScrollLockService : IScrollLockService
{
    private readonly IJSRuntime _js;
    private int _count;

    public ScrollLockService(IJSRuntime js) => _js = js;

    public async ValueTask LockAsync()
    {
        _count++;
        if (_count == 1)
        {
            try { await _js.InvokeVoidAsync("lipiOverlay.lockBodyScroll"); }
            catch (JSException)            { }
            catch (JSDisconnectedException){ }
            catch (TaskCanceledException)  { }
        }
    }

    public async ValueTask UnlockAsync()
    {
        _count = Math.Max(0, _count - 1);
        if (_count == 0)
        {
            try { await _js.InvokeVoidAsync("lipiOverlay.unlockBodyScroll"); }
            catch (JSException)            { }
            catch (JSDisconnectedException){ }
            catch (TaskCanceledException)  { }
        }
    }
}
