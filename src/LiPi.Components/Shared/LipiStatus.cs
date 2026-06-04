// SPEC: docs/00-COMPONENTS/2.8/00-Phase2.8-Overview.md
//   §2.1 (Shared status tokens — Option C locked decision)
//   §2.2 (Status string-constants class design)
//   §2.3 (Status token CSS structure — data-status="..." attribute mapping)
// CROSS-REF: 01-LipiTable-Spec.md §22.2 (RowStatus parameter consumes these values),
//            §24.3.4 (lipi-status-tokens.css consumes via attribute selectors)
// PHASE: 2.8 Data Display — Stage 1A (types foundation)
// COMPONENT: Shared infrastructure
//
// Reference status taxonomy as string constants. Status values are intentionally strings
// (not C# enum at consumption points) so consuming apps can define their own taxonomies
// without modifying the LiPi component library. Per §2.2 of Phase 2.8 Overview.
//
// Consumers (existing and future):
//   • LipiTable / LipiList (Phase 2.8 — this phase)
//   • LipiCard / LipiAlert (Phase 2.6.1 — retrofit during Phase 2.10 audit per Q-1)
//   • LipiBadge / LipiPill (Phase 2.7 — already use the same tokens via composition)
//   • Any future component rendering status
//
// The shared CSS file lipi-status-tokens.css (Stage 1C) maps these string values to color
// tokens via data-status="..." attribute selectors. Unknown status strings fall back to
// neutral grey via the --color-status-unknown token.

namespace LiPi.Components.Shared;

/// <summary>
/// Reference status taxonomy. Status values are opaque strings at consumption points —
/// consuming apps may use these standard values or define their own. The shared CSS
/// (lipi-status-tokens.css) maps known values to color tokens; unknown values render
/// with the neutral fallback color (--color-status-unknown).
///
/// Usage: pass any status string to LipiTable's RowStatus selector or LipiList's
/// ItemStatus selector. These constants exist so callers can refer to the well-known
/// taxonomy without typing magic strings (LipiStatus.Active vs "active").
/// </summary>
public static class LipiStatus
{
    public const string Active     = "active";
    public const string Pending    = "pending";
    public const string Inactive   = "inactive";
    public const string Suspended  = "suspended";
    public const string Locked     = "locked";
    public const string Archived   = "archived";
    public const string Draft      = "draft";
    public const string Published  = "published";
    public const string InProgress = "in-progress";
    public const string Completed  = "completed";
    public const string Failed     = "failed";
    public const string Cancelled  = "cancelled";
    public const string Warning    = "warning";
    public const string Error      = "error";
    public const string Info       = "info";
    public const string Success    = "success";
    public const string Unknown    = "unknown";
}
