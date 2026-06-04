// SPEC:     docs/00-COMPONENTS/2.7/01-LipiSpinner-Spec.md §4
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// Type definitions for LipiSpinner. Pattern matches LipiButtonTypes.cs (Phase 2.1)
// and LipiTextInputTypes.cs (Phase 2.2) — short enum names without Lipi prefix,
// colocated with the LipiSpinner component in the LiPi.Components.Feedback namespace.
//
// LabelPosition is intentionally reused from LipiSelectionTypes.cs (Phase 2.5.5)
// as the shared cross-family enum (InputLabelPosition). LipiSpinner consumes
// InputLabelPosition.Right (default), .Bottom, .Top, and .Left.

namespace LiPi.Components.Feedback;

/// <summary>
/// Diameter variant for LipiSpinner. Pixel sizes match common use cases:
/// XSmall(12) inline with text and small badges; Small(16) table rows and
/// small components; Medium(24) default for sections and cards; Large(48)
/// page-center and full-card loading states.
/// </summary>
public enum SpinnerSize
{
    /// <summary>12px — inline with body text, badges, dense layouts.</summary>
    XSmall,

    /// <summary>16px — table rows, small inline components.</summary>
    Small,

    /// <summary>24px — default. Sections, cards, mid-page loaders.</summary>
    Medium,

    /// <summary>48px — page-center, full-card loading, splash states.</summary>
    Large
}

/// <summary>
/// Color intent for LipiSpinner. Default uses <c>currentColor</c> so the spinner
/// adopts the parent's text color — useful inside colored buttons or banners.
/// Primary forces the brand navy. Subtle uses tertiary text color for muted
/// states. Inverse forces white for placement on dark backdrops.
/// <para>
/// The <c>Color</c> parameter (raw CSS color string) overrides this enum when set.
/// </para>
/// </summary>
public enum SpinnerIntent
{
    /// <summary>currentColor — inherits text color of the parent element.</summary>
    Default,

    /// <summary>--color-primary — LiPi brand navy.</summary>
    Primary,

    /// <summary>--color-text-tertiary — muted for low-emphasis loading.</summary>
    Subtle,

    /// <summary>White — for dark backdrops (modals, dark-mode toasts, hero banners).</summary>
    Inverse
}
