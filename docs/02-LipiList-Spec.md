# LipiList Specification

**Phase:** 2.8 — Data Display
**Component:** `LipiList<TItem>` + sub-components
**Status:** Spec body for build — locked
**Companion to:** LipiTable (this is the row-oriented sibling)

---

## §1 — Overview and design principles

### §1.1 — What LipiList is

LipiList is the row-oriented sibling to LipiTable. Each item is a self-contained vertical unit fully templated by the caller. LipiList provides the scaffolding (selection, pagination, status strip, separators, density, virtualization, empty/error/loading states) but doesn't dictate item layout.

Use cases:
- Notification feeds
- Activity logs / audit trails
- Search results (when results are heterogeneous)
- Comment threads
- File / attachment lists
- Chat-style message lists
- "Recently viewed" surfaces
- Mobile-friendly alternative to LipiTable (per §2.6 of LipiTable spec — when viewport < 1024px)

### §1.2 — When to use LipiList vs LipiTable

| Criterion | LipiTable | LipiList |
|---|---|---|
| Row structure | Parallel columns across rows | Each row self-contained |
| User comparison | Compare values down a column | Read each item independently |
| Content density | Many fields per row, scanned horizontally | Mixed content per item, read top-to-bottom |
| Viewport | ≥ 1024px ideal | Any width including mobile |
| Reading direction | Tabular grid scan | Vertical reading |

This is design guidance, not enforcement — the caller picks based on their use case.

### §1.3 — Design principles (inherited from LipiTable §1.3)

LipiList shares all six LipiTable principles:
1. **Component isolation** — redistributable, no HIS coupling
2. **Performance** — virtualization for large lists
3. **Clinical safety** — read-only by default; opt-in interactivity
4. **Token-driven theming** — light/dark/high-contrast via foundation tokens
5. **Accessibility first** — non-negotiable WCAG AA
6. **Caller-controlled rendering** — `<ItemTemplate>` is the API

### §1.4 — Out of scope for v1.0

- **Inline editing of list items** — caller drives edit via modal / drawer / detail expand outside the list
- **Tree / nested structure** — use LipiTable's tree mode for hierarchical data
- **Drag-to-reorder** — deferred; add when first consumer needs it
- **Grouping** — deferred; same reasoning
- **Master-detail** — items themselves can be expandable via caller's template logic, but LipiList doesn't ship a built-in detail mechanism

For each out-of-scope item, the caller composes: a Reorder mode for list items can be built by the caller using a LipiButton drag handle + custom JS; tree-shaped data uses LipiTable; etc.

---

## §2 — Component anatomy

### §2.1 — Visual structure

Same regions as LipiTable but simplified:

```
┌──────────────────────────────────────────────────┐
│ ⓪ Header band (Title, Subtitle, HeaderTemplate)  │  ← optional
├──────────────────────────────────────────────────┤
│ ① Toolbar (Search, Density, slots, Add button)   │  ← optional
├──────────────────────────────────────────────────┤
│ ② Filter chips (when filters active)             │  ← auto
├──────────────────────────────────────────────────┤
│ ③ Bulk action bar (when selection > 0)           │  ← auto
├──────────────────────────────────────────────────┤
│ ▏ ☐ [Item 1 — caller-rendered via ItemTemplate]  │
│ ─────────────────────────────────────────────────│  ← separator
│ ▏ ☑ [Item 2 — caller-rendered]                   │
│ ─────────────────────────────────────────────────│
│ ▏ ☐ [Item 3 — caller-rendered]                   │
│  ...                                              │
├──────────────────────────────────────────────────┤
│ ⑧ Footer (LipiPagination + count)                │
└──────────────────────────────────────────────────┘
```

Region IDs match LipiTable's where shared (⓪, ①, ②, ③, ⑧). LipiList has no ④ group bar, ⑤ cap banner (not applicable), ⑥ column headers (no columns), ⑦ body distinct from items (the item rows ARE the body), ⑧ₐ aggregate row (no aggregation), or ⑨ sticky edit bar (no inline edit).

### §2.2 — Element inventory

| Element | Description |
|---|---|
| Header band | Title / Subtitle / `<HeaderTemplate>` (per LipiTable §16.6) |
| Toolbar | Quick search, density toggle, custom slots, Add button |
| Filter chips strip | Auto when filters active (per LipiTable §7.8) |
| Bulk action bar | Auto when selection > 0 (per LipiTable §5.6.4) |
| Item container | The row wrapper — receives selection / status / density / focus styling |
| Item content | Caller's `<ItemTemplate>` rendering |
| Item separator | Visual divider between items (configurable per §9) |
| Status strip | Left-border accent (opt-in via `ItemStatus`) |
| Selection checkbox | When selection enabled |
| Footer | LipiPagination + count display |

### §2.3 — CSS class naming

All classes follow the `lipi-list-*` prefix per the isolation contract:

- `lipi-list` — outer container
- `lipi-list-toolbar` — region ① (cross-reference LipiTable §2.2.2)
- `lipi-list-filter-chips` — region ② (cross-reference LipiTable §2.2.4)
- `lipi-list-bulk-bar` — region ③ (cross-reference LipiTable §2.2.6)
- `lipi-list-items` — items container
- `lipi-list-item` — single item row
- `lipi-list-item-selected` — selected state modifier
- `lipi-list-item-focused` — keyboard focus modifier
- `lipi-list-item-disabled` — disabled state modifier
- `lipi-list-item-separator` — visual divider between items
- `lipi-list-item-checkbox` — selection checkbox cell
- `lipi-list-item-content` — caller-templated content area
- `lipi-list-footer` — region ⑧

Status strip reuses `lipi-status-strip-left` from shared tokens (per `lipi-status-tokens.css`).

---

## §3 — Generic typing

### §3.1 — `LipiList<TItem>` signature

```csharp
public partial class LipiList<TItem> : ComponentBase, IAsyncDisposable
    where TItem : class
```

- `TItem` = item data type, caller-defined
- **Items** (client-side) OR **DataSource** (server-side) — mutually exclusive, exactly one required
- **KeySelector** — required, returns stable identifier per item

The constraint `where TItem : class` allows null checks on items and prevents value-type semantics that don't work with reference equality.

### §3.2 — Item template contract

`<ItemTemplate>` is the required RenderFragment:

```razor
<LipiList TItem="Notification"
          Items="@_notifications"
          KeySelector="@(n => n.Id)">
    <ItemTemplate Context="n">
        <div class="notification-row">
            <LipiAvatar Src="@n.SenderAvatar" />
            <div class="content">
                <strong>@n.Title</strong>
                <p>@n.Body</p>
                <time>@n.Timestamp.Humanize()</time>
            </div>
            <LipiBadge>@n.Category</LipiBadge>
        </div>
    </ItemTemplate>
</LipiList>
```

The template receives the item directly. Inside, the caller has full rendering freedom — any HTML, any nested components, any conditional logic.

LipiList does NOT provide:
- Column-like field selectors (there are no columns)
- Cell-level conditional formatting
- Per-item type variation hints (every item uses the same template)

For varying item shapes within one list, the caller's `<ItemTemplate>` switches on item state:

```razor
<ItemTemplate Context="item">
    @switch (item.Kind)
    {
        case ItemKind.Notification: <NotificationItem Data="@item" />; break;
        case ItemKind.SystemAlert:  <SystemAlertItem Data="@item" />;  break;
        case ItemKind.Comment:      <CommentItem Data="@item" />;       break;
    }
</ItemTemplate>
```

The switch is the caller's responsibility — LipiList doesn't auto-route.

### §3.3 — Display name selector

```csharp
[Parameter] public Func<TItem, string>? ItemDisplayNameSelector { get; set; }
```

Provides a human-readable name for the item — used in:
- ARIA labels on selection checkboxes ("Select item, Notification from Alice")
- aria-live announcements
- Default focus / expand identifiers

Fallback: `KeySelector(item).ToString()`.

### §3.4 — Type definitions

Per LipiTable §3 conventions:
- All public types live in `LiPi.Components.DataDisplay.LipiList.*` namespace
- Generic types use `LipiList<TItem>` / `LipiListItem<TItem>` patterns
- Enums in separate `.cs` files (e.g., `LipiListSelectionMode`, `LipiListDensity`)

---

## §4 — Data sources

LipiList uses the same data source model as LipiTable §4. Brief summary:

### §4.1 — Items mode (client-side)

```razor
<LipiList Items="@_data" KeySelector="@(x => x.Id)" ...>
```

All filtering / sorting / pagination computed in-memory.

### §4.2 — DataSource mode (server-side)

```razor
<LipiList DataSource="@LoadAsync" KeySelector="@(x => x.Id)" ...>
```

`DataSource` signature is identical to LipiTable:

```csharp
public Func<TableQueryRequest, CancellationToken, Task<TableQueryResponse<TItem>>>? DataSource
```

The same `TableQueryRequest` / `TableQueryResponse<TItem>` types are reused — LipiList doesn't define its own. This is intentional: callers building data sources can switch between LipiTable and LipiList without rewriting the DataSource handler.

### §4.3 — Differences from LipiTable's data source

- **Sort**: LipiList sorts by a caller-specified expression, NOT by column key. The request still uses `SortDescriptor` records, but the column key is replaced by a `Field` identifier the caller chose.
- **Filters**: LipiList exposes a single quick-search input by default. Multi-field filters are caller-driven via `<ToolbarLeft>` slot.
- **Group / aggregate / distinct values**: Not used by LipiList (no grouping in v1.0).

In server-side mode, the caller's `DataSource` handler handles fewer dimensions than LipiTable's. Cross-reference LipiTable §4 for the full request/response shape; LipiList's usage is a subset.

---

## §5 — Selection

LipiList selection is structurally identical to LipiTable §5. Same model: key set, three modes, two-step select-all.

### §5.1 — Selection modes

```csharp
public enum LipiListSelectionMode
{
    None,    // default
    Single,
    Multi
}
```

- **None** — display only; no checkboxes, no selection state
- **Single** — radio-button semantics; only one item selected at a time
- **Multi** — checkbox semantics; multiple items selected

Selection storage = `HashSet<object>` keyed by `KeySelector(item)`. Items removed from data drop from selection automatically.

### §5.2 — Selection UI

Checkbox renders on the **left** of each item, above the status strip:

```
▏ ☐ [Item content...]
▏ ☑ [Item content...]
```

In Single mode, "checkbox" is rendered as a radio. Visual is a small circle that fills when selected.

### §5.3 — Bulk action bar

Identical to LipiTable §5.6.4:
- Appears above the items list when selection > 0
- Renders `<BulkActionTemplate>` on the right, "X selected" on the left, "Clear selection" link
- Sticks to the top of the items container as the user scrolls

### §5.4 — Select all / select all across pages

Identical pattern to LipiTable §5.3.1 / §5.3.2 — "select all on page" → banner offering "select all N across pages" → flag-based selection. Cross-reference LipiTable §5 for the full mechanics.

### §5.5 — Keyboard

| Key | Effect |
|---|---|
| Space | Toggle current item's selection |
| Shift+Space | Extend selection from anchor to current |
| Ctrl/Cmd+Space | Toggle without changing anchor |
| Ctrl/Cmd+A | Select all on page |
| Ctrl/Cmd+Shift+A | Select all across pages (skips banner) |
| Escape | Clear selection (when count > 0) |
| Shift+Arrow Up/Down | Extend selection |

### §5.6 — Events

```csharp
[Parameter] public EventCallback<SelectionChangedContext<TItem>> OnSelectionChanged { get; set; }
[Parameter] public EventCallback<bool> OnSelectAllAcrossPagesChanged { get; set; }
[Parameter] public IReadOnlyList<TItem> SelectedItems { get; set; } = Array.Empty<TItem>();
[Parameter] public EventCallback<IReadOnlyList<TItem>> SelectedItemsChanged { get; set; }
```

Two-way `@bind-SelectedItems` works identically to LipiTable.

---

## §6 — Sort, filter, search

### §6.1 — Sort

LipiList sort works on a single dimension (no multi-sort in v1.0). The caller defines sort options via:

```csharp
[Parameter] public IReadOnlyList<ListSortOption<TItem>> SortOptions { get; set; } = Array.Empty<ListSortOption<TItem>>();

public sealed record ListSortOption<TItem>(
    string Key,
    string Label,
    Func<TItem, object?> Selector,
    SortDirection DefaultDirection
);
```

Example:

```razor
<LipiList TItem="Notification"
          Items="@_notifications"
          KeySelector="@(n => n.Id)"
          SortOptions="@_sortOptions">
    ...
</LipiList>

@code {
    private List<ListSortOption<Notification>> _sortOptions = new()
    {
        new("date-desc", "Newest first", n => n.Timestamp, SortDirection.Descending),
        new("date-asc",  "Oldest first", n => n.Timestamp, SortDirection.Ascending),
        new("sender",    "By sender",    n => n.SenderName, SortDirection.Ascending),
    };
}
```

The toolbar renders a `LipiSelect` of sort options. Default sort = first option in the list.

### §6.2 — Filter

LipiList in v1.0 supports two filter mechanisms:

1. **Quick search** — single text input in toolbar, matches against caller-defined searchable fields:

```csharp
[Parameter] public Func<TItem, string, bool>? QuickSearchMatcher { get; set; }
```

```razor
<LipiList QuickSearchMatcher="@(n, q) => 
    n.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
    n.Body.Contains(q, StringComparison.OrdinalIgnoreCase)" ...>
```

If `QuickSearchMatcher` is not set, the search input is hidden. If set without `ShowQuickFilter`, search input is hidden but caller can still drive search programmatically.

2. **Caller-driven filter chips** — caller renders chips in `<ToolbarLeft>` slot and filters `Items` themselves before passing to LipiList.

LipiList does NOT ship column-level filtering (no columns to filter on). For richer filtering, the caller composes a `LipiSelect` / `LipiDatePicker` set in the toolbar and applies the filter in their own code.

### §6.3 — Search debounce

The quick search input debounces 300ms per LipiTable §7.10.2 pattern (consistent across the component library).

---

## §7 — Pagination

LipiList uses LipiPagination (sibling component, see §27.11.2 of LipiTable spec) for pagination UI.

### §7.1 — Modes

```csharp
public enum LipiListPaginationMode
{
    None,           // no pagination — all items render
    Standard,       // numbered pager + page-size selector
    Compact,        // ◂ Page N of M ▸
    InfiniteScroll, // auto-load next page on near-bottom scroll
    LoadMore        // explicit "Load more" button at bottom
}
```

Cross-reference LipiTable §8 for full pagination behavior. LipiList's modes are 1:1 mappings of LipiTable's. Default: `Standard` when item count > page size; `None` otherwise.

### §7.2 — Page size options

Default `[10, 25, 50, 100, 200, All]`. `All` capped at 1000 in server-side mode (same as LipiTable §8.2.2).

### §7.3 — Composition

The footer renders:

```
Showing 1-20 of 487    [10 ▾]    [◂ 1 2 3 4 5 ... 25 ▸]
        ▲                  ▲                  ▲
   count display      page size       LipiPagination
```

All three are sub-components: `LipiPaginationCountDisplay`, `LipiPaginationPageSize`, `LipiPagination`. Reused from sibling components.

### §7.4 — Persistence

Page size persists; current page does NOT — same rule as LipiTable §8.9.

---

## §8 — Status strip and conditional formatting

### §8.1 — Status strip per item

```csharp
[Parameter] public Func<TItem, string?>? ItemStatus { get; set; }
```

Returns a status string per item; LipiList applies `lipi-status-strip-left` + `data-status="{value}"`. Cross-reference LipiTable §22.2 for the full status taxonomy.

```razor
<LipiList ItemStatus="@(n => n.IsRead ? "inactive" : "active")" ...>
```

Read notifications get a grey strip; unread get a green strip. Status color taxonomy is shared with LipiTable via `lipi-status-tokens.css`.

### §8.2 — Item-level class and style

```csharp
[Parameter] public Func<TItem, string?>? ItemClass { get; set; }
[Parameter] public Func<TItem, string?>? ItemStyle { get; set; }
[Parameter] public Func<TItem, bool>? ItemDisabled { get; set; }
```

Identical semantics to LipiTable's `RowClass`, `RowStyle`, `RowDisabled` (cross-reference LipiTable §22).

### §8.3 — Combined

Same composition rules as LipiTable §22.6. Status + class + style + disabled can all be active on one item; visual layers compose.

### §8.4 — Cell-level formatting

Not applicable — there are no cells. All visual variation lives in the caller's `<ItemTemplate>`.

---

## §9 — Density and separators

### §9.1 — Density

```csharp
public enum LipiListDensity
{
    Compact,
    Comfortable,    // default
    Spacious
}
```

Density affects:
- Item vertical padding
- Item min-height
- Separator visual
- Toolbar height
- Checkbox sizes (cross-reference LipiTable §14)

The caller's `<ItemTemplate>` content is NOT scaled by density — that's caller's call. LipiList scales only the scaffolding around the item content.

Density tokens follow the LipiTable pattern with `--lipi-list-*` namespace (per §16).

### §9.2 — Separators

```csharp
public enum LipiListSeparator
{
    Line,       // 1px line between items (default)
    Spacing,    // no line, just vertical gap
    Card        // no line; each item visually a card with shadow
}
```

- **Line** (default) — clean inset divider, matches LipiTable row borders
- **Spacing** — 8px / 12px / 16px gap per density, no visual line; airier feel for cards-of-content
- **Card** — each item is a rounded card with subtle shadow; appropriate for dashboard-style summary lists

Per-item separator override:

```csharp
[Parameter] public Func<TItem, LipiListSeparator?>? ItemSeparatorOverride { get; set; }
```

Returns the separator type to use BELOW that specific item. Useful for visually distinguishing pinned items, important alerts, etc.

### §9.3 — First/last item handling

By default, the first item has no separator above, and the last item has no separator below. The container's border-top / border-bottom (when shown) provides the visual edge.

For lists embedded in cards with their own borders, set `ShowOuterBorders="false"` (default `true`).

---

## §10 — States (Empty / Loading / Error / FilteredEmpty)

Cross-reference LipiTable §18. LipiList uses the same five-state model with identical priority order (Loading > Error > FilteredEmpty > Empty > Normal) and template slots.

### §10.1 — Slot names

```razor
<LipiList ...>
    <ItemTemplate Context="i">...</ItemTemplate>
    
    <LoadingTemplate>...</LoadingTemplate>
    <EmptyTemplate>...</EmptyTemplate>
    <FilteredEmptyTemplate>...</FilteredEmptyTemplate>
    <ErrorTemplate Context="err">...</ErrorTemplate>
</LipiList>
```

Same render fragments as LipiTable. Same `ErrorContext` exposing `ErrorMessage`, `Exception`, `RetryAsync()`.

### §10.2 — Default UIs

- **Loading** — skeleton items (LipiSkeleton rectangle of approximately one-item-height; ~8 items rendered)
- **Empty** — LipiEmptyState (Lucide `inbox` icon, "No items yet", optional CTA via `EmptyShowAddCta`)
- **FilteredEmpty** — LipiEmptyState (Lucide `search-x` icon, "No matches found", "Clear search" CTA)
- **Error** — LipiEmptyState (Lucide `alert-octagon` icon, "Couldn't load items", Retry button)

### §10.3 — Difference from LipiTable

Skeleton items mimic the **item template** rather than table columns. Since LipiList doesn't know what the item template will render, the skeleton uses generic shapes:

- One avatar circle on the left (40px)
- Two text lines (one long, one shorter)
- One small badge on the right

This is the "generic notification" skeleton. For lists with different item shapes (file list, comment thread), the caller provides `<LoadingTemplate>` with a tailored skeleton.

---

## §11 — Accessibility

Cross-reference LipiTable §19 for the full accessibility framework. LipiList differences:

### §11.1 — ARIA structure

LipiList uses `role="list"` (NOT `role="grid"` — there are no cells to navigate):

```html
<div role="list" 
     aria-label="Notifications"
     aria-multiselectable="true">
    
    <div role="listitem"
         aria-selected="false"
         aria-label="Notification from Alice">
        <!-- item content -->
    </div>
    ...
</div>
```

- `role="list"` for the container
- `role="listitem"` for each item
- `aria-multiselectable` reflects SelectionMode
- `aria-selected` per item (when selection enabled)

### §11.2 — Keyboard navigation

Different from LipiTable — items navigate row-by-row (not cell-by-cell):

| Key | Effect |
|---|---|
| Tab | Move focus into / out of the list |
| Arrow Down | Move focus to next item |
| Arrow Up | Move focus to previous item |
| Home | Move to first item |
| End | Move to last item |
| Page Down | Move down by one viewport-height |
| Page Up | Move up by one viewport-height |
| Space | Toggle selection (when selection enabled) |
| Enter | Activate item (fires `OnItemClick` if set) |

Within the item, Tab moves through the focusable elements inside the `<ItemTemplate>`. The caller's template may contain buttons, links, inputs — these participate in the tab order.

### §11.3 — Screen reader announcements

Same patterns as LipiTable §19.5:
- Initial: "Notifications, list, 47 items"
- Item focus: "Notification from Alice, item 3 of 47" (plus item's display name)
- Selection: "Selected, Notification from Alice. 1 of 47 selected."
- State changes: announced via aria-live polite (loading, empty, error, filtered-empty)

### §11.4 — High-contrast mode

Same token-driven mechanism. LipiList's CSS uses `--color-*` and `--lipi-list-*` tokens that swap at the foundation layer per LipiTable §24.7.

---

## §12 — Virtualization

Cross-reference LipiTable §20. LipiList's virtualization is simpler because rows have fewer DOM nodes:

### §12.1 — Auto threshold

Same default 100-item threshold (`VirtualizationThreshold`). For lists with very heavy `<ItemTemplate>` content (many nested components), lower the threshold to virtualize earlier.

### §12.2 — Variable height

LipiList items have **variable height by default** (caller's template can render any height). The variable-height virtualization mode is the default — no need for an `ItemSize` parameter.

For lists with known-uniform item heights, set `ItemHeight` to enable the faster fixed-height mode:

```csharp
[Parameter] public int? ItemHeight { get; set; }
```

When set, virtualization assumes every item is exactly that pixel height — O(1) scroll math. Faster but breaks if any item exceeds the declared height (truncated).

### §12.3 — Performance characteristics

- 1000 items × generic item template (avatar + 2 lines + badge): ~30 visible items DOM, <50ms first paint
- 10,000 items: same DOM count, <100ms first paint
- 50,000 items (stress test): ~200ms first paint, smooth 60fps scroll

Performance budgets identical to LipiTable §20.11.

---

## §13 — Persistence

Cross-reference LipiTable §21. LipiList persists:

- Sort key (which `ListSortOption` is active)
- Quick search text
- Density
- Page size

Does NOT persist:
- Current page (snap to 1 next visit)
- Selection
- Scroll position

Same `TableId` parameter activates persistence:

```razor
<LipiList TableId="notifications-list" ...>
```

When set, LipiList reads / writes to the same `identity.user_table_preferences` table as LipiTable (uses `ITablePreferenceService`).

The persisted document shape is a subset of LipiTable's:

```json
{
    "version": 1,
    "sort": [{ "columnKey": "date-desc", "direction": "desc", "priority": 0 }],
    "quickSearch": "",
    "density": "Comfortable",
    "pageSize": 25
}
```

Same debounce / flush-on-unmount / cache rules as LipiTable §21.

### §13.1 — `PersistQuickSearch="false"` opt-out

Per LipiTable §21.10.1, when search may contain sensitive identifiers:

```razor
<LipiList PersistQuickSearch="false" ...>
```

Phase 2.10 audit identifies which lists need this.

---

## §14 — Export

Cross-reference LipiTable §17. LipiList supports CSV and Print export. Differences:

### §14.1 — Export contract

Since there are no columns, the caller declares export fields:

```csharp
[Parameter] public IReadOnlyList<ListExportColumn<TItem>> ExportColumns { get; set; } = Array.Empty<ListExportColumn<TItem>>();

public sealed record ListExportColumn<TItem>(
    string Header,
    Func<TItem, object?> ValueSelector,
    string? Format = null
);
```

Example:

```razor
<LipiList TItem="Notification"
          ShowExportButton="true"
          ExportColumns="@_exportColumns" ...>
```

```csharp
private List<ListExportColumn<Notification>> _exportColumns = new()
{
    new("Title", n => n.Title),
    new("Sender", n => n.SenderName),
    new("Date", n => n.Timestamp, "yyyy-MM-dd HH:mm"),
    new("Category", n => n.Category),
};
```

When `ExportColumns` is empty, the Export button is hidden.

### §14.2 — Same exporters

Uses the same `CsvExporter` and `PrintHandler` infrastructure as LipiTable. PDF export is the same Phase 2.10 stub.

### §14.3 — Excel

Same posture as LipiTable §17.8 — not in v1.0; CSV is the recommended path.

---

## §15 — Events

Cross-reference LipiTable §23 for the comprehensive event catalog. LipiList's event surface is a subset:

| Event | Type | Use |
|---|---|---|
| `OnItemClick` | `EventCallback<TItem>` | Navigate to detail |
| `OnItemDoubleClick` | `EventCallback<TItem>` | Custom double-click action |
| `OnSelectionChanged` | `EventCallback<SelectionChangedContext<TItem>>` | Track selection |
| `OnSelectAllAcrossPagesChanged` | `EventCallback<bool>` | Bulk-all UX |
| `OnSortChanged` | `EventCallback<SortChangedContext>` | URL sync |
| `OnQuickSearchChanged` | `EventCallback<QuickSearchChangedContext>` | URL sync |
| `OnPageChanged` | `EventCallback<PageChangedContext>` | URL sync |
| `OnPageSizeChanged` | `EventCallback<PageSizeChangedContext>` | Analytics |
| `OnRefresh` | `EventCallback` | Refresh external data |
| `OnDensityChanged` | `EventCallback<DensityChangedContext>` | Analytics |
| `OnAddClick` | `EventCallback` | Open add-modal (no inline add in LipiList) |
| `OnBeforeExport` | `Func<BeforeExportContext, Task<bool>>?` | Permission check, BG-job |
| `OnAfterExport` | `EventCallback<AfterExportContext>` | Audit log |
| `OnError` | `EventCallback<TableErrorContext>` | App-level error logging |
| `OnQueryComplete` | `EventCallback<QueryCompleteContext<TItem>>` | Performance tracking |
| `OnPersisted` | `EventCallback<PersistedContext>` | UI preference audit |

**Not in LipiList** (LipiTable-only): all inline-edit events (OnRowEditStart, OnRowSave, OnBeforeRowSave, OnCellEditStart, etc.), group events, tree events, master-detail events, column ops events.

LipiList has **16 events** vs LipiTable's 40. The simpler surface matches the simpler component.

### §15.1 — Event ordering

Same deterministic ordering as LipiTable §23.11. The relevant LipiList sequences:

- User changes sort → discard confirm (if any), `OnSortChanged`, page reset to 1, `OnPageChanged(ResetAfterFilter)`, DataSource invoked, `OnQueryComplete`, persistence write, `OnPersisted(SortChange)`
- User selects an item → `OnSelectionChanged`
- User searches → debounced 300ms → `OnQuickSearchChanged`, page reset, DataSource invoked, etc.

### §15.2 — Async handlers and exception handling

Identical to LipiTable §23.12 / §23.13. Async handlers awaited; exceptions caught and logged silently.

### §15.3 — Two-way bindings

Five bindings supported:
- `@bind-SelectedItems`
- `@bind-Density`
- `@bind-PageSize`
- `@bind-Page`
- `@bind-QuickSearchText`

Same shorthand syntax as LipiTable §23.14.4.

---

## §16 — Multi-industry tokens

LipiList uses the same token contract as LipiTable. Specific list-namespace tokens added:

```css
:root {
    /* Item dimensions (per density) */
    --lipi-list-item-min-h-compact:        48px;
    --lipi-list-item-min-h-comfortable:    64px;
    --lipi-list-item-min-h-spacious:       88px;
    --lipi-list-item-min-h:                var(--lipi-list-item-min-h-comfortable);
    
    /* Item padding */
    --lipi-list-item-pad-y-compact:        8px;
    --lipi-list-item-pad-y-comfortable:    12px;
    --lipi-list-item-pad-y-spacious:       16px;
    --lipi-list-item-pad-y:                var(--lipi-list-item-pad-y-comfortable);
    
    --lipi-list-item-pad-x-compact:        12px;
    --lipi-list-item-pad-x-comfortable:    16px;
    --lipi-list-item-pad-x-spacious:       20px;
    --lipi-list-item-pad-x:                var(--lipi-list-item-pad-x-comfortable);
    
    /* Separator */
    --lipi-list-separator-color:           var(--color-border-subtle);
    --lipi-list-separator-w:               1px;
    --lipi-list-separator-gap-spacing:     12px;  /* used when LipiListSeparator=Spacing */
    
    /* Item background and states */
    --lipi-list-bg-item:                   transparent;
    --lipi-list-bg-item-hover:             var(--color-primary-50);
    --lipi-list-bg-item-selected:          var(--color-primary-100);
    --lipi-list-bg-item-focused:           var(--color-primary-50);
    --lipi-list-bg-item-disabled:          var(--color-surface-secondary);
    
    /* Card variant (when LipiListSeparator=Card) */
    --lipi-list-card-radius:               var(--r-3);
    --lipi-list-card-shadow:               var(--shadow-1);
    --lipi-list-card-gap:                  12px;
    
    /* Checkbox sizing per density */
    --lipi-list-checkbox-size-compact:     16px;
    --lipi-list-checkbox-size-comfortable: 18px;
    --lipi-list-checkbox-size-spacious:    20px;
    --lipi-list-checkbox-size:             var(--lipi-list-checkbox-size-comfortable);
    
    /* Z-index */
    --lipi-list-z-bulk-bar:                3;
}
```

Lives in `wwwroot/css/lipi-list-tokens.css`. Mode swaps inherited from foundation tokens per LipiTable §24.7.

---

## §17 — Component isolation contract

Cross-reference LipiTable §25. LipiList follows identical rules:

- **Namespace**: `LiPi.Components.DataDisplay.LipiList.*`
- **CSS prefix**: `lipi-list-*`
- **No HIS-specific names** in public API
- **No banned dependencies** (no external UI libs, no consumer-specific types)
- **JS interop**: shares `lipi-table-interop.js` (no separate LipiList JS module needed)

### §17.1 — Specific isolation items

- LipiList must NOT depend on `LipiTable` source code (independence) — but both depend on shared infrastructure (`LipiStatus`, status tokens, `ITablePreferenceService`, etc.)
- Demo data in StyleGuide demos is domain-agnostic (notifications / files / comments / search results — never clinical)
- Public API uses only generic types (TItem, ListSortOption, ListExportColumn — no HIS terms)

### §17.2 — Phase 2.10 audit verification

Same audit checklist as LipiTable §25.8 applies to LipiList. Build chat runs the same scans.

---

## §18 — StyleGuide additions

Cross-reference LipiTable §26 for the StyleGuide infrastructure. LipiList has **one** demo page:

**Route:** `/styleguide/data-display/lipi-list`

**Purpose:** Show LipiList in two configurations:

### §18.1 — Demo 1: Notification feed

Domain-agnostic sample data: 20 notifications with sender, title, body, timestamp, category.

Demonstrated:
- Default `ItemTemplate` with avatar + content + badge
- `ItemStatus` for read/unread strip
- Multi-selection with bulk action bar ("Mark as read", "Archive")
- Quick search across title + body
- Sort options (newest first / oldest first / by sender)
- Density toggle
- Pagination with 25 per page

### §18.2 — Demo 2: File list (separator variants)

Sample data: 15 files with name, size, modified date, owner.

Demonstrated:
- Same LipiList component, different `ItemTemplate`
- Three separator modes side-by-side (`Line` / `Spacing` / `Card`)
- Status strip showing file state (active / archived / shared)

### §18.3 — A11y notes

Same notes as LipiTable's demos. `role="list"`, arrow nav, screen reader announcements.

The demo page lives at `src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiListDemo.razor`.

---

## §19 — Files to create

Cross-reference LipiTable §27 for the file conventions.

### §19.1 — LipiList source files

```
src/LiPi.Components/DataDisplay/LipiList/
├── LipiList.razor
├── LipiList.razor.cs
├── LipiList.razor.css
├── LipiListItem.razor
├── LipiListItem.razor.css
├── LipiListTypes.cs                    (LipiListSelectionMode, LipiListDensity, LipiListSeparator, LipiListPaginationMode)
└── Internal/
    ├── LipiListToolbar.razor
    ├── LipiListToolbar.razor.css
    ├── LipiListBulkActionBar.razor
    ├── LipiListBulkActionBar.razor.css
    ├── LipiListSkeletonItem.razor
    ├── LipiListSkeletonItem.razor.css
    ├── LipiListFooter.razor
    └── LipiListFooter.razor.css
```

Approximately **15 files**.

### §19.2 — Shared infrastructure (not duplicated)

LipiList reuses:
- `TableQueryRequest` / `TableQueryResponse<TItem>` (from LipiTable)
- `ITablePreferenceService` + `TablePreferences` (from LipiTable)
- `LipiStatus` constants
- `lipi-status-tokens.css`
- `CsvExporter` + `PrintHandler` + export types (from LipiTable)
- `lipi-table-interop.js` (renamed in build chat — or shared)
- LipiPagination sub-components (sibling component)
- LipiEmptyState (sibling component)
- LipiSkeleton (Phase 2.7)
- LipiCheckbox / LipiSelect (Phase 2.2)

No duplication. LipiList is the thin layer atop shared infrastructure.

### §19.3 — Shared CSS

```
src/LiPi.Web/wwwroot/css/lipi-list-tokens.css
```

One new CSS file for LipiList-specific tokens (per §16). Loaded via App.razor link tag.

### §19.4 — Demo file

```
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiListDemo.razor
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiListDemo.razor.cs
```

Already counted in LipiTable §27.13.

### §19.5 — Tests

```
test/LiPi.Components.Tests/DataDisplay/LipiListTests.cs
test/LiPi.Components.Tests/DataDisplay/LipiListSelectionTests.cs
test/LiPi.Components.Tests/DataDisplay/LipiListPersistenceTests.cs
```

Three test files cover LipiList's surface. Most of LipiList's behavior is delegated to or analogous to LipiTable's, so the existing LipiTable tests provide indirect coverage.

### §19.6 — Total LipiList file count

| Category | Count |
|---|---|
| Source files (component + internal sub-components + types) | 15 |
| Shared CSS | 1 |
| Demo files | 2 (already counted) |
| Test files | 3 |
| **Total LipiList-specific** | **~19 files** |

---

## §20 — Deploy script additions

Cross-reference LipiTable §28 for the deploy script conventions.

### §20.1 — Registry entries for LipiList

```powershell
# === Phase 2.8 — LipiList ===

# Source
@{ Source = "2.8-components-LipiList.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiList.razor" },
@{ Source = "2.8-components-LipiList.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiList.razor.cs" },
@{ Source = "2.8-components-LipiList.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiList.razor.css" },
@{ Source = "2.8-components-LipiListItem.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiListItem.razor" },
@{ Source = "2.8-components-LipiListItem.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiListItem.razor.css" },
@{ Source = "2.8-types-LipiListTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiListTypes.cs" },

# Internal sub-components
@{ Source = "2.8-components-internal-LipiListToolbar.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListToolbar.razor" },
@{ Source = "2.8-components-internal-LipiListToolbar.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListToolbar.razor.css" },
@{ Source = "2.8-components-internal-LipiListBulkActionBar.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListBulkActionBar.razor" },
@{ Source = "2.8-components-internal-LipiListBulkActionBar.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListBulkActionBar.razor.css" },
@{ Source = "2.8-components-internal-LipiListSkeletonItem.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListSkeletonItem.razor" },
@{ Source = "2.8-components-internal-LipiListSkeletonItem.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListSkeletonItem.razor.css" },
@{ Source = "2.8-components-internal-LipiListFooter.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListFooter.razor" },
@{ Source = "2.8-components-internal-LipiListFooter.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\Internal\LipiListFooter.razor.css" },

# CSS
@{ Source = "2.8-css-lipi-list-tokens.css"; Target = "src\LiPi.Web\wwwroot\css\lipi-list-tokens.css" },

# Tests
@{ Source = "2.8-test-LipiListTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiListTests.cs" },
@{ Source = "2.8-test-LipiListSelectionTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiListSelectionTests.cs" },
@{ Source = "2.8-test-LipiListPersistenceTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiListPersistenceTests.cs" },
```

### §20.2 — Build stage

LipiList drops in **Stage 7** of the LipiTable build order (per §27.17). After LipiTable + LipiPagination are ready, LipiList builds atop them.

### §20.3 — Modified files

`App.razor` needs the `lipi-list-tokens.css` `<link>` tag added (one line change; full file re-delivered per LiPi standing rule).

---

## §21 — Worked examples (summary)

### §21.1 — Notification feed (the canonical example)

```razor
<LipiList TItem="Notification"
          Items="@_notifications"
          KeySelector="@(n => n.Id)"
          TableId="notifications-list"
          
          SelectionMode="LipiListSelectionMode.Multi"
          @bind-SelectedItems="_selected"
          
          ItemStatus="@(n => n.IsRead ? "inactive" : "active")"
          
          QuickSearchMatcher="@((n, q) => 
              n.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
              n.Body.Contains(q, StringComparison.OrdinalIgnoreCase))"
          
          SortOptions="@_sortOptions"
          
          OnItemClick="@(n => Nav.NavigateTo($"/notifications/{n.Id}"))">
    
    <ItemTemplate Context="n">
        <div class="notification-row">
            <LipiAvatar Src="@n.SenderAvatar" />
            <div class="content">
                <div class="header-row">
                    <strong>@n.SenderName</strong>
                    <time>@n.Timestamp.Humanize()</time>
                </div>
                <h4>@n.Title</h4>
                <p>@n.Body</p>
            </div>
            <LipiBadge Variant="@CategoryToVariant(n.Category)">@n.Category</LipiBadge>
        </div>
    </ItemTemplate>
    
    <BulkActionTemplate>
        <LipiButton OnClick="@MarkSelectedAsRead">Mark as read</LipiButton>
        <LipiButton OnClick="@ArchiveSelected" Variant="ButtonVariant.Secondary">Archive</LipiButton>
    </BulkActionTemplate>
    
    <EmptyTemplate>
        <LipiEmptyState 
            Icon="bell-off"
            Title="No notifications"
            Body="You're all caught up." />
    </EmptyTemplate>
</LipiList>
```

The most common LipiList pattern. Covers: client-side data, multi-select with bulk actions, status strip per item, search + sort + density toggle, custom empty state.

### §21.2 — Server-side activity log with infinite scroll

```razor
<LipiList TItem="ActivityEvent"
          DataSource="@LoadActivityAsync"
          KeySelector="@(e => e.Id)"
          PaginationMode="LipiListPaginationMode.InfiniteScroll"
          Virtualize="VirtualizeMode.Always"
          ItemHeight="@null"
          Density="LipiListDensity.Compact"
          
          OnQueryComplete="@TrackPerformance">
    
    <ItemTemplate Context="e">
        <div class="activity-row">
            <div class="actor">@e.ActorName</div>
            <div class="action">@e.Action</div>
            <div class="target">@e.Target</div>
            <time>@e.Timestamp</time>
        </div>
    </ItemTemplate>
</LipiList>
```

Server-side activity log. Infinite scroll for continuous browsing. Compact density for data density.

### §21.3 — Search results with heterogeneous items

```razor
<LipiList TItem="SearchResult"
          Items="@_results"
          KeySelector="@(r => r.Id)"
          QuickSearchMatcher="@SearchMatcher"
          Separator="LipiListSeparator.Card">
    
    <ItemTemplate Context="r">
        @switch (r.Kind)
        {
            case ResultKind.Article:
                <ArticleResultCard Data="@((Article)r.Data)" />
                break;
            case ResultKind.Person:
                <PersonResultCard Data="@((Person)r.Data)" />
                break;
            case ResultKind.Document:
                <DocumentResultCard Data="@((Document)r.Data)" />
                break;
        }
    </ItemTemplate>
    
    <FilteredEmptyTemplate>
        <LipiEmptyState 
            Icon="search-x"
            Title="No matches"
            Body="Try different search terms." />
    </FilteredEmptyTemplate>
</LipiList>
```

Mixed result types in one list. Card separator for visual separation. Filtered-empty for no-search-results.

---

*End of LipiList spec. Proceed to LipiPagination spec body.*
