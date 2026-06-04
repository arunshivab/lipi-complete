// SPEC:  docs/02-LipiDrawer-Spec.md §5
// PHASE: 2.6.2 — Overlay Surfaces

using Microsoft.AspNetCore.Components;
using LiPi.Components.Overlays;

namespace LiPi.Components.Overlays;

/// <summary>
/// Programmatic API for opening drawers from services or code-behind.
/// Registered as Scoped (per Blazor circuit).
/// </summary>
public interface ILipiDrawerService
{
    /// <summary>Open any <see cref="ComponentBase"/> as a drawer.
    /// Component must expose <c>[Parameter] EventCallback&lt;TResult&gt; OnResult</c>.</summary>
    Task<TResult?> ShowAsync<TComponent, TResult>(
        DrawerPlacement              placement  = DrawerPlacement.Right,
        Dictionary<string, object?>? parameters = null,
        DrawerSize                   size       = DrawerSize.Standard,
        string?                      title      = null)
        where TComponent : ComponentBase;

    /// <summary>Close the drawer at the specified placement.</summary>
    Task CloseAsync(DrawerPlacement placement);

    /// <summary>Returns true if a drawer is currently open at the given placement.</summary>
    bool IsOpen(DrawerPlacement placement);

    /// <summary>Close all open drawers.</summary>
    Task CloseAllAsync();

    /// <summary>Fired whenever drawer state changes — LipiOverlayHost subscribes.</summary>
    event Action OnStateChanged;
}

// ── Internal drawer request record ────────────────────────────────────────────
public sealed class DrawerRequest
{
    public required string                       Title       { get; init; }
    public string?                               Subtitle    { get; init; }
    public DrawerPlacement                       Placement   { get; init; } = DrawerPlacement.Right;
    public DrawerSize                            Size        { get; init; } = DrawerSize.Standard;
    public DrawerBackdrop                        Backdrop    { get; init; } = DrawerBackdrop.Dimmed;
    public required Type                         ComponentType { get; init; }
    public Dictionary<string, object?>?          Parameters  { get; init; }
    public required TaskCompletionSource<object?> Tcs        { get; init; }
    public bool                                  ShowClose   { get; init; } = true;
    public bool                                  CloseOnEsc  { get; init; } = true;
    public bool                                  CloseOnBack { get; init; } = true;
    public bool                                  Animate     { get; init; } = true;
    public int                                   ZIndex      { get; set; } = 700;
}
