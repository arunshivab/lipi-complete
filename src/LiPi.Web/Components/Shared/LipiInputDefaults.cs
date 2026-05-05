// SPEC:     docs/00-COMPONENTS/01.2-TextInputs.md (pending — Phase 2.2 Sub-step)
// DECISION: docs/00-PROJECT-BASELINE.md §12 (Component Library + Multi-Theme)
// PHASE:    Phase 2 Sub-step 2.2 — TextInput component family
// AMEND:    docs/CHANGE-LOG.md A12 (token additions, May 4, 2026)
//
// Global defaults for the LipiTextBox / LipiTextArea / LipiNumberInput / LipiSelect family.
// Each component parameter is nullable; null falls through to the value set here.
//
// Configured in Program.cs via:
//   builder.Services.Configure<LipiInputDefaults>(o => {
//       o.RequiredVisualStyle = RequiredVisualStyle.AsteriskOnly;
//       o.ShowNumberSteppers = false;
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
}
