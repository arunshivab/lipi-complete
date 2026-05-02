// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Theme Switching Mechanism
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 4

using System.Security.Claims;

namespace LiPi.Web.Services;

/// <summary>
/// Manages theme resolution for the current HTTP context / Blazor circuit.
///
/// RESOLUTION ORDER for read methods:
///   Cookie → Database → Hard default
///   Cookies are read during SSR (FOUC prevention). DB is authoritative.
///
/// COOKIE STRATEGY:
///   lipi_theme_mode  — user's light/dark/auto mode preference
///   lipi_brand_theme — clinic's active brand theme
///   HttpOnly  = false   — JS reads cookies before first paint (FOUC prevention)
///   SameSite  = Strict  — CSRF protection
///   Secure    = true    — HTTPS only
///   MaxAge    = 1 year  — persistent across sessions
///
/// VALID BRANDS NOTE:
///   ValidBrands is NOT a hardcoded HashSet. SetBrandAsync queries
///   master.brand_themes WHERE is_active = TRUE AND is_deprecated = FALSE.
///   Adding a new client brand = INSERT row only. No code change required.
///
/// AUDIT:
///   SetModeAsync  → delegates to IUserPreferenceService (fires UserThemeChanged)
///   SetBrandAsync → fires UserBrandChanged directly (clinic-level change, not user-level)
///   Theme prefs are NOT PHI — safe to audit without phi_access_log.
/// </summary>
public interface IThemeContextService
{
    // ── Cookie name constants ─────────────────────────────────────────────────
    // Public so ThemeProvider.razor can reference the same names without magic strings.
    const string CookieThemeMode  = "lipi_theme_mode";
    const string CookieBrandTheme = "lipi_brand_theme";

    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get the effective theme mode for the current user.
    /// Resolution: cookie → identity.user_preferences → "light".
    /// Never throws — returns "light" on any failure.
    /// </summary>
    Task<string> GetModeAsync(CancellationToken ct = default);

    /// <summary>
    /// Get the effective brand theme for the current clinic.
    /// Resolution: cookie → master.clinics.brand_theme_id → "lipi-default".
    /// Never throws — returns "lipi-default" on any failure.
    /// </summary>
    Task<string> GetBrandAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all active, non-deprecated brand themes from master.brand_themes,
    /// ordered by sort_order ascending.
    /// Returns [lipi-default fallback] on DB error — never returns empty list.
    /// </summary>
    Task<IReadOnlyList<BrandThemeDto>> GetActiveBrandsAsync(CancellationToken ct = default);

    // ── Write ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Set theme mode for the current user.
    /// Validates → updates identity.user_preferences → writes cookie → (audit via UserPreferenceService).
    /// Throws <see cref="ArgumentException"/> for invalid mode values.
    /// </summary>
    Task SetModeAsync(
        string            mode,
        ClaimsPrincipal   actor,
        CancellationToken ct = default);

    /// <summary>
    /// Set the brand theme for the current clinic. Admin-only (SiteAdmin+ required).
    /// Validates against master.brand_themes (NOT a hardcoded list) →
    /// updates master.clinics.brand_theme_id → writes cookie → audits UserBrandChanged.
    /// Throws <see cref="ArgumentException"/> if brand not found or inactive/deprecated.
    /// Throws <see cref="UnauthorizedAccessException"/> if actor is not SiteAdmin+.
    /// </summary>
    Task SetBrandAsync(
        string            brand,
        ClaimsPrincipal   actor,
        CancellationToken ct = default);

    // ── Cookie helpers ────────────────────────────────────────────────────────
    // Public so ThemeProvider.razor can sync cookies without re-implementing the policy.

    /// <summary>
    /// Write a theme cookie with the standard LiPi cookie policy
    /// (HttpOnly=false, SameSite=Strict, Secure=true, MaxAge=1yr).
    /// No-op if HttpContext is unavailable.
    /// </summary>
    void SetThemeCookie(string name, string value);

    /// <summary>Read a theme cookie from the current request. Returns null if absent or HttpContext unavailable.</summary>
    string? GetThemeCookie(string name);

    /// <summary>Delete a theme cookie. No-op if HttpContext unavailable.</summary>
    void ClearThemeCookie(string name);
}
