// SPEC:     docs/00-COMPONENTS/2.7/02-LipiBadge-Pill-Spec.md §2 (LipiBadge)
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// Type definitions for LipiBadge (the attached count/dot variant). Sibling
// component LipiPill (standalone label) lives in LipiPillTypes.cs. Both
// follow the existing LiPi.Web.Components.Shared enum-naming pattern:
// short names without Lipi prefix.

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Visual color theme for LipiBadge. Default <see cref="Danger"/> matches the
/// universal notification convention (red unread badges). Other colors cover
/// pending (Warning), completed counts (Success), informational (Info),
/// generic counts (Neutral), and branded (Primary — LiPi navy).
/// </summary>
public enum BadgeColor
{
    /// <summary>Red — default, notification convention.</summary>
    Danger,

    /// <summary>Amber — pending items.</summary>
    Warning,

    /// <summary>Green — completed/successful counts.</summary>
    Success,

    /// <summary>Blue — informational counts.</summary>
    Info,

    /// <summary>Gray — generic counts with no semantic meaning.</summary>
    Neutral,

    /// <summary>LiPi navy — branded counts.</summary>
    Primary
}

/// <summary>
/// Corner attachment point for LipiBadge when not in <c>Inline</c> mode.
/// Default <see cref="TopRight"/> matches the notification convention. The
/// badge is absolutely positioned via these classes; its parent must be
/// <c>position: relative</c> for correct anchoring.
/// </summary>
public enum BadgePosition
{
    /// <summary>Top-right corner (default — notifications convention).</summary>
    TopRight,

    /// <summary>Top-left corner.</summary>
    TopLeft,

    /// <summary>Bottom-right corner.</summary>
    BottomRight,

    /// <summary>Bottom-left corner.</summary>
    BottomLeft
}
