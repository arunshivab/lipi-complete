// SPEC:     docs/00-COMPONENTS/2.7/02-LipiBadge-Pill-Spec.md §2
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiBadge partial class — parameters, derived state, class composition.
// Pure-render component, no events, no JS interop. Visibility resolution
// (Count=0 + !ShowZero → render nothing) computed in OnParametersSet.

using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace LiPi.Web.Components.Shared;

public partial class LipiBadge
{
    // ==========================================================================
    // PARAMETERS (spec §2 — table of params)
    // ==========================================================================

    /// <summary>Numeric count to display. <c>null</c> means no count
    /// (typically used together with <see cref="Dot"/>=true).</summary>
    [Parameter] public int? Count { get; set; }

    /// <summary>Show as a small dot indicator (8px) with no number. When true,
    /// <see cref="Count"/> is ignored visually but still informs a11y label.</summary>
    [Parameter] public bool Dot { get; set; }

    /// <summary>Show "N+" when <see cref="Count"/> exceeds this maximum.
    /// Default 99 — "100+" shows as "99+". Set higher to show more digits.</summary>
    [Parameter] public int Max { get; set; } = 99;

    /// <summary>Visual color theme. Default <see cref="BadgeColor.Danger"/>
    /// (red) matches the notification convention.</summary>
    [Parameter] public BadgeColor Color { get; set; } = BadgeColor.Danger;

    /// <summary>Corner attachment when not Inline. Default
    /// <see cref="BadgePosition.TopRight"/>.</summary>
    [Parameter] public BadgePosition Position { get; set; } = BadgePosition.TopRight;

    /// <summary>Inline next to text instead of positioned at a corner. When
    /// true, the badge is <c>position: static</c> and the parent doesn't need
    /// <c>position: relative</c>.</summary>
    [Parameter] public bool Inline { get; set; }

    /// <summary>Display the badge when <see cref="Count"/> is 0. By default
    /// (false), Count=0 hides the badge entirely — the typical "no unread"
    /// notification UX.</summary>
    [Parameter] public bool ShowZero { get; set; }

    /// <summary>Optional explicit accessibility label, e.g. "3 unread
    /// notifications". When null, a label is generated from Count + Dot.</summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>Append-only layout utility classes.</summary>
    [Parameter] public string? Class { get; set; }


    // ==========================================================================
    // DERIVED STATE (computed in OnParametersSet)
    // ==========================================================================

    private bool ShouldRender_Custom { get; set; }
    private string DisplayText { get; set; } = "";
    private string CssClasses { get; set; } = "";
    private string ResolvedAriaLabel { get; set; } = "";

    protected override void OnParametersSet()
    {
        // Visibility resolution. The badge renders when:
        //   1. Dot mode (no count needed), OR
        //   2. Count is set AND (Count > 0 OR ShowZero is true)
        // Otherwise nothing renders — empty markup, not even an empty span.
        if (Dot)
        {
            ShouldRender_Custom = true;
        }
        else if (Count.HasValue)
        {
            ShouldRender_Custom = Count.Value > 0 || ShowZero;
        }
        else
        {
            // No Count and not Dot — nothing to render. Consumer probably
            // forgot to set either parameter; we render nothing rather than
            // showing a bare positioned span.
            ShouldRender_Custom = false;
        }

        if (!ShouldRender_Custom)
        {
            DisplayText = "";
            CssClasses = "";
            ResolvedAriaLabel = "";
            return;
        }

        // Display text: clamp to "N+" when Count exceeds Max.
        if (Dot)
        {
            DisplayText = "";
        }
        else
        {
            var c = Count ?? 0;
            DisplayText = c > Max
                ? $"{Max.ToString(CultureInfo.InvariantCulture)}+"
                : c.ToString(CultureInfo.InvariantCulture);
        }

        // CSS class composition.
        var classes = new List<string> { "lipi-badge" };
        if (Dot)
        {
            classes.Add("lipi-badge-dot");
        }
        classes.Add(ColorClass(Color));
        if (Inline)
        {
            classes.Add("lipi-badge-inline");
        }
        else
        {
            classes.Add(PositionClass(Position));
        }
        if (!string.IsNullOrWhiteSpace(Class)) classes.Add(Class);
        CssClasses = string.Join(" ", classes);

        // Accessibility label. Caller's explicit AriaLabel wins. Otherwise
        // generate from Count/Dot — provides context for screen readers since
        // the badge text alone ("3") is ambiguous.
        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            ResolvedAriaLabel = AriaLabel!;
        }
        else if (Dot)
        {
            ResolvedAriaLabel = "Indicator";
        }
        else
        {
            var c = Count ?? 0;
            ResolvedAriaLabel = c == 1 ? "1 item" : $"{c} items";
        }
    }


    // ==========================================================================
    // CLASS HELPERS
    // ==========================================================================

    private static string ColorClass(BadgeColor c) => c switch
    {
        BadgeColor.Warning => "lipi-badge-warning",
        BadgeColor.Success => "lipi-badge-success",
        BadgeColor.Info    => "lipi-badge-info",
        BadgeColor.Neutral => "lipi-badge-neutral",
        BadgeColor.Primary => "lipi-badge-primary",
        _                  => "lipi-badge-danger"
    };

    private static string PositionClass(BadgePosition p) => p switch
    {
        BadgePosition.TopLeft     => "lipi-badge-top-left",
        BadgePosition.BottomRight => "lipi-badge-bottom-right",
        BadgePosition.BottomLeft  => "lipi-badge-bottom-left",
        _                         => "lipi-badge-top-right"
    };
}
