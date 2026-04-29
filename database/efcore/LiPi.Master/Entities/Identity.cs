using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

/// <summary>
/// Cross-clinic users: platform staff, organization directors, group admins, external auditors.
/// Clinic-scoped users (doctors, nurses, techs) live in the per-clinic DB's <c>identity</c> schema.
/// </summary>
[Table("global_users", Schema = "master")]
public class GlobalUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string UserType { get; set; } = default!;         // platform_admin | platform_support | org_director | group_admin | auditor
    public Guid? OrganizationId { get; set; }
    public string? PasswordHash { get; set; }                // Argon2id; null if only-SSO
    public bool IsMfaEnabled { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int RowVersion { get; set; }

    public Organization? Organization { get; set; }
    public ICollection<GlobalUserClinicAccess> ClinicAccess { get; set; } = new List<GlobalUserClinicAccess>();
}

[Table("global_user_clinic_access", Schema = "master")]
public class GlobalUserClinicAccess
{
    public Guid GlobalUserId { get; set; }
    public Guid ClinicId { get; set; }
    public string AccessLevel { get; set; } = default!;      // read | report | admin
    public DateTimeOffset GrantedAt { get; set; }
    public Guid? GrantedBy { get; set; }

    public GlobalUser GlobalUser { get; set; } = default!;
    public Clinic Clinic { get; set; } = default!;
}

[Table("identity_providers", Schema = "master")]
public class IdentityProvider
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string IdpType { get; set; } = default!;          // ad_ldap | azure_ad | okta | saml | oidc | keycloak
    public string DisplayName { get; set; } = default!;
    public string? Domain { get; set; }
    public string Config { get; set; } = "{}";               // JSONB
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Organization Organization { get; set; } = default!;
    public ICollection<IdpGroupRoleMapping> GroupRoleMappings { get; set; } = new List<IdpGroupRoleMapping>();
}

[Table("idp_group_role_mappings", Schema = "master")]
public class IdpGroupRoleMapping
{
    public Guid Id { get; set; }
    public Guid IdentityProviderId { get; set; }
    public string AdGroupDn { get; set; } = default!;
    public string RoleCode { get; set; } = default!;
    public Guid? ClinicId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public IdentityProvider IdentityProvider { get; set; } = default!;
    public Clinic? Clinic { get; set; }
}
