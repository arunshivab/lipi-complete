# Phase 2.8 — Strategic Flags Log

**Purpose:** Every "flag" I raised during the strategic spec drafting, captured for build chat handoff and Phase 2.10 audit.

**Format:** Section → Flag ID → Topic → Disposition → Action required.

**Disposition codes:**
- 🟢 **Locked** — decision made and standing; no further action unless contradicted by build experience
- 🟡 **Build chat note** — needs explicit attention during implementation
- 🔵 **Phase 2.10 audit item** — needs verification before redesign begins
- 🔴 **Open** — requires Arun's input before build can proceed

---

## §4 — Data Sources

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 4-1 | `DistinctValueColumns` / `DistinctValues` amendment to request/response shape | 🟡 Build chat note | Integrate the §9.0 amendment into §4 file. Both response and request need the new field. |
| 4-2 | Caller override of distinct values via `StatusOptions` (already exists) + new `DistinctValuesForFilter` per-column param | 🟢 Locked | — |
| 4-3 | High-cardinality protection: server may omit columns from `DistinctValues` even when asked | 🟡 Build chat note | Document the fallback to `LipiCombobox` search-only mode |

---

## §5 — Selection

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 5-1 | Selection key set is the foundation; row state derived from set membership | 🟢 Locked | — |
| 5-2 | Select-all-across-pages uses flag mechanism + exception list (NOT enumerated keys) | 🟢 Locked | Build chat must implement flag-based selection correctly; `GetSelectedKeysAsync()` is async for cases where pending children need resolution |

---

## §6 — Sorting

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 6-1 | Three-state cycle (asc → desc → none) default; two-state opt-out via column | 🟢 Locked | — |
| 6-2 | Multi-sort via shift-click only | 🟢 Locked | — |
| 6-3 | Sort persistence debounced 300ms | 🟢 Locked | — |

---

## §7 — Filtering

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 7-1 | HeaderIcon default + Drawer opt-in per Q7.1 | 🟢 Locked | — |
| 7-2 | Set-filter distinct values needed — gap to §4 surfaced; resolved in §9.0 amendment | 🟡 Build chat note | Cross-section integration required |
| 7-3 | `PersistFilter="false"` per column for sensitive (PHI) values | 🔵 Phase 2.10 audit | Identify which consuming-page columns must opt out (PHI columns: Aadhaar, Mobile, etc.) |
| 7-4 | Filter chip remove = immediate persist (NOT debounced) | 🟢 Locked | — |

---

## §8 — Pagination

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 8-1 | Page size [5, 10, 25, 50, 100, 200, All] with All cap 1000 in server mode | 🟢 Locked | — |
| 8-2 | Page size persists; current page does NOT | 🟢 Locked | — |
| 8-3 | InfiniteScroll vs LoadMore modes per Q1.3 | 🟢 Locked | — |

---

## §9 — Grouping

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 9-1 | §4 amendment for `DistinctValueColumns` / `DistinctValues` is the authoritative spec | 🟡 Build chat note | Build chat updates §4 file when locked |
| 9-2 | Expand state does NOT persist across sessions (groups can change between visits) | 🟢 Locked | Aligns with §8.9 and §10.4.3 |
| 9-3 | 5-level grouping limit; beyond = dev-mode warning, first 5 honored | 🟢 Locked | — |
| 9-4 | `GroupHeaderSticky` defaults `false` (implementation complexity for sticky-with-multiple-other-stickies) | 🟢 Locked | Could revisit in v1.X if user feedback warrants |
| 9-5 | Lazy-load empty `Items` + `ItemCount > 0` pattern for large groups | 🟢 Locked | — |
| 9-6 | Group-change persistence should debounce 300ms (matches sort/filter pattern) | 🟡 Build chat note | Was not explicitly stated; build chat aligns implementation |

---

## §10 — Tree data

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 10-1 | `TreeSelectionCascade="None"` default (consistent with §5 key-set model) | 🟢 Locked | — |
| 10-2 | Pending-select-when-loaded flag for cascade with lazy children | 🟡 Build chat note | `GetSelectedKeysAsync()` MUST resolve pending children before returning; bulk-action handlers depend on this |
| 10-3 | `HasChildrenSelector` strongly recommended for server-side trees (avoids "tap chevron, fetch, empty, chevron disappears" UX) | 🟡 Build chat note | StyleGuide example should illustrate proper use |
| 10-4 | `TreeFilterMode.PreserveAncestors` default (matches Windows Explorer / VS Code) | 🟢 Locked | — |
| 10-5 | Cross-section persistence consistency: structural state persists, ephemeral state does NOT | 🟢 Locked | All sections aligned |

---

## §11 — Master-detail

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 11-1 | `OnBeforeRowCollapse` interceptor for dirty-detail-form discard | 🟢 Locked | — |
| 11-2 | `<DetailTemplate>` MUST be idempotent (no side effects) — virtualization unmount/remount will re-fire | 🟡 Build chat note | StyleGuide examples must demonstrate `OnRowExpand` for side effects, NOT in-template |
| 11-3 | Lazy-load detail data pattern (cached dict + OnRowExpand fetch) | 🟢 Locked | StyleGuide should include this canonical example |
| 11-4 | Detail content Tab seamlessly enters/exits detail area | 🟢 Locked | Build chat must implement Tab navigation correctly |
| 11-5 | Variable-height row support via Microsoft.Virtualize variable mode | 🟢 Locked | Known limitations accepted (§20.3.2) |

---

## §12 — Inline editing

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 12-1 | Force-save retry mechanism via `IsForceSave` flag in RowEditContext; caller MUST honor server-side | 🟡 Build chat note | Server-side endpoints must implement force-save semantics; without it, "Save anyway" creates infinite loop |
| 12-2 | Dirty-state snapshot via `ICloneable` if implemented, else reflection of `Field` selectors | 🟡 Build chat note | Complex types (nested objects, collections) may need caller-provided `RowSnapshotProvider`; document the gotcha; provide override hook in v1.1 if needed |
| 12-3 | `EnableBrowserUnloadGuard="false"` default (avoid duplicate-confirm UX) | 🟢 Locked | — |
| 12-4 | Add-new pseudo-row default at Top (Gmail/Notion/Linear pattern); `NewRowPlacement` configurable | 🟢 Locked | — |
| 12-5 | `Save & Add Another` button only when `StickyBottomBar` AND `ShowAddAnotherButton="true"` | 🟢 Locked | Inline placement doesn't have room |
| 12-6 | Force-save flag honoring is consuming-page contract; LipiTable can't enforce | 🔵 Phase 2.10 audit | Verify consuming pages with edit + concurrency wire force-save correctly |

---

## §13 — Column resize, reorder, pin

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 13-1 | 200ms / 8px drag threshold hardcoded (prevents click-on-sort accidentally triggering drag) | 🟢 Locked | Configurable per-table not in v1.0 |
| 13-2 | Implicit pin-on-drag-zone-crossing | 🟢 Locked | `LockPinState="true"` opt-out per column |
| 13-3 | Right-click context menu + keyboard equivalent (`Shift+F10`) for column ops | 🟡 Build chat note | Both must map to the same menu |
| 13-4 | Actions column convention pinned-right — documentation guidance, NOT auto-pin | 🟢 Locked | — |

---

## §14 — Density

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 14-1 | Per-table density persistence (each TableId has its own); app-wide preference is consuming-page concern | 🟢 Locked | — |
| 14-2 | Per-column `Density` override documented as power-user feature; recommended `<CellTemplate>` instead | 🟢 Locked | — |
| 14-3 | Header font constant 12px across all densities (header = scaffolding, doesn't scale) | 🟢 Locked | — |
| 14-4 | Density change = immediate persistence write (no debounce; infrequent action) | 🟢 Locked | — |

---

## §15 — Aggregation

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 15-1 | Selection-aware aggregates NOT built in (filtered-total is the footer); selection summary externally by consuming page | 🟢 Locked | — |
| 15-2 | Server-side coordination of aggregate function name implicit (client sends column key only); consuming page maps server logic | 🟢 Locked | — |
| 15-3 | `First` / `Last` aggregates sort-dependent — documented in per-type matrix | 🟢 Locked | — |
| 15-4 | Footer aggregates over filtered dataset (NOT current page) — matches Excel/Google Sheets/AG-Grid expectation | 🟢 Locked | — |
| 15-5 | `AggregateContext<TValue>.Extra` dictionary for server-supplied multi-value aggregates (mean/median/stddev) | 🟢 Locked | Documented power-user feature |

---

## §16 — Toolbar and chrome

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 16-1 | 600px toolbar auto-collapse threshold (configurable) | 🟢 Locked | — |
| 16-2 | Collapse priority: quick search > add > everything else | 🟢 Locked | — |
| 16-3 | Auto-refresh indicator two styles (Interval vs LastRefreshed); LastRefreshed has minor perf cost (per-sec update) | 🟢 Locked | Default Interval |
| 16-4 | `OnAddClick` vs `OnAddNew` distinction (non-inline vs inline add); both set = OnAddClick wins | 🟢 Locked | — |
| 16-5 | Toolbar collapses to zero height when empty; `ForceToolbarVisible="true"` opt-in for structural consistency | 🟢 Locked | — |

---

## §17 — Export

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 17-1 | Zero external libraries: CSV in-house, PDF stubbed until Phase 2.10 (LiPi PDF lib), Print via browser, Excel NOT in v1.0 | 🟢 Locked | — |
| 17-2 | PDF integration is Phase 2.10 dependency on LiPi PDF library | 🔵 Phase 2.10 audit | Verify PDF library integration when ready |
| 17-3 | Force-save → in this context, the export `OnBeforeExport` returning false enables background-job hand-off | 🟢 Locked | — |
| 17-4 | HIPAA PHI access logging via `OnAfterExport` is consuming-page responsibility | 🔵 Phase 2.10 audit | Verify clinical-data tables wire OnBeforeExport/OnAfterExport for PHI audit |
| 17-5 | Excel deferred to future amendment | 🟢 Locked | Queued in `03-DEFERRED-ITEMS.md` |

---

## §18 — Body states

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 18-1 | Default loading = skeleton rows (LipiSkeleton from Phase 2.7) — structural, not just spinner | 🟢 Locked | — |
| 18-2 | `ShowLoadingOverlayDuringFetch="false"` default — honest refresh principle | 🟢 Locked | — |
| 18-3 | `ClearOnError="false"` default — previous data stays visible behind dismissible error overlay | 🟢 Locked | Clinical-safety teams override to `true` |
| 18-4 | Empty CTA mirrors toolbar Add button (same handler, same label) | 🟢 Locked | — |
| 18-5 | FilteredEmpty CTA = "Clear filters" (generic — covers column filters AND quick search) | 🟢 Locked | — |
| 18-6 | aria-live="polite" for body states (NOT assertive) | 🟢 Locked | — |

---

## §19 — Accessibility

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 19-1 | `role="grid"` not `role="table"` (grid supports sort/filter/edit semantics) | 🟢 Locked | — |
| 19-2 | Cell-by-cell arrow navigation (grid model) | 🟢 Locked | — |
| 19-3 | aria-live debounce 100ms (prevents over-talking during rapid nav) | 🟢 Locked | — |
| 19-4 | Dual aria-live regions (polite + assertive); assertive for validation errors / conflicts only | 🟢 Locked | — |
| 19-5 | Touch targets 24/28/32px per density; AAA recommends 44px — tablet-primary surfaces should use Spacious | 🟡 Build chat note | Document in StyleGuide; recommend Spacious default for touch contexts |
| 19-6 | NO way to disable accessibility (built-in, always-on) | 🟢 Locked | Non-negotiable per Principle 5 |
| 19-7 | Faint text 3.2:1 contrast (meets AA large-text, not normal-text) — supplementary elements | 🔵 Phase 2.10 audit | Verify contrast in both light/dark mode + high-contrast mode |
| 19-8 | axe-core CI integration | 🔵 Phase 2.10 audit | Build chat configures; CI runs against StyleGuide demos |

---

## §20 — Virtualization

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 20-1 | 100-row threshold for auto-virtualization (Q1.2 locked) | 🟢 Locked | — |
| 20-2 | Variable-height virtualization via Microsoft.Virtualize variable mode — known limitations accepted | 🟢 Locked | Documented (scroll jitter, Ctrl+End overshoot, approximate scrollbar) |
| 20-3 | Edited row pinned in viewport during virtualized scroll | 🟡 Build chat note | Critical UX feature — implementation: track edited row's relative viewport position; on scroll, adjust to keep stable |
| 20-4 | `MaxLoadedRows` default `int.MaxValue` (unbounded); consuming pages with long-session concerns set explicit cap | 🟢 Locked | — |
| 20-5 | Performance budgets are targets, not SLAs | 🟢 Locked | — |
| 20-6 | Phase 2.10 audit runs 6 benchmark workloads | 🔵 Phase 2.10 audit | Workloads documented in §20.11.1; results in `test-results/2.8-perf-{date}.md` |

---

## §21 — Persistence

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 21-1 | Server-side per-user persistence in identity DB (Q10.2 locked) | 🟢 Locked | — |
| 21-2 | Synchronous-feeling read on mount (awaited in OnInitializedAsync) | 🟢 Locked | Slow service implementation delays first paint; default EF impl is fast (<15ms) |
| 21-3 | In-memory cache per circuit; cross-tab sync NOT built in | 🟢 Locked | `InvalidatePreferenceCacheAsync` API for apps that wire SignalR/BroadcastChannel themselves |
| 21-4 | Flush on unmount (forcibly writes pending changes in DisposeAsync) | 🟡 Build chat note | Critical for not losing user changes mid-debounce; test explicitly |
| 21-5 | Silent persistence failures (logged, no user toast) | 🟢 Locked | Persistence = infrastructure |
| 21-6 | `PersistFilter="false"` per-column opt-out for sensitive data | 🔵 Phase 2.10 audit | Identify which consuming-page columns need this; expected: Aadhaar, Mobile, free-text patient identifier fields |
| 21-7 | No audit logging by default for preferences; opt-in via `OnPersisted` event | 🟢 Locked | — |
| 21-8 | Encryption-at-rest inherits identity DB configuration | 🔵 Phase 2.10 audit | Verify identity DB encryption configured per LiPi deployment standard |
| 21-9 | Cache invalidation on user-account deletion via FK cascade | 🟢 Locked | DB schema includes ON DELETE CASCADE |

---

## §22 — Conditional row formatting

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 22-1 | Status taxonomy in `lipi-status-tokens.css` (shared infrastructure) | 🔵 Phase 2.10 audit | LipiCard + LipiAlert retrofit to use the same tokens |
| 22-2 | No per-cell `CellClass` / `CellStyle` selectors — use `<CellTemplate>` instead | 🟢 Locked | Avoids three overlapping mechanisms |
| 22-3 | RowStatus value live-reflects dirty edit state (color updates as user types) | 🟢 Locked | — |
| 22-4 | Hooks should be fast (sync, no allocations, no async work) | 🟡 Build chat note | StyleGuide examples demonstrate proper precomputation patterns |

---

## §23 — Events

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 23-1 | `OnQueryComplete` event — new in §23; fires after every DataSource call | 🟡 Build chat note | Verify it fires consistently across all 8 QueryCompleteReasons |
| 23-2 | `OnRowDoubleClick` suppresses default toggle when set; caller can re-invoke via table API | 🟢 Locked | — |
| 23-3 | Event ordering is deterministic (§23.11) — 5 documented sequences | 🟡 Build chat note | Build chat must implement precisely; Phase 2.10 audit tests sequences |
| 23-4 | Handler exception handling is silent (logged, no user toast) | 🟢 Locked | — |
| 23-5 | Two-way bindings limited to 5 (SelectedItems, Density, PageSize, Page, QuickSearchText) | 🟢 Locked | Richer types use event subscription |
| 23-6 | 40 total events | 🟢 Locked | Summary table is canonical navigation aid |

---

## §25 — Component isolation contract

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 25-1 | Phase 2.10 audit gate — five scans (source, CSS, JS, DI, manual) | 🔵 Phase 2.10 audit | Build chat implements scans; findings block deployment |
| 25-2 | `LiPi.Components.*` namespace is authoritative (NOT `LiPi.Web.*`) | 🟡 Build chat note | Refactor any LipiTable code currently under LiPi.Web to LiPi.Components |
| 25-3 | CSS `--lipi-table-*` tokens computed from foundation | 🟢 Locked | Mode swaps happen at foundation layer |
| 25-4 | JS interop minimal (4 functions), lazy-loaded ES6 module | 🟡 Build chat note | Build chat ensures lazy-load works with IJSRuntime.InvokeAsync |
| 25-5 | Stable v1.0 API — no parameter removals or incompatible type changes within v1.0 lifecycle | 🟢 Locked | — |
| 25-6 | Internal types are NOT public API | 🟡 Build chat note | Mark internal helpers with `internal` keyword OR `*.Internal.*` namespace |
| 25-7 | Standing rule added in `02-STANDING-RULES.md` (per `05-Standing-Rule-Addition-Library-Dependency-Posture.md`) | 🟢 Locked | — |

---

## §26 — StyleGuide additions

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 26-1 | Demo data is domain-agnostic (orders / projects / tasks / releases — NEVER clinical) | 🟡 Build chat note | Enforces isolation contract §25.2.1; any HIS terminology in demos fails audit |
| 26-2 | Stress demos have honest "may take 200-400ms" performance banner | 🟢 Locked | Sets expectations |
| 26-3 | Mobile preview banner directs to LipiList for <1024px viewports | 🟢 Locked | Lack of reflow is by design (per §2.6), not a bug |
| 26-4 | Spec cross-references in demos link to spec sections | 🟡 Build chat note | Build chat decides hosting (markdown in StyleGuide routes vs separate docs site); my lean: in StyleGuide for proximity |

---

## §27 — Files to create

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 27-1 | ~197 files total across all categories | 🟡 Build chat note | Largest single phase; staged delivery essential per §27.17 9-stage build order |
| 27-2 | `LiPi.Components` separate csproj from `LiPi.Web` | 🟡 Build chat note | Restructuring opportunity; `LiPi.Web` references `LiPi.Components` |
| 27-3 | Reuse `IDateFormatService` if exists from prior phases | 🟡 Build chat note | Don't duplicate; check existing codebase |
| 27-4 | Reuse `lipi-status-tokens.css` if exists from LipiCard/LipiAlert retrofit | 🟡 Build chat note | Shared infrastructure across Phase 2.8 components |
| 27-5 | Demos in `LiPi.Web` (NOT redistributable) | 🟢 Locked | Per isolation contract — component code is redistributable, demos are HIS-app-specific |
| 27-6 | 9-stage build order with verification gates per stage | 🟡 Build chat note | Each stage must verify before moving to next; broken-build stages stop and flag |
| 27-7 | SPEC comment header on every file | 🟡 Build chat note | Phase 2.10 audit grep-checks for compliance; templates provided per file type |

---

## §28 — Deploy script additions

| ID | Topic | Disposition | Action |
|---|---|---|---|
| 28-1 | Idempotent script — every run overwrites all registered files | 🟢 Locked | — |
| 28-2 | No duplicate `Source` keys allowed in registry | 🔵 Phase 2.10 audit | Verify in audit |
| 28-3 | Source naming convention with category prefixes for browseability | 🟢 Locked | `2.8-components-*`, `2.8-types-*`, `2.8-export-*`, etc. |
| 28-4 | Existing-file modifications delivered as full file replacements (NOT patches) | 🟢 Locked | Per LiPi standing rule "full files only, no patches" |
| 28-5 | Backup-before-overwrite NOT implemented in v1.0 | 🟡 Build chat note | Future enhancement deferred to v1.X amendment |
| 28-6 | Registry size after Phase 2.8: ~447 entries | 🟡 Build chat note | Optional split into per-phase deploy scripts; deferred — not required for Phase 2.8 to ship |
| 28-7 | CHANGE-LOG update accompanies each stage drop | 🟢 Locked | Per LiPi standing rule on CHANGE-LOG hygiene |
| 28-8 | Each stage drop is self-contained: files + registry snippet + CHANGE-LOG + deploy checklist | 🟢 Locked | Arun has everything needed per stage |

---

## Cross-cutting items (Phase 2.10 audit queue)

Items accumulated across sections that go into the Phase 2.10 audit queue:

| ID | Topic | Source section |
|---|---|---|
| Q-1 | Status taxonomy retrofit — LipiCard / LipiAlert to use `lipi-status-tokens.css` | §22 |
| Q-2 | `ITablePreferenceService` scope review — multi-tenant + GDPR/DPDP | §21 |
| Q-3 | HIPAA audit log scope for preferences | §21 |
| Q-4 | In-house PDF library integration confirmation | §17 |
| Q-5 | Standing rule formalization (component library isolation) | §25 |
| Q-6 | `PersistFilter="false"` identification for sensitive columns | §7, §21 |
| Q-7 | Performance benchmark workloads (6 scenarios per §20.11.1) | §20 |
| Q-8 | axe-core CI integration | §19 |
| Q-9 | Faint text contrast verification (light/dark/high-contrast) | §19 |
| Q-10 | RFC 4180 CSV exporter edge case battery | §17 |
| Q-11 | Encryption-at-rest verification for identity DB | §21 |
| Q-12 | Component isolation scans (source, CSS, JS, DI, manual) | §25 |
| Q-13 | Event ordering test sequences | §23 |
| Q-14 | Force-save server-side honoring verification | §12 |
| Q-15 | PHI export audit hook coverage on clinical tables | §17 |

---

## LipiList sibling spec

| ID | Topic | Disposition | Action |
|---|---|---|---|
| LL-1 | LipiList uses `role="list"` not `role="grid"` (no cells to navigate) | 🟢 Locked | — |
| LL-2 | Variable-height virtualization by default (caller's ItemTemplate is unknown shape) | 🟢 Locked | `ItemHeight` opt-in for known-uniform lists |
| LL-3 | LipiList reuses LipiTable's data source contracts (TableQueryRequest / TableQueryResponse) | 🟢 Locked | Callers can switch between LipiTable and LipiList without rewriting DataSource handler |
| LL-4 | LipiList has 16 events vs LipiTable's 40 (simpler surface for simpler component) | 🟢 Locked | — |
| LL-5 | NO inline editing in LipiList v1.0 (caller drives edit via modal/drawer/detail expand) | 🟢 Locked | Documented out-of-scope |
| LL-6 | NO tree / nested data in LipiList v1.0 (use LipiTable's tree mode) | 🟢 Locked | Documented out-of-scope |
| LL-7 | NO grouping / drag-reorder in LipiList v1.0 | 🟢 Locked | Deferred; add when first consumer needs it |
| LL-8 | LipiList shares persistence backend (ITablePreferenceService) with LipiTable | 🟢 Locked | Same identity DB table, subset of persisted fields |
| LL-9 | Three separator variants (Line / Spacing / Card) | 🟢 Locked | Card variant for dashboard-style summary lists |
| LL-10 | Default skeleton mimics generic notification shape; caller provides tailored `<LoadingTemplate>` for different shapes | 🟡 Build chat note | Build chat documents this in StyleGuide examples |
| LL-11 | ~19 files for LipiList (15 source + 1 CSS + 2 demo + 3 tests) | 🟢 Locked | Build chat Stage 7 |
| LL-12 | LipiList shares `lipi-table-interop.js` (no separate LipiList JS module needed) | 🟢 Locked | Build chat may rename for clarity; either path is fine |

---

## LipiPagination sibling spec

| ID | Topic | Disposition | Action |
|---|---|---|---|
| **LP-0** | **CRITICAL: Variant naming reconciliation across LipiTable §8, LipiList §7, LipiPagination spec** | 🟡 **Build chat note** | **Locked names: `Full` / `Compact` / `Minimal`. Replace any prior "Standard" with "Full" and "Mini" with "Minimal" in LipiTable §8 and LipiList §7 file references. Update file name `LipiPaginationMini.razor` → `LipiPaginationMinimal.razor` per §0 of LipiPagination spec.** |
| LP-1 | LoadMore is a pagination MODE, not a variant — `LipiPaginationLoadMore.razor` is a separate sub-component for that mode | 🟢 Locked | — |
| LP-2 | LipiPagination is stateless (parameters-only render) — same inputs produce same output | 🟢 Locked | Multi-instance two-way binding works automatically |
| LP-3 | "All" maps to `int.MaxValue` page size; server-side cap is parent's responsibility via `ServerSideAllCap` (default 1000) | 🟢 Locked | LipiPagination doesn't enforce cap; parent renders cap banner |
| LP-4 | Zone collapse on narrow containers (jump-to-page < 600px, page-size < 480px, numbers < 360px) | 🟢 Locked | Configurable via threshold parameters |
| LP-5 | Cursor-based pagination NOT in v1.0 (offset-based only) | 🟢 Locked | Deferred to future amendment |
| LP-6 | Multi-instance on one page supported via two-way binding (e.g., top + bottom pagers) | 🟢 Locked | — |
| LP-7 | Density inheritance via CascadingParameter; explicit `Density` parameter overrides | 🟢 Locked | Comfortable default for standalone use |
| LP-8 | Page-number rendering algorithm has many edge cases (totalPages=0, current near boundaries) | 🟡 Build chat note | Dedicated test file `LipiPaginationAlgorithmTests.cs` covers all edge cases |
| LP-9 | ~17 files for LipiPagination (14 source + 1 CSS + 2 tests; demos counted in LipiTable §27.13) | 🟢 Locked | Build chat Stage 4 |
| LP-10 | LipiPagination must work independently of LipiTable / LipiList — no source coupling | 🟢 Locked | Audit verification per LipiTable §25 |

---

## LipiEmptyState sibling spec

| ID | Topic | Disposition | Action |
|---|---|---|---|
| LE-1 | LipiEmptyState is a stateless presentational primitive (no service injection, no internal state, no JS interop) | 🟢 Locked | Same pattern as LipiPagination |
| LE-2 | `Title` is required; Dev-mode throws `InvalidOperationException` on empty Title; Production logs warning and renders no title element (graceful fallback per A14 env-gated validation) | 🟡 Build chat note | Aligns with LipiButton's hard-throw pattern and the deferred v1.1 retrofit queue |
| LE-3 | Six variants: Default / Empty / FilteredEmpty / Error / Success / Coming | 🟢 Locked | Default icon per variant: `inbox` / `inbox` / `search-x` / `alert-octagon` / `check-circle` / `clock` |
| LE-4 | Three sizes: Inline (~120px) / Card (~200px) / Page (~320px); icon sizes 32/48/64px | 🟢 Locked | Inline is default (most common — composed inside LipiTable/LipiList) |
| LE-5 | Heading level varies by size: Inline/Card use `<h3>`, Page uses `<h2>`; `TitleHeadingLevel` parameter for explicit override | 🟢 Locked | Accommodates consuming pages with non-standard heading hierarchies |
| LE-6 | Icon resolution order: IconTemplate > Icon > variant default > none | 🟢 Locked | Setting `Icon=""` explicitly opts out (no icon rendered) |
| LE-7 | Variant defaults are SUGGESTIONS, not hard rules — caller can override every default | 🟢 Locked | Documented in §3.6 reference table |
| LE-8 | LipiTable's `Empty*` parameters (EmptyTitle / EmptyBody / EmptyIcon / EmptyShowAddCta) map directly to LipiEmptyState parameters when composed internally | 🟢 Locked | Users configure via LipiTable parameters; never write LipiEmptyState markup directly inside LipiTable bodies |
| LE-9 | Smallest component — just 6 files (4 source + 1 CSS + 1 test; demos already counted in LipiTable §27.13) | 🟢 Locked | — |
| LE-10 | Build Stage 2 — needed early because LipiTable §18 body states compose LipiEmptyState from the start | 🟡 Build chat note | Build chat order: types/services/CSS/JS → core skeleton → **LipiEmptyState** → row/cell rendering |
| LE-11 | No animations in v1.0 (static icons only); caller composes via IconTemplate for animation if needed | 🟢 Locked | Deferred to future amendment |
| LE-12 | LipiEmptyState must work independently of LipiTable / LipiList / LipiPagination — no source coupling | 🟢 Locked | Audit verification per LipiTable §25 |

---

# 06-FLAGS-LOG.md — Amendment block (Stage 7 design batch)

**Append this entire block to the END of `06-FLAGS-LOG.md`** (immediately before the final "Pending sections" / "End of file" marker — wherever the last LE-12 entry was).

**Source:** strategic chat decisions in the Stage 7 design batch.
**Approval:** Arun confirmed all six Q-replies; explicit amendment on Q1 adding Left and Right placement variants.

---

## LipiPagination — additional build-chat amendments (LP-11 through LP-19)

| ID | Topic | Disposition | Action |
|---|---|---|---|
| **LP-11** | **File-count amendment**: variants (Full/Compact/Minimal) collapsed into single `LipiPagination.razor` with internal `@if (Variant == ...)` branching. `LipiPaginationCompact.razor` and `LipiPaginationMinimal.razor` (and their `.css` files) are NOT created. `LipiPaginationPageSize.razor`, `LipiPaginationCountDisplay.razor`, `LipiPaginationLoadMore.razor` STAY as separate sub-components (independently reusable / different paradigm for LoadMore). **Total: 10 files instead of the spec's 14.** Update the deploy script accordingly. | 🟢 Locked | Build chat applied during Stage 4 implementation. |
| **LP-12** | **Page-size dropdown uses native `<select>` styled with pagination tokens**, NOT `LipiSelect`. Resolves §5.1 vs §11.2 spec contradiction. Native `<select>` provides free screen-reader / mobile / keyboard support; LipiSelect's complexity isn't justified for 6 static options. Removes LipiSelect from LipiPagination's dependency list. | 🟢 Locked | Generalizes to principle: short static lists (≤10 options, no search, no rich content, no multi-select, no async) use native `<select>` + token styling. Consumer-facing form fields and rich-option dropdowns continue to use LipiSelect/LipiCombobox. |
| **LP-13** | **Pager chrome (page numbers, prev/next/first/last) uses plain `<button>` with `lipi-pagination-*` classes**, NOT `LipiButton`. LipiButton STAYS in the dependency list for action buttons only (Go button in jump-to-page, LoadMore button). Generalizes the chrome-vs-action principle introduced in LP-12. Token-driven focus-ring consistency via shared `--lipi-focus-ring-*` foundation tokens handles visual cohesion without component coupling. | 🟢 Locked | Apply same chrome-vs-action test for: LipiTable expand chevrons, sort indicator buttons, column resize handles (use `<div role="separator">`, not button), filter chip remove × buttons, selection checkboxes (plain `<input type="checkbox">` per accessibility a11y example), and any future chrome inside Lipi components. BulkActionTemplate slot is consumer-facing — caller chooses what to put in it. |
| **LP-14** | **Page-number algorithm + API**: adopt the AntDesign / Material rule (always show first/last, sibling-window around current, ellipsis for gaps > 1, reclaim ellipsis when gap == 1). REPLACE `MaxPageButtons` (default 5) with `Siblings` (default 1) + `BoundaryCount` (default 1). The old worked examples in LipiPagination §3.5 were drawn by hand and contained internal inconsistencies (e.g., `cur=3` left-hugged but `cur=18` did not mirror) — they are SUPERSEDED by the algorithmic rule. | 🟡 Build chat note | Caught by pure unit test against the spec examples before writing Blazor markup. **Add to operational pattern**: any algorithm with >3 edge cases or >2 inputs gets extracted to a pure static method or pure record type under `LiPi.Components.Internal.Algorithms` namespace, unit-tested against examples, THEN wired into Blazor markup. Phase 2.10 audit checks that every non-trivial algorithm has a pure helper + test file. |
| **LP-15** | **Stage 7 LipiTable+LipiPagination wiring — six design decisions locked**: see expanded entry below covering Q1-Q6 from the Stage 7 handoff. Includes Q1 amendment adding **Left and Right placement** to the Placement enum. | 🟢 Locked | See LP-15 expanded entry below. |
| **LP-16** | **Header checkbox tri-state per CURRENT PAGE** (not across all data): all-page-rows-selected → checked; some-page-rows-selected → indeterminate; no-page-rows-selected → unchecked. Click semantics: unchecked or indeterminate → select all on current page; checked → deselect all on current page. **NOT a blind toggle.** Match Gmail / GitHub / AG-Grid behavior. | 🟢 Locked | Implemented in Stage 7 alongside pagination wiring. |
| **LP-17** | **Stage 7 leaves event hook `OnAllOnPageSelected` for Stage 4c** to attach the across-pages banner UI. No banner code in Stage 7. The across-pages flag mechanism (per LipiTable §5.3.2) lives in Stage 4c. Don't refactor Stage 4a/4b selection code — the key-set model already supports everything needed. | 🟢 Locked | Seam between Stage 7 and Stage 4c documented. |
| **LP-18** | **Page-size change → reset to page 1** (matches AntDesign; simpler than computing approximate-position based on first previously-visible item). If a consumer wants smart-preserve behavior in v1.X, that's a future amendment — not blocking v1.0. | 🟢 Locked | — |
| **LP-19** | **Focus landing rules after page change**: via Alt+PageDown/Up → first cell of first row, preserving column position; via Ctrl+Home → first cell of first row of first page; via Ctrl+End → last cell of last row of last page; via pager button click → stays on clicked button; via prev/next chevron → stays on chevron button; via jump-to-page input → stays on Go button. LipiTable subscribes to LipiPagination's `OnCurrentPageChanged` and switches on `PageChangeReason` enum (already in spec §8.11) to decide whether and where to restore focus. | 🟢 Locked | New event hook `OnPageChangedAsync` added to LipiTable for focus coordination. |

---

## LP-15 — EXPANDED: Stage 7 six-decision design batch

This is the consolidated entry. Stage 7 LipiTable+LipiPagination wiring locks the following:

### Q1 — Placement (with Left/Right amendment)

**Locked enum:**

```csharp
public enum PaginationPlacement
{
    Bottom,    // default — region ⑧ from LipiTable §2.1
    Top,       // region ④ from LipiTable §2.1
    Both,      // top + bottom pair (Both does NOT include Left/Right)
    Left,      // ← NEW per Q1 amendment: side-anchored vertical pager
    Right,     // ← NEW per Q1 amendment: side-anchored vertical pager (mirror)
    None       // escape hatch; consumer composes externally via @bind state
}
```

**Six values. LipiTable owns rendering by default.** `None` is the escape hatch for consumers who want fully custom placement (e.g., dashboard with pager in a header bar separate from the table).

**Pairs not in the enum:** Left+Right pairing or Top+Left etc. are NOT enumerated — that would explode the matrix to ~16 values. Consumers who want both-side pagination use `None` and compose two LipiPagination instances externally with shared `@bind-CurrentPage` / `@bind-PageSize`.

### Left/Right placement — behavioral implications (NEW per Q1 amendment)

A side-anchored pager has architectural consequences. **LipiPagination gains an `Orientation` parameter:**

```csharp
public enum PaginationOrientation
{
    Horizontal,   // default — page numbers laid out left-to-right
    Vertical      // page numbers stacked top-to-bottom (for side placement)
}

[Parameter] public PaginationOrientation Orientation { get; set; } = PaginationOrientation.Horizontal;
```

When `Placement` is `Left` or `Right` on LipiTable, the embedded LipiPagination is rendered with `Orientation="Vertical"` automatically.

**Variant compatibility with vertical orientation:**

| Variant | Horizontal | Vertical |
|---|---|---|
| Full | ✓ | ✗ (page-size + count + numbers + jump-to-page stacked vertically is visually too heavy) |
| Compact | ✓ | ✓ (`[‹]` / `Page 3/20` / `[›]` stacked) |
| Minimal | ✓ | ✓ (just `[‹]` above `[›]`) |

**Auto-downgrade rule:** when `Placement` is `Left` or `Right` AND `Variant` is `Full`, LipiTable silently downgrades to `Compact` for the embedded pager and logs a Dev-mode warning:

```
LipiTable: Variant=Full is not compatible with Placement=Left|Right.
Auto-downgrading to Compact. To suppress this warning, set Variant=Compact 
or Variant=Minimal explicitly.
```

Production: no warning; downgrade silent (graceful degradation per A14 env-gated validation pattern).

**Page-size selector behavior in side placement:**

The page-size dropdown is too wide for a narrow side column. When `Placement` is `Left` or `Right`:
- `PaginationOptions.ShowPageSize` defaults to `false` (page-size selector hidden in the side pager)
- Recommended pattern: caller exposes a separate page-size control in the table's toolbar (LipiTable's region ① per §2.1) for that table
- If the consumer wants the page-size selector inline in the side pager anyway, they explicitly set `PaginationOptions(ShowPageSize: true)` — but the resulting layout may look cramped on narrow columns; consumer's responsibility

**Row count display in side placement:**

"Showing 51-75 of 487" is horizontal text awkward in a narrow column. When `Orientation="Vertical"`:
- Default compact format: `51-75 / 487` (slash separator, "Showing" and "of" omitted)
- New token: `--lipi-pagination-count-format-compact` controls the format template
- Consumer can override via `PaginationOptions.RowCountTemplate` (already in LipiTable §8.3.5)

**Side pager column width:**

```csharp
[Parameter] public string PaginationSideWidth { get; set; } = "48px";
```

48px default fits a vertical column of ~36px square page buttons plus side padding. Configurable per table.

**Sticky behavior:**

When `Placement` is `Left` or `Right` AND the table scrolls vertically inside its container, the side pager uses `position: sticky; top: 0` to remain visible during scroll. CSS-only; no JS coordination needed.

**Selection bulk-action bar interaction:**

Bulk action bar (per LipiTable §5.6.4) still renders horizontally above the rows. With a side pager, the bulk bar's left edge offsets to clear the side pager's column width. The CSS Grid layout handles this naturally — bulk bar is a sibling of the data body, not a sibling of the side pager.

**System columns + side pager:**

Selection checkbox column and expand chevron column stay leftmost-in-the-data-area regardless of Placement. The side pager (when Left) sits to the LEFT of the system columns:

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
  side pager column │   │
        selection col │
              expand col
```

### Q2 — Mode (client vs server)

LOCKED per existing LipiTable §4 spec. Mode is determined by which parameter the consumer sets:
- `Items` set → client mode (LipiTable slices in-memory: filter → sort → paginate)
- `DataSource` set → server mode (LipiTable calls back with `TableQueryRequest`, gets `TableQueryResponse<TItem>`)
- Both set → Dev-mode warning; DataSource wins (explicit beats implicit)
- Neither set → empty state

Server contract requires `TotalCount` (pagination math needs it; `-1` allowed for "unknown total" with row-count-display caveat per §8.3.5). Loading state shows pager visible-but-disabled during DataSource in-flight; don't hide it (causes layout shift).

Stage 7 task: implement client-side slicing first, then wire DataSource path. Both flow into the same internal `{currentPage, pageSize, totalCount}` state.

### Q3 — Selection × paging

LOCKED per existing LipiTable §5 spec. Selection is `HashSet<object>` of row keys; navigation doesn't touch it. Filter that drops rows silently drops their keys from selection (matching, not intentional clearing).

**Pre-wire in Stage 7:**
1. Selection persistence across page nav (already works because Stage 4a/4b shipped the key-set model)
2. Header checkbox tri-state per current page (per LP-16)
3. Header checkbox click semantics (per LP-16, NOT blind toggle)
4. Selection counter in bulk action bar shows total key count across all pages

**Defer to Stage 4c:**
1. Across-pages banner UI ("All 25 on this page selected. Select all 247?")
2. Across-pages flag mechanism (per §5.3.2)
3. Server-keys API for resolving "all across pages"
4. Bulk action bar "Across all pages" qualifier text

**Seam:** Stage 7's header-checkbox click fires internal event `OnAllOnPageSelected` (per LP-17) that Stage 4c subscribes to for banner display. Stage 7 just defines the event; doesn't render the banner.

### Q4 — Keyboard

LOCKED:
- **Plain PageDown/PageUp**: viewport-height cell navigation per Stage 4d (UNCHANGED). Moves focus by ~viewport-height rows; scrolls viewport. Does NOT change pagination page.
- **Alt+PageDown / Alt+PageUp**: change pagination page (next / previous). Focus lands on first cell of first row on new page; column position preserved.
- **Ctrl+Home**: focus on first cell of first row of FIRST PAGE (navigates pages if needed).
- **Ctrl+End**: focus on last cell of last row of LAST PAGE (navigates pages if needed).
- **Pager button click (pointer)**: focus stays on clicked button (standard pointer behavior).
- **Prev/next chevron click**: focus stays on chevron.
- **Jump-to-page input + Enter or Go button**: focus stays on Go button.

LipiTable subscribes to LipiPagination's `OnCurrentPageChanged` and switches on `PageChangeReason` enum to decide focus action. PageChangeReason is already in LipiTable §8.11 spec.

### Q5 — Page-snap ownership

LOCKED clean separation:
- **LipiPagination owns the clamp math**: when TotalCount or PageSize props change, clamps current page to [1, totalPages] and fires `OnCurrentPageChanged` with reason `PageSnap`. Already implemented in A50.
- **LipiTable owns the policy** of when to reset page to 1: sort change → page=1; filter change → page=1; page-size change → page=1 (per LP-18). LipiTable resets BEFORE re-fetching/re-rendering, then passes new page=1 to LipiPagination. LipiPagination just sees a new page prop and re-renders.

LipiPagination doesn't know about sort/filter; can't decide. LipiTable knows about everything; delegates the math to LipiPagination.

### Q6 — Variant/style defaults when embedded

LOCKED — adopt AntDesign's pattern. LipiTable parameters added in Stage 7:

```csharp
[Parameter] public PaginationPlacement Placement { get; set; } = PaginationPlacement.Bottom;
[Parameter] public PaginationVariant Variant { get; set; } = PaginationVariant.Full;
[Parameter] public PaginationOptions? PaginationOptions { get; set; }
[Parameter] public string PaginationSideWidth { get; set; } = "48px";  // for Left/Right placement
```

Where `PaginationOptions` is a record bag:

```csharp
public sealed record PaginationOptions(
    PaginationCellStyle? CellStyle = null,         // null = LipiPagination default (Bordered)
    PaginationActiveStyle? ActiveStyle = null,     // null = LipiPagination default (Solid)
    PaginationChevronStyle? ChevronStyle = null,   // null = LipiPagination default (Auto)
    int? Siblings = null,                          // null = LipiPagination default (1)
    int? BoundaryCount = null,                     // null = LipiPagination default (1)
    IReadOnlyList<int>? PageSizeOptions = null,
    int? DefaultPageSize = null,
    bool? ShowFirstLast = null,
    bool? ShowJumpToPage = null,
    bool? ShowPageSize = null,                     // auto-overridden to false in Left/Right placement
    bool? ShowRowCount = null,
    Func<PaginationRange, string>? RowCountTemplate = null
);
```

Null fields delegate to LipiPagination's own defaults. Consumer overrides selectively.

Static preset for HIS app:

```csharp
public static class PaginationOptionsPresets
{
    public static readonly PaginationOptions LipiHisDefault = new(
        CellStyle: PaginationCellStyle.Borderless,
        ActiveStyle: PaginationActiveStyle.Tint,
        ChevronStyle: PaginationChevronStyle.Auto);
}
```

**Embedded default = same as LipiPagination's standalone default** (Bordered + Solid + Auto). LipiTable is in `LiPi.Components.*` (redistributable); component isolation contract (§25) prohibits baking HIS-specific visual decisions. HIS app explicitly opts into `PaginationOptionsPresets.LipiHisDefault`.

Consumer usage:

```razor
<!-- Zero-config (gets LipiPagination's defaults — Bordered/Solid/Auto) -->
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

## CSS Lint — new standing rule

| ID | Topic | Disposition | Action |
|---|---|---|---|
| **CL-1** | **CSS comment/brace lint**: every CSS file in `LiPi.Components.*` and HIS app must pass a comment/brace balance check before delivery. Silent rule-discard from glob-text-in-comment (e.g., literal `lipi-*/` in a comment confuses the parser) or unterminated multi-token comments (`--color-*/--r-*/--sp-*` interpreted as a comment that never closes) are the failure modes. Build chat caught two latent instances in A50.1 that caused a long false "staleness" hunt. | 🟡 Standing rule | Add to pre-delivery checklist (alongside SPEC comment header + deploy-downloads.ps1 update). Add to Phase 2.10 audit's CSS scan. Lint can be a regex check or a CSS parser (`postcss` config) — build chat decides implementation. |

---

## Page-size default — drafting correction

| ID | Topic | Disposition | Action |
|---|---|---|---|
| **DOC-1** | **LipiPagination §3.3 prose said page sizes start at 10**; the shipped code (per LipiTable §8.2.1) starts at 5. The shipped code is correct. The §3.3 prose is a drafting error from strategic chat. | 🟢 Locked | At Phase 2.8 spec consolidation (per LipiTable §27.14), update LipiPagination §3.3 prose to match LipiTable §8.2.1: `[5, 10, 25, 50, 100, 200, All]` with default 25. No code change needed. |

---

## A50.1 — visual polish memorialization

| ID | Topic | Disposition | Action |
|---|---|---|---|
| **LP-A50** | **Three-axis style decomposition for LipiPagination** (introduced in A50.1, runtime-verified by Arun): `PaginationCellStyle` { Bordered, Borderless }, `PaginationActiveStyle` { Solid, Tint }, `PaginationChevronStyle` { Auto, Bordered, Ghost }. Three orthogonal axes; compose freely. Zero-config default = Bordered + Solid + Auto. Deep tint uses CSS `color-mix` (mode-aware) because `--color-primary-pale` reads too faint in light mode. Tint-in-bordered-grid gets a primary ring to remain affordant. | 🟢 Locked | This is the canonical style API for LipiPagination. Update LipiPagination spec at consolidation time (§2.2-2.4) to reflect the three-axis model instead of the original single-variant approach. |

---

## Updated audit queue items (Phase 2.10)

Add to the cross-cutting Phase 2.10 audit list:

| ID | Topic | Source |
|---|---|---|
| Q-16 | Verify every non-trivial algorithm has a pure helper under `LiPi.Components.Internal.Algorithms` + dedicated test file | LP-14 |
| Q-17 | CSS comment/brace lint integrated into CI; every CSS file passes before merge | CL-1 |
| Q-18 | Verify chrome-vs-action principle applied across all components: plain HTML for chrome (page buttons, sort indicators, resize handles, expand chevrons, filter chip removes, selection checkboxes); composed Lipi components for actions only | LP-12, LP-13 |
| Q-19 | Verify side-placement (Left/Right) auto-downgrade Full→Compact works with Dev-mode warning + Prod silent fallback | LP-15 (Q1 amendment) |
| Q-20 | Verify focus landing rules after page change for all six trigger paths (Alt+PgDn/Up, Ctrl+Home/End, pager button, chevron, Go button) | LP-19 |

---

## How to merge this amendment

In the existing `06-FLAGS-LOG.md`:

1. Find the section header **"## LipiEmptyState sibling spec"** with entries LE-1 through LE-12.
2. After that section, BEFORE the "## Pending sections" / "Phase 2.8 strategic spec corpus is LOCKED" closing block, INSERT the entire content of this file (from "LipiPagination — additional build-chat amendments" through "Updated audit queue items").
3. The closing block's status text remains accurate ("Phase 2.8 strategic spec corpus is LOCKED") — these amendments are build-chat-driven refinements, not spec re-opens. The corpus is still locked; the flags log just captures the wrinkles that emerged during implementation.

---

*Amendment file end. Add to the build chat handoff folder so the next session has the cumulative flag set.*


## Pending sections

LipiTable spec is **COMPLETE** (28 sections delivered).
LipiList sibling spec is **COMPLETE** (21 sections).
LipiPagination sibling spec is **COMPLETE** (12 sections).
LipiEmptyState sibling spec is **COMPLETE** (10 sections + §0 cross-section reconciliation).

🎯 **Phase 2.8 strategic spec corpus is LOCKED.**

Build chat handoff ready. Total spec corpus ~20,000+ lines across:
- `00-Phase2.8-Overview.md`
- `01-LipiTable-Spec.md` (28 sections consolidated)
- `02-LipiList-Spec.md`
- `03-LipiPagination-Spec.md`
- `04-LipiEmptyState-Spec.md`
- `05-Standing-Rule-Addition-Library-Dependency-Posture.md`
- `06-FLAGS-LOG.md` (this file)

---

*This file updated after every section delivery. Build chat reads this before starting any LipiTable implementation work.*
