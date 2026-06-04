// SPEC:  docs/03-LipiDynamicTabs-Spec.md §6, §7
// PHASE: 2.6.2 — Overlay Surfaces
// AMEND: docs/CHANGE-LOG.md A32 — MaxTabs cap enforcement (spec §7 step 4).
//        When _maxTabs > 0 and adding would exceed the cap, OpenAsync shows
//        an alert modal and refuses to open. Set via SetMaxTabs from the
//        LipiDynamicTabs component on init.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using LiPi.Components.Overlays;

namespace LiPi.Components.Overlays;

/// <summary>
/// <see cref="ILipiDynamicTabsService"/> implementation.
/// Scoped — one instance per Blazor circuit (one per user session).
/// </summary>
public sealed class LipiDynamicTabsService : ILipiDynamicTabsService
{
    private readonly NavigationManager         _nav;
    private readonly ILipiModalService         _modal;
    private readonly ILogger<LipiDynamicTabsService> _log;
    private readonly List<DynamicTabInfo>      _tabs = new();
    private readonly object                    _lock = new();

    /// <summary>Soft cap on open tabs. 0 = unlimited. Set by LipiDynamicTabs
    /// component on init. A32: per spec §7 step 4, hitting the cap shows a
    /// warning modal instead of opening the new tab.</summary>
    private int _maxTabs;

    public LipiDynamicTabsService(
        NavigationManager               nav,
        ILipiModalService               modal,
        ILogger<LipiDynamicTabsService> log)
    {
        _nav   = nav;
        _modal = modal;
        _log   = log;
    }

    public event Action? OnTabsChanged;

    public IReadOnlyList<DynamicTabInfo> OpenTabs
    {
        get { lock (_lock) return _tabs.ToList(); }
    }

    public DynamicTabInfo? ActiveTab
    {
        get { lock (_lock) return _tabs.FirstOrDefault(t => t.IsActive); }
    }

    // ── Cap management (A32) ──────────────────────────────────────────────────

    public void SetMaxTabs(int maxTabs)
    {
        _maxTabs = Math.Max(0, maxTabs);
    }

    // ── Open ─────────────────────────────────────────────────────────────────

    public async Task OpenAsync(
        string  url,
        string? tabKey     = null,
        string? title      = null,
        string? icon       = null,
        string? closeRoute = null)
    {
        var resolvedKey = tabKey ?? url;

        // A32 cap check — pre-allocation. If tab already exists, no cap concern
        // (we just activate). Cap only blocks NEW tab creation.
        bool tabAlreadyOpen;
        int currentCount;
        lock (_lock)
        {
            tabAlreadyOpen = _tabs.Any(t => t.TabKey == resolvedKey);
            currentCount   = _tabs.Count;
        }

        if (!tabAlreadyOpen && _maxTabs > 0 && currentCount >= _maxTabs)
        {
            _log.LogInformation(
                "[LipiDynamicTabs] Refused to open '{Url}' — cap reached ({Count}/{Max})",
                url, currentCount, _maxTabs);

            await _modal.AlertAsync(
                title:   "Too many tabs open",
                message: $"You have {currentCount} tabs open. " +
                         $"Close some before opening more.",
                intent:  AlertIntent.Warning);
            return;
        }

        lock (_lock)
        {
            var existing = _tabs.FirstOrDefault(t => t.TabKey == resolvedKey);
            if (existing is not null)
            {
                // Tab already open — just activate it
                foreach (var t in _tabs) t.IsActive = false;
                existing.IsActive = true;
                _nav.NavigateTo(existing.Url);
                OnTabsChanged?.Invoke();
                return;
            }

            // Deactivate all existing tabs
            foreach (var t in _tabs) t.IsActive = false;

            _tabs.Add(new DynamicTabInfo
            {
                TabKey     = resolvedKey,
                Title      = title ?? "Loading…",
                Icon       = icon,
                Url        = url,
                CloseRoute = closeRoute ?? "/dashboard",
                IsActive   = true
            });
        }

        _nav.NavigateTo(url);
        OnTabsChanged?.Invoke();
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    public async Task<bool> CloseAsync(string tabKey)
    {
        DynamicTabInfo? tab;
        lock (_lock) tab = _tabs.FirstOrDefault(t => t.TabKey == tabKey);
        if (tab is null) return true;

        if (tab.IsDirty)
        {
            // Open 3-option dirty confirmation via modal service
            var hasSaveHandler = tab.SaveHandler is not null;
            var result = await _modal.ShowAsync<LiPi.Components.Overlays.DirtyTabConfirmDialog, TabCloseResult>(
                parameters: new Dictionary<string, object?>
                {
                    ["TabTitle"]       = tab.Title,
                    ["HasSaveHandler"] = hasSaveHandler
                },
                size: ModalSize.Compact);

            switch (result)
            {
                case TabCloseResult.Cancel:
                    return false;

                case TabCloseResult.Discard:
                    // fall through to close
                    break;

                case TabCloseResult.SaveAndClose:
                    if (tab.SaveHandler is not null)
                    {
                        try
                        {
                            await tab.SaveHandler();
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "[LipiDynamicTabs] Save handler failed for tab {TabKey}", tabKey);
                            return false; // keep tab open, show error surfaced by save handler
                        }
                    }
                    break;
            }
        }

        RemoveTab(tab);
        return true;
    }

    public Task<bool> CloseCurrentAsync()
    {
        DynamicTabInfo? active;
        lock (_lock) active = _tabs.FirstOrDefault(t => t.IsActive);
        return active is null ? Task.FromResult(true) : CloseAsync(active.TabKey);
    }

    private void RemoveTab(DynamicTabInfo tab)
    {
        bool wasActive;
        string? navigateTo = null;

        lock (_lock)
        {
            wasActive = tab.IsActive;
            _tabs.Remove(tab);

            if (wasActive && _tabs.Count > 0)
            {
                var next = _tabs[^1];
                next.IsActive = true;
                navigateTo = next.Url;
            }
            else if (wasActive)
            {
                navigateTo = tab.CloseRoute;
            }
        }

        if (navigateTo is not null)
            _nav.NavigateTo(navigateTo);

        OnTabsChanged?.Invoke();
    }

    // ── State mutations ───────────────────────────────────────────────────────

    public void SetDirty(string tabKey, bool isDirty)
    {
        DynamicTabInfo? tab;
        lock (_lock) tab = _tabs.FirstOrDefault(t => t.TabKey == tabKey);
        if (tab is null) return;
        tab.IsDirty = isDirty;
        OnTabsChanged?.Invoke();
    }

    public void UpdateTitle(string tabKey, string newTitle)
    {
        DynamicTabInfo? tab;
        lock (_lock) tab = _tabs.FirstOrDefault(t => t.TabKey == tabKey);
        if (tab is null) return;
        tab.Title = newTitle;
        OnTabsChanged?.Invoke();
    }

    public void RegisterSaveHandler(string tabKey, Func<Task> handler)
    {
        DynamicTabInfo? tab;
        lock (_lock) tab = _tabs.FirstOrDefault(t => t.TabKey == tabKey);
        if (tab is null) return;
        tab.SaveHandler = handler;
    }

    public void ActivateTab(string tabKey)
    {
        DynamicTabInfo? active;
        lock (_lock)
        {
            foreach (var t in _tabs) t.IsActive = t.TabKey == tabKey;
            active = _tabs.FirstOrDefault(t => t.TabKey == tabKey);
        }

        if (active is not null)
        {
            // A32: only navigate if URL differs from current — avoids
            // self-navigation when the active tab is already current.
            var currentPath = _nav.ToBaseRelativePath(_nav.Uri).TrimEnd('/').ToLowerInvariant();
            var targetPath  = active.Url.TrimStart('/').TrimEnd('/').ToLowerInvariant();
            if (currentPath != targetPath)
                _nav.NavigateTo(active.Url);
        }
        OnTabsChanged?.Invoke();
    }
}
