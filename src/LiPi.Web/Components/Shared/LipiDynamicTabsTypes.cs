// SPEC:  docs/03-LipiDynamicTabs-Spec.md §5
// PHASE: 2.6.2 — Overlay Surfaces

namespace LiPi.Web.Components.Shared;

public enum TabOverflowMode
{
    Scroll,     // horizontal scroll bar (default)
    Dropdown    // tabs beyond visible space collapse into "⋯" menu
}

public enum TabCloseResult
{
    Cancel,         // keep tab open
    Discard,        // close, lose changes
    SaveAndClose    // save first, then close
}
