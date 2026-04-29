using LiPi.Clinic.Core;
using LiPi.Clinic.Identity;
using LiPi.Master;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

/// <summary>
/// Creates IdentityDbContext instances scoped to a specific clinic's database.
/// User discovery now uses master.platform_users + clinic_memberships.
/// IdentityDbContext is used for: sessions, roles, login_attempts, clinic_profiles.
/// </summary>
public class ClinicDbFactory
{
    private readonly ClinicConnectionService            _connSvc;
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ILogger<ClinicDbFactory>           _log;
    private readonly IConfiguration                     _config;

    public ClinicDbFactory(
        ClinicConnectionService            connSvc,
        IDbContextFactory<MasterDbContext> masterFactory,
        ILogger<ClinicDbFactory>           log,
        IConfiguration                     config)
    {
        _connSvc       = connSvc;
        _masterFactory = masterFactory;
        _log           = log;
        _config        = config;
    }

    /// <summary>Create IdentityDbContext for a specific clinic.</summary>
    public async Task<IdentityDbContext?> CreateForClinicAsync(Guid clinicId)
    {
        var connStr = await _connSvc.GetConnectionStringAsync(clinicId);

        if (string.IsNullOrEmpty(connStr))
        {
            connStr = _config.GetConnectionString("IdentityConnection");
            _log.LogWarning("No per-clinic connection for {ClinicId} — using IdentityConnection", clinicId);
            if (string.IsNullOrEmpty(connStr)) return null;
        }

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connStr)
            .Options;

        return new IdentityDbContext(options);
    }

    /// <summary>
    /// Get all clinics a user has access to.
    /// Uses master.platform_users + clinic_memberships — no identity.users needed.
    /// </summary>
    /// <summary>Create ClinicCoreDbContext for a specific clinic (core schema).</summary>
    public async Task<ClinicCoreDbContext?> CreateCoreForClinicAsync(Guid clinicId)
    {
        var connStr = await _connSvc.GetConnectionStringAsync(clinicId);
        if (string.IsNullOrEmpty(connStr))
        {
            connStr = _config.GetConnectionString("IdentityConnection");
            _log.LogWarning("No per-clinic core connection for {ClinicId} — using IdentityConnection", clinicId);
            if (string.IsNullOrEmpty(connStr)) return null;
        }

        var opts = new DbContextOptionsBuilder<ClinicCoreDbContext>()
            .UseNpgsql(connStr)
            .Options;

        return new ClinicCoreDbContext(opts);
    }

        public async Task<List<ClinicAccess>> GetUserClinicAccessAsync(string username)
    {
        var result = new List<ClinicAccess>();
        try
        {
            await using var db = await _masterFactory.CreateDbContextAsync();

            var pu = await db.PlatformUsers
                .Include(u => u.ClinicMemberships).ThenInclude(cm => cm.Clinic)
                .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null);

            if (pu == null) return result;

            // Global/Sys admins get all active clinics
            if (pu.UserType is "global_admin" or "sys_admin")
            {
                var allClinics = await db.Clinics
                    .Where(c => c.DeletedAt == null &&
                                (c.Status == "active" || c.Status == "provisioning"))
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return allClinics.Select(c => new ClinicAccess(
                    c.Id, c.Code, c.Name, c.City,
                    pu.Id, pu.Status, [pu.UserType], pu.UserType
                )).ToList();
            }

            // Site admin / Staff — use memberships
            return pu.ClinicMemberships
                .Where(cm => cm.Status == "active" && cm.Clinic?.DeletedAt == null)
                .Select(cm => new ClinicAccess(
                    cm.ClinicId, cm.Clinic!.Code, cm.Clinic.Name, cm.Clinic.City,
                    pu.Id, pu.Status, [pu.UserType], pu.UserType
                )).ToList();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to get clinic access for {User}", username);
            return result;
        }
    }

    public static string DetermineAdminLevel(string[] roles) => roles switch
    {
        _ when roles.Contains("global_admin") => "global_admin",
        _ when roles.Contains("sys_admin")    => "sys_admin",
        _ when roles.Contains("site_admin")   => "site_admin",
        _                                      => "staff",
    };
}

public record ClinicAccess(
    Guid     ClinicId,
    string   ClinicCode,
    string   ClinicName,
    string   ClinicCity,
    Guid     UserId,
    string   UserStatus,
    string[] Roles,
    string   AdminLevel
);
