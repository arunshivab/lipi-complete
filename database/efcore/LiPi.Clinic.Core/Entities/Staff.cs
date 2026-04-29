using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

[Table("staff", Schema = "core")]
public class Staff
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PersonId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string StaffType { get; set; } = default!;        // doctor | nurse | technician | pharmacist | radiographer | medical_physicist | dosimetrist | therapist | admin | support | manager | lab_tech
    public Guid? PrimarySpecialtyId { get; set; }
    public Guid? PrimaryDepartmentId { get; set; }
    public string? Qualification { get; set; }
    public string? MciRegistrationNo { get; set; }
    public string? HprId { get; set; }                       // ABDM Healthcare Professional Registry
    public DateOnly? JoiningDate { get; set; }
    public DateOnly? ExitDate { get; set; }
    public Guid? ReportingToId { get; set; }
    public bool OnCall { get; set; }
    public bool IsActive { get; set; } = true;
    public string ExtensionData { get; set; } = "{}";        // JSONB

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public Person Person { get; set; } = default!;
    public Specialty? PrimarySpecialty { get; set; }
    public Department? PrimaryDepartment { get; set; }
    public Staff? ReportingTo { get; set; }

    public ICollection<StaffDepartment> DepartmentMemberships { get; set; } = new List<StaffDepartment>();
}

[Table("staff_departments", Schema = "core")]
public class StaffDepartment
{
    public Guid StaffId { get; set; }
    public Guid DepartmentId { get; set; }
    public string? RoleInDept { get; set; }
    public bool IsPrimary { get; set; }

    public Staff Staff { get; set; } = default!;
    public Department Department { get; set; } = default!;
}

[Table("consents", Schema = "core")]
public class Consent
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string ConsentType { get; set; } = default!;      // treatment | surgery | anaesthesia | chemotherapy | radiation | research | data_sharing | abdm_link | hie_share | telemedicine | photography | autopsy | organ_donation | dpdp_processing
    public string Scope { get; set; } = "{}";                // JSONB — FHIR-style provisions
    public bool Granted { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
    public Guid? WitnessStaffId { get; set; }
    public string? SignatureS3Key { get; set; }
    public string LanguageUsed { get; set; } = "en";
    public string? AbdmConsentArtefactId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public int RowVersion { get; set; }

    public Patient Patient { get; set; } = default!;
    public Staff? Witness { get; set; }
}
