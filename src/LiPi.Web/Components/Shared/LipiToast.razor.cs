// SPEC:     docs/00-COMPONENTS/2.7/05-LipiToast-Spec.md §5, §10
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiToast partial class. Renders a single ToastEntry. Doesn't subscribe to
// the service directly — the host injects state via the Entry parameter and
// re-renders the toast list when the service fires OnChanged.
//
// Click handlers delegate back to the host (OnDismissRequested, OnActionInvoked)
// because dismiss/action affects service state, which only the host owns.

using LiPi.Web.Services;
using Microsoft.AspNetCore.Components;

namespace LiPi.Web.Components.Shared;

public partial class LipiToast
{
    // ==========================================================================
    // PARAMETERS
    // ==========================================================================

    /// <summary>The toast entry to render. Supplied by LipiToastHost from the
    /// service's Active list.</summary>
    [Parameter, EditorRequired] public ToastEntry Entry { get; set; } = default!;

    /// <summary>Callback when the user clicks the ✕ button. The host invokes
    /// ILipiToastService.DismissAsync.</summary>
    [Parameter] public EventCallback<string> OnDismissRequested { get; set; }

    /// <summary>Callback when the user clicks the action button. The host
    /// invokes the action delegate and then dismisses.</summary>
    [Parameter] public EventCallback<ToastEntry> OnActionInvoked { get; set; }


    // ==========================================================================
    // DERIVED STATE
    // ==========================================================================

    private string CssClasses =>
        string.Join(" ",
            "lipi-toast",
            SeverityClass(Entry.Severity),
            Entry.IsLoading ? "lipi-toast-loading" : "");

    private string SeverityIcon => Entry.Severity switch
    {
        ToastSeverity.Success => "check",
        ToastSeverity.Warning => "alert-triangle",
        ToastSeverity.Error   => "x",
        _                     => "info"          // Info default
    };

    /// <summary>ARIA role per spec §10:
    ///   Error → "alert" (assertive — interrupts screen reader)
    ///   Others → "status" (polite — waits for a pause)</summary>
    private string AriaRole =>
        Entry.Severity == ToastSeverity.Error ? "alert" : "status";

    private string AriaLive =>
        Entry.Severity == ToastSeverity.Error ? "assertive" : "polite";

    /// <summary>Action button label capped at 12 chars per spec §11. Longer
    /// labels get a trailing ellipsis. We could throw in Development but the
    /// truncation degrades gracefully and is easy to spot visually.</summary>
    private string TruncatedActionLabel
    {
        get
        {
            var label = Entry.Options.Action?.Label ?? "";
            if (label.Length <= 12) return label;
            return label.Substring(0, 11) + "…";
        }
    }


    // ==========================================================================
    // EVENT HANDLERS
    // ==========================================================================

    private async Task HandleDismiss()
    {
        if (OnDismissRequested.HasDelegate)
        {
            await OnDismissRequested.InvokeAsync(Entry.Id);
        }
    }

    private async Task HandleAction()
    {
        if (OnActionInvoked.HasDelegate)
        {
            await OnActionInvoked.InvokeAsync(Entry);
        }
    }


    // ==========================================================================
    // CLASS HELPERS
    // ==========================================================================

    private static string SeverityClass(ToastSeverity s) => s switch
    {
        ToastSeverity.Success => "lipi-toast-success",
        ToastSeverity.Warning => "lipi-toast-warning",
        ToastSeverity.Error   => "lipi-toast-error",
        _                     => "lipi-toast-info"
    };
}
