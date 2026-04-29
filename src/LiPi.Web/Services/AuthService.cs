using System.Net;
using System.Text.Json;
using Isopoh.Cryptography.Argon2;
using LiPi.Master;
using LiPi.Master.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ILogger<AuthService>               _log;
    private readonly IConfiguration                     _config;

    public AuthService(
        IDbContextFactory<MasterDbContext> masterFactory,
        ILogger<AuthService>               log,
        IConfiguration                     config)
    {
        _masterFactory = masterFactory;
        _log           = log;
        _config        = config;
    }

    // ── Step 1: Credentials only ──────────────────────────────────────────
    public async Task<AuthResult> SignInAsync(
        string  username,
        string  password,
        string? clinicCode = null,
        string? clientIp   = null,
        string? userAgent  = null)
    {
        return await AuthenticatePlatformUser(username, password, clinicCode, clientIp, userAgent);
    }

    // ── Step 2: After clinic picker ───────────────────────────────────────
    public async Task<AuthResult> SignInWithClinicAsync(
        string  username,
        string  password,
        Guid    clinicId,
        string? clientIp  = null,
        string? userAgent = null)
    {
        await using var db = await _masterFactory.CreateDbContextAsync();

        var pu = await db.PlatformUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null);

        if (pu == null || !VerifyArgon2id(pu.PasswordHash, password))
            return Fail("Invalid credentials.");

        // Global/Sys admins have access to all clinics — no membership record needed
        if (pu.UserType is "global_admin" or "sys_admin")
        {
            var clinic = await db.Clinics
                .FirstOrDefaultAsync(c => c.Id == clinicId && c.DeletedAt == null);
            if (clinic == null) return Fail("Clinic not found.");
            return BuildResult(pu, clinic.Code, clinic.Name, clinicId);
        }

        // Staff/Site admin — must have an active membership
        var membership = await db.ClinicMemberships
            .Include(cm => cm.Clinic)
            .FirstOrDefaultAsync(cm => cm.PlatformUserId == pu.Id && cm.ClinicId == clinicId && cm.Status == "active");

        if (membership == null) return Fail("Access denied to this clinic.");

        return BuildResult(pu, membership.Clinic!.Code, membership.Clinic.Name, clinicId);
    }

    // ── Platform user authentication ──────────────────────────────────────
    private async Task<AuthResult> AuthenticatePlatformUser(
        string  username,
        string  password,
        string? clinicCode,
        string? clientIp,
        string? userAgent)
    {
        _log.LogInformation("PlatformUser auth: {User}", username);

        await using var db = await _masterFactory.CreateDbContextAsync();

        var pu = await db.PlatformUsers
            .Include(u => u.ClinicMemberships).ThenInclude(cm => cm.Clinic)
            .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null);

        if (pu == null) return Fail("Invalid username or password.");
        if (!VerifyArgon2id(pu.PasswordHash, password)) return Fail("Invalid username or password.");
        if (pu.Status != "active")
            return Fail(pu.Status == "locked" ? "Account is locked." : "Account is inactive.");

        // Update last login
        try
        {
            pu.FailedLoginCount = 0;
            pu.LockedUntil      = null;
            pu.LastLoginAt      = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        catch { }

        // Global/Sys admins — get all clinics
        if (pu.UserType is "global_admin" or "sys_admin")
        {
            var allClinics = await db.Clinics
                .Where(c => c.DeletedAt == null && (c.Status == "active" || c.Status == "provisioning"))
                .OrderBy(c => c.Name)
                .ToListAsync();

            var gaList = allClinics.Select(c => new ClinicAccess(
                c.Id, c.Code, c.Name, c.City, pu.Id, pu.Status, [pu.UserType], pu.UserType
            )).ToList();

            if (!string.IsNullOrEmpty(clinicCode))
            {
                var target = gaList.FirstOrDefault(a => a.ClinicCode == clinicCode);
                if (target != null) return BuildResult(pu, target.ClinicCode, target.ClinicName, target.ClinicId);
            }
            if (gaList.Count == 1) return BuildResult(pu, gaList[0].ClinicCode, gaList[0].ClinicName, gaList[0].ClinicId);
            return new AuthResult(true, null, pu.Id, pu.Username, pu.DisplayName, pu.UserType,
                null, [pu.UserType], null, pu.UserType, false, false, gaList);
        }

        // Site admin / Staff — use memberships
        var memberships = pu.ClinicMemberships
            .Where(cm => cm.Status == "active" && cm.Clinic?.DeletedAt == null)
            .ToList();

        if (!memberships.Any()) return Fail("No clinic access configured.");

        var staffList = memberships.Select(cm => new ClinicAccess(
            cm.ClinicId, cm.Clinic!.Code, cm.Clinic.Name, cm.Clinic.City,
            pu.Id, pu.Status, [pu.UserType], pu.UserType
        )).ToList();

        if (!string.IsNullOrEmpty(clinicCode))
        {
            var sc = staffList.FirstOrDefault(a => a.ClinicCode == clinicCode);
            if (sc != null) return BuildResult(pu, sc.ClinicCode, sc.ClinicName, sc.ClinicId);
        }
        if (staffList.Count == 1) return BuildResult(pu, staffList[0].ClinicCode, staffList[0].ClinicName, staffList[0].ClinicId);
        return new AuthResult(true, null, pu.Id, pu.Username, pu.DisplayName, pu.UserType,
            null, [pu.UserType], null, pu.UserType, false, false, staffList);
    }

    // ── Helpers
    // ── Helpers ───────────────────────────────────────────────────────────
    private static AuthResult BuildResult(PlatformUser pu, string clinicCode, string clinicName, Guid clinicId) =>
        new(true, null, pu.Id, pu.Username, pu.DisplayName, pu.UserType,
            clinicCode, [pu.UserType], clinicId,
            ClinicDbFactory.DetermineAdminLevel([pu.UserType]), false,
            pu.MustChangePassword);

    private bool VerifyArgon2id(string? hash, string password)
    {
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(password)) return false;
        try { return Argon2.Verify(hash, password); }
        catch (Exception ex) { _log.LogError(ex, "Argon2.Verify error"); return false; }
    }

    private static AuthResult Fail(string msg) =>
        new(false, msg,
            null, null, null, null, null, []);
}
