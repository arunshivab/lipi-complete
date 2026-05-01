using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

// NOTE: Person entity REMOVED — merged into Patient (collapsed schema v3).
// PersonAddress entity REMOVED — replaced by Address table.

[Table("contact_points", Schema = "core")]
public class ContactPoint
{
    public Guid    Id               { get; set; }
    public Guid    EntityId         { get; set; }       // this record's stable UUID
    public Guid    PatientEntityId  { get; set; }       // references Patient.EntityId
    public Guid?   PreviousId       { get; set; }
    public DateTimeOffset  ValidFrom { get; set; }
    public DateTimeOffset? ValidTo   { get; set; }
    public Guid?   ChangedBy        { get; set; }
    public string? ChangeReason     { get; set; }
    public Guid    ClinicId         { get; set; }
    public string  System           { get; set; } = default!;  // phone|email|fax|url|sms|whatsapp
    public string  Value            { get; set; } = default!;
    public string? UseType          { get; set; }              // home|work|mobile|temp|old
    public bool    IsPrimary        { get; set; }
    public bool    IsVerified       { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

[Table("addresses", Schema = "core")]
public class Address
{
    public Guid    Id               { get; set; }
    public Guid    EntityId         { get; set; }
    public Guid    PatientEntityId  { get; set; }
    public Guid?   PreviousId       { get; set; }
    public DateTimeOffset  ValidFrom { get; set; }
    public DateTimeOffset? ValidTo   { get; set; }
    public Guid?   ChangedBy        { get; set; }
    public string? ChangeReason     { get; set; }
    public Guid    ClinicId         { get; set; }
    public string  AddressType      { get; set; } = "current";  // current|permanent|work|temporary
    public string  Line1            { get; set; } = default!;
    public string? Line2            { get; set; }
    public string? District         { get; set; }
    public string  City             { get; set; } = default!;
    public string  State            { get; set; } = default!;
    public string? Pincode          { get; set; }
    public string  CountryCode      { get; set; } = "IN";
    public bool    IsAspirational   { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
