# Phase 2.7 — LipiBadge + LipiPill Specification

**Status:** LOCKED
**Components:** LipiBadge (attached count/dot) + LipiPill (standalone label/tag)

---

## 1. Why two components

| Concept | Industry name | LiPi component |
|---|---|---|
| Count attached to icon/element (top-right corner) | Badge | LipiBadge |
| Dot indicator attached (no number) | Badge | LipiBadge |
| Standalone status label in flow | Pill / Chip | LipiPill |
| Dismissible tag in filter/multiselect | Chip | LipiPill |

Conceptually different:
- **Badge** is positioned ON something (absolute positioning at corner)
- **Pill** is standalone INLINE element

Material Design, MUI, Ant Design all split these. We follow the convention.

---

## 2. LipiBadge — attached count or dot

### Usage examples

```razor
<!-- Count attached to bell icon (notification) -->
<div class="position-relative">
    <LucideIcon Name="bell" />
    <LipiBadge Count="3" />
</div>

<!-- Dot indicator -->
<div class="position-relative">
    <LucideIcon Name="bell" />
    <LipiBadge Dot />
</div>

<!-- Inline (e.g., next to tab text) -->
<button>
    Open Patients <LipiBadge Count="5" Inline />
</button>

<!-- Different colors -->
<LipiBadge Count="3" Color="BadgeColor.Danger" />   <!-- red (default) -->
<LipiBadge Count="7" Color="BadgeColor.Warning" />  <!-- amber -->
<LipiBadge Count="2" Color="BadgeColor.Success" />  <!-- green -->
<LipiBadge Count="9" Color="BadgeColor.Info" />     <!-- blue -->
<LipiBadge Count="4" Color="BadgeColor.Primary" />  <!-- navy -->

<!-- Custom max for overflow -->
<LipiBadge Count="247" Max="9" />   <!-- shows "9+" -->
<LipiBadge Count="247" Max="999" /> <!-- shows "247" -->

<!-- Show zero (default hides at 0) -->
<LipiBadge Count="0" ShowZero />

<!-- Position override -->
<LipiBadge Count="3" Position="BadgePosition.TopLeft" />
```

### Parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Count` | `int?` | null | Numeric count to display |
| `Dot` | `bool` | false | Show as dot, no number (Count ignored) |
| `Max` | `int` | 99 | Show "N+" when count exceeds |
| `Color` | `BadgeColor` | `Danger` | Visual color theme |
| `Position` | `BadgePosition` | `TopRight` | Corner attachment when not Inline |
| `Inline` | `bool` | false | Inline next to text, not positioned |
| `ShowZero` | `bool` | false | Display when Count is 0 (default: hide) |
| `Class` | `string?` | null | Append-only |

### Enums (LipiBadgeTypes.cs)

```csharp
public enum BadgeColor
{
    Danger,    // red - default, notification convention
    Warning,   // amber - pending items
    Success,   // green - completed counts
    Info,      // blue - informational counts
    Neutral,   // gray - generic counts
    Primary    // LiPi navy - branded counts
}

public enum BadgePosition
{
    TopRight,    // default - notifications convention
    TopLeft,
    BottomRight,
    BottomLeft
}
```

### Visual specs

- Count badge: 16-18px circle (10px font), shifts based on digits (1 digit → circle, 2 digits → pill)
- Dot badge: 8px circle, no text
- White text on solid color background
- Negative margin to overlap parent edge slightly
- z-index `var(--lipi-badge-z, 10)`

```css
.lipi-badge {
    position: absolute;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 16px;
    height: 16px;
    padding: 0 4px;
    border-radius: 9px;
    font-size: 10px;
    font-weight: 600;
    line-height: 1;
    color: white;
    z-index: var(--lipi-badge-z, 10);
}

.lipi-badge-dot {
    width: 8px;
    height: 8px;
    padding: 0;
    min-width: 0;
}

.lipi-badge-top-right    { top: -4px; right: -4px; }
.lipi-badge-top-left     { top: -4px; left: -4px; }
.lipi-badge-bottom-right { bottom: -4px; right: -4px; }
.lipi-badge-bottom-left  { bottom: -4px; left: -4px; }

.lipi-badge-inline {
    position: static;
    margin-left: 6px;
}

.lipi-badge-danger   { background: var(--color-danger-fill); }
.lipi-badge-warning  { background: var(--color-warning-fill); }
.lipi-badge-success  { background: var(--color-success-fill); }
.lipi-badge-info     { background: var(--color-info-fill); }
.lipi-badge-neutral  { background: var(--color-bg-strong); color: var(--color-text-primary); }
.lipi-badge-primary  { background: var(--color-primary); }
```

### Accessibility

- `aria-label` describes badge meaning (e.g., "3 unread notifications")
- For decorative count chips with adjacent visible text, `aria-hidden="true"` is acceptable
- Screen readers should not announce duplicate info — consumer responsibility to coordinate

---

## 3. LipiPill — standalone label/tag

### Usage examples

```razor
<!-- Status pills -->
<LipiPill>Active</LipiPill>
<LipiPill Intent="PillIntent.Success">Active</LipiPill>
<LipiPill Intent="PillIntent.Danger">Critical</LipiPill>
<LipiPill Intent="PillIntent.Warning">Pending</LipiPill>

<!-- With icon -->
<LipiPill Intent="PillIntent.Danger" Icon="alert-triangle">DNR</LipiPill>

<!-- Different variants -->
<LipiPill Variant="PillVariant.Outlined" Intent="PillIntent.Info">v1.0</LipiPill>
<LipiPill Variant="PillVariant.Subtle" Intent="PillIntent.Success">Online</LipiPill>

<!-- Different sizes -->
<LipiPill Size="PillSize.Small">SM</LipiPill>
<LipiPill Size="PillSize.Medium">MD</LipiPill>  <!-- default -->
<LipiPill Size="PillSize.Large">LG</LipiPill>

<!-- Dismissible (filter chip) -->
<LipiPill Dismissible OnDismiss="@RemoveOncologyFilter">Oncology</LipiPill>
```

### Parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `ChildContent` | `RenderFragment` | required | Pill text/content |
| `Intent` | `PillIntent` | `Default` | Visual color theme |
| `Variant` | `PillVariant` | `Filled` | Filled / Outlined / Subtle |
| `Size` | `PillSize` | `Medium` | Small / Medium / Large |
| `Icon` | `string?` | null | Left icon (Lucide name) |
| `Dismissible` | `bool` | false | Show × button |
| `OnDismiss` | `EventCallback` | — | Click on × |
| `Class` | `string?` | null | Append-only |

### Enums (LipiPillTypes.cs)

```csharp
public enum PillIntent
{
    Default,
    Primary,
    Success,
    Warning,
    Danger,
    Info,
    Neutral
}

public enum PillVariant
{
    Filled,    // solid background
    Outlined,  // border only, transparent background
    Subtle     // light tint background
}

public enum PillSize
{
    Small,    // height 20px, font 10px
    Medium,   // height 24px, font 11px (default)
    Large     // height 28px, font 12px
}
```

### Visual specs

- Border-radius: 12px (full pill shape — half the height)
- Padding: 4px 10px (Medium); 2px 8px (Small); 6px 12px (Large)
- Display: inline-flex
- Gap between icon and text: 4px
- Dismiss button: 12px × icon, 4px margin-left

```css
.lipi-pill {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    height: 24px;
    padding: 0 10px;
    border-radius: 12px;
    font-size: 11px;
    font-weight: 500;
    line-height: 1;
    white-space: nowrap;
}

.lipi-pill-sm { height: 20px; padding: 0 8px; font-size: 10px; border-radius: 10px; }
.lipi-pill-lg { height: 28px; padding: 0 12px; font-size: 12px; border-radius: 14px; }

/* Filled variant */
.lipi-pill-filled.lipi-pill-success   { background: var(--color-success-fill); color: white; }
.lipi-pill-filled.lipi-pill-warning   { background: var(--color-warning-fill); color: white; }
.lipi-pill-filled.lipi-pill-danger    { background: var(--color-danger-fill); color: white; }
.lipi-pill-filled.lipi-pill-info      { background: var(--color-info-fill); color: white; }
.lipi-pill-filled.lipi-pill-neutral   { background: var(--color-bg-strong); color: var(--color-text-primary); }
.lipi-pill-filled.lipi-pill-primary   { background: var(--color-primary); color: white; }

/* Outlined variant */
.lipi-pill-outlined { background: transparent; border: 1px solid currentColor; }
.lipi-pill-outlined.lipi-pill-success { color: var(--color-success-text); }
/* ... per intent ... */

/* Subtle variant */
.lipi-pill-subtle.lipi-pill-success   { background: var(--color-success-subtle); color: var(--color-success-text-strong); }
/* ... per intent ... */

.lipi-pill-icon { font-size: inherit; line-height: 1; }

.lipi-pill-dismiss {
    margin-left: 4px;
    cursor: pointer;
    opacity: 0.7;
}
.lipi-pill-dismiss:hover { opacity: 1; }
```

### Accessibility

- `role="status"` if pill represents dynamic state (e.g., "Online")
- Dismiss button has `aria-label="Remove {pill text}"`
- Keyboard: Tab to focus dismissible pill, Enter/Space activates dismiss

---

## 4. StyleGuide sections

### `#badges` section
1. Count variants (1, 99, 100, with Max=9 → 9+)
2. Dot variants
3. All 6 colors
4. All 4 positions
5. Inline vs. positioned
6. ShowZero behavior
7. On various parent elements (icon button, tab, list item)

### `#pills` section
1. All 7 intents × 3 variants (matrix)
2. All 3 sizes
3. With icon
4. Dismissible
5. Real-world examples (status row, filter chips, role labels)

---

*Both components are passive (no service, no state). Pure render. Used widely across LiPi.*
