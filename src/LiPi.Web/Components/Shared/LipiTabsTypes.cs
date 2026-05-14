// SPEC:  docs/02-LipiTabs-Spec.md — Phase 2.6.1
// PHASE: 2.6.1 — Layout components (Tabs + Alert + Card)
// AMEND: CHANGE-LOG.md A21 — TabState.Optional XML-doc accuracy fix
//        (removes the phantom "required-field tracking" claim; cross-references
//        LipiTab.Optional so the two "optional" mechanisms are not conflated)
//
// All four enums for LipiTabs + LipiTab.
// Pattern: short names without Lipi prefix, one file per component family.
// Mirrors LipiButtonTypes.cs (Phase 2.1), LipiSelectionTypes.cs (Phase 2.5).

namespace LiPi.Web.Components.Shared;

/// <summary>Visual variant of the LipiTabs component.
/// <list type="bullet">
///   <item><term>Underline</term><description>Tab strip with 2px active underline. Default. Patient records, admin switchers.</description></item>
///   <item><term>Pill</term><description>Pill/segmented container. Calendar view toggles, mode switchers. No TabState shown.</description></item>
///   <item><term>Vertical</term><description>Left-rail tab strip. Settings pages, PatientNew form sections. Supports IconOnly mode.</description></item>
/// </list>
/// Box tabs were evaluated and excluded — legacy aesthetic, no clinical use case identified.
/// Phase 2.6.1 decision log §1.
/// </summary>
public enum TabsVariant
{
    /// <summary>Horizontal tab strip with full-radius active background + 2px underline.
    /// Option C radius (Underline design session). Full underline runs width of tab, not clipped by radius.</summary>
    Underline,

    /// <summary>Segmented pill container. No TabState (view-switcher context only — no validation semantics).
    /// Container: subtle bg. Active pill: surface bg + light border.</summary>
    Pill,

    /// <summary>Left-rail vertical tabs. Option B active treatment: full rounded background block, no right-border.
    /// Supports <see cref="LipiTabs.IconOnly"/> mode (52px icon strip, PatientNew pattern).</summary>
    Vertical
}

/// <summary>Validation/completion state of a LipiTab. Drives colour treatment and state indicator.
/// <para>Pill variant ignores TabState (view-switcher only).</para>
/// <list type="bullet">
///   <item><term>Default</term><description>Untouched or unknown — neutral styling.</description></item>
///   <item><term>Complete</term><description>All required fields filled and valid — green signal.</description></item>
///   <item><term>Partial</term><description>Some required fields filled but not all — amber signal.</description></item>
///   <item><term>Empty</term><description>No required fields filled — red signal. Used after first visit.</description></item>
///   <item><term>Optional</term><description>Dashed-border "optional content" colour cue. Distinct from
///     <c>LipiTab.Optional</c> (the Vertical-rail section divider) — see that member's docs.</description></item>
/// </list>
/// </summary>
public enum TabState
{
    /// <summary>Neutral — no state signal. Default for all tabs before user interaction.</summary>
    Default,

    /// <summary>Green — all required fields in this tab are complete and valid.</summary>
    Complete,

    /// <summary>Amber — some required fields filled, others still needed.</summary>
    Partial,

    /// <summary>Red — tab has been visited but required fields are unfilled or invalid.</summary>
    Empty,

    /// <summary>Marks the tab's content as fully optional — a visual cue only; it does
    /// not itself enforce or track anything. Renders a dashed border in the Underline
    /// variant. In the Vertical variant the tab is treated as stateless (no state dot).
    /// Ignored by the Pill variant. Used for the Review tab in PatientNew.
    /// <para>NOT the same as <see cref="LipiTab.Optional"/>, which inserts a section
    /// divider in the Vertical rail. <c>State=TabState.Optional</c> (dashed border) and
    /// <c>Optional="true"</c> (divider) are intentionally separate mechanisms — see the
    /// LipiTab header note ("TWO OPTIONAL MECHANISMS") and docs/02-LipiTabs-Spec.md.</para></summary>
    Optional
}

/// <summary>Panel rendering mode for LipiTabs.
/// <list type="bullet">
///   <item><term>Lazy</term><description>Only the active panel is mounted in DOM. Switching tabs unmounts
///     the previous panel — component state resets. Default. Use for admin switchers, patient record tabs.</description></item>
///   <item><term>Eager</term><description>All panels mounted, active shown via CSS, inactive hidden.
///     Component state (validation errors, _isTouched) survives tab switching.
///     Required for PatientNew — form state must persist across tab navigation.</description></item>
/// </list>
/// </summary>
public enum TabRenderMode
{
    /// <summary>Only active panel mounted. Inactive panels unmounted — state resets on tab switch.
    /// Memory-efficient. Use when panels are independent (admin settings, patient record tabs).</summary>
    Lazy,

    /// <summary>All panels mounted. Inactive panels have <c>display: none</c> via CSS.
    /// State survives tab switching. Required for multi-section forms (PatientNew).
    /// Slightly higher initial render cost — acceptable for form contexts.</summary>
    Eager
}

/// <summary>Keyboard shortcut pattern for switching tabs.
/// Applied from anywhere inside the LipiTabs container, including while focus is in a textbox.
/// <para>WAI-ARIA Pattern A (arrow keys on strip) is always active regardless of this setting.</para>
/// <para>Arrow keys NEVER switch tabs when focus is inside panel content — only when focused on the tab strip.</para>
/// <para>Ctrl+Tab is NOT supported (browser-reserved — switches browser tabs).</para>
/// <list type="bullet">
///   <item><term>None</term><description>No global shortcut. Recommended for all form-context tabs (PatientNew etc.).</description></item>
///   <item><term>CtrlNumber</term><description>Ctrl+1 through Ctrl+9 switch to tab by position. Fires from anywhere inside component.</description></item>
///   <item><term>AltNumber</term><description>Alt+1 through Alt+9. Safer in forms — Alt does not conflict with text editing. Recommended over CtrlNumber.</description></item>
/// </list>
/// Shortcut hint shown in IconOnly mode tooltip: "Identity (Alt+1)".
/// </summary>
public enum TabShortcutPattern
{
    /// <summary>No keyboard shortcut for tab switching. Default. Recommended for form-context tabs.</summary>
    None,

    /// <summary>Ctrl+1 through Ctrl+9 switch tabs. Fires from anywhere inside LipiTabs container.
    /// Note: Ctrl+1 in a textbox fires and switches tabs — inform users when enabled.</summary>
    CtrlNumber,

    /// <summary>Alt+1 through Alt+9 switch tabs. Safer than CtrlNumber in form contexts.
    /// Alt key does not conflict with text editing. Recommended shortcut pattern when shortcuts are needed.</summary>
    AltNumber
}
