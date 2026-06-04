// SPEC:    docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d)
// PHASE:   2 Sub-step 2.4 — Date/Time component family (Batch 9d)
// AMEND:   docs/CHANGE-LOG.md A20 (pending)
//
// LipiDateTimeTypes.cs — Public types shared across the Phase 2.4 Date/Time
// component family. Includes:
//   - DatePickerInputMode enum     (Field | Segments) — LipiDatePicker
//   - DateTimeLayout enum          (Stacked | Inline) — LipiDateTimePicker
//   - DateRangePreset record       — caller-defined preset shape
//   - LipiDateRangePresets static  — 12 individual presets + 2 starter bundles
//
// Why a single file: types are small, related, and used together in caller
// markup. One file means one @using import for callers. Mirrors the
// LipiTextInputTypes.cs pattern from Phase 2.2 / 2.2.5.

using System;
using System.Collections.Generic;

namespace LiPi.Components.Forms;

// =============================================================================
// LipiDatePicker input mode
// =============================================================================

/// <summary>Controls whether <c>LipiDatePicker</c> renders as a single text field
/// or as compound segments (DD MM YYYY etc.).</summary>
public enum DatePickerInputMode
{
    /// <summary>Single text input — user types DD/MM/YYYY directly OR clicks the
    /// calendar icon to open the popover. Default.</summary>
    Field,

    /// <summary>Compound segments — separate inputs for day / month / year per the
    /// clinic's date format. Calendar icon also present for popover access.
    /// Segment order driven by <see cref="LiPi.Components.Forms.LipiDateFormat.GetSegmentOrder"/>.</summary>
    Segments
}

// =============================================================================
// LipiDateTimePicker layout
// =============================================================================

/// <summary>Controls how <c>LipiDateTimePicker</c> arranges its date and time
/// sub-controls.</summary>
public enum DateTimeLayout
{
    /// <summary>Date control on top, time control below. Default. Matches dense-form
    /// layouts where horizontal space is constrained. Mobile auto-collapses to
    /// stacked at &lt;640px regardless of this parameter.</summary>
    Stacked,

    /// <summary>Date control on the left, time control on the right.
    /// Best for wide forms where the two are naturally read together.
    /// Auto-collapses to <see cref="Stacked"/> at &lt;640px viewport via CSS.</summary>
    Inline
}

// =============================================================================
// DateRangePreset — caller-defined preset for LipiDateRangePicker
// =============================================================================

/// <summary>A reusable preset for <c>LipiDateRangePicker</c>'s
/// preset panel. Each preset has a label shown in the panel and a resolver
/// function that returns the (Start, End) tuple for the current moment.
///
/// Resolvers are evaluated lazily each time the preset panel opens — this means
/// "Today" / "Yesterday" / etc. always reflect the current calendar day rather
/// than the moment the preset was constructed.</summary>
/// <param name="Label">Display text shown in the preset panel.</param>
/// <param name="Resolver">Function that returns the (Start, End) tuple for "now".</param>
public sealed record DateRangePreset(
    string Label,
    Func<(DateOnly Start, DateOnly End)> Resolver);

// =============================================================================
// LipiDateRangePresets — built-in presets + starter bundles
// =============================================================================

/// <summary>Static library of common <see cref="DateRangePreset"/>s plus two
/// starter bundles (<see cref="LipiDateRangePresets.CommonReports"/>, <see cref="LipiDateRangePresets.CommonScheduling"/>)
/// for documentation and demo purposes.
///
/// Real-world bundles emerge from module needs in Phase 4.x; the starter bundles
/// here are intentionally minimal — they show how to compose presets, not what
/// the production catalog should be.
///
/// All presets use the clinic's local <see cref="DateOnly"/> (i.e., today as the
/// clinic sees it). The current implementation uses <c>DateOnly.FromDateTime(DateTime.Today)</c>
/// — when <c>IClinicTimezoneService</c> is widely adopted, presets will resolve
/// "today" via clinic context.
///
/// JUDGMENT: presets are lazy <c>Func</c>s — evaluated each open of the preset
/// panel — so "Today" always reflects the current day, not the moment the
/// LipiDateRangePicker was constructed. Static cached presets would freeze the
/// values at app startup, which is wrong for a Now-relative UI.</summary>
/// <summary>Which temporal direction of presets to surface (Stage: DateTime migration A54).</summary>
public enum PresetDirection { Past, Future, Both }

/// <summary>Cumulative span tier of presets. Each tier includes all tiers above it:
/// Day &lt; Week &lt; Month &lt; Quarter &lt; Year.</summary>
public enum PresetSpan { Day, Week, Month, Quarter, Year }

public static class LipiDateRangePresets
{
    // -------------------------------------------------------------------------
    // PAST presets (most common in reports / audit / look-back queries)
    // -------------------------------------------------------------------------

    public static readonly DateRangePreset Today = new(
        "Today",
        () =>
        {
            var t = DateOnly.FromDateTime(DateTime.Today);
            return (t, t);
        });

    public static readonly DateRangePreset Yesterday = new(
        "Yesterday",
        () =>
        {
            var y = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            return (y, y);
        });

    public static readonly DateRangePreset Last7Days = new(
        "Last 7 days",
        () =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return (today.AddDays(-6), today);  // inclusive 7-day window
        });

    public static readonly DateRangePreset Last30Days = new(
        "Last 30 days",
        () =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return (today.AddDays(-29), today);
        });

    public static readonly DateRangePreset ThisMonth = new(
        "This month",
        () =>
        {
            var today = DateTime.Today;
            var start = new DateOnly(today.Year, today.Month, 1);
            var end = DateOnly.FromDateTime(today);
            return (start, end);
        });

    public static readonly DateRangePreset LastMonth = new(
        "Last month",
        () =>
        {
            var today = DateTime.Today;
            var firstOfThis = new DateTime(today.Year, today.Month, 1);
            var lastOfPrev = firstOfThis.AddDays(-1);
            var firstOfPrev = new DateTime(lastOfPrev.Year, lastOfPrev.Month, 1);
            return (DateOnly.FromDateTime(firstOfPrev), DateOnly.FromDateTime(lastOfPrev));
        });

    public static readonly DateRangePreset ThisYear = new(
        "This year",
        () =>
        {
            var today = DateTime.Today;
            var start = new DateOnly(today.Year, 1, 1);
            var end = DateOnly.FromDateTime(today);
            return (start, end);
        });

    // -------------------------------------------------------------------------
    // FUTURE presets (most common in scheduling / appointment planning)
    // -------------------------------------------------------------------------

    public static readonly DateRangePreset Tomorrow = new(
        "Tomorrow",
        () =>
        {
            var t = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            return (t, t);
        });

    public static readonly DateRangePreset Next7Days = new(
        "Next 7 days",
        () =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return (today, today.AddDays(6));
        });

    public static readonly DateRangePreset NextWeek = new(
        "Next week",
        () =>
        {
            // "Next week" = Mon-Sun of the upcoming week.
            // JUDGMENT: using ISO week semantics (Mon-Sun) since that's the
            // most common scheduling-context "week" interpretation, NOT the
            // clinic's configurable week start. If clinics complain, expose
            // via a parameter; until then, ISO is the safe default for
            // scheduling preset semantics.
            var today = DateTime.Today;
            var daysUntilMon = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilMon == 0) daysUntilMon = 7;  // if today IS Monday, "next" Monday is +7
            var nextMon = today.AddDays(daysUntilMon);
            var nextSun = nextMon.AddDays(6);
            return (DateOnly.FromDateTime(nextMon), DateOnly.FromDateTime(nextSun));
        });

    public static readonly DateRangePreset NextMonth = new(
        "Next month",
        () =>
        {
            var today = DateTime.Today;
            var firstOfThis = new DateTime(today.Year, today.Month, 1);
            var firstOfNext = firstOfThis.AddMonths(1);
            var lastOfNext = firstOfNext.AddMonths(1).AddDays(-1);
            return (DateOnly.FromDateTime(firstOfNext), DateOnly.FromDateTime(lastOfNext));
        });

    public static readonly DateRangePreset Next30Days = new(
        "Next 30 days",
        () =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return (today, today.AddDays(29));
        });

    // -------------------------------------------------------------------------
    // STARTER BUNDLES — examples only. Real bundles emerge from module needs.
    //
    // Don't add more bundles here. Modules (Phase 4.x) define their own.
    // -------------------------------------------------------------------------

    /// <summary>Common preset bundle for past-window report builders. Example only —
    /// real report modules in Phase 4.x will define their own bundles based on
    /// actual user research.</summary>
    public static readonly List<DateRangePreset> CommonReports = new()
    {
        Today,
        Last7Days,
        Last30Days,
        ThisMonth,
        LastMonth
    };

    /// <summary>Common preset bundle for forward-window scheduling UIs. Example only —
    /// real scheduling modules in Phase 4.x will define their own bundles based on
    /// actual user research.</summary>
    public static readonly List<DateRangePreset> CommonScheduling = new()
    {
        Today,
        Tomorrow,
        Next7Days,
        NextWeek,
        NextMonth
    };

    // =========================================================================
    // Direction × Span preset builder (DateTime migration A54).
    // Resolvers are parameterized by "today", week start, and fiscal-year start month
    // so they honor the picker's TimeSource/WeekStart/FiscalYearStart. Cumulative spans.
    // Date math validated against a fixed-date reference table before implementation.
    // =========================================================================

    private static DateOnly Eom(int y, int m) => new(y, m, DateTime.DaysInMonth(y, m));

    private static (DateOnly s, DateOnly e) FyBounds(DateOnly d, int fyStartMonth)
    {
        var startYear = d.Month >= fyStartMonth ? d.Year : d.Year - 1;
        var start = new DateOnly(startYear, fyStartMonth, 1);
        var end = start.AddYears(1).AddDays(-1);
        return (start, end);
    }

    private static (DateOnly s, DateOnly e) FiscalQuarterBounds(DateOnly d, int fyStartMonth)
    {
        var (fys, _) = FyBounds(d, fyStartMonth);
        var monthsIn = (d.Year - fys.Year) * 12 + (d.Month - fys.Month);
        var qIdx = monthsIn / 3;
        var qStart = fys.AddMonths(qIdx * 3);
        var qEnd = qStart.AddMonths(3).AddDays(-1);
        return (qStart, qEnd);
    }

    // Most-recent week-start day on or before d.
    private static DateOnly WeekStartOnOrBefore(DateOnly d, DayOfWeek weekStart)
    {
        int delta = ((int)d.DayOfWeek - (int)weekStart + 7) % 7;
        return d.AddDays(-delta);
    }

    /// <summary>
    /// Build the preset list for a direction + cumulative span, resolving relative to
    /// <paramref name="today"/>. Week presets use <paramref name="weekStart"/>; quarter and FY
    /// presets align to <paramref name="fyStartMonth"/> (1–12; April = 4 for the Indian FY default,
    /// January = 1 yields calendar quarters). Resolvers capture the supplied anchors lazily so
    /// the panel reflects the values at build time.
    /// </summary>
    public static List<DateRangePreset> Build(
        PresetDirection direction,
        PresetSpan span,
        DateOnly today,
        DayOfWeek weekStart = DayOfWeek.Sunday,
        int fyStartMonth = 4)
    {
        bool past = direction is PresetDirection.Past or PresetDirection.Both;
        bool future = direction is PresetDirection.Future or PresetDirection.Both;
        var list = new List<DateRangePreset>();

        // ── Day tier (always) ──
        list.Add(new("Today", () => (today, today)));
        if (past) list.Add(new("Yesterday", () => { var y = today.AddDays(-1); return (y, y); }));
        if (future) list.Add(new("Tomorrow", () => { var t = today.AddDays(1); return (t, t); }));

        // ── Week tier ──
        if (span >= PresetSpan.Week)
        {
            var ws = WeekStartOnOrBefore(today, weekStart);
            if (past)
            {
                list.Add(new("Last 7 days", () => (today.AddDays(-6), today)));
                list.Add(new("Last week", () => (ws.AddDays(-7), ws.AddDays(-1))));
            }
            if (future)
            {
                list.Add(new("Next 7 days", () => (today, today.AddDays(6))));
                list.Add(new("Next week", () => (ws.AddDays(7), ws.AddDays(13))));
            }
        }

        // ── Month tier ──
        if (span >= PresetSpan.Month)
        {
            var firstOfThis = new DateOnly(today.Year, today.Month, 1);
            list.Add(new("This month", () => (firstOfThis, Eom(today.Year, today.Month))));
            if (past)
            {
                list.Add(new("Last 30 days", () => (today.AddDays(-29), today)));
                list.Add(new("Last month", () => { var lm = firstOfThis.AddMonths(-1); return (lm, Eom(lm.Year, lm.Month)); }));
            }
            if (future)
            {
                list.Add(new("Next 30 days", () => (today, today.AddDays(29))));
                list.Add(new("Next month", () => { var nm = firstOfThis.AddMonths(1); return (nm, Eom(nm.Year, nm.Month)); }));
            }
        }

        // ── Quarter tier (fiscal, aligned to fyStartMonth) ──
        if (span >= PresetSpan.Quarter)
        {
            list.Add(new("This quarter", () => FiscalQuarterBounds(today, fyStartMonth)));
            if (past)
                list.Add(new("Last quarter", () => { var (qs, _) = FiscalQuarterBounds(today, fyStartMonth); return FiscalQuarterBounds(qs.AddDays(-1), fyStartMonth); }));
            if (future)
                list.Add(new("Next quarter", () => { var (_, qe) = FiscalQuarterBounds(today, fyStartMonth); return FiscalQuarterBounds(qe.AddDays(1), fyStartMonth); }));
        }

        // ── Year tier (FY + calendar year) ──
        if (span >= PresetSpan.Year)
        {
            list.Add(new("This FY", () => FyBounds(today, fyStartMonth)));
            list.Add(new("Current calendar year", () => (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31))));
            if (past)
            {
                list.Add(new("Last FY", () => { var (fs, _) = FyBounds(today, fyStartMonth); return FyBounds(fs.AddDays(-1), fyStartMonth); }));
                list.Add(new("Previous calendar year", () => (new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31))));
            }
        }

        return list;
    }
}
