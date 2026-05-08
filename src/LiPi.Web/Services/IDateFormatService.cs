// SPEC:    docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d-c)
// PHASE:   2 Sub-step 2.4 — Date/Time component family (Batch 9d-a)
// AMEND:   docs/CHANGE-LOG.md A20 (pending)
//
// IDateFormatService — clinic-configurable date and time format provider.
//
// Phase 2.4 ships components that bind to DateOnly?, TimeOnly?, and
// DateTimeOffset?. The DISPLAY format of these values varies by clinic
// preference (DD/MM/YYYY for India, MM/DD/YYYY for US, ISO yyyy-MM-dd for
// machine-friendly forms, etc.). DATABASE storage is always ISO 8601 — this
// service only governs display + parse at the UI layer.
//
// Default implementation hardcodes India defaults (DD/MM/YYYY, 24h, Sunday-
// first week, etc.). When clinic-config schema lands (master.clinics gains
// date_format / time_format / week_start_day / timezone_id columns — see
// design package §7), the service implementation reads from clinic context
// without touching consumers.
//
// Per-component override: every Date/Time component has a Format / TimeFormat
// parameter (string?). When set, overrides this service. When null, falls back.

using System;
using System.Collections.Generic;

namespace LiPi.Web.Services;

/// <summary>
/// Provides clinic-configurable date and time format defaults for the LiPi
/// Date/Time component family. Implementations should read from the current
/// clinic's configuration; the default Phase 2.4 implementation hardcodes
/// India defaults until the clinic-config schema lands.
/// </summary>
public interface IDateFormatService
{
    /// <summary>
    /// Get the clinic's preferred date display format.
    /// Default: "DD/MM/YYYY" (India). Other supported tokens: D/DD, M/MM/MMM/MMMM, YY/YYYY,
    /// with /, -, ., or space separators. See <c>DateFormatService.SupportedDateFormats</c>
    /// for the full list of recognized format strings.
    /// </summary>
    string GetClinicDateFormat();

    /// <summary>
    /// Get the clinic's preferred time display format.
    /// Default: "24h" (clinical safety default — no AM/PM ambiguity for medication times).
    /// Values: "12h" | "24h".
    /// </summary>
    string GetClinicTimeFormat();

    /// <summary>
    /// Get the clinic's preferred week start day.
    /// Default: <see cref="DayOfWeek.Sunday"/> (India + US convention).
    /// European clinics typically use <see cref="DayOfWeek.Monday"/>.
    /// </summary>
    DayOfWeek GetClinicWeekStart();

    /// <summary>
    /// Format a <see cref="DateOnly"/> for display per the clinic's preferred format
    /// (or the explicit override).
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <param name="format">Optional override — if null, uses <see cref="GetClinicDateFormat"/>.</param>
    /// <returns>Formatted string like "07/05/2026", "May 7 2026", etc.</returns>
    string FormatDate(DateOnly date, string? format = null);

    /// <summary>
    /// Parse user input as a <see cref="DateOnly"/> per the clinic's preferred format
    /// (or the explicit override). Returns null on parse failure — do not throw.
    ///
    /// Word-month formats (DD MMM YYYY, etc.) are accepted on input only when the
    /// override format also uses word-months. For segment-input contexts, callers
    /// should pass the numeric format equivalent (DD/MM/YYYY) since segment inputs
    /// are numeric-only.
    /// </summary>
    DateOnly? ParseDate(string input, string? format = null);

    /// <summary>
    /// Format a <see cref="TimeOnly"/> for display per the clinic's preferred format
    /// (or the explicit override).
    /// </summary>
    /// <param name="time">The time to format.</param>
    /// <param name="format">Optional override — if null, uses <see cref="GetClinicTimeFormat"/>.
    /// Values: "12h" / "24h" / null.</param>
    string FormatTime(TimeOnly time, string? format = null);

    /// <summary>
    /// Parse user input as a <see cref="TimeOnly"/> per the clinic's preferred format
    /// (or the explicit override). Returns null on parse failure — do not throw.
    /// </summary>
    TimeOnly? ParseTime(string input, string? format = null);

    /// <summary>
    /// Get the segment order for compound-input mode of LipiDatePicker.
    /// Returns the token order for the given format (or the clinic default).
    /// e.g., "DD/MM/YYYY" returns ["DD", "MM", "YYYY"]; "MM/DD/YYYY" returns
    /// ["MM", "DD", "YYYY"]; "YYYY-MM-DD" returns ["YYYY", "MM", "DD"].
    ///
    /// Used by InputMode=Segments rendering on LipiDatePicker. Word-month
    /// formats fall back to numeric MM for segment input (display-time formats
    /// stay rich; input-time formats are numeric-only — see design package §2.1).
    /// </summary>
    List<string> GetSegmentOrder(string? format = null);
}
