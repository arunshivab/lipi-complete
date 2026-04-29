using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

/// <summary>
/// Platform-level user — identity data stored ONCE in master DB.
/// Valid across all clinics. Auth always checks here first.
/// Replaces the per-clinic identity.users for credential management.
/// </summary>
[Table("platform_users", Schema = "master")]
public class PlatformUser
{
    public Guid   Id       { get; set; }
    public string Username { get; set; } = default!;
    public string? PasswordHash { get; set; }         // Argon2id

    // ── Identity ──────────────────────────────────────────────────────────
    public string  FirstName   { get; set; } = default!;
    public string? MiddleName  { get; set; }
    public string  LastName    { get; set; } = default!;
    public string  DisplayName { get; set; } = default!;
    public string? Title       { get; set; }           // Dr. / Mr. / Mrs. / Ms.
    public string? Gender      { get; set; }
    public string? DateOfBirth { get; set; }           // yyyy-MM-dd
    public string? BloodGroup  { get; set; }
    public string? Nationality { get; set; } = "Indian";

    // ── Contact ───────────────────────────────────────────────────────────
    public string  Email            { get; set; } = default!;
    public string? Phone            { get; set; }
    public string? PhoneCountryCode { get; set; } = "+91";

    // ── Professional identity (belongs to person, not clinic) ─────────────
    public string? NmcRegNumber  { get; set; }
    public string? AerbRpNumber  { get; set; }

    // ── Media + Qualifications (stored as JSONB) ──────────────────────────
    // Contains: photoData, signatureData, qualifications[], staffType
    public string ExtensionData { get; set; } = "{}";

    // ── User type (platform-level) ────────────────────────────────────────
    public string UserType { get; set; } = "staff";
    // global_admin | sys_admin | site_admin | staff

    // ── Auth / Security ───────────────────────────────────────────────────
    public string   Status             { get; set; } = "active";
    public bool     MustChangePassword { get; set; } = true;
    public bool     IsMfaEnforced      { get; set; } = false;
    public int      FailedLoginCount   { get; set; }
    public DateTime? LockedUntil       { get; set; }
    public DateTime? LastLoginAt       { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid?    CreatedBy  { get; set; }
    public int      RowVersion { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────
    public ICollection<ClinicMembership> ClinicMemberships { get; set; }
        = new List<ClinicMembership>();
}

/// <summary>
/// Maps a platform user to a clinic. One record per user-clinic pair.
/// Status is per-clinic — a user can be active in Chennai but suspended in Mumbai.
/// </summary>
[Table("clinic_memberships", Schema = "master")]
public class ClinicMembership
{
    public Guid   Id             { get; set; }
    public Guid   PlatformUserId { get; set; }
    public Guid   ClinicId       { get; set; }
    public string Status         { get; set; } = "active"; // active | suspended | terminated
    public DateTime CreatedAt    { get; set; }
    public Guid?  CreatedBy      { get; set; }
    public DateTime? SuspendedAt   { get; set; }
    public DateTime? TerminatedAt  { get; set; }
    public DateTime? UpdatedAt     { get; set; }

    public PlatformUser PlatformUser { get; set; } = default!;
    public Clinic       Clinic       { get; set; } = default!;
}
