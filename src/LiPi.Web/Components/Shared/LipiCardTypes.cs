// SPEC:  docs/04-LipiCard-Spec.md — Phase 2.6.1
// PHASE: 2.6.1 — Layout components (Tabs + Alert + Card)

namespace LiPi.Web.Components.Shared;

/// <summary>Visual variant of the LipiCard container.
/// <list type="bullet">
///   <item><term>Outlined</term><description>White bg + visible border. Default. Form sections, content panels, patient record tabs.</description></item>
///   <item><term>Flat</term><description>Subtle bg tint, no border. Metric/KPI cards, nested content, dashboard sections.</description></item>
///   <item><term>Elevated</term><description>Shadow, no border. Featured content, active selection emphasis.</description></item>
///   <item><term>Clickable</term><description>Outlined + hover/active/selected states. Search results, appointment slot grids, specialty selection.</description></item>
///   <item><term>Accent</term><description>Left color strip (same language as LipiAlert LeftBorder). Priority/severity indicator on card-level items.</description></item>
/// </list>
/// </summary>
public enum CardVariant
{
    Outlined,
    Flat,
    Elevated,
    Clickable,
    Accent
}

/// <summary>Accent color for the left-border strip when <see cref="CardVariant.Accent"/> is used.
/// Color palette matches <see cref="AlertSeverity"/> for consistent severity language across the system.
/// </summary>
public enum CardAccentColor
{
    /// <summary>Navy — informational, neutral priority.</summary>
    Primary,

    /// <summary>Green — positive status, normal/healthy.</summary>
    Success,

    /// <summary>Amber — attention needed, review recommended.</summary>
    Warning,

    /// <summary>Red — action required, physician override possible.</summary>
    Danger,

    /// <summary>Dark red — mandatory action, highest clinical priority.</summary>
    Critical
}

/// <summary>Flex justification of the <see cref="CardFooter"/> action row.
/// <list type="bullet">
///   <item><term>Start</term><description>Actions left-aligned.</description></item>
///   <item><term>End</term><description>Actions right-aligned. Default — primary action on the right.</description></item>
///   <item><term>SpaceBetween</term><description>Cancel on left, confirm on right — standard dialog footer pattern.</description></item>
/// </list>
/// </summary>
public enum CardFooterAlign
{
    Start,
    End,
    SpaceBetween
}
