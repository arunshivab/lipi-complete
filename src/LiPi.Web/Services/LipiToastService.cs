// SPEC:     docs/00-COMPONENTS/2.7/05-LipiToast-Spec.md
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// LipiToast service implementation. Scoped per Blazor Server circuit.
//
// Responsibilities:
//   1. Queue + active-list management (max 4 visible, FIFO promotion from queue)
//   2. Dedup by Id — same Id updates existing toast in place
//   3. Auto-dismiss timers per toast (CancellationTokenSource per entry)
//   4. Promise-style morph (Loading → Success or Error, same Id)
//   5. Clinical safety: errors are persistent by default
//   6. Notify host via OnChanged event
//
// Thread-safety: a Blazor circuit is logically single-threaded, but
// auto-dismiss callbacks come from Task.Delay continuations on threadpool —
// we serialize mutations under a private lock and emit OnChanged outside it.

using System.Collections.Concurrent;
using LiPi.Web.Components.Shared;

namespace LiPi.Web.Services;

/// <inheritdoc cref="ILipiToastService"/>
public sealed class LipiToastService : ILipiToastService, IDisposable
{
    // ==========================================================================
    // STATE
    // ==========================================================================

    private readonly object _gate = new();
    private readonly List<ToastEntry> _active = new();
    private readonly Queue<ToastEntry> _waiting = new();

    /// <inheritdoc/>
    public event Action? OnChanged;

    /// <inheritdoc/>
    public IReadOnlyList<ToastEntry> Active
    {
        get
        {
            lock (_gate)
            {
                // Return a snapshot — the host enumerates this in markup; we
                // must not give it a live reference that mutates mid-render.
                return _active.ToList();
            }
        }
    }

    /// <inheritdoc/>
    public int MaxVisible { get; set; } = 4;

    /// <inheritdoc/>
    public ToastPosition DefaultPosition { get; set; } = ToastPosition.TopRight;


    // ==========================================================================
    // SHORTCUT METHODS
    // ==========================================================================

    public Task Success(string message, ToastOptions? options = null) =>
        ShowAsync(new ToastDescriptor
        {
            Message  = message,
            Severity = ToastSeverity.Success,
            Options  = options ?? new ToastOptions()
        });

    public Task Error(string message, ToastOptions? options = null) =>
        ShowAsync(new ToastDescriptor
        {
            Message  = message,
            Severity = ToastSeverity.Error,
            Options  = options ?? new ToastOptions()
        });

    public Task Warning(string message, ToastOptions? options = null) =>
        ShowAsync(new ToastDescriptor
        {
            Message  = message,
            Severity = ToastSeverity.Warning,
            Options  = options ?? new ToastOptions()
        });

    public Task Info(string message, ToastOptions? options = null) =>
        ShowAsync(new ToastDescriptor
        {
            Message  = message,
            Severity = ToastSeverity.Info,
            Options  = options ?? new ToastOptions()
        });


    // ==========================================================================
    // SHOWASYNC — core dispatch
    // ==========================================================================

    /// <inheritdoc/>
    public Task ShowAsync(ToastDescriptor toast)
    {
        ToastEntry entry;
        bool isErrorAndShouldBePersistent;

        lock (_gate)
        {
            // Dedup: same Id → update existing in-place. Spec §6.
            if (!string.IsNullOrWhiteSpace(toast.Options.Id))
            {
                var existing = _active.FirstOrDefault(e => e.Id == toast.Options.Id);
                if (existing != null)
                {
                    // Cancel the existing auto-dismiss timer if any — we may
                    // be morphing severity, which changes the duration.
                    existing.AutoDismissCts?.Cancel();
                    existing.AutoDismissCts?.Dispose();
                    existing.AutoDismissCts = null;

                    existing.Message   = toast.Message;
                    existing.Severity  = toast.Severity;
                    existing.Options   = toast.Options;
                    existing.IsLoading = false;

                    entry = existing;
                    isErrorAndShouldBePersistent = ResolvePersistent(entry);

                    // Re-arm dismiss timer for the new severity (if not persistent).
                    if (!isErrorAndShouldBePersistent)
                    {
                        ArmAutoDismiss(entry);
                    }

                    goto NotifyAndReturn;
                }
            }

            // New toast.
            entry = new ToastEntry
            {
                Id        = string.IsNullOrWhiteSpace(toast.Options.Id)
                                ? Guid.NewGuid().ToString("N")
                                : toast.Options.Id!,
                Message   = toast.Message,
                Severity  = toast.Severity,
                Options   = toast.Options,
                Position  = toast.Options.Position ?? DefaultPosition,
                IsLoading = false,
                CreatedAt = DateTime.UtcNow
            };

            isErrorAndShouldBePersistent = ResolvePersistent(entry);

            // Promote into active list if room, else queue.
            if (_active.Count < MaxVisible)
            {
                _active.Add(entry);
                if (!isErrorAndShouldBePersistent)
                {
                    ArmAutoDismiss(entry);
                }
            }
            else
            {
                _waiting.Enqueue(entry);
            }
        }

        NotifyAndReturn:
        EmitChanged();
        return Task.CompletedTask;
    }


    // ==========================================================================
    // PROMISE — morph in place
    // ==========================================================================

    /// <inheritdoc/>
    public async Task<TResult> PromiseAsync<TResult>(
        Task<TResult> operation,
        ToastPromiseOptions options)
    {
        var id = string.IsNullOrWhiteSpace(options.Id)
                    ? Guid.NewGuid().ToString("N")
                    : options.Id!;

        // 1. Loading phase: insert a persistent toast with IsLoading=true.
        lock (_gate)
        {
            var loading = new ToastEntry
            {
                Id        = id,
                Message   = options.LoadingMessage,
                Severity  = ToastSeverity.Info,            // visual placeholder; spinner overrides icon
                Options   = new ToastOptions { Id = id, Persistent = true },
                Position  = DefaultPosition,
                IsLoading = true,
                CreatedAt = DateTime.UtcNow
            };

            if (_active.Count < MaxVisible)
            {
                _active.Add(loading);
            }
            else
            {
                // Promote inline at the front of waiting — Promise loading
                // toasts should not get stuck behind transient stack overflow.
                // We accept that this nudges queue ordering slightly.
                _waiting.Enqueue(loading);
            }
        }
        EmitChanged();

        // 2. Await the operation.
        try
        {
            var result = await operation.ConfigureAwait(false);

            // 3a. Success path — morph in place via ShowAsync with same Id.
            await ShowAsync(new ToastDescriptor
            {
                Message  = options.SuccessMessage,
                Severity = ToastSeverity.Success,
                Options  = new ToastOptions { Id = id }
            });

            return result;
        }
        catch (Exception ex)
        {
            // 3b. Error path — morph to persistent error.
            await ShowAsync(new ToastDescriptor
            {
                Message  = options.ErrorMessage(ex),
                Severity = ToastSeverity.Error,
                Options  = new ToastOptions { Id = id }
            });
            throw;     // rethrow — caller still sees the exception
        }
    }

    /// <inheritdoc/>
    public async Task PromiseAsync(Task operation, ToastPromiseOptions options)
    {
        // Wrap the void-returning Task as a Task<bool> so we can reuse the
        // generic Promise. The bool result is discarded.
        await PromiseAsync(
            WrapAsync(operation),
            options).ConfigureAwait(false);
    }

    private static async Task<bool> WrapAsync(Task t)
    {
        await t.ConfigureAwait(false);
        return true;
    }


    // ==========================================================================
    // DISMISS
    // ==========================================================================

    /// <inheritdoc/>
    public Task DismissAsync(string toastId)
    {
        bool changed;
        lock (_gate)
        {
            changed = RemoveById(toastId);
            if (changed) PromoteFromQueue();
        }
        if (changed) EmitChanged();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DismissAllAsync()
    {
        lock (_gate)
        {
            foreach (var e in _active)
            {
                e.AutoDismissCts?.Cancel();
                e.AutoDismissCts?.Dispose();
                e.AutoDismissCts = null;
            }
            _active.Clear();
            _waiting.Clear();
        }
        EmitChanged();
        return Task.CompletedTask;
    }


    // ==========================================================================
    // INTERNAL HELPERS
    // ==========================================================================

    /// <summary>
    /// Resolve effective persistence — errors are persistent unless caller
    /// EXPLICITLY overrode DurationMs to a positive non-zero value. The
    /// Persistent flag in options always wins.
    /// </summary>
    private static bool ResolvePersistent(ToastEntry entry)
    {
        if (entry.Options.Persistent) return true;
        if (entry.Severity == ToastSeverity.Error)
        {
            // Error default = persistent. Allow explicit override only when
            // DurationMs is set to a positive non-zero value.
            return !(entry.Options.DurationMs.HasValue && entry.Options.DurationMs.Value > 0);
        }
        // For non-error severities, only DurationMs == 0 forces persistence.
        return entry.Options.DurationMs.HasValue && entry.Options.DurationMs.Value == 0;
    }

    /// <summary>Resolve per-severity default duration when caller didn't set one.</summary>
    private static int DefaultDurationMs(ToastSeverity s) => s switch
    {
        ToastSeverity.Success => 3000,
        ToastSeverity.Info    => 5000,
        ToastSeverity.Warning => 7000,
        ToastSeverity.Error   => 0,    // never reached — error is persistent path
        _                     => 5000
    };

    /// <summary>
    /// Arm the auto-dismiss timer for an entry. Cancels any prior CTS so
    /// repeated morphs / re-shows don't leak timers.
    /// </summary>
    private void ArmAutoDismiss(ToastEntry entry)
    {
        var duration = entry.Options.DurationMs ?? DefaultDurationMs(entry.Severity);
        if (duration <= 0) return;     // persistent

        var cts = new CancellationTokenSource();
        entry.AutoDismissCts = cts;
        var id = entry.Id;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(duration, cts.Token).ConfigureAwait(false);
                await DismissAsync(id).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Timer cancelled — toast was morphed, manually dismissed, or
                // the service is being disposed. Clean exit.
            }
        });
    }

    /// <summary>Remove an entry from active list. Caller holds _gate.</summary>
    private bool RemoveById(string id)
    {
        var idx = _active.FindIndex(e => e.Id == id);
        if (idx < 0)
        {
            // Maybe in the waiting queue — strip it there too.
            if (_waiting.Count > 0)
            {
                var remaining = _waiting.Where(e => e.Id != id).ToList();
                if (remaining.Count != _waiting.Count)
                {
                    _waiting.Clear();
                    foreach (var e in remaining) _waiting.Enqueue(e);
                    return true;
                }
            }
            return false;
        }
        _active[idx].AutoDismissCts?.Cancel();
        _active[idx].AutoDismissCts?.Dispose();
        _active[idx].AutoDismissCts = null;
        _active.RemoveAt(idx);
        return true;
    }

    /// <summary>Promote the next queued toast into the active list (if any).
    /// Caller holds _gate.</summary>
    private void PromoteFromQueue()
    {
        while (_active.Count < MaxVisible && _waiting.Count > 0)
        {
            var next = _waiting.Dequeue();
            _active.Add(next);
            if (!ResolvePersistent(next))
            {
                ArmAutoDismiss(next);
            }
        }
    }

    private void EmitChanged()
    {
        try { OnChanged?.Invoke(); }
        catch { /* one bad subscriber shouldn't poison others — best-effort dispatch */ }
    }


    // ==========================================================================
    // DISPOSE
    // ==========================================================================

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var e in _active)
            {
                e.AutoDismissCts?.Cancel();
                e.AutoDismissCts?.Dispose();
            }
            _active.Clear();
            _waiting.Clear();
        }
    }
}
