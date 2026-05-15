# Phase 2.7 — LipiSpinner Specification

**Status:** LOCKED
**Component:** LipiSpinner

---

## 1. Purpose

General-purpose loading indicator. Distinct from `LipiButtonSpinner` (button-internal,
small only). LipiSpinner is for page-level loading, section loading, inline loading,
or any "waiting" UX outside button context.

---

## 2. Usage examples

```razor
<!-- Smallest, inline with text -->
Loading <LipiSpinner Size="SpinnerSize.XSmall" />

<!-- Default centered loader -->
<LipiSpinner />

<!-- Page-level large -->
<div class="page-center">
    <LipiSpinner Size="SpinnerSize.Large" Label="Loading patients..." />
</div>

<!-- Custom intent -->
<LipiSpinner Intent="SpinnerIntent.Subtle" />

<!-- Custom color override -->
<LipiSpinner Color="#0B2545" />

<!-- Inverse (for dark backgrounds) -->
<div class="bg-dark">
    <LipiSpinner Intent="SpinnerIntent.Inverse" />
</div>

<!-- With label below -->
<LipiSpinner Size="SpinnerSize.Large" 
             Label="Loading..." 
             LabelPosition="LabelPosition.Bottom" />
```

---

## 3. Parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Size` | `SpinnerSize` | `Medium` | XSmall(12) / Small(16) / Medium(24) / Large(48) |
| `SizePx` | `int?` | null | Override size in px, wins over `Size` |
| `Intent` | `SpinnerIntent` | `Default` | Default (currentColor) / Primary / Subtle / Inverse |
| `Color` | `string?` | null | Custom CSS color, wins over `Intent` |
| `Label` | `string?` | null | Optional text label |
| `LabelPosition` | `LabelPosition` | `Right` | Reuses Phase 2.5.5 enum |
| `Class` | `string?` | null | Append-only layout utility classes |

---

## 4. Enums (LipiSpinnerTypes.cs)

```csharp
public enum SpinnerSize
{
    XSmall,   // 12px - inline with text, badges
    Small,    // 16px - table rows, small components
    Medium,   // 24px - default, sections, cards
    Large     // 48px - page-center, full-card
}

public enum SpinnerIntent
{
    Default,  // currentColor — inherits text color of parent
    Primary,  // --color-primary (LiPi navy)
    Subtle,   // --color-text-muted
    Inverse   // white — for dark backgrounds
}
```

`LabelPosition` reuses the existing enum from Phase 2.5.5 (`LiPi.Web.Components.Shared.LabelPosition`).

---

## 5. Visual design

### Structure
- SVG `<circle>` for full track (faint, currentColor at low opacity)
- SVG `<circle>` for arc (stroke-dasharray creates ~25% visible arc)
- Container `<svg>` rotates via CSS keyframe animation

### CSS

```css
.lipi-spinner {
    display: inline-block;
    width: 24px; /* size variant overrides */
    height: 24px;
    animation: lipi-spinner-rotate var(--lipi-spinner-duration, 1.0s) linear infinite;
    color: currentColor;
}

.lipi-spinner-xs { width: 12px; height: 12px; }
.lipi-spinner-sm { width: 16px; height: 16px; }
.lipi-spinner-md { width: 24px; height: 24px; }
.lipi-spinner-lg { width: 48px; height: 48px; }

.lipi-spinner-track {
    stroke: var(--lipi-spinner-track);
    fill: none;
    stroke-width: 2;
}

.lipi-spinner-arc {
    stroke: currentColor;
    fill: none;
    stroke-width: 2;
    stroke-linecap: round;
    stroke-dasharray: 60 90;  /* visible arc length */
}

@keyframes lipi-spinner-rotate {
    from { transform: rotate(0deg); }
    to   { transform: rotate(360deg); }
}

/* With label */
.lipi-spinner-wrap {
    display: inline-flex;
    align-items: center;
    gap: 8px;
}

.lipi-spinner-wrap-bottom {
    flex-direction: column;
}

.lipi-spinner-label {
    font-size: 12px;
    color: var(--color-text-secondary);
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
    .lipi-spinner {
        animation-duration: 2.5s;  /* slower, less stimulating */
    }
}

/* Intent colors */
.lipi-spinner-default  { color: currentColor; }
.lipi-spinner-primary  { color: var(--color-primary); }
.lipi-spinner-subtle   { color: var(--color-text-muted); }
.lipi-spinner-inverse  { color: white; }
```

### SVG markup

```razor
<svg class="lipi-spinner @SizeClass @IntentClass"
     viewBox="0 0 24 24"
     style="@(Color != null ? $"color:{Color}" : null)"
     role="status"
     aria-label="@(Label ?? "Loading")">
    <circle class="lipi-spinner-track" cx="12" cy="12" r="10" />
    <circle class="lipi-spinner-arc" cx="12" cy="12" r="10" />
</svg>
```

---

## 6. Accessibility

- `role="status"` (announces but doesn't interrupt screen reader flow)
- `aria-label` defaults to `"Loading"` or the `Label` value if present
- `aria-live="polite"` on the wrapper when shown
- Respects `prefers-reduced-motion` (slower rotation, no stimulation)

---

## 7. Animation timing

- Default: 1.0 second per rotation
- Customizable via `--lipi-spinner-duration` CSS variable (set on parent or root)
- Linear easing (steady rotation, no acceleration)

---

## 8. StyleGuide section

Add `#spinners` section with:
1. All 4 sizes side-by-side
2. All 4 intents (Default/Primary/Subtle/Inverse) — Inverse shown on dark background
3. With custom Color
4. With Label (right + bottom positions)
5. Inline with text vs. standalone
6. Reduced-motion comparison (note for testers)

---

*Foundational component — built first in Phase 2.7. LipiToast Promise-style API uses LipiSpinner internally for loading state.*
