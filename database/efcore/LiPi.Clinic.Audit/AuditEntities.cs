using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Audit.Entities;

public class EventType
{
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public bool IsPhiAccess { get; set; }
    public string? Description { get; set; }
    public bool NabhRequired { get; set; }
    public bool DpdpRequired { get; set; }
}

public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime EventTs { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? ActorUserId { get; set; }
    public string? ActorUsername { get; set; }
    public List<string> ActorRoleCodes { get; set; } = new();
    public System.Net.IPAddress? ActorIp { get; set; }
    public string? ActorUserAgent { get; set; }
    public Guid? ActorSessionId { get; set; }
    public Guid? ActorServiceAcctId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? EntityParentId { get; set; }
    public string? ActionDetail { get; set; }
    public string? BeforeState { get; set; }  // jsonb — serialized with System.Text.Json
    public string? AfterState { get; set; }   // jsonb — serialized with System.Text.Json
    public string? ChangeReason { get; set; }
    public Guid? RequestId { get; set; }
    public Guid? CorrelationId { get; set; }
    public string Outcome { get; set; } = "success";
    public byte[]? PreviousHash { get; set; }
    public byte[] CurrentHash { get; set; } = Array.Empty<byte>();
}

public class HashChainTip
{
    public Guid ClinicId { get; set; }
    public byte[] TipHash { get; set; } = Array.Empty<byte>();
    public Guid TipEventId { get; set; }
    public DateTime TipEventTs { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PhiAccessLog
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AccessedAt { get; set; }
    public Guid? ActorUserId { get; set; }
    public List<string> ActorRoleCodes { get; set; } = new();
    public System.Net.IPAddress? ActorIp { get; set; }
    public Guid PatientId { get; set; }
    public string? MrnSnapshot { get; set; }
    public string AccessContext { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public Guid? ResourceId { get; set; }
    public string? PurposeOfUse { get; set; }
    public string? Justification { get; set; }
    public bool MinimumNecessaryPrincipleOk { get; set; } = true;
    public string? ConsentRef { get; set; }
    public Guid? RequestId { get; set; }
}

public class ExportLog
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime ExportedAt { get; set; }
    public Guid? ActorUserId { get; set; }
    public System.Net.IPAddress? ActorIp { get; set; }
    public string ExportType { get; set; } = string.Empty;
    public string ExportFormat { get; set; } = string.Empty;
    public int? RecordCount { get; set; }
    public List<Guid>? PatientIds { get; set; }
    public string? QueryParameters { get; set; }  // jsonb — serialized with System.Text.Json
    public string Destination { get; set; } = string.Empty;
    public string? DestinationDetail { get; set; }
    public byte[]? FileSha256 { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? DlpScanResult { get; set; }
    public string? ConsentRef { get; set; }
    public string? Justification { get; set; }
}

public class PrintLog
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime PrintedAt { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? PrinterName { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public Guid? DocumentRefId { get; set; }
    public Guid? PatientId { get; set; }
    public int Copies { get; set; } = 1;
    public bool WatermarkApplied { get; set; } = true;
    public System.Net.IPAddress? ClientIp { get; set; }
}

public class BlockchainAnchor
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string AnchorType { get; set; } = string.Empty;
    public DateTime WindowFrom { get; set; }
    public DateTime WindowTo { get; set; }
    public long RowCount { get; set; }
    public byte[] BatchMerkleRoot { get; set; } = Array.Empty<byte>();
    public string? LedgerTxId { get; set; }
    public long? LedgerBlockNo { get; set; }
    public DateTime AnchoredAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string Status { get; set; } = "pending";
}

public class HashChainVerificationRun
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime WindowFrom { get; set; }
    public DateTime WindowTo { get; set; }
    public long RowsChecked { get; set; }
    public int BreaksFound { get; set; }
    public Guid? FirstBreakId { get; set; }
    public string Outcome { get; set; } = string.Empty;
}
