using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

/// <summary>
/// Legal entity that owns one or more clinics.
/// Maps to <c>master.organizations</c>.
/// </summary>
[Table("organizations", Schema = "master")]
public class Organization
{
    public Guid Id { get; set; }
    public string LegalName { get; set; } = default!;
    public string? TradingName { get; set; }
    public string OrgType { get; set; } = default!;              // single_clinic | hospital_chain | clinic_group | government | academic
    public string CountryCode { get; set; } = "IN";
    public string? Pan { get; set; }
    public string? Gstin { get; set; }
    public string? Cin { get; set; }
    public string RegisteredAddress { get; set; } = "{}";        // JSONB
    public string PrimaryContact { get; set; } = "{}";           // JSONB
    public string Status { get; set; } = "active";
    public DateTimeOffset? OnboardedAt { get; set; }
    public string ExtensionData { get; set; } = "{}";            // JSONB

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    public ICollection<ClinicGroup> ClinicGroups { get; set; } = new List<ClinicGroup>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<IdentityProvider> IdentityProviders { get; set; } = new List<IdentityProvider>();
}
