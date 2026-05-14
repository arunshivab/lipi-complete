# Phase 2.6.2 — Overlay Surfaces (Modal + Drawer + Dynamic Tabs)

**Status:** Design LOCKED — ready for build chat
**Phase:** 2.6.2
**Components:** LipiModal, LipiDrawer, LipiDynamicTabs
**Prerequisite:** Phase 2.6.1 complete (LipiTabs, LipiAlert, LipiCard)

---

## Scope

Three components that share underlying infrastructure (focus trap, scroll lock,
stack management) but serve distinct UX purposes:

| Component | Surface | Primary purpose |
|---|---|---|
| LipiModal | Centered overlay with backdrop | All 11 modal patterns: confirm, form, picker, wizard, preview, alert, sign, re-auth, break-glass, progress, info |
| LipiDrawer | Side panel (R/L/T/B) | Contextual panels, navigation, filters, comments, notifications |
| LipiDynamicTabs | Workspace tab strip | Multi-patient workstation, draft management, parallel work contexts |

---

## Build order (mandatory)

1. **Shared infrastructure first** — `LipiOverlayHost` + focus trap + scroll lock + stack manager
2. **LipiModal** — built on the shared infrastructure
3. **LipiDrawer** — built on the shared infrastructure
4. **LipiDynamicTabs** — uses dirty-state confirmation (which uses LipiModal)

Building Modal before the shared infrastructure means rewriting Modal when Drawer
joins. Don't.

---

## Shared infrastructure — `LipiOverlayHost`

Single component placed once in `TopNavLayout.razor` (the authenticated clinical
layout). All clinical pages use TopNavLayout — overlays appear there.

`MainLayout.razor` is the public/unauthenticated layout (login, home). In v1.0
it does not need overlay infrastructure. If a future requirement surfaces
(e.g., session-timeout modal on the login screen), a second host instance
can be added later. For now: TopNavLayout only.

Manages:
- Active modal stack (max 3)
- Active drawer slots (1 per side, 4 sides)
- Body scroll lock when any overlay is open
- `inert` attribute on page content when overlay is open
- z-index allocation (modals: 800 + stackIndex*10; drawers: 700 + sideIndex*5)

```razor
@* In TopNavLayout.razor — exactly once, before the closing </div> *@
<LiPi.Web.Components.Shared.PatientFab />
<LipiOverlayHost />
```

### Focus trap (shared service)

```csharp
public interface IFocusTrapService
{
    Task ActivateAsync(ElementReference container, ElementReference? initialFocus = null);
    Task DeactivateAsync();
}
```

Implementation uses JS interop. Tab cycles through focusable elements within
`container`. Shift+Tab cycles backward. Escape is NOT handled here — each
component handles its own escape behavior.

### Scroll lock (shared service)

```csharp
public interface IScrollLockService
{
    void Lock();
    void Unlock();
}
```

Reference-counted. First `Lock()` adds `overflow: hidden` to `<body>`. Last
`Unlock()` removes it. Modal opening over an open drawer doesn't double-lock.

### Dirty-state confirmation (uses LipiModalService)

When any overlay with `IsDirty=true` is asked to close, the consuming component
calls:
```csharp
var discard = await Modal.ConfirmAsync(
    "Discard changes?",
    "You have unsaved changes. Closing this will lose them.",
    ConfirmIntent.Warning);
if (discard) await CloseAsync();
```

Same protocol for Modal, Drawer, and DynamicTabs (with a third option for tabs).

---

## File structure (Phase 2.6.2)

```
src/LiPi.Web/Components/Shared/
├── LipiOverlayHost.razor             ← shared host (registered in MainLayout)
├── LipiOverlayHost.razor.cs
├── LipiOverlayTypes.cs               ← shared enums

├── LipiModal.razor
├── LipiModal.razor.cs
├── LipiModal.razor.css
├── LipiModalService.cs               ← programmatic API
├── ConfirmDialog.razor               ← internal — used by ConfirmAsync
├── AlertDialog.razor                 ← internal — used by AlertAsync
├── PromptDialog.razor                ← internal — used by PromptAsync

├── LipiDrawer.razor
├── LipiDrawer.razor.cs
├── LipiDrawer.razor.css
├── LipiDrawerService.cs              ← programmatic API

├── LipiDynamicTabs.razor
├── LipiDynamicTabs.razor.cs
├── LipiDynamicTabs.razor.css
├── LipiDynamicTab.razor              ← child component for declarative use
├── LipiDynamicTabsService.cs         ← programmatic tab management

src/LiPi.Web/wwwroot/css/
├── lipi-overlays.css                 ← shared layout CSS (host, backdrop, animations)
├── lipi-dynamic-tabs.css             ← tab-specific layout CSS

src/LiPi.Web/Services/
├── FocusTrapService.cs               ← interop with JS
├── ScrollLockService.cs              ← reference-counted

src/LiPi.Web/wwwroot/js/
├── lipi-overlay-interop.js           ← focus trap JS, scroll lock fallback
```

---

## DI registration

In `Program.cs`:

```csharp
builder.Services.AddScoped<IFocusTrapService, FocusTrapService>();
builder.Services.AddScoped<IScrollLockService, ScrollLockService>();
builder.Services.AddScoped<ILipiModalService, LipiModalService>();
builder.Services.AddScoped<ILipiDrawerService, LipiDrawerService>();
builder.Services.AddScoped<ILipiDynamicTabsService, LipiDynamicTabsService>();
```

All scoped (per circuit). Each Blazor Server session has its own service instances.

---

## Standing rule for Phase 2.6.2

**Never duplicate focus trap or scroll lock logic.** If LipiModal needs focus
trapping, it calls `IFocusTrapService.ActivateAsync`. It does NOT have its own
keydown listener for Tab/Shift+Tab. Same for LipiDrawer. This is non-negotiable
because divergent focus trap implementations cause subtle accessibility bugs
that only surface with screen readers.

---

## Ghost click prevention (MANDATORY for all overlays)

Every popup / dropdown / overlay built in Phase 2.6.2 (and all subsequent phases) MUST implement ghost click prevention:

- **Backdrop captures mousedown** — prevents click-through to underlying elements
- **Underlying controls get `pointer-events: none`** when overlay is open
- **50ms close-guard** — if overlay is closing while user clicks a new control, the late click event is ignored (prevents stale-state interactions)
- **Escape closes overlay BEFORE other key handlers fire** — single Escape press dismisses topmost overlay, not multiple
- **Backdrop event handlers bound immediately** — no animation-delay window where backdrop is visually present but not interactive
- **Touch tap handled separately from click** — use `pointerdown` not `click` for tablet primary interaction
- **Tab order trapped within overlay when open** — see focus trap service

Applies to: LipiModal, LipiDrawer, ConfirmDialog, AlertDialog, PromptDialog, DirtyTabConfirmDialog, and any future popup/dropdown/menu/overlay component.

This is item #11 in the pre-delivery checklist.

---

## Cross-component interactions

These are valid and must work:

1. **Modal opens on top of open drawer** — drawer stays visible but inert (no focus). Modal traps focus. Closing modal returns focus and interactivity to drawer.

2. **Drawer opens while modal is open** — VALID but unusual. Drawer renders behind the modal (lower z-index). User must close modal first to interact with drawer. Dev warning logged.

3. **Modal on top of dynamic tab content** — normal. Tab stays as the underlying page surface.

4. **Closing a dirty dynamic tab opens a confirmation modal** — modal renders on top of the tab strip. Tab strip stays visible. Modal handles 3-option dialog (Cancel/Discard/Save & Close).

5. **Drawer in a dynamic tab context** — opens at the workspace level, not the tab level. Drawer is global navigation chrome, not per-tab content.

---

## Accessibility — non-negotiable

- All overlays: `role="dialog"` (modal) or `role="complementary"` (drawer)
- `aria-modal="true"` on modal containers
- `aria-labelledby` pointing to title element
- `aria-describedby` for body content (where applicable)
- Focus trap active when overlay is open
- Focus returns to trigger element on close
- `inert` on page content outside the overlay
- Escape handling per-component (modal default yes, drawer default yes, can be disabled)
- Screen reader announces overlay opening via `aria-live="polite"` region

---

*See:*
- `01-LipiModal-Spec.md`
- `02-LipiDrawer-Spec.md`
- `03-LipiDynamicTabs-Spec.md`
- `BUILD-CHAT-HANDOFF.md`
