// SPEC: docs/00-COMPONENTS/01.3-CompoundField.md (shipping in Batch 9c)
// PHASE: 2.3 (Batch 9a)
// AMEND: docs/CHANGE-LOG.md A19 (pending — Phase 2.3 close-out)

using System.Threading.Tasks;

namespace LiPi.Components;

/// <summary>
/// Contract for segments that participate in a LipiCompoundField. Implementing
/// segments register themselves with the parent via CascadingParameter on init
/// and unregister on dispose. The parent uses this interface to:
/// <list type="bullet">
///   <item><description>Aggregate touched-state across all segments (Q2 = Z: focus-leaves-compound)</description></item>
///   <item><description>Aggregate error state for the field-level red outline (Q3 = C+C1: per-segment border + field-level outline when any errored)</description></item>
///   <item><description>Pull the FIRST failing segment's message into the parent's helper slot (locked Q3 sub-decision)</description></item>
///   <item><description>Auto-advance focus on segment-completion (Q4 = C: smart auto-advance)</description></item>
/// </list>
///
/// Phase 2.3 ships two implementations: SelectSegment (dropdown) and TextSegment
/// (plain text input). Phase 2.4+ may add DateSegment (waits for LipiDatePicker).
/// </summary>
public interface ICompoundSegment
{
    /// <summary>True after the user has interacted with this segment (selected an
    /// item, typed input, or visited and blurred). Used by the parent to gate
    /// validation message display until any segment has been touched.</summary>
    bool IsTouched { get; }

    /// <summary>True when this segment has a validation failure that should be
    /// surfaced. Mirrors ResolvedState == InputState.Error on the underlying
    /// LipiInputBase descendant.</summary>
    bool HasError { get; }

    /// <summary>The message text for the segment's current error, if any. Used
    /// by the parent to populate its single helper slot — the first segment with
    /// HasError=true wins (left-to-right segment order).</summary>
    string? ErrorMessage { get; }

    /// <summary>Called by the parent during auto-advance (Q4) to move focus into
    /// this segment. Implementations should focus their primary interactive
    /// element (text input or dropdown anchor) and place cursor at position 0
    /// for text inputs (no auto-select-all per Q3 refinement).</summary>
    ValueTask FocusAsync();
}
