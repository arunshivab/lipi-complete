# Phase 2.6.2 — LipiModal Specification

**Status:** LOCKED
**Component:** LipiModal + LipiModalService
**Built on:** Shared overlay infrastructure (see 00-Phase2.6.2-Overview.md)

---

## 1. Usage patterns

### Declarative (inline modal)
```razor
@if (_editOpen)
{
    <LipiModal Title="Edit holiday"
               Subtitle="Annual leave override"
               Icon="ti-calendar-event"
               IconColor="ModalIconColor.Info"
               Size="ModalSize.Standard"
               IsDirty="@(_form != _original)"
               OnClose="@CloseEdit">
        <ModalBody>
            ... form fields ...
        </ModalBody>
        <ModalFooter>
            <LipiButton Variant="ButtonVariant.Secondary" OnClick="@CloseEdit">
                Cancel
            </LipiButton>
            <LipiButton OnClick="@SaveAsync" IsBusy="@_saving">
                Save
            </LipiButton>
        </ModalFooter>
    </LipiModal>
}
```

### Programmatic (confirmation)
```razor
@inject ILipiModalService Modal

private async Task DeleteUser()
{
    var confirmed = await Modal.ConfirmAsync(
        title: "Delete user?",
        message: $"This will permanently delete {user.Name}. This action cannot be undone.",
        intent: ConfirmIntent.Danger);

    if (confirmed)
        await UserService.DeleteAsync(user.Id);
}
```

### Programmatic (alert)
```csharp
await Modal.AlertAsync(
    title: "Critical lab result",
    message: "K+ 6.8 mEq/L — physician review required.",
    intent: AlertIntent.Critical);
```

### Programmatic (prompt)
```csharp
var reason = await Modal.PromptAsync(
    title: "Override allergy warning",
    label: "Reason (required for HIPAA audit)",
    defaultValue: null);

if (reason != null) /* user provided reason */;
```

### Programmatic (custom component)
```csharp
var result = await Modal.ShowAsync<DrugPickerModal, DrugSelection>(
    parameters: new() {
        ["PatientId"] = patient.Id,
        ["Indication"] = "infection"
    });
```

---

## 2. LipiModal parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `Title` | `string` | required | Modal heading |
| `Subtitle` | `string?` | null | Muted line below title |
| `Icon` | `string?` | null | Tabler/Lucide icon class |
| `IconColor` | `ModalIconColor` | `None` | None/Info/Success/Warning/Danger/Critical |
| `Size` | `ModalSize` | `Standard` | Compact (400) / Standard (520) / Wide (680) / Fullscreen (95vw) |
| `MaxWidth` | `int?` | null | px override, wins over Size |
| `Intent` | `ModalIntent` | `Default` | Default / Confirmation / Alert / Wizard / Preview / Progress — drives subtle styling, defaults |
| `ShowCloseButton` | `bool` | `true` | ✕ in top-right. Auto-false for Critical/Progress |
| `CloseOnEscape` | `bool` | `true` | Auto-false for Critical/Sign/Progress |
| `CloseOnBackdrop` | `bool` | `true` | Auto-false for Critical/Sign/Progress/Wizard |
| `IsDirty` | `bool` | `false` | Triggers discard confirmation on close attempts |
| `IsBusy` | `bool` | `false` | Disables all controls, shows spinner on primary action |
| `Animate` | `bool` | `true` | Auto-false for Critical intent |
| `Animation` | `ModalAnimation` | `FadeSlide` | None / Fade / FadeSlide / FadeScale |
| `OnClose` | `EventCallback` | — | Fires when modal closes via any path |
| `AutoFocusSelector` | `string?` | null | CSS selector for initial focus override |

### Structured action params (footer shortcut)
| Parameter | Type | Default |
|---|---|---|
| `PrimaryAction` | `string?` | null |
| `PrimaryVariant` | `ButtonVariant` | `Primary` |
| `OnPrimaryAction` | `EventCallback` | — |
| `SecondaryAction` | `string?` | null |
| `SecondaryVariant` | `ButtonVariant` | `Secondary` |
| `OnSecondaryAction` | `EventCallback` | — |
| `FooterAlign` | `ModalFooterAlign` | `End` | Start / End / SpaceBetween |

When `<ModalFooter>` slot is provided, structured params are ignored (slot wins, dev warning logged).

---

## 3. Sub-components

### `<ModalBody>` — content area
Wraps the modal body. Has consistent padding (`16px 20px`). Scrolls internally
when content exceeds `max-height: 75vh`.

### `<ModalFooter>` — actions area
Sticky footer at bottom. `border-top: 0.5px solid var(--color-border-tertiary)`.
Padding `12px 20px`. Default flex-end alignment.

---

## 4. Enums (LipiModalTypes.cs)

```csharp
public enum ModalSize
{
    Compact,    // 400px - confirmations, simple inputs
    Standard,   // 520px - forms, most use cases
    Wide,       // 680px - complex content, pickers, duplicate detection
    Fullscreen  // 95vw - wizards, previews, large datasets
}

public enum ModalIconColor
{
    None,
    Info,       // blue tint
    Success,    // green tint
    Warning,    // amber tint
    Danger,     // red tint
    Critical    // dark red tint
}

public enum ModalIntent
{
    Default,       // form, info, generic
    Confirmation,  // confirm action
    Alert,         // info/success/warning notice
    Wizard,        // multi-step (auto: CloseOnEscape=false, FooterAlign=SpaceBetween)
    Preview,       // image/document (auto: Size=Fullscreen)
    Progress       // long operation (auto: ShowCloseButton=false, CloseOnEscape=false)
}

public enum ModalAnimation
{
    None,
    Fade,         // 150ms opacity
    FadeSlide,    // 200ms opacity + 20px translateY
    FadeScale     // 200ms opacity + scale 0.95 → 1.0
}

public enum ModalFooterAlign
{
    Start,        // left-aligned (back/cancel only)
    End,          // right-aligned — default
    SpaceBetween  // wizards — Back left, Next right
}

public enum ConfirmIntent
{
    Default,    // generic yes/no
    Danger,     // delete/destructive - red primary button
    Warning,    // overwrite/replace - amber primary button
    Critical    // mandatory - no Cancel, primary acts as "I understand"
}

public enum AlertIntent
{
    Info,
    Success,
    Warning,
    Danger,
    Critical
}
```

---

## 5. LipiModalService API

```csharp
public interface ILipiModalService
{
    // Yes/no confirmation
    Task<bool> ConfirmAsync(
        string title,
        string message,
        ConfirmIntent intent = ConfirmIntent.Default,
        string? primaryLabel = null,    // default: intent-specific ("Delete", "Confirm")
        string? cancelLabel = null);    // default: "Cancel"

    // Acknowledgment dialog
    Task AlertAsync(
        string title,
        string message,
        AlertIntent intent = AlertIntent.Info,
        string? okLabel = null);        // default: "OK"

    // Single-input prompt
    Task<string?> PromptAsync(
        string title,
        string label,
        string? defaultValue = null,
        string? placeholder = null,
        bool required = true);          // null returned on cancel

    // Open any component as a modal
    Task<TResult?> ShowAsync<TComponent, TResult>(
        Dictionary<string, object?>? parameters = null,
        ModalSize size = ModalSize.Standard,
        string? title = null)
        where TComponent : ComponentBase;

    // Close topmost modal programmatically
    Task CloseTopAsync(object? result = null);

    // Stack depth
    int StackDepth { get; }
}
```

`ShowAsync` requires the component to expose a `[Parameter] public EventCallback<TResult> OnResult` for returning the value to the caller.

---

## 6. Stack management

- Maximum 3 modals deep
- Stack depth 3 logs dev warning (`[LipiModal] Stack depth reached 3 — consider redesigning flow`)
- Stack depth 4 throws `InvalidOperationException`
- Each new modal renders at z-index `800 + (stackIndex * 10)`
- Backdrop renders once at z-index `799`, opacity does NOT compound
- Only topmost modal traps focus; lower modals are visually present but `inert`

---

## 7. Visual design

### Backdrop
```css
.lipi-overlay-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(11, 37, 69, 0.45);  /* navy with alpha */
    z-index: 799;
}
```

### Modal box
```css
.lipi-modal {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background: var(--color-bg-surface);
    border-radius: var(--lipi-modal-radius, var(--r-lg));
    box-shadow: 0 20px 60px rgba(11, 37, 69, 0.3);
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    z-index: 800; /* +10 per stack level */
}

.lipi-modal-compact    { width: 400px; max-width: 96vw; }
.lipi-modal-standard   { width: 520px; max-width: 96vw; }
.lipi-modal-wide       { width: 680px; max-width: 96vw; }
.lipi-modal-fullscreen { width: 95vw; height: 95vh; }
```

### Header (sticky)
```css
.lipi-modal-header {
    position: sticky;
    top: 0;
    background: var(--color-bg-surface);
    padding: 14px 20px;
    border-bottom: 0.5px solid var(--color-border-tertiary);
    display: flex;
    align-items: center;
    gap: 10px;
    z-index: 1;
}

.lipi-modal-icon {
    width: 32px; height: 32px;
    border-radius: 8px;
    display: flex; align-items: center; justify-content: center;
    flex-shrink: 0;
}

.lipi-modal-title { font-size: 14px; font-weight: 600; flex: 1; }
.lipi-modal-subtitle { font-size: 11px; color: var(--color-text-muted); }

.lipi-modal-close {
    width: 28px; height: 28px;
    border-radius: 6px;
    border: 0.5px solid var(--color-border-default);
    background: var(--color-bg-surface);
    cursor: pointer;
}
```

### IconColor backgrounds
| IconColor | Background | Foreground |
|---|---|---|
| Info | `#E6F1FB` | `#185FA5` |
| Success | `#EAF3DE` | `#3B6D11` |
| Warning | `#FAEEDA` | `#854F0B` |
| Danger | `#FCEBEB` | `#A32D2D` |
| Critical | `#501313` | `#F09595` (text color reversed) |

### Body
```css
.lipi-modal-body {
    padding: 16px 20px;
    overflow-y: auto;
    flex: 1;
    min-height: 0;
}
```

### Footer
```css
.lipi-modal-footer {
    padding: 12px 20px;
    border-top: 0.5px solid var(--color-border-tertiary);
    display: flex;
    gap: 10px;
    align-items: center;
    justify-content: flex-end;  /* End - default */
    background: var(--color-bg-surface);
    flex-shrink: 0;
}

.lipi-modal-footer-start         { justify-content: flex-start; }
.lipi-modal-footer-space-between { justify-content: space-between; }
```

---

## 8. Animations

| Animation | CSS |
|---|---|
| Fade | `opacity: 0 → 1` over 150ms ease-out |
| FadeSlide | `opacity: 0 → 1` + `translateY(20px) → 0` over 200ms ease-out |
| FadeScale | `opacity: 0 → 1` + `scale(0.95) → 1` over 200ms ease-out |
| None | instant |

Implemented via CSS class swap. JavaScript not required.

Critical intent forces `Animation=None` regardless of param value.

---

## 9. Multi-industry tokens

```css
--lipi-modal-radius:           var(--r-lg);        /* 12px default */
--lipi-modal-shadow:           0 20px 60px rgba(11,37,69,.3);
--lipi-modal-backdrop-color:   rgba(11,37,69,.45);
--lipi-modal-padding-body:     16px 20px;
--lipi-modal-padding-head:     14px 20px;
--lipi-modal-padding-foot:     12px 20px;
```

---

## 10. Intent → auto-defaults

Setting `Intent` cascades sensible defaults that the consumer can still override:

| Intent | Auto-sets |
|---|---|
| Default | (no auto-defaults) |
| Confirmation | Size=Compact, FooterAlign=End |
| Alert | Size=Compact, FooterAlign=End |
| Wizard | CloseOnEscape=false, FooterAlign=SpaceBetween |
| Preview | Size=Fullscreen, padding=0 on body |
| Progress | ShowCloseButton=false, CloseOnEscape=false, CloseOnBackdrop=false, Animate=false |

Explicit params win over Intent auto-defaults.

---

## 11. Migration from existing bespoke modals

These four existing patterns get replaced:

| Existing | Replace with |
|---|---|
| `pe-confirm-box` | `Modal.ConfirmAsync(...)` programmatic |
| `pr-modal-box` (identifier entry) | `<LipiModal Size="Standard" Title="Patient identifiers" Icon="ID" IconColor="Info">` |
| `sc-modal-box` (scheduler template) | `<LipiModal Size="Standard" Title="Edit template">` |
| `dup-box` (duplicate detection) | `<LipiModal Size="Wide" Title="Potential duplicate" Icon="ti-alert" IconColor="Warning">` |

Migration is per-page, not all-at-once. Old modals continue working until each page is touched.

---

## 12. StyleGuide additions

New section `#modals`:
1. All four sizes
2. All six IconColor values
3. All four animations (with toggle)
4. Confirmation via service (programmatic button trigger)
5. Alert via service
6. Prompt via service
7. Custom modal via ShowAsync
8. IsDirty discard confirmation
9. IsBusy state
10. Stack: open modal that opens another modal
11. All six Intent presets

---

*See: `00-Phase2.6.2-Overview.md` for shared infrastructure, `BUILD-CHAT-HANDOFF.md` for build order.*
