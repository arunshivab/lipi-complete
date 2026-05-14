# Phase 2.6.2 — LipiDrawer Specification

**Status:** LOCKED
**Component:** LipiDrawer + LipiDrawerService
**Built on:** Shared overlay infrastructure (see 00-Phase2.6.2-Overview.md)

---

## 1. Usage patterns

### Declarative (inline drawer)
```razor
@if (_drawerOpen)
{
    <LipiDrawer Placement="DrawerPlacement.Right"
                Size="DrawerSize.Standard"
                Title="Patient quick view"
                Icon="ti-user"
                Backdrop="DrawerBackdrop.Dimmed"
                IsDirty="@(_hasChanges)"
                OnClose="@CloseDrawer">
        <DrawerBody>
            ... patient summary content ...
        </DrawerBody>
        <DrawerFooter>
            <LipiButton OnClick="@OpenFullRecord">Open full record</LipiButton>
        </DrawerFooter>
    </LipiDrawer>
}
```

### Programmatic
```csharp
@inject ILipiDrawerService Drawer

private async Task OpenComments()
{
    await Drawer.ShowAsync<CommentsPanel, bool>(
        placement: DrawerPlacement.Right,
        parameters: new() { ["EntityId"] = patient.Id });
}
```

---

## 2. LipiDrawer parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Placement` | `DrawerPlacement` | `Right` | Right / Left / Top / Bottom |
| `Size` | `DrawerSize` | `Standard` | Compact / Standard / Wide / FullSide |
| `SizePx` | `int?` | null | px override, wins over Size |
| `Title` | `string` | required | Drawer heading |
| `Subtitle` | `string?` | null | Muted line below title |
| `Icon` | `string?` | null | Icon class |
| `IconColor` | `DrawerIconColor` | `None` | Same as modal icon colors |
| `Backdrop` | `DrawerBackdrop` | `Dimmed` | Dimmed / Light / None |
| `ShowCloseButton` | `bool` | `true` | ✕ button |
| `CloseOnEscape` | `bool` | `true` | |
| `CloseOnBackdropClick` | `bool` | `true` | Only applies when Backdrop != None |
| `IsDirty` | `bool` | `false` | Triggers discard confirmation |
| `IsBusy` | `bool` | `false` | Disables controls, shows busy state |
| `Animate` | `bool` | `true` | Slide-in animation |
| `OnClose` | `EventCallback` | — | Fires when drawer closes |
| `AutoFocusSelector` | `string?` | null | Initial focus override |
| `Pinnable` | `bool` | `false` | When true, shows 📌 pin button in header |
| `PinScope` | `DrawerPinScope` | `PageScoped` | Global / RouteScoped / PageScoped (see Pin Mode section) |
| `RouteScopePrefix` | `string?` | null | Required when PinScope = RouteScoped (e.g., "/patients") |
| `PinPersistKey` | `string?` | null | localStorage key for pin state (required when Pinnable = true) |
| `DefaultPinned` | `bool` | `false` | Initial pin state if no localStorage value exists |

### Structured action params (footer shortcut)
| Parameter | Type | Default |
|---|---|---|
| `PrimaryAction` | `string?` | null |
| `PrimaryVariant` | `ButtonVariant` | `Primary` |
| `OnPrimaryAction` | `EventCallback` | — |
| `SecondaryAction` | `string?` | null |
| `SecondaryVariant` | `ButtonVariant` | `Secondary` |
| `OnSecondaryAction` | `EventCallback` | — |

`<DrawerFooter>` slot wins when both provided.

---

## 3. Sub-components

### `<DrawerBody>` — content area
Padding `16px 20px`. Scrolls internally. Flex-1 to fill available space.

### `<DrawerFooter>` — actions area
Sticky at bottom. `border-top: 0.5px solid var(--color-border-tertiary)`.

---

## 4. Enums (LipiDrawerTypes.cs)

```csharp
public enum DrawerPlacement
{
    Right,    // most common — slides in from right
    Left,     // secondary navigation, filters
    Top,      // alerts, global notifications
    Bottom    // mobile-style action sheets
}

public enum DrawerSize
{
    Compact,   // R/L 320px · T/B 240px
    Standard,  // R/L 400px · T/B 400px (default)
    Wide,      // R/L 560px · T/B 480px
    FullSide   // R/L 95vw · T/B 95vh
}

public enum DrawerIconColor
{
    None,
    Info,
    Success,
    Warning,
    Danger,
    Critical
}

public enum DrawerBackdrop
{
    Dimmed,   // rgba(11,37,69,.45) — default
    Light,    // rgba(11,37,69,.10) — faint
    None      // transparent — page stays fully interactive
}

public enum DrawerPinScope
{
    Global,        // pinned everywhere across all routes (e.g., Notifications, Messages)
    RouteScoped,   // pinned only within RouteScopePrefix (e.g., Patient Summary scoped to /patients/*)
    PageScoped     // pinned only on current page (e.g., Filter drawer)
}
```

---

## 4.5 Pin Mode — detailed behavior

Drawer can be opened in two modes: **overlay** (default — slides over content) or **pinned** (occupies own column, content shrinks to make room).

### Pin mode availability
- Right + Left placement: pin mode supported
- Top + Bottom placement: pin mode NOT supported (vertical space too precious)

### Pin scope policies

| Scope | Behavior | Example use case |
|---|---|---|
| `Global` | Stays pinned everywhere across all routes | Notifications drawer, Messages drawer |
| `RouteScoped` | Stays pinned only within `RouteScopePrefix` (e.g., `/patients`). Auto-closes when navigating outside scope. | Patient Summary drawer (scope = `/patients/*`) |
| `PageScoped` | Pinned only on current page. Closes on any navigation. | Filter drawer, Sort options, page-specific tools |

### Pin state persistence
- Pin choice persists per-drawer-type via localStorage
- Key format: `lipi-drawer-pin:{PinPersistKey}` → stores `{ pinned: bool }`
- First time user opens a Pinnable drawer: starts in `DefaultPinned` state
- After user clicks 📌 pin button: state is remembered for next session

### Responsive behavior
- Below 1024px viewport width: pin mode automatically disabled
- Drawer renders as overlay regardless of pin state
- Pin state is preserved (remembered for when viewport is wide again)
- Above 1024px: pin state is honored

### Layout impact when pinned
- Page content area gets CSS class `lipi-page-shifted-{right|left}` 
- Content area width shrinks by drawer width
- Tab strip and BottomNav stay full width (drawer doesn't extend over them visually)

### Stack rules with pin mode
- Only **one pinned drawer at a time** (across all sides)
- Pinning a second drawer unpins the first (with dirty-state confirmation if applicable)
- Pinned drawer + overlay drawer on different side: not supported (overlay closes pin first)
- Modal can open on top of pinned drawer: drawer stays pinned and visible but inert

---

## 5. LipiDrawerService API

```csharp
public interface ILipiDrawerService
{
    // Open any component as a drawer
    Task<TResult?> ShowAsync<TComponent, TResult>(
        DrawerPlacement placement = DrawerPlacement.Right,
        Dictionary<string, object?>? parameters = null,
        DrawerSize size = DrawerSize.Standard,
        string? title = null)
        where TComponent : ComponentBase;

    // Close drawer at a placement
    Task CloseAsync(DrawerPlacement placement);

    // Check if drawer is open at placement
    bool IsOpen(DrawerPlacement placement);

    // Close all drawers
    Task CloseAllAsync();
}
```

`ShowAsync` requires the component to expose `[Parameter] public EventCallback<TResult> OnResult`.

---

## 6. Stack rules

- **Maximum one drawer per side** at any time
- Opening a second drawer on the same side closes the first (with dirty-state confirmation if applicable)
- Different sides can coexist: Right + Bottom simultaneously is valid (rare)
- Modal can open on top of an open drawer — drawer stays visible but inert, modal traps focus
- Opening a drawer while a modal is open: drawer renders BEHIND the modal (lower z-index), dev warning logged

---

## 7. Visual design

### Drawer positioning
```css
.lipi-drawer {
    position: fixed;
    background: var(--color-bg-surface);
    box-shadow: var(--lipi-drawer-shadow, 0 0 30px rgba(11,37,69,.2));
    display: flex;
    flex-direction: column;
    z-index: 700;  /* +5 per side */
}

.lipi-drawer-right {
    top: 0; right: 0; bottom: 0;
    height: 100vh;
    border-left: 0.5px solid var(--color-border-tertiary);
}
.lipi-drawer-left {
    top: 0; left: 0; bottom: 0;
    height: 100vh;
    border-right: 0.5px solid var(--color-border-tertiary);
}
.lipi-drawer-top {
    top: 0; left: 0; right: 0;
    width: 100vw;
    border-bottom: 0.5px solid var(--color-border-tertiary);
}
.lipi-drawer-bottom {
    bottom: 0; left: 0; right: 0;
    width: 100vw;
    border-top: 0.5px solid var(--color-border-tertiary);
}
```

### Sizes per placement
```css
/* Right / Left */
.lipi-drawer-right.lipi-drawer-compact,
.lipi-drawer-left.lipi-drawer-compact   { width: 320px; }
.lipi-drawer-right.lipi-drawer-standard,
.lipi-drawer-left.lipi-drawer-standard  { width: 400px; }
.lipi-drawer-right.lipi-drawer-wide,
.lipi-drawer-left.lipi-drawer-wide      { width: 560px; }
.lipi-drawer-right.lipi-drawer-fullside,
.lipi-drawer-left.lipi-drawer-fullside  { width: 95vw; }

/* Top / Bottom */
.lipi-drawer-top.lipi-drawer-compact,
.lipi-drawer-bottom.lipi-drawer-compact   { height: 240px; }
.lipi-drawer-top.lipi-drawer-standard,
.lipi-drawer-bottom.lipi-drawer-standard  { height: 400px; }
.lipi-drawer-top.lipi-drawer-wide,
.lipi-drawer-bottom.lipi-drawer-wide      { height: 480px; }
.lipi-drawer-top.lipi-drawer-fullside,
.lipi-drawer-bottom.lipi-drawer-fullside  { height: 95vh; }
```

### Backdrop
```css
.lipi-drawer-backdrop-dimmed { background: rgba(11,37,69,.45); }
.lipi-drawer-backdrop-light  { background: rgba(11,37,69,.10); }
/* DrawerBackdrop.None: no backdrop element rendered */
```

### Header / Body / Footer
Same structural pattern as LipiModal — sticky header, scrollable body,
sticky footer. Uses same CSS classes prefixed with `lipi-drawer-*`.

### Slide-in animations
```css
.lipi-drawer-right { transform: translateX(100%); transition: transform 250ms ease-out; }
.lipi-drawer-right.open { transform: translateX(0); }

.lipi-drawer-left { transform: translateX(-100%); transition: transform 250ms ease-out; }
.lipi-drawer-left.open { transform: translateX(0); }

.lipi-drawer-top { transform: translateY(-100%); transition: transform 250ms ease-out; }
.lipi-drawer-top.open { transform: translateY(0); }

.lipi-drawer-bottom { transform: translateY(100%); transition: transform 250ms ease-out; }
.lipi-drawer-bottom.open { transform: translateY(0); }
```

When `Animate=false`, transitions are disabled. Drawer appears instantly.

---

## 8. Multi-industry tokens

```css
--lipi-drawer-radius:        0;             /* drawers don't typically round - edges are flush */
--lipi-drawer-shadow:        0 0 30px rgba(11,37,69,.2);
--lipi-drawer-padding-body:  16px 20px;
--lipi-drawer-padding-head:  14px 20px;
--lipi-drawer-padding-foot:  12px 20px;
--lipi-drawer-anim-duration: 250ms;
```

---

## 9. Clinical use case mapping

| Use case | Placement | Size | Backdrop |
|---|---|---|---|
| Patient quick view | Right | Standard | Dimmed |
| Notification center | Right | Compact | None |
| Activity / audit log | Right | Wide | Light |
| Comments panel on entity | Right | Standard | None |
| Filter panel for list page | Left | Compact | None |
| Mobile primary nav | Left | Standard | Dimmed |
| Mobile action sheet | Bottom | Compact | Dimmed |
| Global alert banner | Top | Compact | None |

---

## 10. Accessibility

- `role="complementary"` (not `dialog` — drawer is a side region, not a modal dialog)
- Exception: when `Backdrop=Dimmed`, behaves modally → `role="dialog"`, `aria-modal="true"`
- `aria-labelledby` pointing to title
- Focus trap active when `Backdrop=Dimmed` OR `Backdrop=Light`
- Focus trap NOT active when `Backdrop=None` (user can interact with page beneath)
- `inert` on outer page content only when `Backdrop=Dimmed`

---

## 11. StyleGuide additions

New section `#drawers`:
1. All four placements (Right/Left/Top/Bottom)
2. All four sizes per placement
3. All three backdrop modes
4. Drawer with form + dirty state
5. Drawer opened programmatically
6. Right drawer + Bottom drawer open simultaneously
7. Modal opens on top of drawer — focus handoff
8. Drawer with IsBusy state

---

*See: `00-Phase2.6.2-Overview.md` for shared infrastructure.*
