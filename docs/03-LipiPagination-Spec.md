# LipiPagination Specification

**Phase:** 2.8 — Data Display
**Component:** `LipiPagination` + supporting sub-components
**Status:** Spec body for build — locked
**Composed by:** LipiTable footer, LipiList footer, and any consuming page wanting consistent pagination UX
**Companion to:** LipiTable §8 (pagination integration), LipiList §7

---

## §0 — Naming reconciliation (CRITICAL READ FOR BUILD CHAT)

During spec drafting, three sources used different variant names for the same three visual layouts. Reconciled here:

| Concept | LipiTable §8 used | LipiList §7 used | LipiPagination outline | **Locked name** |
|---|---|---|---|---|
| Full pager with size + count + jump | "Standard" | "Standard" | "Full" | **`Full`** |
| Page X of M + size dropdown | "Compact" | "Compact" | "Compact" | **`Compact`** |
| Prev / Next only | "Mini" | "Mini" | "Minimal" | **`Minimal`** |

LipiTable §8 and LipiList §7 file names referenced `LipiPaginationCompact`, `LipiPaginationMini`, `LipiPaginationLoadMore`. These ALSO need updating per this reconciliation:

| File reference in LipiTable §8 | **Corrected name** |
|---|---|
| `LipiPaginationCompact.razor` | `LipiPaginationCompact.razor` (unchanged) |
| `LipiPaginationMini.razor` | `LipiPaginationMinimal.razor` |
| `LipiPaginationLoadMore.razor` | `LipiPaginationLoadMore.razor` (unchanged; this is a different mode, not a variant) |

**Build chat: when implementing §27 file paths, use `LipiPaginationMinimal.razor` instead of `LipiPaginationMini.razor`. Update the deploy script entry accordingly.** Flagged in `06-FLAGS-LOG.md` as item LP-0.

Additionally, `LoadMore` is a pagination **mode** (a navigation paradigm), not a **variant** (a visual layout). The mode is owned by LipiTable / LipiList; LipiPagination provides the LoadMore button as a sibling sub-component but doesn't expose it via the `Variant` enum.

---

## §1 — Overview and design principles

### §1.1 — What LipiPagination is

LipiPagination is the standalone page-navigation control. It renders:
- A page-size selector dropdown
- A row count display ("Showing X-Y of Z")
- Page-number buttons with prev/next/first/last
- A jump-to-page input

Each zone is opt-in. The component is composed into LipiTable / LipiList footers and is also a public standalone component for consuming pages that want pagination UX outside data displays (image galleries, card grids, search results pages, etc.).

### §1.2 — Single source of truth

LipiTable / LipiList do NOT reimplement pagination UI internally. They compose LipiPagination. This keeps the pagination UX consistent across the component library — fixing a bug or improving a feature happens in one place.

For LipiTable / LipiList, integration is in their footer (region ⑧ per LipiTable §2.1).

### §1.3 — Out of scope for v1.0

- **Cursor-based pagination** (next-token / prev-token API instead of offset+limit) — current model is offset-based. Cursor pagination is a different paradigm; deferred to a future amendment when needed.
- **Infinite scroll** — handled directly by LipiTable / LipiList (per LipiTable §8.5.2). LipiPagination doesn't render scroll-detection logic.
- **Jump to first / last row** — only first / last page. Per-row jumping would need item context LipiPagination doesn't have.

### §1.4 — Design principles inherited from LipiTable §1.3

Same six principles:
1. Component isolation (no consumer coupling)
2. Performance (no unnecessary renders)
3. Clinical safety (no destructive page navigation without user intent)
4. Token-driven theming (mode swaps via foundation)
5. Accessibility first (WCAG AA non-negotiable)
6. Caller-controlled (variant + zones opt-in; LipiPagination doesn't decide)

---

## §2 — Visual variants

### §2.1 — `PaginationVariant` enum

```csharp
public enum PaginationVariant
{
    Full,       // page-size + count + page numbers + first/last + jump-to-page
    Compact,    // prev/next + "Page X of M" + page-size dropdown
    Minimal     // prev/next only
}
```

### §2.2 — Full variant (default)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  Rows per page: [25 ▾]    Showing 51-75 of 487    [◂ 1 … 3 4 5 6 7 … 20 ▸]   Page [3] of 20 [Go]  │
└──────────────────────────────────────────────────────────────────────────────┘
   ↑ size dropdown            ↑ row count display       ↑ page number buttons              ↑ jump-to-page input
```

Five zones, all opt-in. Default visibility depends on viewport width — narrow toolbars auto-collapse less-essential zones (similar to LipiTable §16.2.3).

### §2.3 — Compact variant

```
┌─────────────────────────────────────────────┐
│  [25 ▾]    ◂ Page 3 of 20 ▸                 │
└─────────────────────────────────────────────┘
```

Minimal zones: page-size selector + prev/next with page indicator. Used by:
- Drawer-embedded tables (narrow horizontal space)
- Card-style data displays where the chrome must be compact
- Mobile / tablet viewports (when the parent table allows compact pagination)

### §2.4 — Minimal variant

```
┌─────────────────────┐
│    ◂   ▸            │
└─────────────────────┘
```

Just prev / next chevron buttons. No counts, no indicator, no page-size selector.

Used by:
- Inline pagination inside cards (where space is at a premium)
- Sub-views inside drawers / modals
- "Pinned" navigation alongside the data (e.g., a vertical sidebar with prev/next buttons)

### §2.5 — Layout zones in Full variant

Each zone is independently controllable:

```csharp
[Parameter] public bool ShowPageSize { get; set; } = true;       // dropdown left
[Parameter] public bool ShowRowCount { get; set; } = true;       // "Showing X-Y of Z"
[Parameter] public bool ShowPageNav { get; set; } = true;        // page number buttons
[Parameter] public bool ShowFirstLast { get; set; } = true;      // ◂◂ ▸▸ buttons
[Parameter] public bool ShowJumpToPage { get; set; } = false;    // input + Go button (off by default — power user feature)
```

Setting all to false renders an empty container (no visible content). Useful for tables that opt out of pagination chrome entirely but still want LipiPagination's keyboard/state handling.

### §2.6 — Zone collapse on narrow widths

For variants embedded in narrow containers (< 600px), zones auto-collapse:

- < 600px: jump-to-page collapses (input takes too much room)
- < 480px: page-size selector collapses (variant feels like Compact)
- < 360px: visible page numbers reduce to current ± 1 with ellipses

The container's actual width drives this, not the viewport. Use `LipiPagination` in a wide footer and all zones render; embed it in a narrow card and zones drop progressively.

Threshold parameters available for customization:

```csharp
[Parameter] public int CollapseJumpToPageBelowPx { get; set; } = 600;
[Parameter] public int CollapsePageSizeBelowPx { get; set; } = 480;
[Parameter] public int CollapseNumbersBelowPx { get; set; } = 360;
```

---

## §3 — API surface

### §3.1 — Required parameters

```csharp
[Parameter, EditorRequired] public int TotalCount { get; set; }
[Parameter, EditorRequired] public int PageSize { get; set; }
[Parameter, EditorRequired] public int CurrentPage { get; set; }
[Parameter] public EventCallback<int> CurrentPageChanged { get; set; }
[Parameter] public EventCallback<int> PageSizeChanged { get; set; }
```

Standard pattern with `[EditorRequired]` for the data values and `EventCallback<T>` for two-way binding.

Usage with two-way bindings:

```razor
<LipiPagination TotalCount="@_total"
                @bind-CurrentPage="@_page"
                @bind-PageSize="@_size" />
```

Or with explicit handlers:

```razor
<LipiPagination TotalCount="@_total"
                CurrentPage="@_page"
                PageSize="@_size"
                CurrentPageChanged="@HandlePageChanged"
                PageSizeChanged="@HandleSizeChanged" />
```

### §3.2 — Optional parameters

```csharp
[Parameter] public PaginationVariant Variant { get; set; } = PaginationVariant.Full;

[Parameter] public IReadOnlyList<PageSizeOption> PageSizeOptions { get; set; } = 
    DefaultPageSizeOptions;

[Parameter] public int MaxPageButtons { get; set; } = 5;

[Parameter] public bool ShowPageSize { get; set; } = true;
[Parameter] public bool ShowRowCount { get; set; } = true;
[Parameter] public bool ShowPageNav { get; set; } = true;
[Parameter] public bool ShowFirstLast { get; set; } = true;
[Parameter] public bool ShowJumpToPage { get; set; } = false;

[Parameter] public int ServerSideAllCap { get; set; } = 1000;

[Parameter] public bool Disabled { get; set; } = false;

[Parameter] public string? AriaLabel { get; set; }      // override default "Pagination"

[Parameter] public string? Class { get; set; }
[Parameter] public string? Style { get; set; }
```

### §3.3 — `PageSizeOption` record

```csharp
public sealed record PageSizeOption(int Value, string DisplayLabel);
```

The label can differ from the value (e.g., `Value=int.MaxValue, DisplayLabel="All"`):

```csharp
public static readonly IReadOnlyList<PageSizeOption> DefaultPageSizeOptions = new[]
{
    new PageSizeOption(10, "10"),
    new PageSizeOption(25, "25"),
    new PageSizeOption(50, "50"),
    new PageSizeOption(100, "100"),
    new PageSizeOption(200, "200"),
    new PageSizeOption(int.MaxValue, "All")
};
```

Callers can supply custom options:

```razor
<LipiPagination PageSizeOptions="@(new[] { 
    new PageSizeOption(20, "20"),
    new PageSizeOption(50, "50") 
})" ...>
```

### §3.4 — "All" handling

"All" in the dropdown maps to `int.MaxValue`. Behavior:

- **Standalone use**: when "All" selected, `PageSizeChanged(int.MaxValue)` fires. Consuming page handles however it wants.
- **Inside LipiTable / LipiList**: parent caps at `ServerSideAllCap` (default 1000) in server-side mode per LipiTable §8.2.2. LipiPagination itself doesn't enforce the cap — the parent renders the cap banner.

When `PageSize = int.MaxValue` (All), the row count display reads "Showing all N rows" instead of "Showing 1-N of N".

### §3.5 — Page number rendering algorithm

When `ShowPageNav="true"` and `Variant="Full"`:

1. Compute `totalPages = ceil(TotalCount / PageSize)` (or 1 when PageSize is MaxValue and TotalCount > 0).
2. If `totalPages <= MaxPageButtons`: render all pages as buttons (1 to N).
3. Otherwise: always render page 1, page N, and `MaxPageButtons - 2` pages around CurrentPage; insert ellipsis (`…`) between non-contiguous ranges.

Example for `MaxPageButtons=5`, `currentPage=10`, `totalPages=20`:

```
[1] [...] [9] [10] [11] [...] [20]
```

Example for `MaxPageButtons=7`, `currentPage=10`, `totalPages=20`:

```
[1] [...] [8] [9] [10] [11] [12] [...] [20]
```

Example for `MaxPageButtons=5`, `currentPage=3`, `totalPages=20`:

```
[1] [2] [3] [4] [5] [...] [20]
```

(Current page is near start; no ellipsis on the left side.)

Edge cases:
- `totalPages == 0` (no data) → only "no pages" placeholder text, no buttons
- `totalPages == 1` → only page 1 button; prev/next disabled
- `currentPage > totalPages` (after data shrinks) → silently snap to `totalPages` and fire `CurrentPageChanged` with the corrected value

### §3.6 — Programmatic API

```csharp
public ValueTask GoToPageAsync(int page);
public ValueTask GoToFirstAsync();
public ValueTask GoToLastAsync();
public ValueTask GoToNextAsync();
public ValueTask GoToPreviousAsync();
public ValueTask SetPageSizeAsync(int size);
```

Each fires the corresponding `*Changed` event. Useful for keyboard shortcuts at the page level (Ctrl+Right = next page, etc.) or for navigation flows that span the table.

### §3.7 — Imperative re-render

LipiPagination is purely a function of its parameters — same inputs always produce same output. If the consuming page mutates state that LipiPagination depends on (rare), call:

```csharp
public ValueTask RefreshAsync();
```

Forces a re-render. Rarely needed in normal use.

---

## §4 — States

### §4.1 — Disabled state

```razor
<LipiPagination Disabled="true" ... />
```

All controls non-interactive:
- Page number buttons: muted text + `cursor: not-allowed`
- Page-size dropdown: greyed out
- Jump-to-page input: disabled
- Prev/next/first/last buttons: muted

Used during data loading (per LipiTable §18.3.5 — pagination stays visible but disabled during loading).

### §4.2 — Loading state (parent-driven)

LipiPagination itself has no loading state. The parent (LipiTable / LipiList) sets `Disabled="true"` during their own loading state.

For standalone use without a parent component, the consuming page does the same.

### §4.3 — Empty state

When `TotalCount == 0`:
- All page number buttons hidden
- Prev/next disabled
- Row count display reads "No items" (or empty when `ShowRowCount="false"`)
- Page-size selector still visible (caller may want it visible for visual stability)

This matches the FilteredEmpty / Empty body states in LipiTable §18 — pagination stays in the layout but reflects emptiness.

### §4.4 — Single-page state

When `totalPages == 1`:
- Page 1 button rendered as active (`aria-current="page"`)
- Prev/next/first/last buttons disabled
- Row count display reads "Showing 1-N of N" (or "Showing all N rows" if PageSize == int.MaxValue)
- Page-size selector visible

### §4.5 — Currently-on-page-1 state

- First button disabled (already at first)
- Prev button disabled (no previous page)
- Next button enabled (if totalPages > 1)
- Last button enabled (if totalPages > 1)

### §4.6 — Currently-on-last-page state

Mirror of §4.5:
- Next button disabled
- Last button disabled
- First button enabled
- Prev button enabled

---

## §5 — Accessibility

### §5.1 — ARIA structure

```html
<nav role="navigation" aria-label="Pagination">
    <div class="lipi-pagination-size">
        <label for="page-size-{id}">Rows per page:</label>
        <select id="page-size-{id}" aria-label="Rows per page">...</select>
    </div>
    
    <div class="lipi-pagination-count" aria-live="polite">
        Showing 51-75 of 487
    </div>
    
    <ul class="lipi-pagination-nav" role="list">
        <li><button aria-label="First page" disabled>«</button></li>
        <li><button aria-label="Previous page" disabled>‹</button></li>
        <li><button aria-label="Page 1" aria-current="page">1</button></li>
        <li><span aria-hidden="true">…</span></li>
        <li><button aria-label="Page 3">3</button></li>
        ...
        <li><button aria-label="Next page">›</button></li>
        <li><button aria-label="Last page">»</button></li>
    </ul>
    
    <div class="lipi-pagination-jump">
        <label for="jump-page-{id}">Page</label>
        <input id="jump-page-{id}" type="number" aria-label="Jump to page" />
        <span>of 20</span>
        <button>Go</button>
    </div>
</nav>
```

Key elements:
- `role="navigation"` on the outer container — screen readers announce as "Pagination, navigation"
- `aria-current="page"` on the active page button (only one at a time)
- Ellipsis (`…`) span has `aria-hidden="true"` (decorative)
- Each button has a descriptive `aria-label` ("Next page" not just "›")
- Row count display has `aria-live="polite"` so changes are announced

### §5.2 — Keyboard navigation

| Key | Effect |
|---|---|
| Tab | Move through the controls (size → count → nav buttons → jump input → Go) |
| Shift+Tab | Reverse |
| Enter (on page button) | Navigate to that page |
| Enter (in jump-to input) | Trigger Go |
| Arrow Left (on page button) | Move focus to previous page button |
| Arrow Right (on page button) | Move focus to next page button |
| Home (on page button) | Move focus to first page button |
| End (on page button) | Move focus to last page button |

Tab moves between control groups; arrow keys move within a control group. Standard pattern.

### §5.3 — Screen reader announcements

When the user changes page:
- Page button is activated
- `CurrentPage` updates
- Row count display updates (e.g., "Showing 26-50 of 487")
- Screen reader announces the count change (via aria-live polite)

When the user changes page size:
- Page may reset to 1 (parent's responsibility)
- Row count updates
- Announced via aria-live

For high-frequency interactions (e.g., user rapidly clicking next/prev), announcements debounce 100ms per LipiTable §19.5.10.

### §5.4 — Focus management on page change

When the user clicks a page button:
- The page changes
- Focus stays on the clicked button
- The button's `aria-current` updates to "page"
- The previously-current button removes `aria-current`

This avoids "where did focus go?" confusion. The clicked button becomes the active button — focus and visual state match.

When the user clicks "Next" or "Prev":
- Page changes
- Focus stays on the Next / Prev button (which itself may become disabled if user reached last/first page)
- If focus would land on a now-disabled button, focus moves to the matching page number button instead

### §5.5 — Color and contrast

Same as LipiTable §19.6. Active page button uses primary background + on-primary text (contrast ratio meets AA). Disabled buttons use muted-text + transparent background.

### §5.6 — Reduced motion

Per LipiTable §19.7. Page number button hover transitions disabled. Ellipsis-to-button transitions (when range shifts) instant.

---

## §6 — Composition with parent components

### §6.1 — Used by LipiTable

LipiTable's footer composes LipiPagination:

```razor
@if (PaginationMode != PaginationMode.None && _totalCount > 0)
{
    <LipiPagination Variant="@MapVariant(PaginationVariant)"
                    TotalCount="@_totalCount"
                    @bind-CurrentPage="@_currentPage"
                    @bind-PageSize="@_pageSize"
                    PageSizeOptions="@PageSizeOptions"
                    ServerSideAllCap="@ServerSideAllCap"
                    Disabled="@(_loadingState == LoadingState.Loading)"
                    OnPageChanged="@HandleChildPageChanged" />
}
```

LipiTable's `PaginationVariant` parameter maps to LipiPagination's `Variant` enum.

When the parent is in loading state, LipiPagination is disabled. When the parent's data refreshes, LipiPagination's TotalCount updates and page number buttons re-render.

### §6.2 — Used by LipiList

Identical pattern to LipiTable. LipiList's footer composes LipiPagination with the same parameter forwarding.

### §6.3 — Used standalone

```razor
<div class="my-gallery">
    @foreach (var image in _pagedImages)
    {
        <GalleryImage Data="@image" />
    }
</div>

<LipiPagination Variant="PaginationVariant.Compact"
                TotalCount="@_totalImages"
                @bind-CurrentPage="@_page"
                @bind-PageSize="@_size"
                ShowJumpToPage="false" />
```

The consuming page renders any data display they want; LipiPagination handles the paging UX. The page-changed event is the consuming page's responsibility to act on (re-fetch / re-render).

### §6.4 — Embedded in cards / drawers / modals

For pagination inside narrow containers, the variant matters:

```razor
<LipiCard>
    <h3>Recent activity</h3>
    <ul>...</ul>
    <LipiPagination Variant="PaginationVariant.Minimal" 
                    TotalCount="@_count"
                    @bind-CurrentPage="@_page"
                    @bind-PageSize="@_size" />
</LipiCard>
```

Minimal variant fits in tight spaces. Drawer-embedded data displays often use Compact.

### §6.5 — Multiple LipiPagination instances on one page

A page can have multiple LipiPagination instances — one per data display. Each is independent. No coordination needed; the components are stateless.

For tables with pagination at both top AND bottom (rare but valid), the consuming page renders LipiPagination twice with the same bindings. Both reflect the same state via `@bind-CurrentPage`.

---

## §7 — Multi-industry tokens

LipiPagination uses the same foundation tokens as LipiTable. Specific pagination-namespace tokens:

```css
:root {
    /* Button dimensions */
    --lipi-pagination-button-size-compact:        28px;
    --lipi-pagination-button-size-comfortable:    32px;   /* default */
    --lipi-pagination-button-size-spacious:       40px;
    --lipi-pagination-button-size:                var(--lipi-pagination-button-size-comfortable);
    
    /* Button padding (for text buttons like page numbers) */
    --lipi-pagination-button-pad-x:               8px;
    
    /* Colors */
    --lipi-pagination-bg-button:                  transparent;
    --lipi-pagination-bg-button-hover:            var(--color-primary-50);
    --lipi-pagination-bg-button-active:           var(--color-primary-500);
    --lipi-pagination-bg-button-disabled:         transparent;
    
    --lipi-pagination-text-button:                var(--color-text-primary);
    --lipi-pagination-text-button-hover:          var(--color-text-primary);
    --lipi-pagination-text-button-active:         var(--color-on-primary);
    --lipi-pagination-text-button-disabled:       var(--color-text-faint);
    
    /* Border-radius */
    --lipi-pagination-button-radius:              var(--r-2);
    
    /* Spacing between buttons */
    --lipi-pagination-button-gap:                 2px;
    
    /* Zone spacing */
    --lipi-pagination-zone-gap:                   16px;
    
    /* Focus ring */
    --lipi-pagination-focus-ring-w:               2px;
    --lipi-pagination-focus-ring-color:           var(--color-primary-500);
    
    /* Row count text */
    --lipi-pagination-text-count:                 var(--color-text-secondary);
    --lipi-pagination-text-count-size:            13px;
}
```

Lives in `wwwroot/css/lipi-pagination-tokens.css`. Same mode-swap mechanism as the other components.

### §7.1 — Density inheritance

When LipiPagination is composed inside LipiTable / LipiList, the parent's density propagates via a cascading parameter:

```csharp
[CascadingParameter] public TableDensity? ParentDensity { get; set; }
```

If `ParentDensity` is set, LipiPagination's button sizes match (compact → 28px buttons, etc.). Standalone use defaults to comfortable.

Caller can override via explicit parameter:

```csharp
[Parameter] public TableDensity? Density { get; set; }
```

When set, takes precedence over the cascaded value.

---

## §8 — StyleGuide additions

LipiPagination gets its own demo page at `/styleguide/data-display/lipi-pagination-standalone`:

### §8.1 — Demo content

Side-by-side three variants:

```
=== Full variant ===
[ Rows per page: 25 ▾ ]   Showing 51-75 of 487   [« ‹ 1 … 4 5 6 … 20 › »]   Page [3] of 20 [Go]

=== Compact variant ===
[ 25 ▾ ]   ‹ Page 3 of 20 ›

=== Minimal variant ===
‹ ›
```

Each is bound to the same `_currentPage` and `_pageSize` state. Changing one updates the others (shows the parameter consistency).

### §8.2 — Interactive toggles

The demo includes:
- A `LipiSegmented` for variant selection
- Toggles for each zone visibility (`ShowPageSize`, `ShowRowCount`, etc.)
- Page-size options input (caller can paste custom options)
- A "Disabled" toggle to show the disabled state
- A slider for `TotalCount` to simulate different data sizes

### §8.3 — A11y notes

- `role="navigation"` + `aria-label="Pagination"`
- `aria-current="page"` on active button
- Aria-live polite for count display
- Keyboard nav: Tab between groups, Arrow Left/Right within nav buttons

### §8.4 — Code snippet

The Code tab shows the canonical use case in three forms (Full / Compact / Minimal), each in <30 lines.

---

## §9 — Files to create

### §9.1 — Source files

```
src/LiPi.Components/DataDisplay/LipiPagination/
├── LipiPagination.razor
├── LipiPagination.razor.cs
├── LipiPagination.razor.css
├── LipiPaginationCompact.razor          (variant sub-component)
├── LipiPaginationCompact.razor.css
├── LipiPaginationMinimal.razor          (variant sub-component) — note: renamed from "Mini"
├── LipiPaginationMinimal.razor.css
├── LipiPaginationLoadMore.razor         (mode sub-component, NOT a variant)
├── LipiPaginationLoadMore.razor.css
├── LipiPaginationPageSize.razor         (page-size dropdown sub-component)
├── LipiPaginationPageSize.razor.css
├── LipiPaginationCountDisplay.razor     ("Showing X-Y of Z" sub-component)
├── LipiPaginationCountDisplay.razor.css
└── LipiPaginationTypes.cs               (PaginationVariant, PageSizeOption, supporting types)
```

Approximately **14 files**.

### §9.2 — Shared CSS

```
src/LiPi.Web/wwwroot/css/lipi-pagination-tokens.css
```

One new CSS file for pagination-specific tokens. Loaded via App.razor link tag.

### §9.3 — Demo files

```
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiPaginationStandaloneDemo.razor
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiPaginationStandaloneDemo.razor.cs
```

Already counted in LipiTable §27.13.

### §9.4 — Tests

```
test/LiPi.Components.Tests/DataDisplay/LipiPaginationTests.cs
test/LiPi.Components.Tests/DataDisplay/LipiPaginationAlgorithmTests.cs       (the page-number rendering algorithm)
```

Two test files. The algorithm tests are important — page-number rendering has many edge cases (totalPages=0, current near boundaries, MaxPageButtons variations).

### §9.5 — Total file count

| Category | Count |
|---|---|
| Source files | 14 |
| Shared CSS | 1 |
| Demo files | 2 (already counted) |
| Test files | 2 |
| **Total LipiPagination-specific** | **~17 files** |

---

## §10 — Deploy script additions

```powershell
# === Phase 2.8 — LipiPagination ===

# Main component
@{ Source = "2.8-components-LipiPagination.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPagination.razor" },
@{ Source = "2.8-components-LipiPagination.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPagination.razor.cs" },
@{ Source = "2.8-components-LipiPagination.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPagination.razor.css" },

# Variant sub-components
@{ Source = "2.8-components-LipiPaginationCompact.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationCompact.razor" },
@{ Source = "2.8-components-LipiPaginationCompact.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationCompact.razor.css" },
@{ Source = "2.8-components-LipiPaginationMinimal.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationMinimal.razor" },
@{ Source = "2.8-components-LipiPaginationMinimal.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationMinimal.razor.css" },

# Mode sub-components (separate from variants)
@{ Source = "2.8-components-LipiPaginationLoadMore.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationLoadMore.razor" },
@{ Source = "2.8-components-LipiPaginationLoadMore.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationLoadMore.razor.css" },

# Element sub-components
@{ Source = "2.8-components-LipiPaginationPageSize.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationPageSize.razor" },
@{ Source = "2.8-components-LipiPaginationPageSize.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationPageSize.razor.css" },
@{ Source = "2.8-components-LipiPaginationCountDisplay.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationCountDisplay.razor" },
@{ Source = "2.8-components-LipiPaginationCountDisplay.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationCountDisplay.razor.css" },

# Types
@{ Source = "2.8-types-LipiPaginationTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationTypes.cs" },

# CSS
@{ Source = "2.8-css-lipi-pagination-tokens.css"; Target = "src\LiPi.Web\wwwroot\css\lipi-pagination-tokens.css" },

# Tests
@{ Source = "2.8-test-LipiPaginationTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiPaginationTests.cs" },
@{ Source = "2.8-test-LipiPaginationAlgorithmTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiPaginationAlgorithmTests.cs" },
```

Build stage: **Stage 4** of LipiTable's build order (pagination is "interactive features" — comes after the core component and before advanced features).

Modified files: `App.razor` needs the `lipi-pagination-tokens.css` `<link>` tag.

---

## §11 — Component isolation contract

Cross-reference LipiTable §25. LipiPagination follows identical rules:

### §11.1 — Naming invariants

- Namespace: `LiPi.Components.DataDisplay.LipiPagination.*`
- CSS prefix: `lipi-pagination-*`
- Type names: `LipiPagination`, `PaginationVariant`, `PageSizeOption`, `LipiPaginationCountDisplay`, etc.

No HIS-specific terminology. The component is entirely generic.

### §11.2 — Dependency invariants

LipiPagination depends only on:
- `Microsoft.AspNetCore.Components.*`
- `Microsoft.Extensions.Logging.Abstractions`
- `LiPi.Components.Shared` (for `LipiButton` composition, `LipiSelect` for the page-size dropdown)
- `System.*`

It does NOT depend on:
- `LipiTable` source code (independence)
- `LipiList` source code (independence)
- Any HIS-specific types or services

This independence matters because LipiPagination is a public standalone component — consuming pages may use it without using LipiTable or LipiList. The component must work in isolation.

### §11.3 — CSS contract

All CSS uses tokens (`--color-*`, `--sp-*`, `--r-*`, `--lipi-pagination-*`). No hex literals, no px literals (other than 0, 1, 2). Same audit pattern as LipiTable §25.4.

### §11.4 — No state coupling

LipiPagination is a **stateless** component — its render is purely a function of its parameters. No internal state, no service injection beyond the optional `IJSRuntime` for focus management.

This means:
- Two LipiPagination instances bound to the same state stay in sync automatically
- LipiPagination can mount / unmount freely without state loss (state lives in the consuming page)
- Testing is straightforward (parameter inputs → DOM output assertions)

The exception: brief internal state for the jump-to-page input (the typed-but-not-yet-submitted value). This state is local to the input element and resets on unmount.

---

## §12 — Worked examples

### §12.1 — Standalone in a search results page

```razor
<div class="search-results">
    @foreach (var result in _pagedResults)
    {
        <SearchResultCard Data="@result" />
    }
</div>

<LipiPagination 
    Variant="PaginationVariant.Full"
    TotalCount="@_totalResults"
    @bind-CurrentPage="@_page"
    @bind-PageSize="@_size"
    PageSizeOptions="@(new[] { 
        new PageSizeOption(10, "10"),
        new PageSizeOption(20, "20"),
        new PageSizeOption(50, "50")
    })" />

@code {
    private List<SearchResult> _pagedResults = new();
    private int _totalResults;
    private int _page = 1;
    private int _size = 20;
    
    protected override async Task OnParametersSetAsync()
    {
        var skip = (_page - 1) * _size;
        var result = await _searchService.SearchAsync(_query, skip, _size);
        _pagedResults = result.Items;
        _totalResults = result.Total;
    }
}
```

Search results paginated with LipiPagination. Page or size change → `OnParametersSetAsync` re-fetches.

### §12.2 — Image gallery with compact pagination

```razor
<div class="image-grid">
    @foreach (var img in _pagedImages)
    {
        <img src="@img.Url" alt="@img.Caption" />
    }
</div>

<LipiPagination 
    Variant="PaginationVariant.Compact"
    TotalCount="@_totalImages"
    @bind-CurrentPage="@_page"
    @bind-PageSize="@_size" />
```

Compact variant for tight footer space below an image grid.

### §12.3 — Inline minimal pagination in a card

```razor
<LipiCard>
    <h3>Recent uploads</h3>
    <ul>
        @foreach (var u in _pagedUploads)
        {
            <li>@u.Name</li>
        }
    </ul>
    <div class="card-footer">
        <small>@_totalUploads uploads</small>
        <LipiPagination 
            Variant="PaginationVariant.Minimal"
            TotalCount="@_totalUploads"
            @bind-CurrentPage="@_page"
            PageSize="5" />
    </div>
</LipiCard>
```

Minimal variant (just prev/next) for in-card pagination. Caller's "small" tag provides the count info; LipiPagination just handles navigation.

### §12.4 — Two LipiPagination instances on one page (top + bottom)

```razor
<LipiPagination Variant="PaginationVariant.Compact"
                TotalCount="@_total"
                @bind-CurrentPage="@_page"
                @bind-PageSize="@_size"
                ShowPageSize="false" />

<div class="data-list">
    @foreach (var item in _pagedItems)
    {
        <DataItem Data="@item" />
    }
</div>

<LipiPagination Variant="PaginationVariant.Full"
                TotalCount="@_total"
                @bind-CurrentPage="@_page"
                @bind-PageSize="@_size" />
```

Two instances bound to the same state. Top uses Compact (without size selector since the bottom shows it); bottom uses Full. Both reflect the same current page.

---

*End of LipiPagination spec. Proceed to LipiEmptyState spec body.*
