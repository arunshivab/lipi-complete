using LiPi.Master;
using LiPi.Master.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

/// <summary>
/// When a new clinic is created, auto-assigns all existing SysAdmins + GlobalAdmins
/// by adding clinic_membership records in master DB.
/// No per-clinic user records needed — auth uses master.platform_users directly.
/// </summary>
public class SysAdminAutoAssignService
{
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ILogger<SysAdminAutoAssignService> _log;

    public SysAdminAutoAssignService(
        IDbContextFactory<MasterDbContext> masterFactory,
        ILogger<SysAdminAutoAssignService> log)
    {
        _masterFactory = masterFactory;
        _log           = log;
    }

    public async Task AssignGlobalAndSysAdminsToClinicAsync(Guid newClinicId, string clinicCode)
    {
        _log.LogInformation("Auto-assigning Global/SysAdmins to new clinic {Code}", clinicCode);
        try
        {
            await using var db = await _masterFactory.CreateDbContextAsync();

            var admins = await db.PlatformUsers
                .Where(u => u.DeletedAt == null &&
                            (u.UserType == "global_admin" || u.UserType == "sys_admin"))
                .ToListAsync();

            foreach (var admin in admins)
            {
                var exists = await db.ClinicMemberships
                    .AnyAsync(cm => cm.PlatformUserId == admin.Id && cm.ClinicId == newClinicId);

                if (exists) continue;

                db.ClinicMemberships.Add(new ClinicMembership
                {
                    Id             = Guid.NewGuid(),
                    PlatformUserId = admin.Id,
                    ClinicId       = newClinicId,
                    Status         = "active",
                    CreatedAt      = DateTime.UtcNow,
                });
                _log.LogInformation("Assigned {User} ({Type}) to clinic {Code}",
                    admin.Username, admin.UserType, clinicCode);
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to auto-assign admins to clinic {ClinicId}", newClinicId);
        }
    }
}
