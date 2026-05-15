// SPEC:     docs/00-COMPONENTS/2.7/02-LipiBadge-Pill-Spec.md §3
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiPill partial class — parameters, class composition, dismiss handler.
// Pure-render with one event (OnDismiss). No JS interop, no lifecycle hooks
// beyond OnParametersSet.

using Microsoft.AspNetCore.Components;

namespace LiPi.Web.Components.Shared;

public partial class LipiPill
{
    // ==========================================================================
    // PARAMETERS (spec §3 — table of params)
    // ==========================================================================

    /// <summary>Pill text/content. Required — the pill makes no sense empty.</summary>
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }

    /// <summary>Visual intent (color theme). Default <see cref="PillIntent.Default"/>.</summary>
    [Parameter] public PillIntent Intent { get; set; } = PillIntent.Default;

    /// <summary>Visual variant. Default <see cref="PillVariant.Filled"/>.</summary>
    [Parameter] public PillVariant Variant { get; set; } = PillVariant.Filled;

    /// <summary>Size variant. Default <see cref="PillSize.Medium"/>.</summary>
    [Parameter] public PillSize Size { get; set; } = PillSize.Medium;

    /// <summary>Optional Lucide icon name to render on the left.</summary>
    [Parameter] public string? Icon { get; set; }

    /// <summary>When true, renders a × button on the right that fires
    /// <see cref="OnDismiss"/> when clicked. Used for filter chips.</summary>
    [Parameter] public bool Dismissible { get; set; }

    /// <summary>Fired when the user clicks the × button. Consumer is
    /// responsible for hiding the pill (Blazor doesn't auto-hide).</summary>
    [Parameter] public EventCallback OnDismiss { get; set; }

    /// <summary>Append-only layout utility classes.</summary>
    [Parameter] public string? Class { get; set; }


    // ==========================================================================
    // DERIVED STATE (computed in OnParametersSet)
    // ==========================================================================

    private string CssClasses { get; set; } = "";
    private string AriaRole { get; set; } = "";
    private int IconPx { get; set; } = 12;
    private int DismissIconPx { get; set; } = 12;
    private string DismissAriaLabel { get; set; } = "Remove";

    protected override void OnParametersSet()
    {
        // Class composition: base + variant + intent + size + caller's Class.
        var classes = new List<string>
        {
            "lipi-pill",
            VariantClass(Variant),
            IntentClass(Intent),
            SizeClass(Size)
        };
        if (Dismissible) classes.Add("lipi-pill-dismissible");
        if (!string.IsNullOrWhiteSpace(Class)) classes.Add(Class);
        CssClasses = string.Join(" ", classes);

        // ARIA role — "status" for dynamic state pills (e.g., "Online"); empty
        // role for static labels (filter chips). We can't reliably distinguish
        // without explicit signal, so default to "status" which is correct for
        // the common case and degrades safely (status is a non-interrupting
        // landmark — extra announcement only).
        AriaRole = "status";

        // Icon and dismiss icon size scale with pill size.
        IconPx = Size switch
        {
            PillSize.Small => 10,
            PillSize.Large => 14,
            _              => 12
        };
        DismissIconPx = IconPx;

        // Dismiss aria-label includes pill text if extractable, otherwise generic.
        // We can't read ChildContent text directly from C# (RenderFragment is opaque),
        // so use a generic label. Consumers can override via parent aria-label if needed.
        DismissAriaLabel = "Remove";
    }


    // ==========================================================================
    // EVENT HANDLERS
    // ==========================================================================

    private async Task OnDismissClickInternal()
    {
        if (OnDismiss.HasDelegate)
        {
            await OnDismiss.InvokeAsync();
        }
    }


    // ==========================================================================
    // CLASS HELPERS
    // ==========================================================================

    private static string VariantClass(PillVariant v) => v switch
    {
        PillVariant.Outlined => "lipi-pill-outlined",
        PillVariant.Subtle   => "lipi-pill-subtle",
        _                    => "lipi-pill-filled"
    };

    private static string IntentClass(PillIntent i) => i switch
    {
        PillIntent.Primary => "lipi-pill-primary",
        PillIntent.Success => "lipi-pill-success",
        PillIntent.Warning => "lipi-pill-warning",
        PillIntent.Danger  => "lipi-pill-danger",
        PillIntent.Info    => "lipi-pill-info",
        PillIntent.Neutral => "lipi-pill-neutral",
        _                  => "lipi-pill-default"
    };

    private static string SizeClass(PillSize s) => s switch
    {
        PillSize.Small => "lipi-pill-sm",
        PillSize.Large => "lipi-pill-lg",
        _              => "lipi-pill-md"
    };
}
