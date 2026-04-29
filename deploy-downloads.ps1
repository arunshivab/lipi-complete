# =============================================================================
# LiPi HIS -- Deploy Downloads
# Run from project root:
#   C:\Users\aruns\Documents\lipi-complete\lipi-complete> .\deploy-downloads.ps1
#
# Save all Claude-generated files into Downloads\LiPi\ before running.
# Safe to run multiple times -- skips files not present in Downloads\LiPi.
# =============================================================================

$downloads = "$env:USERPROFILE\Downloads\LiPi"
$root      = $PSScriptRoot

# Create the LiPi folder automatically if it does not exist yet
if (!(Test-Path $downloads)) {
    New-Item -ItemType Directory -Path $downloads -Force | Out-Null
    Write-Host "  Created Downloads\LiPi folder" -ForegroundColor DarkGray
}

# Map: filename in Downloads\LiPi --> relative destination in project
$files = @{

    # Web app

    "_Imports.razor"                   = "src\LiPi.Web\_Imports.razor"
    "App.razor"                      = "src\LiPi.Web\App.razor"
    "CLAUDE.md"                      = "CLAUDE.md"

    # Pages
    "Roles.razor"                    = "src\LiPi.Web\Pages\Admin\Roles.razor"
    "Audit.razor"                    = "src\LiPi.Web\Pages\Admin\Audit.razor"
    "Dashboard.razor"                = "src\LiPi.Web\Pages\Dashboard.razor"
    "Admin.razor"                         = "src\LiPi.Web\Pages\Admin.razor"
    "Settings.razor"                    = "src\LiPi.Web\Pages\Admin\Settings.razor"
    "Users.razor"                    = "src\LiPi.Web\Pages\Admin\Users.razor"
    "UsersNew.razor"                 = "src\LiPi.Web\Pages\Admin\UsersNew.razor"
    "UserRights.razor"               = "src\\LiPi.Web\\Pages\\Admin\\UserRights.razor"
    "UserRoles.razor"                = "src\\LiPi.Web\\Pages\\Admin\\UserRoles.razor"
    "UsersEdit.razor"                = "src\LiPi.Web\Pages\Admin\UsersEdit.razor"
    "Clinics.razor"                     = "src\LiPi.Web\Pages\Admin\Clinics.razor"
    "Orgs.razor"                     = "src\LiPi.Web\Pages\Admin\Orgs.razor"
    "ClinicsEdit.razor"                 = "src\LiPi.Web\Pages\Admin\ClinicsEdit.razor"
    "OrgsEdit.razor"                 = "src\LiPi.Web\Pages\Admin\OrgsEdit.razor"
    "ClinicsNew.razor"               = "src\LiPi.Web\Pages\Admin\ClinicsNew.razor"
    "OrgsNew.razor"                  = "src\LiPi.Web\Pages\Admin\OrgsNew.razor"
    "PatientNew.razor"               = "src\LiPi.Web\Pages\Patients\PatientNew.razor"
    "Register.razor"                 = "src\LiPi.Web\Pages\Patients\Register.razor"
    "UhidSettings.razor"             = "src\LiPi.Web\Pages\Admin\UhidSettings.razor"
    "SchedulerSettings.razor"        = "src\LiPi.Web\Pages\Admin\SchedulerSettings.razor"
    "AppointmentCalendar.razor"      = "src\LiPi.Web\Pages\Patients\AppointmentCalendar.razor"
    "AppointmentBook.razor"          = "src\LiPi.Web\Pages\Patients\AppointmentBook.razor"
    "DuplicateDetectionService.cs"   = "src\LiPi.Web\Services\DuplicateDetectionService.cs"

    "Program.cs"                     = "src\LiPi.Web\Program.cs"
    # Session 13-14 — Patient Registration + Nav
    "PatientSearch.razor"            = "src\LiPi.Web\Pages\Patients\PatientSearch.razor"
    "PatientEdit.razor"              = "src\LiPi.Web\Pages\Patients\PatientEdit.razor"
    "PatientQueue.razor"             = "src\LiPi.Web\Pages\Patients\PatientQueue.razor"
    "PatientFab.razor"               = "src\LiPi.Web\Components\Shared\PatientFab.razor"
    "lipi-nav.js"                    = "src\LiPi.Web\wwwroot\js\lipi-nav.js"
    "Profile.razor"                  = "src\LiPi.Web\Pages\Profile.razor"

    # Services
    "AdminData.cs"                   = "src\LiPi.Web\Services\AdminData.cs"
    "ClinicSeeder.cs"                = "src\LiPi.Web\Services\ClinicSeeder.cs"
    "AuthService.cs"                 = "src\LiPi.Web\Services\AuthService.cs"
    "IAuthService.cs"               = "src\LiPi.Web\Services\IAuthService.cs"
    "Login.razor"                   = "src\LiPi.Web\Pages\Login.razor"
    "AuditService.cs"                = "src\LiPi.Web\Services\AuditService.cs"
    "AadhaarXmlService.cs"           = "src\LiPi.Web\Services\AadhaarXmlService.cs"

    # CSS + JavaScript
    # admin_pr_additions.css → managed via $appendFiles below (sentinel: .pr-body)
    "app.css"                        = "src\LiPi.Web\wwwroot\css\app.css"
    "dashboard.css"                  = "src\LiPi.Web\wwwroot\css\dashboard.css"

    # JavaScript
    "lipi-topnav.js"                 = "src\LiPi.Web\wwwroot\js\lipi-topnav.js"
    "lipi-theme.js"                  = "src\LiPi.Web\wwwroot\js\lipi-theme.js"

    # Layouts
    "TopNavLayout.razor"            = "src\LiPi.Web\Components\Layouts\TopNavLayout.razor"
    "TopNavLayout.razor.css"        = "src\LiPi.Web\Components\Layouts\TopNavLayout.razor.css"
    "LiPi-TopNav.css"               = "src\LiPi.Web\wwwroot\css\LiPi-TopNav.css"
    "UserDropdown.razor"             = "src\LiPi.Web\Components\UserDropdown.razor"
    "UserFab.razor"                  = "src\LiPi.Web\Components\UserFab.razor"
    "AddressBlock.razor"             = "src\LiPi.Web\Components\AddressBlock.razor"

    # Components
    "RedirectToLogin.razor"          = "src\LiPi.Web\Components\RedirectToLogin.razor"
    "MainLayout.razor"               = "src\LiPi.Web\Components\MainLayout.razor"
    "EmptyLayout.razor"              = "src\LiPi.Web\Components\Layouts\EmptyLayout.razor"
    "AdminLayout.razor"              = "src\LiPi.Web\Components\Layouts\AdminLayout.razor"
    "Module.razor"                   = "src\LiPi.Web\Pages\Module.razor"
    "lipi-login.js"                  = "src\LiPi.Web\wwwroot\js\lipi-login.js"

    # Machine components
    "MachineBrachy.razor"            = "src\LiPi.Web\Components\Machines\MachineBrachy.razor"
    "MachineCT.razor"                = "src\LiPi.Web\Components\Machines\MachineCT.razor"
    "MachineCathlab.razor"           = "src\LiPi.Web\Components\Machines\MachineCathlab.razor"
    "MachineLinac.razor"             = "src\LiPi.Web\Components\Machines\MachineLinac.razor"
    "MachineMRI.razor"               = "src\LiPi.Web\Components\Machines\MachineMRI.razor"
    "MachineOT.razor"                = "src\LiPi.Web\Components\Machines\MachineOT.razor"
    "MachinePETCT.razor"             = "src\LiPi.Web\Components\Machines\MachinePETCT.razor"

    # Production DB setup scripts (run in order)
    "prod-01-create-databases.sql"   = "prod-01-create-databases.sql"
    "prod-02-master-schema.sql"      = "prod-02-master-schema.sql"
    "prod-03-training-schema.sql"    = "prod-03-training-schema.sql"
    "prod-04-seed-master.sql"        = "prod-04-seed-master.sql"
    "prod-05-seed-training.sql"      = "prod-05-seed-training.sql"

    # Platform User schema (Jobs #1 + #2 refinement)
    "PlatformUser.cs"                = "database\efcore\LiPi.Master\Entities\PlatformUser.cs"
    "ClinicProfile.cs"               = "database\efcore\LiPi.Clinic.Identity\Entities\ClinicProfile.cs"
    "MasterDbContext.cs"             = "database\efcore\LiPi.Master\MasterDbContext.cs"

    "cleanup-identity-tables.sql"    = "cleanup-identity-tables.sql"
    "cleanup-identity-admins.sql"    = "cleanup-identity-admins.sql"

    "AssignToClinic.razor"           = "src\LiPi.Web\Pages\Admin\AssignToClinic.razor"
    "ClinicConfig.razor"              = "src\LiPi.Web\Pages\Admin\ClinicConfig.razor"
    "migrate-platform-users.sql"     = "migrate-platform-users.sql"
    "cleanup-admins.sql"             = "cleanup-admins.sql"

    # Email OTP + Forgot Password
    "IEmailService.cs"               = "src\LiPi.Web\Services\IEmailService.cs"
    "SmtpEmailService.cs"            = "src\LiPi.Web\Services\SmtpEmailService.cs"
    "ForgotPassword.razor"           = "src\LiPi.Web\Pages\ForgotPassword.razor"
    "ResetPassword.razor"            = "src\LiPi.Web\Pages\ResetPassword.razor"

    # Multi-tenant + Admin Hierarchy (Job #1 + #2)

    "drop-identity-users.sql"        = "drop-identity-users.sql"

    "ClinicConnectionService.cs"     = "src\LiPi.Web\Services\ClinicConnectionService.cs"
    "ClinicDbFactory.cs"             = "src\LiPi.Web\Services\ClinicDbFactory.cs"
    "ClaimsHelper.cs"                = "src\LiPi.Web\Services\ClaimsHelper.cs"
    "GlobalAdminBootstrap.cs"        = "src\LiPi.Web\Services\GlobalAdminBootstrap.cs"
    "SysAdminAutoAssignService.cs"   = "src\LiPi.Web\Services\SysAdminAutoAssignService.cs"
    "SysAdmins.razor"                = "src\LiPi.Web\Pages\Admin\SysAdmins.razor"
    "AdminList.razor"                = "src\LiPi.Web\Components\AdminList.razor"
    "SecurityPolicy.razor"           = "src\LiPi.Web\Pages\Admin\SecurityPolicy.razor"
    "SecurityPolicy.cs"              = "database\efcore\LiPi.Clinic.Identity\Entities\SecurityPolicy.cs"

    "LiPi.Web.csproj"                = "src\LiPi.Web\LiPi.Web.csproj"
    "OtpService.cs"                  = "src\LiPi.Web\Services\OtpService.cs"
    "VerifyOtp.razor"                = "src\LiPi.Web\Pages\VerifyOtp.razor"

    "ChangePassword.razor"             = "src\\LiPi.Web\\Pages\\ChangePassword.razor"
    "ClinicPicker.razor"             = "src\LiPi.Web\Pages\ClinicPicker.razor"
    "appsettings.json"               = "src\LiPi.Web\appsettings.json"
    "appsettings.Development.json"   = "src\LiPi.Web\appsettings.Development.json"
    "Clinic.cs"                      = "database\efcore\LiPi.Master\Entities\Clinic.cs"

    # Identity project
    "IdentityDbContext.cs"           = "database\efcore\LiPi.Clinic.Identity\IdentityDbContext.cs"
    "User.cs"                        = "database\efcore\LiPi.Clinic.Identity\Entities\User.cs"
    "Identity.cs"                    = "database\efcore\LiPi.Clinic.Identity\Entities\Identity.cs"

    # Audit project
    "AuditDbContext.cs"              = "database\efcore\LiPi.Clinic.Audit\AuditDbContext.cs"
    "AuditEntities.cs"               = "database\efcore\LiPi.Clinic.Audit\AuditEntities.cs"

    # Master project

    # csproj files
    "LiPi.Clinic.Core.csproj"        = "database\efcore\LiPi.Clinic.Core\LiPi.Clinic.Core.csproj"
    "LiPi.Master.csproj"             = "database\efcore\LiPi.Master\LiPi.Master.csproj"
    "LiPi.Clinic.Identity.csproj"    = "database\efcore\LiPi.Clinic.Identity\LiPi.Clinic.Identity.csproj"
    "LiPi.Clinic.Audit.csproj"       = "database\efcore\LiPi.Clinic.Audit\LiPi.Clinic.Audit.csproj"
    "LiPi.Clinic.Security.csproj"    = "database\efcore\LiPi.Clinic.Security\LiPi.Clinic.Security.csproj"
    "LiPi.Clinic.Compliance.csproj"  = "database\efcore\LiPi.Clinic.Compliance\LiPi.Clinic.Compliance.csproj"
    "LiPi.Clinic.Abdm.csproj"        = "database\efcore\LiPi.Clinic.Abdm\LiPi.Clinic.Abdm.csproj"
    "LiPi.Clinic.Certs.csproj"       = "database\efcore\LiPi.Clinic.Certs\LiPi.Clinic.Certs.csproj"
    "LiPi.Clinic.Sigma.csproj"       = "database\efcore\LiPi.Clinic.Sigma\LiPi.Clinic.Sigma.csproj"

    # ── Session 12 — Patient Registration module ─────────────────────────────
    # PatientNew.razor and Register.razor already in map above (lines 45-46)
    # App.razor already in map above (line 25)

}

Write-Host ""
Write-Host "  LiPi HIS -- Deploy from Downloads\LiPi" -ForegroundColor Cyan
Write-Host "  ----------------------------------------" -ForegroundColor DarkGray
Write-Host ""

$copied  = 0
$skipped = 0

foreach ($filename in $files.Keys) {
    $src  = Join-Path $downloads $filename
    $dest = Join-Path $root $files[$filename]

    if (Test-Path $src) {
        $destDir = Split-Path $dest -Parent
        if (!(Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        Copy-Item -Path $src -Destination $dest -Force
        Write-Host "  OK  $filename" -ForegroundColor Green
        Write-Host "      $($files[$filename])" -ForegroundColor DarkGray
        $copied++
    } else {
        Write-Host "  --  $filename (not found, skipped)" -ForegroundColor DarkYellow
        $skipped++
    }
}

# =============================================================================
# APPEND-ONLY files — content added to an existing target, not replaced.
# Pre-clean: strip any previously appended blocks (identified by sentinels)
# so the append is always applied to a clean base admin.css.
# =============================================================================

# Strip previously appended CSS blocks from admin.css before re-appending
$adminCssDest = Join-Path $root "src\LiPi.Web\wwwroot\css\admin.css"
if (Test-Path $adminCssDest) {
    $adminContent = Get-Content $adminCssDest -Raw -Encoding UTF8
    # Truncate at the first sentinel (start of appended content)
    $firstSentinel = @(".pr-body", ".tn-dock-flat", ".ps-body") |
        ForEach-Object { $adminContent.IndexOf($_) } |
        Where-Object { $_ -gt 0 } |
        Sort-Object |
        Select-Object -First 1
    if ($firstSentinel) {
        # Find the comment block start (/* ===) before the sentinel
        $searchFrom = [Math]::Max(0, $firstSentinel - 500)
        $blockStart = $adminContent.IndexOf("/* ═", $searchFrom)
        if ($blockStart -lt 0) { $blockStart = $firstSentinel }
        $cleaned = $adminContent.Substring(0, $blockStart).TrimEnd()
        Set-Content -Path $adminCssDest -Value $cleaned -Encoding UTF8 -NoNewline
        Write-Host "  CL  admin.css stripped of previous appended blocks" -ForegroundColor DarkCyan
    }
}

$appendFiles = @{
    "admin_pr_additions.css" = @{
        Dest     = "src\LiPi.Web\wwwroot\css\admin.css"
        Sentinel = ".pr-body"
    }
    "admin_nav_updates.css" = @{
        Dest     = "src\LiPi.Web\wwwroot\css\admin.css"
        Sentinel = ".tn-dock-flat"
    }
    "admin_pse_additions.css" = @{
        Dest     = "src\LiPi.Web\wwwroot\css\admin.css"
        Sentinel = ".ps-body"
    }
    "admin_title_fix.css" = @{
        Dest     = "src\LiPi.Web\wwwroot\css\admin.css"
        Sentinel = ".tn-pt-title-fix"
    }
    "admin_sched_additions.css" = @{
        Dest     = "src\LiPi.Web\wwwroot\css\admin.css"
        Sentinel = ".sc-body"
    }
}

$appended = 0

foreach ($filename in $appendFiles.Keys) {
    $src      = Join-Path $downloads $filename
    $entry    = $appendFiles[$filename]
    $dest     = Join-Path $root $entry.Dest
    $sentinel = $entry.Sentinel

    if (Test-Path $src) {
        if (Test-Path $dest) {
            $existing = Get-Content $dest -Raw -Encoding UTF8
            if ($existing -notlike "*$sentinel*") {
                $addition = Get-Content $src -Raw -Encoding UTF8
                Add-Content -Path $dest -Value "`n$addition" -Encoding UTF8
                Write-Host "  AP  $filename -> $($entry.Dest)" -ForegroundColor Cyan
                $appended++
            } else {
                Write-Host "  SK  $filename already in target (sentinel found), skipped" -ForegroundColor DarkYellow
            }
        } else {
            Write-Host "  !!  $filename target not found: $($entry.Dest)" -ForegroundColor Red
        }
    } else {
        Write-Host "  --  $filename (not in Downloads\LiPi, skipped)" -ForegroundColor DarkYellow
    }
}

Write-Host ""
Write-Host "  Done. $copied deployed, $appended appended, $skipped skipped." -ForegroundColor Cyan
Write-Host "  Source: $downloads" -ForegroundColor DarkGray
Write-Host ""
