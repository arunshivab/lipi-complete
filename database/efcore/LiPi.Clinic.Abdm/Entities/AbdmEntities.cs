using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Abdm.Entities;

public class FacilityRegistry
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid FacilityId { get; set; }
    public string HfrFacilityId { get; set; } = string.Empty;
    public string HfrFacilityName { get; set; } = string.Empty;
    public string? Ownership { get; set; }
    public string HfrStatus { get; set; } = string.Empty;
    public DateTime? RegisteredAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public Dictionary<string, object> RegistryPayload { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RowVersion { get; set; }
}

public class ProfessionalRegistry
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid StaffId { get; set; }
    public string HprId { get; set; } = string.Empty;
    public string HprName { get; set; } = string.Empty;
    public string? CouncilName { get; set; }
    public string? CouncilRegNo { get; set; }
    public string? Qualification { get; set; }
    public string HprStatus { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public Dictionary<string, object> RegistryPayload { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RowVersion { get; set; }
}

public class AbhaProfile
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string AbhaNumber { get; set; } = string.Empty;
    public string AbhaAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool MobileVerified { get; set; }
    public bool EmailVerified { get; set; }
    public bool KycVerified { get; set; }
    public string? KycMethod { get; set; }
    public string? AbhaQrS3Key { get; set; }
    public Dictionary<string, object> ProfilePayload { get; set; } = new();
    public DateTime LinkedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RowVersion { get; set; }

    public virtual ICollection<LinkRequest> LinkRequests { get; set; } = new List<LinkRequest>();
    public virtual ICollection<CareContext> CareContexts { get; set; } = new List<CareContext>();
    public virtual ICollection<ConsentRequest> ConsentRequests { get; set; } = new List<ConsentRequest>();
}

public class LinkRequest
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AbhaProfileId { get; set; }
    public Guid PatientId { get; set; }
    public string LinkToken { get; set; } = string.Empty;
    public string LinkType { get; set; } = string.Empty;
    public string AuthMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? LinkedAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDetail { get; set; }

    public virtual AbhaProfile AbhaProfile { get; set; } = null!;
}

public class CareContext
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AbhaProfileId { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public string ContextType { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; }
    public DateTime? UnlinkedAt { get; set; }
    public bool Visible { get; set; } = true;

    public virtual AbhaProfile AbhaProfile { get; set; } = null!;
}

public class ConsentRequest
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AbhaProfileId { get; set; }
    public string RequesterType { get; set; } = string.Empty;
    public string? ConsentManagerRequestId { get; set; }
    public string PurposeCode { get; set; } = string.Empty;
    public string? PurposeText { get; set; }
    public List<string> HiTypes { get; set; } = new();
    public DateTime DateRangeFrom { get; set; }
    public DateTime DateRangeTo { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string FetchMode { get; set; } = string.Empty;
    public string? FrequencyUnit { get; set; }
    public int? FrequencyValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RaisedAt { get; set; }
    public DateTime? GrantedAt { get; set; }
    public DateTime? DeniedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Dictionary<string, object> ConsentPayload { get; set; } = new();

    public virtual AbhaProfile AbhaProfile { get; set; } = null!;
    public virtual ICollection<ConsentArtefact> ConsentArtefacts { get; set; } = new List<ConsentArtefact>();
}

public class ConsentArtefact
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid ConsentRequestId { get; set; }
    public string ArtefactId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public Dictionary<string, object> ArtefactPayload { get; set; } = new();
    public DateTime IssuedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }

    public virtual ConsentRequest ConsentRequest { get; set; } = null!;
    public virtual ICollection<HiExchangeSession> HiExchangeSessions { get; set; } = new List<HiExchangeSession>();
}

public class HiExchangeSession
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid ConsentArtefactId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public Dictionary<string, object> KeyMaterial { get; set; } = new();
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ErrorDetail { get; set; }

    public virtual ConsentArtefact ConsentArtefact { get; set; } = null!;
    public virtual ICollection<HiCareBundleLog> HiCareBundles { get; set; } = new List<HiCareBundleLog>();
}

public class HiCareBundleLog
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid HiExchangeSessionId { get; set; }
    public string CareContextRef { get; set; } = string.Empty;
    public byte[] BundleSha256 { get; set; } = Array.Empty<byte>();
    public string BundleS3Key { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    public virtual HiExchangeSession HiExchangeSession { get; set; } = null!;
}

public class GatewayRequest
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime RequestedAt { get; set; }
    public string ApiPath { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Dictionary<string, object>? RequestBody { get; set; }
    public int? ResponseStatus { get; set; }
    public Dictionary<string, object>? ResponseBody { get; set; }
    public int? DurationMs { get; set; }
    public string Outcome { get; set; } = string.Empty;
}

public class AadhaarEkycRequest
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? StaffId { get; set; }
    public byte[] AadhaarReferenceHash { get; set; } = Array.Empty<byte>();
    public string TxnId { get; set; } = string.Empty;
    public string AuthMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public byte[]? EkycPayloadEncrypted { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDetail { get; set; }
}

public class SubscriptionCallback
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AbhaProfileId { get; set; }
    public string SubscriptionId { get; set; } = string.Empty;
    public string SubscriberHiuId { get; set; } = string.Empty;
    public List<string> HiTypes { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual AbhaProfile AbhaProfile { get; set; } = null!;
}
