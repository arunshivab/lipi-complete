// SPEC:  docs/01-LipiModal-Spec.md §5, §6
// PHASE: 2.6.2 — Overlay Surfaces
// AMEND: docs/CHANGE-LOG.md A32 — stack guard off-by-one fix (spec §6).
//        Previous code allowed max stack depth 4 (warned at 3, threw at 4 — but
//        the warn pre-push check fired when count was already 3, so push of
//        the 4th succeeded with a warning; throw only fired on the 5th push).
//        Spec §6 specifies max 3 — warn approaching 3, throw at 3.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using LiPi.Web.Components.Shared;

namespace LiPi.Web.Services;

/// <summary>
/// <see cref="ILipiModalService"/> implementation.
/// Maintains a modal stack (max 3) and notifies <see cref="LipiOverlayHost"/>
/// via <see cref="OnStateChanged"/> whenever the stack changes.
/// All state mutations are thread-safe via lock.
/// </summary>
public sealed class LipiModalService : ILipiModalService
{
    private const int MaxStackDepth = 3;

    private readonly ILogger<LipiModalService> _log;
    private readonly List<ModalRequest> _stack = new();
    private readonly object _lock = new();

    public LipiModalService(ILogger<LipiModalService> log) => _log = log;

    public event Action? OnStateChanged;

    public int StackDepth
    {
        get { lock (_lock) return _stack.Count; }
    }

    // ── Core push / pop ─────────────────────────────────────────────────────

    private async Task<object?> PushAsync(ModalRequest request)
    {
        lock (_lock)
        {
            // A32 fix: warn pre-push when about to reach the max (count == 2 →
            // pushing makes it 3). Throw pre-push when ALREADY at max (count == 3 →
            // pushing would make it 4, which violates spec §6 cap of 3).
            if (_stack.Count == MaxStackDepth - 1)
            {
                _log.LogWarning(
                    "[LipiModal] Stack depth reached {Depth} — consider redesigning flow",
                    MaxStackDepth);
            }
            if (_stack.Count >= MaxStackDepth)
            {
                throw new InvalidOperationException(
                    $"[LipiModal] Maximum modal stack depth ({MaxStackDepth}) exceeded. " +
                    "Close existing modals before opening new ones.");
            }

            request.ZIndex = 800 + _stack.Count * 10;
            _stack.Add(request);
        }

        OnStateChanged?.Invoke();

        try
        {
            return await request.Tcs.Task;
        }
        finally
        {
            lock (_lock) _stack.Remove(request);
            OnStateChanged?.Invoke();
        }
    }

    internal IReadOnlyList<ModalRequest> GetStack()
    {
        lock (_lock) return _stack.ToList();
    }

    // ── ConfirmAsync ────────────────────────────────────────────────────────

    public async Task<bool> ConfirmAsync(
        string title,
        string message,
        ConfirmIntent intent       = ConfirmIntent.Default,
        string?       primaryLabel = null,
        string?       cancelLabel  = null)
    {
        var tcs = new TaskCompletionSource<object?>();
        var req = new ModalRequest
        {
            Title         = title,
            Size          = ModalSize.Compact,
            Animation     = ModalAnimation.FadeSlide,
            ShowClose     = intent != ConfirmIntent.Critical,
            CloseOnEsc    = intent != ConfirmIntent.Critical,
            CloseOnBack   = intent != ConfirmIntent.Critical,
            ComponentType = typeof(LiPi.Web.Components.Shared.ConfirmDialog),
            Parameters    = new Dictionary<string, object?>
            {
                ["Message"]      = message,
                ["Intent"]       = intent,
                ["PrimaryLabel"] = primaryLabel,
                ["CancelLabel"]  = cancelLabel,
                ["Tcs"]          = tcs
            },
            Tcs           = tcs
        };

        var result = await PushAsync(req);
        return result is bool b && b;
    }

    // ── AlertAsync ──────────────────────────────────────────────────────────

    public async Task AlertAsync(
        string title,
        string message,
        AlertIntent intent  = AlertIntent.Info,
        string?     okLabel = null)
    {
        var tcs = new TaskCompletionSource<object?>();
        var req = new ModalRequest
        {
            Title         = title,
            Size          = ModalSize.Compact,
            Animation     = ModalAnimation.FadeSlide,
            ShowClose     = intent != AlertIntent.Critical,
            CloseOnEsc    = intent != AlertIntent.Critical,
            CloseOnBack   = false,
            ComponentType = typeof(LiPi.Web.Components.Shared.AlertDialog),
            Parameters    = new Dictionary<string, object?>
            {
                ["Message"] = message,
                ["Intent"]  = intent,
                ["OkLabel"] = okLabel,
                ["Tcs"]     = tcs
            },
            Tcs           = tcs
        };

        await PushAsync(req);
    }

    // ── PromptAsync ─────────────────────────────────────────────────────────

    public async Task<string?> PromptAsync(
        string  title,
        string  label,
        string? defaultValue = null,
        string? placeholder  = null,
        bool    required     = true)
    {
        var tcs = new TaskCompletionSource<object?>();
        var req = new ModalRequest
        {
            Title         = title,
            Size          = ModalSize.Compact,
            Animation     = ModalAnimation.FadeSlide,
            ShowClose     = true,
            CloseOnEsc    = true,
            CloseOnBack   = false,
            ComponentType = typeof(LiPi.Web.Components.Shared.PromptDialog),
            Parameters    = new Dictionary<string, object?>
            {
                ["Label"]        = label,
                ["DefaultValue"] = defaultValue,
                ["Placeholder"]  = placeholder,
                ["Required"]     = required,
                ["Tcs"]          = tcs
            },
            Tcs           = tcs
        };

        var result = await PushAsync(req);
        return result as string;
    }

    // ── ShowAsync ───────────────────────────────────────────────────────────

    public async Task<TResult?> ShowAsync<TComponent, TResult>(
        Dictionary<string, object?>? parameters = null,
        ModalSize                    size        = ModalSize.Standard,
        string?                      title       = null)
        where TComponent : ComponentBase
    {
        var tcs = new TaskCompletionSource<object?>();

        // Inject the result callback into the component parameters
        var allParams = parameters ?? new Dictionary<string, object?>();
        allParams["OnResult"] = Microsoft.AspNetCore.Components.EventCallback.Factory.Create<TResult>(
            this, (TResult result) => tcs.TrySetResult(result));

        var req = new ModalRequest
        {
            Title         = title ?? string.Empty,
            Size          = size,
            Animation     = ModalAnimation.FadeSlide,
            ShowClose     = true,
            CloseOnEsc    = true,
            CloseOnBack   = true,
            ComponentType = typeof(TComponent),
            Parameters    = allParams,
            Tcs           = tcs
        };

        var result = await PushAsync(req);
        if (result is TResult typed) return typed;
        return default;
    }

    // ── CloseTopAsync ───────────────────────────────────────────────────────

    public Task CloseTopAsync(object? result = null)
    {
        ModalRequest? top;
        lock (_lock)
        {
            top = _stack.Count > 0 ? _stack[^1] : null;
        }
        top?.Tcs.TrySetResult(result);
        return Task.CompletedTask;
    }
}
