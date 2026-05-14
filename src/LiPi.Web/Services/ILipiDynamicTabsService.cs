// SPEC:  docs/03-LipiDynamicTabs-Spec.md §6
// PHASE: 2.6.2 — Overlay Surfaces
// AMEND: docs/CHANGE-LOG.md A32 — SetMaxTabs method added for cap enforcement.
//        Spec §6 service API didn't include cap-setting; A32 amends the
//        interface to support spec §7 step 4 ("If at cap → show warning modal").

namespace LiPi.Web.Services;

/// <summary>Runtime state of a single dynamic tab.</summary>
public sealed class DynamicTabInfo
{
    public required string   TabKey    { get; init; }
    public required string   Title     { get; set; }
    public string?           Icon      { get; init; }
    public required string   Url       { get; init; }
    public required string   CloseRoute { get; init; }
    public bool              IsDirty   { get; set; }
    public bool              IsActive  { get; set; }
    public DateTime          OpenedAt  { get; init; } = DateTime.UtcNow;
    public Func<Task>?       SaveHandler { get; set; }
}

/// <summary>
/// Manages runtime-opened tabs for the multi-patient workstation.
/// Each tab corresponds to a URL. Switching tabs navigates Blazor's router.
/// Registered as Scoped (per Blazor circuit).
/// </summary>
public interface ILipiDynamicTabsService
{
    /// <summary>Open a tab for the given URL. If the URL is already open,
    /// activates that tab instead of creating a duplicate.
    /// <para>Honors the cap set via <see cref="SetMaxTabs"/> — if the tab
    /// would push count above the cap, an alert is shown via
    /// <see cref="ILipiModalService"/> and the tab is NOT opened.</para></summary>
    Task OpenAsync(string url, string? tabKey = null, string? title = null, string? icon = null, string? closeRoute = null);

    /// <summary>Close a tab by its key. Returns true if the tab was closed,
    /// false if the user cancelled (dirty-state guard).</summary>
    Task<bool> CloseAsync(string tabKey);

    /// <summary>Close the currently active tab.</summary>
    Task<bool> CloseCurrentAsync();

    /// <summary>Mark a tab's dirty state.</summary>
    void SetDirty(string tabKey, bool isDirty);

    /// <summary>Update a tab's display title (call after async data loads).</summary>
    void UpdateTitle(string tabKey, string newTitle);

    /// <summary>Register a save handler for a tab (used by Save &amp; Close dialog option).</summary>
    void RegisterSaveHandler(string tabKey, Func<Task> handler);

    /// <summary>Navigate to the active tab's URL (called on strip re-render).</summary>
    void ActivateTab(string tabKey);

    /// <summary>A32 amendment — sets the soft cap on open tabs. When the cap
    /// is reached, <see cref="OpenAsync"/> displays a warning modal instead
    /// of opening the new tab. Called by <c>LipiDynamicTabs.razor</c> on init
    /// with the component's <c>MaxTabs</c> parameter.</summary>
    void SetMaxTabs(int maxTabs);

    IReadOnlyList<DynamicTabInfo> OpenTabs  { get; }
    DynamicTabInfo?               ActiveTab  { get; }

    /// <summary>Fired when the tab list or any tab's state changes.</summary>
    event Action OnTabsChanged;
}
