// SPEC:  docs/00-Phase2.6.2-Overview.md — Shared infrastructure
// PHASE: 2.6.2 — Overlay Surfaces
//
// Focus trap service — shared by LipiModal and LipiDrawer.
// NEVER duplicate Tab/Shift+Tab handling inside individual overlay components.
// All focus trapping runs through this single service. Divergent implementations
// cause screen-reader bugs that only surface in live testing.

namespace LiPi.Web.Services;

/// <summary>
/// Manages keyboard focus trapping within overlay surfaces (modals, drawers).
/// Implemented via JS interop — Tab and Shift+Tab cycle only within the
/// registered container. Escape is NOT handled here; each overlay manages its
/// own Escape behavior.
/// <para>
/// Stack-aware: nested modals each push their container onto the trap stack.
/// Deactivate pops the topmost entry and returns focus to the previous container.
/// </para>
/// </summary>
public interface IFocusTrapService
{
    /// <summary>
    /// Activate focus trapping within <paramref name="container"/>.
    /// Moves focus to <paramref name="initialFocus"/> if provided, otherwise
    /// to the first focusable element within <paramref name="container"/>.
    /// Pushes onto an internal stack — nested calls are safe.
    /// </summary>
    Task ActivateAsync(
        Microsoft.AspNetCore.Components.ElementReference container,
        Microsoft.AspNetCore.Components.ElementReference? initialFocus = null);

    /// <summary>
    /// Deactivate the topmost focus trap and return focus to the element that
    /// held it before the trap was activated.
    /// </summary>
    Task DeactivateAsync();
}
