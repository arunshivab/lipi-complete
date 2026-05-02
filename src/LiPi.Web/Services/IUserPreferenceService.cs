// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 4

using System.Security.Claims;

namespace LiPi.Web.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

/// <summary>
/// Immutable snapshot of a user's stored UI preferences.
/// All fields have safe defaults — never null.
/// </summary>
public record UserPreferenceDto(
    Guid   UserId,
    string ThemeMode,   // light | dark | auto | high-contrast
    string Density,     // comfortable | compact | spacious
    string FontSize,    // standard | larger
    string Language);   // BCP 47 (e.g. "en", "hi", "en-IN")

/// <summary>
/// Active brand theme metadata from master.brand_themes.
/// Used by the brand picker in settings and LipiThemeSwitcher.
/// </summary>
public record BrandThemeDto(
    string  BrandId,
    string  DisplayName,
    string? Description,
    string  CssFilePath,
    string? LogoLightUrl,
    string? LogoDarkUrl,
    int     SortOrder);

// ── Interface ─────────────────────────────────────────────────────────────────

/// <summary>
/// CRUD for identity.user_preferences in each clinic's identity database.
///
/// DESIGN CONTRACT:
/// - Caller provides clinicId — the service does not read HttpContext.
///   This keeps it testable and decoupled from request context.
/// - All read methods never throw — they return defaults on failure.
/// - All write methods validate inputs and throw ArgumentException for invalid values.
/// - All write methods fire an audit event (USER_THEME_CHANGED) via IAuditService.
/// - Theme preferences are NOT PHI — safe to audit; no phi_access_log needed.
/// </summary>
public interface IUserPreferenceService
{
    /// <summary>
    /// Get the current user's preferences for the given clinic.
    /// Returns defaults (light/compact/standard/en) if no row exists or on any error.
    /// Never throws.
    /// </summary>
    Task<UserPreferenceDto> GetAsync(
        Guid              userId,
        Guid              clinicId,
        CancellationToken ct = default);

    /// <summary>
    /// Set theme mode. Valid: light | dark | auto | high-contrast.
    /// Upserts identity.user_preferences. Audits with before/after state.
    /// Throws <see cref="ArgumentException"/> for invalid mode values.
    /// </summary>
    Task SetThemeModeAsync(
        Guid              userId,
        Guid              clinicId,
        string            mode,
        ClaimsPrincipal   actor,
        CancellationToken ct = default);

    /// <summary>
    /// Set UI density. Valid: comfortable | compact | spacious.
    /// Throws <see cref="ArgumentException"/> for invalid density values.
    /// </summary>
    Task SetDensityAsync(
        Guid              userId,
        Guid              clinicId,
        string            density,
        ClaimsPrincipal   actor,
        CancellationToken ct = default);

    /// <summary>
    /// Set font size. Valid: standard | larger.
    /// Throws <see cref="ArgumentException"/> for invalid font size values.
    /// </summary>
    Task SetFontSizeAsync(
        Guid              userId,
        Guid              clinicId,
        string            fontSize,
        ClaimsPrincipal   actor,
        CancellationToken ct = default);

    /// <summary>
    /// Set language. Must be a valid BCP 47 code (e.g. "en", "hi", "en-IN").
    /// Throws <see cref="ArgumentException"/> for invalid language codes.
    /// </summary>
    Task SetLanguageAsync(
        Guid              userId,
        Guid              clinicId,
        string            language,
        ClaimsPrincipal   actor,
        CancellationToken ct = default);
}
