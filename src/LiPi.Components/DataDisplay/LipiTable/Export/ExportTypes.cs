// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §17.2 (ExportOptions + ExportAsync API)
//   §17.3 (ExportScope)
//   §17.3.4 (ServerSideAllCap interaction with ExportScope.All)
//   §17.5 (Print = browser-native; PDF stubbed until Phase 2.10)
//   §17.8.2 (ExportProgress)
//   §17.9 (BeforeExportContext + AfterExportContext)
//   §27.7 (Export/ExportTypes.cs file taxonomy)
// PHASE: 2.8 Data Display — Stage 1A (types foundation — additive deploy entry)
// COMPONENT: LipiTable export
//
// Public export-type surface. These types form the contract between LipiTable and its
// callers for the export feature. The actual exporter classes (CsvExporter, etc.) ship
// in Stage 6. The PDF code path will be stubbed in Stage 6 (throws
// "PDF library pending Phase 2.10 integration") until the in-house LiPi PDF library lands.
//
// FILE-PLACEMENT NOTE: §28.4 deploy registry in the spec does not list this file in
// Stage 1. Stage 1A adds it because BeforeExportContext / AfterExportContext (referenced
// by the public event surface) carry ExportFormat; placing them in Contexts.cs without
// ExportFormat in scope would create a Stage-6-forward-declaration coupling. Co-locating
// the full export-type family in Export/ExportTypes.cs at Stage 1A matches §27.7 file
// taxonomy. Surfaced to strategic chat for confirmation; recorded in CHANGE-LOG A37.

using System;
using System.Collections.Generic;

namespace LiPi.Components.DataDisplay.Export;

/// <summary>
/// Export output format. v1.0 ships Csv + Print fully; Pdf is stubbed (real integration
/// in Phase 2.10 when the in-house LiPi PDF library ships); Excel is deferred to a future
/// amendment (CSV is the bridge — every spreadsheet app opens CSV). Per §17.5 / Phase 2.8
/// Overview §3.
/// </summary>
public enum ExportFormat
{
    Csv,
    Pdf,
    Print
}

/// <summary>
/// Which rows the export includes. Filtered is the default (DefaultExportScope per §17.3.3).
/// In server-side mode, ExportScope.All is capped at ServerSideAllCap (default 1000)
/// per §17.3.4.
/// </summary>
public enum ExportScope
{
    /// <summary>Current view: filtered + sorted + paged (what's on screen).</summary>
    View,
    /// <summary>Filtered + sorted, all pages — default.</summary>
    Filtered,
    /// <summary>All underlying data, filters bypassed. Capped server-side per §17.3.4.</summary>
    All,
    /// <summary>Only rows in SelectedItems / SelectedKeys.</summary>
    Selected
}

/// <summary>
/// Caller-supplied options for programmatic export via ExportAsync(ExportOptions) and
/// the "More options" modal. Per §17.2.
/// </summary>
public sealed class ExportOptions
{
    /// <summary>Output format. Required.</summary>
    public ExportFormat Format { get; init; }

    /// <summary>Which rows to include.</summary>
    public ExportScope Scope { get; init; }

    /// <summary>Columns to include. Null means "all visible columns with IncludeInExport=true".</summary>
    public IReadOnlyList<string>? ColumnKeys { get; init; }

    /// <summary>Output filename without extension. Null means auto-generated from TableId + timestamp.</summary>
    public string? Filename { get; init; }

    /// <summary>Include header row. Default true.</summary>
    public bool IncludeHeader { get; init; } = true;

    /// <summary>Include aggregate footer row. Default false.</summary>
    public bool IncludeAggregateFooter { get; init; }

    /// <summary>For CSV: include UTF-8 BOM. Default true (improves Excel compatibility).</summary>
    public bool IncludeBom { get; init; } = true;

    /// <summary>
    /// Format-specific options. Keys vary by Format. For Print: "Orientation" ∈ {"Portrait",
    /// "Landscape", "Auto"}; "FontScale" ∈ [0.8, 1.2]. For CSV: "Delimiter" (default ",").
    /// Per §17.7.5.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? FormatOptions { get; init; }
}

/// <summary>
/// Progress update fired during long-running export via OnExportProgress. Percentage may
/// be -1 to indicate indeterminate progress (server hasn't sent intermediate updates).
/// Stage describes the current phase ("Fetching data", "Generating CSV", etc.). Per §17.8.2.
/// </summary>
public sealed record ExportProgress(
    int RowsProcessed,
    int? TotalRows,
    string? Stage,
    double Percentage);

/// <summary>
/// Fires after the user confirms export (modal OK or direct click) but BEFORE row
/// generation. Setting Cancel=true aborts the export — used for background-job hand-off
/// per §17.9.3 ("export this large dataset will run in background; we'll email you
/// when ready"). The Options record is the resolved options after the user's modal
/// interactions; the caller can read it for audit purposes. Per §17.9.
/// </summary>
public sealed record BeforeExportContext(
    ExportFormat Format,
    ExportScope Scope,
    IReadOnlyList<string> ColumnKeys,
    ExportOptions Options)
{
    /// <summary>Set to true to cancel the export.</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Fires after an export completes — success, failure, or user cancellation. Succeeded
/// is false for failure or cancellation; ErrorMessage carries the reason. Used by
/// consuming pages for HIPAA PHI audit logging (per §17.4 / Q-15 audit queue item).
/// Per §17.9.
/// </summary>
public sealed record AfterExportContext(
    ExportFormat Format,
    int RowsExported,
    long BytesGenerated,
    TimeSpan Duration,
    bool Succeeded,
    string? ErrorMessage);
