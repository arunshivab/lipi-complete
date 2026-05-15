# Phase 2.7 — LipiToast Specification

**Status:** LOCKED
**Components:** LipiToast + LipiToastService + LipiToastHost

---

## 1. Purpose

Transient notifications that appear in a corner, auto-dismiss after a duration.
For confirmation feedback after actions ("Patient saved successfully"),
not for critical alerts (those use CDS modals or status overlays).

**Distinction from other feedback:**
- LipiToast = transient confirmation (auto-dismiss for non-errors)
- LipiAlert = persistent inline alert in page content
- LipiModal critical = mandatory acknowledgment
- Code call banner = facility-wide emergency

---

## 2. Service-based architecture

LipiToast is exclusively service-driven. No inline `<LipiToast Show="@_show" />` component.

```csharp
@inject ILipiToastService Toast

private async Task SavePatient()
{
    await PatientService.SaveAsync(_patient);
    Toast.Success("Patient saved successfully.");
}
```

A single `<LipiToastHost />` lives in TopNavLayout. The service dispatches to it.

---

## 3. Service API

```csharp
public interface ILipiToastService
{
    // Quick severity shortcuts
    Task Success(string message, ToastOptions? options = null);
    Task Error(string message, ToastOptions? options = null);
    Task Warning(string message, ToastOptions? options = null);
    Task Info(string message, ToastOptions? options = null);
    
    // Full-control method
    Task ShowAsync(ToastDescriptor toast);
    
    // Promise-style for async operations
    Task<TResult> PromiseAsync<TResult>(
        Task<TResult> operation,
        ToastPromiseOptions options);
    
    // Imperative
    Task DismissAsync(string toastId);
    Task DismissAllAsync();
}
```

### ToastOptions

```csharp
public class ToastOptions
{
    public string? Title { get; set; }
    public int? DurationMs { get; set; }     // null = use default per severity
    public ToastAction? Action { get; set; }  // optional button
    public bool Persistent { get; set; } = false;  // no auto-dismiss
    public string? Id { get; set; }          // dedup key
    public ToastPosition? Position { get; set; }  // override clinic default
}

public class ToastAction
{
    public string Label { get; set; }        // max 12 chars
    public Func<Task> OnClick { get; set; }
}

public class ToastDescriptor
{
    public string Message { get; set; }
    public ToastSeverity Severity { get; set; }
    public ToastOptions Options { get; set; } = new();
}

public class ToastPromiseOptions
{
    public string LoadingMessage { get; set; } = "Loading...";
    public string SuccessMessage { get; set; } = "Done.";
    public Func<Exception, string> ErrorMessage { get; set; } = e => e.Message;
    public string? Id { get; set; }    // dedup throughout lifecycle
}
```

### Usage examples

```csharp
// Simplest
Toast.Success("Patient saved.");

// With title
Toast.Success("Patient saved.", new() { Title = "Success" });

// With action button
Toast.Info("Draft saved.", new() {
    Action = new() { Label = "Open draft", OnClick = OpenDraft }
});

// Persistent (no auto-dismiss)
Toast.Error("Connection lost. Reconnecting...", new() { Persistent = true });

// Promise-style
var result = await Toast.PromiseAsync(
    PatientService.SaveAsync(_patient),
    new() {
        LoadingMessage = "Saving patient...",
        SuccessMessage = "Patient saved successfully.",
        ErrorMessage = e => $"Save failed: {e.Message}"
    });

// Programmatic dismiss
await Toast.DismissAsync("my-toast-id");
```

---

## 4. Enums (LipiToastTypes.cs)

```csharp
public enum ToastSeverity
{
    Success,
    Info,
    Warning,
    Error
}

public enum ToastPosition
{
    TopRight,      // default
    TopCenter,
    TopLeft,
    BottomRight,
    BottomCenter,
    BottomLeft
}
```

---

## 5. Position

**Default: TopRight.** Configurable per clinic via Settings > Clinic > Toast position.

**Per-clinic config (not per-user):**
- Toasts are functional UX, not personal preference
- Per-user adds complexity (every login restores position)
- Clinic consistency helps when staff move between desks

**Per-toast override:** `options.Position` overrides clinic default for special cases.

**Why TopRight default:**
- Doesn't conflict with patient banner (top center)
- Doesn't overlap with BottomNav (bottom)
- TopNav user dropdown is on top-right corner — toasts appear below it with margin

---

## 6. Stacking + queue behavior

- **Stack vertically**, newest at top of stack, oldest at bottom
- **Max 4 visible at once** (configurable per clinic)
- **Beyond 4**, toasts queue silently — when one dismisses, next appears
- **8px gap** between stacked toasts
- **Dedup:** If `options.Id` matches existing toast, update existing (don't stack new)
- **Slide animation:** newly stacked toasts push older toasts down with animation

---

## 7. Auto-dismiss durations per severity

| Severity | Default duration | Reason |
|---|---|---|
| Success | 3000ms | Brief confirmation |
| Info | 5000ms | Read once, dismiss |
| Warning | 7000ms | Needs attention, slightly longer |
| Error | **0ms (persistent)** | Errors NEVER auto-dismiss — must be acknowledged |

**Clinical safety call:** Error toasts being persistent is non-negotiable.
Auto-dismissing an error is dangerous in HIS — user might glance away,
miss it, and not know that the save failed.

Override via `options.DurationMs`. Set to 0 (or set `Persistent = true`) for persistent.

---

## 8. Visual design

### Single toast

```
┌─────────────────────────────────────────────┐
│ ✓  Patient saved successfully.        ×    │  Success — green
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│ ⚠  Could not connect to server.       ×    │  Error — red, persistent
│    [Retry]                                  │  with action button
└─────────────────────────────────────────────┘
```

### Specs

- **Width:** 360px (standard); 280px (compact); 480px (wide)
- **Min height:** 56px (single line); auto-grows for title + message + action
- **Background:** `var(--lipi-toast-bg)` — white (light) / dark gray (dark mode)
- **Border:** 1px solid `var(--lipi-toast-border)`
- **Colored left bar:** 4px wide, severity color
- **Icon:** 20px, severity-colored, left side
- **Title:** 13px semibold (when present)
- **Message:** 12px regular
- **Dismiss button:** 16px X, top-right corner
- **Shadow:** `var(--lipi-toast-shadow)` — soft drop shadow
- **Border-radius:** `var(--r-md)` (8px)

### CSS

```css
.lipi-toast-host {
    position: fixed;
    z-index: 3500;  /* above CDS toast (3000), below modals */
    display: flex;
    flex-direction: column;
    gap: 8px;
    pointer-events: none;  /* host doesn't intercept clicks */
    padding: 16px;
}

.lipi-toast-host-top-right    { top: 0; right: 0; }
.lipi-toast-host-top-center   { top: 0; left: 50%; transform: translateX(-50%); }
.lipi-toast-host-top-left     { top: 0; left: 0; }
.lipi-toast-host-bottom-right { bottom: 0; right: 0; flex-direction: column-reverse; }
/* etc. */

.lipi-toast {
    width: 360px;
    min-height: 56px;
    background: var(--lipi-toast-bg);
    border: 1px solid var(--lipi-toast-border);
    border-left-width: 4px;
    border-radius: var(--r-md);
    box-shadow: var(--lipi-toast-shadow);
    padding: 12px 14px;
    display: flex;
    gap: 12px;
    align-items: flex-start;
    pointer-events: auto;  /* toasts themselves are clickable */
    
    animation: lipi-toast-enter 200ms ease-out;
}

.lipi-toast-success { border-left-color: var(--color-success-fill); }
.lipi-toast-info    { border-left-color: var(--color-info-fill); }
.lipi-toast-warning { border-left-color: var(--color-warning-fill); }
.lipi-toast-error   { border-left-color: var(--color-danger-fill); }

.lipi-toast-icon {
    flex-shrink: 0;
    width: 20px;
    height: 20px;
}

.lipi-toast-success .lipi-toast-icon { color: var(--color-success-text-strong); }
.lipi-toast-info    .lipi-toast-icon { color: var(--color-info-text-strong); }
.lipi-toast-warning .lipi-toast-icon { color: var(--color-warning-text-strong); }
.lipi-toast-error   .lipi-toast-icon { color: var(--color-danger-text-strong); }

.lipi-toast-content {
    flex: 1;
    min-width: 0;
}

.lipi-toast-title {
    font-size: 13px;
    font-weight: 600;
    color: var(--color-text-primary);
    margin-bottom: 2px;
}

.lipi-toast-message {
    font-size: 12px;
    color: var(--color-text-secondary);
    line-height: 1.4;
}

.lipi-toast-action {
    margin-top: 8px;
    font-size: 12px;
    font-weight: 500;
    padding: 4px 10px;
    border: 1px solid currentColor;
    border-radius: var(--r-sm);
    background: transparent;
    cursor: pointer;
    max-width: 100%;
}

.lipi-toast-dismiss {
    flex-shrink: 0;
    width: 16px;
    height: 16px;
    color: var(--color-text-muted);
    cursor: pointer;
}

.lipi-toast-dismiss:hover {
    color: var(--color-text-primary);
}
```

---

## 9. Animation

**Appear:**
- Slide in from outer edge (right edge for top-right; left edge for top-left; etc.)
- Fade in simultaneously
- 200ms ease-out

**Dismiss:**
- Slide out same direction
- Fade out
- 150ms ease-in

**Stack reflow:**
- When a toast dismisses, remaining toasts smoothly translate to fill the gap
- 200ms ease-out

**Reduced motion:**
- Respect `prefers-reduced-motion: reduce`
- Fall back to fade-only (no slide)
- Stack reflow disabled

```css
@keyframes lipi-toast-enter {
    from { transform: translateX(100%); opacity: 0; }
    to   { transform: translateX(0); opacity: 1; }
}

@media (prefers-reduced-motion: reduce) {
    .lipi-toast {
        animation: lipi-toast-fade-only 150ms ease-out;
    }
}

@keyframes lipi-toast-fade-only {
    from { opacity: 0; }
    to   { opacity: 1; }
}
```

---

## 10. Promise-style API behavior

```csharp
var result = await Toast.PromiseAsync(
    PatientService.SaveAsync(_patient),
    new() {
        LoadingMessage = "Saving patient...",
        SuccessMessage = "Patient saved successfully.",
        ErrorMessage = e => $"Save failed: {e.Message}"
    });
```

**Behavior:**

1. **Loading toast appears** — spinner icon + LoadingMessage, persistent (no auto-dismiss)
2. **While Task is pending** — toast persists
3. **On success:**
   - Toast morphs in place (same Id) to Success
   - Icon swap: spinner → checkmark (animated)
   - Message swap: LoadingMessage → SuccessMessage
   - Auto-dismisses after 3s (Success default)
4. **On exception:**
   - Toast morphs to Error
   - Icon swap: spinner → X (animated)
   - Message swap: LoadingMessage → ErrorMessage(exception)
   - Persistent (no auto-dismiss — manual dismiss only)

**Same Id throughout** — single toast lifecycle, no stacking.

This is the Sonner library pattern, productive for async-heavy code.

---

## 11. Action button

A toast can have **ONE** action button (e.g., "Undo", "Retry", "Open draft").

```csharp
Toast.Success("Patient deleted.", new() {
    Action = new() { 
        Label = "Undo", 
        OnClick = async () => await UndoDelete(patientId)
    }
});
```

**Behavior:**
- Single action only (no multiple buttons — toast isn't a dialog)
- Button label max 12 chars (any longer → ellipsis)
- Clicking action invokes OnClick AND dismisses the toast
- Action button positioned below message (own row) OR inline right (if message is short)

---

## 12. Accessibility

- Wrapper has `role="status"` (success/info/warning) or `role="alert"` (error)
- `aria-live="polite"` for success/info/warning
- `aria-live="assertive"` for error (interrupts current speech)
- **Toast does NOT steal focus** from user's current input — this is critical
- Dismiss button has `aria-label="Dismiss notification"`
- Action button (if present) is keyboard-tabbable
- After dismissing via keyboard, focus returns to wherever it was

**The "don't steal focus" rule is the most important.** Toasts are informational —
they should not interrupt typing/clicking flow. Notification appears, user
notices peripherally, continues working.

---

## 13. LipiToastHost placement

`<LipiToastHost />` lives in `TopNavLayout.razor`, just after `<LipiOverlayHost />`:

```razor
@* TopNavLayout.razor *@
<div class="topnav-layout">
    <TopNav />
    <PatientBanner />
    <DynamicTabStrip />
    <main>@Body</main>
    <BottomNav />
    
    <LipiOverlayHost />   @* modals + drawers *@
    <LipiToastHost />     @* toasts *@
</div>
```

Single host instance. Service dispatches to host. Host renders toasts.

---

## 14. DI registration

```csharp
// In Program.cs
builder.Services.AddScoped<ILipiToastService, LipiToastService>();
```

Scoped (per Blazor Server circuit). Each user session has its own service instance
and its own toast queue.

---

## 15. Z-index

```
3500  LipiToastHost (toasts)
3000  CDS critical lab toast (Phase 7+, similar position)
800   LipiModal stack
700   LipiDrawer
```

Toasts appear ABOVE modals visually — modal opens, toast still visible.
Toasts are informational and shouldn't be blocked by modal interactions.

(Critical lab toasts and break-glass banners are higher priority overlays
with their own z-index from Layout Architecture spec.)

---

## 16. StyleGuide section

Add `#toasts` section with:
1. All 4 severities (manual trigger buttons)
2. With and without title
3. With action button
4. Persistent (no auto-dismiss)
5. Promise-style demo (3 buttons: success path, error path, custom messages)
6. Position picker — show toasts in all 6 positions
7. Stack overflow demo — trigger 6 toasts rapidly (4 visible + 2 queued)
8. Dismiss all button

---

*Most complex Phase 2.7 component. Uses LucideIcon, LipiSpinner. Service-driven, host-based, queue-managed.*
