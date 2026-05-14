// SPEC:  docs/03-LipiAlert-Spec.md — Phase 2.6.1
// PHASE: 2.6.1 — Layout components (Tabs + Alert + Card)

namespace LiPi.Web.Components.Shared;

/// <summary>Severity level of a LipiAlert. Drives color, icon, dismissibility,
/// and auto-dismiss eligibility.
/// <list type="bullet">
///   <item><term>Info</term><description>Informational, no urgency. Blue. Freely dismissible, auto-dismiss allowed.</description></item>
///   <item><term>Success</term><description>Positive outcome confirmation. Green. Freely dismissible, auto-dismiss allowed.</description></item>
///   <item><term>Warning</term><description>Attention needed, action optional. Amber. Dismissible, no auto-dismiss.</description></item>
///   <item><term>Danger</term><description>Action required, physician override possible. Red. Dismissible, no auto-dismiss.</description></item>
///   <item><term>Critical</term><description>Mandatory action, no passive dismissal. Dark red. No ✕ button ever.
///     No auto-dismiss. Requires explicit action buttons. Clinical safety decision.</description></item>
/// </list>
/// </summary>
public enum AlertSeverity
{
    Info,
    Success,
    Warning,
    Danger,
    Critical
}

/// <summary>Visual style of a LipiAlert.
/// <list type="bullet">
///   <item><term>Filled</term><description>Background wash + colored border. Maximum visibility. Default.</description></item>
///   <item><term>LeftBorder</term><description>Left color bar. Use <c>Contained</c> param:
///     true = full outline box (standalone), false = bottom border only (list/timeline rows).</description></item>
///   <item><term>Banner</term><description>Full-width strip, border-radius always 0. Page-level system messages.
///     Consumer places it — component does not force sticky positioning.</description></item>
///   <item><term>Outline</term><description>Transparent bg, full colored border. Quieter weight.
///     Critical + Outline silently upgrades to Filled with a dev warning.</description></item>
/// </list>
/// </summary>
public enum AlertStyle
{
    Filled,
    LeftBorder,
    Banner,
    Outline
}
