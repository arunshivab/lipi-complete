using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

[Table("feature_flags", Schema = "master")]
public class FeatureFlag
{
    public Guid Id { get; set; }
    public string FlagKey { get; set; } = default!;
    public string? Description { get; set; }
    public bool DefaultEnabled { get; set; }
    public string RolloutRules { get; set; } = "{}";         // JSONB
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<FeatureFlagOverride> Overrides { get; set; } = new List<FeatureFlagOverride>();
}

[Table("feature_flag_overrides", Schema = "master")]
public class FeatureFlagOverride
{
    public Guid Id { get; set; }
    public Guid FeatureFlagId { get; set; }
    public string ScopeType { get; set; } = default!;        // organization | clinic | user
    public Guid ScopeId { get; set; }
    public bool Enabled { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public FeatureFlag FeatureFlag { get; set; } = default!;
}

[Table("audit_events", Schema = "master")]
public class MasterAuditEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset EventTs { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorIp { get; set; }
    public string? ActorUserAgent { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public Guid? EntityId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? ClinicId { get; set; }
    public string? BeforeState { get; set; }                 // JSONB
    public string? AfterState { get; set; }                  // JSONB
    public Guid? RequestId { get; set; }
    public byte[]? PreviousHash { get; set; }
    public byte[] CurrentHash { get; set; } = default!;
}
