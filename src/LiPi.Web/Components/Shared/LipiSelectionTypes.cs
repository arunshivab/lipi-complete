// SPEC:     docs/03-Phase-2.5-Selection-Components/05-Cross-Cutting.md §5.4
// DECISION: Phase 2.5 — Selection Components family
// PHASE:    Phase 2 Sub-step 2.5 — Selection components
//           (Checkbox, CheckboxGroup, Radio, RadioGroup, Toggle)
// AMEND:    docs/CHANGE-LOG.md
//           A15 (May 9, 2026): file ships with CheckboxGroupDensity (step 2);
//                              extended with CheckboxGroupOrientation and
//                              InputLabelPosition (step 4)
//
// Type definitions shared by LipiCheckbox / LipiCheckboxGroup /
// LipiRadio / LipiRadioGroup / LipiToggle.
// Pattern matches LipiButtonTypes.cs (Phase 2.1) and
// LipiTextInputTypes.cs (Phase 2.2) — short enum names without
// Lipi prefix, colocated with consuming components in
// LiPi.Web.Components.Shared namespace.
//
// Per audit item #15c: InputLabelPosition is a SHARED enum across
// LipiCheckbox, LipiRadio, and LipiToggle (identical values, identical
// semantics, identical CSS output). Phase 2.5.5 will extend usage to
// LipiTextBox, LipiSelect, LipiDate, etc., when the cross-family
// LabelPosition retrofit ships. Naming chosen for that forward-compat
// (matches InputSize / InputState family naming).

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Density (row-gap and column-gap spacing) for LipiCheckboxGroup and LipiRadioGroup
/// options containers. Selection family shares the density vocabulary — the same enum
/// drives both group types. Per-deployment defaults via
/// <see cref="LipiInputDefaults.CheckboxGroupDensity"/> and
/// <see cref="LipiInputDefaults.RadioGroupDensity"/>; per-component override via
/// the Density parameter on each group component.
/// </summary>
public enum CheckboxGroupDensity
{
    /// <summary>2px row gap, 12px col gap. Compact tables/lists where vertical density matters.</summary>
    Tight,

    /// <summary>8px row gap, 16px col gap. Default for forms — comfortable scanning, no crowding.</summary>
    Standard,

    /// <summary>16px row gap, 24px col gap. Consents, attestations, large-touch contexts (mobile/glove use).</summary>
    Spacious
}

/// <summary>
/// Layout direction for LipiCheckboxGroup and LipiRadioGroup options containers.
/// Default <see cref="Vertical"/>. CSS rules live in lipi-selection-family.css per
/// the Layout-vs-Shape rule (audit item #1) — component scoped CSS does not own
/// any display/flex/grid declarations.
/// </summary>
public enum CheckboxGroupOrientation
{
    /// <summary>flex-direction: column (default). Standard form layout — one option per row.</summary>
    Vertical,

    /// <summary>flex-direction: row + flex-wrap: wrap. Use for short option lists (3-5 items) that fit one line.</summary>
    Horizontal,

    /// <summary>display: grid + repeat(N, 1fr). Use for medium lists (6-30 items). Columns parameter sets N (2-6).</summary>
    Grid
}

/// <summary>
/// Position of the label relative to the indicator (checkbox square / radio circle /
/// toggle pill) for individual selection components: LipiCheckbox, LipiRadio, LipiToggle.
/// <para>
/// Default <see cref="Right"/> — label text to the right of the indicator (LTR reading
/// order, matches native HTML and every major design system).
/// </para>
/// <para>
/// Phase 2.5.5 (Label Position cross-family retrofit) extends usage of this enum to
/// LipiTextBox, LipiSelect, LipiNumberInput, LipiDate, LipiCheckboxGroup, LipiRadioGroup
/// for unified label-position behavior across the entire input family.
/// </para>
/// <para>
/// CSS class output: <c>.lipi-input-label-pos-{right,left,top,bottom}</c> (shared family).
/// RTL (right-to-left languages) deferred to v2.x.
/// </para>
/// </summary>
public enum InputLabelPosition
{
    /// <summary>flex-direction: row (default). Indicator on left, label on right. Most common, LTR reading order.</summary>
    Right,

    /// <summary>flex-direction: row-reverse. Label on left, indicator on right. Settings tables, choice columns.</summary>
    Left,

    /// <summary>flex-direction: column; align-items: center. Indicator above label. Card-style choice tiles.</summary>
    Top,

    /// <summary>flex-direction: column-reverse. Label above indicator. Caption-style (uncommon).</summary>
    Bottom
}
