using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using LiPi.Clinic.Identity;
using LiPi.Master;
using LiPi.Clinic.Audit;
using LiPi.Web;
using LiPi.Web.Components.Shared;
using LiPi.Web.Services;
using LiPi.Components.DataDisplay;
using LiPi.Components;  // input/selection family migrated here (A43)
using LiPi.Components.Overlays;  // overlay cluster migrated here (PR2)

// ── Bootstrap check ──────────────────────────────────────────────────────────
if (args.Contains("--setup-global-admin"))
{
    var bootBuilder = WebApplication.CreateBuilder(args);
    var bootMasterConn = bootBuilder.Configuration.GetConnectionString("MasterConnection")
                      ?? bootBuilder.Configuration.GetConnectionString("DefaultConnection")!;
    bootBuilder.Services.AddDbContextFactory<LiPi.Master.MasterDbContext>(
        o => o.UseNpgsql(bootMasterConn));
    var bootApp = bootBuilder.Build();
    await LiPi.Web.Services.GlobalAdminBootstrap.RunAsync(bootApp.Services);
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// ── Databases — use BOTH DbContext (for SSR) AND DbContextFactory (for interactive) ──
var identityConn = builder.Configuration.GetConnectionString("IdentityConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")!;
var masterConn   = builder.Configuration.GetConnectionString("MasterConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")!;

// DbContextFactory only — never mix AddDbContext + AddDbContextFactory (CLAUDE.md Rule 8)
builder.Services.AddDbContextFactory<IdentityDbContext>(o => o.UseNpgsql(identityConn));
builder.Services.AddDbContextFactory<MasterDbContext>  (o => o.UseNpgsql(masterConn));
builder.Services.AddDbContextFactory<AuditDbContext>     (o => o.UseNpgsql(identityConn));

// ── Auth ──────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath         = "/";
        options.LogoutPath        = "/logout";
        options.AccessDeniedPath  = "/access-denied";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;  // Reset on activity — HIPAA allows inactivity-based timeout
        options.Cookie.HttpOnly   = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite     = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ClinicConnectionService>();
builder.Services.AddScoped<ClinicDbFactory>();
builder.Services.AddScoped<SysAdminAutoAssignService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<OtpService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

// Session (needed for clinic picker state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout        = TimeSpan.FromMinutes(10); // picker timeout
    opt.Cookie.HttpOnly    = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.SameSite    = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
});
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
builder.Services.AddScoped<IAadhaarXmlService, AadhaarXmlService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpContextAccessor();

// HTTP clients
builder.Services.AddHttpClient();   // registers IHttpClientFactory
builder.Services.AddHttpClient("IndiaPinApi", client =>
{
    client.BaseAddress = new Uri("https://api.postalpincode.in/");
    client.Timeout     = TimeSpan.FromSeconds(5);
});

// Always show detailed errors so we can diagnose issues.
// NOTE: AddServerSideBlazor() is a .NET 7 legacy API. It is retained here ONLY
// for the .AddCircuitOptions(o => o.DetailedErrors = true) call until Phase 2.10
// Infrastructure Audit migrates this to the modern pattern. Removing it without
// the migration breaks Blazor events (see docs/00-PROJECT-BASELINE.md §CRITICAL
// BUG FIXES). Queued in roadmap memory.
builder.Services.AddServerSideBlazor().AddCircuitOptions(o => o.DetailedErrors = true);

// ── Theme services — Decision #12, Phase 1 ────────────────────────────
// SPEC: docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Theme Provider Component
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
builder.Services.AddScoped<IThemeContextService, ThemeContextService>();

// ── Phase 2.2 Lipi input defaults (Decision #12, Sub-step 2.2) ─────────────
// SPEC: docs/00-COMPONENTS/01.2-TextInputs.md (pending)
// Global default for input components (LipiTextBox, LipiTextArea, LipiNumberInput, LipiSelect).
// Per-component parameters override these defaults when set.
// To change app-wide defaults, pass a configure delegate:
//   builder.Services.Configure<LipiInputDefaults>(o => {
//       o.RequiredVisualStyle = RequiredVisualStyle.AsteriskOnly;
//   });
builder.Services.Configure<LipiInputDefaults>(_ => { });  // accept built-in defaults

// ── Phase 2.4 Date/Time services (Sub-step 2.4 — Batch 9d) ─────────────────
// SPEC: docs/00-COMPONENTS/01.5-DateTime.md
// AMEND: docs/CHANGE-LOG.md A20
//
// IDateFormatService:    clinic-configurable date/time format (parse + format
//                        + segment order). Phase 2.4 default impl hardcodes
//                        India (DD/MM/YYYY, 24h, Sunday-first week).
// IClinicTimezoneService: clinic timezone resolution (NOT system clock).
//                        Phase 2.4 default hardcodes "Asia/Kolkata" (UTC+5:30,
//                        no DST). Defensive fallback to fixed +05:30 offset
//                        if ICU TZ database unavailable.
//
// Both are scoped — instances are recreated per circuit / per request, which
// allows future swap-in of clinic-context-aware impls without registration
// changes when the master.clinics config columns land.
builder.Services.AddScoped<IDateFormatService, DateFormatService>();
builder.Services.AddScoped<IClinicTimezoneService, ClinicTimezoneService>();

// ── Phase 2.6.2 Overlay infrastructure ─────────────────────────────────────
// SPEC: docs/00-Phase2.6.2-Overview.md — Shared infrastructure
// All Scoped — one instance per Blazor circuit (per user session).
// Build order: FocusTrap + ScrollLock first (shared), then Modal / Drawer / DynamicTabs.
builder.Services.AddScoped<IFocusTrapService, FocusTrapService>();
builder.Services.AddScoped<IScrollLockService, ScrollLockService>();
builder.Services.AddScoped<ILipiModalService, LipiModalService>();
builder.Services.AddScoped<ILipiDrawerService, LipiDrawerService>();
builder.Services.AddScoped<ILipiDynamicTabsService, LipiDynamicTabsService>();

// ── Phase 2.7 Feedback Components (A35 — 2026-05-15) ───────────────────────
// SPEC:  docs/00-COMPONENTS/2.7/05-LipiToast-Spec.md §3
// AMEND: docs/CHANGE-LOG.md A35
//
// ILipiToastService dispatches and manages the active toast queue per Blazor
// circuit. Scoped lifetime — each user session has its own queue + state.
// The host (LipiToastHost in TopNavLayout.razor) subscribes to OnChanged and
// re-renders when state mutates.
//
// All Phase 2.7 components (LipiSpinner, LipiBadge, LipiPill, LipiSkeleton*,
// LipiValidationSummary, LipiToast) are pure-render Razor components — no DI
// registration needed beyond this service.
builder.Services.AddScoped<ILipiToastService, LipiToastService>();

// ── Phase 2.8 Data Display — table preference persistence (A39) ────────────
// SPEC:  docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §21.4
// AMEND: docs/CHANGE-LOG.md A38 (Option C architecture), A39 (wire-up)
//
// Option C (store abstraction). LiPi.Components ships the default high-level service
// (TablePreferenceService: JSON + 300ms debounce + per-circuit cache + silent errors +
// async dispose flush). LiPi.Web supplies only the two lower-level pieces:
//   • IUserTablePreferenceStore  → EfUserTablePreferenceStore (per-clinic IdentityDbContext)
//   • ICurrentUserAccessor       → BlazorCurrentUserAccessor (NameIdentifier claim)
//
// All three Scoped — one instance per Blazor circuit (single user + single clinic),
// which makes the accessor/store id-memoization and the service cache safe.
// Register the two low-level deps before the high-level service for readability
// (DI order does not matter for resolution).
builder.Services.AddScoped<ICurrentUserAccessor, BlazorCurrentUserAccessor>();
builder.Services.AddScoped<IUserTablePreferenceStore, EfUserTablePreferenceStore>();
builder.Services.AddScoped<ITablePreferenceService, TablePreferenceService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ── Seed ──────────────────────────────────────────────────────────────────
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    await ClinicSeeder.SeedAsync(app.Services, startupLogger);
    startupLogger.LogInformation("Database seeding complete");
}
catch (Exception ex)
{
    startupLogger.LogWarning(ex, "Seeding skipped");
}

// ── Middleware ─────────────────────────────────────────────────────────────
// Always show exceptions in dev
app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("Cookies");
    return Results.Redirect("/");
});

app.Run();
