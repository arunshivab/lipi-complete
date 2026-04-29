using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Identity.Entities;

/// <summary>
/// Per-clinic security policy — one row per clinic database.
/// Stored in the clinic's own identity schema.
/// </summary>
[Table("security_policy", Schema = "identity")]
public class SecurityPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Password Policy ──────────────────────────────────────────
    public int  MinLength          { get; set; } = 8;
    public bool RequireUppercase   { get; set; } = true;
    public bool RequireLowercase   { get; set; } = true;
    public bool RequireDigit       { get; set; } = true;
    public bool RequireSymbol      { get; set; } = true;
    public int  ExpiryDays         { get; set; } = 30;   // 0 = never
    public int  HistoryCount       { get; set; } = 0;    // 0 = allow reuse
    public int  MaxFailedAttempts  { get; set; } = 5;
    public int  LockoutMinutes     { get; set; } = 30;

    // ── Session Policy ───────────────────────────────────────────
    public int  IdleTimeoutMinutes  { get; set; } = 60;
    public int  SessionMaxHours     { get; set; } = 10;
    public bool EnforceSingleSession{ get; set; } = true;

    // ── MFA Policy ───────────────────────────────────────────────
    public bool   MfaRequired       { get; set; } = false;
    public string AllowedMfaMethods { get; set; } = string.Empty; // CSV: totp,sms,email
    public int    MfaGraceDays      { get; set; } = 0;

    public DateTimeOffset UpdatedAt  { get; set; } = DateTimeOffset.UtcNow;
    public string?        UpdatedBy  { get; set; }
}
