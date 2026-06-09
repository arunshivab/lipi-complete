// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §S3 (filter editor / draft model)
// CROSS-REF: CHANGE-LOG.md A65 (PR6b — shared filter editor extraction)
// PHASE: 2.8 Data Display — filter track (PR6b)
//
// Ephemeral working copy of ONE column's in-progress filter edit (operator + value(s)),
// shared by LipiTable's filter surfaces (popover / drawer / sidebar / filter bar) and by the
// standalone <LipiFilterBar>. Promoted from LipiTable's former private ColumnDraft so the
// extracted <LipiFilterEditor> and both hosts bind to a single type — no drift.

using System;
using System.Collections.Generic;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Mutable, ephemeral draft of a single column's filter edit. (Re)seeded from the applied
/// filter when an editor opens; converted to a <see cref="FilterDescriptor"/> on commit via
/// <see cref="LipiFilterOperators.BuildDescriptor"/>.
/// </summary>
public sealed class LipiFilterDraft
{
    /// <summary>The operator being edited.</summary>
    public FilterOperator Operator = FilterOperator.Contains;

    /// <summary>Primary value (text/number/date string, or single bound).</summary>
    public string Value = string.Empty;

    /// <summary>Upper bound for Between (number/date range).</summary>
    public string ValueEnd = string.Empty;

    /// <summary>Selected tokens for the In multi-select editor.</summary>
    public readonly HashSet<string> Multi = new();

    /// <summary>Date-range editor lower bound.</summary>
    public DateOnly? DateStart;

    /// <summary>Date-range editor upper bound.</summary>
    public DateOnly? DateEnd;
}
