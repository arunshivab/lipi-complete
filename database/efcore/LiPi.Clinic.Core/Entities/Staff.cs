using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

// Staff entity removed — staff identity lives in master.platform_users.
// Staff references in patient records are plain UUIDs resolved from master DB at display time.

[Table("consents", Schema = "core")]
public class Consent
{
    public Guid    Id               { get; set; }
    public Guid    ClinicId         { get; set; }
    public Guid    PatientEntityId  { get; set; }
    public string  ConsentType      { get; set; } = default!;
    public string  Scope            { get; set; } = "{}";
    public bool    Granted          { get; set; }
    public DateTimeOffset  GrantedAt  { get; set; }
    public DateTimeOffset  ValidFrom  { get; set; }
    public DateTimeOffset? ValidTo    { get; set; }
    public DateTimeOffset? RevokedAt  { get; set; }
    public string? RevocationReason  { get; set; }
    public string? SignatureS3Key   { get; set; }
    public string  LanguageUsed     { get; set; } = "en";
    public string? WitnessName      { get; set; }
    public DateTimeOffset  CreatedAt { get; set; }
    public Guid?   CreatedBy        { get; set; }
}
