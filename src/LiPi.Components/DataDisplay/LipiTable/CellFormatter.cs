// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §3.3.2 (per-type rendering)
// PHASE: 2.8 Data Display — Stage 2 core shell
// COMPONENT: LipiTable internal — value formatting
//
// Converts a boxed cell value + ColumnType into the display STRING for the common,
// dependency-free types. The table's render loop calls FormatValue() and wraps the
// result in the appropriate element (mono span, status chip, etc.).
//
// SCOPE NOTE (Stage 2 bare chassis) — deliberate simplifications, each completed later:
//   • Date/DateTime/Time: formatted via the column Format string or invariant ToString().
//     Culture-aware formatting via an injected IDateFormatService abstraction is a later
//     stage (the service lives in LiPi.Web; injecting it into LiPi.Components needs an
//     isolation-clean abstraction, deferred).
//   • Status: returns the raw string; the table wraps it in a <span data-status> chip
//     styled by lipi-status-tokens.css. Full LipiBadge/LipiPill composition waits until
//     those Phase 2.7 components migrate into LiPi.Components (Phase 2.10) or a
//     Components-local badge is built.
//   • Currency: uses the column Format or culture currency format; CurrencyCode override
//     deferred.
//   • Avatar/File/Actions: handled by the table render loop with minimal placeholders,
//     not here (they're not plain value->string).

using System;
using System.Globalization;

namespace LiPi.Components.DataDisplay;

internal static class CellFormatter
{
    /// <summary>
    /// Format a boxed cell value to its display string for the given column type.
    /// Returns empty string for null. Avatar/File/Actions/Custom are handled by the
    /// table render loop, not here.
    /// </summary>
    public static string FormatValue(object? value, ColumnType type, string? format)
    {
        if (value is null) return string.Empty;

        // Explicit format string wins for any type that supports string.Format.
        if (!string.IsNullOrEmpty(format))
        {
            try { return string.Format(CultureInfo.CurrentCulture, "{0:" + format + "}", value); }
            catch (FormatException) { /* fall through to type default */ }
        }

        return type switch
        {
            ColumnType.Number   => FormatNumber(value),
            ColumnType.Currency => FormatCurrency(value),
            ColumnType.Date     => FormatDate(value),
            ColumnType.DateTime => FormatDateTime(value),
            ColumnType.Time     => FormatTime(value),
            ColumnType.Boolean  => FormatBoolean(value),
            // Text, Mono, Status, Link, Custom -> raw string
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatNumber(object value)
    {
        // Mono-tabular numerals + culture thousands separators. Integers -> N0, else N2.
        return value switch
        {
            int i    => i.ToString("N0", CultureInfo.CurrentCulture),
            long l   => l.ToString("N0", CultureInfo.CurrentCulture),
            short s  => s.ToString("N0", CultureInfo.CurrentCulture),
            byte b   => b.ToString("N0", CultureInfo.CurrentCulture),
            decimal m => m == Math.Truncate(m)
                            ? m.ToString("N0", CultureInfo.CurrentCulture)
                            : m.ToString("N2", CultureInfo.CurrentCulture),
            double d => d == Math.Truncate(d)
                            ? d.ToString("N0", CultureInfo.CurrentCulture)
                            : d.ToString("N2", CultureInfo.CurrentCulture),
            float f  => f.ToString("N2", CultureInfo.CurrentCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatCurrency(object value)
    {
        // Culture currency format (₹ for en-IN, $ for en-US, etc.). CurrencyCode override deferred.
        return value switch
        {
            decimal m => m.ToString("C2", CultureInfo.CurrentCulture),
            double d  => d.ToString("C2", CultureInfo.CurrentCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatDate(object value)
    {
        return value switch
        {
            DateOnly d    => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime dt   => dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatDateTime(object value)
    {
        return value switch
        {
            DateTime dt        => dt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatTime(object value)
    {
        return value switch
        {
            TimeOnly t  => t.ToString("HH:mm", CultureInfo.InvariantCulture),
            TimeSpan ts => ts.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("HH:mm", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatBoolean(object value)
    {
        // Default true/false display handled by the table (it renders a check glyph or
        // TrueLabel/FalseLabel). This returns the raw token for fallback/copy contexts.
        return value switch
        {
            bool b => b ? "true" : "false",
            _ => value.ToString() ?? string.Empty
        };
    }
}
