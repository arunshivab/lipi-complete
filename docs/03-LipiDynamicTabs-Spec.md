# Phase 2.6.2 — LipiDynamicTabs Specification

**Status:** LOCKED
**Component:** LipiDynamicTabs + LipiDynamicTabsService
**Purpose:** Multi-patient workstation, draft management, parallel work contexts

---

## 1. Why this exists

Phase 2.6.1's LipiTabs handles **static** tabs (fixed list, defined in markup).
LipiDynamicTabs handles **runtime-managed** tabs:

- Tabs open and close throughout the session
- Each tab represents independent work (patient, draft, report)
- Tabs have dirty state and confirmation on close
- Active tab corresponds to a URL — switching tabs is navigation
- Tab set persists across page navigation within the workspace

This is the core innovation for clinical multi-patient workflows.

---

## 2. Architecture

### Placement
LipiDynamicTabs is **placement-flexible** — can live in a layout (workspace-level)
or a page (module-scoped). Primary documented pattern is layout-level for clinical workstations.

```razor
@* In WorkspaceLayout.razor *@
<div class="workspace">
    <TopNav />
    <LipiDynamicTabs />          <!-- workspace tab strip -->
    <div class="workspace-body">
        @Body                     <!-- active tab content -->
    </div>
</div>
```

### Routing integration

Each tab is a route. Switching tabs navigates Blazor's router. URL changes.
Browser back/forward works. Bookmarks restore the same patient.

When the user navigates to a URL that matches a tab definition, LipiDynamicTabs
either:
- **If tab already open**: activates that tab (no new tab created)
- **If tab not open**: adds new tab and activates it

### Tab definition

A page declares itself as a dynamic tab via `@attribute [DynamicTab(...)]`:

```razor
@page "/patients/{Uhid}"
@attribute [DynamicTab(
    TabKey = "patient-{Uhid}",
    TitleProvider = nameof(GetTabTitle),
    IconProvider = nameof(GetTabIcon),
    CloseRoute = "/dashboard")]

@code {
    [Parameter] public string Uhid { get; set; }

    public string GetTabTitle() => $"Patient: {_patient?.DisplayName ?? "Loading..."}";
    public string GetTabIcon() => "ti-user";
}
```

The attribute tells LipiDynamicTabs:
- `TabKey` — unique tab identifier (templated with route params)
- `TitleProvider` — method name returning current title (re-called when data loads)
- `IconProvider` — method name returning icon
- `CloseRoute` — where to navigate when this tab is closed

---

## 3. LipiDynamicTabs parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `MaxTabs` | `int` | `10` | Soft cap — warning dialog above this |
| `OverflowMode` | `TabOverflowMode` | `Scroll` | Scroll / Dropdown |
| `ShowAddButton` | `bool` | `false` | Optional "+" button to open new tab |
| `OnAddClick` | `EventCallback` | — | Fires when "+" is clicked |
| `EmptyStateMessage` | `string?` | `"No open tabs"` | Shown when no tabs are open |
| `TabBackground` | `string?` | null | Override strip background color |

The component itself has very few parameters — it's mostly driven by the
`[DynamicTab]` attribute and the service.

---

## 4. Dynamic tab item

| Property | Type | Notes |
|---|---|---|
| `TabKey` | `string` | Unique identifier |
| `Title` | `string` | Current title (live, from TitleProvider) |
| `Icon` | `string?` | Icon class |
| `Url` | `string` | Route URL |
| `IsDirty` | `bool` | Unsaved changes — shown as `*` after title |
| `IsActive` | `bool` | Currently focused tab |
| `OnCloseRequested` | `EventCallback<TabCloseRequest>?` | Called when ✕ clicked |
| `OnSaveRequested` | `EventCallback?` | Called for "Save & close" |

---

## 5. Enums (LipiDynamicTabsTypes.cs)

```csharp
public enum TabOverflowMode
{
    Scroll,    // horizontal scroll bar appears
    Dropdown   // tabs beyond visible space collapse into "⋯" menu
}

public enum TabCloseResult
{
    Cancel,         // keep tab open
    Discard,        // close, lose changes
    SaveAndClose    // save first, then close
}
```

---

## 6. LipiDynamicTabsService API

```csharp
public interface ILipiDynamicTabsService
{
    // Open a tab programmatically (navigates to URL)
    Task OpenAsync(string url);

    // Close a tab by key
    Task<bool> CloseAsync(string tabKey);

    // Close current tab
    Task<bool> CloseCurrentAsync();

    // Mark current tab dirty/clean
    void SetDirty(string tabKey, bool isDirty);

    // Update tab title (called when async data loads)
    void UpdateTitle(string tabKey, string newTitle);

    // Register a save handler for current tab
    void RegisterSaveHandler(string tabKey, Func<Task> saveHandler);

    // Get all open tabs
    IReadOnlyList<DynamicTabInfo> OpenTabs { get; }

    // Currently active tab
    DynamicTabInfo? ActiveTab { get; }

    // Event fired when tabs change
    event Action OnTabsChanged;
}

public class DynamicTabInfo
{
    public string TabKey { get; init; }
    public string Title { get; set; }
    public string? Icon { get; init; }
    public string Url { get; init; }
    public bool IsDirty { get; set; }
    public bool IsActive { get; set; }
    public DateTime OpenedAt { get; init; }
}
```

---

## 7. Tab lifecycle

### Open
1. User navigates to a `[DynamicTab]` route (via link, search result, button)
2. LipiDynamicTabs intercepts navigation, checks if tab already exists
3. If exists → activate existing tab
4. If not → check MaxTabs cap
   - If under cap → add tab, activate
   - If at cap → show warning modal: "You have 10 tabs open. Close some before opening more."
5. Navigation completes — page renders inside active tab

### Activate (click existing tab)
1. User clicks tab in strip
2. LipiDynamicTabs calls `NavigationManager.NavigateTo(tab.Url)`
3. Blazor router activates the corresponding page
4. Previous tab content unmounts (destroyed — see Decision 1 in design doc)

### Close (✕ click or programmatic)
1. User clicks ✕ on a tab
2. LipiDynamicTabs checks `IsDirty` on that tab
3. **If clean**: tab closes immediately
   - If tab was active → navigate to `CloseRoute` (or previous tab, or dashboard)
   - If tab was inactive → just remove from strip
4. **If dirty**: open 3-option confirmation modal
   - **Cancel** → no change
   - **Discard changes** → close tab, lose work
   - **Save & close** → fire registered save handler, then close
     - If save fails → keep tab open, show error
     - If save succeeds → close and navigate as in (3)

### Destroy on close
Closed tab content is **unmounted**. State is lost. Reopening the same patient
hits the database for fresh data. This is intentional — clinical safety + memory cost.

---

## 8. Visual design

### Tab strip
```css
.lipi-dtabs-strip {
    display: flex;
    align-items: stretch;
    background: var(--color-bg-subtle);
    border-bottom: 0.5px solid var(--color-border-tertiary);
    overflow-x: auto;
    overflow-y: hidden;
    scrollbar-width: thin;
    min-height: 40px;
}
```

### Individual tab
```css
.lipi-dtab {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 0 12px 0 14px;
    border-right: 0.5px solid var(--color-border-tertiary);
    background: var(--color-bg-subtle);
    font-size: 12px;
    color: var(--color-text-secondary);
    cursor: pointer;
    transition: background 0.15s;
    max-width: 220px;
    min-width: 140px;
    flex-shrink: 0;
}

.lipi-dtab:hover { background: var(--color-bg-hover); }

.lipi-dtab-active {
    background: var(--color-bg-surface);
    color: var(--color-text-primary);
    font-weight: 500;
    box-shadow: inset 0 -2px 0 var(--color-primary);
}

.lipi-dtab-title {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.lipi-dtab-dirty::after {
    content: "*";
    color: var(--color-warning);
    margin-left: 4px;
    font-weight: 700;
}

.lipi-dtab-close {
    width: 18px; height: 18px;
    border-radius: 4px;
    display: flex; align-items: center; justify-content: center;
    cursor: pointer;
    color: var(--color-text-tertiary);
    flex-shrink: 0;
}

.lipi-dtab-close:hover {
    background: var(--color-bg-hover-strong);
    color: var(--color-text-primary);
}

.lipi-dtab-icon { font-size: 14px; flex-shrink: 0; }
```

### Overflow handling
When tabs exceed strip width:
- **TabOverflowMode.Scroll** (default): horizontal scrollbar; active tab auto-scrolls into view
- **TabOverflowMode.Dropdown**: visible tabs + "⋯ N more" menu showing remaining tabs

### Add button (when ShowAddButton=true)
```css
.lipi-dtab-add {
    width: 36px;
    border: none;
    background: var(--color-bg-subtle);
    color: var(--color-text-secondary);
    cursor: pointer;
    font-size: 16px;
}

.lipi-dtab-add:hover {
    background: var(--color-bg-hover);
    color: var(--color-text-primary);
}
```

---

## 9. Dirty state confirmation dialog

Triggered when user closes a dirty tab. Uses LipiModalService internally:

```csharp
var result = await Modal.ShowAsync<DirtyTabConfirmDialog, TabCloseResult>(
    parameters: new() {
        ["TabTitle"] = tab.Title,
        ["HasSaveHandler"] = tab.OnSaveRequested != null
    },
    size: ModalSize.Compact);

switch (result)
{
    case TabCloseResult.Cancel: return;
    case TabCloseResult.Discard: await CloseTabAsync(tab.TabKey); break;
    case TabCloseResult.SaveAndClose:
        try { await tab.OnSaveRequested.InvokeAsync(); }
        catch { /* keep tab open, surface error */ return; }
        await CloseTabAsync(tab.TabKey);
        break;
}
```

If no save handler is registered for the tab, the "Save & close" button is not shown — only Cancel and Discard.

---

## 10. Multi-industry tokens

```css
--lipi-dtab-min-width:    140px;
--lipi-dtab-max-width:    220px;
--lipi-dtab-height:       40px;
--lipi-dtab-active-bar:   var(--color-primary);
--lipi-dtab-strip-bg:     var(--color-bg-subtle);
```

---

## 11. Clinical use cases

| Use case | Tab key pattern | Source |
|---|---|---|
| Open patient chart | `patient-{Uhid}` | Search result click, patient list click |
| Draft prescription | `rx-draft-{TempId}` | "New prescription" button |
| Lab report viewer | `lab-{ReportId}` | Lab inbox click |
| Radiology study | `dicom-{StudyId}` | Radiology worklist click |
| Encounter note | `note-{EncounterId}` | Charting button |

Each pattern is defined by a `[DynamicTab]` attribute on the relevant page route.

---

## 12. Persistence (deferred to Phase 2.10+)

Phase 2.6.2 scope: tabs reset on page reload (no localStorage, no server-side state).

Future: persist open tabs across browser sessions so a clinician resumes where they left off.
This is queued in Phase 2.10 (Infrastructure Audit) — needs design discussion on
where state lives (localStorage / server session / per-user DB), refresh behavior,
and security implications (HIPAA: should PHI tab context persist?).

---

## 13. Accessibility

- Tab strip uses `role="tablist"` with `aria-orientation="horizontal"`
- Each tab uses `role="tab"`, `aria-selected="true|false"`, `aria-controls` pointing to content region
- Tab content area uses `role="tabpanel"`, `aria-labelledby` pointing to active tab
- Keyboard: ← → arrows move focus through tabs but DO NOT activate
- Enter/Space on focused tab activates (navigates)
- Ctrl+W (browser-reserved — cannot intercept) — workaround: Alt+W as closeable shortcut
- ✕ button has `aria-label="Close {tab title}"`
- Closing a dirty tab announces "Closing tab — unsaved changes" via aria-live

---

## 14. StyleGuide additions

New section `#dynamic-tabs`:
1. Empty state (no tabs open)
2. 3 tabs open, one active
3. Tab with dirty state (* marker)
4. Closing a clean tab (instant)
5. Closing a dirty tab (3-option modal)
6. Save & close success path
7. Save & close failure path (tab stays open)
8. Soft cap warning (open 11th tab)
9. Overflow: scroll mode
10. Overflow: dropdown mode
11. Add button + custom OnAddClick

---

*See: `00-Phase2.6.2-Overview.md` for shared infrastructure.*
