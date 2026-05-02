// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 4
//
// DB ACCESS:
//   Uses ClinicDbFactory.CreateForClinicAsync(clinicId) rather than
//   IDbContextFactory<IdentityDbContext> directly. Reason: identity.user_preferences
//   lives in per-clinic databases. ClinicDbFactory resolves the correct connection
//   string per clinic and returns an IdentityDbContext pointed at that clinic's DB.
//   This satisfies the intent of "use IDbContextFactory<IdentityDbContext>" in the
//   kickoff spec while supporting multi-clinic isolation.

using System.Security.Claims;
using System.Text.RegularExpressions;
using LiPi.Clinic.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

public class UserPreferenceService : IUserPreferenceService
{
    // ── Allowlists — spec-defined constants, not DB-driven ────────────────────
    private static readonly HashSet<string> ValidModes =
        ["light", "dark", "auto", "high-contrast"];

    private static readonly HashSet<string> ValidDensities =
        ["comfortable", "compact", "spacious"];

    private static readonly HashSet<string> ValidFontSizes =
        ["standard", "larger"];

    // BCP 47: "en", "hi", "en-IN" — two lowercase letters, optional dash + two uppercase
    private static readonly Regex BcpLangRegex =
        new(@"^[a-z]{2}(-[A-Z]{2})?$", RegexOptions.Compiled);

    private readonly ClinicDbFactory               _clinicDbFactory;
    private readonly IAuditService                 _audit;
    private readonly ILogger<UserPreferenceService> _log;

    public UserPreferenceService(
        ClinicDbFactory                clinicDbFactory,
        IAuditService                  audit,
        ILogger<UserPreferenceService> log)
    {
        _clinicDbFactory = clinicDbFactory;
        _audit           = audit;
        _log             = log;
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public async Task<UserPreferenceDto> GetAsync(
        Guid userId, Guid clinicId, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId);
            if (db == null) return DefaultPrefs(userId);

            var pref = await db.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId, ct);

            return pref == null ? DefaultPrefs(userId) : ToDto(pref);
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "UserPreferenceService.GetAsync failed — userId={UserId} clinicId={ClinicId}. " +
                "Returning defaults.",
                userId, clinicId);
            return DefaultPrefs(userId);
        }
    }

    // ── Writes ────────────────────────────────────────────────────────────────

    public async Task SetThemeModeAsync(
        Guid userId, Guid clinicId, string mode,
        ClaimsPrincipal actor, CancellationToken ct = default)
    {
        if (!ValidModes.Contains(mode))
            throw new ArgumentException(
                $"Invalid theme mode '{mode}'. Valid values: {string.Join(", ", ValidModes)}");

        await UpsertAsync(
            userId, clinicId, actor,
            mutate: pref =>
            {
                var before = new { pref.ThemeMode };
                pref.ThemeMode = mode;
                return ((object)before, (object)new { ThemeMode = mode });
            },
            auditAction: AuditActions.UserThemeChanged,
            auditLabel:  $"Theme mode changed to '{mode}'",
            ct);
    }

    public async Task SetDensityAsync(
        Guid userId, Guid clinicId, string density,
        ClaimsPrincipal actor, CancellationToken ct = default)
    {
        if (!ValidDensities.Contains(density))
            throw new ArgumentException(
                $"Invalid density '{density}'. Valid values: {string.Join(", ", ValidDensities)}");

        await UpsertAsync(
            userId, clinicId, actor,
            mutate: pref =>
            {
                var before = new { pref.Density };
                pref.Density = density;
                return ((object)before, (object)new { Density = density });
            },
            auditAction: AuditActions.UserThemeChanged,
            auditLabel:  $"UI density changed to '{density}'",
            ct);
    }

    public async Task SetFontSizeAsync(
        Guid userId, Guid clinicId, string fontSize,
        ClaimsPrincipal actor, CancellationToken ct = default)
    {
        if (!ValidFontSizes.Contains(fontSize))
            throw new ArgumentException(
                $"Invalid font size '{fontSize}'. Valid values: {string.Join(", ", ValidFontSizes)}");

        await UpsertAsync(
            userId, clinicId, actor,
            mutate: pref =>
            {
                var before = new { pref.FontSize };
                pref.FontSize = fontSize;
                return ((object)before, (object)new { FontSize = fontSize });
            },
            auditAction: AuditActions.UserThemeChanged,
            auditLabel:  $"Font size changed to '{fontSize}'",
            ct);
    }

    public async Task SetLanguageAsync(
        Guid userId, Guid clinicId, string language,
        ClaimsPrincipal actor, CancellationToken ct = default)
    {
        if (!BcpLangRegex.IsMatch(language))
            throw new ArgumentException(
                $"Invalid language code '{language}'. Must match BCP 47 (e.g. 'en', 'hi', 'en-IN').");

        await UpsertAsync(
            userId, clinicId, actor,
            mutate: pref =>
            {
                var before = new { pref.Language };
                pref.Language = language;
                return ((object)before, (object)new { Language = language });
            },
            auditAction: AuditActions.UserThemeChanged,
            auditLabel:  $"Language changed to '{language}'",
            ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Generic upsert + audit. Single code path for all preference field updates.
    /// Opens per-clinic DB, finds-or-creates the preference row, applies the
    /// mutation delegate, saves, then fires the audit event.
    /// </summary>
    private async Task UpsertAsync(
        Guid                                                     userId,
        Guid                                                     clinicId,
        ClaimsPrincipal                                          actor,
        Func<UserPreference, (object Before, object After)>     mutate,
        string                                                   auditAction,
        string                                                   auditLabel,
        CancellationToken                                        ct)
    {
        await using var db = await _clinicDbFactory.CreateForClinicAsync(clinicId)
            ?? throw new InvalidOperationException(
                $"Could not connect to clinic DB for clinicId={clinicId}. " +
                "Ensure ClinicDatabase entry exists in master DB.");

        var pref = await db.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        object beforeState;
        object afterState;

        if (pref == null)
        {
            // No row yet — INSERT with defaults, then apply mutation
            pref = new UserPreference { UserId = userId };
            var (before, after) = mutate(pref);
            beforeState = before;
            afterState  = after;
            db.UserPreferences.Add(pref);
        }
        else
        {
            // Existing row — mutate in place (EF change tracking handles UPDATE)
            var (before, after) = mutate(pref);
            beforeState = before;
            afterState  = after;
        }

        await db.SaveChangesAsync(ct);

        // Audit — not PHI; no phi_access_log needed
        await _audit.LogAsync(
            clinicId:    clinicId,
            actor:       actor,
            action:      auditAction,
            entityType:  "user_preference",
            entityId:    userId,
            entityLabel: auditLabel,
            beforeState: beforeState,
            afterState:  afterState);
    }

    private static UserPreferenceDto DefaultPrefs(Guid userId) =>
        new(userId, "light", "compact", "standard", "en");

    private static UserPreferenceDto ToDto(UserPreference p) =>
        new(p.UserId, p.ThemeMode, p.Density, p.FontSize, p.Language);
}
