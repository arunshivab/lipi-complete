// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §S3a/S3b (operators, editors), §A58 (relative-date gating)
// CROSS-REF: CHANGE-LOG.md A65 (PR6b — shared filter editor extraction)
// PHASE: 2.8 Data Display — filter track (PR6b)
//
// Pure, stateless filter metadata + draft helpers shared by LipiTable's filter surfaces and the
// standalone <LipiFilterBar>: which operators a column type offers (with A58 relative-date
// gating), which value editor an operator needs, operator labels, value coercion, seeding a
// draft from an applied filter, and building a FilterDescriptor from a draft. Single source of
// truth so the table and the bar never drift. Extracted verbatim from LipiTable.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LiPi.Components.DataDisplay;

/// <summary>Stateless filter-editor metadata and draft↔descriptor helpers.</summary>
public static class LipiFilterOperators
{
    /// <summary>Which value editor a given draft operator needs (drives the editor body).</summary>
    public enum FilterEditor { None, Text, Number, NumberRange, Date, DateRange, RelativeN, Multi }

    /// <summary>
    /// Operators offered for a column type. Date/DateTime appends the relative operators gated by
    /// the column's resolved <paramref name="relativeSpans"/> (null = all buckets).
    /// </summary>
    public static IReadOnlyList<FilterOperator> OperatorsFor(ColumnType type, DateSpan? relativeSpans)
    {
        switch (type)
        {
            case ColumnType.Number:
            case ColumnType.Currency:
                return new[]
                {
                    FilterOperator.Equals, FilterOperator.NotEquals,
                    FilterOperator.GreaterThan, FilterOperator.GreaterThanOrEqual,
                    FilterOperator.LessThan, FilterOperator.LessThanOrEqual,
                    FilterOperator.Between, FilterOperator.Empty, FilterOperator.NotEmpty
                };

            case ColumnType.Date:
            case ColumnType.DateTime:
            {
                var ops = new List<FilterOperator>
                {
                    FilterOperator.Equals,            // On
                    FilterOperator.LessThan,          // Before
                    FilterOperator.GreaterThan,       // After
                    FilterOperator.Between,
                };
                var spans = relativeSpans;
                foreach (var op in RelativeOperatorsInOrder)
                    if ((spans & SpanOf(op)) != 0)
                        ops.Add(op);
                ops.Add(FilterOperator.Empty);
                ops.Add(FilterOperator.NotEmpty);
                return ops;
            }

            case ColumnType.Boolean:
                return new[] { FilterOperator.IsTrue, FilterOperator.IsFalse };

            case ColumnType.Status:
                return new[]
                {
                    FilterOperator.In, FilterOperator.Equals, FilterOperator.NotEquals,
                    FilterOperator.Empty, FilterOperator.NotEmpty
                };

            default: // Text, Mono, Link, etc.
                return new[]
                {
                    FilterOperator.Contains, FilterOperator.NotContains,
                    FilterOperator.Equals, FilterOperator.NotEquals,
                    FilterOperator.StartsWith, FilterOperator.EndsWith,
                    FilterOperator.In,
                    FilterOperator.Empty, FilterOperator.NotEmpty
                };
        }
    }

    /// <summary>Which editor a given column type + operator needs.</summary>
    public static FilterEditor EditorFor(ColumnType type, FilterOperator op)
    {
        if (op is FilterOperator.Empty or FilterOperator.NotEmpty
               or FilterOperator.IsTrue or FilterOperator.IsFalse) return FilterEditor.None;
        if (IsRelativeDateOperator(op))
            return op is FilterOperator.LastNDays or FilterOperator.NextNDays
                ? FilterEditor.RelativeN : FilterEditor.None;
        if (op == FilterOperator.In) return FilterEditor.Multi;
        if (op == FilterOperator.Between)
            return type is ColumnType.Date or ColumnType.DateTime
                ? FilterEditor.DateRange : FilterEditor.NumberRange;
        if (type is ColumnType.Date or ColumnType.DateTime) return FilterEditor.Date;
        if (type is ColumnType.Number or ColumnType.Currency) return FilterEditor.Number;
        return FilterEditor.Text;
    }

    /// <summary>Does an operator need a value input? Empty/NotEmpty/boolean/fixed-window don't.</summary>
    public static bool OperatorNeedsValue(FilterOperator op)
    {
        if (op is FilterOperator.Empty or FilterOperator.NotEmpty
               or FilterOperator.IsTrue or FilterOperator.IsFalse) return false;
        if (op is FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow
               or FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek
               or FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth
               or FilterOperator.ThisQuarter or FilterOperator.LastQuarter
               or FilterOperator.ThisYear or FilterOperator.LastYear) return false;
        return true;
    }

    /// <summary>True for any relative-date operator (Today … LastNDays/NextNDays).</summary>
    public static bool IsRelativeDateOperator(FilterOperator op) => op is
        FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow or
        FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek or
        FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth or
        FilterOperator.ThisQuarter or FilterOperator.LastQuarter or
        FilterOperator.ThisYear or FilterOperator.LastYear or
        FilterOperator.LastNDays or FilterOperator.NextNDays;

    /// <summary>Relative date operators in display order — gated per-column by RelativeDateSpans.</summary>
    public static readonly FilterOperator[] RelativeOperatorsInOrder =
    {
        FilterOperator.Today, FilterOperator.Yesterday, FilterOperator.Tomorrow,
        FilterOperator.ThisWeek, FilterOperator.LastWeek, FilterOperator.NextWeek,
        FilterOperator.ThisMonth, FilterOperator.LastMonth, FilterOperator.NextMonth,
        FilterOperator.ThisQuarter, FilterOperator.LastQuarter,
        FilterOperator.ThisYear, FilterOperator.LastYear,
        FilterOperator.LastNDays, FilterOperator.NextNDays,
    };

    /// <summary>Which DateSpan bucket a relative operator belongs to.</summary>
    public static DateSpan SpanOf(FilterOperator op) => op switch
    {
        FilterOperator.Today or FilterOperator.Yesterday or FilterOperator.Tomorrow
            or FilterOperator.LastNDays or FilterOperator.NextNDays => DateSpan.Day,
        FilterOperator.ThisWeek or FilterOperator.LastWeek or FilterOperator.NextWeek => DateSpan.Week,
        FilterOperator.ThisMonth or FilterOperator.LastMonth or FilterOperator.NextMonth => DateSpan.Month,
        FilterOperator.ThisQuarter or FilterOperator.LastQuarter => DateSpan.Quarter,
        FilterOperator.ThisYear or FilterOperator.LastYear => DateSpan.Year,
        _ => DateSpan.None
    };

    /// <summary>Type-aware operator label (date columns relabel on/before/after).</summary>
    public static string OperatorLabelFor(ColumnType type, FilterOperator op)
    {
        if (type is ColumnType.Date or ColumnType.DateTime)
        {
            switch (op)
            {
                case FilterOperator.Equals:      return "on";
                case FilterOperator.LessThan:    return "before";
                case FilterOperator.GreaterThan: return "after";
            }
        }
        return OperatorLabel(op);
    }

    /// <summary>Plain operator label.</summary>
    public static string OperatorLabel(FilterOperator op) => op switch
    {
        FilterOperator.Contains    => "contains",
        FilterOperator.NotContains => "does not contain",
        FilterOperator.Equals      => "equals",
        FilterOperator.NotEquals   => "not equals",
        FilterOperator.StartsWith  => "starts with",
        FilterOperator.EndsWith    => "ends with",
        FilterOperator.Empty       => "is empty",
        FilterOperator.NotEmpty    => "is not empty",
        FilterOperator.GreaterThan => "greater than",
        FilterOperator.GreaterThanOrEqual => "\u2265",
        FilterOperator.LessThan    => "less than",
        FilterOperator.LessThanOrEqual    => "\u2264",
        FilterOperator.Between     => "between",
        FilterOperator.In          => "in",
        FilterOperator.IsTrue      => "is true",
        FilterOperator.IsFalse     => "is false",
        FilterOperator.Today       => "is today",
        FilterOperator.Yesterday   => "is yesterday",
        FilterOperator.Tomorrow    => "is tomorrow",
        FilterOperator.ThisWeek    => "this week",
        FilterOperator.LastWeek    => "last week",
        FilterOperator.NextWeek    => "next week",
        FilterOperator.ThisMonth   => "this month",
        FilterOperator.LastMonth   => "last month",
        FilterOperator.NextMonth   => "next month",
        FilterOperator.ThisQuarter => "this quarter",
        FilterOperator.LastQuarter => "last quarter",
        FilterOperator.ThisYear    => "this year",
        FilterOperator.LastYear    => "last year",
        FilterOperator.LastNDays   => "in the last N days",
        FilterOperator.NextNDays   => "in the next N days",
        _ => op.ToString().ToLowerInvariant()
    };

    /// <summary>Coerce a draft string into the typed value the engine compares against.</summary>
    public static object Coerce(FilterEditor editor, string raw)
    {
        switch (editor)
        {
            case FilterEditor.Number:
            case FilterEditor.NumberRange:
                if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
                return raw;
            case FilterEditor.Date:
            case FilterEditor.DateRange:
                if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return dt;
                return raw;
            case FilterEditor.RelativeN:
                if (int.TryParse(raw, out var n)) return n;
                return 0;
            default:
                return raw;
        }
    }

    /// <summary>(Re)seed a draft from a column's applied filter (or operator default when unfiltered).</summary>
    public static LipiFilterDraft SeedDraft(FilterDescriptor? existing, ColumnType type, IReadOnlyList<FilterOperator> ops)
    {
        var d = new LipiFilterDraft();
        d.Operator = existing?.Operator ?? ops[0];
        d.ValueEnd = existing?.ValueEnd?.ToString() ?? string.Empty;
        if (existing?.Operator == FilterOperator.In && existing.Value is System.Collections.IEnumerable en)
            foreach (var it in en) { var t = it?.ToString(); if (t is not null) d.Multi.Add(t); }
        d.Value = existing?.Operator == FilterOperator.In
            ? string.Empty
            : existing?.Value?.ToString() ?? string.Empty;
        if (type == ColumnType.Boolean)
        {
            d.Value = existing?.Operator switch
            {
                FilterOperator.IsTrue  => "true",
                FilterOperator.IsFalse => "false",
                _                      => string.Empty
            };
        }
        if (type is ColumnType.Date or ColumnType.DateTime)
        {
            if (existing?.Value is DateOnly ds) d.DateStart = ds;
            if (existing?.ValueEnd is DateOnly de) d.DateEnd = de;
        }
        return d;
    }

    /// <summary>Build the 0-or-1 descriptor for a column from its draft (null = clear the column).</summary>
    public static FilterDescriptor? BuildDescriptor(string key, LipiFilterDraft d, ColumnType type)
    {
        FilterDescriptor? desc = null;

        if (type is ColumnType.Date or ColumnType.DateTime)
        {
            if (d.DateStart is { } s && d.DateEnd is { } e)
                desc = new FilterDescriptor(key, FilterOperator.Between, s, e);
            else if (d.DateStart is { } s2)
                desc = new FilterDescriptor(key, FilterOperator.GreaterThanOrEqual, s2, null);
            else if (d.DateEnd is { } e2)
                desc = new FilterDescriptor(key, FilterOperator.LessThanOrEqual, e2, null);
        }
        else if (type == ColumnType.Boolean)
        {
            if (d.Value == "true") desc = new FilterDescriptor(key, FilterOperator.IsTrue, null, null);
            else if (d.Value == "false") desc = new FilterDescriptor(key, FilterOperator.IsFalse, null, null);
        }
        else
        {
            var needsValue = OperatorNeedsValue(d.Operator);
            var editor = EditorFor(type, d.Operator);
            if (!needsValue)
                desc = new FilterDescriptor(key, d.Operator, null, null);
            else if (editor == FilterEditor.Multi)
            {
                if (d.Multi.Count > 0)
                    desc = new FilterDescriptor(key, d.Operator,
                        (IReadOnlyList<object>)d.Multi.Cast<object>().ToList(), null);
            }
            else if (editor is FilterEditor.NumberRange or FilterEditor.DateRange)
            {
                if (!string.IsNullOrEmpty(d.Value) && !string.IsNullOrEmpty(d.ValueEnd))
                    desc = new FilterDescriptor(key, d.Operator,
                        Coerce(editor, d.Value), Coerce(editor, d.ValueEnd));
            }
            else if (!string.IsNullOrEmpty(d.Value))
                desc = new FilterDescriptor(key, d.Operator, Coerce(editor, d.Value), null);
        }

        return desc;
    }
}
