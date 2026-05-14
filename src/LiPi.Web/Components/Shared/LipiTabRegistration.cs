// PHASE: 2.6.1 — Layout components
// AMEND: Phase 2.6.3 Stage 2 — registration loop fix
// Part of the LipiTabs compound component family.
//
// Extracted to a standalone .cs file because Razor-defined types (@code block
// types) are not visible across .razor files. Both LipiTabs.razor (parent)
// and LipiTab.razor (child) need this type, so it must live in a plain .cs file.

namespace LiPi.Web.Components.Shared;

/// <summary>Internal registration record populated by a <see cref="LipiTab"/>
/// child and consumed by the parent <see cref="LipiTabs"/> for rendering.
/// Not part of the public API — do not reference outside the LipiTabs family.
///
/// <para>Phase 2.6.3 Stage 2 changes:</para>
/// <list type="bullet">
///   <item>Static auto-key counter REMOVED. Auto-key generation moved to
///         <see cref="LipiTab"/> where it lives on the instance and is generated
///         exactly once in <c>OnInitialized</c>. Previous static counter bled
///         across LipiTabs instances and across page lifecycles, and produced
///         a new key on every BuildRegistration call for unkeyed tabs.</item>
///   <item><c>ResolvedKey</c> is now a required constructor argument — the
///         caller (LipiTab) supplies the stable key. The record no longer
///         decides keys.</item>
///   <item>Implemented <see cref="HasSameParameterValues"/> for detecting whether
///         a re-registration carries genuinely different parameter values, so
///         <see cref="LipiTabs.UpdateTab"/> can skip rerenders when nothing changed.</item>
/// </list>
/// </summary>
internal sealed class LipiTabRegistration
{
    public LipiTabRegistration(
        string  resolvedKey,
        string? key,
        string  label,
        string? icon,
        TabState state,
        int?    count,
        bool    optional,
        bool    disabled,
        Microsoft.AspNetCore.Components.RenderFragment? childContent)
    {
        ResolvedKey  = resolvedKey;
        Key          = key;
        Label        = label;
        Icon         = icon;
        State        = state;
        Count        = count;
        Optional     = optional;
        Disabled     = disabled;
        ChildContent = childContent;
    }

    /// <summary>Resolved key — uses caller-provided Key, or LipiTab's
    /// instance-generated auto-key. Stable for the lifetime of the LipiTab
    /// instance that produced this registration.</summary>
    public string   ResolvedKey  { get; }
    public string?  Key          { get; }
    public string   Label        { get; }
    public string?  Icon         { get; }
    public TabState State        { get; }
    public int?     Count        { get; }
    public bool     Optional     { get; }
    public bool     Disabled     { get; }
    public Microsoft.AspNetCore.Components.RenderFragment? ChildContent { get; }

    /// <summary>True when this registration carries the same meaningful parameter
    /// values as <paramref name="other"/>. Used by <see cref="LipiTabs.UpdateTab"/>
    /// to skip rerenders when a re-registration is a no-op.
    ///
    /// <para><c>ChildContent</c> is excluded from comparison. Razor generates a
    /// new RenderFragment delegate on every render even when the content is
    /// structurally identical, so reference comparison would always report
    /// "changed". The latest ChildContent delegate is always swapped in by the
    /// caller — what we care about for rerender suppression is whether the
    /// tablist itself needs to redraw, which depends on Label/Icon/State/Count
    /// /Optional/Disabled but NOT on the panel content delegate.</para>
    /// </summary>
    public bool HasSameParameterValues(LipiTabRegistration other)
        => ResolvedKey == other.ResolvedKey
        && Key         == other.Key
        && Label       == other.Label
        && Icon        == other.Icon
        && State       == other.State
        && Count       == other.Count
        && Optional    == other.Optional
        && Disabled    == other.Disabled;
}
