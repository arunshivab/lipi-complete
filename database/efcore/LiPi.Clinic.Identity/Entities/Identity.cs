using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Identity.Entities;

/// <summary>
/// Role definition for clinic users.
/// </summary>
public class Role
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Code { get; set; } = string.Empty;  // e.g., 'oncologist', 'opd_nurse'
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RowVersion { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// System-wide permission definition.
/// </summary>
public class Permission
{
    public Guid Id { get; set; }
    public string PermissionCode { get; set; } = string.Empty;  // e.g., 'opd.encounter.create'
    public string Module { get; set; } = string.Empty;  // 'opd', 'ipd', 'rtqa', etc.
    public string Action { get; set; } = string.Empty;  // 'read', 'create', 'update', 'delete', etc.
    public string? Description { get; set; }
    public bool IsPhiSensitive { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Junction table: roles have permissions.
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime GrantedAt { get; set; }
    public Guid? GrantedBy { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

/// <summary>
/// User-role assignment, optionally scoped to a department.
/// </summary>
public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? ScopeDepartmentId { get; set; }  // NULL = global role
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

/// <summary>
/// Active user session (JWT + refresh token state).
/// </summary>
public class Session
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }
    public byte[] RefreshTokenHash { get; set; } = Array.Empty<byte>();
    public Guid JwtJti { get; set; }

    public System.Net.IPAddress? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceFingerprint { get; set; }

    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }

    public virtual User User { get; set; } = null!;
}

/// <summary>
/// MFA methods: TOTP, WebAuthn, SMS OTP, email OTP.
/// </summary>
public class MfaMethod
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid UserId { get; set; }

    public string MethodType { get; set; } = string.Empty;  // totp, webauthn, sms_otp, email_otp, backup_codes
    public string? Label { get; set; }  // e.g., 'iPhone 15'

    public byte[]? SecretEncrypted { get; set; }  // KMS envelope-encrypted
    public byte[]? WebauthnCredentialId { get; set; }
    public byte[]? WebauthnPublicKey { get; set; }
    public long? WebauthnCounter { get; set; }

    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;
}

/// <summary>
/// Password history tracking.
/// </summary>
public class PasswordHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime SetAt { get; set; }

    public virtual User User { get; set; } = null!;
}

/// <summary>
/// Login attempt tracking (successful + failed).
/// </summary>
public class LoginAttempt
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AttemptedAt { get; set; }

    public string? Username { get; set; }
    public Guid? UserId { get; set; }

    public System.Net.IPAddress? ClientIp { get; set; }
    public string? UserAgent { get; set; }

    public string? AuthMethod { get; set; }  // password, ad, saml, oidc, api_key, service, mfa
    public string Outcome { get; set; } = string.Empty;  // success, bad_password, unknown_user, mfa_failed, locked, disabled, expired, rate_limited, suspicious

    public string? FailureDetail { get; set; }
    public string? GeoCountry { get; set; }
    public string? GeoCity { get; set; }

    public virtual User? User { get; set; }
}

/// <summary>
/// Service account for integrations (lab analyser, modality bridge, etc.).
/// </summary>
public class ServiceAccount
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? UserId { get; set; }  // backed by a user row

    public string Purpose { get; set; } = string.Empty;  // modality_bridge, lab_analyser, etc.
    public List<string> AllowedIpCidrs { get; set; } = new();

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}

/// <summary>
/// API key for service account authentication.
/// </summary>
public class ApiKey
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid ServiceAccountId { get; set; }

    public string KeyPrefix { get; set; } = string.Empty;  // first 8 chars for display
    public byte[] KeyHash { get; set; } = Array.Empty<byte>();  // SHA-256

    public List<string> Scopes { get; set; } = new();  // permission codes

    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public System.Net.IPAddress? LastUsedIp { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public virtual ServiceAccount ServiceAccount { get; set; } = null!;
}

/// <summary>
/// AD/LDAP synchronization run tracking.
/// </summary>
public class AdSyncRun
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid IdentityProviderId { get; set; }  // FK to master.identity_providers

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string Status { get; set; } = string.Empty;  // running, success, failed, partial

    public int UsersCreated { get; set; }
    public int UsersUpdated { get; set; }
    public int UsersDisabled { get; set; }
    public int UsersUnchanged { get; set; }
    public int ErrorCount { get; set; }

    // Stored as jsonb string — avoids Npgsql 8+ dynamic-JSON opt-in requirement.
    public string ErrorDetails { get; set; } = "[]";
    public string? HighWaterMark { get; set; }  // last processed uSNChanged
}
