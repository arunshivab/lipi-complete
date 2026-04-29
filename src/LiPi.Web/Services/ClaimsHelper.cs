using System.Security.Claims;

namespace LiPi.Web.Services;

/// <summary>
/// Typed helpers for reading LiPi session claims.
/// Use these everywhere instead of raw claim lookups.
/// </summary>
public static class ClaimsHelper
{
    public static Guid?   UserId(ClaimsPrincipal u) =>
        Guid.TryParse(u.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : null;

    public static string  Username(ClaimsPrincipal u)    => u.FindFirstValue(ClaimTypes.Name)   ?? string.Empty;
    public static string  DisplayName(ClaimsPrincipal u) => u.FindFirstValue("displayName")      ?? Username(u);
    public static string  ClinicCode(ClaimsPrincipal u)  => u.FindFirstValue("clinic")           ?? string.Empty;
    public static string  ClinicName(ClaimsPrincipal u)  => u.FindFirstValue("clinicName")       ?? ClinicCode(u);
    public static string  AdminLevel(ClaimsPrincipal u)  => u.FindFirstValue("adminLevel")       ?? "staff";

    public static Guid? ClinicId(ClaimsPrincipal u) =>
        Guid.TryParse(u.FindFirstValue("clinicId"), out var g) ? g : null;

    public static string[] Roles(ClaimsPrincipal u) =>
        u.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

    // Admin level checks
    public static bool IsGlobalAdmin(ClaimsPrincipal u) => AdminLevel(u) == "global_admin";
    public static bool IsSysAdmin(ClaimsPrincipal u)    => AdminLevel(u) is "global_admin" or "sys_admin";
    public static bool IsSiteAdmin(ClaimsPrincipal u)   => AdminLevel(u) is "global_admin" or "sys_admin" or "site_admin";
    public static bool IsAnyAdmin(ClaimsPrincipal u)    => AdminLevel(u) != "staff";

    // Greeting
    public static string Greeting()
    {
        var h = DateTime.Now.Hour;
        return h < 12 ? "Good morning" : h < 17 ? "Good afternoon" : "Good evening";
    }
}
