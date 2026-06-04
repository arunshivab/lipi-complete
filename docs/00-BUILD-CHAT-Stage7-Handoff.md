# BUILD CHAT — Stage 7 Handoff: LipiTable + LipiPagination Wiring

Copy this entire message into the build chat (next-slice context), along with the cumulative spec files already in the project knowledge.

---

## What's done (don't redo)

LipiTable Stages 1–6 are SHIPPED and runtime-verified by Arun. Cumulative A-numbers in the LiPi journal: A37 → A48. Specifically:

- **Stage 1 (Foundation)**: types, services, DB migration, shared CSS, JS interop — DONE
- **Stage 2 (Core)**: LipiTable + LipiColumn + LipiEmptyState + layout shells — DONE
- **Stage 3 (Rows/cells)**: row + cell sub-components — DONE
- **Stage 4 (Interactive)**: sort + filter + selection (4a/4b core + 4d keyboard) — DONE; **4c (across-pages banner + bulk bar + server keys) is INTENTIONALLY DEFERRED to after Stage 7**
- **Stage 5 (Advanced)**: inline edit + tree + master-detail + grouping — DONE
- **Stage 6 (Polish + persistence)**: ITablePreferenceService + density + column ops + column picker + export — DONE

**LipiPagination component itself: A50.1 SHIPPED.** Runtime-verified light + dark. The three-axis style API (`PaginationCellStyle` / `PaginationActiveStyle` / `PaginationChevronStyle`) is locked. StyleGuide demo done. Zero-config default = `Bordered` + `Solid` + `Auto`. HIS in-app look = `Borderless` + `Tint`.

## What this slice covers

**Stage 7 = wiring LipiPagination into LipiTable.** Not building LipiPagination (done); not rebuilding selection (done); not building Stage 4c banner (that's next). Just the integration layer between two already-shipped components.

Six design decisions locked in strategic chat after the Stage 7 handoff. They're in `06-FLAGS-LOG-Amendment-Stage7.md` as LP-15 (expanded) but reproduced here for in-context reference.

---

## The six locked decisions

### Q1 — Placement (with Left/Right amendment)

**Locked enum** (six values):

```csharp
public enum PaginationPlacement
{
    Bottom,    // default — region ⑧
    Top,       // region ④
    Both,      // top + bottom pair (Both does NOT include Left/Right)
    Left,      // ← NEW: side-anchored vertical pager
    Right,     // ← NEW: side-anchored vertical pager (mirror)
    None       // escape hatch; consumer composes externally via @bind
}
```

LipiTable owns rendering by default. `None` is the escape hatch.

**Left/Right placement has architectural implications.** A new `PaginationOrientation` parameter is added to LipiPagination:

```csharp
public enum PaginationOrientation { Horizontal, Vertical }   // default Horizontal
```

When `Placement` is `Left` or `Right` on LipiTable, the embedded LipiPagination renders with `Orientation="Vertical"` automatically.

**Variant compatibility with vertical orientation:**

| Variant | Vertical-compatible? |
|---|---|
| `Full` | ❌ (page-size + count + numbers + jump-to-page stacked vertically is visually too heavy) |
| `Compact` | ✓ |
| `Minimal` | ✓ |

**Auto-downgrade rule:** when `Placement` is `Left` or `Right` AND `Variant` is `Full`, LipiTable silently downgrades to `Compact` and logs a Dev-mode warning:

```
LipiTable: Variant=Full is not compatible with Placement=Left|Right.
Auto-downgrading to Compact. To suppress this warning, set Variant=Compact 
or Variant=Minimal explicitly.
```

Production: no warning; downgrade silent (graceful per A14 env-gated validation pattern).

**Page-size selector in side placement:**
- `PaginationOptions.ShowPageSize` defaults to `false` (dropdown too wide for narrow column)
- Recommended pattern: caller exposes a separate page-size control in LipiTable's toolbar (region ①)
- Consumer can explicitly override `ShowPageSize: true` if they accept the cramped layout

**Row count display in side placement:**
- Default compact format: `51-75 / 487` (slash separator, no "Showing" / "of")
- New token: `--lipi-pagination-count-format-compact`
- Consumer can override via `PaginationOptions.RowCountTemplate`

**Side pager column width:**

```csharp
[Parameter] public string PaginationSideWidth { get; set; } = "48px";   // 48px default fits ~36px square buttons + side padding
```

**Sticky behavior:** when `Placement` is `Left` or `Right` AND the table scrolls vertically, the side pager uses `position: sticky; top: 0`. CSS-only.

**Layout with system columns:**

```
┌─────┬───┬───┬─────────────────┐
│     │ ☐ │ ▸ │ Data columns... │
│ ‹   │   │   │                 │
│ 1   │ ☑ │ ▸ │                 │
│ 2   │ ☐ │ ▸ │                 │
│ 3   │ ☐ │ ▸ │                 │
│ …   │   │   │                 │
│ ›   │   │   │                 │
└─────┴───┴───┴─────────────────┘
  ↑     ↑   ↑
  side  sel exp
  pager col col
```

Side pager sits OUTSIDE the data area (to the left of selection / expand columns), not interspersed.

### Q2 — Mode (client vs server)

LOCKED per existing LipiTable §4 spec. Mode determined by which parameter the consumer sets:

- `Items` set → client mode (LipiTable slices in-memory: filter → sort → paginate)
- `DataSource` set → server mode (LipiTable calls back with `TableQueryRequest`, gets `TableQueryResponse<TItem>`)
- Both set → Dev-mode warning; `DataSource` wins (explicit beats implicit)
- Neither set → empty state

**Server contract requires `TotalCount`** (pagination math). `-1` allowed for "unknown total" with row-count-display caveat per §8.3.5.

**Loading state:** pager visible-but-disabled during DataSource in-flight. Don't hide it (causes layout shift).

**Implementation order:** client-side slicing first, then wire DataSource path. Both flow into the same internal `{currentPage, pageSize, totalCount}` state.

### Q3 — Selection × paging

LOCKED per LipiTable §5. Selection is `HashSet<object>` of row keys; navigation doesn't touch it.

**Pre-wire in Stage 7:**

1. Selection persistence across page nav (already works from 4a/4b; just verify after pagination wires in)
2. Header checkbox tri-state per CURRENT page:
   - All page rows selected → checked
   - Some page rows selected → indeterminate
   - No page rows selected → unchecked
3. Header checkbox click semantics (NOT a blind toggle):
   - Unchecked or indeterminate → select all rows on current page
   - Checked → deselect all rows on current page
   - Matches Gmail / GitHub / AG-Grid
4. Selection counter in bulk action bar shows total key count across all pages

**Defer to Stage 4c:**

- Across-pages banner UI ("All 25 on this page selected. Select all 247?")
- Across-pages flag mechanism (per §5.3.2)
- Server-keys API for resolving "all across pages"
- Bulk action bar "Across all pages" qualifier text

**The seam:** Stage 7's header-checkbox click fires internal event `OnAllOnPageSelected` that Stage 4c subscribes to for banner display. Stage 7 just defines the event; doesn't render the banner. **Don't refactor Stage 4a/4b selection code** — the key-set model already supports everything needed.

### Q4 — Keyboard

LOCKED:

| Key | Behavior |
|---|---|
| Plain PageDown / PageUp | UNCHANGED from Stage 4d: viewport-height cell navigation. Moves focus ~viewport-height rows; scrolls viewport. Does NOT change page. |
| **Alt+PageDown / Alt+PageUp** | Change pagination page (next / previous). Focus lands on first cell of first row on new page; column position preserved. |
| **Ctrl+Home** | Focus on first cell of first row of FIRST page (navigates pages if needed). |
| **Ctrl+End** | Focus on last cell of last row of LAST page (navigates pages if needed). |
| Pager button click (pointer) | Focus stays on clicked button. |
| Prev/next chevron click | Focus stays on chevron. |
| Jump-to-page input + Enter or Go | Focus stays on Go button. |

LipiTable subscribes to LipiPagination's `OnCurrentPageChanged` and switches on `PageChangeReason` enum (already in §8.11) to decide focus action.

### Q5 — Page-snap ownership

LOCKED clean separation:

- **LipiPagination owns the math**: when `TotalCount` or `PageSize` props change, clamps current page to `[1, totalPages]` and fires `OnCurrentPageChanged` with reason `PageSnap`. Already implemented in A50.
- **LipiTable owns the policy**: 
  - Sort change → page = 1
  - Filter change → page = 1
  - Page-size change → page = 1 (per LP-18; matches AntDesign; simpler than computing approximate-position based on first previously-visible item)
  
  LipiTable resets BEFORE re-fetching/re-rendering, then passes new `page=1` to LipiPagination.

### Q6 — Variant/style defaults when embedded

LOCKED — adopt AntDesign's pattern.

**LipiTable parameters added in Stage 7:**

```csharp
[Parameter] public PaginationPlacement Placement { get; set; } = PaginationPlacement.Bottom;
[Parameter] public PaginationVariant Variant { get; set; } = PaginationVariant.Full;
[Parameter] public PaginationOptions? PaginationOptions { get; set; }
[Parameter] public string PaginationSideWidth { get; set; } = "48px";
```

**`PaginationOptions` is a record bag with all null defaults** (null delegates to LipiPagination's own defaults):

```csharp
public sealed record PaginationOptions(
    PaginationCellStyle? CellStyle = null,
    PaginationActiveStyle? ActiveStyle = null,
    PaginationChevronStyle? ChevronStyle = null,
    int? Siblings = null,
    int? BoundaryCount = null,
    IReadOnlyList<int>? PageSizeOptions = null,
    int? DefaultPageSize = null,
    bool? ShowFirstLast = null,
    bool? ShowJumpToPage = null,
    bool? ShowPageSize = null,             // auto-overridden to false in Left/Right placement
    bool? ShowRowCount = null,
    Func<PaginationRange, string>? RowCountTemplate = null
);
```

**Static preset for HIS app:**

```csharp
public static class PaginationOptionsPresets
{
    public static readonly PaginationOptions LipiHisDefault = new(
        CellStyle: PaginationCellStyle.Borderless,
        ActiveStyle: PaginationActiveStyle.Tint,
        ChevronStyle: PaginationChevronStyle.Auto);
}
```

**Embedded default = same as LipiPagination's standalone default** (Bordered + Solid + Auto). LipiTable is in `LiPi.Components.*` (redistributable); the component isolation contract (§25) prohibits baking HIS-specific visual decisions. HIS app explicitly opts into `PaginationOptionsPresets.LipiHisDefault`.

**Consumer usage examples:**

```razor
<!-- Zero-config -->
<LipiTable TItem="..." ... />

<!-- HIS in-app look -->
<LipiTable TItem="..." 
           PaginationOptions="@PaginationOptionsPresets.LipiHisDefault" ... />

<!-- Side placement -->
<LipiTable TItem="..." 
           Placement="PaginationPlacement.Left"
           PaginationSideWidth="56px" ... />

<!-- Custom -->
<LipiTable TItem="..." 
           PaginationOptions="@(new PaginationOptions(
               CellStyle: PaginationCellStyle.Borderless,
               ShowJumpToPage: true,
               DefaultPageSize: 50))" ... />
```

---

## Operational patterns to apply going forward

Five patterns emerged from build chat catches LP-11 through LP-14 + the A50.1 lint episode. They're now permanent operating procedure — apply without asking:

### Chrome-vs-action principle (LP-12, LP-13)

For component-internal elements:

- **Chrome** (tight, custom-styled, fixed semantics, internal to a parent component): plain HTML with token styling.
- **Action** (consumer-facing actions, labeled, general-purpose): composed Lipi components.

Apply to: page number buttons (plain `<button>`), prev/next chevrons (plain `<button>`), page-size dropdown (native `<select>`), sort indicator buttons (plain `<button>`), expand chevrons (plain `<button>`), column resize handles (`<div role="separator">`, not button), filter chip remove × buttons (plain `<button>`), selection checkboxes (plain `<input type="checkbox">`).

LipiButton stays for: Go button in jump-to-page, LoadMore button, BulkActionTemplate slot content (consumer-controlled), any consumer-facing CTA.

Token-driven focus-ring consistency via shared `--lipi-focus-ring-*` foundation tokens — no component coupling needed.

### Pure-helper testing for algorithms (LP-14)

Any algorithm with >3 edge cases or >2 inputs:

1. Extract to a pure static method or pure record type under `LiPi.Components.Internal.Algorithms` namespace
2. Unit-test against documented examples
3. Then wire into Blazor markup

Saves Blazor compile cycles (~30s) per debug iteration; catches spec ambiguity early. **Add to stage-delivery CHANGE-LOG entries: which algorithms you extracted and how many test cases each has.** Phase 2.10 audit checks for this.

Relevant for Stage 7: nothing major — pagination's algorithm is already extracted (LP-14). But the page-snap clamp logic (Q5) is one place to apply this if you're not already.

### CSS comment/brace lint (CL-1)

Every CSS file must pass a comment/brace balance check before delivery. Silent rule-discard from glob-text-in-comment (`lipi-*/` confuses the parser) or unterminated multi-token (`--color-*/--r-*/--sp-*` treated as a comment that never closes) is the failure mode. The A50.1 staleness hunt cost real time on this.

Implementation: regex check or `postcss` config or whatever — build chat decides. Add to pre-delivery checklist and Phase 2.10 audit CSS scan.

### File-count judgment (LP-11)

§27 file lists are suggestions, not contracts. Apply the three-criterion test before creating sub-components:

- Is this independently reusable by consuming pages? → keep separate
- Is this composed only inside the parent component AND short? → collapse into parent with internal branching
- Does this represent a distinct paradigm/mode? → keep separate

Don't ask permission to apply this. Log the collapse in your stage-delivery CHANGE-LOG. Caveat: LipiTable's sub-components (regions ⓪–⑨) are structurally justified due to distinct lifecycles, state, and a11y semantics — don't collapse those.

### Spec amendment flow (general)

When you find a spec defect (internal contradiction, drawn-by-hand inconsistency, missing case), surface it back with:

- The specific contradiction or gap (cite section numbers)
- Your proposed resolution with industry-norm references
- A test verification table if it's algorithmic

Strategic chat will lock with a flag entry. Examples: LP-0 (variant naming), LP-12 (`<select>`), LP-13 (button), LP-14 (algorithm + API rename), LP-15 (the Q1–Q6 batch with Q1 amendment).

---

## Stage 7 task breakdown (suggested sequence)

Pick your slice; this is one viable ordering:

1. **Add LipiTable parameters**: `Placement`, `Variant`, `PaginationOptions`, `PaginationSideWidth`. Add `PaginationOptionsPresets` static class with `LipiHisDefault`.
2. **Client-mode slicing**: when `Items` is set, internal `{currentPage, pageSize, totalCount}` state slices the filtered+sorted Items array. Drives bottom-pager rendering.
3. **Bottom placement rendering**: LipiTable renders LipiPagination in region ⑧ when `Placement=Bottom`. Two-way binding on current page + page size. Verify visually against existing 6-stage demo data.
4. **Selection × pagination wiring**: header checkbox tri-state per current page (LP-16). Click semantics. `OnAllOnPageSelected` event hook for Stage 4c (LP-17). Verify selection persists across page nav (LP-3 / existing 4a/4b model).
5. **Server-mode wiring**: when `DataSource` is set, pagination state drives `TableQueryRequest`. Loading state shows pager visible-but-disabled. Verify `PageSnap` reason fires when current page exceeds new total.
6. **Page-reset policies (Q5)**: sort change → page=1, filter change → page=1, page-size change → page=1. Implement in LipiTable; pass new page=1 to LipiPagination.
7. **Keyboard wiring (Q4)**: Alt+PageDown/Up, Ctrl+Home/End. Focus landing rules per LP-19. Coordinate with existing Stage 4d roving-tabindex.
8. **Other placements**: Top, Both, None. Verify Both renders two pagers with shared state. Verify None doesn't render a pager but exposes `@bind-CurrentPage` / `@bind-PageSize`.
9. **Left/Right placement + Orientation parameter on LipiPagination**: add `PaginationOrientation` parameter to LipiPagination. Auto-downgrade Full → Compact with Dev warning. Side-pager column with `PaginationSideWidth`. Compact row count format. Sticky positioning.
10. **Tests**: unit tests for page-snap policy. Component tests for placement combinations. Keyboard interaction tests for Alt+PageDown/Up and Ctrl+Home/End.

Each step is independently verifiable. Stop and surface back if any reveal a wrinkle the spec didn't anticipate.

---

## What comes after Stage 7

The roadmap (per LiPi journal):

- **After Stage 7**: LipiList wiring (compose LipiPagination similarly)
- **Then Stage 4c**: across-pages banner + bulk action bar + server keys API
- **Then Phase 2.9**: Navigation (LipiBreadcrumb, etc.)
- **Then Phase 2.10**: Infrastructure audit (5 isolation scans + 20 audit queue items including the new Q-16 to Q-20)

LipiList's pagination wiring will reuse most of Stage 7's work — same `PaginationOptions` bag, same placement enum, same Orientation. LipiList §7 already specifies this composition. The seam between Stage 7 (LipiTable) and the LipiList work is mostly mechanical: apply the same wiring patterns with `LipiList`'s simpler API surface.

---

## Cumulative flags reference (LP-11 through LP-19 + CL-1 + DOC-1 + LP-A50)

Full entries in `06-FLAGS-LOG-Amendment-Stage7.md`. Quick reference:

| ID | Topic | Disposition |
|---|---|---|
| LP-11 | File-count amendment — variants collapsed into single LipiPagination.razor | 🟢 Locked |
| LP-12 | Native `<select>` for page-size, not LipiSelect | 🟢 Locked |
| LP-13 | Plain `<button>` for pager chrome, not LipiButton | 🟢 Locked |
| LP-14 | Algorithm + Siblings/BoundaryCount API | 🟢 Locked |
| LP-15 | Stage 7 six-decision batch (Q1–Q6 with Q1 Left/Right amendment) | 🟢 Locked |
| LP-16 | Header checkbox tri-state per current page | 🟢 Locked |
| LP-17 | Stage 7/4c seam via OnAllOnPageSelected event hook | 🟢 Locked |
| LP-18 | Page-size change → reset to page 1 | 🟢 Locked |
| LP-19 | Focus landing rules after page change | 🟢 Locked |
| CL-1 | CSS comment/brace lint standing rule | 🟡 Standing rule |
| DOC-1 | LipiPagination §3.3 prose says 10; shipped code says 5 (correct) | 🟢 Locked (spec correction at consolidation) |
| LP-A50 | Three-axis style decomposition (memorialization) | 🟢 Locked |

---

## How we'll work together

Same model as the Stage 4 through Stage 6 cycles:

- **Don't summarize the spec back to me at the start.** I wrote it; I know what it says. Confirm you've read this handoff and start.
- **Drop files in `Downloads\LiPi\` with the existing naming convention** from §28.3. I'll run `deploy-downloads.ps1` to deploy.
- **Per-stage delivery format**: source files + PowerShell registry snippet + CHANGE-LOG entry + deploy checklist.
- **Apply the operational patterns without asking permission**: chrome-vs-action calls, pure-helper extraction for algorithms, file-count judgment via three-criterion test, CSS lint before delivery.
- **Surface deviations immediately.** If something in the locked decisions reveals a wrinkle in implementation reality, surface it back the same way LP-11 → LP-14 emerged. The six Q-decisions are standards-first defaults; deviating with concrete reasons is fine — we'll lock with a new flag entry.
- **Reserve clarifying questions for**: genuine spec contradictions, decisions that affect public API surface (parameter additions/removals), LipiTable's complex sub-component decisions (where decomposition IS structurally justified), cross-component patterns not yet documented.

---

## Starting prompt

After reading this handoff, respond with:

1. Confirmation that you've internalized the six locked decisions (especially the Q1 Left/Right amendment with `PaginationOrientation` parameter and auto-downgrade rules) and the five operational patterns.
2. Any clarifying questions BEFORE starting implementation.
3. Your proposed Stage 7 delivery sequence (or confirmation that the suggested 10-step sequence above is your starting plan, with any reordering you'd prefer).

Then wait for go-ahead before generating code.

Ready when you are.
