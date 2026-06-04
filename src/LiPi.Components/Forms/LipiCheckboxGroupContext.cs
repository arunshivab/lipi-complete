// SPEC:     docs/03-Phase-2.5-Selection-Components/02-LipiCheckboxGroup.md §2.9
//           (Disabled propagation + ReadOnly cascade)
// DECISION: Phase 2.5 — Selection Components family
//           Audit item #12: Disabled field dropped (native fieldset cascade
//           handles disabled propagation; field was set but never read).
// PHASE:    Phase 2 Sub-step 2.5 — Selection components
// AMEND:    docs/CHANGE-LOG.md A15 (May 9, 2026): file ships
//
// Cascaded by LipiCheckboxGroup<TItem, TValue>.razor via:
//   <CascadingValue Value="..." Name="LipiCheckboxGroupContext" IsFixed="false">
//
// Consumed by LipiCheckbox<TValue> children via:
//   [CascadingParameter(Name = "LipiCheckboxGroupContext")]
//   public LipiCheckboxGroupContext? GroupContext { get; set; }
//
// Pattern matches ThemeContext.cs (Phase 1, Theming) — cascade record in its
// own file, colocated with the consuming component family.
//
// Distinct from LipiRadioGroupContext for type safety (audit item #12):
// rendering a LipiCheckbox inside LipiRadioGroup — or vice versa — would
// produce a parameter resolution miss (different Name string), failing
// explicitly rather than silently coordinating wrong.
//
// Note: there is NO Disabled field in this record. Disabled cascade is
// handled by native HTML5 <fieldset disabled> — no C# wiring per child
// needed. The record carries only ReadOnly (no native HTML cascade for
// readonly on checkboxes) and OnChildInteracted (touched-state coordination
// per Decision 2.13).

using Microsoft.AspNetCore.Components;

namespace LiPi.Components;

/// <summary>
/// Cascade context broadcast from LipiCheckboxGroup to its LipiCheckbox children.
/// Carries shared state (ReadOnly) and a callback for touched-state coordination.
/// </summary>
/// <param name="ReadOnly">
/// True when the group is read-only. LipiCheckbox children resolve their effective
/// read-only state as <c>(GroupContext?.ReadOnly ?? false) || ReadOnly</c> — group
/// read-only forces all children read-only; individual children can additionally
/// be read-only when the group is not.
/// </param>
/// <param name="OnChildInteracted">
/// Callback invoked by a child checkbox when it receives focus or is toggled.
/// The group uses this to mark itself as "touched" for validation purposes
/// (apricot tint clears, error messages can fire, EditContext notifications flow).
/// See Decision 2.13 for the touched-state cascade contract.
/// </param>
public record LipiCheckboxGroupContext(
    bool ReadOnly,
    EventCallback OnChildInteracted
);
