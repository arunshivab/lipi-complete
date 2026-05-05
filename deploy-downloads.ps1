# =============================================================================
# LiPi HIS -- Deploy from Downloads\LiPi
#
# Run from project root:
#   C:\Users\aruns\Documents\lipi-complete\lipi-complete> .\deploy-downloads.ps1
#
# Drop any file into Downloads\LiPi\ and run.
# Safe to run multiple times -- skips files not in Downloads\LiPi.
# No appending. No sentinels. No patching. One file in = one file deployed.
# =============================================================================

$downloads = "$env:USERPROFILE\Downloads\LiPi"
$root      = $PSScriptRoot

if (!(Test-Path $downloads)) {
    New-Item -ItemType Directory -Path $downloads -Force | Out-Null
    Write-Host "  Created $downloads" -ForegroundColor DarkGray
}

$files = @{

    # -- Web app core ----------------------------------------------------------
    "_Imports.razor"                 = "src\LiPi.Web\_Imports.razor"
    "App.razor"                      = "src\LiPi.Web\App.razor"
    "Program.cs"                     = "src\LiPi.Web\Program.cs"
    "CLAUDE.md"                      = "CLAUDE.md"

    # -- CSS / JS --------------------------------------------------------------
    "admin.css"                      = "src\LiPi.Web\wwwroot\css\admin.css"
    "app.css"                        = "src\LiPi.Web\wwwroot\css\app.css"
    "dashboard.css"                  = "src\LiPi.Web\wwwroot\css\dashboard.css"
    "LiPi-TopNav.css"                = "src\LiPi.Web\wwwroot\css\LiPi-TopNav.css"
    "lipi-topnav.js"                 = "src\LiPi.Web\wwwroot\js\lipi-topnav.js"
    "lipi-theme.js"                  = "src\LiPi.Web\wwwroot\js\lipi-theme.js"
    "lipi-shortcuts.js"              = "src\LiPi.Web\wwwroot\js\lipi-shortcuts.js"
    "lipi-login.js"                  = "src\LiPi.Web\wwwroot\js\lipi-login.js"
    "lipi-nav.js"                    = "src\LiPi.Web\wwwroot\js\lipi-nav.js"

    # -- CSS / JS -- Phase 1 Theme (Decision #12) ------------------------------
    "theme-switcher.js"              = "src\LiPi.Web\wwwroot\js\theme-switcher.js"
    "brand-lipi.css"                 = "src\LiPi.Web\wwwroot\themes\brand-lipi.css"
    "mode-light.css"                 = "src\LiPi.Web\wwwroot\themes\mode-light.css"
    "mode-dark.css"                  = "src\LiPi.Web\wwwroot\themes\mode-dark.css"

    # -- CSS -- Phase 2.0 Foundation (Decision #12, Sub-step 2.0) -------------
    "00-baseline.css"                = "src\LiPi.Web\wwwroot\css\00-baseline.css"

    # -- CSS / JS -- Phase 2.2 input family shared base (Batches 3-5) ----------
    # SPEC: docs/00-COMPONENTS/01.2-TextInputs.md (shipped Batch 6)
    # AMEND: CHANGE-LOG.md A13 (Batch 3) + A15 (Batches 4-5.4)
    "lipi-inputs.css"                = "src\LiPi.Web\wwwroot\css\lipi-inputs.css"
    "lipi-input.js"                  = "src\LiPi.Web\wwwroot\js\lipi-input.js"

    # -- Layouts & shared components -------------------------------------------
    "TopNavLayout.razor"             = "src\LiPi.Web\Components\Layouts\TopNavLayout.razor"
    "TopNavLayout.razor.css"         = "src\LiPi.Web\Components\Layouts\TopNavLayout.razor.css"
    "MainLayout.razor"               = "src\LiPi.Web\Components\MainLayout.razor"
    "EmptyLayout.razor"              = "src\LiPi.Web\Components\Layouts\EmptyLayout.razor"
    "AdminLayout.razor"              = "src\LiPi.Web\Components\Layouts\AdminLayout.razor"
    "RedirectToLogin.razor"          = "src\LiPi.Web\Components\RedirectToLogin.razor"
    "UserDropdown.razor"             = "src\LiPi.Web\Components\UserDropdown.razor"
    "UserFab.razor"                  = "src\LiPi.Web\Components\UserFab.razor"
    "AddressBlock.razor"             = "src\LiPi.Web\Components\AddressBlock.razor"
    "AdminList.razor"                = "src\LiPi.Web\Components\AdminList.razor"
    "PatientFab.razor"               = "src\LiPi.Web\Components\Shared\PatientFab.razor"

    # -- Components -- Theme (Phase 1, Decision #12) ---------------------------
    "ThemeProvider.razor"            = "src\LiPi.Web\Components\Theme\ThemeProvider.razor"
    "ThemeContext.cs"                = "src\LiPi.Web\Components\Theme\ThemeContext.cs"

    # -- Components -- Phase 2.1 (LipiButton + companions) --------------------
    "LipiButton.razor"               = "src\LiPi.Web\Components\Shared\LipiButton.razor"
    "LipiButton.razor.css"           = "src\LiPi.Web\Components\Shared\LipiButton.razor.css"
    "LipiButtonTypes.cs"             = "src\LiPi.Web\Components\Shared\LipiButtonTypes.cs"
    "LipiButtonSpinner.razor"        = "src\LiPi.Web\Components\Shared\LipiButtonSpinner.razor"
    "LucideIcon.razor"               = "src\LiPi.Web\Components\Shared\LucideIcon.razor"

    # -- Components -- Phase 2.2 (LipiTextBox + companions, LipiTextArea, LipiNumberInput, LipiSelect/LipiCombobox) --
    # Batch 1 (May 4): theme tokens (A12 in CHANGE-LOG.md)
    # Batch 2 (May 4): LipiTextBox + companions
    # Batch 3 (May 5): LipiTextArea + lipi-inputs.css extraction (A13 pending)
    # Batch 4 (May 5): LipiNumberInput<TValue> + lipi-input.js selectAll helper (A14 pending)
    # Batch 4.1 (May 5): arrow keys + EditForm test page
    # Batch 4.2 (May 5): DisableArrowKeys + BlockNonNumericInput + LipiButton hotfix
    # Batch 4.3 (May 5): JS DOM value-sync (closes value-sync issue from 4.2 ship notes)
    # Batch 5 (May 5): LipiSelect<TValue> + LipiCombobox<TValue,TItem> + LipiSelectBase + dropdown JS
    "LipiTextBox.razor"              = "src\LiPi.Web\Components\Shared\LipiTextBox.razor"
    "LipiTextBox.razor.css"          = "src\LiPi.Web\Components\Shared\LipiTextBox.razor.css"
    "LipiTextArea.razor"             = "src\LiPi.Web\Components\Shared\LipiTextArea.razor"
    "LipiTextArea.razor.css"         = "src\LiPi.Web\Components\Shared\LipiTextArea.razor.css"
    "LipiNumberInput.razor"          = "src\LiPi.Web\Components\Shared\LipiNumberInput.razor"
    "LipiNumberInput.razor.css"      = "src\LiPi.Web\Components\Shared\LipiNumberInput.razor.css"
    "LipiSelectBase.cs"              = "src\LiPi.Web\Components\Shared\LipiSelectBase.cs"
    "LipiSelect.razor"               = "src\LiPi.Web\Components\Shared\LipiSelect.razor"
    "LipiSelect.razor.css"           = "src\LiPi.Web\Components\Shared\LipiSelect.razor.css"
    "LipiCombobox.razor"             = "src\LiPi.Web\Components\Shared\LipiCombobox.razor"
    "LipiCombobox.razor.css"         = "src\LiPi.Web\Components\Shared\LipiCombobox.razor.css"
    "LipiTextInputTypes.cs"          = "src\LiPi.Web\Components\Shared\LipiTextInputTypes.cs"
    "LipiInputDefaults.cs"           = "src\LiPi.Web\Components\Shared\LipiInputDefaults.cs"
    "AutocompleteValidator.cs"       = "src\LiPi.Web\Components\Shared\AutocompleteValidator.cs"

    # -- Pages -- Admin --------------------------------------------------------
    "Admin.razor"                    = "src\LiPi.Web\Pages\Admin.razor"
    "Dashboard.razor"                = "src\LiPi.Web\Pages\Dashboard.razor"
    "Module.razor"                   = "src\LiPi.Web\Pages\Module.razor"
    "Profile.razor"                  = "src\LiPi.Web\Pages\Profile.razor"
    "Settings.razor"                 = "src\LiPi.Web\Pages\Admin\Settings.razor"
    "Audit.razor"                    = "src\LiPi.Web\Pages\Admin\Audit.razor"
    "Roles.razor"                    = "src\LiPi.Web\Pages\Admin\Roles.razor"
    "Users.razor"                    = "src\LiPi.Web\Pages\Admin\Users.razor"
    "UsersNew.razor"                 = "src\LiPi.Web\Pages\Admin\UsersNew.razor"
    "UsersEdit.razor"                = "src\LiPi.Web\Pages\Admin\UsersEdit.razor"
    "UserRoles.razor"                = "src\LiPi.Web\Pages\Admin\UserRoles.razor"
    "UserRights.razor"               = "src\LiPi.Web\Pages\Admin\UserRights.razor"
    "Clinics.razor"                  = "src\LiPi.Web\Pages\Admin\Clinics.razor"
    "ClinicsNew.razor"               = "src\LiPi.Web\Pages\Admin\ClinicsNew.razor"
    "ClinicsEdit.razor"              = "src\LiPi.Web\Pages\Admin\ClinicsEdit.razor"
    "Orgs.razor"                     = "src\LiPi.Web\Pages\Admin\Orgs.razor"
    "OrgsNew.razor"                  = "src\LiPi.Web\Pages\Admin\OrgsNew.razor"
    "OrgsEdit.razor"                 = "src\LiPi.Web\Pages\Admin\OrgsEdit.razor"
    "AssignToClinic.razor"           = "src\LiPi.Web\Pages\Admin\AssignToClinic.razor"
    "ClinicConfig.razor"             = "src\LiPi.Web\Pages\Admin\ClinicConfig.razor"
    "SysAdmins.razor"                = "src\LiPi.Web\Pages\Admin\SysAdmins.razor"
    "SecurityPolicy.razor"           = "src\LiPi.Web\Pages\Admin\SecurityPolicy.razor"
    "AspirationalDistricts.razor"    = "src\LiPi.Web\Pages\Admin\AspirationalDistricts.razor"
    "UhidSettings.razor"             = "src\LiPi.Web\Pages\Admin\UhidSettings.razor"
    "SchedulerSettings.razor"        = "src\LiPi.Web\Pages\Admin\SchedulerSettings.razor"

    # -- Pages -- Phase 2.0 (Style Guide, Decision #12 Sub-step 2.0) ----------
    "StyleGuide.razor"               = "src\LiPi.Web\Pages\StyleGuide.razor"
    "StyleGuide.razor.css"           = "src\LiPi.Web\Pages\StyleGuide.razor.css"

    # -- Pages -- Test scaffolds (Phase 2.2 verification — coexist alongside the
    # StyleGuide showcase. Test pages exercise edge cases (200-item virtualization,
    # locale comparison, etc.) that are too dense for the showcase grid. The
    # StyleGuide showcase has the polished demo; these test pages stay for
    # regression verification.) --
    # Filename uppercase to satisfy Razor compiler (RZ10011 — component names cannot start
    # with lowercase). Route preserved via @page directive.
    "TextareaTest.razor"             = "src\LiPi.Web\Pages\Test\TextareaTest.razor"
    "NumberInputTest.razor"          = "src\LiPi.Web\Pages\Test\NumberInputTest.razor"
    "SelectTest.razor"               = "src\LiPi.Web\Pages\Test\SelectTest.razor"

    # -- Pages -- Auth ---------------------------------------------------------
    "Login.razor"                    = "src\LiPi.Web\Pages\Login.razor"
    "ForgotPassword.razor"           = "src\LiPi.Web\Pages\ForgotPassword.razor"
    "ResetPassword.razor"            = "src\LiPi.Web\Pages\ResetPassword.razor"
    "VerifyOtp.razor"                = "src\LiPi.Web\Pages\VerifyOtp.razor"
    "ChangePassword.razor"           = "src\LiPi.Web\Pages\ChangePassword.razor"
    "ClinicPicker.razor"             = "src\LiPi.Web\Pages\ClinicPicker.razor"

    # -- Pages -- Patients -----------------------------------------------------
    "PatientNew.razor"               = "src\LiPi.Web\Pages\Patients\PatientNew.razor"
    "PatientEdit.razor"              = "src\LiPi.Web\Pages\Patients\PatientEdit.razor"
    "PatientSearch.razor"            = "src\LiPi.Web\Pages\Patients\PatientSearch.razor"
    "PatientQueue.razor"             = "src\LiPi.Web\Pages\Patients\PatientQueue.razor"
    "Register.razor"                 = "src\LiPi.Web\Pages\Patients\Register.razor"
    "AppointmentCalendar.razor"      = "src\LiPi.Web\Pages\Patients\AppointmentCalendar.razor"
    "AppointmentBook.razor"          = "src\LiPi.Web\Pages\Patients\AppointmentBook.razor"

    # -- Machine components ----------------------------------------------------
    "MachineBrachy.razor"            = "src\LiPi.Web\Components\Machines\MachineBrachy.razor"
    "MachineCT.razor"                = "src\LiPi.Web\Components\Machines\MachineCT.razor"
    "MachineCathlab.razor"           = "src\LiPi.Web\Components\Machines\MachineCathlab.razor"
    "MachineLinac.razor"             = "src\LiPi.Web\Components\Machines\MachineLinac.razor"
    "MachineMRI.razor"               = "src\LiPi.Web\Components\Machines\MachineMRI.razor"
    "MachineOT.razor"                = "src\LiPi.Web\Components\Machines\MachineOT.razor"
    "MachinePETCT.razor"             = "src\LiPi.Web\Components\Machines\MachinePETCT.razor"

    # -- Services --------------------------------------------------------------
    "AdminData.cs"                   = "src\LiPi.Web\Services\AdminData.cs"
    "AuthService.cs"                 = "src\LiPi.Web\Services\AuthService.cs"
    "IAuthService.cs"                = "src\LiPi.Web\Services\IAuthService.cs"
    "AuditService.cs"                = "src\LiPi.Web\Services\AuditService.cs"
    "AadhaarXmlService.cs"           = "src\LiPi.Web\Services\AadhaarXmlService.cs"
    "ClinicSeeder.cs"                = "src\LiPi.Web\Services\ClinicSeeder.cs"
    "ClinicConnectionService.cs"     = "src\LiPi.Web\Services\ClinicConnectionService.cs"
    "ClinicDbFactory.cs"             = "src\LiPi.Web\Services\ClinicDbFactory.cs"
    "ClaimsHelper.cs"                = "src\LiPi.Web\Services\ClaimsHelper.cs"
    "GlobalAdminBootstrap.cs"        = "src\LiPi.Web\Services\GlobalAdminBootstrap.cs"
    "SysAdminAutoAssignService.cs"   = "src\LiPi.Web\Services\SysAdminAutoAssignService.cs"
    "DuplicateDetectionService.cs"   = "src\LiPi.Web\Services\DuplicateDetectionService.cs"
    "IEmailService.cs"               = "src\LiPi.Web\Services\IEmailService.cs"
    "SmtpEmailService.cs"            = "src\LiPi.Web\Services\SmtpEmailService.cs"
    "OtpService.cs"                  = "src\LiPi.Web\Services\OtpService.cs"

    # -- Services -- Theme (Phase 1, Decision #12) -----------------------------
    "IUserPreferenceService.cs"      = "src\LiPi.Web\Services\IUserPreferenceService.cs"
    "UserPreferenceService.cs"       = "src\LiPi.Web\Services\UserPreferenceService.cs"
    "IThemeContextService.cs"        = "src\LiPi.Web\Services\IThemeContextService.cs"
    "ThemeContextService.cs"         = "src\LiPi.Web\Services\ThemeContextService.cs"

    # -- Config ----------------------------------------------------------------
    "appsettings.json"               = "src\LiPi.Web\appsettings.json"
    "appsettings.Development.json"   = "src\LiPi.Web\appsettings.Development.json"

    # -- EF Core -- Master DB --------------------------------------------------
    "MasterDbContext.cs"             = "database\efcore\LiPi.Master\MasterDbContext.cs"
    "PlatformUser.cs"                = "database\efcore\LiPi.Master\Entities\PlatformUser.cs"
    "Clinic.cs"                      = "database\efcore\LiPi.Master\Entities\Clinic.cs"
    "AspirationalDistrict.cs"        = "database\efcore\LiPi.Master\Entities\AspirationalDistrict.cs"

    # -- EF Core -- Master DB -- Phase 1 Theme (Decision #12) -----------------
    "BrandTheme.cs"                  = "database\efcore\LiPi.Master\Entities\BrandTheme.cs"

    # -- EF Core -- Clinic Core DB ---------------------------------------------
    "ClinicCoreDbContext.cs"         = "database\efcore\LiPi.Clinic.Core\ClinicCoreDbContext.cs"
    "Patient.cs"                     = "database\efcore\LiPi.Clinic.Core\Entities\Patient.cs"
    "ContactPoint.cs"                = "database\efcore\LiPi.Clinic.Core\Entities\ContactPoint.cs"
    "PatientAddress.cs"              = "database\efcore\LiPi.Clinic.Core\Entities\PatientAddress.cs"
    "PatientIdentifier.cs"           = "database\efcore\LiPi.Clinic.Core\Entities\PatientIdentifier.cs"
    "PatientConsent.cs"              = "database\efcore\LiPi.Clinic.Core\Entities\PatientConsent.cs"

    # -- EF Core -- Clinic Identity DB -----------------------------------------
    "IdentityDbContext.cs"           = "database\efcore\LiPi.Clinic.Identity\IdentityDbContext.cs"
    "UserRole.cs"                    = "database\efcore\LiPi.Clinic.Identity\Entities\UserRole.cs"
    "AdSyncLog.cs"                   = "database\efcore\LiPi.Clinic.Identity\Entities\AdSyncLog.cs"

    # -- EF Core -- Clinic Identity DB -- Phase 1 Theme (Decision #12) --------
    "UserPreference.cs"              = "database\efcore\LiPi.Clinic.Identity\Entities\UserPreference.cs"

    # -- EF Core -- project files ----------------------------------------------
    "LiPi.Master.csproj"             = "database\efcore\LiPi.Master\LiPi.Master.csproj"
    "LiPi.Clinic.Core.csproj"        = "database\efcore\LiPi.Clinic.Core\LiPi.Clinic.Core.csproj"
    "LiPi.Clinic.Identity.csproj"    = "database\efcore\LiPi.Clinic.Identity\LiPi.Clinic.Identity.csproj"
    "LiPi.Clinic.Compliance.csproj"  = "database\efcore\LiPi.Clinic.Compliance\LiPi.Clinic.Compliance.csproj"
    "LiPi.Clinic.Abdm.csproj"        = "database\efcore\LiPi.Clinic.Abdm\LiPi.Clinic.Abdm.csproj"
    "LiPi.Clinic.Certs.csproj"       = "database\efcore\LiPi.Clinic.Certs\LiPi.Clinic.Certs.csproj"
    "LiPi.Clinic.Sigma.csproj"       = "database\efcore\LiPi.Clinic.Sigma\LiPi.Clinic.Sigma.csproj"

    # -- SQL -- Clinic schema (run manually via pgAdmin / psql) ----------------
    # Order: 01_core_v3.sql -> 02_geodata_seed.sql -> 02_identity.sql
    "01_core_v3.sql"                 = "database\clinic\01_core_v3.sql"
    "02_geodata_seed.sql"            = "database\clinic\02_geodata_seed.sql"
    "02_identity.sql"                = "database\clinic\02_identity.sql"

    # -- SQL -- Production setup (run once in order) ---------------------------
    "prod-01-create-databases.sql"   = "prod-01-create-databases.sql"
    "prod-02-master-schema.sql"      = "prod-02-master-schema.sql"
    "prod-03-training-schema.sql"    = "prod-03-training-schema.sql"
    "prod-04-seed-master.sql"        = "prod-04-seed-master.sql"
    "prod-05-seed-training.sql"      = "prod-05-seed-training.sql"

    # -- SQL -- One-off migrations (run manually) ------------------------------
    "cleanup-identity-tables.sql"    = "cleanup-identity-tables.sql"
    "cleanup-identity-admins.sql"    = "cleanup-identity-admins.sql"
    "migrate-platform-users.sql"     = "migrate-platform-users.sql"
    "cleanup-admins.sql"             = "cleanup-admins.sql"
    "drop-identity-users.sql"        = "drop-identity-users.sql"

    # -- SQL -- Phase 1 migrations (Decision #12) ------------------------------
    # Run PART A on master DB first, then PART B on each clinic DB
    "2026-05-02-decision-12-theming-up.sql"   = "database\migrations\2026-05-02-decision-12-theming-up.sql"
    "2026-05-02-decision-12-theming-down.sql" = "database\migrations\2026-05-02-decision-12-theming-down.sql"

    # -- Docs -- Component Library ---------------------------------------------
    "00.2-THEMING-ARCHITECTURE.md"   = "docs\00-COMPONENTS\00.2-THEMING-ARCHITECTURE.md"
    "01.1-Buttons.md"                = "docs\00-COMPONENTS\01.1-Buttons.md"
    "01.2-TextInputs.md"             = "docs\00-COMPONENTS\01.2-TextInputs.md"

    # -- Docs -- Top-level -----------------------------------------------------
    "CHANGE-LOG.md"                  = "docs\CHANGE-LOG.md"
}

# -- Deploy -------------------------------------------------------------------
Write-Host ""
Write-Host "  LiPi HIS -- Deploy from Downloads\LiPi" -ForegroundColor Cyan
Write-Host "  Source: $downloads" -ForegroundColor DarkGray
Write-Host ""

$deployed = 0
$skipped  = 0

foreach ($filename in ($files.Keys | Sort-Object)) {
    $src  = Join-Path $downloads $filename
    $dest = Join-Path $root $files[$filename]

    if (Test-Path $src) {
        $destDir = Split-Path $dest -Parent
        if (!(Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }
        Copy-Item -Path $src -Destination $dest -Force
        Write-Host "  OK  $filename" -ForegroundColor Green
        $deployed++
    } else {
        $skipped++
    }
}

Write-Host ""
Write-Host "  Done. $deployed deployed, $skipped skipped." -ForegroundColor Cyan
Write-Host "  Source: $downloads" -ForegroundColor DarkGray
Write-Host ""
