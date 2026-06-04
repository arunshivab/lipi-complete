// SPEC: docs/00-COMPONENTS/2.8/DATETIME-CAPABILITY-SPEC-LOCKED.md §6.1
// PHASE: DateTime migration → LiPi.Components.Forms (CHANGE-LOG A54)
// COMPONENT: Date/Time picker family — format/parse/segment engine
//
// Ports the *engine* of the former LiPi.Web DateFormatService into the package as a
// static helper. The format/parse/segment-order logic is moved VERBATIM (token
// translation, the DD-sentinel, ISO + forgiving fallbacks, cross-format time parse,
// the segment regex). The only behavioral relocation: the three GetClinic* default
// getters are REMOVED — callers (the pickers) resolve format/timeformat/weekstart via
// `param ?? LipiInputDefaults.DefaultX` and pass the resolved string in. This helper
// always takes an explicit format; it never looks up a default itself.
//
// Format token vocabulary (design package §2.1):
//   D/DD day · M/MM/MMM/MMMM month · YY/YYYY year · separators  /  -  .  space

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LiPi.Components.Forms;

/// <summary>
/// Static date/time formatting + parsing + segment-order for the Date/Time picker family.
/// Format strings use design tokens (D/DD/M/MM/MMM/MMMM/YY/YYYY); translated to .NET tokens
/// internally. All methods take an explicit format — defaults are resolved by the caller.
/// </summary>
internal static class LipiDateFormat
{
    // ── Token translation: design tokens → .NET culture tokens (verbatim from service) ──
    // .NET uses lowercase 'y' for years; design package uses uppercase 'Y'. Convert at the
    // boundary so ToString/ParseExact get what they expect.
    private static string ToDotNetFormat(string designFormat)
    {
        var s = designFormat;
        s = s.Replace("YYYY", "yyyy");
        s = s.Replace("YY", "yy");
        // DD → dd / D → d, longest-first via a sentinel to avoid double-replace.
        s = s.Replace("DD", "\u0001\u0001");
        s = s.Replace("D", "d");
        s = s.Replace("\u0001\u0001", "dd");
        // M tokens unchanged — .NET uses uppercase M for months (lowercase m = minutes).
        return s;
    }

    // ── Date format / parse ──────────────────────────────────────────────
    public static string FormatDate(DateOnly date, string format)
    {
        try
        {
            return date.ToString(ToDotNetFormat(format), CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);  // ISO fallback
        }
    }

    public static DateOnly? ParseDate(string input, string format)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var dotNet = ToDotNetFormat(format);

        if (DateOnly.TryParseExact(input.Trim(), dotNet,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        // Always accept ISO 8601 (DB storage form; users may paste it).
        if (DateOnly.TryParseExact(input.Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            return parsed;

        // Final forgiving fallback — unambiguous general parse, else null (never throw).
        if (DateOnly.TryParse(input.Trim(),
                CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            return parsed;

        return null;
    }

    // ── Time format / parse ("12h" | "24h") (verbatim from service) ───────
    public static string FormatTime(TimeOnly time, string format)
    {
        var fmt = (format ?? "24h").ToLowerInvariant();
        return fmt switch
        {
            "12h" => time.ToString("h:mm tt", CultureInfo.InvariantCulture),
            _ => time.ToString("HH:mm", CultureInfo.InvariantCulture),
        };
    }

    public static TimeOnly? ParseTime(string input, string format)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input.Trim();
        var fmt = (format ?? "24h").ToLowerInvariant();

        var configured = fmt switch
        {
            "12h" => new[] { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt" },
            _ => new[] { "HH:mm", "H:mm", "HHmm", "Hmm" },
        };
        foreach (var pat in configured)
            if (TimeOnly.TryParseExact(trimmed, pat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var t1))
                return t1;

        // Cross-format fallback — accept the OTHER style if the user mixes them.
        var cross = fmt switch
        {
            "12h" => new[] { "HH:mm", "H:mm" },
            _ => new[] { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt" },
        };
        foreach (var pat in cross)
            if (TimeOnly.TryParseExact(trimmed, pat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var t2))
                return t2;

        if (TimeOnly.TryParse(trimmed,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var t3))
            return t3;

        return null;
    }

    // ── Segment order for InputMode=Segments (verbatim from service) ──────
    private static readonly Regex TokenRegex =
        new("(YYYY|YY|MMMM|MMM|MM|M|DD|D)", RegexOptions.Compiled);

    public static List<string> GetSegmentOrder(string format)
    {
        var matches = TokenRegex.Matches(format);
        var order = new List<string>(matches.Count);

        foreach (Match m in matches)
        {
            var token = m.Value;
            var normalized = token switch
            {
                "D" => "DD",
                "M" => "MM",
                "MMM" => "MM",   // word-month → numeric for segment input
                "MMMM" => "MM",
                "YY" => "YY",
                _ => token,       // DD, MM, YYYY pass through
            };
            order.Add(normalized);
        }

        if (order.Count == 0)
            return new List<string> { "DD", "MM", "YYYY" };  // defensive default

        return order;
    }
}
