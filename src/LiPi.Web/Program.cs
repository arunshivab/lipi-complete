using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using LiPi.Clinic.Identity;
using LiPi.Master;
using LiPi.Clinic.Audit;
using LiPi.Web;
using LiPi.Web.Components.Shared;
using LiPi.Web.Services;

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

// Always show detailed errors so we can diagnose issues
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
