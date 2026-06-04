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
    "GlobalUsings.cs" = "src\LiPi.Web\GlobalUsings.cs"   # PR2 overlay-migration consumer repoint
    # ==========================================================================
    # ⚠ A43 MIGRATION — MANUAL STEP REQUIRED *BEFORE* RUNNING THIS DEPLOY ⚠
    # ==========================================================================
    # Phase 2.8 input/selection family migrated from LiPi.Web.Components.Shared
    # into the LiPi.Components package (namespace LiPi.Components, folder
    # src\LiPi.Components\Forms\). The 37 files below now deploy to the NEW path.
    #
    # The OLD copies in src\LiPi.Web\Components\Shared\ MUST be deleted first, or
    # the build will see DUPLICATE type definitions in two projects (ambiguous
    # reference errors everywhere). Run this block ONCE, before deploying:
    #
    #   $old = "src\LiPi.Web\Components\Shared"
    #   $migrated = @(
    #     "LipiInputBase.cs","LipiInputDefaults.cs","LipiSelectionTypes.cs","LipiTextInputTypes.cs",
    #     "LipiCheckbox.razor","LipiCheckbox.razor.css",
    #     "LipiCheckboxGroup.razor","LipiCheckboxGroup.razor.css","LipiCheckboxGroupContext.cs",
    #     "LipiRadio.razor","LipiRadio.razor.css","LipiRadioGroup.razor","LipiRadioGroup.razor.css","LipiRadioGroupContext.cs",
    #     "LipiToggle.razor","LipiToggle.razor.css",
    #     "LipiTextBox.razor","LipiTextBox.razor.css","LipiTextArea.razor","LipiTextArea.razor.css",
    #     "LipiNumberInput.razor","LipiNumberInput.razor.css",
    #     "LipiSelect.razor","LipiSelect.razor.css","LipiSelectBase.cs",
    #     "LipiCombobox.razor","LipiCombobox.razor.css",
    #     "LipiMultiSelect.razor","LipiMultiSelectBase.cs","LipiMultiCombobox.razor",
    #     "LipiCompoundField.razor","ICompoundSegment.cs","SelectSegment.razor","TextSegment.razor","LipiContainerBase.cs",
    #     "AutocompleteValidator.cs","MustBeTrueAttribute.cs"
    #   )
    #   foreach ($f in $migrated) { Remove-Item (Join-Path $old $f) -ErrorAction SilentlyContinue }
    #
    # Sequence: (1) run the Remove-Item block above  →  (2) run this deploy
    #           →  (3) dotnet build src\LiPi.Web\LiPi.Web.csproj
    # The .razor.css files for migrated components live next to their .razor in
    # the new Forms\ folder (Blazor scoped-CSS convention). The global CSS
    # (lipi-inputs.css, lipi-selection-family.css) STAYS in wwwroot — unchanged.
    # ==========================================================================
    # -- Web app core ----------------------------------------------------------
    "_Imports.razor"                 = "src\LiPi.Web\_Imports.razor"
    # A44: LiPi.Components project-level _Imports (framework usings for migrated .razor
    # components). Source file renamed to _Imports.Components.razor to avoid a Downloads-
    # folder filename collision with the Web _Imports.razor above; deploys to the correct
    # name at the destination path.
    "_Imports.Components.razor"      = "src\LiPi.Components\_Imports.razor"
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
    "IFocusTrapService.cs" = "src\LiPi.Components\Overlays\IFocusTrapService.cs"
    "FocusTrapService.cs" = "src\LiPi.Components\Overlays\FocusTrapService.cs"
    "IScrollLockService.cs" = "src\LiPi.Components\Overlays\IScrollLockService.cs"
    "ScrollLockService.cs" = "src\LiPi.Components\Overlays\ScrollLockService.cs"
    "lipi-overlay-interop.js"        = "src\LiPi.Web\wwwroot\js\lipi-overlay-interop.js"
    "LipiOverlayHost.razor" = "src\LiPi.Components\Overlays\LipiOverlayHost.razor"
    # LipiModal
    # NOTE: LipiModalTypes.cs rebuilt 2026-05-14 from spec §4 after loss (A30).
    # NOTE: LipiModal.razor family added 2026-05-14 (A31) — declarative path
    #       per spec §2. Single-file (no .razor.cs) matching project pattern.
    "LipiModalTypes.cs" = "src\LiPi.Components\Overlays\LipiModalTypes.cs"
    "LipiModal.razor" = "src\LiPi.Components\Overlays\LipiModal.razor"
    "LipiModal.razor.css" = "src\LiPi.Components\Overlays\LipiModal.razor.css"
    "LipiModalBody.razor" = "src\LiPi.Components\Overlays\LipiModalBody.razor"
    "LipiModalFooter.razor" = "src\LiPi.Components\Overlays\LipiModalFooter.razor"
    # A33: StyleGuide demo body for Modal.ShowAsync<T, string?>
    "SampleCustomModal.razor"        = "src\LiPi.Web\Components\Shared\SampleCustomModal.razor"
    "ILipiModalService.cs" = "src\LiPi.Components\Overlays\ILipiModalService.cs"
    "LipiModalService.cs" = "src\LiPi.Components\Overlays\LipiModalService.cs"
    "ConfirmDialog.razor" = "src\LiPi.Components\Overlays\ConfirmDialog.razor"
    "AlertDialog.razor" = "src\LiPi.Components\Overlays\AlertDialog.razor"
    "PromptDialog.razor" = "src\LiPi.Components\Overlays\PromptDialog.razor"
    # LipiDrawer
    "LipiDrawerTypes.cs" = "src\LiPi.Components\Overlays\LipiDrawerTypes.cs"
    "ILipiDrawerService.cs" = "src\LiPi.Components\Overlays\ILipiDrawerService.cs"
    "LipiDrawerService.cs" = "src\LiPi.Components\Overlays\LipiDrawerService.cs"
    "LipiDrawer.razor" = "src\LiPi.Components\Overlays\LipiDrawer.razor"
    "LipiDrawer.razor.css" = "src\LiPi.Components\Overlays\LipiDrawer.razor.css"
    "LipiDrawerBody.razor" = "src\LiPi.Components\Overlays\LipiDrawerBody.razor"
    "LipiDrawerFooter.razor" = "src\LiPi.Components\Overlays\LipiDrawerFooter.razor"
    # A33: StyleGuide demo body for Drawer.ShowAsync<T, bool>
    "SampleDrawerPanel.razor"        = "src\LiPi.Web\Components\Shared\SampleDrawerPanel.razor"
    # LipiDynamicTabs
    "LipiDynamicTabsTypes.cs" = "src\LiPi.Components\Overlays\LipiDynamicTabsTypes.cs"
    "DynamicTabAttribute.cs" = "src\LiPi.Components\Overlays\DynamicTabAttribute.cs"
    "ILipiDynamicTabsService.cs" = "src\LiPi.Components\Overlays\ILipiDynamicTabsService.cs"
    "LipiDynamicTabsService.cs" = "src\LiPi.Components\Overlays\LipiDynamicTabsService.cs"
    "LipiDynamicTabs.razor" = "src\LiPi.Components\Overlays\LipiDynamicTabs.razor"
    "LipiDynamicTabs.razor.css" = "src\LiPi.Components\Overlays\LipiDynamicTabs.razor.css"
    "LipiDynamicTab.razor" = "src\LiPi.Components\Overlays\LipiDynamicTab.razor"
    "DirtyTabConfirmDialog.razor" = "src\LiPi.Components\Overlays\DirtyTabConfirmDialog.razor"
    # Shared CSS + modified files
    "lipi-overlays.css"              = "src\LiPi.Web\wwwroot\css\lipi-overlays.css"
    "lipi-dynamic-tabs.css"          = "src\LiPi.Web\wwwroot\css\lipi-dynamic-tabs.css"
    # -- Phase 2.7 — Feedback Components (Spinner + Badge + Pill + Skeleton +
    #                                     ValidationSummary + Toast) ----------
    # SPEC:  docs/00-COMPONENTS/2.7/00-Phase2.7-Overview.md (and 01-05 sibling specs)
    # AMEND: CHANGE-LOG.md A35 (2026-05-15)
    #
    # 32 new files: 25 component files (Spinner 4 + Badge 4 + Pill 4 +
    # Skeleton 3 single-file primitives + ValidationSummary 4 + Toast 4 +
    # ToastHost 2 + ToastTypes 1 + ToastShared 1 — Types files split per
    # handout literal except Skeleton primitives which are single-file
    # pure-render) + 2 service files (ILipiToastService + LipiToastService)
    # + 3 wwwroot files (lipi-skeleton.css + lipi-validation.css +
    # lipi-validation.js) + 2 StyleGuide pages (Feedback .razor + .razor.css).
    #
    # Modified files in this batch: mode-light.css, mode-dark.css (token
    # additions), App.razor (cache 20260525→20260526, new <link> + <script>
    # entries), TopNavLayout.razor (<LipiToastHost /> mount), Program.cs
    # (AddScoped<ILipiToastService, LipiToastService>), StyleGuide.razor
    # (Phase 2.7 sidebar nav group). All modified files are already mapped
    # above in their existing blocks — no duplicate entries needed.
    #
    # LipiSpinner — general-purpose loading indicator (distinct from
    # LipiButtonSpinner). Reuses Phase 2.5.5 InputLabelPosition for all 4
    # label directions.
    "LipiSpinnerTypes.cs" = "src\LiPi.Components\Feedback\LipiSpinnerTypes.cs"
    "LipiSpinner.razor" = "src\LiPi.Components\Feedback\LipiSpinner.razor"
    "LipiSpinner.razor.cs" = "src\LiPi.Components\Feedback\LipiSpinner.razor.cs"
    "LipiSpinner.razor.css" = "src\LiPi.Components\Feedback\LipiSpinner.razor.css"
    # LipiBadge — attached count/dot indicator (parent must be position:relative).
    # Sibling pattern to LipiPill — Badge attaches, Pill stands alone.
    "LipiBadgeTypes.cs"              = "src\LiPi.Web\Components\Shared\LipiBadgeTypes.cs"
    "LipiBadge.razor"                = "src\LiPi.Web\Components\Shared\LipiBadge.razor"
    "LipiBadge.razor.cs"             = "src\LiPi.Web\Components\Shared\LipiBadge.razor.cs"
    "LipiBadge.razor.css"            = "src\LiPi.Web\Components\Shared\LipiBadge.razor.css"
    # LipiPill — standalone label/tag/chip (7 intents × 3 variants × 3 sizes
    # + dismissible). Inline in text flow; not attached to a parent element.
    "LipiPillTypes.cs"               = "src\LiPi.Web\Components\Shared\LipiPillTypes.cs"
    "LipiPill.razor"                 = "src\LiPi.Web\Components\Shared\LipiPill.razor"
    "LipiPill.razor.cs"              = "src\LiPi.Web\Components\Shared\LipiPill.razor.cs"
    "LipiPill.razor.css"             = "src\LiPi.Web\Components\Shared\LipiPill.razor.css"
    # LipiSkeleton — 3 single-file pure-render primitives. No per-primitive
    # scoped .razor.css; the family shares lipi-skeleton.css (shimmer
    # keyframe + 3 shape classes + reduced-motion handling).
    "LipiSkeletonLine.razor"         = "src\LiPi.Web\Components\Shared\LipiSkeletonLine.razor"
    "LipiSkeletonCircle.razor"       = "src\LiPi.Web\Components\Shared\LipiSkeletonCircle.razor"
    "LipiSkeletonRect.razor"         = "src\LiPi.Web\Components\Shared\LipiSkeletonRect.razor"
    # LipiValidationSummary — form-level error summary. Auto-discovers errors
    # from cascading EditContext, resolves [Display(Name)] attributes, and
    # provides click-to-field navigation via JS interop (lipi-validation.js).
    "LipiValidationSummaryTypes.cs"  = "src\LiPi.Web\Components\Shared\LipiValidationSummaryTypes.cs"
    "LipiValidationSummary.razor"    = "src\LiPi.Web\Components\Shared\LipiValidationSummary.razor"
    "LipiValidationSummary.razor.cs" = "src\LiPi.Web\Components\Shared\LipiValidationSummary.razor.cs"
    "LipiValidationSummary.razor.css"= "src\LiPi.Web\Components\Shared\LipiValidationSummary.razor.css"
    # LipiToast family — service-driven transient notifications. ToastTypes.cs
    # holds all enums + data classes + internal ToastEntry (single file per
    # handout literal). Errors are PERSISTENT BY DEFAULT — clinical safety
    # decision. Promise-style morph swaps Loading icon → Success/Error in place.
    "LipiToastTypes.cs" = "src\LiPi.Components\Overlays\LipiToastTypes.cs"
    "ILipiToastService.cs" = "src\LiPi.Components\Overlays\ILipiToastService.cs"
    "LipiToastService.cs" = "src\LiPi.Components\Overlays\LipiToastService.cs"
    "LipiToast.razor" = "src\LiPi.Components\Overlays\LipiToast.razor"
    "LipiToast.razor.cs" = "src\LiPi.Components\Overlays\LipiToast.razor.cs"
    "LipiToast.razor.css" = "src\LiPi.Components\Overlays\LipiToast.razor.css"
    "LipiToastHost.razor" = "src\LiPi.Components\Overlays\LipiToastHost.razor"
    "LipiToastHost.razor.cs" = "src\LiPi.Components\Overlays\LipiToastHost.razor.cs"
    # Shared CSS + JS for Phase 2.7 — see App.razor link order (15, 16, JS).
    # lipi-skeleton.css is the shared shimmer + shape classes for the 3
    # Skeleton primitives. lipi-validation.css is GLOBAL scope (not scoped
    # to LipiValidationSummary) because the field-flash effect targets any
    # input element on the page, not just elements rendered by the summary
    # component itself. lipi-validation.js exposes
    # window.lipiValidation.scrollToField for click-to-field navigation.
    "lipi-skeleton.css"              = "src\LiPi.Web\wwwroot\css\lipi-skeleton.css"
    "lipi-validation.css"            = "src\LiPi.Web\wwwroot\css\lipi-validation.css"
    "lipi-validation.js"             = "src\LiPi.Web\wwwroot\js\lipi-validation.js"
    # -- Phase 2.8 — Data Display Stage 1A (Type foundation) -------------------
    # SPEC:  docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §25.2.2 (namespace),
    #        §27.3 (file taxonomy), §28.4 (deploy registry — adapted)
    # AMEND: CHANGE-LOG.md A37 (2026-05-16)
    #
    # 10 new files = 1 csproj + 9 type-only .cs files. Stage 1A introduces the
    # new LiPi.Components.csproj (redistributable component library, .NET 10,
    # Microsoft.NET.Sdk.Razor, zero NuGet refs). All Phase 2.8 component code
    # lives in LiPi.Components.* namespace per §25.2.2 — the first deviation
    # from the existing LiPi.Web.Components.Shared layout. Existing Phase 2.1-2.7
    # components stay where they are; Phase 2.10 audit migrates them later.
    #
    # Source files use the redistributable namespace:
    #   LiPi.Components.DataDisplay         (LipiTable + sibling components)
    #   LiPi.Components.DataDisplay.Export  (export-related types)
    #   LiPi.Components.Shared              (cross-component infrastructure)
    #
    # NOT in this stage:
    #   • Services + DB migration + EF entity                       → Stage 1B
    #   • Shared CSS + JS interop + App.razor + Program.cs DI +
    #     LiPi.sln/csproj project-reference wire-up                 → Stage 1C
    #   • Spec docs deploy entries (00–04 Phase 2.8 spec markdown)  → Stage 8
    #
    # Three deviations from §28.4 spec registry, all recorded in A37:
    #   (1) ExportTypes.cs added to Stage 1A — BeforeExportContext /
    #       AfterExportContext carry ExportFormat. Co-locating the full export
    #       type family avoids a Stage-6 forward-declaration coupling.
    #   (2) §27.2 vs §28.4 csproj-layout reconciled to a single project.
    #       §27.2 diagram suggested LiPi.Components.Shared as a separate csproj;
    #       §28.4 deploy paths use Shared\ as a subfolder. Going with §28.4
    #       (one csproj — DataDisplay\ and Shared\ as subfolders).
    #   (3) PersistedContext + PersistedTrigger deferred to Stage 1B.
    #       §23.10.7 places them in Contexts.cs but they reference
    #       TablePreferences (Stage 1B). Relocated to keep Stage 1A
    #       standalone-compilable.
    #
    # Filename convention: bare filenames (project convention) — NOT the
    # `2.8-types-*` prefix mandated by §28.3. Reality on the ground (existing
    # deploy script) wins; recorded in A37 as a divergence from §28.3.
    #
    # Project layout:
    #   src\LiPi.Components\LiPi.Components.csproj
    #   src\LiPi.Components\DataDisplay\LipiTable\*.cs                  (7 files)
    #   src\LiPi.Components\DataDisplay\LipiTable\Export\ExportTypes.cs (1 file)
    #   src\LiPi.Components\Shared\LipiStatus.cs                        (1 file)
    "LiPi.Components.csproj"         = "src\LiPi.Components\LiPi.Components.csproj"
    "LipiTableTypes.cs"              = "src\LiPi.Components\DataDisplay\LipiTable\LipiTableTypes.cs"
    "LipiColumnTypes.cs"             = "src\LiPi.Components\DataDisplay\LipiTable\LipiColumnTypes.cs"
    "SortDescriptor.cs"              = "src\LiPi.Components\DataDisplay\LipiTable\SortDescriptor.cs"
    "TableQueryRequest.cs"           = "src\LiPi.Components\DataDisplay\LipiTable\TableQueryRequest.cs"
    "TableQueryResponse.cs"          = "src\LiPi.Components\DataDisplay\LipiTable\TableQueryResponse.cs"
    "SaveResult.cs"                  = "src\LiPi.Components\DataDisplay\LipiTable\SaveResult.cs"
    "Contexts.cs"                    = "src\LiPi.Components\DataDisplay\LipiTable\Contexts.cs"
    "ExportTypes.cs"                 = "src\LiPi.Components\DataDisplay\LipiTable\Export\ExportTypes.cs"
    "LipiStatus.cs"                  = "src\LiPi.Components\Shared\LipiStatus.cs"
    # -- Phase 2.8 — Data Display Stage 1B (Services + entity + migrations) ----
    # SPEC:  docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §21.4 (persistence),
    #        docs/00-COMPONENTS/2.8/00-Phase2.8-Overview.md §2.5 (DB schema)
    # AMEND: CHANGE-LOG.md A38 (2026-05-16)
    #
    # 8 new files: 5 .cs (service abstractions + records + default impl in
    # LiPi.Components) + 1 .cs (EF entity in LiPi.Clinic.Core) + 2 .sql
    # (migration up/down at database\migrations\).
    #
    # Architecture: Option C — store abstraction. LiPi.Components defines BOTH
    # the high-level ITablePreferenceService AND the low-level
    # IUserTablePreferenceStore + ICurrentUserAccessor abstractions. LiPi.Components
    # also ships the default TablePreferenceService implementation (JSON serialize,
    # 300ms debounce, per-circuit cache, silent error handling, async dispose flush)
    # — this compiles standalone because it depends only on the two lower-level
    # interfaces. LiPi.Web in Stage 1C will implement just those two interfaces
    # (EfUserTablePreferenceStore + BlazorCurrentUserAccessor).
    #
    # Pattern mirrors ASP.NET Identity's UserManager<T> + IUserStore<T> split:
    # library owns the high-level service; consumer owns storage + auth specifics.
    #
    # Persistence target: per-clinic DB (NOT master). Decided in A38 — table
    # preferences are clinic-scoped because tables themselves are clinic-scoped.
    # Migration applies PART B (each clinic DB), not PART A (master).
    #
    # Cross-DB FK note: user_id references master.identity.users(id) but
    # PostgreSQL cannot enforce the constraint cross-DB. App-layer validation
    # only via auth context. Acceptable because LiPi never hard-deletes users
    # — clinic-user-deletion is access revocation only, so orphans never accumulate.
    #
    # PersistedContext + PersistedTrigger ship in TablePreferences.cs (not
    # Contexts.cs) per Stage 1A's A37 deferral — they reference TablePreferences
    # which lives in Stage 1B.
    #
    # NOT in this stage:
    #   • EfUserTablePreferenceStore concrete impl in LiPi.Web         → Stage 1C
    #   • BlazorCurrentUserAccessor concrete impl in LiPi.Web          → Stage 1C
    #   • DbSet<UserTablePreference> on the clinic-side DbContext      → Stage 1C
    #   • Program.cs DI registration (3 Scoped services)               → Stage 1C
    #   • LiPi.sln + LiPi.Web.csproj <ProjectReference> wire-up        → Stage 1C
    #   • Migration SQL APPLICATION (the .sql files deploy here but
    #     are NOT auto-executed; manual apply via pgAdmin / psql)
    #
    # Three convention divergences from spec §28.4, recorded in A38:
    #   (1) EF entity placement: spec said LiPi.Identity.Core (project doesn't
    #       exist); reality is LiPi.Clinic.Core (matches per-clinic decision).
    #   (2) Migration SQL path: spec said database\migrations\identity\
    #       subfolder; project convention is flat database\migrations\.
    #   (3) EF config: spec said separate Configurations\ folder; project
    #       convention uses data annotations on the entity (matches
    #       UserPreference / UserRole / AdSyncLog precedent).
    "ITablePreferenceService.cs"     = "src\LiPi.Components\DataDisplay\LipiTable\Services\ITablePreferenceService.cs"
    "IUserTablePreferenceStore.cs"   = "src\LiPi.Components\DataDisplay\LipiTable\Services\IUserTablePreferenceStore.cs"
    "ICurrentUserAccessor.cs"        = "src\LiPi.Components\DataDisplay\LipiTable\Services\ICurrentUserAccessor.cs"
    "TablePreferences.cs"            = "src\LiPi.Components\DataDisplay\LipiTable\Services\TablePreferences.cs"
    "TablePreferenceService.cs"      = "src\LiPi.Components\DataDisplay\LipiTable\Services\TablePreferenceService.cs"
    "UserTablePreference.cs"         = "database\efcore\LiPi.Clinic.Identity\Entities\UserTablePreference.cs"
    "2026-05-16-phase-2.8-user-table-prefs-up.sql"   = "database\migrations\2026-05-16-phase-2.8-user-table-prefs-up.sql"
    "2026-05-16-phase-2.8-user-table-prefs-down.sql" = "database\migrations\2026-05-16-phase-2.8-user-table-prefs-down.sql"
    # -- Phase 2.8 — Data Display Stage 1C (Service wire-up) -------------------
    # SPEC:  docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §21.4, §25.3.4
    # AMEND: CHANGE-LOG.md A39 (2026-05-16)
    #
    # Wires persistence end-to-end so the whole app compiles and table prefs
    # round-trip. Three NEW files registered here + four MODIFIED files routed
    # by entries that already exist elsewhere in this script:
    #   • Program.cs           — existing entry (top of script); +3 Scoped regs
    #   • IdentityDbContext.cs — existing entry (Phase-1 block); +DbSet +config
    #   • UserTablePreference.cs — Stage 1B entry REPOINTED above
    #                              (LiPi.Clinic.Core → LiPi.Clinic.Identity)
    #   • LiPi.Web.csproj      — NEW entry below; +ProjectReference to LiPi.Components
    #
    # A39 correction: Stage 1B (A38) placed the entity in LiPi.Clinic.Core to
    # match a (nonexistent) spec "LiPi.Identity.Core". Seeing the real codebase,
    # the established sibling is identity.user_preferences in LiPi.Clinic.Identity
    # / IdentityDbContext. Entity moved there. Migration SQL was already correct
    # (identity schema, per clinic DB) and is unchanged.
    #
    # MANUAL STEP after deploy — delete the orphaned Stage 1B file:
    #   Remove-Item "database\efcore\LiPi.Clinic.Core\Entities\UserTablePreference.cs"
    # (It is superseded by the LiPi.Clinic.Identity copy. Leaving it is harmless
    #  dead code in a different namespace, but delete it to avoid confusion.)
    #
    # MANUAL STEP after deploy — apply the migration to each clinic DB:
    #   psql -d <clinic_db> -f database\migrations\2026-05-16-phase-2.8-user-table-prefs-up.sql
    # The table identity.user_table_preferences must exist before the first
    # SaveChanges. For dev, apply to the Armoki clinic DB.
    #
    # ClinicDbFactory dependency: EfUserTablePreferenceStore calls
    # CreateForClinicAsync(clinicId) → IdentityDbContext (same call shape as
    # UserPreferenceService). If that method name/signature differs in the
    # current ClinicDbFactory, the store won't compile — flag it and I'll adjust.
    #
    # NOT in this stage (moved to Stage 1D, just before Stage 2):
    #   • lipi-table-tokens.css / lipi-table-print.css / lipi-status-tokens.css
    #   • lipi-table-interop.js
    #   • App.razor cache-version bump + <link>/<script> tags
    #   Nothing renders yet, so no CSS/JS is consumed until Stage 2 components ship.
    "LiPi.Web.csproj"                = "src\LiPi.Web\LiPi.Web.csproj"
    "EfUserTablePreferenceStore.cs"  = "src\LiPi.Web\Services\EfUserTablePreferenceStore.cs"
    "BlazorCurrentUserAccessor.cs"   = "src\LiPi.Web\Services\BlazorCurrentUserAccessor.cs"
    # -- Phase 2.8 — Data Display Stage 1D + Stage 2 -------------------------
    #    (Lipicons integration + LipiEmptyState, the first Data Display component)
    # SPEC:  docs/00-COMPONENTS/2.8/04-LipiEmptyState-Spec.md
    # AMEND: CHANGE-LOG.md A40 (Lipicons integration) + A41 (LipiEmptyState)
    #
    # STAGE 1D — Lipicons integration:
    #   Vendored LiPicons.Blazor into libs\LiPicons.Blazor\ (7 files: 6 source +
    #   icons.json). The icon strategy lock (A40) brings Lipicons into Phase 2.8
    #   (was roadmapped for 3.0). LiPi.Components + LiPi.Web reference the vendored
    #   project (full csproj files updated via their existing entries above). When
    #   LiPicons.Blazor publishes to a NuGet feed, the two ProjectReferences swap to
    #   a single PackageReference — see A40.
    #
    #   The vendored csproj differs from upstream ONLY in the EmbeddedResource path
    #   for icons.json (repointed ../../dist/json/icons.json -> local icons.json).
    #
    # STAGE 2 — LipiEmptyState (first rendered Data Display component):
    #   4 component files in LiPi.Components + 1 token CSS in LiPi.Web wwwroot +
    #   1 demo page (.razor + .razor.css) in LiPi.Web. Variant default icons use
    #   the A40 §5 Lipicons-native name mapping (empty-state/search/warning/
    #   check-circle/clock). lipi-empty-tokens.css is the single seam binding
    #   --lipi-empty-* to the real --color-* contract (A40 token reconciliation:
    #   text-faint->text-tertiary, danger-alpha-04->danger-pale,
    #   success-alpha-04->success-pale).
    #
    # MODIFIED files routed by EXISTING entries above (no new keys, full files):
    #   • App.razor              — entry at line ~24; cache bump 20260526->20260530
    #                              + new lipi-empty-tokens.css <link>
    #   • LiPi.Components.csproj  — entry above; +ProjectReference to LiPicons.Blazor
    #   • LiPi.Web.csproj         — entry above; +ProjectReference to LiPicons.Blazor
    #
    # NOTE: icons.json is 436KB — the full manifest, embedded in the assembly so
    # icons render offline (no wwwroot copy, no CDN). HIS embeds ALL 1,149 icons.
    # Stage 1D — vendored LiPicons.Blazor (libs\LiPicons.Blazor\)
    "LiPicons.Blazor.csproj"         = "libs\LiPicons.Blazor\LiPicons.Blazor.csproj"
    "LipiIcon.razor"                 = "libs\LiPicons.Blazor\LipiIcon.razor"
    "LipiconVariant.cs"              = "libs\LiPicons.Blazor\LipiconVariant.cs"
    "LipiconRenderer.cs"             = "libs\LiPicons.Blazor\LipiconRenderer.cs"
    "IconManifest.cs"                = "libs\LiPicons.Blazor\IconManifest.cs"
    "LipiconName.cs"                 = "libs\LiPicons.Blazor\LipiconName.cs"
    "icons.json"                     = "libs\LiPicons.Blazor\icons.json"
    # Stage 2 — LipiEmptyState component (LiPi.Components)
    "LipiEmptyStateTypes.cs"         = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyStateTypes.cs"
    "LipiEmptyState.razor"           = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor"
    "LipiEmptyState.razor.cs"        = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.cs"
    "LipiEmptyState.razor.css"       = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.css"
    # Stage 2 — token CSS (LiPi.Web wwwroot) + demo page (LiPi.Web StyleGuide)
    "lipi-empty-tokens.css"          = "src\LiPi.Web\wwwroot\css\lipi-empty-tokens.css"
    "StyleGuideDataDisplay.razor"     = "src\LiPi.Web\Pages\StyleGuideDataDisplay.razor"
    "StyleGuideTableFilters.razor"   = "src\LiPi.Web\Pages\StyleGuideTableFilters.razor"
    "StyleGuideTableFilters.razor.css" = "src\LiPi.Web\Pages\StyleGuideTableFilters.razor.css"
    "StyleGuideDataDisplay.razor.css" = "src\LiPi.Web\Pages\StyleGuideDataDisplay.razor.css"
    # -- Phase 2.8 — Data Display Stage 2 core shell (LipiTable bare chassis) ---
    # SPEC:  docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §2/§3/§3.6.1/§18/§19
    # AMEND: CHANGE-LOG.md A42 (2026-05-31)
    #
    # The "rolling chassis": generic LipiTable<TItem> + declarative LipiColumn<TItem,TValue>,
    # CSS-Grid layout (role="grid"), per-type cell rendering for the 14 ColumnTypes,
    # Items data path, and Loading/Error/Empty/Normal body states delegating to
    # LipiEmptyState. NO sort/filter/selection/pagination/edit/toolbar yet — those are
    # later stages (declared-but-inert params keep the API stable).
    #
    # New shared infra created here (didn't exist before): lipi-status-tokens.css —
    # the --color-status-* taxonomy + strip/chip classes (Phase 2.8 Overview §2.1/§2.3),
    # first consumed by LipiTable Status cells. lipi-table-tokens.css — the --lipi-table-*
    # seam bound to the real --color-* contract (themes/mode-*.css).
    #
    # MODIFIED files routed by EXISTING entries above (no new keys, full files):
    #   • App.razor                       — cache bump 20260530->20260531 + 2 CSS links
    #   • StyleGuideDataDisplay.razor      — +LipiTable demo section + DemoStaff data
    #   • StyleGuideDataDisplay.razor.css  — +sg-note-p / sg-subhead classes
    #
    # SCOPE NOTE flagged simplifications (completed in later stages):
    #   Status -> simple chip (not LipiBadge); Date/Time -> Format/invariant (no
    #   IDateFormatService inject); Avatar -> initials placeholder; Actions -> template-only.
    #
    # Component files (LiPi.Components):
    "ColumnDefinition.cs"            = "src\LiPi.Components\DataDisplay\LipiTable\ColumnDefinition.cs"
    "CellFormatter.cs"               = "src\LiPi.Components\DataDisplay\LipiTable\CellFormatter.cs"
    "LipiColumn.razor"               = "src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor"
    "LipiColumn.razor.cs"            = "src\LiPi.Components\DataDisplay\LipiTable\LipiColumn.razor.cs"
    "LipiTable.razor"                = "src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor"
    "LipiTable.razor.cs"             = "src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor.cs"
    "LipiTable.razor.css"            = "src\LiPi.Components\DataDisplay\LipiTable\LipiTable.razor.css"
    "IdentifierHumanizer.cs"         = "src\LiPi.Components\Shared\Internal\IdentifierHumanizer.cs"
    # A45: LipiTable copy affordance JS — component-local static web asset. Lives in
    # LiPi.Components\wwwroot\ so the Razor SDK serves it at _content/LiPi.Components/
    # lipi-table.js (self-contained package; packed into the NuGet via IsPackable). First
    # asset in this wwwroot — deploy auto-creates the directory.
    "lipi-table.js"                  = "src\LiPi.Components\wwwroot\lipi-table.js"
    # Shared CSS (LiPi.Web wwwroot) — new token files:
    "lipi-table-tokens.css"          = "src\LiPi.Web\wwwroot\css\lipi-table-tokens.css"
    "lipi-status-tokens.css"         = "src\LiPi.Web\wwwroot\css\lipi-status-tokens.css"
    # -- Phase 2.8 — Data Display Stage 7 (LipiPagination) --------------------
    # SPEC:  docs/00-COMPONENTS/2.8/03-LipiPagination-Spec.md
    # AMEND: CHANGE-LOG.md A50 (2026-06-01)
    #
    # LipiPagination — page navigation for LipiTable / LipiList. Three variants
    # (Full / Compact / Minimal per LP-0) collapsed into ONE LipiPagination.razor
    # via internal branching (LP-11). Three independently-reusable sub-components
    # kept separate: LipiPaginationPageSize (composes LipiSelect<PageSizeOption>,
    # LP-12), LipiPaginationCountDisplay (the "Showing X-Y of Z" readout), and
    # LipiPaginationLoadMore (a distinct append-on-demand paradigm, NOT a variant).
    # Composes the package-clean LipiSelect (A43/A44) + LipiButton (A49) — all-LiPi
    # surface, zero native controls.
    #
    # PaginationMath.cs is a PURE static helper under LiPi.Components.Internal.Algorithms
    # (the LP-14 page-window algorithm: boundary pages + 2*Siblings+1 centred window +
    # ellipsis for gaps>1 + all-fit guard). 15/15 unit tests pass. Per the generalized
    # "test pure with the SDK before Blazor" decision, non-trivial algorithms live as
    # pure helpers under Internal\Algorithms — the Phase 2.10 audit checks this.
    #
    # lipi-pagination-tokens.css (LiPi.Web wwwroot) is the --lipi-pagination-* seam
    # bound to the REAL --color-* contract. NOTE: spec §7 referenced token names that
    # don't exist in this project (--color-primary-50/500, --color-on-primary,
    # --color-text-faint/secondary, --r-2); remapped to the deployed foundation tokens
    # (--color-primary, --color-primary-pale, --color-bg-hover, --color-text-inverse,
    # --color-text-tertiary, --r-sm). Same seam pattern as lipi-table-tokens.css.
    #
    # MODIFIED files routed by EXISTING entries above (no new keys, full files):
    #   • App.razor       — cache bump 20260539->20260540 + lipi-pagination-tokens.css <link>
    #   • StyleGuide.razor — +"Pagination →" link under the Phase 2.8 nav group
    #
    # NOT in this stage: LipiTable-side wiring (compose LipiPagination into the table,
    # client-side paging, PaginationVariant/Placement/Mode params) — next slice; LipiList;
    # across-pages selection banner (4c). PDF export stays stubbed until 2.10; Excel deferred.
    #
    # Component files (LiPi.Components\DataDisplay\):
    "LipiPaginationTypes.cs"         = "src\LiPi.Components\DataDisplay\LipiPaginationTypes.cs"
    "LipiPagination.razor"           = "src\LiPi.Components\DataDisplay\LipiPagination.razor"
    "LipiPagination.razor.cs"        = "src\LiPi.Components\DataDisplay\LipiPagination.razor.cs"
    "LipiPagination.razor.css"       = "src\LiPi.Components\DataDisplay\LipiPagination.razor.css"
    "LipiPaginationPageSize.razor"     = "src\LiPi.Components\DataDisplay\LipiPaginationPageSize.razor"
    "LipiPaginationPageSize.razor.css" = "src\LiPi.Components\DataDisplay\LipiPaginationPageSize.razor.css"
    "LipiPaginationCountDisplay.razor"     = "src\LiPi.Components\DataDisplay\LipiPaginationCountDisplay.razor"
    "LipiPaginationCountDisplay.razor.css" = "src\LiPi.Components\DataDisplay\LipiPaginationCountDisplay.razor.css"
    "LipiPaginationLoadMore.razor"     = "src\LiPi.Components\DataDisplay\LipiPaginationLoadMore.razor"
    "LipiPaginationLoadMore.razor.css" = "src\LiPi.Components\DataDisplay\LipiPaginationLoadMore.razor.css"
    # Pure algorithm helper (LiPi.Components\Internal\Algorithms\):
    "PaginationMath.cs"              = "src\LiPi.Components\Internal\Algorithms\PaginationMath.cs"
    # Token seam (LiPi.Web wwwroot):
    "lipi-pagination-tokens.css"     = "src\LiPi.Web\wwwroot\css\lipi-pagination-tokens.css"
    # Standalone demo page (LiPi.Web):
    "StyleGuidePagination.razor"     = "src\LiPi.Web\Pages\StyleGuidePagination.razor"
    "StyleGuidePagination.razor.css" = "src\LiPi.Web\Pages\StyleGuidePagination.razor.css"
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
    # A49 (Stage 7 prereq): MIGRATED to LiPi.Components/Shared — package-clean
    # (LucideIcon → LipiIcon) so LipiPagination can compose it. Old LiPi.Web copies deleted.
    "LipiButton.razor"               = "src\LiPi.Components\Shared\LipiButton.razor"
    "LipiButton.razor.css"           = "src\LiPi.Components\Shared\LipiButton.razor.css"
    "LipiButtonTypes.cs"             = "src\LiPi.Components\Shared\LipiButtonTypes.cs"
    "LipiButtonSpinner.razor"        = "src\LiPi.Components\Shared\LipiButtonSpinner.razor"
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
    "LipiTextBox.razor"              = "src\LiPi.Components\Forms\LipiTextBox.razor"
    "LipiTextBox.razor.css"          = "src\LiPi.Components\Forms\LipiTextBox.razor.css"
    "LipiTextArea.razor"             = "src\LiPi.Components\Forms\LipiTextArea.razor"
    "LipiTextArea.razor.css"         = "src\LiPi.Components\Forms\LipiTextArea.razor.css"
    "LipiNumberInput.razor"          = "src\LiPi.Components\Forms\LipiNumberInput.razor"
    "LipiNumberInput.razor.css"      = "src\LiPi.Components\Forms\LipiNumberInput.razor.css"
    "LipiInputBase.cs"               = "src\LiPi.Components\Forms\LipiInputBase.cs"

    # DateTime migration (A54): package-side date/time helpers (LiPi.Components.Forms)
    "LipiTimeResolver.cs"            = "src\LiPi.Components\Forms\LipiTimeResolver.cs"
    "LipiDateFormat.cs"              = "src\LiPi.Components\Forms\LipiDateFormat.cs"
    "LipiSelectBase.cs"              = "src\LiPi.Components\Forms\LipiSelectBase.cs"
    "LipiSelect.razor"               = "src\LiPi.Components\Forms\LipiSelect.razor"
    "LipiSelect.razor.css"           = "src\LiPi.Components\Forms\LipiSelect.razor.css"
    "LipiCombobox.razor"             = "src\LiPi.Components\Forms\LipiCombobox.razor"
    "LipiCombobox.razor.css"         = "src\LiPi.Components\Forms\LipiCombobox.razor.css"
    "LipiTextInputTypes.cs"          = "src\LiPi.Components\Forms\LipiTextInputTypes.cs"
    "LipiInputDefaults.cs"           = "src\LiPi.Components\Forms\LipiInputDefaults.cs"
    "AutocompleteValidator.cs"       = "src\LiPi.Components\Forms\AutocompleteValidator.cs"
    # -- Components -- Phase 2.3 (Compound field family) -----------------------
    # SPEC: docs/00-COMPONENTS/01.3-CompoundField.md (shipping in Batch 9c)
    # AMEND: CHANGE-LOG.md A19 (pending)
    # Batch 9a (May 6): LipiContainerBase + ICompoundSegment + LipiCompoundField + SelectSegment + TextSegment + CompoundFieldTest
    "LipiContainerBase.cs"           = "src\LiPi.Components\Forms\LipiContainerBase.cs"
    "ICompoundSegment.cs"            = "src\LiPi.Components\Forms\ICompoundSegment.cs"
    "LipiCompoundField.razor"        = "src\LiPi.Components\Forms\LipiCompoundField.razor"
    "SelectSegment.razor"            = "src\LiPi.Components\Forms\SelectSegment.razor"
    "TextSegment.razor"              = "src\LiPi.Components\Forms\TextSegment.razor"
    # -- Components -- Phase 2.3 (Multi-select family) -------------------------
    # SPEC: docs/00-COMPONENTS/01.4-MultiSelect.md (shipping in Batch 9c-b)
    # AMEND: CHANGE-LOG.md A19 (pending)
    # Batch 9b (May 6/7): LipiMultiSelectBase + LipiMultiSelect + MultiSelectTest
    # Batch 9c-a (May 7): LipiMultiCombobox + MultiComboboxTest (templated multi-select)
    "LipiMultiSelectBase.cs"         = "src\LiPi.Components\Forms\LipiMultiSelectBase.cs"
    "LipiMultiSelect.razor"          = "src\LiPi.Components\Forms\LipiMultiSelect.razor"
    "LipiMultiCombobox.razor"        = "src\LiPi.Components\Forms\LipiMultiCombobox.razor"
    # -- Components -- Phase 2.4 (Date/Time family) ----------------------------
    # SPEC: docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d)
    # AMEND: CHANGE-LOG.md A20 (pending)
    # Batch 9d (May 7): Single-batch ship of full Date/Time family.
    # Architecture: LipiDatePicker / LipiTimePicker / LipiDateTimePicker inherit
    # LipiInputBase. LipiDateRangePicker inherits ComponentBase directly (dual-
    # value binding shape). Services + types support all four.
    "LipiDateTimeTypes.cs"          = "src\LiPi.Components\Forms\LipiDateTimeTypes.cs"
    "LipiDatePicker.razor"           = "src\LiPi.Components\Forms\LipiDatePicker.razor"
    "LipiDatePicker.razor.css"       = "src\LiPi.Components\Forms\LipiDatePicker.razor.css"
    "LipiTimePicker.razor"           = "src\LiPi.Components\Forms\LipiTimePicker.razor"
    "LipiTimePicker.razor.css"       = "src\LiPi.Components\Forms\LipiTimePicker.razor.css"
    "LipiDateTimePicker.razor"       = "src\LiPi.Components\Forms\LipiDateTimePicker.razor"
    "LipiDateTimePicker.razor.css"   = "src\LiPi.Components\Forms\LipiDateTimePicker.razor.css"
    "LipiDateRangePicker.razor"      = "src\LiPi.Components\Forms\LipiDateRangePicker.razor"
    "LipiDateRangePicker.razor.css"  = "src\LiPi.Components\Forms\LipiDateRangePicker.razor.css"
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
    "LipiSelectionTypes.cs"          = "src\LiPi.Components\Forms\LipiSelectionTypes.cs"
    "MustBeTrueAttribute.cs"         = "src\LiPi.Components\Forms\MustBeTrueAttribute.cs"
    "LipiCheckboxGroupContext.cs"    = "src\LiPi.Components\Forms\LipiCheckboxGroupContext.cs"
    "LipiRadioGroupContext.cs"       = "src\LiPi.Components\Forms\LipiRadioGroupContext.cs"
    "lipi-selection-family.css"      = "src\LiPi.Web\wwwroot\css\lipi-selection-family.css"
    "lipi-form-utilities.css"        = "src\LiPi.Web\wwwroot\css\lipi-form-utilities.css"
    "LipiCheckbox.razor"             = "src\LiPi.Components\Forms\LipiCheckbox.razor"
    "LipiCheckbox.razor.css"         = "src\LiPi.Components\Forms\LipiCheckbox.razor.css"
    "LipiCheckboxGroup.razor"        = "src\LiPi.Components\Forms\LipiCheckboxGroup.razor"
    "LipiCheckboxGroup.razor.css"    = "src\LiPi.Components\Forms\LipiCheckboxGroup.razor.css"
    "LipiRadio.razor"                = "src\LiPi.Components\Forms\LipiRadio.razor"
    "LipiRadio.razor.css"            = "src\LiPi.Components\Forms\LipiRadio.razor.css"
    "LipiRadioGroup.razor"           = "src\LiPi.Components\Forms\LipiRadioGroup.razor"
    "LipiRadioGroup.razor.css"       = "src\LiPi.Components\Forms\LipiRadioGroup.razor.css"
    "LipiToggle.razor"               = "src\LiPi.Components\Forms\LipiToggle.razor"
    "LipiToggle.razor.css"           = "src\LiPi.Components\Forms\LipiToggle.razor.css"
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
    # A35 (2026-05-15): Phase 2.7 Feedback Components consolidated showcase.
    # Single @page /admin/style-guide/feedback covering all 6 components
    # (Spinner, Badge, Pill, Skeleton×3, ValidationSummary, Toast). Sidebar
    # link added to main StyleGuide.razor under new "Phase 2.7" nav group.
    "StyleGuideFeedback.razor"       = "src\LiPi.Web\Pages\StyleGuideFeedback.razor"
    "StyleGuideFeedback.razor.css"   = "src\LiPi.Web\Pages\StyleGuideFeedback.razor.css"
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
    # -- Docs -- Phase 2.7 specs (A35, mirrors A34's 2.6.2-spec mapping fix) ---
    # Six spec docs covering the 6 Phase 2.7 components. Mapped from the
    # initial drop so edits flow through the standard deploy workflow from
    # day one (vs A29/A34 which had to retro-fit mappings after the fact).
    "00-Phase2.7-Overview.md"        = "docs\00-COMPONENTS\2.7\00-Phase2.7-Overview.md"
    "01-LipiSpinner-Spec.md"         = "docs\00-COMPONENTS\2.7\01-LipiSpinner-Spec.md"
    "02-LipiBadge-Pill-Spec.md"      = "docs\00-COMPONENTS\2.7\02-LipiBadge-Pill-Spec.md"
    "03-LipiSkeleton-Spec.md"        = "docs\00-COMPONENTS\2.7\03-LipiSkeleton-Spec.md"
    "04-LipiValidationSummary-Spec.md" = "docs\00-COMPONENTS\2.7\04-LipiValidationSummary-Spec.md"
    "05-LipiToast-Spec.md"           = "docs\00-COMPONENTS\2.7\05-LipiToast-Spec.md"
    # -- Docs -- Phase 2.10 audit checklist (A36) ------------------------------
    # Consolidated tracking doc for every Phase 2.10 Infrastructure Audit item,
    # regardless of source (locked roadmap, earlier audits, Phase 2.x builds).
    # Living doc: items append as later phases surface them; items removed as
    # the audit fixes/defers/confirms them. See CHANGE-LOG.md A36.
    "2.10-Audit-Checklist.md"        = "docs\00-COMPONENTS\2.10-Audit-Checklist.md"
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
