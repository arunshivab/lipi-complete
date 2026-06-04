# Phase 2.8 — Data Display — Overview

**Status:** OUTLINE
**Phase:** 2.8 — Data Display family
**Components in this phase:**
- LipiTable (large — 28 sections, see `01-LipiTable-Spec.md`)
- LipiList (medium — 20 sections, see `02-LipiList-Spec.md`)
- LipiPagination (small — 11 sections, see `03-LipiPagination-Spec.md`)
- LipiEmptyState (small — 9 sections, see `04-LipiEmptyState-Spec.md`)

**Shared infrastructure built in this phase:**
- Shared status tokens CSS (`lipi-status-tokens.css`) — used by LipiTable, LipiList, LipiCard, LipiAlert, future components
- Shared status string-constants class (`LipiStatus.cs`) — domain-neutral taxonomy
- Table preference service (`ITablePreferenceService` + default implementation) — persistence backend
- Database migration for user table preferences

---

## §1 — Phase scope and posture

### §1.1 — What this phase delivers
Four components covering the "show me a list of things" use case across the entire LiPi product surface. The four split by use case:

| Component | Use case | Example |
|---|---|---|
| LipiTable | Tabular data with multiple columns per row | User list, patient list, billing line items, lab worklists |
| LipiList | Vertical stack of templated rows (no column structure) | Notification feed, activity log, search results, recently viewed |
| LipiPagination | Page navigation control | Composed into LipiTable; also usable standalone |
| LipiEmptyState | Icon + title + body + CTA pattern for empty / zero / not-found surfaces | Empty table, no search results, "create your first X" |

### §1.2 — Component isolation contract (reaffirmed)
All four components are domain-neutral. No clinical / HIS / patient / billing types. Caller integrates domain by providing templates, format selectors, event handlers, and status taxonomies. Per the standing rule that the LiPi component library must be redistributable for any Blazor project.

### §1.3 — Zero external functional-library posture (reaffirmed)
LiPi's project posture is to **build in-house** for any functional surface that's bounded and replaceable, rather than take an external library dependency. External libraries are reserved for infrastructure that's effectively unique to the platform (ASP.NET Core, EF Core, Npgsql, etc.).

This phase honors that posture:
- **No ClosedXML** (would have been Excel export library) — Excel export deferred entirely (see §3)
- **No QuestPDF** or other external PDF library — PDF export uses the in-house PDF library (in development, expected ready by Phase 2.10)
- **CSV** — trivial string-builder, written inline in LipiTable's export service
- **Print** — uses the browser's native print API + a print stylesheet, no library needed

This phase introduces zero new NuGet packages.

### §1.4 — Build sequence (within Phase 2.8)
1. Shared status tokens (`lipi-status-tokens.css`) + `LipiStatus.cs` (foundation)
2. LipiEmptyState (used by LipiTable and LipiList)
3. LipiPagination (used by LipiTable)
4. LipiList
5. LipiTable (largest, depends on the three above)
6. DB migration for user_table_preferences
7. Default TablePreferenceService implementation
8. StyleGuide showcases for all four

---

## §2 — Shared infrastructure

### §2.1 — Shared status tokens (Option C from strategic chat)
Locked decision: `LipiStatusStrip` is NOT a standalone component. The status taxonomy and colors are shared infrastructure used by anything that needs to render a status visually.

**Files created in this phase:**
- `wwwroot/css/lipi-status-tokens.css` — `--color-status-*` tokens + `.lipi-status-strip-{left,right,top}` utility classes
- `Components/Shared/LipiStatus.cs` — string-constants class + `StatusColors` static map

**Consumers** (existing and future):
- LipiTable (this phase)
- LipiList (this phase)
- LipiCard (Phase 2.6.1 — retrofit during Phase 2.10 audit)
- LipiAlert (Phase 2.6.1 — retrofit during Phase 2.10 audit)
- LipiBadge (Phase 2.7 — uses same tokens via composition)
- LipiPill (Phase 2.7)
- Any future component rendering status

### §2.2 — Status string-constants class design
Generic, domain-neutral. Status values are strings (not C# enum at LipiTable API level), so consuming apps can define their own taxonomies without modifying LiPi.

```csharp
namespace LiPi.Web.Components.Shared;

// Reference taxonomy — opaque strings at consumption points.
// Apps can use these standard values or define their own.
public static class LipiStatus
{
    public const string Active        = "active";
    public const string Pending       = "pending";
    public const string Inactive      = "inactive";
    public const string Suspended     = "suspended";
    public const string Locked        = "locked";
    public const string Archived      = "archived";
    public const string Draft         = "draft";
    public const string Published     = "published";
    public const string InProgress    = "in-progress";
    public const string Completed     = "completed";
    public const string Failed        = "failed";
    public const string Cancelled     = "cancelled";
    public const string Warning       = "warning";
    public const string Error         = "error";
    public const string Info          = "info";
    public const string Success       = "success";
}
```

The CSS file defines tokens for each known status (`--color-status-active`, etc.) AND a generic fallback. Unknown status strings fall back to a neutral grey.

### §2.3 — Status token CSS structure
```css
/* lipi-status-tokens.css */
:root {
    --color-status-active:        var(--color-success-600, #10B981);
    --color-status-pending:       var(--color-warning-500, #F59E0B);
    --color-status-locked:        var(--color-danger-500,  #EF4444);
    --color-status-suspended:     var(--color-warning-600, #D97706);
    /* ... etc ... */
    --color-status-unknown:       var(--color-neutral-400, #94A3B8);

    --lipi-status-strip-width:    4px;
}

/* Mode-aware overrides handled by mode-light.css / mode-dark.css */

.lipi-status-strip-left {
    border-left: var(--lipi-status-strip-width) solid var(--color-status-unknown);
}

/* Map data-status to color */
[data-status="active"]    { border-left-color: var(--color-status-active);    }
[data-status="pending"]   { border-left-color: var(--color-status-pending);   }
/* ... etc ... */
```

### §2.4 — User table preference service
LipiTable persistence depends on `ITablePreferenceService`. Defined in this phase, used by LipiTable §21.

**Interface** (lives in `LiPi.Web.Components.Shared`):
```csharp
public interface ITablePreferenceService
{
    Task<TablePreferences?> GetAsync(string tableId, CancellationToken ct = default);
    Task SaveAsync(string tableId, TablePreferences prefs, CancellationToken ct = default);
    Task ResetAsync(string tableId, CancellationToken ct = default);
}
```

**Default implementation** (lives in `LiPi.Web.Services`, EF-based against identity DB):
- Reads / writes `user_table_preferences` table
- Per-user, per-table-id JSON storage
- Cached in scoped service for the current circuit
- Debounced writes (300ms)

### §2.5 — Database migration
New table `identity.user_table_preferences`:
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

Migration files:
- `2026-XX-XX-phase-2.8-user-table-prefs-up.sql`
- `2026-XX-XX-phase-2.8-user-table-prefs-down.sql`

---

## §3 — Export strategy

Phase 2.8 ships export in three formats. **No external libraries used.**

| Format | Phase 2.8 implementation | Notes |
|---|---|---|
| **CSV** | In-house string-builder (single ~150-line `CsvExporter.cs`) | Trivial format; RFC 4180 compliant; UTF-8 BOM optional via parameter |
| **PDF** | Integrates with the in-house LiPi PDF library | LiPi PDF library expected ready by Phase 2.10. Until then, the PDF export pathway is stubbed in the spec but throws "PDF library pending Phase 2.10 integration" if invoked. StyleGuide demo will be skipped for PDF until the library lands. |
| **Print** | Browser native print API + dedicated print stylesheet | No library. CSS `@media print` rules in `lipi-table-print.css` |

**Excel (.xlsx) is deferred to a later phase.** When users need Excel today, CSV is the bridge — every spreadsheet app opens CSV. A future in-house Excel writer (or a focused mini-writer) can be added as a separate amendment when the consuming pages demand it. Until then, LipiTable's export UI does not show "Excel" as an option.

**LipiList export** follows the same plan: CSV + PDF + Print, no Excel for v1.0.

### §3.1 — Future Excel option (deferred)
- Could be built as part of the in-house library suite (after the PDF library proves the pattern)
- Could be a 200–400-line focused .xlsx writer that handles the tabular cases LiPi tables need (headers, cells, basic types, column widths) — no full Excel feature parity
- Both options keep LiPi at zero external dependencies for export

This is queued in `03-DEFERRED-ITEMS.md` under "Future infrastructure phases" but not on the Phase 2.10 must-do list.

---

## §4 — Phase 2.10 audit additions

The following items are added to the Phase 2.10 Infrastructure Audit checklist as a result of Phase 2.8:

1. **Status taxonomy retrofit** — LipiCard / LipiAlert update to use `lipi-status-tokens.css` (currently use inline colors per their Phase 2.6.1 build)
2. **ITablePreferenceService scope review** — confirm multi-tenant implications (does same TableId mean same prefs across clinics for global users?); GDPR / DPDP review
3. **HIPAA audit log scope for preferences** — table-preference writes are not PHI but represent user behavior; confirm if `phi_access_log` or `audit_events` should capture them
4. **In-house PDF library integration confirmation** — once the LiPi PDF library ships, validate API stability, integrate into LipiTable + LipiList export paths, remove the "pending Phase 2.10" stubs, enable StyleGuide PDF demos
5. **Standing rule formalization** — add "no external functional-library" rule to `02-STANDING-RULES.md` (see §6 of this overview)

---

## §5 — Cross-component composition map

How the four components compose with each other and with existing components:

```
LipiTable
├── composes LipiPagination (footer)
├── composes LipiEmptyState (empty / zero-state slot)
├── composes LipiSkeleton  [Phase 2.7]   (loading state)
├── composes LipiValidationSummary [Phase 2.7] (row validation)
├── composes LipiToast [Phase 2.7]  (save success / failure)
├── composes LipiSpinner [Phase 2.7]  (server-side loading)
├── composes LipiBadge / LipiPill [Phase 2.7]  (status cells)
├── composes LipiButton [Phase 2.1]   (toolbar, actions)
├── composes LipiTextBox / LipiNumberInput / LipiDatePicker / LipiSelect [Phase 2.2]  (edit inputs)
├── composes LipiCheckbox [Phase 2.5]   (selection)
├── composes LipiModal / LipiDrawer [Phase 2.6.2]  (filter drawer, export modal, conflict modal)
├── integrates LiPi PDF library [Phase 2.10]  (PDF export, stubbed until library lands)
└── shares lipi-status-tokens.css with LipiCard, LipiAlert, LipiBadge, LipiPill

LipiList
├── composes LipiPagination (optional footer)
├── composes LipiEmptyState (empty slot)
├── composes LipiSkeleton (loading state)
├── integrates LiPi PDF library [Phase 2.10]  (PDF export, stubbed)
└── shares lipi-status-tokens.css

LipiPagination
└── composes LipiButton (page-number buttons)
   composes LipiSelect (page-size dropdown)

LipiEmptyState
└── composes LipiButton (CTA button)
```

---

## §6 — Proposed standing-rules addition

The zero-external-functional-library posture is implicit in `04-OPERATIONAL-RULES.md §5` (LipiMail / LipiHttp / LipiApi infrastructure phases) and visible in `03-DEFERRED-ITEMS.md` (MailKit and JWT Bearer queued for review). It is not yet stated as a hard standing rule.

**Proposed new rule for `02-STANDING-RULES.md`:**

```markdown
## Library dependency posture (MANDATORY)

LiPi builds in-house for any functional surface that is bounded and replaceable.
External libraries are reserved for infrastructure that is effectively unique
to the platform.

### Allowed (infrastructure)
- ASP.NET Core, EF Core, Npgsql, runtime BCL
- Microsoft.AspNetCore.* (Identity, Authentication, Authorization, Components.Web)
- (Anything required to run on .NET 10 + PostgreSQL)

### Build in-house (functional)
- PDF generation, Excel/spreadsheet writers, file format encoders/decoders
- Email service abstractions, HTTP/API client wrappers
- Charting, diagramming, visualization
- Custom UI primitives (the entire `Components/Shared/Lipi*` family)
- Cryptography helpers where BCL is sufficient

### Currently under review (Phase 2.10 audit queue)
External functional libraries pre-existing in `LiPi.Web.csproj` and queued for
in-house replacement:
- `MailKit` (SMTP) → LipiMail service
- `Microsoft.AspNetCore.Authentication.JwtBearer` → confirm scope or remove
- `SharpZipLib` (ZIP password support) → consider in-house when next Aadhaar
  module touches the code

### When in doubt
- Strategic chat raises the option, presents standard external libraries with
  trade-offs, and confirms with the user before any new dependency is added
- Default answer is "build in-house" unless the user explicitly approves the
  external dependency
- Every approved external library is logged in `03-DEFERRED-ITEMS.md` for
  Phase 2.10 review
```

**Action:** Add the above to `02-STANDING-RULES.md` as a new section. Strategic chat to deliver the standing-rule update file as part of Phase 2.8 housekeeping.

---

## §7 — Files created by Phase 2.8

### Shared infrastructure
- `Components/Shared/LipiStatus.cs`
- `Components/Shared/ITablePreferenceService.cs`
- `Components/Shared/LipiTablePreferences.cs`
- `Services/TablePreferenceService.cs`
- `Database/Entities/UserTablePreference.cs`
- `wwwroot/css/lipi-status-tokens.css`
- Migration: `database/migrations/2026-XX-XX-phase-2.8-user-table-prefs-up.sql`
- Migration: `database/migrations/2026-XX-XX-phase-2.8-user-table-prefs-down.sql`

### LipiTable (see `01-LipiTable-Spec.md` §27 for full list)
Approximately 15 .razor / .razor.cs / .razor.css files plus interop JS plus CsvExporter.

### LipiList (see `02-LipiList-Spec.md` §19)
- `Components/Shared/LipiList.razor`
- `Components/Shared/LipiList.razor.css`
- `Components/Shared/LipiListTypes.cs`
- `wwwroot/css/lipi-list.css`

### LipiPagination (see `03-LipiPagination-Spec.md` §9)
- `Components/Shared/LipiPagination.razor`
- `Components/Shared/LipiPagination.razor.css`
- `Components/Shared/LipiPaginationTypes.cs`
- `wwwroot/css/lipi-pagination.css`

### LipiEmptyState (see `04-LipiEmptyState-Spec.md` §7)
- `Components/Shared/LipiEmptyState.razor`
- `Components/Shared/LipiEmptyState.razor.css`
- `Components/Shared/LipiEmptyStateTypes.cs`
- `wwwroot/css/lipi-empty-state.css`

### Export support
- `Components/Shared/Export/CsvExporter.cs` (in-house, ~150 lines)
- `wwwroot/css/lipi-table-print.css` (print stylesheet for browser-native Print)
- PDF: integration stub (real integration deferred to Phase 2.10 when LiPi PDF library ships)

### App.razor updates
- Cache version stamps bumped
- New CSS files added to head
- New JS files added before closing body

### Program.cs updates
- `ITablePreferenceService` registered as scoped

### `02-STANDING-RULES.md` update
- New section: "Library dependency posture" (per §6 of this overview)

### `03-DEFERRED-ITEMS.md` update
- Add Phase 2.10 items per §4 of this overview
- Add "Excel export — in-house writer" to "Future infrastructure phases"
- Remove any stale references to ClosedXML or QuestPDF (none exist currently, but verify)

### Deploy script
- All new files mapped in `deploy-downloads.ps1`
- Phase 2.8 header annotated

---

## §8 — NuGet impact: zero

Phase 2.8 ships **no new NuGet packages**. The phase honors the zero-external-functional-library posture in full. The `LiPi.Web.csproj` file is unchanged by this phase except for project references if any new internal projects are added (none currently planned).

---

## §9 — StyleGuide additions across Phase 2.8

New top-level section `#data-display` containing sub-sections for each component. See each spec's StyleGuide section for full demos.

PDF export demos in StyleGuide are marked "Available after Phase 2.10 — LiPi PDF library integration" until the library lands. CSV and Print demos work from day one.

---

## §10 — Acceptance criteria (phase-level)

Phase 2.8 ships when:
- [ ] All four component spec docs frozen
- [ ] Build chat delivers all four components
- [ ] Shared status tokens CSS reviewed and merged
- [ ] DB migration applied to dev clinic
- [ ] TablePreferenceService default implementation tested
- [ ] StyleGuide section #data-display renders all demos (PDF demos marked pending)
- [ ] Light + dark mode verified for each component
- [ ] CSV + Print export tested end-to-end
- [ ] PDF export pathway stubbed and ready for Phase 2.10 wiring
- [ ] No new NuGet packages added (verified by csproj diff)
- [ ] `02-STANDING-RULES.md` updated with library-dependency-posture rule
- [ ] `03-DEFERRED-ITEMS.md` updated with Phase 2.10 audit items + Excel writer in future-phases
- [ ] CHANGE-LOG.md updated with amendments
- [ ] No regressions in Phase 2.6.x or 2.7 components
- [ ] Deploy script complete

---

*See per-component spec files for full details.*
