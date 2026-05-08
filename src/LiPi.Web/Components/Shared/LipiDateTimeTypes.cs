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

namespace LiPi.Web.Components.Shared;

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
    /// Segment order driven by <see cref="LiPi.Web.Services.IDateFormatService.GetSegmentOrder"/>.</summary>
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

/// <summary>A reusable preset for <see cref="LiPi.Web.Components.Shared.LipiDateRangePicker"/>'s
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
/// starter bundles (<see cref="CommonReports"/>, <see cref="CommonScheduling"/>)
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
}
