// SPEC:     docs/00-COMPONENTS/2.7/05-LipiToast-Spec.md §6
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiToastHost partial class. Owns:
//   1. Subscription to ILipiToastService.OnChanged
//   2. Marshalling state changes onto the renderer via InvokeAsync
//   3. Grouping the flat Active list by Position for corner-stack rendering
//   4. Forwarding dismiss + action callbacks to the service
//
// Subscription is set up in OnInitialized and torn down in Dispose. The
// service is Scoped per circuit, so its lifetime tracks the host — no
// risk of stale event handlers across circuit recycles.

using LiPi.Components.Overlays;
using Microsoft.AspNetCore.Components;

namespace LiPi.Components.Overlays;

public partial class LipiToastHost
{
    // ==========================================================================
    // LIFECYCLE
    // ==========================================================================

    protected override void OnInitialized()
    {
        ToastService.OnChanged += HandleServiceChanged;
    }

    public void Dispose()
    {
        ToastService.OnChanged -= HandleServiceChanged;
    }

    /// <summary>
    /// Service change handler. OnChanged fires from the service's mutation
    /// paths (potentially from background threads via Task.Delay continuations
    /// — auto-dismiss callbacks). Marshal to the renderer.
    /// </summary>
    private void HandleServiceChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }


    // ==========================================================================
    // GROUPED RENDER STATE
    // ==========================================================================

    /// <summary>
    /// Group active toasts by position so each corner stack renders its own
    /// subset. We preserve creation order within each group — the FIFO order
    /// matches the visual "oldest at top, newest below" stacking (for top
    /// positions) and the column-reverse flips it for bottom positions
    /// (newest at bottom-most).
    /// </summary>
    private Dictionary<ToastPosition, List<ToastEntry>> GroupedByPosition
    {
        get
        {
            var snapshot = ToastService.Active;
            var groups = new Dictionary<ToastPosition, List<ToastEntry>>();
            foreach (var entry in snapshot)
            {
                if (!groups.TryGetValue(entry.Position, out var list))
                {
                    list = new List<ToastEntry>();
                    groups[entry.Position] = list;
                }
                list.Add(entry);
            }
            return groups;
        }
    }


    // ==========================================================================
    // EVENT HANDLERS
    // ==========================================================================

    private async Task HandleDismiss(string toastId)
    {
        await ToastService.DismissAsync(toastId);
    }

    private async Task HandleAction(ToastEntry entry)
    {
        // Run the action delegate first (consumer-supplied — may throw or be slow).
        // Always dismiss afterward, even if the action throws — the user has
        // already engaged with the toast and the toast should not stick around
        // forever showing a stale "Undo" button after the undo path errored.
        try
        {
            if (entry.Options.Action?.OnClick is not null)
            {
                await entry.Options.Action.OnClick.Invoke();
            }
        }
        finally
        {
            await ToastService.DismissAsync(entry.Id);
        }
    }


    // ==========================================================================
    // POSITION CLASS HELPER
    // ==========================================================================

    private static string PositionClass(ToastPosition p) => p switch
    {
        ToastPosition.TopRight     => "lipi-toast-host-top-right",
        ToastPosition.TopCenter    => "lipi-toast-host-top-center",
        ToastPosition.TopLeft      => "lipi-toast-host-top-left",
        ToastPosition.BottomRight  => "lipi-toast-host-bottom-right",
        ToastPosition.BottomCenter => "lipi-toast-host-bottom-center",
        ToastPosition.BottomLeft   => "lipi-toast-host-bottom-left",
        _                          => "lipi-toast-host-top-right"
    };
}
