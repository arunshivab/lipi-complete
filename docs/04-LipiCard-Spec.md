# Phase 2.6.1 — LipiCard Design Specification

**Status:** LOCKED — ready for build chat
**Component:** LipiCard
**Phase:** 2.6.1 (Card + Alert + Tabs — Card third)
**Next:** LipiModal (2.6.2), LipiDynamicTabs (2.6.2)

---

## 1. Scope boundary

LipiCard is a **container component** (Type 1). It provides a visual boundary
that groups related content with consistent padding, border, radius, and optional
header/footer structure.

Domain-specific data cards (patient card, appointment card, lab result card) are
Phase 5/6 components that will USE LipiCard internally. They are NOT in scope here.

PatientSearch's `ps-pt-card` will be rebuilt using LipiCard when the patient
module is redesigned — no media slot is needed for that card (it uses an
initials-based CSS avatar, not an image file).

---

## 2. Component structure — named slots with free content fallback

Option C (from design session): named slots when provided, free content fallback
when no slots used. One component handles both patterns.

```razor
@* Pattern A — Slotted (recommended for structured content) *@
<LipiCard>
    <CardHeader Title="Patient demographics"
                Subtitle="Last updated 10 May 2026">
        <CardHeaderAction>
            <LipiButton Size="ButtonSize.Small">Edit</LipiButton>
        </CardHeaderAction>
    </CardHeader>
    <CardBody>
        @* arbitrary content — form fields, tables, anything *@
    </CardBody>
    <CardFooter>
        <LipiButton Variant="ButtonVariant.Secondary">Cancel</LipiButton>
        <LipiButton>Save changes</LipiButton>
    </CardFooter>
</LipiCard>

@* Pattern B — Free content (for panels where tab label IS the header) *@
<LipiCard>
    <p>Anything here — no header, no footer slots.</p>
</LipiCard>

@* Pattern C — Clickable card (search result, appointment slot) *@
<LipiCard Variant="CardVariant.Clickable"
          OnClick="@SelectAppointment"
          Selected="@(_selectedSlot == slot.Id)">
    <CardBody>...</CardBody>
</LipiCard>

@* Pattern D — Navigation card (links to another page) *@
<LipiCard Variant="CardVariant.Clickable"
          Href="/patients/PT-2026-004821">
    <CardBody>...</CardBody>
</LipiCard>

@* Pattern E — Accent card (severity/priority indicator) *@
<LipiCard Variant="CardVariant.Accent"
          AccentColor="CardAccentColor.Danger">
    <CardHeader Title="Critical lab result" />
    <CardBody>K⁺ 6.8 mEq/L — action required</CardBody>
</LipiCard>
```

---

## 3. LipiCard parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Variant` | `CardVariant` | `Outlined` | See §4 |
| `ChildContent` | `RenderFragment?` | null | Free content — used when no named slots provided |
| `OnClick` | `EventCallback` | — | Enables hover/active states automatically |
| `Href` | `string?` | null | Renders card as `<a>` element for navigation |
| `Clickable` | `bool` | `false` | Explicit override — hover/active even without OnClick/Href |
| `Selected` | `bool` | `false` | Clickable variant only — adds selection border + tint |
| `OnSelectedChanged` | `EventCallback<bool>` | — | Two-way binding for selected state |
| `AccentColor` | `CardAccentColor?` | null | Accent variant only |
| `Disabled` | `bool` | `false` | Clickable/Accent variants — mutes hover, blocks OnClick |
| `Class` | `string?` | null | Append-only layout utility (for grid placement etc.) |

### Sub-components

**CardHeader**

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string?` | null | Bold heading. Optional — header renders even without title (for CardHeaderAction only) |
| `Subtitle` | `string?` | null | Secondary line below title — muted text, 11px |
| `ChildContent` | `RenderFragment?` | null | Replaces Title+Subtitle with arbitrary content |
| `CardHeaderAction` | `RenderFragment?` | null | Right-aligned slot — buttons, badges, menus |

CardHeader renders a subtle `0.5px` bottom border separating it from CardBody.
When `Title` and `ChildContent` are both null, CardHeader renders only the
`CardHeaderAction` slot (useful for a card with only a top-right action button).

**CardBody**

No parameters beyond `ChildContent`. Pure content wrapper with consistent
padding that adjusts per card variant.

**CardFooter**

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `ChildContent` | `RenderFragment?` | null | Action buttons, metadata, secondary info |
| `Align` | `CardFooterAlign` | `End` | Start / End / SpaceBetween — controls flex justification |

CardFooter renders a subtle `0.5px` top border separating it from CardBody.

---

## 4. Enums (new file: LipiCardTypes.cs)

```csharp
public enum CardVariant
{
    Outlined,   // default — white bg, visible border. Form sections, content panels.
    Flat,       // no border, subtle bg tint. Metric cards, nested content, dashboards.
    Elevated,   // shadow, no border. Featured content, active/focus state.
    Clickable,  // outlined + hover/active/selected states. Search results, selection grids.
    Accent      // left color strip (same language as LipiAlert LeftBorder). Priority/severity.
}

public enum CardAccentColor
{
    Primary,    // navy — informational
    Success,    // green — positive
    Warning,    // amber — attention
    Danger,     // red — action required
    Critical    // dark red — mandatory action
}

public enum CardFooterAlign
{
    Start,
    End,          // default — right-aligned actions
    SpaceBetween  // cancel left, confirm right
}
```

---

## 5. Interactive states (Clickable variant)

Triggered by: `OnClick` wired, OR `Href` set, OR `Clickable="true"`.

| State | Visual treatment |
|---|---|
| Default | `Outlined` base — white bg, 0.5px border |
| Hover | `background: var(--color-bg-hover)`, `border-color: var(--color-border-strong)`, `cursor: pointer` |
| Active (press) | `scale(0.99)`, `background: var(--color-primary-pale)` |
| Selected | `border: 2px solid var(--color-primary)`, `background: var(--color-primary-pale)` |
| Disabled | `opacity: 0.5`, `cursor: not-allowed`, hover/active blocked |
| Focus (keyboard) | `outline: 2px solid var(--color-border-focus)`, `outline-offset: 2px` |

When `Href` is set, the card renders as `<a href="@Href">` — correct semantics
for keyboard navigation, right-click "Open in new tab", screen readers.
When `OnClick` is set, card renders as `<div role="button" tabindex="0">`.
When both set, `Href` wins (link semantics preferred).

---

## 6. Visual design per variant

### Outlined (default)
```css
.lipi-card-outlined {
    background: var(--color-bg-surface);
    border: 0.5px solid var(--color-border-default);
    border-radius: var(--lipi-card-radius, var(--r-md));
    box-shadow: none;
}
```

### Flat
```css
.lipi-card-flat {
    background: var(--color-bg-subtle);
    border: none;
    border-radius: var(--lipi-card-radius, var(--r-md));
    box-shadow: none;
}
```

### Elevated
```css
.lipi-card-elevated {
    background: var(--color-bg-surface);
    border: none;
    border-radius: var(--lipi-card-radius, var(--r-md));
    box-shadow: var(--sh-md);
}
```

### Clickable
```css
/* Extends Outlined */
.lipi-card-clickable {
    background: var(--color-bg-surface);
    border: 0.5px solid var(--color-border-default);
    border-radius: var(--lipi-card-radius, var(--r-md));
    cursor: pointer;
    transition: background var(--tr-fast),
                border-color var(--tr-fast),
                transform var(--tr-fast),
                box-shadow var(--tr-fast);
}

.lipi-card-clickable:hover:not(.lipi-card-disabled) {
    background: var(--color-bg-hover);
    border-color: var(--color-border-strong);
    box-shadow: var(--sh-xs);
}

.lipi-card-clickable:active:not(.lipi-card-disabled) {
    transform: scale(0.99);
    background: var(--color-primary-pale);
}

.lipi-card-clickable.lipi-card-selected {
    border: 2px solid var(--color-primary);
    background: var(--color-primary-pale);
}

.lipi-card-clickable.lipi-card-disabled {
    opacity: 0.5;
    cursor: not-allowed;
    pointer-events: none;
}
```

### Accent
```css
.lipi-card-accent {
    background: var(--color-bg-surface);
    border: 0.5px solid var(--color-border-default);
    border-left: var(--lipi-card-accent-w, 4px) solid var(--lipi-card-accent-color);
    border-radius: var(--lipi-card-radius, var(--r-md));
}
/* AccentColor drives --lipi-card-accent-color via inline style */
```

---

## 7. Padding system

Consistent internal padding per section:

```css
.lipi-card-header {
    padding: 12px 16px;
    border-bottom: 0.5px solid var(--color-border-tertiary);
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
}

.lipi-card-body {
    padding: 16px;
}

.lipi-card-footer {
    padding: 10px 16px;
    border-top: 0.5px solid var(--color-border-tertiary);
    display: flex;
    align-items: center;
    gap: 8px;
}

/* When only CardBody is present (no header/footer),
   it gets all-around padding */
.lipi-card-body:only-child {
    padding: 16px;
}

/* When CardHeader immediately precedes CardBody,
   reduce body top-padding (header already has bottom padding) */
.lipi-card-header + .lipi-card-body {
    padding-top: 12px;
}
```

---

## 8. Multi-industry token overrides

Following the principle from §17 of LipiTabs spec:
any visual property that varies by brand gets a CSS custom property.

```css
--lipi-card-radius:       var(--r-md);    /* card corner radius — 10px LiPi default */
--lipi-card-shadow:       var(--sh-md);   /* Elevated variant shadow */
--lipi-card-accent-w:     4px;            /* Accent variant left-border width */
--lipi-card-border-w:     0.5px;          /* standard border width */
--lipi-card-padding-body: 16px;           /* CardBody padding */
--lipi-card-padding-head: 12px 16px;      /* CardHeader padding */
```

Override examples for other industries:
```css
/* Softer consumer health app */
:root {
    --lipi-card-radius: var(--r-xl);      /* 20px — very rounded */
    --lipi-card-border-w: 0;              /* no border, shadow only */
    --lipi-card-shadow: 0 4px 16px rgba(0,0,0,.08);
}

/* Dense enterprise dashboard */
:root {
    --lipi-card-radius: var(--r-sm);      /* 6px — tight corners */
    --lipi-card-padding-body: 12px;       /* compact body */
    --lipi-card-padding-head: 8px 12px;   /* compact header */
}
```

---

## 9. Accessibility

- `Clickable` + `OnClick` → `role="button"`, `tabindex="0"`, `@onkeydown` handles Enter/Space
- `Clickable` + `Href` → renders as `<a>` — native keyboard handling, correct semantics
- `Selected` state → `aria-pressed="@Selected"` on button role, `aria-current="true"` on link role
- `Disabled` → `aria-disabled="true"`, pointer-events blocked
- CardHeader Title → renders as `<h3>` (or configurable `HeadingLevel` — see §10)
- CardHeader Subtitle → `aria-describedby` target
- Focus ring: `outline: 2px solid var(--color-border-focus); outline-offset: 2px`

---

## 10. HeadingLevel parameter (accessibility)

CardHeader Title renders as a semantic heading. The correct heading level depends
on page context — an `<h2>` on a page with `<h1>` page title, or `<h3>` inside
a section already titled with `<h2>`.

```razor
<LipiCard>
    <CardHeader Title="Patient demographics" HeadingLevel="3" />
    ...
</LipiCard>
```

| Parameter | Type | Default | Range |
|---|---|---|---|
| `HeadingLevel` | `int` | `3` | 2–6 |

Default `3` — assumes a page h1 (page title) and a section h2 (section group).
Consumer overrides when nesting level differs.

---

## 11. Clinical use case mapping

| Use case | Variant | Interactive | Notes |
|---|---|---|---|
| Patient demographics section | Outlined | No | Form section, tab panel content |
| Vital signs panel | Outlined | No | Clinical data display |
| Dashboard metric | Flat | No | Number + label, no border |
| Appointment slot selection | Clickable | OnClick + Selected | Grid of available slots |
| Search result (patient, drug) | Clickable | Href | Navigates to detail page |
| Critical lab result | Accent + Danger | No | Left red strip draws attention |
| Settings section | Outlined | No | LabelPosition=Left fields inside |
| PatientNew tab panel | Outlined | No | No header — tab label IS the header |
| Featured/active panel | Elevated | No | Shadow draws eye |

---

## 12. Deferred (not Phase 2.6.1)

- **Media slot** (`<CardMedia>`) — image zones for radiology thumbnails, patient photos. Phase 5/6.
- **Loading/skeleton state** — `Loading` boolean prop. Deferred in favour of standalone `LipiSkeleton` component in Phase 2.7. LipiCard is synchronous.
- **Dynamic card actions** (context menu ⋮) — Phase 2.6.2+ alongside Modal.

---

## 13. Files to create

| File | Location |
|---|---|
| `LipiCardTypes.cs` | `src/LiPi.Web/Components/Shared/` |
| `LipiCard.razor` | `src/LiPi.Web/Components/Shared/` |
| `LipiCard.razor.css` | `src/LiPi.Web/Components/Shared/` |
| `CardHeader.razor` | `src/LiPi.Web/Components/Shared/` |
| `CardBody.razor` | `src/LiPi.Web/Components/Shared/` |
| `CardFooter.razor` | `src/LiPi.Web/Components/Shared/` |
| `lipi-cards.css` | `src/LiPi.Web/wwwroot/css/` |

`lipi-cards.css` — shared family CSS (all layout: display, flex, grid, border-radius).
`LipiCard.razor.css` — scoped shape-only (hover scale transform, focus ring offset).
`CardHeader.razor` / `CardBody.razor` / `CardFooter.razor` — named sub-components
(§2); each renders its own HTML directly inside LipiCard's `CascadingValue`.

---

## 14. Deploy script additions

```
"LipiCardTypes.cs"  = "src\LiPi.Web\Components\Shared\LipiCardTypes.cs"
"LipiCard.razor"    = "src\LiPi.Web\Components\Shared\LipiCard.razor"
"LipiCard.razor.css" = "src\LiPi.Web\Components\Shared\LipiCard.razor.css"
"CardHeader.razor"  = "src\LiPi.Web\Components\Shared\CardHeader.razor"
"CardBody.razor"    = "src\LiPi.Web\Components\Shared\CardBody.razor"
"CardFooter.razor"  = "src\LiPi.Web\Components\Shared\CardFooter.razor"
"lipi-cards.css"    = "src\LiPi.Web\wwwroot\css\lipi-cards.css"
```

App.razor cache bump required (new CSS file — lipi-cards.css).
App.razor also needs `<link>` tag for lipi-cards.css added after lipi-alerts.css.

---

## 15. StyleGuide additions

New section `#cards` demonstrating:
1. All five variants side by side
2. Clickable grid — 4 appointment slots, one selected
3. Accent cards — all five AccentColor values
4. Full slotted card — CardHeader (title + subtitle + action) + CardBody + CardFooter
5. Free content card (no slots — PatientNew panel pattern)
6. Flat cards in a 3-column metric dashboard layout
7. Elevated card as a featured panel
8. Clickable + Href — rendered as `<a>` (inspect shows anchor element)
9. Disabled clickable card

---

*End of Phase 2.6.1 LipiCard Specification*
