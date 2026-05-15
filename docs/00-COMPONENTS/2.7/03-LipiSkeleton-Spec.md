# Phase 2.7 — LipiSkeleton Specification

**Status:** LOCKED
**Components:** LipiSkeletonLine, LipiSkeletonCircle, LipiSkeletonRect

---

## 1. Purpose

Placeholder shapes that appear while content is loading. Show the shape of
content-to-come instead of a generic spinner.

UX research shows skeleton placeholders reduce **perceived load time by ~30%**
compared to spinners. For clinical UX where every cognitive friction point
matters, skeletons earn their place.

---

## 2. Three primitives only

LiPi ships **primitive components**, not pre-built templates. Each consuming
page composes its own skeleton layout using the primitives. This keeps the
component library decoupled from page-specific layouts.

| Component | Purpose | Replaces |
|---|---|---|
| `LipiSkeletonLine` | Text/paragraph placeholder | Loading rows of text |
| `LipiSkeletonCircle` | Avatar/icon placeholder | Profile photos, badges |
| `LipiSkeletonRect` | Image/card/block placeholder | Images, cards, generic blocks |

No `LipiSkeletonAvatar`, no `LipiSkeletonImage`, no pre-built `LipiSkeletonCard`.
Consumers compose primitives.

---

## 3. Usage examples

### Single line of text

```razor
<LipiSkeletonLine Width="200px" />
```

### Multiple lines (paragraph)

```razor
<LipiSkeletonLine Width="100%" />
<LipiSkeletonLine Width="90%" />
<LipiSkeletonLine Width="75%" />
```

### Avatar + name composition

```razor
<div class="d-flex align-items-center gap-2">
    <LipiSkeletonCircle Size="48" />
    <div>
        <LipiSkeletonLine Width="120px" />
        <LipiSkeletonLine Width="80px" Height="0.8em" />
    </div>
</div>
```

### Card with image

```razor
<div class="card">
    <LipiSkeletonRect Width="100%" Height="160px" BorderRadius="0" />
    <div class="card-body">
        <LipiSkeletonLine Width="80%" Height="1.4em" />
        <LipiSkeletonLine Width="100%" />
        <LipiSkeletonLine Width="100%" />
        <LipiSkeletonLine Width="60%" />
    </div>
</div>
```

### Loading state pattern

```razor
@if (_loading)
{
    <LipiSkeletonLine Width="200px" />
    <LipiSkeletonLine Width="100px" />
}
else
{
    <h3>@_patient.Name</h3>
    <p>@_patient.Phone</p>
}
```

**No convenience wrapper.** Use `@if/else`. Simple, Blazor-idiomatic.

---

## 4. Parameters per primitive

### LipiSkeletonLine

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Width` | `string` | `"100%"` | px or % |
| `Height` | `string` | `"1em"` | typically matches line-height of replaced text |
| `Animate` | `bool` | `true` | Shimmer animation on/off |
| `Class` | `string?` | null | Append-only |

### LipiSkeletonCircle

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Size` | `int` | `40` | Diameter in px |
| `Animate` | `bool` | `true` | Shimmer animation on/off |
| `Class` | `string?` | null | Append-only |

### LipiSkeletonRect

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Width` | `string` | `"100%"` | px or % |
| `Height` | `string` | `"100px"` | px or % |
| `BorderRadius` | `string?` | null | Uses `--r-md` (8px) if not set |
| `Animate` | `bool` | `true` | Shimmer animation on/off |
| `Class` | `string?` | null | Append-only |

---

## 5. Visual design

### Base styles (shared in `lipi-skeleton.css`)

```css
.lipi-skeleton {
    display: block;
    background: var(--lipi-skeleton-bg);
    position: relative;
    overflow: hidden;
}

.lipi-skeleton-animate::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(
        90deg,
        transparent 0%,
        var(--lipi-skeleton-highlight) 50%,
        transparent 100%
    );
    transform: translateX(-100%);
    animation: lipi-skeleton-shimmer 1.5s linear infinite;
}

@keyframes lipi-skeleton-shimmer {
    100% { transform: translateX(100%); }
}

/* Specific shapes */
.lipi-skeleton-line {
    border-radius: var(--r-sm);
    margin: 4px 0;
}

.lipi-skeleton-circle {
    border-radius: 50%;
}

.lipi-skeleton-rect {
    border-radius: var(--r-md);
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
    .lipi-skeleton-animate::after {
        animation: none;
        opacity: 0.5;
    }
}
```

### Color tokens (added to baseline)

```css
/* mode-light.css */
--lipi-skeleton-bg:        var(--color-bg-muted);
--lipi-skeleton-highlight: rgba(255, 255, 255, 0.5);

/* mode-dark.css */
--lipi-skeleton-bg:        var(--color-bg-elevated);
--lipi-skeleton-highlight: rgba(255, 255, 255, 0.1);
```

---

## 6. Accessibility

- Wrapping element has `aria-hidden="true"` (skeleton is decorative)
- `role="status"` on the loading container (not skeleton itself) — consumer responsibility
- Skeleton itself is NOT announced to screen readers (consumer sets `aria-busy="true"` on the loading region)

```razor
<div role="status" aria-busy="true" aria-label="Loading patient...">
    <LipiSkeletonLine Width="200px" />
    <LipiSkeletonLine Width="100px" />
</div>
```

---

## 7. Animation behavior

- Shimmer: light highlight moves left → right across the gray block
- Duration: 1.5s per sweep
- Easing: linear (smooth, no acceleration)
- Reduced motion: animation disabled, static gray with 0.5 opacity

---

## 8. StyleGuide section

Add `#skeletons` section with:
1. All 3 primitives — single example each
2. Common compositions:
   - Avatar + name + subtitle
   - Card with image
   - Table row (4-5 horizontal Lines)
   - List of patient cards
3. Animate=false (static state)
4. Reduced-motion demo (note for testers)

---

*Three primitives, simple API. Consumer composes per-page. No templates.*
