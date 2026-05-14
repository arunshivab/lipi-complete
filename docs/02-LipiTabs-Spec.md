# Phase 2.6.1 — LipiTabs Design Specification

**Status:** LOCKED — ready for build chat  
**Component:** LipiTabs + LipiTab  
**Phase:** 2.6.1 (Card + Alert + Tabs — Tabs first)  
**Next:** LipiCard (2.6.1), LipiAlert (2.6.1), LipiModal (2.6.2), LipiDynamicTabs (2.6.2)

---

## 1. Component structure

Two components, always used together:

```razor
<LipiTabs Variant="TabsVariant.Underline"
           RenderMode="TabRenderMode.Lazy"
           @bind-ActiveTab="_activeTab">

    <LipiTab Key="demographics" Label="Demographics"
             Icon="user" State="TabState.Complete">
        <p>Panel content here</p>
    </LipiTab>

    <LipiTab Key="labs" Label="Lab results"
             State="TabState.Empty" Count="3">
        <p>Lab panel</p>
    </LipiTab>

</LipiTabs>
```

---

## 2. LipiTabs parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Variant` | `TabsVariant` | `Underline` | Underline / Pill / Vertical |
| `RenderMode` | `TabRenderMode` | `Lazy` | Lazy / Eager |
| `IconOnly` | `bool` | `false` | Vertical only — icon strip with label as tooltip |
| `ActiveTab` | `string?` | first tab key | Controlled active tab |
| `ActiveTabChanged` | `EventCallback<string?>` | — | Two-way binding |
| `TabShortcutPattern` | `TabShortcutPattern` | `None` | None / CtrlNumber / AltNumber |
| `Class` | `string?` | null | Append-only layout utility |
| `AriaLabel` | `string?` | null | Accessible label for the tablist |

---

## 3. LipiTab parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Key` | `string` | auto (tab-0…N) | Stable identity for @bind-ActiveTab |
| `Label` | `string` | required | Visible label. Used as aria-label in IconOnly mode. |
| `Icon` | `string?` | null | Lucide icon name. Required when IconOnly=true. |
| `State` | `TabState` | `Default` | Default / Complete / Partial / Empty / Optional |
| `Optional` | `bool` | `false` | **Vertical variant only** — inserts a section divider in the rail before this tab. Inert on Underline/Pill. NOT the dashed border — see §3.1. |
| `Disabled` | `bool` | `false` | Not clickable. Content hidden. |
| `Count` | `int?` | null | Badge count shown on tab (e.g. pending lab results) |
| `ChildContent` | `RenderFragment` | required | Panel content |

Dynamic tabs (Phase 2.6.2 — NOT in this phase):  
`CanClose`, `IsDirty`, `OnClose` — deferred to LipiDynamicTabs.

---

## 3.1. The two "Optional" mechanisms

`LipiTab` expresses "this tab's content is optional" through **two intentionally
separate mechanisms**. They are not duplicates and are not interchangeable — each
is the optional-content cue for a different variant.

| Mechanism | What it does | Where it applies |
|---|---|---|
| `Optional` (bool, on `LipiTab`) | Inserts a section divider in the rail **before** this tab. | **Vertical only.** Inert in Underline and Pill. |
| `State="TabState.Optional"` | Renders the **dashed-border** colour treatment. In Vertical, also treats the tab as stateless (no state dot). | Underline (dashed border) + Vertical (stateless). Ignored by Pill. |

The optional cue is variant-specific by design: the Vertical rail shows it as a
divider, the Underline strip shows it as a dashed border. A tab can use either,
both, or neither.

**Why two mechanisms (locked — Phase 2.6.1 design session).** A rail divider and a
dashed underline are different visual languages suited to different variants;
collapsing them into one flag would force one variant's treatment onto the other.
The split was deliberate and is kept as-is. Three options were weighed (keep both,
merge into one, rename the bool) — "keep both, mitigate" was chosen. Full decision
record: `CHANGE-LOG.md` A21.

**Developer guard.** Because `Optional` is silently inert outside the Vertical
variant, `LipiTab.OnInitialized` guards the likely mistake — setting
`Optional="true"` on an Underline or Pill `LipiTabs`. In **Development** this throws
`InvalidOperationException` pointing the developer at `State="TabState.Optional"`;
in **Production** it logs via `ILogger<LipiTab>` and continues (the flag is inert,
so rendering is unaffected — there is nothing to fall back to). This mirrors the
env-gated validation pattern used by LipiButton (`CHANGE-LOG.md` A14).

**Quick reference.**

```razor
@* Underline — optional-content cue = dashed border *@
<LipiTab Key="review" Label="Review" State="TabState.Optional"> ... </LipiTab>

@* Vertical — optional-content cue = rail section divider *@
<LipiTab Key="notes" Label="Notes" Optional="true"> ... </LipiTab>

@* Vertical — divider AND stateless treatment *@
<LipiTab Key="notes" Label="Notes" Optional="true" State="TabState.Optional"> ... </LipiTab>
```

---

## 4. Enums (new file: LipiTabsTypes.cs)

```csharp
public enum TabsVariant    { Underline, Pill, Vertical }
public enum TabRenderMode  { Lazy, Eager }
public enum TabState       { Default, Complete, Partial, Empty, Optional }
public enum TabShortcutPattern { None, CtrlNumber, AltNumber }
```

---

## 5. Visual design per variant

### 5A — Underline (default)

- Tab strip: `display: flex`, `border-bottom: 1.5px solid var(--color-border-tertiary)`
- Each tab: `padding: .6rem 1rem`, `border-bottom: 2px solid transparent`, `-1.5px margin-bottom` (overlaps strip border)
- Active: `color: var(--color-primary)`, `border-bottom-color: var(--color-primary)`
- Hover: `color: var(--color-text-primary)`, `background: var(--color-bg-hover)` (subtle)
- Tab strip padding: `0 1rem` (aligns with page content gutters)

**TabState on Underline:**
| State | Underline color | Label color |
|---|---|---|
| Default | transparent | `--color-text-secondary` |
| Complete | `--color-success` | `--color-success-text-strong` |
| Partial | `--color-warning` | `--color-warning-text-strong` |
| Empty | `--color-danger` | `--color-danger` |
| Optional | dashed `--color-border-default` | `--color-text-secondary` |

### 5B — Pill

- Outer container: `display: flex`, `gap: 4px`, `padding: .5rem`, `background: var(--color-bg-subtle)`, `border-radius: var(--r-md)`
- Each tab: `padding: .4rem .85rem`, `border-radius: var(--r-sm)`, `color: var(--color-text-secondary)`
- Active: `background: var(--color-bg-surface)`, `color: var(--color-text-primary)`, `font-weight: 500`, `border: 0.5px solid var(--color-border-default)`
- **No TabState on Pill** — view-switcher only, no validation context

### 5C — Vertical

- Rail: `width: 200px`, `border-right: 0.5px solid var(--color-border-tertiary)`, `padding: .5rem 0`
- Each tab: `padding: .5rem .85rem`, `border-right: 2px solid transparent`, `-0.5px margin-right`
- Active: `color: var(--color-primary)`, `border-right-color: var(--color-primary)`, `background: var(--color-primary-pale)`
- State dot: `6px circle`, right-aligned in tab row. Colors match TabState.
- Section dividers: `0.5px horizontal line` between tab groups (e.g. before Review tab in PatientNew)

**IconOnly mode (Vertical only):**
- Rail width: `52px`
- Each tab: `40×40px`, `border-radius: var(--r-sm)`, icon `18px`, step number `8px` below icon
- State badge: `8px circle`, top-right corner of icon cell
- Tooltip: label + shortcut hint on hover (`title` attribute — no custom tooltip component needed in Phase 2.6.1)
- Active border-right preserved at `2px`

---

## 6. Keyboard navigation

### On the tab strip (focus on a tab header)
- `ArrowRight` / `ArrowDown` — move to next tab, activate immediately (WAI-ARIA Pattern A)
- `ArrowLeft` / `ArrowUp` — move to previous tab, activate immediately
- `Home` — first tab
- `End` — last tab
- `Tab` — moves focus INTO the active panel content (not to next tab)

### Inside panel content (focus in a textbox etc.)
- Arrow keys → normal text cursor movement. NO tab switching.
- `Tab` / `Shift+Tab` → normal focus movement through form fields.
- Shortcut (if `TabShortcutPattern != None`) → switches tab from anywhere inside component.

### Shortcut patterns
- `CtrlNumber`: `Ctrl+1` through `Ctrl+9` — switches to tab by position. Fires from anywhere inside `LipiTabs` container.
- `AltNumber`: `Alt+1` through `Alt+9` — same behaviour. Safer in forms (Alt doesn't conflict with text editing).
- Shortcut hint shown in IconOnly tooltip: e.g. "Identity (Alt+1)"
- Ctrl+Tab is NOT used — browser-reserved (switches browser tabs).

---

## 7. Panel rendering

### Lazy (default)
- Only the active panel is rendered in DOM.
- Switching tabs unmounts previous panel — component state (_isTouched, open dropdowns) resets.
- Use for: admin switchers, patient record tabs, calendar views.

### Eager
- All panels rendered, active shown via `display: block`, inactive via `display: none`.
- Component state survives tab switching.
- Use for: PatientNew (form state + validation must survive tab navigation).
- CSS: `.lipi-tab-panel { display: none; }` `.lipi-tab-panel.active { display: block; }`

---

## 8. Active tab binding

Key-based — `@bind-ActiveTab` takes and emits a string key.

```razor
@* Controlled *@
<LipiTabs @bind-ActiveTab="_activeTab">
    <LipiTab Key="identity" Label="Identity">...</LipiTab>
    <LipiTab Key="address"  Label="Address">...</LipiTab>
</LipiTabs>

@* Uncontrolled — LipiTabs manages internally *@
<LipiTabs>
    <LipiTab Key="day"   Label="Day">...</LipiTab>
    <LipiTab Key="week"  Label="Week">...</LipiTab>
</LipiTabs>
```

If no `Key` set on a `LipiTab`, auto-generates `tab-0`, `tab-1` etc. as stable keys (stable across re-renders for a given tab count).

---

## 9. CSS class catalog

### LipiTabs wrapper
```
.lipi-tabs                          — root wrapper
.lipi-tabs-underline                — Underline variant
.lipi-tabs-pill                     — Pill variant
.lipi-tabs-vertical                 — Vertical variant
.lipi-tabs-icon-only                — Vertical + IconOnly mode
.lipi-tabs-eager                    — Eager render mode (affects panel CSS)
```

### Tab strip / list
```
.lipi-tablist                       — the tab strip (role="tablist")
```

### Individual tab buttons
```
.lipi-tab                           — each tab button (role="tab")
.lipi-tab-active                    — currently active tab
.lipi-tab-disabled                  — disabled tab
.lipi-tab-state-complete            — TabState.Complete
.lipi-tab-state-partial             — TabState.Partial
.lipi-tab-state-empty               — TabState.Empty
.lipi-tab-state-optional            — TabState.Optional (dashed)
.lipi-tab-icon                      — icon element inside tab
.lipi-tab-label                     — label text element
.lipi-tab-count                     — count badge element
.lipi-tab-state-dot                 — state dot (Vertical only)
.lipi-tab-step-num                  — step number (IconOnly mode)
```

### Panels
```
.lipi-tab-panels                    — panels container
.lipi-tab-panel                     — individual panel (role="tabpanel")
.lipi-tab-panel-active              — currently visible panel
```

### Layout rules (shared CSS — lipi-tabs.css, NOT scoped)
All display / flex / grid rules in shared CSS per Layout-vs-Shape rule.

---

## 10. Accessibility

- `role="tablist"` on the tab strip
- `role="tab"` + `aria-selected="true/false"` + `aria-controls="panel-{key}"` on each tab
- `role="tabpanel"` + `id="panel-{key}"` + `aria-labelledby="tab-{key}"` on each panel
- `tabindex="0"` on active tab, `tabindex="-1"` on all others (roving tabindex)
- `aria-disabled="true"` on disabled tabs
- In IconOnly mode: `aria-label="{Label}"` on the icon-only button. `title="{Label} ({shortcut})"` for hover tooltip.
- Keyboard shortcut announced: LipiTabs renders a `<div class="sr-only">` listing available shortcuts when `TabShortcutPattern != None`

---

## 11. PatientNew mapping

```razor
<LipiTabs Variant="TabsVariant.Vertical"
           IconOnly="true"
           RenderMode="TabRenderMode.Eager"
           TabShortcutPattern="TabShortcutPattern.None"
           @bind-ActiveTab="_activeTab">

    <LipiTab Key="identity"      Label="Identity"      Icon="user"
             State="@_identityState" />

    <LipiTab Key="address"       Label="Address"        Icon="map-pin"
             State="@_addressState" />

    <LipiTab Key="socioeconomic" Label="Socioeconomic"  Icon="chart-bar"
             State="@_socioState"  Optional="true" />

    <LipiTab Key="reg-details"   Label="Reg. details"   Icon="clipboard"
             State="@_regState" />

    @* Section divider before Review *@
    <LipiTab Key="review"        Label="Review"         Icon="eye"
             Optional="true" />

</LipiTabs>
```

Tab state computed from form validation per tab — each tab tracks `_isTouched` of its fields, reads `EditContext` messages, computes `TabState`.

---

## 12. Dynamic tabs (Phase 2.6.2 — NOT in scope here)

Additional parameters deferred to `LipiDynamicTabs` (separate component built on LipiTabs):
- `CanClose`, `IsDirty`, `OnClose` per tab
- `OnTabAdded`, max tab count
- Dirty-close confirmation dialog (uses LipiModal from Phase 2.6.2)
- Multi-patient workstation use case

Design session required before Phase 2.6.2 build opens.

---

## 13. Files to create

| File | Location |
|---|---|
| `LipiTabsTypes.cs` | `src/LiPi.Web/Components/Shared/` |
| `LipiTabs.razor` | `src/LiPi.Web/Components/Shared/` |
| `LipiTab.razor` | `src/LiPi.Web/Components/Shared/` |
| `lipi-tabs.css` | `src/LiPi.Web/wwwroot/css/` |

No scoped `.razor.css` — all layout rules in `lipi-tabs.css` (shared).  
Reference in `App.razor` CSS link order after `lipi-inputs.css`.

---

## 14. Deploy script

Add four new entries to `deploy-downloads.ps1`:
```
"LipiTabsTypes.cs"   = "src\LiPi.Web\Components\Shared\LipiTabsTypes.cs"
"LipiTabs.razor"     = "src\LiPi.Web\Components\Shared\LipiTabs.razor"
"LipiTab.razor"      = "src\LiPi.Web\Components\Shared\LipiTab.razor"
"lipi-tabs.css"      = "src\LiPi.Web\wwwroot\css\lipi-tabs.css"
```

App.razor cache bump required (new CSS file).

---

## 15. StyleGuide additions

New section `#tabs` demonstrating:
1. All three variants side by side
2. All five TabState values on Underline
3. IconOnly vertical (PatientNew pattern) with state dots
4. Pill (calendar view switcher, no state)
5. Vertical with section divider
6. AltNumber shortcut pattern (hint visible on hover)

---

*End of Phase 2.6.1 LipiTabs Specification*

---

## 16. Visual decisions locked

### Underline variant
- **Corner radius:** Option C — full radius on hover/active background cell.
  `border-radius: var(--lipi-tab-radius, var(--r-sm))` on the tab button.
  Underline still runs full tab width (not clipped by radius).
- **Strip background:** Transparent.

### Pill variant
- Inherent rounded shape. `border-radius: var(--lipi-tab-radius, var(--r-sm))` on each item.
- Container: `background: var(--color-bg-subtle)`, `border-radius: var(--r-md)`.
- No TabState (view-switcher context only).

### Vertical variant
- **Active treatment:** Option B — full rounded background block inside the rail.
  Active tab: `background: var(--lipi-tab-active-bg, var(--color-primary-pale))`,
  `border-radius: var(--lipi-tab-radius, var(--r-sm))`. No right-border signal.
- Rail padding: `.5rem` on all sides to give background blocks breathing room.

---

## 17. Multi-industry token overrides

**Design principle (system-wide, not just tabs):**
Any visual property that varies by brand or industry gets a CSS custom property
with the LiPi clinical value as its fallback. Structural layout properties
(grid columns, flex direction, display mode) stay hardcoded — they define
component behaviour, not brand expression.

### Tab-specific custom properties

```css
/* Defaults encode the LiPi clinical aesthetic.
   Consuming projects override in their own root CSS — no component changes needed. */

--lipi-tab-radius:            var(--r-sm);                   /* 6px  — tab item corner radius */
--lipi-tab-underline-width:   2px;                           /* active underline thickness */
--lipi-tab-strip-bg:          transparent;                   /* tab strip background */
--lipi-tab-active-bg:         var(--color-primary-pale);     /* active tab background */
--lipi-tab-active-color:      var(--color-primary);          /* active tab text + underline */
--lipi-tab-hover-bg:          var(--color-bg-hover);         /* hover background */
```

### Example overrides for other industries

```css
/* Enterprise B2B — folder tab style, subtle strip */
:root {
    --lipi-tab-radius: 0;                                /* Option B flat */
    --lipi-tab-strip-bg: var(--color-bg-subtle);         /* washed strip */
    --lipi-tab-underline-width: 3px;
}

/* Consumer health app — soft, rounded */
:root {
    --lipi-tab-radius: 12px;                             /* extra soft */
    --lipi-tab-active-bg: var(--color-primary-pale);
}

/* Developer tools — minimal, no bg fill */
:root {
    --lipi-tab-radius: 0;
    --lipi-tab-active-bg: transparent;                   /* underline only */
    --lipi-tab-hover-bg: transparent;
}
```

### How to implement in CSS

Every tab visual property uses the custom property with LiPi value as fallback:

```css
/* Do this — brand-overridable */
.lipi-tab {
    border-radius: var(--lipi-tab-radius, var(--r-sm));
    background: transparent;
}

.lipi-tab:hover {
    background: var(--lipi-tab-hover-bg, var(--color-bg-hover));
}

.lipi-tab-active {
    background: var(--lipi-tab-active-bg, var(--color-primary-pale));
    color: var(--lipi-tab-active-color, var(--color-primary));
}

/* NOT this — hardcoded, brand-locked */
.lipi-tab {
    border-radius: 6px;        /* ← never hardcode visual values */
    background: transparent;
}
```

### Structural properties (stay hardcoded — not overridable)

```css
/* These define component behaviour — never expose as custom properties */
.lipi-tablist            { display: flex; }           /* layout */
.lipi-tabs-vertical      { display: flex; }           /* layout */
.lipi-tab-panel          { display: none; }           /* behaviour */
.lipi-tab-panel-active   { display: block; }          /* behaviour */
```

---

*End of Phase 2.6.1 LipiTabs Specification (updated with §16 visual decisions + §17 multi-industry tokens)*
