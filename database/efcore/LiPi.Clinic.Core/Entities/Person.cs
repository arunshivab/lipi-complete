using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

/// <summary>
/// Shared base identity — patients, staff, kin, emergency contacts all reference a single person row.
/// </summary>
[Table("persons", Schema = "core")]
public class Person
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string? Title { get; set; }
    public string GivenName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string? FamilyName { get; set; }
    public string DisplayName { get; set; } = default!;      // GENERATED STORED column — read-only
    public string Gender { get; set; } = default!;           // male | female | other | unknown
    public DateOnly? DateOfBirth { get; set; }
    public string? BloodGroup { get; set; }                  // A+ A- B+ B- AB+ AB- O+ O- unknown
    public string? MaritalStatus { get; set; }
    public string NationalityCode { get; set; } = "IN";
    public string PreferredLanguage { get; set; } = "en";
    public string? PhotoS3Key { get; set; }
    public string ExtensionData { get; set; } = "{}";        // JSONB

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public ICollection<ContactPoint> ContactPoints { get; set; } = new List<ContactPoint>();
    public ICollection<PersonAddress> Addresses { get; set; } = new List<PersonAddress>();
    public Patient? Patient { get; set; }
    public Staff? Staff { get; set; }
}

[Table("contact_points", Schema = "core")]
public class ContactPoint
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PersonId { get; set; }
    public string System { get; set; } = default!;           // phone | email | fax | url | sms | whatsapp
    public string Value { get; set; } = default!;
    public string? UseType { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Person Person { get; set; } = default!;
}

[Table("person_addresses", Schema = "core")]
public class PersonAddress
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PersonId { get; set; }
    public Guid AddressId { get; set; }
    public bool IsPrimary { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }

    public Person Person { get; set; } = default!;
    public Address Address { get; set; } = default!;
}
