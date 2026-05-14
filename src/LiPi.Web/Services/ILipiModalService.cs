// SPEC:  docs/01-LipiModal-Spec.md §5
// PHASE: 2.6.2 — Overlay Surfaces

using Microsoft.AspNetCore.Components;
using LiPi.Web.Components.Shared;

namespace LiPi.Web.Services;

/// <summary>
/// Programmatic API for opening modals from services or code-behind.
/// All methods are async — they resolve when the modal is dismissed.
/// Registered as Scoped (per Blazor circuit).
/// </summary>
public interface ILipiModalService
{
    // ── Confirmation dialog ─────────────────────────────────────────────────
    /// <summary>Opens a yes/no confirmation modal. Returns true if confirmed.</summary>
    Task<bool> ConfirmAsync(
        string title,
        string message,
        ConfirmIntent intent       = ConfirmIntent.Default,
        string?       primaryLabel = null,   // intent-specific default: "Delete", "Confirm"
        string?       cancelLabel  = null);  // default: "Cancel"

    // ── Alert dialog ────────────────────────────────────────────────────────
    /// <summary>Opens an acknowledgment modal. Returns when user clicks OK.</summary>
    Task AlertAsync(
        string title,
        string message,
        AlertIntent intent   = AlertIntent.Info,
        string?     okLabel  = null);   // default: "OK"

    // ── Prompt dialog ───────────────────────────────────────────────────────
    /// <summary>Opens a single-input prompt modal. Returns the entered string or
    /// null if cancelled.</summary>
    Task<string?> PromptAsync(
        string  title,
        string  label,
        string? defaultValue = null,
        string? placeholder  = null,
        bool    required     = true);

    // ── Custom component ────────────────────────────────────────────────────
    /// <summary>Opens any <see cref="ComponentBase"/> as a modal.
    /// The component must expose <c>[Parameter] EventCallback&lt;TResult&gt; OnResult</c>.</summary>
    Task<TResult?> ShowAsync<TComponent, TResult>(
        Dictionary<string, object?>? parameters = null,
        ModalSize                    size        = ModalSize.Standard,
        string?                      title       = null)
        where TComponent : ComponentBase;

    // ── Control ─────────────────────────────────────────────────────────────
    /// <summary>Programmatically close the topmost modal.</summary>
    Task CloseTopAsync(object? result = null);

    /// <summary>Current number of open modals (0–3).</summary>
    int StackDepth { get; }

    /// <summary>Fired whenever the modal stack changes — LipiOverlayHost subscribes.</summary>
    event Action OnStateChanged;
}

// ── Internal modal request record ─────────────────────────────────────────────
// Carries everything LipiOverlayHost needs to render a modal entry.
public sealed class ModalRequest
{
    public required string                       Title       { get; init; }
    public string?                               Subtitle    { get; init; }
    public ModalSize                             Size        { get; init; } = ModalSize.Standard;
    public ModalAnimation                        Animation   { get; init; } = ModalAnimation.FadeSlide;
    public bool                                  ShowClose   { get; init; } = true;
    public bool                                  CloseOnEsc  { get; init; } = true;
    public bool                                  CloseOnBack { get; init; } = true;
    public required Type                         ComponentType { get; init; }
    public Dictionary<string, object?>?          Parameters  { get; init; }
    public required TaskCompletionSource<object?> Tcs        { get; init; }
    public int                                   ZIndex      { get; set; } = 800;
}
