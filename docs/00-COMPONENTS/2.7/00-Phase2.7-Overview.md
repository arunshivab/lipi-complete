# Phase 2.7 — Feedback Components

**Status:** Design LOCKED — ready for build
**Phase:** 2.7
**Components:** LipiSpinner, LipiBadge, LipiPill, LipiSkeleton, LipiValidationSummary, LipiToast (6 total)
**Prerequisite:** Phase 2.6.2 complete (LipiModal/Drawer/DynamicTabs)

---

## Scope

Six small-to-medium components covering feedback patterns: loading indicators,
status badges, placeholder skeletons, form validation summary, and transient toasts.

| Component | Type | Complexity |
|---|---|---|
| LipiSpinner | Passive (render only) | Low |
| LipiBadge | Passive (positioned indicator) | Low |
| LipiPill | Passive (standalone label/tag) | Low |
| LipiSkeleton (Line/Circle/Rect) | Passive (animated placeholders) | Low |
| LipiValidationSummary | Active (EditContext integration) | Medium |
| LipiToast | Active (service + queue + host) | Medium-High |

---

## Critical: build order is MANDATORY

1. **LipiSpinner** — foundational (Toast uses it, Skeleton borrows animation timing)
2. **LipiBadge** + **LipiPill** — used by ValidationSummary and elsewhere
3. **LipiSkeleton** family (3 primitives)
4. **LipiValidationSummary** — builds on LipiAlert (from Phase 2.6.1)
5. **LipiToast** + **LipiToastService** + **LipiToastHost** — uses everything

---

## No external libraries

All Phase 2.7 components are pure Blazor + pure CSS. No animation libraries,
no toast libraries, no notification packages. Same standing rule as all
previous phases — LiPi is library-agnostic except for Lucide icons (which
are inline SVG paths in our own `LucideIcon.razor`, swappable to Lipicons later).

---

## File structure

```
src/LiPi.Web/Components/Shared/
├── LipiSpinner.razor
├── LipiSpinner.razor.cs
├── LipiSpinner.razor.css
├── LipiSpinnerTypes.cs              ← SpinnerIntent enum + InputSize reuse

├── LipiBadge.razor
├── LipiBadge.razor.cs
├── LipiBadge.razor.css
├── LipiBadgeTypes.cs                ← BadgeColor, BadgePosition

├── LipiPill.razor
├── LipiPill.razor.cs
├── LipiPill.razor.css
├── LipiPillTypes.cs                 ← PillIntent, PillVariant, PillSize

├── LipiSkeletonLine.razor
├── LipiSkeletonCircle.razor
├── LipiSkeletonRect.razor
├── lipi-skeleton.css                ← shared shimmer animation

├── LipiValidationSummary.razor
├── LipiValidationSummary.razor.cs
├── LipiValidationSummary.razor.css
├── LipiValidationSummaryTypes.cs    ← ValidationSummaryMode, Placement (flags)

├── LipiToast.razor                  ← internal: renders a single toast
├── LipiToast.razor.cs
├── LipiToast.razor.css
├── LipiToastHost.razor              ← the host (lives in TopNavLayout)
├── LipiToastHost.razor.cs
├── LipiToastService.cs              ← programmatic API
├── ILipiToastService.cs             ← interface
├── LipiToastTypes.cs                ← ToastSeverity, ToastOptions, ToastDescriptor, ToastAction, ToastPromiseOptions, Position

src/LiPi.Web/wwwroot/css/
├── lipi-feedback.css                ← shared color tokens, skeleton shimmer keyframes
```

---

## DI registration (in Program.cs)

```csharp
builder.Services.AddScoped<ILipiToastService, LipiToastService>();
```

Only LipiToast needs a service. All other Phase 2.7 components are pure
render components, no services.

---

## TopNavLayout additions

After Phase 2.6.2 added `<LipiOverlayHost />`, Phase 2.7 adds:

```razor
@* In TopNavLayout.razor, after <LipiOverlayHost /> *@
<LipiOverlayHost />
<LipiToastHost />
```

Both are end-of-layout, positioned absolutely, manage their own z-index.

---

## New CSS tokens added to baseline

Both `mode-light.css` and `mode-dark.css` gain:

```css
/* Skeleton */
--lipi-skeleton-bg:        var(--color-bg-muted);
--lipi-skeleton-highlight: rgba(255, 255, 255, 0.5);  /* light mode */
/* In dark mode override: */
--lipi-skeleton-highlight: rgba(255, 255, 255, 0.1);

/* Spinner */
--lipi-spinner-duration:   1.0s;
--lipi-spinner-track:      rgba(0, 0, 0, 0.1);  /* light mode track */
/* In dark mode override: */
--lipi-spinner-track:      rgba(255, 255, 255, 0.1);

/* Toast */
--lipi-toast-shadow:       0 8px 24px rgba(11, 37, 69, 0.16);
--lipi-toast-bg:           var(--color-bg-surface);
--lipi-toast-border:       var(--color-border-default);

/* Badge */
--lipi-badge-z:            10;  /* z-index when overlapping parent */
```

---

## Cross-component reuse

- **LipiValidationSummary uses LipiAlert internally** (severity=Danger). No new visual styling needed.
- **LipiToast uses LucideIcon** for severity icons (check, alert-triangle, x, info).
- **LipiToast uses LipiSpinner** in Promise-style API (loading state).
- **LipiPill is used by LipiBadge consumers in some patterns** (e.g., role badges in user lists — but not in 2.7 directly).

---

## Standing rules for Phase 2.7

1. **No external libraries** — all CSS, all SVG, all components are ours
2. **Honor `prefers-reduced-motion`** — all animations (spinner, skeleton shimmer, toast slide) have reduced-motion fallbacks
3. **Accessibility first** — every component has proper ARIA, role, aria-live where applicable
4. **Theme tokens only** — no hardcoded colors, use semantic tokens
5. **Honor pre-delivery checklist** (12 items including ghost click prevention, applies to LipiToast dismiss interactions)

---

*See spec files:*
- `01-LipiSpinner-Spec.md`
- `02-LipiBadge-Pill-Spec.md`
- `03-LipiSkeleton-Spec.md`
- `04-LipiValidationSummary-Spec.md`
- `05-LipiToast-Spec.md`
- `BUILD-CHAT-HANDOFF.md`
