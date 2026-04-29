using Isopoh.Cryptography.Argon2;
using LiPi.Master;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LiPi.Web.Services;

/// <summary>
/// Seeds every clinic with default roles + 3 system users using raw SQL.
/// Safe to run multiple times — all inserts are idempotent (ON CONFLICT DO NOTHING).
/// </summary>
public static class ClinicSeeder
{
    private static readonly (string Username, string Password, string RoleCode, string Display)[] DefaultUsers =
    [
        ("Admin",     "Admin@123",     "global_admin", "Global Administrator"),
        ("SysAdmin",  "SysAdmin@123",  "sys_admin",    "System Administrator"),
        ("SiteAdmin", "SiteAdmin@123", "site_admin",   "Site Administrator"),
    ];

    private static readonly (string Code, string Name, bool IsSystem)[] DefaultRoles =
    [
        ("global_admin",  "Global Administrator", true),
        ("sys_admin",     "System Administrator", true),
        ("site_admin",    "Site Administrator",   true),
        ("director",      "Director",             false),
        ("dept_manager",  "Department Manager",   false),
        ("physician",     "Physician",            false),
        ("consultant",    "Consultant",           false),
        ("resident",      "Resident",             false),
        ("charge_nurse",  "Charge Nurse",         false),
        ("nurse",         "Nurse",                false),
        ("physicist",     "Medical Physicist",    false),
        ("dosimetrist",   "Dosimetrist",          false),
        ("radiographer",  "Radiographer",         false),
        ("pharmacist",    "Pharmacist",           false),
        ("lab_tech",      "Lab Technician",       false),
        ("radiologist",   "Radiologist",          false),
        ("billing_staff", "Billing Staff",        false),
        ("reception",     "Receptionist",         false),
        ("data_entry",    "Data Entry",           false),
    ];

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope    = services.CreateScope();
        var masterDb       = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        var identityConn   = scope.ServiceProvider
            .GetRequiredService<IConfiguration>()
            .GetConnectionString("IdentityConnection")
            ?? scope.ServiceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("DefaultConnection")!;

        // ── 1. Seed Training org + clinic in master ───────────────────────
        await SeedMasterAsync(masterDb, logger);

        // ── 2. Get all clinics ────────────────────────────────────────────
        List<(Guid Id, string Code, string Name)> clinics = [];
        try
        {
            var rows = await masterDb.Clinics
                .Where(c => c.DeletedAt == null)
                .Select(c => new { c.Id, c.Code, c.Name })
                .ToListAsync();
            clinics = rows.Select(r => (r.Id, r.Code, r.Name)).ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load clinics from master DB");
            return;
        }

        if (clinics.Count == 0)
        {
            logger.LogWarning("No clinics in master DB — skipping identity seeding");
            return;
        }

        // ── 3. Seed identity for each clinic using raw SQL ────────────────
        await using var conn = new NpgsqlConnection(identityConn);
        try
        {
            await conn.OpenAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot connect to identity DB for seeding");
            return;
        }

        foreach (var (clinicId, clinicCode, clinicName) in clinics)
        {
            await SeedClinicAsync(conn, clinicId, clinicCode, clinicName, logger);
        }
    }

    private static async Task SeedMasterAsync(MasterDbContext db, ILogger logger)
    {
        try
        {
            var org = await db.Organizations
                .FirstOrDefaultAsync(o => o.LegalName == "Training Organization");

            if (org == null)
            {
                org = new LiPi.Master.Entities.Organization
                {
                    Id                = new Guid("00000000-0000-0000-0000-000000000001"),
                    LegalName         = "Training Organization",
                    OrgType           = "single_clinic",
                    CountryCode       = "IN",
                    RegisteredAddress = "{}",
                    PrimaryContact    = "{}",
                    ExtensionData     = "{}",
                    Status            = "active",
                    OnboardedAt       = DateTimeOffset.UtcNow,
                    CreatedAt         = DateTimeOffset.UtcNow,
                    UpdatedAt         = DateTimeOffset.UtcNow,
                };
                db.Organizations.Add(org);
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded Training Organization");
            }

            var clinic = await db.Clinics.FirstOrDefaultAsync(c => c.Code == "training");
            if (clinic == null)
            {
                db.Clinics.Add(new LiPi.Master.Entities.Clinic
                {
                    Id             = new Guid("00000000-0000-0000-0000-000000000002"),
                    OrganizationId = org.Id,
                    Code           = "training",
                    Name           = "Training",
                    ClinicType     = "clinic",
                    City           = "Mumbai",
                    State          = "Maharashtra",
                    CountryCode    = "IN",
                    Address        = "{\"line1\":\"Training Facility\"}",
                    Timezone       = "Asia/Kolkata",
                    ExtensionData  = "{}",
                    Status         = "active",
                    CreatedAt      = DateTimeOffset.UtcNow,
                    UpdatedAt      = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded Training Clinic");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not seed master DB");
        }
    }

    private static async Task SeedClinicAsync(
        NpgsqlConnection conn,
        Guid clinicId, string clinicCode, string clinicName,
        ILogger logger)
    {
        try
        {
            // ── Seed roles ────────────────────────────────────────────────
            foreach (var (code, name, isSystem) in DefaultRoles)
            {
                await using var cmd = new NpgsqlCommand(@"
                    INSERT INTO identity.roles
                        (id, clinic_id, code, name, is_system_role, is_active, created_at, updated_at, row_version)
                    VALUES
                        (gen_random_uuid(), @clinic_id, @code, @name, @is_system, true, now(), now(), 1)
                    ON CONFLICT (clinic_id, code) DO NOTHING", conn);

                cmd.Parameters.AddWithValue("clinic_id", clinicId);
                cmd.Parameters.AddWithValue("code",      code);
                cmd.Parameters.AddWithValue("name",      name);
                cmd.Parameters.AddWithValue("is_system", isSystem);
                await cmd.ExecuteNonQueryAsync();
            }

            // ── Seed default users ────────────────────────────────────────
            // User seeding removed — users now created via master.platform_users
            // Use Register User UI (/admin/users/new) to create staff users

        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not seed identity for clinic {Clinic}", clinicName);
        }
    }
}
