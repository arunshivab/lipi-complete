// SPEC:     docs/00-COMPONENTS/2.7/05-LipiToast-Spec.md §3, §4
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// Type definitions for the LipiToast family. Per the handout, all Toast types
// live in this single file: enums (ToastSeverity, ToastPosition), data classes
// (ToastOptions, ToastAction, ToastDescriptor, ToastPromiseOptions), and the
// internal ToastEntry used by the service + host.
//
// Pattern matches sibling Phase 2.7 *Types.cs files. The internal ToastEntry
// is the only public-but-internally-scoped type (we don't expose it on the
// service interface but the host needs to consume it directly via service
// state events).

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Severity of a toast. Drives the icon, color bar, default duration, and
/// ARIA live region politeness level. Error toasts are persistent by default
/// (clinical safety — see spec §7).
/// </summary>
public enum ToastSeverity
{
    /// <summary>Green — successful actions. Default 3000ms auto-dismiss.</summary>
    Success,

    /// <summary>Blue — informational. Default 5000ms.</summary>
    Info,

    /// <summary>Amber — warnings. Default 7000ms.</summary>
    Warning,

    /// <summary>Red — errors. PERSISTENT BY DEFAULT (0ms = no auto-dismiss).
    /// Clinical safety decision: errors must be acknowledged.</summary>
    Error
}

/// <summary>
/// Screen corner where the toast host renders. Configured per-clinic via
/// Settings. Default <see cref="TopRight"/>.
/// </summary>
public enum ToastPosition
{
    /// <summary>Top-right corner (default).</summary>
    TopRight,

    /// <summary>Top-center — under the TopNav.</summary>
    TopCenter,

    /// <summary>Top-left corner.</summary>
    TopLeft,

    /// <summary>Bottom-right corner — above BottomNav.</summary>
    BottomRight,

    /// <summary>Bottom-center — above BottomNav.</summary>
    BottomCenter,

    /// <summary>Bottom-left corner — above BottomNav.</summary>
    BottomLeft
}

/// <summary>
/// Caller-supplied options for an individual toast. All optional — defaults
/// per severity apply when fields are null/false.
/// </summary>
public class ToastOptions
{
    /// <summary>Optional title — bold first line above the message.</summary>
    public string? Title { get; set; }

    /// <summary>Override the per-severity default duration (ms). Set to 0 or
    /// <see cref="Persistent"/>=true for no auto-dismiss.</summary>
    public int? DurationMs { get; set; }

    /// <summary>Optional single action button (e.g., "Undo", "Retry").
    /// Max 12 chars in the label per spec §11.</summary>
    public ToastAction? Action { get; set; }

    /// <summary>When true, never auto-dismisses regardless of DurationMs.
    /// Always true for <see cref="ToastSeverity.Error"/> by default.</summary>
    public bool Persistent { get; set; }

    /// <summary>Dedup key. When supplied, a subsequent toast with the same Id
    /// updates the existing toast in place instead of stacking a new one.
    /// Required for Promise-style API.</summary>
    public string? Id { get; set; }

    /// <summary>Override the clinic-default position for this single toast.
    /// Use sparingly — consistency helps users.</summary>
    public ToastPosition? Position { get; set; }
}

/// <summary>
/// A single action button on a toast. Limited to one action per toast (spec §11)
/// — toasts aren't dialogs. Label is capped to 12 characters by the host.
/// </summary>
public class ToastAction
{
    /// <summary>Button label. Max 12 chars (longer → ellipsis at render time).</summary>
    public string Label { get; set; } = "";

    /// <summary>Async callback when the button is clicked. The toast dismisses
    /// after this completes (or immediately if you await nothing).</summary>
    public Func<Task>? OnClick { get; set; }
}

/// <summary>
/// Full descriptor for <see cref="ILipiToastService.ShowAsync"/> — bundles
/// message + severity + options. Used when the shortcut methods (Success(),
/// Error(), etc.) don't fit.
/// </summary>
public class ToastDescriptor
{
    /// <summary>Toast body message. Required.</summary>
    public string Message { get; set; } = "";

    /// <summary>Severity — drives icon, color, default duration.</summary>
    public ToastSeverity Severity { get; set; } = ToastSeverity.Info;

    /// <summary>Caller options.</summary>
    public ToastOptions Options { get; set; } = new();
}

/// <summary>
/// Options for the Promise-style toast lifecycle. The toast morphs in place
/// from a spinning Loading state to either Success or Error depending on
/// whether the wrapped Task completes or throws.
/// </summary>
public class ToastPromiseOptions
{
    /// <summary>Message shown during the loading phase.</summary>
    public string LoadingMessage { get; set; } = "Loading...";

    /// <summary>Message shown after successful completion.</summary>
    public string SuccessMessage { get; set; } = "Done.";

    /// <summary>Builds the error message from the caught exception. Default
    /// returns <c>e.Message</c>; consumers typically wrap with context
    /// (e.g., <c>e => $"Save failed: {e.Message}"</c>).</summary>
    public Func<Exception, string> ErrorMessage { get; set; } = e => e.Message;

    /// <summary>Optional toast Id. When supplied, dedup across calls; auto-
    /// generated when null. Required internally — the morph mechanic depends
    /// on the loading/success/error toasts sharing one Id.</summary>
    public string? Id { get; set; }
}

/// <summary>
/// Internal toast record managed by the service + host. Consumers should not
/// construct these directly — use the service methods. Public visibility is
/// for the host's foreach binding to work without internal-visibility tricks.
/// </summary>
public class ToastEntry
{
    /// <summary>Stable Id — used for dedup, dismiss, morph operations.</summary>
    public string Id { get; set; } = "";

    /// <summary>Current message text. Mutable for Promise-style morph.</summary>
    public string Message { get; set; } = "";

    /// <summary>Current severity. Mutable for Promise-style morph
    /// (Loading → Success/Error swap).</summary>
    public ToastSeverity Severity { get; set; }

    /// <summary>Options passed at creation (or last morph).</summary>
    public ToastOptions Options { get; set; } = new();

    /// <summary>Toast position resolved at the time of creation.</summary>
    public ToastPosition Position { get; set; }

    /// <summary>When true, the toast renders a spinner instead of a
    /// severity icon. Used for the Promise-style Loading phase.</summary>
    public bool IsLoading { get; set; }

    /// <summary>UTC timestamp of creation — used for ordering when stacking.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Auto-dismiss CancellationTokenSource. Cancelled on manual
    /// dismiss, morph, or component disposal.</summary>
    public CancellationTokenSource? AutoDismissCts { get; set; }
}
