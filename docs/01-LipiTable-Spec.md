# LipiTable Specification — Consolidated

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>` + sub-components
**Status:** Build-ready spec — locked
**Section count:** 28 sections
**Companion specs:** 02-LipiList-Spec.md, 03-LipiPagination-Spec.md, 04-LipiEmptyState-Spec.md
**Flags:** 06-FLAGS-LOG.md

---

## Table of contents

| § | Section | Approx lines |
|---|---|---|
| 1  | Overview | 378 |
| 2  | Anatomy | 380 |
| 3  | Column model | 746 |
| 4  | Data sources | 541 |
| 5  | Selection | 612 |
| 6  | Sorting | 566 |
| 7  | Filtering | 1062 |
| 8  | Pagination | 561 |
| 9  | Grouping | 787 |
| 10 | Tree data | 693 |
| 11 | Master-detail | 670 |
| 12 | Inline editing | 1266 |
| 13 | Column ops | 509 |
| 14 | Density | 361 |
| 15 | Aggregation | 599 |
| 16 | Toolbar | 611 |
| 17 | Export | 683 |
| 18 | Body states | 714 |
| 19 | Accessibility | 753 |
| 20 | Virtualization | 448 |
| 21 | Persistence | 805 |
| 22 | Conditional formatting | 686 |
| 23 | Events | 969 |
| 24 | Tokens | 622 |
| 25 | Isolation contract | 541 |
| 26 | StyleGuide | 562 |
| 27 | Files to create | 533 |
| 28 | Deploy script | 541 |

**Total:** ~17,225 lines

---


# LipiTable Spec — §1 Overview and design principles

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>` + `LipiColumn<TItem, TValue>` + supporting types
**Status:** Section body — draft for review

---

## §1.1 — What LipiTable is

LipiTable is the canonical tabular-data component for the LiPi component library. It renders any collection of rows where each row has a parallel column structure, and it owns the full surface area of features a modern data table needs: sorting, filtering, pagination, grouping, tree data, master-detail, inline editing (cell or row), selection, density, column resize/reorder/pin, virtualization, persistence of user preferences, conditional formatting, status strips, aggregation, and export.

It is **generic** — `LipiTable<TItem>` where `TItem` is the caller's row data type — and **domain-neutral**. Nothing in LipiTable knows about patients, clinics, billing, lab results, or any other LiPi-specific concept. Domain semantics are integrated entirely through caller-provided templates, format selectors, event handlers, and string-typed status taxonomies. Per the locked component-package-isolation rule, the LiPi component library (including LipiTable) is redistributable to any Blazor application — the same component file ships unchanged whether the consumer is Armoki HIS, a future clinic chain, or an unrelated industry's Blazor app.

LipiTable is the **largest** component in the LiPi library by API surface, behavioral complexity, and CSS footprint. This is by design: the consuming pages it serves vary enormously (a 5-row settings table, a 10,000-row clinical worklist, a 50-column billing-detail grid, a 2-column quick-lookup), and LipiTable carries the variation so each consuming page doesn't have to.

### What LipiTable is not

LipiTable is not:

- **A spreadsheet.** Even with cell-edit mode, it does not support formula cells, computed columns referencing other cells, named ranges, or pivot tables. The user-facing feel of cell-edit (Tab to move, Excel-style keystrokes) is borrowed from spreadsheets, but the underlying model is "edit a record's fields," not "evaluate a formula graph."
- **A chart container.** Cells render text-equivalent content (with optional avatars, status pills, links, files). Visual charts inside cells are not supported in v1.0; charting belongs to a separate composite component.
- **A form builder.** Inline editing reuses LipiInputBase-family components in cells, but LipiTable doesn't generate forms from schema. The caller declares each column's editability and edit template explicitly.
- **A query builder.** Filters operate per-column with a fixed operator set per column type (see §7.3). Complex query composition (boolean groups, nested expressions) is a separate component if ever needed.
- **A reporting engine.** Export to CSV, PDF, and Print is supported (see §17), but designing report layouts, header/footer templates, watermarks, multi-grid composition, etc. lives outside LipiTable.

These exclusions matter because each of them is a real and reasonable feature to want — but each would expand LipiTable's scope by another component family's worth of complexity. By stating them out of scope, this spec keeps LipiTable focused on "show structured tabular data; let the user explore, edit, and export it" and nothing more.

---

## §1.2 — Composition partners

LipiTable does not stand alone. It composes a number of existing LiPi components for its various features. Listing them here so the spec body can reference them without re-explaining:

**From Phase 2.1 (Buttons / actions):**
- `LipiButton` — toolbar actions, row actions, page buttons, modal/drawer triggers, CTA in empty states
- `LipiIconButton` — compact icon-only actions in headers and rows
- `LipiButtonGroup` — segmented controls in the toolbar (density, view mode)

**From Phase 2.2 (Inputs):**
- `LipiTextBox` — default text edit cell, quick search input
- `LipiNumberInput` — default Number/Currency edit cell, between-filter inputs
- `LipiDatePicker` — default Date edit cell, single-date filter
- `LipiDateTimePicker` — default DateTime edit cell
- `LipiTimePicker` — default Time edit cell
- `LipiDateRangePicker` — between-filter for date columns, also for explicit date-range filters

**From Phase 2.3 (Selection):**
- `LipiSelect` — default Status edit cell (when caller provides options), default for any column with a fixed value set
- `LipiCombobox` — same role for searchable / large option sets
- `LipiMultiSelect` — set-filter implementation (multi-select of distinct values)

**From Phase 2.5 (Toggles):**
- `LipiCheckbox` — selection column, "select all" header, group / tree expand state
- `LipiSwitch` — boolean edit cell when configured

**From Phase 2.6.1 (Layout):**
- `LipiTabs` — tabbed filter / view-mode controls (when caller composes)
- `LipiAlert` — inline messages for filtered-empty / error / partial-load states
- `LipiCard` — wraps LipiTable in many consuming pages (LipiTable does not own the card)

**From Phase 2.6.2 (Overlays):**
- `LipiModal` — export "More options" modal, concurrency conflict modal (when in Modal resolution mode), conflict diff popover (rendered as a non-modal popover but reusing modal infrastructure), "Discard unsaved edit?" confirm
- `LipiDrawer` — filter drawer (when in Drawer filter mode), column picker drawer (alternative layout), bulk action drawer for complex multi-action workflows
- `LipiDynamicTabs` (dirty-state pattern) — pattern reused for "edit second row while first is dirty" prompt

**From Phase 2.7 (Feedback):**
- `LipiBadge` — Status column type rendering, group count badges, selection-count chip in the bulk action bar
- `LipiPill` — alternative for Status, also for tags / categories in cells
- `LipiSkeleton` (Line / Rect / Circle) — loading-state row scaffolding
- `LipiSpinner` — server-side fetch indicator, async export progress, row-level edit-save spinner
- `LipiToast` — save-success / save-error notifications, export-complete notifications, copy-to-clipboard "Copied!" confirmation
- `LipiValidationSummary` — row-level validation surfacing (composed by the consuming page; LipiTable feeds it row errors)

**From Phase 2.8 (this phase, sibling components):**
- `LipiPagination` — composed in LipiTable's footer (or top, or both, or none per `PaginationPlacement`)
- `LipiEmptyState` — default empty / filtered-empty / error state UI (caller can override)
- (LipiList is not composed by LipiTable; they're peer components.)

**Shared infrastructure (this phase):**
- `lipi-status-tokens.css` — status colors used by Status column type, row status strip, group status indicators
- `LipiStatus.cs` — reference string constants for status values
- `ITablePreferenceService` — persistence backend for per-user table state

LipiTable composes these — it does not subclass, fork, or duplicate any of them. Every visual element of LipiTable that has an equivalent existing component **must** use that component. This rule is enforced in code review and audited in Phase 2.10.

---

## §1.3 — Design principles

LipiTable's design follows seven principles. Each principle resolved real trade-offs during the strategic design discussion (covered in the §1.4 decision summary); they're restated here so the spec body can reference them rather than re-justifying.

### Principle 1 — Caller controls semantics; LipiTable controls presentation

The caller knows what's in their data and what it means. LipiTable knows how to render a table well.

This split shows up everywhere in the API. The caller declares column types (Text, Number, Currency, Date, Status, etc.), provides selectors (`Field`, `ValueSelector`), and optionally supplies templates (`CellTemplate`, `EditTemplate`, `HeaderTemplate`). LipiTable then renders that declaration according to its design system: same padding, same typography, same density rules, same sort indicators, same filter UI, same focus rings, same colors. The caller never has to think about "what does an active row look like" or "where do sort indicators go" — those are LipiTable's call.

This principle is why declarative children (the `<LipiColumn>` markup pattern) is the primary API rather than a config object: declarative children let the caller express **what** each column is without coupling to LipiTable's internal rendering pipeline. If LipiTable's rendering changes (e.g., a new density mode is added, sort indicators move from text to icon), the caller's column declarations don't change.

### Principle 2 — Both client-side and server-side are first-class

LipiTable supports two data-source models, and neither is the "primary" with the other as a fallback. Both are equal-weight. Caller picks per page based on data shape:

- Pass `Items` — LipiTable runs the full sort / filter / page / group pipeline in memory. Suitable for ≤ ~1000 rows.
- Pass `DataSource` callback — LipiTable surfaces sort / filter / page / group state to the caller, who returns the appropriate page from the server.

The API surface for column declarations, selection, editing, etc. is **identical** in both modes. The caller picks the data path; the column code doesn't know which mode it's in.

This matters because real LiPi usage spans both:

- Settings tables, admin tables, picklist tables — client-side (load once, work in memory)
- Patient lists, audit logs, lab worklists, billing — server-side (potentially millions of rows)

If client-side were "first" and server-side a tacked-on alternative, the clinical worklists would feel like second-class consumers. They aren't. They drive most of the table-feature requirements (virtualization, server-side filtering, optimistic concurrency on inline edit), so they deserve first-class treatment.

### Principle 3 — Clinical safety is the default; productivity is opt-in

Several features have a "safe" and a "fast" variant. Where the choice would be invisible to the caller, LipiTable defaults to the safer behavior. Examples:

- **Edit mode default = `None`** (read-only). Row-edit and cell-edit are opt-in. A misconfigured table doesn't become editable by accident.
- **Enter in cell-edit = save and stay** (not Excel-style move-down). A doctor entering a single corrected value doesn't accidentally cascade into the wrong row. Excel-style is available via Ctrl+Enter for users who want it.
- **Save flow default = Pessimistic** (wait for server confirmation before reflecting change). Optimistic UI is opt-in via `OptimisticUpdate.Optimistic`.
- **Concurrency check default = on** when `RowVersionSelector` is provided. The conflict UX (banner + diff) is the default, not last-write-wins.
- **Critical-field confirmation** is a per-column opt-in via `RequireConfirmEdit="true"`. Mark the field, get the confirm dialog. No global toggle that could be forgotten.
- **Destructive bulk actions** integrate with the existing confirmation-dialog standing rule (built-in confirm modal before bulk delete).

The caller can override all of these for high-throughput workflows (lab tech updating 200 result rows, etc.). The defaults exist because the failure mode of "this misclicked into clinical data" is more expensive than the failure mode of "this took one extra keystroke."

### Principle 4 — Persistence is per-user, opt-in, and reversible

User preferences for a table (column widths, sort, filter, group, page size, etc.) are persisted per-user, per-table-id, in the identity DB. But:

- Persistence is **opt-in** via `TableId` — if the caller doesn't set `TableId`, no persistence happens. Ad-hoc tables (e.g., a quick lookup in a modal) don't litter the user's prefs table with state that won't be reused.
- Persistence is **reversible** — the column picker always exposes "Reset to defaults" which clears all stored state for that TableId for that user.
- Persistence is **isolated per-user, per-TableId** — no cross-user leakage; no cross-table interference.

The persistence layer (`ITablePreferenceService`) is an interface, with a default EF-based implementation against the identity DB. Consuming apps can replace the implementation if they want different storage (e.g., localStorage for offline use, or a different DB schema). LipiTable doesn't care.

### Principle 5 — Every behavior is keyboard-accessible

Mouse-first interaction is supported, but every behavior must also be reachable by keyboard. This is non-negotiable for clinical contexts (clinicians often work with one hand on a keyboard and one on a phone/chart) and for accessibility compliance (WCAG 2.1 AA minimum).

The keyboard model is documented in §19.3 and §12.7. Highlights:

- Tab into the table, then arrow keys to navigate cells
- Space to select, Enter to activate
- Sorting via Enter/Space on column header (with screen-reader announcement)
- Filtering via Alt+Down on column header (opens filter popover)
- Inline edit entered via Enter / F2 / direct typing
- Save / cancel during edit via Enter / Ctrl+Enter / Tab / Escape
- Selection: Space toggles, Shift+arrows extend, Ctrl/Cmd+A select-all-page

Screen-reader behavior is also specified: row count announced on load, sort changes announced, selection changes announced, edit mode entered / exited announced, validation errors announced via aria-live.

### Principle 6 — Performance is a feature, not an afterthought

LipiTable has to handle small tables (5–50 rows) without overhead and large tables (5,000–50,000+ rows) without freezing. The performance model:

- Auto-detect virtualization (default threshold 100 rows). Below the threshold, render normally. Above, switch to Microsoft.AspNetCore.Components.Web.Virtualization.
- Sticky header rendered outside the virtualized region (avoids re-render per scroll).
- Pinned columns rendered for visible viewport rows only.
- Selection state stored by key (from `KeySelector`), not by row index — survives sort / filter / page / scroll without re-mapping.
- Debounced persistence writes (300ms) — typing in a filter doesn't fire a save call per keystroke.
- Server-side mode never loads more than the current page — full dataset stays on the server.
- Server-side filtering, sorting, grouping, and pagination all flow through a single `TableQueryRequest` shape, so the server can produce one optimized query per state change.

Performance budgets are stated in the spec where relevant:
- First paint: ≤ 200ms for ≤ 100 rows
- Filter / sort response: ≤ 100ms client-side, server-RTT bound otherwise
- Scroll: 60fps maintained during virtualized scroll
- Edit save: ≤ 50ms client work + server RTT

These are targets, not hard SLAs — they document the design intent so reviewers can spot regressions.

### Principle 7 — Composition over configuration

When a feature can be solved by composing an existing component into LipiTable's slot, that's the preferred design. When it needs configuration parameters, those parameters stay minimal.

Examples:

- **Empty state** — LipiEmptyState is composed into the `<EmptyTemplate>` slot. LipiTable does not have its own `EmptyTitle` / `EmptyBody` / `EmptyIcon` / `EmptyCta` parameters duplicating LipiEmptyState's API.
- **Loading state** — LipiSkeleton (Line / Rect) composes into a default loading template. The caller can override via `<LoadingTemplate>`.
- **Validation summary** — `LipiValidationSummary` (Phase 2.7) is composed by the consuming page above or below LipiTable. LipiTable feeds it row errors; it doesn't bake its own summary UI.
- **Bulk actions** — caller provides `<BulkActions>` RenderFragment. LipiTable renders the slot + selection count; the action buttons are caller-defined LipiButtons.
- **Toolbar customization** — `<ToolbarLeft>` and `<ToolbarRight>` slots for caller-specific buttons.

The opposite anti-pattern would be a parameter list of 200 booleans and 50 string templates that try to cover every visual variation. LipiTable instead keeps about 60 first-class parameters and exposes the rest of the variation through ~15 named RenderFragment slots.

---

## §1.4 — Locked decisions summary

The strategic-chat design discussion produced ~80 locked decisions across 12 question batches. They are cross-referenced throughout this spec. The table below summarizes them in one place, mapping each decision to its implementing section.

### Data model & paging

| Q | Decision | Section |
|---|---|---|
| Q1.1 | Both client-side (`Items`) and server-side (`DataSource`) supported equal-weight | §4 |
| Q1.2 | Auto-detect virtualization with caller override (default threshold 100 rows) | §20 |
| Q1.3 | Page size options `[5, 10, 25, 50, 100, 200, "All"]`, default 25, "All" caps at 1000 in server mode with banner | §8 |

### Selection

| Q | Decision | Section |
|---|---|---|
| Q2.1 | Selection persists across pages, filter changes, sort changes | §5.2 |
| Q2.2 | Two-step "Select all" semantics — page first, banner offers all-across-pages | §5.3 |
| Q2.3 | Shift-click range + Ctrl/Cmd-click toggle | §5.4 |

### Inline editing (the largest batch)

| Q | Decision | Section |
|---|---|---|
| Q3.1.a | Both Cell-edit and Row-edit modes supported; default `EditMode.None` | §12.1 |
| Q3.1.b | Hybrid critical-field confirm: per-column `RequireConfirmEdit` + `ConfirmEditMessage` AND `OnBeforeRowSave` callback | §12.5 |
| Q3.1.c.i | Enter in cell-edit = save and stay (clinically safe); Ctrl+Enter = save and move down (Excel-style) | §12.7 |
| Q3.1.c.ii | Direct typing replaces value (Excel-style implicit edit) | §12.7 |
| Q3.1.c.iii | Tab wraps to next row at end of row | §12.7 |
| Q3.1.d | Hybrid Save/Cancel placement: inline at row right (default), sticky bottom bar opt-in | §12.3 |
| Q3.1.e.i | Selection works on non-edited rows; edited row's checkbox disabled | §12.8 |
| Q3.1.e.ii | Editing second row prompts to discard first | §12.8 |
| Q3.1.e.iii | Tab / page / sort / filter change while dirty fires discard confirm (LipiDynamicTabs pattern) | §12.8 |
| Q3.1.f.i | Concurrency conflict default = Banner with on-demand diff popover; Modal mode opt-in; Custom RenderFragment opt-in | §12.11 |
| Q3.1.f.ii | Banner-popover diff shows all fields with differences highlighted; Modal diff shows only differing fields | §12.11 |
| Q3.1.g | Add-new-row hybrid: toolbar `ShowAddButton="true"` AND programmatic `AddNewRowAsync(item)` API | §12.12 |
| Q3.1.h | Validation errors: per-field via LipiInputBase pattern; per-row red strip above the row + ⚠ icon in actions column | §12.6 |
| Q3.2 | Validation timing: per-field on input (after first blur) AND per-row on Save | §12.6 |
| Q3.4 | Save flow: caller picks via `OptimisticUpdate.Optimistic|Pessimistic`, default Pessimistic | §12.9 |
| Q3.5 | Optimistic concurrency via `RowVersionSelector` parameter (locked: row_version pattern is non-negotiable for clinical data) | §12.10 |
| Q3.6 | Add-new-row is caller's responsibility with optional inline `<AddRowTemplate>` for cases where inline add is appropriate | §12.12 |

### Grouping

| Q | Decision | Section |
|---|---|---|
| Q4.1 | Group bar visibility opt-in via `ShowGroupBar="true"`; default off (declarative grouping via `GroupBy` parameter works without bar) | §9.2 |
| Q4.2 | Group header rendering: `<GroupHeaderTemplate>` override available; default = `Value (Count)` | §9.3 |
| Q4.3 | Default group expand state: caller picks via `DefaultGroupState.Expanded|Collapsed`, default Expanded | §9.4 |

### Tree data

| Q | Decision | Section |
|---|---|---|
| Q5.1 | Both `ChildrenSelector` (nested shape) and `ParentSelector` (flat-with-parent-id shape) supported, caller picks one | §10.1 |
| Q5.2 | Tree and grouping mutually exclusive — if `ChildrenSelector` or `ParentSelector` is set, grouping is disabled | §10.5 + §9.6 |

### Master-detail

| Q | Decision | Section |
|---|---|---|
| Q6.1 | `<DetailTemplate>` accepts arbitrary Razor (no sub-grid restriction) | §11.1 |
| Q6.2 | Expand trigger via chevron in dedicated first column | §11.2 |
| Q6.3 | Multiple detail rows open at once allowed by default; caller can force accordion mode via `MultiExpand="false"` | §11.3 |

### Filtering

| Q | Decision | Section |
|---|---|---|
| Q7.1 | Filter UI: `FilterMode.HeaderIcon` (default) + `FilterMode.Drawer` opt-in. Inline filter bar skipped entirely. | §7.1 |
| Q7.2 | Date filter operators include relative operators (today, tomorrow, yesterday, this/last/next week/month/quarter/year, lastNDays, nextNDays); `between` uses LipiDateRangePicker | §7.3 + §7.4 |
| Q7.3 | Quick search: opt-in via `ShowQuickFilter="true"` AND caller-driven parameter mode supported | §7.8 |

### Export

| Q | Decision | Section |
|---|---|---|
| Q8.1 | Formats: CSV + PDF + Print in Phase 2.8 (Excel deferred); PDF uses in-house LiPi PDF library (stubbed until Phase 2.10) | §17.1 |
| Q8.2 | Export scope caller-chooses: View / Filtered (default) / All / Selected | §17.3 |
| Q8.3 | Hybrid trigger: toolbar dropdown for common formats + "More options" modal for scope / column selection | §17.2 |

### Toolbar

| Q | Decision | Section |
|---|---|---|
| Q9.1 | Toolbar zones: always-on quick-search + filter-chips; opt-out density-toggle + column-picker; opt-in title + export + refresh + add; auto bulk-action-bar | §16.1 |
| Q9.2 | Bulk action zone: caller-defined actions only (LipiTable renders count + slot, no built-in actions) | §16 + §5.6 |

### Persistence

| Q | Decision | Section |
|---|---|---|
| Q10.1 | Persist all of: column widths, order, visibility, pin, sort, filter, group, density, page size | §21.2 |
| Q10.2 | Storage in `identity.user_table_preferences` table (server-side, per-user, per-table-id JSON) | §21.3 + §21.6 |

### Column types

| Q | Decision | Section |
|---|---|---|
| Q11.1 | Per-column `Copyable="true"` opt-in (explicit, generic — no auto-copyable by type) | §3.3 |
| Q11.2 | `<CellTemplate>` available on every column (not restricted to Custom type) | §3.4 |
| — | Built-in column types: Text, Number, Currency, Date, DateTime, Time, Boolean, Mono, Status, Avatar, Link, File, Actions, Custom (14 generic types, zero clinical) | §3.3 |

### Misc API

| Q | Decision | Section |
|---|---|---|
| Q12.1 | Strongly typed: `LipiTable<TItem>` + `LipiColumn<TItem, TValue>` | §3.1 + §3.2 |
| Q12.2 | `KeySelector` required parameter — no implicit fallback | §3.1 |
| Q12.3 | Empty/Loading/Error states: built-in default + caller override slot (both available) | §18 |

### Status strip (cross-cutting)

| — | Status strip is row anatomy, not standalone component. Implemented via shared `lipi-status-tokens.css` + `data-status` attribute on row element. Per-row driven by `RowStatus` selector. | §22.3 + Phase 2.8 overview §2.1 |

---

## §1.5 — Out of scope for v1.0

Stated explicitly so consuming pages and reviewers know what NOT to expect:

### Hard out-of-scope (not coming in any near phase)

- **Cell formulas / computed cells** — LipiTable is not a spreadsheet
- **Pivot tables** — full pivot (rows × columns × measures) is a separate component if ever needed
- **In-cell charts** — sparklines, bar fills, etc. (caller can render any RenderFragment via `<CellTemplate>`, including a chart, but LipiTable doesn't ship built-in chart types)
- **Cross-table linked editing** — drag a value from one table into another
- **Excel-style fill-down / fill-right** — drag the selection corner to replicate values

### Deferred to future phases (might come)

- **Excel (.xlsx) export** — deferred per Phase 2.8 overview §3 (queued in `03-DEFERRED-ITEMS.md`)
- **Column-level conditional formatting rules** (e.g., "format cells where value > X in red") — out of v1.0; the row-level `RowClass` / `RowStyle` covers most cases
- **Custom aggregation pipelines** — beyond Sum/Avg/Count/Min/Max/Distinct + caller-defined function (the built-ins cover the 90% case)
- **Real-time row subscriptions / live updates** — pushed updates from a SignalR / server-sent-events source. Caller can drive re-renders manually via `Items` binding for now.
- **Native rich-text editing in cells** — current `EditTemplate` accepts any RenderFragment including rich-text components, but no built-in rich-text cell type
- **Drag-to-reorder rows in client-side mode** — deferred until a real consumer needs it
- **Cell merging / row spanning** — different rows sharing a cell value visually
- **Custom keyboard shortcut registration** — Phase 2.8 ships a fixed keyboard map (§19.3 + §12.7), caller cannot add new shortcuts at runtime
- **Theme variants beyond light + dark** — high-contrast mode is honored via CSS `prefers-contrast`, but no separately authored variant

### Conditional / on-demand

- **Inline-edit for non-LipiInputBase cell types** — caller can supply a custom `<EditTemplate>`. If the custom edit component doesn't follow LipiInputBase contracts (touched-state, EditContext participation), the per-field validation flow may not work correctly. Documented as a gotcha in §12.2.
- **Server-side aggregation results** — server returns aggregates in `TableQueryResponse.Aggregates`; LipiTable renders them in the footer. Server-side group-level aggregates are supported but require the server to compute them; LipiTable does not re-compute on the client.

---

## §1.6 — Forward references and structure

This spec is organized into 28 sections (§1–§28). The remaining sections lay out the component in detail. The reading order (top to bottom) is one valid path; the writing/development order (locked separately) is another. Cross-references between sections use the `§N.M` convention.

**Foundation (§1–§3):** This section, anatomy, generic typing and column model.

**Data flow (§4–§5):** Data sources, selection.

**Query operations (§6–§9):** Sort, filter, pagination, grouping.

**Hierarchical (§10–§11):** Tree data, master-detail.

**Inline editing (§12):** The largest section — 13 sub-sections covering Cell vs Row mode, validation, save flow, concurrency, add-new-row, dirty-state handling.

**Column operations (§13–§14):** Resize / reorder / pin, density.

**Numerical and output (§15–§17):** Aggregation, toolbar, export.

**States, accessibility, performance (§18–§20):** Empty / loading / error states, ARIA + keyboard + screen reader, virtualization.

**Cross-cutting (§21–§23):** Persistence, conditional row formatting, events.

**Reference (§24–§28):** Tokens, isolation contract reaffirmation, StyleGuide demos, files to create, deploy script additions.

---

## §1.7 — Versioning

LipiTable is **v1.0** in Phase 2.8. Future changes are tracked in `CHANGE-LOG.md` with `LipiTable` as the component scope. The spec's locked decisions (§1.4 table) are baseline-frozen — no v1.0 patch may change a locked decision. Changes that touch the locked decisions are explicit v1.X amendments with their own design discussion and CHANGE-LOG entry, per LiPi's standard process.

Specific amendments anticipated post-Phase-2.8 (no commitment, just visible work):

- **Phase 2.10 amendment:** PDF export wired to the in-house LiPi PDF library when ready; PDF demos enabled in StyleGuide; `03-DEFERRED-ITEMS.md` item closed.
- **Future amendment:** Excel export added when in-house Excel writer ships
- **Future amendment:** any feature currently in §1.5 deferred list that gets prioritized

---

*End of §1. Proceed to §2 — Component anatomy.*


# LipiTable Spec — §2 Component anatomy

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1

---

## §2.1 — Master visual layout

LipiTable is a vertical stack of regions. Some regions always render; some are conditional on configuration; some appear and disappear based on runtime state. The complete inventory from top to bottom:

```
┌─────────────────────────────────────────────────────────────────┐
│  ⓪  Header band   [Title]                          [Subtitle]   │  ← opt-in (§2.2.1)
├─────────────────────────────────────────────────────────────────┤
│  ①  Toolbar      [search] [filters] [density] [picker] [export] │  ← config-driven (§2.2.2)
│                                                       [+ Add]   │
├─────────────────────────────────────────────────────────────────┤
│  ②  Filter chips  Status: Active × | Role: Doctor × | Clear all │  ← when filters active (§2.2.3)
├─────────────────────────────────────────────────────────────────┤
│  ③  Bulk action   3 selected | [Action 1] [Action 2]   [Clear]  │  ← when selection active (§2.2.4)
├─────────────────────────────────────────────────────────────────┤
│  ④  Group bar     [Drag column here]   or   [Doctor ×]          │  ← when ShowGroupBar (§2.2.5)
├═════════════════════════════════════════════════════════════════┤
│  ⑤  Pagination (top)   [< 1 2 3 >]                              │  ← if PaginationPlacement=Top/Both
├─────────────────────────────────────────────────────────────────┤
│  ⑥  Column header  [✓] | Name ↕ ▽ | Role ↕ | Status ↕ | Actions │  ← STICKY (§2.2.6)
├─────────────────────────────────────────────────────────────────┤
│  ⑦  Body                                                        │
│  ▏      Group row — Active (12)                                 │  ← when grouped
│  ▏ [▸] [✓] Row 1                                                │  ← left border = status strip
│  ▏ [▸] [ ] Row 2                                                │
│  ▏ [▾] [✓] Row 3                                                │
│  ▏      └── Detail panel (caller's RenderFragment)              │  ← when expanded
│  ▏ [▸] [ ] Row 4 [editing inline]    [Cancel]  [Save]           │  ← when in row-edit
│  ▏ [▸] [ ] Row 5                                                │
│  ▏     ⚠ This row was modified by another user. [View] [Reload]│  ← when concurrency conflict
│  ▏ [▸] [ ] Row 6                                                │
├─────────────────────────────────────────────────────────────────┤
│  ⑧  Footer                                                      │
│  ⑧ₐ  Aggregate row    Σ Total: ₹4,28,500    Avg: ₹21,425        │  ← when AggregateFn columns
│  ⑧ᵦ  Pagination       Showing 1-20 of 487   [< 1 2 3 ... 25 >]  │  ← if Placement=Bottom/Both
├─────────────────────────────────────────────────────────────────┤
│  ⑨  Sticky edit bar    Editing Row #4    [Cancel]   [Save]       │  ← when EditButtonPlacement=StickyBottomBar
└─────────────────────────────────────────────────────────────────┘
```

The regions render in this order when present. Adjacent regions share a 0.5px border or 8px gap (see §24 for tokens). When a region is absent (e.g., no toolbar, no filter chips), the surrounding regions close up — there's no reserved blank space.

**Render-conditional regions** (visible only sometimes):

| Region | Visible when |
|---|---|
| ⓪ Header band | `Title` and/or `Subtitle` is set, or `<HeaderTemplate>` is provided |
| ① Toolbar | At least one toolbar element is opt-in (quick search, density toggle, column picker, export, add, or caller content). Toolbar can be entirely absent. |
| ② Filter chips | At least one filter is active OR `ShowFilterChips="Always"` |
| ③ Bulk action bar | `SelectionMode != None` AND at least one row is selected |
| ④ Group bar | `ShowGroupBar="true"` |
| ⑤ Pagination (top) | `PaginationPlacement` is `Top` or `Both` |
| ⑥ Column header | Always — even when zero rows. Sticky on vertical scroll. |
| ⑦ Body | Always rendered; contents vary by state (empty / loading / error / filtered-empty / normal) |
| ⑧ₐ Aggregate row | Any column has `AggregateFn` |
| ⑧ᵦ Pagination (bottom) | `PaginationPlacement` is `Bottom` (default) or `Both` |
| ⑨ Sticky edit bar | A row is in edit mode AND `EditButtonPlacement="StickyBottomBar"` |

### Body states

Region ⑦ (Body) renders one of five mutually exclusive states. Priority (highest first) defined in §18.1:

| State | When | Default render |
|---|---|---|
| Loading | Data fetch in progress | Skeleton rows (count = previous page size or 8) |
| Error | Data fetch failed | LipiEmptyState with retry CTA (Variant=Error) |
| FilteredEmpty | Data exists; current filters yield zero | LipiEmptyState with "Clear filters" CTA |
| Empty | No data exists at all | LipiEmptyState (caller-provided copy) |
| Normal | At least one row to display | Rows render |

Each state has a corresponding caller override slot (`<LoadingTemplate>`, `<ErrorTemplate>`, `<FilteredEmptyTemplate>`, `<EmptyTemplate>`) — §18.3.

---

## §2.2 — Element inventory and naming

Every visual element gets a class name. The naming follows the `lipi-table-*` namespace strictly — no element borrows from another component family. The full inventory:

### §2.2.1 — Header band (region ⓪)

| Element | Class | Notes |
|---|---|---|
| Container | `lipi-table-header-band` | Padding 16px top/sides, 12px bottom |
| Title | `lipi-table-header-title` | h2-sized text |
| Subtitle | `lipi-table-header-subtitle` | Body-sized, muted color |
| Custom slot | `lipi-table-header-custom` | When `<HeaderTemplate>` is set, replaces title/subtitle |

### §2.2.2 — Toolbar (region ①)

| Element | Class | Notes |
|---|---|---|
| Container | `lipi-table-toolbar` | Horizontal flex, height 48px |
| Left zone | `lipi-table-toolbar-left` | Holds quick search, custom left content |
| Center zone | `lipi-table-toolbar-center` | (Usually empty; reserved for future) |
| Right zone | `lipi-table-toolbar-right` | Density toggle, column picker, export, refresh, add, custom right content |
| Quick search input | `lipi-table-quick-search` | Wraps a `LipiTextBox` |
| Refresh button | `lipi-table-refresh-btn` | Icon-only `LipiIconButton`, rotates on activation |
| Density toggle | `lipi-table-density-toggle` | Segmented `LipiButtonGroup` (3 options) |
| Column picker button | `lipi-table-picker-btn` | Opens column picker popover |
| Export button | `lipi-table-export-btn` | Opens export dropdown |
| Add button | `lipi-table-add-btn` | Primary `LipiButton` with "+ Add" |
| Custom slot | `lipi-table-toolbar-custom-left` / `-right` | Caller's `<ToolbarLeft>` / `<ToolbarRight>` slots |

### §2.2.3 — Filter chips (region ②)

| Element | Class | Notes |
|---|---|---|
| Strip container | `lipi-table-filter-chips` | Horizontal flex, wraps to multi-row when many chips |
| Chip | `lipi-table-filter-chip` | Single filter representation |
| Chip column | `lipi-table-filter-chip-col` | "Status:" prefix |
| Chip operator | `lipi-table-filter-chip-op` | "contains" etc. — small text |
| Chip value | `lipi-table-filter-chip-val` | The filter value |
| Chip remove | `lipi-table-filter-chip-x` | ✕ icon button |
| Clear all | `lipi-table-filter-chip-clear-all` | "Clear all" text button |

### §2.2.4 — Bulk action bar (region ③)

| Element | Class | Notes |
|---|---|---|
| Container | `lipi-table-bulk-bar` | Horizontal flex, primary-tinted background |
| Count chip | `lipi-table-bulk-count` | "3 selected" with `LipiBadge` |
| Actions slot | `lipi-table-bulk-actions` | Caller's `<BulkActions>` RenderFragment |
| Clear button | `lipi-table-bulk-clear` | "Clear selection" text button |

### §2.2.5 — Group bar (region ④)

| Element | Class | Notes |
|---|---|---|
| Container | `lipi-table-group-bar` | Horizontal flex, dashed border (drop target) |
| Empty hint | `lipi-table-group-bar-hint` | "Drag column here to group" |
| Grouped chip | `lipi-table-group-bar-chip` | Represents one active grouping level |
| Chip column | `lipi-table-group-bar-chip-name` | Column name |
| Chip remove | `lipi-table-group-bar-chip-x` | ✕ to ungroup |

### §2.2.6 — Column header (region ⑥)

| Element | Class | Notes |
|---|---|---|
| Header row | `lipi-table-header-row` | Sticky, position:sticky top:0 |
| Header cell | `lipi-table-header-cell` | One per column |
| Selection-all cell | `lipi-table-header-cell-select` | Contains "select all" checkbox |
| Expand chevron cell | `lipi-table-header-cell-expand` | Empty header for expand column (master-detail or tree) |
| Cell text | `lipi-table-header-cell-text` | Column header label |
| Sort indicator | `lipi-table-header-sort` | ↕ / ↑ / ↓ icon |
| Sort priority badge | `lipi-table-header-sort-priority` | Small "1", "2", "3" for multi-sort |
| Filter icon | `lipi-table-header-filter` | ▽ icon, primary-colored when filter active |
| Resize handle | `lipi-table-header-resize` | 4px-wide vertical handle on right edge |
| Pin indicator | `lipi-table-header-pin` | 📌 when column is pinned |
| Drag handle (reorder) | `lipi-table-header-drag` | Optional; some headers use the whole header as drag handle |
| Sort affordance hover | `lipi-table-header-cell--sortable` (modifier) | Cursor:pointer + hover bg |
| Pinned column modifier | `lipi-table-header-cell--pinned-left` / `--pinned-right` | Sticky left/right + shadow |

### §2.2.7 — Body (region ⑦)

| Element | Class | Notes |
|---|---|---|
| Body container | `lipi-table-body` | Scrollable region |
| Virtualization container | `lipi-table-body-virtual` | When virtualization active |
| Group row | `lipi-table-group-row` | Header for a group, spans all columns |
| Group expand chevron | `lipi-table-group-row-chevron` | ▸ / ▾ |
| Group label | `lipi-table-group-row-label` | "Active (12)" or caller template |
| Group aggregate | `lipi-table-group-row-aggregate` | Per-column aggregate within group |
| Group selection | `lipi-table-group-row-select` | Checkbox (indeterminate when partial) |
| Data row | `lipi-table-row` | One per data row |
| Row in edit | `lipi-table-row--editing` | Modifier when row is in inline edit |
| Row dirty | `lipi-table-row--dirty` | Modifier when row has unsaved changes |
| Row disabled | `lipi-table-row--disabled` | Modifier per `RowDisabled` selector |
| Row hover | `lipi-table-row--hover` | Modifier on hover (CSS-only) |
| Row selected | `lipi-table-row--selected` | Modifier when in `SelectedItems` |
| Row focus | `lipi-table-row--focus` | Modifier when keyboard focus is on the row |
| Row status strip | `lipi-status-strip-left` + `data-status="..."` | Shared utility from `lipi-status-tokens.css` |
| Row checkbox cell | `lipi-table-cell-select` | Contains row checkbox |
| Row expand cell | `lipi-table-cell-expand` | Contains chevron for tree/master-detail |
| Data cell | `lipi-table-cell` | One per cell |
| Cell content | `lipi-table-cell-content` | Inner wrapper (allows padding control) |
| Cell ellipsis | `lipi-table-cell--ellipsis` | Modifier for overflow truncation |
| Cell copy icon | `lipi-table-cell-copy` | ⧉ icon, visible on hover when `Copyable=true` |
| Cell sort key | `lipi-table-cell--sort-anchor` | Modifier on the cell being sorted (subtle highlight) |
| Cell pin shadow | `lipi-table-cell--pinned-left` / `--pinned-right` | Inherited from column pin |
| Detail row | `lipi-table-row-detail` | Container for expanded detail panel |
| Detail content | `lipi-table-row-detail-content` | Caller's `<DetailTemplate>` rendered inside |
| Inline edit input | `lipi-table-cell-edit` | Wrapper around `LipiInputBase`-derived input |
| Inline edit Save | `lipi-table-row-edit-save` | Save button in row-edit mode |
| Inline edit Cancel | `lipi-table-row-edit-cancel` | Cancel button in row-edit mode |
| Inline edit error strip | `lipi-table-row-error` | Red banner above row when row-level validation fails |
| Concurrency conflict banner | `lipi-table-conflict-banner` | Red banner above row when row-version mismatch |
| Add-new-row marker | `lipi-table-row--new` | Modifier for the new-row pseudo-row at top |
| Skeleton row (loading) | `lipi-table-row-skeleton` | Composed from `LipiSkeleton` |
| Empty state | `lipi-table-empty` | Wraps `LipiEmptyState` in body |
| Error state | `lipi-table-error` | Wraps error UI in body |
| Filtered empty state | `lipi-table-filtered-empty` | Wraps filtered-empty UI in body |
| Tree indent | `lipi-table-cell-indent` | Indent block per tree depth level |
| Tree expand chevron | `lipi-table-cell-tree-chevron` | ▸ / ▾ on tree rows |

### §2.2.8 — Footer (region ⑧)

| Element | Class | Notes |
|---|---|---|
| Footer container | `lipi-table-footer` | Border-top, flex stack of footer sub-regions |
| Aggregate row | `lipi-table-footer-aggregate` | Per-column aggregates |
| Aggregate cell | `lipi-table-footer-aggregate-cell` | One per column with `AggregateFn` |
| Pagination wrapper | `lipi-table-footer-pagination` | Wraps `LipiPagination` |
| Row count | `lipi-table-footer-count` | "Showing 1-20 of 487" |
| Server-cap banner | `lipi-table-footer-cap-banner` | "Showing first 1000 of 47,381 — narrow filters" |

### §2.2.9 — Sticky edit bar (region ⑨)

| Element | Class | Notes |
|---|---|---|
| Container | `lipi-table-edit-stickybar` | Position:sticky bottom:0, full-width |
| Status label | `lipi-table-edit-stickybar-status` | "Editing Row #4" |
| Cancel button | `lipi-table-edit-stickybar-cancel` | `LipiButton` Secondary |
| Save button | `lipi-table-edit-stickybar-save` | `LipiButton` Primary |
| Discard confirm modal | `lipi-table-edit-discard-modal` | Composed `LipiModal` for discard prompt |

### §2.2.10 — Toolbar dropdowns / popovers / modals

These are children of the toolbar but render as overlays:

| Element | Class | Notes |
|---|---|---|
| Filter popover | `lipi-table-filter-popover` | Per-column filter UI, anchored to header filter icon |
| Filter popover op select | `lipi-table-filter-popover-op` | Operator dropdown |
| Filter popover input | `lipi-table-filter-popover-input` | Value input(s) |
| Filter popover actions | `lipi-table-filter-popover-actions` | Apply / Clear buttons |
| Filter drawer | `lipi-table-filter-drawer` | Full-page filter form (composed `LipiDrawer`) |
| Column picker popover | `lipi-table-picker-popover` | Drop list of columns with checkboxes + drag handles |
| Column picker item | `lipi-table-picker-item` | One column entry |
| Column picker reset | `lipi-table-picker-reset` | "Reset to defaults" button |
| Export dropdown | `lipi-table-export-menu` | CSV / PDF / Print / More options |
| Export modal | `lipi-table-export-modal` | Composed `LipiModal` for "More options" |
| Conflict diff popover | `lipi-table-conflict-popover` | All-fields diff |
| Conflict modal | `lipi-table-conflict-modal` | Composed `LipiModal` for Modal resolution mode |
| Conflict diff table | `lipi-table-conflict-diff` | The Original / Your / Theirs table |
| Conflict diff row diff | `lipi-table-conflict-diff-row--changed` | Modifier for differing fields (highlighted) |

---

## §2.3 — Modifier conventions

Modifiers follow BEM-style double-dash: `lipi-table-row--editing`. Boolean states use a modifier; variants of an element use a modifier; states that combine use multiple modifiers space-separated.

The full modifier vocabulary:

| Modifier | Applied to | Meaning |
|---|---|---|
| `--editing` | row | Row is currently in inline-edit mode |
| `--dirty` | row | Row has unsaved changes |
| `--disabled` | row | Per `RowDisabled` selector |
| `--hover` | row | (CSS-only; not for JS — added by `:hover` pseudo) |
| `--selected` | row | Row is in `SelectedItems` |
| `--focus` | row, cell | Keyboard focus is here |
| `--clickable` | row | `OnRowClick` is set; cursor:pointer |
| `--editable` | cell | Column is editable (cell-edit mode) |
| `--copying` | cell | Cell is briefly highlighted after copy-to-clipboard |
| `--ellipsis` | cell | Overflow truncation active |
| `--sort-anchor` | cell | Cell is in the column being sorted (subtle vertical highlight) |
| `--pinned-left` | header cell, cell | Pinned to left edge |
| `--pinned-right` | header cell, cell | Pinned to right edge |
| `--sortable` | header cell | Column is sortable; hover/focus styles apply |
| `--filterable` | header cell | Column is filterable; filter icon visible |
| `--filtered` | header cell | Column has an active filter |
| `--sorted-asc` | header cell | Sort direction ascending |
| `--sorted-desc` | header cell | Sort direction descending |
| `--resizing` | header cell | User is actively dragging the resize handle |
| `--dragging` | header cell | User is actively dragging the column to reorder |
| `--new` | row | The add-new-row pseudo-row at top of table |
| `--saving` | row | Row save in progress (server roundtrip) |
| `--conflict` | row | Row is in concurrency conflict state |
| `--expanded` | row | Master-detail or tree row is expanded |
| `--collapsed` | row | Master-detail or tree row is collapsed |
| `--group` | row | Row is a group header (not a data row) |

---

## §2.4 — Sub-component file map

For build reference, here's how the visual elements map to the `.razor` sub-components. The file list is in §27; this is the conceptual map:

| Visual region(s) | Sub-component file |
|---|---|
| Outer scaffold, region orchestration | `LipiTable.razor` (the top-level) |
| Region ⓪ Header band | (inline in LipiTable.razor; no separate file) |
| Region ① Toolbar | `LipiTableToolbar.razor` |
| Region ② Filter chips | `LipiTableFilterChips.razor` |
| Region ③ Bulk action bar | `LipiTableBulkActionBar.razor` |
| Region ④ Group bar | `LipiTableGroupBar.razor` |
| Region ⑤+⑧ᵦ Pagination | composes `LipiPagination` (no LipiTable-specific file) |
| Region ⑥ Column header | `LipiTableHeader.razor` |
| Region ⑦ Body — data rows | `LipiTableRow.razor` |
| Region ⑦ Body — edit rows | `LipiTableEditRow.razor` (or shares with LipiTableRow via modifier flag — TBD at build time) |
| Region ⑦ Body — group rows | `LipiTableGroupRow.razor` |
| Region ⑦ Body — detail rows | `LipiTableDetailRow.razor` |
| Region ⑦ Body — skeleton rows | `LipiTableSkeleton.razor` |
| Region ⑦ Conflict banner | `LipiTableConflictBanner.razor` |
| Region ⑦ Conflict popover | `LipiTableConflictDiffPopover.razor` |
| Region ⑧ₐ Aggregate row | `LipiTableAggregateRow.razor` |
| Region ⑨ Sticky edit bar | `LipiTableEditStickyBar.razor` |
| Region ⑩ Filter popover | `LipiTableFilterPopover.razor` |
| Region ⑩ Column picker | `LipiTableColumnPicker.razor` |
| Region ⑩ Export modal | `LipiTableExportModal.razor` |

This is **15 sub-components**. The decision (§27) is one .razor file per sub-component for buildability and reviewability. Single-file would be ~4500 lines.

---

## §2.5 — Z-index and stacking

LipiTable uses these stacking layers (low → high). Tokens defined in §24.

| Layer | Element | z-index token |
|---|---|---|
| 0 | Body rows (normal) | (default, no z-index) |
| 1 | Pinned columns (left + right) | `--lipi-table-z-pinned: 1` |
| 2 | Sticky column header | `--lipi-table-z-header: 2` |
| 3 | Pinned column AT sticky header intersection | `--lipi-table-z-pinned-header: 3` |
| 4 | Group row header (sticky-within-group when scrolling) | `--lipi-table-z-group: 4` |
| 5 | Sticky edit bar (region ⑨) | `--lipi-table-z-stickybar: 5` |
| 10 | Filter popover, column picker popover | `--lipi-table-z-popover: 10` |
| 20 | Conflict diff popover (in-table) | `--lipi-table-z-conflict-popover: 20` |
| 1000+ | Modals + drawers (LipiModal owns its own z-index — these compose) | (deferred to LipiModal / LipiDrawer) |

Pinned columns at sticky header intersection get the highest non-overlay z-index so that pinned cells stay above the header rule line during horizontal scroll while the header stays above body rows during vertical scroll.

---

## §2.6 — Responsive behavior

LipiTable is **not** responsive in the "mobile-first reflow" sense. It is designed for desktop and tablet-landscape viewports where horizontal scrolling is acceptable. The minimum supported viewport for full LipiTable functionality is **1024px wide**.

On viewports below 1024px:

- LipiTable does NOT collapse columns into a stacked card layout.
- LipiTable does NOT auto-hide columns.
- The toolbar wraps its zones onto multiple lines.
- The pagination footer wraps similarly.
- The body region permits horizontal scrolling; sticky header + pinned columns remain functional.

If a consuming page needs a mobile-collapsed view, the correct answer is to **swap LipiTable for LipiList on small viewports**. Both components share status tokens and persistence infrastructure, so the data layer is shared; only the presentation switches. This is the responsibility of the consuming page (e.g., using `MediaQuery` cascade or a `LipiResponsive` helper that may ship later). LipiTable does not own the responsive swap.

This decision aligns with industry practice — AG-Grid, Telerik, MudBlazor all behave the same way. A mobile-collapsed data grid is a different component, not the same component on a smaller screen.

The print stylesheet (§17.7) is a separate concern from responsive layout and renders differently (collapsed toolbar, no scroll, paginated by paper).

---

## §2.7 — Light + dark mode

Every element in §2.2 honors the LiPi token system (defined in §24). The following surfaces respond to light/dark mode:

- Toolbar background (light: surface; dark: elevated surface)
- Filter chip background (subtle, mode-aware)
- Bulk action bar background (primary-tint with mode-aware alpha)
- Group bar background (dashed border, mode-aware color)
- Column header background (subtle, mode-aware)
- Body row background (alternate-row striping optional via `StripedRows="true"` — both light and dark)
- Row hover background (mode-aware overlay)
- Row selected background (primary-tint with mode-aware alpha)
- Row focus ring (3px outline, primary color)
- Pinned column shadow (mode-aware drop shadow)
- Status strip color (read from `lipi-status-tokens.css` — already mode-aware)
- Inline-edit row background (subtle yellow tint in light, subtle warm tint in dark)
- Conflict banner background (danger-tint, mode-aware)
- Skeleton shimmer (gradient sweep, mode-aware base + highlight)
- Empty / Error / FilteredEmpty state — inherits from LipiEmptyState (mode-aware)

The CSS is structured so that switching the global theme (light↔dark) requires no LipiTable code changes — only token re-resolution. The mode-light.css / mode-dark.css token files own the actual color values.

---

*End of §2. Proceed to §3 — Generic typing and the column model.*


# LipiTable Spec — §3 Generic typing and the column model

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>` + `LipiColumn<TItem, TValue>`
**Status:** Section body — draft for review
**Depends on:** §1, §2

---

## §3.1 — `LipiTable<TItem>` signature

LipiTable is generic over `TItem` — the data type of one row. There is **no constraint on TItem** (no `class`, no `IComparable`, no `IEquatable`, no record requirement). Reference types, value types, records, structs, and anonymous types all work. Identity is established by the caller-provided `KeySelector`, not by reference equality or `Equals`.

### §3.1.1 — Declaration shape

```csharp
[CascadingTypeParameter(nameof(TItem))]
public partial class LipiTable<TItem> : ComponentBase
{
    // Constructor / lifecycle handled by Blazor; this class does not have
    // an explicit constructor in user-facing code.
}
```

The `[CascadingTypeParameter]` attribute is critical — without it, child `<LipiColumn>` components cannot infer `TItem` from the parent, and every column declaration would need an explicit `<LipiColumn TItem="Patient" ...>`. With it, only the table needs `TItem`, and columns infer.

### §3.1.2 — Usage shape

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           TableId="patients-list">
    <LipiColumn Field="@(p => p.Name)"        Header="Name" />
    <LipiColumn Field="@(p => p.DateOfBirth)" Header="DOB"  Type="ColumnType.Date" />
    <LipiColumn Field="@(p => p.Mobile)"      Header="Mobile" Copyable="true" />
    <LipiColumn Field="@(p => p.Status)"      Header="Status" Type="ColumnType.Status" />
</LipiTable>
```

Inside child columns, `TItem` is `Patient` automatically (cascaded). The closing tag is `</LipiTable>`; no special syntax.

### §3.1.3 — Required parameters

| Parameter | Type | Notes |
|---|---|---|
| `TItem` | type parameter | Specified on the opening tag |
| `KeySelector` | `Func<TItem, object>` | Returns a stable identity per row. Used for selection, edit-tracking, virtualization, persistence. May not return null. May return any non-null object — string, int, Guid, composite. Hard requirement; missing it throws `InvalidOperationException` in Development per the existing env-gated validation pattern, logs and renders fallback in Production. |
| **One of** `Items` **or** `DataSource` | `IReadOnlyList<TItem>?` or `Func<TableQueryRequest, Task<TableQueryResponse<TItem>>>?` | Exactly one must be non-null. If both are non-null, throw. If neither is non-null, render empty state with a developer-mode console warning. |

### §3.1.4 — Common optional parameters (top-level overview)

The complete parameter reference is at the end of this section (§3.8). Common groupings:

- **Identity & data:** `Items`, `DataSource`, `KeySelector`, `TableId`
- **Selection:** `SelectionMode`, `SelectedItems`, `SelectedItemsChanged`, `PreserveSelectionOnDataChange`
- **Sort & filter:** `DefaultSort`, `FilterMode`, `ShowQuickFilter`, `QuickFilterText`, `QuickFilterPredicate`
- **Pagination:** `DefaultPageSize`, `PageSizeOptions`, `PaginationPlacement`, `PaginationMode`, `ServerSideAllCap`
- **Grouping:** `GroupBy`, `ShowGroupBar`, `DefaultGroupState`
- **Tree:** `ChildrenSelector`, `ParentSelector`, `DefaultTreeState`, `TreeIndent`
- **Master-detail:** `MultiExpand`
- **Inline edit:** `EditMode`, `EditButtonPlacement`, `EditValidator`, `OptimisticUpdate`, `RowVersionSelector`, `ConflictResolutionMode`, `OnBeforeRowSave`, `OnRowSave`, `ShowAddButton`, `OnAddNew`
- **Visual:** `Density`, `ShowDensityToggle`, `StripedRows`, `RowClass`, `RowStyle`, `RowStatus`, `RowDisabled`, `Class`
- **Toolbar:** `Title`, `Subtitle`, `ShowColumnPicker`, `ShowExportButton`, `ShowRefreshButton`
- **States:** `IsLoading`, `LoadError`
- **Virtualization:** `Virtualize` (Auto / Always / Never)
- **Persistence:** `TableId` (also under identity)
- **Events:** `OnRowClick`, `OnRowDoubleClick`, `OnRowEditStart`, `OnRowEditEnd`, `OnCellEditStart`, `OnCellEditEnd`, `OnSortChanged`, `OnFilterChanged`, `OnPageChanged`, `OnPageSizeChanged`, `OnGroupChanged`, `OnRowExpand`, `OnRowCollapse`, `OnDirtyStateChanged`, `OnBeforeExport`, `OnAfterExport`, `OnError`

### §3.1.5 — RenderFragment slots

| Slot | Purpose |
|---|---|
| `ChildContent` | The `<LipiColumn>` children (default content slot) |
| `<HeaderTemplate>` | Full custom header replacing Title + Subtitle |
| `<ToolbarLeft>` | Custom content in left toolbar zone |
| `<ToolbarRight>` | Custom content in right toolbar zone |
| `<BulkActions>` | Caller's bulk action buttons (rendered when selection active) |
| `<DetailTemplate>` | Master-detail row content (`Context="row"`) |
| `<GroupHeaderTemplate>` | Group header row override (`Context="group"`) |
| `<EmptyTemplate>` | Empty state override |
| `<LoadingTemplate>` | Loading state override |
| `<ErrorTemplate>` | Error state override (`Context="error"`) |
| `<FilteredEmptyTemplate>` | Filtered-empty state override |
| `<AddRowTemplate>` | Inline add-new-row content (`Context="newRow"`) |
| `<ConflictTemplate>` | Custom concurrency conflict UI (`Context="conflictCtx"`) — when `ConflictResolutionMode.Custom` |

---

## §3.2 — `LipiColumn<TItem, TValue>` declaration

Columns are declared as **child components** inside the `<LipiTable>` markup. Each `<LipiColumn>` is itself a generic component that registers with its parent table during `OnInitialized` and contributes a column definition.

### §3.2.1 — Why declarative children, not a config array

The locked decision (Q-API design) is declarative-only. Reasoning:

- **Type-safety per column.** `<LipiColumn TItem="Patient" TValue="DateOnly">` lets the compiler verify `Field`, `CellTemplate`, `EditTemplate` against `DateOnly`. A config-array approach would type everything as `object` and lose this safety.
- **RenderFragment-friendly.** `<CellTemplate>`, `<EditTemplate>`, `<HeaderTemplate>`, `<FilterTemplate>` are Razor markup. They cannot be expressed in a C# config array without ugly delegate construction.
- **Discoverable.** A new developer reading a page sees `<LipiColumn Field="..." Type="...">` and knows what each column does. A config array hides the structure.
- **Standard Blazor pattern.** Matches the existing LipiSelect / LipiCombobox / LipiTabs declarative-children pattern.

The optional `Columns="..."` array parameter (covered briefly in the strategic-chat scoping discussion) is **not in v1.0**. If a real consumer needs dynamic column lists (e.g., user-configurable picklist columns), we revisit then.

### §3.2.2 — Generic signature

```csharp
public partial class LipiColumn<TItem, TValue> : ComponentBase
{
    [CascadingParameter] private LipiTable<TItem>? Parent { get; set; }
    
    // Identity
    [Parameter] public Expression<Func<TItem, TValue>>? Field { get; set; }
    [Parameter] public Func<TItem, TValue>? ValueSelector { get; set; }
    
    // Display
    [Parameter] public string? Header { get; set; }
    [Parameter] public ColumnType Type { get; set; } = ColumnType.Text;
    [Parameter] public RenderFragment<TItem>? CellTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    
    // Format
    [Parameter] public string? Format { get; set; }
    [Parameter] public Func<TValue, string>? FormatFn { get; set; }
    
    // Width
    [Parameter] public string? Width { get; set; }       // CSS length: "120px", "1.5fr", "minmax(100px, 1fr)"
    [Parameter] public string? MinWidth { get; set; }
    [Parameter] public string? MaxWidth { get; set; }
    
    // Sort / filter / group / edit / pin
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public IComparer<TValue>? SortComparer { get; set; }
    [Parameter] public bool Filterable { get; set; } = true;
    [Parameter] public RenderFragment<FilterContext<TValue>>? FilterTemplate { get; set; }
    [Parameter] public bool Groupable { get; set; } = false;
    [Parameter] public bool AllowGroup { get; set; } = false;
    [Parameter] public bool Editable { get; set; } = false;
    [Parameter] public RenderFragment<TItem>? EditTemplate { get; set; }
    [Parameter] public bool RequireConfirmEdit { get; set; } = false;
    [Parameter] public string? ConfirmEditMessage { get; set; }
    [Parameter] public ColumnPin Pinned { get; set; } = ColumnPin.None;
    [Parameter] public bool Resizable { get; set; } = true;
    [Parameter] public bool Reorderable { get; set; } = true;
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public bool Copyable { get; set; } = false;
    
    // Aggregation
    [Parameter] public LipiAggregate? AggregateFn { get; set; }
    [Parameter] public Func<IEnumerable<TValue>, object>? CustomAggregate { get; set; }
    [Parameter] public string? AggregateFormat { get; set; }
    
    // Alignment override
    [Parameter] public ColumnAlign? Align { get; set; }
    
    // Identification (for persistence + serialization)
    [Parameter] public string? ColumnKey { get; set; }   // explicit; falls back to Field expression
    
    // Lifecycle
    protected override void OnInitialized()
    {
        if (Parent is null)
            throw new InvalidOperationException(
                "LipiColumn must be a child of LipiTable.");
        Parent.RegisterColumn(this);
    }
    
    public void Dispose() => Parent?.UnregisterColumn(this);
}
```

### §3.2.3 — `Field` vs `ValueSelector`

Both produce the cell value. They serve different purposes:

| Parameter | Type | When to use |
|---|---|---|
| `Field` | `Expression<Func<TItem, TValue>>` | When the value comes from a single property/path. Provides free metadata: column key derived from the expression (e.g., `x => x.Name` → key `"Name"`), default Header text derived from property name, change tracking, persistence key. |
| `ValueSelector` | `Func<TItem, TValue>` | When the value is computed (e.g., `x => x.FirstName + " " + x.LastName`). No metadata extracted; column must provide explicit `Header` and `ColumnKey`. |

**Rules:**

- Exactly one of `Field` or `ValueSelector` must be set. If both are set, `Field` wins and `ValueSelector` is ignored (with a developer-mode warning).
- If neither is set AND `CellTemplate` is set, the column is "template-only" — it renders the template but has no sortable value, no filter, no group, no export value, no aggregate. Useful for action columns and decorative cells.
- If `Field` is set, `ColumnKey` defaults to the dotted property path (e.g., `x.Patient.Name` → `"Patient.Name"`). If `ValueSelector` or no field, `ColumnKey` must be supplied explicitly.

### §3.2.4 — `Header` resolution

In order of precedence:

1. `<HeaderTemplate>` (custom Razor; overrides everything else)
2. `Header="..."` parameter (string)
3. Derived from `Field` expression: property name with humanization (e.g., `DateOfBirth` → `"Date Of Birth"`, `firstName` → `"First Name"`)
4. `ColumnKey` value (last-resort fallback)

The humanization (camelCase / PascalCase → "Title Case With Spaces") uses a single utility in `Components/Shared/Internal/IdentifierHumanizer.cs`. Same humanizer used by LipiList field declarations.

### §3.2.5 — `ColumnKey` and persistence

`ColumnKey` is used as the per-column identifier in:

- Persisted user preferences (`identity.user_table_preferences`)
- Sort state, filter state, group state serialization
- Server-side `TableQueryRequest.SortDescriptors[].ColumnKey`
- Export header (when no Header is explicitly set)

The key must be **stable across deployments** for persistence to work. If a developer renames a property, the persisted state for that column is orphaned (still in DB, never read). This is benign — the user sees default column state on next visit. We do not attempt to migrate keys across renames.

Explicit `ColumnKey` overrides the derived key. Useful when refactoring property names without breaking persistence:

```razor
<LipiColumn Field="@(p => p.MobilePrimary)" ColumnKey="Mobile" Header="Mobile" />
```

After renaming `Mobile` → `MobilePrimary` in the model, the `ColumnKey="Mobile"` keeps existing user prefs working.

---

## §3.3 — Built-in column types (14 generic types)

The `Type` parameter on `<LipiColumn>` is an enum:

```csharp
public enum ColumnType
{
    Text,       // default
    Number,
    Currency,
    Date,
    DateTime,
    Time,
    Boolean,
    Mono,
    Status,
    Avatar,
    Link,
    File,
    Actions,
    Custom      // signals "I'm providing CellTemplate; don't pick a default renderer"
}
```

Per the **component isolation contract**, none of these types know about clinical concepts. Patient identifiers, vitals, allergies, etc. are rendered by callers using `Type="Custom"` with `<CellTemplate>` — they're not built-in types.

### §3.3.1 — Type behavior matrix

The 14 types differ along these facets. The matrix below summarizes; per-type detail follows.

| Type | TValue expected | Alignment | Sortable | Filter operators | Aggregate compat | Auto-width hint |
|---|---|---|---|---|---|---|
| Text | `string?` | left | yes | text set | count, distinct | flex 1.0 |
| Number | `int? long? double? decimal?` | right | yes | number set | sum, avg, count, min, max, distinct | 100–140px |
| Currency | `decimal? double?` | right | yes | number set | sum, avg, count, min, max | 120–160px |
| Date | `DateOnly? DateTime?` | center | yes | date set (full) | min, max, count | 110–130px |
| DateTime | `DateTime? DateTimeOffset?` | center | yes | date set (full) | min, max, count | 150–180px |
| Time | `TimeOnly? TimeSpan?` | center | yes | time set (subset of date) | min, max, count | 90–110px |
| Boolean | `bool? bool` | center | yes | bool set | count-true, count-false, count | 60–80px |
| Mono | `string?` | left | yes | text set | count, distinct | mono-font-derived |
| Status | `string?` | center | yes | set filter (distinct values) | count, distinct, count-per-value | 100–130px |
| Avatar | `string?` (URL/initials) | center | no | not filterable by default | count | 56px (fixed) |
| Link | any (rendered via `Href`) | left | yes (by value text) | text set | count | flex 1.0 |
| File | `LipiFileRef?` | left | no (by default) | not filterable | count | flex 1.0 |
| Actions | `void` (template-only) | right | no | no | no | content-sized |
| Custom | any | inherits | yes (if SortComparer) | inherits (if FilterTemplate) | inherits | flex 1.0 |

Alignment follows the standing rule for tables (§02-STANDING-RULES: first col left, S.No/Date center, rest center, actions right). `Align` parameter override is available per column.

### §3.3.2 — Per-type detail

#### Text
- **Default rendering:** `<span class="lipi-table-cell-content">{value ?? ""}</span>` with `--ellipsis` modifier when overflow.
- **Sort:** Culture-aware `StringComparer.CurrentCulture` (or invariant when `SortComparer` overrides). Empty / null sorts to end on ascending.
- **Filter operators:** contains, equals, startsWith, endsWith, notContains, empty, notEmpty. Case-insensitive by default; `FilterCaseSensitive="true"` parameter on column overrides.
- **Export:** raw string value, CSV-escaped per RFC 4180.
- **Copyable:** copies raw `value` (not the rendered HTML).
- **Format / FormatFn:** if set, `Format` is treated as `string.Format("{0:" + Format + "}", value)`; `FormatFn(value)` overrides Format if both are set.
- **Light/dark:** body text color (`--color-text-primary`); muted color (`--color-text-secondary`) when null/empty.

#### Number
- **Default rendering:** right-aligned mono-tabular numerals; localized thousands separator from current culture; respects `Format` (e.g., `"N0"`, `"N2"`, `"F1"`).
- **Sort:** numeric comparison; null sorts to end.
- **Filter operators:** equals, notEquals, greaterThan, greaterThanOrEqual, lessThan, lessThanOrEqual, between, empty, notEmpty.
- **Export:** raw numeric value (CSV: unquoted unless number contains thousands separators or scientific notation; PDF: formatted per culture).
- **Aggregate:** sum / avg / count / min / max / distinct supported.
- **Copyable:** copies the raw number as plain string (no thousand separators, no symbols). E.g., `15234.50` not `15,234.50`.

#### Currency
- **Default rendering:** right-aligned, prefix or suffix symbol per current culture (₹ for `hi-IN` / `en-IN`, $ for `en-US`, etc.); two decimals by default; mono-tabular numerals.
- **Sort:** numeric.
- **Filter operators:** same as Number.
- **Export:** raw decimal value (CSV); formatted with symbol (PDF / Print).
- **Aggregate:** sum / avg / count / min / max — output respects currency formatting.
- **Copyable:** raw decimal as plain string (`15234.50`). Symbol stripped.
- **Currency code parameter:** `CurrencyCode="INR"` on column overrides the culture default. Useful for multi-currency systems.

#### Date
- **Default rendering:** formatted via `IDateFormatService` (Phase 2.4 service); typically `DD/MM/YYYY` for India default. `Format` parameter overrides.
- **Sort:** chronological; null sorts to end.
- **Filter operators:** on, before, after, between, today, tomorrow, yesterday, thisWeek, lastWeek, nextWeek, thisMonth, lastMonth, nextMonth, thisQuarter, lastQuarter, thisYear, lastYear, lastNDays, nextNDays, empty, notEmpty.
- **Export:** ISO 8601 date in CSV (`2026-05-15`); formatted display in PDF / Print.
- **Aggregate:** min / max / count.
- **Copyable:** ISO 8601 (`2026-05-15`).

#### DateTime
- Same as Date, but also includes time component.
- **Default rendering:** `DD/MM/YYYY HH:mm` (24-hour) or culture-specified 12-hour with AM/PM.
- **Filter operators:** same date set (operates on date portion); plus optional time-of-day predicates if needed (`between` becomes range with time inputs).
- **Copyable:** ISO 8601 datetime (`2026-05-15T14:30:00+05:30`).

#### Time
- **Default rendering:** `HH:mm` (24-hour) by default; configurable via `IDateFormatService`.
- **Sort:** chronological by time.
- **Filter operators:** equals, before, after, between, empty, notEmpty.
- **Export:** ISO 8601 time (`14:30:00`) in CSV.
- **Copyable:** raw time string (`14:30`).

#### Boolean
- **Default rendering:** ✓ (filled circle or check mark, `--color-text-success`) for true; em-dash or blank for false/null. Optional `TrueLabel="Yes"` / `FalseLabel="No"` parameters render text labels instead.
- **Sort:** false → true → null.
- **Filter operators:** true, false, empty.
- **Aggregate:** count of true / count of false / total count.
- **Export:** "true"/"false" (CSV), or TrueLabel/FalseLabel if set.
- **Copyable:** "true"/"false" raw.

#### Mono
- **Default rendering:** monospace font (`--font-mono`), left-aligned, typically used for identifiers, codes, hashes, version strings, technical strings.
- **Sort:** string sort.
- **Filter operators:** same as Text (contains, equals, etc.).
- **Width hint:** auto-derived from longest visible value; mono-tabular makes this predictable.
- **Copyable:** raw value.
- The Mono type **does not auto-set Copyable=true.** Caller still must set `Copyable="true"` explicitly (per locked Approach A — generic, explicit opt-in).

#### Status
- **Default rendering:** `LipiBadge` (or `LipiPill` per column option `StatusVariant`) with text from `Field` value and color from `lipi-status-tokens.css` mapping. Lowercased value used as `data-status` attribute. Unknown status values fall back to `--color-status-unknown` (neutral grey).
- **Sort:** alphabetical on status text; or custom order via `StatusOrder` parameter (an `IReadOnlyList<string>` defining the sort priority).
- **Filter operators:** set filter only — multi-select chip list of distinct values. The set is computed from current data in client mode; passed from server in server mode.
- **Aggregate:** count, distinct, count-per-value (the last renders as a stacked bar or comma-separated list in the footer).
- **Copyable:** raw status string.

The Status type is the canonical use of the **shared status tokens** infrastructure (Phase 2.8 overview §2). It does not import any clinical taxonomy; consuming pages define their own status strings.

#### Avatar
- **Default rendering:** circular image (when value is a URL) OR initials circle (when value is a name string and `IsInitials="true"`). Diameter follows density: compact 24px, comfortable 32px, spacious 40px.
- **Sort:** disabled by default (avatars rarely have meaningful order).
- **Filter:** disabled by default (filter icon hidden).
- **Aggregate:** count.
- **Copyable:** disabled (avatars are visual; raw URL is rarely useful).
- **Background color for initials:** derived from a hash of the value, mapped to a palette of 8 muted colors. Same hash → same color (deterministic).
- **Fallback:** broken-image fallback shows a person silhouette icon.

#### Link
- **Default rendering:** `<a href="..." class="lipi-table-cell-link">{linkText}</a>`. The `Href` parameter on the column accepts `Func<TItem, string>` to compute per-row URL. Link text comes from `Field` / `ValueSelector`.
- **Click behavior:** standard `<a>` navigation; `Target` parameter (`_blank` / `_self`) controls. SPA-router-aware: when `Href` starts with `/` and is a known route, uses `NavigationManager.NavigateTo()` instead of full browser navigation.
- **Sort:** by displayed link text.
- **Filter operators:** same as Text.
- **Export:** link text (CSV); link text with URL in parentheses (PDF / Print).
- **Copyable:** link text by default; `CopyTarget="Href"` parameter copies the URL instead.

#### File
- **Default rendering:** file icon (per MIME type via `FileIconSelector`) + file name + size (formatted as "KB", "MB", "GB"). Click invokes `OnFileClick` callback — does NOT bake in any specific previewer (per component isolation contract).
- **Required parameters when Type="File":**
  - `FileNameSelector="@(x => x.FileName)"` — `Func<TItem, string>` for display name
  - `FileSizeSelector="@(x => x.SizeBytes)"` — `Func<TItem, long?>` (optional; if null, size not shown)
  - `FileIconSelector="@(x => x.MimeType)"` — `Func<TItem, string>` (optional; defaults to generic file icon)
  - `OnFileClick="@(item => ...)"` — `EventCallback<TItem>` invoked on click
- **Sort:** by file name.
- **Filter:** disabled by default; caller can supply `FilterTemplate` for custom file filters.
- **Export:** file name + size (CSV); same in PDF.
- **Copyable:** file name by default; `CopyTarget="Url"` copies a URL if `FileUrlSelector` is supplied.

The File type's `LipiFileRef` is **not a required model**. The caller's `TItem` can be anything — a `Patient` with a `FileName` property, a `LabResult` with attachment fields, anything. The Selector parameters extract the relevant strings from `TItem`. This keeps File domain-neutral.

#### Actions
- **Default rendering:** right-aligned horizontal flex of icon buttons / text buttons / divider. The cell's `CellTemplate` is **required** for Actions columns — there's no useful default. Pattern:

```razor
<LipiColumn Type="ColumnType.Actions" Header="">
    <CellTemplate Context="row">
        <LipiIconButton Icon="edit"   OnClick="@(() => Edit(row))" />
        <LipiIconButton Icon="trash"  OnClick="@(() => Delete(row))" Variant="ButtonVariant.Danger" />
    </CellTemplate>
</LipiColumn>
```

- **Sort:** disabled.
- **Filter:** disabled.
- **Export:** excluded from export by default. `IncludeInExport="false"` is set automatically.
- **Visible:** can be hidden via column picker (rare), but Actions columns are usually `Pinned="ColumnPin.Right"` so they survive horizontal scroll.

When `EditMode="Row"` is set on the table and no Actions column is declared, LipiTable **auto-injects** an Actions column at the right edge containing the pencil (Edit) icon plus the Save / Cancel buttons that appear during edit. If an Actions column IS declared, LipiTable merges the auto-edit-icon into it.

When `ShowAddButton="true"` and no toolbar Add button is rendered for any reason, LipiTable does **not** auto-inject anything into Actions. The Add button lives in the toolbar; Actions is row-scoped.

#### Custom
- **Signals** to LipiTable that the caller is providing `CellTemplate` and the built-in renderer should not be picked.
- **Sort:** disabled unless `SortComparer` is provided.
- **Filter:** disabled unless `FilterTemplate` is provided.
- **Aggregate:** disabled unless `CustomAggregate` is provided.
- **Export:** uses `Field` / `ValueSelector` value if available; if the value is non-string, calls `value?.ToString() ?? ""`.

The Custom type is the escape hatch. When a caller needs a cell that the 13 typed renderers can't express, Custom + CellTemplate handles it.

### §3.3.3 — Alignment summary (cross-reference)

Per the standing rule and the matrix above, here is the table-wide alignment summary that consuming pages and LipiTable defaults follow:

| Column scenario | Default alignment |
|---|---|
| First column | Left |
| First column is S.No / Date | Center; second column becomes Left |
| Text / Mono / Link / File | Left (unless first-column rule above) |
| Number / Currency | Right |
| Date / DateTime / Time / Boolean / Status / Avatar | Center |
| Actions | Right |

The `Align` parameter on `<LipiColumn>` overrides the default. Values: `Left`, `Center`, `Right`, `Inherit`.

---

## §3.4 — Custom cell templates

### §3.4.1 — `<CellTemplate>` — overrides built-in renderer

Per the locked decision (Q11.2), `<CellTemplate>` is available on **every** column type. When set, it replaces the built-in renderer.

```razor
<LipiColumn Field="@(p => p.Name)" Header="Name" Type="ColumnType.Text">
    <CellTemplate Context="patient">
        <strong>@patient.Name</strong>
        @if (patient.IsVip)
        {
            <LipiBadge Variant="warning" Text="VIP" />
        }
    </CellTemplate>
</LipiColumn>
```

When `CellTemplate` is set:
- The built-in renderer for that Type is not used.
- Sort still uses the `Field` value (or `ValueSelector`) — not the rendered HTML.
- Filter still uses the `Field` value.
- Export still uses the `Field` value (unless overridden — see `<ExportTemplate>` below if added later).
- Copyable still operates on the raw value, not the rendered HTML.

This separation matters: a `CellTemplate` is purely visual. Sort / filter / export / copy operate on the **underlying value**, so the user's mental model of "what's in this cell" stays consistent even when the visual is enriched.

### §3.4.2 — `<HeaderTemplate>` — overrides header text

```razor
<LipiColumn Field="@(p => p.Status)" Header="Status">
    <HeaderTemplate>
        <LipiIcon Name="activity" /> Status
    </HeaderTemplate>
</LipiColumn>
```

When set, replaces the header text. Sort indicator, filter icon, resize handle, pin indicator are still rendered by LipiTable — `HeaderTemplate` only owns the label area.

### §3.4.3 — `<EditTemplate>` — overrides edit input

```razor
<LipiColumn Field="@(p => p.DateOfBirth)" Editable="true">
    <EditTemplate Context="patient">
        <LipiDatePicker @bind-Value="patient.DateOfBirth" Required="true" />
    </EditTemplate>
</LipiColumn>
```

When set, replaces the default edit input. The default mapping (when EditTemplate is NOT set) is:

| ColumnType | Default edit input |
|---|---|
| Text | `LipiTextBox` |
| Number | `LipiNumberInput` |
| Currency | `LipiNumberInput` with currency formatter |
| Date | `LipiDatePicker` |
| DateTime | `LipiDateTimePicker` |
| Time | `LipiTimePicker` |
| Boolean | `LipiCheckbox` |
| Mono | `LipiTextBox` (mono font applied) |
| Status | `LipiSelect` — but caller MUST provide options via `StatusOptions` parameter; if not provided, falls back to free-text `LipiTextBox` with a developer-mode warning |
| Avatar / Link / File / Actions | Not editable by default; `Editable="true"` requires `<EditTemplate>` |
| Custom | Requires `<EditTemplate>` |

**Gotcha:** the default edit inputs are all `LipiInputBase`-derived components and integrate with the touched-state / EditContext validation flow (§12.6). If a caller supplies a custom `<EditTemplate>` with a non-LipiInputBase component, the per-field validation flow may not work — documented gotcha; caller's responsibility to ensure the custom input participates correctly.

### §3.4.4 — `<FilterTemplate>` — overrides filter UI

```razor
<LipiColumn Field="@(p => p.AgeYears)" Type="ColumnType.Number" Filterable="true">
    <FilterTemplate Context="ctx">
        <div>
            <span>Pediatric age band:</span>
            <button @onclick="@(() => ctx.SetFilter(o => o.IsBetween(0, 1)))">Infant</button>
            <button @onclick="@(() => ctx.SetFilter(o => o.IsBetween(1, 12)))">Child</button>
            <button @onclick="@(() => ctx.SetFilter(o => o.IsBetween(13, 18)))">Teen</button>
            <button @onclick="@(() => ctx.ClearFilter())">All</button>
        </div>
    </FilterTemplate>
</LipiColumn>
```

When set, replaces the built-in filter popover for that column. The `FilterContext<TValue>` provided to the template exposes:

- `CurrentOperator` — current filter operator
- `CurrentValue` — current filter value
- `SetFilter(...)` — apply a new filter
- `ClearFilter()` — clear this column's filter
- `Close()` — close the popover

---

## §3.5 — Column ordering, visibility, and pinning

### §3.5.1 — Initial order
Declaration order in markup. The first `<LipiColumn>` is the leftmost column. Order applies before pinning is computed.

### §3.5.2 — Initial visibility
`Visible="true"` (default). Set `Visible="false"` to declare a column that exists in the column picker but is hidden by default.

### §3.5.3 — Initial pinning
`Pinned="ColumnPin.None"` (default). Values: `None`, `Left`, `Right`. Pinned columns render at the left or right edge regardless of declaration order, and stick during horizontal scroll.

### §3.5.4 — User reorder (runtime)
- User drags column header to new position.
- Drag is constrained: pinned-left columns can only be reordered within the pinned-left group; pinned-right similarly; unpinned columns reorder within unpinned.
- Drop indicator (vertical blue line) shows insertion point.
- User can also drag a column INTO a pin region to pin it (or OUT to unpin).
- Reorder is disabled per-column via `Reorderable="false"`.
- Reorder is disabled globally via `AllowColumnReorder="false"` on the table.

### §3.5.5 — Column picker (runtime visibility)
Toolbar button opens a dropdown / popover listing all columns:

```
┌─ Columns ────────────────────────┐
│ Pinned left                       │
│   ☑ ⋮⋮ Select                     │
│ ☑ ⋮⋮ Name                         │
│ ☑ ⋮⋮ Role                         │
│ ☐ ⋮⋮ Department                   │
│ ☑ ⋮⋮ Status                       │
│ ☑ ⋮⋮ Last Active                  │
│ Pinned right                      │
│   ☑ ⋮⋮ Actions                    │
│                                   │
│ [Reset to defaults]               │
└───────────────────────────────────┘
```

- Checkbox toggles `Visible`.
- Drag handle (`⋮⋮`) reorders.
- Pin sub-menu per column (via right-click or pin icon) toggles pin state.
- "Reset to defaults" clears persisted state for this `TableId` for the current user.

The column picker is itself rendered as `LipiTableColumnPicker.razor` (§27).

### §3.5.6 — Persistence
When `TableId` is set, column order / visibility / pin / width are persisted per-user. On next page load, the persisted state is restored before rendering. The declaration order in markup is only the **default**; persisted state wins.

When the developer changes the markup (adds / removes / renames columns), the persistence handles each case:

| Markup change | Behavior |
|---|---|
| Added column | New column appears in default position (declaration order); persisted state ignored for that key |
| Removed column | Persisted entry for that key is orphaned; ignored on read |
| Renamed property (no explicit ColumnKey) | Treated as removed + added — user sees default state for the renamed column |
| Renamed property WITH explicit ColumnKey unchanged | Persistence honored; UI continues from where the user left it |

Mid-deployment churn (developer fixes a typo and redeploys) doesn't lose user state if `ColumnKey` is set. Without explicit ColumnKey, the developer is asking persistence to follow the source code, which it cannot do safely.

---

## §3.6 — Width handling

### §3.6.1 — CSS Grid backing
The table body uses CSS Grid (§2 anatomy). Each column contributes one grid track. The track size comes from the column's `Width` / `MinWidth` / `MaxWidth` parameters.

### §3.6.2 — Width parameter shapes
| Width value | Grid track value | Meaning |
|---|---|---|
| `"120px"` | `120px` | Fixed |
| `"1fr"` | `1fr` | Flex share |
| `"1.5fr"` | `1.5fr` | Flex share weighted higher |
| `"minmax(100px, 1fr)"` | `minmax(100px, 1fr)` | Flex with min |
| `"auto"` | `auto` | Content-sized |
| `null` (default) | Type-default (see matrix §3.3.1) | Type picks reasonable default |

### §3.6.3 — Resize behavior
- User drags right-edge resize handle of column header.
- During drag, grid track size updates live (preview).
- Release commits the new width and persists if `TableId` is set.
- Min width = `MinWidth` parameter OR a hard floor of 40px (no column can be narrower than 40px regardless).
- Max width = `MaxWidth` parameter OR unlimited.
- Resize is disabled per-column via `Resizable="false"`.
- Double-click handle → auto-fit to widest visible cell content (within Min/Max bounds).

### §3.6.4 — Auto-width hint
For columns with no explicit Width, the type's auto-width hint (matrix §3.3.1) is used. This is a hint, not a hard size:

- Numeric / Date types → `minmax(100px, auto)` — content can grow but starts compact.
- Text / Link → `minmax(120px, 1fr)` — flex with reasonable min.
- Avatar / Boolean → fixed small size.
- Actions → `auto` (content-sized).

The result is a sensible default layout for tables that don't specify widths, while allowing pages with specific width needs to override.

---

## §3.7 — Aggregation

### §3.7.1 — Built-in aggregates
```csharp
public enum LipiAggregate
{
    None,
    Sum,
    Avg,
    Count,           // count of rows (ignores Field value)
    CountNonNull,    // count of rows where Field value is non-null
    CountDistinct,
    Min,
    Max,
    First,           // first non-null value
    Last             // last non-null value
}
```

Set on column: `<LipiColumn Field="@(x => x.Total)" Type="ColumnType.Currency" AggregateFn="LipiAggregate.Sum" />`.

### §3.7.2 — Custom aggregate
`<LipiColumn ... CustomAggregate="@(values => values.Average() * 1.18m)" />` — function takes `IEnumerable<TValue>` and returns `object`. Output formatted via `AggregateFormat` parameter or default culture format.

### §3.7.3 — Aggregate compatibility per type
Already covered in matrix §3.3.1. Mismatched aggregates (e.g., Sum on Text) → developer-mode error.

### §3.7.4 — Aggregate placement
- **Footer aggregate row** (region ⑧ₐ from §2.1) — rendered whenever any column has `AggregateFn`.
- **Group-level aggregate** — rendered in group header row (when grouped). Same per-column AggregateFn computes per group.
- **Server-side aggregates** — when in server mode, aggregates come from `TableQueryResponse.Aggregates`. LipiTable does not re-compute on the client.

Detailed in §15.

---

## §3.8 — Complete `LipiColumn` parameter reference

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Field` | `Expression<Func<TItem, TValue>>?` | null | Value selector (preferred). Provides metadata. |
| `ValueSelector` | `Func<TItem, TValue>?` | null | Function-based value selector. No metadata. |
| `Header` | `string?` | derived | Column header text. |
| `Type` | `ColumnType` | `Text` | Built-in type for default rendering. |
| `CellTemplate` | `RenderFragment<TItem>?` | null | Override cell rendering. |
| `HeaderTemplate` | `RenderFragment?` | null | Override header label. |
| `EditTemplate` | `RenderFragment<TItem>?` | null | Override edit input. |
| `FilterTemplate` | `RenderFragment<FilterContext<TValue>>?` | null | Override filter UI. |
| `Format` | `string?` | null | Format string (passed to `string.Format`). |
| `FormatFn` | `Func<TValue, string>?` | null | Format function. Overrides Format. |
| `Width` | `string?` | type-default | CSS grid track size. |
| `MinWidth` | `string?` | null | CSS min size. |
| `MaxWidth` | `string?` | null | CSS max size. |
| `Sortable` | `bool` | type-default | Enable header sort. |
| `SortComparer` | `IComparer<TValue>?` | null | Custom sort comparator. |
| `Filterable` | `bool` | type-default | Enable filter icon / popover. |
| `Groupable` | `bool` | false | Allow GroupBy on this column. |
| `AllowGroup` | `bool` | false | Show in drag-to-group bar (Q4.1). |
| `Editable` | `bool` | false | Enable inline edit. |
| `RequireConfirmEdit` | `bool` | false | Show confirm dialog before save when this field changes. |
| `ConfirmEditMessage` | `string?` | "This field requires confirmation. Save change?" | Custom message for confirm. |
| `Pinned` | `ColumnPin` | `None` | Initial pin (`None`/`Left`/`Right`). |
| `Resizable` | `bool` | true | Allow resize. |
| `Reorderable` | `bool` | true | Allow reorder. |
| `Visible` | `bool` | true | Initial visibility. |
| `Copyable` | `bool` | false | Show hover-copy icon. |
| `CopyTarget` | `CopyTarget` | `Value` | What to copy on click: `Value` / `Href` / `Url` (type-dependent). |
| `AggregateFn` | `LipiAggregate?` | null | Built-in aggregate. |
| `CustomAggregate` | `Func<IEnumerable<TValue>, object>?` | null | Custom aggregate function. |
| `AggregateFormat` | `string?` | inherits | Format for aggregate display. |
| `Align` | `ColumnAlign?` | null (=auto) | Override default alignment. |
| `ColumnKey` | `string?` | derived from Field | Persistence key. |
| `IncludeInExport` | `bool` | true (false for Actions) | Whether export includes this column. |
| `CurrencyCode` | `string?` | culture-default | For Currency type. |
| `StatusVariant` | `StatusVariant` | `Badge` | For Status type: `Badge` or `Pill`. |
| `StatusOrder` | `IReadOnlyList<string>?` | null | Custom sort order for Status. |
| `StatusOptions` | `IReadOnlyList<string>?` | null | Options for Status edit. |
| `IsInitials` | `bool` | false (Avatar) | Treat value as name → initials. |
| `Href` | `Func<TItem, string>?` | null (Link) | Per-row URL. |
| `Target` | `string?` | null (Link) | `_blank` / `_self`. |
| `FileNameSelector` | `Func<TItem, string>?` | null (File) | Display name. |
| `FileSizeSelector` | `Func<TItem, long?>?` | null (File) | Size in bytes. |
| `FileIconSelector` | `Func<TItem, string>?` | null (File) | MIME type for icon. |
| `FileUrlSelector` | `Func<TItem, string>?` | null (File) | URL for copy/open. |
| `OnFileClick` | `EventCallback<TItem>` | (none) | File click handler. |
| `TrueLabel` | `string?` | null (Boolean) | Label for true; default ✓. |
| `FalseLabel` | `string?` | null (Boolean) | Label for false; default em-dash. |
| `FilterCaseSensitive` | `bool` | false (Text/Mono) | Case-sensitive filtering. |

About 50 parameters total. About 15 are universal (Field, Header, Type, etc.); the rest are type-specific and ignored when irrelevant.

---

## §3.9 — Supporting types reference

The following supporting types are introduced by §3 and live in `Components/Shared/LipiTableTypes.cs`:

```csharp
public enum ColumnType
{
    Text, Number, Currency, Date, DateTime, Time, Boolean,
    Mono, Status, Avatar, Link, File, Actions, Custom
}

public enum ColumnPin { None, Left, Right }
public enum ColumnAlign { Left, Center, Right }
public enum CopyTarget { Value, Href, Url }
public enum StatusVariant { Badge, Pill }

public sealed class FilterContext<TValue>
{
    public FilterOperator CurrentOperator { get; init; }
    public TValue? CurrentValue { get; init; }
    public Action<Action<FilterBuilder<TValue>>> SetFilter { get; init; } = default!;
    public Action ClearFilter { get; init; } = default!;
    public Action Close { get; init; } = default!;
}

public sealed class FilterBuilder<TValue>
{
    // Fluent API for constructing filter predicates from FilterTemplate
    public FilterBuilder<TValue> IsEqualTo(TValue value);
    public FilterBuilder<TValue> Contains(string substring);
    public FilterBuilder<TValue> IsBetween(TValue min, TValue max);
    public FilterBuilder<TValue> IsBefore(TValue value);
    public FilterBuilder<TValue> IsAfter(TValue value);
    public FilterBuilder<TValue> IsIn(IEnumerable<TValue> values);
    public FilterBuilder<TValue> Predicate(Func<TValue, bool> predicate);
    // ... etc
}
```

The full type listing is in §27 file inventory. Other supporting types (`TableQueryRequest`, `TableQueryResponse`, `SortDescriptor`, `FilterDescriptor`, `GroupDescriptor`, `SaveResult`, `RowEditContext`, `ConflictContext`, etc.) are introduced as their sections cover them — §4 for query types, §12 for edit types, etc.

---

*End of §3. Proceed to §4 — Data sources.*


# LipiTable Spec — §4 Data sources

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2, §3

---

## §4.1 — The two data-source models

LipiTable supports two data-source models. The locked decision (Q1.1 = C) is that both are equal-weight — neither is the primary with the other as a fallback. Caller picks per page based on data shape.

### §4.1.1 — Client-side mode (`Items`)

The caller passes the **full dataset** as a property:

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)">
    <LipiColumn Field="@(p => p.Name)" Header="Name" />
    <LipiColumn Field="@(p => p.Status)" Header="Status" Type="ColumnType.Status" />
</LipiTable>
```

`Items` is `IReadOnlyList<TItem>?`. LipiTable handles sort / filter / page / group / aggregate **entirely in memory**.

**When to use client-side:**
- Dataset ≤ ~1000 rows
- Caller already has the full data (e.g., from a single API call on page load)
- Caller wants client-side responsiveness for filtering / sorting (no server round-trip per interaction)
- Settings tables, admin pages, picklists, master-data views

**What client-side does NOT do:**
- It does NOT lazy-load. The caller is responsible for fetching the full dataset and passing it.
- It does NOT page-fetch from a server. All paging is in-memory slicing.
- It does NOT page-virtual-load. Virtualization (§20) still operates over the full in-memory list.

### §4.1.2 — Server-side mode (`DataSource`)

The caller provides a **callback** that LipiTable invokes whenever the table state changes:

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list">
    <LipiColumn Field="@(p => p.Name)" Header="Name" />
    <LipiColumn Field="@(p => p.Status)" Header="Status" Type="ColumnType.Status" />
</LipiTable>

@code {
    private async Task<TableQueryResponse<Patient>> LoadPatientsAsync(
        TableQueryRequest request,
        CancellationToken ct)
    {
        var (rows, total) = await _patientService.QueryAsync(request, ct);
        return new TableQueryResponse<Patient>(rows, total);
    }
}
```

LipiTable invokes the callback whenever sort, filter, page, page-size, group, or quick-search changes. The callback returns the page of rows + total count for the current state.

**When to use server-side:**
- Dataset > ~1000 rows
- Dataset is unbounded or unknown size
- Sort / filter / group must execute on the server (database-indexed, query-optimized)
- Concurrent access — many users querying the same table where each sees a fresh server-computed slice
- Clinical worklists, audit logs, billing detail, full patient lists

**Server-side contract:**
- LipiTable sends a `TableQueryRequest` describing the current state.
- Server returns `TableQueryResponse<TItem>` with rows + total count + optional aggregates.
- Server is responsible for applying sort / filter / page / group to the underlying data source (DB query, API, etc.).
- LipiTable does NOT re-sort / re-filter the returned rows. Whatever order the server returns is the order rendered.

### §4.1.3 — Mutually exclusive

Exactly one of `Items` or `DataSource` must be non-null on each `LipiTable` instance. Three failure modes:

| Scenario | Behavior |
|---|---|
| Both `Items` and `DataSource` are non-null | Throws `InvalidOperationException` at parameter-set time. Development environment throws hard per the env-gated validation pattern; Production logs the error and falls back to `Items` (with `DataSource` ignored). |
| Neither `Items` nor `DataSource` is non-null | Renders empty state. Developer-mode console warning: "LipiTable has no data source. Pass Items or DataSource." |
| Switching at runtime (Items → DataSource or back) | Treated as a full reset. Existing selection / sort / filter / edit state — see §4.4. |

### §4.1.4 — API surface symmetry

Every other LipiTable parameter — column declarations, selection, editing, density, persistence, virtualization — has **identical behavior** in both modes. The caller picks the data-source mode independently of every other concern.

This means a consuming page can refactor from client-side to server-side (or back) by changing only the data-source parameter. Column markup, event handlers, persistence keys all stay the same.

The only behaviors that differ between modes are summarized in §4.5.

---

## §4.2 — `TableQueryRequest`

The shape sent to the server-side `DataSource` callback. Lives in `Components/Shared/LipiTableQuery.cs`.

### §4.2.1 — Type definition

```csharp
public sealed record TableQueryRequest
{
    // Pagination
    public int Page { get; init; }                  // 1-indexed
    public int PageSize { get; init; }              // current page size
    public bool RequestAllRows { get; init; }       // true when user picked "All"
    
    // Sort
    public IReadOnlyList<SortDescriptor> Sort { get; init; } = Array.Empty<SortDescriptor>();
    
    // Filters
    public IReadOnlyList<FilterDescriptor> Filters { get; init; } = Array.Empty<FilterDescriptor>();
    public string? QuickSearch { get; init; }       // free-text search across all string columns
    
    // Grouping
    public IReadOnlyList<GroupDescriptor> Groups { get; init; } = Array.Empty<GroupDescriptor>();
    
    // Tree (when ChildrenSelector or ParentSelector is set)
    public object? ExpandedParentKey { get; init; } // when lazy-loading a tree node's children
    public int? TreeDepth { get; init; }            // 0 = root, 1 = first level, etc.
    
    // Selection intent
    public bool SelectAllAcrossPages { get; init; } // signals "user wants all rows selected, not just current page"
    
    // Aggregates
    public IReadOnlyList<string> AggregateColumns { get; init; } = Array.Empty<string>();
    
    // Locale / culture hints for server-side formatting (rarely used)
    public string? CultureName { get; init; }
    
    // Caller-extensible payload
    public IReadOnlyDictionary<string, object?> Extra { get; init; } = ImmutableDictionary<string, object?>.Empty;
}
```

### §4.2.2 — `SortDescriptor`

```csharp
public sealed record SortDescriptor(
    string ColumnKey,
    SortDirection Direction,    // Asc | Desc
    int Priority                // 0 = primary; 1 = secondary; etc.
);
```

When multi-column sort is active, the list is ordered by priority. Single-column sort always has exactly one descriptor with priority 0.

### §4.2.3 — `FilterDescriptor`

```csharp
public sealed record FilterDescriptor(
    string ColumnKey,
    FilterOperator Operator,    // see enum below
    object? Value,              // primary filter value
    object? ValueEnd            // secondary value (for Between / IsInRange)
);

public enum FilterOperator
{
    // Text
    Contains, NotContains, Equals, NotEquals, StartsWith, EndsWith,
    
    // Number / date generic
    GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Between,
    
    // Date relative
    Today, Tomorrow, Yesterday,
    ThisWeek, LastWeek, NextWeek,
    ThisMonth, LastMonth, NextMonth,
    ThisQuarter, LastQuarter,
    ThisYear, LastYear,
    LastNDays, NextNDays,
    
    // Boolean
    IsTrue, IsFalse,
    
    // Set
    In,                         // Value is IReadOnlyList<object>
    
    // Universal
    Empty, NotEmpty
}
```

`Value` is `object?` because filter values vary per column type. Server-side handlers cast based on the column's declared type. For `Between`, `Value` is the lower bound and `ValueEnd` is the upper. For `LastNDays` / `NextNDays`, `Value` is the `int` count.

### §4.2.4 — `GroupDescriptor`

```csharp
public sealed record GroupDescriptor(
    string ColumnKey,
    SortDirection Direction = SortDirection.Asc
);
```

When multi-level grouping is active, the list is ordered top-to-bottom (first descriptor = outermost group). Direction controls the order of group headers within the parent group.

### §4.2.5 — Idempotency
The same `TableQueryRequest` with the same data should produce the same `TableQueryResponse`. Server implementers should treat this as a query, not a command. LipiTable may invoke the callback multiple times for the same state during reconciliation; the server must not have side effects.

### §4.2.6 — `Extra` payload
The optional `Extra` dictionary lets consuming pages pass page-specific state through the query (e.g., a department filter that the page wires up outside LipiTable's filter UI). LipiTable round-trips `Extra` opaquely — passed in via parameter on the table, sent to the callback, available on the server.

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           QueryExtra="@(_pageContext.ToDictionary())">
    ...
</LipiTable>
```

Pattern: when extra context changes (e.g., user switches department dropdown above the table), the `QueryExtra` parameter changes, LipiTable detects the change, and the callback fires with the new value.

---

## §4.3 — `TableQueryResponse<TItem>`

The shape returned by the server-side callback.

### §4.3.1 — Type definition

```csharp
public sealed record TableQueryResponse<TItem>(
    IReadOnlyList<TItem> Rows,
    long TotalCount
)
{
    // Optional fields
    public IReadOnlyDictionary<string, object?>? Aggregates { get; init; }
    public IReadOnlyList<GroupBucket<TItem>>? PreGrouped { get; init; }
    public IReadOnlyDictionary<object, IReadOnlyList<TItem>>? TreeChildren { get; init; }
    public long? FilteredCount { get; init; }
    public string? CapBanner { get; init; }
    public IReadOnlyDictionary<string, object?>? ServerExtra { get; init; }
}
```

### §4.3.2 — `Rows`
The page of rows for the current state. Order is preserved by LipiTable (no re-sort).

### §4.3.3 — `TotalCount`
The total number of rows in the underlying dataset matching the **current filters** but ignoring pagination. Used by:
- Pagination control to compute page count
- Row count display ("Showing 1-20 of 487")
- "Select all across pages" banner

If the server cannot compute `TotalCount` cheaply (e.g., very large search index), return `-1` to signal "unknown total." LipiTable then renders:
- Pagination as "Page N" with no "of M"
- Row count as "Showing 1-20 of many"
- "Select all" banner offers "Select all loaded" instead of "all N"

### §4.3.4 — `Aggregates`
Optional. When the table has columns with `AggregateFn`, LipiTable sends `AggregateColumns` in the request listing which column keys need aggregates. Server computes and returns them keyed by column key:

```csharp
Aggregates = new Dictionary<string, object?>
{
    ["TotalAmount"] = 4_28_500.00m,
    ["LineCount"] = 47L
}
```

Footer aggregate row renders the values, formatted per the column's `AggregateFormat`. If `Aggregates` is null but the table has aggregate columns, LipiTable renders "—" in those footer cells and logs a developer-mode warning.

### §4.3.5 — `PreGrouped`
Optional. When grouping is active, the server can return rows pre-grouped instead of flat:

```csharp
public sealed record GroupBucket<TItem>(
    string ColumnKey,
    object? GroupValue,
    long ItemCount,
    IReadOnlyList<TItem> Items,
    IReadOnlyDictionary<string, object?>? Aggregates,
    IReadOnlyList<GroupBucket<TItem>>? SubGroups
);
```

When `PreGrouped` is non-null, LipiTable renders the supplied tree directly. When `PreGrouped` is null but Groups are active in the request, LipiTable groups the flat `Rows` client-side (which only makes sense when the rows include all data for the current page — see §4.6 server-side grouping caveat).

### §4.3.6 — `TreeChildren`
Optional. When tree data is active (`ChildrenSelector` or `ParentSelector` set, see §10) and the request carries `ExpandedParentKey`, this is the response with that parent's children.

Server returns one or more pages of children. LipiTable expects:

```csharp
TreeChildren = new Dictionary<object, IReadOnlyList<Patient>>
{
    [parentPatientId] = childPatients
}
```

Lazy-loaded tree expansion calls `DataSource` once per expand operation, sending `ExpandedParentKey`. Initial load returns the root level only.

### §4.3.7 — `FilteredCount`
Optional. When the dataset has more rows than match current filters, `FilteredCount` reports the filtered count and `TotalCount` may report the unfiltered total. Used in advanced UIs to show "47 of 4,872 patients" (filtered of total). Most cases use only `TotalCount`; `FilteredCount` is set only when the consuming page wants the unfiltered-total display.

### §4.3.8 — `CapBanner`
Optional. When the user selects "All" and the server-side cap (`ServerSideAllCap`, default 1000) kicks in, the server can return a banner string to display:

```csharp
CapBanner = "Showing first 1000 of 47,381 — narrow your filters to see more"
```

If `CapBanner` is null, LipiTable renders a default banner using `TotalCount` and `Rows.Count`. Setting the banner explicitly lets the server craft a more accurate / localized message.

### §4.3.9 — `ServerExtra`
Round-trip payload. Whatever the server includes here is passed to the consuming page via the `OnQueryComplete` event for any side-channel use (e.g., server-computed statistics, server-side warnings to display elsewhere). LipiTable itself does not interpret `ServerExtra`.

---

## §4.4 — State preservation across data changes

What persists across `Items` reference change or `DataSource` re-invocation:

| State | Client-side (`Items` changes) | Server-side (`DataSource` re-invoked) |
|---|---|---|
| Selection | Persists if `PreserveSelectionOnDataChange="true"` (default true). Keys not in new data are dropped silently. | Persists by key. Server-side "select all across pages" intent persists. |
| Sort | Persists | Persists; new request issued with same sort |
| Filter | Persists | Persists; new request issued with same filters |
| Page | Persists if total ≥ current page * page size; else snaps to last valid page | Same |
| Group | Persists | Same |
| Tree expand state | Persists for keys still in new data | Persists for keys; lazy-load may re-fire for expanded but evicted nodes |
| Column widths / order / visibility / pin | Persists (orthogonal to data) | Persists |
| In-progress edit (dirty row) | Discard confirm fires before data swap (LipiDynamicTabs pattern, §12.8) | Same |
| Bulk action state | Auto-clears when selection clears | Same |
| Quick search text | Persists | Persists |

The general rule: **table state belongs to the table; data is just what flows through it**. Replacing the data doesn't reset the table's view configuration.

---

## §4.5 — Differences between client-side and server-side

While the API surface is symmetric, the internal behavior differs in a few places. This table is for build-chat / spec reviewers — consuming pages don't need to think about most of this.

| Behavior | Client-side | Server-side |
|---|---|---|
| Sort execution | In-memory via `SortComparer` per column (defaults: culture-aware string, numeric, chronological) | Sent in `TableQueryRequest.Sort`; server executes |
| Filter execution | In-memory predicate per column type | Sent in `TableQueryRequest.Filters`; server executes |
| Quick search match | Built-in: case-insensitive contains across all string-renderable columns; or caller's `QuickFilterPredicate` | Sent in `TableQueryRequest.QuickSearch`; server executes |
| Group execution | In-memory `GroupBy` on the full dataset | Either: pre-grouped in `TableQueryResponse.PreGrouped`, OR flat `Rows` with `Groups` descriptor (and the server groups during query) |
| Aggregate execution | In-memory (`Sum`, `Avg`, etc. via LINQ over the filtered dataset, NOT the current page) | Server-computed; returned in `TableQueryResponse.Aggregates` |
| Pagination | In-memory slicing of filtered + sorted dataset | Server returns one page at a time |
| Virtualization | Operates over the full in-memory list (typically ≤ 1000 rows) | Operates over the current page (virtualized within page) |
| "Select all across pages" | Trivially selects all `Items` | Sends `SelectAllAcrossPages=true`; server identifies the matching keys server-side (caller's responsibility — see §5.3 for details) |
| Refresh | Re-render with same `Items` reference (or new reference) | Re-invokes `DataSource` callback with same request |
| `Items` change detection | Reference equality on `Items` parameter | (N/A — DataSource is the source) |
| `"All"` page size cap | No cap (all rows already in memory) | `ServerSideAllCap` parameter (default 1000) enforced |
| Initial load | First render with `Items` value | First render fires `DataSource` callback automatically |
| Empty vs FilteredEmpty | If `Items.Count == 0` → Empty. If filters drop all rows → FilteredEmpty. | If `TotalCount == 0` AND no filters active → Empty. If `TotalCount == 0` AND filters active → FilteredEmpty. |
| Loading state | Only when caller toggles `IsLoading="true"` manually | Automatic when callback is in-flight |
| Error state | When caller passes `LoadError="..."` manually | Automatic when callback throws |

### §4.5.1 — Loading state in server-side mode

When the `DataSource` callback is in-flight, LipiTable automatically shows the loading state (region ⑦ body — skeleton rows). The previous page's data remains visible during the load by default; setting `ShowLoadingOverlayDuringFetch="true"` instead grays out the body and shows a centered spinner.

Subsequent fetches (page change, sort change, filter change) re-trigger loading. First fetch after mount also triggers loading.

### §4.5.2 — Error handling in server-side mode

If the `DataSource` callback throws, LipiTable catches the exception, renders the error state, and fires `OnError`. The previous page's data remains visible behind the error overlay by default; setting `ClearOnError="true"` instead replaces the body with the error state fully.

The user's recovery path is "Retry" — a button on the error state re-fires the callback with the same `TableQueryRequest`. No automatic retry / backoff is built in.

### §4.5.3 — Cancellation

LipiTable passes a `CancellationToken` to the callback. The token is canceled when:
- The user changes state again before the previous fetch completes (filter / sort / page change while a request is in-flight)
- The component is disposed (page navigation away)
- The caller explicitly invokes `tableRef.CancelPendingFetchAsync()`

Server-side implementers should honor the token; LipiTable doesn't wait for cancellation to complete — once a newer fetch fires, the older one's result is discarded regardless of its return.

---

## §4.6 — Server-side grouping caveat

If grouping is active server-side, the page semantics get tricky. Two approaches the server can take:

### Approach 1 — Pre-grouped pages
Server returns `PreGrouped` with one or more group buckets. LipiTable renders them as-is.

Pros: server controls the grouping; consistent regardless of page boundaries.
Cons: page-size meaning becomes "number of groups returned" or "number of rows returned" depending on server interpretation.

### Approach 2 — Flat pages with Groups descriptor
Server returns flat `Rows` matching the current page, with the understanding that LipiTable will group them client-side.

Pros: simpler server contract.
Cons: a page boundary can split a group in half ("Active (12)" appears on page 1 with 8 rows; another "Active (12)" on page 2 with 4 rows). Confusing.

### LipiTable's recommendation
Use **Approach 1 (pre-grouped)** for server-side grouped tables. The `TableQueryResponse.PreGrouped` field exists for this. Pagination then operates on groups, not rows, and the cursor / page concept becomes "which slice of the group tree to load."

For server-side tables without grouping (the more common case), flat `Rows` + `TotalCount` is sufficient. No `PreGrouped` needed.

The spec documents both approaches; the choice is the server implementer's. LipiTable accepts either response shape.

---

## §4.7 — Refresh and reload

### §4.7.1 — `tableRef.RefreshAsync()`
Programmatic refresh. In client-side mode, triggers a re-render with the current `Items` (caller must have updated the reference if data has changed). In server-side mode, re-invokes the `DataSource` callback with the current `TableQueryRequest`.

### §4.7.2 — Toolbar refresh button
Visible when `ShowRefreshButton="true"` (default true in server-side mode, default false in client-side). User clicks → same effect as `RefreshAsync()`.

The button shows a rotation animation during in-flight fetch.

### §4.7.3 — Automatic refresh on parameter change
The following parameter changes automatically trigger refresh:
- `Items` reference change (client-side)
- `QueryExtra` dictionary change (server-side)
- Any change to declared columns (added / removed / property change)
- `TableId` change (treated as a fresh start)

The following are debounced (300ms) before refresh:
- Sort change
- Filter change
- Page change
- Page size change
- Group change

Debouncing prevents a burst of fetches when the user makes multiple rapid changes (e.g., shift-clicking three column headers in quick succession to set multi-sort).

### §4.7.4 — Auto-refresh interval (optional)
`AutoRefreshInterval="@TimeSpan.FromSeconds(30)"` — if set, LipiTable refreshes on a timer.

Restrictions:
- Auto-refresh **pauses** when a row is in edit mode (preventing data swap during a user's active edit).
- Auto-refresh **pauses** when the user is interacting with a popover (filter, picker).
- Auto-refresh **does not** pause when the user is just scrolling or has the table focused.

Default = `null` (no auto-refresh). Consuming pages that want polling enable it explicitly.

This is the only "real-time data" feature in v1.0. Push-based subscriptions (SignalR, server-sent events) are deferred per §1.5.

---

## §4.8 — Data immutability assumption

LipiTable assumes `Items` (client-side) and `Rows` (server-side response) are **immutable collections**. Mutating the underlying list after passing it to LipiTable is undefined behavior.

To update data:
- **Client-side**: caller creates a new `Items` reference and passes it. LipiTable detects the reference change and re-renders.
- **Server-side**: caller invokes `tableRef.RefreshAsync()` after the server data has changed.

LipiTable does NOT subscribe to `INotifyCollectionChanged` / `ObservableCollection<T>` mutations. The reason is performance: change-tracking individual mutations to a 1000-row list across selection / sort / filter / virtualization state would multiply the per-mutation work. Reference-replace + LipiTable re-render is cheaper and clearer.

If the caller's data layer uses mutable collections, the caller wraps them: `Items="@_patients.ToList()"` creates a snapshot every render. (Be aware of allocation cost; for large lists, prefer a stable reference that's replaced only when data actually changes.)

---

## §4.9 — Initial state on mount

Per Q1.1 = C and the symmetric API model:

| Step | Client-side | Server-side |
|---|---|---|
| 1 | Read persisted user prefs (sort / filter / group / density / page size / column state) | Same |
| 2 | Apply persisted state OR defaults | Same |
| 3 | Render header + toolbar + empty body skeleton (during step 4) | Same |
| 4 | Compute first view of data | Invoke `DataSource` with the computed initial `TableQueryRequest` |
| 5 | Render rows | Render rows from response |
| 6 | Fire `OnQueryComplete` (with metadata about the first fetch) | Same |

The user sees the toolbar / header / skeleton within ~50ms (limited by Blazor render cycle). The body fills in within ~200ms (client-side) or whatever the server RTT is (server-side).

### §4.9.1 — `DefaultSort`, `DefaultFilters`, `DefaultGroups` parameters
When persistence has no state (first visit OR `TableId` not set OR Reset was clicked), these parameters seed the initial state:

```razor
<LipiTable TItem="Patient"
           DefaultSort="@(new[] { new SortDescriptor(nameof(Patient.UpdatedAt), SortDirection.Desc, 0) })"
           DefaultFilters="@(new[] { new FilterDescriptor(nameof(Patient.IsActive), FilterOperator.IsTrue, true, null) })"
           ...>
```

When persisted state is present, defaults are ignored — the user's last state wins.

---

## §4.10 — `OnQueryComplete` event

Fires after each successful data fetch (client-side: after the in-memory pipeline; server-side: after callback returns successfully).

```csharp
public sealed record TableQueryCompleteContext<TItem>(
    TableQueryRequest Request,
    TableQueryResponse<TItem> Response,
    TimeSpan Duration,
    bool FromCache
);

[Parameter] public EventCallback<TableQueryCompleteContext<TItem>> OnQueryComplete { get; set; }
```

Use cases:
- Page wants to log query timing for performance monitoring
- Page wants to display "Last refreshed: 12 seconds ago"
- Page wants to act on server-side metadata returned via `ServerExtra`
- Page wants to compute its own analytics outside the table

`FromCache` is currently always `false` (LipiTable does not cache queries in v1.0). The field is reserved for a future amendment.

---

## §4.11 — Practical decision guide for consuming pages

When picking client-side vs server-side, the consuming-page developer should consider:

| Question | Client-side wins | Server-side wins |
|---|---|---|
| Dataset size | ≤ 1000 rows | > 1000 rows or unbounded |
| Can the data fit in a single API call? | yes | no |
| Does the data change frequently across users? | rarely | often (concurrent edits) |
| Does the user filter / sort a lot? | yes (no server hits) | doesn't matter |
| Is the data indexed / searchable on the server? | doesn't matter | yes (use the index) |
| Are there cross-cutting filters from other UI (date range, department dropdown)? | works in either; client-side requires the full list to be re-fetched on change | works in either; server-side uses `QueryExtra` |
| Does the page need real-time-ish updates? | trickier (caller must re-fetch and pass new `Items`) | natural fit (`AutoRefreshInterval` or manual refresh) |
| Initial-paint latency matters | client-side: needs full dataset first | server-side: first page only |

For LiPi specifically:
- Admin tables (Users, Clinics, Orgs) → client-side (typically dozens of rows, infrequent changes)
- Patient list → server-side (potentially thousands)
- Lab worklist / OPD queue → server-side (real-time-ish, busy)
- Settings tables → client-side
- Billing detail (per encounter) → client-side (encounter has bounded line items)
- Audit log → server-side (unbounded growth)

---

*End of §4. Proceed to §5 — Selection model.*


# LipiTable Spec — §5 Selection model

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2, §3, §4

---

## §5.1 — Selection modes

LipiTable supports three selection modes via the `SelectionMode` parameter:

```csharp
public enum SelectionMode
{
    None,    // default — no selection affordance, no checkbox column
    Single,  // exactly one row may be selected at a time
    Multi    // any number of rows may be selected
}
```

### §5.1.1 — `SelectionMode.None` (default)
- No selection checkbox column rendered
- No header "select all" checkbox
- No bulk action bar
- `OnRowClick` still fires when set; selection state is unaffected
- `SelectedItems` parameter and `OnSelectionChanged` event are ignored if set (developer-mode warning)

This is the default because most LiPi tables (settings, picklists, read-only views) don't need selection. Opting in is explicit.

### §5.1.2 — `SelectionMode.Single`
- Selection checkbox column rendered at the leftmost position (unless `SelectionPlacement="Right"` overrides)
- Header has no "select all" checkbox (single-mode has nothing to select-all)
- Clicking a checkbox selects that row and clears any prior selection
- Clicking the same row's checkbox a second time deselects (mode-dependent — see §5.1.4)
- `SelectedItems` is a list of length 0 or 1
- Bulk action bar appears when count = 1 (still useful for "edit this one selected row" patterns)

### §5.1.3 — `SelectionMode.Multi`
- Selection checkbox column rendered (same placement as Single)
- Header has a "select all" checkbox with three states: empty, indeterminate, checked
- Two-step "select all" semantics per Q2.2 = C (§5.3 below)
- Shift-click and Ctrl/Cmd-click supported per Q2.3 = B (§5.4)
- `SelectedItems` is a list of any size

### §5.1.4 — `AllowDeselectInSingleMode` parameter
When `SelectionMode.Single` is active, deselecting the currently selected row is configurable:

| Value | Behavior |
|---|---|
| `true` (default) | Clicking the selected row's checkbox deselects → empty selection allowed |
| `false` | Clicking the selected row's checkbox is a no-op → at least one row always selected (after first selection) |

Use `false` for "pick exactly one" workflows where the table acts as a chooser (e.g., "which doctor handles this case"). Use `true` (default) for "I might want to select one, or none."

---

## §5.2 — Selection persistence across pages, filter, sort

Per Q2.1 = A — locked decision.

### §5.2.1 — Persistence rule

**Selection persists across:** page navigation, page-size change, sort change, filter change, group change, density change, and refresh / reload.

**Selection clears on:**
- `Items` parameter reference change (when `PreserveSelectionOnDataChange="false"`) — see §5.2.4
- `TableId` change
- `DataSource` parameter change (different callback)
- Programmatic `tableRef.ClearSelectionAsync()`
- User clicking "Clear selection" in the bulk action bar
- Component disposal

### §5.2.2 — How persistence works

Selection is stored as **a set of row keys** (computed via `KeySelector`), not as a set of `TItem` references. This means:

- Selecting row A on page 1, navigating to page 2 → row A's key stays in the selected set
- Returning to page 1 → row A's checkbox is checked because its key is in the set
- Filtering hides row A → row A's key remains in the set (silently)
- Removing the filter → row A's checkbox is checked again
- Reloading data and row A no longer exists → its key remains in the set but never renders again (orphaned but harmless)

### §5.2.3 — Selected count display

The bulk action bar (region ③) shows the **total count of keys in the selected set**, including keys for rows not currently visible (off-page, filtered out, off-screen in virtualization).

This is critical UX: "3 selected" must mean "you've actually picked 3 things," even if only 1 is on screen right now. Otherwise the user can't tell what "Delete selected" is about to delete.

When the bulk action bar shows "47 selected" and the visible page shows 20 rows of which 3 are checked, the meaning is: "3 of the 47 you've selected are visible here; 44 others are on other pages / filtered out."

### §5.2.4 — `PreserveSelectionOnDataChange` parameter

```csharp
[Parameter] public bool PreserveSelectionOnDataChange { get; set; } = true;
```

Controls whether selection persists when `Items` reference changes:

| Value | When `Items` ref changes |
|---|---|
| `true` (default) | Selection persists; orphaned keys (not in new data) silently retained in the set |
| `false` | Selection cleared completely on `Items` reference change |

Most cases want `true` (e.g., page refresh, data update, edit-and-resave). Some workflows want `false` (e.g., "load a different view of the data; old selection no longer makes sense"). Caller decides.

### §5.2.5 — Server-side selection persistence

In server-side mode, the selected-set is still a set of keys. The complication: the keys may refer to rows not in the current page's response.

When the user has "47 selected" and only 20 rows are loaded, LipiTable's selected set has 47 entries. The other 27 entries are keys, not full `TItem` instances. The visible-row checkboxes are checked or unchecked based on whether each visible row's key is in the set.

When the consuming page wants to **act on the selection** (e.g., bulk delete), it has two options:
1. The page already knows the keys, so it sends "delete these 47 keys" to the server (no need to fetch full rows).
2. The page asks LipiTable for the keys via `tableRef.GetSelectedKeysAsync()` and acts on them.

If the page needs the full `TItem` instances for the selection (e.g., to render a confirmation modal showing names), it must fetch them separately. LipiTable does not auto-fetch unloaded selected rows. This is documented as a gotcha for server-side mode.

### §5.2.6 — "Select all across pages" intent

When the user activates the two-step select-all banner (§5.3), the intent isn't "select these N keys" — it's "select everything that matches the current filter." That intent persists as a flag:

```csharp
public bool IsSelectAllAcrossPagesActive { get; }
```

This flag survives until:
- User manually deselects any row → flag clears; the remaining selection becomes an explicit key set
- User clears all selection → flag clears
- Filters change → flag clears (the matching set just changed)
- Component disposes

When the flag is active and the user invokes a bulk action, the consuming page receives:
- The list of keys explicitly selected (initially empty in this mode; populated if the user deselects specific rows)
- The "select all across pages" flag
- The current filter / quick search state

The page then sends a "delete all matching THIS filter, except these specific keys" request to the server. This avoids transmitting potentially millions of keys.

---

## §5.3 — The two-step "Select all" semantics

Per Q2.2 = C — locked decision. The select-all-header-checkbox has staged semantics:

### §5.3.1 — Step 1: Click select-all checkbox

The header checkbox in column ⑥ is the "select all on this page" toggle:

```
┌───────────────────────────────────────────────────────────────┐
│ [☑] | Name        | Role          | Status      | Actions     │  ← header
├───────────────────────────────────────────────────────────────┤
│ [☑] | Dr. Reddy   | Consultant    | Active      |             │  ← rows on this page
│ [☑] | Dr. Patel   | Surgeon       | Active      |             │  ← all checked after step 1
│ [☑] | Dr. Sharma  | Resident      | Active      |             │
│ [☑] | Dr. Iyer    | Nurse         | Active      |             │
└───────────────────────────────────────────────────────────────┘
```

Clicking the header checkbox while no rows are selected → all rows on the current page are selected (added to the selected set).

If some rows on the page are already selected (from prior interactions), the header checkbox is in indeterminate state (square with line, not check). Clicking it then completes the page-selection (selects the remaining unselected rows on the page).

If all rows on the current page are selected, the header checkbox is checked. Clicking it then deselects all rows on the current page (removes their keys from the selected set).

### §5.3.2 — Step 2: The "select all across pages" banner

When step 1 results in "all rows on this page are selected" AND the total count exceeds the current page size, a banner appears between the toolbar and the header:

```
┌───────────────────────────────────────────────────────────────┐
│ All 20 rows on this page are selected.                        │
│  [Select all 487 rows matching current filters]               │
└───────────────────────────────────────────────────────────────┘
│ [☑] | Name        | ...                                       │  ← page select-all
├───────────────────────────────────────────────────────────────┤
│ [☑] | row 1                                                   │
│ [☑] | row 2 ...                                               │
```

Clicking the banner activates "select all across pages" mode. The banner updates:

```
┌───────────────────────────────────────────────────────────────┐
│ All 487 rows matching current filters are selected.           │
│  [Clear selection]                                            │
└───────────────────────────────────────────────────────────────┘
```

The bulk action bar (region ③) reads "487 selected." The header checkbox is checked. Every visible row's checkbox is checked.

### §5.3.3 — Banner visibility rules

The banner appears only when:
- `SelectionMode = Multi`
- All rows on the current page are selected
- `TotalCount > Rows.Count` (i.e., there are unloaded / other-page rows that could be added)
- "Select all across pages" is not already active

The banner disappears when:
- The user deselects any row (returns to explicit key-set selection)
- The user clicks "Select all 487 rows matching current filters" (flag activates, banner reframes)
- The user navigates to a different page where not all rows are selected

### §5.3.4 — Banner copy

The banner is text-only (no LipiAlert / LipiBanner — kept minimal):

| State | Default copy |
|---|---|
| Page-all selected, more rows exist | "All N rows on this page are selected. [Select all M rows matching current filters]" |
| All-across-pages active | "All M rows matching current filters are selected. [Clear selection]" |
| Unknown total (`TotalCount = -1`) | "All N rows on this page are selected. [Select all loaded rows]" |

Customizable via `SelectAllBannerTemplate` RenderFragment (caller provides custom messaging).

### §5.3.5 — Behavior when filters are not active

If no filters are active, the banner text reads "Select all M rows" (no "matching current filters" qualifier). The behavior is identical; only the copy changes.

### §5.3.6 — Behavior when "select all across pages" is active and user changes filters

Filters change → "select all across pages" flag clears (the matching set just changed and would be misleading to call selected). The selected set returns to explicit keys. Most cases this means selected count drops to 0 unless the user had previously deselected specific rows (those deselections persist briefly as explicit deselects, but with the flag cleared, they're discarded).

The bulk action bar updates to "0 selected" and disappears.

### §5.3.7 — Programmatic select-all
```csharp
await tableRef.SelectAllOnPageAsync();              // step 1 equivalent
await tableRef.SelectAllAcrossPagesAsync();         // step 2 equivalent (sets the flag)
await tableRef.ClearSelectionAsync();               // clears everything
await tableRef.SelectKeysAsync(IEnumerable<object>);// programmatic specific selection
```

---

## §5.4 — Shift-click and Ctrl/Cmd-click

Per Q2.3 = B — locked decision. Both desktop patterns supported.

### §5.4.1 — Shift-click range selection

Definition: clicking a row checkbox while holding Shift selects (or deselects) the range from the **last-clicked row** to the **just-clicked row**, inclusive.

Mechanics:
- LipiTable remembers the "anchor row" — the last row whose checkbox was clicked (without modifier OR with Ctrl/Cmd).
- Shift-click resolves to: take all rows between the anchor and the just-clicked row (in display order), and apply the just-clicked row's new state to all of them.
- If the anchor row's current state is checked and the user shift-clicks an unchecked row, the range becomes checked (anchor was checked; user is asking to extend the checked state).
- If the anchor row's current state is unchecked and the user shift-clicks a checked row, the range becomes unchecked.

This matches the Windows / macOS / Web standard. Users from Gmail, Outlook, Notion, Linear, Excel will recognize it.

### §5.4.2 — Anchor reset

The anchor is updated whenever:
- A row checkbox is clicked without Shift (with or without Ctrl/Cmd)
- A row is keyboard-toggled via Space (the row that received Space becomes the new anchor)

The anchor is cleared when:
- The selected set is cleared programmatically (`ClearSelectionAsync`)
- The component is disposed
- The data changes such that the anchor row's key is no longer present

If the user shift-clicks without ever having clicked first (anchor is null), the shift-click is treated as a regular click — just toggles the one row's selection.

### §5.4.3 — Cross-page shift-click

If the anchor row is on page 1 and the user navigates to page 3 and shift-clicks a row there:
- The range spans pages 1-3 by row key (or row index in the displayed order if keys aren't comparable)
- All keys in the range become selected (or deselected per §5.4.1)

The visible rows on page 3 update their checkbox state; the off-page rows have their keys added to the set silently.

In server-side mode, "all keys in the range" is computed only for the rows currently loaded. Rows that are between anchor and clicked but not in the current page response are NOT automatically added. The reason: LipiTable doesn't know their keys without fetching. The user can still complete the range by paginating to the intermediate page and shift-clicking there, but auto-spanning across unloaded data is not supported.

Documented gotcha for server-side consumers: shift-click range over unloaded data is page-bounded.

### §5.4.4 — Ctrl/Cmd-click

Clicking a row checkbox while holding Ctrl (Windows / Linux) or Cmd (macOS) toggles **only that row's** selection — does not affect any other row. This is equivalent to clicking the checkbox directly (no Shift); the modifier is essentially decorative for keyboard-mouse users who already have Ctrl/Cmd held for other reasons.

In practice the modifier matters when **row click** (`OnRowClick`) is also set. Then:
- Plain click on the row body fires `OnRowClick` (e.g., navigation to row detail)
- Ctrl/Cmd-click on the row body **selects** the row instead of firing `OnRowClick` — matches Web standard for "select without navigating"
- Shift-click on the row body extends the selection range, same as shift-click on the checkbox

LipiTable detects modifier state from the click event's `ctrlKey` / `metaKey` properties.

### §5.4.5 — Modifier conflicts and platform differences

- On macOS, Ctrl-click typically triggers context menu, not selection. LipiTable uses Cmd (metaKey) for the toggle modifier; Ctrl is left alone.
- On Windows / Linux, Ctrl is the toggle modifier; Cmd doesn't exist.
- Shift is universal.

LipiTable detects the platform from `navigator.platform` (via JS interop) and shows tooltip hints accordingly ("⌘+click to select" on Mac, "Ctrl+click to select" elsewhere). The behavior is the same; only the tooltip differs.

---

## §5.5 — Keyboard selection

The keyboard model for selection (cross-referenced from §19.3 — full keyboard map):

| Key | Effect |
|---|---|
| Space (on a row) | Toggle that row's selection (anchor updates) |
| Shift+Space (on a row) | Extend selection from anchor to this row |
| Ctrl/Cmd+Space (on a row) | Toggle that row's selection (same as Space; modifier ignored for parity with mouse-Ctrl) |
| Ctrl/Cmd+A | "Select all on current page" — equivalent to step 1 of two-step select-all |
| Ctrl/Cmd+Shift+A | "Select all across pages" — equivalent to step 2 (skips the banner) |
| Escape | If selection > 0, clear selection. (If selection is 0, behavior is row-context-dependent — see §19.3) |
| Shift+Arrow Up/Down | Extend selection by one row in the arrow direction |
| Tab into header "select all" + Space | Toggle "select all on page" |

Keyboard selection observes the same `SelectionMode` rules — Single mode prevents Ctrl/Cmd+A (developer-mode warning if attempted), and Shift+Arrow degrades to plain arrow navigation.

---

## §5.6 — Selection rendering

### §5.6.1 — Checkbox column

By default, the selection checkbox column is the **leftmost** column. Override via:

```csharp
[Parameter] public SelectionPlacement SelectionPlacement { get; set; } = SelectionPlacement.Left;
```

Values: `Left` (default), `Right`. `Right` puts the checkbox at the rightmost position before any pinned-right columns. Useful in dense tables where the leftmost column should be the primary key.

The checkbox column has these CSS classes (from §2.2):
- `lipi-table-header-cell-select` (header)
- `lipi-table-cell-select` (body cells)

Width is fixed: 36px (compact), 44px (comfortable), 52px (spacious) — sized to the density mode.

The column does **not** appear in the column picker (it's a system column, not a user column) and cannot be hidden or reordered while `SelectionMode != None`. Switching to `SelectionMode.None` removes the column entirely.

### §5.6.2 — Header "select all" checkbox

In `SelectionMode.Multi`, the header cell renders a `LipiCheckbox`. Its state is computed from current selection:

| Visible rows | Selected count among visible | Checkbox state |
|---|---|---|
| 0 visible rows | (any) | hidden (no body to select) |
| ≥ 1 visible row | 0 visible selected | unchecked |
| ≥ 1 visible row | 0 < N < visible count | indeterminate |
| ≥ 1 visible row | visible count | checked |

In `SelectionMode.Single`, the header cell is empty (no select-all-applicable in single mode).

### §5.6.3 — Row checkbox

Each row's checkbox is a `LipiCheckbox`, two-way bound to `selected[row.key]`. Clicking the checkbox toggles selection (with modifier rules above).

When a row is in inline-edit mode, its checkbox is **disabled** (per Q3.1.e.i = C). This prevents accidental selection toggling while the user is editing.

When a row is `RowDisabled="@(item => true)"` (caller-driven), its checkbox is also disabled (the entire row is not interactive).

### §5.6.4 — Bulk action bar (region ③)

Composition (from §2.2.4):

```
┌─────────────────────────────────────────────────────────────────┐
│ ☑ 3 selected   | <BulkActions slot>   | [Clear selection]       │
└─────────────────────────────────────────────────────────────────┘
```

Layout details:
- Background: `--color-primary-50` in light mode, `--color-primary-900-tinted` in dark mode (subtle highlight)
- Height: 48px (matches toolbar)
- Border-top: 0.5px solid `--color-border-tertiary`
- Animates in/out: 200ms slide from top + opacity fade. `prefers-reduced-motion` disables animation.

The "N selected" chip uses `LipiBadge` (Phase 2.7) with a primary variant.

`<BulkActions>` slot is caller-provided RenderFragment. LipiTable renders the slot inline; the caller's buttons / dropdowns / divider / spacing are their call. Convention:

```razor
<BulkActions>
    <LipiButton OnClick="@DeleteSelected" Variant="ButtonVariant.Danger">Delete</LipiButton>
    <LipiButton OnClick="@ExportSelected" Variant="ButtonVariant.Secondary">Export</LipiButton>
    <LipiButton OnClick="@TagSelected" Variant="ButtonVariant.Secondary">Tag…</LipiButton>
</BulkActions>
```

The "Clear selection" button on the right edge is always rendered by LipiTable (not in the caller's slot). It clears all selection including the "select all across pages" flag.

### §5.6.5 — Selected row visual highlight

Selected rows get the `lipi-table-row--selected` modifier class. CSS treatment:
- Background: subtle primary tint (`--color-primary-50` light, `--color-primary-900-tinted` dark, with `--color-primary-alpha-08` overlay)
- Status strip color preserved (selection highlight doesn't replace it; both visible simultaneously)
- No font-weight change (avoids layout reflow on selection)

The highlight is visible regardless of which page the row is on — opening page 1 to find row A still highlighted (if its key is selected) is the expected behavior.

---

## §5.7 — Selection-driven events

### §5.7.1 — `SelectedItems` and `SelectedItemsChanged`

Two-way binding. Caller can read and write the selection.

```razor
<LipiTable TItem="Patient"
           SelectionMode="SelectionMode.Multi"
           @bind-SelectedItems="_selected">
    ...
</LipiTable>
```

The bound type is `IReadOnlyList<TItem>` — only rows currently materialized in the visible data. For server-side mode where the selection may include keys for unloaded rows, see §5.7.2 below.

`SelectedItems` is **never** null. If selection is empty, it's an empty list.

### §5.7.2 — `SelectedKeys` (for server-side completeness)

```csharp
[Parameter] public IReadOnlyCollection<object>? SelectedKeys { get; set; }
[Parameter] public EventCallback<IReadOnlyCollection<object>> SelectedKeysChanged { get; set; }
```

Two-way binding for the raw key set. Useful when the consuming page needs to act on keys for rows not currently loaded (server-side bulk delete pattern):

```razor
<LipiTable TItem="Patient"
           SelectionMode="SelectionMode.Multi"
           @bind-SelectedKeys="_selectedKeys">
    ...
</LipiTable>
```

`SelectedKeys` always reflects every selected key, including off-page ones. `SelectedItems` reflects only loaded ones.

### §5.7.3 — `OnSelectionChanged` event

```csharp
public sealed record SelectionChangedContext<TItem>(
    IReadOnlyCollection<object> SelectedKeys,
    IReadOnlyList<TItem> SelectedItemsVisible,
    bool IsSelectAllAcrossPagesActive,
    SelectionChangeReason Reason
);

public enum SelectionChangeReason
{
    UserCheckboxClick,
    UserShiftClick,
    UserCtrlClick,
    UserSelectAllHeaderClick,
    UserSelectAllAcrossPages,
    UserClearSelection,
    UserKeyboard,
    Programmatic,
    DataChange    // selection trimmed because items removed from data
}

[Parameter] public EventCallback<SelectionChangedContext<TItem>> OnSelectionChanged { get; set; }
```

Fires after every selection change. Caller can use `Reason` to differentiate user-initiated vs programmatic vs data-driven changes.

### §5.7.4 — Programmatic selection API

```csharp
public ValueTask SelectAsync(TItem item);
public ValueTask DeselectAsync(TItem item);
public ValueTask ToggleSelectionAsync(TItem item);

public ValueTask SelectKeysAsync(IEnumerable<object> keys);
public ValueTask DeselectKeysAsync(IEnumerable<object> keys);

public ValueTask SelectAllOnPageAsync();
public ValueTask SelectAllAcrossPagesAsync();
public ValueTask ClearSelectionAsync();

public IReadOnlyCollection<object> GetSelectedKeys();
public IReadOnlyList<TItem> GetSelectedVisibleItems();
public bool IsSelectAllAcrossPagesActive { get; }
public bool IsSelected(TItem item);
public bool IsKeySelected(object key);
```

These methods are on the `LipiTable<TItem>` component instance, accessible via `@ref`. All async to allow LipiTable to internally orchestrate state changes that may involve rendering coordination.

---

## §5.8 — Bulk action confirmation patterns

Per the LiPi standing rule on destructive actions (§02-STANDING-RULES: "Confirmation dialog before every destructive or irreversible action"), bulk delete / bulk archive / bulk lock / etc. must show confirmation.

LipiTable does **not** automatically wrap caller-provided `<BulkActions>` buttons in confirmations. The caller's bulk-action button handler is responsible for showing its own `LipiModal`-based confirm before invoking the destructive operation.

This is intentional — LipiTable can't know which actions are destructive. A "Tag…" or "Export" action shouldn't pop a confirm; only the dangerous ones should. The caller has full control.

Convention for destructive bulk actions:

```razor
<BulkActions>
    <LipiButton OnClick="@ConfirmAndDeleteSelected" Variant="ButtonVariant.Danger">Delete</LipiButton>
</BulkActions>

@code {
    private async Task ConfirmAndDeleteSelected()
    {
        var keys = await _tableRef.GetSelectedKeys();
        var count = keys.Count;
        var confirmed = await ModalService.ShowConfirmAsync(
            title: $"Delete {count} patients?",
            body: "This cannot be undone.",
            confirmText: "Delete",
            confirmVariant: ButtonVariant.Danger);
        if (!confirmed) return;
        await _patientService.BulkDeleteAsync(keys);
        await _tableRef.RefreshAsync();
    }
}
```

When "select all across pages" is active, the caller's confirm should reference the total count, not just the loaded keys. Caller reads `_tableRef.IsSelectAllAcrossPagesActive` and adapts the message: "Delete all 487 patients matching current filters?"

---

## §5.9 — Selection performance considerations

The selected set is a `HashSet<object>` keyed by row key. Lookup is O(1). The set scales to tens of thousands of keys without performance issues.

When rendering each row's checkbox, LipiTable does a single hash lookup against the selected set. For a virtualized 10,000-row table showing 30 visible rows, that's 30 lookups per render — negligible.

When the "select all across pages" flag is active in server-side mode, the selected set may be effectively empty (the flag implies "everything matching filters"). Rendering checkboxes still requires checking each visible row against the filter — but the flag is a single boolean, not a per-row computation. The check is: "if flag is active AND row matches current filters" → checked.

For "select all across pages" in client-side mode with thousands of rows, the selected set is materialized as the actual set of keys (all of them). Memory cost is one object per key — usually 16-24 bytes per key for a `Guid` or `long`. 10,000 selected → ~250KB. Acceptable.

---

## §5.10 — Worked examples

### §5.10.1 — Single-select doctor picker

```razor
<LipiTable TItem="Doctor"
           Items="@_doctors"
           KeySelector="@(d => d.Id)"
           SelectionMode="SelectionMode.Single"
           AllowDeselectInSingleMode="false"
           @bind-SelectedItems="_selectedDoctor"
           OnSelectionChanged="@(_ => OnDoctorChanged())">
    <LipiColumn Field="@(d => d.Name)" Header="Doctor" />
    <LipiColumn Field="@(d => d.Specialty)" Header="Specialty" />
    <LipiColumn Field="@(d => d.IsAvailable)" Header="Available" Type="ColumnType.Boolean" />
</LipiTable>
```

User must pick exactly one doctor. The selected item appears in `_selectedDoctor[0]`. Page proceeds when one is picked.

### §5.10.2 — Multi-select with bulk delete

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)"
           SelectionMode="SelectionMode.Multi"
           TableId="lab-results-list">
    <LipiColumn Field="@(r => r.PatientName)" Header="Patient" />
    <LipiColumn Field="@(r => r.TestName)"    Header="Test" />
    <LipiColumn Field="@(r => r.ReportedAt)"  Header="Reported" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(r => r.Status)"      Header="Status" Type="ColumnType.Status" />
    
    <BulkActions>
        <LipiButton OnClick="@BulkArchiveSelected"  Variant="ButtonVariant.Secondary">Archive</LipiButton>
        <LipiButton OnClick="@BulkExportSelected"   Variant="ButtonVariant.Secondary">Export</LipiButton>
        <LipiButton OnClick="@BulkDeleteSelected"   Variant="ButtonVariant.Danger">Delete</LipiButton>
    </BulkActions>
</LipiTable>
```

User can select rows across pages. Three bulk actions. Delete fires its own confirm modal before destruction.

### §5.10.3 — Programmatic selection from a parent component

```razor
<LipiButton OnClick="@SelectActiveOnly">Select active doctors</LipiButton>

<LipiTable TItem="Doctor"
           @ref="_table"
           Items="@_doctors"
           KeySelector="@(d => d.Id)"
           SelectionMode="SelectionMode.Multi">
    <LipiColumn Field="@(d => d.Name)" />
    <LipiColumn Field="@(d => d.IsActive)" Type="ColumnType.Boolean" />
</LipiTable>

@code {
    private LipiTable<Doctor>? _table;

    private async Task SelectActiveOnly()
    {
        var activeKeys = _doctors.Where(d => d.IsActive).Select(d => (object)d.Id);
        await _table!.SelectKeysAsync(activeKeys);
    }
}
```

Caller programmatically populates the selection. Useful for "select all matching a sub-criterion" workflows that don't quite fit the built-in filter UI.

---

*End of §5. Proceed to §6 — Sorting.*


# LipiTable Spec — §6 Sorting

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model + SortComparer), §4 (TableQueryRequest.Sort)

---

## §6.1 — Single-column sort (default)

### §6.1.1 — Activation

A column is sortable when:
1. The `<LipiColumn>` has `Sortable="true"` (the default for all column types except Avatar, File, Actions per §3.3.1)
2. The table-level `AllowSort` parameter is `true` (default)

The header cell of a sortable column gets the `--sortable` modifier (cursor:pointer, hover background).

### §6.1.2 — Click cycle

Clicking a sortable header cycles through three states:

```
unsorted ──click──▶ ascending ──click──▶ descending ──click──▶ unsorted
   ↕                    ↕                     ↕
 (no       ↑                                      ↑
  indicator)  ────────── single click ─────────────
```

The user can complete the cycle with three clicks. Some users prefer "always returns to unsorted is wasteful" — they should sort by clicking once for asc / twice for desc and use another column to break out. Some users prefer the third state. The cycle is locked as **three-state** (industry standard, matches Excel, Google Sheets, AG-Grid, MudBlazor, Telerik).

Override via column parameter:

```csharp
[Parameter] public SortCycleMode SortCycle { get; set; } = SortCycleMode.ThreeState;

public enum SortCycleMode
{
    ThreeState,  // unsorted → asc → desc → unsorted (default)
    TwoState     // asc → desc → asc (sort never clears once started)
}
```

`TwoState` is for columns where "unsorted" is meaningless (e.g., a primary identifier where the user always wants *some* order). Rarely used.

### §6.1.3 — Sort indicator

The header cell renders an icon to the right of the column text:

| State | Icon | Color |
|---|---|---|
| Sortable, not sorted | ↕ (chevron-up-down, faint) | `--lipi-table-text-faint` |
| Sorted ascending | ↑ (chevron-up, solid) | `--lipi-table-text` |
| Sorted descending | ↓ (chevron-down, solid) | `--lipi-table-text` |

The icon is part of `lipi-table-header-cell-text` via the `lipi-table-header-sort` class (§2.2.6). The faint state is intentionally subtle — visible but not noisy when 10 columns are all sortable and only one is sorted.

Icon source: Lucide `chevrons-up-down` / `chevron-up` / `chevron-down`.

### §6.1.4 — Click vs keyboard

The header cell is keyboard-focusable when sortable. Activation:

| Input | Effect |
|---|---|
| Click | Cycle sort state |
| Enter (when header focused) | Cycle sort state |
| Space (when header focused) | Cycle sort state |
| Alt+Down (when header focused) | Open filter popover (does NOT cycle sort) |

The keyboard parity ensures keyboard users can sort exactly like mouse users.

### §6.1.5 — Screen reader announcement

When sort state changes, an aria-live region announces:

- "Sorted by [Column Name], ascending"
- "Sorted by [Column Name], descending"
- "Sort cleared from [Column Name]"

The column header itself carries `aria-sort="ascending"` / `"descending"` / `"none"` so screen readers can pick up sort state on focus.

---

## §6.2 — Multi-column sort

### §6.2.1 — Activation

Multi-column sort engages when the user **shift-clicks** a column header. The shift modifier signals "add this to the existing sort, don't replace it."

| User action | Effect |
|---|---|
| Click column A | Sort by A (asc). Anchor reset. |
| Click column B | Sort by B (asc). A's sort cleared. |
| Shift-click column C | Sort by B, then C. B keeps its current direction. |
| Shift-click column C again | Sort by B, then C (desc). |
| Shift-click column C again | Sort by B (C removed from chain). |
| Click column D (no shift) | Sort by D only. All previous sorts cleared. |

Plain click without shift always resets to single-column sort. Shift-click adds / cycles / removes from the chain.

### §6.2.2 — Priority indicator

When two or more columns are sorted, each sorted header shows a small priority badge to the left of the sort arrow:

```
| Name 1↑ |    Role 2↓ |   Department 3↑ |
```

The badge is `lipi-table-header-sort-priority` (§2.2.6). Visual:
- Small numeric (1, 2, 3, ...)
- 14px square (compact density: 12px)
- Background `--color-primary-alpha-12`
- Text `--color-primary-700`
- Border-radius `--r-xs` (3px)

The primary sort (priority 1) is the outermost grouping; subsequent priorities break ties. Standard SQL `ORDER BY` semantics.

### §6.2.3 — Maximum sort columns

Default: unlimited. Most tables won't exceed 3–4 concurrent sort columns in practice. Override via:

```csharp
[Parameter] public int MaxSortColumns { get; set; } = int.MaxValue;
```

Setting `MaxSortColumns="3"` means shift-clicking a fourth column either replaces the lowest-priority sort or is rejected (caller picks via `MaxSortColumnsBehavior.Replace | Reject`, default Replace).

### §6.2.4 — Reordering sort priorities

A user might want to change which column has priority 1 vs 2 after setting up a multi-sort. Two approaches considered:

- **Drag-and-drop on priority badges** — drag a badge to reorder. Implementation overhead, niche use case.
- **Re-shift-click to advance priority** — shift-click an already-sorted column to "bump it up" in priority. Confusing.
- **Just re-set the sort from scratch** — caller clicks column A first, then shift-clicks B, then C. Order = click order.

**LipiTable's choice: the third.** Sort priority follows click order. To change priority, clear and re-set. This is industry standard (AG-Grid, MudBlazor, Telerik all do this).

To clear all sorts at once: header context menu has "Clear all sorts" entry, OR programmatic `tableRef.ClearSortAsync()`, OR the column picker's "Reset to defaults" includes sort.

---

## §6.3 — Default sort

### §6.3.1 — `DefaultSort` parameter

When persistence has no stored state for this `TableId` (or `TableId` isn't set), `DefaultSort` seeds the initial sort:

```razor
<LipiTable TItem="Patient"
           DefaultSort="@(new[]
           {
               new SortDescriptor(nameof(Patient.UpdatedAt), SortDirection.Desc, 0)
           })"
           ...>
```

`DefaultSort` is `IReadOnlyList<SortDescriptor>`. When passed:

- Loaded on first render if no persistence entry exists for this user
- Ignored if user has a persisted sort (persistence wins)
- Re-applied when user invokes "Reset to defaults" in the column picker

If `DefaultSort` is `null` or empty (the default), LipiTable renders with no initial sort (rows appear in source order).

### §6.3.2 — Single-column default

Convenience overload:

```csharp
[Parameter] public string? DefaultSortColumn { get; set; }
[Parameter] public SortDirection DefaultSortDirection { get; set; } = SortDirection.Asc;
```

Same effect as a single-entry `DefaultSort` array. Useful for the 90% case:

```razor
<LipiTable TItem="Patient"
           DefaultSortColumn="UpdatedAt"
           DefaultSortDirection="SortDirection.Desc"
           ...>
```

If both `DefaultSort` (array) and `DefaultSortColumn` (scalar) are set, the array wins; the scalar is ignored with a developer-mode warning.

---

## §6.4 — Custom sort comparators

### §6.4.1 — `SortComparer` parameter on `<LipiColumn>`

For columns where the default comparator is wrong, the caller provides a custom `IComparer<TValue>`:

```razor
<LipiColumn Field="@(p => p.Severity)" 
            Type="ColumnType.Text"
            Header="Severity"
            SortComparer="@_severityComparer" />

@code {
    // Sort severity by clinical importance, not alphabetically
    private static readonly IComparer<string> _severityComparer =
        Comparer<string>.Create((a, b) =>
        {
            var order = new[] { "Critical", "Severe", "Moderate", "Mild", "None" };
            var ai = Array.IndexOf(order, a);
            var bi = Array.IndexOf(order, b);
            return ai.CompareTo(bi);
        });
}
```

The comparer receives the column's value type (`TValue`). For nullable types, the comparer must handle null itself. LipiTable does not pre-screen nulls before invoking the comparer.

### §6.4.2 — Default comparators per type

When `SortComparer` is not specified, LipiTable uses these defaults:

| Type | Default comparator | Null behavior |
|---|---|---|
| Text | `StringComparer.CurrentCulture` | Null sorts to end on asc |
| Mono | `StringComparer.Ordinal` (case-sensitive, no culture) | Null sorts to end on asc |
| Number | Numeric comparison via `IComparable<TValue>` | Null sorts to end on asc |
| Currency | Numeric (same as Number) | Same |
| Date / DateTime / Time | Chronological via `IComparable<TValue>` | Null sorts to end on asc |
| Boolean | false → true → null on asc | Null sorts to end on asc |
| Status | Alphabetical on status string; or `StatusOrder` parameter override | Null sorts to end on asc |
| Avatar | Sort disabled by default | (N/A) |
| Link | Alphabetical on rendered link text | Null sorts to end on asc |
| File | Alphabetical on file name | Null sorts to end on asc |
| Actions | Sort disabled | (N/A) |
| Custom | Sort disabled unless `SortComparer` provided | Caller's responsibility |

### §6.4.3 — Null-sort behavior parameter

Override the null behavior per column:

```csharp
[Parameter] public NullSortOrder NullSort { get; set; } = NullSortOrder.AlwaysLast;

public enum NullSortOrder
{
    AlwaysFirst,   // null always before non-null regardless of direction
    AlwaysLast,    // null always after non-null regardless of direction (default)
    FirstOnAsc,    // null first when sorting asc, last when desc
    LastOnAsc      // null last when sorting asc, first when desc
}
```

Most consuming pages keep the default. The override exists for cases like "outstanding items (with no due date set) should always appear at the top," where `AlwaysFirst` makes sense regardless of sort direction.

### §6.4.4 — Sort stability

LipiTable's client-side sort is **stable**: when two rows compare equal, their relative order is preserved from the previous render. This matters when:

- User sorts by Status (ascending) — ties broken by row order
- User then sorts by Date (ascending) — ties within Date broken by Status (the previous sort)

Effective behavior: multi-column sort via single-clicks works because each sort preserves the previous order as the tiebreaker. The multi-column explicit shift-click pattern (§6.2) is more efficient for the user but the stable single-click chain produces the same result.

Server-side sort stability depends on the server's query semantics. LipiTable cannot enforce stability across server-side sorts; it sends the explicit `SortDescriptor` list in the request and trusts the server.

---

## §6.5 — Server-side sort

### §6.5.1 — Wire format

Sort state is sent in `TableQueryRequest.Sort` (§4.2.2):

```csharp
public sealed record SortDescriptor(
    string ColumnKey,
    SortDirection Direction,
    int Priority
);
```

The list is ordered by priority (0 = primary). Single-column sort has exactly one descriptor with priority 0.

### §6.5.2 — Server contract

Server is responsible for translating descriptors into a query. For SQL:

```csharp
private static IQueryable<Patient> ApplySort(
    IQueryable<Patient> source, 
    IReadOnlyList<SortDescriptor> sorts)
{
    IOrderedQueryable<Patient>? ordered = null;
    foreach (var s in sorts.OrderBy(x => x.Priority))
    {
        // EF Core dynamic ORDER BY: use System.Linq.Dynamic.Core OR a switch
        // on s.ColumnKey to call .OrderBy/.OrderByDescending/.ThenBy/.ThenByDescending
        ordered = ApplySortStep(ordered ?? (IOrderedQueryable<Patient>)source, s);
    }
    return ordered ?? (IOrderedQueryable<Patient>)source;
}
```

LipiTable provides a sample helper in spec-companion code (§27 file list) — `TableQueryHelpers.ApplySort<T>(IQueryable<T>, IReadOnlyList<SortDescriptor>)` — for consumers using EF Core. Use is optional; some servers have their own query builders.

### §6.5.3 — Unknown column key handling

If the server receives a `ColumnKey` it doesn't recognize (e.g., LipiTable spec drift, or a caller's `ColumnKey` doesn't match a server property), the server should:

- **Either** ignore that descriptor and apply remaining sorts
- **Or** throw an error returned to the caller

The recommended pattern is **silent ignore** for forward compatibility. If LipiTable sends a sort by a new column the server hasn't deployed yet, the table degrades gracefully (rows still load, just not sorted by the unknown column).

### §6.5.4 — Server-side stability

If the server's query engine doesn't produce stable sort, the consuming page is responsible for adding a tiebreaker (e.g., always `ORDER BY ... THEN BY Id`). LipiTable does not require server-side stability for correctness; it does require the server to be deterministic — the same `TableQueryRequest` should produce the same row order across invocations within the same dataset version.

Non-deterministic server-side sort (e.g., random order on ties) causes pagination jitter: the user paginates from page 2 back to page 1 and sees different rows. LipiTable cannot detect this; the consuming page must prevent it.

---

## §6.6 — Sort and grouping interaction

When grouping is active (`GroupBy` set), sorting operates **within** groups, not across them. Group order is determined by `GroupDescriptor.Direction` (the order of the group headers), and sort within each group's rows is independent.

Example:

```
[Group: Cardiology]     ← group header (Cardiology, Neurology, etc. are alpha-sorted)
  Dr. Reddy   Active
  Dr. Patel   Active   ← rows within group, sorted by Name asc
  Dr. Sharma  Inactive
[Group: Neurology]
  Dr. Iyer    Active   ← group sort applies inside this group too
```

The user can sort by clicking a column header — the sort applies inside each group independently. The user cannot sort *across* groups in a way that breaks group integrity (that would defeat grouping).

When the user clicks a header for the column that's currently grouped (e.g., Department in the example), the **group header order** is what gets sorted. The internal row sort is unchanged.

---

## §6.7 — Sort and tree data interaction

When tree data is active (`ChildrenSelector` or `ParentSelector` set), sorting operates **within each parent's children**, not across the entire flattened tree.

Example:

```
▾ Department: Cardiology   ← root
   ▾ Sub-unit: ICU              ← child of Cardiology, sorted with siblings (other Cardiology children)
      Dr. Reddy                 ← leaf rows under ICU, sorted with each other
      Dr. Patel
   ▸ Sub-unit: Cath Lab         ← sibling of ICU
```

Parents stay grouped with their children regardless of sort. The user's sort affects within-sibling ordering at every level.

This preserves the "I can see the tree structure" affordance — sorting by Name doesn't scatter parents and children across the table.

---

## §6.8 — Sort and quick-filter interaction

Quick-filter (§7.8) and sort are independent. Filter narrows the dataset; sort orders what remains. The combination is intuitive: filter first conceptually, then sort the result.

The order of user operations doesn't matter — entering a filter while a sort is active maintains the sort on the filtered result; sorting a filtered table works as expected.

---

## §6.9 — Programmatic sort API

```csharp
public ValueTask SortByAsync(string columnKey, SortDirection direction = SortDirection.Asc);
public ValueTask AddSortAsync(string columnKey, SortDirection direction = SortDirection.Asc);
public ValueTask RemoveSortAsync(string columnKey);
public ValueTask ClearSortAsync();
public ValueTask SetSortAsync(IReadOnlyList<SortDescriptor> sorts);

public IReadOnlyList<SortDescriptor> CurrentSort { get; }
```

| Method | Effect |
|---|---|
| `SortByAsync` | Single-column sort (replaces any existing) |
| `AddSortAsync` | Multi-column add (equivalent to shift-click) |
| `RemoveSortAsync` | Remove a specific column from the sort chain |
| `ClearSortAsync` | Clear all sorts |
| `SetSortAsync` | Replace the entire sort chain |
| `CurrentSort` | Read current sort state (read-only) |

All methods are async to allow LipiTable to coordinate persistence writes and (in server-side mode) re-fetches.

Use case — link from another component to a pre-sorted table:

```razor
<LipiButton OnClick="@SortByRecent">Show recent first</LipiButton>
<LipiTable @ref="_table" ...>...</LipiTable>

@code {
    private async Task SortByRecent()
    {
        await _table!.SortByAsync(nameof(Patient.UpdatedAt), SortDirection.Desc);
    }
}
```

---

## §6.10 — Sort persistence

When `TableId` is set, sort state is persisted to `identity.user_table_preferences` (cross-reference §21). The persisted shape:

```json
{
  "sort": [
    { "columnKey": "UpdatedAt", "direction": "desc", "priority": 0 },
    { "columnKey": "Name",      "direction": "asc",  "priority": 1 }
  ]
}
```

Persistence rules:
- Writes are debounced 300ms (the user clicking through `unsorted → asc → desc` quickly produces one write at the end, not three)
- Persisted state is restored on next mount, overriding `DefaultSort`
- If a persisted column key no longer exists in the column declaration (developer removed the column), it's silently skipped — remaining sorts apply
- "Reset to defaults" clears the persisted sort and applies `DefaultSort` (if set)

---

## §6.11 — Sort events

```csharp
public sealed record SortChangedContext(
    IReadOnlyList<SortDescriptor> NewSort,
    IReadOnlyList<SortDescriptor> PreviousSort,
    SortChangeReason Reason
);

public enum SortChangeReason
{
    UserHeaderClick,
    UserShiftHeaderClick,
    UserHeaderContextMenu,    // (when context-menu clear-sort is invoked)
    UserKeyboard,
    Programmatic,
    PersistenceRestore,        // restored from user prefs on mount
    DefaultApplied,            // applied from DefaultSort because no persistence
    ColumnRemoved              // sort cleared because the sorted column was removed from declaration
}

[Parameter] public EventCallback<SortChangedContext> OnSortChanged { get; set; }
```

Fires after every sort state change. Consuming pages can use it for analytics, logging, side-effects (e.g., URL parameter sync).

---

## §6.12 — Disabling sort entirely

Three levels of disabling:

| Level | How | Effect |
|---|---|---|
| Per-column | `<LipiColumn Sortable="false">` | That column's header is not sort-clickable; no indicator rendered |
| Per-table | `<LipiTable AllowSort="false">` | All headers non-clickable; no sort cycle, no indicators, no priority badges |
| Lock current sort | `<LipiTable AllowSort="true" LockSort="true">` | Current sort is fixed; user can't change. Useful for "default sort is mandatory" workflows |

`LockSort="true"` is rare but real — sometimes a list must always be in a specific order (chronological audit log, ranked priority queue).

---

## §6.13 — Worked examples

### §6.13.1 — Settings table, no sort persistence

```razor
<LipiTable TItem="Setting"
           Items="@_settings"
           KeySelector="@(s => s.Key)"
           DefaultSortColumn="Key"
           DefaultSortDirection="SortDirection.Asc">
    <LipiColumn Field="@(s => s.Key)"       Header="Setting" />
    <LipiColumn Field="@(s => s.Value)"     Header="Value" />
    <LipiColumn Field="@(s => s.UpdatedAt)" Header="Updated" Type="ColumnType.DateTime" />
</LipiTable>
```

Initial sort by Key asc. User clicks Updated header → sort by Updated. No `TableId` set, so sort doesn't persist across navigations.

### §6.13.2 — Patient list with persisted multi-sort

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list"
           DefaultSort="@_defaultSort">
    <LipiColumn Field="@(p => p.Name)"        Header="Name" />
    <LipiColumn Field="@(p => p.UpdatedAt)"   Header="Updated" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(p => p.Status)"      Header="Status" Type="ColumnType.Status" />
    <LipiColumn Field="@(p => p.LastVisit)"   Header="Last visit" Type="ColumnType.Date" />
</LipiTable>

@code {
    private readonly SortDescriptor[] _defaultSort = new[]
    {
        new SortDescriptor(nameof(Patient.UpdatedAt), SortDirection.Desc, 0)
    };
}
```

User can shift-click multiple headers to build a multi-column sort (e.g., Status → Last visit). Their custom sort persists per-user. Reopening the page restores their sort. Clicking "Reset" returns to the `DefaultSort` (just Updated desc).

### §6.13.3 — Custom severity sort

```razor
<LipiColumn Field="@(a => a.Severity)"
            Header="Severity"
            SortComparer="@_severityOrder"
            DefaultSortColumn="Severity"
            DefaultSortDirection="SortDirection.Asc" />

@code {
    private static readonly IComparer<string> _severityOrder =
        Comparer<string>.Create((a, b) =>
        {
            int Rank(string s) => s switch
            {
                "Critical" => 0,
                "Severe"   => 1,
                "Moderate" => 2,
                "Mild"     => 3,
                "None"     => 4,
                null       => 5,
                _          => 99
            };
            return Rank(a).CompareTo(Rank(b));
        });
}
```

Severity column sorts by clinical importance, not alphabetically. Critical first on asc.

### §6.13.4 — Programmatic re-sort from a parent

```razor
<LipiButton OnClick="@(() => SortAge(SortDirection.Asc))">Youngest first</LipiButton>
<LipiButton OnClick="@(() => SortAge(SortDirection.Desc))">Oldest first</LipiButton>

<LipiTable @ref="_table" TItem="Patient" Items="@_patients" KeySelector="@(p => p.Id)">
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Age)" Type="ColumnType.Number" />
</LipiTable>

@code {
    private LipiTable<Patient>? _table;
    private Task SortAge(SortDirection direction) => 
        _table!.SortByAsync(nameof(Patient.Age), direction).AsTask();
}
```

External buttons drive the sort. Useful for dashboard surfaces where sort affordances live outside the table chrome.

---

*End of §6. Proceed to §7 — Filtering.*


# LipiTable Spec — §7 Filtering

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model + FilterTemplate), §4 (TableQueryRequest.Filters)

---

## §7.1 — Filter modes overview

Per Q7.1 (locked) — LipiTable supports two filter modes plus an off switch:

```csharp
public enum FilterMode
{
    HeaderIcon,   // default — Q7.1 option A — filter icon in each column header
    Drawer,       // Q7.1 option C — single button opens a drawer with all filters
    None          // no filter UI at all
}
```

The "inline filter bar" (Q7.1 option B — always-visible row of inputs below the header) was explicitly **skipped** per the locked decision. Quick search (§7.10) covers the "fast text filter across everything" case more efficiently.

Caller picks the mode at the table level:

```razor
<LipiTable TItem="..." FilterMode="FilterMode.HeaderIcon"> @* default *@
<LipiTable TItem="..." FilterMode="FilterMode.Drawer">
<LipiTable TItem="..." FilterMode="FilterMode.None">
```

`FilterMode.None` is for read-only display surfaces (e.g., audit-detail panels) where filtering is not the user's job.

---

## §7.2 — Filter mode mechanics

### §7.2.1 — `FilterMode.HeaderIcon` (default)

Each sortable / filterable column header renders a filter icon (▽) to the right of the sort indicator. Clicking the icon opens a popover anchored to the icon:

```
┌─ Name ──────────────────────────┐
│ ↕ ▽                              │
└──────────────────────────────────┘
            │
            ▼
        ┌────────────────────────────────┐
        │ FILTER: NAME                    │
        │ ┌────────────────────────────┐  │
        │ │ contains            ▾      │  │  ← operator dropdown
        │ └────────────────────────────┘  │
        │ ┌────────────────────────────┐  │
        │ │ Enter value...             │  │  ← value input
        │ └────────────────────────────┘  │
        │                                 │
        │       [Apply]  [Clear]          │
        └────────────────────────────────┘
```

**Visual properties:**
- Filter icon: Lucide `filter` (16px, faint when no filter active, `--lipi-table-text-link` when filter active)
- Popover: anchored below+right-aligned to icon, 240px wide, padding 12px, border-radius `--r-md`
- Operator dropdown: `LipiSelect` (Phase 2.3)
- Value input: `LipiTextBox` / `LipiNumberInput` / `LipiDatePicker` / `LipiDateRangePicker` / `LipiMultiSelect` per column type (§7.4)
- Action row: Apply (primary) and Clear (secondary). Apply applies the filter and closes; Clear removes any active filter from this column and closes.

**Interaction:**
- Click outside the popover → closes without applying (current input discarded if not yet applied)
- Escape → same as click outside
- Enter inside the value input → equivalent to clicking Apply
- The popover does NOT live-filter; the user must hit Apply (avoids cascading re-renders during typing for client-side, avoids server hammering for server-side)

### §7.2.2 — `FilterMode.Drawer`

The toolbar renders a single "Filters" button. Clicking opens a `LipiDrawer` (Phase 2.6.2) on the right edge containing all filterable columns' filters in one form:

```
┌──── Filters ─────────────────────────┐
│ NAME                                  │
│  contains  ▾                          │
│  [Dr. Reddy                       ]   │
│                                       │
│ ROLE                                  │
│  equals    ▾                          │
│  [Consultant                      ▾]  │
│                                       │
│ STATUS (set filter)                   │
│   ☑ Active                            │
│   ☑ Pending                           │
│   ☐ Locked                            │
│                                       │
│ LAST VISIT                            │
│  between   ▾                          │
│  [2026-01-01] → [2026-05-15]          │
│                                       │
│ ─────────────────────────────────     │
│        [Apply]  [Reset all]           │
└───────────────────────────────────────┘
```

**Visual properties:**
- Drawer placement: right, size compact (320px) by default; `FilterDrawerSize="LipiDrawerSize.Wide"` opens 480px
- Each column's filter rendered as a labeled section
- Drawer is pin-able if the consuming page wants it always-open (LipiDrawer pin support per Phase 2.6.2)
- Apply applies all changes simultaneously; Reset all clears every column's filter

**Interaction:**
- Drawer follows standard LipiDrawer dismiss behavior (Esc, backdrop click)
- Changes within the drawer are staged; they apply only on Apply button (or Enter inside any input)
- Reset all clears every filter and applies

The Drawer mode is the right choice when:
- Many columns (10+) need filters and per-header icons are visually noisy
- Filters are complex (multiple date ranges, multi-selects) and the popover is too cramped
- The user typically sets several filters at once rather than tweaking one at a time

### §7.2.3 — `FilterMode.None`

No filter UI. Filter icons not rendered, no Filters button in toolbar, no drawer. `FilterTemplate` slots in column declarations are ignored.

Quick search (§7.10) can still be enabled independently if the caller sets `ShowQuickFilter="true"`. Quick search is orthogonal to FilterMode.

Programmatic filter setting via `tableRef.SetFilterAsync(...)` still works — the filter applies, but there's no UI to show or change it. Useful when the consuming page drives all filtering via custom controls.

---

## §7.3 — Per-column filter UI (HeaderIcon mode detail)

### §7.3.1 — Default operator per column type

When the user first opens a column's filter popover with no current filter, the operator dropdown shows a sensible default:

| Column type | Default operator |
|---|---|
| Text, Mono | `Contains` |
| Number, Currency | `Equals` |
| Date | `On` |
| DateTime | `On` (operates on date portion) |
| Time | `Equals` |
| Boolean | `IsTrue` |
| Status | `In` (set filter — multi-select) |
| Link | `Contains` (on link text) |
| File | (filter disabled by default; `<FilterTemplate>` required) |
| Avatar, Actions | (filter disabled; icon hidden) |
| Custom | (filter disabled unless `<FilterTemplate>` provided) |

Caller override per column:

```csharp
[Parameter] public FilterOperator? DefaultFilterOperator { get; set; }
```

Useful when the column's data shape favors a different default — e.g., a "Tags" Text column where `Contains` would be sensible, but the data is exact-match enums so `Equals` is better.

### §7.3.2 — Operator list per column type

The operator dropdown contents per type:

#### Text / Mono / Link
- `Contains` (default)
- `Equals`
- `Not equals`
- `Starts with`
- `Ends with`
- `Does not contain`
- `Is empty`
- `Is not empty`

#### Number / Currency
- `Equals` (default)
- `Not equals`
- `Greater than`
- `Greater than or equal`
- `Less than`
- `Less than or equal`
- `Between`
- `Is empty`
- `Is not empty`

#### Date / DateTime
Single-date operators:
- `On` (default for Date)
- `Before`
- `After`
- `Between`
- `Is empty`
- `Is not empty`

Relative operators (covered fully in §7.5):
- `Today`, `Yesterday`, `Tomorrow`
- `This week`, `Last week`, `Next week`
- `This month`, `Last month`, `Next month`
- `This quarter`, `Last quarter`
- `This year`, `Last year`
- `Last N days`, `Next N days`

#### Time
- `Equals` (default)
- `Before`
- `After`
- `Between`
- `Is empty`
- `Is not empty`

#### Boolean
- `Is true` (default if Field is non-null bool)
- `Is false`
- `Is empty` (if Field is `bool?`)

The dropdown is rendered as `LipiSelect`. Filter operator labels are passed through `IStringLocalizer` if the consuming app uses localization; defaults are English.

### §7.3.3 — Value input per operator

The value input area below the operator dropdown changes based on the operator:

| Operator | Value input |
|---|---|
| `Contains`, `Equals`, `Not equals`, `Starts with`, `Ends with`, `Does not contain` (Text) | `LipiTextBox` |
| `Equals`, `Not equals`, `Greater than`, etc. (Number, Currency) | `LipiNumberInput` |
| `Between` (Number, Currency) | Two `LipiNumberInput` (min, max) |
| `On`, `Before`, `After` (Date) | `LipiDatePicker` |
| `Between` (Date, DateTime) | `LipiDateRangePicker` |
| `Last N days`, `Next N days` | `LipiNumberInput` (integer only, min=1) |
| `In` (Status set filter) | `LipiMultiSelect` with distinct values from data |
| All relative date operators (`Today`, `This week`, etc.) | (no value input; operator alone is the filter) |
| `Is true`, `Is false`, `Is empty`, `Is not empty` | (no value input) |

When the operator dropdown changes, the value input animates a 120ms cross-fade. The previous operator's value is cleared on change (no carry-over — different operators expect different value types).

### §7.3.4 — Filter icon states

| Icon state | Meaning | Visual |
|---|---|---|
| Hidden | Column has `Filterable="false"` OR `FilterMode="None"` | (no icon) |
| Faint | Column is filterable, no filter active | `--lipi-table-text-faint` |
| Active | Filter applied to this column | `--lipi-table-text-link` (primary color) |
| Hovered | Pointer over the icon | `--lipi-table-text` |
| Focused | Keyboard focus on the icon | `--lipi-table-text` + 2px outline |

The "active" state is the most important — it lets the user scan the header row and see at a glance which columns are filtered.

### §7.3.5 — Keyboard activation

The filter icon is keyboard-focusable. Activation:

| Input | Effect |
|---|---|
| Tab (when header focused) | Move focus to filter icon (if present) |
| Enter / Space (when filter icon focused) | Open filter popover |
| Alt+Down (when header text focused) | Open filter popover (skips having to tab to icon) |
| Escape (when popover open) | Close without applying |
| Enter (when value input focused) | Apply filter |

The popover traps focus when open (FocusTrap from Phase 2.6.2 infrastructure). Tab cycles through operator → value → Apply → Clear → operator.

---

## §7.4 — Filter operators full reference

Cross-reference §4.2.3 (`FilterOperator` enum). Per-operator semantics:

### Text operators

| Operator | Semantics | Case behavior |
|---|---|---|
| `Contains` | Substring match | Case-insensitive by default; `FilterCaseSensitive="true"` overrides |
| `Equals` | Exact match | Case-insensitive by default |
| `Not equals` | Logical negation of Equals | Same |
| `Starts with` | Prefix match | Same |
| `Ends with` | Suffix match | Same |
| `Does not contain` | Logical negation of Contains | Same |
| `Is empty` | Value is null OR empty string OR whitespace-only | (N/A) |
| `Is not empty` | Logical negation of Is empty | (N/A) |

Whitespace-trimming behavior: trailing/leading whitespace in the user's filter value is stripped before matching. To search for literal leading/trailing space, the consuming page must use a custom `<FilterTemplate>` that bypasses the trim.

### Numeric operators

| Operator | Semantics |
|---|---|
| `Equals` | Strict equality (with floating-point tolerance for `double` — see below) |
| `Not equals` | Logical negation |
| `Greater than` | `value > filter` |
| `Greater than or equal` | `value >= filter` |
| `Less than` | `value < filter` |
| `Less than or equal` | `value <= filter` |
| `Between` | `value >= min AND value <= max` (inclusive both ends) |
| `Is empty` | Value is null (for nullable types) |
| `Is not empty` | Negation |

**Floating-point tolerance:** for `double` / `float` column types, `Equals` uses an absolute tolerance of `1e-9`. For `decimal`, exact equality. For `int` / `long`, exact equality.

### Date / DateTime operators

| Operator | Semantics |
|---|---|
| `On` | Same calendar date (ignores time portion for DateTime) |
| `Before` | Strictly earlier calendar date |
| `After` | Strictly later calendar date |
| `Between` | `value >= start AND value <= end` (inclusive both ends, by calendar date) |
| All relative operators | See §7.5 |

### Set operators

| Operator | Semantics |
|---|---|
| `In` | `value` is in the set of selected items (Status filter pattern) |

### Universal operators

| Operator | Semantics |
|---|---|
| `Is true` | `value == true` (Boolean only) |
| `Is false` | `value == false` (Boolean only) |
| `Is empty` | Value is null / default / empty per column type |
| `Is not empty` | Negation of Is empty |

---

## §7.5 — Date relative operators (full set)

Per Q7.2 (locked) — the full list of relative date operators. Each requires a reference "now" — by default the current clinic-local time (via `IClinicTimezoneService` from Phase 2.4), not UTC, not system clock.

### §7.5.1 — Relative operators list

| Operator | Semantics |
|---|---|
| `Today` | Same calendar date as clinic-local "now" |
| `Yesterday` | One calendar day before |
| `Tomorrow` | One calendar day after |
| `This week` | Within current calendar week (week-start configurable per clinic, defaults Monday) |
| `Last week` | Within the previous calendar week |
| `Next week` | Within the next calendar week |
| `This month` | Within current calendar month |
| `Last month` | Within the previous calendar month |
| `Next month` | Within the next calendar month |
| `This quarter` | Within current calendar quarter (Q1=Jan-Mar, etc.) |
| `Last quarter` | Within the previous calendar quarter |
| `This year` | Within current calendar year |
| `Last year` | Within the previous calendar year |
| `Last N days` | Within the last N calendar days (inclusive of today) |
| `Next N days` | Within the next N calendar days (inclusive of today) |

### §7.5.2 — Week-start configuration

The "This week" / "Last week" / "Next week" operators depend on the week-start day. LipiTable reads this from `IDateFormatService.GetWeekStartDay()` (Phase 2.4 service). Default for India: Monday.

Some industries / locales prefer Sunday as week start (US healthcare commonly, religious calendars sometimes). The service-level config supports all 7 days; LipiTable reflects whatever it returns.

### §7.5.3 — Time-of-day semantics for DateTime

When a DateTime column uses date-relative operators, the **date portion** is compared, ignoring the time portion. "Today" matches any DateTime whose date is today in clinic-local time.

Example: a DateTime value of `2026-05-15T02:30:00+05:30` (very early morning local) matches `Today` if "now" is `2026-05-15T14:00:00+05:30` (same calendar date).

### §7.5.4 — Edge cases

- **Daylight saving boundaries:** "Today" / "Yesterday" use calendar dates in clinic-local time. DST transitions are handled by the timezone service.
- **Quarter boundaries:** quarters are calendar-based (Q1 = Jan-Mar) by default. Some clinical reporting uses fiscal quarters; that's a consuming-page concern via custom `<FilterTemplate>`.
- **Year boundaries:** December 31 and January 1 of the next year are "Last year" / "This year" respectively. Linear cutover at calendar midnight clinic-local.

### §7.5.5 — Server-side computation

In server-side mode, the relative operator and "now" reference are sent in `TableQueryRequest`. The server is responsible for computing the actual date range. To prevent clock-skew bugs:

- LipiTable sends `Operator=Today` (the symbolic name), NOT a resolved `[date1, date2]` range
- Server uses its own clock (and clinic timezone) to materialize the range

This ensures consistency: "Today" on a paginated query that crosses midnight doesn't suddenly mean two different dates between pages.

Alternative: LipiTable can send the resolved range (computed at client time). Default is **symbolic** for the consistency reason above; override via `RelativeDateFilterResolution="Client" | "Server"` (default Server). Most server-side consumers want server resolution.

---

## §7.6 — `Between` operator UI

The `Between` operator needs two value inputs. The popover layout adapts:

### §7.6.1 — Numeric Between

```
┌────────────────────────────┐
│ FILTER: AMOUNT             │
│ ┌──────────────────────┐   │
│ │ between          ▾   │   │
│ └──────────────────────┘   │
│ ┌─────────┐    ┌─────────┐  │
│ │ Min     │ →  │ Max     │  │
│ └─────────┘    └─────────┘  │
│      [Apply]   [Clear]     │
└────────────────────────────┘
```

Two `LipiNumberInput` side-by-side. Min ≤ Max validation runs on Apply; if Min > Max, the inputs swap silently (treating user input as "between these two values" regardless of order).

### §7.6.2 — Date Between (LipiDateRangePicker)

Per Q7.2 (locked) — `Between` for Date / DateTime uses `LipiDateRangePicker`:

```
┌─────────────────────────────────────────┐
│ FILTER: LAST VISIT                       │
│ ┌─────────────────────────────────────┐  │
│ │ between                       ▾     │  │
│ └─────────────────────────────────────┘  │
│ ┌─────────────────────────────────────┐  │
│ │ 2026-01-01  →  2026-05-15          │  │
│ │ (LipiDateRangePicker)               │  │
│ └─────────────────────────────────────┘  │
│        [Apply]      [Clear]              │
└─────────────────────────────────────────┘
```

LipiDateRangePicker has its own preset bundle (Last 7 days, This month, This quarter, etc.) — these presets work inside the filter context. When the user picks a preset, the date range fills in.

### §7.6.3 — Preset shortcuts vs date-relative operators

There's potential overlap: "Last 7 days" is available both as:
- A relative date operator (`LastNDays` with N=7)
- A preset inside `Between` (`LipiDateRangePicker`'s built-in preset)

Both work. The relative operator is more compact in the chip strip ("Last 7 days" reads cleaner than "between 2026-05-08 and 2026-05-15"). The preset-inside-Between produces an absolute range that won't shift as time passes.

Recommendation in the popover UX: when the user picks a `LipiDateRangePicker` preset that exactly matches a relative operator, offer a one-click "Use relative range instead" affordance. This converts the filter from absolute dates to a symbolic relative operator. Power-user feature, opt-in via `SuggestRelativeRange="true"` on the column.

Default: off. Most users don't need it.

---

## §7.7 — Custom filter templates

When the built-in filter UI doesn't fit, `<FilterTemplate>` on the column replaces it:

```razor
<LipiColumn Field="@(p => p.AgeYears)" Type="ColumnType.Number" Filterable="true">
    <FilterTemplate Context="ctx">
        <div class="custom-age-filter">
            <span>Pediatric age band:</span>
            <LipiButton OnClick="@(() => ctx.SetFilter(b => b.IsBetween(0, 1)))">Infant</LipiButton>
            <LipiButton OnClick="@(() => ctx.SetFilter(b => b.IsBetween(1, 12)))">Child</LipiButton>
            <LipiButton OnClick="@(() => ctx.SetFilter(b => b.IsBetween(13, 18)))">Teen</LipiButton>
            <LipiButton OnClick="@(() => ctx.SetFilter(b => b.IsBetween(19, 64)))">Adult</LipiButton>
            <LipiButton OnClick="@(() => ctx.SetFilter(b => b.GreaterThan(64)))">Senior</LipiButton>
            <LipiButton Variant="ButtonVariant.Secondary" OnClick="@ctx.ClearFilter">Clear</LipiButton>
        </div>
    </FilterTemplate>
</LipiColumn>
```

### §7.7.1 — `FilterContext<TValue>` API

Cross-reference §3.9. The context provided to `<FilterTemplate>` exposes:

```csharp
public sealed class FilterContext<TValue>
{
    public FilterOperator CurrentOperator { get; init; }
    public TValue? CurrentValue { get; init; }
    public TValue? CurrentValueEnd { get; init; }     // for Between
    public Action<Action<FilterBuilder<TValue>>> SetFilter { get; init; } = default!;
    public Action ClearFilter { get; init; } = default!;
    public Action Close { get; init; } = default!;
    public Action ApplyAndClose { get; init; } = default!;
}
```

`SetFilter` takes a fluent builder action. Available builder methods:

```csharp
public sealed class FilterBuilder<TValue>
{
    public FilterBuilder<TValue> IsEqualTo(TValue value);
    public FilterBuilder<TValue> IsNotEqualTo(TValue value);
    public FilterBuilder<TValue> Contains(string substring);
    public FilterBuilder<TValue> DoesNotContain(string substring);
    public FilterBuilder<TValue> StartsWith(string prefix);
    public FilterBuilder<TValue> EndsWith(string suffix);
    public FilterBuilder<TValue> GreaterThan(TValue value);
    public FilterBuilder<TValue> GreaterThanOrEqual(TValue value);
    public FilterBuilder<TValue> LessThan(TValue value);
    public FilterBuilder<TValue> LessThanOrEqual(TValue value);
    public FilterBuilder<TValue> IsBetween(TValue min, TValue max);
    public FilterBuilder<TValue> IsBefore(TValue value);
    public FilterBuilder<TValue> IsAfter(TValue value);
    public FilterBuilder<TValue> IsToday();
    public FilterBuilder<TValue> IsThisWeek();
    /* ... all relative date operators ... */
    public FilterBuilder<TValue> IsInLastNDays(int n);
    public FilterBuilder<TValue> IsInNextNDays(int n);
    public FilterBuilder<TValue> IsIn(IEnumerable<TValue> values);
    public FilterBuilder<TValue> IsEmpty();
    public FilterBuilder<TValue> IsNotEmpty();
    public FilterBuilder<TValue> IsTrue();
    public FilterBuilder<TValue> IsFalse();
    public FilterBuilder<TValue> Predicate(Func<TValue, bool> predicate);
}
```

The `Predicate(...)` escape hatch lets the caller supply an arbitrary client-side filter function. Note: `Predicate` is **client-side only** — in server-side mode it cannot be serialized to the wire, so it's not sent to the server. Developer-mode warning if `Predicate` is used in server-side mode.

### §7.7.2 — `<FilterTemplate>` rendering

The template renders inside the same popover that would have held the default UI. The popover container, Apply / Clear buttons, and outer styling are still LipiTable's — only the *contents* of the popover are caller-owned.

If the caller wants to skip the Apply / Clear buttons (e.g., the custom UI applies filters immediately on button click), set `<FilterTemplate AutoApply="true">` — the popover renders without footer actions and closes when the user clicks outside.

---

## §7.8 — Filter chips strip (region ②)

Active filters are surfaced as chips in a strip below the toolbar (region ② per §2.1). The strip is always rendered when ≥ 1 filter is active.

### §7.8.1 — Chip anatomy

```
[ Status: contains Active  ×  ]   [ Last visit: this month ×  ]   [ Clear all ]
```

Each chip:
- Column label (from column's `Header` or derived name)
- Colon separator
- Operator + value (or just operator for value-less operators like "is empty")
- ✕ close icon → removes that filter
- "Clear all" appears at the end when ≥ 2 chips

**Visual properties** (cross-reference §24.3.10):
- Chip background: `--lipi-table-chip-bg`
- Chip border: 1px solid `--lipi-table-chip-border`
- Border-radius: `--r-pill` (fully rounded)
- Padding: 4px 12px
- Font size: 12px
- ✕ hover: `--lipi-table-chip-x-hover` (danger color)

### §7.8.2 — Chip value formatting

The value text shown in the chip:

| Operator | Chip text |
|---|---|
| `Equals "Active"` | `"Status: Active"` (operator implicit) |
| `Contains "dr"` | `"Name: contains 'dr'"` |
| `Between 2026-01-01 and 2026-05-15` | `"Last visit: 2026-01-01 → 2026-05-15"` |
| `Last 7 days` | `"Last visit: last 7 days"` |
| `This month` | `"Last visit: this month"` |
| `In ["Active", "Pending"]` | `"Status: Active, Pending"` (or `"Status: 2 selected"` if > 3 values) |
| `Is empty` | `"Last visit: empty"` |
| `Is not empty` | `"Last visit: not empty"` |

For long values, the chip truncates with ellipsis at ~30 characters and shows full value on hover (LipiTooltip from Phase 2.7).

### §7.8.3 — Chip behaviors

- **Click ✕** → removes that filter from the column, re-runs the query
- **Click the chip body** (not the ✕) → opens the column's filter popover for editing (in HeaderIcon mode; opens the drawer scoll-anchored to that column in Drawer mode)
- **Click "Clear all"** → removes every column filter (but does NOT clear quick search; quick search has its own chip — see §7.10)

### §7.8.4 — Chip overflow

When many filters are active and the chips overflow the available width:

- Default: wrap to multiple rows (the chip strip expands vertically)
- If `FilterChipOverflow="Collapse"` is set on the table, chips beyond the first row collapse to a "+N more" chip that opens a popover listing them
- `FilterChipOverflow="Wrap"` (default) for tables with vertical space; `Collapse` for height-constrained surfaces

### §7.8.5 — Chip strip and Drawer mode

In `FilterMode.Drawer`, the chips render the same way. Clicking a chip opens the drawer and scrolls to that filter's section.

---

## §7.9 — Drawer filter mode detail

### §7.9.1 — Drawer trigger

In `FilterMode.Drawer`, the toolbar replaces the per-column filter icons with a single button:

```
[ ▽ Filters ]    or    [ ▽ Filters (3) ]   ← (N) shown when N filters active
```

Click → opens the LipiDrawer (right placement, compact 320px width by default).

### §7.9.2 — Drawer contents

Every filterable column renders a section:

```
┌──── Filters ─────────────────────────┐
│ NAME                                  │
│  ┌────────────────────────────────┐   │
│  │ contains  ▾                    │   │
│  └────────────────────────────────┘   │
│  ┌────────────────────────────────┐   │
│  │ Enter value...                 │   │
│  └────────────────────────────────┘   │
│                                       │
│ ROLE                                  │
│  ┌────────────────────────────────┐   │
│  │ equals    ▾                    │   │
│  └────────────────────────────────┘   │
│  ┌────────────────────────────────┐   │
│  │ Consultant                  ▾  │   │
│  └────────────────────────────────┘   │
│                                       │
│ STATUS                                │
│   ☑ Active                            │
│   ☑ Pending                           │
│   ☐ Locked                            │
│                                       │
│ ─────────────────────────────────     │
│        [Apply]  [Reset all]           │
└───────────────────────────────────────┘
```

Each section uses the same operator + value layout as the HeaderIcon popover, but stacked vertically and spaced more comfortably.

### §7.9.3 — Drawer apply semantics

- Changes inside the drawer are staged. The body of the table does NOT re-query during drawer interaction.
- `Apply` button at the bottom applies all changes simultaneously and closes the drawer.
- `Reset all` clears every filter and applies (table re-queries).
- Closing the drawer without `Apply` discards staged changes (with a "Discard unsaved filter changes?" confirm if there are any).

### §7.9.4 — Drawer custom layout

For pages where the default per-column auto-layout doesn't fit, the caller can provide a `<FilterDrawerTemplate>`:

```razor
<LipiTable FilterMode="FilterMode.Drawer" ...>
    <FilterDrawerTemplate Context="filterCtx">
        <section>
            <h3>Demographics</h3>
            <!-- caller-rendered filters for Name, DOB, Gender -->
        </section>
        <section>
            <h3>Clinical</h3>
            <!-- caller-rendered filters for Status, Last visit -->
        </section>
    </FilterDrawerTemplate>
</LipiTable>
```

When `<FilterDrawerTemplate>` is provided, LipiTable's drawer renders the template instead of the auto-layout. The caller uses `filterCtx` (table-level `FilterDrawerContext`) to invoke filter changes for any column.

---

## §7.10 — Quick search

Per Q7.3 (locked) — quick search supports both built-in opt-in (option B) and caller parameter (option C).

### §7.10.1 — Built-in opt-in (`ShowQuickFilter="true"`)

```razor
<LipiTable TItem="Patient" ... ShowQuickFilter="true">
```

A search input renders in the left zone of the toolbar:

```
┌─────────────────────────────────────────────────────────────────┐
│ [🔍 Search...]                          [▽ Filters (3)] [Export]│
└─────────────────────────────────────────────────────────────────┘
```

**Default behavior:**
- Text input (LipiTextBox with a search icon prefix)
- Debounced 300ms — typing doesn't fire a query per keystroke
- Matches case-insensitively against every string-renderable column value
- Empty string clears the quick search

The set of "string-renderable" columns is automatically computed: every column with `Type` in `{ Text, Mono, Link, Status, File }` plus any column where `<CellTemplate>` produces text. Number / Currency / Date / DateTime / Time / Avatar / Boolean / Actions columns are excluded by default.

### §7.10.2 — Caller parameter mode

```razor
<LipiTextBox @bind-Value="_searchText" Placeholder="Search patients..." />
<LipiTable TItem="Patient" QuickFilterText="@_searchText" ...>
    ...
</LipiTable>
```

The caller renders their own search input (anywhere on the page, with any styling) and passes the current text to LipiTable via the `QuickFilterText` parameter.

- LipiTable does NOT render its own toolbar search input in this mode
- Changes to `QuickFilterText` propagate immediately (no debounce — caller controls debounce externally if desired)
- Setting `QuickFilterText="@null"` or `""` clears

The two modes are mutually exclusive — setting both `ShowQuickFilter="true"` AND `QuickFilterText` is a developer-mode error.

### §7.10.3 — Custom matcher (`QuickFilterPredicate`)

For cases where the default "case-insensitive contains across string columns" doesn't fit:

```razor
<LipiTable TItem="Patient"
           ShowQuickFilter="true"
           QuickFilterPredicate="@CustomMatch">
    ...
</LipiTable>

@code {
    private bool CustomMatch(Patient p, string searchText)
    {
        return p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || p.Uhid.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || p.Mobile.EndsWith(searchText)
            || p.Aliases.Any(a => a.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    }
}
```

The predicate receives `(TItem, searchText)` and returns `bool`. Used in client-side mode only. In server-side mode, the search text goes to the server as `TableQueryRequest.QuickSearch`; the server's predicate is the source of truth.

### §7.10.4 — Quick search chip

When quick search is active, a special chip appears at the start of the filter chips strip:

```
[🔍 "dr reddy" ×]   [Status: Active ×]   [Last visit: this month ×]   [Clear all]
```

The 🔍 prefix distinguishes the quick search chip from regular filter chips. The ✕ clears the quick search (different action from "Clear all" which clears column filters).

---

## §7.11 — Server-side filtering

Cross-reference §4.2.3 (`FilterDescriptor`).

### §7.11.1 — Wire format

Filter state is sent in `TableQueryRequest.Filters`:

```csharp
public sealed record FilterDescriptor(
    string ColumnKey,
    FilterOperator Operator,
    object? Value,
    object? ValueEnd
);
```

Each active column filter is one descriptor. Quick search goes in the separate `QuickSearch` field of the request.

### §7.11.2 — Value type mapping

Filter values are `object?` over the wire. Type expectations per operator:

| Operator | Value type | ValueEnd |
|---|---|---|
| Text operators (Contains, Equals, etc.) | `string` | null |
| Number operators (Equals, GreaterThan, etc.) | numeric (`int`, `long`, `double`, `decimal`) | null (or numeric for Between) |
| Date / DateTime operators | `DateOnly` / `DateTime` / `DateTimeOffset` (matches column TValue) | same type for Between |
| Boolean operators (IsTrue, IsFalse) | `null` (operator is the filter) | null |
| Relative date operators (Today, etc.) | `null` (operator is the filter) | null |
| `LastNDays` / `NextNDays` | `int` (the N) | null |
| `In` (set filter) | `IReadOnlyList<object>` of TValue | null |
| `IsEmpty` / `IsNotEmpty` | null | null |

Server deserializes values per the column's declared type. Type mismatches (e.g., server expects `int` but gets `string`) should be rejected with a clear error.

### §7.11.3 — Server contract

The server applies the filter list to its underlying query. For SQL via EF Core:

```csharp
private static IQueryable<Patient> ApplyFilters(
    IQueryable<Patient> source,
    IReadOnlyList<FilterDescriptor> filters)
{
    foreach (var f in filters)
    {
        source = (f.ColumnKey, f.Operator) switch
        {
            ("Name", FilterOperator.Contains) =>
                source.Where(p => EF.Functions.ILike(p.Name, $"%{(string?)f.Value}%")),
            ("Name", FilterOperator.Equals) =>
                source.Where(p => p.Name == (string?)f.Value),
            ("UpdatedAt", FilterOperator.Today) =>
                source.Where(p => p.UpdatedAt.Date == DateTime.UtcNow.Date),
            // ... etc per column / operator combination ...
            _ => source
        };
    }
    return source;
}
```

LipiTable does NOT ship a generic filter applier (see §6.5.2 — the same reasoning applies: consuming pages know their entity types best). What LipiTable does ship is:

- The wire format (`FilterDescriptor`, `FilterOperator`)
- A clear list of operator semantics (this section)
- The reference relative-date resolution logic so server implementers know what "Today" means

### §7.11.4 — Multiple filters = AND

Multiple `FilterDescriptor`s in one request are combined with **AND**. The server applies each in sequence; only rows matching ALL filters pass through.

There's no built-in OR semantics. Cross-column OR is rare in tabular data — usually expressed as a single column's operator (e.g., `Status In [Active, Pending]` rather than `Status = Active OR Status = Pending`).

For cases where OR is genuinely needed, the consuming page uses a custom `<FilterTemplate>` that bundles multiple conditions into a single descriptor with a `Predicate(...)` (client-side only) or a custom `Operator` value the server recognizes.

### §7.11.5 — Quick search semantics on the server

`TableQueryRequest.QuickSearch` is a single string. The server decides what to do with it:

- Full-text search across configured columns
- Trigram / fuzzy match
- ILIKE on key fields
- Search-index lookup

LipiTable doesn't prescribe. Some servers will do a simple `OR` of `ILIKE` clauses; others will use Postgres `tsvector`; others will use Elasticsearch. The wire format is opaque to LipiTable.

---

## §7.12 — Filter persistence

### §7.12.1 — Persisted shape

When `TableId` is set, filter state is persisted to `identity.user_table_preferences`:

```json
{
  "filters": [
    { "columnKey": "Status", "operator": "in", "value": ["Active", "Pending"], "valueEnd": null },
    { "columnKey": "UpdatedAt", "operator": "lastNDays", "value": 7, "valueEnd": null }
  ],
  "quickSearch": "dr reddy"
}
```

### §7.12.2 — Persistence rules

- Filter changes are debounced 300ms before write
- Apply / Reset all triggers an immediate write (no debounce)
- Persisted state restored on next mount, overrides `DefaultFilters`
- Removed columns: persisted filter for non-existent ColumnKey is silently skipped
- "Reset to defaults" in column picker also clears persisted filters and applies `DefaultFilters`

### §7.12.3 — Sensitive filter values

Some filter values may be sensitive (e.g., patient identifiers, phone numbers). Per HIPAA / DPDP, persisting these to the DB may violate minimum-necessary disclosure rules.

LipiTable's posture: filter values are persisted by default. Consuming pages with sensitive columns should set `<LipiColumn PersistFilter="false">` to exclude that column's filter from persistence. Filter still works during the session; just doesn't survive across sessions.

The decision of "what's sensitive" is the consuming page's call. LipiTable doesn't analyze content.

Phase 2.10 audit item: review whether `PersistFilter="false"` should be the default for any column types, or whether it should be opt-out per column.

---

## §7.13 — Programmatic filter API

```csharp
public ValueTask SetFilterAsync(string columnKey, FilterOperator op, object? value, object? valueEnd = null);
public ValueTask SetFiltersAsync(IReadOnlyList<FilterDescriptor> filters);
public ValueTask RemoveFilterAsync(string columnKey);
public ValueTask ClearAllFiltersAsync();
public ValueTask SetQuickSearchAsync(string? text);

public IReadOnlyList<FilterDescriptor> CurrentFilters { get; }
public string? CurrentQuickSearch { get; }
public bool IsFilterActive(string columnKey);
```

Use case — link with pre-applied filter:

```razor
<LipiButton OnClick="@ShowActiveOnly">Show active patients only</LipiButton>

@code {
    private async Task ShowActiveOnly()
    {
        await _table!.SetFilterAsync(
            nameof(Patient.Status),
            FilterOperator.Equals,
            "Active");
    }
}
```

---

## §7.14 — Filter events

```csharp
public sealed record FilterChangedContext(
    IReadOnlyList<FilterDescriptor> NewFilters,
    IReadOnlyList<FilterDescriptor> PreviousFilters,
    string? NewQuickSearch,
    string? PreviousQuickSearch,
    FilterChangeReason Reason
);

public enum FilterChangeReason
{
    UserPopoverApply,
    UserPopoverClear,
    UserDrawerApply,
    UserDrawerReset,
    UserChipRemove,
    UserChipClearAll,
    UserQuickSearchChange,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ColumnRemoved
}

[Parameter] public EventCallback<FilterChangedContext> OnFilterChanged { get; set; }
```

---

## §7.15 — Interaction with sort, group, tree

### §7.15.1 — Filter + sort
Independent. Filter narrows rows; sort orders the remaining rows. Order of operations doesn't matter to the user (LipiTable applies filter, then sort).

### §7.15.2 — Filter + group
Group operates on filtered rows. Empty groups (where all rows are filtered out) are hidden by default. Override via `ShowEmptyGroups="true"` to keep them visible.

### §7.15.3 — Filter + tree
When filtering a tree, matching rows are shown along with their **ancestors** (so context is preserved). Non-matching siblings of an ancestor are hidden, but the path from root to the matched row remains visible.

Tree filtering uses these rules:
- A leaf row matches → it's shown along with all its ancestors
- An interior row matches its own filter criteria → it's shown along with ancestors AND its non-matching descendants (so the user can see the matched node's context downward)

This is industry standard for tree filtering (Windows Explorer search, file managers, hierarchical task lists).

---

## §7.16 — Performance considerations

### §7.16.1 — Client-side filter execution

In client-side mode, every filter change re-runs the filter pipeline against the full `Items` list. For 1000 rows × 5 filters, that's 5000 predicate evaluations per change. Negligible.

For 10,000+ rows, consider:
- Setting `DataSourceMode="ServerSide"` and moving filtering to a properly-indexed query
- OR `QueryDebounceMs="500"` to batch rapid changes
- OR splitting the data into smaller chunks (one table per category, etc.)

### §7.16.2 — Quick search performance

The built-in quick-search predicate iterates every string-renderable column for every row. For 1000 rows × 5 columns × 50-character string compares, that's 50,000 character compares per keystroke (post-debounce).

For larger datasets, the caller should provide a `QuickFilterPredicate` that targets specific columns (e.g., name + UHID only) rather than every string column.

### §7.16.3 — Server-side filter coalescing

LipiTable batches the user's filter changes within the 300ms debounce window into a single `TableQueryRequest`. If the user enters a name in the Name filter and a date range in the Date filter within 300ms, only one request fires with both filters.

The previous in-flight request (if any) is canceled per §4.5.3 cancellation rules.

---

## §7.17 — Disabling filtering

| Level | How | Effect |
|---|---|---|
| Per-column | `<LipiColumn Filterable="false">` | That column's filter icon hidden; ignored in Drawer mode |
| Per-table | `<LipiTable FilterMode="FilterMode.None">` | No filter UI anywhere |
| Per-feature | `<LipiTable ShowQuickFilter="false">` | Quick search hidden specifically |

`FilterMode.None` is the cleanest off-switch when no filtering is desired.

---

## §7.18 — Worked examples

### §7.18.1 — Default HeaderIcon mode

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           ShowQuickFilter="true"
           TableId="patients-list">
    <LipiColumn Field="@(p => p.Name)" Header="Name" />
    <LipiColumn Field="@(p => p.UpdatedAt)" Header="Updated" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(p => p.Status)" Header="Status" Type="ColumnType.Status" />
</LipiTable>
```

User can filter per-column via header icons + quick search across all string columns. All filters persist.

### §7.18.2 — Drawer mode for complex filter forms

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)"
           FilterMode="FilterMode.Drawer"
           FilterDrawerSize="LipiDrawerSize.Wide"
           ShowQuickFilter="true"
           TableId="lab-results-list">
    <LipiColumn Field="@(r => r.PatientName)" Header="Patient" />
    <LipiColumn Field="@(r => r.TestName)" Header="Test" />
    <LipiColumn Field="@(r => r.ReportedAt)" Header="Reported" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(r => r.Status)" Header="Status" Type="ColumnType.Status" />
    <LipiColumn Field="@(r => r.AbnormalFlag)" Header="Flag" Type="ColumnType.Boolean" />
</LipiTable>
```

Lab results table opens a side drawer for all filters at once. Quick search runs alongside.

### §7.18.3 — Custom filter for clinical age band

```razor
<LipiColumn Field="@(p => p.AgeYears)" Type="ColumnType.Number" Header="Age">
    <FilterTemplate Context="ctx">
        <div class="age-band-filter">
            <LipiButton Size="LipiSize.Small" OnClick="@(() => ctx.SetFilter(b => b.IsBetween(0, 1)))">
                Infant (<1)
            </LipiButton>
            <LipiButton Size="LipiSize.Small" OnClick="@(() => ctx.SetFilter(b => b.IsBetween(1, 12)))">
                Child (1-12)
            </LipiButton>
            <LipiButton Size="LipiSize.Small" OnClick="@(() => ctx.SetFilter(b => b.IsBetween(13, 18)))">
                Teen (13-18)
            </LipiButton>
            <LipiButton Size="LipiSize.Small" OnClick="@(() => ctx.SetFilter(b => b.IsBetween(19, 64)))">
                Adult (19-64)
            </LipiButton>
            <LipiButton Size="LipiSize.Small" OnClick="@(() => ctx.SetFilter(b => b.GreaterThan(64)))">
                Senior (65+)
            </LipiButton>
            <LipiButton Size="LipiSize.Small" Variant="ButtonVariant.Tertiary" OnClick="@ctx.ClearFilter">
                Any age
            </LipiButton>
        </div>
    </FilterTemplate>
</LipiColumn>
```

The popover shows five clinical age bands as one-click buttons instead of a number-range UI.

### §7.18.4 — Caller-driven quick search

```razor
<div class="page-header">
    <h1>Patients</h1>
    <LipiTextBox @bind-Value="_searchText" Placeholder="Search by name, UHID, mobile..." />
</div>

<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           QuickFilterText="@_searchText"
           KeySelector="@(p => p.Id)">
    ...
</LipiTable>
```

Search input lives in the page header, not LipiTable's toolbar. LipiTable receives the text and re-queries.

---

*End of §7. Proceed to §8 — Pagination.*


# LipiTable Spec — §8 Pagination

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>` (composing `LipiPagination`)
**Status:** Section body — draft for review
**Depends on:** §1, §4 (data sources), §5 (selection persistence), §24 (tokens)
**Cross-references:** `03-LipiPagination-Spec.md` (separate spec for the composed component)

---

## §8.1 — Composition relationship

LipiTable's pagination is delegated to `LipiPagination` — a Phase 2.8 sibling component (`03-LipiPagination-Spec.md`). LipiTable composes one or two LipiPagination instances in its layout regions (⑤ top, ⑧ᵦ bottom per §2.1).

The composition contract:
- LipiTable owns the pagination *state* (current page, page size)
- LipiPagination owns the pagination *rendering* (page buttons, page-size dropdown, row count display)
- State flows down via parameters; user actions flow up via callbacks

LipiTable is NOT a wrapper around LipiPagination — it's a composer. Other surfaces (card grids, list views, custom layouts) can use LipiPagination standalone with the same contract.

---

## §8.2 — Page size

### §8.2.1 — Default page size options

Per Q1.3 (locked) — the default page size options:

```csharp
public static readonly IReadOnlyList<int> DefaultPageSizeOptions = 
    new[] { 5, 10, 25, 50, 100, 200, int.MaxValue }; // int.MaxValue represents "All"
```

The default value is **25**. The dropdown shows `5 / 10 / 25 / 50 / 100 / 200 / All` (with `"All"` rendered as text for the `int.MaxValue` entry).

Override per table:

```razor
<LipiTable TItem="..." 
           DefaultPageSize="50"
           PageSizeOptions="@(new[] { 10, 50, 100 })"
           ...>
```

`PageSizeOptions` is `IReadOnlyList<int>`. `DefaultPageSize` must be a member of `PageSizeOptions` (else developer-mode warning and falls back to the first option).

### §8.2.2 — "All" semantics

Selecting "All" in the dropdown sends `PageSize = int.MaxValue` and `Page = 1`. Behavior differs by data source mode:

**Client-side mode:** LipiTable renders all rows on a single page. Virtualization (§20) still applies if row count exceeds threshold, so memory isn't an issue. No row count limit.

**Server-side mode:** Per Q1.3 (locked) — capped at `ServerSideAllCap` (default 1000).

```csharp
[Parameter] public int ServerSideAllCap { get; set; } = 1000;
```

When the user picks "All" in server mode:
- `TableQueryRequest.PageSize = ServerSideAllCap` (NOT `int.MaxValue`)
- `TableQueryRequest.RequestAllRows = true` (signal flag for the server)
- Server responds with up to `ServerSideAllCap` rows
- LipiTable renders a banner above the body (region ② position, before any filter chips):

```
┌───────────────────────────────────────────────────────────┐
│ ⓘ Showing first 1000 of 47,381 rows. Narrow your filters │
│   to see more, or pick a smaller page size.               │
└───────────────────────────────────────────────────────────┘
```

The server can override the banner text via `TableQueryResponse.CapBanner` (§4.3.8). LipiTable's default banner uses `TotalCount` and `Rows.Count` from the response.

The banner uses `lipi-table-footer-cap-banner` class (§2.2.8) for styling. Colors: info-tinted background (`--color-info-alpha-08`), info-700 text.

### §8.2.3 — Setting "All" cap to higher / unlimited

A consuming page that knows its server can handle unlimited can set `ServerSideAllCap="int.MaxValue"`. The cap banner won't appear; the server is responsible for honoring the request.

Conversely, a page expecting tiny datasets can set `ServerSideAllCap="100"` for safety.

### §8.2.4 — Empty page handling

If the current page index is beyond the available data (e.g., user was on page 5 of 5, applied a filter that removed all but 2 pages of results), LipiTable:

1. Snaps `Page` to the last valid page (`max(1, ceil(totalCount / pageSize))`)
2. Re-queries (server mode) or re-renders (client mode)
3. Fires `OnPageChanged` with reason `PageSnap`

The user sees the last page of results, not an empty page.

---

## §8.3 — Page navigation

### §8.3.1 — Navigation controls

LipiPagination renders these controls (full variants in `03-LipiPagination-Spec.md`):

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ Rows per page: [25 ▾]   Showing 51-75 of 487   [⏮] [◀] [1] ... [3] [4] [5] ... [20] [▶] [⏭]   Page [3] of 20 [Go] │
└──────────────────────────────────────────────────────────────────────────────┘
```

Three variants supported per LipiPagination spec:

| Variant | Composition |
|---|---|
| `Full` (default) | Page size + row count + page buttons + jump-to-page |
| `Compact` | Prev/next + "Page N of M" + page size |
| `Minimal` | Prev/next only |

LipiTable selects per `PaginationVariant` parameter:

```csharp
[Parameter] public PaginationVariant PaginationVariant { get; set; } = PaginationVariant.Full;
```

### §8.3.2 — First / last buttons

`Full` variant includes `⏮` (First) and `⏭` (Last) buttons. Useful for tables with many pages. Disable via:

```csharp
[Parameter] public bool ShowFirstLast { get; set; } = true;
```

### §8.3.3 — Page number buttons

Show up to `MaxPageButtons` numbered page buttons centered around current page, with ellipsis (`...`) for skipped ranges:

```
[1] ... [3] [4] [5] ... [20]    ← current page = 4, MaxPageButtons = 5
[1] [2] [3] [4] [5]              ← current page = 3, MaxPageButtons = 5, total pages = 5
```

Default `MaxPageButtons = 5`. Configurable via:

```csharp
[Parameter] public int MaxPageButtons { get; set; } = 5;
```

For extreme page counts (1000+ pages), use `Compact` variant — the numbered buttons become noisy.

### §8.3.4 — Jump-to-page input

`Full` variant includes a small input + "Go" button for jumping directly to a page number:

```
Page [3] of 20 [Go]
```

Useful for tables with many pages where clicking through page-by-page is tedious. Hidden by default in `Compact` and `Minimal` variants. Configurable:

```csharp
[Parameter] public bool ShowJumpToPage { get; set; } = true;  // default for Full
```

The input is a `LipiNumberInput` constrained to `[1, totalPages]`. Enter / Go button navigates; invalid input shows inline error.

### §8.3.5 — Row count display

`Full` and `Compact` variants render row count text:

| State | Text |
|---|---|
| Normal | `"Showing 51-75 of 487"` |
| Single row | `"Showing 1 of 1"` |
| Empty | (row count hidden when no rows; the empty state owns the body) |
| Unknown total (`TotalCount = -1`) | `"Showing 51-75 of many"` |
| "All" mode, capped | (cap banner takes over per §8.2.2; row count still shows actual range) |

Text customizable per LipiPagination spec via `RowCountTemplate` parameter on LipiTable:

```csharp
[Parameter] public Func<PaginationRange, string>? RowCountTemplate { get; set; }
```

Where `PaginationRange` is `(start, end, total)` ints. Useful for i18n or domain-specific phrasing ("Showing items 51 through 75 of 487 patients").

---

## §8.4 — Pagination placement

### §8.4.1 — `PaginationPlacement` parameter

```csharp
public enum PaginationPlacement
{
    Bottom,    // default — region ⑧ᵦ from §2.1
    Top,       // region ⑤ from §2.1
    Both,      // top + bottom (rare; useful for very tall tables)
    None       // no built-in pagination; caller composes their own
}
```

`None` is the escape hatch — the caller renders their own LipiPagination instance somewhere on the page, wired to the table's state via two-way binding on `CurrentPage` and `CurrentPageSize`:

```razor
<LipiTable @ref="_table" TItem="Patient" 
           DataSource="@LoadPatientsAsync"
           PaginationPlacement="PaginationPlacement.None"
           @bind-CurrentPage="_page"
           @bind-CurrentPageSize="_pageSize">
    ...
</LipiTable>

<!-- Custom pagination location -->
<div class="my-custom-pagination-wrapper">
    <LipiPagination 
        @bind-CurrentPage="_page"
        @bind-PageSize="_pageSize"
        TotalCount="@_total" />
</div>
```

### §8.4.2 — When to use each placement

| Placement | Use case |
|---|---|
| `Bottom` | Standard — most tables. User scans rows top-to-bottom; pagination is at the natural end. |
| `Top` | When the table is in a fixed-height container that's never tall enough to scroll, and the user wants pagination immediately visible. |
| `Both` | Very long pages where the user pages within the table while scrolling. Reduces back-and-forth. |
| `None` | Custom layout — pagination in a sidebar, footer, sticky bar, etc. |

---

## §8.5 — Pagination modes

### §8.5.1 — `PaginationMode` parameter

```csharp
public enum PaginationMode
{
    Paginated,         // default — discrete pages with navigation
    InfiniteScroll,    // load next page when user scrolls near bottom
    LoadMore           // explicit "Load more" button at bottom
}
```

`Paginated` is the default and applies for all standard cases.

### §8.5.2 — InfiniteScroll mode

When `PaginationMode="InfiniteScroll"`:

- Pagination control hidden (regions ⑤ and ⑧ᵦ collapse)
- Body scrolls naturally; near-bottom triggers next page load
- LipiTable maintains a "loaded so far" buffer — each fetched page is appended to the visible rows
- Virtualization (§20) still operates within the buffer
- Server-side mode is required (`Items` mode with all-in-memory makes infinite scroll pointless)
- Row count display reads "Loaded 87 of 487" while scrolling, "Loaded all 487" when complete

Scroll-trigger threshold: when the user scrolls within 200px of the buffer bottom, the next page fetch fires. Threshold configurable via:

```csharp
[Parameter] public int InfiniteScrollThresholdPx { get; set; } = 200;
```

Mutually exclusive with `Paginated`. Cannot combine.

### §8.5.3 — LoadMore mode

When `PaginationMode="LoadMore"`:

- Pagination control hidden
- A "Load more" button renders at the bottom of the body
- Click → fetches next page, appends to buffer
- Row count display same as InfiniteScroll
- Button shows spinner during fetch
- Button auto-hides when all data is loaded (`Rows.Count == TotalCount`)

LoadMore is the more conservative variant of InfiniteScroll — explicit user action vs scroll-triggered. Some consuming pages prefer this for control (user knows when a fetch is happening, no surprise loading on scroll).

### §8.5.4 — Restrictions on InfiniteScroll / LoadMore

| Feature | Paginated | InfiniteScroll | LoadMore |
|---|---|---|---|
| Sort | ✓ (re-queries from page 1) | ✓ (clears buffer, re-fetches) | ✓ (clears buffer, re-fetches) |
| Filter | ✓ | ✓ (clears buffer, re-fetches) | ✓ (clears buffer, re-fetches) |
| Group | ✓ | ✓ (with caveat — group boundaries can span scroll fetches; see §9) | ✓ (same caveat) |
| Tree | ✓ | ✓ | ✓ |
| Master-detail | ✓ | ✓ | ✓ |
| Inline edit | ✓ | ✓ (caveat: edited row's position may shift on scroll fetch if data is mutated) | ✓ |
| Selection across pages | ✓ | ✓ (selection persists across buffer growth) | ✓ |
| Aggregation footer | ✓ (per-page or full) | Full only (running totals as buffer grows) | Same |
| Server-side cap "All" | ✓ | (N/A — no page size dropdown) | (same) |

The infinite scroll caveat for grouping (§9.7) is the trickiest interaction. Documented gotcha for consuming pages.

---

## §8.6 — Pagination + selection

Cross-reference §5.2 — locked behavior: selection persists across page navigation via the row-key set.

Pagination-specific notes:
- Changing the current page does NOT clear selection
- Changing the page size does NOT clear selection (rows on screen change; selection state for visible rows is preserved)
- Snapping to a valid page after filter change does NOT clear selection
- "Select all on page" only operates on currently visible rows (the new page's rows)

The "select all across pages" flag (§5.3.6) and pagination interact correctly: the flag covers all rows matching current filters across all pages.

---

## §8.7 — Pagination + grouping

When grouping is active:

- **Client-side mode:** pagination operates on rows AFTER grouping. The page boundary may split a group across two pages. Standard behavior; the user navigates within the grouped view.
- **Server-side mode with `PreGrouped` response (§4.3.5):** pagination operates on groups. Each "page" is a slice of the group tree. The server is responsible for consistent group/page semantics.
- **Server-side mode with flat rows:** pagination operates on rows. Groups are rendered client-side from the page's flat list. Page boundary may split a group. Generally avoid this combination — pre-grouped is cleaner.

The locked decision (§9.6 — to be detailed in §9): server-side grouping should use pre-grouped response shape.

---

## §8.8 — Pagination + sort + filter

Order of operations:
1. Filter narrows the dataset
2. Sort orders the filtered dataset
3. Pagination slices the sorted dataset

In client-side mode, every change re-runs the pipeline. In server-side mode, every change re-fetches with the new state in `TableQueryRequest`.

When sort or filter changes, **`Page` snaps to 1** (because the previous page index is meaningless on the new dataset). User behavior: applying a filter takes them back to page 1 of the filtered view. This matches every mainstream table library.

The page snap fires `OnPageChanged` with reason `SortFilterReset`.

---

## §8.9 — Pagination persistence

### §8.9.1 — What persists

Per Q10.1 (locked) — page size is one of the persisted preferences. Not the current page (per-session navigation, not durable preference).

Persisted shape (cross-reference §21):

```json
{
  "pageSize": 50
}
```

When the user changes page size, it's persisted (debounced 300ms). Next visit to the same `TableId` restores the page size. Default `25` if no persistence.

### §8.9.2 — What does NOT persist

- Current page (`Page`) — always resets to 1 on next visit
- "All" mode selection — does NOT persist (each visit starts at the saved size, not at "All")

The reasoning for not persisting current page: between visits, the data may have changed materially. Landing the user on "page 47" of a list they last saw two weeks ago is more disorienting than landing them on page 1 with their saved sort / filter / page size restored.

Similarly, "All" is a transient power-user choice ("show me everything right now"); persisting it can surprise the user on next visit with a slow render of thousands of rows.

Exception: `RestoreLastPageOnMount="true"` parameter overrides this for consuming pages where last-page restoration makes sense (e.g., an audit log where page numbers map to time periods).

---

## §8.10 — Programmatic pagination API

```csharp
public ValueTask GoToPageAsync(int page);
public ValueTask NextPageAsync();
public ValueTask PreviousPageAsync();
public ValueTask FirstPageAsync();
public ValueTask LastPageAsync();
public ValueTask SetPageSizeAsync(int pageSize);

public int CurrentPage { get; }
public int CurrentPageSize { get; }
public int TotalPages { get; }
public long TotalCount { get; }
public bool HasMore { get; }    // true in InfiniteScroll/LoadMore when more data exists
public ValueTask LoadMoreAsync();   // InfiniteScroll/LoadMore programmatic trigger
```

Use case — link with deep page state:

```razor
<LipiButton OnClick="@JumpToLastPage">Jump to last page</LipiButton>

@code {
    private Task JumpToLastPage() => _table!.LastPageAsync().AsTask();
}
```

URL-parameter sync (consuming page wires this up):

```razor
<LipiTable TItem="..." 
           @bind-CurrentPage="_page"
           @bind-CurrentPageSize="_pageSize"
           OnPageChanged="@SyncUrlAsync"
           ...>
```

`@code { private async Task SyncUrlAsync(...) { /* push to NavigationManager.NavigateTo with new ?page=... */ } }`

---

## §8.11 — Pagination events

```csharp
public sealed record PageChangedContext(
    int NewPage,
    int PreviousPage,
    int CurrentPageSize,
    PageChangeReason Reason
);

public sealed record PageSizeChangedContext(
    int NewPageSize,
    int PreviousPageSize,
    int CurrentPage
);

public enum PageChangeReason
{
    UserClickPageNumber,
    UserClickPrevNext,
    UserClickFirstLast,
    UserJumpToPage,
    UserKeyboardNav,
    Programmatic,
    PageSnap,             // filter / data change made current page invalid; snapped to valid
    SortFilterReset,      // sort or filter change reset to page 1
    InfiniteScroll,       // next page auto-fetched in infinite mode
    LoadMoreClick,        // explicit "Load more" button
    PersistenceRestore    // not for current page (doesn't persist), but for page size
}

[Parameter] public EventCallback<PageChangedContext> OnPageChanged { get; set; }
[Parameter] public EventCallback<PageSizeChangedContext> OnPageSizeChanged { get; set; }
```

`OnPageChanged` fires whenever `CurrentPage` changes, regardless of cause. `OnPageSizeChanged` is separate so callers can distinguish "user navigated" from "user changed page size."

---

## §8.12 — Single-page collapse

When the total result count fits in a single page (`TotalCount <= CurrentPageSize`), the pagination control may collapse:

```csharp
[Parameter] public bool HideWhenSinglePage { get; set; } = false;
```

- `false` (default): pagination always renders (shows "Showing 1-N of N" + a disabled prev/next; minimum visual presence)
- `true`: pagination control completely hidden when only one page exists

`true` is sometimes preferred for clean visuals on tables that often have small result sets. `false` is the conservative default — pagination presence reinforces that the user is looking at all the data, not just the first slice.

---

## §8.13 — Pagination in empty / loading / error states

### §8.13.1 — Empty state

When the body is in `Empty` or `FilteredEmpty` state (no rows to render):
- Pagination control hidden (region ⑧ᵦ collapses)
- Row count text hidden
- The empty state UI (region ⑦ body) owns the surface

### §8.13.2 — Loading state

While the data is loading:
- Pagination control stays visible BUT controls are disabled (`Disabled="true"` on LipiPagination)
- Row count shows previous values until response arrives
- Refresh button (if shown) spins

Once loading completes, controls re-enable and row count updates.

### §8.13.3 — Error state

When the data fetch errors:
- Pagination control stays visible but disabled
- Row count shows last known values OR "—" if no prior data
- The error state UI in the body invites retry

---

## §8.14 — Worked examples

### §8.14.1 — Standard server-side patient list

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list"
           DefaultPageSize="50"
           PaginationPlacement="PaginationPlacement.Bottom"
           ServerSideAllCap="2000">
    ...
</LipiTable>
```

Default page size 50, max 2000 with "All". Pagination at bottom. User can pick from default options.

### §8.14.2 — Compact pagination for narrow surface

```razor
<LipiTable TItem="Note"
           Items="@_notes"
           KeySelector="@(n => n.Id)"
           PaginationVariant="PaginationVariant.Compact"
           DefaultPageSize="5"
           PageSizeOptions="@(new[] { 5, 10, 25 })">
    ...
</LipiTable>
```

Notes table inside a side panel. Compact variant fits narrow width.

### §8.14.3 — Infinite scroll audit log

```razor
<LipiTable TItem="AuditEvent"
           DataSource="@LoadAuditAsync"
           KeySelector="@(e => e.Id)"
           PaginationMode="PaginationMode.InfiniteScroll"
           DefaultPageSize="50"
           InfiniteScrollThresholdPx="300"
           Virtualize="VirtualizeMode.Always">
    ...
</LipiTable>
```

Audit log auto-loads on scroll. Larger threshold for early prefetch. Virtualization always on (audit log can grow huge).

### §8.14.4 — Custom pagination location

```razor
<LipiTable @ref="_table"
           TItem="Patient"
           DataSource="@LoadPatientsAsync"
           PaginationPlacement="PaginationPlacement.None"
           @bind-CurrentPage="_page"
           @bind-CurrentPageSize="_size">
    ...
</LipiTable>

<aside class="patient-list-sidebar">
    <LipiPagination 
        @bind-CurrentPage="_page"
        @bind-PageSize="_size"
        TotalCount="@_total"
        Variant="PaginationVariant.Compact" />
</aside>
```

LipiTable renders without its own pagination; the consuming page places LipiPagination in a sidebar with two-way state binding.

---

*End of §8. Proceed to §12 — Inline editing (the largest section).*


# LipiTable Spec — §9 Grouping

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review (back-fill section)
**Depends on:** §1, §3 (column model — Groupable, AllowGroup), §4 (data sources — GroupDescriptor, PreGrouped, GroupBucket), §15 (aggregation — group-level), §7 (filtering — set filter distinct values)
**Decisions consolidated:** Q4.1, Q4.2, Q4.3, Q5.2 (group/tree exclusion)

---

## §9.0 — §4 amendment: distinct values for set filters and grouping

While drafting §7 (filtering) and §9 (this section), a gap surfaced in the §4 response shape: the **set-filter dropdown** (used for `In` operator on Status columns, multi-select pickers) and the **grouping discovery** ("what are the distinct departments?") both need a list of distinct values per column. Client-side mode computes these from `Items`. Server-side mode has no built-in way to deliver them today.

### §9.0.1 — Amendment to `TableQueryRequest`

Add a new field signaling which columns need distinct-value lists:

```csharp
public sealed record TableQueryRequest
{
    // ... existing fields per §4.2.1 ...
    public IReadOnlyList<string> DistinctValueColumns { get; init; } = Array.Empty<string>();
}
```

LipiTable populates this list with column keys that:
- Have an active set filter (or are about to open the set-filter picker), OR
- Are the active grouping column (server needs to know group memberships)

### §9.0.2 — Amendment to `TableQueryResponse<TItem>`

Add the corresponding response field:

```csharp
public sealed record TableQueryResponse<TItem>(
    IReadOnlyList<TItem> Rows,
    long TotalCount)
{
    // ... existing optional fields per §4.3 ...
    public IReadOnlyDictionary<string, IReadOnlyList<object?>>? DistinctValues { get; init; }
}
```

Keyed by column key. Each value is the ordered list of distinct values present in the filtered dataset (NOT the current page). The list is what populates the set-filter multi-select picker AND informs LipiTable about possible group values for collapsed-on-mount logic.

For columns with very high cardinality (millions of distinct strings — e.g., a free-text Notes column), the server should NOT include the column in this dictionary even if asked. LipiTable falls back to client-side rendering: the picker shows "Type to search..." instead of a full checkbox list. This is the same UX as a `LipiCombobox` falling back from list to search.

### §9.0.3 — Caller override via column parameter

For static-enum columns where round-tripping the list is wasteful, the caller can short-circuit via existing column parameters:

```razor
<LipiColumn Field="@(p => p.Status)" 
            Type="ColumnType.Status"
            StatusOptions="@(new[] { "Active", "Pending", "Locked" })" />
```

`StatusOptions` is already on the column (per §3.3.2) for the edit dropdown. When set, LipiTable reuses it for:
1. The set-filter multi-select picker
2. The grouping menu (when the column is groupable)

When `StatusOptions` is set, LipiTable does NOT add the column to `TableQueryRequest.DistinctValueColumns` (no need — the list is local).

For Text / Number / Date columns without a Status-style options list but where the caller pre-knows the distinct values:

```csharp
[Parameter] public IReadOnlyList<object?>? DistinctValuesForFilter { get; set; }
```

Same short-circuit behavior: when set, server isn't asked.

### §9.0.4 — When neither server nor caller provides distinct values

If LipiTable needs distinct values for a column AND:
- Client-side mode → computes from `Items` (always available)
- Server-side mode AND no `DistinctValues` in response AND no `StatusOptions` / `DistinctValuesForFilter` on column → the set-filter picker degrades to a search-only input (`LipiCombobox` text-search mode); grouping shows groups as data arrives but no pre-computed group list

This degradation is graceful — the user can still filter and group. Just the "see all possible values at once" affordance is missing.

### §9.0.5 — File location of amendment

The amendment is reflected in `01-LipiTable-S04-DataSources.md` (§4.2.1, §4.3) once this §9 is locked. The two response shapes are kept consistent across sections.

---

## §9.1 — Overview

Grouping organizes rows into collapsible buckets based on a column's value. Each unique value gets its own group; rows sharing that value appear together under a group header that can be expanded or collapsed.

```
▾ Department: Cardiology (12)              ← group header (with item count)
   Dr. Reddy        Consultant   Active
   Dr. Patel        Surgeon      Active
   Dr. Sharma       Resident     Inactive
▾ Department: Neurology (8)
   Dr. Iyer         Consultant   Active
   Dr. Kumar        Surgeon      Active
▸ Department: Oncology (15)                ← collapsed
```

Grouping is opt-in:
- Per Q4.1 — `ShowGroupBar="false"` (default). Caller can still group declaratively via `GroupBy` parameter without showing the drag bar.
- Per Q4.3 — default group state is caller's pick via `DefaultGroupState`, default Expanded.

Grouping is multi-level: a table can group by Department, then within each Department by Sub-unit, then within each Sub-unit by Status.

---

## §9.2 — `GroupBy` parameter

### §9.2.1 — Declarative grouping

```razor
<LipiTable TItem="Doctor"
           Items="@_doctors"
           KeySelector="@(d => d.Id)"
           GroupBy="@(new[] { nameof(Doctor.Department) })">
    <LipiColumn Field="@(d => d.Name)" />
    <LipiColumn Field="@(d => d.Department)" Groupable="true" />
    <LipiColumn Field="@(d => d.Role)" />
    <LipiColumn Field="@(d => d.Status)" Type="ColumnType.Status" />
</LipiTable>
```

`GroupBy` is `IReadOnlyList<string>` — list of column keys. When set, the table renders with those columns active as group dimensions.

Single-level grouping: one entry in the list.
Multi-level grouping: multiple entries; first = outermost.

When `GroupBy` is null or empty (default), no grouping. Rows render flat.

### §9.2.2 — Column-level `Groupable`

Per the column model (§3.2):

```razor
<LipiColumn Field="@(d => d.Department)" Groupable="true" />
```

`Groupable="false"` (default) means:
- The column doesn't appear in the drag-to-group bar's allowed-drop list
- Attempting to group by it programmatically (via `GroupBy` parameter) is rejected with a developer-mode warning

`Groupable="true"` enables the column for grouping. Status / Text / Mono / Boolean columns are common groupable targets. Number / Currency / Date columns can be groupable but produce many small groups (one per unique value), so it's usually more useful to bucket them first via custom logic.

### §9.2.3 — Sortable interaction

A grouped column can still be sorted — the sort applies to the group order (not the rows within groups). See §6.6 (Sort and grouping interaction).

### §9.2.4 — Hiding the grouped column

When a column is the active grouping dimension, the column data is in the group header. Showing it in each row is redundant.

By default, LipiTable **does not** auto-hide grouped columns — the column data still appears per row. To explicitly hide grouped columns from the body:

```razor
<LipiTable HideGroupedColumns="true" ...>
```

When `true`, columns active in `GroupBy` are hidden from the body grid (they still appear in the group header). Excel-style behavior.

---

## §9.3 — Group bar (drag-to-group)

Per Q4.1 (locked = B) — drag bar opt-in via `ShowGroupBar="true"`.

### §9.3.1 — Visual rendering

```
┌─ Drag column here to group ─────────────────┐
│ [Department ×]   [Status ×]                  │   ← active group dimensions, drag to reorder
└──────────────────────────────────────────────┘
```

Region ④ from §2.1. CSS classes per §2.2.5.

Empty state: dashed border + hint text "Drag column here to group".
Active state: solid border + active group chips with ✕ to remove + reorder handle.

### §9.3.2 — Drag from column header

User can drag a column header into the group bar:

1. User starts dragging a column header (per the reorder gesture from §13.3 — 200ms / 8px threshold)
2. If `ShowGroupBar="true"`, the group bar highlights as a valid drop target
3. Drop the column into the group bar → column becomes the grouping dimension (appended to existing list for multi-level)
4. The column's data renders as group headers

### §9.3.3 — Reorder grouping levels

When multiple group dimensions are active, the chips in the group bar appear left-to-right matching outermost-to-innermost grouping order. Dragging chips reorders the dimensions:

```
[Department ×]   [Status ×]              ← Dept is outer, Status is inner

(user drags Status to leftmost)

[Status ×]   [Department ×]              ← Status is now outer, Dept is inner
```

The body re-renders with the new grouping hierarchy.

### §9.3.4 — Remove grouping

Click ✕ on a chip → removes that dimension from grouping. The body re-renders with one less level.

Removing all chips → ungroups entirely. Body renders flat.

### §9.3.5 — Constraints

| Action | Allowed? |
|---|---|
| Drag a `Groupable="false"` column into the group bar | No — the drag is rejected (chip won't drop) |
| Drag the same column twice | No — duplicate dimensions are rejected (no-op) |
| Drag a column already pinned-left or pinned-right | Yes — the column gets unpinned implicitly when it becomes a group dimension |
| Group by an Avatar / File / Actions / Custom column | No — these types are not groupable (per §3.3.1) |
| Group while a row is in inline edit | Yes — the dirty-state confirm (§12.8.4) fires first |

### §9.3.6 — Opt-out

```razor
<LipiTable ShowGroupBar="false" ...>   <!-- default -->
```

Group bar is hidden. Declarative `GroupBy` still works. The user just can't drag-to-group at runtime.

---

## §9.4 — Group header row

### §9.4.1 — Default rendering

```
▾ Department: Cardiology (12)
```

Anatomy:
- Expand/collapse chevron (▾ expanded / ▸ collapsed)
- Group dimension label ("Department:") — small text, muted
- Group value ("Cardiology") — bold, primary text
- Item count "(12)" — muted text

CSS class: `lipi-table-group-row` (§2.2.7). Background: `--lipi-table-bg-group-row` (subtle tint).

The group header row **spans all data columns**. The selection / expand-chevron / actions columns don't have group-header content. The first column slot has the chevron + label + value + count.

When aggregates are configured per §15.4.4, the group header row also shows per-column aggregates aligned to their respective columns.

### §9.4.2 — `<GroupHeaderTemplate>` slot

Per Q4.2 (locked) — caller can override the group header:

```razor
<LipiTable TItem="Doctor" ...>
    <GroupHeaderTemplate Context="group">
        <div class="custom-group-header">
            <LipiBadge Variant="@DeptColor(group.Value)">@group.Value</LipiBadge>
            <span>@group.ItemCount doctors</span>
            <span class="dept-meta">
                @group.Aggregates["Salary"] avg salary · 
                @group.Aggregates["Count"] active
            </span>
        </div>
    </GroupHeaderTemplate>
    
    <!-- columns -->
</LipiTable>
```

`GroupContext<TItem>` provides:

```csharp
public sealed class GroupContext<TItem>
{
    public string ColumnKey { get; }                  // "Department"
    public object? Value { get; }                     // "Cardiology"
    public string FormattedValue { get; }             // value formatted via column's Format/FormatFn
    public long ItemCount { get; }
    public IReadOnlyList<TItem> Items { get; }        // rows in this group (visible page only in server mode)
    public IReadOnlyDictionary<string, object?>? Aggregates { get; }
    public IReadOnlyList<GroupContext<TItem>>? SubGroups { get; }   // when multi-level
    public bool IsExpanded { get; }
    public int Depth { get; }                         // 0 = outermost
    
    public Task ExpandAsync();
    public Task CollapseAsync();
    public Task ToggleAsync();
}
```

When `<GroupHeaderTemplate>` is set, it replaces the default rendering. The chevron is still rendered by LipiTable (caller's template doesn't include it) so the expand/collapse affordance is consistent.

### §9.4.3 — Group selection checkbox

When `SelectionMode="Multi"` is active, the group header row includes a selection checkbox in the selection column position. State:

| Group state | Checkbox |
|---|---|
| 0 rows in group selected | unchecked |
| All rows in group selected | checked |
| Some rows in group selected | indeterminate |

Click → toggles selection for all rows in the group (adds / removes their keys from the selected set).

### §9.4.4 — Group sticky-on-scroll

For long groups, the group header can stick to the top of the viewport as the user scrolls past:

```csharp
[Parameter] public bool GroupHeaderSticky { get; set; } = false;
```

When `true`, scrolling past a group's header keeps it visible at the top until the next group's header replaces it. Provides context for which group the user is currently viewing.

Default `false` (no sticky group headers) to keep the implementation simple. Set `true` for tables where groups are long enough to scroll past entirely.

---

## §9.5 — Default group state

Per Q4.3 (locked) — caller picks via `DefaultGroupState`:

```csharp
public enum DefaultGroupState
{
    Expanded,    // default — all groups start expanded
    Collapsed    // all groups start collapsed
}
```

```razor
<LipiTable DefaultGroupState="DefaultGroupState.Collapsed" ...>
```

Default `Expanded`. User can expand / collapse individual groups by clicking the chevron.

### §9.5.1 — Persistence

When `TableId` is set, the user's expand/collapse state per group **does not** persist across sessions. Each new session starts with the `DefaultGroupState`.

The reasoning: groups can change between sessions (new departments added, old ones archived). Persisting "Department: Oncology is collapsed" loses meaning when Oncology no longer exists.

For "remember which groups I collapsed in this session," the consuming page can wire this via the `OnGroupExpand` / `OnGroupCollapse` events and store state in their own preferences.

### §9.5.2 — Programmatic expand/collapse

```csharp
public ValueTask ExpandGroupAsync(string groupPath);
public ValueTask CollapseGroupAsync(string groupPath);
public ValueTask ToggleGroupAsync(string groupPath);
public ValueTask ExpandAllGroupsAsync();
public ValueTask CollapseAllGroupsAsync();
```

`groupPath` is a "/"-delimited path through the group hierarchy. For single-level grouping: just the group value (`"Cardiology"`). For multi-level: full path (`"Cardiology/ICU"`).

---

## §9.6 — Multi-level grouping

### §9.6.1 — Declaration

```razor
<LipiTable GroupBy="@(new[] { 
    nameof(Doctor.Department), 
    nameof(Doctor.Subunit) 
})" ...>
```

Two levels: outermost = Department, innermost = Subunit. Rendered:

```
▾ Department: Cardiology (12)
   ▾ Sub-unit: ICU (5)
      Dr. Reddy
      Dr. Patel
      Dr. Sharma
   ▾ Sub-unit: Cath Lab (4)
      Dr. Iyer
      Dr. Kumar
   ▸ Sub-unit: OPD (3)              ← collapsed
▸ Department: Neurology (8)         ← collapsed (outer)
```

Inner groups are indented relative to outer groups. The indent uses `--lipi-table-tree-indent` (§24.2.6) — same token as tree data.

### §9.6.2 — Independent expand/collapse

Each group level expand/collapse is independent. Collapsing an outer group also hides its inner groups (which retain their own state); expanding the outer reveals them in their saved state.

### §9.6.3 — Aggregates at each level

Aggregates compute at every group level. The outermost group's aggregate is over all rows in that outer group (across all inner groups). The inner group's aggregate is over just its rows.

The grand-total footer (§15.4.5) shows the total across all groups at every level.

### §9.6.4 — Deep nesting limits

LipiTable supports up to 5 levels of grouping. Beyond that, the visual indent + scrolling becomes unmanageable. Setting `GroupBy` to more than 5 entries → developer-mode warning; only first 5 are honored.

Practical limit: 2-3 levels is typical. 4+ levels suggests a tree-data shape (§10) might be a better fit.

---

## §9.7 — Group / tree exclusion

Per Q5.2 (locked = A) — grouping and tree data are **mutually exclusive**.

### §9.7.1 — Why

Tree data has a built-in hierarchical structure (parent → children). Grouping imposes a different hierarchical structure (by group column value). Combining them creates nested-tree-of-groups-of-trees confusion that no end user can navigate.

### §9.7.2 — Detection

When the table has both `GroupBy` and (`ChildrenSelector` or `ParentSelector`) set:

| Mode | Behavior |
|---|---|
| Development | Hard `InvalidOperationException` at parameter-set time |
| Production | Tree data wins; grouping is silently dropped (warning logged) |

The Phase 2.10 audit will verify no consuming page has both set.

---

## §9.8 — Client-side grouping

### §9.8.1 — Computation

In `Items` mode, LipiTable computes groups in memory via LINQ `GroupBy`:

1. Apply filters → filtered rows
2. Apply sort → sorted rows
3. Group by `GroupBy[0]` → first-level groups
4. Within each first-level group, recursively group by remaining `GroupBy` entries
5. Compute per-group aggregates
6. Render

This is straightforward LINQ. Performance: O(N) per grouping level. For 1000 rows × 3 group levels, ~3000 operations. Negligible.

### §9.8.2 — Group order

By default, groups are ordered by the group column's sort direction (asc on the group column). User-set sort on the group column changes group order.

If sort is active on a non-group column, that sort applies WITHIN each group (rows within a group are sorted by the non-group column). Group order remains alphabetical / natural by the group column.

For custom group ordering (e.g., chronological months instead of alphabetical), set the column's `SortComparer` — LipiTable uses it for both row order and group order.

### §9.8.3 — Pagination interaction

Cross-reference §8.7. In client-side mode, pagination operates on rows after grouping. A page boundary may split a group across two pages.

For "one group per page" semantics, set a very small page size and use `Compact` pagination variant, OR switch to infinite-scroll / load-more mode.

---

## §9.9 — Server-side grouping

### §9.9.1 — Two approaches

Cross-reference §4.6 + §8.7. Server-side grouping has two response shapes:

**Approach 1 — Pre-grouped (RECOMMENDED):**

Server returns `TableQueryResponse.PreGrouped` as a tree of `GroupBucket<TItem>`. LipiTable renders directly. Pagination operates on group order (e.g., "page 1 = first 50 groups across all levels").

```csharp
PreGrouped = new[]
{
    new GroupBucket<Doctor>(
        ColumnKey: "Department",
        GroupValue: "Cardiology",
        ItemCount: 12,
        Items: cardiologyDoctorsPage,
        Aggregates: new Dictionary<string, object?> { ["Salary_Avg"] = 850000m },
        SubGroups: cardiologySubunits
    ),
    new GroupBucket<Doctor>(
        ColumnKey: "Department",
        GroupValue: "Neurology",
        ItemCount: 8,
        Items: neurologyDoctorsPage,
        Aggregates: new Dictionary<string, object?> { ["Salary_Avg"] = 920000m },
        SubGroups: neurologySubunits
    )
}
```

**Approach 2 — Flat:**

Server returns flat `Rows` with the understanding that LipiTable will group client-side. Page boundary may split a group. Generally avoid this for server-side grouping.

### §9.9.2 — Pre-grouped pagination semantics

When `PreGrouped` is returned with pagination:

- `TableQueryResponse.TotalCount` = total number of rows across all groups (not group count)
- Pagination control still shows "Showing X-Y of Z rows"
- Groups can be partially loaded — `GroupBucket.ItemCount` is the full count, `GroupBucket.Items` is just what's on this page
- When the user expands a group whose `Items` is partial, LipiTable fires `OnGroupExpand` with a request flag that signals "fetch more rows for this group"

### §9.9.3 — Lazy-load group contents

For very large groups (10,000+ rows in one group), pre-grouped responses can return `Items` as empty:

```csharp
new GroupBucket<Doctor>(
    ColumnKey: "Department",
    GroupValue: "Cardiology",
    ItemCount: 487,
    Items: Array.Empty<Doctor>(),    // not loaded yet
    Aggregates: cardiologyAggregates,
    SubGroups: null
)
```

The group renders as collapsed by default (regardless of `DefaultGroupState`). Expanding it fires `OnGroupExpand` with a flag:

```csharp
public sealed record GroupExpandContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    bool RequiresContentFetch,    // true when Items is empty but ItemCount > 0
    bool IsManualExpand
);
```

Caller can re-invoke `DataSource` with the group's `Items` populated. LipiTable replaces the bucket's `Items` and renders.

This pattern keeps server response sizes bounded — the initial load returns group structure + aggregates, content fetches happen on-demand per expand.

### §9.9.4 — Group ordering on the server

The server is responsible for the order of `PreGrouped` entries. LipiTable renders them in the order received. The server should honor the sort state for the group column, but LipiTable doesn't re-sort the group list client-side.

---

## §9.10 — Interaction with other features

### §9.10.1 — Sort

Cross-reference §6.6. Sort applies within groups by default. Clicking the grouped column's header sorts the group order.

### §9.10.2 — Filter

Filter applies before grouping. Filtered-out rows don't appear in any group. Empty groups (where all members were filtered out) are hidden by default:

```csharp
[Parameter] public bool ShowEmptyGroups { get; set; } = false;
```

Setting `true` keeps empty groups visible (showing "(0)" item count). Useful when groups represent expected categories ("Always show all 5 priority levels, even when empty").

### §9.10.3 — Selection

The group selection checkbox (§9.4.3) toggles selection for all rows in the group. Cross-page selection persistence (§5.2) applies — selecting a group's rows from page 1 and navigating to page 2 keeps them in the selected set.

### §9.10.4 — Inline edit

Editing a row's grouped column value moves the row to a different group (or creates a new group if the value doesn't exist). The transition is animated (200ms slide) so the user sees the row shift.

If the new group is collapsed, the row "disappears into" the collapsed group. A brief toast confirms: "Moved Dr. Reddy to 'Cardiology'."

### §9.10.5 — Aggregation

Cross-reference §15.4.4. Group-level aggregates render in group headers. Footer aggregate row shows the grand total across all groups when `ShowFooterAggregateWhenGrouped="true"` (default).

### §9.10.6 — Density

Group header rows respect the active density (compact = smaller header, etc.). Group indent for multi-level grouping uses `--lipi-table-tree-indent` which scales with density.

### §9.10.7 — Virtualization

Cross-reference §20. Virtualization works through grouping — visible group headers + visible rows are virtualized together. Collapsed groups contribute one virtual row (the header). Expanded groups contribute the header + all visible rows.

For very deep/wide trees of groups, the virtualization recomputes when groups expand/collapse. Scroll position is preserved by group path.

### §9.10.8 — Persistence

Per Q10.1 (locked) — group state is one of the persisted preferences:

```json
{
  "groupBy": ["Department", "Subunit"]
}
```

Saved when the user adds / removes / reorders groups via the group bar OR programmatically. Restored on next mount. Cleared on "Reset to defaults."

The expanded/collapsed state per group does NOT persist (per §9.5.1).

---

## §9.11 — Programmatic API

```csharp
public ValueTask GroupByAsync(params string[] columnKeys);
public ValueTask AddGroupByAsync(string columnKey);
public ValueTask RemoveGroupByAsync(string columnKey);
public ValueTask ClearGroupingAsync();
public ValueTask ReorderGroupByAsync(IReadOnlyList<string> columnKeys);

public ValueTask ExpandGroupAsync(string groupPath);
public ValueTask CollapseGroupAsync(string groupPath);
public ValueTask ToggleGroupAsync(string groupPath);
public ValueTask ExpandAllGroupsAsync();
public ValueTask CollapseAllGroupsAsync();

public IReadOnlyList<string> CurrentGroupBy { get; }
public IReadOnlyList<string> ExpandedGroupPaths { get; }
public bool IsGroupExpanded(string groupPath);
```

Use case — link with pre-grouped layout:

```razor
<LipiButton OnClick="@GroupByStatus">Group by Status</LipiButton>

@code {
    private Task GroupByStatus() =>
        _table!.GroupByAsync(nameof(Doctor.Status)).AsTask();
}
```

---

## §9.12 — Group events

```csharp
public sealed record GroupChangedContext(
    IReadOnlyList<string> NewGroupBy,
    IReadOnlyList<string> PreviousGroupBy,
    GroupChangeReason Reason
);

public enum GroupChangeReason
{
    UserDragToGroupBar,
    UserRemoveFromGroupBar,
    UserReorderGroupBar,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

public sealed record GroupExpandedContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    bool RequiresContentFetch,
    GroupExpandReason Reason
);

public sealed record GroupCollapsedContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    GroupCollapseReason Reason
);

public enum GroupExpandReason   { UserClickChevron, Programmatic, DefaultExpanded }
public enum GroupCollapseReason { UserClickChevron, Programmatic, DefaultCollapsed }

[Parameter] public EventCallback<GroupChangedContext> OnGroupChanged { get; set; }
[Parameter] public EventCallback<GroupExpandedContext> OnGroupExpand { get; set; }
[Parameter] public EventCallback<GroupCollapsedContext> OnGroupCollapse { get; set; }
```

Use cases:
- `OnGroupExpand` with `RequiresContentFetch=true` → caller fetches group's rows and updates the data source
- `OnGroupChanged` → URL parameter sync (`?groupBy=Department,Status`)

---

## §9.13 — Disabling grouping

| Setting | Effect |
|---|---|
| `<LipiColumn Groupable="false">` (default) | Column cannot be a grouping dimension |
| `ShowGroupBar="false"` (default) | Drag-to-group bar hidden; declarative GroupBy still works |
| `GroupBy="@null"` or `@(Array.Empty<string>())` | No grouping active |
| `AllowGrouping="false"` (table-level) | Grouping disabled globally; GroupBy parameter is ignored |

`AllowGrouping="false"` is the off switch when a consuming page wants to ensure no grouping happens regardless of accidental parameter setting.

---

## §9.14 — Worked examples

### §9.14.1 — Simple single-level grouping

```razor
<LipiTable TItem="Doctor"
           Items="@_doctors"
           KeySelector="@(d => d.Id)"
           GroupBy="@(new[] { nameof(Doctor.Department) })"
           DefaultGroupState="DefaultGroupState.Expanded"
           HideGroupedColumns="true">
    <LipiColumn Field="@(d => d.Name)" />
    <LipiColumn Field="@(d => d.Department)" Groupable="true" />
    <LipiColumn Field="@(d => d.Role)" />
    <LipiColumn Field="@(d => d.Status)" Type="ColumnType.Status" />
</LipiTable>
```

Doctors grouped by Department. Department column hidden from body (data is in group headers). Groups start expanded.

### §9.14.2 — User-driven multi-level grouping

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)"
           ShowGroupBar="true"
           TableId="lab-results">
    <LipiColumn Field="@(r => r.PatientName)" />
    <LipiColumn Field="@(r => r.TestCategory)" Groupable="true" />
    <LipiColumn Field="@(r => r.OrderingDept)" Groupable="true" />
    <LipiColumn Field="@(r => r.Value)" Type="ColumnType.Number" />
    <LipiColumn Field="@(r => r.Status)" Type="ColumnType.Status" Groupable="true" />
</LipiTable>
```

User can drag any of three columns into the group bar. They might pick "Test Category → Status" for a two-level grouping. Choice persists.

### §9.14.3 — Lazy-loaded large groups (server-side)

```razor
<LipiTable TItem="AuditEvent"
           DataSource="@LoadAuditAsync"
           KeySelector="@(e => e.Id)"
           GroupBy="@(new[] { nameof(AuditEvent.Date) })"
           DefaultGroupState="DefaultGroupState.Collapsed"
           OnGroupExpand="@HandleGroupExpandAsync">
    <LipiColumn Field="@(e => e.Date)" Type="ColumnType.Date" />
    <LipiColumn Field="@(e => e.Actor)" />
    <LipiColumn Field="@(e => e.Action)" />
</LipiTable>

@code {
    private async Task HandleGroupExpandAsync(GroupExpandedContext ctx)
    {
        if (ctx.RequiresContentFetch)
        {
            await _table!.RefreshAsync();   // re-fetch with this group expanded
        }
    }
}
```

Audit log grouped by date. Groups start collapsed. Expanding a date fires the lazy-load callback; server returns that date's events; LipiTable renders them.

### §9.14.4 — Custom group header with aggregate

```razor
<LipiTable TItem="Invoice"
           Items="@_invoices"
           KeySelector="@(i => i.Id)"
           GroupBy="@(new[] { nameof(Invoice.Department) })">
    
    <GroupHeaderTemplate Context="g">
        <div class="invoice-group-header">
            <strong>@g.FormattedValue</strong>
            <LipiBadge>@g.ItemCount invoices</LipiBadge>
            <span class="group-total">
                Total: @(((decimal?)g.Aggregates?["Amount"])?.ToString("C") ?? "—")
            </span>
        </div>
    </GroupHeaderTemplate>
    
    <LipiColumn Field="@(i => i.Number)" />
    <LipiColumn Field="@(i => i.Department)" Groupable="true" />
    <LipiColumn Field="@(i => i.Amount)" 
                Type="ColumnType.Currency"
                AggregateFn="LipiAggregate.Sum" />
</LipiTable>
```

Invoice list grouped by department. Custom group header shows department name, invoice count badge, and total amount inline.

---

*End of §9. Proceed to §10 — Tree data.*


# LipiTable Spec — §10 Tree data

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model), §4 (TableQueryResponse.TreeChildren), §5 (selection), §9 (tree/group exclusion), §24 (tree indent token)
**Decisions consolidated:** Q5.1, Q5.2

---

## §10.1 — Overview

Tree data renders hierarchical rows: each row may have child rows, each child may have its own children, and so on. Click the expand chevron on a parent row to reveal its children; click again to collapse.

```
▾ Dr. Reddy (Cardiology)
   ▾ Dr. Patel (Cardiology - ICU)
      Nurse Iyer
      Nurse Kumar
   ▸ Dr. Sharma (Cardiology - Cath Lab)
▾ Dr. Krishnan (Neurology)
   Nurse Singh
   Nurse Joseph
▸ Dr. Mehta (Oncology)
```

Tree data differs from **grouping** (§9):
- **Grouping** derives hierarchy from a column's value at runtime
- **Tree data** uses the data's own hierarchical structure (parent-child relationships in the model)

The two are mutually exclusive (per Q5.2, locked) — a table is either tree-shaped or grouped, never both.

---

## §10.2 — Data shape: two supported models

Per Q5.1 (locked = C) — LipiTable supports both nested and flat representations of trees. Caller picks the one matching their data:

### §10.2.1 — Nested shape via `ChildrenSelector`

The model has children embedded as a nested collection:

```csharp
public class OrgNode
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public List<OrgNode> Children { get; set; } = new();
}
```

LipiTable navigates the tree via the selector:

```razor
<LipiTable TItem="OrgNode"
           Items="@_rootNodes"
           KeySelector="@(n => n.Id)"
           ChildrenSelector="@(n => n.Children)">
    <LipiColumn Field="@(n => n.Name)" Header="Name" />
</LipiTable>
```

`ChildrenSelector` returns an `IEnumerable<TItem>` of immediate children, or `null` / empty for leaf rows.

`Items` is the root list — top-level rows. LipiTable recursively traverses `ChildrenSelector` for each row to discover descendants.

### §10.2.2 — Flat shape via `ParentSelector`

The model has parents referenced by ID:

```csharp
public class Folder
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }    // null for root folders
    public string Name { get; set; } = "";
}
```

LipiTable builds the tree by matching parent IDs:

```razor
<LipiTable TItem="Folder"
           Items="@_allFolders"
           KeySelector="@(f => f.Id)"
           ParentSelector="@(f => f.ParentId)">
    <LipiColumn Field="@(f => f.Name)" Header="Folder" />
</LipiTable>
```

`ParentSelector` returns the parent's key (matching the `KeySelector` value of the parent row), or `null` for root rows. `Items` is the flat list of ALL rows; LipiTable groups by parent-id to construct the tree.

### §10.2.3 — When to use which

| Use | Shape |
|---|---|
| Data already comes nested from API (JSON tree, EF Include hierarchy) | Nested via `ChildrenSelector` |
| Data is flat in DB (parent_id foreign key) and fetched as flat list | Flat via `ParentSelector` |
| Lazy-load children on demand from server | Nested with empty children; populate via `TreeChildren` response (§10.10) |

Both shapes produce identical UX. The choice is about the data layer, not the user experience.

### §10.2.4 — Mutual exclusion

Setting BOTH `ChildrenSelector` AND `ParentSelector` is an error:

- Development: throws `InvalidOperationException`
- Production: `ChildrenSelector` wins; `ParentSelector` ignored with warning

Setting NEITHER means tree mode is off — the table renders flat (no expand chevrons, no indent, no tree behavior).

---

## §10.3 — Tree rendering

### §10.3.1 — Expand chevron

Each row that has children renders an expand chevron in the first content column (after selection/expand-chevron system columns):

```
[☐]  ▾  Dr. Reddy        Cardiology
[☐]  ▸  Dr. Patel        Cardiology      ← collapsed (has children)
[☐]      Nurse Iyer       Cardiology      ← no chevron (leaf)
```

Chevron states:
- ▾ (chevron-down) — expanded
- ▸ (chevron-right) — collapsed
- (none) — leaf row (no children)

Chevron column width follows density: 28px / 36px / 44px (per §24.2.5). The chevron is part of the cell where the row's "name" / primary text rendered; it's not a separate column.

### §10.3.2 — Tree indent

Children are indented to show their depth in the tree. Each depth level adds `--lipi-table-tree-indent` of left padding (16px / 24px / 32px per density):

```
Depth 0 (root):       ▾ Dr. Reddy
Depth 1 (children):       ▾ Dr. Patel
Depth 2 (grand-children):     Nurse Iyer
```

The indent applies to the cell content, not the cell itself. The cell's grid position stays the same; only the inner content padding changes.

CSS class: `lipi-table-cell-indent` (§2.2.7) with `padding-left: calc(var(--lipi-table-tree-indent) * var(--depth))` inline-styled per row.

Caller override:

```csharp
[Parameter] public string? TreeIndent { get; set; }
```

When set, overrides the density-derived indent. Useful for very deep trees where the default indent would push leaf content off-screen.

### §10.3.3 — Indent guide lines (optional)

For deep trees, optional vertical guide lines show ancestry:

```
▾ Dr. Reddy
│  ▾ Dr. Patel
│  │   Nurse Iyer
│  │   Nurse Kumar
│  ▸ Dr. Sharma
▸ Dr. Krishnan
```

Opt-in:

```csharp
[Parameter] public bool ShowTreeIndentGuides { get; set; } = false;
```

Default off. Guide lines add visual weight that helps for trees deeper than 3 levels but is noise for shallow trees.

CSS: rendered as `background-image: linear-gradient(...)` on the indented cell content — pure CSS, no extra DOM.

---

## §10.4 — Expand / collapse

### §10.4.1 — User interactions

| Trigger | Effect |
|---|---|
| Click chevron | Toggle the row's expand state |
| Enter / Space on focused row | Toggle (when not in another mode like edit) |
| ArrowRight on collapsed row | Expand |
| ArrowLeft on expanded row | Collapse |
| ArrowLeft on collapsed row | Move focus to parent row |
| Double-click row body | Toggle (when no `OnRowDoubleClick` is set) |

Keyboard model matches Windows Explorer / VS Code file explorer. Standard pattern.

### §10.4.2 — Default expand state

Per `DefaultTreeState` parameter:

```csharp
public enum DefaultTreeState
{
    Collapsed,    // all parents start collapsed — default
    Expanded,     // all parents start expanded
    FirstLevelExpanded   // root rows expanded, deeper levels collapsed
}
```

Default `Collapsed` — for large trees, expanding all by default would be overwhelming. `FirstLevelExpanded` is a good middle-ground.

```razor
<LipiTable DefaultTreeState="DefaultTreeState.FirstLevelExpanded" ...>
```

### §10.4.3 — Expand state persistence

Per the pattern established in §9.5.1 — tree expand state does NOT persist across sessions by default. Each new session starts from `DefaultTreeState`.

Same rationale: data can change between sessions; expanded-state for keys that no longer exist is meaningless.

Opt-in for consuming pages that want persistence:

```csharp
[Parameter] public bool PersistTreeExpandState { get; set; } = false;
```

When `true`, expanded row keys persist to user prefs (cross-reference §21). Stale keys (no longer in data) are silently dropped on read.

Tree expand state persistence is reasonable for stable hierarchies (org charts, file system folders). For dynamic hierarchies (real-time worklists), keep the default.

### §10.4.4 — Programmatic expand/collapse

```csharp
public ValueTask ExpandRowAsync(TItem row);
public ValueTask CollapseRowAsync(TItem row);
public ValueTask ToggleRowAsync(TItem row);
public ValueTask ExpandAllAsync();
public ValueTask CollapseAllAsync();
public ValueTask ExpandToDepthAsync(int depth);    // expand all up to depth N

public bool IsRowExpanded(TItem row);
public IReadOnlyList<TItem> ExpandedRows { get; }
```

`ExpandToDepthAsync(2)` expands all rows at depth 0 and 1 (so depth-2 rows are visible). Useful for "show me the first three levels" patterns.

### §10.4.5 — Animation

Expand and collapse use a 200ms slide animation (children slide in from top with opacity fade). Respects `prefers-reduced-motion` (instant when reduced motion is requested).

For trees with very large groups (100+ children), the animation can be disabled per-table:

```csharp
[Parameter] public bool TreeAnimateExpand { get; set; } = true;
```

Setting `false` makes expand/collapse instant. Useful for performance-sensitive surfaces.

---

## §10.5 — Tree filtering

When filtering is active and tree data is shown, the filter must preserve hierarchy. Standard tree-filter rules:

- A leaf row matches the filter → it's shown along with ALL its ancestors (context preserved up to root)
- An interior row matches → it's shown along with all ancestors AND all descendants (the user can drill down from a matched ancestor)
- Non-matching siblings of a matched ancestor are hidden — the path from root to the match is the only visible path through that subtree

```
Filter: name contains "patel"

▾ Dr. Reddy (Cardiology)            ← shown (ancestor of match)
   ▾ Dr. Patel (Cardiology - ICU)   ← matched
      Nurse Iyer                     ← shown (descendant of matched interior node)
      Nurse Kumar                    ← shown
   (Dr. Sharma row hidden — sibling of match, no match in its subtree)
(Dr. Krishnan row hidden — no match in its subtree)
```

### §10.5.1 — Auto-expand on filter

When a filter is applied, matching rows' ancestors auto-expand so the matches are visible. When the filter clears, the expand state reverts to whatever the user had before (NOT the `DefaultTreeState`).

LipiTable tracks two expand states:
- User-initiated expand state (the user's actual chevron clicks)
- Filter-induced expand state (auto-expanded to show matches)

When filter clears, filter-induced expansions collapse back; user-initiated expansions remain.

### §10.5.2 — Match-only mode (alternative)

For trees where preserving ancestors creates too much noise:

```csharp
[Parameter] public TreeFilterMode TreeFilterMode { get; set; } = TreeFilterMode.PreserveAncestors;

public enum TreeFilterMode
{
    PreserveAncestors,    // default — show match + ancestors + descendants
    MatchOnly             // show only matched rows; tree structure collapsed flat
}
```

`MatchOnly` flattens the tree to a list of matched rows (no chevrons, no indent). Useful when the user is searching for a specific item and doesn't care about context.

---

## §10.6 — Tree sorting

Cross-reference §6.7. Sort operates **within each parent's children** — siblings sort against each other. Parents don't sort across siblings of other parents.

```
Sort by Name asc:

▾ Cardiology (parent)
   Dr. Patel       ← siblings sorted by Name
   Dr. Reddy
   Dr. Sharma
▾ Neurology (parent)
   Dr. Iyer        ← siblings sorted independently
   Dr. Krishnan
```

The two top-level parents (Cardiology, Neurology) sort against each other at the root level. Within each, children sort against their siblings only.

This preserves the tree structure under sort. Sorting by Name does NOT flatten the tree into a single ordered list.

---

## §10.7 — Tree selection

Per §5 — selection is a set of row keys. Tree selection has one additional design decision: when a parent is selected, do its children become selected too?

### §10.7.1 — Cascade modes

```csharp
public enum TreeSelectionCascade
{
    None,        // default — parent and children are independent
    Descendants, // selecting a parent selects all descendants recursively
    Manual       // caller handles via OnRowSelect callback
}
```

```razor
<LipiTable TreeSelectionCascade="TreeSelectionCascade.Descendants" ...>
```

**Default `None`** — each row is selected independently. Selecting a parent does NOT affect its children. Most clinical / data-table use cases want this.

**`Descendants`** — selecting a parent selects all visible AND non-visible descendants (entire subtree). Deselecting a parent deselects all descendants. Partial-children-selected → parent shows indeterminate checkbox.

**`Manual`** — neither cascade fires; the caller's `OnSelectionChanged` handler is responsible for implementing the desired semantics.

### §10.7.2 — Indeterminate state with cascade

When `TreeSelectionCascade="Descendants"` and a parent has some but not all descendants selected, the parent's checkbox shows the **indeterminate** state (the half-filled checkbox visual).

Clicking the indeterminate checkbox:
- If MORE than half of descendants are selected → checks the parent (selects all remaining descendants)
- If LESS than half → unchecks the parent (deselects all descendants)
- If exactly half → checks (rounds up)

This matches Gmail / Outlook bulk-selection patterns.

### §10.7.3 — Cascade with lazy-loaded children

When `Descendants` cascade is active and a parent has lazy-loaded children (not yet fetched), selecting the parent:
- Selects the parent's key immediately
- Sets a "select-all-descendants-when-loaded" flag for that parent
- When the user later expands and the children load, they're auto-selected

Bulk action handlers must account for this — caller should call `tableRef.GetSelectedKeysAsync()` to get the resolved set (LipiTable will fetch any unloaded children that have the pending-select flag).

---

## §10.8 — Tree + grouping exclusion

Per Q5.2 (locked = A) — repeated from §9.7.

Setting BOTH `GroupBy` AND (`ChildrenSelector` or `ParentSelector`) → tree wins, grouping silently dropped (with developer-mode error in Development).

The Phase 2.10 audit verifies no consuming page has both.

---

## §10.9 — Client-side tree

### §10.9.1 — Computation with `ChildrenSelector`

LipiTable traverses the tree depth-first from each item in `Items`:

```csharp
private void TraverseTree(TItem node, int depth, List<RenderedRow> output)
{
    output.Add(new RenderedRow(node, depth, ChildrenSelector(node)?.Any() == true));
    if (IsExpanded(node))
    {
        foreach (var child in ChildrenSelector(node) ?? Enumerable.Empty<TItem>())
        {
            TraverseTree(child, depth + 1, output);
        }
    }
}
```

The result is a flat ordered list of `(row, depth, hasChildren)` tuples — ready for rendering and virtualization.

Performance: O(N) per render where N = visible rows. For 10,000 row trees with most parents collapsed, only ~100 rows are rendered at any time. Negligible cost.

### §10.9.2 — Computation with `ParentSelector`

LipiTable groups `Items` by `ParentSelector(item)` once at mount and on `Items` reference change:

```csharp
private Dictionary<object?, List<TItem>> _childrenByParent = new();

private void RebuildIndex()
{
    _childrenByParent = Items
        .GroupBy(item => ParentSelector(item))
        .ToDictionary(g => g.Key, g => g.ToList());
}

private IEnumerable<TItem> GetChildren(TItem parent)
{
    return _childrenByParent.TryGetValue(KeySelector(parent), out var children)
        ? children
        : Enumerable.Empty<TItem>();
}
```

The index makes lookup O(1) per parent. Traversal is the same as ChildrenSelector.

### §10.9.3 — Pagination with trees

Cross-reference §8.5. Pagination on tree data operates on the **flattened, currently-visible row list**:

- A 50-row page = 50 visible rows (collapsed parents count as 1; expanded parents + their visible descendants count individually)
- Expanding a parent can push some rows to the next page
- Collapsing pulls them back

This is the natural behavior most users expect. For "one root + all its descendants on one page" semantics, the consuming page should adjust page size or use `InfiniteScroll` mode.

---

## §10.10 — Server-side tree

### §10.10.1 — Initial load

The server returns root-level rows in `TableQueryResponse.Rows` and `TotalCount` (count of roots, not total tree nodes).

LipiTable renders the roots collapsed (per `DefaultTreeState`). When the user expands a parent, LipiTable fires `OnTreeNodeExpand` with the parent's key.

### §10.10.2 — Lazy load children

Cross-reference §4.3.6. The caller's `DataSource` handles the expand request:

```csharp
private async Task<TableQueryResponse<Folder>> LoadFoldersAsync(TableQueryRequest req, CancellationToken ct)
{
    if (req.ExpandedParentKey is not null)
    {
        // Loading a specific parent's children
        var parentId = (Guid)req.ExpandedParentKey;
        var children = await _folderService.GetChildrenAsync(parentId, ct);
        return new TableQueryResponse<Folder>(
            Rows: children,
            TotalCount: children.Count,
            TreeChildren: new Dictionary<object, IReadOnlyList<Folder>>
            {
                [parentId] = children
            });
    }
    else
    {
        // Initial load — return roots
        var roots = await _folderService.GetRootsAsync(req, ct);
        return new TableQueryResponse<Folder>(roots, roots.Count);
    }
}
```

LipiTable populates the parent's children from `TreeChildren[parentId]` and renders them. Subsequent expand/collapse on already-loaded children doesn't re-fire `OnTreeNodeExpand`.

### §10.10.3 — Has-children hint

For lazy-loaded trees, LipiTable needs to know which roots are "leaves with no children" vs "parents with potential children" — without fetching children for every row.

Per-row hint:

```csharp
[Parameter] public Func<TItem, bool>? HasChildrenSelector { get; set; }
```

```razor
<LipiTable TItem="Folder"
           DataSource="@LoadFoldersAsync"
           KeySelector="@(f => f.Id)"
           ParentSelector="@(f => f.ParentId)"
           HasChildrenSelector="@(f => f.HasChildren)">
    <!-- ... -->
</LipiTable>
```

When `HasChildrenSelector` is set, LipiTable:
- Renders a chevron only if `HasChildrenSelector(row) == true`
- On first expand, fires `OnTreeNodeExpand` to fetch the children
- After fetch, the chevron reflects actual children count

When NOT set, LipiTable assumes every row might have children (chevron rendered on every row). Clicking a leaf row's chevron triggers a fetch that returns empty children; the chevron disappears.

For client-side mode with full data available, `HasChildrenSelector` isn't needed — LipiTable can inspect `ChildrenSelector` / parent grouping directly.

### §10.10.4 — Pre-loaded children in initial response

The server can return some children pre-loaded in the initial response:

```csharp
return new TableQueryResponse<Folder>(
    Rows: roots,
    TotalCount: roots.Count,
    TreeChildren: new Dictionary<object, IReadOnlyList<Folder>>
    {
        [roots[0].Id] = await _folderService.GetChildrenAsync(roots[0].Id, ct)  // pre-load first root
    });
```

LipiTable treats those parents as "children loaded" — no fetch fires on first expand. Useful for "always expand the first root" UX.

---

## §10.11 — Tree events

```csharp
public sealed record TreeNodeExpandedContext<TItem>(
    TItem Row,
    int Depth,
    bool RequiresContentFetch,
    TreeNodeExpandReason Reason
);

public sealed record TreeNodeCollapsedContext<TItem>(
    TItem Row,
    int Depth,
    TreeNodeCollapseReason Reason
);

public enum TreeNodeExpandReason
{
    UserClickChevron,
    UserKeyboard,
    UserFilterAutoExpand,
    Programmatic,
    DefaultStateExpanded
}

public enum TreeNodeCollapseReason
{
    UserClickChevron,
    UserKeyboard,
    UserFilterReverted,
    Programmatic,
    DefaultStateCollapsed
}

[Parameter] public EventCallback<TreeNodeExpandedContext<TItem>> OnTreeNodeExpand { get; set; }
[Parameter] public EventCallback<TreeNodeCollapsedContext<TItem>> OnTreeNodeCollapse { get; set; }
```

Use cases:
- `OnTreeNodeExpand` with `RequiresContentFetch=true` → server-side lazy-load
- Analytics: which folders / sub-units do users explore most?
- URL parameter sync (`?expanded=folder1,folder2,folder3`)

---

## §10.12 — Tree-specific styling

CSS hooks for caller customization:

- `lipi-table-cell-tree-chevron` — the chevron icon container
- `lipi-table-cell-indent` — the indent wrapper inside the cell
- `lipi-table-row--depth-0`, `lipi-table-row--depth-1`, `lipi-table-row--depth-2`, ... (modifier class per depth)
- `lipi-table-row--has-children` — modifier for rows with children
- `lipi-table-row--leaf` — modifier for leaf rows

The depth modifier class is capped at depth 10 (`lipi-table-row--depth-10`). Beyond depth 10, no class is added — caller styles use inline padding.

---

## §10.13 — Disabling tree features

| Setting | Effect |
|---|---|
| Don't set `ChildrenSelector` or `ParentSelector` | Tree mode off; flat rendering |
| `<LipiColumn Type="ColumnType.Actions" IncludeInTreeChevron="false">` | This column doesn't host the chevron (chevron goes to next eligible column) |
| `TreeAnimateExpand="false"` | Instant expand/collapse, no animation |
| `ShowTreeIndentGuides="false"` (default) | No vertical guide lines |

---

## §10.14 — Worked examples

### §10.14.1 — Org chart with nested data (client-side)

```razor
<LipiTable TItem="OrgMember"
           Items="@_topLevel"
           KeySelector="@(m => m.Id)"
           ChildrenSelector="@(m => m.Reports)"
           DefaultTreeState="DefaultTreeState.FirstLevelExpanded"
           SelectionMode="SelectionMode.Multi"
           TreeSelectionCascade="TreeSelectionCascade.Descendants">
    <LipiColumn Field="@(m => m.Name)" Header="Member" />
    <LipiColumn Field="@(m => m.Role)" Header="Role" />
    <LipiColumn Field="@(m => m.Department)" Header="Department" />
</LipiTable>
```

Org chart with nested children. First level expanded on mount. Selecting a manager cascades to all reports.

### §10.14.2 — File explorer with flat data + lazy load

```razor
<LipiTable TItem="Folder"
           DataSource="@LoadFoldersAsync"
           KeySelector="@(f => f.Id)"
           ParentSelector="@(f => f.ParentId)"
           HasChildrenSelector="@(f => f.HasSubfolders)"
           DefaultTreeState="DefaultTreeState.Collapsed"
           OnTreeNodeExpand="@HandleExpandAsync"
           PersistTreeExpandState="true"
           ShowTreeIndentGuides="true">
    <LipiColumn Field="@(f => f.Name)" Header="Folder" />
    <LipiColumn Field="@(f => f.ItemCount)" Type="ColumnType.Number" />
    <LipiColumn Field="@(f => f.LastModified)" Type="ColumnType.DateTime" />
</LipiTable>

@code {
    private async Task HandleExpandAsync(TreeNodeExpandedContext<Folder> ctx)
    {
        if (ctx.RequiresContentFetch)
        {
            await _table!.RefreshAsync();   // re-fetches with this folder's ExpandedParentKey
        }
    }
}
```

File explorer — flat data, lazy-load children, guide lines for visual ancestry, persist expand state across sessions (file structure is stable).

### §10.14.3 — Patient family tree with filter

```razor
<LipiTable TItem="FamilyMember"
           Items="@_familyMembers"
           KeySelector="@(m => m.Id)"
           ParentSelector="@(m => m.ParentMemberId)"
           DefaultTreeState="DefaultTreeState.Expanded"
           ShowQuickFilter="true"
           TreeFilterMode="TreeFilterMode.PreserveAncestors">
    <LipiColumn Field="@(m => m.Name)" />
    <LipiColumn Field="@(m => m.Relation)" Header="Relation to patient" />
    <LipiColumn Field="@(m => m.ConditionStatus)" Type="ColumnType.Status" />
</LipiTable>
```

Family-history tree. Quick filter searches by name; matches preserve ancestors so the user sees the family-position context.

### §10.14.4 — Programmatic tree manipulation

```razor
<LipiButton OnClick="@ExpandAllManagers">Expand all managers</LipiButton>
<LipiButton OnClick="@CollapseToRoots">Collapse to roots only</LipiButton>

<LipiTable @ref="_table" TItem="OrgMember" ...>
    ...
</LipiTable>

@code {
    private Task ExpandAllManagers() => 
        _table!.ExpandToDepthAsync(1).AsTask();
    
    private Task CollapseToRoots() => 
        _table!.CollapseAllAsync().AsTask();
}
```

External buttons control tree expand state programmatically. Useful for dashboard surfaces with multiple expansion presets.

---

*End of §10. Proceed to §11 — Master-detail.*


# LipiTable Spec — §11 Master-detail

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model), §10 (tree data — shared expand chevron pattern), §12 (inline edit — interaction)
**Decisions consolidated:** Q6.1, Q6.2, Q6.3

---

## §11.1 — Overview

Master-detail expands each row to show a **detail panel** beneath it. The panel renders arbitrary Razor content provided by the caller — a sub-table, a form, a chart, a key-value summary, anything.

```
[▸]  Dr. Reddy        Consultant     Active
[▾]  Dr. Patel        Surgeon        Active
     ╔══════════════════════════════════════╗
     ║  Profile     Education     Schedule  ║   ← detail panel (caller's RenderFragment)
     ║                                       ║
     ║  License:   MD-23456                  ║
     ║  Specialty: Cardiothoracic surgery    ║
     ║  Joined:    March 2019                ║
     ║                                       ║
     ╚══════════════════════════════════════╝
[▸]  Dr. Sharma       Resident       Active
```

Master-detail differs from **tree data** (§10):
- **Tree data** — every row in the table has the same shape; children are more rows
- **Master-detail** — the detail is a separate UI surface (template) with its own structure

The two can coexist on the same table — a tree's leaf row can have a detail panel that drops down — but the typical pattern is one or the other.

---

## §11.2 — Activation

Master-detail activates when `<DetailTemplate>` is set on the table:

```razor
<LipiTable TItem="Doctor"
           Items="@_doctors"
           KeySelector="@(d => d.Id)">
    <LipiColumn Field="@(d => d.Name)" />
    <LipiColumn Field="@(d => d.Role)" />
    
    <DetailTemplate Context="doctor">
        <div class="doctor-detail">
            <h3>@doctor.Name</h3>
            <p>License: @doctor.License</p>
            <p>Specialty: @doctor.Specialty</p>
        </div>
    </DetailTemplate>
</LipiTable>
```

When `<DetailTemplate>` is provided:
- A dedicated expand-chevron column is auto-injected at the leftmost position (after the selection column if present)
- Every row gets a chevron
- Clicking a chevron expands / collapses that row's detail panel

When `<DetailTemplate>` is NOT set, no chevron column, no detail behavior.

---

## §11.3 — Expand chevron column

Per Q6.2 (locked) — chevron lives in a **dedicated column**, not embedded in another column's content.

### §11.3.1 — Auto-injection

When `<DetailTemplate>` is set, LipiTable auto-injects:
- Column key: `__expand__` (internal, not in column-picker, not customizable order)
- Position: leftmost data column position (after selection column if present)
- Width: `--lipi-table-col-expand-w` per density (28px / 36px / 44px)
- Content: chevron icon (▾ when expanded, ▸ when collapsed)
- Header: empty
- Sortable / Filterable / Groupable / Editable / Pinnable: all false (system column)

### §11.3.2 — Chevron interaction

Same interaction model as tree expand (§10.4):

| Trigger | Effect |
|---|---|
| Click chevron | Toggle that row's detail |
| Enter / Space (chevron focused) | Toggle |
| ArrowRight on collapsed row | Expand |
| ArrowLeft on expanded row | Collapse |
| Double-click row body | Toggle (when no `OnRowDoubleClick` is set) |

The chevron animates a 90° rotation when toggling (200ms, respects reduced motion).

### §11.3.3 — Per-row chevron disable

By default, every row gets a chevron. To hide the chevron on specific rows (e.g., rows that have no detail to show):

```csharp
[Parameter] public Func<TItem, bool>? HasDetailSelector { get; set; }
```

```razor
<LipiTable TItem="Doctor"
           HasDetailSelector="@(d => d.HasProfileData)"
           ...>
    <DetailTemplate Context="doctor">
        ...
    </DetailTemplate>
</LipiTable>
```

When `HasDetailSelector` returns false for a row, the chevron is hidden (the cell is empty) and the row cannot be expanded.

When `HasDetailSelector` is not set, every row gets a chevron.

### §11.3.4 — Chevron column with tree data

When BOTH master-detail (`<DetailTemplate>`) AND tree data (`ChildrenSelector` / `ParentSelector`) are configured:
- The first column hosts the tree chevron (▾ / ▸ for tree children)
- A second dedicated column hosts the detail chevron (▾ / ▸ for detail panel)

This is rare and visually busy. Most consuming pages use one or the other. When both are needed, LipiTable supports it but the consuming page should consider whether the data could be re-shaped into one model.

---

## §11.4 — Detail panel rendering

### §11.4.1 — Detail row

When a row is expanded, a **detail row** renders directly beneath the data row. The detail row spans **all data columns** (full table width minus selection and chevron columns).

```
┌──┬──┬─────────┬──────────┬──────────┐
│ ☐│▾ │ Dr. Patel│ Surgeon │ Active   │   ← data row
├──┴──┴──────────┴──────────┴──────────┤
│                                       │   ← detail row (full-width)
│   <DetailTemplate content here>      │
│                                       │
└───────────────────────────────────────┘
```

CSS:
- Detail row: `lipi-table-row-detail` (§2.2.7), `lipi-table-row-detail-content`
- Background: `--lipi-table-bg-detail` (subtle tint to distinguish from data rows)
- Border-top / border-bottom: 1px solid `--lipi-table-border-strong` (visual separation)
- Padding: 16px / 20px / 24px per density

### §11.4.2 — Detail content

The `<DetailTemplate>` content is wrapped in the detail row container. The caller's content has full layout freedom within that container — multi-column grids, tabs, forms, nested tables, etc.

The container is not virtualized — even when the body uses virtualization (§20), an expanded detail row stays in the DOM as long as it's expanded.

### §11.4.3 — Height behavior

Detail row height = whatever the content renders to (auto height). LipiTable does NOT impose a max height by default.

For detail panels with potentially very tall content (long lists, large forms), the consuming page should either:
- Use a scrollable container inside the template (`max-height: 400px; overflow: auto`)
- Use `<LipiTabs>` (Phase 2.6.1) to organize content into compact tabs
- Use `<LipiDrawer>` (Phase 2.6.2) for the detail content instead of master-detail (better for very large detail surfaces)

### §11.4.4 — Detail-only context

The `Context` of `<DetailTemplate>` is the row's `TItem`:

```razor
<DetailTemplate Context="row">
    <div>@row.Name</div>
</DetailTemplate>
```

If the caller needs additional context (the row's depth in a tree, its index, its selection state, etc.), the table-level `RowContext` helper exposes them:

```razor
<DetailTemplate Context="row">
    @{
        var ctx = _table!.GetRowContext(row);
    }
    <div>
        Row index: @ctx.Index   |   Selected: @ctx.IsSelected
    </div>
</DetailTemplate>
```

`GetRowContext(TItem)` returns a `RowContext<TItem>` exposing index, depth, selection state, parent (for tree rows), etc. Rarely used; most details just need the row itself.

---

## §11.5 — Multi-expand vs accordion mode

Per Q6.3 (locked = C) — caller picks via parameter, default multi-expand.

```csharp
[Parameter] public bool MultiExpand { get; set; } = true;
```

| Mode | Behavior |
|---|---|
| `MultiExpand="true"` (default) | Any number of rows can be expanded simultaneously |
| `MultiExpand="false"` (accordion) | At most ONE row can be expanded; expanding row B auto-collapses row A |

Multi-expand is the default because it's the more flexible option — the user can compare details across multiple rows side-by-side.

Accordion mode is right when:
- Detail content is large (a single expanded detail fills the screen anyway)
- The page wants to enforce "focus on one row at a time" UX (e.g., a master record selector)
- Virtualization concerns (each expanded detail adds DOM weight)

### §11.5.1 — Accordion mode interaction

When `MultiExpand="false"` and row A is expanded:
- User clicks chevron on row B → row A collapses; row B expands
- User clicks chevron on row A again → row A collapses; no row is expanded

No confirmation prompt fires when auto-collapsing row A — the data isn't "dirty" (master-detail expand is read-only navigation, not edit state).

Exception: if row A's detail panel contains an inline-edited form that's dirty (the caller's `<DetailTemplate>` includes an inline form with unsaved changes), the caller can intercept via `OnBeforeRowCollapse`:

```csharp
[Parameter] public Func<TItem, Task<bool>>? OnBeforeRowCollapse { get; set; }
```

Returning false cancels the collapse. The caller's handler is responsible for the discard-confirm UX.

---

## §11.6 — Default expand state

```csharp
public enum DefaultDetailState
{
    Collapsed,    // default — all details start collapsed
    Expanded      // all details start expanded
}
```

Default `Collapsed`. Expanding all rows on mount is rare (defeats the purpose of master-detail — the user wants to drill in on demand).

### §11.6.1 — Programmatic initial expand

For "expand the first row" or "expand by default if condition":

```csharp
[Parameter] public Func<TItem, bool>? DefaultExpandedSelector { get; set; }
```

When set, returns true for rows that should be expanded on mount:

```razor
<LipiTable TItem="Order"
           DefaultExpandedSelector="@(o => o.HasErrors)"
           ...>
    <DetailTemplate Context="order">
        @if (order.HasErrors)
        {
            <LipiAlert Variant="danger">@order.ErrorSummary</LipiAlert>
        }
    </DetailTemplate>
</LipiTable>
```

Rows with errors auto-expand on mount, drawing user attention.

### §11.6.2 — Persistence

Per the established pattern (§9.5.1, §10.4.3) — detail expand state does NOT persist across sessions by default. Each session starts fresh.

Opt-in:

```csharp
[Parameter] public bool PersistDetailExpandState { get; set; } = false;
```

When `true`, expanded row keys persist to user prefs. Stale keys (no longer in data) silently dropped on read.

---

## §11.7 — Keyboard and accessibility

### §11.7.1 — Keyboard navigation

Cross-reference §10.4.1 for the shared expand/collapse keyboard model. Plus:

| Key | Effect |
|---|---|
| Tab from data row | Move focus into the expanded detail content (when expanded) |
| Tab through detail content | Move through interactive elements inside the template |
| Tab past last detail element | Move focus to the next data row (skip remaining cells of the master row) |
| Shift+Tab | Reverse |

When a detail is expanded, the keyboard treats the detail content as if it were inline with the master row — the user can Tab seamlessly through the master row's cells and into the detail.

### §11.7.2 — ARIA roles

- Detail row: `role="region"`, `aria-label="Details for [row identifier]"`
- The aria-label uses the row's display value (via `RowDisplayNameSelector` if set, else `KeySelector` value)
- Chevron button: `aria-expanded="true|false"`, `aria-controls="<detail-row-id>"`
- Detail row has `id="<table-id>-detail-<row-key>"` for the aria-controls relationship

Screen readers announce "Expanded, details for [row identifier]" when the user expands.

### §11.7.3 — Focus management

When a row is expanded via keyboard:
- Focus stays on the chevron (or moves to the data row containing the chevron)
- User Tabs forward to enter the detail content

When a row is collapsed (by chevron click or Escape on a focused detail element):
- Focus returns to the row's chevron

When a row is collapsed via accordion mode (auto-collapse because another row expanded):
- Focus moves to the newly-expanded row's chevron
- Avoids losing focus when the previously-focused detail row is removed from the DOM

---

## §11.8 — Master-detail and selection

### §11.8.1 — Selection independence

A row's selection state is independent of its expand state. Selecting a row does NOT expand it. Expanding a row does NOT select it.

This matches §5 (selection is per-row key, independent of UI state).

### §11.8.2 — Selection checkbox in detail content

A common pattern: the detail panel contains a nested table or list with its own selection. The two selection sets are independent:

- LipiTable's selection (the master rows)
- The caller's nested-table selection (within the detail)

The caller maintains their own state for the nested-table selection; LipiTable doesn't reach into the template.

### §11.8.3 — Bulk action bar and master-detail

When the bulk action bar (§5.6.4) is visible (selection > 0), expanded detail rows continue to render normally. The bulk bar applies to the master rows' selection — detail content is unaffected.

---

## §11.9 — Master-detail and inline edit

### §11.9.1 — Master row in edit

When a master row enters inline edit mode (§12), its detail panel:
- Stays in whatever expand state it was in (no auto-collapse)
- Detail content remains rendered (user can see the detail while editing the master row)
- Detail content can still be interactive

This lets users edit a row while keeping its detail visible for reference. Useful for "edit while comparing to extra context."

### §11.9.2 — Detail content has its own edit form

If the `<DetailTemplate>` contains its own form (e.g., a tabbed editor for the detail's nested data), that form is **independent** of LipiTable's inline edit:

- LipiTable's inline edit fires for the master row's cells
- The detail's form fires for the detail content
- Both can be active simultaneously (master edit + detail form open)
- Save / cancel of each is independent

The dirty-state warning (§12.8.4) only applies to LipiTable's own inline edit, not to forms inside the detail template. If the detail's form is dirty and the user collapses the row, the form's content is NOT automatically saved or warned-about. The caller's detail form is responsible for its own dirty-state UX.

Pattern recommendation: pages with detail-form-edit should use `OnBeforeRowCollapse` to prompt the user before collapsing a dirty detail.

---

## §11.10 — Pagination and virtualization

### §11.10.1 — Detail row in pagination

The detail row counts as part of the master row visually but NOT as a separate row in pagination math. A page of 25 rows means 25 master rows; expanded detail panels render in addition to those 25.

If the user expands 10 detail panels and each is 300px tall, the page becomes effectively very tall — but the row count display still reads "Showing 1-25 of 487."

### §11.10.2 — Detail row in virtualization

Cross-reference §20. Virtualization handles variable-height rows including expanded details:

- Collapsed master rows: `ItemSize` = density default (32 / 40 / 52px)
- Expanded master row + detail: actual rendered height measured at runtime
- Virtualization recomputes scroll regions when expand state changes
- Off-screen detail content is unmounted (DOM-removed) when scrolled away to save memory
- Re-mounting when scrolled back uses the same `TItem` instance, so the detail content is rebuilt by the template

Caller's `<DetailTemplate>` must be **idempotent** — rendering it again from the same `TItem` should produce the same UI without side effects (no auto-fetches, no state-modifying logic). Side effects belong in `OnRowExpand` callback.

### §11.10.3 — InfiniteScroll and master-detail

Infinite scroll (§8.5.2) works with master-detail. As the user scrolls past loaded rows, the next page is fetched and rendered. Expanded details from earlier pages stay visible (their state persists in memory).

If the user has expanded 50 rows across many pages, the DOM gets heavy. Recommendation: use `MultiExpand="false"` (accordion) for InfiniteScroll-heavy tables to bound the DOM.

---

## §11.11 — Programmatic API

```csharp
public ValueTask ExpandDetailAsync(TItem row);
public ValueTask CollapseDetailAsync(TItem row);
public ValueTask ToggleDetailAsync(TItem row);
public ValueTask ExpandAllDetailsAsync();
public ValueTask CollapseAllDetailsAsync();

public bool IsDetailExpanded(TItem row);
public IReadOnlyList<TItem> ExpandedDetailRows { get; }
```

Use case — link with deep-linked detail expansion:

```razor
@code {
    [Parameter] public Guid? OpenDetailId { get; set; }   // from URL query param
    
    protected override async Task OnParametersSetAsync()
    {
        if (OpenDetailId.HasValue && _table is not null)
        {
            var row = _doctors.FirstOrDefault(d => d.Id == OpenDetailId.Value);
            if (row is not null)
            {
                await _table.ExpandDetailAsync(row);
            }
        }
    }
}
```

URL `?openDetail=<guid>` auto-expands the matching row on page load.

---

## §11.12 — Events

```csharp
public sealed record RowDetailExpandedContext<TItem>(
    TItem Row,
    DetailExpandReason Reason
);

public sealed record RowDetailCollapsedContext<TItem>(
    TItem Row,
    DetailCollapseReason Reason
);

public enum DetailExpandReason
{
    UserClickChevron,
    UserKeyboard,
    UserDoubleClickRow,
    Programmatic,
    DefaultExpanded,
    PersistenceRestore
}

public enum DetailCollapseReason
{
    UserClickChevron,
    UserKeyboard,
    AccordionAutoCollapse,
    Programmatic,
    DefaultCollapsed,
    DataRemoved        // row was removed from data; its detail collapsed automatically
}

[Parameter] public EventCallback<RowDetailExpandedContext<TItem>> OnRowExpand { get; set; }
[Parameter] public EventCallback<RowDetailCollapsedContext<TItem>> OnRowCollapse { get; set; }
[Parameter] public Func<TItem, Task<bool>>? OnBeforeRowCollapse { get; set; }
```

Use cases:
- `OnRowExpand` → lazy-load the detail's data (avoid pre-fetching every row's detail data)
- `OnRowExpand` → analytics ("user explored row X's details")
- `OnBeforeRowCollapse` → discard-confirm for dirty detail forms

### §11.12.1 — Lazy-load detail data pattern

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           OnRowExpand="@HandleExpandAsync">
    <LipiColumn Field="@(p => p.Name)" />
    
    <DetailTemplate Context="patient">
        @if (_detailData.TryGetValue(patient.Id, out var detail))
        {
            <PatientDetailView Data="@detail" />
        }
        else
        {
            <LipiSpinner />   <!-- loading state -->
        }
    </DetailTemplate>
</LipiTable>

@code {
    private Dictionary<Guid, PatientDetail> _detailData = new();
    
    private async Task HandleExpandAsync(RowDetailExpandedContext<Patient> ctx)
    {
        if (!_detailData.ContainsKey(ctx.Row.Id))
        {
            var detail = await _patientService.GetDetailAsync(ctx.Row.Id);
            _detailData[ctx.Row.Id] = detail;
            StateHasChanged();
        }
    }
}
```

The master list loads efficiently (no detail data per row). Detail data fetches only when expanded. Cached in memory for re-expansions.

---

## §11.13 — Detail-related parameters summary

| Parameter | Default | Purpose |
|---|---|---|
| `<DetailTemplate>` | — | Caller's detail content (RenderFragment) |
| `MultiExpand` | `true` | Allow multiple expanded at once |
| `HasDetailSelector` | null | Per-row chevron visibility |
| `DefaultExpandedSelector` | null | Initial expand condition |
| `PersistDetailExpandState` | `false` | Persist across sessions |
| `OnRowExpand` | — | Expand event |
| `OnRowCollapse` | — | Collapse event |
| `OnBeforeRowCollapse` | — | Pre-collapse interceptor (returns bool) |
| `RowDisplayNameSelector` | null | Used for aria-label on detail row |

---

## §11.14 — Worked examples

### §11.14.1 — Simple per-row detail panel

```razor
<LipiTable TItem="Order"
           Items="@_orders"
           KeySelector="@(o => o.Id)">
    <LipiColumn Field="@(o => o.Number)" Header="Order #" />
    <LipiColumn Field="@(o => o.Customer)" />
    <LipiColumn Field="@(o => o.Total)" Type="ColumnType.Currency" />
    
    <DetailTemplate Context="order">
        <div class="order-detail">
            <h4>Line items</h4>
            <ul>
                @foreach (var line in order.Lines)
                {
                    <li>@line.Description — @line.Quantity × @line.UnitPrice.ToString("C")</li>
                }
            </ul>
            <p><strong>Notes:</strong> @order.Notes</p>
        </div>
    </DetailTemplate>
</LipiTable>
```

Each order row expands to show its line items and notes.

### §11.14.2 — Accordion mode for large detail content

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)"
           MultiExpand="false"
           HasDetailSelector="@(r => r.HasFullReport)">
    <LipiColumn Field="@(r => r.TestName)" />
    <LipiColumn Field="@(r => r.ReportedAt)" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(r => r.Status)" Type="ColumnType.Status" />
    
    <DetailTemplate Context="result">
        <LipiTabs>
            <LipiTab Title="Report">
                <pre>@result.FullReportText</pre>
            </LipiTab>
            <LipiTab Title="Values">
                <LipiTable TItem="LabValue" Items="@result.Values" KeySelector="@(v => v.Id)">
                    <LipiColumn Field="@(v => v.Name)" />
                    <LipiColumn Field="@(v => v.Value)" />
                    <LipiColumn Field="@(v => v.Reference)" />
                </LipiTable>
            </LipiTab>
            <LipiTab Title="Comments">
                @result.Comments
            </LipiTab>
        </LipiTabs>
    </DetailTemplate>
</LipiTable>
```

Lab result expanded shows a tabbed detail panel including a nested LipiTable. Accordion mode keeps DOM bounded. `HasDetailSelector` hides chevrons on results that don't have a full report.

### §11.14.3 — Lazy-load detail data

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           OnRowExpand="@LoadDetailAsync">
    <LipiColumn Field="@(p => p.Uhid)" />
    <LipiColumn Field="@(p => p.Name)" />
    
    <DetailTemplate Context="p">
        @if (_details.TryGetValue(p.Id, out var d))
        {
            <PatientSummaryPanel Data="@d" />
        }
        else
        {
            <LipiSpinner /> <span>Loading details...</span>
        }
    </DetailTemplate>
</LipiTable>

@code {
    private Dictionary<Guid, PatientSummary> _details = new();
    
    private async Task LoadDetailAsync(RowDetailExpandedContext<Patient> ctx)
    {
        if (!_details.ContainsKey(ctx.Row.Id))
        {
            _details[ctx.Row.Id] = await _patientService.GetSummaryAsync(ctx.Row.Id);
            StateHasChanged();
        }
    }
}
```

Patient list loads master rows efficiently; detail data fetched on-demand and cached.

### §11.14.4 — Master-detail with dirty-form discard

```razor
<LipiTable TItem="Doctor"
           Items="@_doctors"
           KeySelector="@(d => d.Id)"
           OnBeforeRowCollapse="@HandleBeforeCollapseAsync">
    <LipiColumn Field="@(d => d.Name)" />
    
    <DetailTemplate Context="doctor">
        <DoctorProfileForm Doctor="@doctor" @ref="@_form" />
    </DetailTemplate>
</LipiTable>

@code {
    private DoctorProfileForm? _form;
    
    private async Task<bool> HandleBeforeCollapseAsync(Doctor doctor)
    {
        if (_form?.IsDirty == true)
        {
            var confirm = await ModalService.ShowConfirmAsync(
                "Discard unsaved profile changes?",
                "You have unsaved changes to this doctor's profile.",
                confirmText: "Discard",
                confirmVariant: ButtonVariant.Danger);
            return confirm;  // false to keep expanded; true to discard and collapse
        }
        return true;
    }
}
```

When the detail panel has a dirty form, collapsing the row prompts the user to confirm discard.

---

*End of §11. Proceed to §18 — Empty, loading, error states.*


# LipiTable Spec — §12 Inline editing

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model + edit template + RequireConfirmEdit), §4 (data sources), §5 (selection interaction), §11 (master-detail interaction)
**Decisions consolidated:** Q3.1.a through Q3.1.h, Q3.2, Q3.4, Q3.5, Q3.6

---

## §12.1 — Edit modes

### §12.1.1 — `TableEditMode` enum

Per Q3.1.a (locked) — three modes:

```csharp
public enum TableEditMode
{
    None,    // default — table is read-only
    Row,     // pencil icon → entire row enters edit state at once
    Cell     // double-click / F2 → individual cell enters edit state
}
```

Default `None` is a deliberate clinical-safety stance per Principle 3 (§1.3). Read-only tables don't accidentally become editable.

### §12.1.2 — When to use Row vs Cell mode

The two modes serve different workflows:

| Mode | Strengths | Trade-offs | Typical use |
|---|---|---|---|
| `Row` | Each change is explicit; row-level validation runs atomically; row-level audit; safer for clinical data | Slower for single-field corrections; requires Save / Cancel ceremony | Patient demographics edit, order parameter update, structured-record editing |
| `Cell` | Excel-style speed for bulk corrections; tab-through workflow; minimal ceremony | Easier to misclick into editing; per-cell saves can transiently violate row-level rules; audit per cell | Lab result entry, price/inventory updates, batch typo correction |

Row mode is the safer default for structured clinical data. Cell mode is the productivity default for bulk-correction workflows.

### §12.1.3 — Per-column editability

Per the column model (§3.2), columns opt in to editability:

```razor
<LipiColumn Field="@(p => p.Name)" Editable="true" />
<LipiColumn Field="@(p => p.UpdatedAt)" Editable="false" />   <!-- read-only column -->
<LipiColumn Field="@(p => p.Notes)" Editable="true" EditTemplate="@..." />
```

When `EditMode != None` AND a column has `Editable="true"`:
- Row mode: the cell renders as an input when the row enters edit mode
- Cell mode: the cell is interactive (double-clickable, F2-activatable, focus-on-Tab)

When `Editable="false"` (default), the cell is read-only regardless of the table-level mode. Useful for system fields (timestamps, computed values, audit columns).

### §12.1.4 — Selecting the edit input

Per §3.4.3 — the default edit input per column type is determined by the column's `Type`:

| ColumnType | Default edit input |
|---|---|
| Text | `LipiTextBox` |
| Number | `LipiNumberInput` |
| Currency | `LipiNumberInput` with currency formatting |
| Date | `LipiDatePicker` |
| DateTime | `LipiDateTimePicker` |
| Time | `LipiTimePicker` |
| Boolean | `LipiCheckbox` (or `LipiSwitch` if `BooleanEditAs="Switch"`) |
| Mono | `LipiTextBox` with mono font |
| Status | `LipiSelect` (requires `StatusOptions`) |
| Avatar / Link / File / Actions | Not editable unless `<EditTemplate>` provided |
| Custom | Requires `<EditTemplate>` |

All default inputs are `LipiInputBase`-derived and participate in the touched-state / EditContext validation flow (§12.6). Custom `<EditTemplate>` is the caller's call — if they use a non-LipiInputBase component, per-field validation flow may not work as documented gotcha (§3.4.3).

### §12.1.5 — Editable but disabled-per-row

Independent of column-level editability, a specific row may be edit-disabled:

```csharp
[Parameter] public Func<TItem, bool>? RowEditable { get; set; }
```

Example: only rows in `Draft` status are editable; everything else is locked:

```razor
<LipiTable TItem="Order"
           EditMode="TableEditMode.Row"
           RowEditable="@(o => o.Status == OrderStatus.Draft)">
    ...
</LipiTable>
```

When `RowEditable` returns false for a row, the pencil icon (Row mode) is disabled OR the cell is non-editable (Cell mode). Tooltip on the disabled affordance can be customized via `RowEditDisabledTooltip`.

---

## §12.2 — Row-edit flow

### §12.2.1 — Entering edit mode

In `TableEditMode.Row`, a pencil icon appears in the row's actions area (auto-injected if no actions column exists per §3.3.2). Clicking the pencil:

1. The clicked row enters edit state (`lipi-table-row--editing` modifier applied)
2. Every editable cell in that row transitions from display to input mode
3. Save / Cancel buttons appear (placement per §12.2.5)
4. The first editable input receives keyboard focus
5. `OnRowEditStart` event fires with the row's `TItem`

Other rows remain in display mode. Only one row can be in edit mode at a time per §12.8 (interaction during edit).

### §12.2.2 — Input rendering

When a row is in edit mode, each editable column's cell renders:

- The cell's default edit input (per §12.1.4) OR the column's `<EditTemplate>`
- Two-way bound to the row's `TItem` via the column's `Field` expression
- Touched-state and EditContext participate per LipiInputBase contract
- Input height fits within the row's height (compact: 28px input in 32px row; comfortable: 32px input in 40px row; spacious: 40px input in 52px row)

Non-editable cells in the row remain in display mode — readable, but not interactive. Their visual treatment is unchanged.

### §12.2.3 — The dirty state

Once any field in the row is modified, the row is marked **dirty**:
- `lipi-table-row--dirty` modifier applied
- A subtle indicator (modified-dot, 6px filled circle) renders next to the row's pencil/save area
- `OnDirtyStateChanged` event fires (`isDirty: true`)

Dirty state matters for:
- Confirm-on-cancel: clicking Cancel on a dirty row asks "Discard unsaved changes?"
- Confirm-on-discard-action: tab/page/sort/filter change asks the same (§12.8)
- Conflict detection: only saves when actually dirty (no-op saves don't fire concurrency check)

Computing dirty: LipiTable maintains a snapshot of the row's `TItem` at edit-start (deep copy via `ICloneable` if implemented, else field-by-field reflection of `Field` selectors). Current values are compared against snapshot on every input change.

### §12.2.4 — Saving

The user clicks Save (or hits Enter inside an input that isn't multiline). LipiTable:

1. Runs per-field validation (already validated as user types per §12.6.1)
2. Runs per-row validation via `EditValidator` if provided
3. If invalid → renders errors (§12.6.3), keeps row in edit mode, focuses first error
4. If valid → runs `OnBeforeRowSave` callback if provided
5. If `OnBeforeRowSave` returns false → cancel save, stay in edit mode
6. Runs critical-field confirmation if any modified field has `RequireConfirmEdit="true"` (§12.5)
7. Calls `OnRowSave` callback (async)
8. If `OnRowSave` succeeds → exit edit mode, apply changes to UI, fire `OnRowEditEnd`
9. If `OnRowSave` returns a concurrency error → render conflict UX (§12.11)
10. If `OnRowSave` throws → render generic save error, keep row in edit mode

Step 6's critical-field confirm fires BEFORE step 7's save callback per the locked decision (Q3.1.b). Caller's confirm dialog (built-in) plus caller's `OnBeforeRowSave` (custom logic) compose: built-in confirm first, then callback. Either can cancel.

### §12.2.5 — Save / Cancel placement

Per Q3.1.d (locked) — hybrid: inline at row right by default, sticky bottom bar opt-in.

```csharp
public enum EditButtonPlacement
{
    InlineRowRight,     // default — buttons at row's right edge
    StickyBottomBar     // floating bar at table bottom
}
```

**InlineRowRight (default):**
- Save and Cancel buttons render in the actions area of the edited row
- Save: primary (`lipi-table-row-edit-save`)
- Cancel: secondary (`lipi-table-row-edit-cancel`)
- Button sizes scale with density (compact: 24px height; comfortable: 28px; spacious: 32px)
- Buttons displace any non-edit action icons during edit; on save/cancel, action icons return

If the row has too few columns or the actions area is too narrow to fit both buttons, the buttons stack vertically OR overflow into the next visual zone. Edge case; rare in practice.

**StickyBottomBar:**
- Save and Cancel buttons render in a sticky bar at the bottom of the table (region ⑨ per §2.1)
- Always visible regardless of horizontal scroll
- Larger buttons (40px height regardless of density)
- Status label: "Editing Row #3 — Priya Sharma" (with row number + caller-supplied label via `EditRowLabel="@(r => r.Name)"`)
- More room for advanced controls (Save / Save & Add Another / Cancel / Reset)

Use `StickyBottomBar` when:
- Tables are wide with many columns (right-edge buttons would scroll off)
- Forms involve many edits before save (user wants persistent visible Save button)
- Advanced controls (Save & Add Another, Reset to original) are needed

### §12.2.6 — Cancelling

Clicking Cancel:
- If row is dirty: show "Discard unsaved changes?" confirm
- If user confirms (or row is clean): revert all field values to the snapshot, exit edit mode, fire `OnRowEditEnd` with reason `UserCancelled`
- If user cancels the confirm: row stays in edit mode

Escape key on any input has the same effect as clicking Cancel (with discard confirm if dirty).

### §12.2.7 — Auto-injection of edit icon

When `EditMode="TableEditMode.Row"` and no explicit Actions column is declared, LipiTable auto-injects an Actions column on the right with the pencil icon and (during edit) Save / Cancel buttons.

If an Actions column IS declared, the edit icon merges into it as the first icon (left-most in the actions strip). The caller's existing action icons appear to the right of it.

Auto-injection skipped when:
- An explicit `<LipiColumn Type="ColumnType.Actions">` is declared and `AutoInjectEditAction="false"` is set on the table
- `EditMode="None"`

---

## §12.3 — Cell-edit flow

### §12.3.1 — Entering cell edit

In `TableEditMode.Cell`, a single cell enters edit mode by any of:

| Trigger | Effect |
|---|---|
| Double-click on cell | Cell enters edit mode; input gets focus; cursor positioned at end |
| F2 with cell focused (keyboard) | Same |
| Enter with cell focused (keyboard) | Same |
| Direct typing with cell focused (non-modifier keystroke) | Cell enters edit mode with first keystroke replacing value (Q3.1.c.ii — Excel-style implicit edit) |
| Programmatic `tableRef.StartCellEditAsync(row, columnKey)` | Same |

Only **one cell** is in edit mode at a time (single-cell-edit-at-a-time model). Triggering edit on a second cell auto-saves the first.

### §12.3.2 — Cell input rendering

The cell's display content is replaced with the column's edit input (default per type, or `<EditTemplate>` override). The input fits within the cell's bounds (no row-height jump).

Visual treatment:
- Cell gets `lipi-table-cell--editing` modifier
- Subtle yellow tint background (`--lipi-table-bg-row-editing` at lighter alpha)
- Input element occupies the cell's content area
- Other cells in the row remain in display mode

### §12.3.3 — Saving on cell-edit

Per Q3.1.c.i (locked) — Enter saves and stays on the same cell (web-form style, clinically safer). Tab moves to next cell. Other save triggers below.

| Trigger | Effect |
|---|---|
| Enter | Save current cell, return to nav mode on same cell |
| Tab | Save current cell, move to next editable cell (wraps row per Q3.1.c.iii) |
| Shift+Tab | Save current cell, move to previous editable cell |
| Ctrl+Enter | Save current cell, move down one row (same column) — Excel-style column-major data entry per Q3.1.c.i ripple |
| F2 | Toggle out of edit mode (save) |
| Focus loss (click outside the cell) | Save current cell |
| Programmatic `tableRef.SaveCellEditAsync()` | Save |

Save flow (single cell):
1. Per-field validation runs (already validated as user types per §12.6.1)
2. If invalid → keep cell in edit mode, error highlight on cell, focus stays
3. If valid → run `OnBeforeCellEdit` callback if provided
4. If `OnBeforeCellEdit` returns false → cancel save, stay in edit mode
5. Run critical-field confirmation if column has `RequireConfirmEdit="true"`
6. Call `OnCellSave` callback (async)
7. On success → exit cell edit, fire `OnCellEditEnd`
8. On concurrency conflict → render conflict UX (§12.11)
9. On error → render generic error, keep cell in edit mode

Cell-edit fires the row's concurrency check just like row-edit (§12.10). The row's `RowVersion` is checked before applying the cell's value.

### §12.3.4 — Cancelling cell-edit

| Trigger | Effect |
|---|---|
| Escape | Cancel edit, revert value, return to nav mode on same cell |
| Programmatic `tableRef.CancelCellEditAsync()` | Same |

Cell-edit doesn't have a dirty-state confirm — the granularity is the cell, so cancel is per-cell and cheap.

### §12.3.5 — Direct typing replaces value (implicit edit)

Per Q3.1.c.ii (locked) — Excel-style. With a cell focused (not in edit mode), pressing a printable key:

1. The cell enters edit mode
2. The current value is replaced (not appended) by the keystroke
3. Cursor positioned after the new character
4. Subsequent keystrokes append normally

This makes bulk-correction workflows fast: focus a cell, type the new value, Tab to next cell, type, Tab, etc. The user never has to explicitly trigger edit mode.

Keys that DO NOT trigger replace-and-edit:
- Modifier keys (Shift, Ctrl, Alt, Cmd alone)
- Navigation keys (Arrow, Home, End, Page Up/Down)
- Function keys (F1-F12 except F2)
- Tab, Enter, Escape (have their own meanings)
- Space (treated as text only if `SpaceTriggersEdit="true"` per column — default false to allow Space-to-select-row at table level)

To start editing without replacing, the user uses F2 or double-click or Enter.

---

## §12.4 — Cell-edit keyboard map (consolidated)

Per Q3.1.c locked decisions. Cross-referenced from §19.3 (full keyboard map) — restated here for inline-edit context:

| Key | In nav mode (cell focused, not editing) | In edit mode (cell focused, editing) |
|---|---|---|
| Arrow keys | Move focus between cells | Move cursor inside input |
| Tab | Move focus to next cell (wraps to next row) | Save current cell, move to next editable cell (wraps row) |
| Shift+Tab | Move focus to previous cell | Save current cell, move to previous editable cell |
| Enter | Enter edit mode on focused cell | **Save current cell, stay on same cell (return to nav mode)** |
| Ctrl+Enter | Enter edit mode on focused cell | **Save current cell, move down one row (same column) — Excel-style** |
| F2 | Enter edit mode on focused cell | Toggle out of edit mode (save) |
| Escape | Close any open menu | Cancel edit, revert value |
| Direct typing | Replace cell value (implicit edit) | Type into input |
| Home | Move to first cell of row | Move cursor to start of input |
| End | Move to last cell of row | Move cursor to end of input |
| Ctrl+Home | Move to first cell of first row | (no special behavior) |
| Ctrl+End | Move to last cell of last row | (no special behavior) |
| Space | Toggle row selection (when `SelectionMode != None`) | Insert space character |
| Shift+Space | Extend selection from anchor | Insert space |
| Delete | (no-op in cell mode) | Insert delete |

The three speeds (§Q3.1.c.i discussion):
- **One-at-a-time correction**: type → Enter → stay
- **Row-major bulk entry**: type → Tab → next column (most common for clinical data)
- **Column-major bulk entry**: type → Ctrl+Enter → next row, same column (Excel-style)

---

## §12.5 — Critical-field confirmation

### §12.5.1 — Per-column declarative

Per Q3.1.b (locked) — hybrid model. Per-column declarative:

```razor
<LipiColumn Field="@(p => p.Allergies)" 
            Editable="true" 
            RequireConfirmEdit="true" />

<LipiColumn Field="@(p => p.Dose)" 
            Editable="true" 
            RequireConfirmEdit="true"
            ConfirmEditMessage="Changes to dose affect medication safety. Confirm save?" />
```

When `RequireConfirmEdit="true"` AND that column's value has changed during the edit:
- Built-in confirm dialog appears before save commits
- Default message: "This field requires confirmation. Save change?"
- Custom message via `ConfirmEditMessage`
- Dialog uses `LipiModal` from Phase 2.6.2

If MULTIPLE columns in a single row-edit have `RequireConfirmEdit="true"` AND their values changed, the dialog enumerates them:

```
┌─ Confirm changes ────────────────────────┐
│ You changed these fields:                 │
│   • Allergies                             │
│   • Dose                                  │
│                                           │
│ Each requires confirmation before saving. │
│                                           │
│         [Confirm save]   [Review]         │
└───────────────────────────────────────────┘
```

"Review" cancels the save and returns to the edit form. "Confirm save" proceeds to step 7 (the actual save callback).

### §12.5.2 — Per-row callback (custom logic)

```csharp
[Parameter] public Func<RowEditContext<TItem>, Task<bool>>? OnBeforeRowSave { get; set; }
```

```csharp
public sealed class RowEditContext<TItem>
{
    public TItem Original { get; }     // snapshot at edit-start
    public TItem Current { get; }      // current edit state
    public IReadOnlyList<string> ChangedFieldKeys { get; }
    public bool IsAddNew { get; }      // true if this is an add-new-row, not an edit
}
```

The callback runs **after** the built-in critical-field confirm (if any) AND before the `OnRowSave` save action. Caller returns:
- `true` to proceed with save
- `false` to cancel save (stay in edit mode)

Used for conditional confirms, server-side pre-validation, async dependency checks, etc.

Example:

```csharp
private async Task<bool> HandleBeforeSave(RowEditContext<Patient> ctx)
{
    // If dose changed by more than 50%, ask explicitly
    if (ctx.ChangedFieldKeys.Contains(nameof(Patient.Dose)))
    {
        var ratio = ctx.Current.Dose / Math.Max(ctx.Original.Dose, 0.01);
        if (ratio > 1.5 || ratio < 0.5)
        {
            var ok = await ModalService.ShowConfirmAsync(
                $"Dose changed from {ctx.Original.Dose} to {ctx.Current.Dose} ({ratio:P0}). Confirm?");
            if (!ok) return false;
        }
    }
    return true;
}
```

### §12.5.3 — Ordering: declarative then callback

When both `RequireConfirmEdit` AND `OnBeforeRowSave` are configured:

1. Built-in declarative confirm fires first (covers the changed-RequireConfirmEdit-fields)
2. If user confirms (or no declarative confirm was needed) → `OnBeforeRowSave` callback fires
3. If callback returns true → proceed to actual save
4. If callback returns false → cancel

Both can cancel independently. Either's cancel keeps the row in edit mode.

### §12.5.4 — Per-cell critical-field confirm (Cell mode)

In Cell mode, the same column-level `RequireConfirmEdit="true"` triggers a confirm on cell save:

```
┌──────────────────────────────────────────┐
│ Changing Allergies                        │
│ from "None known"                         │
│   to "Penicillin"                         │
│                                           │
│         [Confirm save]    [Cancel]        │
└──────────────────────────────────────────┘
```

The dialog shows the before/after values so the user sees what they're confirming.

Cell-mode equivalent of `OnBeforeRowSave` is `OnBeforeCellEdit`:

```csharp
[Parameter] public Func<CellEditContext<TItem>, Task<bool>>? OnBeforeCellEdit { get; set; }
```

---

## §12.6 — Validation

Per Q3.2 (locked) — hybrid: per-field as user types (after first blur) AND per-row on Save.

### §12.6.1 — Per-field validation

Per-field validation comes from `LipiInputBase` (the base class for all LiPi input components). Behavior:

- The user types in an input
- First blur (focus leaves the input) → input is "touched"
- Subsequent input changes run validation rules attached to the input (DataAnnotations, FluentValidation, or custom)
- Errors render below the input (helper text replaced by error text, red border, ⚠ icon on label)

This is the standard LipiInputBase touched-state pattern. LipiTable inherits it automatically because the cell's edit input IS a `LipiInputBase` (or a caller's `<EditTemplate>` that uses one).

If the caller's `<EditTemplate>` uses a non-LipiInputBase component, the per-field validation flow doesn't run automatically — caller is responsible. Documented gotcha (§3.4.3).

### §12.6.2 — Per-row validation

Per-row validation via the table-level `EditValidator`:

```csharp
[Parameter] public Func<TItem, ValidationResult>? EditValidator { get; set; }
```

```csharp
public sealed class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }
}

public sealed record ValidationError(string Message, string? FieldKey, ValidationSeverity Severity);

public enum ValidationSeverity { Error, Warning }
```

The validator runs when Save is clicked (Row mode) OR on cell save attempt (Cell mode). Returns errors that span multiple fields or aren't field-specific.

Example:

```csharp
private ValidationResult ValidatePatientEdit(Patient p)
{
    var errors = new List<ValidationError>();
    
    if (p.DateOfBirth >= p.DateOfDeath)
        errors.Add(new ValidationError(
            "DOB cannot be on or after death date",
            nameof(Patient.DateOfBirth),
            ValidationSeverity.Error));
    
    if (p.Allergies?.Count == 0 && p.OrderedMedications?.Any() == true)
        errors.Add(new ValidationError(
            "Allergies should be confirmed before medications are ordered",
            null,
            ValidationSeverity.Warning));
    
    return new ValidationResult(errors.Count == 0, errors);
}
```

`ValidationSeverity.Error` blocks save. `ValidationSeverity.Warning` shows the warning but allows save (user can review).

### §12.6.3 — Error rendering

Per Q3.1.h (locked) — per-field via LipiInputBase pattern; per-row red strip above the row + ⚠ icon in actions column.

**Per-field rendering** (driven by LipiInputBase):
- Red border on the input
- Error text below the input (replaces helper text)
- ⚠ icon on the input's label

**Per-row rendering:**
- Red strip rendered above the row containing the error messages:

```
┌─────────────────────────────────────────────────────────┐
│ ⚠ DOB cannot be on or after death date                  │
│ ⚠ Mobile is required                                    │
├─────────────────────────────────────────────────────────┤
│ | 4 | [Priya Sharma_]  | [22/07/85] | [_____] | [⚠][💾][✕] │
└─────────────────────────────────────────────────────────┘
```

Strip styling (cross-reference §24.3.9):
- Background: `--lipi-table-edit-error-strip-bg` (light danger tint)
- Text: `--lipi-table-edit-error-strip-text` (danger-700)
- Each error on its own line with ⚠ prefix
- The row beneath gets the same red `border-left` as the conflict banner

**Actions area:**
- ⚠ icon prepended to the action buttons (Save / Cancel still render)
- Tooltip on ⚠ shows count: "2 errors prevent save"

### §12.6.4 — Composition with LipiValidationSummary

Phase 2.7's `LipiValidationSummary` can compose externally above the table:

```razor
<LipiValidationSummary @ref="_summary" />

<LipiTable TItem="..." 
           EditMode="TableEditMode.Row"
           ValidationSummary="@_summary"
           ...>
```

When `ValidationSummary` is bound, row-level validation errors are also pushed to the external summary component. The user sees errors both inline (above the row) and in the summary (above the table). Click-to-field navigation in the summary scrolls to the edited row + focuses the offending input.

This is opt-in — most tables use only the inline strip.

### §12.6.5 — Warning-level validation flow

When validation returns `ValidationSeverity.Warning`:
- Save is NOT blocked
- Warning strip renders above the row (yellow tint instead of red)
- On Save click, a "Save with warnings?" confirm dialog appears listing the warnings
- User can dismiss the dialog and proceed, or cancel and resolve the warnings first

Useful for soft validation rules where the action is permitted but advised against.

---

## §12.7 — Cell-edit keyboard model (already covered)

See §12.4. Restated together with §12.3 for completeness.

---

## §12.8 — Interaction during edit

Per Q3.1.e locked decisions.

### §12.8.1 — Selection during edit (Q3.1.e.i = C)

- Other rows' selection works normally (checkbox click, shift-click, etc.)
- The edited row's checkbox is **disabled** — the user can't change its selection while editing
- The selected-count display includes the edited row if it was selected before edit started
- The "select all on page" header checkbox skips the edited row (selecting all unedited rows; the edited row's state is preserved)

Rationale: the edited row's data is in flux; allowing selection toggling on a row mid-save creates ambiguous selection semantics. Other rows are stable and selectable.

### §12.8.2 — Editing a second row while first is dirty (Q3.1.e.ii = B)

If row A is in edit mode and dirty, and the user attempts to enter edit on row B (clicks pencil on row B, or programmatic edit-start on row B):

1. A confirm dialog appears: "Row A has unsaved changes. Discard them?"
2. If user confirms → row A reverts to snapshot and exits edit; row B enters edit
3. If user cancels → row B's edit attempt is rejected; row A stays in edit
4. If row A is in edit but NOT dirty → row A exits edit silently (no confirm) and row B enters edit

Single-row-edit-at-a-time is enforced. This applies to both Row mode (only one row in edit) and Cell mode (only one cell across all rows in edit).

### §12.8.3 — Cell edit on a second row while first cell is dirty

In Cell mode: starting edit on a second cell auto-saves the first cell (per §12.3.3 save triggers). The "dirty row + edit another row" prompt does NOT apply at the cell level. Each cell save is independent.

### §12.8.4 — Tab / page / sort / filter change while dirty (Q3.1.e.iii = A)

If a row is dirty and the user changes:
- Page (clicks pagination, infinite-scroll, jump-to-page)
- Sort (clicks a column header)
- Filter (applies / removes a filter, including quick search)
- Group (changes grouping)
- Page size

A "Discard unsaved changes?" confirm fires:

```
┌─ Discard unsaved changes? ───────────────┐
│ You have unsaved changes to row #4.       │
│                                           │
│ Continue without saving?                  │
│                                           │
│         [Discard]      [Stay]             │
└───────────────────────────────────────────┘
```

- **Discard:** revert row to snapshot, apply the requested change (sort / filter / page)
- **Stay:** abort the requested change (table state unchanged)

This is the same UX pattern as LipiDynamicTabs dirty-tab-close (Phase 2.6.2). LipiTable reuses the same `LipiModal` template for visual consistency.

### §12.8.5 — Component disposal / navigation

If the user navigates away from the page (or otherwise causes LipiTable to unmount) while a row is dirty:
- Browser `beforeunload` event fires standard browser confirm if `EnableBrowserUnloadGuard="true"` (default false to avoid duplicate-confirm UX)
- The consuming page's own navigation guards (Blazor route guards, NavigationManager.LocationChanging) should hook in
- LipiTable doesn't enforce — it's the page's responsibility

For SPA navigation within Blazor, the consuming page can subscribe to `NavigationManager.LocationChanging` and intercept based on `_table.HasDirtyRow`.

### §12.8.6 — Auto-refresh interaction

If `AutoRefreshInterval` is set (§4.7.4) and a row is in edit mode:
- Auto-refresh **pauses** until the edit completes (saved or cancelled)
- No banner or notification — just silently paused
- Resumes immediately on edit end

Rationale: auto-refresh during edit would either discard the user's changes (replacing data mid-edit) or create complex conflict-detection scenarios. Pausing is the safe default.

---

## §12.9 — Save flow (optimistic vs pessimistic)

Per Q3.4 (locked) — caller picks via parameter, default pessimistic.

### §12.9.1 — `OptimisticUpdate` enum

```csharp
public enum OptimisticUpdate
{
    Pessimistic,    // default — wait for server confirmation before reflecting change
    Optimistic      // reflect change immediately; revert on failure
}
```

```csharp
[Parameter] public OptimisticUpdate OptimisticUpdate { get; set; } = OptimisticUpdate.Pessimistic;
```

### §12.9.2 — Pessimistic flow (default)

1. User clicks Save
2. Validation passes; confirms pass
3. Row enters "saving" state (`lipi-table-row--saving` modifier)
4. Spinner appears in the row's actions area (Save button replaced by spinner)
5. All inputs in the row become read-only (`fieldset disabled`)
6. `OnRowSave` callback runs (server roundtrip)
7. On success → row exits edit mode, data refreshes, fire `OnRowEditEnd(success)`
8. On error → row exits saving state, returns to dirty edit mode, error displayed
9. On concurrency conflict → render conflict banner (§12.11)

User experience: clear "saving" feedback; no apparent UI change until server confirms. Safe for clinical data — the user knows the save isn't done until they see it confirmed.

### §12.9.3 — Optimistic flow

1. User clicks Save
2. Validation passes; confirms pass
3. Row exits edit mode immediately (no spinner, no delay)
4. Updated data renders in display mode
5. `OnRowSave` callback runs in background (server roundtrip)
6. On success → no UI change (the optimistic state was correct)
7. On error → revert row to original snapshot, return to edit mode with error displayed, fire `OnRowSaveFailed`
8. On concurrency conflict → revert row, return to edit mode, render conflict banner (§12.11)

User experience: snappy; feels like changes are saved instantly. Higher risk: the user may move on and not notice a delayed error.

### §12.9.4 — `OnRowSave` contract

```csharp
[Parameter] public Func<RowEditContext<TItem>, Task<SaveResult>>? OnRowSave { get; set; }

public sealed class SaveResult
{
    public SaveOutcome Outcome { get; init; }       // Success | ValidationError | ConcurrencyConflict | Error
    public string? Message { get; init; }            // user-facing error message
    public IReadOnlyList<ValidationError>? Errors { get; init; }
    public ConflictInfo? Conflict { get; init; }
}

public enum SaveOutcome { Success, ValidationError, ConcurrencyConflict, Error }
```

The caller's save handler returns a `SaveResult` indicating outcome. Each outcome triggers different LipiTable behavior:

| Outcome | LipiTable behavior |
|---|---|
| `Success` | Exit edit mode; update display |
| `ValidationError` | Stay in edit mode; render errors per §12.6.3 |
| `ConcurrencyConflict` | Stay in edit mode; render conflict UX per §12.11 |
| `Error` | Stay in edit mode; render generic error toast (LipiToast) with `Message` |

If the caller's save handler throws an exception instead of returning a result, LipiTable treats it as `Outcome.Error` with `Message` derived from the exception. Caller errors should ideally return `SaveResult` explicitly so error messages are user-friendly.

### §12.9.5 — Cell save callback

```csharp
[Parameter] public Func<CellEditContext<TItem>, Task<SaveResult>>? OnCellSave { get; set; }

public sealed class CellEditContext<TItem>
{
    public TItem Item { get; }
    public string ColumnKey { get; }
    public object? OriginalValue { get; }
    public object? NewValue { get; }
}
```

Cell save is per-cell. `SaveResult.Outcome` semantics identical to row save.

---

## §12.10 — Optimistic concurrency

Per Q3.5 (locked) — row-version pattern is non-negotiable for clinical data. LipiTable supports it natively.

### §12.10.1 — `RowVersionSelector` parameter

```csharp
[Parameter] public Func<TItem, object>? RowVersionSelector { get; set; }
```

Example:

```razor
<LipiTable TItem="Patient"
           ...
           RowVersionSelector="@(p => p.RowVersion)">
```

When set:
- The row's version is captured at edit-start
- The version is included in the save request (caller's `OnRowSave` passes it to the server)
- Server compares against current DB version; if mismatch → return `SaveOutcome.ConcurrencyConflict`

LipiTable doesn't transmit the version automatically — the caller's save handler does. But LipiTable surfaces the version on the `RowEditContext` so the caller can access it:

```csharp
public sealed class RowEditContext<TItem>
{
    public TItem Original { get; }
    public TItem Current { get; }
    public object? OriginalRowVersion { get; }     // captured at edit-start
    public IReadOnlyList<string> ChangedFieldKeys { get; }
    public bool IsAddNew { get; }
}
```

### §12.10.2 — Conflict detection

The server is the source of truth for conflicts. It compares the row version the client provides against its current DB row version:
- Match → save succeeds; DB version increments; server returns the new version
- Mismatch → another user updated the row; server returns `ConcurrencyConflict` with the current DB row

The caller's `OnRowSave` returns the conflict via:

```csharp
return new SaveResult
{
    Outcome = SaveOutcome.ConcurrencyConflict,
    Conflict = new ConflictInfo
    {
        ServerRow = currentDbRow,        // the row as it exists on the server now
        ServerRowVersion = currentVersion,
        ConflictingUserDisplay = "Dr. Patel",   // optional — who made the conflicting change
        ConflictedAt = DateTime.UtcNow.AddSeconds(-120)
    }
};
```

LipiTable then renders the conflict UX per §12.11.

### §12.10.3 — What gets versioned

The `RowVersionSelector` returns whatever object represents the row's version. Common implementations:

- `Func<TItem, byte[]>` — SQL Server `rowversion` (timestamp) column → byte array
- `Func<TItem, int>` — incrementing integer version column
- `Func<TItem, Guid>` — Guid stored as ETag
- `Func<TItem, string>` — string ETag from a REST API

LipiTable treats the version as opaque `object`. The caller is responsible for round-tripping it correctly through the save callback.

### §12.10.4 — When `RowVersionSelector` is not set

If the caller doesn't provide `RowVersionSelector`:
- No concurrency check happens (last-write-wins semantics)
- The conflict UX in §12.11 doesn't trigger
- `RowEditContext.OriginalRowVersion` is null

For non-clinical / single-user contexts, this is fine. For multi-user clinical data, set the version selector.

---

## §12.11 — Concurrency conflict UX

Per Q3.1.f.i (locked) — Banner default + on-demand diff popover; Modal mode opt-in; Custom RenderFragment opt-in.

### §12.11.1 — `ConflictResolutionMode` enum

```csharp
public enum ConflictResolutionMode
{
    Banner,    // default — inline banner above the conflicted row + diff popover on demand
    Modal,     // modal dialog with diff inline; demands attention before any other action
    Custom     // caller provides ConflictTemplate RenderFragment
}
```

### §12.11.2 — Banner mode (default)

When `OnRowSave` returns `ConcurrencyConflict`:

1. The row stays in edit mode (user's changes preserved)
2. A red banner renders above the row:

```
┌─────────────────────────────────────────────────────────────────┐
│ ⚠ This row was modified by Dr. Patel 2 minutes ago while you   │
│   were editing.                                                 │
│   [View changes]  [Reload (lose mine)]  [Save anyway]           │
└─────────────────────────────────────────────────────────────────┘
```

3. The conflicted row gets `lipi-table-row--conflict` modifier (red left border, subtle red tint)
4. The user picks a resolution:
   - **View changes** → opens a popover showing the full-row diff (§12.11.4)
   - **Reload** → discards user's changes, fetches server's current version of the row, exits edit mode
   - **Save anyway** → re-attempts save with `forceSave=true` flag (caller's save handler must honor this)

Banner copy is templated:

```csharp
[Parameter] public Func<ConflictInfo, string>? ConflictBannerText { get; set; }
```

Default produces:
- If `ConflictingUserDisplay` is provided: "This row was modified by {user} {duration} ago while you were editing."
- If not: "This row was modified by another user while you were editing."

### §12.11.3 — Modal mode

When `ConflictResolutionMode="Modal"`:

The row's edit stays paused; a modal dialog opens immediately on conflict:

```
┌─ Conflict detected ──────────────────────┐
│ ⚠ Conflict detected                       │
│                                           │
│ Dr. Patel modified Priya Sharma 2 minutes │
│ ago while you were editing.               │
│                                           │
│ ┌─────────────────────────────────────┐  │
│ │ Field    | Yours | Theirs            │  │
│ │ Weight   | 68.5  | 69.0   (differs) │  │
│ └─────────────────────────────────────┘  │
│                                           │
│  [Cancel edit]  [Reload (lose mine)]      │
│                 [Save mine anyway]        │
└───────────────────────────────────────────┘
```

The modal blocks all other interaction until the user resolves. Diff inside the modal shows **only differing fields** (per Q3.1.f.ii) — kept compact for the decision moment.

### §12.11.4 — Diff popover (Banner mode "View changes")

Per Q3.1.f.ii (locked) — Banner-mode popover shows **all fields with differences highlighted**.

```
┌─ Comparing changes — Priya Sharma (PT-4830) ─────┐
│ Field      | Original     | Yours        | Theirs    │
│────────────|──────────────|──────────────|───────────│
│ Name       | Priya Sharma | Priya Sharma | Priya     │
│            |              |              | Sharma    │
│ Weight     | 70.0         | 68.5         | 69.0      │  ← highlighted (differs)
│ DOB        | 22/07/1985   | 22/07/1985   | 22/07/85  │
│                                                       │
│         [Close]  [Reload (lose mine)]  [Save mine]   │
└───────────────────────────────────────────────────────┘
```

Three columns: Original (snapshot at edit-start), Yours (current edit), Theirs (server-current). Rows that differ between Yours and Theirs are highlighted (`--lipi-table-conflict-diff-changed` background).

The popover is rendered by `LipiTableConflictDiffPopover.razor` (§27 file list). It's a popover, not a modal — appears anchored near the banner, dismissible by clicking outside.

### §12.11.5 — Custom mode

When `ConflictResolutionMode="Custom"`:

```razor
<LipiTable ... ConflictResolutionMode="ConflictResolutionMode.Custom">
    <ConflictTemplate Context="ctx">
        <div class="my-custom-conflict-ui">
            <h3>Concurrency conflict on @ctx.RowDisplayName</h3>
            <!-- Caller-owned conflict resolution UI -->
            <LipiButton OnClick="@(() => ctx.ReloadAsync())">Reload</LipiButton>
            <LipiButton OnClick="@(() => ctx.ForceSaveAsync())">Force save</LipiButton>
            <LipiButton OnClick="@(() => ctx.CancelAsync())">Cancel edit</LipiButton>
        </div>
    </ConflictTemplate>
</LipiTable>
```

`ConflictContext<TItem>` provides:

```csharp
public sealed class ConflictContext<TItem>
{
    public TItem OriginalRow { get; }
    public TItem CurrentEditRow { get; }
    public TItem ServerRow { get; }
    public string? ConflictingUserDisplay { get; }
    public DateTime? ConflictedAt { get; }
    public string RowDisplayName { get; }    // caller-configurable via RowDisplayNameSelector
    
    public Task ReloadAsync();
    public Task ForceSaveAsync();
    public Task CancelAsync();
}
```

Useful for: domain-specific conflict UI, deep diff visualization, ML-based merge suggestions, escalation workflows.

### §12.11.6 — Force-save flag

When the user picks "Save anyway" (banner) or "Save mine anyway" (modal), LipiTable re-invokes `OnRowSave` with a flag:

```csharp
public sealed class RowEditContext<TItem>
{
    public TItem Original { get; }
    public TItem Current { get; }
    public object? OriginalRowVersion { get; }
    public IReadOnlyList<string> ChangedFieldKeys { get; }
    public bool IsAddNew { get; }
    public bool IsForceSave { get; }      // ← true on retry after conflict
}
```

The caller's save handler is responsible for honoring this:

```csharp
private async Task<SaveResult> HandleSave(RowEditContext<Patient> ctx)
{
    if (ctx.IsForceSave)
    {
        // skip version check; force-overwrite
        await _service.UpdatePatientForceAsync(ctx.Current);
    }
    else
    {
        try
        {
            await _service.UpdatePatientAsync(ctx.Current, ctx.OriginalRowVersion);
        }
        catch (ConcurrencyException ex)
        {
            return new SaveResult { Outcome = SaveOutcome.ConcurrencyConflict, Conflict = ... };
        }
    }
    return new SaveResult { Outcome = SaveOutcome.Success };
}
```

---

## §12.12 — Add-new-row pattern

Per Q3.1.g (locked) — hybrid: toolbar Add button AND programmatic API.

### §12.12.1 — Toolbar Add button

```razor
<LipiTable TItem="Patient"
           ShowAddButton="true"
           OnAddNew="@HandleAddNew">
    <LipiColumn Field="@(p => p.Name)" Editable="true" />
    <LipiColumn Field="@(p => p.Mobile)" Editable="true" />
    <AddRowTemplate Context="newItem">
        <!-- Inputs for new row; @bind to newItem -->
    </AddRowTemplate>
</LipiTable>
```

`ShowAddButton="true"` renders a "+ Add" button in the toolbar's right zone (§16). Clicking it:

1. Invokes `OnAddNew` callback if provided — caller returns an initial `TItem` (or default if no callback)
2. Inserts a new pseudo-row at the top of the body with `lipi-table-row--new` modifier
3. The pseudo-row enters edit mode immediately
4. First editable field receives focus
5. Save / Cancel buttons appear (inline or sticky bar per `EditButtonPlacement`)

On Save:
- `OnRowSave` fires with `RowEditContext.IsAddNew = true`
- On success → pseudo-row becomes a real row (data refreshes; if server returns the row's new key, the row settles into the data flow)
- On failure → pseudo-row stays in edit mode, errors displayed

On Cancel:
- "Discard new row?" confirm fires (since the new row is always considered dirty)
- On confirm → pseudo-row removed; toolbar Add button re-enables

### §12.12.2 — `<AddRowTemplate>` slot

The `<AddRowTemplate>` slot is OPTIONAL. When provided, it renders inside the new pseudo-row instead of the standard edit inputs from each column.

Use case: the add-new flow needs different fields than an edit flow. E.g., a "quick add" with only name + mobile required, while edit shows all fields.

When `<AddRowTemplate>` is NOT provided, LipiTable uses each column's standard edit input (the same UI as Row mode edit).

### §12.12.3 — Programmatic add-new

```csharp
public ValueTask<RowEditContext<TItem>> AddNewRowAsync(TItem? initialValue = null);
```

Caller can invoke directly from any UI:

```razor
<LipiButton OnClick="@(() => _table!.AddNewRowAsync())">Quick add</LipiButton>
```

Equivalent to clicking the toolbar Add button (without firing the `OnAddNew` callback's initial-value provider).

### §12.12.4 — Save & Add Another flow

When `EditButtonPlacement="StickyBottomBar"` and `ShowAddAnotherButton="true"`:

```
═══════════════════════════════════════════════════════
  Adding new patient — [Save & add another]  [Save]  [Cancel]
═══════════════════════════════════════════════════════
```

After Save & Add Another:
- The current new row is saved
- A second new row is inserted (with fresh defaults)
- First editable field receives focus
- The flow loops until the user clicks plain Save or Cancel

Useful for high-volume entry workflows (registering 30 new patients at registration desk).

### §12.12.5 — Auto-position of new row

The new pseudo-row appears at the **top** of the current page by default. Caller can override:

```csharp
[Parameter] public NewRowPlacement NewRowPlacement { get; set; } = NewRowPlacement.Top;

public enum NewRowPlacement { Top, Bottom, AfterFocused }
```

- `Top`: always at the top (default; matches Gmail, Notion, Linear "new" pattern)
- `Bottom`: always at the bottom (Excel pattern)
- `AfterFocused`: inserts after the currently focused row, if any (Notebook-style notes pattern)

---

## §12.13 — Edit state events

```csharp
public sealed record RowEditStartContext<TItem>(TItem Row, bool IsAddNew);
public sealed record RowEditEndContext<TItem>(TItem Row, RowEditEndReason Reason, bool WasAddNew);
public sealed record CellEditStartContext<TItem>(TItem Row, string ColumnKey);
public sealed record CellEditEndContext<TItem>(TItem Row, string ColumnKey, CellEditEndReason Reason);
public sealed record DirtyStateChangedContext<TItem>(TItem Row, bool IsDirty, IReadOnlyList<string> ChangedFieldKeys);

public enum RowEditEndReason
{
    UserSaved,
    UserCancelled,
    UserDiscarded,           // discarded via "Discard unsaved changes?" prompt
    Programmatic,
    SaveFailed,              // edit ended due to save failure with no retry
    ConflictResolved         // conflict resolved by user picking Reload
}

public enum CellEditEndReason
{
    UserSaved,
    UserCancelled,
    UserMovedToNextCell,
    UserFocusLoss,
    Programmatic
}

[Parameter] public EventCallback<RowEditStartContext<TItem>> OnRowEditStart { get; set; }
[Parameter] public EventCallback<RowEditEndContext<TItem>> OnRowEditEnd { get; set; }
[Parameter] public EventCallback<CellEditStartContext<TItem>> OnCellEditStart { get; set; }
[Parameter] public EventCallback<CellEditEndContext<TItem>> OnCellEditEnd { get; set; }
[Parameter] public EventCallback<DirtyStateChangedContext<TItem>> OnDirtyStateChanged { get; set; }
```

Use cases:
- `OnRowEditStart` → capture entry timestamp for audit, lock the row in a service-side mutex
- `OnRowEditEnd` → release lock, log outcome
- `OnDirtyStateChanged` → enable/disable a "Save changes" button in a parent toolbar that's not LipiTable's

---

## §12.14 — Programmatic edit API

```csharp
// Row edit
public ValueTask<RowEditContext<TItem>> StartRowEditAsync(TItem row);
public ValueTask<SaveResult> SaveRowEditAsync();
public ValueTask CancelRowEditAsync(bool skipDirtyConfirm = false);

// Cell edit
public ValueTask StartCellEditAsync(TItem row, string columnKey);
public ValueTask<SaveResult> SaveCellEditAsync();
public ValueTask CancelCellEditAsync();

// Add new
public ValueTask<RowEditContext<TItem>> AddNewRowAsync(TItem? initialValue = null);

// State queries
public bool HasDirtyRow { get; }
public TItem? CurrentEditingRow { get; }
public string? CurrentEditingColumnKey { get; }
public IReadOnlyList<string> ChangedFieldKeysOfDirtyRow { get; }
```

---

## §12.15 — Disabling edit features

| Setting | Effect |
|---|---|
| `EditMode="None"` (default) | Read-only table; no edit UI rendered |
| `<LipiColumn Editable="false">` | Column not editable; other columns may still be |
| `RowEditable="@(_ => false)"` | All rows non-editable at runtime (effectively read-only) |
| `OnRowSave` not provided | Save button still renders; clicking shows developer-mode warning. Production: no-op. |
| `ShowAddButton="false"` (default) | No toolbar Add button; programmatic add still works unless `AllowAddNew="false"` |

---

## §12.16 — Worked examples

### §12.16.1 — Simple row-edit with concurrency

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           EditMode="TableEditMode.Row"
           RowVersionSelector="@(p => p.RowVersion)"
           OnRowSave="@HandleSavePatientAsync"
           TableId="patients-list">
    <LipiColumn Field="@(p => p.Name)"   Editable="true" />
    <LipiColumn Field="@(p => p.Mobile)" Editable="true" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" Editable="true" StatusOptions="@_statusOptions" />
    <LipiColumn Field="@(p => p.UpdatedAt)" Type="ColumnType.DateTime" Editable="false" />
</LipiTable>

@code {
    private async Task<SaveResult> HandleSavePatientAsync(RowEditContext<Patient> ctx)
    {
        try
        {
            await _patientService.UpdateAsync(ctx.Current, (byte[])ctx.OriginalRowVersion!);
            return new SaveResult { Outcome = SaveOutcome.Success };
        }
        catch (DbUpdateConcurrencyException)
        {
            var current = await _patientService.GetByIdAsync(ctx.Current.Id);
            return new SaveResult
            {
                Outcome = SaveOutcome.ConcurrencyConflict,
                Conflict = new ConflictInfo { ServerRow = current, ... }
            };
        }
    }
}
```

### §12.16.2 — Row-edit with critical-field confirm

```razor
<LipiTable TItem="Order"
           EditMode="TableEditMode.Row"
           OnRowSave="@HandleSaveOrderAsync"
           OnBeforeRowSave="@HandleBeforeSaveAsync">
    <LipiColumn Field="@(o => o.DrugName)" Editable="true" />
    <LipiColumn Field="@(o => o.Dose)"
                Type="ColumnType.Number"
                Editable="true"
                RequireConfirmEdit="true"
                ConfirmEditMessage="Confirm dose change?" />
    <LipiColumn Field="@(o => o.Frequency)" Editable="true" />
    <LipiColumn Field="@(o => o.Notes)" Editable="true" />
</LipiTable>
```

User changes the dose → on Save, declarative confirm fires ("Confirm dose change?"). User confirms → `OnBeforeRowSave` runs (might check % change in dose). Then actual save fires.

### §12.16.3 — Cell-edit for bulk lab result entry

```razor
<LipiTable TItem="LabResult"
           Items="@_results"
           KeySelector="@(r => r.Id)"
           EditMode="TableEditMode.Cell"
           OnCellSave="@HandleSaveCellAsync"
           RowVersionSelector="@(r => r.RowVersion)">
    <LipiColumn Field="@(r => r.TestName)" Editable="false" />
    <LipiColumn Field="@(r => r.ReferenceRange)" Editable="false" />
    <LipiColumn Field="@(r => r.Value)" Type="ColumnType.Number" Editable="true" />
    <LipiColumn Field="@(r => r.IsAbnormal)" Type="ColumnType.Boolean" Editable="true" />
</LipiTable>
```

Lab tech enters values: type → Tab → next column → type → Tab → next column. After last editable column, Tab wraps to next row. Excel-style flow.

### §12.16.4 — Add-new with custom template

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           EditMode="TableEditMode.Row"
           ShowAddButton="true"
           ShowAddAnotherButton="true"
           EditButtonPlacement="EditButtonPlacement.StickyBottomBar"
           OnAddNew="@CreateBlankPatient"
           OnRowSave="@HandleSavePatientAsync">
    <LipiColumn Field="@(p => p.Name)" Editable="true" />
    <LipiColumn Field="@(p => p.DateOfBirth)" Type="ColumnType.Date" Editable="true" />
    <LipiColumn Field="@(p => p.Mobile)" Editable="true" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" Editable="false" />
    
    <AddRowTemplate Context="newPatient">
        <LipiTextBox @bind-Value="newPatient.Name" Placeholder="Full name" Required="true" />
        <LipiDatePicker @bind-Value="newPatient.DateOfBirth" Required="true" />
        <LipiTextBox @bind-Value="newPatient.Mobile" Placeholder="Mobile" Required="true" />
        <!-- Status not editable in add flow; defaults to "Draft" set by CreateBlankPatient -->
    </AddRowTemplate>
</LipiTable>

@code {
    private Patient CreateBlankPatient() => new Patient { Status = "Draft" };
}
```

Add button → sticky bottom bar → "Save & add another" loops for high-volume entry. Each new patient defaults to "Draft" status; the add template excludes the status column entirely.

---

*End of §12. Proceed to §13 — Column resize, reorder, pin.*


# LipiTable Spec — §13 Column resize, reorder, pin

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model — Resizable, Reorderable, Pinned, Width parameters), §21 (persistence)

---

## §13.1 — Overview

Three orthogonal column-manipulation features:
- **Resize** — drag right-edge of a column header to change its width
- **Reorder** — drag a column header to a new horizontal position
- **Pin** — anchor a column to the left or right edge so it doesn't horizontally scroll

All three are interactive (mouse + keyboard + programmatic). All three persist per-user when `TableId` is set. All three can be disabled per-column or per-table.

The three features compose: a column can be pinned-left, resized, and reordered within the pinned-left group all at once. Constraints are enforced (e.g., a pinned-right column can't be reordered into the unpinned middle without first being unpinned).

---

## §13.2 — Column resize

### §13.2.1 — Resize handle

Each resizable column header has a 4px-wide vertical handle along its right edge:

```
┌─────────────┬─────────────┬─────────────┐
│ Name        │ Role        │ Status      │
│             ▍│             ▍│             │   ← 4px-wide handle on right edge
└─────────────┴─────────────┴─────────────┘
                  (faint when idle; primary color on hover)
```

CSS class: `lipi-table-header-resize` (§2.2.6). Token: `--lipi-table-resize-handle-w` (4px from §24.2.8).

States:
- Idle: hidden (no visible color)
- Hover: 4px solid `--color-primary-300`, cursor:col-resize
- Dragging: 4px solid `--color-primary-500`, cursor:col-resize, full-height drag preview line extends to bottom of table

### §13.2.2 — Drag mechanic

1. User mouse-downs on the handle
2. Cursor becomes `col-resize`
3. As user drags horizontally, the column's grid track width updates live (real-time preview)
4. Other columns adjust to maintain grid integrity (flex columns absorb the change; fixed columns stay fixed)
5. Mouse-up commits the new width
6. Persistence write fires (debounced 300ms after final commit)

The drag preview is the actual width change, not a ghost — what the user sees during drag is what they get on release. This matches AG-Grid / Telerik / Excel behavior.

### §13.2.3 — Constraints

| Constraint | Source |
|---|---|
| Minimum width | `MinWidth` parameter on `<LipiColumn>`, OR 40px hard floor (whichever is greater) |
| Maximum width | `MaxWidth` parameter on `<LipiColumn>`, OR unbounded |
| Snapping | None by default; consuming page can request snap-to-grid via `ResizeSnapPx="8"` for 8px snap |

If the user drags below min, the column snaps to min (visual feedback: brief shake of the handle). If they drag above max, the column snaps to max.

### §13.2.4 — Per-column opt-out

```razor
<LipiColumn Field="@(p => p.Id)" Resizable="false" />
```

Resizable defaults to `true`. Setting `false` hides the handle for that column. The adjacent column's handle still functions (resizing the next-rightward column's left edge effectively).

### §13.2.5 — Per-table opt-out

```razor
<LipiTable AllowColumnResize="false" ...>
```

Disables resize for all columns. Handles are hidden globally. Programmatic `tableRef.ResizeColumnAsync` still works (caller-driven width changes ignore the user-allow flag).

### §13.2.6 — Double-click auto-fit

Double-clicking the resize handle auto-fits the column to the widest visible cell content:

1. LipiTable measures each visible cell's natural content width
2. Picks the max across visible rows + header
3. Adds the cell's horizontal padding
4. Clamps to `[MinWidth, MaxWidth]`
5. Sets the column's width to that value
6. Persists the new width

Constraints:
- Auto-fit is per visible rows only — off-page rows are not measured (would require fetching all data in server-side mode)
- For virtualized tables, only currently-rendered virtual rows are measured
- The user can re-double-click after scrolling to refit based on newly visible content

If the column's `<CellTemplate>` produces content that doesn't have a measurable text width (e.g., an `<svg>`), auto-fit uses the cell's `getBoundingClientRect().width` as a fallback.

Auto-fit can be disabled per-column via `AutoFitOnDoubleClick="false"`.

### §13.2.7 — Programmatic resize

```csharp
public ValueTask ResizeColumnAsync(string columnKey, string widthValue);
public ValueTask AutoFitColumnAsync(string columnKey);
public ValueTask ResetColumnWidthsAsync();   // restore declared defaults
```

`widthValue` accepts the same CSS length values as `Width` parameter (§3.6.2): `"120px"`, `"1fr"`, `"minmax(100px, 1fr)"`, etc.

### §13.2.8 — Resize events

```csharp
public sealed record ColumnResizedContext(
    string ColumnKey,
    string OldWidth,
    string NewWidth,
    ColumnResizeReason Reason
);

public enum ColumnResizeReason
{
    UserDrag,
    UserAutoFit,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

[Parameter] public EventCallback<ColumnResizedContext> OnColumnResized { get; set; }
```

---

## §13.3 — Column reorder

### §13.3.1 — Drag mechanic

1. User mouse-downs on a column header (not on the resize handle, not on sort indicator, not on filter icon — the central header text area)
2. After 200ms hold or 8px drag movement (whichever first), drag mode engages
3. Cursor becomes `grabbing`
4. The header cell ghosts (50% opacity, semi-transparent clone follows cursor)
5. A vertical drop indicator (2px wide, primary color) renders between potential drop targets
6. As user drags horizontally, the indicator updates to show insertion point
7. Mouse-up at the drop position commits the reorder
8. Persistence write fires (debounced 300ms)

The 200ms / 8px threshold prevents accidental drags when the user is clicking the header to sort.

```
┌──────┬──────┬──────────┬──────┐
│ Name │ Role ▌ Status   │ Date │   ← drop indicator between Role and Status
└──────┴──────┴──────────┴──────┘
              ▲
       [Status]  ← ghost following cursor
```

### §13.3.2 — Drop targets

Valid drop targets:
- Between any two unpinned columns
- Before the first unpinned column
- After the last unpinned column
- Into the pinned-left zone (only if the dragged column has `Reorderable="true"` and `Pinned` is `Left` or `None` — see §13.4.4 below)
- Into the pinned-right zone (mirror)

Invalid drop targets:
- Onto the selection checkbox column (system column)
- Onto the expand chevron column (system column)
- Outside the column row entirely

If the drop position is invalid, mouse-up cancels the drag (no reorder happens) and the ghost returns to origin with a 200ms snap-back animation (respects `prefers-reduced-motion`).

### §13.3.3 — Constraints on reorder

The pinned-zone model creates three reorder zones: pinned-left, unpinned, pinned-right. Reorder is constrained:

| Source zone | Drop in pinned-left | Drop in unpinned | Drop in pinned-right |
|---|---|---|---|
| Pinned-left column | ✓ (reorder within pinned-left) | ✓ (unpins the column) | ✓ (re-pins to right) |
| Unpinned column | ✓ (pins to left) | ✓ (reorder within unpinned) | ✓ (pins to right) |
| Pinned-right column | ✓ (re-pins to left) | ✓ (unpins) | ✓ (reorder within pinned-right) |

Crossing zone boundaries during drag implicitly changes the column's pin state. This is intentional — pin and reorder are the same gesture.

If the caller wants to lock pin state and only allow reorder within the current zone, set `<LipiColumn LockPinState="true">`.

### §13.3.4 — Per-column opt-out

```razor
<LipiColumn Field="@(p => p.Name)" Reorderable="false" />
```

Non-reorderable columns:
- Cannot be dragged
- Other columns can be dragged past them (their position is fixed)
- Visually no different from reorderable columns (no special indicator)

Typical use: first column (often serves as primary key column) locked in position. System Actions columns also frequently `Reorderable="false"`.

### §13.3.5 — Per-table opt-out

```razor
<LipiTable AllowColumnReorder="false" ...>
```

Disables reorder for all columns. Header drag does nothing. Programmatic reorder still works.

### §13.3.6 — Programmatic reorder

```csharp
public ValueTask ReorderColumnAsync(string columnKey, int newIndex);
public ValueTask ReorderColumnsAsync(IReadOnlyList<string> orderedColumnKeys);
public ValueTask ResetColumnOrderAsync();   // restore declaration order
```

`newIndex` is the target position in the overall ordered column list (0-based). Pin zones are honored — moving a pinned-left column to index 5 (in the middle of unpinned columns) implicitly unpins it.

`ReorderColumnsAsync` accepts the full ordered list; LipiTable applies the change atomically rather than column-by-column.

### §13.3.7 — Reorder events

```csharp
public sealed record ColumnReorderedContext(
    string ColumnKey,
    int OldIndex,
    int NewIndex,
    ColumnPin OldPin,
    ColumnPin NewPin,
    ColumnReorderReason Reason
);

public enum ColumnReorderReason
{
    UserDrag,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

[Parameter] public EventCallback<ColumnReorderedContext> OnColumnReordered { get; set; }
```

The event includes pin state because reorder can implicitly change it.

---

## §13.4 — Column pinning

### §13.4.1 — `Pinned` parameter

Per the column model (§3.2):

```csharp
[Parameter] public ColumnPin Pinned { get; set; } = ColumnPin.None;

public enum ColumnPin { None, Left, Right }
```

Initial pin state at declaration time. User can change at runtime; persistence captures the change.

### §13.4.2 — Visual rendering of pinned columns

Pinned columns stick to the table's left or right edge during horizontal scroll:

```
┌──────────┬─────[ scroll region ]─────┬──────────┐
│  Pinned  │  Col3  │  Col4  │  Col5   │  Pinned  │
│   left   │        │        │         │   right  │
├──────────┼────────┼────────┼─────────┼──────────┤
│ row data │  ...   │  ...   │  ...    │ actions  │
│ row data │  ...   │  ...   │  ...    │ actions  │
└──────────┴────────┴────────┴─────────┴──────────┘
   ▲                                          ▲
   sticky position:left                       sticky position:right
   shadow extends rightward                   shadow extends leftward
```

CSS implementation:
- Pinned columns use `position: sticky; left: 0` (or `right: 0`)
- Z-index `--lipi-table-z-pinned` (1)
- Drop shadow `--lipi-table-pin-shadow-left` / `-right` indicates more content beyond
- Pin column shadow visible when content is scrollable beneath
- Shadow fades when scroll position is at the edge (no shadow when fully scrolled to leftmost)

The shadow is the visual cue that pinning is active — users immediately understand "this column is fixed and there's more to the side."

### §13.4.3 — Pin via header context menu

Right-click on a column header opens a context menu with pin options:

```
┌─────────────────────────┐
│ Sort by Name (asc)      │
│ Sort by Name (desc)     │
│ Clear sort              │
│ ──────────────────────  │
│ Pin to left             │
│ Pin to right            │
│ Unpin                   │
│ ──────────────────────  │
│ Hide column             │
│ Resize to fit           │
│ ──────────────────────  │
│ Reset to defaults       │
└─────────────────────────┘
```

The menu uses the existing LipiTable column-context-menu infrastructure (referenced from §16.3 column picker). Click "Pin to left" → column moves to the pinned-left zone (animated transition 200ms).

### §13.4.4 — Pin via drag (implicit)

Per §13.3.3 — dragging a column into a pin zone implicitly pins it. The user sees:
- During drag: pin zones highlight as the cursor enters them (subtle background tint)
- On drop: the column animates into its new pin state

This makes the pin/unpin gesture muscle-memory for users who already drag to reorder.

### §13.4.5 — Pin constraints

- **System columns** (selection checkbox, expand chevron) are always at the leftmost edge regardless of pin state. They're always effectively pinned to the left.
- **Actions column** is conventionally pinned-right. LipiTable does NOT auto-pin it — caller chooses. But default behavior strongly recommends `Pinned="ColumnPin.Right"` for Actions columns to keep them visible during horizontal scroll.
- **Pin state and reorder** interact per §13.3.3.
- **Multiple pinned columns** in the same zone maintain their relative order; reorder within the zone works as expected.

### §13.4.6 — Per-column opt-out

```razor
<LipiColumn Field="@(p => p.Name)" Pinnable="false" />
```

`Pinnable="true"` is default. Setting `false`:
- Disables the pin entries in the context menu for that column
- Disables drag-to-pin (column drags within its current zone only)
- The column's `Pinned` parameter still controls initial state (set programmatically if needed)

### §13.4.7 — Per-table opt-out

```razor
<LipiTable AllowColumnPin="false" ...>
```

Disables runtime pin changes for all columns. Initial `Pinned` parameters still honored; programmatic pin still works.

### §13.4.8 — Programmatic pin

```csharp
public ValueTask PinColumnAsync(string columnKey, ColumnPin pin);
public ValueTask UnpinColumnAsync(string columnKey);   // shorthand for PinColumnAsync(key, None)
public ValueTask ResetColumnPinsAsync();   // restore declared defaults
```

### §13.4.9 — Pin events

```csharp
public sealed record ColumnPinnedContext(
    string ColumnKey,
    ColumnPin OldPin,
    ColumnPin NewPin,
    ColumnPinReason Reason
);

public enum ColumnPinReason
{
    UserContextMenu,
    UserDrag,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

[Parameter] public EventCallback<ColumnPinnedContext> OnColumnPinned { get; set; }
```

---

## §13.5 — Performance considerations

### §13.5.1 — Resize performance

The resize drag updates the grid track in real-time. For large tables (100+ visible rows, 30+ columns), the grid layout recomputation happens on every mouse-move (throttled to ~16ms = 60fps).

CSS Grid's layout is hardware-accelerated for column-width changes alone (no row reflow needed when only column widths change). Tested up to 200 visible virtualized rows × 50 columns at 60fps on mid-tier hardware.

### §13.5.2 — Reorder performance

Reorder during drag updates only the drop indicator position, not the actual column arrangement. The reorder commit at mouse-up triggers one re-render of the column list. O(N) where N = column count. Trivial.

### §13.5.3 — Pin performance

Pinning a column requires re-rendering the table with the new column-arrangement (the pinned column moves to a new zone). The animation is CSS-driven (transform-based), not JS layout work. Each pin operation is one render pass.

For tables with many rows, pin/unpin during scroll could feel slow if the row count is huge. Mitigation: pin/unpin happens at mouse-up, not during drag, so the user's scroll position is preserved and rendering happens once.

---

## §13.6 — Persistence

Cross-reference §21 for full persistence model.

### §13.6.1 — What's persisted per column

When `TableId` is set:
- Column **width** (last user-set width via drag or auto-fit)
- Column **order** (position in the column list)
- Column **pin state** (Left / Right / None)
- Column **visibility** (Visible / Hidden — covered in §3.5.5)

### §13.6.2 — Persisted shape

```json
{
  "columns": {
    "Name":       { "width": "180px", "order": 0, "pin": "left",  "visible": true  },
    "Role":       { "width": "120px", "order": 1, "pin": "none",  "visible": true  },
    "Department": { "width": "140px", "order": 2, "pin": "none",  "visible": false },
    "Status":     { "width": "100px", "order": 3, "pin": "none",  "visible": true  },
    "Actions":    { "width": "auto",  "order": 4, "pin": "right", "visible": true  }
  }
}
```

### §13.6.3 — Persistence rules

- Writes are debounced 300ms (mid-resize updates collapse into one final write)
- On mount, persisted state is restored before first render (column layout settles immediately)
- If a persisted column key no longer exists, it's silently dropped
- If a new column is declared that has no persisted state, it uses its declared defaults (declared order, declared width, declared pin)
- "Reset to defaults" (from column picker or context menu) clears persisted column state and re-applies declaration

### §13.6.4 — Cross-device persistence

Per the locked decision (Q10.2), persistence is server-side in `identity.user_table_preferences`. User's column customizations follow them across devices and browsers. A user who customizes the patient-list columns on their desktop sees the same layout on their tablet.

---

## §13.7 — Worked examples

### §13.7.1 — Default pin pattern for clinical tables

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list">
    <LipiColumn Field="@(p => p.Uhid)"        Pinned="ColumnPin.Left"   Reorderable="false" Width="100px" />
    <LipiColumn Field="@(p => p.Name)"        Pinned="ColumnPin.Left"   Width="200px" />
    <LipiColumn Field="@(p => p.Age)"         Type="ColumnType.Number"  Width="80px" />
    <LipiColumn Field="@(p => p.Gender)"      Width="80px" />
    <LipiColumn Field="@(p => p.Mobile)"      Width="140px" />
    <LipiColumn Field="@(p => p.LastVisit)"   Type="ColumnType.Date"    Width="120px" />
    <LipiColumn Field="@(p => p.Status)"      Type="ColumnType.Status"  Width="100px" />
    <LipiColumn Type="ColumnType.Actions"     Pinned="ColumnPin.Right"  Reorderable="false">
        <CellTemplate Context="p">
            <LipiIconButton Icon="eye"  OnClick="@(() => Open(p))" />
            <LipiIconButton Icon="edit" OnClick="@(() => Edit(p))" />
        </CellTemplate>
    </LipiColumn>
</LipiTable>
```

UHID and Name pinned left; Actions pinned right. Demographics middle scrolls horizontally if narrow viewport. UHID is non-reorderable (always first); Actions non-reorderable (always last).

### §13.7.2 — User customizes layout programmatically

```razor
<LipiButton OnClick="@PinNameOnly">Show name-pinned layout</LipiButton>
<LipiButton OnClick="@ResetLayout">Reset layout</LipiButton>

<LipiTable @ref="_table" TItem="Patient" ...>
    ...
</LipiTable>

@code {
    private LipiTable<Patient>? _table;
    
    private async Task PinNameOnly()
    {
        await _table!.PinColumnAsync(nameof(Patient.Name), ColumnPin.Left);
        await _table.UnpinColumnAsync(nameof(Patient.Uhid));
    }
    
    private async Task ResetLayout()
    {
        await _table!.ResetColumnPinsAsync();
        await _table.ResetColumnOrderAsync();
        await _table.ResetColumnWidthsAsync();
    }
}
```

External buttons drive column customization. Useful for "view modes" — "Compact view" / "Detail view" / "Print view" each set up different layouts programmatically.

### §13.7.3 — Resize disabled, reorder allowed

```razor
<LipiTable TItem="Patient" AllowColumnResize="false" ...>
    <!-- columns can be reordered/pinned but not resized -->
</LipiTable>
```

Useful for tables where the column widths are carefully chosen and shouldn't drift, but the user can still rearrange.

---

*End of §13. Proceed to §14 — Density.*


# LipiTable Spec — §14 Density

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §24 (density tokens), §21 (persistence)

---

## §14.1 — Overview

Density controls how vertically compact the table is. The same data renders in three different heights depending on the active mode:

- **Compact** — most data per screen; small text, tight padding. Suitable for power-user data review.
- **Comfortable** — balanced default. Suitable for general use.
- **Spacious** — most breathable; larger text, generous padding. Suitable for clinical / safety-critical contexts where misreading must be minimized.

The user toggles density via a toolbar control (opt-out via `ShowDensityToggle="false"`). The choice persists per-user when `TableId` is set.

---

## §14.2 — `TableDensity` enum

```csharp
public enum TableDensity
{
    Compact,
    Comfortable,    // default
    Spacious
}
```

Set the table's initial density via parameter:

```razor
<LipiTable TItem="..." Density="TableDensity.Compact" ...>
```

Default `Comfortable`. Persistence overrides when applicable.

---

## §14.3 — Visual specifications per density

The full token list is in §24.2. The visible differences:

| Property | Compact | Comfortable | Spacious |
|---|---|---|---|
| Row height | 32px | 40px | 52px |
| Header height | 36px | 44px | 52px |
| Cell font size | 11px | 13px | 14px |
| Header font size | 12px (uniform across densities) | 12px | 12px |
| Cell padding-y | 4px | 8px | 12px |
| Cell padding-x | 8px | 12px | 16px |
| Selection column width | 36px | 44px | 52px |
| Expand chevron column width | 28px | 36px | 44px |
| Tree indent (per depth) | 16px | 24px | 32px |
| Inline edit input height | 28px | 32px | 40px |
| Selection-checkbox size (LipiCheckbox) | small | medium | medium |
| Status badge size (LipiBadge) | xs | sm | md |
| Avatar diameter | 24px | 32px | 40px |
| Pencil / actions icon button size | 24px | 28px | 32px |
| Row's `border-bottom` width | 0.5px | 1px | 1px |
| Row hover transition | 80ms | 120ms | 160ms |

The header font is intentionally constant at 12px across all densities — it's structural scaffolding, not content. Only the body scales.

### §14.3.1 — Visual reference

```
Compact (32px rows, 11px font):
┌──────────────────────────────────────┐
│ ☐ Dr. Reddy    Consultant   Active   │
│ ☐ Dr. Patel    Surgeon      Active   │
│ ☐ Dr. Sharma   Resident     Inactive │
└──────────────────────────────────────┘

Comfortable (40px rows, 13px font):
┌─────────────────────────────────────────┐
│  ☐  Dr. Reddy     Consultant    Active  │
│                                          │
│  ☐  Dr. Patel     Surgeon       Active  │
│                                          │
│  ☐  Dr. Sharma    Resident      Inactive│
└─────────────────────────────────────────┘

Spacious (52px rows, 14px font):
┌──────────────────────────────────────────────┐
│                                               │
│   ☐   Dr. Reddy      Consultant      Active   │
│                                               │
│                                               │
│   ☐   Dr. Patel      Surgeon         Active   │
│                                               │
└──────────────────────────────────────────────┘
```

---

## §14.4 — Density toggle (toolbar)

### §14.4.1 — Toggle control

When `ShowDensityToggle="true"` (default), a segmented control renders in the toolbar's right zone (region ① per §2.1):

```
        [Compact | Comfortable | Spacious]
            ▲           ▲             ▲
         active state — currently selected
```

The control is a `LipiButtonGroup` (Phase 2.1) with three options. Visual:
- Active option: primary background + on-primary text
- Inactive options: surface background + secondary text
- Icons (optional, opt-in via `DensityToggleStyle="IconWithLabel"`): rows-tight / rows-medium / rows-wide

### §14.4.2 — Compact-mode display

For tables in narrow toolbars, the toggle can collapse to an icon dropdown:

```csharp
[Parameter] public DensityToggleStyle DensityToggleStyle { get; set; } = DensityToggleStyle.Segmented;

public enum DensityToggleStyle
{
    Segmented,        // default — three-button segmented
    IconWithLabel,    // segmented with icon prefix on each label
    IconDropdown      // single icon → opens dropdown with three options
}
```

`IconDropdown` is the right choice for toolbars where horizontal space is tight (e.g., narrow sidebar tables, drawer-embedded tables).

### §14.4.3 — Opt-out

```razor
<LipiTable ShowDensityToggle="false" ...>
```

Toggle hidden from toolbar. Density still set via parameter; user can't change at runtime. Programmatic `tableRef.SetDensityAsync()` still works.

---

## §14.5 — Programmatic density

```csharp
public ValueTask SetDensityAsync(TableDensity density);
public TableDensity CurrentDensity { get; }
```

Useful for "view mode" patterns where external buttons drive density:

```razor
<LipiButtonGroup>
    <LipiButton OnClick="@(() => _table!.SetDensityAsync(TableDensity.Compact))">Compact</LipiButton>
    <LipiButton OnClick="@(() => _table!.SetDensityAsync(TableDensity.Spacious))">Detail</LipiButton>
</LipiButtonGroup>

<LipiTable @ref="_table" Density="TableDensity.Comfortable" ShowDensityToggle="false" ...>
```

The page's external buttons control density; the table's built-in toggle is hidden.

---

## §14.6 — Density persistence

When `TableId` is set, the user's density choice persists. Cross-reference §21.

### §14.6.1 — Persisted shape

```json
{
  "density": "Spacious"
}
```

### §14.6.2 — Persistence rules

- Writes immediate on density change (no debounce — density changes are infrequent and intentional)
- Restored on mount; overrides `Density` parameter
- "Reset to defaults" clears the persisted value; table falls back to declared `Density`

### §14.6.3 — Cross-table density preference

Each table has its own density preference. A user's choice of Spacious for the patient list doesn't apply to the audit log.

For applications wanting a single density preference across all tables, the consuming app can implement a global setting:

```razor
@inject IUserPreferences Prefs

<LipiTable Density="@Prefs.PreferredTableDensity"
           ShowDensityToggle="false"
           ...>
```

The app's preferences service owns the global value; LipiTable just consumes it. Some apps may surface this as a Settings page option ("Default table density: Compact / Comfortable / Spacious").

---

## §14.7 — Per-column density override

A column can override the table's density via its `Density` parameter:

```razor
<LipiColumn Field="@(p => p.Notes)" Density="TableDensity.Spacious" />
```

This is **rarely used** — density is fundamentally a table-wide property. But for cases where a single column has multi-line content (e.g., a long Notes field) that needs more vertical breathing room while the rest of the table stays compact, per-column override is the escape hatch.

### §14.7.1 — How per-column density interacts with row height

A column's density override affects:
- That column's font size
- That column's cell padding

But NOT the row height — row height is uniform across all cells in a row (governed by the table-level density). A column with `Density="Spacious"` inside a `Compact` table renders with larger font + padding in a smaller row, which usually means the content's vertical centering shifts.

For complex per-column density variation (e.g., multi-line Notes inside a Compact table), the recommended approach is **caller-side via `<CellTemplate>`** instead of per-column density override:

```razor
<LipiColumn Field="@(p => p.Notes)">
    <CellTemplate Context="p">
        <div class="multiline-notes-cell">@p.Notes</div>
    </CellTemplate>
</LipiColumn>
```

Per-column `Density` override exists but is documented as a power-user feature.

---

## §14.8 — Density and other features

### §14.8.1 — Density and density tokens

The density mode applies via a class on the table root: `lipi-table--compact`, `lipi-table--comfortable`, `lipi-table--spacious`. The class swaps the active tokens (per §24.2).

```css
.lipi-table--compact {
    --lipi-table-row-h:           var(--lipi-table-row-h-compact);
    --lipi-table-header-h:        var(--lipi-table-header-h-compact);
    --lipi-table-cell-pad-y:      var(--lipi-table-cell-pad-y-compact);
    --lipi-table-cell-pad-x:      var(--lipi-table-cell-pad-x-compact);
    --lipi-table-font:            var(--lipi-table-font-compact);
    --lipi-table-col-select-w:    var(--lipi-table-col-select-w-compact);
    --lipi-table-col-expand-w:    var(--lipi-table-col-expand-w-compact);
    --lipi-table-tree-indent:     var(--lipi-table-tree-indent-compact);
}

.lipi-table--comfortable { /* defaults */ }
.lipi-table--spacious    { /* spacious variants */ }
```

### §14.8.2 — Density and virtualization

Virtualization (§20) needs a known row height to compute scroll regions. The density mode supplies this:

- Compact: `ItemSize=32`
- Comfortable: `ItemSize=40`
- Spacious: `ItemSize=52`

When density changes, virtualization recomputes its viewport. The scroll position is preserved by-key (the focused row stays focused; surrounding rows redistribute around it).

For tables with `<DetailTemplate>` (master-detail) or variable-height rows from `<CellTemplate>` content, the density `ItemSize` is the closed-row height. Expanded detail rows have separate heights that virtualization handles via its variable-height mode.

### §14.8.3 — Density and inline edit

Edit inputs scale with density (per §14.3 table). In Compact mode, edit inputs are 28px tall; in Spacious, 40px. The input fits inside the row's natural height without forcing the row to grow.

Long text inputs (e.g., a Notes field with `LipiTextArea` as `<EditTemplate>`) will exceed the row's natural height regardless of density — the row grows to fit.

### §14.8.4 — Density and selection

Selection checkbox size and column width both scale (§14.3 table). Compact mode uses `LipiCheckbox Size="Small"`; Comfortable and Spacious use `Size="Medium"`.

### §14.8.5 — Density and status

Status badge / pill size scales (§14.3 table). Compact: xs (16px tall); Comfortable: sm (20px); Spacious: md (24px).

---

## §14.9 — Density events

```csharp
public sealed record DensityChangedContext(
    TableDensity NewDensity,
    TableDensity PreviousDensity,
    DensityChangeReason Reason
);

public enum DensityChangeReason
{
    UserToggle,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

[Parameter] public EventCallback<DensityChangedContext> OnDensityChanged { get; set; }
```

Fires when density changes. Most consumers don't need this — density is a UI preference. The event exists for analytics ("most users pick Spacious in clinical contexts") and for consumers that need to coordinate density with other surfaces.

---

## §14.10 — Worked examples

### §14.10.1 — Patient list with spacious default for clinical safety

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list"
           Density="TableDensity.Spacious"
           ShowDensityToggle="true">
    <LipiColumn Field="@(p => p.Uhid)" />
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.DateOfBirth)" Type="ColumnType.Date" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
</LipiTable>
```

Default Spacious — large text, generous padding — for clinical context. User can switch to Compact if they want more data on screen. Choice persists.

### §14.10.2 — Audit log with compact default for data-density

```razor
<LipiTable TItem="AuditEvent"
           DataSource="@LoadAuditAsync"
           KeySelector="@(e => e.Id)"
           TableId="audit-log"
           Density="TableDensity.Compact"
           PaginationMode="PaginationMode.InfiniteScroll">
    <LipiColumn Field="@(e => e.Timestamp)" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(e => e.Actor)" />
    <LipiColumn Field="@(e => e.Action)" />
    <LipiColumn Field="@(e => e.Resource)" />
</LipiTable>
```

Audit log defaults to Compact — fits more events per screen for power-user review. Infinite scroll for continuous browsing.

### §14.10.3 — Application-wide density preference

```razor
@inject IUserPreferences Prefs

<LipiTable Density="@Prefs.PreferredTableDensity"
           ShowDensityToggle="false"
           ...>
```

The user's account settings page has a global "Default table density" preference. All LipiTables across the app read it. Per-table toggles are hidden — density is centrally managed.

---

*End of §14. Proceed to §15 — Aggregation.*


# LipiTable Spec — §15 Aggregation

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column model — AggregateFn, CustomAggregate, AggregateFormat), §4 (TableQueryResponse.Aggregates), §9 (grouping — group-level aggregates)

---

## §15.1 — Overview

Aggregation lets a column display a summary value across rows — sum of all values, average, count, min/max, etc. Aggregates render in two places:

- **Footer aggregate row** (region ⑧ₐ per §2.1) — one aggregate per column, summarizing the whole filtered dataset
- **Group header rows** (§9.3) — per-group aggregates when grouping is active

Aggregation is opt-in per column via the `AggregateFn` parameter (§3.2.2). Without it, no aggregate renders for that column.

LipiTable supports a closed set of built-in aggregates plus a caller-supplied custom function. Server-side mode delegates aggregate computation to the server (returned via `TableQueryResponse.Aggregates`).

---

## §15.2 — Built-in aggregates

### §15.2.1 — `LipiAggregate` enum

```csharp
public enum LipiAggregate
{
    None,
    Sum,
    Avg,
    Count,            // count of rows (ignores Field value)
    CountNonNull,     // count of rows where Field value is non-null
    CountDistinct,
    Min,
    Max,
    First,            // first non-null value in current order
    Last              // last non-null value in current order
}
```

### §15.2.2 — Per-type compatibility

Each aggregate is only meaningful for certain column types:

| Aggregate | Compatible types | Notes |
|---|---|---|
| `Sum` | Number, Currency | Adds all non-null values |
| `Avg` | Number, Currency | Arithmetic mean of non-null values |
| `Count` | All | Counts all rows; ignores Field value |
| `CountNonNull` | All | Counts rows where Field value is non-null |
| `CountDistinct` | All | Counts distinct Field values (null counted once if present) |
| `Min` | Number, Currency, Date, DateTime, Time, Text, Mono | Min comparable value |
| `Max` | Same as Min | Max comparable value |
| `First` | All | First row's Field value in current sort order |
| `Last` | All | Last row's Field value in current sort order |

Incompatible combinations (e.g., `Sum` on a Text column) produce a developer-mode error and render `—` in the aggregate cell. Production: same render, error logged.

### §15.2.3 — Null handling

Aggregates skip null values by default (except `Count` which counts all rows including null):

| Aggregate | Null behavior |
|---|---|
| `Sum` | Nulls skipped; sum of remaining |
| `Avg` | Nulls skipped; avg of remaining (denominator = non-null count) |
| `Count` | Nulls counted (every row counts) |
| `CountNonNull` | Nulls skipped |
| `CountDistinct` | Nulls treated as one distinct value if present |
| `Min` / `Max` | Nulls skipped |
| `First` / `Last` | Nulls skipped — finds first/last non-null |

If every row has a null value for the column, aggregates return:
- `Sum` → 0
- `Avg` → null (no average is meaningful)
- `Count` → row count
- `CountNonNull` → 0
- `CountDistinct` → 0 (or 1 if "null is a value" semantics are wanted — controlled by `CountDistinctTreatsNullAsValue` parameter, default false)
- `Min` / `Max` / `First` / `Last` → null

Null aggregate values render as `—` (em-dash) in the footer cell by default. Override via `AggregateFormat` to customize.

### §15.2.4 — Empty dataset behavior

When the table has zero rows (Empty state or FilteredEmpty state):
- Aggregate row is hidden (collapses)
- Group rows have no aggregates (no groups exist)
- `Count` would be 0 — semantically meaningful, but UX-wise the empty state should be a clean "no data" message, not "Count: 0"

If the consuming page wants the aggregate row visible even when empty (e.g., dashboards that always show "Total: 0"), set `ShowAggregateRowWhenEmpty="true"` on the table.

---

## §15.3 — Custom aggregates

### §15.3.1 — `CustomAggregate` parameter

```razor
<LipiColumn Field="@(o => o.LineTotal)" 
            Type="ColumnType.Currency"
            CustomAggregate="@AverageWithDiscount"
            AggregateFormat="C2" />

@code {
    private static readonly Func<IEnumerable<decimal?>, object?> AverageWithDiscount =
        values =>
        {
            var nonNull = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (nonNull.Count == 0) return null;
            var avg = nonNull.Average();
            return avg * 0.85m;   // 15% discount applied to avg
        };
}
```

`CustomAggregate` takes `Func<IEnumerable<TValue>, object?>` and returns the aggregate value. The return is `object?` because custom aggregates can return any type (a string label, a tuple, a decimal, etc.).

When both `AggregateFn` and `CustomAggregate` are set, `CustomAggregate` wins. `AggregateFn` is ignored with a developer-mode warning.

### §15.3.2 — Custom aggregate format

The returned object renders via:
1. `AggregateFormat` parameter — applied as `string.Format("{0:" + Format + "}", value)` if value is `IFormattable`
2. The column's type formatter (e.g., Currency uses currency symbol) if no AggregateFormat
3. `value?.ToString() ?? "—"` as final fallback

For complex aggregate outputs (e.g., "Avg 21,425 / Max 50,000"), provide a `<FooterAggregateTemplate>` instead (see §15.5.4 below).

### §15.3.3 — Server-side delivery of custom aggregates

In server-side mode, custom aggregates can't be computed client-side (the data isn't there). The server is responsible for computing the value and returning it via `TableQueryResponse.Aggregates`. The custom function on the client is then **not invoked**; LipiTable just renders the server-supplied value.

This means the server's aggregate logic must match the client's intended behavior — coordination between client (declaring the aggregate) and server (computing it) is the consuming page's responsibility.

If a server-side table has `CustomAggregate` set but the server doesn't return a value in `Aggregates`, LipiTable renders `—` and logs a developer-mode warning.

---

## §15.4 — Aggregate placement

### §15.4.1 — Footer aggregate row (region ⑧ₐ)

Rendered whenever any column has `AggregateFn` or `CustomAggregate`. Layout:

```
┌────┬─────────────┬──────────┬────────────┐
│ ☑  │ Patient     │ Visits   │ Total      │
├────┼─────────────┼──────────┼────────────┤
│ ☐  │ Patel       │ 12       │ ₹4,28,500  │
│ ☐  │ Reddy       │ 8        │ ₹2,15,000  │
│ ☐  │ Sharma      │ 5        │ ₹1,12,000  │
├════┼═════════════┼══════════┼════════════┤
│    │ 3 patients  │ Σ 25     │ Σ ₹7,55,500│   ← aggregate footer
└────┴─────────────┴──────────┴────────────┘
```

Style:
- Background: `--lipi-table-bg-header` (slightly tinted, like the header)
- Border-top: heavier (1.5px) to separate from data rows
- Font weight: medium (slightly emphasized)
- Selection / expand cells in the aggregate row are blank (no checkbox, no chevron)
- Per-cell aggregate value rendered, formatted per column

### §15.4.2 — Aggregate prefix

Each aggregate cell can optionally show a prefix indicating which aggregate function was applied:

```
Σ ₹7,55,500    ← Sum prefix
~ ₹25,183      ← Avg prefix
↓ ₹1,12,000    ← Min prefix
↑ ₹4,28,500    ← Max prefix
# 25            ← Count prefix
```

Controlled by:

```csharp
[Parameter] public bool ShowAggregatePrefix { get; set; } = true;
```

Default `true` (prefix renders). Disable for tables where a single aggregate type is used throughout and the prefix is noise.

The prefix uses `--font-mono` font so the symbol aligns cleanly with numeric content.

### §15.4.3 — Footer aggregate scope

The footer aggregate operates on:

- **Client-side mode**: the **filtered** dataset (NOT the current page). If the user filters to 3 patients and paginates page 1 of 1, the aggregate is for those 3 patients across all pages — same value across page changes.
- **Server-side mode**: whatever the server returns in `TableQueryResponse.Aggregates`. Convention: server computes aggregates over the filtered dataset (excluding pagination).

This means the user can change page size, navigate pages, and the footer aggregate stays consistent — it represents "all filtered data," not just "what's on screen."

If the consuming page wants page-scoped aggregates ("sum of current page only"), they can compute it manually via `<FooterAggregateTemplate>`.

### §15.4.4 — Group-level aggregates

When grouping is active (§9), each group's header row renders aggregates for that group:

```
▾ Doctor: Dr. Reddy (8)  | Σ ₹2,15,000  ← group aggregate row
  ☐  Patel    3 visits   ₹85,000
  ☐  Sharma   2 visits   ₹50,000
  ☐  Iyer     3 visits   ₹80,000
▾ Doctor: Dr. Patel (5)  | Σ ₹1,12,000
  ☐  ...
```

Group aggregates use the same `AggregateFn` / `CustomAggregate` set on each column. The set of rows aggregated is the group's members.

When the group is collapsed, the group header still shows the aggregate (more useful when collapsed than when expanded — collapsed groups need a summary).

### §15.4.5 — Group + footer aggregate

When both grouping AND footer aggregates are configured:
- Each group header shows the group's aggregate
- The footer row shows the **grand total** across all groups

This matches Excel pivot table semantics.

If the consuming page wants only group-level (no grand total), set `ShowFooterAggregateWhenGrouped="false"` on the table.

---

## §15.5 — Format and templates

### §15.5.1 — `AggregateFormat` parameter

Per the column model (§3.2.2):

```razor
<LipiColumn Field="@(p => p.LineTotal)" 
            Type="ColumnType.Currency"
            AggregateFn="LipiAggregate.Sum"
            AggregateFormat="C2" />
```

`AggregateFormat` is a format string applied via `string.Format("{0:" + Format + "}", value)`. Common patterns:

| Format | Output |
|---|---|
| `C0` | `₹7,55,500` (currency, no decimals) |
| `C2` | `₹7,55,500.00` (currency, 2 decimals) |
| `N0` | `7,55,500` (number, no decimals) |
| `N2` | `7,55,500.00` (number, 2 decimals) |
| `F1` | `7755500.0` (fixed, 1 decimal) |
| `P0` | `75%` (percentage, no decimals) |
| `D` | `15/05/2026` (date, short) |
| `MMM yyyy` | `May 2026` (custom date) |

When `AggregateFormat` is not set:
- Currency / Number columns: inherits the column's display format
- Date / DateTime / Time: inherits the column's display format via IDateFormatService
- Text / Mono: no format (raw `ToString()`)

### §15.5.2 — Format inheritance from column

By default, an aggregate uses the column's standard cell formatting. A Currency column showing values as "₹1,500.00" will show its sum as "₹7,55,500.00" without explicit AggregateFormat.

Override only when the aggregate's natural format differs from the cell format. Example: cell shows full precision currency; aggregate shows rounded sum.

### §15.5.3 — Custom aggregate label

By default, the aggregate cell shows just the formatted value (with optional prefix per §15.4.2). To add a custom label:

```csharp
[Parameter] public string? AggregateLabel { get; set; }
```

```razor
<LipiColumn Field="@(o => o.LineTotal)" 
            AggregateFn="LipiAggregate.Sum"
            AggregateLabel="Subtotal:"
            AggregateFormat="C2" />
```

Renders as:

```
Subtotal: ₹7,55,500.00
```

The label is plain text rendered before the value. No semantic meaning beyond presentation.

### §15.5.4 — `<FooterAggregateTemplate>` slot per column

For aggregate cells that need full custom rendering:

```razor
<LipiColumn Field="@(o => o.LineTotal)" 
            Type="ColumnType.Currency"
            CustomAggregate="@_complexAgg">
    <FooterAggregateTemplate Context="aggCtx">
        <div class="dual-aggregate">
            <div>Sum: @(aggCtx.Value)</div>
            <div>Avg: @(aggCtx.Extra["avg"])</div>
        </div>
    </FooterAggregateTemplate>
</LipiColumn>
```

The context exposes:

```csharp
public sealed class AggregateContext<TValue>
{
    public object? Value { get; }                  // primary aggregate result
    public IReadOnlyDictionary<string, object?>? Extra { get; }   // server-supplied extras
    public LipiAggregate? FunctionUsed { get; }
    public long RowCount { get; }                  // number of rows aggregated
    public bool IsGroupLevel { get; }              // true when rendering a group header, false for footer
    public object? GroupValue { get; }             // group's discriminator value when IsGroupLevel
}
```

The `Extra` dictionary lets the server return multiple values per column (e.g., sum + avg + median + std dev) without forcing the aggregate to a single value. Useful for analytical tables.

---

## §15.6 — Server-side aggregation

### §15.6.1 — Wire format

Cross-reference §4.2.1 — the request includes `AggregateColumns`:

```csharp
public sealed record TableQueryRequest
{
    // ...
    public IReadOnlyList<string> AggregateColumns { get; init; } = Array.Empty<string>();
    // ...
}
```

LipiTable populates this list with the `ColumnKey` of every column that has `AggregateFn` or `CustomAggregate`. The server uses this list to know which aggregates to compute.

Response (cross-reference §4.3.4):

```csharp
public sealed record TableQueryResponse<TItem>
{
    // ...
    public IReadOnlyDictionary<string, object?>? Aggregates { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>?>? GroupAggregates { get; init; }
    // ...
}
```

- `Aggregates` — keyed by `ColumnKey`, value = the aggregate for that column over the filtered dataset
- `GroupAggregates` — keyed by group-key-path (e.g., `"Cardiology"` or `"Cardiology/ICU"`), value = inner dictionary keyed by `ColumnKey`

Example response:

```json
{
  "rows": [ ... ],
  "totalCount": 487,
  "aggregates": {
    "LineTotal": 755500.00,
    "Visits": 1247
  },
  "groupAggregates": {
    "Cardiology": { "LineTotal": 320500.00, "Visits": 542 },
    "Neurology":  { "LineTotal": 215000.00, "Visits": 311 }
  }
}
```

### §15.6.2 — Server contract details

The server is expected to:
1. Read `request.AggregateColumns` to know which columns need aggregates
2. Compute aggregates over the **filtered** dataset (NOT the current page)
3. Return values in the response

If the server doesn't return aggregates (response's `Aggregates` is null), LipiTable renders `—` for all aggregate cells and logs a developer-mode warning. Production fallback: silent `—` render.

### §15.6.3 — Aggregate function hint

LipiTable doesn't tell the server *which* aggregate function to use — only the column keys. The server is expected to know:
- "For column `LineTotal`, the configured aggregate is Sum"
- "For column `Visits`, the configured aggregate is Sum"

This couples client-side declarations to server-side query logic. The consuming page is responsible for keeping them in sync.

Alternative pattern (for fully-decoupled): the server returns extras inside `Aggregates` (e.g., `LineTotal_sum`, `LineTotal_avg`, `LineTotal_max`) and the client picks which to display via `CustomAggregate`. Not built into LipiTable; left to the consuming page.

### §15.6.4 — Group-level server aggregates

For grouped tables in server-side mode using the pre-grouped response shape (`TableQueryResponse.PreGrouped`), per-group aggregates live on each `GroupBucket`:

```csharp
public sealed record GroupBucket<TItem>(
    string ColumnKey,
    object? GroupValue,
    long ItemCount,
    IReadOnlyList<TItem> Items,
    IReadOnlyDictionary<string, object?>? Aggregates,
    IReadOnlyList<GroupBucket<TItem>>? SubGroups
);
```

The `Aggregates` dictionary on each bucket holds per-column values for that group. LipiTable renders them in the group header row.

---

## §15.7 — Aggregation and other features

### §15.7.1 — Aggregation and filtering

Aggregates respect filters. When the user changes filters, the aggregate row updates to reflect the new filtered dataset.

- Client-side: re-runs aggregate computation over the new filtered rows
- Server-side: re-fetches with new filters; server returns new aggregate values

### §15.7.2 — Aggregation and selection

Aggregates do NOT respect selection. They always compute over the full filtered dataset, not just selected rows.

For "aggregate of selected rows only," the consuming page can render this separately:

```razor
<LipiTable @ref="_table" TItem="Order" ... SelectionMode="SelectionMode.Multi">
    ...
</LipiTable>

<div class="selection-summary">
    @if (_selected.Any())
    {
        <span>Selected total: @_selected.Sum(o => o.LineTotal):C2</span>
    }
</div>
```

LipiTable doesn't build this into the footer because:
- Mixing "filtered total" and "selected total" semantics in the same row is confusing
- The bulk action bar (§5.6.4) is the natural surface for selection-specific summaries

### §15.7.3 — Aggregation and pagination

Aggregates span pages. Client-side: full filtered dataset. Server-side: server's aggregate result.

The aggregate value does NOT change when the user navigates pages. Consistent across pagination.

### §15.7.4 — Aggregation and sort

Sort doesn't affect aggregate values for most functions (Sum, Avg, Count, etc.). Aggregates are order-independent.

Exception: `First` and `Last` depend on current sort. The "first non-null value" is computed in the current sort order. Changing sort changes which row is "first" / "last."

### §15.7.5 — Aggregation and virtualization

Aggregates compute over the full filtered dataset (or server response), not over the virtualized window. The aggregate row renders once at the table footer, independent of scroll position.

### §15.7.6 — Aggregation and inline edit

When a row is in inline edit mode and its values are dirty (not yet saved), aggregates do NOT include the dirty values:

- Client-side: aggregates use the row's saved state, not the in-progress edit values
- Server-side: aggregates use whatever the server's data shows at query time (no awareness of client-side dirty rows)

After save, the row's new values are in the dataset and aggregates update accordingly.

---

## §15.8 — Performance considerations

### §15.8.1 — Client-side aggregate performance

For 1000 rows × 5 aggregate columns, computation is approximately 5000 LINQ operations per re-aggregation. Negligible.

For 10,000+ rows, computation cost grows. Mitigation:
- Aggregates only recompute on filter / sort changes (not on virtualization scroll)
- Cached across page changes in client-side mode
- For very large datasets, consider server-side mode where aggregates are computed once on the server

### §15.8.2 — Aggregate caching

LipiTable caches aggregate values keyed by `(filtered-data-snapshot-hash, aggregate-set)`. As long as the filtered dataset doesn't change, aggregates aren't recomputed.

The hash is invalidated by:
- `Items` reference change (client-side)
- Filter change
- New `DataSource` response (server-side)
- Programmatic refresh

---

## §15.9 — Disabling aggregation

Aggregation has no global on/off — it's purely per-column. To disable, simply don't set `AggregateFn` or `CustomAggregate` on any column.

If aggregates are configured but the consuming page wants to hide the footer row temporarily:

```csharp
[Parameter] public bool ShowAggregateRow { get; set; } = true;
```

`ShowAggregateRow="false"` hides the row entirely. Configured aggregates are computed but not rendered. Used rarely.

---

## §15.10 — Worked examples

### §15.10.1 — Simple footer sum

```razor
<LipiTable TItem="LineItem" Items="@_lineItems" KeySelector="@(l => l.Id)">
    <LipiColumn Field="@(l => l.Description)" Header="Description" />
    <LipiColumn Field="@(l => l.Quantity)" 
                Type="ColumnType.Number"
                AggregateFn="LipiAggregate.Sum"
                AggregateLabel="Total qty:" />
    <LipiColumn Field="@(l => l.UnitPrice)" 
                Type="ColumnType.Currency"
                AggregateFn="LipiAggregate.Avg"
                AggregateLabel="Avg price:" />
    <LipiColumn Field="@(l => l.LineTotal)" 
                Type="ColumnType.Currency"
                AggregateFn="LipiAggregate.Sum"
                AggregateLabel="Subtotal:" />
</LipiTable>
```

Footer renders quantity total, average unit price, line subtotal.

### §15.10.2 — Grouped with per-group + grand total

```razor
<LipiTable TItem="Invoice"
           DataSource="@LoadInvoicesAsync"
           KeySelector="@(i => i.Id)"
           GroupBy="@(i => i.Department)"
           ShowGroupBar="true"
           DefaultGroupState="DefaultGroupState.Expanded">
    <LipiColumn Field="@(i => i.Number)"     Header="Invoice #" />
    <LipiColumn Field="@(i => i.Date)"       Type="ColumnType.Date" />
    <LipiColumn Field="@(i => i.Amount)"     
                Type="ColumnType.Currency"
                AggregateFn="LipiAggregate.Sum" />
</LipiTable>
```

Each department's group header shows the group's invoice total. Footer shows grand total across all departments.

### §15.10.3 — Custom aggregate (server-supplied extras)

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)">
    <LipiColumn Field="@(r => r.Value)" 
                Type="ColumnType.Number"
                CustomAggregate="@(_ => null)"   <!-- placeholder; server supplies -->
                AggregateFormat="N2">
        <FooterAggregateTemplate Context="ctx">
            <div>Mean: @(ctx.Extra?["mean"])</div>
            <div>Median: @(ctx.Extra?["median"])</div>
            <div>StdDev: @(ctx.Extra?["stddev"])</div>
        </FooterAggregateTemplate>
    </LipiColumn>
</LipiTable>
```

Server computes statistical aggregates server-side and returns them in `Aggregates["Value"]` as an extras dictionary. Footer template renders the dictionary inline.

### §15.10.4 — Selection-aware summary outside the table

```razor
<LipiTable @ref="_table"
           TItem="Order"
           Items="@_orders"
           KeySelector="@(o => o.Id)"
           SelectionMode="SelectionMode.Multi"
           @bind-SelectedItems="_selected">
    <LipiColumn Field="@(o => o.Description)" />
    <LipiColumn Field="@(o => o.Amount)" 
                Type="ColumnType.Currency"
                AggregateFn="LipiAggregate.Sum"
                AggregateLabel="All-orders total:" />
</LipiTable>

@if (_selected.Any())
{
    <div class="selection-summary-card">
        Selected: @_selected.Count orders
        Total: @_selected.Sum(o => o.Amount):C2
    </div>
}
```

Footer shows total of all filtered orders. Below the table, a separate card shows the total of selected orders only.

---

*End of §15. Proceed to §16 — Toolbar and chrome.*


# LipiTable Spec — §16 Toolbar and chrome

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2 (anatomy regions ⓪ and ①), §5 (bulk action bar), §7 (filter chips), §14 (density toggle), §17 (export)

---

## §16.1 — Overview

The toolbar (region ①) and its companion regions (header band ⓪, filter chips ②, bulk action bar ③, group bar ④) together form LipiTable's "chrome" — the controls and indicators surrounding the data body. This section covers the toolbar specifically; the companion regions are covered in their respective feature sections.

The toolbar is configurable: it can be empty (no toolbar rendered at all), packed with built-in controls, or extensively customized via slots. Per Q9.1 (locked) — some toolbar elements are always-on, some are opt-out, some are opt-in, and one is auto-rendered.

---

## §16.2 — Toolbar zones

### §16.2.1 — Three horizontal zones

The toolbar (region ①) splits horizontally into three zones:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Left zone                  │   Center zone   │  Right zone                  │
│  [🔍 search]                 │   (reserved)    │  [density] [picker] [export] │
│  [custom-left content]      │                 │  [refresh] [+ Add] [custom]  │
└─────────────────────────────────────────────────────────────────────────────┘
```

CSS classes (cross-reference §2.2.2):
- `lipi-table-toolbar` — outer container (flex row, 48px height per `--lipi-table-toolbar-h`)
- `lipi-table-toolbar-left`
- `lipi-table-toolbar-center` (reserved; usually empty)
- `lipi-table-toolbar-right`

The center zone exists for future expansion (e.g., a tab strip for view modes, a status indicator). In v1.0 it stays empty by default — callers can populate via the `<ToolbarCenter>` slot.

### §16.2.2 — Zone occupants by default

When the corresponding feature is enabled:

| Zone | Default occupants |
|---|---|
| Left | Quick search input (if `ShowQuickFilter="true"`), then `<ToolbarLeft>` slot content |
| Center | `<ToolbarCenter>` slot content (empty default) |
| Right | `<ToolbarRight>` slot content, then density toggle, column picker, refresh, export, add button |

Slot content appears BEFORE built-in controls in their zone — caller's content is "primary," built-in controls are "secondary."

### §16.2.3 — Auto-collapse on narrow toolbars

When the toolbar's container is narrow (< 600px), built-in controls collapse into an overflow menu:

```
┌──────────────────────────────────────┐
│ [🔍 search]    [⋯ ] [+ Add]          │
└──────────────────────────────────────┘
                  ▲
            overflow menu opens:
            ┌──────────────────┐
            │ Density          │
            │   ◯ Compact      │
            │   ● Comfortable  │
            │   ◯ Spacious     │
            │ ─────────────    │
            │ Column picker    │
            │ Refresh          │
            │ Export           │
            └──────────────────┘
```

Threshold and collapse priority:
1. Quick search stays visible as long as possible (highest priority — primary user affordance)
2. Add button stays visible (high-frequency action)
3. Density / picker / refresh / export collapse first

Configurable via `ToolbarCollapseThresholdPx` (default 600). For tables in known-narrow surfaces (sidebars), set to 0 to always-collapse.

---

## §16.3 — Built-in toolbar controls

### §16.3.1 — Always-on

Always-on means: render when the corresponding feature is enabled. Cannot be hidden without disabling the feature.

| Control | Visible when | Disable by |
|---|---|---|
| Quick search input | `ShowQuickFilter="true"` (or `QuickFilterText` parameter is bound) | Setting `ShowQuickFilter="false"` (or removing parameter binding) |
| Filter chips strip | At least one filter is active (region ② per §7.8) | Disabling all filters |
| Bulk action bar | At least one row is selected and `SelectionMode != None` (region ③ per §5.6.4) | Setting `SelectionMode="None"` |

### §16.3.2 — Opt-out (default visible)

Opt-out means: visible by default; the feature still works programmatically when hidden.

| Control | Default state | Hide by |
|---|---|---|
| Density toggle (§14.4) | Visible | `ShowDensityToggle="false"` |
| Column picker (§16.4) | Visible | `ShowColumnPicker="false"` |
| Refresh button (§16.5) | Visible in server-side mode; hidden in client-side | `ShowRefreshButton="false"` (or `true` to force in client mode) |

### §16.3.3 — Opt-in (default hidden)

| Control | Visible by | Default |
|---|---|---|
| Title + subtitle | Setting `Title="..."` and/or `Subtitle="..."` | Hidden |
| Header band (region ⓪) | Setting Title / Subtitle / `<HeaderTemplate>` | Hidden |
| Export button | `ShowExportButton="true"` | Hidden |
| Add button | `ShowAddButton="true"` | Hidden |
| Auto-refresh status indicator (§16.5.4) | `AutoRefreshInterval` is set | Hidden when no interval |

### §16.3.4 — Auto

| Control | Trigger | Section |
|---|---|---|
| Bulk action bar | Selection count > 0 | §5.6.4 |
| Filter chips strip | At least one filter active | §7.8 |
| Cap banner | Server-side "All" cap kicks in | §8.2.2 |
| Group bar | `ShowGroupBar="true"` | §9.2 |

---

## §16.4 — Column picker

### §16.4.1 — Trigger

The column picker is a toolbar button:

```
[ ▦ Columns ]    or    [ ▦ Columns (12 of 18) ]   ← count shown when some columns hidden
```

Click → opens a popover anchored to the button.

### §16.4.2 — Popover contents

```
┌─ Columns ────────────────────────────────┐
│ 🔍 Filter columns...                      │  ← search for columns by name (when many)
│                                           │
│ PINNED LEFT                               │
│   ☑ ⋮⋮ Select   (system, locked)         │
│   ☑ ⋮⋮ UHID                              │
│   ☑ ⋮⋮ Name                              │
│                                           │
│ UNPINNED                                  │
│   ☑ ⋮⋮ Age                               │
│   ☑ ⋮⋮ Mobile                            │
│   ☐ ⋮⋮ DOB                               │
│   ☑ ⋮⋮ Status                            │
│   ☐ ⋮⋮ Last Visit                        │
│                                           │
│ PINNED RIGHT                              │
│   ☑ ⋮⋮ Actions  (locked position)        │
│                                           │
│ ─────────────────────────────────────     │
│   [Reset to defaults]                     │
└───────────────────────────────────────────┘
```

Anatomy:
- Search input at top — filters the column list by name (visible only when there are 8+ columns)
- Three zones (Pinned Left / Unpinned / Pinned Right) — matching the table's pin zones
- Each entry: checkbox (toggle visibility) + drag handle (`⋮⋮`) + column name
- System columns (selection checkbox, expand chevron) are listed but locked (no drag, no hide)
- Reset to defaults button at bottom

### §16.4.3 — Interactions

| Action | Effect |
|---|---|
| Click checkbox | Toggle visibility for that column |
| Drag handle (`⋮⋮`) | Reorder within / across zones (same constraints as header drag per §13.3.3) |
| Click pin indicator next to column (if visible) | Cycle pin state: None → Left → Right → None |
| Search input | Filter the visible list (doesn't change visibility — just navigation aid) |
| Reset to defaults | Clear all persisted column state for this `TableId` for this user; reload declared defaults |

### §16.4.4 — Auto-hide for narrow toolbars

In overflow menu (per §16.2.3), the column picker condenses to a single "Columns" entry that, when clicked, opens the full popover as a modal-style overlay (since the overflow context can't host a popover cleanly).

### §16.4.5 — Programmatic

```csharp
public ValueTask OpenColumnPickerAsync();
public ValueTask CloseColumnPickerAsync();
```

Useful for tutorial / onboarding flows ("Click here to customize your columns").

### §16.4.6 — Disable

```razor
<LipiTable ShowColumnPicker="false" ...>
```

Picker button hidden. Programmatic `OpenColumnPickerAsync` still works.

---

## §16.5 — Refresh button

### §16.5.1 — Rendering

Single icon button (Lucide `refresh-cw`) in the toolbar right zone:

```
[ ⟳ ]    or    [ ⟳ ]   ← spinning while in-flight
```

Default visible in server-side mode (`ShowRefreshButton="true"` is default when `DataSource` is set), hidden in client-side mode (data is in memory, "refresh" has no server-roundtrip semantics).

### §16.5.2 — Click behavior

Clicking the button:
- Server-side: re-invokes `DataSource` with the current `TableQueryRequest`. Same effect as `tableRef.RefreshAsync()`.
- Client-side (when force-enabled): triggers a re-render with the current `Items` reference. If the caller has mutated `Items` between renders, this is the way to make LipiTable pick up the changes (though `Items` mutation is discouraged per §4.8).

During in-flight refresh, the icon rotates 360° at the LipiTable spin rate (1.2 seconds per rotation). The button is disabled (no double-click).

### §16.5.3 — Force-enable in client-side mode

For client-side tables where the data updates externally (e.g., a SignalR subscription mutating a shared list):

```razor
<LipiTable ShowRefreshButton="true" 
           OnRefresh="@HandleRefreshAsync"
           ...>
```

`OnRefresh` callback fires before the re-render — the consuming page can re-fetch / re-compute as needed:

```csharp
private async Task HandleRefreshAsync()
{
    _items = await _service.FetchLatestAsync();
    StateHasChanged();
}
```

### §16.5.4 — Auto-refresh interval

Cross-reference §4.7.4. When `AutoRefreshInterval` is set, an indicator appears next to the refresh button:

```
[ ⟳ ] Refreshes every 30s   or   [ ⟳ ] Last refreshed 12s ago
```

Two display modes via `AutoRefreshIndicatorStyle`:
- `Interval` (default) — shows the configured interval
- `LastRefreshed` — shows time since last successful refresh (updates every second)

Indicator visible only when `AutoRefreshInterval != null`. Hidden otherwise.

### §16.5.5 — Pause indicator

When auto-refresh is paused (due to inline edit or popover interaction per §4.7.4), the indicator changes:

```
[ ⟳ ] Auto-refresh paused (editing)
```

Resumes silently when the pause condition clears.

---

## §16.6 — Title and subtitle (header band)

### §16.6.1 — Rendering

When `Title="..."` or `Subtitle="..."` (or both) is set, the header band (region ⓪) renders ABOVE the toolbar:

```
┌────────────────────────────────────┐
│ Patient List                        │  ← Title (h2-sized, primary text)
│ Active patients seen in last 30 days│  ← Subtitle (body-sized, muted)
└────────────────────────────────────┘
┌────────────────────────────────────┐
│ [🔍] [▼ Filters]   [density][⟳]    │  ← Toolbar
└────────────────────────────────────┘
```

CSS:
- Header band: `lipi-table-header-band` — padding 16px 16px 12px
- Title: `lipi-table-header-title` — h2 (font 20px in comfortable density)
- Subtitle: `lipi-table-header-subtitle` — body (font 13px), `--lipi-table-text-muted`

### §16.6.2 — Custom header via `<HeaderTemplate>`

When the title/subtitle pattern isn't enough:

```razor
<LipiTable TItem="..." ...>
    <HeaderTemplate>
        <div class="custom-table-header">
            <LipiAvatar Src="@_deptIcon" />
            <div>
                <h2>Patient List — Cardiology</h2>
                <p>487 patients · last sync 2 minutes ago</p>
            </div>
            <LipiButton OnClick="@OpenDeptSettings">⚙ Department settings</LipiButton>
        </div>
    </HeaderTemplate>
    
    <!-- columns -->
</LipiTable>
```

`<HeaderTemplate>` replaces the entire header band content. Title and Subtitle parameters are ignored when HeaderTemplate is provided.

The header band container (padding, border-bottom) still applies; the template fills the inner area.

### §16.6.3 — When to use which

| Use | Pattern |
|---|---|
| Plain title only | `Title="..."` parameter |
| Title + subtitle | `Title="..."` + `Subtitle="..."` |
| Rich layout (icon + multi-line + actions) | `<HeaderTemplate>` |
| No header at all | Don't set any of the above |

---

## §16.7 — Custom toolbar slots

### §16.7.1 — Three slots

```razor
<LipiTable TItem="..." ...>
    <ToolbarLeft>
        <LipiButton OnClick="@OpenFilterPresets">Quick filters ▾</LipiButton>
    </ToolbarLeft>
    
    <ToolbarCenter>
        <span class="view-mode-indicator">Tile View</span>
    </ToolbarCenter>
    
    <ToolbarRight>
        <LipiButton OnClick="@ShareTable" Variant="ButtonVariant.Secondary">Share</LipiButton>
    </ToolbarRight>
    
    <!-- columns -->
</LipiTable>
```

The slots are RenderFragments. Caller-rendered content appears alongside (and BEFORE) the built-in controls in each zone.

### §16.7.2 — Ordering within a zone

Within `ToolbarLeft`:
1. Quick search input (if enabled)
2. Caller's `<ToolbarLeft>` content

Within `ToolbarRight`:
1. Caller's `<ToolbarRight>` content
2. Density toggle
3. Column picker
4. Refresh button
5. Export button
6. Add button

This ordering puts caller content closer to the table's "primary" actions (search on left, custom controls before built-in chrome on right). Reverses are not configurable in v1.0 — caller adapts to this ordering or omits built-in controls and recreates them in the slot.

### §16.7.3 — Slot spacing

Slots use `--lipi-table-toolbar-gap` (8px) between adjacent items. Caller's content inherits this spacing automatically — adjacent buttons in `<ToolbarRight>` are spaced 8px apart without the caller explicitly setting margins.

---

## §16.8 — Add button

### §16.8.1 — Rendering

When `ShowAddButton="true"`:

```
[ + Add ]    or    [ + Add Patient ]   ← caller-provided label via AddButtonLabel
```

Right-aligned primary `LipiButton`. Icon (Lucide `plus`) prefixed by default.

### §16.8.2 — Click behavior

Cross-reference §12.12.1 (Add-new-row pattern):

1. Invokes `OnAddNew` callback if provided — caller returns an initial `TItem`
2. If `<AddRowTemplate>` is provided, inserts the new pseudo-row inline and enters edit mode
3. If no template AND `EditMode != None`, uses the standard inline edit inputs for the new row
4. If `EditMode = None`, fires `OnAddClick` instead of starting inline add — caller handles (opens modal, drawer, separate page)

### §16.8.3 — `OnAddClick` for non-inline add

When the table doesn't support inline editing but still wants an Add button:

```razor
<LipiTable TItem="Patient"
           ShowAddButton="true"
           AddButtonLabel="+ Add Patient"
           EditMode="TableEditMode.None"
           OnAddClick="@OpenAddDialog">
    ...
</LipiTable>

@code {
    private async Task OpenAddDialog()
    {
        var newPatient = await ModalService.ShowAsync<AddPatientModal, Patient>();
        if (newPatient is not null)
        {
            await _patientService.AddAsync(newPatient);
            await _table!.RefreshAsync();
        }
    }
}
```

`OnAddClick` is just a callback. The caller controls the entire add flow.

If both `OnAddClick` AND inline add are configured (which is unusual), `OnAddClick` wins — caller's explicit handler takes precedence over the inline flow.

### §16.8.4 — Disabled state

```csharp
[Parameter] public bool AddButtonDisabled { get; set; }
```

Caller can disable the button without hiding it (e.g., "user lacks permission to add patients"). Tooltip via `AddButtonDisabledTooltip` explains why.

---

## §16.9 — Density toggle (cross-reference §14.4)

Already detailed in §14. Restated here for toolbar completeness:
- Visible by default in toolbar right zone
- `ShowDensityToggle="false"` hides
- Three style variants (Segmented / IconWithLabel / IconDropdown)
- Programmatic via `SetDensityAsync()`

---

## §16.10 — Export button (forward reference §17)

Detailed in §17. Toolbar-specific behavior:
- Visible when `ShowExportButton="true"` (default false)
- Single button → opens dropdown of formats (CSV / PDF / Print / "More options...")
- Per Q8.3 (locked) hybrid trigger

---

## §16.11 — Toolbar visibility logic

The full visibility logic for the toolbar container itself:

The toolbar renders **if any of these are true**:
- `ShowQuickFilter="true"`
- `ShowDensityToggle="true"` (default)
- `ShowColumnPicker="true"` (default)
- `ShowRefreshButton="true"` (default for server-side)
- `ShowExportButton="true"`
- `ShowAddButton="true"`
- `<ToolbarLeft>` slot has content
- `<ToolbarCenter>` slot has content
- `<ToolbarRight>` slot has content

Otherwise the toolbar collapses entirely (no row of empty space — region ① has zero height).

This means a stripped-down read-only table (`SelectionMode="None"`, no quick filter, no density toggle, no column picker, no slots) renders with NO toolbar at all — just header (if Title set) + body + footer.

To force the toolbar to render even when empty (rare):

```csharp
[Parameter] public bool ForceToolbarVisible { get; set; }
```

Useful when the toolbar acts as visual structure even without controls.

---

## §16.12 — Toolbar accessibility

### §16.12.1 — ARIA roles

- Toolbar container: `role="toolbar"`, `aria-label="Table controls"`
- Quick search input: standard input role, `aria-label="Search rows"`
- Each button has descriptive `aria-label` (e.g., "Open column picker", "Refresh data", "Open export menu")
- Density toggle: `role="radiogroup"` with three `role="radio"` children

### §16.12.2 — Keyboard navigation

Standard toolbar keyboard pattern:
- Tab into the toolbar → first focusable control receives focus
- Arrow keys (left/right) navigate within the toolbar (skips dividers)
- Tab moves out of the toolbar to the next focusable region (header / body)
- Enter / Space activate focused control
- Escape closes any open popover (column picker, export menu) and returns focus to the trigger button

### §16.12.3 — Screen reader announcements

- Toolbar reads as "Table controls toolbar"
- Quick search reads as "Search input, [N rows match]" — count updates live as user types
- Refresh button reads as "Refresh, [status]" where status reflects last refresh time
- Auto-refresh paused state announces "Auto-refresh paused due to editing"

---

## §16.13 — Worked examples

### §16.13.1 — Minimal toolbar (just quick search)

```razor
<LipiTable TItem="Setting"
           Items="@_settings"
           KeySelector="@(s => s.Key)"
           ShowQuickFilter="true"
           ShowDensityToggle="false"
           ShowColumnPicker="false">
    <LipiColumn Field="@(s => s.Key)" />
    <LipiColumn Field="@(s => s.Value)" />
</LipiTable>
```

Toolbar shows only the search input. Nothing else.

### §16.13.2 — Full chrome with custom slots

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list"
           Title="Patient List"
           Subtitle="Active patients across all clinics"
           ShowQuickFilter="true"
           ShowAddButton="true"
           AddButtonLabel="+ Add Patient"
           ShowExportButton="true"
           OnAddClick="@OpenAddPatientModal">
    
    <ToolbarLeft>
        <LipiSelect TValue="string" @bind-Value="_clinicFilter">
            <LipiOption Value="">All clinics</LipiOption>
            <LipiOption Value="north">North Clinic</LipiOption>
            <LipiOption Value="south">South Clinic</LipiOption>
        </LipiSelect>
    </ToolbarLeft>
    
    <ToolbarRight>
        <LipiButton Variant="ButtonVariant.Secondary" OnClick="@OpenSettings">⚙</LipiButton>
    </ToolbarRight>
    
    <!-- columns -->
</LipiTable>
```

Title + subtitle. Clinic dropdown in left zone (caller's custom). Built-in search + density + picker + refresh + export + Add Patient. Settings icon on far right.

### §16.13.3 — Toolbar-less compact table

```razor
<LipiTable TItem="Tag"
           Items="@_tags"
           KeySelector="@(t => t.Id)"
           SelectionMode="SelectionMode.None"
           ShowQuickFilter="false"
           ShowDensityToggle="false"
           ShowColumnPicker="false"
           ShowRefreshButton="false">
    <LipiColumn Field="@(t => t.Name)" />
    <LipiColumn Field="@(t => t.Color)" />
</LipiTable>
```

No toolbar renders at all (region ① has zero height). Pure data display in a card or panel.

### §16.13.4 — Custom header band

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)">
    
    <HeaderTemplate>
        <div class="lab-results-header">
            <div class="header-icon">
                <LipiIcon Name="flask-conical" />
            </div>
            <div class="header-text">
                <h2>Lab Results — @PatientName</h2>
                <div class="header-meta">
                    UHID: @Uhid · Last result: @LastResultTime · @ResultCount results
                </div>
            </div>
            <div class="header-actions">
                <LipiButton OnClick="@PrintAll">Print all</LipiButton>
                <LipiButton OnClick="@OpenTimeline">Timeline view</LipiButton>
            </div>
        </div>
    </HeaderTemplate>
    
    <!-- columns -->
</LipiTable>
```

Rich custom header with icon, multi-line context, and patient-specific actions. Replaces the simple Title/Subtitle pattern entirely.

---

*End of §16. Proceed to §17 — Export.*


# LipiTable Spec — §17 Export

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §3 (column types + IncludeInExport), §4 (data sources), §16 (toolbar export button)
**Phase 2.8 overview reference:** §3 — Export strategy

---

## §17.1 — Format strategy (zero external dependencies)

Per Phase 2.8 overview §3 and the locked standing rule on library dependencies, LipiTable's export supports three formats in v1.0:

| Format | Implementation | Status |
|---|---|---|
| **CSV** | In-house `CsvExporter.cs` (~150 lines), RFC 4180 compliant | ✅ Built in Phase 2.8 |
| **PDF** | Integrates in-house LiPi PDF library | 🔄 Stubbed in Phase 2.8; wired in Phase 2.10 |
| **Print** | Browser native print API + `lipi-table-print.css` | ✅ Built in Phase 2.8 |
| **Excel (.xlsx)** | Deferred — queued in `03-DEFERRED-ITEMS.md` for future in-house writer | ❌ Not in v1.0 |

Zero NuGet packages added. Zero external functional dependencies. CSV opens in every spreadsheet app — sufficient for "I want to see this in Excel" workflows.

The PDF export pathway is fully designed and the call site is wired up; the actual PDF rendering is stubbed (throws "PDF library pending Phase 2.10 integration" if invoked). StyleGuide PDF demos are marked "Available after Phase 2.10."

---

## §17.2 — Export trigger (locked hybrid)

Per Q8.3 (locked) — hybrid: toolbar dropdown of common formats + "More options..." modal for advanced controls.

### §17.2.1 — Toolbar Export button

When `ShowExportButton="true"`, a button renders in the toolbar's right zone:

```
[ ⤓ Export ▾ ]
```

Lucide icon `download` prefix. Caret indicates dropdown.

Click → dropdown opens:

```
┌──────────────────────┐
│ 📄 CSV               │
│ 📑 PDF               │  ← grayed/disabled until Phase 2.10
│ 🖨 Print             │
│ ──────────────────── │
│ ⚙ More options...    │
└──────────────────────┘
```

### §17.2.2 — Direct-format click

Clicking a format directly (CSV / PDF / Print) triggers export with:
- Format = the clicked entry
- Scope = `DefaultExportScope` (default `Filtered` per Q8.2)
- Columns = all visible columns (excluding columns with `IncludeInExport="false"`)
- Filename = auto-generated (see §17.6.1)

This is the "happy path" — one click, sensible defaults, immediate result.

### §17.2.3 — "More options..." opens modal

The bottom entry opens an Export Options modal (`LipiModal` from Phase 2.6.2):

```
┌─ Export options ─────────────────────────────────┐
│                                                   │
│ Format                                            │
│   ◉ CSV                                           │
│   ◯ PDF                                           │
│   ◯ Print                                         │
│                                                   │
│ Scope                                             │
│   ◯ Current view (filtered, sorted, paged)        │
│   ◉ Filtered (filtered, sorted, all pages)        │
│   ◯ All data (no filters)                         │
│   ◯ Selected rows only (3 selected)               │
│                                                   │
│ Columns  [ Select all ]  [ Reset ]                │
│   ☑ Name                                          │
│   ☑ Mobile                                        │
│   ☐ Internal ID                                   │
│   ☑ Status                                        │
│   ☑ Last visit                                    │
│                                                   │
│ Options                                           │
│   ☑ Include header row                            │
│   ☐ Include aggregate footer row                  │
│   ☑ Use UTF-8 BOM (Excel compatibility)           │
│                                                   │
│        [ Cancel ]   [ Export ]                    │
└───────────────────────────────────────────────────┘
```

Format options:
- **Format** — radio group with CSV / PDF (disabled until Phase 2.10) / Print
- **Scope** — radio group (§17.3)
- **Columns** — checkbox list of every visible column with `IncludeInExport != false`. "Select all" / "Reset" buttons. The "Selected rows" option appears only when `SelectionMode != None` and rows are selected.
- **Options** — format-specific toggles (header row, aggregate footer, BOM for CSV; orientation, page-numbers for PDF when ready)

Click "Export" → execute with the configured options. Click "Cancel" → dismiss without exporting.

### §17.2.4 — Modifier shortcut

Holding **Shift** while clicking the Export button (or any format option in the dropdown) opens the "More options" modal directly, pre-filled with that format. Power-user shortcut documented in tooltips ("Shift+click for options").

### §17.2.5 — Programmatic

```csharp
public ValueTask ExportAsync(ExportOptions options);
public ValueTask<bool> ExportAsync(ExportFormat format);   // shorthand using defaults
public ValueTask OpenExportModalAsync(ExportFormat? preselectedFormat = null);
```

```csharp
public sealed class ExportOptions
{
    public ExportFormat Format { get; init; }
    public ExportScope Scope { get; init; }
    public IReadOnlyList<string>? ColumnKeys { get; init; }   // null = all visible
    public string? Filename { get; init; }                     // null = auto-generated
    public bool IncludeHeader { get; init; } = true;
    public bool IncludeAggregateFooter { get; init; } = false;
    public bool IncludeBom { get; init; } = true;              // CSV only
    public IReadOnlyDictionary<string, object?>? FormatOptions { get; init; }  // format-specific
}
```

Used by consuming pages that drive export from their own UI (e.g., a custom "Export Q3 Report" button in a page header).

---

## §17.3 — Export scope

Per Q8.2 (locked) — caller picks scope; default = Filtered.

### §17.3.1 — `ExportScope` enum

```csharp
public enum ExportScope
{
    View,         // current view: filtered + sorted + paged (what's on screen)
    Filtered,     // filtered + sorted, all pages — default
    All,          // all underlying data, no filters
    Selected      // only rows in SelectedItems / SelectedKeys
}
```

### §17.3.2 — Scope semantics

| Scope | Client-side | Server-side |
|---|---|---|
| `View` | Filtered + sorted, current page only | Send current `TableQueryRequest` (page included), use returned rows |
| `Filtered` | Filtered + sorted, all pages (in-memory list) | Send request with `PageSize = int.MaxValue` (or `ServerSideAllCap` for safety), filtered as configured |
| `All` | All `Items` (filters bypassed) | Send request with empty `Filters` and empty `QuickSearch`; `PageSize = int.MaxValue` (capped) |
| `Selected` | Filter `Items` to `SelectedKeys` (in-memory) | Send `SelectedKeys` to the server; server filters to those rows |

### §17.3.3 — `DefaultExportScope` parameter

```csharp
[Parameter] public ExportScope DefaultExportScope { get; set; } = ExportScope.Filtered;
```

Determines what scope direct-click export uses (per §17.2.2). The "More options" modal still lets the user override.

### §17.3.4 — Server-side `All` scope cap

Just like the "All" page size (§8.2.2), `ExportScope.All` in server-side mode is capped at `ServerSideAllCap` (default 1000). If the user picks "All data" in the export modal and there are 47,000 rows in the underlying dataset, the export returns only the first 1000 with a banner in the resulting file:

```csv
# Export limited to 1000 of 47381 total rows.
# Narrow your filters and re-export to capture the rest.
Name,Mobile,Status,...
```

For larger exports, the consuming page must implement chunked export server-side and orchestrate it externally (LipiTable's built-in export doesn't handle multi-request chunks in v1.0).

### §17.3.5 — `Selected` scope when nothing is selected

If the user picks "Selected rows only" in the modal but no rows are selected, the Export button in the modal is disabled with a tooltip: "Select at least one row to export selected only."

In the direct-click flow, `Selected` is only available as `DefaultExportScope` if the consuming page wants — if `DefaultExportScope="Selected"` and no rows are selected, direct-click falls back to `Filtered` with a console warning.

---

## §17.4 — Column selection

### §17.4.1 — Default included columns

By default, every column with `IncludeInExport="true"` (the default) and `Visible="true"` is exported. Actions columns default to `IncludeInExport="false"` (no useful export value).

### §17.4.2 — Per-column override

```csharp
[Parameter] public bool IncludeInExport { get; set; } = true;
```

```razor
<LipiColumn Field="@(p => p.Name)" IncludeInExport="true" />
<LipiColumn Field="@(p => p.InternalId)" IncludeInExport="false" />
<LipiColumn Type="ColumnType.Actions" IncludeInExport="false" />   <!-- default false for Actions -->
```

When `IncludeInExport="false"`, the column:
- Does not appear in the column-selection list in the Export Options modal
- Is not exported via direct-click
- Can still be force-included via programmatic `ExportAsync` with explicit `ColumnKeys`

### §17.4.3 — Modal column selection

In the "More options" modal, the user can toggle individual columns for THIS export (one-off override). The selection doesn't persist — next export starts from `IncludeInExport` defaults again.

For "always export this column" configuration, the caller sets `IncludeInExport="true"` permanently.

### §17.4.4 — Export value vs display value

For most column types, the exported value is the **raw underlying value** (from `Field` / `ValueSelector`), not the rendered display. This matters for:

| Column type | Display | Export value |
|---|---|---|
| Date | "15/05/2026" (formatted) | `2026-05-15` (ISO 8601 in CSV) |
| Currency | "₹1,500.00" | `1500.00` (raw decimal, CSV) |
| Status | LipiBadge | The status string |
| Boolean | ✓ / em-dash | `true` / `false` |
| Avatar | Image | URL or initials text |
| Link | Hyperlinked text | Link text (CSV); link text with URL (PDF / Print) |
| File | Icon + name + size | File name + size string |

Exception: PDF and Print export typically use the **display** values (formatted dates, currency symbols, badges as text) for readability. CSV uses raw values for data interoperability.

The convention:
- **CSV** → raw values, ISO 8601 dates, decimal numbers, true/false booleans
- **PDF** → display values, formatted per culture
- **Print** → display values via the print stylesheet (same as PDF)

Override via column-level `ExportValueSelector`:

```csharp
[Parameter] public Func<TItem, object?>? ExportValueSelector { get; set; }
```

```razor
<LipiColumn Field="@(p => p.DateOfBirth)" 
            Type="ColumnType.Date"
            ExportValueSelector="@(p => p.DateOfBirth?.ToString("yyyy-MM-dd"))" />
```

When set, the export uses this value regardless of format.

### §17.4.5 — Custom column export template (PDF / Print only)

For PDF and Print, the rendered display can use a custom template:

```razor
<LipiColumn Field="@(p => p.Status)">
    <CellTemplate Context="p"><LipiBadge>@p.Status</LipiBadge></CellTemplate>
    <ExportCellTemplate Context="p">@p.Status (verified)</ExportCellTemplate>
</LipiColumn>
```

`<ExportCellTemplate>` is rendered when generating PDF / Print, replacing the cell's normal `<CellTemplate>` (which often uses LipiTable-specific components not suitable for export).

CSV ignores `<ExportCellTemplate>` — it can't render RenderFragments to text. CSV always uses raw values or `ExportValueSelector`.

---

## §17.5 — CSV export

### §17.5.1 — Format

CSV per RFC 4180:
- UTF-8 encoded
- Optional UTF-8 BOM (default `true` for Excel compatibility)
- Comma separator (configurable via `CsvSeparator` parameter, default `,`)
- Double-quote (`"`) wraps fields containing the separator, double-quote, or newline
- Embedded double-quotes escaped as `""`
- CRLF line endings (`\r\n`)

Header row included by default; skip via `IncludeHeader="false"` in `ExportOptions`.

### §17.5.2 — Value formatting

| TValue / type | CSV representation |
|---|---|
| `string` | Raw text, escaped per RFC 4180 |
| `int` / `long` / `decimal` / `double` / `float` | Raw numeric (no thousands separator, period decimal) |
| `bool` | `true` / `false` |
| `DateOnly` / `DateTime.Date` | `yyyy-MM-dd` |
| `DateTime` / `DateTimeOffset` | ISO 8601 (`yyyy-MM-ddTHH:mm:ss.fffffffzzz`) |
| `TimeOnly` / `TimeSpan` | `HH:mm:ss` |
| `null` | Empty cell (no value between separators) |
| Enum | Enum name (string representation) |
| Other `IFormattable` | `value.ToString(null, CultureInfo.InvariantCulture)` |
| Other object | `value.ToString() ?? ""` (with escape) |

All numeric and date formatting uses **InvariantCulture** by default — produces data that parses consistently across locales. Override via `CsvCulture` parameter on the table if a specific culture is needed.

### §17.5.3 — Aggregate footer row

When `IncludeAggregateFooter="true"` AND the table has aggregate columns, an extra row appears below the data:

```csv
Name,Mobile,LastVisit,LineTotal
"Dr. Reddy",9876543210,2026-05-10,12500.00
"Dr. Patel",9876512345,2026-05-12,8500.00
"Dr. Sharma",9876598765,2026-05-14,15000.00
,,,Total: 36000.00
```

The aggregate values render per `AggregateFormat` if set, falling back to invariant-culture formatting otherwise. The label (e.g., "Total:") prefixes the value in the last aggregate column.

### §17.5.4 — Filename auto-generation

Default filename: `{TableTitle-or-TableId}_{yyyy-MM-dd}_{HH-mm-ss}.csv`

Examples:
- Title="Patients" → `Patients_2026-05-15_14-30-00.csv`
- TableId="lab-results" (no Title) → `lab-results_2026-05-15_14-30-00.csv`
- Neither set → `table-export_2026-05-15_14-30-00.csv`

Override via `ExportOptions.Filename`:

```csharp
await _table!.ExportAsync(new ExportOptions
{
    Format = ExportFormat.Csv,
    Filename = "patients-q2-report.csv"
});
```

### §17.5.5 — Download mechanism

CSV is generated as a string, encoded to bytes, and delivered to the browser via:
1. Server-side render-and-stream: the consuming page can hook `OnBeforeExport` to write to a server-side stream and return a URL
2. Default: client-side JS interop creates a Blob, an anchor tag with `download` attribute, and clicks it to trigger browser download

Default works without any server-side code. Server-side streaming is for large exports (e.g., 50,000 rows where keeping the full string in client memory is risky).

---

## §17.6 — PDF export (stubbed for Phase 2.8)

### §17.6.1 — Status

PDF export is fully specified at the API level. The actual rendering pathway is **stubbed in Phase 2.8** — clicking PDF (after Phase 2.10 wires the library) will produce a real PDF; clicking it before then throws:

```
LiPi PDF library not yet integrated. Expected in Phase 2.10.
```

The StyleGuide marks PDF demos as "Available after Phase 2.10."

### §17.6.2 — Specification (for when library lands)

When the LiPi PDF library is integrated:

- **Page size**: A4 default; configurable via `PdfPageSize` parameter (`A4`, `Letter`, `Legal`, etc.)
- **Orientation**: auto-detected based on column count (>6 columns → landscape; ≤6 → portrait). Override via `PdfOrientation`.
- **Header**: title at top (`Title` parameter or filename), timestamp at top-right
- **Footer**: page number + total pages, "Generated by LiPi" attribution
- **Header row**: repeats on every page (table header sticks across page breaks)
- **Cell rendering**: uses display values (formatted dates, currency symbols, badges as text labels)
- **Status badges**: rendered as colored text labels (e.g., `Active` in green) — uses LipiBadge's text + color, not the visual pill (PDF text-rendering is cleaner)
- **Row striping**: optional `PdfStripedRows="true"` for alternate-row backgrounds (improves readability for wide tables)

### §17.6.3 — Aggregate footer row (PDF)

When `IncludeAggregateFooter="true"`, the aggregate row renders at the bottom of the data, with heavier top border to separate from data rows. Aggregate values formatted per column.

### §17.6.4 — Multi-page handling

Large datasets paginate across PDF pages. Header row repeats on each page. Page numbers in footer.

For datasets exceeding 500 rows, the export may take several seconds — progress modal (§17.8) renders during generation.

### §17.6.5 — Custom PDF cell template

`<ExportCellTemplate>` (per §17.4.5) renders for PDF. If not provided, the standard `<CellTemplate>` is invoked and converted to text (limited support: HTML tags stripped, text content extracted). For best results in PDF, provide explicit `<ExportCellTemplate>` for columns with complex `<CellTemplate>`.

### §17.6.6 — Filename

Same auto-generation as CSV with `.pdf` extension.

---

## §17.7 — Print

### §17.7.1 — Print stylesheet

A dedicated stylesheet, `lipi-table-print.css`, applies under `@media print`. It transforms the table's appearance for paper output:

- Toolbar, filter chips, bulk action bar, pagination, group bar — **hidden**
- Selection column, expand chevron column, actions column — **hidden**
- Row selection background, hover background — **removed** (white backgrounds throughout)
- Row striping (if enabled) — **preserved** in light grey only
- Status badges — **rendered as colored text labels** (similar to PDF — better readability than pill backgrounds on paper)
- Borders — **simplified** to 0.5px grey on all cells
- Header row — repeats on each printed page via CSS `position: running()` and `@top-center` (modern browsers only — Chrome / Edge / Safari)
- Font sizes — **adjusted** for print legibility (slight increase from screen sizes)

### §17.7.2 — Print trigger

Clicking "Print" in the export dropdown:
1. Adds the `lipi-table-print-active` class to the table root (allows the print stylesheet to scope to the active print)
2. Calls `window.print()` via JS interop
3. Browser's native print dialog opens
4. User picks printer / Save as PDF / etc.
5. After the dialog closes (or print completes), the `print-active` class is removed

This produces a print-styled output without any LipiTable code generating actual paper rendering — the browser does that work.

### §17.7.3 — Print scope

Print uses the same `ExportScope` semantics as CSV / PDF:
- `View` — current page's rendered DOM (the user's eyes already see this)
- `Filtered` — all filtered data; LipiTable temporarily expands its body to render all matching rows, then triggers print, then collapses
- `All` — all data; same expand-print-collapse
- `Selected` — only selected rows; LipiTable filters the body to selected rows before printing

For `Filtered` / `All` / `Selected` scopes, virtualization (§20) is temporarily disabled so all rows are in the DOM during the print operation. Re-enabled after.

### §17.7.4 — Page break control

CSS `@media print` rules:
- Avoid breaking rows across pages (`page-break-inside: avoid` on row elements)
- Avoid breaking groups across pages (when grouping is active)
- Repeat header on each page

For long tables, page breaks happen between rows naturally. Group headers can split from their first member in extreme cases; LipiTable mitigates via `page-break-after: avoid` on group headers.

### §17.7.5 — Print options

The "More options" modal exposes print-specific options:
- `IncludeHeader` (default true)
- `IncludeAggregateFooter` (default false)
- `PrintOrientation` — Portrait / Landscape / Auto (default Auto, detected per column count)
- `PrintFontScale` — 0.8 to 1.2 (default 1.0, useful for fitting wide tables)

These are applied via temporary CSS variables set on the table root before `window.print()` is invoked.

---

## §17.8 — Async export with progress

### §17.8.1 — Progress modal

For exports that take noticeable time (large datasets, PDF generation, server-side fetch), LipiTable shows a progress modal:

```
┌─ Exporting... ───────────────────────────┐
│                                           │
│ Preparing CSV file                        │
│ ████████████░░░░░░░  62%                  │
│ 3100 of 5000 rows                         │
│                                           │
│         [ Cancel ]                        │
└───────────────────────────────────────────┘
```

Modal appears when an export operation exceeds 500ms — quick exports complete without the modal interrupting.

### §17.8.2 — Progress reporting

For server-side exports, the consuming page can report progress via:

```csharp
[Parameter] public EventCallback<ExportProgress> OnExportProgress { get; set; }
```

```csharp
public sealed record ExportProgress(
    int RowsProcessed,
    int? TotalRows,
    string? Stage,                // "Fetching data" / "Generating CSV" / etc.
    double Percentage             // 0.0 to 1.0; -1 if indeterminate
);
```

LipiTable updates the modal as progress events fire. If progress isn't reported (caller doesn't fire events), the modal shows an indeterminate spinner with the current row count.

### §17.8.3 — Cancellation

The "Cancel" button in the progress modal calls `CancellationTokenSource.Cancel()` on the export operation. The consuming page's export handler is expected to honor the token:

```csharp
private async Task HandleExportAsync(ExportContext ctx, CancellationToken ct)
{
    foreach (var row in await _service.QueryAllAsync(ct))
    {
        ct.ThrowIfCancellationRequested();
        // ... write row to CSV ...
    }
}
```

If the export completes cancellation gracefully, the modal closes and a toast notification confirms "Export cancelled." If the export was already mostly done, the partial output is discarded.

### §17.8.4 — Non-blocking

The export modal doesn't block other table interactions. The user can keep navigating, filtering, sorting while the export runs in the background. If the user changes filter state mid-export, the export continues with the original `ExportOptions` (snapshot at start) — not the live state.

This is intentional: a partway-through export shouldn't suddenly produce different data because the user clicked something else.

---

## §17.9 — Export events

```csharp
public sealed record BeforeExportContext(
    ExportFormat Format,
    ExportScope Scope,
    IReadOnlyList<string> ColumnKeys,
    ExportOptions Options
);

public sealed record AfterExportContext(
    ExportFormat Format,
    int RowsExported,
    long BytesGenerated,
    TimeSpan Duration,
    bool Succeeded,
    string? ErrorMessage
);

[Parameter] public EventCallback<BeforeExportContext> OnBeforeExport { get; set; }
[Parameter] public EventCallback<AfterExportContext> OnAfterExport { get; set; }
```

`OnBeforeExport` fires after the user confirms (modal OK or direct click) but BEFORE row generation starts. Caller can:
- Cancel the export by setting `BeforeExportContext.Cancel = true` (mutable property added to the record)
- Modify the export options (mutable property `Options`)
- Trigger side effects (audit log, analytics)

`OnAfterExport` fires after the export completes (or fails). Caller can:
- Show a toast notification ("Exported 247 rows successfully")
- Log to audit trail (especially important for PHI exports per HIPAA)
- Trigger downstream workflows

### §17.9.1 — HIPAA audit hook

Per the LiPi standing rule on HIPAA-grade audit, every export of PHI must be logged. Consuming pages wire this in `OnAfterExport`:

```csharp
private async Task HandleAfterExportAsync(AfterExportContext ctx)
{
    if (ctx.Succeeded)
    {
        await _audit.RecordAsync(new AuditEvent
        {
            Action = AuditActions.PhiExport,
            ResourceType = "Patient",
            ResourceCount = ctx.RowsExported,
            Format = ctx.Format.ToString(),
            BytesGenerated = ctx.BytesGenerated,
            Duration = ctx.Duration
        });
    }
}
```

LipiTable itself doesn't decide what's PHI — that's the consuming page's call. LipiTable provides the hook.

---

## §17.10 — Disabling export

| Setting | Effect |
|---|---|
| `ShowExportButton="false"` (default) | Toolbar button hidden; `ExportAsync` programmatic still works |
| `AllowExport="false"` | Toolbar button hidden AND programmatic export throws |
| Column-level `IncludeInExport="false"` | That column excluded from all exports |

`AllowExport="false"` is a hard lock — useful for compliance scenarios where export must be completely disabled (e.g., kiosk mode, audit-restricted views).

---

## §17.11 — Worked examples

### §17.11.1 — Simple export with CSV default

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           Title="Patients"
           ShowExportButton="true">
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Mobile)" />
    <LipiColumn Field="@(p => p.LastVisit)" Type="ColumnType.Date" />
</LipiTable>
```

Toolbar shows "⤓ Export ▾" button. User clicks → dropdown → CSV. File `Patients_2026-05-15_14-30-00.csv` downloads.

### §17.11.2 — Export with HIPAA audit logging

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           Title="Patient List"
           ShowExportButton="true"
           OnBeforeExport="@HandleBeforeExportAsync"
           OnAfterExport="@HandleAfterExportAsync">
    <LipiColumn Field="@(p => p.Uhid)" />
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.DateOfBirth)" Type="ColumnType.Date" />
    <LipiColumn Field="@(p => p.Mobile)" />
</LipiTable>

@code {
    private async Task HandleBeforeExportAsync(BeforeExportContext ctx)
    {
        // Could deny export based on role / context
        if (!_auth.CurrentUser.CanExportPatientData)
        {
            ctx.Cancel = true;
            await _toast.ShowAsync("You don't have permission to export patient data", ToastVariant.Error);
        }
    }

    private async Task HandleAfterExportAsync(AfterExportContext ctx)
    {
        await _audit.RecordPhiExportAsync(
            user: _auth.CurrentUser.Id,
            resource: "Patient",
            count: ctx.RowsExported,
            format: ctx.Format.ToString());
    }
}
```

Pre-flight permission check via `OnBeforeExport`, audit logging via `OnAfterExport`.

### §17.11.3 — Programmatic export from external button

```razor
<div class="page-actions">
    <LipiButton OnClick="@ExportQ2Report" Variant="ButtonVariant.Secondary">
        Export Q2 report
    </LipiButton>
</div>

<LipiTable @ref="_table" TItem="Invoice" ...>
    ...
</LipiTable>

@code {
    private LipiTable<Invoice>? _table;

    private async Task ExportQ2Report()
    {
        await _table!.ExportAsync(new ExportOptions
        {
            Format = ExportFormat.Csv,
            Scope = ExportScope.Filtered,
            Filename = $"Q2_Report_{DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}.csv",
            IncludeAggregateFooter = true,
            IncludeBom = true
        });
    }
}
```

External button drives a specific named export. Aggregate footer included for accounting use.

### §17.11.4 — Custom export value selector

```razor
<LipiColumn Field="@(p => p.Status)"
            Type="ColumnType.Status"
            ExportValueSelector="@(p => $"{p.Status} (verified by {p.VerifiedBy ?? "—"})")" />
```

Custom export value embeds verifier name. CSV gets the full string; the displayed badge stays simple.

---

*End of §17. Proceed to §9 — Grouping (back-fill of deferred section, addresses §4 distinct-values gap).*


# LipiTable Spec — §18 Empty, loading, error states

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2 (body region states), §4 (data sources — load states), §7 (filtering — filtered-empty distinction), §24 (tokens)
**Cross-references:** `04-LipiEmptyState-Spec.md` (the composed component)

---

## §18.1 — Five body states

The body region (region ⑦ per §2.1) renders one of five mutually exclusive states. Priority from highest to lowest:

```
1. Loading      ── data fetch in progress (server-side) or IsLoading="true" (client-side)
2. Error        ── data fetch failed or LoadError parameter is set
3. FilteredEmpty── data exists but current filters yield zero rows
4. Empty        ── no data exists at all
5. Normal       ── at least one row to render
```

The priority means: if the data is loading AND a previous error exists, Loading wins. If the data is loaded AND filters are active AND no rows match, FilteredEmpty wins over Empty.

### §18.1.1 — Determining the active state

```
Active state = first match from priority list

(IsLoading == true OR DataSource is in-flight) → Loading
(LoadError != null OR DataSource threw)         → Error
(Items.Count > 0 OR TotalCount > 0) AND filtered count == 0 → FilteredEmpty
(Items.Count == 0 OR TotalCount == 0) AND no filters active  → Empty
otherwise                                                     → Normal
```

The "filtered count" check uses LipiTable's understanding of which rows would render given current filters. In server-side mode, this is `TableQueryResponse.Rows.Count` AND total = 0 with active filters; in client-side mode, it's the in-memory filtered count.

### §18.1.2 — State transitions

States change reactively as data and parameters change:

- User types in quick search → if no rows match, body transitions to FilteredEmpty
- User clears all filters → if data exists, body transitions to Normal
- Server-side fetch starts → body transitions to Loading
- Server-side fetch fails → body transitions to Error
- Programmatic `Items = Array.Empty<TItem>()` → body transitions to Empty

Each transition is rendered without animation by default. To animate state changes (200ms cross-fade):

```csharp
[Parameter] public bool AnimateStateChanges { get; set; } = false;
```

Default off — state changes happen during data fetch, where the user is expecting the change. Animation adds latency to perception without functional benefit.

---

## §18.2 — Normal state

The default state when rows exist. Each row renders per §3 column model. This section covers the four non-normal states.

---

## §18.3 — Loading state

### §18.3.1 — When it shows

In server-side mode, automatically when `DataSource` callback is in-flight (fired on mount, sort change, filter change, page change, refresh).

In client-side mode, manually when the caller sets `IsLoading="true"`:

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           IsLoading="@_isLoading"
           KeySelector="@(p => p.Id)">
    ...
</LipiTable>

@code {
    private bool _isLoading;
    private List<Patient> _patients = new();
    
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _patients = await _patientService.LoadAsync();
        _isLoading = false;
    }
}
```

### §18.3.2 — Default loading UI

Per the locked principle (§1.3 Principle 6 — performance is a feature): the loading state should be **structural**, not just a spinner. LipiTable renders skeleton rows that mimic the table's layout:

```
┌────┬────────────────────┬──────────┬────────────┐
│ ☐  │ ▓▓▓▓▓▓▓▓▓▓▓        │ ▓▓▓▓▓▓   │ ▓▓▓▓▓▓▓▓▓  │   ← skeleton row 1
│ ☐  │ ▓▓▓▓▓▓▓▓▓▓         │ ▓▓▓▓▓▓▓  │ ▓▓▓▓▓▓     │   ← skeleton row 2
│ ☐  │ ▓▓▓▓▓▓▓▓           │ ▓▓▓▓     │ ▓▓▓▓▓▓▓▓▓▓ │   ← skeleton row 3
│ ☐  │ ▓▓▓▓▓▓▓▓▓▓▓▓       │ ▓▓▓▓▓    │ ▓▓▓▓▓▓▓    │   ← skeleton row 4
│ ☐  │ ▓▓▓▓▓▓▓▓▓          │ ▓▓▓▓▓▓   │ ▓▓▓▓▓▓▓▓   │   ← skeleton row 5
└────┴────────────────────┴──────────┴────────────┘
```

Implementation:
- Each skeleton row uses `LipiSkeleton` (Phase 2.7) per cell
- Cell type determines skeleton shape: Text/Mono → Line; Avatar → Circle; Status → Rect (small)
- Cell width matches column width
- Number of skeleton rows = previous page's row count (so the layout "stays the same size") OR 8 if no previous data
- Shimmer animation runs (per LipiSkeleton spec)

Header + toolbar + footer chrome remain visible during loading. Only the body content swaps to skeletons.

### §18.3.3 — Loading overlay alternative

For tables where preserving the previous page's data behind the loading indicator is preferred:

```csharp
[Parameter] public bool ShowLoadingOverlayDuringFetch { get; set; } = false;
```

When `true`:
- Previous data stays rendered
- A semi-transparent overlay covers the body (`--color-surface-primary` at 80% opacity)
- A centered `LipiSpinner` (Phase 2.7) shows progress

Use case: "show me the previous page's data while the next page loads" — useful when scrolling through pages where most data is stable and only a few rows change.

Default `false` (skeleton replaces previous data). The skeleton pattern is more honest — the user sees the data is being refreshed, not just decorated.

### §18.3.4 — Initial load vs subsequent load

By default, the first load on mount uses the skeleton state; subsequent re-fetches (sort change, filter change, page change) also use skeleton.

To differentiate:

```csharp
[Parameter] public LoadingStrategy LoadingStrategy { get; set; } = LoadingStrategy.AlwaysSkeleton;

public enum LoadingStrategy
{
    AlwaysSkeleton,         // default — skeleton on every load
    SkeletonOnInitial,      // skeleton only for first mount; overlay for subsequent
    OverlayAlways           // overlay for every load
}
```

`SkeletonOnInitial` is a reasonable middle-ground for power-user surfaces: first load shows skeleton structure; subsequent loads preserve context with overlay.

### §18.3.5 — `<LoadingTemplate>` slot

Full caller override:

```razor
<LipiTable ...>
    <LoadingTemplate>
        <div class="custom-loader">
            <LipiSpinner Size="LipiSize.Large" />
            <p>Fetching patient records from the secure server...</p>
        </div>
    </LoadingTemplate>
</LipiTable>
```

When `<LoadingTemplate>` is set, it replaces the default skeleton rendering. The template renders in the body region (region ⑦), centered by default.

Caller-supplied templates should be **fast** to render — they show during loading, so they shouldn't trigger additional async work.

---

## §18.4 — Error state

### §18.4.1 — When it shows

In server-side mode, automatically when `DataSource` callback throws an exception (caught by LipiTable).

In any mode, manually when caller sets `LoadError`:

```csharp
[Parameter] public string? LoadError { get; set; }
```

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           LoadError="@_lastError"
           KeySelector="@(p => p.Id)">
    ...
</LipiTable>

@code {
    private string? _lastError;
    
    private async Task LoadAsync()
    {
        try
        {
            _patients = await _service.LoadAsync();
            _lastError = null;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
        }
    }
}
```

### §18.4.2 — Default error UI

The body renders an `LipiEmptyState` (Phase 2.8 sibling) with the Error variant:

```
                  ┌────────────────────────────────┐
                  │                                 │
                  │         [Lucide alert-octagon] │
                  │                                 │
                  │   Couldn't load data            │
                  │                                 │
                  │   {LoadError or exception msg}  │
                  │                                 │
                  │         [Retry]                 │
                  │                                 │
                  └────────────────────────────────┘
```

- Icon: Lucide `alert-octagon` (danger color)
- Title: "Couldn't load data" (customizable via `ErrorTitle` parameter)
- Body: the error message (from `LoadError` or exception's `Message`)
- Retry button: `LipiButton` primary; clicking re-invokes `DataSource` (server-side) or `OnRetry` callback (client-side)

Header + toolbar remain visible. Footer pagination stays visible but disabled (last known values).

### §18.4.3 — Previous data behind error

By default, when an error occurs, previous data remains visible behind the error overlay. This lets the user retain context while seeing the error.

To replace data entirely with the error UI:

```csharp
[Parameter] public bool ClearOnError { get; set; } = false;
```

`true` removes previous data; error fills the entire body. Useful for cases where the previous data is no longer trustworthy (e.g., a stale page that should be hidden).

### §18.4.4 — Retry behavior

Clicking Retry:
- Server-side: re-invokes `DataSource` with the current `TableQueryRequest`
- Client-side with `OnRetry` callback: calls the callback
- Client-side without `OnRetry`: re-renders with current `Items` (essentially a no-op refresh)

No automatic retry / exponential backoff is built in. If the consuming page wants retry logic, they implement it in the `DataSource` handler.

### §18.4.5 — `<ErrorTemplate>` slot

Full caller override with context:

```razor
<LipiTable ...>
    <ErrorTemplate Context="errorCtx">
        <div class="custom-error">
            <LipiIcon Name="alert-triangle" Size="LipiSize.Large" />
            <h3>Patient data unavailable</h3>
            <p>@errorCtx.ErrorMessage</p>
            <div class="error-actions">
                <LipiButton OnClick="@errorCtx.RetryAsync">Retry</LipiButton>
                <LipiButton OnClick="@(() => Nav.NavigateTo("/dashboard"))" Variant="ButtonVariant.Secondary">
                    Return to dashboard
                </LipiButton>
            </div>
        </div>
    </ErrorTemplate>
</LipiTable>
```

`ErrorContext` exposes:

```csharp
public sealed class ErrorContext
{
    public string ErrorMessage { get; }
    public Exception? Exception { get; }      // null if error came from LoadError parameter, not exception
    public Task RetryAsync();
}
```

### §18.4.6 — Error events

```csharp
public sealed record TableErrorContext(
    string Message,
    Exception? Exception,
    TableErrorSource Source
);

public enum TableErrorSource
{
    DataSourceException,
    LoadErrorParameter,
    OnRowSaveException,
    OnBeforeExportException,
    GenericInternal
}

[Parameter] public EventCallback<TableErrorContext> OnError { get; set; }
```

Fires whenever LipiTable encounters an error. Consuming pages use this for:
- Application-level error logging
- Toast notifications ("Couldn't load data — please retry")
- Analytics

LipiTable doesn't show toasts on errors automatically — the error state UI is the primary surfacing.

---

## §18.5 — Empty state

### §18.5.1 — When it shows

When the dataset has zero rows AND no filters are active:

- Client-side: `Items.Count == 0` (or `Items == null`)
- Server-side: `TableQueryResponse.TotalCount == 0` (or response.Rows empty AND TotalCount unspecified) AND `TableQueryRequest.Filters` and `QuickSearch` are both empty

If filters ARE active and zero rows match, the state is FilteredEmpty (§18.6), not Empty. The distinction matters because the UX differs — Empty means "no data exists, you might want to create some," while FilteredEmpty means "your filters are too restrictive, adjust them."

### §18.5.2 — Default empty UI

Renders an `LipiEmptyState` with the Empty variant:

```
                  ┌────────────────────────────────┐
                  │                                 │
                  │         [Lucide inbox]          │
                  │                                 │
                  │   No patients yet               │
                  │                                 │
                  │   Get started by adding         │
                  │   the first patient.            │
                  │                                 │
                  │       [+ Add Patient]           │
                  │                                 │
                  └────────────────────────────────┘
```

- Icon: Lucide `inbox` (muted color)
- Title: "No data" (customizable via `EmptyTitle` parameter)
- Body: "There's nothing here yet" (customizable via `EmptyBody`)
- CTA button: appears only when `ShowAddButton="true"` AND `EmptyShowAddCta="true"` (default true)

The empty-state CTA mirrors the toolbar's Add button — same handler, same label. Users get the same affordance regardless of which they click.

### §18.5.3 — Customizing the default empty state

Without a full template override, the caller can customize the default via parameters:

```razor
<LipiTable EmptyTitle="No patients yet"
           EmptyBody="Get started by adding the first patient or import from CSV."
           EmptyIcon="users"
           EmptyShowAddCta="true"
           AddButtonLabel="+ Add Patient"
           OnAddClick="@OpenAddModal">
    ...
</LipiTable>
```

The icon name is a Lucide icon identifier. Defaults to `inbox`.

### §18.5.4 — `<EmptyTemplate>` slot

Full override:

```razor
<LipiTable ...>
    <EmptyTemplate>
        <div class="custom-empty-state">
            <img src="/images/empty-clinic.svg" alt="" />
            <h2>Your clinic isn't set up yet</h2>
            <p>Run the onboarding wizard to populate sample patients and clinic settings.</p>
            <LipiButton OnClick="@StartOnboarding">Start onboarding</LipiButton>
        </div>
    </EmptyTemplate>
</LipiTable>
```

Replaces the default empty UI entirely. Caller has full control over icon (or no icon), text, layout, and actions.

---

## §18.6 — Filtered-empty state

### §18.6.1 — When it shows

When data exists but current filters yield zero rows:

- Client-side: `Items.Count > 0` AND filtered result is empty
- Server-side: `TableQueryResponse.TotalCount > 0` AND `Rows.Count == 0` (the response has zero rows for current page, but the dataset has rows when ignoring filters)

The detection rule: if filtering away would reveal rows, the state is FilteredEmpty; if nothing exists regardless, it's Empty.

### §18.6.2 — Default filtered-empty UI

Renders an `LipiEmptyState` with the FilteredEmpty variant:

```
                  ┌────────────────────────────────┐
                  │                                 │
                  │     [Lucide search-x]           │
                  │                                 │
                  │   No matching patients          │
                  │                                 │
                  │   Try adjusting your filters    │
                  │   or clear them to see all.     │
                  │                                 │
                  │       [Clear filters]           │
                  │                                 │
                  └────────────────────────────────┘
```

- Icon: Lucide `search-x` (muted color)
- Title: "No matches found" (customizable via `FilteredEmptyTitle`)
- Body: "Try adjusting your filters" (customizable via `FilteredEmptyBody`)
- "Clear filters" button: appears by default; clicking calls `ClearAllFiltersAsync()`

The "Clear filters" CTA is the primary affordance — the user's most likely next action is clearing the over-restrictive filters.

### §18.6.3 — Customizing

```razor
<LipiTable FilteredEmptyTitle="No results"
           FilteredEmptyBody="No patients match those criteria. Try a different name or status filter."
           FilteredEmptyIcon="search-x"
           ShowClearFiltersCta="true">
    ...
</LipiTable>
```

`ShowClearFiltersCta="false"` hides the Clear filters button (rare; useful when filters are inherent to the page and can't be cleared).

### §18.6.4 — `<FilteredEmptyTemplate>` slot

```razor
<LipiTable ...>
    <FilteredEmptyTemplate>
        <div class="custom-filtered-empty">
            <p>No results for "<strong>@_currentSearch</strong>".</p>
            <p>Maybe try:</p>
            <ul>
                @foreach (var suggestion in _searchSuggestions)
                {
                    <li>
                        <LipiButton OnClick="@(() => SetSearch(suggestion))">
                            "@suggestion"
                        </LipiButton>
                    </li>
                }
            </ul>
        </div>
    </FilteredEmptyTemplate>
</LipiTable>
```

Useful for search experiences with "did you mean" suggestions.

---

## §18.7 — State priority and edge cases

### §18.7.1 — Loading takes priority over everything

Even if `LoadError` is set, if a load is in-flight, Loading wins. The retry button in the error state, when clicked, transitions through Loading before potentially returning to Error or Normal.

### §18.7.2 — Empty during initial load

On first mount in server-side mode:
1. Body renders Loading state (skeleton)
2. `DataSource` callback returns
3. If response has rows → Normal
4. If response has zero rows AND no filters → Empty
5. If response has zero rows WITH filters → FilteredEmpty
6. If callback threw → Error

The skeleton doesn't appear if `DataSource` returns instantly (synchronous resolution). For very fast servers (cached responses), the body transitions directly from initial render to Normal without a visible skeleton.

### §18.7.3 — Empty after a save

When inline editing deletes a row and the dataset becomes empty:
- If filters are still active → FilteredEmpty
- If no filters → Empty

The transition fires `OnDataStateChanged` (a coarser-grained event than the specific state events) so consuming pages can react if needed.

### §18.7.4 — Error while previous data is visible

When `ClearOnError="false"` (default) and an error occurs:
- Previous data stays in DOM
- Error UI overlays the body region
- Pagination, toolbar, etc. remain visible
- User can dismiss the error (a small ✕ in the corner of the error overlay) to return to viewing stale data

When `ClearOnError="true"`, the previous data is removed; error fills the body fully. No dismissal — only retry returns to data.

### §18.7.5 — Pagination during loading

Cross-reference §8.13.2. Pagination control stays visible but disabled during loading. Row count display shows previous values until response arrives.

### §18.7.6 — Loading during empty / error

A new `RefreshAsync()` call from an Empty or Error state transitions back to Loading. Skeleton shows; previous Empty/Error UI clears.

---

## §18.8 — Bypassing state UI

In rare cases, the consuming page wants to handle Loading / Error / Empty / FilteredEmpty entirely outside LipiTable:

```csharp
[Parameter] public bool HideAllBodyStates { get; set; } = false;
```

When `true`:
- Loading: body renders empty (no skeleton); chrome stays visible
- Error: body renders empty; consuming page must surface the error
- Empty: body renders empty (no empty-state UI)
- FilteredEmpty: body renders empty

The consuming page is responsible for any feedback to the user (banner above the table, toast, sidebar message, etc.).

Use case: a dashboard where the table is one of many panels, and a shared "this dashboard is loading..." banner is shown by the page rather than per-panel. LipiTable's chrome stays useful (toolbar, header) but the body stays clean.

Rare. Default `false` — LipiTable renders meaningful body states.

---

## §18.9 — Persistence and state

None of the body states persist — they're purely runtime. The Loading state happens during fetches; Error stays until retry succeeds or component unmounts; Empty / FilteredEmpty render based on current data.

Filter state itself persists (per §7.12), which means a user can return to a table with restrictive filters in place and see FilteredEmpty immediately. The state is computed from persistent filter state + current data, not persisted itself.

---

## §18.10 — Accessibility

### §18.10.1 — ARIA live regions

State transitions are announced via aria-live:

- Loading: "Loading data, please wait"
- Error: "Failed to load data. {error message}"
- Empty: "No data available"
- FilteredEmpty: "No results match your filters"

The announcements use `aria-live="polite"` (not assertive) — screen readers announce them at the next pause, not interrupting current speech.

### §18.10.2 — Focus management

When the body transitions to Error or FilteredEmpty:
- If focus was inside the body (on a row or cell), focus moves to the CTA button (Retry / Clear filters)
- If focus was outside the body, focus stays where it was

When the body transitions back to Normal:
- Focus on the CTA button moves to the first focusable element in the data (typically the first row's first cell)

This avoids "focus left in nowhere" after state transitions.

### §18.10.3 — Screen reader skeleton announcement

During Loading state, the skeleton rows have `role="status"` and `aria-busy="true"`. Screen readers announce "Loading data" once when entering the state; subsequent renders within the same loading state don't re-announce.

---

## §18.11 — Worked examples

### §18.11.1 — Default behavior (no customization)

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)">
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
</LipiTable>
```

- Loading: skeleton rows during fetch
- Error: default error UI with retry
- Empty: default empty UI ("No data")
- FilteredEmpty: default filtered-empty UI with "Clear filters"

All four states work out of the box with sensible defaults.

### §18.11.2 — Customized empty state with Add CTA

```razor
<LipiTable TItem="Doctor"
           DataSource="@LoadDoctorsAsync"
           KeySelector="@(d => d.Id)"
           ShowAddButton="true"
           AddButtonLabel="+ Add Doctor"
           OnAddClick="@OpenAddDoctorModal"
           EmptyTitle="No doctors yet"
           EmptyBody="Add the first doctor to start managing your clinic's staff."
           EmptyIcon="users"
           EmptyShowAddCta="true">
    <LipiColumn Field="@(d => d.Name)" />
    <LipiColumn Field="@(d => d.Department)" />
</LipiTable>
```

When no doctors exist, the empty state shows a tailored message and a "+ Add Doctor" button that opens the same modal as the toolbar's Add button.

### §18.11.3 — Custom error template with multiple actions

```razor
<LipiTable TItem="LabResult"
           DataSource="@LoadResultsAsync"
           KeySelector="@(r => r.Id)">
    
    <ErrorTemplate Context="errorCtx">
        <div class="lab-error">
            <LipiIcon Name="alert-octagon" Size="LipiSize.Large" />
            <h3>Couldn't load lab results</h3>
            <p>@errorCtx.ErrorMessage</p>
            
            <div class="error-actions">
                <LipiButton OnClick="@errorCtx.RetryAsync">Retry</LipiButton>
                <LipiButton OnClick="@RunDiagnostics" Variant="ButtonVariant.Secondary">
                    Run diagnostics
                </LipiButton>
                <LipiButton OnClick="@ContactSupport" Variant="ButtonVariant.Tertiary">
                    Contact support
                </LipiButton>
            </div>
            
            <details class="error-details">
                <summary>Technical details</summary>
                <pre>@errorCtx.Exception?.ToString()</pre>
            </details>
        </div>
    </ErrorTemplate>
    
    <!-- columns -->
</LipiTable>
```

Lab-results table with rich error UI — retry, run diagnostics, contact support, plus collapsible technical details for IT staff.

### §18.11.4 — Bypassed body states for dashboard

```razor
<div class="dashboard-panel">
    @if (_isDashboardLoading)
    {
        <LipiAlert Variant="info">Loading dashboard data...</LipiAlert>
    }
    
    <LipiTable TItem="Order"
               Items="@_orders"
               KeySelector="@(o => o.Id)"
               HideAllBodyStates="true">
        <LipiColumn Field="@(o => o.Number)" />
        <LipiColumn Field="@(o => o.Status)" Type="ColumnType.Status" />
    </LipiTable>
</div>
```

Page-level loading banner; LipiTable body stays clean (no skeleton). Multiple tables on the dashboard share one loading indicator.

### §18.11.5 — Search experience with suggestions

```razor
<LipiTable TItem="Article"
           Items="@_articles"
           KeySelector="@(a => a.Id)"
           ShowQuickFilter="true">
    <LipiColumn Field="@(a => a.Title)" />
    <LipiColumn Field="@(a => a.Author)" />
    
    <FilteredEmptyTemplate>
        <div class="search-empty">
            <h3>No articles match "@_table?.CurrentQuickSearch"</h3>
            <p>Did you mean:</p>
            <ul>
                @foreach (var suggestion in GetSuggestions(_table?.CurrentQuickSearch))
                {
                    <li>
                        <LipiButton 
                            Variant="ButtonVariant.Tertiary"
                            OnClick="@(() => _table!.SetQuickSearchAsync(suggestion))">
                            @suggestion
                        </LipiButton>
                    </li>
                }
            </ul>
            <LipiButton OnClick="@(() => _table!.ClearAllFiltersAsync())">
                Clear search
            </LipiButton>
        </div>
    </FilteredEmptyTemplate>
</LipiTable>
```

Article search shows "did you mean" suggestions when no results match. Custom filtered-empty template includes suggestions + clear search action.

---

*End of §18. Proceed to §19 — Accessibility.*


# LipiTable Spec — §19 Accessibility

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2 (anatomy), §5 (selection keyboard), §6 (sort keyboard), §10 (tree keyboard), §12 (edit keyboard), §16 (toolbar a11y), §24 (high-contrast tokens)

---

## §19.1 — Compliance targets

Per Principle 5 (§1.3) — every behavior must be reachable by keyboard. Non-negotiable for clinical contexts.

| Target | Mode |
|---|---|
| WCAG 2.1 Level AA | Default — applies to normal LipiTable rendering |
| WCAG 2.1 Level AAA | High-contrast mode (per §24.7) |

WCAG AA highlights:
- Text contrast 4.5:1 (normal), 3:1 (large text / UI components)
- Keyboard reachable for all functionality
- Focus visible (3px ring in our case, configurable)
- No keyboard traps
- Status messages programmatically announced

WCAG AAA highlights (high-contrast mode only):
- Text contrast 7:1 (normal), 4.5:1 (large)
- No content meaning conveyed by color alone

LipiTable's design satisfies AA out of the box. Phase 2.10 audit will verify with a contrast checker across the token palette in both light and dark modes.

---

## §19.2 — ARIA structure

### §19.2.1 — Outer table role

LipiTable does NOT use `role="table"` directly. Instead, it uses `role="grid"` because grid semantics support sort, filter, selection, and editing — table semantics are too restrictive.

```html
<div class="lipi-table"
     role="grid"
     aria-rowcount="487"
     aria-colcount="6"
     aria-label="Patient list"
     aria-multiselectable="true">
    ...
</div>
```

- `aria-rowcount` — total rows across all pages (NOT the visible page count); `-1` when unknown (per §4.3.3)
- `aria-colcount` — total columns including hidden ones; non-hidden count reflected in `aria-colindex` per cell
- `aria-label` — table identification (from `Title` parameter; falls back to "Data table" if not set)
- `aria-multiselectable` — true when `SelectionMode="Multi"`

### §19.2.2 — Rowgroup and rows

```html
<div role="rowgroup">  <!-- header -->
    <div role="row" aria-rowindex="1">
        <div role="columnheader" aria-colindex="1" aria-sort="ascending" aria-label="Name, sortable, currently sorted ascending">Name</div>
        ...
    </div>
</div>

<div role="rowgroup">  <!-- body -->
    <div role="row" aria-rowindex="2" aria-selected="false">
        <div role="gridcell" aria-colindex="1">Dr. Reddy</div>
        ...
    </div>
</div>
```

### §19.2.3 — Selection cells

When `SelectionMode != None`, the selection column's cell:

```html
<div role="gridcell" aria-colindex="1">
    <input type="checkbox" 
           aria-label="Select row, Dr. Reddy"
           aria-checked="false" />
</div>
```

The `aria-label` includes the row's display name for context. Screen readers announce "Select row, Dr. Reddy, checkbox, not checked."

Row's `aria-selected` attribute is set on the `role="row"` element — independent of the checkbox state for clarity.

### §19.2.4 — Expand chevron cells

Master-detail (§11) and tree (§10) expand chevrons:

```html
<div role="gridcell" aria-colindex="2">
    <button type="button" 
            aria-label="Expand details for Dr. Reddy"
            aria-expanded="false"
            aria-controls="lipi-table-detail-{rowkey}">
        <svg>...</svg>  <!-- chevron icon -->
    </button>
</div>
```

The `aria-controls` references the detail-row's `id` attribute (when expanded). Screen readers can navigate from chevron to detail content via the relationship.

### §19.2.5 — Action cells

Action columns (§3.3.2 Actions type):

```html
<div role="gridcell" aria-colindex="6">
    <div role="group" aria-label="Actions for Dr. Reddy">
        <button aria-label="Edit Dr. Reddy">...</button>
        <button aria-label="Delete Dr. Reddy">...</button>
    </div>
</div>
```

Wrapping `role="group"` keeps the action buttons logically grouped under one screen-reader landmark per row.

### §19.2.6 — Row identification

Screen readers need to identify rows by their semantic key, not just position. LipiTable supports this via:

```csharp
[Parameter] public Func<TItem, string>? RowDisplayNameSelector { get; set; }
```

```razor
<LipiTable RowDisplayNameSelector="@(p => $"{p.Name}, UHID {p.Uhid}")" ...>
```

When set, the display name is used in:
- Row's `aria-label`
- Selection checkbox's `aria-label`
- Expand chevron's `aria-label`
- Action buttons' implicit context
- Any aria-live announcements mentioning the row

When NOT set, LipiTable falls back to the first column's value (typically the name / title / identifier).

---

## §19.3 — Keyboard model (consolidated)

Cross-references §5 (selection), §6 (sort), §7 (filter popovers), §10 (tree), §11 (master-detail), §12 (inline edit), §16 (toolbar). Restated here as a single keyboard map.

### §19.3.1 — Navigation (focus inside body)

| Key | Effect |
|---|---|
| Tab | Move focus to next region (out of body → toolbar / footer / outside table) |
| Shift+Tab | Move focus to previous region |
| Arrow Right | Move focus to next cell (within row) |
| Arrow Left | Move focus to previous cell |
| Arrow Down | Move focus to same cell of next row |
| Arrow Up | Move focus to same cell of previous row |
| Home | Move focus to first cell of current row |
| End | Move focus to last cell of current row |
| Ctrl+Home | Move focus to first cell of first row |
| Ctrl+End | Move focus to last cell of last row |
| Page Down | Move focus down by one page (visible rows count) |
| Page Up | Move focus up by one page |
| Ctrl+Page Down | Move to first cell of next page (pagination) |
| Ctrl+Page Up | Move to first cell of previous page |

The arrow-key navigation is **cell-by-cell** (grid model), not row-by-row. Each cell can receive focus independently. This is the standard pattern for `role="grid"`.

### §19.3.2 — Selection (when `SelectionMode != None`)

| Key | Effect |
|---|---|
| Space | Toggle current row's selection; anchor updates |
| Shift+Space | Extend selection from anchor to current row |
| Ctrl/Cmd+Space | Toggle current row's selection without changing anchor |
| Ctrl/Cmd+A | "Select all on page" (equivalent to step 1 of two-step select-all per §5.3.1) |
| Ctrl/Cmd+Shift+A | "Select all across pages" (equivalent to step 2; skips banner) |
| Escape | Clear selection (when count > 0); otherwise context-dependent |
| Shift+Arrow Up/Down | Extend selection by one row in direction |

### §19.3.3 — Sort and filter (when focus on header)

| Key | Effect |
|---|---|
| Enter / Space (on sortable column header) | Cycle sort state |
| Shift+Enter / Shift+Space (on sortable column header) | Add to multi-sort chain |
| Alt+Down (on column header) | Open filter popover for that column |
| Escape (in filter popover) | Close without applying |
| Enter (in filter value input) | Apply filter |
| Shift+F10 (on column header) | Open column context menu (pin / hide / resize / etc.) |

`Shift+F10` is the standard "context menu" shortcut — equivalent to right-click. Build chat should map this to the same menu as right-click for full keyboard parity (per §13.4.3).

### §19.3.4 — Inline edit (cross-reference §12.4)

In nav mode (cell focused, NOT editing):

| Key | Effect |
|---|---|
| Enter / F2 | Enter edit mode on focused cell (Cell mode) OR row (Row mode) |
| Direct typing (alphanumeric) | Replace cell value (Excel-style implicit edit, Cell mode) |
| Escape | Close any open popover; otherwise no-op |

In edit mode:

| Key | Effect |
|---|---|
| Enter | Save current cell, return to nav mode on same cell |
| Ctrl+Enter | Save current cell, move down one row (same column) |
| Tab | Save current cell, move to next editable cell (wraps row) |
| Shift+Tab | Save current cell, move to previous editable cell |
| F2 | Toggle out of edit mode (save) |
| Escape | Cancel edit, revert value |

Row-edit mode: Save / Cancel via the explicit buttons (or Enter on Save button, Escape with discard confirm).

### §19.3.5 — Expand/collapse (tree + master-detail)

| Key | Effect |
|---|---|
| Arrow Right (on collapsed row) | Expand |
| Arrow Left (on expanded row) | Collapse |
| Arrow Left (on collapsed row) | Move focus to parent row (tree only) |
| Enter / Space (on chevron) | Toggle |
| Double-Enter | Toggle (when no `OnRowDoubleClick` handler) |

The expand/collapse model matches Windows Explorer / VS Code / accessible tree widgets. Consistent across tree data (§10) and master-detail (§11).

### §19.3.6 — Toolbar (when focus inside toolbar)

| Key | Effect |
|---|---|
| Tab into toolbar | First focusable control receives focus |
| Arrow Left/Right within toolbar | Navigate within toolbar (skip dividers) |
| Tab out of toolbar | Move to next region |
| Enter / Space | Activate focused control |
| Escape | Close any open popover (column picker, export menu) and return focus to trigger |

---

## §19.4 — Focus management

### §19.4.1 — Focus indicator

All focusable elements show a visible focus ring:

```css
.lipi-table *:focus-visible {
    outline: var(--lipi-table-focus-ring-w) solid var(--color-primary-500);
    outline-offset: 1px;
}
```

- Default ring width: 2px (per `--lipi-table-focus-ring-w`)
- High-contrast mode: 3px (per §24.7)
- Color: primary-500 in light mode, primary-300 in dark mode

`:focus-visible` (not `:focus`) — ring shows only when keyboard-focused, not when mouse-clicked. Standard pattern.

### §19.4.2 — Initial focus on mount

When LipiTable mounts, focus is NOT automatically moved into the table. Focus stays wherever it was before mount (usually outside the table).

To programmatically move focus to the table on mount:

```csharp
public ValueTask FocusFirstCellAsync();
public ValueTask FocusCellAsync(TItem row, string columnKey);
```

Use case: a navigation flow where the previous page's "View" link should land the user on the first row of the destination table.

```razor
@code {
    private LipiTable<Patient>? _table;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _table!.FocusFirstCellAsync();
        }
    }
}
```

### §19.4.3 — Focus during loading / error / empty / filtered-empty

Cross-reference §18.10.2.

- Loading: focus stays where it was; skeleton rows are not focusable
- Error: if focus was inside body, moves to Retry button
- Empty: if focus was inside body, moves to CTA button (if present)
- FilteredEmpty: if focus was inside body, moves to "Clear filters" button
- Returning to Normal: focus stays on the CTA (which is now gone) → re-routes to first cell of first row

### §19.4.4 — Focus during inline edit transitions

- Entering row edit: focus moves to first editable input
- Saving row edit: focus moves back to the row's first cell (or to Save button briefly during the save animation, then back to cell)
- Cancelling row edit (with confirm): focus stays on the Cancel button during confirm; after confirm closes, returns to row's first cell
- Entering cell edit (F2 / Enter): focus stays in the cell, but the cell's contents become editable
- Saving cell edit (Enter): focus stays on cell, content becomes display
- Tab during cell edit: focus moves to next editable cell after saving current cell
- Cancelling cell edit (Escape): focus stays on cell, content reverts

### §19.4.5 — Focus during pagination / sort / filter changes

When the page changes (or sort/filter changes), the **first cell of the first row of the new page** becomes the focus target. If focus was inside the previous page's data, it moves to this new target. If focus was outside the body (in the toolbar, in pagination), it stays where it was.

This prevents "focus left in nowhere" when data changes and the previously-focused row is no longer rendered.

### §19.4.6 — Focus traps

LipiTable does NOT trap focus inside the body. Tab from any cell moves out of the body to the next region (toolbar, footer, next page element).

The exceptions are popovers and modals:
- Filter popover traps focus (Escape or click outside to exit — focus returns to trigger)
- Column picker popover traps focus
- Export modal (`<LipiModal>`) traps focus per LipiModal contract
- Confirmation modals trap focus

These traps are handled by their respective components (LipiModal in Phase 2.6.2 has the focus-trap infrastructure). LipiTable doesn't add its own — it composes.

---

## §19.5 — Screen reader behavior

### §19.5.1 — Initial announcement

When LipiTable receives focus for the first time:

```
"Patient list, data grid, 487 rows by 6 columns"
```

Composition: `aria-label` + role + dimensions. Screen reader pronounces in user's language.

### §19.5.2 — Cell focus announcements

Each cell focus announces the column + value:

```
"Name, column 1 of 6, Dr. Reddy"
```

For special cells:
- Selection checkbox: "Select row, Dr. Reddy, checkbox, not checked"
- Expand chevron: "Expand details for Dr. Reddy, button, collapsed"
- Sortable header: "Name, column 1 of 6, sortable, currently sorted ascending"
- Filterable header with active filter: "Status, column 3 of 6, sortable, filtered"

### §19.5.3 — Sort change announcements

When sort changes:

```
"Sorted by Name, ascending" (single-column)
"Sorted by Name ascending, then Status descending" (multi-column)
"Sort cleared from Name" (when third-state clears)
```

Announced via `aria-live="polite"` region.

### §19.5.4 — Filter change announcements

```
"Filter applied to Name: contains 'reddy'"
"Filter removed from Status"
"3 filters active"  (when multiple changes happen in quick succession)
"All filters cleared"
```

### §19.5.5 — Selection change announcements

```
"Row selected, Dr. Reddy. 1 of 487 selected."
"Row deselected, Dr. Patel. 0 selected."
"3 rows selected on this page. 487 total."  (after select-all-on-page)
"All 487 rows selected across all pages."     (after select-all-across-pages)
"Selection cleared."
```

### §19.5.6 — Edit state announcements

```
"Editing row Dr. Reddy"
"Saving changes"
"Changes saved"  (Success)
"Validation error: Mobile is required"
"Concurrency conflict detected. Choose Reload or Save anyway."
"Edit cancelled, changes discarded"
```

### §19.5.7 — Expand / collapse announcements

```
"Expanded details for Dr. Reddy"
"Collapsed details for Dr. Patel"
```

### §19.5.8 — Loading / error / empty announcements

Cross-reference §18.10.1.

```
"Loading data, please wait"
"Failed to load data. Server temporarily unavailable. Retry button focused."
"No data available"
"No results match your filters"
```

### §19.5.9 — Page change announcements

```
"Page 3 of 20. Showing rows 51 to 75 of 487."
```

### §19.5.10 — Aria-live region implementation

LipiTable maintains a single hidden `aria-live="polite"` region:

```html
<div class="visually-hidden" 
     aria-live="polite" 
     aria-atomic="true"
     role="status">
    {announcement text}
</div>
```

Announcements are debounced 100ms to prevent over-talking when rapid changes happen (e.g., user holding arrow key to navigate cells fires 10 announcements per second without debounce; with debounce, only the final state announces).

For critical errors that should interrupt current speech, an additional `aria-live="assertive"` region exists:

```html
<div class="visually-hidden" 
     aria-live="assertive" 
     aria-atomic="true"
     role="alert">
    {critical announcement}
</div>
```

Used for: validation errors during edit save, concurrency conflicts, critical export failures. Not for routine state changes.

---

## §19.6 — Color and contrast

### §19.6.1 — Meaning beyond color

WCAG AA prohibits relying on color alone to convey meaning. LipiTable's design uses color + secondary signal in every case:

| Element | Color signal | Secondary signal |
|---|---|---|
| Status strip on rows | Status color (via tokens) | `data-status` attribute exposed in DOM |
| Sort indicator | Color shift (faint → solid) | Icon shape change (↕ → ↑ → ↓) |
| Filter active indicator | Color shift (faint → primary) | Filled vs outline filter icon |
| Selected row | Background tint | `aria-selected="true"` + checkbox state |
| Dirty edit row | Subtle yellow tint | "Modified" indicator + ⚠ icon |
| Conflict row | Red banner + tint | Banner text describes conflict |
| Error state | Red icon + text | "Error" word in title + retry button |
| Validation error | Red border + helper text | ⚠ icon + helper text describing error |

Color-blind users (red-green deficiency, ~8% of men, ~0.5% of women) still get full information from the secondary signals.

### §19.6.2 — Contrast ratios

Tested in light mode against `--color-surface-primary` (#FFFFFF baseline):

| Element | Foreground | Contrast |
|---|---|---|
| Primary text | `--color-text-primary` (#1A1D21) | ~16.4:1 ✓ AAA |
| Secondary text | `--color-text-secondary` (#6B7280) | ~4.8:1 ✓ AA |
| Tertiary / faint text | `--color-text-tertiary` (#9CA3AF) | ~3.2:1 ✓ AA large-text only |
| Link text | `--color-link` (#2563EB) | ~6.2:1 ✓ AA |
| Success text | `--color-text-success` (#047857) | ~5.8:1 ✓ AA |
| Warning text | `--color-text-warning` (#B45309) | ~4.6:1 ✓ AA (borderline) |
| Danger text | `--color-text-danger` (#B91C1C) | ~5.9:1 ✓ AA |

Faint text used for unsorted indicators, helper text, placeholders — meets AA large-text (3:1 minimum) but not AA normal-text (4.5:1). This is acceptable per WCAG — these elements are decorative or supplementary; their content is also conveyed by other elements.

High-contrast mode (per §24.7) shifts these tokens to higher-contrast values targeting AAA. The token system supports the mode swap without LipiTable code changes.

Dark mode contrast verified similarly against `--color-surface-primary` dark mode value.

### §19.6.3 — Status strip colors

Status strip uses 4px-wide left border per §24.3.4 and `lipi-status-tokens.css`. The strip is supplementary (the row's content carries the status text). Some users with severe color blindness may not distinguish strip colors; the row text always describes the status.

For tables where status is critical and the row text is dense:

```razor
<LipiColumn Field="@(p => p.Status)" 
            Type="ColumnType.Status"
            StatusVariant="StatusVariant.Pill" />
```

LipiPill renders status with both color AND text, eliminating color-only meaning.

---

## §19.7 — Reduced motion

Per §24.8 — `prefers-reduced-motion: reduce` collapses all transitions to 0ms.

Specific reduced-motion behaviors:
- Skeleton shimmer disabled (renders as flat block)
- Row hover state still applies, but instantly (no fade)
- Filter popover, column picker, export menu appear/disappear instantly
- Bulk action bar appears instantly (no slide-in)
- Master-detail expand: instant (no slide)
- Tree expand: instant
- Concurrency conflict banner: instant
- Density change: instant token swap
- Sort indicator change: instant
- Drag-to-reorder: drag preview still works (direct manipulation, not animation)
- Drag-to-resize: same

Decision tree: any animation purely decorative is disabled. Any motion that provides feedback for direct manipulation (drag, scroll) stays.

---

## §19.8 — Sticky elements and screen readers

Sticky elements (column header, pinned columns, sticky edit bar) can confuse screen readers if not handled properly.

### §19.8.1 — Sticky column header

The column header has `role="rowgroup"` (per §19.2.2). When the user scrolls down, the header stays visible but its DOM position is unchanged — only CSS `position: sticky` keeps it visible.

Screen readers traverse the DOM order, not visual order. The header is announced first regardless of where the user has scrolled. This is correct behavior.

### §19.8.2 — Pinned columns

Pinned columns (§13.4) are visually positioned via `position: sticky` but remain in their DOM order (declared column order). Screen readers navigate cells in the data-row order (not pin-order), which means pinned cells are announced where they were declared, not where they appear visually.

For tables with heavy pinning, this can cause minor confusion ("the leftmost column on screen is pinned, but the screen reader announces it later in the row"). The mitigation: caller's column declaration order should match visual reading order; pinning is for keep-on-screen behavior, not for re-ordering.

### §19.8.3 — Sticky edit bar

The sticky edit bar (region ⑨) is positioned at the bottom of the table via `position: sticky`. It's announced as part of the table when focus is on its buttons.

The bar has `role="toolbar"` + `aria-label="Editing row {row identifier}"`. Screen readers identify it clearly.

---

## §19.9 — Touch and assistive devices

### §19.9.1 — Touch targets

All interactive elements meet WCAG 2.1 Level AAA touch-target size:
- Buttons / icons: minimum 24×24px in compact density; 28×28px in comfortable; 32×32px in spacious
- Checkboxes: 16×16px visual, 24×24px hit area
- Row click area: full row width × density-derived row height
- Expand chevron: 24×24px hit area (in 28px column width)

AAA recommends 44×44px which we don't meet at compact density. The trade-off is data density vs touch — for clinical tables that are mouse-and-keyboard primary, AA (24×24px minimum) is the target. Spacious density gets closer to AAA.

For tablet-primary surfaces, consuming pages should set `Density="TableDensity.Spacious"` for AAA touch compliance.

### §19.9.2 — Voice control

Voice control software (Dragon, Voice Access) reads `aria-label` to identify elements. LipiTable's labels are written to be voice-friendly:
- "Sort by Name" (not just "Name")
- "Open filter popover for Status" (not just "filter")
- "Edit Dr. Reddy" (not just "edit")
- "Select row, Dr. Reddy" (not just "checkbox")

Voice commands like "click sort by name" or "click edit Dr. Reddy" should work.

### §19.9.3 — Switch control

Switch users navigate by tabbing through interactive elements. LipiTable's tab order is logical (toolbar → header → body cells → footer), and skip links allow quick navigation to body / pagination.

```html
<nav aria-label="Skip links" class="visually-hidden">
    <a href="#table-{tableId}-body">Skip to table body</a>
    <a href="#table-{tableId}-pagination">Skip to pagination</a>
</nav>
```

The skip links are visible when focused (typical pattern for skip links — visually-hidden by default, visible when tab-focused).

---

## §19.10 — Internationalization (i18n)

While not strictly accessibility, i18n affects screen reader behavior and is closely related.

### §19.10.1 — Localized strings

All LipiTable user-facing strings come from `IStringLocalizer` when the consuming app uses ASP.NET Core localization:

- Toolbar labels ("Search", "Export", "Refresh", "Add")
- Pagination ("Page 1 of 20", "Showing 1-25 of 487", "Rows per page")
- Empty / Error / FilteredEmpty messages
- Aria announcements
- Filter operator labels ("contains", "equals", "between")
- Status taxonomy (when caller uses `LipiStatus` constants)
- Confirmation dialog text

Default fallback is English. The consuming app provides resources for other languages.

### §19.10.2 — RTL support

LipiTable supports right-to-left languages (Arabic, Hebrew, Urdu) when the document's `dir="rtl"` attribute is set.

CSS:
- Column order reverses (leftmost = rightmost in RTL)
- Pin-left becomes pin-right in RTL (semantic anchoring stays consistent)
- Selection checkbox moves to right side
- Actions column to left side
- Sort indicators flip
- Filter chips strip reads right-to-left

The implementation uses CSS logical properties (`margin-inline-start`, `padding-inline-end`) wherever possible to avoid duplicating CSS for LTR vs RTL.

### §19.10.3 — Bidirectional text

Cell content that mixes LTR and RTL text (e.g., a name in Hindi followed by a UHID in English digits) is rendered using the browser's natural bidirectional algorithm. No special handling required.

For columns where the text direction is known and forced:

```csharp
[Parameter] public TextDirection? TextDirection { get; set; }
```

```razor
<LipiColumn Field="@(p => p.NameInArabic)" TextDirection="TextDirection.RTL" />
```

The cell forces RTL rendering regardless of surrounding document direction.

---

## §19.11 — Testing accessibility

### §19.11.1 — Automated checks

LipiTable's StyleGuide demos run with `axe-core` (accessibility testing library) integrated. Build chat configures this in CI.

Phase 2.10 audit will run axe-core against:
- Each LipiTable demo in StyleGuide
- Each state (Loading, Empty, Error, FilteredEmpty, Normal)
- Each density mode
- Light + dark mode
- High-contrast mode

Findings flagged as critical block deployment.

### §19.11.2 — Manual testing

Per LiPi standing rules, manual accessibility testing covers:
- Tab navigation through every region (keyboard-only)
- Screen reader navigation with NVDA (Windows) and VoiceOver (macOS)
- High-contrast mode (`prefers-contrast: more`)
- Reduced motion (`prefers-reduced-motion: reduce`)
- Zoom to 200% (text should reflow, no horizontal scroll)
- Color-blindness simulation (Chrome DevTools or browser extensions)

### §19.11.3 — Accessibility statement

The consuming app's accessibility statement page (typically `/accessibility`) should mention LipiTable's compliance:

> Data tables throughout this application use the LipiTable component, which meets WCAG 2.1 Level AA. High-contrast mode (Settings > Display > Contrast) raises this to Level AAA. For assistance with any accessibility issues, contact [support].

---

## §19.12 — Disabling accessibility features

There are no parameters to disable LipiTable's accessibility features. They're built in and always on. Specifically:

- ARIA roles cannot be turned off
- Keyboard model cannot be disabled
- Focus indicators cannot be hidden (the `--lipi-table-focus-ring-w` token can be reduced but not zeroed — minimum 1px)
- Screen reader announcements cannot be silenced

This is a deliberate non-negotiable.

For testing environments that need to suppress aria-live announcements (e.g., automated tests timing out on debounced announcements), the test harness can manipulate the DOM directly rather than relying on user-facing controls.

---

## §19.13 — Worked examples

### §19.13.1 — Standard accessible table (no special configuration)

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           Title="Patient list"
           RowDisplayNameSelector="@(p => $"{p.Name}, UHID {p.Uhid}")"
           SelectionMode="SelectionMode.Multi">
    <LipiColumn Field="@(p => p.Uhid)" />
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
</LipiTable>
```

Out of the box:
- `aria-label="Patient list"` on the grid
- Row aria-labels include "{name}, UHID {uhid}"
- Multi-selection supported via keyboard (Space / Shift+Arrow / Ctrl+A)
- Sort + filter via Enter on headers, Alt+Down for filter popover
- Status column color + text (no color-only meaning)

### §19.13.2 — High-data-density table with explicit AAA target

```razor
<LipiTable TItem="LabResult"
           Items="@_results"
           KeySelector="@(r => r.Id)"
           Density="TableDensity.Spacious"
           Title="Lab results for Priya Sharma">
    <LipiColumn Field="@(r => r.TestName)" />
    <LipiColumn Field="@(r => r.Value)" Type="ColumnType.Number" />
    <LipiColumn Field="@(r => r.AbnormalFlag)" Type="ColumnType.Boolean" 
                TrueLabel="Abnormal"
                FalseLabel="Normal" />
</LipiTable>
```

Spacious density gives 32×32px touch targets (close to AAA). Boolean column uses `TrueLabel` / `FalseLabel` instead of color-only ✓/✗. The full table is clinical context — high contrast (CSS media query) applies AAA token swaps automatically.

### §19.13.3 — Internationalized table (RTL Arabic)

```html
<html dir="rtl" lang="ar">
```

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           Title="@Localizer["PatientList"]"
           RowDisplayNameSelector="@(p => p.NameInArabic)">
    <LipiColumn Field="@(p => p.NameInArabic)" Header="@Localizer["Name"]" />
    <LipiColumn Field="@(p => p.UhidNumber)" Header="UHID" TextDirection="TextDirection.LTR" />
    <LipiColumn Field="@(p => p.Status)" 
                Type="ColumnType.Status"
                Header="@Localizer["Status"]" />
</LipiTable>
```

Document direction = RTL. Headers / chrome from localizer. UHID column forces LTR (numbers always read left-to-right). All ARIA labels announce in Arabic.

---

*End of §19. Proceed to §20 — Virtualization and performance.*


# LipiTable Spec — §20 Virtualization and performance

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1 (performance principle), §4 (data sources), §11 (master-detail variable height), §14 (density row heights)
**Decisions consolidated:** Q1.2 (auto-detect virtualization, threshold 100)

---

## §20.1 — Overview

Virtualization renders only the rows currently visible in the viewport (plus a small buffer) and recycles DOM nodes as the user scrolls. Without virtualization, a 10,000-row table renders 10,000 DOM rows — too slow on most hardware. With virtualization, the same 10,000-row table renders ~30 rows at any moment.

LipiTable uses `Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize<TItem>` under the hood. Per Q1.2 (locked) — virtualization is **auto-detected** by row count with caller override.

---

## §20.2 — Activation

### §20.2.1 — `VirtualizeMode` enum

```csharp
public enum VirtualizeMode
{
    Auto,      // default — virtualize when row count >= threshold
    Always,    // virtualize regardless of row count
    Never      // never virtualize
}
```

```razor
<LipiTable TItem="..." Virtualize="VirtualizeMode.Auto" ...>  <!-- default -->
```

### §20.2.2 — Auto threshold

```csharp
[Parameter] public int VirtualizationThreshold { get; set; } = 100;
```

In `VirtualizeMode.Auto`, virtualization engages when:
- Client-side mode: `Items.Count >= VirtualizationThreshold`
- Server-side mode: `TableQueryResponse.Rows.Count >= VirtualizationThreshold`

The default of 100 is conservative — most tables with 100+ rows benefit from virtualization; below 100, the overhead of virtualization (extra DOM measurements, scroll math) outweighs the savings.

For tables that should always virtualize regardless of size (e.g., for consistent scroll behavior), use `VirtualizeMode.Always`. For tables that should never virtualize (e.g., when print compatibility requires all rows in DOM), use `VirtualizeMode.Never`.

### §20.2.3 — Why not always virtualize?

Virtualization adds overhead:
- DOM measurement on each scroll event
- Scroll-position math
- Row recycling logic
- Potential layout jitter when row heights are variable

For tables with 20 rows, this overhead is wasted. Native browser scroll over 20 DOM rows is faster than virtualized scroll.

For tables between 50 and 100 rows, the trade-off depends on row complexity (cell templates, status strips, etc.). The default threshold of 100 errs on the side of "render natively until we really need virtualization."

---

## §20.3 — Row height model

### §20.3.1 — Fixed height (default)

Most LipiTable rows have a known fixed height per density (per §14.3):

- Compact: 32px
- Comfortable: 40px (default)
- Spacious: 52px

The virtualization component uses this as the `ItemSize` parameter. Fixed-height rows are the fastest virtualization mode — scroll math is `itemIndex × itemSize`, O(1).

### §20.3.2 — Variable height

Some rows have variable heights:
- Expanded master-detail rows (§11) — height depends on the `<DetailTemplate>` content
- Expanded tree-data rows (§10) — same as master-detail for any detail-bearing tree rows
- Custom `<CellTemplate>` content that wraps multi-line text — height grows to fit
- Inline-edit rows when the edit input is taller than the default row (e.g., `LipiTextArea` for notes)
- Conflict banner above a row (§12.11.2)
- Validation error strip above a row (§12.6.3)

LipiTable uses Microsoft's Virtualize component's **variable-height mode** for these cases. The component measures each row on first render and caches the height; subsequent scroll math uses cached heights for already-measured rows and estimated heights (`ItemSize` default) for not-yet-measured rows.

The Microsoft.Virtualize variable-height mode has known limitations:
- Scroll position can jitter when measurements differ from estimates
- Jumping to a specific row (via Ctrl+End) may scroll past, then correct
- Initial scrollbar size is approximate; gets accurate as rows are measured

These limitations are accepted trade-offs of running on the standard Blazor virtualization primitive without rolling our own. Phase 2.10 audit will verify performance is acceptable for the LiPi-specific workloads.

### §20.3.3 — Row height estimation

LipiTable provides `ItemSize` to the Virtualize component based on:

```
ItemSize = base row height (density-derived)
         + (HasDetailRow ? estimated detail height : 0)
         + (HasConflictBanner ? 32px : 0)
         + (HasValidationErrorStrip ? estimatedStripHeight : 0)
```

For tables where master-detail is enabled but most rows aren't expanded, the estimate stays near the base row height. Expanded rows are measured at runtime.

Caller can hint the estimated detail height via:

```csharp
[Parameter] public int? DetailRowEstimatedHeight { get; set; }
```

When set, the virtualization uses this hint for not-yet-measured expanded rows. Improves initial scrollbar accuracy. Optional; LipiTable defaults to a heuristic if not set.

---

## §20.4 — Viewport configuration

### §20.4.1 — Overscan

Microsoft.Virtualize renders a small number of off-screen rows above and below the viewport (called "overscan") to ensure smooth scroll without flashes of empty space.

Default overscan = 3 rows. Configurable via:

```csharp
[Parameter] public int VirtualizationOverscan { get; set; } = 3;
```

For very fast scroll surfaces (touch-momentum, mouse wheel), 5-10 overscan reduces flashing. For very deep tables, more overscan means more rendered DOM at any time — diminishing returns past 10.

### §20.4.2 — Viewport container

The Virtualize component requires a fixed-height scrollable container. LipiTable's body is configured with:

```css
.lipi-table-body-virtual {
    height: var(--lipi-table-viewport-h, 600px);
    overflow-y: auto;
    overflow-x: auto;
}
```

The viewport height defaults to `600px` (or a CSS-defined alternative). For consuming pages embedding LipiTable in custom layouts, override:

```razor
<LipiTable TItem="..." ViewportHeight="800px">    <!-- explicit height -->
<LipiTable TItem="..." ViewportHeight="100%">     <!-- fill parent -->
<LipiTable TItem="..." ViewportHeight="80vh">     <!-- viewport-relative -->
```

The container must have a height — auto-height with virtualization doesn't work (no viewport to compute scroll position). Setting `ViewportHeight="auto"` falls back to a flat (non-virtualized) rendering.

### §20.4.3 — Non-virtualized fallback

When virtualization is disabled (small tables, `VirtualizeMode.Never`, or `ViewportHeight="auto"`), the body uses native browser scroll:

```css
.lipi-table-body {
    /* No fixed height; grows with content */
    overflow-x: auto;     /* horizontal scroll for wide tables */
}
```

The parent container handles vertical overflow. Most pages put LipiTable inside a card or panel that scrolls.

---

## §20.5 — Sticky header during virtualized scroll

The column header (region ⑥) stays visible while the user scrolls through virtualized rows:

```css
.lipi-table-header-row {
    position: sticky;
    top: 0;
    z-index: var(--lipi-table-z-header);
}
```

The header is **outside** the virtualized region — its DOM stays static during scroll. Only the body rows recycle.

This separation matters: if the header were inside the virtualization, every scroll would re-render it. By keeping it sticky and external, the header renders once and stays put.

Same for the aggregate footer row and the cap banner — both render outside the virtualized region.

---

## §20.6 — Selection during virtualized scroll

Cross-reference §5.2 — selection is a set of row keys, not row references. This makes virtualization trivial:

- A row that scrolls off-screen has its DOM unmounted
- Its key remains in the selected set
- When the user scrolls back, the row re-renders with its checkbox checked because its key is in the set

No selection state lives on the DOM — only on the row key set. Zero coordination needed between virtualization and selection.

### §20.6.1 — Selection in 10,000-row scenarios

When the user has "select all across pages" active and 10,000 selected rows exist conceptually (per §5.3.2), the flag mechanism (not actual key materialization) keeps performance constant. The selected set is one flag + an exception list of explicit deselects.

If the user explicitly selects 10,000 individual keys (without using the flag), the `HashSet<object>` of keys uses ~250KB of memory and O(1) lookup per row render — still trivial.

---

## §20.7 — Sort, filter, group during virtualized scroll

Sort, filter, and group **recompute the data list**, not the virtualization:

- Client-side: re-runs the filter → sort → group pipeline; the new flat row list is passed to Virtualize
- Server-side: re-invokes DataSource; the new page response is passed to Virtualize

Virtualization recomputes its viewport based on the new list. Scroll position is preserved by-key when possible:
- If the previously-focused row is still in the new list, scroll position is adjusted to keep it visible
- If the previously-focused row is filtered out, scroll resets to top

This avoids the "I scrolled to row 5000, applied a filter, and now I'm somewhere random" problem.

---

## §20.8 — Inline edit during virtualized scroll

Cross-reference §12. Inline edit interacts with virtualization in two ways:

### §20.8.1 — Edited row stays focused

When a row is in inline edit, scrolling the table does NOT cause the edited row to scroll out of view. LipiTable pins the edited row's position relative to the viewport while the user scrolls:

- Edited row stays at the top of the viewport (or wherever it was when edit started)
- Other rows scroll around it
- The sticky edit bar (§12.2.5 StickyBottomBar option) is always visible

When the user saves or cancels the edit, the row joins the scroll flow again.

### §20.8.2 — Auto-refresh during edit

Per §4.7.4 — auto-refresh pauses during inline edit. Combined with virtualization, this means:
- Edit row stays stable
- Other rows don't change underneath
- User can scroll freely without surprise data refreshes

### §20.8.3 — Cell edit and virtualization

Cell-edit mode is more sensitive to virtualization:
- Editing cell A, then Tab to cell B — cell B must be visible (mounted)
- If cell B is in a row that's currently virtualized off-screen, Tab triggers a scroll-to-cell-B before editing
- The scroll animation completes (within 200ms), then the cell enters edit mode

Edge case: very rapid Tab through a virtualized table can cause cell-edit transitions to feel laggy if scrolls accumulate. Default behavior keeps it usable; for high-throughput data entry, consuming pages should consider non-virtualized rendering (`VirtualizeMode.Never`) for those specific tables.

---

## §20.9 — Variable density and virtualization

When the user changes density (Compact → Spacious), every row's height changes. Virtualization re-measures:

- All cached heights invalidate
- `ItemSize` updates to the new density's default
- The viewport reflows with new dimensions
- The previously-focused row stays in view (scroll position preserved by key)

During the transition (200ms density-change animation, or instant in reduced-motion), the user sees the row heights shift. Some jitter is acceptable — density change is intentional, not background.

---

## §20.10 — Server-side virtualization

In server-side mode, virtualization operates over the **current page** of data, not the full dataset:

- DataSource returns one page (e.g., 50 rows)
- Virtualize renders ~30 of those visible + 3 overscan above/below
- User scrolls within the page
- User clicks pagination → new page fetched → Virtualize resets

For "infinite scroll virtualization" patterns where the user scrolls continuously through unloaded data, the `PaginationMode.InfiniteScroll` mode (§8.5.2) combines pagination + virtualization:

- DataSource returns first page (50 rows)
- Virtualize renders visible rows
- User scrolls to near-bottom → DataSource called for next page
- Newly-loaded rows append to the buffer
- Virtualize integrates them into the scrollable region
- Buffer grows as more pages load

The buffer is unbounded — for very long sessions of infinite scroll, memory grows. Consuming pages should consider:
- A max buffer size (`MaxLoadedRows` parameter — default `int.MaxValue`, set to limit)
- Periodic buffer truncation (e.g., drop rows older than current view by 5+ pages)

```csharp
[Parameter] public int MaxLoadedRows { get; set; } = int.MaxValue;
```

When set and reached, infinite scroll stops fetching. User sees "End of loaded data — apply filters to see more" banner.

---

## §20.11 — Performance budgets

Per Principle 6 (§1.3) — performance targets stated as design intent:

| Operation | Target | Actual (expected) |
|---|---|---|
| First paint (≤100 rows) | ≤ 200ms | ~80-150ms on mid-tier hardware |
| First paint (100-1000 rows, virtualized) | ≤ 300ms | ~150-250ms |
| Filter / sort response (client-side, ≤1000 rows) | ≤ 100ms | ~30-80ms |
| Filter / sort response (server-side) | RTT bound | server-dependent |
| Scroll fps (virtualized) | 60 fps | 55-60 fps |
| Scroll fps (non-virtualized, ≤100 rows) | 60 fps | 60 fps |
| Edit save (client-side) | ≤ 50ms | ~20-40ms |
| Edit save (server-side) | RTT + 50ms | server-dependent |
| Selection toggle | ≤ 16ms (single frame) | ~5-10ms |
| Density change | ≤ 200ms | ~100-150ms (CSS-driven) |
| Pagination page change (client-side) | ≤ 50ms | ~10-30ms |
| Column resize (live preview) | 60 fps | 60 fps (hardware-accelerated CSS Grid) |

These are targets, not hard SLAs. Regressions beyond targets should be investigated. Phase 2.10 audit runs benchmarks against representative workloads.

### §20.11.1 — Benchmark workloads

Phase 2.10 audit uses these workloads:

| Workload | Rows | Columns | Features |
|---|---|---|---|
| Small admin | 50 | 5 | basic select + sort |
| Medium clinical | 500 | 12 | filter + sort + select + status strip |
| Large server-side | 10,000 (paged 50/page) | 20 | filter + sort + select + group + multi-pin |
| Stress test | 50,000 (virtualized client-side) | 8 | scroll-only, no chrome |
| Inline edit heavy | 1000 | 10 | row-edit mode, frequent save/cancel |
| Master-detail | 500 (20 expanded) | 6 | with detail templates of varying heights |

Each workload measures first paint, scroll fps, filter response, edit response. Results recorded in `test-results/2.8-perf-{date}.md` per LiPi standing rules.

---

## §20.12 — Memory considerations

### §20.12.1 — DOM weight

A virtualized LipiTable has approximately:

- Chrome (toolbar, header, footer): ~20-50 DOM nodes
- Visible rows × columns: ~30 × 10 = 300 cells = ~1500-3000 DOM nodes
- Total visible DOM: ~2000-3000 nodes regardless of total row count

A non-virtualized 10,000-row × 10-column table has 100,000+ DOM nodes — typically too slow.

### §20.12.2 — Selection set memory

Per §20.6.1 — a `HashSet<object>` of selected keys uses ~16-24 bytes per key. 10,000 selected = ~250KB. Acceptable.

### §20.12.3 — Filter / sort state

Persisted state (filter descriptors, sort descriptors, group descriptors) is bounded — typically <1KB JSON for even complex tables. Trivial.

### §20.12.4 — Cached aggregates

Per §15.8.2 — aggregates cached by filtered-data-snapshot + aggregate-set. Cache size is bounded by the number of distinct filter combinations the user applies in a session. Typically <100KB.

### §20.12.5 — Detail data cache (consumer-managed)

Per §11.12.1 — lazy-loaded detail data lives in the consumer's dictionary, not LipiTable's. For very long sessions where the user expands many details, the cache grows. Consuming pages with this concern should implement LRU eviction.

---

## §20.13 — When NOT to use LipiTable

For extreme workloads, LipiTable's general-purpose design may not be optimal:

| Workload | LipiTable choice | Alternative |
|---|---|---|
| Real-time tick data (1000 rows/sec updates) | Use Items mode with reference-replace per tick; expect significant CPU | Custom canvas/SVG rendering |
| Spreadsheet-like cell-formula workflows | Cell-edit mode can simulate, but no formula engine | Different component (out of LiPi v1.0 scope) |
| Pivot tables with row × column × measure intersection | Not supported in v1.0 | Custom analytical component |
| Mobile-first responsive data display | LipiTable doesn't reflow below 1024px | Swap to LipiList on small viewports (per §2.6) |
| Print-heavy reporting with templated headers/footers | Print stylesheet exists but is simple | Use in-house PDF library directly (Phase 2.10+) |

LipiTable is the right component for ~95% of tabular data use cases in LiPi. The exceptions above are real but rare.

---

## §20.14 — Worked examples

### §20.14.1 — Default auto-virtualization

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           DefaultPageSize="50">
    <!-- columns -->
</LipiTable>
```

50 rows per page = non-virtualized (under 100 threshold). When user picks "200" page size, virtualization auto-engages. Transparent to user.

### §20.14.2 — Always virtualize for stress test

```razor
<LipiTable TItem="AuditEvent"
           Items="@_audit"
           KeySelector="@(e => e.Id)"
           Virtualize="VirtualizeMode.Always"
           ViewportHeight="600px"
           PaginationMode="PaginationMode.InfiniteScroll"
           MaxLoadedRows="5000"
           VirtualizationOverscan="5">
    <LipiColumn Field="@(e => e.Timestamp)" Type="ColumnType.DateTime" />
    <LipiColumn Field="@(e => e.Actor)" />
    <LipiColumn Field="@(e => e.Action)" />
</LipiTable>
```

Audit log with infinite scroll. Always virtualize. 600px viewport. Buffer max 5000 rows. Higher overscan for smooth fast-scroll.

### §20.14.3 — Disable virtualization for print compatibility

```razor
<LipiTable TItem="Invoice"
           Items="@_invoices"
           KeySelector="@(i => i.Id)"
           Virtualize="VirtualizeMode.Never">
    <!-- columns -->
</LipiTable>
```

Invoice list never virtualizes — all rows in DOM. Print-to-PDF captures everything; ad-hoc Ctrl+F finds any row.

### §20.14.4 — Master-detail with height estimate

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           Virtualize="VirtualizeMode.Auto"
           DetailRowEstimatedHeight="320">
    <LipiColumn Field="@(p => p.Name)" />
    
    <DetailTemplate Context="patient">
        <PatientDemographicsCard Patient="@patient" />   <!-- ~300px tall -->
    </DetailTemplate>
</LipiTable>
```

Detail rows are ~300px when expanded; hint helps initial scrollbar accuracy.

---

*End of §20. Proceed to §21 — Persistence.*


# LipiTable Spec — §21 Persistence

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §6.10 (sort), §7.12 (filter), §8.9 (pagination), §9.10.8 (group), §13.6 (column ops), §14.6 (density)
**Decisions consolidated:** Q10.1, Q10.2

---

## §21.1 — Overview

Per Q10.1 (locked) — LipiTable persists user preferences across sessions for the following table-level state:

- Column **widths**
- Column **order**
- Column **visibility**
- Column **pin state**
- **Sort** (single or multi-column)
- **Filter** values per column
- **Quick search** text
- **Group** dimensions (which columns are active for grouping)
- **Density** (Compact / Comfortable / Spacious)
- **Page size**

Per Q10.2 (locked) — persistence storage is **server-side**, in a per-user, per-table-id JSON document. This makes preferences follow the user across devices and browsers.

State that does NOT persist:
- Current page (snapped to 1 on next visit) — see §8.9.2
- Selection state — selection is ephemeral
- Tree expand state (default opt-out) — see §10.4.3
- Group expand state (default opt-out) — see §9.5.1
- Master-detail expand state (default opt-out) — see §11.6.2
- Inline edit state (ephemeral)
- Scroll position

The opt-out states have parameters (`PersistTreeExpandState="true"`, etc.) for consuming pages that want them, but default to off because their utility depends on stable data over time.

---

## §21.2 — Activation

### §21.2.1 — `TableId` parameter

Persistence is opt-in via `TableId`:

```razor
<LipiTable TItem="Patient"
           TableId="patients-list"
           ...>
```

When `TableId` is set:
- LipiTable reads persisted state on mount (before first render)
- LipiTable writes persisted state when user changes any persisted field (debounced)

When `TableId` is null or empty (default):
- No persistence reads or writes happen
- The table starts fresh on every mount with declared defaults

`TableId` must be **globally unique** within a consuming application. Two tables with the same `TableId` would share preferences, which is almost never the intent. Convention: use a route-like identifier (`"patients-list"`, `"audit-log"`, `"settings-users"`).

### §21.2.2 — TableId stability

Once a consuming page is deployed with a specific `TableId`, that ID should remain stable across deployments. Changing it orphans existing user preferences (still in DB, but never read for that table).

If a TableId needs to change (e.g., a major redesign of the page where new fields invalidate old preferences), the consuming page can write a migration:

```csharp
// At app startup
await _tablePrefService.RenameTableIdAsync(
    oldId: "patients-v1",
    newId: "patients-v2");
```

Or just accept the orphan (users lose their preferences once; on next visit, default state applies). Phase 2.10 audit will recommend a strategy per consuming page.

---

## §21.3 — Storage backend

### §21.3.1 — Database table

Per the Phase 2.8 overview §2.5, the schema:

```sql
CREATE TABLE identity.user_table_preferences (
    user_id        UUID NOT NULL,
    table_id       TEXT NOT NULL,
    prefs_json     JSONB NOT NULL,
    updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (user_id, table_id),
    FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE CASCADE
);
CREATE INDEX ix_user_table_prefs_user ON identity.user_table_preferences(user_id);
```

- One row per `(user_id, table_id)` pair
- Preferences stored as JSON in the `prefs_json` column
- Postgres JSONB for indexed querying if needed (rare)
- Cascade delete: if a user is deleted, their preferences disappear

### §21.3.2 — JSON document shape

The persisted document has top-level keys per state category:

```json
{
    "version": 1,
    "columns": {
        "Uhid":       { "width": "100px", "order": 0, "pin": "left", "visible": true },
        "Name":       { "width": "200px", "order": 1, "pin": "left", "visible": true },
        "Mobile":     { "width": "140px", "order": 2, "pin": "none", "visible": true },
        "Department": { "width": "120px", "order": 3, "pin": "none", "visible": false },
        "Status":     { "width": "100px", "order": 4, "pin": "none", "visible": true },
        "Actions":    { "width": "auto",  "order": 5, "pin": "right", "visible": true }
    },
    "sort": [
        { "columnKey": "UpdatedAt", "direction": "desc", "priority": 0 }
    ],
    "filters": [
        { "columnKey": "Status", "operator": "in", "value": ["Active", "Pending"], "valueEnd": null }
    ],
    "quickSearch": "",
    "groupBy": [],
    "density": "Comfortable",
    "pageSize": 50
}
```

Each section is optional — missing sections mean "use declared defaults." This makes the document forward-compatible: adding new persisted fields in future amendments doesn't break old documents.

### §21.3.3 — Schema version field

The `version` field tracks the persistence schema version. v1.0 = `1`. Future amendments that change the document shape increment the version.

On read, LipiTable checks the version:
- Version matches current → use as-is
- Version older than current → migrate using registered migrators (see §21.7)
- Version newer than current → log warning, use declared defaults (don't trust forward-versioned data)

### §21.3.4 — Document size limits

Each user's persisted document for a single table is typically <2KB. Maximum reasonable size is ~10KB (very large filter values, many columns). Postgres JSONB handles this trivially.

A hard upper bound of 50KB is enforced in `TablePreferenceService` — exceeding it logs a warning and truncates the largest filter values. Indicates a misuse pattern (storing PHI in filter values, or runaway persistence growth from a bug).

---

## §21.4 — `ITablePreferenceService`

### §21.4.1 — Interface

The persistence backend is abstracted behind an interface:

```csharp
public interface ITablePreferenceService
{
    Task<TablePreferences?> GetAsync(string tableId, CancellationToken ct = default);
    Task SaveAsync(string tableId, TablePreferences prefs, CancellationToken ct = default);
    Task ResetAsync(string tableId, CancellationToken ct = default);
    
    // Bulk operations (admin use)
    Task<IReadOnlyList<string>> ListTableIdsAsync(CancellationToken ct = default);
    Task RenameTableIdAsync(string oldId, string newId, CancellationToken ct = default);
}

public sealed class TablePreferences
{
    public int Version { get; init; } = 1;
    public IReadOnlyDictionary<string, ColumnPreference>? Columns { get; init; }
    public IReadOnlyList<SortDescriptor>? Sort { get; init; }
    public IReadOnlyList<FilterDescriptor>? Filters { get; init; }
    public string? QuickSearch { get; init; }
    public IReadOnlyList<string>? GroupBy { get; init; }
    public string? Density { get; init; }
    public int? PageSize { get; init; }
}

public sealed record ColumnPreference(
    string? Width,
    int Order,
    string Pin,
    bool Visible
);
```

LipiTable depends only on the interface. The default implementation is an EF-based service against the identity DB; consuming apps can swap implementations.

### §21.4.2 — Default implementation

Lives in `Services/TablePreferenceService.cs`:

```csharp
public sealed class TablePreferenceService : ITablePreferenceService
{
    private readonly IdentityDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<TablePreferenceService> _logger;
    
    public async Task<TablePreferences?> GetAsync(string tableId, CancellationToken ct = default)
    {
        var entity = await _db.UserTablePreferences
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.Id && p.TableId == tableId, ct);
        
        if (entity is null) return null;
        
        return JsonSerializer.Deserialize<TablePreferences>(entity.PrefsJson);
    }
    
    public async Task SaveAsync(string tableId, TablePreferences prefs, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(prefs);
        
        // Upsert
        var existing = await _db.UserTablePreferences
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.Id && p.TableId == tableId, ct);
        
        if (existing is null)
        {
            _db.UserTablePreferences.Add(new UserTablePreference
            {
                UserId = _currentUser.Id,
                TableId = tableId,
                PrefsJson = json,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.PrefsJson = json;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        
        await _db.SaveChangesAsync(ct);
    }
    
    public async Task ResetAsync(string tableId, CancellationToken ct = default)
    {
        await _db.UserTablePreferences
            .Where(p => p.UserId == _currentUser.Id && p.TableId == tableId)
            .ExecuteDeleteAsync(ct);
    }
    
    // ListTableIdsAsync, RenameTableIdAsync similar
}
```

Registered as **scoped** in DI:

```csharp
// Program.cs
services.AddScoped<ITablePreferenceService, TablePreferenceService>();
```

Scoped because it depends on `ICurrentUser` (typically scoped per request / circuit).

### §21.4.3 — Caching layer

The default implementation includes in-memory caching for the current circuit:

```csharp
private TablePreferences? _cachedPrefs;
private string? _cachedTableId;

public async Task<TablePreferences?> GetAsync(string tableId, CancellationToken ct = default)
{
    if (_cachedTableId == tableId && _cachedPrefs is not null)
        return _cachedPrefs;
    
    var prefs = await LoadFromDb(tableId, ct);
    _cachedTableId = tableId;
    _cachedPrefs = prefs;
    return prefs;
}
```

Cache invalidates on `SaveAsync`. Subsequent reads in the same circuit hit memory, not DB. Improves performance when the same table mounts multiple times (e.g., re-mount after a brief navigation).

### §21.4.4 — Custom implementations

Consuming apps can swap the implementation:

```csharp
// localStorage-backed (for offline-first apps)
services.AddScoped<ITablePreferenceService, LocalStorageTablePreferenceService>();

// Or a different DB schema
services.AddScoped<ITablePreferenceService, CustomTablePreferenceService>();

// Or a hosted service (e.g., user preferences microservice)
services.AddScoped<ITablePreferenceService, HostedTablePreferenceService>();
```

LipiTable doesn't care — it depends on the interface only.

---

## §21.5 — Debouncing writes

### §21.5.1 — Debounce rules per change type

Cross-references from prior sections, consolidated:

| Change | Debounce |
|---|---|
| Column width (drag) | 300ms after drag completes |
| Column width (auto-fit double-click) | 300ms |
| Column order (drag) | 300ms after drop |
| Column pin (drag or context menu) | 300ms |
| Column visibility (column picker) | 300ms |
| Sort change | 300ms after last user click |
| Filter change (popover Apply) | Immediate |
| Filter change (drawer Apply) | Immediate |
| Filter chip remove (✕) | Immediate |
| Filter clear all | Immediate |
| Quick search text change | 300ms after last keystroke |
| Group dimension change | 300ms |
| Density change | Immediate |
| Page size change | Immediate |

The pattern: deliberate one-shot actions (Apply button click, density change) write immediately; continuous actions (typing, dragging) debounce 300ms.

### §21.5.2 — Implementation

LipiTable maintains a single pending-write timer:

```csharp
private System.Threading.Timer? _writeTimer;
private bool _hasPendingWrite;

private void SchedulePersist(TimeSpan? debounce = null)
{
    _hasPendingWrite = true;
    
    if (debounce is null)
    {
        // Immediate write
        _writeTimer?.Dispose();
        _writeTimer = null;
        _ = PersistNowAsync();
    }
    else
    {
        // Debounced write — reset timer
        _writeTimer?.Dispose();
        _writeTimer = new System.Threading.Timer(
            async _ => await PersistNowAsync(),
            null,
            debounce.Value,
            Timeout.InfiniteTimeSpan);
    }
}
```

Multiple rapid changes within the debounce window collapse to one final write.

### §21.5.3 — Flush on unmount

When LipiTable disposes (component unmounts), any pending write is **forcibly flushed**:

```csharp
public async ValueTask DisposeAsync()
{
    if (_hasPendingWrite)
    {
        await PersistNowAsync();
    }
    _writeTimer?.Dispose();
}
```

This prevents data loss when the user navigates away mid-debounce (e.g., types in a filter, clicks a nav link before the 300ms elapses).

### §21.5.4 — Failure handling

If `SaveAsync` throws (network error, DB unavailable), LipiTable:
- Logs the error
- Does NOT show a user-facing notification (persistence is silent infrastructure; users shouldn't see toasts about it)
- Retries on the next debounce tick (the next change triggers a fresh write attempt)

If persistence fails persistently (every write throws), state still works in memory for the current session — just doesn't survive across sessions. Acceptable degradation.

---

## §21.6 — Reading on mount

### §21.6.1 — Sequence

When LipiTable mounts with `TableId` set:

1. Read persisted state from `ITablePreferenceService.GetAsync(tableId)`
2. Apply persisted state to override declared defaults
3. Render with the resolved state
4. Continue normally (subsequent changes write back per §21.5)

The read is **synchronous from the user's perspective** — LipiTable awaits the GetAsync call before first render. This avoids a flash of declared-default state followed by a swap to persisted state.

For performance: the read happens during `OnInitializedAsync`. If the service implementation is slow, the table's first paint is delayed. The default EF-backed implementation typically returns in <10ms (single indexed lookup).

### §21.6.2 — Cache miss

If no persisted state exists for the user × tableId combination:
- `GetAsync` returns null
- LipiTable uses declared defaults entirely
- No write happens until the user makes a change

### §21.6.3 — Cache invalidation on data change

If the consuming page refreshes data such that new columns appear (e.g., a feature flag enables a new column), the persisted state may reference old column keys. LipiTable handles this:
- Persisted columns no longer in declarations are silently dropped from the resolved state
- Newly declared columns not in persisted state use their declared defaults
- The resolved state is what renders

The user sees:
- Old, removed columns: gone
- Existing columns: their persisted order / width / pin / visibility
- New columns: at their declared position

No user-facing notification. The change is invisible to the user; they just see their familiar columns with the new ones appended.

---

## §21.7 — Schema migration

### §21.7.1 — Version field

Per §21.3.3 — the document has a `version` field. v1.0 = `1`.

### §21.7.2 — Future amendments

If a future LipiTable amendment changes the document shape (e.g., adds a new top-level key, changes a field's format), it bumps the version and registers a migrator:

```csharp
// In LipiTable's persistence-handling code
private static readonly IReadOnlyDictionary<int, Func<JsonObject, JsonObject>> _migrators =
    new Dictionary<int, Func<JsonObject, JsonObject>>
    {
        [1] = MigrateV1ToV2,
        [2] = MigrateV2ToV3,
        // ... etc
    };

private TablePreferences ReadAndMigrate(JsonObject json)
{
    var version = json["version"]?.GetValue<int>() ?? 1;
    while (version < CurrentVersion)
    {
        if (_migrators.TryGetValue(version, out var migrator))
        {
            json = migrator(json);
            version++;
        }
        else
        {
            // Unknown version gap — use defaults
            _logger.LogWarning("No migrator for version {Version}; falling back to defaults.", version);
            return new TablePreferences { Version = CurrentVersion };
        }
    }
    return json.Deserialize<TablePreferences>()!;
}
```

This keeps existing users' persistence working across LipiTable upgrades. v1.0 ships with no migrators (no prior versions exist).

### §21.7.3 — Backward compatibility

Within the v1.0 lifecycle, no breaking changes to the persistence schema. Future v1.X amendments may add new fields (forward-compatible — old code ignores unknown keys; new code adds defaults when keys missing).

A v2.0 schema overhaul would bump the version + add migrators. Not anticipated for the LipiTable lifecycle.

---

## §21.8 — Reset

### §21.8.1 — User-initiated reset

The column picker (§16.4) and any custom "Reset to defaults" UI button calls:

```csharp
public ValueTask ResetPreferencesAsync();
```

Which:
1. Calls `ITablePreferenceService.ResetAsync(tableId)` — deletes the persisted row from DB
2. Clears in-memory state to declared defaults
3. Re-renders the table
4. Fires `OnPreferencesReset` event

The user sees their table snap back to declared defaults — all columns visible in their declared order with declared widths, no sort, no filter, default density, default page size.

### §21.8.2 — Confirmation

The "Reset to defaults" button in the column picker shows a confirmation modal first:

```
┌─ Reset table preferences? ────────────────┐
│ This will clear:                          │
│   • Column widths and order               │
│   • Sort, filter, and grouping            │
│   • Density and page size                 │
│                                           │
│ This cannot be undone.                    │
│                                           │
│         [Cancel]    [Reset]               │
└───────────────────────────────────────────┘
```

Standard LipiTable destructive-action confirmation pattern. Programmatic `ResetPreferencesAsync()` does NOT show confirmation — caller is responsible for any pre-reset UX.

### §21.8.3 — Per-state reset

Beyond full reset, LipiTable supports partial resets:

```csharp
public ValueTask ResetSortAsync();
public ValueTask ResetFiltersAsync();           // calls ClearAllFiltersAsync()
public ValueTask ResetColumnLayoutAsync();      // widths + order + pin + visibility
public ValueTask ResetDensityAsync();
public ValueTask ResetPageSizeAsync();
public ValueTask ResetGroupingAsync();          // clears group dimensions
```

Each persists immediately (no debounce). Useful for targeted UX patterns — a "Reset sort" link next to the sort indicator, etc.

---

## §21.9 — Multi-tenant considerations

Per LiPi's multi-tenant architecture, each clinic has its own database. User preferences live in the **identity DB** (shared across all clinics for the user's account), not per-clinic.

This means a user with access to multiple clinics sees the same table layout across them. If their patient list has UHID-pinned-left in clinic A, it's pinned-left in clinic B too.

For preferences that should be clinic-scoped (e.g., per-clinic role-based column visibility), the consuming page must encode the clinic context into the `TableId`:

```razor
<LipiTable TableId="@($"patients-list:{_currentClinic.Id}")" ...>
```

The `TableId` becomes `"patients-list:clinic-uuid"` per clinic. Each clinic has separate persistence.

This is opt-in. Default behavior (`TableId="patients-list"`) shares across clinics.

---

## §21.10 — HIPAA / DPDP considerations

### §21.10.1 — Sensitive filter values

Per §7.12.3 — filter values may include sensitive data (patient identifiers, phone numbers, etc.). LipiTable persists these by default; consuming pages with sensitive columns should opt out per-column:

```razor
<LipiColumn Field="@(p => p.Aadhaar)" 
            Filterable="true" 
            PersistFilter="false" />
```

When `PersistFilter="false"`, that column's filter:
- Works during the session
- Is NOT included in the persisted JSON document
- Resets to default on next mount

Quick search text (`quickSearch`) is also persisted by default. To opt out:

```razor
<LipiTable PersistQuickSearch="false" ...>
```

Default `true`. Consuming pages with searches that might include identifying terms should consider opting out.

### §21.10.2 — Phase 2.10 audit

Phase 2.10 audit will:
- Identify columns across consuming pages where `PersistFilter="false"` should be set
- Verify quick-search opt-out for sensitive surfaces
- Review the encryption-at-rest configuration for `identity.user_table_preferences`
- Confirm cascade delete on user-account removal works

The audit is the verification step; the spec defines the mechanism.

### §21.10.3 — Audit logging

LipiTable does NOT audit-log preference changes (e.g., "User X resized column Y"). Preferences are user-private UI configuration, not clinical data.

If a consuming app wants to log preference activity (e.g., for security investigations), it can subscribe to LipiTable's persistence events:

```csharp
[Parameter] public EventCallback<PersistedContext> OnPersisted { get; set; }
```

```csharp
public sealed record PersistedContext(
    string TableId,
    TablePreferences NewPrefs,
    PersistedTrigger Trigger
);

public enum PersistedTrigger
{
    ColumnLayoutChange,
    SortChange,
    FilterChange,
    DensityChange,
    PageSizeChange,
    GroupChange,
    Reset
}
```

The event fires after each successful write. Consuming page can log to its audit system.

---

## §21.11 — Encryption at rest

The `identity.user_table_preferences` table is part of the identity DB, which inherits the database-level encryption-at-rest configuration. Per LiPi's deployment standard (Postgres + AWS RDS with encryption at rest), no LipiTable-specific encryption is needed.

For deployments using a different identity store (file-based, in-memory), the consuming app is responsible for any additional encryption.

Per Phase 2.10 audit — the encryption configuration for the identity DB is verified separately from LipiTable.

---

## §21.12 — Performance

### §21.12.1 — Read performance

`GetAsync(tableId)` is one indexed lookup in `identity.user_table_preferences`. Typical: <5ms in dev, <15ms in production over a network connection.

The lookup is `O(log n)` over the primary key index. With 100,000 users × 50 tables each = 5M rows, lookup time stays sub-15ms.

### §21.12.2 — Write performance

`SaveAsync(tableId, prefs)` is one upsert. Typical: <10ms in dev, <30ms in production.

With 300ms debouncing, even rapid user interactions trigger at most one write per 300ms window. A 5-minute session with constant column dragging produces ~1000 writes — sustainable.

### §21.12.3 — Caching

Per §21.4.3 — in-memory cache eliminates repeated DB reads within the same circuit. First mount reads from DB; subsequent re-mounts hit cache.

The cache is **per-circuit**, NOT per-application. Different users' circuits have different caches. A user's preferences sync across their own browser tabs via cache invalidation on save (each tab's cache invalidates when any tab writes).

### §21.12.4 — Cross-tab sync

Cross-tab synchronization is NOT built into LipiTable. If a user has the same table open in two browser tabs and changes column widths in tab 1, tab 2 doesn't see the update until next mount.

For consuming apps that want cross-tab sync, they can use BroadcastChannel or SignalR + invalidate LipiTable's cache externally. LipiTable's interface supports this:

```csharp
public ValueTask InvalidatePreferenceCacheAsync();
```

When called, the next change triggers a fresh DB read.

Not in v1.0 default behavior. Documented for consuming pages that need it.

---

## §21.13 — Programmatic access

```csharp
public ValueTask<TablePreferences?> GetPersistedPreferencesAsync();
public ValueTask SetPersistedPreferencesAsync(TablePreferences prefs);
public ValueTask ResetPreferencesAsync();
public ValueTask ResetSortAsync();
public ValueTask ResetFiltersAsync();
public ValueTask ResetColumnLayoutAsync();
public ValueTask ResetDensityAsync();
public ValueTask ResetPageSizeAsync();
public ValueTask ResetGroupingAsync();
public ValueTask InvalidatePreferenceCacheAsync();
```

Use cases:
- Admin tool: "View this user's table preferences" for support
- Migration: set preferences in bulk during a feature rollout
- Custom UI: a "View modes" dropdown that programmatically swaps preference sets

---

## §21.14 — Disabling persistence

| Setting | Effect |
|---|---|
| `TableId="@null"` or unset (default) | No persistence; table starts fresh each mount |
| `<LipiColumn PersistFilter="false">` | That column's filter doesn't persist |
| `PersistQuickSearch="false"` on table | Quick search text doesn't persist |
| `PersistTreeExpandState="false"` (default) | Tree expand state doesn't persist |
| Don't register `ITablePreferenceService` in DI | LipiTable falls back to no-op (logs warning) |

The DI fallback is important: if a consuming app forgets to register the service, LipiTable doesn't break — it just doesn't persist. Warning logged once per circuit.

---

## §21.15 — Worked examples

### §21.15.1 — Default persistence

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list">
    <LipiColumn Field="@(p => p.Uhid)" Pinned="ColumnPin.Left" />
    <LipiColumn Field="@(p => p.Name)" Pinned="ColumnPin.Left" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
</LipiTable>
```

All persisted state works out of the box. User customizations follow them across devices.

### §21.15.2 — Persistence with sensitive filter opt-out

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list"
           PersistQuickSearch="false">
    <LipiColumn Field="@(p => p.Uhid)" />
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Aadhaar)" PersistFilter="false" />
    <LipiColumn Field="@(p => p.Mobile)" PersistFilter="false" />
</LipiTable>
```

Aadhaar and Mobile filter values don't persist. Quick search doesn't persist (since it might include identifying terms). Column layout / sort / non-sensitive filters DO persist.

### §21.15.3 — Per-clinic preferences for multi-tenant role

```razor
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="@($"patients-list:{_clinicContext.CurrentClinicId}")">
    ...
</LipiTable>
```

User's preferences scope to the current clinic. Switching clinics shows a different table layout per the user's choice for that clinic.

### §21.15.4 — Programmatic preferences swap (view modes)

```razor
<LipiSelect TValue="string" 
            @bind-Value="_currentViewMode"
            OnChange="@ApplyViewMode">
    <LipiOption Value="default">Default view</LipiOption>
    <LipiOption Value="compact">Compact view</LipiOption>
    <LipiOption Value="detail">Detail view</LipiOption>
</LipiSelect>

<LipiTable @ref="_table" TItem="Patient" TableId="@($"patients-list:{_currentViewMode}")" ...>
    ...
</LipiTable>

@code {
    private string _currentViewMode = "default";
    
    private async Task ApplyViewMode(string newMode)
    {
        // TableId changes → LipiTable re-reads persistence for the new mode
        await _table!.InvalidatePreferenceCacheAsync();
    }
}
```

Three view modes, each persisting separately. User customizes each view independently.

### §21.15.5 — Reset with confirmation

```razor
<LipiButton OnClick="@HandleReset" Variant="ButtonVariant.Secondary">
    Reset table to defaults
</LipiButton>

<LipiTable @ref="_table" ... TableId="patients-list">
    ...
</LipiTable>

@code {
    private async Task HandleReset()
    {
        var confirmed = await ModalService.ShowConfirmAsync(
            title: "Reset table preferences?",
            body: "Your column layout, filters, sort, and other settings will be cleared. This cannot be undone.",
            confirmText: "Reset",
            confirmVariant: ButtonVariant.Danger);
        
        if (confirmed)
        {
            await _table!.ResetPreferencesAsync();
            await ToastService.ShowAsync("Table preferences reset.");
        }
    }
}
```

External reset button with caller-controlled confirmation modal. Useful when reset should be available outside the column picker.

---

*End of §21. Proceed to §22 — Conditional row formatting.*


# LipiTable Spec — §22 Conditional row formatting

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2 (row anatomy), §3 (column model — CellTemplate), §24 (status tokens, color tokens)
**Cross-references:** `lipi-status-tokens.css` (Phase 2.8 shared infrastructure)

---

## §22.1 — Overview

Conditional row formatting lets the consuming page change row appearance based on data — highlight critical rows, color-strip by status, dim disabled rows, etc. LipiTable exposes four hooks:

| Hook | Returns | Purpose |
|---|---|---|
| `RowClass` | `string?` (one or more CSS class names) | Add caller-defined classes to the row element |
| `RowStyle` | `string?` (inline CSS) | Add inline styles to the row element |
| `RowStatus` | `string?` (status string) | Set the row's status strip via shared status tokens |
| `RowDisabled` | `bool` | Mark the row as visually disabled and non-interactive |

These hooks are **per-row** — invoked as `Func<TItem, ...>` returning the appropriate type for each row.

The **status strip** is the most prominent visual cue and uses the shared `lipi-status-tokens.css` infrastructure (Phase 2.8 overview §2.1). Caller doesn't write CSS for status colors — the token system handles it.

---

## §22.2 — `RowStatus` — the status strip

### §22.2.1 — Activation

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           RowStatus="@(p => p.Status?.ToLowerInvariant())">
    <LipiColumn Field="@(p => p.Name)" />
</LipiTable>
```

`RowStatus` returns a status string per row. LipiTable applies the shared `lipi-status-strip-left` class + `data-status="{value}"` attribute on the row:

```html
<div role="row" 
     class="lipi-table-row lipi-status-strip-left" 
     data-status="active">
    ...
</div>
```

The shared CSS file (`lipi-status-tokens.css`) maps `data-status` values to colors:

```css
[data-status="active"]    { border-left-color: var(--color-status-active);   }
[data-status="pending"]   { border-left-color: var(--color-status-pending);  }
[data-status="locked"]    { border-left-color: var(--color-status-locked);   }
/* ... etc ... */
```

Result: a 4px-wide colored left border on the row indicating its status.

### §22.2.2 — Status string conventions

Per `LipiStatus.cs` constants (Phase 2.8 §2.2), the reference taxonomy:

| Status | Color token |
|---|---|
| `active` | success (green) |
| `pending` | warning (amber) |
| `inactive` | neutral (grey) |
| `suspended` | warning-strong (deep amber) |
| `locked` | danger (red) |
| `archived` | neutral-strong (dark grey) |
| `draft` | neutral (grey) |
| `published` | success (green) |
| `in-progress` | info (blue) |
| `completed` | success-strong (deep green) |
| `failed` | danger (red) |
| `cancelled` | neutral-strong (dark grey) |
| `warning` | warning (amber) |
| `error` | danger (red) |
| `info` | info (blue) |
| `success` | success (green) |

Unknown status strings fall back to `--color-status-unknown` (neutral grey).

The taxonomy is reference-only — consuming pages can use these standard values OR define their own (e.g., clinical app might use `"vital-critical"`, `"vital-stable"`). Domain-specific values need corresponding CSS tokens added to the shared file OR per-app overrides.

### §22.2.3 — Per-app status extension

When a consuming app introduces domain-specific status values, the app extends the shared CSS file:

```css
/* App-level status extension */
[data-status="vital-critical"]  { border-left-color: var(--color-danger-strong); }
[data-status="vital-stable"]    { border-left-color: var(--color-success); }
[data-status="opd-waiting"]     { border-left-color: var(--color-info-light); }
```

LipiTable doesn't ship these — they're consuming-page concerns. The mechanism (CSS attribute selectors + LipiTable's `data-status` attribute) is the contract.

For type-safety, consuming pages can define their own constants class:

```csharp
public static class PatientStatusValues
{
    public const string VitalCritical = "vital-critical";
    public const string VitalStable = "vital-stable";
    public const string OpdWaiting = "opd-waiting";
}
```

Used in the `RowStatus` selector:

```razor
RowStatus="@(p => p.IsCritical ? PatientStatusValues.VitalCritical : PatientStatusValues.VitalStable)"
```

### §22.2.4 — Status strip vs status column

A `Status` column type (§3.3.2) renders a `LipiBadge` or `LipiPill` with the status value as text. The row's `RowStatus` strip is the left-border indicator.

These work together — typically a clinical table has BOTH:
- Status column showing the status word ("Critical", "Stable")
- Status strip on the row giving a glanceable color cue

Or one or the other, depending on density / column count constraints.

The two derive from the same data but are independent visual elements. The caller controls each.

### §22.2.5 — Null / empty status

When `RowStatus` returns `null` or empty string:
- No `data-status` attribute on the row
- No strip rendered (default border-left = transparent)

For rows that should be visually unmarked, return null. For rows that should explicitly show "unknown / not categorized," return `"unknown"`:

```razor
RowStatus="@(p => p.Status ?? "unknown")"   // explicit grey strip for null statuses
RowStatus="@(p => p.Status)"                  // no strip for null statuses
```

### §22.2.6 — Status strip placement

Default: left edge of the row (`lipi-status-strip-left`). The shared CSS also defines right and top variants:

```csharp
public enum RowStatusPlacement
{
    Left,    // default
    Right,
    Top
}
```

```razor
<LipiTable RowStatusPlacement="RowStatusPlacement.Top" ...>
```

`Top` is a 4px-tall colored bar above the row. Used when the table is in a card with rounded corners and the left edge doesn't have a visible border. Rarer pattern.

---

## §22.3 — `RowClass` — caller-defined classes

### §22.3.1 — Activation

```razor
<LipiTable TItem="Order"
           RowClass="@(o => o.IsOverdue ? "row-overdue" : null)">
    <!-- columns -->
</LipiTable>
```

`RowClass` returns a class name (or space-separated list of class names) to apply to the row element:

```html
<div role="row" class="lipi-table-row row-overdue">
    ...
</div>
```

The caller defines the corresponding CSS:

```css
.row-overdue {
    background-color: rgba(239, 68, 68, 0.04);
    font-weight: 500;
}
```

### §22.3.2 — Multiple classes

The function can return multiple space-separated classes:

```razor
RowClass="@(o => 
    string.Join(" ", new[] 
    {
        o.IsOverdue ? "row-overdue" : null,
        o.IsHighValue ? "row-high-value" : null,
        o.IsArchived ? "row-archived" : null
    }.Where(c => c is not null))!)"
```

Or a helper method:

```csharp
private string? GetRowClasses(Order o)
{
    var classes = new List<string>();
    if (o.IsOverdue) classes.Add("row-overdue");
    if (o.IsHighValue) classes.Add("row-high-value");
    if (o.IsArchived) classes.Add("row-archived");
    return classes.Count > 0 ? string.Join(" ", classes) : null;
}
```

### §22.3.3 — Composition with LipiTable's own classes

The caller's classes are **appended** to LipiTable's standard row classes:

```html
<div role="row" class="lipi-table-row lipi-table-row--editing row-overdue">
```

The caller cannot replace LipiTable's classes — `lipi-table-row` and its modifiers (`--editing`, `--selected`, etc.) always render. The caller's classes layer on top.

CSS specificity rules apply normally. If the caller's class targets the same property as LipiTable's, the more specific or later-declared rule wins. Caller should write specific enough selectors to override defaults if needed:

```css
.lipi-table-row.row-overdue {        /* matches both classes — higher specificity */
    background-color: rgba(239, 68, 68, 0.08);
}
```

### §22.3.4 — When NOT to use RowClass

For visual cues that fit the status taxonomy, prefer `RowStatus` over `RowClass`:
- Status strip is the established visual idiom
- Token-driven (theme-aware, light/dark/high-contrast adapts automatically)
- More accessible (data-status attribute exposes meaning to screen readers via app's aria announcements)

Use `RowClass` for orthogonal visual concerns:
- Layout adjustments (extra padding for a specific row)
- Non-status-related highlights (e.g., "this is the row the user just edited")
- One-off styles that don't fit the design system

---

## §22.4 — `RowStyle` — inline styles

### §22.4.1 — Activation

```razor
<LipiTable TItem="Score"
           RowStyle="@(s => $"background: linear-gradient(to right, transparent, {GetHeatColor(s.Score)} 100%);")">
    <!-- columns -->
</LipiTable>
```

`RowStyle` returns an inline CSS string applied to the row:

```html
<div role="row" 
     class="lipi-table-row" 
     style="background: linear-gradient(to right, transparent, rgba(239,68,68,0.2) 100%);">
    ...
</div>
```

### §22.4.2 — When to use RowStyle

Inline styles are an **escape hatch** for cases where:
- The visual is dynamic per-row in ways static CSS classes can't easily express (color gradients, dynamic numeric values)
- The styling is one-off and doesn't justify a CSS class
- The value comes from data that can't be enumerated in advance

Common patterns:
- **Heat-map cells** — background color intensity scales with cell value
- **Progress indicators** — width of a background gradient = progress percentage
- **Risk indicators** — opacity / saturation reflects severity

### §22.4.3 — When NOT to use RowStyle

Inline styles bypass the CSS cascade and theme system. Prefer `RowClass` + CSS when:
- The styling is static (boolean condition → one of N fixed appearances)
- The styling should respect light/dark mode tokens
- The styling should be auditable / debuggable in CSS files

Inline styles also have a small performance cost — they're written to the DOM every render. For tables with 1000 rows in a static-styled scenario, this is wasted work compared to a single CSS rule.

### §22.4.4 — Combining with RowClass

Both can be active on the same row:

```razor
<LipiTable TItem="Score"
           RowClass="@(s => s.IsAlert ? "score-alert" : null)"
           RowStyle="@(s => $"--score-value: {s.Value};")">
    ...
</LipiTable>
```

The CSS class provides static styling; the inline style passes a dynamic value (via CSS custom property) that the class's CSS can consume:

```css
.score-alert {
    background: linear-gradient(90deg, 
        rgba(239, 68, 68, calc(var(--score-value) / 100)) 0%,
        transparent 100%);
}
```

This pattern preserves themability (the gradient uses tokens) while supporting per-row dynamic values.

---

## §22.5 — `RowDisabled` — disabled rows

### §22.5.1 — Activation

```razor
<LipiTable TItem="Doctor"
           RowDisabled="@(d => !d.IsActive)">
    <!-- columns -->
</LipiTable>
```

When `RowDisabled` returns `true` for a row:
- `lipi-table-row--disabled` modifier class applied
- Background: `--lipi-table-bg-row-disabled` (subtle grey tint)
- Text: muted color
- Selection checkbox disabled
- Inline edit pencil disabled (greyed out)
- Action buttons disabled (caller's action templates should also check this and disable themselves)
- Click events still fire (caller can decide whether to suppress in handlers)
- Keyboard navigation can still focus the row (for screen reader accessibility)

### §22.5.2 — Visual treatment

```css
.lipi-table-row--disabled {
    background: var(--lipi-table-bg-row-disabled);
    color: var(--lipi-table-text-faint);
}

.lipi-table-row--disabled .lipi-table-cell-select input,
.lipi-table-row--disabled .lipi-table-cell-actions button {
    opacity: 0.5;
    cursor: not-allowed;
    pointer-events: none;
}
```

The cell content remains readable (muted, not invisible). The row is visibly marked as inactive but its data is still accessible.

### §22.5.3 — Selection of disabled rows

By default, disabled rows cannot be selected — their checkbox is non-interactive. The "select all on page" checkbox skips disabled rows.

For consuming pages that want disabled rows to remain selectable (e.g., to bulk-archive both active and inactive items together):

```csharp
[Parameter] public bool AllowSelectDisabled { get; set; } = false;
```

When `true`, disabled rows can be selected normally; only the visual treatment is muted.

### §22.5.4 — Edit of disabled rows

Per §12.1.5 — `RowEditable` is the dedicated parameter for edit-eligibility. `RowDisabled` is independent.

A row can be:
- Editable + enabled (normal)
- Editable + disabled (edit pencil visible but greyed; clicking has no effect — but `RowDisabled` taking precedence)
- Non-editable + enabled (no pencil, normal appearance)
- Non-editable + disabled (greyed, no pencil)

For most cases, `RowDisabled` implies `RowEditable=false`. The two parameters are independent for flexibility in edge cases (e.g., admin user can edit even inactive doctors).

### §22.5.5 — Click handlers on disabled rows

`OnRowClick` still fires for disabled rows. The caller's handler is responsible for checking `RowDisabled` if it should suppress the action:

```razor
<LipiTable RowDisabled="@(d => !d.IsActive)" 
           OnRowClick="@HandleRowClick">
    ...
</LipiTable>

@code {
    private void HandleRowClick(Doctor d)
    {
        if (!d.IsActive) return;  // ignore clicks on disabled rows
        Nav.NavigateTo($"/doctors/{d.Id}");
    }
}
```

This is intentional — sometimes clicking a disabled row should still do something (e.g., navigate to a detail page that shows why the doctor was deactivated). The caller decides.

### §22.5.6 — Screen reader behavior

Disabled rows are announced with `aria-disabled="true"`:

```html
<div role="row" aria-disabled="true" aria-rowindex="5">
    ...
</div>
```

Screen readers announce "row, disabled, Dr. Sharma" so the user knows the row's state. The row remains in the tab order (per §19.3.1).

---

## §22.6 — Combined formatting hooks

All four hooks can be active simultaneously. The render order in the DOM:

```html
<div role="row" 
     class="lipi-table-row lipi-table-row--selected lipi-status-strip-left row-overdue"
     data-status="pending"
     aria-disabled="true"
     aria-selected="true"
     style="--score-value: 47;">
    ...
</div>
```

Each hook contributes independently:
- LipiTable's own classes / modifiers always present
- Status strip from `RowStatus`
- Custom classes from `RowClass`
- Inline style from `RowStyle`
- Disabled state from `RowDisabled`

The visual result combines them — for a disabled, selected, status-pending, custom-styled row, the user sees:
- Disabled grey tint (from `--disabled` modifier)
- Selected primary tint overlay (from `--selected` modifier)
- Yellow left border (from `data-status="pending"`)
- Custom row-overdue styling (from `RowClass`)
- Dynamic inline gradient (from `RowStyle`)

CSS layering and specificity rules apply. Some combinations may visually conflict (e.g., a high-saturation `RowStyle` might fight with the disabled grey). Consuming pages test their combinations.

---

## §22.7 — Cell-level conditional formatting

For per-cell (not per-row) visual cues, use `<CellTemplate>` (per §3.4.1):

```razor
<LipiColumn Field="@(p => p.Risk)" Type="ColumnType.Number">
    <CellTemplate Context="p">
        <span class="@(p.Risk > 75 ? "cell-high-risk" : "cell-normal-risk")">
            @p.Risk
        </span>
    </CellTemplate>
</LipiColumn>
```

LipiTable does NOT provide a `CellClass` / `CellStyle` selector per column. The reason: cell templates already provide full control, and adding per-cell hooks would create three overlapping mechanisms (template + class + style) for the same use case.

For dense conditional formatting across many columns, the consuming page can compose:
- `RowClass` to add a row-level class
- CSS targeting cells under that class to style specific columns

```css
.row-high-risk .lipi-table-cell:nth-child(3) {  /* third cell only */
    background: rgba(239, 68, 68, 0.1);
}
```

Combines row-level data with column-level styling without per-cell hooks.

---

## §22.8 — Performance considerations

### §22.8.1 — Hook invocation frequency

Each hook (`RowClass`, `RowStyle`, `RowStatus`, `RowDisabled`) is invoked once per visible row per render. For a virtualized table with 30 visible rows × 4 hooks = 120 function calls per render.

The hooks should be **fast** — synchronous, no allocations, no async work. Common pattern:

```csharp
private string? GetRowClass(Order o) => 
    o.IsOverdue ? "row-overdue" : null;   // simple property access, no work
```

Anti-patterns:
- Database lookups inside the hook (`o => _service.IsCustomerVip(o.CustomerId) ? "vip" : null` — N database hits per render)
- Async work (`async o => await CheckSomething(o)` — hooks should be sync)
- Allocation per call (`o => new[] { "a", "b" }.Join(" ")` — heap pressure)

For data that requires precomputation, the consuming page should compute it once and store on the data model (or in a parallel dictionary):

```csharp
private Dictionary<Guid, string> _rowClasses = new();

protected override async Task OnInitializedAsync()
{
    foreach (var order in _orders)
    {
        _rowClasses[order.Id] = ComputeClass(order);   // precompute
    }
}

// In markup:
RowClass="@(o => _rowClasses.TryGetValue(o.Id, out var c) ? c : null)"
```

### §22.8.2 — Memoization

LipiTable does NOT memoize hook results. The hooks are called every render. Memoization is the caller's responsibility if needed.

For typical data, the cost is negligible — a comparison + string return is ~10ns per call. 120 calls = ~1μs per render. Not worth memoizing.

For expensive hooks (heavy computation), the caller should:
- Precompute as in §22.8.1
- Or use a memoization helper (cache by row key + a "data version" counter that invalidates the cache when data changes)

---

## §22.9 — Combining with inline edit

### §22.9.1 — Hooks during edit

When a row is in inline edit, the formatting hooks continue to fire. The combined visual:

- `RowClass` and `RowStyle` apply as normal
- `RowStatus` applies as normal (status strip preserved)
- `RowDisabled` is overridden — edit takes precedence; the row is not greyed during edit
- `--editing` modifier overlays the visual cues with a subtle warning tint

If a row was disabled before edit AND somehow entered edit mode (programmatically), the disabled modifier yields to the editing modifier. After edit ends, disabled reapplies.

### §22.9.2 — Status changing via edit

If the user changes a row's status field during edit (and the table uses `RowStatus="@(p => p.Status)"`), the strip color updates as the user types — live reflection of the dirty value.

This is intentional: visual cues should reflect the data the user is working with, including unsaved edits. After save, the strip color stays the same (because it was already correct).

If the consuming page prefers strip color reflect only saved state:

```razor
RowStatus="@(p => _persistedStatuses.GetValueOrDefault(p.Id, p.Status))"
```

Where `_persistedStatuses` is a snapshot updated only on save. Rare pattern — most tables prefer live reflection.

---

## §22.10 — Worked examples

### §22.10.1 — Status strip on patient list

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           RowStatus="@(p => p.Status?.ToLowerInvariant())">
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
    <LipiColumn Field="@(p => p.LastVisit)" Type="ColumnType.Date" />
</LipiTable>
```

Standard pattern. Status column shows the word; row strip provides the color cue. User can scan the strip for at-a-glance triage.

### §22.10.2 — Highlight overdue invoices

```razor
<LipiTable TItem="Invoice"
           Items="@_invoices"
           KeySelector="@(i => i.Id)"
           RowClass="@GetInvoiceRowClass">
    <LipiColumn Field="@(i => i.Number)" />
    <LipiColumn Field="@(i => i.DueDate)" Type="ColumnType.Date" />
    <LipiColumn Field="@(i => i.Amount)" Type="ColumnType.Currency" />
</LipiTable>

<style>
    .invoice-overdue {
        background: var(--color-danger-alpha-04);
        font-weight: 500;
    }
    .invoice-due-soon {
        background: var(--color-warning-alpha-04);
    }
</style>

@code {
    private string? GetInvoiceRowClass(Invoice i)
    {
        if (i.DueDate < DateOnly.FromDateTime(DateTime.Today)) return "invoice-overdue";
        if (i.DueDate < DateOnly.FromDateTime(DateTime.Today.AddDays(7))) return "invoice-due-soon";
        return null;
    }
}
```

Overdue invoices in light red; due-within-7-days in light amber. Token-based colors respect light/dark mode.

### §22.10.3 — Heat-map cell intensity via RowStyle

```razor
<LipiTable TItem="LabResult"
           Items="@_results"
           KeySelector="@(r => r.Id)"
           RowStyle="@GetRowStyle">
    <LipiColumn Field="@(r => r.TestName)" />
    <LipiColumn Field="@(r => r.Value)" Type="ColumnType.Number" />
    <LipiColumn Field="@(r => r.AbnormalLevel)" Type="ColumnType.Number" />
</LipiTable>

@code {
    private string? GetRowStyle(LabResult r)
    {
        if (r.AbnormalLevel == 0) return null;
        var alpha = r.AbnormalLevel / 100.0 * 0.15;  // 0–0.15 alpha
        return $"background: rgba(239, 68, 68, {alpha:F2});";
    }
}
```

Lab results with abnormal-level intensity reflected in background opacity. Continuous data → continuous visual.

### §22.10.4 — Disabled doctors (read-only)

```razor
<LipiTable TItem="Doctor"
           Items="@_doctors"
           KeySelector="@(d => d.Id)"
           RowDisabled="@(d => d.Status == "Inactive")"
           OnRowClick="@HandleClick">
    <LipiColumn Field="@(d => d.Name)" />
    <LipiColumn Field="@(d => d.Specialty)" />
    <LipiColumn Field="@(d => d.Status)" Type="ColumnType.Status" />
</LipiTable>

@code {
    private void HandleClick(Doctor d)
    {
        // Clicking inactive doctor still navigates — to a "why inactive" detail page
        Nav.NavigateTo($"/doctors/{d.Id}");
    }
}
```

Inactive doctors are visually muted but still clickable for inspection.

### §22.10.5 — Combined: status + class + disabled

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)"
           RowStatus="@(p => p.Status?.ToLowerInvariant())"
           RowClass="@(p => p.HasUrgentAlerts ? "row-urgent" : null)"
           RowDisabled="@(p => p.IsArchived)">
    <LipiColumn Field="@(p => p.Name)" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
</LipiTable>

<style>
    .row-urgent {
        border-left-width: 6px !important;  /* override the 4px default for urgent emphasis */
        background: var(--color-danger-alpha-04);
    }
</style>
```

Three layered cues:
1. Status strip color from the data
2. Wider strip + tinted background for urgent patients
3. Greyed-out treatment for archived patients

Disabled state takes visual precedence; archived patients are muted regardless of urgency.

---

*End of §22. Proceed to §23 — Events.*


# LipiTable Spec — §23 Events

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** all prior sections (this is a consolidated catalog)

---

## §23.1 — Overview

This section catalogs every event LipiTable exposes. Events fall into nine groups:

1. **Row interaction** — clicks, double-clicks, hover, focus
2. **Selection** — checkbox changes, select-all transitions
3. **Sort, filter, search** — when the user reshapes data
4. **Pagination** — page navigation, page size changes
5. **Group** — group dimensions added / removed / reordered, group expand / collapse
6. **Tree** — tree node expand / collapse
7. **Master-detail** — detail expand / collapse
8. **Inline edit** — edit start / end / save / failure / dirty / conflict
9. **Lifecycle** — refresh, density change, column ops, export, error, persistence

Each event is an `EventCallback<T>` or `Func<T, Task<bool>>` parameter on `LipiTable<TItem>`. Most are optional — leaving the parameter unset means LipiTable still handles the action correctly, just doesn't notify the caller.

A small number are `Func<T, Task<bool>>` interceptors that let the caller cancel an action (e.g., `OnBeforeRowSave`). These are explicitly noted.

---

## §23.2 — Row interaction events

### §23.2.1 — `OnRowClick`

```csharp
[Parameter] public EventCallback<TItem> OnRowClick { get; set; }
```

Fires when the user clicks anywhere in a row's body (not on a checkbox, chevron, action button, or inside an editable cell in edit mode).

Common use: navigate to a detail page on row click.

```razor
<LipiTable OnRowClick="@(p => Nav.NavigateTo($"/patients/{p.Id}"))" ...>
```

Notes:
- Disabled rows still fire `OnRowClick` (per §22.5.5); caller's handler decides
- During inline cell-edit, click on the edited cell is suppressed (it's already the edit target); click on other cells in the same row fires normally
- During inline row-edit, clicks on non-input areas of the edited row don't fire `OnRowClick`

### §23.2.2 — `OnRowDoubleClick`

```csharp
[Parameter] public EventCallback<TItem> OnRowDoubleClick { get; set; }
```

Fires on row double-click. Default behavior when `OnRowDoubleClick` is NOT set:
- Tree row with children: toggle expand
- Master-detail row: toggle detail
- Cell-edit mode row: enter cell-edit on the double-clicked cell

When `OnRowDoubleClick` IS set, the default toggle behavior is **suppressed** — caller takes full responsibility. To preserve default behavior while adding custom logic, the caller can call back into the table's API:

```razor
<LipiTable OnRowDoubleClick="@HandleDoubleClickAsync" @ref="_table" ...>
```

```csharp
private async Task HandleDoubleClickAsync(TItem item)
{
    await _logger.LogClickAsync(item);
    if (_table is not null)
        await _table.ToggleDetailAsync(item);   // restore default
}
```

### §23.2.3 — `OnRowHover` / `OnRowMouseEnter` / `OnRowMouseLeave`

Not in v1.0. Hover states are visual-only (CSS-driven). If callers need hover events, they wire via `<CellTemplate>` per-cell or compose at a higher level.

### §23.2.4 — `OnRowFocus`

```csharp
[Parameter] public EventCallback<TItem> OnRowFocus { get; set; }
```

Fires when a cell in the row receives keyboard focus AND the previously focused cell was in a different row (i.e., the focus moved to a new row, not within the same row).

Use case: keep a sidebar synchronized with the keyboard-focused row in the table.

Not commonly needed. Documented for completeness.

---

## §23.3 — Selection events

### §23.3.1 — `OnSelectionChanged`

```csharp
public sealed record SelectionChangedContext<TItem>(
    IReadOnlyList<TItem> SelectedItems,
    IReadOnlyList<TItem> AddedItems,
    IReadOnlyList<TItem> RemovedItems,
    SelectionChangeReason Reason
);

public enum SelectionChangeReason
{
    UserClickCheckbox,
    UserKeyboardSpace,
    UserShiftClick,
    UserCtrlClick,
    UserSelectAllOnPage,
    UserSelectAllAcrossPages,
    UserClearSelection,
    Programmatic,
    DataChange         // items removed from data; their keys dropped from selection
}

[Parameter] public EventCallback<SelectionChangedContext<TItem>> OnSelectionChanged { get; set; }
```

Fires after every selection mutation. Includes the added/removed items for diff-aware consumers and the full current selection for snapshot consumers.

For two-way binding without an explicit handler:

```razor
<LipiTable TItem="..." @bind-SelectedItems="_selectedItems" ...>
```

LipiTable binds to the caller's list via `@bind-SelectedItems` — equivalent to subscribing to `OnSelectionChanged` and replacing the list.

### §23.3.2 — `OnSelectAllAcrossPagesChanged`

```csharp
[Parameter] public EventCallback<bool> OnSelectAllAcrossPagesChanged { get; set; }
```

Fires when the user transitions into or out of the "select all across pages" state (step 2 of the two-step select-all per §5.3.2). Argument: `true` when entering across-pages mode; `false` when leaving.

Use case: show a custom banner / analytics event for the bulk-select-all action.

---

## §23.4 — Sort, filter, search events

### §23.4.1 — `OnSortChanged`

```csharp
public sealed record SortChangedContext(
    IReadOnlyList<SortDescriptor> NewSort,
    IReadOnlyList<SortDescriptor> PreviousSort,
    SortChangeReason Reason
);

public enum SortChangeReason
{
    UserClickHeader,
    UserShiftClickHeader,
    UserContextMenu,
    UserClearAll,
    Programmatic,
    PersistenceRestore,
    ResetToDefault
}

[Parameter] public EventCallback<SortChangedContext> OnSortChanged { get; set; }
```

Fires after sort state changes. `NewSort` is the active sort chain (empty for no sort, one entry for single sort, multiple for multi-sort).

URL sync pattern:

```csharp
private void HandleSortChanged(SortChangedContext ctx)
{
    var sortParam = string.Join(",", ctx.NewSort.Select(s => $"{s.ColumnKey}:{s.Direction}"));
    Nav.NavigateTo(QueryHelpers.AddQueryString(Nav.Uri, "sort", sortParam));
}
```

### §23.4.2 — `OnFilterChanged`

```csharp
public sealed record FilterChangedContext(
    IReadOnlyList<FilterDescriptor> NewFilters,
    IReadOnlyList<FilterDescriptor> PreviousFilters,
    FilterChangeReason Reason
);

public enum FilterChangeReason
{
    UserApplyPopover,
    UserApplyDrawer,
    UserRemoveChip,
    UserClearAll,
    Programmatic,
    PersistenceRestore,
    ResetToDefault
}

[Parameter] public EventCallback<FilterChangedContext> OnFilterChanged { get; set; }
```

Fires whenever filters change — column filter applied, chip removed, "Clear all" clicked, etc.

### §23.4.3 — `OnQuickSearchChanged`

```csharp
public sealed record QuickSearchChangedContext(string? NewValue, string? PreviousValue);

[Parameter] public EventCallback<QuickSearchChangedContext> OnQuickSearchChanged { get; set; }
```

Fires after the quick search input changes (debounced 300ms per §7.10.2).

Most consumers don't need this — quick search is handled internally. Useful for URL sync or analytics.

---

## §23.5 — Pagination events

### §23.5.1 — `OnPageChanged`

```csharp
public sealed record PageChangedContext(
    int NewPage,
    int PreviousPage,
    int PageSize,
    PageChangeReason Reason
);

public enum PageChangeReason
{
    UserClickPager,
    UserJumpToPage,
    UserKeyboard,
    InfiniteScrollLoadMore,
    LoadMoreButton,
    Programmatic,
    DataChange,       // current page no longer exists due to data change
    ResetAfterFilter
}

[Parameter] public EventCallback<PageChangedContext> OnPageChanged { get; set; }
```

Fires after the active page changes. `PreviousPage = 0` when this is the initial mount.

URL sync:

```csharp
private void HandlePageChanged(PageChangedContext ctx)
{
    Nav.NavigateTo(QueryHelpers.AddQueryString(Nav.Uri, "page", ctx.NewPage.ToString()));
}
```

### §23.5.2 — `OnPageSizeChanged`

```csharp
public sealed record PageSizeChangedContext(
    int NewSize,
    int PreviousSize,
    PageSizeChangeReason Reason
);

public enum PageSizeChangeReason
{
    UserSelectSize,
    UserAllOption,
    Programmatic,
    PersistenceRestore,
    ResetToDefault
}

[Parameter] public EventCallback<PageSizeChangedContext> OnPageSizeChanged { get; set; }
```

---

## §23.6 — Group events

### §23.6.1 — `OnGroupChanged`

Cross-reference §9.12. Reproduced here:

```csharp
public sealed record GroupChangedContext(
    IReadOnlyList<string> NewGroupBy,
    IReadOnlyList<string> PreviousGroupBy,
    GroupChangeReason Reason
);

public enum GroupChangeReason
{
    UserDragToGroupBar,
    UserRemoveFromGroupBar,
    UserReorderGroupBar,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

[Parameter] public EventCallback<GroupChangedContext> OnGroupChanged { get; set; }
```

### §23.6.2 — `OnGroupExpand` / `OnGroupCollapse`

```csharp
public sealed record GroupExpandedContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    bool RequiresContentFetch,
    GroupExpandReason Reason
);

public sealed record GroupCollapsedContext(
    string GroupPath,
    string ColumnKey,
    object? GroupValue,
    GroupCollapseReason Reason
);

[Parameter] public EventCallback<GroupExpandedContext> OnGroupExpand { get; set; }
[Parameter] public EventCallback<GroupCollapsedContext> OnGroupCollapse { get; set; }
```

`OnGroupExpand` with `RequiresContentFetch=true` is the lazy-load trigger (per §9.9.3).

---

## §23.7 — Tree events

Cross-reference §10.11.

```csharp
public sealed record TreeNodeExpandedContext<TItem>(
    TItem Row,
    int Depth,
    bool RequiresContentFetch,
    TreeNodeExpandReason Reason
);

public sealed record TreeNodeCollapsedContext<TItem>(
    TItem Row,
    int Depth,
    TreeNodeCollapseReason Reason
);

[Parameter] public EventCallback<TreeNodeExpandedContext<TItem>> OnTreeNodeExpand { get; set; }
[Parameter] public EventCallback<TreeNodeCollapsedContext<TItem>> OnTreeNodeCollapse { get; set; }
```

---

## §23.8 — Master-detail events

Cross-reference §11.12.

```csharp
public sealed record RowDetailExpandedContext<TItem>(
    TItem Row,
    DetailExpandReason Reason
);

public sealed record RowDetailCollapsedContext<TItem>(
    TItem Row,
    DetailCollapseReason Reason
);

[Parameter] public EventCallback<RowDetailExpandedContext<TItem>> OnRowExpand { get; set; }
[Parameter] public EventCallback<RowDetailCollapsedContext<TItem>> OnRowCollapse { get; set; }
[Parameter] public Func<TItem, Task<bool>>? OnBeforeRowCollapse { get; set; }
```

`OnBeforeRowCollapse` is the interceptor for dirty-detail-form discard prompts (§11.5.1, §11.14.4).

---

## §23.9 — Inline edit events

Cross-reference §12.13.

### §23.9.1 — Row edit lifecycle

```csharp
public sealed record RowEditStartContext<TItem>(TItem Row, bool IsAddNew);
public sealed record RowEditEndContext<TItem>(TItem Row, RowEditEndReason Reason, bool WasAddNew);

public enum RowEditEndReason
{
    UserSaved,
    UserCancelled,
    UserDiscarded,
    Programmatic,
    SaveFailed,
    ConflictResolved
}

[Parameter] public EventCallback<RowEditStartContext<TItem>> OnRowEditStart { get; set; }
[Parameter] public EventCallback<RowEditEndContext<TItem>> OnRowEditEnd { get; set; }
```

### §23.9.2 — Cell edit lifecycle

```csharp
public sealed record CellEditStartContext<TItem>(TItem Row, string ColumnKey);
public sealed record CellEditEndContext<TItem>(TItem Row, string ColumnKey, CellEditEndReason Reason);

public enum CellEditEndReason
{
    UserSaved,
    UserCancelled,
    UserMovedToNextCell,
    UserFocusLoss,
    Programmatic
}

[Parameter] public EventCallback<CellEditStartContext<TItem>> OnCellEditStart { get; set; }
[Parameter] public EventCallback<CellEditEndContext<TItem>> OnCellEditEnd { get; set; }
```

### §23.9.3 — Dirty state

```csharp
public sealed record DirtyStateChangedContext<TItem>(
    TItem Row,
    bool IsDirty,
    IReadOnlyList<string> ChangedFieldKeys
);

[Parameter] public EventCallback<DirtyStateChangedContext<TItem>> OnDirtyStateChanged { get; set; }
```

### §23.9.4 — Save interception and execution

```csharp
[Parameter] public Func<RowEditContext<TItem>, Task<bool>>? OnBeforeRowSave { get; set; }
[Parameter] public Func<RowEditContext<TItem>, Task<SaveResult>>? OnRowSave { get; set; }

[Parameter] public Func<CellEditContext<TItem>, Task<bool>>? OnBeforeCellEdit { get; set; }
[Parameter] public Func<CellEditContext<TItem>, Task<SaveResult>>? OnCellSave { get; set; }
```

`OnBeforeRowSave` / `OnBeforeCellEdit` are interceptors — return false to cancel. `OnRowSave` / `OnCellSave` return `SaveResult` indicating outcome (Success / ValidationError / ConcurrencyConflict / Error).

These are the two most critical edit events. Most edit-enabled tables wire `OnRowSave` (the actual save handler) and optionally `OnBeforeRowSave` (custom pre-save checks).

### §23.9.5 — Add new row

```csharp
[Parameter] public Func<TItem>? OnAddNew { get; set; }    // returns initial TItem for new row
[Parameter] public EventCallback OnAddClick { get; set; } // non-inline add (caller handles modal)
```

Per §16.8 — `OnAddNew` for inline add (returns initial), `OnAddClick` for non-inline (caller opens modal). If both are configured, `OnAddClick` wins (caller's explicit handler takes precedence).

---

## §23.10 — Lifecycle events

### §23.10.1 — `OnRefresh`

```csharp
[Parameter] public EventCallback OnRefresh { get; set; }
```

Fires when the refresh button is clicked (toolbar §16.5) OR `RefreshAsync()` is called programmatically.

For server-side mode, this is the right place to re-fetch external data not directly served by `DataSource`. For client-side mode, the caller's handler should refresh `Items`.

### §23.10.2 — `OnDensityChanged`

```csharp
public sealed record DensityChangedContext(
    TableDensity NewDensity,
    TableDensity PreviousDensity,
    DensityChangeReason Reason
);

public enum DensityChangeReason
{
    UserToggle,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}

[Parameter] public EventCallback<DensityChangedContext> OnDensityChanged { get; set; }
```

### §23.10.3 — Column ops events

Cross-reference §13.2.8, §13.3.7, §13.4.9.

```csharp
[Parameter] public EventCallback<ColumnResizedContext> OnColumnResized { get; set; }
[Parameter] public EventCallback<ColumnReorderedContext> OnColumnReordered { get; set; }
[Parameter] public EventCallback<ColumnPinnedContext> OnColumnPinned { get; set; }
[Parameter] public EventCallback<ColumnVisibilityChangedContext> OnColumnVisibilityChanged { get; set; }
```

The last one (`OnColumnVisibilityChanged`) covers user toggling visibility via the column picker:

```csharp
public sealed record ColumnVisibilityChangedContext(
    string ColumnKey,
    bool IsVisible,
    ColumnVisibilityChangeReason Reason
);

public enum ColumnVisibilityChangeReason
{
    UserColumnPicker,
    Programmatic,
    PersistenceRestore,
    DefaultApplied,
    ResetToDefault
}
```

### §23.10.4 — Export events

Cross-reference §17.10.

```csharp
[Parameter] public Func<BeforeExportContext, Task<bool>>? OnBeforeExport { get; set; }
[Parameter] public EventCallback<AfterExportContext> OnAfterExport { get; set; }
```

`OnBeforeExport` returns false to cancel (e.g., for background-job hand-off per §17.9.3). `OnAfterExport` fires after success or cancellation; `AfterExportContext` includes the file size, duration, and `Cancelled` flag.

### §23.10.5 — `OnError`

Cross-reference §18.4.6.

```csharp
public sealed record TableErrorContext(
    string Message,
    Exception? Exception,
    TableErrorSource Source
);

public enum TableErrorSource
{
    DataSourceException,
    LoadErrorParameter,
    OnRowSaveException,
    OnBeforeExportException,
    GenericInternal
}

[Parameter] public EventCallback<TableErrorContext> OnError { get; set; }
```

### §23.10.6 — `OnQueryComplete`

```csharp
public sealed record QueryCompleteContext<TItem>(
    long ItemCount,
    long TotalCount,
    TimeSpan Duration,
    QueryCompleteReason Reason,
    TableQueryRequest? Request,
    TableQueryResponse<TItem>? Response
);

public enum QueryCompleteReason
{
    InitialLoad,
    Refresh,
    SortChange,
    FilterChange,
    PageChange,
    PageSizeChange,
    GroupChange,
    QuickSearchChange
}

[Parameter] public EventCallback<QueryCompleteContext<TItem>> OnQueryComplete { get; set; }
```

Fires after a `DataSource` invocation completes (server-side mode) OR after a client-side filter/sort recomputation. Includes the full request/response for debugging and the duration for performance tracking.

For client-side mode, `Request` and `Response` are null; only `ItemCount` and `Duration` are meaningful.

### §23.10.7 — `OnPersisted`

Cross-reference §21.10.3.

```csharp
public sealed record PersistedContext(
    string TableId,
    TablePreferences NewPrefs,
    PersistedTrigger Trigger
);

public enum PersistedTrigger
{
    ColumnLayoutChange,
    SortChange,
    FilterChange,
    DensityChange,
    PageSizeChange,
    GroupChange,
    Reset
}

[Parameter] public EventCallback<PersistedContext> OnPersisted { get; set; }
```

Fires after each successful persistence write. Use case: audit log of UI preference changes (rare).

### §23.10.8 — `OnPreferencesReset`

```csharp
[Parameter] public EventCallback OnPreferencesReset { get; set; }
```

Fires after `ResetPreferencesAsync()` completes — toast notification, analytics, etc.

---

## §23.11 — Event ordering

When a single user action triggers multiple events, the firing order is deterministic:

### §23.11.1 — User saves a dirty row (Row mode)

1. User clicks Save (or presses Enter on Save button)
2. Per-field validation runs (internal, no event)
3. Per-row validation via `EditValidator` (internal)
4. `OnBeforeRowSave` fires — if returns false, sequence aborts
5. Built-in critical-field confirm modal opens (if any)
6. After confirm: `OnRowSave` fires
7. After server response:
   - Success → `OnDirtyStateChanged(isDirty: false)` → `OnRowEditEnd(reason: UserSaved)`
   - Conflict → `OnRowEditEnd(reason: ConflictResolved)` only if user picks Reload; otherwise stays in edit
   - Error → `OnError(source: OnRowSaveException)` → row stays in edit

### §23.11.2 — User changes page

1. User clicks pagination
2. If row is dirty → discard confirm fires:
   - User confirms discard → `OnDirtyStateChanged(false)` → `OnRowEditEnd(UserDiscarded)` → continue
   - User cancels → sequence aborts; page doesn't change
3. `OnPageChanged` fires
4. `DataSource` invoked (server-side) OR data re-filtered (client-side)
5. `OnQueryComplete` fires when complete

### §23.11.3 — User applies a filter

1. User clicks Apply in filter popover (or removes filter chip)
2. If row is dirty → discard confirm (as above)
3. `OnFilterChanged` fires
4. Page resets to 1 (per §8.13.1) → `OnPageChanged(reason: ResetAfterFilter)` fires
5. `DataSource` invoked → `OnQueryComplete` fires
6. Persistence write scheduled (immediate, per §21.5.1)
7. After write → `OnPersisted(trigger: FilterChange)` fires

### §23.11.4 — User selects a row

1. User clicks checkbox
2. `OnSelectionChanged` fires with the new SelectedItems + added item

For Shift+Click extending selection across many rows:
- Single `OnSelectionChanged` fires with all added/removed items, not one per row

For "select all on page":
1. User clicks header checkbox
2. `OnSelectionChanged` fires with all page rows added

For "select all across pages" (step 2):
1. User clicks the banner action
2. `OnSelectAllAcrossPagesChanged(true)` fires
3. `OnSelectionChanged` fires (conceptually, "all rows are now selected")

### §23.11.5 — User cancels a dirty edit

1. User clicks Cancel (or presses Escape)
2. Discard confirm fires (if dirty)
3. User confirms:
   - `OnDirtyStateChanged(false)` fires
   - `OnRowEditEnd(reason: UserCancelled)` fires
4. User cancels the confirm → row stays in edit; no events fire

---

## §23.12 — Event handler exception handling

If a caller's event handler throws an exception:

- LipiTable catches it
- Logs the exception via `ILogger<LipiTable<TItem>>`
- Continues normally (the action that triggered the event still completes)
- Does NOT show a user-facing error (handler exceptions are programming bugs, not user errors)

For interceptor events (`OnBeforeRowSave`, `OnBeforeCellEdit`, `OnBeforeRowCollapse`, `OnBeforeExport`):
- If the interceptor throws, LipiTable treats it as "interceptor returned false" (cancels the action)
- Logs the exception
- Optionally fires `OnError(source: GenericInternal)`

This makes LipiTable robust to consuming-page bugs — a bad handler doesn't break the table's state machine.

---

## §23.13 — Async event handlers

All event handlers can be async:

```razor
<LipiTable OnRowClick="@HandleRowClickAsync" ...>

@code {
    private async Task HandleRowClickAsync(Patient p)
    {
        await _service.LogClickAsync(p);
        Nav.NavigateTo($"/patients/{p.Id}");
    }
}
```

LipiTable awaits async handlers. For events that block subsequent UI (e.g., `OnBeforeRowSave`), the handler's duration delays the save UI — caller should keep them fast or use background jobs for long-running work.

For events that don't block UI (e.g., `OnRowClick` for analytics logging), the await is non-blocking from the user's perspective — UI continues normally while the async handler runs.

`EventCallback<T>` invocations include automatic `StateHasChanged()` — caller doesn't need to call it explicitly in handlers that update component state.

---

## §23.14 — Event handler binding patterns

### §23.14.1 — Method group

Most concise:

```razor
<LipiTable OnRowClick="HandleRowClick" ...>

@code {
    private void HandleRowClick(Patient p) => Nav.NavigateTo($"/patients/{p.Id}");
}
```

### §23.14.2 — Lambda

Inline:

```razor
<LipiTable OnRowClick="@(p => Nav.NavigateTo($"/patients/{p.Id}"))" ...>
```

### §23.14.3 — EventCallback.Factory.Create

For programmatically-created callbacks (less common):

```csharp
@code {
    private EventCallback<Patient> _onClick;
    
    protected override void OnInitialized()
    {
        _onClick = EventCallback.Factory.Create<Patient>(this, HandleRowClick);
    }
}
```

### §23.14.4 — Two-way binding shorthand

For events that pair with a single state value:

```razor
<LipiTable @bind-SelectedItems="_selected" ...>
```

Equivalent to:

```razor
<LipiTable SelectedItems="@_selected" 
           SelectedItemsChanged="@(items => _selected = items)" 
           ...>
```

Two-way bindings supported:
- `@bind-SelectedItems`
- `@bind-Density`
- `@bind-PageSize`
- `@bind-Page`
- `@bind-QuickSearchText`

Each has a corresponding `*Changed` event that fires when the value changes.

---

## §23.15 — Disabling events

Not applicable — all events are opt-in (caller doesn't subscribe = no notification). To "disable" an event, just don't pass the parameter.

For interceptor events that affect behavior even when present (e.g., `OnBeforeRowSave` can cancel saves), the only way to disable interception is to leave the parameter null. There's no "enabled" / "disabled" flag on individual events.

---

## §23.16 — Event summary table

Quick reference for all events:

| Event | Type | Section | Common use |
|---|---|---|---|
| `OnRowClick` | `EventCallback<TItem>` | §23.2.1 | Navigate to detail |
| `OnRowDoubleClick` | `EventCallback<TItem>` | §23.2.2 | Custom double-click action |
| `OnRowFocus` | `EventCallback<TItem>` | §23.2.4 | Sidebar sync |
| `OnSelectionChanged` | `EventCallback<SelectionChangedContext<TItem>>` | §23.3.1 | Track selection |
| `OnSelectAllAcrossPagesChanged` | `EventCallback<bool>` | §23.3.2 | Bulk-all UX |
| `OnSortChanged` | `EventCallback<SortChangedContext>` | §23.4.1 | URL sync |
| `OnFilterChanged` | `EventCallback<FilterChangedContext>` | §23.4.2 | URL sync |
| `OnQuickSearchChanged` | `EventCallback<QuickSearchChangedContext>` | §23.4.3 | URL sync |
| `OnPageChanged` | `EventCallback<PageChangedContext>` | §23.5.1 | URL sync |
| `OnPageSizeChanged` | `EventCallback<PageSizeChangedContext>` | §23.5.2 | Analytics |
| `OnGroupChanged` | `EventCallback<GroupChangedContext>` | §23.6.1 | URL sync |
| `OnGroupExpand` | `EventCallback<GroupExpandedContext>` | §23.6.2 | Lazy-load groups |
| `OnGroupCollapse` | `EventCallback<GroupCollapsedContext>` | §23.6.2 | — |
| `OnTreeNodeExpand` | `EventCallback<TreeNodeExpandedContext<TItem>>` | §23.7 | Lazy-load tree |
| `OnTreeNodeCollapse` | `EventCallback<TreeNodeCollapsedContext<TItem>>` | §23.7 | — |
| `OnRowExpand` | `EventCallback<RowDetailExpandedContext<TItem>>` | §23.8 | Lazy-load detail |
| `OnRowCollapse` | `EventCallback<RowDetailCollapsedContext<TItem>>` | §23.8 | — |
| `OnBeforeRowCollapse` | `Func<TItem, Task<bool>>?` | §23.8 | Dirty-form discard |
| `OnRowEditStart` | `EventCallback<RowEditStartContext<TItem>>` | §23.9.1 | Audit log |
| `OnRowEditEnd` | `EventCallback<RowEditEndContext<TItem>>` | §23.9.1 | Audit log |
| `OnCellEditStart` | `EventCallback<CellEditStartContext<TItem>>` | §23.9.2 | Bulk-edit UX |
| `OnCellEditEnd` | `EventCallback<CellEditEndContext<TItem>>` | §23.9.2 | — |
| `OnDirtyStateChanged` | `EventCallback<DirtyStateChangedContext<TItem>>` | §23.9.3 | "Save changes" button |
| `OnBeforeRowSave` | `Func<RowEditContext<TItem>, Task<bool>>?` | §23.9.4 | Custom pre-save logic |
| `OnRowSave` | `Func<RowEditContext<TItem>, Task<SaveResult>>?` | §23.9.4 | The save handler |
| `OnBeforeCellEdit` | `Func<CellEditContext<TItem>, Task<bool>>?` | §23.9.4 | Custom pre-cell-save logic |
| `OnCellSave` | `Func<CellEditContext<TItem>, Task<SaveResult>>?` | §23.9.4 | Cell save handler |
| `OnAddNew` | `Func<TItem>?` | §23.9.5 | Initial value for inline add |
| `OnAddClick` | `EventCallback` | §23.9.5 | Open add-modal |
| `OnRefresh` | `EventCallback` | §23.10.1 | Refresh external data |
| `OnDensityChanged` | `EventCallback<DensityChangedContext>` | §23.10.2 | Analytics |
| `OnColumnResized` | `EventCallback<ColumnResizedContext>` | §23.10.3 | Analytics |
| `OnColumnReordered` | `EventCallback<ColumnReorderedContext>` | §23.10.3 | — |
| `OnColumnPinned` | `EventCallback<ColumnPinnedContext>` | §23.10.3 | — |
| `OnColumnVisibilityChanged` | `EventCallback<ColumnVisibilityChangedContext>` | §23.10.3 | — |
| `OnBeforeExport` | `Func<BeforeExportContext, Task<bool>>?` | §23.10.4 | Permission check, BG-job |
| `OnAfterExport` | `EventCallback<AfterExportContext>` | §23.10.4 | Audit log |
| `OnError` | `EventCallback<TableErrorContext>` | §23.10.5 | App-level error logging |
| `OnQueryComplete` | `EventCallback<QueryCompleteContext<TItem>>` | §23.10.6 | Performance tracking |
| `OnPersisted` | `EventCallback<PersistedContext>` | §23.10.7 | UI preference audit |
| `OnPreferencesReset` | `EventCallback` | §23.10.8 | Toast notification |

Total: **40 events** across all groups. The Row interaction + Selection + Sort/Filter + Pagination + Edit events are the most commonly wired; the remaining are specialized.

---

## §23.17 — Worked example: comprehensive event wiring

Realistic clinical-list page with most events wired:

```razor
<LipiTable @ref="_table"
           TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           TableId="patients-list"
           SelectionMode="SelectionMode.Multi"
           EditMode="TableEditMode.Row"
           ShowExportButton="true"
           
           @bind-SelectedItems="_selected"
           
           OnRowClick="@(p => Nav.NavigateTo($"/patients/{p.Id}"))"
           OnRowEditStart="@HandleEditStart"
           OnRowEditEnd="@HandleEditEnd"
           OnBeforeRowSave="@CheckEditPermissions"
           OnRowSave="@SavePatientAsync"
           
           OnBeforeExport="@AuditExportStart"
           OnAfterExport="@AuditExportEnd"
           
           OnError="@HandleError"
           OnQueryComplete="@TrackQueryPerformance">
    
    <LipiColumn Field="@(p => p.Uhid)" Pinned="ColumnPin.Left" />
    <LipiColumn Field="@(p => p.Name)" Pinned="ColumnPin.Left" />
    <LipiColumn Field="@(p => p.Mobile)" />
    <LipiColumn Field="@(p => p.Status)" Type="ColumnType.Status" />
</LipiTable>

@code {
    private LipiTable<Patient>? _table;
    private IReadOnlyList<Patient> _selected = Array.Empty<Patient>();
    
    private async Task<TableQueryResponse<Patient>> LoadPatientsAsync(TableQueryRequest req, CancellationToken ct)
    {
        return await _service.QueryAsync(req, ct);
    }
    
    private async Task HandleEditStart(RowEditStartContext<Patient> ctx)
    {
        await _auditLog.LogEditStartAsync(_currentUser, "Patient", ctx.Row.Id);
    }
    
    private async Task HandleEditEnd(RowEditEndContext<Patient> ctx)
    {
        await _auditLog.LogEditEndAsync(_currentUser, "Patient", ctx.Row.Id, ctx.Reason);
    }
    
    private async Task<bool> CheckEditPermissions(RowEditContext<Patient> ctx)
    {
        return await _authz.UserCanEdit(_currentUser, ctx.Current);
    }
    
    private async Task<SaveResult> SavePatientAsync(RowEditContext<Patient> ctx)
    {
        try
        {
            await _service.UpdateAsync(ctx.Current, (byte[])ctx.OriginalRowVersion!);
            return new SaveResult { Outcome = SaveOutcome.Success };
        }
        catch (DbUpdateConcurrencyException)
        {
            var current = await _service.GetByIdAsync(ctx.Current.Id);
            return new SaveResult 
            { 
                Outcome = SaveOutcome.ConcurrencyConflict, 
                Conflict = new ConflictInfo { ServerRow = current, ... }
            };
        }
    }
    
    private async Task<bool> AuditExportStart(BeforeExportContext ctx)
    {
        if (!await _authz.UserCanExport(_currentUser))
        {
            await ToastService.ShowAsync("You don't have export permission.");
            return false;
        }
        return true;
    }
    
    private async Task AuditExportEnd(AfterExportContext ctx)
    {
        if (!ctx.Cancelled)
        {
            await _auditLog.LogExportAsync(_currentUser, "Patient", ctx);
        }
    }
    
    private void HandleError(TableErrorContext ctx)
    {
        _logger.LogError(ctx.Exception, "Table error: {Message}", ctx.Message);
    }
    
    private void TrackQueryPerformance(QueryCompleteContext<Patient> ctx)
    {
        _telemetry.RecordQueryDuration(ctx.Duration, ctx.ItemCount);
    }
}
```

Covers: navigation on click, multi-selection binding, audit log for edits, permission checks, optimistic concurrency on save, export permission and audit, error logging, performance tracking.

---

*End of §23. Proceed to §25 — Component isolation contract.*


# LipiTable Spec — §24 Multi-industry tokens

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** §1, §2 (anatomy + z-index layers)

---

## §24.1 — Token philosophy

All LipiTable CSS variables live in the `--lipi-table-*` namespace and are defined in `wwwroot/css/lipi-table.css`. The structure follows LiPi's established token cascade:

1. **Baseline tokens** (`00-baseline.css`) — generic LiPi values: `--color-*`, `--sp-*`, `--r-*`, `--font-*`, `--tr-*`, `--sh-*`. These are theme-agnostic structure.
2. **Mode tokens** (`mode-light.css` / `mode-dark.css`) — concrete color values per mode. Override `--color-*` tokens.
3. **Brand tokens** (`brand-lipi.css`) — LiPi-specific overrides for primary / accent. Override `--color-primary-*` etc.
4. **Component tokens** (`lipi-table.css`) — LipiTable-specific values, composed from baseline + mode tokens. This section's scope.
5. **Shared component tokens** (`lipi-status-tokens.css`) — phase-2.8 shared infrastructure used by LipiTable and other components.

**Cascade rule:** component tokens reference upstream tokens via `var()` with fallback. A new theme can override mode tokens without touching `lipi-table.css`. LipiTable code never hardcodes a color value.

```css
/* GOOD */
--lipi-table-header-bg: var(--color-surface-secondary, #F8F9FA);

/* BAD — bypasses theme cascade */
--lipi-table-header-bg: #F8F9FA;
```

The `, #F8F9FA` fallback exists for safety only — in normal operation the baseline token resolves and the fallback is never used. The fallback prevents broken rendering if baseline tokens are missing during initial load.

---

## §24.2 — Token reference: structural

These tokens control sizing, spacing, and layout. They are **mode-independent** (no light/dark variation).

### §24.2.1 — Row heights per density

```css
--lipi-table-row-h-compact:       32px;
--lipi-table-row-h-comfortable:   40px;
--lipi-table-row-h-spacious:      52px;
--lipi-table-row-h:               var(--lipi-table-row-h-comfortable);  /* current density */
```

Row height includes vertical padding. The active density swaps `--lipi-table-row-h` via a class on the table root (`.lipi-table--compact`, `.lipi-table--comfortable`, `.lipi-table--spacious`).

### §24.2.2 — Header height per density

```css
--lipi-table-header-h-compact:      36px;
--lipi-table-header-h-comfortable:  44px;
--lipi-table-header-h-spacious:     52px;
--lipi-table-header-h:              var(--lipi-table-header-h-comfortable);
```

Header is typically slightly taller than body rows for affordance.

### §24.2.3 — Cell padding per density

```css
--lipi-table-cell-pad-y-compact:      4px;
--lipi-table-cell-pad-y-comfortable:  8px;
--lipi-table-cell-pad-y-spacious:     12px;

--lipi-table-cell-pad-x-compact:      8px;
--lipi-table-cell-pad-x-comfortable:  12px;
--lipi-table-cell-pad-x-spacious:     16px;

--lipi-table-cell-pad-y:              var(--lipi-table-cell-pad-y-comfortable);
--lipi-table-cell-pad-x:              var(--lipi-table-cell-pad-x-comfortable);
```

Cell padding affects content positioning; row height contains padding + content + line-height.

### §24.2.4 — Font sizes per density

```css
--lipi-table-font-compact:       11px;
--lipi-table-font-comfortable:   13px;
--lipi-table-font-spacious:      14px;
--lipi-table-font:               var(--lipi-table-font-comfortable);

--lipi-table-font-header:        12px;  /* always uniform; doesn't scale with density */
--lipi-table-font-header-weight: 500;
```

Header font size is intentionally consistent across densities — the header is structural; only the body scales.

### §24.2.5 — Column-specific widths

```css
--lipi-table-col-select-w-compact:      36px;
--lipi-table-col-select-w-comfortable:  44px;
--lipi-table-col-select-w-spacious:     52px;
--lipi-table-col-select-w:              var(--lipi-table-col-select-w-comfortable);

--lipi-table-col-expand-w-compact:      28px;
--lipi-table-col-expand-w-comfortable:  36px;
--lipi-table-col-expand-w-spacious:     44px;
--lipi-table-col-expand-w:              var(--lipi-table-col-expand-w-comfortable);

--lipi-table-col-actions-w:             auto;  /* content-sized */
--lipi-table-col-min-w:                 40px;  /* hard floor; no column narrower */
```

The selection column and expand-chevron column have fixed widths per density. The actions column auto-sizes to content.

### §24.2.6 — Tree indent

```css
--lipi-table-tree-indent-compact:       16px;
--lipi-table-tree-indent-comfortable:   24px;
--lipi-table-tree-indent-spacious:      32px;
--lipi-table-tree-indent:               var(--lipi-table-tree-indent-comfortable);
```

Indent per tree depth level. Caller can override via the `TreeIndent` parameter (§10).

### §24.2.7 — Toolbar / footer / chrome heights

```css
--lipi-table-toolbar-h:              48px;
--lipi-table-toolbar-gap:            8px;
--lipi-table-footer-h-min:           44px;  /* minimum; can grow with content */
--lipi-table-bulk-bar-h:             48px;
--lipi-table-filter-chips-h-min:     36px;
--lipi-table-group-bar-h:            44px;
--lipi-table-edit-stickybar-h:       56px;
--lipi-table-header-band-pad:        16px 16px 12px;
```

The header band, toolbar, filter chips, bulk bar, group bar, footer, and sticky edit bar each have fixed minimum heights. They wrap content if necessary but render at the minimum when content is short.

### §24.2.8 — Border widths

```css
--lipi-table-border-w:                 0.5px;
--lipi-table-border-w-strong:          1px;
--lipi-table-divider-w:                1px;       /* between rows */
--lipi-table-status-strip-w:           var(--lipi-status-strip-width, 4px);  /* shared */
--lipi-table-resize-handle-w:          4px;
--lipi-table-focus-ring-w:             2px;
```

LipiTable uses thinner borders than baseline (0.5px) for visual lightness; LipiTable internal dividers (between rows) use 1px because half-pixel rules don't always render predictably on body rows.

### §24.2.9 — Border radii

```css
--lipi-table-r:                  var(--r-md, 8px);     /* outer container */
--lipi-table-r-toolbar-btn:      var(--r-sm, 4px);
--lipi-table-r-chip:             var(--r-pill, 999px);
--lipi-table-r-badge:            var(--r-sm, 4px);
--lipi-table-r-popover:          var(--r-md, 8px);
```

The outer table container has a rounded corner. Internal elements use smaller radii.

### §24.2.10 — Animation durations

```css
--lipi-table-tr-fast:        var(--tr-fast, 120ms);     /* hover, focus */
--lipi-table-tr-base:        var(--tr-base, 180ms);     /* row state changes */
--lipi-table-tr-slow:        var(--tr-slow, 300ms);     /* bulk-bar appearance, banner */
--lipi-table-tr-skeleton:    1600ms;                    /* shimmer cycle */
--lipi-table-tr-easing:      var(--tr-easing, cubic-bezier(0.4, 0, 0.2, 1));
```

All transitions respect `prefers-reduced-motion: reduce` — when the user requests reduced motion, durations collapse to 0ms.

---

## §24.3 — Token reference: colors (mode-aware)

These tokens reference baseline color tokens, which are themselves mode-aware. The LipiTable layer composes them into semantic combinations.

### §24.3.1 — Background colors

```css
--lipi-table-bg:                       var(--color-surface-primary,   #FFFFFF);
--lipi-table-bg-header:                var(--color-surface-secondary, #F8F9FA);
--lipi-table-bg-row:                   var(--color-surface-primary,   #FFFFFF);
--lipi-table-bg-row-alt:               var(--color-surface-secondary, #F8F9FA);  /* striped rows */
--lipi-table-bg-row-hover:             var(--color-surface-hover,     rgba(0,0,0,0.03));
--lipi-table-bg-row-focus:             var(--color-surface-focus,     rgba(0,0,0,0.04));
--lipi-table-bg-row-selected:          var(--color-primary-alpha-08,  rgba(27,77,160,0.08));
--lipi-table-bg-row-selected-hover:    var(--color-primary-alpha-12,  rgba(27,77,160,0.12));
--lipi-table-bg-row-editing:           var(--color-warning-alpha-08,  rgba(245,158,11,0.08));
--lipi-table-bg-row-conflict:          var(--color-danger-alpha-08,   rgba(239,68,68,0.08));
--lipi-table-bg-row-new:               var(--color-success-alpha-06,  rgba(16,185,129,0.06));
--lipi-table-bg-row-disabled:          var(--color-surface-disabled,  #F1F3F5);
--lipi-table-bg-group-row:             var(--color-surface-secondary, #F8F9FA);
--lipi-table-bg-detail:                var(--color-surface-secondary, #F8F9FA);
```

The `-alpha-*` tokens (e.g., `--color-primary-alpha-08`) are mode-aware: in dark mode, the same semantic intent renders differently (typically a lighter tint of the primary at higher alpha to compensate for the darker base). Both `mode-light.css` and `mode-dark.css` define these.

### §24.3.2 — Text colors

```css
--lipi-table-text:                 var(--color-text-primary,   #1A1D21);
--lipi-table-text-muted:           var(--color-text-secondary, #6B7280);
--lipi-table-text-faint:           var(--color-text-tertiary,  #9CA3AF);
--lipi-table-text-link:            var(--color-link,           #2563EB);
--lipi-table-text-link-hover:      var(--color-link-hover,     #1D4ED8);
--lipi-table-text-success:         var(--color-text-success,   #047857);
--lipi-table-text-warning:         var(--color-text-warning,   #B45309);
--lipi-table-text-danger:          var(--color-text-danger,    #B91C1C);
--lipi-table-text-on-primary:      var(--color-on-primary,     #FFFFFF);
--lipi-table-text-header:          var(--color-text-secondary, #6B7280);
```

Header text is intentionally muted (secondary text color) — column names are scaffolding, not content. Data text is primary (high contrast).

### §24.3.3 — Border colors

```css
--lipi-table-border:                 var(--color-border-tertiary,  #E5E7EB);
--lipi-table-border-strong:          var(--color-border-secondary, #D1D5DB);
--lipi-table-border-emphasis:        var(--color-border-primary,   #9CA3AF);
--lipi-table-border-focus:           var(--color-primary-500,      #1B4DA0);
--lipi-table-border-error:           var(--color-danger-500,       #EF4444);
--lipi-table-divider:                var(--color-border-tertiary,  #E5E7EB);  /* between rows */
```

Inter-row dividers use the tertiary (lightest) border. The outer container border is a notch heavier. Focus rings use primary.

### §24.3.4 — Status strip colors (shared)

These come from `lipi-status-tokens.css` (Phase 2.8 shared infrastructure). LipiTable consumes them; it does not define them. Listed here for completeness:

```css
/* Defined in lipi-status-tokens.css */
--color-status-active:        var(--color-success-600);
--color-status-pending:       var(--color-warning-500);
--color-status-inactive:      var(--color-neutral-400);
--color-status-suspended:     var(--color-warning-600);
--color-status-locked:        var(--color-danger-500);
--color-status-archived:      var(--color-neutral-500);
--color-status-draft:         var(--color-neutral-400);
--color-status-published:     var(--color-success-500);
--color-status-in-progress:   var(--color-info-500);
--color-status-completed:     var(--color-success-600);
--color-status-failed:        var(--color-danger-500);
--color-status-cancelled:     var(--color-neutral-500);
--color-status-warning:       var(--color-warning-500);
--color-status-error:         var(--color-danger-500);
--color-status-info:          var(--color-info-500);
--color-status-success:       var(--color-success-500);
--color-status-unknown:       var(--color-neutral-400);
```

LipiTable renders the status strip via the shared `lipi-status-strip-left` utility class plus `data-status="..."` attribute on row elements. The status string maps to the corresponding color token via attribute selectors in the shared CSS file (see Phase 2.8 overview §2.3).

### §24.3.5 — Pin shadow colors

```css
--lipi-table-pin-shadow-left:    inset 8px 0 8px -8px var(--color-shadow-strong, rgba(0,0,0,0.12));
--lipi-table-pin-shadow-right:   inset -8px 0 8px -8px var(--color-shadow-strong, rgba(0,0,0,0.12));
```

Pinned columns cast a subtle shadow into the scrollable region indicating that more content lies beyond. Dark-mode override uses a higher-alpha shadow.

### §24.3.6 — Skeleton (loading) colors

```css
--lipi-table-skeleton-base:        var(--color-surface-disabled,  #F1F3F5);
--lipi-table-skeleton-highlight:   var(--color-surface-elevated,  #FFFFFF);
--lipi-table-skeleton-bg:          linear-gradient(
                                       90deg,
                                       var(--lipi-table-skeleton-base) 0%,
                                       var(--lipi-table-skeleton-highlight) 50%,
                                       var(--lipi-table-skeleton-base) 100%
                                   );
```

The shimmer effect is a gradient that animates horizontally. Dark mode uses lighter base + darker highlight for inverted shimmer.

### §24.3.7 — Bulk action bar colors

```css
--lipi-table-bulk-bar-bg:        var(--color-primary-alpha-08, rgba(27,77,160,0.08));
--lipi-table-bulk-bar-border:    var(--color-primary-alpha-20, rgba(27,77,160,0.20));
--lipi-table-bulk-bar-text:      var(--color-primary-700,      #1E3A8A);
```

In dark mode the alphas shift higher to remain visible against the dark surface.

### §24.3.8 — Conflict UI colors

```css
--lipi-table-conflict-banner-bg:        var(--color-danger-alpha-08,  rgba(239,68,68,0.08));
--lipi-table-conflict-banner-border:    var(--color-danger-alpha-20,  rgba(239,68,68,0.20));
--lipi-table-conflict-banner-text:      var(--color-danger-700,       #991B1B);
--lipi-table-conflict-diff-changed:     var(--color-warning-alpha-12, rgba(245,158,11,0.12));
--lipi-table-conflict-diff-yours:       var(--color-info-700,         #1E40AF);
--lipi-table-conflict-diff-theirs:      var(--color-warning-700,      #B45309);
```

Yours / theirs use distinct semantic colors (info-blue / warning-amber) so the diff is readable without relying on labels alone — color is one of three signals (label + position + color).

### §24.3.9 — Edit mode colors

```css
--lipi-table-edit-row-bg:           var(--color-warning-alpha-04, rgba(245,158,11,0.04));
--lipi-table-edit-row-border:       var(--color-warning-alpha-20, rgba(245,158,11,0.20));
--lipi-table-edit-save-bg:          var(--color-primary-600,      #1B4DA0);
--lipi-table-edit-save-fg:          var(--color-on-primary,       #FFFFFF);
--lipi-table-edit-cancel-bg:        transparent;
--lipi-table-edit-cancel-border:    var(--color-border-secondary, #D1D5DB);
--lipi-table-edit-error-strip-bg:   var(--color-danger-alpha-08,  rgba(239,68,68,0.08));
--lipi-table-edit-error-strip-text: var(--color-danger-700,       #991B1B);
```

The edit-row background is a very subtle warning-amber tint — communicates "this row is in a special mode" without being aggressive. The validation-error strip above the row is more saturated to draw attention.

### §24.3.10 — Filter chip colors

```css
--lipi-table-chip-bg:           var(--color-surface-elevated, #F8FAFC);
--lipi-table-chip-border:       var(--color-border-tertiary,  #E5E7EB);
--lipi-table-chip-text:         var(--color-text-primary,     #1A1D21);
--lipi-table-chip-text-op:      var(--color-text-secondary,   #6B7280);
--lipi-table-chip-x-color:      var(--color-text-tertiary,    #9CA3AF);
--lipi-table-chip-x-hover:      var(--color-danger-500,       #EF4444);
--lipi-table-chip-clear-all:    var(--color-primary-600,      #1B4DA0);
```

### §24.3.11 — Group bar colors

```css
--lipi-table-group-bar-bg:           var(--color-surface-secondary, #F8F9FA);
--lipi-table-group-bar-border:       var(--color-border-tertiary,   #E5E7EB);
--lipi-table-group-bar-border-style: dashed;  /* visual cue for drop target */
--lipi-table-group-bar-hint:         var(--color-text-tertiary,     #9CA3AF);
--lipi-table-group-bar-drop:         var(--color-primary-alpha-12,  rgba(27,77,160,0.12));
--lipi-table-group-chip-bg:          var(--color-surface-primary,   #FFFFFF);
--lipi-table-group-chip-border:      var(--color-border-secondary,  #D1D5DB);
```

The group bar uses dashed border to signal "drop target." When the user starts dragging a column, the border switches to solid and the background highlights.

---

## §24.4 — Token reference: typography

LipiTable inherits the LiPi font family unless overridden:

```css
--lipi-table-font-family:        var(--font-sans);
--lipi-table-font-mono:          var(--font-mono);  /* for Mono column type, IDs, codes */
--lipi-table-font-tabular:       'tabular-nums';    /* CSS feature flag for numeric alignment */

--lipi-table-line-height:        1.4;
--lipi-table-letter-spacing:     0;
--lipi-table-letter-spacing-header:  0.3px;  /* small uppercase emphasis on headers */
```

The `tabular-nums` feature is applied to Number / Currency cells so columns of numbers align vertically by digit position regardless of glyph width. Critical for financial tables.

---

## §24.5 — Token reference: z-index

Cross-reference §2.5. Restated here as tokens:

```css
--lipi-table-z-base:              0;
--lipi-table-z-pinned:            1;
--lipi-table-z-header:            2;
--lipi-table-z-pinned-header:     3;   /* pinned + header intersection */
--lipi-table-z-group-row:         4;
--lipi-table-z-stickybar:         5;
--lipi-table-z-popover:           10;  /* filter, column picker */
--lipi-table-z-conflict-popover:  20;
```

These are local to LipiTable's stacking context. Modals and drawers (LipiModal / LipiDrawer with their own z-index scale at 1000+) compose above this stack.

---

## §24.6 — Light vs dark mode

The mode switch happens at the `--color-*` baseline tokens, not at LipiTable's `--lipi-table-*` tokens. LipiTable's tokens reference baseline tokens via `var()`, so changing mode changes every LipiTable color simultaneously without LipiTable-specific code.

Worked example — row hover background:

```css
/* In lipi-table.css (mode-agnostic): */
--lipi-table-bg-row-hover: var(--color-surface-hover, rgba(0,0,0,0.03));

/* In mode-light.css: */
:root {
    --color-surface-hover: rgba(0,0,0,0.03);
}

/* In mode-dark.css: */
:root {
    --color-surface-hover: rgba(255,255,255,0.04);
}
```

When the user toggles modes, only `mode-light.css` / `mode-dark.css` is swapped. `lipi-table.css` stays loaded; LipiTable's tokens automatically re-resolve.

### §24.6.1 — Light-mode-specific overrides

A few LipiTable tokens have slightly different optimal values in light vs dark that aren't covered by simple inversion. These overrides live in light-mode and dark-mode dedicated blocks within `lipi-table.css`:

```css
/* Default (light mode assumed) */
.lipi-table {
    --lipi-table-pin-shadow-left:  inset 8px 0 8px -8px rgba(0,0,0,0.12);
}

/* Dark mode adjustment */
.theme-dark .lipi-table {
    --lipi-table-pin-shadow-left:  inset 8px 0 8px -8px rgba(0,0,0,0.40);
}
```

Pin shadows need higher alpha in dark mode to remain visible against the dark surface. Light mode's softer shadow looks gentle; dark mode's needs more contrast.

### §24.6.2 — Mode-specific tokens summary

Tokens that have explicit dark-mode overrides (not just baseline cascade):

| Token | Reason |
|---|---|
| `--lipi-table-pin-shadow-left` / `-right` | Shadow alpha boost for dark |
| `--lipi-table-skeleton-base` / `-highlight` | Inverted gradient direction |
| `--lipi-table-bulk-bar-bg` | Higher alpha for visibility |
| `--lipi-table-conflict-banner-bg` | Higher alpha for visibility |
| `--lipi-table-edit-row-bg` | Higher alpha for visibility |

All other tokens rely on the baseline cascade (mode-light.css / mode-dark.css swap their referenced `--color-*` tokens).

---

## §24.7 — High contrast mode

When the user OS sets `prefers-contrast: more`, LipiTable applies a high-contrast variant:

```css
@media (prefers-contrast: more) {
    .lipi-table {
        --lipi-table-border:                var(--color-border-emphasis);     /* heavier */
        --lipi-table-border-w:              1px;                              /* not 0.5 */
        --lipi-table-divider-w:             1.5px;                            /* not 1 */
        --lipi-table-bg-row-hover:          var(--color-surface-strong);
        --lipi-table-bg-row-selected:       var(--color-primary-alpha-20);    /* higher alpha */
        --lipi-table-focus-ring-w:          3px;                              /* not 2 */
    }
}
```

The intent: improve discrimination between elements at the cost of visual subtlety. Users who request high contrast are typically prioritizing readability over aesthetics; LipiTable obliges.

WCAG AAA contrast (7:1 for normal text, 4.5:1 for large) is targeted in high-contrast mode. Normal LipiTable rendering targets WCAG AA (4.5:1 normal text, 3:1 large text + UI components).

---

## §24.8 — Reduced motion

When the user OS sets `prefers-reduced-motion: reduce`, all LipiTable transitions collapse to 0ms:

```css
@media (prefers-reduced-motion: reduce) {
    .lipi-table {
        --lipi-table-tr-fast:        0ms;
        --lipi-table-tr-base:        0ms;
        --lipi-table-tr-slow:        0ms;
        --lipi-table-tr-skeleton:    0ms;  /* shimmer disabled */
    }
}
```

Specific reduced-motion behaviors:
- Bulk action bar appears/disappears instantly (no slide)
- Skeleton shimmer disabled — skeleton renders as flat block
- Row hover state still applies, but instantly (no transition)
- Conflict banner appears instantly
- Drag preview during resize / reorder still functions (drag is direct manipulation, not animation)

---

## §24.9 — Print mode

When `@media print` is active, a different token set applies (cross-reference §17.7):

```css
@media print {
    .lipi-table {
        --lipi-table-bg:                       #FFFFFF;
        --lipi-table-bg-row:                   #FFFFFF;
        --lipi-table-bg-row-alt:               #FFFFFF;       /* striping disabled in print */
        --lipi-table-bg-row-selected:          transparent;   /* hide selection highlight */
        --lipi-table-bg-row-hover:             transparent;
        --lipi-table-text:                     #000000;
        --lipi-table-text-muted:               #444444;
        --lipi-table-border:                   #999999;
        --lipi-table-border-w:                 0.5px;
        --lipi-table-divider-w:                0.5px;
    }
}
```

Plus the print stylesheet hides toolbar, pagination, selection checkboxes, action columns, bulk bar, group bar, sticky edit bar, filter chips, etc. (see §17.7 for the full hide list).

---

## §24.10 — File structure

The token file is `wwwroot/css/lipi-table.css`, organized in this order:

```css
/* lipi-table.css */

/* §1 — Mode-independent structural tokens */
:root {
    --lipi-table-row-h-compact: 32px;
    /* ... all structural tokens ... */
}

/* §2 — Mode-aware color references */
.lipi-table {
    --lipi-table-bg: var(--color-surface-primary, #FFFFFF);
    /* ... all color tokens referencing baseline ... */
}

/* §3 — Density modifiers */
.lipi-table--compact {
    --lipi-table-row-h:   var(--lipi-table-row-h-compact);
    --lipi-table-font:    var(--lipi-table-font-compact);
    /* ... etc ... */
}

.lipi-table--comfortable { /* defaults; explicit for switch */ }
.lipi-table--spacious    { /* ... */ }

/* §4 — Dark mode adjustments */
.theme-dark .lipi-table {
    --lipi-table-pin-shadow-left: inset 8px 0 8px -8px rgba(0,0,0,0.40);
    /* ... dark-mode-specific overrides ... */
}

/* §5 — High contrast */
@media (prefers-contrast: more) { /* ... */ }

/* §6 — Reduced motion */
@media (prefers-reduced-motion: reduce) { /* ... */ }

/* §7 — Print */
@media print { /* ... */ }

/* §8 — Layout rules (uses tokens) */
.lipi-table { /* container */ }
.lipi-table-header-band { /* ... */ }
/* ... all the actual layout CSS ... */
```

Tokens are defined first; actual rules consume them. Splitting tokens and rules keeps the file scannable — `Ctrl+F` for a token name finds its definition immediately.

The file is ≤ 1200 lines targeting compactness. If it grows beyond, sub-files are split:
- `lipi-table-tokens.css` — definitions only
- `lipi-table-layout.css` — rules
- `lipi-table-print.css` — print stylesheet (already separate per §17.7)

All loaded via `App.razor` per the existing pattern.

---

## §24.11 — Token compliance audit

A subset of these tokens must be **always present** for LipiTable to render correctly. Phase 2.10 audit will verify:

**Required (LipiTable will not render correctly without these):**
- `--color-surface-primary`, `--color-surface-secondary`
- `--color-text-primary`, `--color-text-secondary`
- `--color-border-tertiary`
- `--color-primary-500`, `--color-primary-600` and their alpha variants (`-alpha-08`, `-alpha-12`)
- `--color-success-600`, `--color-warning-500`, `--color-danger-500`, `--color-info-500` and alpha variants
- `--font-sans`, `--font-mono`
- `--r-md`, `--r-sm`, `--r-pill`
- `--sp-4`, `--sp-8`, `--sp-12`, `--sp-16` (spacing scale)
- `--tr-base`, `--tr-easing`

These must be defined in `00-baseline.css` and overridden in `mode-light.css` / `mode-dark.css`. The Phase 2.10 audit checks that the baseline provides all of them and that LipiTable's fallback values match the baseline defaults.

**Recommended (LipiTable falls back gracefully if missing, but visual quality degrades):**
- `--color-surface-hover`, `--color-surface-focus`, `--color-surface-elevated`, `--color-surface-disabled`
- `--color-border-secondary`, `--color-border-primary`, `--color-border-emphasis`
- `--color-text-tertiary`, `--color-text-success`, `--color-text-warning`, `--color-text-danger`
- `--color-on-primary`, `--color-link`, `--color-link-hover`
- `--color-shadow-strong`

If any of these are missing, LipiTable uses its inline fallback (the `, #XXXXXX` in the `var()` call) — visually similar but not theme-tuned.

**Optional (LipiTable doesn't reference these, but consuming pages might via custom `RowStyle`):**
- Any other LiPi token from the baseline

---

## §24.12 — Token count summary

| Category | Count |
|---|---|
| Structural (sizing, spacing, density) | ~50 |
| Color (mode-aware) | ~45 |
| Typography | ~7 |
| Z-index | 8 |
| Mode-specific overrides | ~5 |
| Total LipiTable tokens | **~115** |

Plus 17 shared status-strip tokens from `lipi-status-tokens.css`.

For comparison: AG-Grid uses ~280 CSS variables across its themes; Telerik Blazor Grid uses ~150 in its tokens; MudBlazor DataGrid is closer to ~60. LipiTable at 115 sits in the middle — comprehensive enough for theming flexibility, focused enough to avoid token sprawl.

---

*End of §24. Proceed to §6 — Sorting.*


# LipiTable Spec — §25 Component isolation contract

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** all prior sections (this defines invariants that must hold across all of them)
**Cross-references:** `02-STANDING-RULES.md` (component library isolation rule, permanent)

---

## §25.1 — Purpose

LipiTable must be **redistributable** — usable in any Blazor application, not just LiPi HIS. This means no hard-coded references to HIS-specific concepts, no implicit dependencies on HIS services, no clinical assumptions baked into the API.

The isolation contract specifies what LipiTable CAN and CANNOT do. It's the rule book the Phase 2.10 audit checks against before LipiTable ships as a standalone package.

This section formalizes the invariants. Some have been mentioned in passing throughout the spec; this is the authoritative statement.

---

## §25.2 — Naming invariants

### §25.2.1 — No HIS-specific names

LipiTable's public API must not contain HIS-specific terminology:

| Banned | Reason |
|---|---|
| `Patient`, `Doctor`, `Clinic`, `Appointment`, etc. | Domain entities; LipiTable is generic |
| `Phi`, `Hipaa`, `Dpdp`, `Clinical` in member names | Compliance context; not the component's concern |
| `Uhid`, `Aadhaar`, `Mrn` | Identifiers; specific to healthcare |
| `Encounter`, `Admission`, `Discharge` | Clinical workflows |
| Any LiPi-app feature names (`LiPi.PatientRegistration.*`) | Coupling to specific consumer modules |

The API uses generic terms: `TItem`, `Row`, `Column`, `Cell`, `Data`, `Filter`, `Sort`, `Group`, `Detail`, `Selection`, etc.

### §25.2.2 — Approved namespace

Components live under:

```
LiPi.Components.DataDisplay
LiPi.Components.DataDisplay.Filters
LiPi.Components.DataDisplay.Inputs
LiPi.Components.Shared
LiPi.Components.Shared.Status
LiPi.Components.Shared.Tokens
LiPi.Components.Shared.Export
LiPi.Components.Shared.Skills
```

These are the redistributable namespaces. NOT inside `LiPi.Web.*` (which is HIS application code).

Phase 2.10 audit will verify that no file under `LiPi.Components.*` references `LiPi.Web.*` types or `LiPi.HIS.*` types.

### §25.2.3 — Approved CSS prefix

All CSS classes that LipiTable emits begin with `lipi-`. Specifically:

| Pattern | Use |
|---|---|
| `lipi-table-*` | LipiTable's own classes |
| `lipi-table-row-*`, `lipi-table-cell-*`, etc. | Sub-element classes |
| `lipi-status-*` | Shared status taxonomy (from `lipi-status-tokens.css`) |
| `lipi-skeleton-*`, `lipi-toast-*`, etc. | Other LiPi component classes |

**Banned prefixes** (used by other LiPi or HIS code, would create conflicts):
- `reg-*` (registration module)
- `ue-*` (user-edit screens)
- `cn-*` (clinic-network admin)
- Any module-specific prefix

This was locked as a permanent standing rule in `02-STANDING-RULES.md` (per Phase 2.8 §3.1).

### §25.2.4 — `data-*` attribute prefix

LipiTable emits a small set of `data-*` attributes for CSS targeting and screen reader hooks:

- `data-status="active|pending|..."` (status taxonomy per §22.2)
- `data-column="ColumnKey"` (cell's column identifier)
- `data-row-key="..."` (row's KeySelector value)
- `data-depth="0|1|2|..."` (tree depth)

These are namespaced semantically (`data-status`, `data-column`, etc.) rather than prefixed (`data-lipi-status`). The pattern is conventional — `data-*` is already a namespaced attribute family.

If a consuming app has conflicting `data-*` usage, the app prefixes theirs (e.g., `data-app-status`).

---

## §25.3 — Dependency invariants

### §25.3.1 — Allowed dependencies

LipiTable's source code may depend on:

| Package | Use |
|---|---|
| `Microsoft.AspNetCore.Components.*` | Blazor framework |
| `Microsoft.AspNetCore.Components.Web.Virtualization` | Virtualization |
| `Microsoft.Extensions.Logging.Abstractions` | Logging interface (consuming app provides implementation) |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI service registration |
| `Microsoft.Extensions.Localization.Abstractions` | i18n |
| `System.*` | .NET BCL (text encoding, IO, generic collections, etc.) |
| Other `LiPi.Components.*` | LiPi component library (peer components) |

### §25.3.2 — Banned dependencies

LipiTable's source code MUST NOT depend on:

| Pattern | Why banned |
|---|---|
| `LiPi.Web.*` | HIS application code; would couple component to consumer |
| `LiPi.HIS.*` | Domain-specific |
| `LiPi.Clinic.*` | Clinical data layer |
| `LiPi.Identity.*` | Identity services (consuming app provides via interfaces) |
| Any package named `*Patient*`, `*Doctor*`, etc. | Domain entities |
| External UI libraries (MudBlazor, Telerik, Syncfusion, AntDesign Blazor, etc.) | Per Phase 2.8 zero-external-library posture |
| External data libraries (System.Linq.Dynamic.Core, etc.) | Per Phase 2.8 zero-external-library posture |
| External CSS frameworks (Bootstrap, Tailwind, etc.) | Per Phase 2.8 zero-external-library posture |
| External icon libraries beyond what's already locked (Lucide for v1.0; Lipicons later) | Per established roadmap |

The Phase 2.10 audit will scan `.csproj` and `using` statements to verify no banned dependencies exist.

### §25.3.3 — Service injection invariants

LipiTable injects services via constructor injection (or `[Inject]` attribute in Razor components). The injected types must be:

- **Microsoft.* generic interfaces** — `ILogger<T>`, `IStringLocalizer<T>`, `IJSRuntime`, `NavigationManager`, etc.
- **LiPi.Components.* interfaces** — `ITablePreferenceService`, `IDateFormatService` (when used), etc.
- **System.* types** — `HttpClient` (if needed; LipiTable doesn't directly), `TimeProvider`, etc.

Banned injection patterns:

- Direct DbContext (e.g., `IdentityDbContext`, `ClinicDbContext`) — DB access happens through abstractions
- Concrete service types from consuming apps — only interfaces in `LiPi.Components.*`
- HTTP clients with hardcoded base URLs — consuming app configures `HttpClient`

### §25.3.4 — Caller-supplied services

When LipiTable needs a service that's consumer-specific (e.g., the persistence backend per §21.4), it defines an interface in `LiPi.Components.*` and the consuming app provides the implementation via DI.

Examples:
- `ITablePreferenceService` — LipiTable defines interface; default implementation in `LiPi.Components.DataDisplay.Persistence`; consuming app can swap
- `IDateFormatService` — for clinic-locale date formatting (when used)
- Any future hook for caller-injected formatters / validators / etc.

This is the dependency-inversion pattern. LipiTable doesn't know about the consumer's DB / API / preferences; the consumer provides what LipiTable needs.

---

## §25.4 — CSS token contract

### §25.4.1 — Token namespace

LipiTable's CSS depends on a documented token contract. All tokens have the namespace prefix `--color-*`, `--sp-*`, `--r-*`, `--lipi-table-*`, etc.

Categories:

| Prefix | Use |
|---|---|
| `--color-*` | Color tokens (foundation; consuming app provides) |
| `--sp-*` | Spacing tokens (4px base unit grid) |
| `--r-*` | Border-radius tokens |
| `--font-*` | Font tokens |
| `--shadow-*` | Shadow tokens |
| `--lipi-table-*` | Table-specific tokens (computed from foundation tokens) |
| `--lipi-status-*` | Status tokens (shared with other LiPi components) |

### §25.4.2 — Foundation tokens (consuming app provides)

LipiTable expects these to exist in the consuming app's CSS:

```css
:root {
    /* Color foundation */
    --color-surface-primary: ...;
    --color-surface-secondary: ...;
    --color-text-primary: ...;
    --color-text-secondary: ...;
    --color-text-faint: ...;
    --color-primary-50 through --color-primary-900: ...;
    --color-success, --color-warning, --color-danger, --color-info: ...;
    
    /* Spacing (4px base) */
    --sp-0: 0;
    --sp-1: 4px;
    --sp-2: 8px;
    --sp-3: 12px;
    --sp-4: 16px;
    --sp-5: 20px;
    --sp-6: 24px;
    /* ... */
    
    /* Border-radius */
    --r-0: 0;
    --r-1: 2px;
    --r-2: 4px;
    --r-3: 6px;
    --r-4: 8px;
    
    /* Font */
    --font-mono: 'JetBrains Mono', monospace;
    --font-sans: system-ui, sans-serif;
}
```

The complete foundation token set is documented in the consuming app's CSS (typically `00-baseline.css`).

### §25.4.3 — Table-specific tokens

`lipi-table-tokens.css` defines table-specific tokens computed from the foundation:

```css
:root {
    /* Row dimensions */
    --lipi-table-row-h-compact: 32px;
    --lipi-table-row-h-comfortable: 40px;
    --lipi-table-row-h-spacious: 52px;
    --lipi-table-row-h: var(--lipi-table-row-h-comfortable);   /* active default */
    
    /* Header */
    --lipi-table-header-h-compact: 36px;
    --lipi-table-header-h-comfortable: 44px;
    --lipi-table-header-h-spacious: 52px;
    --lipi-table-header-h: var(--lipi-table-header-h-comfortable);
    
    /* Cell padding (per density) */
    --lipi-table-cell-pad-y-compact: 4px;
    /* ... */
    
    /* Colors */
    --lipi-table-bg-row: var(--color-surface-primary);
    --lipi-table-bg-row-alt: var(--color-surface-secondary);
    --lipi-table-bg-row-hover: var(--color-primary-50);
    --lipi-table-bg-row-selected: var(--color-primary-100);
    --lipi-table-text: var(--color-text-primary);
    --lipi-table-border: var(--color-border-subtle);
    /* ... */
    
    /* Z-index layering */
    --lipi-table-z-pinned: 1;
    --lipi-table-z-header: 2;
    --lipi-table-z-bulk-bar: 3;
}
```

The full set is in §24 (tokens). Phase 2.10 audit verifies the token contract is complete and used consistently.

### §25.4.4 — Hardcoded values banned

LipiTable's component CSS files (`Lipi*.razor.css`) and shared CSS files (`lipi-table-tokens.css`) must NOT contain hardcoded color values, hardcoded spacing values, or hardcoded fonts. Every visual property uses tokens.

Examples of banned patterns:

```css
/* BANNED */
.lipi-table-row {
    background-color: #FFFFFF;
    padding: 8px 12px;
    font-family: 'Arial', sans-serif;
}

/* CORRECT */
.lipi-table-row {
    background-color: var(--lipi-table-bg-row);
    padding: var(--lipi-table-cell-pad-y) var(--lipi-table-cell-pad-x);
    font-family: var(--font-sans);
}
```

The audit will grep for hex-color regex, px-value regex (other than 0 and 1 for borders), and font-family literals.

### §25.4.5 — Mode swap mechanism

Light mode, dark mode, and high-contrast mode are activated by toggling foundation token values via CSS selectors:

```css
:root { /* light mode defaults */ }
:root[data-theme="dark"] { /* dark mode token overrides */ }
:root[data-theme="high-contrast"] { /* AAA token overrides */ }
@media (prefers-contrast: more) { /* auto high-contrast */ }
```

LipiTable doesn't need mode-aware code — it just references tokens. Mode swaps happen at the foundation layer.

This means LipiTable's source code has zero theming logic. Adding a new mode (sepia, holiday theme, etc.) is purely a consuming-app-CSS concern.

---

## §25.5 — JS interop invariants

### §25.5.1 — Allowed JS

LipiTable ships a small JS module for browser APIs not exposed via Blazor:

```
wwwroot/lipi-table-interop.js
```

Contains:
- `triggerPrint(tableId)` — calls `window.print()`
- `downloadBlob(filename, mimeType, base64Data)` — triggers a file download (used by CSV / PDF export)
- `measureCellWidth(cellElement)` — auto-fit measurements (per §13.2.6)
- `scrollIntoView(rowElement)` — virtualization-aware scroll-to-row

The module is ES6 modules (modern Blazor pattern). Loaded lazily on first use.

### §25.5.2 — Banned JS

LipiTable's JS module must NOT:

- Reach into the consuming app's DOM outside the table (no `document.querySelector` for arbitrary selectors)
- Modify global state (`window.*`, `localStorage`, `sessionStorage` directly — those go through the abstraction layer)
- Depend on external JS libraries (jQuery, Lodash, etc.)
- Make HTTP calls (those go through Blazor HttpClient / fetch via callers)

Phase 2.10 audit scans `lipi-table-interop.js` for compliance.

### §25.5.3 — JS feature detection

For browser features that may not be supported (rarely needed for LipiTable's surface), the JS module feature-detects and falls back gracefully:

```javascript
export function triggerPrint(tableId) {
    if (typeof window.print !== 'function') {
        console.warn('Print not supported in this environment');
        return;
    }
    // ... rest
}
```

---

## §25.6 — State invariants

### §25.6.1 — No global state

LipiTable does not maintain global / static state. Every instance is independent:

- No `static` fields holding per-table data
- No singleton services that store table state
- No event subscriptions to global notifications outside the component's lifetime

Multi-instance use case: a page with multiple LipiTables simultaneously (one for active patients, one for archived). Each must work independently — sort in one doesn't affect the other.

### §25.6.2 — Lifecycle correctness

LipiTable disposes correctly:
- Timers (debounce timers for persistence, etc.) are disposed
- JS interop module references cleared
- Event subscriptions to NavigationManager / other Blazor services unsubscribed
- Any background tasks cancelled via the component's disposal token

Phase 2.10 audit verifies `DisposeAsync` implementations.

### §25.6.3 — Re-mount safety

When LipiTable un-mounts and re-mounts (e.g., user navigates away and back), the new instance:
- Starts fresh (no carry-over state from the previous instance)
- Reads persisted state from the abstraction (per §21.6.1)
- Doesn't reference the disposed instance's resources

---

## §25.7 — API surface invariants

### §25.7.1 — Public surface defined by `<LipiTable>` parameters and slots

The public API of LipiTable is exactly:
- Parameters on `LipiTable<TItem>` (annotated `[Parameter]`)
- Parameters on `LipiColumn<TItem, TValue>` (annotated `[Parameter]`)
- Render fragments / templates (e.g., `<DetailTemplate>`, `<EditTemplate>`, `<HeaderTemplate>`)
- Programmatic methods on the table reference (via `@ref="_table"` and `ValueTask` / `Task` return types)
- Events (per §23)
- Public types in `LiPi.Components.DataDisplay.*` (the request/response/context records)

Anything else is internal and may change between minor versions. Consuming apps must not reflect into internals.

### §25.7.2 — Stable API for v1.0 lifecycle

Within the v1.0 lifecycle (v1.0, v1.1, v1.2, etc.):
- No public parameter is removed
- No public parameter's type changes incompatibly
- New parameters can be added (with sensible defaults)
- New events / methods can be added
- Existing event signatures don't change

Breaking changes are reserved for v2.0+ (not anticipated in the LipiTable lifecycle).

### §25.7.3 — Internal types

Types marked `internal` or in `LiPi.Components.DataDisplay.Internal.*` namespaces are NOT part of the public surface. Consuming apps that reference them via reflection or by adding `InternalsVisibleTo` should expect breakage.

---

## §25.8 — Audit checklist (Phase 2.10)

Phase 2.10 audit verifies these invariants. Findings block deployment:

### §25.8.1 — Source code scan

```
✗ Any file under LiPi.Components.* references LiPi.Web.* or LiPi.HIS.*
✗ Any reference to a banned external library
✗ Any HIS-specific name in public API
✗ Any direct DbContext injection
✗ Any concrete type from consuming apps in service injection
```

### §25.8.2 — CSS scan

```
✗ Any class name not starting with `lipi-` (or `data-*` for attributes)
✗ Any hex color literal in component CSS
✗ Any px value other than 0, 1, 2 (small known constants) without a token
✗ Any font-family literal
✗ Any module-prefixed class name (`reg-`, `ue-`, `cn-`)
✗ Banned external CSS imports (`@import` from CDN, etc.)
```

### §25.8.3 — JS scan

```
✗ Any external JS library import
✗ Any global state mutation outside the table
✗ Any HTTP call from JS
✗ Any direct DOM access outside the table's subtree
```

### §25.8.4 — DI scan

```
✗ LipiTable's source references services not in approved list
✗ Hard-coded base URLs / configurations
✗ Cascading parameters that reach into consumer-specific contexts
```

### §25.8.5 — Manual review

```
✓ Public API uses only generic terms (Row, Column, Cell, etc.)
✓ Documentation doesn't assume HIS-specific use
✓ Examples cover non-HIS scenarios (e.g., e-commerce orders, project tasks)
✓ Standalone redistribution as NuGet package is feasible
```

---

## §25.9 — What ISN'T part of the contract

The contract specifies what LipiTable does. It does NOT specify:

- What consuming pages do with LipiTable — they're free to use HIS-specific names in their column declarations and templates
- How the consuming app sets up DI — LipiTable just expects the interfaces to be registered
- What the consuming app's CSS overrides — overrides are allowed and expected; LipiTable just provides the surface

The contract is **about what LipiTable emits and depends on**, not about how it's used.

Examples:
- A consuming page can have `<LipiColumn Field="@(p => p.Uhid)">` — HIS-specific in the lambda is fine
- A consuming page can have a CSS override `.my-app-table { --lipi-table-row-h: 36px; }` — overrides are fine
- A consuming page can wire `OnRowSave` to a HIS-specific service — that's consumer-side coupling, not LipiTable's concern

---

## §25.10 — Worked examples

### §25.10.1 — Compliant column declaration

```razor
<LipiTable TItem="Patient"
           Items="@_patients"
           KeySelector="@(p => p.Id)">
    <LipiColumn Field="@(p => p.Uhid)" Header="UHID" />
    <LipiColumn Field="@(p => p.Name)" Header="Patient name" />
</LipiTable>
```

Compliant: `Patient` is the consumer's type (TItem). `Uhid` is in the consumer's lambda. LipiTable sees only generic types.

### §25.10.2 — Compliant CSS override

In the consuming app's CSS:

```css
/* Override LipiTable's default row height for compact admin pages */
.patient-admin-page .lipi-table {
    --lipi-table-row-h-comfortable: 36px;
}
```

Compliant: scoped override via parent class. LipiTable just uses the token; doesn't care what value it has.

### §25.10.3 — Non-compliant (would fail audit)

```csharp
// In LipiTable's source code — BANNED
public class LipiTable<TItem>
{
    [Inject] private PatientService PatientService { get; set; } = null!;   // ❌ HIS service
    
    private string LookupClinicName(Guid clinicId) =>
        PatientService.GetClinicName(clinicId);   // ❌ HIS-specific logic
}
```

Would fail isolation audit:
- Injects HIS-specific service
- Has HIS-specific logic
- Couples LipiTable to consumer

Correct pattern:

```csharp
// In LipiTable's source — COMPLIANT
public class LipiTable<TItem>
{
    [Parameter] public Func<TItem, string>? RowDisplayNameSelector { get; set; }   // ✓ generic
}

// In consumer's code
<LipiTable TItem="Patient"
           RowDisplayNameSelector="@(p => $"{p.Name} — {p.ClinicName}")">
```

Consumer provides the HIS-specific lookup via a generic hook.

---

## §25.11 — Standing rule cross-reference

This section codifies invariants from the permanent standing rule (per `02-STANDING-RULES.md` — added in Phase 2.8 per the `05-Standing-Rule-Addition-Library-Dependency-Posture.md` deliverable):

> The LiPi component library must be designed as a standalone redistributable package for any Blazor app. Strict isolation requirements: no HIS-specific names, routes, or services; CSS uses `lipi-*` prefix only (never `reg-*`/`ue-*`/`cn-*`); CSS depends only on the documented token contract (`--color-*`, `--sp-*`, `--r-*`); components inject only `Microsoft.*`/`LiPi.*` generics — never HIS-specific services. Phase 2.10 audits compliance before redesign begins.

Phase 2.10 audit references this section for the detailed checklist. Standing rule is the permanent commitment; this section is the operational specification.

---

*End of §25. Proceed to §26 — StyleGuide additions.*


# LipiTable Spec — §26 StyleGuide additions

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** all prior sections (StyleGuide demos illustrate concepts from each)
**Cross-references:** existing StyleGuide structure under `LiPi.Web/Components/Pages/StyleGuide/`

---

## §26.1 — Purpose

The LiPi StyleGuide is the live demo page where developers see every component in action. Phase 2.8 adds a `Data Display` section to the StyleGuide containing 12 demo pages covering LipiTable + sibling components (LipiList, LipiPagination, LipiEmptyState).

Each demo:
- Shows the rendered component with realistic sample data
- Has tabs for **Demo**, **Code**, **Props**, **A11y notes**
- Uses domain-agnostic sample data (orders, articles, projects — NOT patients) per the component isolation contract (§25.2.1)
- Documents the props in a table with name + type + default + description
- Lists relevant keyboard shortcuts and screen-reader behavior

The StyleGuide is the second-largest deliverable of Phase 2.8 (after the component source itself). It's the canonical reference build chat builds against.

---

## §26.2 — StyleGuide site structure

### §26.2.1 — Existing structure

The StyleGuide currently has sections:

```
StyleGuide/
├── Foundations/           (color, spacing, typography, tokens)
├── Inputs/                (LipiTextBox, LipiSelect, LipiDatePicker, etc.)
├── Layout/                (LipiCard, LipiAlert, LipiTabs)
├── Overlays/              (LipiModal, LipiDrawer)
├── Feedback/              (LipiSpinner, LipiBadge, LipiPill, LipiSkeleton, LipiToast, LipiValidationSummary)
```

Phase 2.8 adds:

```
StyleGuide/
├── DataDisplay/           ← NEW (Phase 2.8)
│   ├── LipiTable-Basics
│   ├── LipiTable-Selection
│   ├── LipiTable-Sort
│   ├── LipiTable-Filter
│   ├── LipiTable-Pagination
│   ├── LipiTable-Edit
│   ├── LipiTable-Tree
│   ├── LipiTable-MasterDetail
│   ├── LipiTable-Grouping
│   ├── LipiList
│   ├── LipiPagination-Standalone
│   └── LipiEmptyState-Standalone
```

12 demo pages. Each routed at `/styleguide/data-display/{component}-{topic}`.

### §26.2.2 — Side navigation

A new sidebar group "Data Display" appears in the StyleGuide left navigation, expanded by default when the user is on any data-display page. The order matches the file structure above.

---

## §26.3 — Demo page template

Each demo page follows a consistent layout:

```razor
@page "/styleguide/data-display/lipi-table-basics"
@layout StyleGuideLayout

<StyleGuidePageHeader
    Title="LipiTable — Basics"
    Description="Generic data grid with sort, filter, selection, pagination, edit, group, tree, master-detail, and zero external libraries.">
    
    <LipiBadge Variant="success">Phase 2.8</LipiBadge>
</StyleGuidePageHeader>

<LipiTabs>
    <LipiTab Title="Demo" Icon="play">
        <DemoTabContent />
    </LipiTab>
    
    <LipiTab Title="Code" Icon="code-2">
        <CodeTabContent SourceFiles="@(new[] { "Demos/LipiTableBasicsDemo.razor" })" />
    </LipiTab>
    
    <LipiTab Title="Props" Icon="list-tree">
        <PropsTable Props="@_props" />
    </LipiTab>
    
    <LipiTab Title="A11y" Icon="accessibility">
        <A11yNotes>
            <p>Keyboard navigation: Arrow keys move between cells. Enter expands details.</p>
            <p>Screen reader: Announces "Data grid, N rows by M columns" on focus.</p>
        </A11yNotes>
    </LipiTab>
</LipiTabs>

@code {
    private List<PropMetadata> _props = new()
    {
        new("Items", "IEnumerable<TItem>", "null", "Client-side data source"),
        new("DataSource", "Func<TableQueryRequest, CancellationToken, Task<TableQueryResponse<TItem>>>", "null", "Server-side data source"),
        new("KeySelector", "Func<TItem, object>", "required", "Stable identifier per row"),
        // ... etc
    };
}
```

The `StyleGuidePageHeader`, `DemoTabContent`, `CodeTabContent`, `PropsTable`, and `A11yNotes` components already exist in the StyleGuide infrastructure from prior phases.

---

## §26.4 — Demo page 1: LipiTable — Basics

**Route:** `/styleguide/data-display/lipi-table-basics`

**Purpose:** First-contact demo showing the simplest possible LipiTable usage. Establishes the visual baseline and the basic API.

**Sample data:** Orders (order number, customer, date, amount, status). Hardcoded list of 8 sample orders. No domain coupling.

**Demonstrated:**
- Items mode (client-side)
- 5 columns of different types (Text, Currency, Date, Status, Number)
- KeySelector
- Status column with badges
- Status strip on row left edge via `RowStatus`
- Default density Comfortable
- Default chrome (header, footer, no toolbar features enabled)

**Demo code:**

```razor
<LipiTable TItem="DemoOrder"
           Items="@_orders"
           KeySelector="@(o => o.Id)"
           RowStatus="@(o => o.Status.ToLowerInvariant())">
    <LipiColumn Field="@(o => o.Number)" Header="Order #" />
    <LipiColumn Field="@(o => o.Customer)" Header="Customer" />
    <LipiColumn Field="@(o => o.Date)" Type="ColumnType.Date" Header="Date" />
    <LipiColumn Field="@(o => o.Amount)" Type="ColumnType.Currency" Header="Amount" />
    <LipiColumn Field="@(o => o.Status)" Type="ColumnType.Status" Header="Status" />
</LipiTable>

@code {
    private List<DemoOrder> _orders = SampleData.Orders;
    
    private class DemoOrder
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = "";
        public string Customer { get; set; } = "";
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
    }
}
```

**A11y notes:** Standard `role="grid"` semantics, ARIA labels, keyboard navigation.

---

## §26.5 — Demo page 2: LipiTable — Selection

**Route:** `/styleguide/data-display/lipi-table-selection`

**Purpose:** Single-select, multi-select, and select-all-across-pages.

**Sample data:** Projects (name, owner, status, hours logged, deadline). 47 projects (enough for multi-page selection).

**Demonstrated:**
- `SelectionMode="None"` (default), `SelectionMode="Single"`, `SelectionMode="Multi"`
- `@bind-SelectedItems` two-way binding
- Two-step select-all (page → banner → across-pages per §5.3.1)
- Selection count display
- Bulk action bar (region ③)
- Keyboard selection (Space, Shift+Click extension, Ctrl+A)
- Programmatic selection via `tableRef.SelectAllOnPageAsync()`

**Demo includes a `LipiSegmented` to switch between None / Single / Multi modes.**

**Props highlighted:** SelectionMode, SelectedItems, BulkActionTemplate, OnSelectionChanged.

**A11y notes:** Selection checkbox aria-label includes row context. Space toggles, Shift+Space extends. Bulk bar announced via aria-live.

---

## §26.6 — Demo page 3: LipiTable — Sort

**Route:** `/styleguide/data-display/lipi-table-sort`

**Purpose:** Sort behavior — three-state, two-state, multi-column, custom comparators.

**Sample data:** Articles (title, author, published date, view count, tags). 25 articles.

**Demonstrated:**
- Default three-state sort (asc → desc → none)
- Two-state opt-out on a column (`SortCycle="TwoState"`)
- Multi-column sort via Shift+Click
- `Sortable="false"` per column (lock a column from sort)
- Custom `SortComparer` (e.g., sort tags by length, not alphabetical)
- Programmatic `tableRef.SortByAsync("Author", SortDirection.Ascending)`

**Demo includes a "Show current sort state" panel** rendering `_table.CurrentSort` so users see the effect of their clicks.

**Props highlighted:** Sortable, SortCycle, SortComparer, DefaultSort, OnSortChanged.

---

## §26.7 — Demo page 4: LipiTable — Filter

**Route:** `/styleguide/data-display/lipi-table-filter`

**Purpose:** Filter UX — header icon, drawer, quick search, chip strip.

**Sample data:** Tasks (title, assignee, priority, status, created date, tags). 50 tasks.

**Demonstrated:**
- `ShowQuickFilter="true"` quick search
- HeaderIcon filter mode (default per §7.1)
- Drawer filter mode (`FilterMode="FilterMode.Drawer"`)
- Filter chips strip (region ②)
- Operators per column type (Text: contains/equals/startsWith; Number: equals/greater/less/between; Date: equals/before/after/relative; Status: in/not-in)
- Apply / Clear / Cancel in popover
- "Clear all filters" action
- `OnFilterChanged` event

**Demo includes a `LipiSegmented` to switch between HeaderIcon and Drawer modes.**

**Props highlighted:** Filterable, FilterMode, FilterOperators, ShowQuickFilter, FilterDescriptors, OnFilterChanged.

---

## §26.8 — Demo page 5: LipiTable — Pagination

**Route:** `/styleguide/data-display/lipi-table-pagination`

**Purpose:** Pagination modes — Standard, InfiniteScroll, LoadMore.

**Sample data:** Events log (timestamp, actor, action, target). 500+ events (enough to demonstrate pagination).

**Demonstrated:**
- Standard pagination (default): Page X of Y, jump to page, page-size selector
- InfiniteScroll: auto-load next page as user scrolls near bottom
- LoadMore: explicit "Load more" button at bottom of current data
- Page size options [10, 25, 50, 100, All]
- "All" with server-side cap warning
- "Showing X-Y of Z" display
- Programmatic `tableRef.GoToPageAsync(5)`

**Demo includes a tab strip switching between the three modes.**

**Props highlighted:** PaginationMode, DefaultPageSize, PageSizeOptions, ServerSideAllCap, OnPageChanged.

---

## §26.9 — Demo page 6: LipiTable — Edit

**Route:** `/styleguide/data-display/lipi-table-edit`

**Purpose:** Inline editing — Row mode and Cell mode, with all the surfaces.

**Sample data:** Inventory items (SKU, name, price, stock, status). 30 items with realistic data.

**Demonstrated:**
- `EditMode="TableEditMode.Row"` with pencil icon
- `EditMode="TableEditMode.Cell"` with double-click / F2 / direct typing
- Save flows (pessimistic default with spinner; optimistic opt-in)
- Concurrency conflict simulation (a "Simulate conflict" button triggers a mock conflict response)
- Conflict UX in Banner mode (default)
- Critical-field confirm via `RequireConfirmEdit="true"` on the price column
- Per-row validation via `EditValidator`
- Force-save retry via `IsForceSave` flag
- Add new row via toolbar + `<AddRowTemplate>`
- Cancel with discard confirm
- Dirty state indicator (modified-dot)

**Demo includes a `LipiSegmented` to switch between Row and Cell modes, plus action buttons to simulate concurrency conflict and validation error.**

**Props highlighted:** EditMode, EditButtonPlacement, RowVersionSelector, OptimisticUpdate, ConflictResolutionMode, EditValidator, OnRowSave, OnCellSave, OnBeforeRowSave.

**A11y notes:** Edit keyboard model (Enter saves stay, Ctrl+Enter saves down, Tab next cell, Escape cancel). Edit state announced via aria-live. Concurrency conflict via aria-live assertive.

---

## §26.10 — Demo page 7: LipiTable — Tree

**Route:** `/styleguide/data-display/lipi-table-tree`

**Purpose:** Tree data — both ChildrenSelector and ParentSelector shapes.

**Sample data:** Org chart (manager → reports → individual contributors). 15 nodes across 3 levels.

**Demonstrated:**
- `ChildrenSelector` for nested data shape
- `ParentSelector` for flat data shape (toggle between the two for comparison)
- Expand chevron in first content column
- Tree indent per density
- `ShowTreeIndentGuides="true"` opt-in visual guide lines
- `DefaultTreeState.FirstLevelExpanded` middle-ground
- Keyboard expand/collapse (ArrowRight/Left, Enter)
- Tree filter behavior (PreserveAncestors default vs MatchOnly)
- `TreeSelectionCascade="Descendants"` opt-in cascade
- Lazy-load children via `OnTreeNodeExpand` (simulated 500ms delay)

**Demo includes toggles for: data shape, filter mode, cascade mode, indent guides.**

**Props highlighted:** ChildrenSelector, ParentSelector, HasChildrenSelector, DefaultTreeState, TreeFilterMode, TreeSelectionCascade, OnTreeNodeExpand.

---

## §26.11 — Demo page 8: LipiTable — Master-detail

**Route:** `/styleguide/data-display/lipi-table-masterdetail`

**Purpose:** Master-detail with arbitrary detail content.

**Sample data:** Software releases (version, date, release manager). 12 releases.

**Demonstrated:**
- `<DetailTemplate>` rendering rich content (multi-column grid with release notes, contributors, deployment status)
- Expand chevron in dedicated leftmost column
- `MultiExpand="true"` (default) vs `MultiExpand="false"` accordion
- `HasDetailSelector="@(r => r.HasReleaseNotes)"` hiding chevron on details-less rows
- Detail content with nested table (LipiTable inside LipiTable for the contributors list)
- `OnRowExpand` for lazy-load detail data pattern
- `OnBeforeRowCollapse` for dirty-detail-form discard

**Demo includes a toggle for MultiExpand mode.**

**Props highlighted:** DetailTemplate, MultiExpand, HasDetailSelector, DetailRowEstimatedHeight, OnRowExpand, OnBeforeRowCollapse.

---

## §26.12 — Demo page 9: LipiTable — Grouping

**Route:** `/styleguide/data-display/lipi-table-grouping`

**Purpose:** Grouping by column value — single and multi-level.

**Sample data:** Bug reports (title, severity, assignee, status, age in days). 40 bugs.

**Demonstrated:**
- Declarative `GroupBy` parameter
- `ShowGroupBar="true"` drag-to-group bar
- Single-level grouping (by Status)
- Multi-level grouping (by Status → Severity)
- Group header rendering with item count
- Custom `<GroupHeaderTemplate>` with avg-age aggregate
- Group expand/collapse
- `DefaultGroupState.Collapsed` for large datasets
- `HideGroupedColumns="true"` Excel-style
- Group aggregation showing count and avg in header

**Demo includes:**
- Buttons to programmatically group by different columns
- A "Show all in one group" button (sets GroupBy=null)
- Toggle for `DefaultGroupState`

**Props highlighted:** GroupBy, ShowGroupBar, GroupHeaderTemplate, DefaultGroupState, HideGroupedColumns, OnGroupChanged.

---

## §26.13 — Demo page 10: LipiList

**Route:** `/styleguide/data-display/lipi-list`

**Purpose:** Sibling component — list-style data display for mobile/narrow surfaces or non-tabular data.

**Sample data:** Notification list (icon, title, body, timestamp, read state). 20 items.

**Demonstrated:**
- LipiList API (subset of LipiTable's; row-oriented not column-oriented)
- `<ItemTemplate>` for each list item
- Compact density variant
- Selection (single, multi)
- Read/unread state via row status strip
- Quick search
- Empty state with custom CTA
- LipiList as a fallback for narrow viewports (when LipiTable wouldn't fit per §2.6)

**Sibling component spec:** `02-LipiList-Spec-OUTLINE.md` (separate file, drafted earlier).

**A11y notes:** `role="list"` + `role="listitem"`. Keyboard navigation Up/Down arrows. Selection Space.

---

## §26.14 — Demo page 11: LipiPagination (standalone)

**Route:** `/styleguide/data-display/lipi-pagination-standalone`

**Purpose:** Pagination as a standalone component, usable outside LipiTable.

**Sample data:** Generic paged content — image gallery or search results.

**Demonstrated:**
- `LipiPagination` outside any table context
- Standard / Compact / Mini variants
- Page-size selector standalone
- "Showing X-Y of Z" display
- Two-way binding on Page and PageSize
- Programmatic API

**Use case:** A page wants pagination UI consistent with LipiTable's footer but for a custom data display (image gallery, KPI cards, etc.).

**Sibling component spec:** `03-LipiPagination-Spec-OUTLINE.md`.

---

## §26.15 — Demo page 12: LipiEmptyState (standalone)

**Route:** `/styleguide/data-display/lipi-empty-state-standalone`

**Purpose:** Empty/Error/FilteredEmpty state component, usable outside LipiTable.

**Sample data:** Various scenarios shown side-by-side.

**Demonstrated:**
- Empty variant with default icon
- Error variant with retry
- FilteredEmpty variant with clear-filters CTA
- Custom variant with caller-supplied icon and CTA
- Tiny variant (for cards / sidebar panels)
- Standard variant (for full-page empty states)
- Composition: inside a card, inside a section, full-page

**Sibling component spec:** `04-LipiEmptyState-Spec-OUTLINE.md`.

---

## §26.16 — Cross-cutting demo content

### §26.16.1 — Sample data generator

A single shared `SampleData.cs` in `Demos/Shared/` provides realistic-feeling demo data for all 12 demos:

```csharp
public static class SampleData
{
    public static List<DemoOrder> Orders => /* 8 hardcoded orders */;
    public static List<DemoProject> Projects(int count) => /* generated */;
    public static List<DemoArticle> Articles => /* 25 articles */;
    public static List<DemoTask> Tasks => /* 50 tasks */;
    public static List<DemoEvent> Events(int count) => /* generated */;
    public static List<DemoInventoryItem> InventoryItems => /* 30 items */;
    public static List<DemoOrgNode> OrgChart => /* nested tree */;
    public static List<DemoRelease> Releases => /* 12 releases */;
    public static List<DemoBug> Bugs => /* 40 bugs */;
    public static List<DemoNotification> Notifications => /* 20 items */;
}
```

All data is domain-agnostic (orders, projects, tasks, releases — NOT patients/clinical) per §25.2.1.

### §26.16.2 — Code snippet syntax highlighting

Code tabs use a syntax-highlighting library that the StyleGuide already uses (typically Prism.js or highlight.js). LipiTable demos don't introduce new highlighting requirements.

### §26.16.3 — "Try it" interactivity

Each demo page lets the user manipulate the component:
- Selection mode toggles
- Density toggle
- Filter examples
- Sort by clicking headers
- Add a row to see the table grow

The demos are functional — the user can interact with them as they would in a real consuming page.

### §26.16.4 — Performance notice on stress demos

The infinite-scroll pagination demo and the "1000-row stress" sub-demo display a banner:

```
ℹ This demo uses 1000+ rows to demonstrate virtualization. Initial load may take 200-400ms on slower devices.
```

Sets expectations honestly. Users understand the data is large.

---

## §26.17 — Light/dark mode in StyleGuide

Each demo respects the StyleGuide's existing light/dark mode toggle (top right of the StyleGuide layout). LipiTable's tokens swap automatically per §25.4.5.

The high-contrast mode is exposed via a third button (or `prefers-contrast: more` system pref). Demos render correctly in all three modes without any per-demo code.

---

## §26.18 — Mobile preview

The StyleGuide layout includes a "Mobile view" toggle that constrains the demo viewport to 360px (typical mobile width). When activated:
- LipiTable does NOT reflow (it requires 1024px minimum per §2.6)
- A banner appears: "LipiTable requires 1024px minimum width. Use LipiList for mobile layouts (see LipiList demo)."

The LipiList demo shows the mobile-friendly alternative.

---

## §26.19 — Code copy-to-clipboard

The Code tab includes a "Copy" button that copies the demo source to the clipboard. Standard StyleGuide infrastructure feature.

For long demos, the code is collapsible — only the most relevant snippet is shown by default; "Show full source" expands.

---

## §26.20 — Search

The StyleGuide has a global search (Ctrl+K) that includes new pages added in Phase 2.8. No new search infrastructure required — pages register themselves into the search index via metadata.

Search queries that match LipiTable demos:
- "table", "data table", "grid" → LipiTable-Basics
- "selection", "checkboxes" → LipiTable-Selection
- "sort", "ascending", "descending" → LipiTable-Sort
- "filter", "search" → LipiTable-Filter
- "pagination", "infinite scroll", "load more" → LipiTable-Pagination
- "edit", "inline edit", "cell edit" → LipiTable-Edit
- "tree", "hierarchical" → LipiTable-Tree
- "master detail", "expandable rows" → LipiTable-MasterDetail
- "group", "grouping", "categorize" → LipiTable-Grouping
- "list", "mobile" → LipiList
- "pagination" → LipiPagination-Standalone
- "empty", "no data", "error state" → LipiEmptyState-Standalone

---

## §26.21 — Documentation cross-references

Each demo page links to the relevant spec sections:

```razor
<StyleGuideDocsLink href="/spec/lipitable/12-inline-editing">
    Read the spec: Inline editing
</StyleGuideDocsLink>
```

The spec files are themselves hosted in the StyleGuide (or in a separate docs site). Build chat decides whether to host the markdown specs directly in the StyleGuide route table or in a dedicated docs section.

---

## §26.22 — Phase 2.10 audit verification

The StyleGuide demos are the primary verification surface for Phase 2.10 audit:

| Audit item | Verified by |
|---|---|
| Component isolation contract (§25) | Source code scan of `Demos/` directory; should reference only `LiPi.Components.*` |
| Accessibility (§19) | axe-core runs against every demo page |
| Performance (§20) | Benchmark workloads use the stress demos in `LipiTable-Pagination` |
| Token contract (§24, §25.4) | CSS scan of demos for hex literals or non-token values |
| Documentation completeness | Manual review — every parameter in spec sections appears in at least one demo |

---

*End of §26. Proceed to §27 — Files to create.*


# LipiTable Spec — §27 Files to create

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review
**Depends on:** all prior sections (this enumerates the deliverables)
**Cross-references:** `deploy-downloads.ps1` (per LiPi operational rules, must be updated with every new file)

---

## §27.1 — Purpose

This section is the build-chat handoff manifest. Every file that LipiTable + its sibling components produce, grouped by category, with paths and dependencies.

Build chat reads this section and creates files in exact paths specified. The deploy script (§28) is updated to copy each new file.

Total file count: **~95 files** spanning source code (Razor / C#), scoped CSS, services, JS interop, supporting types, sample data, StyleGuide demos, and spec docs. Most of the source files are sub-components within LipiTable.

---

## §27.2 — File location strategy

Per the component isolation contract (§25.2.2), all redistributable code lives under:

```
src/LiPi.Components/                     ← redistributable package
└── DataDisplay/
    ├── LipiTable/                       ← LipiTable + sub-components
    ├── LipiList/
    ├── LipiPagination/
    └── LipiEmptyState/

src/LiPi.Components.Shared/              ← cross-component infrastructure
└── (status tokens, skeleton, etc. shared with other phases)

src/LiPi.Web/                            ← HIS application code
└── Components/Pages/StyleGuide/DataDisplay/   ← demo pages only
```

Build chat creates these directories if they don't exist. `LiPi.Components` and `LiPi.Components.Shared` are separate csproj projects from `LiPi.Web`.

---

## §27.3 — Core LipiTable files

### §27.3.1 — Main component

| File | Purpose |
|---|---|
| `LipiTable.razor` | Main component markup + render logic |
| `LipiTable.razor.cs` | Code-behind with parameters and state |
| `LipiTable.razor.css` | Scoped CSS for the component root |
| `LipiTable.razor.js` | (Optional) collocated JS module; alternative is shared `lipi-table-interop.js` |

The code-behind handles:
- Parameter validation (Dev-mode throwing per Phase 2.5.5 OnEditContextReset pattern)
- Internal state (dirty rows, expand state, sort/filter/group dimensions, etc.)
- DataSource invocation
- Event dispatch
- Programmatic API surface

### §27.3.2 — Column component

| File | Purpose |
|---|---|
| `LipiColumn.razor` | The `<LipiColumn>` declaration component |
| `LipiColumn.razor.cs` | Code-behind capturing parameters |

LipiColumn doesn't render its own DOM — it registers itself with the parent LipiTable on init. Parameters captured here include Field, Header, Sortable, Filterable, etc.

### §27.3.3 — Type definitions

| File | Purpose |
|---|---|
| `LipiTableTypes.cs` | All enums (TableEditMode, SelectionMode, SortDirection, PaginationMode, etc.) |
| `LipiColumnTypes.cs` | Column-specific enums (ColumnType, ColumnPin, ColumnSortCycle, LipiAggregate, etc.) |
| `TableQueryRequest.cs` | Data source request record |
| `TableQueryResponse.cs` | Data source response record + GroupBucket + ConflictInfo |
| `SaveResult.cs` | SaveResult + SaveOutcome enum |
| `Contexts.cs` | All event context records (SelectionChangedContext, SortChangedContext, etc.) |
| `SortDescriptor.cs` | SortDescriptor + FilterDescriptor records |
| `LipiStatus.cs` | Reference status constants (Active, Pending, etc.) |

Per the convention from earlier phases (Phase 2.5.5+) — types in `.cs` files, not nested in `.razor` files. This keeps the type surface visible without parsing Razor.

---

## §27.4 — LipiTable sub-components

LipiTable composes multiple sub-components for clean separation. Each lives in `LiPi.Components/DataDisplay/LipiTable/Internal/`:

### §27.4.1 — Layout sub-components

| File | Purpose |
|---|---|
| `LipiTableHeader.razor` + `.razor.css` | Region ⓪ title/subtitle band |
| `LipiTableToolbar.razor` + `.razor.css` | Region ① toolbar with three zones |
| `LipiTableFilterChips.razor` + `.razor.css` | Region ② active-filter chips strip |
| `LipiTableBulkActionBar.razor` + `.razor.css` | Region ③ bulk action bar |
| `LipiTableGroupBar.razor` + `.razor.css` | Region ④ drag-to-group bar |
| `LipiTableCapBanner.razor` + `.razor.css` | Region ⑤ "showing first N of M" cap warning |
| `LipiTableColumnHeaders.razor` + `.razor.css` | Region ⑥ column header row |
| `LipiTableBody.razor` + `.razor.css` | Region ⑦ body with virtualization wrapper |
| `LipiTableFooter.razor` + `.razor.css` | Region ⑧/⑧ₐ footer with pagination + aggregate row |
| `LipiTableEditStickyBar.razor` + `.razor.css` | Region ⑨ sticky edit save bar (when StickyBottomBar mode) |

### §27.4.2 — Row sub-components

| File | Purpose |
|---|---|
| `LipiTableRow.razor` + `.razor.css` | Single data row (handles selection, expand, edit, status strip) |
| `LipiTableGroupHeaderRow.razor` + `.razor.css` | Group header row (when grouping active) |
| `LipiTableDetailRow.razor` + `.razor.css` | Master-detail expanded detail row |
| `LipiTableSkeletonRow.razor` + `.razor.css` | Loading state skeleton row |
| `LipiTableAggregateRow.razor` + `.razor.css` | Footer aggregate row |

### §27.4.3 — Cell sub-components

| File | Purpose |
|---|---|
| `LipiTableCell.razor` + `.razor.css` | Default cell rendering |
| `LipiTableHeaderCell.razor` + `.razor.css` | Header cell with sort + filter affordances |
| `LipiTableEditCell.razor` + `.razor.css` | Cell in edit mode (hosts the input) |
| `LipiTableSelectionCell.razor` + `.razor.css` | Selection checkbox cell |
| `LipiTableExpandCell.razor` + `.razor.css` | Expand chevron cell (tree + master-detail) |
| `LipiTableActionsCell.razor` + `.razor.css` | Actions column cell |

### §27.4.4 — Popover / drawer sub-components

| File | Purpose |
|---|---|
| `LipiTableColumnPicker.razor` + `.razor.css` | Column picker popover (per §16.4) |
| `LipiTableContextMenu.razor` + `.razor.css` | Right-click context menu (per §13.4.3) |
| `LipiTableFilterPopover.razor` + `.razor.css` | HeaderIcon filter popover (per §7) |
| `LipiTableFilterDrawer.razor` + `.razor.css` | Drawer filter mode (per §7.1) |
| `LipiTableExportModal.razor` + `.razor.css` | Export "More options..." modal (per §17.2.3) |
| `LipiTableConflictBanner.razor` + `.razor.css` | Concurrency conflict banner (per §12.11.2) |
| `LipiTableConflictDiffPopover.razor` + `.razor.css` | "View changes" diff popover (per §12.11.4) |
| `LipiTableConflictModal.razor` + `.razor.css` | Modal mode conflict resolution (per §12.11.3) |
| `LipiTableValidationStrip.razor` + `.razor.css` | Above-row validation error strip (per §12.6.3) |

### §27.4.5 — Pagination sub-components

These live in `LipiPagination/` (sibling component) but are used by LipiTable as well:

| File | Purpose |
|---|---|
| `LipiPagination.razor` + `.razor.css` | Standard pagination control |
| `LipiPaginationCompact.razor` + `.razor.css` | Compact variant |
| `LipiPaginationMini.razor` + `.razor.css` | Mini variant (just prev/next) |
| `LipiPaginationLoadMore.razor` + `.razor.css` | Load more button |
| `LipiPaginationPageSize.razor` + `.razor.css` | Page size selector |

---

## §27.5 — Filter operator components

Per §7, filter UIs vary by operator. Each operator has a corresponding input component:

| File | Purpose |
|---|---|
| `Filters/FilterTextInput.razor` | "contains", "startsWith", "endsWith", "equals" for text |
| `Filters/FilterNumberInput.razor` | "equals", "greater", "less" for number |
| `Filters/FilterNumberRangeInput.razor` | "between" for number |
| `Filters/FilterDateInput.razor` | "before", "after", "on" for date |
| `Filters/FilterDateRangeInput.razor` | "between" for date |
| `Filters/FilterRelativeDateInput.razor` | "today", "yesterday", "last 7 days", etc. |
| `Filters/FilterSetInput.razor` | "in" / "not in" (multi-select) for Status / enum columns |
| `Filters/FilterBooleanInput.razor` | "is true" / "is false" |
| `Filters/FilterBuilder.razor` | Composite filter builder (per §7.7) |

All filter inputs are LipiInputBase-derived so they participate in the touched-state validation flow (per §12.6.1).

---

## §27.6 — Edit input components

Per §3.4 — each column type has a default edit input. These exist in the LiPi component library (some from prior phases, some new in Phase 2.8). LipiTable references them via the column's edit-input registry:

| Column type | Default edit input | Source |
|---|---|---|
| Text | `LipiTextBox` | Phase 2.2 |
| Number | `LipiNumberInput` | Phase 2.2 |
| Currency | `LipiNumberInput` (formatted) | Phase 2.2 |
| Date | `LipiDatePicker` | Phase 2.4 |
| DateTime | `LipiDateTimePicker` | Phase 2.4 |
| Time | `LipiTimePicker` | Phase 2.4 |
| Boolean | `LipiCheckbox` / `LipiSwitch` | Phase 2.3 |
| Mono | `LipiTextBox` mono variant | Phase 2.2 |
| Status | `LipiSelect` with options | Phase 2.2 |

No new edit input components are added in Phase 2.8 — LipiTable reuses what exists.

---

## §27.7 — Export infrastructure

| File | Purpose |
|---|---|
| `Export/CsvExporter.cs` | In-house CSV exporter (RFC 4180 compliant, ~150 LOC) |
| `Export/CsvExportOptions.cs` | CSV-specific options record |
| `Export/PdfExporter.cs` | Stub for Phase 2.10 (throws NotImplementedException) |
| `Export/PdfExportOptions.cs` | PDF-specific options record |
| `Export/PrintHandler.cs` | Print invocation via JS interop |
| `Export/ExportTypes.cs` | ExportFormat enum + ExportScope enum + shared records |
| `Export/IExportColumnInfo.cs` | Internal interface for column→exporter info adapter |

Build chat implements `CsvExporter` fully; `PdfExporter` is the stub.

---

## §27.8 — Services and abstractions

| File | Purpose |
|---|---|
| `Services/ITablePreferenceService.cs` | Persistence backend interface |
| `Services/TablePreferenceService.cs` | Default EF-backed implementation (per §21.4.2) |
| `Services/TablePreferences.cs` | TablePreferences + ColumnPreference records |
| `Services/IDateFormatService.cs` | (Existing from prior phases; used here for date column formatting) |

If `IDateFormatService` already exists in another phase's codebase, reuse — don't duplicate.

---

## §27.9 — JS interop

| File | Purpose |
|---|---|
| `wwwroot/lipi-table-interop.js` | ES6 module with `triggerPrint`, `downloadBlob`, `measureCellWidth`, `scrollIntoView` |

Lazy-loaded via `IJSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/LiPi.Components/lipi-table-interop.js")`.

---

## §27.10 — CSS files

Beyond the scoped `.razor.css` files (one per component), shared CSS:

| File | Purpose |
|---|---|
| `wwwroot/css/lipi-table-tokens.css` | Table-specific tokens (per §24) |
| `wwwroot/css/lipi-table-print.css` | Print stylesheet (per §17.7.2) |
| `wwwroot/css/lipi-status-tokens.css` | Shared status taxonomy tokens (per Phase 2.8 §2.2) — used by LipiTable + LipiCard + LipiAlert |

`lipi-status-tokens.css` is shared infrastructure. If it already exists from earlier Phase 2.8 work on LipiCard / LipiAlert retrofit, LipiTable references the same file (don't duplicate).

---

## §27.11 — Sibling component files

### §27.11.1 — LipiList

```
src/LiPi.Components/DataDisplay/LipiList/
├── LipiList.razor + .razor.cs + .razor.css
├── LipiListItem.razor + .razor.css
├── LipiListTypes.cs
└── (sub-components per LipiList spec)
```

Full file list in `02-LipiList-Spec-OUTLINE.md` (and its forthcoming body section).

### §27.11.2 — LipiPagination

```
src/LiPi.Components/DataDisplay/LipiPagination/
├── LipiPagination.razor + .razor.cs + .razor.css
├── LipiPaginationCompact.razor + .razor.css
├── LipiPaginationMini.razor + .razor.css
├── LipiPaginationLoadMore.razor + .razor.css
├── LipiPaginationPageSize.razor + .razor.css
└── LipiPaginationTypes.cs
```

Full file list in `03-LipiPagination-Spec-OUTLINE.md`.

### §27.11.3 — LipiEmptyState

```
src/LiPi.Components/DataDisplay/LipiEmptyState/
├── LipiEmptyState.razor + .razor.cs + .razor.css
├── LipiEmptyStateTypes.cs
└── (variants if multi-file)
```

Full file list in `04-LipiEmptyState-Spec-OUTLINE.md`.

---

## §27.12 — Database migrations

| File | Purpose |
|---|---|
| `database/migrations/identity/2026-XX-XX_AddUserTablePreferences.sql` | Create the `identity.user_table_preferences` table per §21.3.1 |
| `database/efcore/LiPi.Identity.Core/Entities/UserTablePreference.cs` | EF entity for the table |
| `database/efcore/LiPi.Identity.Core/Configurations/UserTablePreferenceConfiguration.cs` | Fluent API config for EF |

Per LiPi's hybrid DB docs (per locked decision #3 from `00-PROJECT-FACTS`):
- SQL migration in `database/migrations/`
- EF entity in `database/efcore/`
- Documentation in `docs/00-DATABASE/`

Build chat writes all three.

---

## §27.13 — Demo files

Per §26, 12 demo pages. Each lives at:

```
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/
├── LipiTableBasicsDemo.razor + .razor.cs
├── LipiTableSelectionDemo.razor + .razor.cs
├── LipiTableSortDemo.razor + .razor.cs
├── LipiTableFilterDemo.razor + .razor.cs
├── LipiTablePaginationDemo.razor + .razor.cs
├── LipiTableEditDemo.razor + .razor.cs
├── LipiTableTreeDemo.razor + .razor.cs
├── LipiTableMasterDetailDemo.razor + .razor.cs
├── LipiTableGroupingDemo.razor + .razor.cs
├── LipiListDemo.razor + .razor.cs
├── LipiPaginationStandaloneDemo.razor + .razor.cs
├── LipiEmptyStateStandaloneDemo.razor + .razor.cs
└── Shared/
    ├── SampleData.cs                    ← shared demo data generator
    └── (demo model classes — DemoOrder, DemoProject, etc.)
```

Demo files live in `LiPi.Web` (not the redistributable `LiPi.Components`) per §25.2.2 — demos are HIS-app-specific even though they avoid HIS terminology.

The shared `SampleData.cs` is referenced by all 12 demos. Demo model classes (DemoOrder, DemoProject, etc.) are POCOs without LiPi-domain ties.

---

## §27.14 — Spec documentation files

Per §21.4 and §26.21, the spec markdown files themselves are deliverables. They live under:

```
docs/08-COMPONENTS/2.8-DataDisplay/
├── 00-Phase2.8-Overview.md
├── 01-LipiTable-Spec.md             ← consolidated from all S## section files
├── 02-LipiList-Spec.md
├── 03-LipiPagination-Spec.md
├── 04-LipiEmptyState-Spec.md
├── 05-Standing-Rule-Addition-Library-Dependency-Posture.md
└── 06-FLAGS-LOG.md
```

Build chat consolidates the section files (`01-LipiTable-S01-Overview.md` through `01-LipiTable-S28-Deploy.md`) into a single `01-LipiTable-Spec.md` for the docs directory. The section files remain as working copies in `lipi-specs/2.8/sections/`.

The `CHANGE-LOG.md` at the project root is updated with a Phase 2.8 entry pointing to these docs.

---

## §27.15 — Test files

Per LiPi's standing rule on testing (and the Phase 2.10 audit's automation expectations):

| File | Purpose |
|---|---|
| `test/LiPi.Components.Tests/DataDisplay/LipiTableTests.cs` | Unit tests for LipiTable core |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableSelectionTests.cs` | Selection-specific tests |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableSortFilterTests.cs` | Sort + filter tests |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableEditTests.cs` | Inline edit tests including conflict scenarios |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableTreeTests.cs` | Tree data tests |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableMasterDetailTests.cs` | Master-detail tests |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableGroupingTests.cs` | Grouping tests |
| `test/LiPi.Components.Tests/DataDisplay/LipiTablePersistenceTests.cs` | Persistence + ITablePreferenceService tests |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableExportTests.cs` | Export tests (CSV especially; PDF stubbed) |
| `test/LiPi.Components.Tests/DataDisplay/LipiTableEventOrderingTests.cs` | Event ordering sequences per §23.11 |
| `test/LiPi.Components.Tests/DataDisplay/Export/CsvExporterTests.cs` | RFC 4180 edge cases |
| `test/LiPi.Components.Tests/DataDisplay/Services/TablePreferenceServiceTests.cs` | Persistence backend tests |
| `test/LiPi.Components.Tests/DataDisplay/Helpers/MockDataSource.cs` | Test helpers |

Build chat writes the tests using bUnit (Blazor's standard testing library) per the LiPi standing rule on test framework choice.

---

## §27.16 — Summary file count

Approximate counts:

| Category | Count |
|---|---|
| Core LipiTable (component + types) | 8 |
| Layout sub-components (×3 files each: razor + cs + css) | ~30 |
| Row sub-components | ~15 |
| Cell sub-components | ~18 |
| Popover/drawer sub-components | ~27 |
| Pagination sub-components | ~15 |
| Filter operator components | ~9 |
| Export infrastructure | 7 |
| Services + abstractions | 4 |
| JS interop | 1 |
| CSS files (shared) | 3 |
| LipiList | ~6 |
| LipiEmptyState | ~4 |
| Database migrations + entities | 3 |
| Demo files (×2 files each: razor + cs) + shared | ~26 |
| Spec documentation | 7 |
| Test files | ~14 |
| **Total** | **~197 files** |

Substantial. Phase 2.8 is the largest single phase in the LiPi component library's history. Build chat should plan for staged delivery rather than one mega-PR.

---

## §27.17 — Recommended build order

Build chat should create files in this order to minimize broken-build periods:

### Stage 1 — Foundation
1. Type files (`LipiTableTypes.cs`, `LipiColumnTypes.cs`, `TableQueryRequest.cs`, etc.)
2. Services (`ITablePreferenceService.cs`, `TablePreferenceService.cs`)
3. Database migration + EF entity + configuration
4. Shared CSS (`lipi-table-tokens.css`, `lipi-status-tokens.css`)
5. JS interop module

**Verification:** project compiles; service registers in DI; migration runs cleanly.

### Stage 2 — LipiTable core
6. `LipiTable.razor` + `.razor.cs` + `.razor.css` (skeleton — empty body, no features)
7. `LipiColumn.razor` + `.razor.cs`
8. Layout sub-components (header band, toolbar shell, body shell, footer shell)

**Verification:** minimal LipiTable renders 5 columns of static data, no interactivity.

### Stage 3 — Row/cell rendering
9. Row sub-components (LipiTableRow, group header, detail row, skeleton row, aggregate row)
10. Cell sub-components (default cell, header cell, selection cell, expand cell, actions cell)

**Verification:** rows render with all cell types; selection checkbox visible.

### Stage 4 — Interactive features
11. Sort + filter (column header sort indicator, filter popover, filter chips)
12. Pagination (LipiPagination component, pagination footer integration)
13. Selection (checkbox interaction, bulk action bar)

**Verification:** sort by clicking headers; filter via popover; paginate; select rows.

### Stage 5 — Advanced features
14. Inline edit (Row mode first, then Cell mode)
15. Tree data (ChildrenSelector first, then ParentSelector)
16. Master-detail
17. Grouping (single-level first, then multi-level + drag-to-group bar)

**Verification:** all interactive demos in StyleGuide work end-to-end.

### Stage 6 — Polish + persistence
18. Persistence (read on mount, debounced writes, reset)
19. Density toggle
20. Column ops (resize, reorder, pin)
21. Column picker
22. Export (CSV first; PDF stubbed; Print)

**Verification:** preferences persist across reloads; export downloads correctly.

### Stage 7 — Sibling components
23. LipiList
24. LipiEmptyState
25. LipiPagination standalone variants

**Verification:** siblings render in their respective StyleGuide demos.

### Stage 8 — Demo pages
26. Each of 12 StyleGuide demo pages
27. Shared SampleData generator
28. Spec docs consolidation into `docs/`

**Verification:** every StyleGuide demo renders; spec docs link from demo pages.

### Stage 9 — Tests + audit
29. Unit tests per §27.15
30. axe-core CI integration
31. Performance benchmark runs
32. Component isolation contract verification (§25.8 audit checklist)

**Verification:** all tests pass; axe-core finds no violations; benchmarks within budgets; isolation audit clean.

---

## §27.18 — Files NOT in LipiTable scope

To prevent confusion, these are NOT files Phase 2.8 creates:

- `LipiCard`, `LipiAlert` — already exist (Phase 2.6.1). Phase 2.10 audit will retrofit them to use `lipi-status-tokens.css` per §22.1.
- `LipiModal`, `LipiDrawer` — already exist (Phase 2.6.2). LipiTable composes them.
- `LipiSpinner`, `LipiBadge`, `LipiPill`, `LipiSkeleton`, `LipiToast`, `LipiValidationSummary` — already exist (Phase 2.7). LipiTable composes them.
- `LipiTextBox`, `LipiSelect`, `LipiDatePicker`, etc. — already exist (Phase 2.2–2.4). LipiTable composes them.
- LiPi PDF library — Phase 2.10 deliverable. LipiTable's PdfExporter stubs until then.
- Excel writer — deferred per §17 + `03-DEFERRED-ITEMS.md`.

If during build, build chat finds a needed component that doesn't exist, the gap is flagged in `06-FLAGS-LOG.md` for next strategic-chat amendment.

---

## §27.19 — File creation contract

Per LiPi standing rules:

- Every new file is added to `deploy-downloads.ps1` (per §28)
- Every file has the SPEC comment header pointing back to the relevant section
- Every Razor file's code-behind is `.razor.cs` (NOT inline `@code` for >50 lines)
- Every Razor file's CSS is scoped `.razor.css`
- Shared CSS goes in `wwwroot/css/lipi-*.css` with documented dependency on the foundation tokens

The spec comment header template per file:

```csharp
// SPEC: docs/08-COMPONENTS/2.8-DataDisplay/01-LipiTable-Spec.md §<section>
// PHASE: 2.8 Data Display
// COMPONENT: LipiTable
//
// <brief description>
```

```razor
@* SPEC: docs/08-COMPONENTS/2.8-DataDisplay/01-LipiTable-Spec.md §<section> *@
@* PHASE: 2.8 *@
```

```css
/* SPEC: docs/08-COMPONENTS/2.8-DataDisplay/01-LipiTable-Spec.md §24 (tokens) */
/* PHASE: 2.8 */
```

This makes every file traceable back to its spec section. Phase 2.10 audit grep-checks every file has a SPEC comment.

---

*End of §27. Proceed to §28 — Deploy script additions.*


# LipiTable Spec — §28 Deploy script additions

**Phase:** 2.8 — Data Display
**Component:** `LipiTable<TItem>`
**Status:** Section body — draft for review (final section of LipiTable spec)
**Depends on:** §27 (files to create)
**Cross-references:** `deploy-downloads.ps1` (project root), LiPi standing rule on full files only

---

## §28.1 — Purpose

Per the LiPi operational rule (per `04-OPERATIONAL-RULES.md`), every new file MUST be registered in `deploy-downloads.ps1`. This section enumerates the additions needed for Phase 2.8.

The deploy script:
- Source: `C:\Users\aruns\Downloads\LiPi\` (where Arun drops new files from Claude)
- Target: `C:\Users\aruns\Documents\lipi-complete\lipi-complete\` (the project root)
- Copies each registered file from source to target, creating directories as needed
- Idempotent — re-running re-copies (overwrites) all files; no incremental logic

This means: build chat receives files in `Downloads\LiPi\`, then Arun runs `deploy-downloads.ps1` to install them. The script is the source of truth for which files exist in the project.

---

## §28.2 — Script structure

The existing `deploy-downloads.ps1` has a structure roughly:

```powershell
$SourceRoot = "$env:USERPROFILE\Downloads\LiPi"
$TargetRoot = "$env:USERPROFILE\Documents\lipi-complete\lipi-complete"

$Files = @(
    @{ Source = "..."; Target = "..." },
    @{ Source = "..."; Target = "..." },
    # ... etc
)

foreach ($file in $Files) {
    $sourcePath = Join-Path $SourceRoot $file.Source
    $targetPath = Join-Path $TargetRoot $file.Target
    
    if (Test-Path $sourcePath) {
        $targetDir = Split-Path $targetPath -Parent
        if (-not (Test-Path $targetDir)) {
            New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
        }
        Copy-Item -Path $sourcePath -Destination $targetPath -Force
        Write-Host "✓ Copied $($file.Source)"
    } else {
        Write-Host "⚠ Missing $($file.Source)" -ForegroundColor Yellow
    }
}
```

The `$Files` array is the registry. Each entry maps a source filename (drop name in `Downloads\LiPi\`) to the destination path (under project root).

Phase 2.8 appends ~197 entries to `$Files`. The convention: build chat appends them grouped by category for readability.

---

## §28.3 — Naming convention for drops

Files dropped in `Downloads\LiPi\` use flattened naming with category prefixes to avoid filename collisions:

| Pattern | Example |
|---|---|
| `2.8-components-{component}.razor` | `2.8-components-LipiTable.razor` |
| `2.8-components-{component}.razor.cs` | `2.8-components-LipiTable.razor.cs` |
| `2.8-components-{component}.razor.css` | `2.8-components-LipiTable.razor.css` |
| `2.8-types-{name}.cs` | `2.8-types-LipiTableTypes.cs` |
| `2.8-services-{name}.cs` | `2.8-services-ITablePreferenceService.cs` |
| `2.8-export-{name}.cs` | `2.8-export-CsvExporter.cs` |
| `2.8-filters-{name}.razor` | `2.8-filters-FilterTextInput.razor` |
| `2.8-css-{name}.css` | `2.8-css-lipi-table-tokens.css` |
| `2.8-js-{name}.js` | `2.8-js-lipi-table-interop.js` |
| `2.8-demo-{name}.razor` | `2.8-demo-LipiTableBasicsDemo.razor` |
| `2.8-db-migration-{date}-{name}.sql` | `2.8-db-migration-2026-05-15-AddUserTablePreferences.sql` |
| `2.8-db-entity-{name}.cs` | `2.8-db-entity-UserTablePreference.cs` |
| `2.8-test-{name}.cs` | `2.8-test-LipiTableTests.cs` |
| `2.8-docs-{name}.md` | `2.8-docs-Phase2.8-Overview.md` |

For sub-components (internal to LipiTable), the prefix includes "internal":

```
2.8-components-internal-{name}.razor
2.8-components-internal-{name}.razor.cs
2.8-components-internal-{name}.razor.css
```

This naming makes the `Downloads\LiPi\` folder browsable — you can find a file by category prefix without remembering the exact path.

---

## §28.4 — Registry additions: foundation

Stage 1 of the build order (per §27.17). Foundation files:

```powershell
# === Phase 2.8 — Stage 1: Foundation ===

# Types
@{ Source = "2.8-types-LipiTableTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiTableTypes.cs" },
@{ Source = "2.8-types-LipiColumnTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiColumnTypes.cs" },
@{ Source = "2.8-types-TableQueryRequest.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\TableQueryRequest.cs" },
@{ Source = "2.8-types-TableQueryResponse.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\TableQueryResponse.cs" },
@{ Source = "2.8-types-SaveResult.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\SaveResult.cs" },
@{ Source = "2.8-types-Contexts.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Contexts.cs" },
@{ Source = "2.8-types-SortDescriptor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\SortDescriptor.cs" },
@{ Source = "2.8-types-LipiStatus.cs"; Target = "src\LiPi.Components\Shared\LipiStatus.cs" },

# Services
@{ Source = "2.8-services-ITablePreferenceService.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Services\ITablePreferenceService.cs" },
@{ Source = "2.8-services-TablePreferenceService.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Services\TablePreferenceService.cs" },
@{ Source = "2.8-services-TablePreferences.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Services\TablePreferences.cs" },

# Database
@{ Source = "2.8-db-migration-2026-05-15-AddUserTablePreferences.sql"; Target = "database\migrations\identity\2026-05-15_AddUserTablePreferences.sql" },
@{ Source = "2.8-db-entity-UserTablePreference.cs"; Target = "database\efcore\LiPi.Identity.Core\Entities\UserTablePreference.cs" },
@{ Source = "2.8-db-config-UserTablePreferenceConfiguration.cs"; Target = "database\efcore\LiPi.Identity.Core\Configurations\UserTablePreferenceConfiguration.cs" },

# Shared CSS
@{ Source = "2.8-css-lipi-table-tokens.css"; Target = "src\LiPi.Web\wwwroot\css\lipi-table-tokens.css" },
@{ Source = "2.8-css-lipi-table-print.css"; Target = "src\LiPi.Web\wwwroot\css\lipi-table-print.css" },
@{ Source = "2.8-css-lipi-status-tokens.css"; Target = "src\LiPi.Web\wwwroot\css\lipi-status-tokens.css" },

# JS interop
@{ Source = "2.8-js-lipi-table-interop.js"; Target = "src\LiPi.Components\wwwroot\lipi-table-interop.js" },
```

After Stage 1 drops, build chat verifies: project compiles, EF migration runs, services register in DI, CSS file referenced in App.razor.

---

## §28.5 — Registry additions: core component

Stage 2: LipiTable + LipiColumn + layout shells.

```powershell
# === Phase 2.8 — Stage 2: Core component ===

# Main LipiTable
@{ Source = "2.8-components-LipiTable.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor" },
@{ Source = "2.8-components-LipiTable.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor.cs" },
@{ Source = "2.8-components-LipiTable.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor.css" },

# LipiColumn
@{ Source = "2.8-components-LipiColumn.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor" },
@{ Source = "2.8-components-LipiColumn.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor.cs" },

# Layout sub-components (region shells)
@{ Source = "2.8-components-internal-LipiTableHeader.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableHeader.razor" },
@{ Source = "2.8-components-internal-LipiTableHeader.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableHeader.razor.css" },
@{ Source = "2.8-components-internal-LipiTableToolbar.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableToolbar.razor" },
@{ Source = "2.8-components-internal-LipiTableToolbar.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableToolbar.razor.css" },
@{ Source = "2.8-components-internal-LipiTableFilterChips.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterChips.razor" },
@{ Source = "2.8-components-internal-LipiTableFilterChips.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterChips.razor.css" },
@{ Source = "2.8-components-internal-LipiTableBulkActionBar.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableBulkActionBar.razor" },
@{ Source = "2.8-components-internal-LipiTableBulkActionBar.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableBulkActionBar.razor.css" },
@{ Source = "2.8-components-internal-LipiTableGroupBar.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableGroupBar.razor" },
@{ Source = "2.8-components-internal-LipiTableGroupBar.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableGroupBar.razor.css" },
@{ Source = "2.8-components-internal-LipiTableCapBanner.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableCapBanner.razor" },
@{ Source = "2.8-components-internal-LipiTableCapBanner.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableCapBanner.razor.css" },
@{ Source = "2.8-components-internal-LipiTableColumnHeaders.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableColumnHeaders.razor" },
@{ Source = "2.8-components-internal-LipiTableColumnHeaders.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableColumnHeaders.razor.css" },
@{ Source = "2.8-components-internal-LipiTableBody.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableBody.razor" },
@{ Source = "2.8-components-internal-LipiTableBody.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableBody.razor.css" },
@{ Source = "2.8-components-internal-LipiTableFooter.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFooter.razor" },
@{ Source = "2.8-components-internal-LipiTableFooter.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFooter.razor.css" },
@{ Source = "2.8-components-internal-LipiTableEditStickyBar.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableEditStickyBar.razor" },
@{ Source = "2.8-components-internal-LipiTableEditStickyBar.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableEditStickyBar.razor.css" },
```

**Note:** Each sub-component has a `.razor` and `.razor.css` pair. Where a sub-component has complex code-behind, a `.razor.cs` is added as a third file.

---

## §28.6 — Registry additions: rows and cells

Stage 3.

```powershell
# === Phase 2.8 — Stage 3: Rows and cells ===

# Rows
@{ Source = "2.8-components-internal-LipiTableRow.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableRow.razor" },
@{ Source = "2.8-components-internal-LipiTableRow.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableRow.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableRow.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableRow.razor.css" },
@{ Source = "2.8-components-internal-LipiTableGroupHeaderRow.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableGroupHeaderRow.razor" },
@{ Source = "2.8-components-internal-LipiTableGroupHeaderRow.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableGroupHeaderRow.razor.css" },
@{ Source = "2.8-components-internal-LipiTableDetailRow.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableDetailRow.razor" },
@{ Source = "2.8-components-internal-LipiTableDetailRow.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableDetailRow.razor.css" },
@{ Source = "2.8-components-internal-LipiTableSkeletonRow.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableSkeletonRow.razor" },
@{ Source = "2.8-components-internal-LipiTableSkeletonRow.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableSkeletonRow.razor.css" },
@{ Source = "2.8-components-internal-LipiTableAggregateRow.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableAggregateRow.razor" },
@{ Source = "2.8-components-internal-LipiTableAggregateRow.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableAggregateRow.razor.css" },

# Cells
@{ Source = "2.8-components-internal-LipiTableCell.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableCell.razor" },
@{ Source = "2.8-components-internal-LipiTableCell.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableCell.razor.css" },
@{ Source = "2.8-components-internal-LipiTableHeaderCell.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableHeaderCell.razor" },
@{ Source = "2.8-components-internal-LipiTableHeaderCell.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableHeaderCell.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableHeaderCell.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableHeaderCell.razor.css" },
@{ Source = "2.8-components-internal-LipiTableEditCell.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableEditCell.razor" },
@{ Source = "2.8-components-internal-LipiTableEditCell.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableEditCell.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableEditCell.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableEditCell.razor.css" },
@{ Source = "2.8-components-internal-LipiTableSelectionCell.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableSelectionCell.razor" },
@{ Source = "2.8-components-internal-LipiTableSelectionCell.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableSelectionCell.razor.css" },
@{ Source = "2.8-components-internal-LipiTableExpandCell.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableExpandCell.razor" },
@{ Source = "2.8-components-internal-LipiTableExpandCell.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableExpandCell.razor.css" },
@{ Source = "2.8-components-internal-LipiTableActionsCell.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableActionsCell.razor" },
@{ Source = "2.8-components-internal-LipiTableActionsCell.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableActionsCell.razor.css" },
```

---

## §28.7 — Registry additions: popovers, drawers, filters

Stage 4 + filter components.

```powershell
# === Phase 2.8 — Stage 4: Interactive features ===

# Popovers and modals
@{ Source = "2.8-components-internal-LipiTableColumnPicker.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableColumnPicker.razor" },
@{ Source = "2.8-components-internal-LipiTableColumnPicker.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableColumnPicker.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableColumnPicker.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableColumnPicker.razor.css" },
@{ Source = "2.8-components-internal-LipiTableContextMenu.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableContextMenu.razor" },
@{ Source = "2.8-components-internal-LipiTableContextMenu.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableContextMenu.razor.css" },
@{ Source = "2.8-components-internal-LipiTableFilterPopover.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterPopover.razor" },
@{ Source = "2.8-components-internal-LipiTableFilterPopover.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterPopover.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableFilterPopover.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterPopover.razor.css" },
@{ Source = "2.8-components-internal-LipiTableFilterDrawer.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterDrawer.razor" },
@{ Source = "2.8-components-internal-LipiTableFilterDrawer.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterDrawer.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableFilterDrawer.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableFilterDrawer.razor.css" },
@{ Source = "2.8-components-internal-LipiTableExportModal.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableExportModal.razor" },
@{ Source = "2.8-components-internal-LipiTableExportModal.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableExportModal.razor.cs" },
@{ Source = "2.8-components-internal-LipiTableExportModal.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableExportModal.razor.css" },
@{ Source = "2.8-components-internal-LipiTableConflictBanner.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableConflictBanner.razor" },
@{ Source = "2.8-components-internal-LipiTableConflictBanner.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableConflictBanner.razor.css" },
@{ Source = "2.8-components-internal-LipiTableConflictDiffPopover.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableConflictDiffPopover.razor" },
@{ Source = "2.8-components-internal-LipiTableConflictDiffPopover.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableConflictDiffPopover.razor.css" },
@{ Source = "2.8-components-internal-LipiTableConflictModal.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableConflictModal.razor" },
@{ Source = "2.8-components-internal-LipiTableConflictModal.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableConflictModal.razor.css" },
@{ Source = "2.8-components-internal-LipiTableValidationStrip.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableValidationStrip.razor" },
@{ Source = "2.8-components-internal-LipiTableValidationStrip.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Internal\LipiTableValidationStrip.razor.css" },

# Filter operator inputs
@{ Source = "2.8-filters-FilterTextInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterTextInput.razor" },
@{ Source = "2.8-filters-FilterNumberInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterNumberInput.razor" },
@{ Source = "2.8-filters-FilterNumberRangeInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterNumberRangeInput.razor" },
@{ Source = "2.8-filters-FilterDateInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterDateInput.razor" },
@{ Source = "2.8-filters-FilterDateRangeInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterDateRangeInput.razor" },
@{ Source = "2.8-filters-FilterRelativeDateInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterRelativeDateInput.razor" },
@{ Source = "2.8-filters-FilterSetInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterSetInput.razor" },
@{ Source = "2.8-filters-FilterBooleanInput.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterBooleanInput.razor" },
@{ Source = "2.8-filters-FilterBuilder.razor"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterBuilder.razor" },
@{ Source = "2.8-filters-FilterBuilder.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Filters\FilterBuilder.razor.cs" },
```

---

## §28.8 — Registry additions: export, pagination, siblings

Stages 5 + 7.

```powershell
# === Phase 2.8 — Stage 5 (export) + Stage 7 (siblings) ===

# Export infrastructure
@{ Source = "2.8-export-CsvExporter.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\CsvExporter.cs" },
@{ Source = "2.8-export-CsvExportOptions.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\CsvExportOptions.cs" },
@{ Source = "2.8-export-PdfExporter.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\PdfExporter.cs" },
@{ Source = "2.8-export-PdfExportOptions.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\PdfExportOptions.cs" },
@{ Source = "2.8-export-PrintHandler.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\PrintHandler.cs" },
@{ Source = "2.8-export-ExportTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\ExportTypes.cs" },
@{ Source = "2.8-export-IExportColumnInfo.cs"; Target = "src\LiPi.Components\DataDisplay\LipiTable\Export\IExportColumnInfo.cs" },

# LipiPagination
@{ Source = "2.8-components-LipiPagination.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPagination.razor" },
@{ Source = "2.8-components-LipiPagination.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPagination.razor.cs" },
@{ Source = "2.8-components-LipiPagination.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPagination.razor.css" },
@{ Source = "2.8-components-LipiPaginationCompact.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationCompact.razor" },
@{ Source = "2.8-components-LipiPaginationCompact.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationCompact.razor.css" },
@{ Source = "2.8-components-LipiPaginationMini.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationMini.razor" },
@{ Source = "2.8-components-LipiPaginationMini.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationMini.razor.css" },
@{ Source = "2.8-components-LipiPaginationLoadMore.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationLoadMore.razor" },
@{ Source = "2.8-components-LipiPaginationLoadMore.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationLoadMore.razor.css" },
@{ Source = "2.8-components-LipiPaginationPageSize.razor"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationPageSize.razor" },
@{ Source = "2.8-components-LipiPaginationPageSize.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationPageSize.razor.css" },
@{ Source = "2.8-types-LipiPaginationTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiPagination\LipiPaginationTypes.cs" },

# LipiList
@{ Source = "2.8-components-LipiList.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiList.razor" },
@{ Source = "2.8-components-LipiList.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiList.razor.cs" },
@{ Source = "2.8-components-LipiList.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiList.razor.css" },
@{ Source = "2.8-components-LipiListItem.razor"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiListItem.razor" },
@{ Source = "2.8-components-LipiListItem.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiListItem.razor.css" },
@{ Source = "2.8-types-LipiListTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiList\LipiListTypes.cs" },

# LipiEmptyState
@{ Source = "2.8-components-LipiEmptyState.razor"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor" },
@{ Source = "2.8-components-LipiEmptyState.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.cs" },
@{ Source = "2.8-components-LipiEmptyState.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.css" },
@{ Source = "2.8-types-LipiEmptyStateTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyStateTypes.cs" },
```

---

## §28.9 — Registry additions: demos

Stage 8.

```powershell
# === Phase 2.8 — Stage 8: StyleGuide demos ===

# Shared demo infrastructure
@{ Source = "2.8-demo-SampleData.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\Shared\SampleData.cs" },

# Demo pages
@{ Source = "2.8-demo-LipiTableBasicsDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableBasicsDemo.razor" },
@{ Source = "2.8-demo-LipiTableBasicsDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableBasicsDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableSelectionDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableSelectionDemo.razor" },
@{ Source = "2.8-demo-LipiTableSelectionDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableSelectionDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableSortDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableSortDemo.razor" },
@{ Source = "2.8-demo-LipiTableSortDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableSortDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableFilterDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableFilterDemo.razor" },
@{ Source = "2.8-demo-LipiTableFilterDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableFilterDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTablePaginationDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTablePaginationDemo.razor" },
@{ Source = "2.8-demo-LipiTablePaginationDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTablePaginationDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableEditDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableEditDemo.razor" },
@{ Source = "2.8-demo-LipiTableEditDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableEditDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableTreeDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableTreeDemo.razor" },
@{ Source = "2.8-demo-LipiTableTreeDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableTreeDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableMasterDetailDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableMasterDetailDemo.razor" },
@{ Source = "2.8-demo-LipiTableMasterDetailDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableMasterDetailDemo.razor.cs" },
@{ Source = "2.8-demo-LipiTableGroupingDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableGroupingDemo.razor" },
@{ Source = "2.8-demo-LipiTableGroupingDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiTableGroupingDemo.razor.cs" },
@{ Source = "2.8-demo-LipiListDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiListDemo.razor" },
@{ Source = "2.8-demo-LipiListDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiListDemo.razor.cs" },
@{ Source = "2.8-demo-LipiPaginationStandaloneDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiPaginationStandaloneDemo.razor" },
@{ Source = "2.8-demo-LipiPaginationStandaloneDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiPaginationStandaloneDemo.razor.cs" },
@{ Source = "2.8-demo-LipiEmptyStateStandaloneDemo.razor"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiEmptyStateStandaloneDemo.razor" },
@{ Source = "2.8-demo-LipiEmptyStateStandaloneDemo.razor.cs"; Target = "src\LiPi.Web\Components\Pages\StyleGuide\DataDisplay\LipiEmptyStateStandaloneDemo.razor.cs" },
```

---

## §28.10 — Registry additions: docs and tests

Final files.

```powershell
# === Phase 2.8 — Documentation ===

@{ Source = "2.8-docs-Phase2.8-Overview.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\00-Phase2.8-Overview.md" },
@{ Source = "2.8-docs-LipiTable-Spec.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\01-LipiTable-Spec.md" },
@{ Source = "2.8-docs-LipiList-Spec.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\02-LipiList-Spec.md" },
@{ Source = "2.8-docs-LipiPagination-Spec.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\03-LipiPagination-Spec.md" },
@{ Source = "2.8-docs-LipiEmptyState-Spec.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\04-LipiEmptyState-Spec.md" },
@{ Source = "2.8-docs-Standing-Rule-Addition.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\05-Standing-Rule-Addition-Library-Dependency-Posture.md" },
@{ Source = "2.8-docs-Flags-Log.md"; Target = "docs\08-COMPONENTS\2.8-DataDisplay\06-FLAGS-LOG.md" },

# === Phase 2.8 — Tests ===

@{ Source = "2.8-test-LipiTableTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableTests.cs" },
@{ Source = "2.8-test-LipiTableSelectionTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableSelectionTests.cs" },
@{ Source = "2.8-test-LipiTableSortFilterTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableSortFilterTests.cs" },
@{ Source = "2.8-test-LipiTableEditTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableEditTests.cs" },
@{ Source = "2.8-test-LipiTableTreeTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableTreeTests.cs" },
@{ Source = "2.8-test-LipiTableMasterDetailTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableMasterDetailTests.cs" },
@{ Source = "2.8-test-LipiTableGroupingTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableGroupingTests.cs" },
@{ Source = "2.8-test-LipiTablePersistenceTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTablePersistenceTests.cs" },
@{ Source = "2.8-test-LipiTableExportTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableExportTests.cs" },
@{ Source = "2.8-test-LipiTableEventOrderingTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiTableEventOrderingTests.cs" },
@{ Source = "2.8-test-CsvExporterTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\Export\CsvExporterTests.cs" },
@{ Source = "2.8-test-TablePreferenceServiceTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\Services\TablePreferenceServiceTests.cs" },
@{ Source = "2.8-test-MockDataSource.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\Helpers\MockDataSource.cs" },
```

---

## §28.11 — Operational rules for the deploy script

Per `04-OPERATIONAL-RULES.md` (LiPi standing rules):

### §28.11.1 — Idempotency

The script is idempotent — running it multiple times produces the same result. No incremental logic; every registered file is overwritten on each run.

This means: editing a file on `Downloads\LiPi\` and re-running the script applies the change.

### §28.11.2 — No duplicate keys

Each `$Files` entry's `Source` must be unique. Two entries with the same source name silently shadow each other (the last one wins in PowerShell hashtable iteration). Phase 2.10 audit verifies uniqueness.

When a build chat needs to deliver an updated file, it drops the same name and the script overwrites. The `Source` name is the stable identifier.

### §28.11.3 — Missing file handling

If `Test-Path $sourcePath` returns false (file not in `Downloads\LiPi\`), the script logs `⚠ Missing` and continues. This lets Arun deploy a partial drop without the script aborting.

### §28.11.4 — Directory creation

The script creates the target directory tree if needed (`New-Item -ItemType Directory -Force`). Build chat doesn't need to worry about whether `src\LiPi.Components\DataDisplay\LipiTable\Internal\` exists — the script creates it.

### §28.11.5 — Backup before overwrite (not implemented)

The current script does NOT back up existing files before overwriting. If a build chat delivers a wrong file, the previous version is lost (unless git history has it).

Future enhancement (deferred — not in Phase 2.8 scope): a `--backup` flag that copies overwritten files to `Downloads\LiPi\backup\YYYYMMDD\`. Could be added in a v1.X amendment.

### §28.11.6 — Post-deploy verification

After running the script, Arun verifies:
1. `dotnet build` succeeds
2. `dotnet run` starts the app without errors
3. The newly-deployed pages render in the browser
4. Console (browser + server terminal) has no errors

If verification fails, Arun reports back to the build chat with the error. The chat diagnoses and re-delivers.

---

## §28.12 — Phased delivery cadence

Per §27.17 — build chat delivers files in stages. Each stage's drop:

1. Build chat creates all files for the stage
2. Drops them into `Downloads\LiPi\` (Arun's local copy)
3. Updates the registry entries in `deploy-downloads.ps1` (build chat appends; Arun runs)
4. Arun runs `deploy-downloads.ps1`
5. Arun verifies (per §28.11.6)
6. Arun reports back: success → next stage; failure → fix-forward in same stage

A complete Phase 2.8 delivery = 9 stages over multiple days. Each stage is one "deployment unit."

### §28.12.1 — Drop naming for stages

Within each stage, the drop is a single zip or folder named like:

```
Downloads\LiPi\2.8-stage-1-foundation\
Downloads\LiPi\2.8-stage-2-core\
Downloads\LiPi\2.8-stage-3-rows-cells\
...
```

The deploy script's `$SourceRoot` is `Downloads\LiPi\` (root), so files in subfolders need their relative path encoded in the `Source` field:

```powershell
@{ Source = "2.8-stage-1-foundation\2.8-types-LipiTableTypes.cs"; Target = "..." }
```

OR the drop subfolder is flattened by build chat into `Downloads\LiPi\` root before delivery. The exact convention is build chat's call.

### §28.12.2 — CHANGE-LOG update per stage

Each stage's drop includes a CHANGE-LOG.md update entry:

```markdown
## Phase 2.8 — Stage 1: Foundation (2026-MM-DD)

- Add LipiTable type definitions (LipiTableTypes, LipiColumnTypes, TableQueryRequest, etc.)
- Add ITablePreferenceService + default EF implementation
- Add database migration for identity.user_table_preferences
- Add shared CSS: lipi-table-tokens.css, lipi-status-tokens.css
- Add JS interop module: lipi-table-interop.js

Files: ~17
```

Arun's review includes verifying the CHANGE-LOG update reflects what's actually in the drop.

---

## §28.13 — Re-registering existing files

Some Phase 2.8 work modifies existing files (e.g., `App.razor` to register Phase 2.8 CSS imports, `Program.cs` to register `ITablePreferenceService` in DI).

For these modifications, the deploy script includes the full file (not a patch):

```powershell
# Existing files being modified
@{ Source = "2.8-modified-App.razor"; Target = "src\LiPi.Web\Components\App.razor" },
@{ Source = "2.8-modified-Program.cs"; Target = "src\LiPi.Web\Program.cs" },
```

Per the standing rule "full files only, no patches" — modifications are delivered as full file replacements. Arun verifies the full file is correct before deploy.

For Phase 2.8, the expected modifications:
- `App.razor` — add `<link>` tags for `lipi-table-tokens.css`, `lipi-table-print.css`, `lipi-status-tokens.css`
- `Program.cs` — register `ITablePreferenceService` and any new services in DI
- `_Imports.razor` — add `@using LiPi.Components.DataDisplay` namespaces

Build chat delivers each as a complete file.

---

## §28.14 — Registry size after Phase 2.8

Before Phase 2.8: the registry has roughly 250 entries from prior phases.

After Phase 2.8: approximately 250 + 197 = **~447 entries**.

For maintainability, the registry can optionally be split into separate files (e.g., `deploy-downloads-base.ps1` for prior phases + `deploy-downloads-2.8.ps1` for Phase 2.8) and a master script that calls each. Deferred — not required for Phase 2.8 to ship; can refactor in v1.X.

---

## §28.15 — Phase 2.10 audit verification

The deploy script is one of the audit surfaces:

| Audit item | Check |
|---|---|
| All Phase 2.8 files registered | Grep `deploy-downloads.ps1` for each path enumerated in §27; missing paths flagged |
| No duplicate `Source` keys | PowerShell script-analysis (or simple deduplication check) |
| `Source` names follow naming convention (§28.3) | Regex grep on `Source` strings |
| All `Target` paths exist after deploy | `Test-Path` each registered target |

---

## §28.16 — Build-chat operational note

Build chat reads §27 + §28 together. §27 says WHAT files to create; §28 says HOW to deliver them.

The output of build chat per stage is:
1. The actual source files (in their final form, with SPEC headers per §27.19)
2. The PowerShell snippet to append to `deploy-downloads.ps1`
3. The CHANGE-LOG.md update entry
4. A short "deploy checklist" for Arun:
   - Files included: [list]
   - Modifications to existing files: [list with brief description of what changed]
   - Post-deploy verification: [what to test specifically]
   - Known issues / partial behavior: [anything explicit]

This makes each drop self-contained — Arun has everything needed to deploy and verify.

---

*End of §28. End of LipiTable spec. Proceed to sibling component spec bodies: LipiList, LipiPagination, LipiEmptyState.*

