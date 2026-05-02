using System.Security.Claims;
using System.Text.Json;
using LiPi.Clinic.Audit;
using LiPi.Clinic.Audit.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

/// <summary>
/// Writes HIPAA-required audit events for every user-facing action.
/// Every call writes to audit.audit_events with before/after state snapshots.
/// HIPAA §164.312(b) — Audit Controls.
/// </summary>
public interface IAuditService
{
    Task LogAsync(
        Guid clinicId,
        ClaimsPrincipal actor,
        string action,
        string entityType,
        Guid entityId,
        string entityLabel,
        object? beforeState = null,
        object? afterState  = null,
        string? reason      = null,
        string? clientIp    = null,
        string? userAgent   = null);
}

public class AuditService : IAuditService
{
    private readonly IDbContextFactory<AuditDbContext> _auditDbFactory;
    private readonly ILogger<AuditService>             _log;

    public AuditService(
        IDbContextFactory<AuditDbContext> auditDbFactory,
        ILogger<AuditService>             log)
    {
        _auditDbFactory = auditDbFactory;
        _log            = log;
    }

    public async Task LogAsync(
        Guid clinicId,
        ClaimsPrincipal actor,
        string action,
        string entityType,
        Guid entityId,
        string entityLabel,
        object? beforeState = null,
        object? afterState  = null,
        string? reason      = null,
        string? clientIp    = null,
        string? userAgent   = null)
    {
        try
        {
            var actorId       = Guid.TryParse(actor.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var aid) ? aid : (Guid?)null;
            var actorUsername = actor.Identity?.Name ?? actor.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
            var actorRoles    = actor.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            await using var db = await _auditDbFactory.CreateDbContextAsync();

            var evt = new AuditEvent
            {
                Id             = Guid.NewGuid(),
                ClinicId       = clinicId,
                EventTs        = DateTime.UtcNow,
                Action         = action,
                ActorUserId    = actorId,
                ActorUsername  = actorUsername,   // denormalised — survives user deletion
                ActorRoleCodes = actorRoles,
                ActorIp        = System.Net.IPAddress.TryParse(clientIp, out var ip) ? ip : null,
                ActorUserAgent = userAgent,
                EntityType     = entityType,
                EntityId       = entityId,
                ActionDetail   = entityLabel,
                BeforeState    = beforeState != null ? JsonSerializer.Serialize(beforeState) : null,
                AfterState     = afterState  != null ? JsonSerializer.Serialize(afterState)  : null,
                ChangeReason   = reason,
                Outcome        = "success",
                CurrentHash    = Array.Empty<byte>(), // hash chaining wired in later
            };

            db.AuditEvents.Add(evt);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // NEVER let audit failure crash the application — log and continue
            // HIPAA note: persistent audit failures must be alerted to security officer
            _log.LogError(ex, "AUDIT WRITE FAILED — action={Action} entity={EntityType}/{EntityId}",
                action, entityType, entityId);
        }
    }
}

/// <summary>Standard action codes for user management audit events.</summary>
public static class AuditActions
{
    public const string UserCreated        = "user.created";
    public const string UserContactUpdated = "user.contact_updated";
    public const string UserRolesChanged   = "user.roles_changed";
    public const string UserLocked         = "user.locked";
    public const string UserUnlocked       = "user.unlocked";
    public const string UserSuspended      = "user.suspended";
    public const string UserReactivated    = "user.reactivated";
    public const string UserTerminated     = "user.terminated";      // "deleted" — soft only
    public const string UserPasswordReset  = "user.password_reset";
    public const string UserSessionRevoked = "user.session_revoked";
	
	    // These are NOT PHI — no phi_access_log needed. audit_events only.

    /// <summary>
    /// User changed their own theme mode, density, font size, or language.
    /// Fired by UserPreferenceService for every write to identity.user_preferences.
    /// entityType = "user_preference", entityId = userId
    /// </summary>
    public const string UserThemeChanged  = "user.theme_changed";

    /// <summary>
    /// Admin changed a clinic's brand theme (master.clinics.brand_theme_id).
    /// Fired by ThemeContextService.SetBrandAsync.
    /// entityType = "clinic", entityId = clinicId
    /// Requires SiteAdmin+ — enforced in ThemeContextService before audit fires.
    /// </summary>
    public const string UserBrandChanged  = "user.brand_changed";
}
