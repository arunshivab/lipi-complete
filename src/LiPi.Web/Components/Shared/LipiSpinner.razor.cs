// SPEC:     docs/00-COMPONENTS/2.7/01-LipiSpinner-Spec.md §3 (parameters), §5 (CSS classes), §6 (a11y)
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiSpinner partial class — parameters + derived state + class composition.
// Pure-render component (no events, no lifecycle hooks needed beyond
// OnParametersSet for class memoization). Single-render-path; no JS interop.

using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace LiPi.Web.Components.Shared;

public partial class LipiSpinner
{
    // ==========================================================================
    // PARAMETERS (spec §3)
    // ==========================================================================

    /// <summary>Size variant. Default <see cref="SpinnerSize.Medium"/> (24px).</summary>
    [Parameter] public SpinnerSize Size { get; set; } = SpinnerSize.Medium;

    /// <summary>Override size in pixels — wins over <see cref="Size"/> when set.
    /// Use for spinners that need to match a specific design token (e.g., icon
    /// size 18px to align with a 16-px text row).</summary>
    [Parameter] public int? SizePx { get; set; }

    /// <summary>Color intent. Default <see cref="SpinnerIntent.Default"/>
    /// (currentColor — inherits parent text color).</summary>
    [Parameter] public SpinnerIntent Intent { get; set; } = SpinnerIntent.Default;

    /// <summary>Custom CSS color string — wins over <see cref="Intent"/>. Accepts
    /// any valid CSS color value (hex, rgb, var(--token), etc.).</summary>
    [Parameter] public string? Color { get; set; }

    /// <summary>Optional text label. When present, the spinner is wrapped in a
    /// flex container alongside the label. Default null (icon only).</summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>Position of the label relative to the spinner. Reuses the
    /// Phase 2.5.5 cross-family <see cref="InputLabelPosition"/> enum. Spinner
    /// supports all 4 directions (Top/Right/Bottom/Left); Right is the default.
    /// </summary>
    [Parameter] public InputLabelPosition LabelPosition { get; set; } = InputLabelPosition.Right;

    /// <summary>Append-only layout utility classes (margin, padding, etc.).</summary>
    [Parameter] public string? Class { get; set; }


    // ==========================================================================
    // DERIVED STATE (computed in OnParametersSet; memoized for render)
    // ==========================================================================

    private string SvgCssClasses { get; set; } = "";
    private string WrapCssClasses { get; set; } = "";
    private string AriaLabel { get; set; } = "Loading";
    private bool HasLabel { get; set; }
    private bool LabelGoesBefore { get; set; }
    private string? InlineStyle { get; set; }

    protected override void OnParametersSet()
    {
        HasLabel = !string.IsNullOrWhiteSpace(Label);
        AriaLabel = HasLabel ? Label! : "Loading";

        // SVG element classes — size + intent + caller's Class (when no wrap).
        var svgClasses = new List<string> { "lipi-spinner", SizeClass(Size), IntentClass(Intent) };
        if (!HasLabel && !string.IsNullOrWhiteSpace(Class))
        {
            svgClasses.Add(Class);
        }
        SvgCssClasses = string.Join(" ", svgClasses);

        // Wrap element classes — flex direction varies with LabelPosition.
        // Caller's Class goes on the wrap when label present (outer element).
        if (HasLabel)
        {
            var wrapClasses = new List<string> { "lipi-spinner-wrap", WrapDirectionClass(LabelPosition) };
            if (!string.IsNullOrWhiteSpace(Class)) wrapClasses.Add(Class);
            WrapCssClasses = string.Join(" ", wrapClasses);
        }
        else
        {
            WrapCssClasses = "";
        }

        // Label-before-svg for Top and Left positions; after for Bottom and Right.
        LabelGoesBefore = LabelPosition is InputLabelPosition.Top or InputLabelPosition.Left;

        // Inline style — only emitted when SizePx override or Color override active.
        var styleParts = new List<string>();
        if (SizePx.HasValue)
        {
            var px = SizePx.Value.ToString(CultureInfo.InvariantCulture);
            styleParts.Add($"width:{px}px");
            styleParts.Add($"height:{px}px");
        }
        if (!string.IsNullOrWhiteSpace(Color))
        {
            styleParts.Add($"color:{Color}");
        }
        InlineStyle = styleParts.Count > 0 ? string.Join(";", styleParts) : null;
    }


    // ==========================================================================
    // CLASS HELPERS
    // ==========================================================================

    private static string SizeClass(SpinnerSize s) => s switch
    {
        SpinnerSize.XSmall => "lipi-spinner-xs",
        SpinnerSize.Small  => "lipi-spinner-sm",
        SpinnerSize.Large  => "lipi-spinner-lg",
        _                  => "lipi-spinner-md"
    };

    private static string IntentClass(SpinnerIntent i) => i switch
    {
        SpinnerIntent.Primary => "lipi-spinner-primary",
        SpinnerIntent.Subtle  => "lipi-spinner-subtle",
        SpinnerIntent.Inverse => "lipi-spinner-inverse",
        _                     => "lipi-spinner-default"
    };

    private static string WrapDirectionClass(InputLabelPosition p) => p switch
    {
        InputLabelPosition.Top    => "lipi-spinner-wrap-top",
        InputLabelPosition.Bottom => "lipi-spinner-wrap-bottom",
        InputLabelPosition.Left   => "lipi-spinner-wrap-left",
        _                         => "lipi-spinner-wrap-right"
    };
}
