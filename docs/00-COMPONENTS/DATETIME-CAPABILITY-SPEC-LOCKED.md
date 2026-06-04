# LiPi.Components — Date/Time Capability Model (LOCKED SPEC)

> **Status:** ✅ LOCKED — ready to build. Supersedes the DRAFT.
> **PR:** Independent migration PR (own slice) — DateTime picker family → `LiPi.Components.Forms`.
> **Goal:** Redistributable, domain-free, parameter-driven date/time library. Absorb the
> *mechanics* of `IDateFormatService` + `IClinicTimezoneService` as static helpers + parameters;
> clinic/tenant policy stays in the (future) HIS, which sets the parameters.
> **CHANGE-LOG:** A54 (this migration).

---

## 0. Scope confirmed (post-investigation)

- **Base classes already package-side.** `LipiInputBase<TValue>` and `LipiContainerBase` are
  in `namespace LiPi.Components`, physically at `src\LiPi.Components\Forms`, injecting only
  package-safe deps (`IOptions<LipiInputDefaults>`, `IWebHostEnvironment`, `ILogger`, `IJSRuntime`).
  **No base-class migration needed. PR does not widen.** (Conflict C-1 resolved.)
- **Picker HIS-coupling surface is exactly:** `@using LiPi.Web.Services` (×4) +
  `@inject IDateFormatService` (Date/Time/Range) + `@inject IClinicTimezoneService` (Date/DateTime).
  Nothing else. The rest is `Microsoft.*` + `System.Globalization`.
- **Pickers move to** `src\LiPi.Components\Forms`, `namespace LiPi.Components.Forms`.

---

## 1. Foundational principle (LOCKED)

Zone-awareness is **opt-in and type-driven**. The bound value's TYPE selects the mode.
The floor requires zero zone knowledge; the ceiling is there when needed.

| Binds | Mode | Zone knowledge | Use |
|---|---|---|---|
| `DateOnly?` | Zoneless date | none | birthday, appointment date |
| `TimeOnly?` | Zoneless time | none | clinic-opens-at, reminder time |
| `DateTime?` | Zoneless datetime (NEW) | none | local wall-clock datetime |
| `DateTimeOffset?` | Zone-aware (opt-in) | yes | audit instants, cross-zone |

(Existing A4 lock preserved; `DateTime?` zoneless datetime added for the naive case.)

---

## 2. `LipiTimeSource` (LOCKED)

```csharp
namespace LiPi.Components.Forms;

public enum LipiTimeSource
{
    Server,        // .NET server wall clock (DateTime.Now/.Today) — DEFAULT, SaaS-safe
    Utc,           // DateTime.UtcNow
    Client,        // browser local clock/zone via JS bridge
    SpecificZone   // explicit TimeZone parameter (e.g. LipiTimeZones.IndiaIST)
}
```

- **Default `Server`** — provider trusts the NTP-synced server, not a client whose clock/zone
  may be wrong/mis-set. Governs Today/Tomorrow/Yesterday/Now/presets.
- `Client` reads the browser via JS (built this PR).
- `SpecificZone` + `TimeZone` (`TimeZoneInfo?`) — pin to a zone (HIS feeds clinic zone here).

**`LipiTimeZones` static helper (NEW, Indian-company batteries-included):**
```csharp
public static class LipiTimeZones
{
    public static TimeZoneInfo IndiaIST { get; }   // "Asia/Kolkata" w/ ICU fallback to +05:30 (ported from ClinicTimezoneService.ResolveIndianTimezone)
}
```

### Worked example (server IST, client PST; 2pm Mon PST = 3:30am Tue IST)
"Today" → `Server`(server in IST)=Tue · `Utc`=Mon · `Client`=Mon · `SpecificZone(IndiaIST)`=Tue.
HIS sets `SpecificZone(IndiaIST)` → all viewers see the clinic's Tue. Consistent + server-trusted.

---

## 3. Display-zone vs value-zone (LOCKED, zone-aware mode only)

- `DisplayZone` (`TimeZoneInfo?`) — show/edit a `DateTimeOffset` value in a different zone than
  its own offset. Incoming offset = the value's zone (self-describing; no separate param).
- **Return convention:** picker returns `DateTimeOffset` in `DisplayZone`'s offset. Instant preserved.
- Zoneless modes ignore this entirely.

### Worked example (point 5): value `09:00+05:30` IST, `DisplayZone=UTC` → picker shows `03:30Z`,
user edits to `04:00Z` → returns `2026-06-03T04:00:00+00:00`. App may re-display as `09:30 IST`.

---

## 4. Range picker + the BP-search question (LOCKED — resolution (a))

`LipiDateRangePicker` stays **zoneless** (two `DateOnly?`, E1 lock intact). It answers "which
two calendar dates." Turning Mon→Wed into an *instant window* (which matters because
Mon 00:00 IST ≠ Mon 00:00 UTC) is the **filter/query layer's** job, using the zone *it* chooses.
- **Implication for S3b-ii:** the table's date filter on a `DateTimeOffset` column carries a
  zone parameter and expands picked dates → `[start 00:00 tz, end 24:00 tz)`. Zone lives on the
  *filter*, not the picker. (Recorded for S3b-ii.)

---

## 5. `LipiInputDefaults` extension (LOCKED — defaults home = option (b))

Add to the existing `LipiInputDefaults` (namespace `LiPi.Components`) so apps configure
library-wide date/time policy once in `Program.cs`; per-component params override. India defaults:

```csharp
// ── Date/Time family defaults (Phase: DateTime migration / A54) ──
public string         DefaultDateFormat { get; set; } = "DD/MM/YYYY";
public string         DefaultTimeFormat { get; set; } = "24h";
public DayOfWeek      DefaultWeekStart  { get; set; } = DayOfWeek.Sunday;
public LipiTimeSource DefaultTimeSource { get; set; } = LipiTimeSource.Server;
public TimeZoneInfo?  DefaultTimeZone   { get; set; } = null;   // used when TimeSource=SpecificZone
```

Resolution in pickers: `LocalParam ?? Defaults.Value.DefaultX`.

---

## 6. Static helpers (LOCKED — drop-in replacements for the two services)

Both `internal static` in `LiPi.Components.Forms`. They contain the **verbatim** engine logic
from `DateFormatService` / `ClinicTimezoneService`, minus DI and clinic framing.

### 6.1 `LipiDateFormat` (ports DateFormatService)
Exact call surface the pickers use (counts from grep): `FormatDate` (12×), `FormatTime` (3×),
`ParseDate` (1×), `ParseTime` (1×), `GetSegmentOrder` (1×), `GetClinic{Date,Time}Format`/
`GetClinicWeekStart` (defaults — now resolved from `LipiInputDefaults`/override).
```csharp
internal static class LipiDateFormat
{
    // format always explicit (caller passes resolved format; no internal default lookup)
    public static string     FormatDate(DateOnly d, string format);
    public static DateOnly?  ParseDate(string input, string format);
    public static string     FormatTime(TimeOnly t, string format /* "12h"|"24h" */);
    public static TimeOnly?  ParseTime(string input, string format);
    public static List<string> GetSegmentOrder(string format);
    // ToDotNetFormat, TokenRegex, ISO/forgiving fallbacks — ported verbatim (private)
}
```
The three `GetClinic*` getters are **removed from the helper**; pickers resolve format/timeformat/
weekstart via `param ?? Defaults.Value.DefaultX` and pass the resolved string into the helper.
(This is the one behavioral relocation: defaults move from service to `LipiInputDefaults`.)

### 6.2 `LipiTimeResolver` (ports ClinicTimezoneService)
Exact surface: `GetClinicLocalNow` (2×) → `ResolveNow`; `ToClinicLocal` (2×) + `ToUtc` (1×) →
parameterized conversions. All driven by `TimeSource`/`TimeZone`/`DisplayZone`, not clinic ctx.
```csharp
internal static class LipiTimeResolver
{
    // "now" wall-clock per the chosen source. Client source resolved separately via JS (async).
    public static DateTime ResolveNow(LipiTimeSource source, TimeZoneInfo? zone);
    public static DateOnly ResolveToday(LipiTimeSource source, TimeZoneInfo? zone)
        => DateOnly.FromDateTime(ResolveNow(source, zone));
    // instant-preserving conversions (port ToClinicLocal/ToUtc, generalized to any zone)
    public static DateTimeOffset ToZone(DateTimeOffset value, TimeZoneInfo zone);
    public static DateTimeOffset Compose(DateOnly d, TimeOnly t, TimeZoneInfo zone); // ToUtc-style offset resolve
}
```
- `Server` → `DateTime.Now`; `Utc` → `DateTime.UtcNow`; `SpecificZone` →
  `TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone)` (the GetClinicLocalNow logic, generalized).
- `Client` → NOT here (needs browser): pickers call the JS bridge (§7) and feed the result in.

---

## 7. JS bridge for `Client` time source (LOCKED — build this PR)

`lipi-input.js` (existing picker JS) gains:
```js
window.lipiInput.getClientNow = function () {
    const d = new Date();
    return { iso: d.toISOString(), tzOffsetMin: d.getTimezoneOffset(),
             y: d.getFullYear(), mo: d.getMonth()+1, da: d.getDate(),
             h: d.getHours(), mi: d.getMinutes() };
};
```
When `TimeSource=Client`, the picker's Now/Today calls this (async, in event handler — fine, not
an ElementReference) and builds the local `DateTime`/`DateOnly` from the returned parts.

---

## 8. Per-component parameters added (all default-via-LipiInputDefaults or null)

All four pickers gain (where meaningful to their type):
`TimeSource` (LipiTimeSource?), `TimeZone` (TimeZoneInfo?), and DateTimePicker additionally
`DisplayZone` (TimeZoneInfo?). Existing params unchanged: `Format`/`TimeFormat`, `InputMode`,
`MinDate`/`MaxDate`/`IsDateAllowed`/`IsDateDisabled`/`GetDisabledReason`, `Step`, `Presets`,
`AllowOpenEnd`, layout, etc. Naive `<LipiDatePicker @bind-Value="d"/>` touches none of the new ones.

---

## 9. Migration mechanics (LOCKED)

Per picker: change `namespace`→`LiPi.Components.Forms`; drop `@using LiPi.Web.Services`; remove
the two `@inject`s; replace `DateFmt.X(...)`/`ClinicTz.X(...)` calls with `LipiDateFormat.X(...)`/
`LipiTimeResolver.X(...)` + resolved-default params; add the new params. `LipiDateTimeTypes.cs`
→ `LiPi.Components.Forms` (presets rework to resolve "today" via `ResolveToday(TimeSource,zone)`).
CSS: namespace header note only (rules already token-compliant). Update `deploy-downloads.ps1`
paths + any `LiPi.Web` demo `@using`. Old `IDateFormatService`/`IClinicTimezoneService`/impls:
**leave in `LiPi.Web` for now** (the future HIS may keep thin clinic-config wrappers that set
`LipiInputDefaults` from clinic context) — they're no longer referenced by the pickers; flag for
later cleanup. Do NOT delete this PR (out of scope; no consumers to break, but tidy separately).

---

## 10. Build order

1. `LipiInputDefaults` += 5 date/time defaults.
2. `LipiTimeZones` (IndiaIST + ICU fallback) + `LipiTimeResolver` + `LipiDateFormat` static helpers.
3. `lipi-input.js` += `getClientNow`.
4. Migrate `LipiDatePicker` (namespace, helpers, params) + its CSS header.
5. Migrate `LipiTimePicker`.
6. Migrate `LipiDateTimePicker` (+ `DisplayZone`, zone-aware + new zoneless `DateTime?` mode).
7. Migrate `LipiDateRangePicker` (+ presets via `ResolveToday`).
8. `LipiDateTimeTypes.cs` → Forms namespace.
9. `deploy-downloads.ps1` + `LiPi.Web` demo references.
10. CHANGE-LOG A54; token-contract audit; verification gate.

## 11. Verification gate
Zero-config `<LipiDatePicker @bind-Value/>` works (DD/MM/YYYY, Sun, Today=server). `TimeSource`
Server/Utc/Client/SpecificZone(IndiaIST) resolve Today per §2 table. `DisplayZone` IST↔UTC per §3.
Range presets honor source. No HIS injects remain in the pickers. Build clean.
