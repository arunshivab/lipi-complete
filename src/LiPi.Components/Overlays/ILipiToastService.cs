// SPEC:     docs/00-COMPONENTS/2.7/05-LipiToast-Spec.md §3
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// Service interface for LipiToast. Registered as Scoped per Blazor Server
// circuit — each user session has its own queue + state.
//
// The host (LipiToastHost) subscribes to OnChanged and re-renders when the
// service mutates state. The service does not touch the renderer directly.

using LiPi.Components.Overlays;

namespace LiPi.Components.Overlays;

/// <summary>
/// Toast dispatch service. Consumers inject this and call the convenience
/// methods (<c>Success</c>, <c>Error</c>, etc.) or the full
/// <see cref="ShowAsync"/> / <see cref="PromiseAsync"/> variants.
/// </summary>
public interface ILipiToastService
{
    /// <summary>Event fired whenever the active toast list changes. The host
    /// subscribes to this to trigger re-renders. Consumer apps generally
    /// don't subscribe directly.</summary>
    event Action? OnChanged;

    /// <summary>Active toast entries, ordered by creation time (oldest first).
    /// The host renders these; consumers don't usually read this directly.</summary>
    IReadOnlyList<ToastEntry> Active { get; }

    /// <summary>Maximum visible toasts at one time. Beyond this, additional
    /// calls queue silently and surface when an active toast dismisses.
    /// Default 4 (spec §6); configurable per clinic.</summary>
    int MaxVisible { get; set; }

    /// <summary>Default position when not overridden per toast.</summary>
    ToastPosition DefaultPosition { get; set; }

    // ─────────── Shortcut methods (severity + message + optional options) ─────────

    /// <summary>Show a Success toast. Auto-dismisses after 3000ms by default.</summary>
    Task Success(string message, ToastOptions? options = null);

    /// <summary>Show an Error toast. PERSISTENT BY DEFAULT — clinical safety
    /// (spec §7). User must dismiss manually.</summary>
    Task Error(string message, ToastOptions? options = null);

    /// <summary>Show a Warning toast. Auto-dismisses after 7000ms by default.</summary>
    Task Warning(string message, ToastOptions? options = null);

    /// <summary>Show an Info toast. Auto-dismisses after 5000ms by default.</summary>
    Task Info(string message, ToastOptions? options = null);

    // ─────────── Full-control method ──────────────────────────────────────────────

    /// <summary>Show a toast with full descriptor control. Use when the
    /// shortcut methods don't fit (e.g., dynamic severity).</summary>
    Task ShowAsync(ToastDescriptor toast);

    // ─────────── Promise-style ────────────────────────────────────────────────────

    /// <summary>Wrap an async operation in a morphing toast — Loading state
    /// while pending, Success or Error on completion. Returns the awaited
    /// result (or rethrows the original exception). Same Id throughout the
    /// lifecycle.</summary>
    Task<TResult> PromiseAsync<TResult>(
        Task<TResult> operation,
        ToastPromiseOptions options);

    /// <summary>Promise-style for non-generic tasks (no return value).</summary>
    Task PromiseAsync(
        Task operation,
        ToastPromiseOptions options);

    // ─────────── Imperative dismiss ───────────────────────────────────────────────

    /// <summary>Dismiss a toast by its Id. Cancels any pending auto-dismiss
    /// timer and removes the entry from the active list.</summary>
    Task DismissAsync(string toastId);

    /// <summary>Dismiss all active toasts at once.</summary>
    Task DismissAllAsync();
}
