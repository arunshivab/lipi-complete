// SPEC:  docs/03-LipiDynamicTabs-Spec.md §2
// PHASE: 2.6.2 — Overlay Surfaces
//
// Applied to @page components that should appear in the dynamic tab strip.
// Example:
//   @page "/patients/{Uhid}"
//   @attribute [DynamicTab(TabKey = "patient-{Uhid}", TitleProvider = nameof(GetTabTitle),
//                          IconProvider = nameof(GetTabIcon), CloseRoute = "/dashboard")]

namespace LiPi.Components.Overlays;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DynamicTabAttribute : Attribute
{
    /// <summary>Unique tab key template. Use route parameter names in braces:
    /// e.g., "patient-{Uhid}". LipiDynamicTabsService resolves the template
    /// against the actual URL segments at navigation time.</summary>
    public required string TabKey { get; init; }

    /// <summary>Name of the public method on the page component that returns
    /// the current tab title (string). Re-called when the service calls UpdateTitle.
    /// Example: nameof(GetTabTitle)</summary>
    public string? TitleProvider { get; init; }

    /// <summary>Name of the public method on the page component that returns
    /// the icon name (string). Optional.</summary>
    public string? IconProvider { get; init; }

    /// <summary>Route to navigate to when this tab is closed and no other
    /// tab is available. Default: "/dashboard".</summary>
    public string CloseRoute { get; init; } = "/dashboard";
}
