using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

/// <summary>
/// Individual clinic / hospital facility. Each clinic has its own OLTP database.
/// Maps to <c>master.clinics</c>.
/// </summary>
[Table("clinics", Schema = "master")]
public class Clinic
{
    public Guid Id { get; set; }
    public Guid? OrganizationId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ClinicType { get; set; } = default!;        // hospital | cancer_centre | diagnostic | day_care | clinic | polyclinic
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string CountryCode { get; set; } = "IN";
    public string Address { get; set; } = "{}";               // JSONB
    public string Timezone { get; set; } = "Asia/Kolkata";
    public int? BedCount { get; set; }
    public bool NabhAccredited { get; set; }
    public bool NablAccredited { get; set; }
    public bool HasOncology { get; set; }
    public bool HasRt { get; set; }
    public bool HasNuclearMed { get; set; }
    public string Status { get; set; } = "provisioning";      // provisioning | active | suspended | terminated

    // ── Clinic Configuration ────────────────────────────────────
    public bool   AadhaarMandatory           { get; set; } = false;
    public bool   AadhaarValidationMandatory { get; set; } = false;
    public string IcdVersion                 { get; set; } = "ICD-10"; // ICD-9M | ICD-10 | ICD-11
    public DateTimeOffset? GoLiveAt { get; set; }
    public string ExtensionData { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public Organization Organization { get; set; } = default!;
    public ClinicDatabase? Database { get; set; }
    public ICollection<ClinicGroupMember> GroupMemberships { get; set; } = new List<ClinicGroupMember>();
    public ICollection<GlobalUserClinicAccess> UserAccess { get; set; } = new List<GlobalUserClinicAccess>();
}

[Table("clinic_databases", Schema = "master")]
public class ClinicDatabase
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string  DbHost { get; set; } = default!;
    public int     DbPort { get; set; } = 5432;
    public string  DbName { get; set; } = default!;
    public string  DbSchemaVersion { get; set; } = default!;
    public string? DbUsername { get; set; }
    public string? EncryptedDbPassword { get; set; }
    public string? EncryptedConnectionString { get; set; }
    public string? ReadReplicaHost { get; set; }
    public string BackupPolicy { get; set; } = "{}";
    public string EncryptionKeyArn { get; set; } = default!;
    public bool FdwLinked { get; set; }
    public DateTimeOffset? LastHealthCheck { get; set; }
    public string? HealthStatus { get; set; }                 // green | yellow | red
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Clinic Clinic { get; set; } = default!;
}

[Table("clinic_groups", Schema = "master")]
public class ClinicGroup
{
    public Guid Id { get; set; }
    public Guid? OrganizationId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string GroupType { get; set; } = default!;         // regional | clinical | financial | reporting | custom
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public Organization Organization { get; set; } = default!;
    public ICollection<ClinicGroupMember> Members { get; set; } = new List<ClinicGroupMember>();
}

[Table("clinic_group_members", Schema = "master")]
public class ClinicGroupMember
{
    public Guid ClinicGroupId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTimeOffset AddedAt { get; set; }

    public ClinicGroup ClinicGroup { get; set; } = default!;
    public Clinic Clinic { get; set; } = default!;
}
