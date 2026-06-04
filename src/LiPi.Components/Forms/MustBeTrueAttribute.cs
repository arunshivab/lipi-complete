// SPEC:     docs/03-Phase-2.5-Selection-Components/01-LipiCheckbox.md
//           §"MustBeTrueAttribute contract"
// DECISION: Phase 2.5 — Selection Components family (audit item #10 contract,
//           audit item #16 path/namespace)
// PHASE:    Phase 2 Sub-step 2.5 — Selection components
// AMEND:    docs/CHANGE-LOG.md A15 (May 9, 2026): file ships
//
// Validation attribute requiring a bool property to be true.
// Used for terms acceptance, HIPAA consent, required setting toggles, and
// other "must be true" gates on bool properties.
//
// Use on:
//   bool properties where the only valid answer is `true`
//     [MustBeTrue(ErrorMessage = "You must accept the HIPAA Privacy Notice")]
//     public bool AcceptedHipaaNotice { get; set; }
//
// DO NOT use on bool? properties (genuine tri-state required questions).
// For bool?, use [Required] instead — null then triggers standard Required
// semantics ("you haven't answered yet").
//
// Composition: [Required] + [MustBeTrue] is allowed but redundant on
// non-nullable bool (non-nullable bool always has a value, even if false).
// Prefer [MustBeTrue] alone for the "must accept" pattern.
//
// Pattern matches AutocompleteValidator.cs (Phase 2.2) — validation helpers
// colocated in LiPi.Components namespace alongside consuming
// components, rather than a separate validation infrastructure layer.

using System.ComponentModel.DataAnnotations;

namespace LiPi.Components;

/// <summary>
/// Validation attribute requiring a <see cref="bool"/> property to be <c>true</c>.
/// <para>
/// Behavior: <c>true</c> → valid; <c>false</c> → invalid; <c>null</c> → invalid;
/// any non-<see cref="bool"/> value → invalid (defensive).
/// </para>
/// <para>
/// Use on <see cref="bool"/> properties for terms acceptance, HIPAA consent, and
/// required setting toggles. For <c>bool?</c> properties (genuine tri-state
/// required questions), use <see cref="RequiredAttribute"/> instead.
/// </para>
/// <para>
/// Default error message: <c>"The {0} field must be true."</c> Override via
/// <see cref="ValidationAttribute.ErrorMessage"/> for production messaging.
/// </para>
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false)]
public sealed class MustBeTrueAttribute : ValidationAttribute
{
    public MustBeTrueAttribute()
        : base("The {0} field must be true.") { }

    public override bool IsValid(object? value)
    {
        return value switch
        {
            bool b => b,
            null => false,
            _ => false  // any non-bool, non-null value is invalid (defensive)
        };
    }
}
