// SPEC:  docs/00-Phase2.6.2-Overview.md — Shared infrastructure
// PHASE: 2.6.2 — Overlay Surfaces
// JS:    wwwroot/js/lipi-overlay-interop.js (window.lipiOverlay.activateFocusTrap)

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LiPi.Components.Overlays;

/// <summary>
/// <see cref="IFocusTrapService"/> implementation using JS interop.
/// Registered as Scoped — one instance per Blazor circuit.
/// </summary>
public sealed class FocusTrapService : IFocusTrapService
{
    private readonly IJSRuntime _js;

    public FocusTrapService(IJSRuntime js) => _js = js;

    /// <inheritdoc/>
    public async Task ActivateAsync(
        ElementReference container,
        ElementReference? initialFocus = null)
    {
        try
        {
            if (initialFocus.HasValue)
                await _js.InvokeVoidAsync(
                    "lipiOverlay.activateFocusTrap", container, initialFocus.Value);
            else
                await _js.InvokeVoidAsync(
                    "lipiOverlay.activateFocusTrap", container, null);
        }
        catch (JSException)            { /* JS not loaded or lipiOverlay undefined */ }
        catch (JSDisconnectedException){ /* circuit disconnecting */ }
        catch (TaskCanceledException)  { /* component unmounting */ }
    }

    /// <inheritdoc/>
    public async Task DeactivateAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("lipiOverlay.deactivateFocusTrap");
        }
        catch (JSException)            { }
        catch (JSDisconnectedException){ }
        catch (TaskCanceledException)  { }
    }
}
