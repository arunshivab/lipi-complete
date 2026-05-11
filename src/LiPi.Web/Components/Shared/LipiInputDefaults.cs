// SPEC:     docs/00-COMPONENTS/01.2-TextInputs.md (pending — Phase 2.2 Sub-step)
// DECISION: docs/00-PROJECT-BASELINE.md §12 (Component Library + Multi-Theme)
// PHASE:    Phase 2 Sub-step 2.2 — TextInput component family (extended in 2.5)
// AMEND:    docs/CHANGE-LOG.md
//           A12 (May 4, 2026):  Phase 2.2 token additions
//           A15 (May 9, 2026):  Phase 2.5 selection-family defaults
//                + CheckboxGroupDensity (default Standard)
//                + RadioGroupDensity (default Standard)
//                + RadioGroupAllowClear (default false — opt-in per use case)
//                SPEC: docs/03-Phase-2.5-Selection-Components/05-Cross-Cutting.md §5.1
//                SPEC: docs/03-Phase-2.5-Selection-Components/03-LipiRadio-and-Group.md
//                      §"Deselection semantics"
//
// Global defaults for the LipiTextBox / LipiTextArea / LipiNumberInput / LipiSelect family,
// extended in Phase 2.5 with selection-family defaults (LipiCheckboxGroup / LipiRadioGroup).
// Each component parameter is nullable; null falls through to the value set here.
//
// Configured in Program.cs via:
//   builder.Services.Configure<LipiInputDefaults>(o => {
//       o.RequiredVisualStyle = RequiredVisualStyle.AsteriskOnly;
//       o.ShowNumberSteppers = false;
//       o.RadioGroupAllowClear = true;  // clinic-wide opt-in if HIS workflow needs it
//   });
//
// Resolved in components via:
//   [Inject] IOptions<LipiInputDefaults> Defaults { get; set; }
//   var resolved = LocalParam ?? Defaults.Value.RequiredVisualStyle;
//
// Why IOptions<> not a singleton: matches Microsoft idiom, supports IOptionsMonitor
// for hot-reload in v1.1+, supports test isolation via service replacement.

namespace LiPi.Web.Components.Shared;

/// <summary>
/// App-wide defaults for the Lipi input component family.
/// Configure once in Program.cs; per-component parameters override when explicitly set.
/// </summary>
public sealed class LipiInputDefaults
{
    /// <summary>
    /// How required fields are visually marked when empty.
    /// Default: <see cref="RequiredVisualStyle.ApricotTint"/> — pairs with red asterisk +
    /// aria-label for HIS context (high-density forms, scanning under time pressure).
    /// Set to <see cref="RequiredVisualStyle.AsteriskOnly"/> for industry-standard minimal marking.
    /// </summary>
    public RequiredVisualStyle RequiredVisualStyle { get; set; } = RequiredVisualStyle.ApricotTint;

    /// <summary>
    /// Default size for input components when not explicitly specified.
    /// Default: <see cref="InputSize.Medium"/> (32px height) — matches LipiButton default.
    /// </summary>
    public InputSize Size { get; set; } = InputSize.Medium;

    /// <summary>
    /// Whether LipiNumberInput components show +/- stepper buttons by default.
    /// Default: false — opt-in per field. Steppers are visually noisy on dense forms;
    /// most numeric fields (age, dose, count) do better with keyboard-only input.
    /// </summary>
    public bool ShowNumberSteppers { get; set; } = false;

    /// <summary>
    /// Whether the helper text slot is always reserved (16px even when empty),
    /// or collapses when no helper/error/warning/success message is present.
    /// Default: true — prevents layout shift when validation messages appear.
    /// </summary>
    public bool AlwaysReserveHelperSlot { get; set; } = true;

    /// <summary>
    /// Locale for number formatting in LipiNumberInput.
    /// Default: "en-IN" — Indian grouping (1,23,456) for HIS deployment.
    /// Set to "en-US" or other BCP-47 tag for non-Indian deployments.
    /// </summary>
    public string NumberLocale { get; set; } = "en-IN";

    // ==========================================================================
    // Phase 2.5 — Selection-family defaults (CheckboxGroup / RadioGroup)
    // ==========================================================================
    // Naming-collision note: the property `CheckboxGroupDensity` shadows the enum
    // type of the same name in instance-member scope. C# resolves the bare
    // identifier to the property, not the type. Fully qualifying the enum reference
    // (LiPi.Web.Components.Shared.CheckboxGroupDensity.Standard) avoids the
    // collision. The property name stays as-is to match the per-component parameter
    // naming on LipiCheckboxGroup / LipiRadioGroup. This pattern parallels the
    // existing RequiredVisualStyle property/enum pair (see LipiInputBase.cs for
    // the precedent and resolution pattern).

    /// <summary>
    /// Default density (row-gap and column-gap spacing) for LipiCheckboxGroup options.
    /// Default: <see cref="CheckboxGroupDensity.Standard"/> — 8px row, 16px col.
    /// Tight = 2px row, 12px col (compact tables/lists).
    /// Spacious = 16px row, 24px col (consents, attestations, large-touch contexts).
    /// Per-component override available via the Density parameter on LipiCheckboxGroup.
    /// </summary>
    public CheckboxGroupDensity CheckboxGroupDensity { get; set; } = CheckboxGroupDensity.Standard;

    /// <summary>
    /// Default density for LipiRadioGroup options. Reuses the CheckboxGroupDensity enum
    /// (selection family shares the density vocabulary). Independent default value from
    /// CheckboxGroupDensity — allows clinics to set looser radio defaults if needed.
    /// Default: <see cref="CheckboxGroupDensity.Standard"/>.
    /// Per-component override available via the Density parameter on LipiRadioGroup.
    /// </summary>
    public CheckboxGroupDensity RadioGroupDensity { get; set; } = CheckboxGroupDensity.Standard;

    /// <summary>
    /// Whether LipiRadioGroup allows the user to CLEAR a selection back to null
    /// (via "Clear" link in legend + Esc key). Default: false — opt-in per use case.
    /// <para>
    /// Default rationale: matches industry convention (native HTML, Material, Carbon,
    /// Ant, Radix all default to no-deselection). Esc-key collision risk in overlay
    /// contexts (Phase 2.6 Modal, dialogs, drawers) is the dominant concern — a user
    /// pressing Esc to dismiss an overlay would silently wipe a radio selection
    /// underneath.
    /// </para>
    /// <para>
    /// Pages that genuinely need clearing (triage revisit, working diagnosis,
    /// provisional categorization) explicitly opt in via <c>AllowClear="true"</c>
    /// on the LipiRadioGroup component. Clinics that want clinical-flow-friendly
    /// clear-by-default everywhere can flip this DI default to true.
    /// </para>
    /// </summary>
    public bool RadioGroupAllowClear { get; set; } = false;
}
