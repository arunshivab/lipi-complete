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

    # -- Phase 2.6.1 — Layout components (LipiTabs + LipiAlert + LipiCard) ----
    # LipiTabs (Step 1): Underline/Pill/Vertical variants, TabState, IconOnly,
    #   keyboard nav (WAI-ARIA Pattern A), TabShortcutPattern, RenderMode Lazy/Eager
    "LipiTabsTypes.cs"               = "src\LiPi.Web\Components\Shared\LipiTabsTypes.cs"
    "LipiTabRegistration.cs"         = "src\LiPi.Web\Components\Shared\LipiTabRegistration.cs"
    "LipiTabs.razor"                 = "src\LiPi.Web\Components\Shared\LipiTabs.razor"
    "LipiTabs.razor.css"             = "src\LiPi.Web\Components\Shared\LipiTabs.razor.css"
    "LipiTab.razor"                  = "src\LiPi.Web\Components\Shared\LipiTab.razor"
    "lipi-tabs.css"                  = "src\LiPi.Web\wwwroot\css\lipi-tabs.css"

    # LipiAlert (Step 2): 5 severities, 4 styles, auto-dismiss + progress bar,
    #   AlertActions slot, Critical safety rules (no ✕, no auto-dismiss, Outline→Filled)
    "LipiAlertTypes.cs"              = "src\LiPi.Web\Components\Shared\LipiAlertTypes.cs"
    "LipiAlert.razor"                = "src\LiPi.Web\Components\Shared\LipiAlert.razor"
    "LipiAlert.razor.css"            = "src\LiPi.Web\Components\Shared\LipiAlert.razor.css"
    "lipi-alerts.css"                = "src\LiPi.Web\wwwroot\css\lipi-alerts.css"

    # -- Phase 2.6.1 — LipiCard --------------------------------------------------
    "LipiCardTypes.cs"               = "src\LiPi.Web\Components\Shared\LipiCardTypes.cs"
    "LipiCard.razor"                 = "src\LiPi.Web\Components\Shared\LipiCard.razor"
    "LipiCard.razor.css"             = "src\LiPi.Web\Components\Shared\LipiCard.razor.css"
    "CardHeader.razor"               = "src\LiPi.Web\Components\Shared\CardHeader.razor"
    "CardBody.razor"                 = "src\LiPi.Web\Components\Shared\CardBody.razor"
    "CardFooter.razor"               = "src\LiPi.Web\Components\Shared\CardFooter.razor"
    "lipi-cards.css"                 = "src\LiPi.Web\wwwroot\css\lipi-cards.css"

    # -- Phase 2.6.2 — Overlay Surfaces (Modal + Drawer + DynamicTabs) -----------
    # AMENDMENTS:
    #   A30 (2026-05-14): LipiModalTypes.cs recovered from spec §4 after file loss
    #   A31 (2026-05-14): LipiModal.razor family added (declarative path per spec §2)
    #   A32 (2026-05-14): CSS fallbacks, modal stack guard fix, drawer 6 missing
    #                     params + pin mode completion, dynamic tabs IDisposable +
    #                     MaxTabs cap + arrow keys, overlay host drawer-via-LipiDrawer
    #                     refactor + aria-live writes + drawer-on-modal warning.
    #                     Files revised: lipi-overlays.css, lipi-dynamic-tabs.css,
    #                     LipiModalService.cs, ILipiDynamicTabsService.cs,
    #                     LipiDynamicTabsService.cs, LipiDynamicTabs.razor,
    #                     LipiOverlayHost.razor, LipiDrawer.razor.
    #   A33 (2026-05-14): StyleGuideOverlays showcase rewritten — 30-demo coverage
    #                     across Modal §12 (11) + Drawer §11 (8) + DynamicTabs §14
    #                     (11). New helper components: SampleCustomModal.razor +
    #                     SampleDrawerPanel.razor (ShowAsync demo bodies);
    #                     StyleGuideOverlayTabDemo.razor stub @page for dtabs nav
    #                     demos. Existing 5–6 stub demos discarded wholesale.
    #   A34 (2026-05-14): DynamicTabs overflow redesign — native scrollbar
    #                     replaced with chevron buttons on both ends per spec
    #                     §8 amendment. Hidden when no overflow, greyed at edges,
    #                     page-width click scroll, hold-to-scroll with accel.
    #                     Files revised: lipi-dynamic-tabs.css (wrapper + chevron
    #                     rules, scrollbar hidden), LipiDynamicTabs.razor (wrapper
    #                     markup, JS interop wiring, DisposeAsync cleanup),
    #                     lipi-overlay-interop.js (new lipiDtabs namespace).
    #                     Spec doc 03-LipiDynamicTabs-Spec.md §8 amended.
    #                     Folded into A34: A29-pattern deploy-mapping fix —
    #                     four Phase 2.6.2 spec docs (00-Phase2.6.2-Overview,
    #                     01-LipiModal-Spec, 02-LipiDrawer-Spec,
    #                     03-LipiDynamicTabs-Spec) added to the Docs section
    #                     under docs\00-COMPONENTS\2.6.2\.
    # Shared infrastructure
    "IFocusTrapService.cs"           = "src\LiPi.Web\Services\IFocusTrapService.cs"
    "FocusTrapService.cs"            = "src\LiPi.Web\Services\FocusTrapService.cs"
    "IScrollLockService.cs"          = "src\LiPi.Web\Services\IScrollLockService.cs"
    "ScrollLockService.cs"           = "src\LiPi.Web\Services\ScrollLockService.cs"
    "lipi-overlay-interop.js"        = "src\LiPi.Web\wwwroot\js\lipi-overlay-interop.js"
    "LipiOverlayHost.razor"          = "src\LiPi.Web\Components\Shared\LipiOverlayHost.razor"
    # LipiModal
    # NOTE: LipiModalTypes.cs rebuilt 2026-05-14 from spec §4 after loss (A30).
    # NOTE: LipiModal.razor family added 2026-05-14 (A31) — declarative path
    #       per spec §2. Single-file (no .razor.cs) matching project pattern.
    "LipiModalTypes.cs"              = "src\LiPi.Web\Components\Shared\LipiModalTypes.cs"
    "LipiModal.razor"                = "src\LiPi.Web\Components\Shared\LipiModal.razor"
    "LipiModal.razor.css"            = "src\LiPi.Web\Components\Shared\LipiModal.razor.css"
    "ModalBody.razor"                = "src\LiPi.Web\Components\Shared\ModalBody.razor"
    "ModalFooter.razor"              = "src\LiPi.Web\Components\Shared\ModalFooter.razor"
    # A33: StyleGuide demo body for Modal.ShowAsync<T, string?>
    "SampleCustomModal.razor"        = "src\LiPi.Web\Components\Shared\SampleCustomModal.razor"
    "ILipiModalService.cs"           = "src\LiPi.Web\Services\ILipiModalService.cs"
    "LipiModalService.cs"            = "src\LiPi.Web\Services\LipiModalService.cs"
    "ConfirmDialog.razor"            = "src\LiPi.Web\Components\Shared\ConfirmDialog.razor"
    "AlertDialog.razor"              = "src\LiPi.Web\Components\Shared\AlertDialog.razor"
    "PromptDialog.razor"             = "src\LiPi.Web\Components\Shared\PromptDialog.razor"
    # LipiDrawer
    "LipiDrawerTypes.cs"             = "src\LiPi.Web\Components\Shared\LipiDrawerTypes.cs"
    "ILipiDrawerService.cs"          = "src\LiPi.Web\Services\ILipiDrawerService.cs"
    "LipiDrawerService.cs"           = "src\LiPi.Web\Services\LipiDrawerService.cs"
    "LipiDrawer.razor"               = "src\LiPi.Web\Components\Shared\LipiDrawer.razor"
    "LipiDrawer.razor.css"           = "src\LiPi.Web\Components\Shared\LipiDrawer.razor.css"
    "DrawerBody.razor"               = "src\LiPi.Web\Components\Shared\DrawerBody.razor"
    "DrawerFooter.razor"             = "src\LiPi.Web\Components\Shared\DrawerFooter.razor"
    # A33: StyleGuide demo body for Drawer.ShowAsync<T, bool>
    "SampleDrawerPanel.razor"        = "src\LiPi.Web\Components\Shared\SampleDrawerPanel.razor"
    # LipiDynamicTabs
    "LipiDynamicTabsTypes.cs"        = "src\LiPi.Web\Components\Shared\LipiDynamicTabsTypes.cs"
    "DynamicTabAttribute.cs"         = "src\LiPi.Web\Components\Shared\DynamicTabAttribute.cs"
    "ILipiDynamicTabsService.cs"     = "src\LiPi.Web\Services\ILipiDynamicTabsService.cs"
    "LipiDynamicTabsService.cs"      = "src\LiPi.Web\Services\LipiDynamicTabsService.cs"
    "LipiDynamicTabs.razor"          = "src\LiPi.Web\Components\Shared\LipiDynamicTabs.razor"
    "LipiDynamicTabs.razor.css"      = "src\LiPi.Web\Components\Shared\LipiDynamicTabs.razor.css"
    "LipiDynamicTab.razor"           = "src\LiPi.Web\Components\Shared\LipiDynamicTab.razor"
    "DirtyTabConfirmDialog.razor"    = "src\LiPi.Web\Components\Shared\DirtyTabConfirmDialog.razor"
    # Shared CSS + modified files
    "lipi-overlays.css"              = "src\LiPi.Web\wwwroot\css\lipi-overlays.css"
    "lipi-dynamic-tabs.css"          = "src\LiPi.Web\wwwroot\css\lipi-dynamic-tabs.css"

    # -- Self-hosted fonts (LiPi Sans + LiPi Mono) ----------------------------
    # Font swap: DM Sans/Mono → LiPi Sans/Mono (HIPAA/DPDP — no external CDN)
    # Manual step: copy lipi-sans/ folder to src\LiPi.Web\wwwroot\ first.
    # Deploy script handles lipi-sans.css; font binaries deploy via folder copy.
    "lipi-sans.css"                  = "src\LiPi.Web\wwwroot\lipi-sans\lipi-sans.css"
    "LiPi-Sans-Latin.woff2"          = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Latin.woff2"
    "LiPi-Sans-Latin-Italic.woff2"   = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Latin-Italic.woff2"
    "LiPi-Sans-Devanagari.woff2"     = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Devanagari.woff2"
    "LiPi-Sans-Bengali.woff2"        = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Bengali.woff2"
    "LiPi-Sans-Tamil.woff2"          = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Tamil.woff2"
    "LiPi-Sans-Telugu.woff2"         = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Telugu.woff2"
    "LiPi-Sans-Malayalam.woff2"      = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Malayalam.woff2"
    "LiPi-Sans-Kannada.woff2"        = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Kannada.woff2"
    "LiPi-Sans-Gujarati.woff2"       = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Gujarati.woff2"
    "LiPi-Sans-Gurmukhi.woff2"       = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Gurmukhi.woff2"
    "LiPi-Sans-Odia.woff2"           = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Sans-Odia.woff2"
    "LiPi-Mono.woff2"                = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Mono.woff2"
    "LiPi-Mono-Italic.woff2"         = "src\LiPi.Web\wwwroot\lipi-sans\fonts\LiPi-Mono-Italic.woff2"
    "app.css"                        = "src\LiPi.Web\wwwroot\css\app.css"
    "index.html"                     = "src\LiPi.Web\wwwroot\index.html"
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

    # -- CSS -- Phase 2.3 compound field family (Batch 9a) ---------------------
    # SPEC: docs/00-COMPONENTS/01.3-CompoundField.md (shipping in Batch 9c)
    # AMEND: CHANGE-LOG.md A19 (pending — Phase 2.3 close-out)
    # lipi-input.js extended in Batch 9a with window.lipiCompound.isInsideElement;
    # already mapped above, no new entry needed for the JS file.
    "lipi-compound.css"              = "src\LiPi.Web\wwwroot\css\lipi-compound.css"

    # -- CSS -- Phase 2.3 multi-select family (Batch 9b) -----------------------
    # SPEC: docs/00-COMPONENTS/01.4-MultiSelect.md (shipping in Batch 9c)
    # AMEND: CHANGE-LOG.md A19 (pending)
    "lipi-multi.css"                 = "src\LiPi.Web\wwwroot\css\lipi-multi.css"

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
    # Batch 3 (May 5): LipiTextArea + lipi-inputs.css extraction (A13)
    # Batch 4 (May 5): LipiNumberInput<TValue> + lipi-input.js selectAll helper (A15)
    # Batch 4.1 (May 5): arrow keys + EditForm test page
    # Batch 4.2 (May 5): DisableArrowKeys + BlockNonNumericInput + LipiButton hotfix
    # Batch 4.3 (May 5): JS DOM value-sync (closes value-sync issue from 4.2 ship notes)
    # Batch 5 (May 5): LipiSelect<TValue> + LipiCombobox<TValue,TItem> + LipiSelectBase + dropdown JS
    # Phase 2.2.5 Batch 8a (May ?): LipiInputBase shared base for EditContext auto-population (A16)
    "LipiTextBox.razor"              = "src\LiPi.Web\Components\Shared\LipiTextBox.razor"
    "LipiTextBox.razor.css"          = "src\LiPi.Web\Components\Shared\LipiTextBox.razor.css"
    "LipiTextArea.razor"             = "src\LiPi.Web\Components\Shared\LipiTextArea.razor"
    "LipiTextArea.razor.css"         = "src\LiPi.Web\Components\Shared\LipiTextArea.razor.css"
    "LipiNumberInput.razor"          = "src\LiPi.Web\Components\Shared\LipiNumberInput.razor"
    "LipiNumberInput.razor.css"      = "src\LiPi.Web\Components\Shared\LipiNumberInput.razor.css"
    "LipiInputBase.cs"               = "src\LiPi.Web\Components\Shared\LipiInputBase.cs"
    "LipiSelectBase.cs"              = "src\LiPi.Web\Components\Shared\LipiSelectBase.cs"
    "LipiSelect.razor"               = "src\LiPi.Web\Components\Shared\LipiSelect.razor"
    "LipiSelect.razor.css"           = "src\LiPi.Web\Components\Shared\LipiSelect.razor.css"
    "LipiCombobox.razor"             = "src\LiPi.Web\Components\Shared\LipiCombobox.razor"
    "LipiCombobox.razor.css"         = "src\LiPi.Web\Components\Shared\LipiCombobox.razor.css"
    "LipiTextInputTypes.cs"          = "src\LiPi.Web\Components\Shared\LipiTextInputTypes.cs"
    "LipiInputDefaults.cs"           = "src\LiPi.Web\Components\Shared\LipiInputDefaults.cs"
    "AutocompleteValidator.cs"       = "src\LiPi.Web\Components\Shared\AutocompleteValidator.cs"

    # -- Components -- Phase 2.3 (Compound field family) -----------------------
    # SPEC: docs/00-COMPONENTS/01.3-CompoundField.md (shipping in Batch 9c)
    # AMEND: CHANGE-LOG.md A19 (pending)
    # Batch 9a (May 6): LipiContainerBase + ICompoundSegment + LipiCompoundField + SelectSegment + TextSegment + CompoundFieldTest
    "LipiContainerBase.cs"           = "src\LiPi.Web\Components\Shared\LipiContainerBase.cs"
    "ICompoundSegment.cs"            = "src\LiPi.Web\Components\Shared\ICompoundSegment.cs"
    "LipiCompoundField.razor"        = "src\LiPi.Web\Components\Shared\LipiCompoundField.razor"
    "SelectSegment.razor"            = "src\LiPi.Web\Components\Shared\SelectSegment.razor"
    "TextSegment.razor"              = "src\LiPi.Web\Components\Shared\TextSegment.razor"

    # -- Components -- Phase 2.3 (Multi-select family) -------------------------
    # SPEC: docs/00-COMPONENTS/01.4-MultiSelect.md (shipping in Batch 9c-b)
    # AMEND: CHANGE-LOG.md A19 (pending)
    # Batch 9b (May 6/7): LipiMultiSelectBase + LipiMultiSelect + MultiSelectTest
    # Batch 9c-a (May 7): LipiMultiCombobox + MultiComboboxTest (templated multi-select)
    "LipiMultiSelectBase.cs"         = "src\LiPi.Web\Components\Shared\LipiMultiSelectBase.cs"
    "LipiMultiSelect.razor"          = "src\LiPi.Web\Components\Shared\LipiMultiSelect.razor"
    "LipiMultiCombobox.razor"        = "src\LiPi.Web\Components\Shared\LipiMultiCombobox.razor"

    # -- Components -- Phase 2.4 (Date/Time family) ----------------------------
    # SPEC: docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d)
    # AMEND: CHANGE-LOG.md A20 (pending)
    # Batch 9d (May 7): Single-batch ship of full Date/Time family.
    # Architecture: LipiDatePicker / LipiTimePicker / LipiDateTimePicker inherit
    # LipiInputBase. LipiDateRangePicker inherits ComponentBase directly (dual-
    # value binding shape). Services + types support all four.
    "LipiDateTimeTypes.cs"           = "src\LiPi.Web\Components\Shared\LipiDateTimeTypes.cs"
    "LipiDatePicker.razor"           = "src\LiPi.Web\Components\Shared\LipiDatePicker.razor"
    "LipiDatePicker.razor.css"       = "src\LiPi.Web\Components\Shared\LipiDatePicker.razor.css"
    "LipiTimePicker.razor"           = "src\LiPi.Web\Components\Shared\LipiTimePicker.razor"
    "LipiTimePicker.razor.css"       = "src\LiPi.Web\Components\Shared\LipiTimePicker.razor.css"
    "LipiDateTimePicker.razor"       = "src\LiPi.Web\Components\Shared\LipiDateTimePicker.razor"
    "LipiDateTimePicker.razor.css"   = "src\LiPi.Web\Components\Shared\LipiDateTimePicker.razor.css"
    "LipiDateRangePicker.razor"      = "src\LiPi.Web\Components\Shared\LipiDateRangePicker.razor"
    "LipiDateRangePicker.razor.css"  = "src\LiPi.Web\Components\Shared\LipiDateRangePicker.razor.css"

    # -- Services -- Phase 2.4 -------------------------------------------------
    "IDateFormatService.cs"          = "src\LiPi.Web\Services\IDateFormatService.cs"
    "DateFormatService.cs"           = "src\LiPi.Web\Services\DateFormatService.cs"
    "IClinicTimezoneService.cs"      = "src\LiPi.Web\Services\IClinicTimezoneService.cs"
    "ClinicTimezoneService.cs"       = "src\LiPi.Web\Services\ClinicTimezoneService.cs"

    # -- Components -- Phase 2.5 (Selection Components family) ----------------
    # SPEC: docs/03-Phase-2.5-Selection-Components/05-Cross-Cutting.md
    # AMEND: CHANGE-LOG.md A15 (pending — Phase 2.5 close-out)
    # Staged ship across multiple steps:
    # Step 1 (May 9): tokens — --r-2xs, --tr-toggle (00-baseline.css);
    #                 --sh-thumb (mode-light.css, mode-dark.css)
    # Step 2 (May 9): LipiInputDefaults extended (3 selection-family properties);
    #                 LipiSelectionTypes.cs ships with CheckboxGroupDensity
    # Step 3 (May 9): MustBeTrueAttribute.cs ships (validation attribute for
    #                 terms acceptance / HIPAA consent / required toggles per
    #                 audit item #10 contract; path per audit item #16)
    # Step 4 (May 9): LipiSelectionTypes.cs gains CheckboxGroupOrientation and
    #                 InputLabelPosition (no new file; same path; per audit
    #                 item #15c shared-enum lock)
    # Step 5 (May 9): cascade context records — LipiCheckboxGroupContext.cs
    #                 and LipiRadioGroupContext.cs (separate files per
    #                 ThemeContext.cs precedent; distinct types per audit
    #                 item #12 cascade-safety lock)
    # Step 6 (May 9): family CSS — lipi-selection-family.css (layout-vs-shape
    #                 architecture per item #1, always-below helper per
    #                 item #2, .lipi-cbg-count-over-max per item #8b);
    #                 form utilities — lipi-form-utilities.css (.lipi-form-row
    #                 per item #5); --color-required-strong added to
    #                 mode-light.css and mode-dark.css per item #17;
    #                 App.razor link tags + cache version 20260516 → 20260516a
    # Step 7 (May 9): LipiCheckbox component — first selection-family component;
    #                 cascade parameter from day one per item #11; Pattern A
    #                 binding via @onchange + (TValue)(object) cast per item-12
    #                 lock; OnChildInteracted fires on @onchange ONLY (not blur
    #                 or focus per item-12 lock 2); helper indent hardcoded
    #                 per size in scoped CSS per lock 3
    # Step 9 (May 16): LipiCheckboxGroup component — multi-selection group;
    #                  fieldset + legend per Decision 2.6; cascade emits via
    #                  CascadingValue with 2-field LipiCheckboxGroupContext per
    #                  item #12; ValueSelector uniqueness check + over-max bound
    #                  defensive check per items #6 + #8b; replacement-mutation
    #                  pattern (new List per toggle) per Decision 2.2; helper
    #                  single-bottom slot per item #2; family CSS amended with
    #                  error-state left-border rule (covers .lipi-cbg-* AND
    #                  .lipi-rg-* upfront for step 11) per item #19 lock 5;
    #                  LipiCheckbox re-shipped with Label/AriaLabel filter fix
    #                  per item #19 lock 6; cache version 20260516a → 20260516b
    # Step 11 (May 10): LipiRadioGroup + LipiRadio re-ship; Pattern A roving
    #                  tabindex via ElementReference.FocusAsync() (no JS needed);
    #                  type-ahead (StartsWith, 1s buffer); AllowClear default
    #                  false per item #4; GroupName + LabelTemplate added to
    #                  LipiRadio; cache 20260516b → 20260516c
    # Step 12 (May 10): LipiToggle — final Phase 2.5 component; non-generic
    #                  bool-only; track+thumb anatomy; --tr-toggle slide +
    #                  M3 pressed-stretch; IconWhenOn for clinical confirmation;
    #                  IsEmpty always false (both on/off valid); role="switch";
    #                  no cascade parameter; cache 20260516c → 20260516d
    #                  Phase 2.5 COMPLETE — all 5 selection components shipped
    "LipiSelectionTypes.cs"          = "src\LiPi.Web\Components\Shared\LipiSelectionTypes.cs"
    "MustBeTrueAttribute.cs"         = "src\LiPi.Web\Components\Shared\MustBeTrueAttribute.cs"
    "LipiCheckboxGroupContext.cs"    = "src\LiPi.Web\Components\Shared\LipiCheckboxGroupContext.cs"
    "LipiRadioGroupContext.cs"       = "src\LiPi.Web\Components\Shared\LipiRadioGroupContext.cs"
    "lipi-selection-family.css"      = "src\LiPi.Web\wwwroot\css\lipi-selection-family.css"
    "lipi-form-utilities.css"        = "src\LiPi.Web\wwwroot\css\lipi-form-utilities.css"
    "LipiCheckbox.razor"             = "src\LiPi.Web\Components\Shared\LipiCheckbox.razor"
    "LipiCheckbox.razor.css"         = "src\LiPi.Web\Components\Shared\LipiCheckbox.razor.css"
    "LipiCheckboxGroup.razor"        = "src\LiPi.Web\Components\Shared\LipiCheckboxGroup.razor"
    "LipiCheckboxGroup.razor.css"    = "src\LiPi.Web\Components\Shared\LipiCheckboxGroup.razor.css"
    "LipiRadio.razor"                = "src\LiPi.Web\Components\Shared\LipiRadio.razor"
    "LipiRadio.razor.css"            = "src\LiPi.Web\Components\Shared\LipiRadio.razor.css"
    "LipiRadioGroup.razor"           = "src\LiPi.Web\Components\Shared\LipiRadioGroup.razor"
    "LipiRadioGroup.razor.css"       = "src\LiPi.Web\Components\Shared\LipiRadioGroup.razor.css"
    "LipiToggle.razor"               = "src\LiPi.Web\Components\Shared\LipiToggle.razor"
    "LipiToggle.razor.css"           = "src\LiPi.Web\Components\Shared\LipiToggle.razor.css"

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
    "StyleGuideLayout.razor"         = "src\LiPi.Web\Pages\StyleGuideLayout.razor"
    "StyleGuideLayout.razor.css"     = "src\LiPi.Web\Pages\StyleGuideLayout.razor.css"
    "StyleGuideOverlays.razor"       = "src\LiPi.Web\Pages\StyleGuideOverlays.razor"
    "StyleGuideOverlays.razor.css"   = "src\LiPi.Web\Pages\StyleGuideOverlays.razor.css"
    # A33: Stub @page navigated to by DynamicTabs demos (§14 demos 2–9)
    "StyleGuideOverlayTabDemo.razor" = "src\LiPi.Web\Pages\StyleGuideOverlayTabDemo.razor"
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
    "TextboxTest.razor"              = "src\LiPi.Web\Pages\Test\TextboxTest.razor"
    "CompoundFieldTest.razor"        = "src\LiPi.Web\Pages\Test\CompoundFieldTest.razor"
    "MultiSelectTest.razor"          = "src\LiPi.Web\Pages\Test\MultiSelectTest.razor"
    "MultiComboboxTest.razor"        = "src\LiPi.Web\Pages\Test\MultiComboboxTest.razor"
    # Phase 2.4 (Batch 9d) — Date/Time family test pages
    "DatePickerTest.razor"           = "src\LiPi.Web\Pages\Test\DatePickerTest.razor"
    "TimePickerTest.razor"           = "src\LiPi.Web\Pages\Test\TimePickerTest.razor"
    "DateTimePickerTest.razor"       = "src\LiPi.Web\Pages\Test\DateTimePickerTest.razor"
    "DateRangePickerTest.razor"      = "src\LiPi.Web\Pages\Test\DateRangePickerTest.razor"

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
    "01.3-CompoundField.md"          = "docs\00-COMPONENTS\01.3-CompoundField.md"
    "01.4-MultiSelect.md"            = "docs\00-COMPONENTS\01.4-MultiSelect.md"
    "01.5-DateTime.md"               = "docs\00-COMPONENTS\01.5-DateTime.md"
    "02-LipiTabs-Spec.md"            = "docs\02-LipiTabs-Spec.md"
    "03-LipiAlert-Spec.md"           = "docs\03-LipiAlert-Spec.md"
    "04-LipiCard-Spec.md"            = "docs\04-LipiCard-Spec.md"

    # -- Docs -- Phase 2.6.2 specs (A34 close-out, fold of A29 pattern) --------
    # Mirrors A29's 2.6.1-spec mapping fix. Without these entries, edits to the
    # four 2.6.2 spec docs could not flow through the standard deploy workflow.
    "00-Phase2.6.2-Overview.md"      = "docs\00-COMPONENTS\2.6.2\00-Phase2.6.2-Overview.md"
    "01-LipiModal-Spec.md"           = "docs\00-COMPONENTS\2.6.2\01-LipiModal-Spec.md"
    "02-LipiDrawer-Spec.md"          = "docs\00-COMPONENTS\2.6.2\02-LipiDrawer-Spec.md"
    "03-LipiDynamicTabs-Spec.md"     = "docs\00-COMPONENTS\2.6.2\03-LipiDynamicTabs-Spec.md"

    # -- Docs -- Top-level -----------------------------------------------------
    "00-PROJECT-BASELINE.md"         = "docs\00-PROJECT-BASELINE.md"
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
