using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using LiPi.Clinic.Identity;
using LiPi.Master;
using LiPi.Clinic.Audit;
using LiPi.Web;
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
