// SPEC:  docs/02-LipiDrawer-Spec.md §4
// PHASE: 2.6.2 — Overlay Surfaces

namespace LiPi.Web.Components.Shared;

public enum DrawerPlacement
{
    Right,      // most common — slides in from right (default)
    Left,       // secondary navigation, filters
    Top,        // alerts, global notifications
    Bottom      // mobile-style action sheets
}

public enum DrawerSize
{
    Compact,    // R/L: 320px   T/B: 240px
    Standard,   // R/L: 400px   T/B: 400px (default)
    Wide,       // R/L: 560px   T/B: 480px
    FullSide    // R/L: 95vw    T/B: 95vh
}

public enum DrawerIconColor
{
    None,
    Info,
    Success,
    Warning,
    Danger,
    Critical
}

public enum DrawerBackdrop
{
    Dimmed,     // rgba(11,37,69,.45) — default, triggers focus trap + inert
    Light,      // rgba(11,37,69,.10) — faint, focus trap active, page visible
    None        // no backdrop — page stays fully interactive, no focus trap
}

public enum DrawerPinScope
{
    /// <summary>Pinned everywhere — persists across all routes.
    /// Example: Notifications, Messages.</summary>
    Global,

    /// <summary>Pinned within <c>RouteScopePrefix</c> only.
    /// Auto-closes when navigating outside the prefix.
    /// Example: Patient Summary scoped to /patients/*.</summary>
    RouteScoped,

    /// <summary>Pinned only on current page.
    /// Closes on any navigation.
    /// Example: Filter drawer, Sort panel.</summary>
    PageScoped
}
