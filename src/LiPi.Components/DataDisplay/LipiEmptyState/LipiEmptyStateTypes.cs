// SPEC: docs/00-COMPONENTS/2.8/04-LipiEmptyState-Spec.md
//   §0.1 (EmptyStateVariant enum), §2.3 (EmptyStateSize enum)
// PHASE: 2.8 Data Display — Stage 2 (LipiEmptyState)
// COMPONENT: LipiEmptyState
//
// Enums for the zero-state primitive. Variant drives the default icon + tone;
// Size drives dimensions (min-height, icon size, type scale, gaps, padding).
// Both are short names without the Lipi prefix per project convention.

namespace LiPi.Components.DataDisplay;

/// <summary>
/// The kind of empty state, which determines the default icon (§0.2), the default CTA
/// tone, and a subtle container accent. Callers can override every default. Per §0.1.
/// </summary>
public enum EmptyStateVariant
{
    /// <summary>Generic empty — caller picks everything. Default icon: empty-state.</summary>
    Default,

    /// <summary>"No data exists yet". Default icon: empty-state.</summary>
    Empty,

    /// <summary>"Filters yield zero". Default icon: search (see A40 §5 mapping).</summary>
    FilteredEmpty,

    /// <summary>"Load failed". Default icon: warning (see A40 §5 mapping). Danger tone.</summary>
    Error,

    /// <summary>"Task complete, nothing pending". Default icon: check-circle. Success tone.</summary>
    Success,

    /// <summary>"Feature coming soon". Default icon: clock. Info tone.</summary>
    Coming
}

/// <summary>
/// Size of the empty state, controlling min-height, icon size, type scale, element gaps,
/// body max-width, and padding (§2.3). Defaults to Inline — the dominant use case
/// (LipiTable / LipiList body states).
/// </summary>
public enum EmptyStateSize
{
    /// <summary>~120px tall — fits inside a table / list empty slot.</summary>
    Inline,

    /// <summary>~200px tall — fits inside a card.</summary>
    Card,

    /// <summary>~320px tall — full-page empty state with larger icon + spacing.</summary>
    Page
}
