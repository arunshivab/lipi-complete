using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Identity.Entities;

/// <summary>
/// Clinic-scoped user identity. May be bound to a staff member or a service account.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }

    public string Username { get; set; } = string.Empty;  // citext
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public Guid? StaffId { get; set; }  // FK to core.staff
    public string? PasswordHash { get; set; }  // Argon2id
    public DateTime? PasswordSetAt { get; set; }

    public bool MustChangePassword { get; set; }

    public string Status { get; set; } = "active";  // invited, active, suspended, locked, terminated

    public bool IsAdManaged { get; set; }
    public string? AdObjectGuid { get; set; }
    public string? AdDn { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public System.Net.IPAddress? LastLoginIp { get; set; }

    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }

    public bool IsMfaEnforced { get; set; } = true;

    public string PreferredLocale { get; set; } = "en-IN";
    public string Timezone { get; set; } = "Asia/Kolkata";

    // Stored as jsonb string — avoids Npgsql 8 dynamic-JSON opt-in requirement.
    public string ExtensionData { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    // Navigation
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<MfaMethod> MfaMethods { get; set; } = new List<MfaMethod>();
    public virtual ICollection<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();
    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
}
