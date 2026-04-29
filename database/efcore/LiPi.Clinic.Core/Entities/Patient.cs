using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

[Table("patients", Schema = "core")]
public class Patient
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PersonId { get; set; }
    public string Mrn { get; set; } = default!;              // Medical Record Number, clinic-local
    public string? Uhid { get; set; }                        // Org-level universal health id
    public DateTimeOffset RegistrationDate { get; set; }
    public Guid? RegisteredBy { get; set; }                  // staff_id
    public bool VipFlag { get; set; }
    public bool Deceased { get; set; }
    public DateTimeOffset? DeceasedDatetime { get; set; }
    public string? DeathCauseIcd10 { get; set; }
    public bool OrganDonor { get; set; }
    public Guid? PrimaryDoctorId { get; set; }               // staff_id
    public string? Occupation { get; set; }
    public string? ReferralSource { get; set; }
    public string PatientType { get; set; } = "general";     // general | vip | international | staff | cgs | ecs | charity | research
    public string ActiveAlerts { get; set; } = "[]";         // JSONB
    public string ExtensionData { get; set; } = "{}";        // JSONB

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public Person Person { get; set; } = default!;
    public Icd10Code? DeathCauseCode { get; set; }
    public Staff? RegisteredByStaff { get; set; }
    public Staff? PrimaryDoctor { get; set; }

    public ICollection<PatientIdentifier> Identifiers { get; set; } = new List<PatientIdentifier>();
    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
    public ICollection<Consent> Consents { get; set; } = new List<Consent>();
}

[Table("patient_identifiers", Schema = "core")]
public class PatientIdentifier
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string IdentifierType { get; set; } = default!;   // abha_number | abha_address | aadhaar | pan | passport | ...
    public string IdentifierValue { get; set; } = default!;
    public string? IssuingAuthority { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerificationRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Patient Patient { get; set; } = default!;
}

[Table("emergency_contacts", Schema = "core")]
public class EmergencyContact
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = default!;
    public string Relationship { get; set; } = default!;     // spouse | parent | child | sibling | guardian | friend | other
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
    public string? AddressJson { get; set; }                 // JSONB column 'address'
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Patient Patient { get; set; } = default!;
}
