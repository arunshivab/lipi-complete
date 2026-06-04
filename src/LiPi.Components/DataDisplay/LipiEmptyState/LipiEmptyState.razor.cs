// SPEC: docs/00-COMPONENTS/2.8/04-LipiEmptyState-Spec.md
//   §3.1 (Parameters), §3.2 (Title required — env-gated), §3.3 (Icon resolution order),
//   §4.2 (Heading level by size + TitleHeadingLevel override), §0.2 (variant default icons)
// CROSS-REF: CHANGE-LOG A40 (Lipicons §5 name mapping), A14 (env-gated validation pattern)
// PHASE: 2.8 Data Display — Stage 2 (LipiEmptyState)
// COMPONENT: LipiEmptyState
//
// Code-behind for the zero-state primitive. Stateless, presentational — no service
// injection beyond IWebHostEnvironment-free env detection (uses the same pattern as
// LipiButton's A14 hard-throw: Development throws on empty Title, Production logs + degrades).
//
// Icon resolution (§3.3):
//   1. IconTemplate set        -> caller's custom fragment
//   2. Icon set (non-null)     -> <LipiIcon Name="@Icon" /> (empty string "" = explicit opt-out, no icon)
//   3. Variant has default     -> mapped Lipicon per §0.2 + A40 §5
//   4. else                    -> no icon
//
// Variant -> Lipicon name mapping (A40 §5 decisions; Lipicons-native names, NOT Lucide):
//   Default/Empty -> "empty-state"   FilteredEmpty -> "search"   Error -> "warning"
//   Success -> "check-circle"        Coming -> "clock"

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LiPicons.Blazor;

namespace LiPi.Components.DataDisplay;

public partial class LipiEmptyState : ComponentBase
{
    // ─── Parameters (§3.1) ───────────────────────────────────────────────

    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string? Body { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public RenderFragment? IconTemplate { get; set; }
    [Parameter] public EmptyStateSize Size { get; set; } = EmptyStateSize.Inline;
    [Parameter] public EmptyStateVariant Variant { get; set; } = EmptyStateVariant.Default;
    [Parameter] public RenderFragment? PrimaryCta { get; set; }
    [Parameter] public RenderFragment? SecondaryCta { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    /// <summary>Override the heading element level (1-6). Out-of-range falls back to size default. §4.2.</summary>
    [Parameter] public int? TitleHeadingLevel { get; set; }

    // ─── Optional DI (env detection only; never required) ────────────────
    // Injected as optional so the component still works in isolation tests / consumers
    // that don't register hosting. When absent, defaults to non-throwing (prod-safe).
    [Inject] public IHostEnvironment? HostEnvironment { get; set; }
    [Inject] public ILogger<LipiEmptyState>? Logger { get; set; }

    // ─── Resolved backing fields (computed in OnParametersSet) ───────────
    private string _resolvedRootClass = "";
    private string? _resolvedIconName;
    private bool _renderBuiltInIcon;
    private LipiconVariant _resolvedIconVariant = LipiconVariant.Regular;
    private int _resolvedIconSize = 32;
    private int _resolvedHeadingLevel = 3;
    private bool _hasTitle;

    private bool IsDevelopment =>
        HostEnvironment?.EnvironmentName is { } env &&
        string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);

    protected override void OnParametersSet()
    {
        // ── Title validation (§3.2; A14 env-gated pattern) ───────────────
        _hasTitle = !string.IsNullOrWhiteSpace(Title);
        if (!_hasTitle)
        {
            if (IsDevelopment)
            {
                throw new InvalidOperationException(
                    "LipiEmptyState requires a non-empty Title parameter. " +
                    "Pass Title=\"...\" to communicate what's missing or why the surface is empty.");
            }

            Logger?.LogWarning(
                "LipiEmptyState rendered without a Title; title element omitted (production fallback).");
        }

        // ── Root class composition (§2.4) ────────────────────────────────
        _resolvedRootClass = ComposeRootClass();

        // ── Icon resolution (§3.3) ───────────────────────────────────────
        ResolveIcon();

        // ── Icon size by size variant (§2.3) ─────────────────────────────
        _resolvedIconSize = Size switch
        {
            EmptyStateSize.Inline => 32,
            EmptyStateSize.Card => 48,
            EmptyStateSize.Page => 64,
            _ => 32
        };

        // ── Heading level (§4.2) ─────────────────────────────────────────
        _resolvedHeadingLevel = ResolveHeadingLevel();
    }

    private string ComposeRootClass()
    {
        var sizeMod = Size switch
        {
            EmptyStateSize.Inline => "lipi-empty--inline",
            EmptyStateSize.Card => "lipi-empty--card",
            EmptyStateSize.Page => "lipi-empty--page",
            _ => "lipi-empty--inline"
        };

        var variantMod = Variant switch
        {
            EmptyStateVariant.Default => "lipi-empty--default",
            EmptyStateVariant.Empty => "lipi-empty--empty",
            EmptyStateVariant.FilteredEmpty => "lipi-empty--filtered",
            EmptyStateVariant.Error => "lipi-empty--error",
            EmptyStateVariant.Success => "lipi-empty--success",
            EmptyStateVariant.Coming => "lipi-empty--coming",
            _ => "lipi-empty--default"
        };

        var classes = $"lipi-empty {sizeMod} {variantMod}";
        if (!string.IsNullOrWhiteSpace(Class))
            classes += $" {Class}";

        return classes;
    }

    private void ResolveIcon()
    {
        // 1. Custom IconTemplate wins (handled in markup; nothing to resolve here).
        if (IconTemplate is not null)
        {
            _renderBuiltInIcon = false;
            _resolvedIconName = null;
            return;
        }

        // 2. Explicit Icon parameter.
        if (Icon is not null)
        {
            // Empty string = explicit opt-out (no icon).
            if (Icon.Length == 0)
            {
                _renderBuiltInIcon = false;
                _resolvedIconName = null;
                return;
            }

            _renderBuiltInIcon = true;
            _resolvedIconName = Icon;
            return;
        }

        // 3. Variant default (A40 §5 mapping — Lipicons-native names).
        _resolvedIconName = Variant switch
        {
            EmptyStateVariant.Default => LipiconName.EmptyState,
            EmptyStateVariant.Empty => LipiconName.EmptyState,
            EmptyStateVariant.FilteredEmpty => LipiconName.Search,
            EmptyStateVariant.Error => LipiconName.Warning,
            EmptyStateVariant.Success => LipiconName.CheckCircle,
            EmptyStateVariant.Coming => LipiconName.Clock,
            _ => null
        };

        _renderBuiltInIcon = _resolvedIconName is not null;
    }

    private int ResolveHeadingLevel()
    {
        // Explicit override (1-6) wins; out-of-range falls back to size default.
        if (TitleHeadingLevel is { } lvl && lvl is >= 1 and <= 6)
            return lvl;

        // Size default: Page = h2, Inline/Card = h3 (§4.2).
        return Size == EmptyStateSize.Page ? 2 : 3;
    }
}
