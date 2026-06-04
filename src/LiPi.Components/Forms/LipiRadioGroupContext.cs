// SPEC:     docs/03-Phase-2.5-Selection-Components/03-LipiRadio-and-Group.md
//           (Group context — same shape as LipiCheckboxGroupContext, distinct type)
// DECISION: Phase 2.5 — Selection Components family
//           Audit item #12: Disabled field dropped (native fieldset cascade);
//           OnSelected field dropped (radio selection uses standard
//           ValueChanged callback for clicks + group-level keyboard handler
//           for arrows; OnSelected served no purpose).
// PHASE:    Phase 2 Sub-step 2.5 — Selection components
// AMEND:    docs/CHANGE-LOG.md A15 (May 9, 2026): file ships
//
// Cascaded by LipiRadioGroup<TItem, TValue>.razor via:
//   <CascadingValue Value="..." Name="LipiRadioGroupContext" IsFixed="false">
//
// Consumed by LipiRadio<TValue> children via:
//   [CascadingParameter(Name = "LipiRadioGroupContext")]
//   public LipiRadioGroupContext? GroupContext { get; set; }
//
// Pattern matches ThemeContext.cs (Phase 1, Theming) and
// LipiCheckboxGroupContext.cs (this phase) — cascade record in its own file,
// colocated with the consuming component family.
//
// Distinct from LipiCheckboxGroupContext for type safety (audit item #12):
// the records have IDENTICAL shape today, but the cascade Name strings
// ("LipiCheckboxGroupContext" vs "LipiRadioGroupContext") differ. Rendering
// a LipiCheckbox inside LipiRadioGroup — or vice versa — produces a
// parameter resolution miss, failing explicitly rather than silently
// coordinating wrong. Cheap insurance.
//
// Note: there is NO Disabled field in this record. Disabled cascade is
// handled by native HTML5 <fieldset disabled> — no C# wiring per child
// needed. There is also NO OnSelected callback: keyboard selection happens
// at group level (Pattern A roving tabindex per WAI-ARIA APG); click
// selection uses standard ValueChanged on individual radios wired through
// the group's @foreach loop, just like checkbox children.

using Microsoft.AspNetCore.Components;

namespace LiPi.Components;

/// <summary>
/// Cascade context broadcast from LipiRadioGroup to its LipiRadio children.
/// Carries shared state (ReadOnly) and a callback for touched-state coordination.
/// Same shape as <see cref="LipiCheckboxGroupContext"/> but distinct type for
/// cascade safety.
/// </summary>
/// <param name="ReadOnly">
/// True when the group is read-only. LipiRadio children resolve their effective
/// read-only state as <c>(GroupContext?.ReadOnly ?? false) || ReadOnly</c> — group
/// read-only forces all children read-only; individual children can additionally
/// be read-only when the group is not.
/// </param>
/// <param name="OnChildInteracted">
/// Callback invoked by a child radio when it receives focus or is toggled.
/// The group uses this to mark itself as "touched" for validation purposes.
/// See Decision 2.13 (carried from LipiCheckboxGroup §2.13) for the
/// touched-state cascade contract.
/// </param>
public record LipiRadioGroupContext(
    bool ReadOnly,
    EventCallback OnChildInteracted
);
