// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Theme Provider Component
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 5
//
// Cascaded by ThemeProvider.razor via CascadingValue<ThemeContext> Name="CurrentTheme".
// Child components consume it via:
//   [CascadingParameter(Name = "CurrentTheme")] ThemeContext? Theme { get; set; }
//
// For actions (switching mode/brand), child components use the ThemeProvider self-reference:
//   [CascadingParameter(Name = "ThemeProvider")] ThemeProvider? Provider { get; set; }
//   await Provider!.SwitchModeAsync("dark");

namespace LiPi.Web.Components.Theme;

/// <summary>
/// Immutable snapshot of the currently active theme dimensions.
/// Cascaded down the component tree by <see cref="ThemeProvider"/>.
///
/// Because this is a record, use 'with' expressions to produce modified copies:
///   _currentTheme = _currentTheme with { Mode = "dark" };
///
/// ThemeProvider re-renders all subscribers (IsFixed=false CascadingValue)
/// whenever Brand or Mode changes.
/// </summary>
public record ThemeContext
{
    /// <summary>
    /// Active brand identifier. Matches data-brand attribute on body
    /// and a row in master.brand_themes.
    /// Example: "lipi-default", "armoki"
    /// </summary>
    public string Brand { get; init; } = "lipi-default";

    /// <summary>
    /// Active mode. One of: light | dark | auto | high-contrast.
    /// Matches data-mode attribute on body.
    /// Default: "light" (per Decision #12.4).
    /// </summary>
    public string Mode { get; init; } = "light";

    // ── Convenience properties ─────────────────────────────────────────────

    /// <summary>True when Mode is "dark".</summary>
    public bool IsDark => Mode == "dark";

    /// <summary>True when Mode is "light".</summary>
    public bool IsLight => Mode == "light";

    /// <summary>True when the default LiPi brand is active.</summary>
    public bool IsDefaultBrand => Brand == "lipi-default";

    // ── Static instances ───────────────────────────────────────────────────

    /// <summary>
    /// Safe default theme. Used as the initial value before OnInitializedAsync resolves
    /// the user's actual preference. Matches the hard defaults in ThemeContextService.
    /// </summary>
    public static ThemeContext Default { get; } = new();
}
