// SPEC:     docs/00-COMPONENTS/2.7/02-LipiBadge-Pill-Spec.md §3 (LipiPill)
// PHASE:    Phase 2 Sub-step 2.7 — Feedback Components family
// AMEND:    docs/CHANGE-LOG.md A35 (2026-05-15)
//
// Type definitions for LipiPill (standalone label/tag/chip). Sibling to
// LipiBadge but conceptually different — pill is inline in flow, badge is
// attached to a parent element. Material/MUI/AntD all split these the same
// way.
//
// Pattern matches LipiBadgeTypes.cs: short enum names without Lipi prefix,
// colocated with consuming components in LiPi.Web.Components.Shared.

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Visual intent (color theme) for LipiPill. Default is unsemantic and uses
/// the neutral surface palette. Other intents map to standard semantic colors.
/// </summary>
public enum PillIntent
{
    /// <summary>Neutral surface — no semantic color, generic label.</summary>
    Default,

    /// <summary>Brand primary (LiPi navy).</summary>
    Primary,

    /// <summary>Green — completed states, online indicators.</summary>
    Success,

    /// <summary>Amber — pending or attention-needed states.</summary>
    Warning,

    /// <summary>Red — critical or error states (e.g., "DNR", "Critical").</summary>
    Danger,

    /// <summary>Blue — informational tags (e.g., version, type labels).</summary>
    Info,

    /// <summary>Gray — explicit neutral with stronger bg than Default.</summary>
    Neutral
}

/// <summary>
/// Visual variant for LipiPill — how the intent color is applied.
/// </summary>
public enum PillVariant
{
    /// <summary>Solid background fill — strongest visual weight.</summary>
    Filled,

    /// <summary>Border-only with transparent background — minimal visual weight,
    /// pairs well with surrounding text.</summary>
    Outlined,

    /// <summary>Light-tint background with darker text — balanced weight,
    /// good for badge-like uses in tables and lists.</summary>
    Subtle
}

/// <summary>
/// Size variant for LipiPill. Heights chosen to align with inline text and
/// input fields. Default <see cref="Medium"/>.
/// </summary>
public enum PillSize
{
    /// <summary>20px height, 10px font — dense table cells, in-row chips.</summary>
    Small,

    /// <summary>24px height, 11px font — default. Standard status/filter pills.</summary>
    Medium,

    /// <summary>28px height, 12px font — hero/header pills, primary status displays.</summary>
    Large
}
