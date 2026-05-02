// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Theme Switching Mechanism
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 4
//
// VALID BRANDS:
//   NOT a hardcoded HashSet. SetBrandAsync queries master.brand_themes at runtime.
//   Adding a new client brand = INSERT row in brand_themes. Zero code changes here.
//
// VALID MODES:
//   HashSet — mode values are spec constants, not DB-driven.
//
// COOKIE NOTE:
//   HttpOnly=false is intentional — theme-switcher.js (Deliverable 6) reads cookies
//   client-side before first paint to apply data-brand/data-mode on <body>.
//   These cookies hold no sensitive data (not PHI).

using System.Security.Claims;
using LiPi.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

public class ThemeContextService : IThemeContextService
{
    // Mode values are spec-defined constants — HashSet is correct here (not DB-driven)
    private static readonly HashSet<string> ValidModes =
        ["light", "dark", "auto", "high-contrast"];

    private static readonly CookieOptions ThemeCookieOptions = new()
    {
        HttpOnly = false,                             // JS must read for FOUC prevention
        Secure   = true,                              // HTTPS only
        SameSite = SameSiteMode.Strict,
        MaxAge   = TimeSpan.FromSeconds(31_536_000)  // 1 year
    };

    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ClinicDbFactory                    _clinicDbFactory;
    private readonly IUserPreferenceService             _prefService;
    private readonly IHttpContextAccessor               _http;
    private readonly IAuditService                      _audit;
    private readonly ILogger<ThemeContextService>       _log;

    public ThemeContextService(
        IDbContextFactory<MasterDbContext> masterFactory,
        ClinicDbFactory                   clinicDbFactory,
        IUserPreferenceService            prefService,
        IHttpContextAccessor              http,
        IAuditService                     audit,
        ILogger<ThemeContextService>      log)
    {
        _masterFactory   = masterFactory;
        _clinicDbFactory = clinicDbFactory;
        _prefService     = prefService;
        _http            = http;
        _audit           = audit;
        _log             = log;
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public async Task<string> GetModeAsync(CancellationToken ct = default)
    {
        // 1. Cookie — available during SSR prerender (FOUC prevention path)
        var cookie = GetThemeCookie(IThemeContextService.CookieThemeMode);
        if (!string.IsNullOrEmpty(cookie) && ValidModes.Contains(cookie))
            return cookie;

        // 2. Database — requires authenticated user
        try
        {
            var (userId, clinicId) = GetCurrentIds();
            if (userId.HasValue && clinicId.HasValue)
            {
                var prefs = await _prefService.GetAsync(userId.Value, clinicId.Value, ct);
                // Sync to cookie so subsequent SSR requests skip the DB hit
                SetThemeCookie(IThemeContextService.CookieThemeMode, prefs.ThemeMode);
                return prefs.ThemeMode;
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex,
                "ThemeContextService.GetModeAsync: DB fallback failed — returning default 'light'");
        }

        // 3. Hard default
        return "light";
    }

    public async Task<string> GetBrandAsync(CancellationToken ct = default)
    {
        // 1. Cookie
        var cookie = GetThemeCookie(IThemeContextService.CookieBrandTheme);
        if (!string.IsNullOrEmpty(cookie)) return cookie;

        // 2. master.clinics.brand_theme_id for the current clinic
        try
        {
            var (_, clinicId) = GetCurrentIds();
            if (clinicId.HasValue)
            {
                await using var db = await _masterFactory.CreateDbContextAsync(ct);
                var brand = await db.Clinics
                    .Where(c => c.Id == clinicId.Value && c.DeletedAt == null)
                    .Select(c => c.BrandThemeId)
                    .FirstOrDefaultAsync(ct);

                if (!string.IsNullOrEmpty(brand))
                {
                    SetThemeCookie(IThemeContextService.CookieBrandTheme, brand);
                    return brand;
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex,
                "ThemeContextService.GetBrandAsync: DB fallback failed — returning default 'lipi-default'");
        }

        // 3. Hard default
        return "lipi-default";
    }

    public async Task<IReadOnlyList<BrandThemeDto>> GetActiveBrandsAsync(CancellationToken ct = default)
    {
        try
        {
            await using var db = await _masterFactory.CreateDbContextAsync(ct);

            var brands = await db.BrandThemes
                .AsNoTracking()
                .Where(b => b.IsActive && !b.IsDeprecated)
                .OrderBy(b => b.SortOrder)
                .Select(b => new BrandThemeDto(
                    b.BrandId,
                    b.DisplayName,
                    b.Description,
                    b.CssFilePath,
                    b.LogoLightUrl,
                    b.LogoDarkUrl,
                    b.SortOrder))
                .ToListAsync(ct);

            return brands.Count > 0 ? brands : FallbackBrands();
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "ThemeContextService.GetActiveBrandsAsync: DB query failed — returning hardcoded fallback");
            return FallbackBrands();
        }
    }

    // ── Write ─────────────────────────────────────────────────────────────────

    public async Task SetModeAsync(
        string mode, ClaimsPrincipal actor, CancellationToken ct = default)
    {
        if (!ValidModes.Contains(mode))
            throw new ArgumentException(
                $"Invalid theme mode '{mode}'. Valid: {string.Join(", ", ValidModes)}");

        var (userId, clinicId) = GetCurrentIdsFromActor(actor);

        // Persist to identity.user_preferences (UserPreferenceService fires audit internally)
        await _prefService.SetThemeModeAsync(userId, clinicId, mode, actor, ct);

        // Sync cookie (client reads this to apply data-mode on <body>)
        SetThemeCookie(IThemeContextService.CookieThemeMode, mode);
    }

    public async Task SetBrandAsync(
        string brand, ClaimsPrincipal actor, CancellationToken ct = default)
    {
        // Admin check — SiteAdmin minimum required to change a clinic's brand
        if (!ClaimsHelper.IsSiteAdmin(actor))
            throw new UnauthorizedAccessException(
                $"User '{ClaimsHelper.Username(actor)}' does not have permission to change brand theme. " +
                "SiteAdmin or higher required.");

        await using var db = await _masterFactory.CreateDbContextAsync(ct);

        // Validate against master.brand_themes — NOT a hardcoded list
        // This is the core of the lookup table design: validity = presence in table
        var brandRecord = await db.BrandThemes
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BrandId == brand && b.IsActive && !b.IsDeprecated, ct);

        if (brandRecord == null)
            throw new ArgumentException(
                $"Brand '{brand}' does not exist or is not currently active. " +
                "Call GetActiveBrandsAsync() for the current valid brand list.");

        // Update master.clinics
        var (_, clinicId) = GetCurrentIdsFromActor(actor);
        var clinic = await db.Clinics.FindAsync([clinicId], ct)
            ?? throw new InvalidOperationException($"Clinic {clinicId} not found in master DB.");

        var priorBrand = clinic.BrandThemeId;
        clinic.BrandThemeId = brand;
        await db.SaveChangesAsync(ct);

        // Sync cookie
        SetThemeCookie(IThemeContextService.CookieBrandTheme, brand);

        // Audit — brand change is a clinic-level event, fired here (not via UserPreferenceService)
        await _audit.LogAsync(
            clinicId:    clinicId,
            actor:       actor,
            action:      AuditActions.UserBrandChanged,
            entityType:  "clinic",
            entityId:    clinicId,
            entityLabel: $"Brand theme changed: '{priorBrand}' → '{brand}'",
            beforeState: new { BrandThemeId = priorBrand },
            afterState:  new { BrandThemeId = brand });
    }

    // ── Cookie helpers ────────────────────────────────────────────────────────

    public void SetThemeCookie(string name, string value)
    {
        // No-op if HttpContext is unavailable (e.g., background service context)
        _http.HttpContext?.Response.Cookies.Append(name, value, ThemeCookieOptions);
    }

    public string? GetThemeCookie(string name)
    {
        return _http.HttpContext?.Request.Cookies.TryGetValue(name, out var v) == true
            ? v
            : null;
    }

    public void ClearThemeCookie(string name)
    {
        _http.HttpContext?.Response.Cookies.Delete(name);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Extract userId + clinicId from HttpContext claims (best-effort, SSR-safe).
    /// Returns nulls if HttpContext is unavailable or user is not authenticated.
    /// </summary>
    private (Guid? UserId, Guid? ClinicId) GetCurrentIds()
    {
        var user = _http.HttpContext?.User;
        return user == null
            ? (null, null)
            : (ClaimsHelper.UserId(user), ClaimsHelper.ClinicId(user));
    }

    /// <summary>
    /// Extract userId + clinicId from a ClaimsPrincipal.
    /// Throws InvalidOperationException if either claim is missing.
    /// Use for write operations where the actor is explicitly provided.
    /// </summary>
    private static (Guid UserId, Guid ClinicId) GetCurrentIdsFromActor(ClaimsPrincipal actor)
    {
        var userId = ClaimsHelper.UserId(actor)
            ?? throw new InvalidOperationException(
                "Cannot resolve userId from actor claims. " +
                "Ensure the user is authenticated before calling write methods.");

        var clinicId = ClaimsHelper.ClinicId(actor)
            ?? throw new InvalidOperationException(
                "Cannot resolve clinicId from actor claims. " +
                "Ensure the user has selected a clinic before calling write methods.");

        return (userId, clinicId);
    }

    /// <summary>
    /// Fallback brand list when master DB is unreachable.
    /// Guarantees GetActiveBrandsAsync never returns an empty list.
    /// </summary>
    private static IReadOnlyList<BrandThemeDto> FallbackBrands() =>
    [
        new BrandThemeDto(
            BrandId:      "lipi-default",
            DisplayName:  "LiPi Default",
            Description:  "Standard LiPi theme (fallback — master DB unreachable)",
            CssFilePath:  "themes/brand-lipi.css",
            LogoLightUrl: "/images/logos/lipi-logo.svg",
            LogoDarkUrl:  "/images/logos/lipi-logo-dark.svg",
            SortOrder:    1)
    ];
}
