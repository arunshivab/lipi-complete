// SPEC:    docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d-c)
// PHASE:   2 Sub-step 2.4 — Date/Time component family (Batch 9d-a)
// AMEND:   docs/CHANGE-LOG.md A20 (pending)
//
// DateFormatService — Phase 2.4 default implementation.
//
// Hardcodes India defaults (DD/MM/YYYY, 24h, Sunday-first). When clinic-
// config schema lands (master.clinics gains date_format / time_format /
// week_start_day columns — see design package §7), this implementation
// will read from clinic context without consumer changes.
//
// Format token vocabulary (from design package §2.1):
//   D    — day, no leading zero  (1, 2, ..., 31)
//   DD   — day, leading zero     (01, 02, ..., 31)
//   M    — month, no leading zero (1, 2, ..., 12)
//   MM   — month, leading zero    (01, 02, ..., 12)
//   MMM  — month abbreviation    (Jan, Feb, ..., Dec)
//   MMMM — month full name       (January, February, ..., December)
//   YY   — year 2-digit          (24, 25, 26)
//   YYYY — year 4-digit          (2024, 2025, 2026)
// Separators: /  -  .  (space)
//
// Tokens use Y (uppercase) for years to align with the user-facing format
// strings shown in the design package and clinic configuration UI. Internally
// we translate to .NET's "yyyy"/"yy" for ToString() / ParseExact() calls.
//
// 12 standard combinations explicitly supported:
//   DD/MM/YYYY    DD-MM-YYYY    DD.MM.YYYY    "DD MM YYYY"
//   DD/MM/YY      DD-MMM-YYYY   "DD MMM YYYY" "DD MMMM YYYY"
//   MM/DD/YYYY    "MMM DD YYYY" DD/MM         YYYY-MM-DD
// Other combinations using the supported tokens + separators also work.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LiPi.Web.Services;

public sealed class DateFormatService : IDateFormatService
{
    // ==========================================================================
    // CLINIC DEFAULTS — Phase 2.4 hardcodes India until clinic-config lands
    // ==========================================================================

    private const string DefaultDateFormat = "DD/MM/YYYY";
    private const string DefaultTimeFormat = "24h";
    private const DayOfWeek DefaultWeekStart = DayOfWeek.Sunday;

    public string GetClinicDateFormat() => DefaultDateFormat;
    public string GetClinicTimeFormat() => DefaultTimeFormat;
    public DayOfWeek GetClinicWeekStart() => DefaultWeekStart;

    // ==========================================================================
    // FORMAT TOKEN TRANSLATION — design tokens (D/DD/M/MM/MMM/MMMM/YY/YYYY) →
    // .NET CultureInfo tokens (d/dd/M/MM/MMM/MMMM/yy/yyyy).
    //
    // .NET uses lowercase 'y' for years; the design package uses uppercase 'Y'
    // for clarity. Convert at the boundary so ToString/ParseExact get what
    // they expect.
    // ==========================================================================

    /// <summary>Translate a design-package format string to a .NET culture format string.
    /// e.g., "DD/MM/YYYY" → "dd/MM/yyyy", "DD-MMM-YYYY" → "dd-MMM-yyyy".</summary>
    private static string ToDotNetFormat(string designFormat)
    {
        // Year tokens: YYYY → yyyy, YY → yy. Order matters (longest first).
        var s = designFormat;
        s = s.Replace("YYYY", "yyyy");
        s = s.Replace("YY",   "yy");
        // D tokens: DD → dd, D → d. Order matters (longest first to avoid
        // converting the first D of "DD" prematurely).
        // Use a placeholder for DD to avoid double-replacing.
        s = s.Replace("DD", "\u0001\u0001");  // sentinel
        s = s.Replace("D",  "d");
        s = s.Replace("\u0001\u0001", "dd");
        // M tokens: keep MMMM/MMM/MM/M unchanged — .NET uses uppercase M for
        // months (lowercase m is minutes). Already correct.
        return s;
    }

    // ==========================================================================
    // DATE FORMAT / PARSE
    // ==========================================================================

    public string FormatDate(DateOnly date, string? format = null)
    {
        var fmt = format ?? GetClinicDateFormat();
        try
        {
            return date.ToString(ToDotNetFormat(fmt), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            // Fallback to ISO if the format string is malformed
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }

    public DateOnly? ParseDate(string input, string? format = null)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var fmt = format ?? GetClinicDateFormat();
        var dotNetFormat = ToDotNetFormat(fmt);

        // Try the configured format first (strict).
        if (DateOnly.TryParseExact(
                input.Trim(),
                dotNetFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed;
        }

        // Forgiving fallbacks — accept ISO 8601 always, since database storage uses it
        // and users may paste ISO timestamps from external systems.
        if (DateOnly.TryParseExact(
                input.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
        {
            return parsed;
        }

        // Final fallback: culture-invariant general parse. Accepts a wider range of
        // formats but only if they're unambiguous. Returns null if uninterpretable.
        if (DateOnly.TryParse(
                input.Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
        {
            return parsed;
        }

        return null;
    }

    // ==========================================================================
    // TIME FORMAT / PARSE
    //   "12h" → 12-hour format with AM/PM (e.g., "9:30 AM", "11:45 PM")
    //   "24h" → 24-hour format (e.g., "09:30", "23:45")
    // ==========================================================================

    public string FormatTime(TimeOnly time, string? format = null)
    {
        var fmt = (format ?? GetClinicTimeFormat()).ToLowerInvariant();
        return fmt switch
        {
            "12h" => time.ToString("h:mm tt", CultureInfo.InvariantCulture),
            _     => time.ToString("HH:mm",   CultureInfo.InvariantCulture),  // 24h default
        };
    }

    public TimeOnly? ParseTime(string input, string? format = null)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input.Trim();
        var fmt = (format ?? GetClinicTimeFormat()).ToLowerInvariant();

        // Try the configured format first.
        var configuredPatterns = fmt switch
        {
            "12h" => new[] { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt" },
            _     => new[] { "HH:mm",   "H:mm",     "HHmm",   "Hmm"     },
        };
        foreach (var pat in configuredPatterns)
        {
            if (TimeOnly.TryParseExact(
                    trimmed,
                    pat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var t1))
            {
                return t1;
            }
        }

        // Cross-format fallback — accept the OTHER format if the user pastes /
        // types in the unexpected style. Many clinical contexts mix formats.
        var crossPatterns = fmt switch
        {
            "12h" => new[] { "HH:mm",   "H:mm" },
            _     => new[] { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt" },
        };
        foreach (var pat in crossPatterns)
        {
            if (TimeOnly.TryParseExact(
                    trimmed,
                    pat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var t2))
            {
                return t2;
            }
        }

        // Final fallback: general parse.
        if (TimeOnly.TryParse(
                trimmed,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var t3))
        {
            return t3;
        }

        return null;
    }

    // ==========================================================================
    // SEGMENT ORDER — used by LipiDatePicker InputMode=Segments rendering.
    //
    // Walks the format string and extracts the order of D/M/Y tokens.
    // Word-month formats (MMM, MMMM) collapse to "MM" for segment input —
    // segments are numeric-only per design package §2.1. Display-time
    // rendering can still use the rich format (FormatDate respects MMM/MMMM).
    // ==========================================================================

    private static readonly Regex TokenRegex = new(
        "(YYYY|YY|MMMM|MMM|MM|M|DD|D)",
        RegexOptions.Compiled);

    public List<string> GetSegmentOrder(string? format = null)
    {
        var fmt = format ?? GetClinicDateFormat();
        var matches = TokenRegex.Matches(fmt);
        var order = new List<string>(matches.Count);

        foreach (Match m in matches)
        {
            var token = m.Value;
            // Collapse single-letter and word variants for segment-input purposes.
            var normalized = token switch
            {
                "D"    => "DD",
                "M"    => "MM",
                "MMM"  => "MM",  // word-month → numeric for input
                "MMMM" => "MM",  // word-month → numeric for input
                "YY"   => "YY",  // 2-digit year stays 2-digit
                _      => token, // DD, MM, YYYY pass through
            };
            order.Add(normalized);
        }

        // Defensive — every supported format produces 2 or 3 segments.
        // If the regex matched zero tokens (malformed format), return India
        // default so consumers don't crash.
        if (order.Count == 0)
        {
            return new List<string> { "DD", "MM", "YYYY" };
        }

        return order;
    }
}
