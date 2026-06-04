# DateTime: Existing Spec (01.5) vs New Capability Model — Feature Comparison

Comparing the **locked** `docs/00-COMPONENTS/01.5-DateTime.md` (the prior design, built for
`LiPi.Web` with HIS services) against the **new** `DATETIME-CAPABILITY-SPEC.md` (the
redistributable, parameter-driven `LiPi.Components` model). Goal: keep every good locked
feature, replace the HIS-service coupling with parameters, and add the new capabilities.

Legend: ✅ keep as-is · 🔄 keep but re-source (service→parameter) · ➕ new in capability model · ⚠️ conflict to resolve

| # | Feature | Existing 01.5 spec | New capability model | Disposition |
|---|---------|--------------------|----------------------|-------------|
| **Architecture** |
| 1 | Four components | DatePicker, TimePicker, DateTimePicker, DateRangePicker | Same four | ✅ keep |
| 2 | Namespace | `LiPi.Web.Components.Shared` | `LiPi.Components.Forms` | 🔄 migrate |
| 3 | Class hierarchy | 3 inherit `LipiInputBase<T>`; RangePicker is direct `ComponentBase` (dual-value) | Same hierarchy (must port `LipiInputBase` into package or its generic equiv) | ⚠️ verify `LipiInputBase` is package-safe / migrate it too |
| 4 | DI services | `IDateFormatService` + `IClinicTimezoneService` (scoped) | **Removed.** Logic → static helpers + parameters | 🔄 the core of this PR |
| **Binding types (A4 lock)** |
| 5 | DatePicker binds | `DateOnly?` | `DateOnly?` (zoneless) | ✅ keep |
| 6 | TimePicker binds | `TimeOnly?` (clinic-local) | `TimeOnly?` (zoneless wall-clock) | ✅ keep (interpretation now consumer's) |
| 7 | DateTimePicker binds | `DateTimeOffset?` (UTC instant+offset) | `DateTimeOffset?` (zone-aware) **+ optional `DateTime?` zoneless mode** | 🔄 + ➕ add zoneless datetime mode |
| 8 | RangePicker binds | two `DateOnly?` (E1 lock, zoneless) | two `DateOnly?` (zoneless) — zone applied by *consumer/query* | ⚠️ see #28 (BP-search) |
| 9 | Type-aware mode selection | implicit (each component = one type) | **explicit principle**: bound type IS the mode selector (§1) | ➕ formalized |
| **Time sourcing (the new core)** |
| 10 | "Now"/"Today" source | `IClinicTimezoneService.GetClinicLocalNow()` — clinic zone, hardcoded IST | `LipiTimeSource { Server, Utc, Client, SpecificZone }` parameter | ➕ developer-selectable (replaces clinic-implicit) |
| 11 | Server-time trust (SaaS) | not addressed | `Server` source — provider trusts NTP-synced server, not client clock | ➕ new (your point 1) |
| 12 | Client-time | not available | `Client` source — JS bridge reads browser zone/clock | ➕ new (JS bridge this PR) |
| 13 | Pinned zone | clinic IST hardcoded in service | `SpecificZone` + `TimeZone` param; **`LipiTimeZones.IndiaIST` built-in helper** | 🔄 + ➕ IST shortcut (Indian co. 🇮🇳) |
| 14 | ICU fallback for IST | in `ClinicTimezoneService` (FindSystemTimeZoneById → fixed +05:30) | ported into the `LipiTimeZones.IndiaIST` helper verbatim | 🔄 keep the robust fallback |
| **Zone conversion** |
| 15 | Display-zone vs value-zone | not available (always clinic-local display) | `DisplayZone` param — show/edit in a different zone than the value (§4) | ➕ new (your point 5) |
| 16 | UTC↔local conversion | `ToClinicLocal` / `ToUtc` in service | static instant-preserving zone helpers, driven by `DisplayZone`/`TimeSource` | 🔄 re-source |
| 17 | DateOnly+TimeOnly→DateTimeOffset composition | via `IClinicTimezoneService` offset | via `TimeSource`/`TimeZone` (no clinic) | 🔄 re-source |
| **Format & calendar** |
| 18 | Date format library | `IDateFormatService` (12 combos, D/DD/M/MM/MMM/MMMM/YY/YYYY, sep `/-.` space) | static `LipiDateFormat` helper, same engine; `DateFormat` param | 🔄 port verbatim |
| 19 | `ToDotNetFormat` token translation (Y→y, DD sentinel) | in service | in static helper | 🔄 port verbatim |
| 20 | Parse w/ ISO + forgiving fallback | in service | in static helper | 🔄 port verbatim |
| 21 | Time format 12h/24h (+cross-format parse) | in service, default 24h | `TimeFormat` param, default `24h` | 🔄 re-source |
| 22 | Week start | `GetClinicWeekStart()`, default Sunday | `WeekStart` param, default **Sunday** (confirmed) | 🔄 re-source |
| 23 | Segment order (InputMode=Segments) | `GetSegmentOrder()` regex token walk, word-month→numeric | static helper, same logic | 🔄 port verbatim |
| 24 | Default format | DD/MM/YYYY (India) | DD/MM/YYYY (India — confirmed) | ✅ keep |
| **Component features (keep all)** |
| 25 | InputMode Field/Segments (DatePicker) | locked (F3) | keep | ✅ |
| 26 | Date constraints: MinDate/MaxDate/IsDateAllowed/IsDateDisabled/GetDisabledReason (+"Date unavailable" fallback, A6) | locked | keep | ✅ |
| 27 | Calendar popover (position:fixed, A2.2), keyboard nav (B5 WAI-ARIA), mobile modal <640px | locked | keep | ✅ |
| 28 | TimePicker Step + asymmetric validation (C2: user input rejects, Now snaps) | locked | keep | ✅ |
| 29 | TimePicker Now button | reads clinic-zone now | reads `TimeSource`-resolved now | 🔄 re-source |
| 30 | DateTimePicker composition (D1: composes Date+Time, not unified popover) + coordination (D2) + Stacked/Inline layout | locked | keep | ✅ |
| 31 | RangePicker dual binding (E1), two-calendar popover, range viz (E3), presets (E2, 12 + 2 bundles), open-ended (E4) | locked | keep; presets resolve via `TimeSource` (lazy, honors source) | ✅ + 🔄 presets honor source |
| 32 | ISO storage, configurable display (A1) | locked (storage is consumer/DB concern) | unchanged — package doesn't store; consumer maps to DB | ✅ keep (note: storage now fully consumer's job) |
| 33 | Time provenance (_source / triple-column) | integration-layer concern, not enforced | unchanged — still consumer/integration concern | ✅ keep note |
| 34 | DST handling | deferred to v1.1 (India no DST) | same deferral; `SpecificZone`/`DisplayZone` for non-DST zones safe | ✅ keep deferral |
| 35 | LipiDobPicker, IME, smart paste | deferred (Phase 4.2 / later) | same | ✅ keep deferral |
| **New in capability model** |
| 36 | Zoneless `DateTime?` datetime mode | — | ➕ for "local datetime, no offset" naive case |
| 37 | Progressive-disclosure principle (zero zone knowledge at floor) | implicit | ➕ explicit design law |
| 38 | General-purpose (non-HIS) usability | HIS-targeted | ➕ explicit goal (your point 6) |

---

## Conflicts to resolve (the ⚠️ rows)

**C-1 (#3) — `LipiInputBase<T>`.** Three pickers inherit it; it lives in `LiPi.Web`. To
migrate the pickers, `LipiInputBase` (and its dependencies: touched-state, EditContext
subscription, the family visual cascade, `LipiContainerBase`?) must either move into
`LiPi.Components` too, or already be package-side. **Need to check where `LipiInputBase`
lives and what it pulls in.** This could widen the PR (it may drag the whole input base
family into the package). *Decision needed: is `LipiInputBase` already package-safe, or
does this PR also migrate the input-base layer?*

**C-2 (#8, #28) — RangePicker + the BP-search timezone question.** Your example: "search
patient BP Monday→Wednesday." A BP reading is a timestamped instant (`DateTimeOffset`).
"Mon→Wed" must become an instant window, and **the zone defining the window's boundaries
matters** (Mon 00:00 IST ≠ Mon 00:00 UTC). The existing spec locked RangePicker as zoneless
`DateOnly` (E1). Two resolutions:
- **(a) Range picker stays zoneless** (picks two calendar dates); the **query/filter layer**
  applies the zone to expand `[Mon 00:00 tz, Wed 24:00 tz)`. Picker = "which dates"; filter
  = "which instants." Keeps E1 intact, picker reusable. *(spec lean)*
- **(b) Range picker becomes zone-aware** (knows the zone, emits an instant range).
*Decision needed.* Note: this directly shapes **S3b-ii** — the table's date filter on a
`DateTimeOffset` column will need a zone parameter to convert picked dates → instant ranges,
regardless of (a)/(b). Under (a), that zone lives on the *filter*; under (b), on the *picker*.

---

## §10 decisions — status after your answers

1. IST built-in → **YES** (`LipiTimeZones.IndiaIST`). 🇮🇳
2. `ServerLocal` name confusing → rename to **`Server`** (vs Utc/Client/SpecificZone). *(confirm)*
3. WeekStart **Sunday** → ✅ confirmed.
4. India format defaults → ✅ confirmed.
5. `Client` JS bridge now → ✅ confirmed.
6. Display/value zone example → provided (IST-stored, UTC-displayed BP edit).
7. RangePicker + BP timezone → **C-2 above, decision needed (a vs b)**.
8. This comparison table → done.

**Still to lock:** enum name `Server` (vs ServerLocal), C-1 (LipiInputBase scope), C-2 (range
zone a/b), zone-aware return convention (§10.6), spec filename + CHANGE-LOG number (A54).
