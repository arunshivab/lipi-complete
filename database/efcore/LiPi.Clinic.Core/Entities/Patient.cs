using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

/// <summary>
/// Collapsed patient record — identity + clinical registration in one table.
/// IMMUTABLE / APPEND-ONLY: no UPDATE or DELETE ever.
/// entity_id = stable patient UUID (used in all FK references).
/// id        = this version UUID (changes on every edit).
/// valid_to IS NULL = current version.
/// </summary>
[Table("patients", Schema = "core")]
public class Patient
{
    // ── Version identity ─────────────────────────────────────────────
    public Guid   Id            { get; set; }
    public Guid   EntityId      { get; set; }
    public Guid?  PreviousId    { get; set; }
    public DateTimeOffset  ValidFrom   { get; set; }
    public DateTimeOffset? ValidTo     { get; set; }
    public Guid?  ChangedBy     { get; set; }
    public string? ChangeReason { get; set; }

    // ── Identity ─────────────────────────────────────────────────────
    public Guid    ClinicId          { get; set; }
    public string? Title             { get; set; }
    public string  FirstName         { get; set; } = default!;
    public string? MiddleName        { get; set; }
    public string  LastName          { get; set; } = default!;
    public string  DisplayName       { get; set; } = default!;  // GENERATED STORED — read-only
    public string  Gender            { get; set; } = default!;
    public DateOnly DateOfBirth      { get; set; }
    public string  DobConfidence     { get; set; } = "self_reported";
    public string? BloodGroup        { get; set; }
    public string? MaritalStatus     { get; set; }
    public string? NationalityCode   { get; set; } = "IN";
    public string? PreferredLanguage { get; set; } = "en";
    public string? PhotoS3Key        { get; set; }

    // ── Clinical registration ─────────────────────────────────────────
    public string  Uhid              { get; set; } = default!;
    public string? Mrn               { get; set; }
    public DateTimeOffset RegistrationDate { get; set; }
    public string  PatientType       { get; set; } = "general";
    public bool    Deceased          { get; set; }
    public DateTimeOffset? DeceasedAt { get; set; }
    public string? DeathCauseIcd10   { get; set; }
    public string? Occupation        { get; set; }
    public string? ReferralSource    { get; set; }
    public string? ReferralChannel   { get; set; }
    public string? ReferredBy        { get; set; }
    public Guid?   RegisteredBy      { get; set; }
    public string  ExtensionData     { get; set; } = "{}";

    // ── Audit ─────────────────────────────────────────────────────────
    public DateTimeOffset CreatedAt  { get; set; }
    public DateTimeOffset UpdatedAt  { get; set; }

    // ── Navigations (dependents reference EntityId) ───────────────────
    public ICollection<ContactPoint>      ContactPoints     { get; set; } = new List<ContactPoint>();
    public ICollection<Address>           Addresses         { get; set; } = new List<Address>();
    public ICollection<PatientIdentifier> Identifiers       { get; set; } = new List<PatientIdentifier>();
    public ICollection<PatientPayer>      Payers            { get; set; } = new List<PatientPayer>();
    public ICollection<EmergencyContact>  EmergencyContacts { get; set; } = new List<EmergencyContact>();
    public ICollection<PatientFlag>       Flags             { get; set; } = new List<PatientFlag>();
    public ICollection<Consent>           Consents          { get; set; } = new List<Consent>();
}

[Table("patient_identifiers", Schema = "core")]
public class PatientIdentifier
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
    public string  IdentifierType   { get; set; } = default!;
    public string  IdentifierValue  { get; set; } = default!;
    public string? IssuingAuthority { get; set; }
    public DateOnly? ValidFromDate  { get; set; }
    public DateOnly? ValidToDate    { get; set; }
    public bool    IsVerified       { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerificationRef  { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

[Table("patient_payers", Schema = "core")]
public class PatientPayer
{
    public Guid    Id              { get; set; }
    public Guid    EntityId        { get; set; }
    public Guid    PatientEntityId { get; set; }
    public Guid?   PreviousId      { get; set; }
    public DateTimeOffset  ValidFrom { get; set; }
    public DateTimeOffset? ValidTo   { get; set; }
    public Guid?   ChangedBy       { get; set; }
    public string? ChangeReason    { get; set; }
    public Guid    ClinicId        { get; set; }
    public string  PayerType       { get; set; } = "self";
    public string? PayerName       { get; set; }
    public string? PolicyNumber    { get; set; }
    public string? SchemeName      { get; set; }
    public DateOnly? CoverageStart { get; set; }
    public DateOnly? CoverageEnd   { get; set; }
    public bool    IsPrimary       { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public Guid?   CreatedBy       { get; set; }
}

[Table("emergency_contacts", Schema = "core")]
public class EmergencyContact
{
    public Guid    Id              { get; set; }
    public Guid    EntityId        { get; set; }
    public Guid    PatientEntityId { get; set; }
    public Guid?   PreviousId      { get; set; }
    public DateTimeOffset  ValidFrom { get; set; }
    public DateTimeOffset? ValidTo   { get; set; }
    public Guid?   ChangedBy       { get; set; }
    public string? ChangeReason    { get; set; }
    public Guid    ClinicId        { get; set; }
    public string  Name            { get; set; } = default!;
    public string  Relationship    { get; set; } = default!;
    public string  Phone           { get; set; } = default!;
    public string? Email           { get; set; }
    public bool    IsPrimary       { get; set; }
    public string? AddressJson     { get; set; }
    public string? Notes           { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

[Table("flag_definitions", Schema = "core")]
public class FlagDefinition
{
    public Guid    Id           { get; set; }
    public Guid    ClinicId     { get; set; }
    public string  Code         { get; set; } = default!;
    public string  Label        { get; set; } = default!;
    public string  ColorHex     { get; set; } = "#F59E0B";
    public string? Icon         { get; set; }
    public string? Description  { get; set; }
    public bool    RequiresNote { get; set; }
    public bool    IsActive     { get; set; } = true;
    public int     SortOrder    { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid?   CreatedBy    { get; set; }
}

[Table("patient_flags", Schema = "core")]
public class PatientFlag
{
    public Guid    Id              { get; set; }
    public Guid    ClinicId        { get; set; }
    public Guid    PatientEntityId { get; set; }
    public Guid    FlagId          { get; set; }
    public string? Note            { get; set; }
    public Guid    FlaggedBy       { get; set; }
    public DateTimeOffset FlaggedAt { get; set; }
    public Guid?   ClearedBy       { get; set; }
    public DateTimeOffset? ClearedAt { get; set; }
    public string? ClearedReason   { get; set; }
    public FlagDefinition Flag     { get; set; } = default!;
}
