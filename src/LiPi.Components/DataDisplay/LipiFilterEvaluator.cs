// SPEC: docs/00-COMPONENTS — Phase 2.8 Data Display / filtering (PR5a shared filter model).
// PR5a — pure, stateless evaluation of a single FilterDescriptor against a cell value,
// extracted verbatim from LipiTable.MatchesFilter so that LipiSlicer (and any future filter
// surface) can evaluate descriptors for faceted counts without a table instance. Relative-date
// windows resolve from the supplied time zone + week start instead of table instance state.
using System;
using System.Collections;
using System.Globalization;

namespace LiPi.Components.DataDisplay;

public static class LipiFilterEvaluator
{
    /// <summary>True when <paramref name="value"/> satisfies the descriptor. Operator dispatch
    /// mirrors the prior LipiTable engine (universal → In → numeric → date → text).</summary>
    public static bool Matches(object? value, FilterDescriptor f, bool caseSensitive,
                               TimeZoneInfo? timeZone, DayOfWeek weekStart)
    {
        var sc = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        // ── Universal (value-independent) ──
        switch (f.Operator)
        {
            case FilterOperator.Empty:
                return value is null || string.IsNullOrEmpty(value.ToString());
            case FilterOperator.NotEmpty:
                return value is not null && !string.IsNullOrEmpty(value.ToString());
            case FilterOperator.IsTrue:
                return value is bool bt && bt;
            case FilterOperator.IsFalse:
                return value is bool bf && !bf;
        }

        // ── Set membership (In): Value is IReadOnlyList<object> of selected tokens ──
        if (f.Operator == FilterOperator.In)
        {
            if (f.Value is not IEnumerable items) return true;
            var sv = value?.ToString();
            foreach (var it in items)
                if (string.Equals(it?.ToString(), sv, sc)) return true;
            return false;
        }

        // ── Numeric operators ──
        if (IsNumericOperator(f.Operator) && TryToDouble(value, out var nv))
        {
            if (f.Operator == FilterOperator.Between)
            {
                if (!TryToDouble(f.Value, out var lo) || !TryToDouble(f.ValueEnd, out var hi)) return true;
                if (lo > hi) (lo, hi) = (hi, lo);
                return nv >= lo && nv <= hi;
            }
            if (!TryToDouble(f.Value, out var cmp)) return true;
            return f.Operator switch
            {
                FilterOperator.Equals             => nv == cmp,
                FilterOperator.NotEquals          => nv != cmp,
                FilterOperator.GreaterThan        => nv > cmp,
                FilterOperator.GreaterThanOrEqual => nv >= cmp,
                FilterOperator.LessThan           => nv < cmp,
                FilterOperator.LessThanOrEqual    => nv <= cmp,
                _ => true
            };
        }

        // ── Date operators (explicit On/Before/After/Between + relative set) ──
        if (TryToDate(value, out var dv))
        {
            if (IsRelativeDateOperator(f.Operator))
            {
                var (rs, re) = RelativeWindow(f.Operator, f.Value, timeZone, weekStart);
                return dv >= rs && dv <= re;
            }
            if (IsExplicitDateOperator(f.Operator))
            {
                if (f.Operator == FilterOperator.Between)
                {
                    if (!TryToDate(f.Value, out var ds) || !TryToDate(f.ValueEnd, out var de)) return true;
                    if (ds > de) (ds, de) = (de, ds);
                    return dv >= ds && dv <= de;
                }
                if (!TryToDate(f.Value, out var d1)) return true;
                return f.Operator switch
                {
                    FilterOperator.Equals             => dv == d1,
                    FilterOperator.NotEquals          => dv != d1,
                    FilterOperator.GreaterThan        => dv > d1,   // After
                    FilterOperator.LessThan           => dv < d1,   // Before
                    FilterOperator.GreaterThanOrEqual => dv >= d1,
                    FilterOperator.LessThanOrEqual    => dv <= d1,
                    _ => true
                };
            }
        }

        // ── Text operators (S3a behavior) ──
        var s = value?.ToString();
        var term = f.Value?.ToString();
        if (string.IsNullOrEmpty(term)) return true;   // no constraint
        if (s is null) return false;
        return f.Operator switch
        {
            FilterOperator.Contains    => s.IndexOf(term, sc) >= 0,
            FilterOperator.NotContains => s.IndexOf(term, sc) < 0,
            FilterOperator.Equals      => string.Equals(s, term, sc),
            FilterOperator.NotEquals   => !string.Equals(s, term, sc),
            FilterOperator.StartsWith  => s.StartsWith(term, sc),
            FilterOperator.EndsWith    => s.EndsWith(term, sc),
            _ => true
        };
    }

    // ── typed-coercion + relative-date helpers (moved verbatim from LipiTable) ──
    private static bool TryToDouble(object? v, out double d)
    {
        switch (v)
        {
            case null: d = 0; return false;
            case double dd: d = dd; return true;
            case float f:   d = f;  return true;
            case decimal m: d = (double)m; return true;
            case int i:     d = i;  return true;
            case long l:    d = l;  return true;
            case short s:   d = s;  return true;
            case byte b:    d = b;  return true;
        }
        return double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d)
            || double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out d);
    }

    private static bool TryToDate(object? v, out DateOnly d)
    {
        switch (v)
        {
            case DateOnly db:        d = db; return true;
            case DateTime dt:        d = DateOnly.FromDateTime(dt); return true;
            case DateTimeOffset dto: d = DateOnly.FromDateTime(dto.DateTime); return true;
            case null:               d = default; return false;
        }
        if (DateOnly.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return true;
        if (DateTime.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
        { d = DateOnly.FromDateTime(dt2); return true; }
        return false;
    }

    private static bool IsNumericOperator(FilterOperator op) => op is
        FilterOperator.Equals or FilterOperator.NotEquals or
        FilterOperator.GreaterThan or FilterOperator.GreaterThanOrEqual or
        FilterOperator.LessThan or FilterOperator.LessThanOrEqual or FilterOperator.Between;

    private static bool IsExplicitDateOperator(FilterOperator op) => op is
        FilterOperator.Equals or FilterOperator.NotEquals or
        FilterOperator.GreaterThan or FilterOperator.LessThan or
        FilterOperator.GreaterThanOrEqual or FilterOperator.LessThanOrEqual or
        FilterOperator.Between;

    private static bool IsRelativeDateOperator(FilterOperator op) => op is
        FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow or
        FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek or
        FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth or
        FilterOperator.ThisQuarter or FilterOperator.LastQuarter or
        FilterOperator.ThisYear or FilterOperator.LastYear or
        FilterOperator.LastNDays or FilterOperator.NextNDays;

    // Resolve a [start,end] DateOnly window for a relative operator, anchored at "today"
    // (resolved in the supplied time zone, else server-local). Week boundaries use weekStart.
    private static (DateOnly start, DateOnly end) RelativeWindow(
        FilterOperator op, object? nValue, TimeZoneInfo? timeZone, DayOfWeek weekStart)
    {
        var today = timeZone is { } tz
            ? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz))
            : DateOnly.FromDateTime(DateTime.Now);
        DateOnly Eom(int y, int m) => new(y, m, DateTime.DaysInMonth(y, m));
        DateOnly WkStart(DateOnly d) { int delta = ((int)d.DayOfWeek - (int)weekStart + 7) % 7; return d.AddDays(-delta); }
        int n() { if (nValue is int i) return i; int.TryParse(nValue?.ToString(), out var k); return k; }

        switch (op)
        {
            case FilterOperator.Today:     return (today, today);
            case FilterOperator.Yesterday: return (today.AddDays(-1), today.AddDays(-1));
            case FilterOperator.Tomorrow:  return (today.AddDays(1), today.AddDays(1));
            case FilterOperator.ThisWeek:  { var s = WkStart(today); return (s, s.AddDays(6)); }
            case FilterOperator.LastWeek:  { var s = WkStart(today).AddDays(-7); return (s, s.AddDays(6)); }
            case FilterOperator.NextWeek:  { var s = WkStart(today).AddDays(7); return (s, s.AddDays(6)); }
            case FilterOperator.ThisMonth: { var s = new DateOnly(today.Year, today.Month, 1); return (s, Eom(s.Year, s.Month)); }
            case FilterOperator.LastMonth: { var s = new DateOnly(today.Year, today.Month, 1).AddMonths(-1); return (s, Eom(s.Year, s.Month)); }
            case FilterOperator.NextMonth: { var s = new DateOnly(today.Year, today.Month, 1).AddMonths(1); return (s, Eom(s.Year, s.Month)); }
            case FilterOperator.ThisQuarter: { int q = (today.Month - 1) / 3; var s = new DateOnly(today.Year, q * 3 + 1, 1); var e = s.AddMonths(3).AddDays(-1); return (s, e); }
            case FilterOperator.LastQuarter: { int q = (today.Month - 1) / 3; var ts = new DateOnly(today.Year, q * 3 + 1, 1); var s = ts.AddMonths(-3); return (s, ts.AddDays(-1)); }
            case FilterOperator.ThisYear:  return (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31));
            case FilterOperator.LastYear:  return (new DateOnly(today.Year - 1, 1, 1), new DateOnly(today.Year - 1, 12, 31));
            case FilterOperator.LastNDays: { int k = Math.Max(0, n()); return (today.AddDays(-k), today); }
            case FilterOperator.NextNDays: { int k = Math.Max(0, n()); return (today, today.AddDays(k)); }
            default: return (today, today);
        }
    }
}
