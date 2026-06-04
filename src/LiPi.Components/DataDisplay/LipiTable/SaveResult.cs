// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §12.6.2 (ValidationResult + ValidationError + ValidationSeverity)
//   §12.9.4 (SaveResult + SaveOutcome)
//   §12.11 (Conflict outcome triggers conflict UX per §12.11)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: LipiTable
//
// Return shapes for caller-supplied save handlers (OnRowSave / OnCellSave) and per-row
// validators (EditValidator). SaveResult drives LipiTable's post-save state machine:
//   Success              → exit edit mode, update display
//   ValidationError      → stay in edit mode, render errors per §12.6.3
//   ConcurrencyConflict  → stay in edit mode, render conflict UX per §12.11
//   Error                → stay in edit mode, render generic LipiToast with Message
//
// If the caller's save handler throws instead of returning, LipiTable treats it as
// Outcome=Error with Message derived from the exception (§12.9.4). Callers SHOULD
// return SaveResult explicitly so error messages are user-friendly.

using System.Collections.Generic;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Outcome of a caller's save handler. Drives LipiTable's post-save state machine.
/// Per §12.9.4.
/// </summary>
public enum SaveOutcome
{
    Success,
    ValidationError,
    ConcurrencyConflict,
    Error
}

/// <summary>
/// Result returned from OnRowSave / OnCellSave handlers. Outcome drives LipiTable's
/// post-save behavior; the optional Message / Errors / Conflict properties carry detail
/// for the relevant outcome.
/// </summary>
public sealed class SaveResult
{
    /// <summary>Outcome category. Required.</summary>
    public SaveOutcome Outcome { get; init; }

    /// <summary>User-facing message. Used for Error outcomes; optional for ValidationError.</summary>
    public string? Message { get; init; }

    /// <summary>Validation errors. Populated when Outcome is ValidationError.</summary>
    public IReadOnlyList<ValidationError>? Errors { get; init; }

    /// <summary>Conflict descriptor. Populated when Outcome is ConcurrencyConflict.</summary>
    public ConflictInfo? Conflict { get; init; }
}

/// <summary>Severity of a validation finding. Per §12.6.2.</summary>
public enum ValidationSeverity
{
    Error,
    Warning
}

/// <summary>
/// One validation finding from an EditValidator. Error-severity findings block save;
/// Warning-severity findings display alongside but allow save to proceed (caller's
/// choice). FieldKey is the column key of the offending field, or null for row-level
/// rules that don't pin to a single field. Per §12.6.2.
/// </summary>
public sealed record ValidationError(
    string Message,
    string? FieldKey,
    ValidationSeverity Severity);

/// <summary>
/// Aggregate validation result returned from a per-row EditValidator. IsValid is true
/// when no Error-severity findings exist (Warnings do not affect this flag). Per §12.6.2.
/// </summary>
public sealed class ValidationResult
{
    public ValidationResult(bool isValid, IReadOnlyList<ValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>True when no Error-severity findings exist. Warnings do not affect this flag.</summary>
    public bool IsValid { get; }

    /// <summary>All findings, including warnings.</summary>
    public IReadOnlyList<ValidationError> Errors { get; }
}
