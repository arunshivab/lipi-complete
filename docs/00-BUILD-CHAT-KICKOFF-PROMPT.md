# BUILD CHAT KICKOFF — Phase 2.8 Data Display

Copy this entire message into the new build chat (project), along with the 7 attached spec files.

---

## Project context

I'm Arun, solo developer building **LiPi HIS** (Hospital Information System) under the **Armoki HIS** brand. Multi-tenant Blazor Web App (.NET 10 + InteractiveServer only, no WASM, PostgreSQL + EF Core 10 + Npgsql). Project root: `C:\Users\aruns\Documents\lipi-complete\lipi-complete`. Deploy via `Downloads\LiPi\` + `deploy-downloads.ps1`.

The strategic chat has just locked the **Phase 2.8 Data Display spec corpus** (~22,000 lines). You're the build chat. Your job: implement Phase 2.8 from these specs, staged across 9 build stages with verification gates between each.

---

## What's attached

7 spec files representing the complete Phase 2.8 design corpus:

1. **`00-Phase2.8-Overview.md`** — Phase scope and posture
2. **`01-LipiTable-Spec.md`** — LipiTable spec (28 sections, ~18,300 lines) — the main component
3. **`02-LipiList-Spec.md`** — LipiList sibling (21 sections)
4. **`03-LipiPagination-Spec.md`** — LipiPagination sibling (12 sections) — **read §0 first for naming reconciliation**
5. **`04-LipiEmptyState-Spec.md`** — LipiEmptyState sibling (10 sections + §0 reconciliation)
6. **`05-Standing-Rule-Addition-Library-Dependency-Posture.md`** — permanent rule added in Phase 2.8
7. **`06-FLAGS-LOG.md`** — 126 flagged items dispositioned for you (LOCKED / Build chat note / Phase 2.10 audit)

---

## Critical reads BEFORE starting any implementation

In this order:

1. **`06-FLAGS-LOG.md`** — read the entire log; ~30 items are flagged "🟡 Build chat note" and need your direct attention during implementation. The single most critical item is **LP-0** (naming reconciliation across LipiPagination variants).

2. **`01-LipiTable-Spec.md §27`** — Files to create (~197 files; the full manifest). And **§28** — Deploy script additions.

3. **`01-LipiTable-Spec.md §25`** — Component isolation contract. This is the audit gate for Phase 2.10. Every implementation decision must comply.

4. **`00-Phase2.8-Overview.md`** — phase principles, especially the zero-external-library posture and shared `lipi-status-tokens.css` infrastructure.

After those reads, proceed with Stage 1 of the build order documented in **§27.17**.

---

## Mandatory operational rules

These are LiPi standing rules — non-negotiable across every interaction:

1. **Full files only** — never patches, snippets, diff hunks. Every code delivery is a complete file. Modifications to existing files = complete file replacement.

2. **Every new file added to `deploy-downloads.ps1`** — per the conventions in §28. No exceptions.

3. **SPEC comment header on every file** — pointing to the relevant section. Templates in §27.19. Phase 2.10 audit grep-checks compliance.

4. **`.razor.cs` for code-behind** (NOT inline `@code` for blocks >50 lines). `.razor.css` for scoped CSS. CSS prefix strictly `lipi-*`.

5. **Standards-first design discussion** — when you find spec ambiguity, surface it; don't silently choose. Present options + trade-offs from industry standards / best practices. I'll decide.

6. **Deviation handling** — if you propose something that contradicts a locked decision (anything marked 🟢 in the flags log, or any explicit "Locked" / "Standing rule" in spec body), STOP, cross-reference the locked decision, and flag the conflict. Never silently deviate.

7. **Stack constraints** — .NET 10 always. Blazor Web App with InteractiveServer (no WASM). PostgreSQL + EF Core 10 + Npgsql. Lucide icons (Lipicons deferred to Phase 3.0). NEVER suggest .NET 7/8/9 patterns unless I ask.

8. **Pre-delivery checklist** (per user memories):
   - Services/*.cs needs `using LiPi.Web.Components.Shared;` (or `LiPi.Components.*` per §25.2.2 for new code) when using its types
   - .razor needs `@using LiPi.Web.Services` (or new `LiPi.Components.*` namespaces) when injecting services
   - Never nest `@* *@` inside `@* *@`
   - `null` in Razor attr ternary → `(string?)null`
   - No `@{ }` inside `@if/@foreach`
   - Cross-razor shared types → `.cs` file
   - No base member redeclaration
   - `int` key for generic dicts
   - No duplicate keys in deploy script

9. **Component isolation** — every line of `LiPi.Components.*` code must comply with §25. No HIS-specific names, routes, services, CSS classes, or domain entities in the component library. Phase 2.10 audit blocks deployment on violations.

---

## Build order — 9 stages

Per LipiTable §27.17:

| Stage | Content | Verification gate |
|---|---|---|
| 1 | Foundation: types, services, DB migration, shared CSS, JS interop | Project compiles; migration runs; services register in DI |
| 2 | Core: LipiTable + LipiColumn + LipiEmptyState (needed early for body states) + layout shells | Minimal LipiTable renders 5 columns of static data |
| 3 | Rows/cells: row + cell sub-components | Rows render with all cell types; selection checkbox visible |
| 4 | Interactive: sort + filter + pagination (LipiPagination) + selection | Sort by clicking headers; filter via popover; paginate; select rows |
| 5 | Advanced: inline edit + tree + master-detail + grouping | All interactive demos work end-to-end |
| 6 | Polish + persistence: ITablePreferenceService + density + column ops + column picker + export | Preferences persist across reloads; export downloads correctly |
| 7 | Siblings: LipiList + LipiPagination standalone variants | Siblings render in their respective StyleGuide demos |
| 8 | Demos: 12 StyleGuide pages + shared SampleData + spec docs consolidation | Every StyleGuide demo renders |
| 9 | Tests + audit: unit tests + axe-core CI + performance benchmarks + isolation contract verification | All tests pass; axe-core clean; benchmarks within budgets; isolation audit clean |

**Per-stage delivery format:**
- All source files for the stage (in their final form, with SPEC comment headers)
- PowerShell registry snippet to append to `deploy-downloads.ps1`
- `CHANGE-LOG.md` update entry for the stage
- Short deploy checklist for me (what's included, what's modified, what to verify post-deploy)

---

## Critical flag for Stage 4 (LipiPagination)

**LP-0 — Variant naming reconciliation.** During strategic spec drafting, three sources used different variant names. Locked names: `Full` / `Compact` / `Minimal` (NOT "Standard" / "Compact" / "Mini").

Implications:
- LipiPagination enum: `PaginationVariant.Full | Compact | Minimal`
- File rename: `LipiPaginationMini.razor` → **`LipiPaginationMinimal.razor`** (update §28 deploy script entry)
- When LipiTable §8 / LipiList §7 reference variants, treat their "Standard" / "Mini" as "Full" / "Minimal"
- `LoadMore` is a MODE (PaginationMode enum), NOT a variant — `LipiPaginationLoadMore.razor` is a separate sub-component

Full reconciliation table in `03-LipiPagination-Spec.md §0`.

---

## How we'll work together

- **Don't summarize the spec back to me at the start.** I wrote it; I know what it says. Just confirm you've read the critical sections and start.

- **Drop files in `Downloads\LiPi\` with the naming convention** from §28.3 (`2.8-components-LipiTable.razor`, `2.8-types-LipiTableTypes.cs`, etc.). I'll run `deploy-downloads.ps1` to deploy.

- **One stage per delivery cycle.** Don't try to dump all 9 stages at once — verification between stages catches issues early.

- **Ask before stage transitions.** After each stage's verification, confirm with me before starting the next. I may want to adjust order based on what I see.

- **Surface deviations immediately.** If something in the spec doesn't work in practice (Blazor reality bites, EF quirks, package versions), STOP and tell me. We'll discuss in this chat OR I'll loop the strategic chat back in for an amendment.

---

## Starting prompt

After you've read the 7 attached files and the critical sections noted above, respond with:

1. Confirmation that you've read: §27 (files), §28 (deploy), §25 (isolation), §27.17 (build order), and the flags log
2. Any clarifying questions BEFORE starting Stage 1
3. Your proposed Stage 1 delivery plan (what files, in what order, what verification gate)

Then wait for my go-ahead before producing code.

Ready when you are.
