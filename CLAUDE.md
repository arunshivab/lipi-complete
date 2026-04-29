# LiPi HIS — Claude Project Context

> This file is read by Claude at the start of every session to restore full project context.
> Keep it updated as the project evolves.

---

## Developer

- Default .NET version: **.NET 10** — always use .NET 10 APIs. Never suggest .NET 7/8/9 patterns.
- IDE: Visual Studio (Windows)
- DB client: pgAdmin 4
- Shell: PowerShell (`dotnet run` from project directory)

---

## Project: LiPi HIS

A comprehensive Hospital Information System (HIS) modelled after Varian ARIA, built for Indian cancer hospitals. Brand name: **LiPi** by Armoki.

### Stack
- **Frontend**: Blazor Web App (.NET 10), `InteractiveServer` render mode only — no WASM
- **Database**: PostgreSQL via EF Core 10 + Npgsql
- **Auth**: Cookie-based, Argon2id password hashing
- **Font**: DM Sans (UI), DM Mono (data/numbers)
- **Design**: Navy (`#0B2545`) + Gold (`#C49A22`) brand, white cards on soft blue-gray bg

### Solution structure
```
lipi-complete/
├── src/
│   └── LiPi.Web/                  ← Blazor Web App (entry point)
│       ├── Program.cs
│       ├── App.razor
│       ├── Pages/Admin/
│       │   ├── UsersNew.razor     ← Staff user registration
│       │   ├── ClinicsNew.razor   ← Clinic creation
│       │   └── OrgsNew.razor      ← Organisation registration
│       ├── Services/
│       │   ├── AdminData.cs       ← All UI lookup data (no hardcoding in pages)
│       │   ├── ClinicSeeder.cs    ← Seeds default roles + admin users
│       │   └── AuthService.cs
│       └── wwwroot/css/
│           └── admin.css          ← Design tokens (--navy, --cobalt, --bg, --sh-sm/md/lg)
└── database/efcore/
    ├── LiPi.Clinic.Identity/      ← Identity schema (users, roles, sessions, MFA)
    ├── LiPi.Master/               ← Master DB (orgs, clinics)
    └── LiPi.Clinic.*/             ← Other schema projects
```

---

## Multi-Tenancy Architecture

- **Completely separate PostgreSQL database per clinic** — zero shared tables between clinics
- Each clinic DB has its own `identity` schema with users, roles, sessions
- Master DB (`lipi_master`) tracks organisations and clinics only
- Connection strings in `appsettings.json`: `IdentityConnection`, `MasterConnection`

---

## Admin Hierarchy (undeletable system users)

| Level | Username | Password | Default Access |
|---|---|---|---|
| **Global Admin** | `Admin` | `Admin@123` | All databases, all clinics — unrestricted |
| **Sys Admin** | `SysAdmin` | `SysAdmin` | All databases by default; Global Admin can restrict |
| **Site Admin** | `SiteAdmin` | `SiteAdmin` | Assigned clinic(s) only; cannot create clinics |

- All three seeded with `must_change_password = true`
- **None can be deleted** — enforced at application + DB level
- Only GlobalAdmin can promote to SysAdmin
- GlobalAdmin + SysAdmin can manage SiteAdmin assignments

---

## Compliance Requirements (non-negotiable on every feature)

### HIPAA
- No PHI in logs — use IDs only, never names/DOB/diagnosis in log messages
- Audit log required for every read/write of PHI (who, when, what, from where)
- MFA enforced for all clinical staff accessing PHI
- Session: 30 min timeout, non-sliding, HttpOnly + SameSite=Strict cookies
- Password: 8+ chars, upper+lower+digit+symbol, 90-day expiry policy
- Account lockout: 5 failed attempts → 30 min lock
- PHI field-level encryption (AES-256-GCM) for patient records
- Emergency "break-glass" access with mandatory reason codes + audit
- Data retention: audit logs 6 years, patient records 6 years from last encounter
- HTTPS enforced in production (`UseHttpsRedirection` + HSTS)

### Six Sigma
- Every dashboard must include Six Sigma quality metrics
- Track defect rates per process (registrations, billing, treatment plans)
- DPMO (Defects Per Million Opportunities) visible to Directors and above
- Control charts, run charts, and process capability (Cpk) on clinical workflows
- Sigma level displayed on relevant module dashboards

---

## Critical Bugs Already Fixed — Do Not Revert

| Issue | Fix |
|---|---|
| `ExtensionData` column not found | `User.ExtensionData` changed from `Dictionary<string,object>` to `string`; serialized manually with `System.Text.Json`; `IdentityDbContext` has explicit `.HasColumnName("extension_data").HasColumnType("jsonb")` |
| Blazor events dead (clicks/input not working) | `AddServerSideBlazor()` removed from `Program.cs` — it is a .NET 7 legacy API that conflicts with .NET 10's `AddRazorComponents().AddInteractiveServerComponents()` |
| Auth state crash on prerender | `<CascadingAuthenticationState>` in `App.razor` must stay — needed for SSR prerender. `AddCascadingAuthenticationState()` in Program.cs handles interactive circuits. Both coexist by design. |
| `Dictionary<string,object>` jsonb error | Npgsql 8+ requires `EnableDynamicJson()` opt-in for `Dictionary<string,object>`. Solution: use `string` instead. |
| Duplicate `onchange` compiler error | Never combine `@bind` + `@bind:event="onchange"` + `@onchange` on the same element. Use one or the other. |
| Escaped quotes in Razor lambdas | Never use `\"` inside `@onchange="..."` string attributes. Use named methods instead. |

---

## Blazor Debugging Protocol

When something is visually broken or non-interactive, always collect **both**:

1. **PowerShell terminal** → server-side errors (build failures, DB exceptions, startup crashes)
2. **Browser F12 → Console tab** → client-side errors (WebSocket failures, circuit errors, JS exceptions)

The browser console is the primary signal for all Blazor interactivity issues.

---

## Design System

- CSS prefix: `uf-` for user form, `tn-` for topnav, `al-` for admin layout
- Card accent: coloured left border (4px) per section, numbered coloured circle badges
- Section colours: 1=`#1565C0` blue, 2=`#6A1B9A` purple, 3=`#00695C` teal, 4=`#BF360C` orange, 5=`#1A237E` navy, 6=`#006064` cyan-dark, 7=`#E65100` amber
- Bottom dock: circular module buttons with per-module coloured border, 3 groups separated by dividers
- All lists from `AdminData.cs` — zero hardcoded values in pages

### Module colours (dock)
| Group | Module | Colour |
|---|---|---|
| Clinical | OP, IP, RO, MO, Sx, CV, OT | #4A9BD4, #3b82f6, #a855f7, #f97316, #ef4444, #0ea5e9, #f43f5e |
| Diagnostics | Ph, Lab, Rad, NM, Dia, Den | #22c55e, #eab308, #67e8f9, #f59e0b, #06b6d4, #10b981 |
| Services | Nu, CS, Ad | #ec4899, #6366f1, gold |

---

## Module Build Order

- [x] User Registration (`/admin/users/new`)
- [ ] Clinic Creation (`/admin/clinics/new`) ← **next**
- [ ] Group/Organisation Registration (`/admin/orgs/new`)
- [ ] Patient Registration
- [ ] Audit Log infrastructure (wire in before patient data)

---

## External API Status

| API | Status |
|---|---|
| NMC India (doctor verification) | No public API. Manual: `nmr.nmc.org.in`. Paid: Surepass NMC API |
| AERB (radiation protection licence) | No public API. Manual: `elora.aerb.gov.in` |
| India Post PIN lookup | Available — integrate for patient address auto-fill |
| GSTIN verification | Available — integrate for clinic registration |
| ABDM/ABHA | Planned |
| DigiLocker | Planned |

---

## UX Rules (mandatory on every page)

### Confirmation dialogs
**Every destructive or irreversible action MUST show a confirmation dialog before executing.**
This includes: Delete, Lock, Suspend, Reset Password, Reactivate, any status change, any role removal.
- Dialog must name the user/record being affected
- Dialog must describe exactly what will happen
- Cancel must be the default / prominent button
- Destructive action button must be red
- No action executes on single click alone — always two steps minimum

### Edit interactions
Inline panel — edit form slides open inside the table row, pushing rows below it down.
Used for: Edit contact (email+phone), Manage roles.
NOT used for: Confirmation dialogs (those are always modal overlays).

### General
- Success messages appear near the action button, not at the top of the page
- Validation errors auto-scroll and focus the first errored field
- All admin actions are HIPAA audit-logged (who, what, when, from where)

---

## Security & Compliance Architecture (already built — do not replace or duplicate)

These are production-grade features already implemented in the codebase.
Always build ON TOP of these — never suggest alternatives.

### Audit Infrastructure (`LiPi.Clinic.Audit`)
- **Hash-chained audit log** — every `AuditEvent` has `PreviousHash` + `CurrentHash`. Records are cryptographically linked. Tampering with any record breaks the chain and is detectable instantly.
- **Blockchain anchoring** — `BlockchainAnchor` stores Merkle roots anchored to an external ledger for independent tamper-proof verification.
- **Separate PHI access log** (`PhiAccessLog`) — every read of patient data logged independently from general audit, with minimum-necessary-principle tracking and consent references.
- **Export log** (`ExportLog`) — every data export tracked with SHA-256 file hash, destination, record count, DLP scan result.
- **Print log** (`PrintLog`) — every print tracked with watermark confirmation.
- **Hash chain verification runs** — automated integrity checks that scan the chain and report any breaks.
- `AuditService` — injectable service (`IAuditService`) that writes to `audit.audit_events`. Always use this for every user-facing action. Never write audit events directly.
- Standard action codes in `AuditActions` static class — use these constants, never raw strings.

### Identity & Auth (`LiPi.Clinic.Identity`)
- **Argon2id password hashing** — via `Isopoh.Cryptography.Argon2`. Never use any other hashing algorithm.
- **Cryptographic session tracking** — `Session` table with JWT JTI, issued/expires/revoked timestamps, IP, user agent.
- **MFA infrastructure** — `MfaMethod` table supporting TOTP, WebAuthn, SMS OTP, email OTP, backup codes.
- **Login attempt tracking** — `LoginAttempt` table with outcome codes, IP, user agent. Auto-lock after 5 failures.
- **Service accounts + API keys** — `ServiceAccount` and `ApiKey` tables for integration auth.
- **AD/LDAP sync tracking** — `AdSyncRun` table for enterprise directory integration.

### Multi-tenancy
- **Per-clinic isolated PostgreSQL database** — each clinic's identity, clinical, and audit data is completely isolated.
- **Soft delete everywhere** — `DeletedAt` timestamp. No hard deletes. Ever. Records retained per HIPAA.
- System users (`Admin`, `SysAdmin`, `SiteAdmin`) are undeletable and unseizable at the application layer.

### Data safety patterns
- `ExtensionData` fields are `string` (not `Dictionary<string,object>`) — serialized with `System.Text.Json`. Npgsql 8+ requires explicit opt-in for dynamic JSON; we avoid it.
- `IPAddress` fields use Npgsql's native `inet` mapping — no `HasConversion`. Removed in .NET 10 upgrade.
- `AuditEvent.BeforeState` and `AfterState` are `string?` — serialized JSON snapshots of the full object before and after every change.

---

## Typography System (enforced across entire app)

### Fonts — exactly two, no others

| Font | Usage | Never use for |
|---|---|---|
| **DM Sans** | ALL UI text — headings, labels, body, buttons, nav, badges, form fields | Data values, numbers, IDs |
| **DM Mono** | ONLY: IDs, timestamps, dates, codes, clinical data values | Labels, buttons, prose, any non-data text |

Google Fonts import (already in App.razor):
```html
<link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&family=DM+Mono:wght@400;500&display=swap" rel="stylesheet"/>
```

### Type scale — use CSS variables, never hardcode font sizes

| Variable | Size | Weight | Usage |
|---|---|---|---|
| `--ts-page-title` | 18px | 600 | Page H1 — "User Management", "Register Staff User" |
| `--ts-section` | 15px | 600 | Card/section titles, panel titles |
| `--ts-body` | 13px | 400 | General body text, table data, form values |
| `--ts-label` | 12px | 500 | Form field labels, nav items, button text |
| `--ts-small` | 11px | 400 | Helper text, email addresses, secondary info |
| `--ts-th` | 10px | 600 | Table column headers — UPPERCASE + letter-spacing 0.4px |
| `--ts-badge` | 10px | 500 | Badges, pills, role tags, status chips |
| `--ts-micro` | 9px | 400 | Captions, footnotes, version strings, system tags |
| `--ts-mono-data` | 12px | 400 | DM Mono: IDs, dates, timestamps |
| `--ts-mono-small` | 11px | 400 | DM Mono: small codes, inline data |

### Key rules
- Table `<th>`: always 10px / 600 / uppercase / tracking 0.4px / DM Sans
- Table `<td>` with data (ID, date, IP): DM Mono 12px via `.mono` class
- Table `<td>` with text (username, email): DM Sans 13px
- Status strips / colour patches: pure CSS background colour on the `<td>` itself — never use a `<span>` inside for this
- Buttons: DM Sans 12px / 500 always — never DM Mono
- Role/status badges: DM Sans 10px / 500

### Status colours (left-strip on table rows)
| Status | Colour |
|---|---|
| active | `#4CAF50` green |
| locked | `#F44336` red |
| suspended | `#FF9800` orange |
| invited | `#2196F3` blue |
| terminated | `#9E9E9E` grey |

---

## Razor / Blazor Coding Rules (MUST follow on every page — prevents recurring build errors)

### Rule 1: Never put string literals inside double-quoted Razor event attributes

This is the #1 recurring build error. Razor parses `@onX="..."` as C# code inside double quotes.
Any inner double quote — string literals, regex, ternary defaults — breaks the parser.

**BROKEN — causes CS1525 / RZ9986 every time:**
```razor
@onblur="() => Touch("name")"                          ❌ string inside quotes
@onchange="e => m.X = e.Value?.ToString() ?? "IN""     ❌ default string inside quotes
@oninput="e => Regex.Replace(e.Value, @"\D", "")"      ❌ regex + empty string inside quotes
@onchange="e => { m.X = e.Value; if(IsTouched("f")) Validate("f"); }"  ❌ string args inside
```

**CORRECT — always use named methods:**
```razor
@onblur="OnBlurName"       ✅
@onchange="OnXChange"      ✅
@oninput="OnXInput"        ✅

// Then in @code:
private void OnBlurName()      => Touch("name");
private void OnXChange(ChangeEventArgs e) => m.X = e.Value?.ToString() ?? "IN";
private void OnXInput(ChangeEventArgs e)
{
    m.X = Regex.Replace(e.Value?.ToString() ?? string.Empty, @"\D", string.Empty);
}
```

**The rule:** Every `@onX` attribute value must be either:
- A simple method reference: `@onclick="MethodName"`
- A simple lambda with NO string literals: `@onclick="() => count++"`
- A lambda calling a method with NO string args: `@onclick="() => HandleClick()"`

If the lambda body contains ANY of: `"string"`, `?? "default"`, `@"\D"`, `Touch("x")`, `Validate("x")`, `IsTouched("x")` — extract it to a named method.

### Rule 2: Never mix `@bind` + `@bind:event` + `@onchange` on the same element

### Rule 3: `IJSRuntime` requires `@using Microsoft.JSInterop`
### Rule 3 (expanded): Every service inject needs its using directive
- `@inject IJSRuntime JS` → requires `@using Microsoft.JSInterop`
- `@inject NavigationManager Nav` → requires `@using Microsoft.AspNetCore.Components`
- `@inject IHttpClientFactory` → requires `@using System.Net.Http`
- Always add the `@using` on the line ABOVE the `@inject` in the same file


### Rule 4: Static `readonly` fields used in Razor markup must be defined in the same component's `@code` block

e.g. `ClinicalGroups`, `SystemUsernames` — if referenced in the HTML section, they must be in `@code`, not just in a service class.

### Rule 5: Layout components (TopNavLayout) are SSR — `@onclick` does NOT fire in them

Extract any interactive behaviour (dropdowns, toggles) into a **child component** with `@rendermode InteractiveServer`. The child component's CSS must be in `admin.css`, not in a `<style>` block inside the component (scoped styles don't inject into `<head>` from InteractiveServer components).

### Rule 6: `position: absolute` dropdown cards get clipped by parent layout

Use `position: fixed; top: 58px; right: 16px` for dropdowns. This escapes all parent overflow constraints.

### Rule 7: NavigateTo in Blazor Server — `<a href> + @onclick="Close"` race condition

When `@onclick` triggers a state change that removes the element, the browser never follows the `href`.
Use `NavigationManager.NavigateTo(url)` in a named method instead.


### ⛔ Rule 10 (CRITICAL — second most recurring issue): Never use double quotes inside Razor attribute strings

Razor attributes use double quotes. Nesting double quotes inside them breaks the parser silently with cryptic errors.

**WRONG — breaks build:**
```razor
@oninput="e => { name = e.Value?.ToString() ?? ""; }"
class="myclass @(hasError ? "err" : "")"
@onclick="() => SetTab("contact")"
```

**CORRECT — always use these alternatives:**
```razor
@oninput="e => { name = e.Value?.ToString() ?? string.Empty; }"
class="myclass @(hasError ? "err" : string.Empty)"
@onclick='() => SetTab("contact")'
```

**Rules:**
- Inside `@oninput`, `@onclick`, `@onchange` etc: replace `?? ""` → `?? string.Empty`
- Inside `class="... @(...)"`: replace `""` → `string.Empty`
- For string literals in `@onclick`: use single quotes `'() => SetTab("contact")'`
- For `@bind-Value` and similar: always use single-quoted lambdas when string args needed
- **Never use `""` anywhere inside a double-quoted Razor HTML attribute**


**ADDITIONAL — Never use assignment lambdas in @onclick attributes:**
```razor
@* WRONG — the > in => confuses Razor parser *@
@onclick="@(() => _field = value)"

@* CORRECT — use a named method *@
@onclick="SetField"

private void SetField() => _field = value;
```
This causes mysterious "unclosed div" and "no matching start tag" errors
that have nothing to do with div structure.

**The symptom:** CS1525, CS1002, CS1513 errors pointing at a `)` or `}` — always check for nested `""` first.

### ⛔ Rule 9 (CRITICAL — most recurring issue): EVERY page with @onclick, @oninput, @onchange MUST have @rendermode InteractiveServer

This is the #1 silent failure in Blazor Server. Without it, the page renders as static HTML —
buttons do nothing, inputs don't respond, no error is shown anywhere.

**MANDATORY on every new page that has ANY of these:**
- `@onclick`
- `@oninput`
- `@onchange`
- `@onblur`
- `@onkeydown`
- `@bind`
- Any interactive element whatsoever

**Pattern — always put it after @layout and @attribute:**
```razor
@layout LiPi.Web.Components.Layouts.TopNavLayout
@attribute [Authorize]
@rendermode InteractiveServer   ← NEVER forget this
```

**Pages without interactive elements** (pure SSR display pages) don't need it — but when in doubt, add it.

**The symptom:** Button clicks do nothing. No error in console or server. Page looks fine but is dead.
**The fix:** Add `@rendermode InteractiveServer`.

This has been the most recurring build/runtime issue in the entire project. Check for it on EVERY new page before writing any other code.

### Rule 8: `IDbContextFactory` only — never `AddDbContext` + `AddDbContextFactory` for same context

In .NET 10, `AddDbContext` registers `DbContextOptions` as Scoped. `AddDbContextFactory` is Singleton.
A singleton cannot consume scoped services — DI validation crashes at startup.
Use only `AddDbContextFactory<T>()` and inject `IDbContextFactory<T>` everywhere.

---

## Deploy Script Rule (deploy-downloads.ps1)

**Every new file I create MUST be added to `deploy-downloads.ps1` immediately — in the same response.**

If a new `.razor`, `.cs`, `.js`, or `.css` file is created and not added to the deploy map,
it will NEVER reach the user's project. The deploy script only copies files explicitly listed.

### When adding a new file, always:
1. Create the file
2. In the same response, add it to `deploy-downloads.ps1` with the correct destination path
3. Present both files together for download

### Path patterns:
- Blazor pages:      `"src\LiPi.Web\Pages\Admin\FileName.razor"`
- Components:        `"src\LiPi.Web\Components\FileName.razor"`
- Services:          `"src\LiPi.Web\Services\FileName.cs"`
- CSS/JS:            `"src\LiPi.Web\wwwroot\css\FileName.css"`
- DB entities:       `"database\efcore\LiPi.Clinic.X\Entities\FileName.cs"`
- Layout:            `"src\LiPi.Web\Components\Layouts\FileName.razor"`
- Pages subfolder:   `"src\LiPi.Web\Pages\Patients\FileName.razor"`

### The UserDropdown lesson:
`UserDropdown.razor` was created across multiple sessions but never added to the deploy script.
Every debugging session (CSS fixes, z-index, navigation) was wasted effort — the component
simply didn't exist in the project. One missing deploy script entry = hours of lost time.

---

## Validation Rules (apply to ALL forms — current and future)

### UX Rules (every form)
1. Success message appears **near the submit button**, not at top of page
2. On submit failure → auto-scroll and focus the **first errored field**
3. Fields validate **live on blur**, not just on submit
4. **Cancel → Close** after successful save (button text changes)
5. All saves **HIPAA audit-logged** (who, what, when, from where)
6. **Confirmation dialog mandatory** before every destructive action

### Name fields (all forms)
7. Letters, spaces, hyphens and dots **only** — digits blocked immediately on input with live error
8. Middle name is **optional** but same letter-only rule applies
9. Display name **auto-populated** from title + first + last; editable by user

### Date of Birth
9b. **Title field**: on all staff/user forms — dropdown: Dr., Mr., Ms., Prof., Sr., Br., Rev., Other
9c. **Display name**: auto-populated as `Title + FirstName + LastName` on every keystroke. User can override manually. If overridden, stop auto-populating until reset.

10. **Users**: required; must be at least 16 years old — max DOB = `DateTime.Today.AddYears(-16).AddDays(-1).ToString("yyyy-MM-dd")` (less than 16 by yesterday)
11. **Patients**: required; cannot be a future date; no minimum age

### Phone fields (all forms)
12. **Digits only** — letters/spaces stripped silently on input
13. **India (+91) mobile**: exactly 10 digits, must start with 6, 7, 8 or 9
14. **Other country mobile**: 7–15 digits
15. **India landline**: 6–11 digits (optional)
16. **WhatsApp**: same rules as mobile

### Email
17. Required on user/staff forms; valid format `name@domain.ext`
18. Optional on clinic/org forms; if entered must be valid

### Address block (ALL forms — use AddressBlock.razor component)
19. **PIN code is the primary entry point** (6 digits)
20. On 6 digits → call `api.postalpincode.in/pincode/{pin}` → auto-fill District + State
21. Show "Looking up…" indicator while fetching
22. If API fails → allow manual entry; show friendly error
23. State and District become read-only after auto-fill; show edit icon to override
24. Clearing PIN → clears City and State
25. Overriding City → clears PIN
26. Overriding State → clears City and PIN
27. **Always use `AddressBlock.razor` component** — never repeat this logic inline

### User / Staff Registration
28. **Username**: required, min 3 chars, lowercase + digits + dots + underscores + hyphens only
29. **Password**: min 8 chars, must have uppercase + lowercase + digit + symbol
30. **Confirm password**: must match exactly
31. **Designation**: required
32. **Staff type**: required
33. **Clinic**: required; if none exists show "create one first" link
34. **At least one role or admin level**: required
35. Clinical role groups (clin / nur / allied / tech) are **mutually exclusive**

### Clinic Registration
36. **Clinic name**: required
37. **Short code**: required, 2–8 chars, lowercase + digits only, **unique** in DB
38. **Clinic type**: required
39. **Organisation**: required
40. **Address, City, State**: via AddressBlock — required

### Organisation Registration
41. **Legal name**: required
42. **Organisation type**: required
43. **Contact name + phone**: required
44. **PAN**: optional; if entered — exactly 10 chars, format `AAAAA0000A`
45. **GSTIN**: optional; if entered — exactly 15 chars, format `22AAAAA0000A1Z5`
46. **CIN**: optional; if entered — exactly 21 chars

### Patient Registration
47. **First name, Last name**: required, letters only
48. **Gender**: required
49. **Date of Birth**: required, not future date
50. **Mobile**: required, same rules as #13/#14
51. **ABHA number**: optional; if entered — exactly 14 digits (numeric only)
52. **Aadhaar — Option A (XML upload)**:
    - Patient uploads ZIP from uidai.gov.in + enters 4-digit share code
    - `AadhaarXmlService` decrypts ZIP (SharpZipLib) → parses XML → auto-fills: Name, DOB, Gender, Address, Photo
    - **Consent checkbox MANDATORY** before upload ("I consent to share my Aadhaar demographics")
    - Full Aadhaar number is NEVER stored anywhere
53. **Aadhaar — Option B (no XML)**: store last 4 digits only (displayed as `XXXX-XXXX-XXXX-1234`)
54. **Emergency contact phone**: optional, digits only

### ABDM (future — not implemented yet)
- ABHA verification + health record linking via ABDM sandbox
- Patient consent mandatory before any ABDM data pull

---

## Form Accessibility Rules (every form, every page — always)

Every `<input>`, `<select>`, `<textarea>` MUST have both `id` and `name` attributes.
Every `<label>` MUST have a `for` attribute matching its input's `id`.

**PATTERN — always pair label + input with matching id/for:**
```razor
<div class="xx-fg">
  <label class="xx-lbl" for="inp-username">Username</label>
  <input id="inp-username" name="username" class="xx-inp" ... />
</div>
```

**Blazor components also need id:**
```razor
<InputText    id="txt-email"    name="email"    class="xx-inp" @bind-Value="m.Email" />
<InputSelect  id="sel-gender"   name="gender"   class="xx-sel" @bind-Value="m.Gender">
<InputCheckbox id="chk-vip"     name="vip"      @bind-Value="m.VipFlag" />
```

**Rule:** When writing any new form field, write the `id`/`name`/`for` in the SAME line as the element.
Never write a `<label>` or `<input>` without these attributes. No exceptions.

---

## Timezone Strategy (CRITICAL — affects all clinical data and TAT reports)

### Rule: Save UTC, Display Local

**ALWAYS save timestamps in UTC. ALWAYS display in the clinic's local timezone.**

This is non-negotiable for:
- HIPAA audit logs (must be immutable UTC)
- TAT (Turn-Around Time) reports (require consistent UTC baseline)
- Multi-clinic deployments across timezones (IST, GST, EST, etc.)

### PostgreSQL / Npgsql rules:
- `timestamp with time zone` (timestamptz) → use `DateTimeOffset` or `DateTime` with UTC Kind
- Npgsql **requires offset = 0 (UTC)** when writing to timestamptz columns
- **Never save `DateTimeOffset.Now` or `DateTime.Now`** — always use `.UtcNow`
- When parsing user input (date pickers etc.) → always call `.ToUniversalTime()` before saving

```csharp
// ✅ CORRECT — always save UTC
CreatedAt  = DateTimeOffset.UtcNow;
GoLiveAt   = DateTimeOffset.TryParse(input, out var dt) ? dt.ToUniversalTime() : null;

// ❌ WRONG — local time has non-zero offset, Npgsql rejects it
CreatedAt  = DateTimeOffset.Now;        // IST = +5:30 → rejected
GoLiveAt   = DateTimeOffset.TryParse(input, out var dt) ? dt : null;  // parsed as IST
```

### DateTime vs DateTimeOffset — which to use:
- **`DateTimeOffset`** → preferred for all new fields (carries offset info explicitly)
- **`DateTime` with UTC Kind** → for legacy fields already typed as `DateTime`
  ```csharp
  field = DateTime.UtcNow;   // Kind = Utc → Npgsql accepts
  ```
- **Never use `DateTime.Now`** — Kind = Local, Npgsql rejects

### Display (UI layer):
```csharp
// Convert UTC → clinic timezone for display
var clinicTz   = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"); // from Clinic.Timezone
var localTime  = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, clinicTz);
// Format: localTime.ToString("dd MMM yyyy HH:mm") + " IST"
```

### Entity field type guide:
| EF Entity Field | C# Type | Save with |
|---|---|---|
| CreatedAt, UpdatedAt | `DateTimeOffset` | `DateTimeOffset.UtcNow` |
| DeletedAt, GoLiveAt | `DateTimeOffset?` | `DateTimeOffset.UtcNow` or `.ToUniversalTime()` |
| RevokedAt, LockedUntil | `DateTime?` (legacy) | `DateTime.UtcNow` |
| Session.IssuedAt, ExpiresAt | `DateTimeOffset` | `DateTimeOffset.UtcNow` |

### TAT Reports — how to calculate:
```csharp
// Correct TAT: always use UTC timestamps, convert for display only
var tatMinutes = (completedAt.UtcDateTime - startedAt.UtcDateTime).TotalMinutes;
// Never calculate TAT on local times — DST and timezone changes corrupt results
```

---

## Default Theme: Light

**LiPi HIS default theme is LIGHT mode.**

- All new pages, components, and CSS must default to light theme
- CSS variables must default to light values (white/light-grey backgrounds, dark text)
- Dark mode is a future feature — do NOT implement dark-first styles
- If writing CSS variables with fallbacks, always use light fallbacks:
  ```css
  background: var(--surface, #ffffff);   /* ✅ light fallback */
  color:      var(--tx, #0F2D5E);        /* ✅ dark text on light bg */

  background: var(--surface, #1a2540);   /* ❌ dark fallback — WRONG */
  ```
- `app.css` body background must be light (`#F8FAFC` or `#ffffff`), NOT dark gradient
- The navy topnav bar (`#0B2545`) stays dark always — that is brand, not theme

---

## CSS Version Bumping (MANDATORY when CSS changes)

Whenever any of these files are modified:
- `wwwroot/css/admin.css`
- `wwwroot/css/app.css`
- `wwwroot/css/dashboard.css`

**You MUST bump the `v=` version string in `App.razor`** to bust the browser cache.
Without this, Blazor SPA navigation will serve the old cached CSS to users.

```html
<!-- In App.razor — update the date/number when any CSS file changes -->
<link rel="stylesheet" href="css/app.css?v=20260424" />
<link rel="stylesheet" href="css/dashboard.css?v=20260424" />
<link rel="stylesheet" href="css/admin.css?v=20260424" />
```

**Rule:** Use today's date in `YYYYMMDD` format as the version string.
If multiple CSS changes happen on the same day, append a counter: `v=20260424b`, `v=20260424c`.

**Why this matters:** Blazor's SPA router does not reload `<head>` on navigation.
The browser caches CSS from the first page load and reuses it for all subsequent
navigations. Only a hard refresh (F5) bypasses the cache. The `?v=` query string
makes the browser treat each new version as a different URL, forcing a fresh download.

## Business Rules

### User Registration / Edit
- **Minimum age: 16 years** — Date of birth must be at least 16 years before today's date.
  - Valid: DOB ≤ today − 16 years
  - Error message: "Staff must be at least 16 years old"
  - Apply to both UsersNew (Register User) and UsersEdit

### Mandatory Fields — ALL Forms
The following fields are ALWAYS mandatory across every registration and edit form:
- **PIN Code** — 6-digit Indian PIN, triggers city/state auto-fill
- **Mobile Number** — required, India: 10 digits starting 6–9
- **City / District** — required, auto-filled from PIN lookup
- **State** — required, auto-filled from PIN lookup
- **First Name** — required, letters only
- **Last Name** — required, letters only
- **Date of Birth** — required, must be a valid past date, minimum 16 years old (staff rule)

Validation rules:
- Show red border + error message on blur (touch-on-blur pattern)
- All mandatory fields touched on Submit/Save click so errors show immediately
- PIN: `^\d{6}$`
- Mobile (India +91): `^[6-9]\d{9}$`
- DOB: not future, at least 16 years before today (`d > DateTime.Today.AddYears(-16)`)

---

## Multi-Tenant Connection String Architecture (Job #1 — Pending Implementation)

### Decision: Option B — Per-clinic connection string stored in Master DB

**Architecture:**
- `lipi_master` DB stores an encrypted connection string per clinic
- On login, app reads clinic's connection string from master, decrypts, connects
- All clinics can be on same server today, different servers tomorrow — zero code change
- Recommended hosting: Single Azure PostgreSQL Flexible Server (all clinics, one subscription under imagiqa)

**Master DB table: `clinic_connections`**
```sql
clinic_id         UUID  (FK → clinics.id)
connection_string TEXT  (AES-256-GCM encrypted)
server_region     TEXT  (e.g. 'azure-south-india', 'azure-west-india')
is_active         BOOL
created_at        TIMESTAMPTZ
updated_at        TIMESTAMPTZ
```

**Login flow:**
```
1. User enters username + password
2. App queries lipi_master → finds user's clinic memberships
3. If multiple clinics → show clinic picker
4. App reads encrypted connection string for chosen clinic
5. Decrypts with app-level key (stored in Azure Key Vault in production)
6. Creates IDbContextFactory pointing to that clinic's DB
7. Session cookie baked with clinicId + roles for THAT clinic only
8. All subsequent requests use clinic-scoped DB connection
```

**Encryption:** AES-256-GCM with key stored in environment variable / Azure Key Vault.
Never store plaintext connection strings in DB.

---

## Admin Hierarchy — Full Specification (Job #2 — Pending Implementation)

### Three-tier admin model

#### Global Admin
- **Count:** Maximum 2 (both equal in power)
- **Created by:** CLI bootstrap script (first one); second by first Global Admin
- **Powers:** Unrestricted access to everything — all clinics, all DBs, all admin functions
- **Restrictions:**
  - Neither Global Admin can delete, demote, or lock the other
  - Cannot be deleted by anyone
  - `is_primary = true` flag on the first one (display only, no extra powers)
- **Lockout recovery:** Other Global Admin unlocks; if BOTH locked → CLI reset script
- **Bootstrap:** One-time CLI command:
  ```bash
  dotnet run --project LiPi.Web -- --setup-global-admin
  ```
  - Checks no Global Admin exists yet
  - Creates with `must_change_password = true`
  - Disables itself after first run
  - Logs to console only (no DB audit yet at this stage)

#### Sys Admin
- **Who:** imagiqa IT team (IT Head, IT GM, IT Manager etc.)
- **Created by:** Global Admin only
- **Count:** Unlimited
- **Default access:** ALL clinics automatically on creation
- **Restrictions:**
  - Global Admin can restrict specific clinics (hidden from SysAdmin if restricted)
  - Cannot create Clinics or Organisations (Global Admin only)
  - Cannot demote or delete Global Admins
- **Auto-assignment:** When a NEW clinic is created → ALL existing SysAdmins automatically get access
- **Dual role possible:** A SysAdmin CAN be a staff member in a clinic (e.g. operational role)
  but this is governed by clinic-scoped sessions (see below)

#### Site Admin
- **Who:** Clinic-side admin (hospital's own IT/admin staff)
- **Created by:** Automatically when clinic is created (1 default); additional by Global Admin or SysAdmin
- **Count:** One default per clinic; more added by Global/SysAdmin
- **Scope:** Only the clinics where they ARE a SiteAdmin (not just a member)
- **Powers:**
  - Register users for their clinics
  - Manage roles for their clinics
  - Add existing users to their clinics (only clinics where they are SiteAdmin)
  - Cannot create or view Clinics or Organisations
  - Cannot deactivate users globally
- **Cross-clinic user addition:**
  - SiteAdmin can add a user to any clinic WHERE THE SITEADMIN IS ALSO A SITEADMIN
  - If user needs access beyond SiteAdmin's reach → must request SysAdmin
- **Can belong to multiple clinics** with different roles in each

#### Staff Users
- Can belong to multiple clinics
- Role is independent per clinic (e.g. Doctor in Mumbai, Radiographer in Chennai)
- Registered by SiteAdmin of their clinic, or Global/SysAdmin

---

### Clinic-Scoped Sessions (Security Architecture)

**Core principle:** Every session is locked to ONE clinic. Roles and permissions are
evaluated ONLY within that clinic's context.

**Session cookie payload:**
```json
{
  "userId":    "abc-123",
  "clinicId":  "clinic_chennai",
  "roles":     ["site_admin"],
  "isAdmin":   true,
  "adminLevel": "site_admin"
}
```

**Login flow for multi-clinic users:**
```
1. Enter username + password
2. System finds ALL clinics this user belongs to
3. If only one clinic → auto-select, proceed
4. If multiple clinics → show CLINIC PICKER screen
5. User selects clinic
6. Session created with roles FOR THAT CLINIC ONLY
7. All pages, route guards, and DB queries use this session's clinicId
```

**Security guarantee:**
- A user who is SiteAdmin in Chennai and Doctor in Mumbai gets:
  - Chennai session: `isAdmin=true, roles=[site_admin]` → admin pages accessible
  - Mumbai session: `isAdmin=false, roles=[doctor]` → admin pages rejected at route guard
- Even if user manually navigates to `/admin/users/new` in Mumbai session → server rejects (401)
- Different clinics = different DB connections = physically impossible to cross-contaminate
- No cache/cookie bleed possible since clinicId is baked into the session token

**Deactivation rules:**
| Action | Who can do it | Scope |
|---|---|---|
| Deactivate user | SiteAdmin | Only in clinics where SiteAdmin has membership |
| Deactivate globally | SysAdmin, Global Admin | All clinics simultaneously |
| Reactivate | Same as deactivate | Same scope |

---

### Permission Matrix

| Action | Global Admin | Sys Admin | Site Admin | Staff |
|---|---|---|---|---|
| Create Global Admin | ✅ | ❌ | ❌ | ❌ |
| Create Sys Admin | ✅ | ❌ | ❌ | ❌ |
| Create Site Admin | ✅ | ✅ | ❌ | ❌ |
| Create Staff User | ✅ | ✅ | ✅ (own clinics) | ❌ |
| Create Clinic | ✅ | ✅ | ❌ | ❌ |
| Create Organisation | ✅ | ✅ | ❌ | ❌ |
| View all Clinics | ✅ | ✅ (unless restricted) | ❌ | ❌ |
| View all Orgs | ✅ | ✅ (unless restricted) | ❌ | ❌ |
| Restrict SysAdmin clinic | ✅ | ❌ | ❌ | ❌ |
| Add user to clinic | ✅ | ✅ | ✅ (own SiteAdmin clinics) | ❌ |
| Deactivate user (clinic) | ✅ | ✅ | ✅ (own clinics) | ❌ |
| Deactivate user (global) | ✅ | ✅ | ❌ | ❌ |
| Unlock account | ✅ | ✅ | ✅ (own clinics) | ❌ |
| Reset password | ✅ | ✅ | ✅ (own clinics) | ❌ |

---

### Implementation Order (when green-lit)

**Job #1: Connection String Routing**
1. Add `clinic_connections` table to MasterDbContext
2. Encrypt/decrypt service (AES-256-GCM)
3. Dynamic DbContext factory — resolves connection from session clinicId
4. Login flow: clinic picker screen for multi-clinic users
5. Session token includes clinicId, roles, adminLevel

**Job #2: Admin Hierarchy**
1. CLI bootstrap script for first Global Admin
2. `SysAdmin` creation UI (Global Admin only)
3. Auto-assign SysAdmins when new clinic is created
4. Clinic restriction UI for SysAdmins (Global Admin only)
5. Clinic picker screen (shared with Job #1)
6. Route guards using clinicId + adminLevel from session
7. SiteAdmin: restrict user creation/management to own SiteAdmin clinics
8. Global deactivation (SysAdmin/GlobalAdmin) vs per-clinic deactivation (SiteAdmin)
9. Audit log every admin creation with actor identity


---

## Demo Mode

### Purpose
Demo mode allows quick UI testing without a real database user. It bypasses `master.platform_users` entirely — no DB query needed.

### Credentials (in `appsettings.Development.json`)
```json
"DemoMode": {
  "Enabled": true,
  "Credentials": {
    "demo.admin":    "Demo@1234",
    "demo.sysadmin": "Demo@1234",
    "demo.site":     "Demo@1234"
  }
}
```

| Username | Password | Role |
|---|---|---|
| `demo.admin` | `Demo@1234` | global_admin |
| `demo.sysadmin` | `Demo@1234` | sys_admin |
| `demo.site` | `Demo@1234` | site_admin |

### How it works
- `AuthService.SignInAsync` checks demo credentials FIRST before querying any DB
- Demo auth returns `IsDemo = true` in `AuthResult`
- Login.razor adds `isDemo = "true"` claim to the session cookie
- TopNavLayout reads this claim and shows a **yellow warning banner** below the topnav

### Demo mode banner
When logged in as a demo user, a yellow strip appears below the topnav:
```
⚠ DEMO MODE — Data is not persisted. Switch to a real account for actual use.
```
- Real users (from `master.platform_users`) never see this banner
- The banner is rendered in `TopNavLayout.razor` — `isDemo` field checked in `OnInitializedAsync`

### Important rules
- Demo users are NOT in the database — any data they "save" goes to the DB but under a random UUID that won't be found next login
- Demo mode should be **disabled in production** — set `DemoMode:Enabled: false` in `appsettings.json`
- Never use `Admin`/`SysAdmin`/`SiteAdmin` as demo usernames — they clash with real seeded users
- Current demo usernames are prefixed with `demo.` to make them clearly distinguishable
- `MustChangePassword` is always false for demo users — password change flow only applies to real platform users

### Disabling demo mode for production
In `appsettings.json` (not Development):
```json
"DemoMode": {
  "Enabled": false
}
```

---

## Before Production Checklist

These items MUST be done before going live on any production server.

| # | Item | Status |
|---|---|---|
| 1 | `appsettings.Development.json` → add to `.gitignore` — contains demo credentials | ⏳ |
| 2 | Set `DemoMode:Enabled: false` in `appsettings.json` | ⏳ |
| 3 | Move photo/signature storage from JSONB to Azure Blob Storage (URL only in DB) | ⏳ |
| 4 | Replace `LiPi:EncryptionKey` in appsettings with Azure Key Vault reference | ⏳ |
| 5 | Enable HTTPS + HSTS in production (`UseHttpsRedirection`) | ⏳ |
| 6 | Set up PostgreSQL SSL connection strings | ⏳ |
| 7 | Configure SMTP for email OTP and notifications | ⏳ |
| 8 | Set up audit log partition maintenance cron job | ⏳ |
| 9 | Review and tighten CORS policy | ⏳ |
| 10 | Run penetration test before first patient data entry | ⏳ |

---

## Break Glass — Emergency Override Access (Pending Implementation)

### What it is
Break Glass allows authorized clinical staff to override normal access restrictions
in a documented emergency. Required by HIPAA — emergency access must be POSSIBLE
but every use must be AUDITED and ALERTED.

### When it triggers
When a clinician tries to open a patient record they do not have permission to view,
instead of a plain "Access Denied" — the system shows a Break Glass option.

### Flow
```
1. Clinician accesses restricted patient record
2. System detects insufficient permission
3. Shows: "You are not authorized to view this record"
4. Red "Break Glass — Emergency Access" button
5. Clinician must select a REASON CODE:
     - Medical emergency
     - Patient unconscious / unable to consent
     - Covering for treating physician
     - Coroner / legal requirement
     - Other (free text required)
6. Access granted immediately
7. EVERYTHING logged to audit.phi_access_log:
     - Who (platform_user_id, name, role)
     - Which patient (patient_id, MRN)
     - Which record (record type, record_id)
     - Reason code + free text
     - Timestamp (UTC)
     - IP address, device/browser
8. Automatic alerts sent to:
     - Patient's primary physician
     - Department head
     - HIPAA Compliance Officer
     - (via email using IEmailService)
```

### Where it applies
Clinical modules ONLY — OP, IP, RO, MO, Lab, Rad etc.
NOT applicable to admin pages (Users, Clinics, Settings).

### Implementation notes
- Break Glass button must be RED with explicit confirmation dialog (CLAUDE.md UX rules)
- Reason code is MANDATORY — cannot proceed without selecting one
- "Other" reason requires minimum 20 characters of free text
- Alert emails must be sent BEFORE access is granted (not after)
- Break Glass events are in a SEPARATE log table from normal PHI access
- Break Glass events are NEVER soft-deleted — retained for minimum 6 years (HIPAA)
- Dashboard widget for compliance officers showing Break Glass usage trends
- Excessive Break Glass use by same clinician triggers a compliance review flag

### Tables needed (when implementing)
```sql
-- In per-clinic identity/audit schema
audit.break_glass_events (
  id, accessed_by_user_id, patient_id, record_type, record_id,
  reason_code, reason_text, accessed_at, client_ip, user_agent,
  alerts_sent_at, acknowledged_by, acknowledged_at
)
```

### Status
⏳ Pending — implement when building first clinical module (OP/IP)

---

## Photo & Signature — Platform Strategy

### Core Decision
LiPi will have THREE platform versions — Web, Tablet, Phone.
Each version optimizes input methods for its platform.

### Photo Upload
| Platform | Method |
|---|---|
| **Web** | File upload + Scan (document scanner/flatbed) |
| **Tablet** | File upload + Scan + Camera capture |
| **Phone** | File upload + Camera capture |

### Signature
| Platform | Method |
|---|---|
| **Web** | File upload only (JPG/PNG of physical signature) |
| **Tablet** | File upload + Draw on screen (finger/stylus — signature pad) |
| **Phone** | File upload + Draw on screen (finger) |

### Current Implementation (Web only)
- File upload only for both photo and signature
- 1MB size limit per file (will revisit for production)
- Auto-compressed client-side before upload
- Stored as base64 in `master.platform_users.extension_data` JSONB
- Production: move to Azure Blob Storage, store URL only (noted in Before Production checklist)

### Display
- Photo and signature shown on Profile page only (for now)
- Future: photo on reports, prescription headers
- Signature displayed on: reports, prescriptions, discharge summaries
- NO DSC (Digital Signature Certificate) required — display only, not legally verified
  - DSC to be considered when building prescription module if legally required

### Storage (current dev)
- Max size: 1MB per file
- ~5 users in dev = ~10MB total — acceptable in JSONB
- Production size limit to be decided based on clinic user count

### Future Platform Split
When building Tablet/Phone versions:
- Camera access for all upload buttons (photo, signature, documents)
- Signature pad (draw on screen) for signature field
- Web and Mobile/Tablet should be treated as SEPARATE app builds
- Consider MAUI Blazor Hybrid or PWA for tablet/phone versions

### Signature Purpose
- Display only — on reports, prescriptions, discharge summaries
- Not legally verified (no DSC)
- DSC implementation deferred until prescription module is built and legal requirement confirmed

---

## User Registration / Edit — Field List (UsersNew.razor & UsersEdit.razor)

### Section ① Personal Details (color: #1565C0)
| Field | Type | Required | Rule |
|---|---|---|---|
| Title | dropdown | no | Dr., Mr., Ms., Prof., Sr., Br., Rev., Other |
| First Name | text | yes | Letters/spaces/hyphens/dots only |
| Middle Name | text | no | Same letter rules |
| Last Name | text | yes | Letters/spaces/hyphens/dots only |
| Nickname | text | no | Free text |
| Display Name | text | no | Auto from Title+First+Last; editable; resets on name change if not manually edited |
| Gender | dropdown | yes | From AdminData.Genders |
| Date of Birth | date | yes | Max = Today−16y−1day |
| Blood Group | dropdown | no | From AdminData.BloodGroups |
| Photo | 80×80 thumb | no | Upload/Scan; max 1MB; JPG/PNG |
| Signature | 80×80 thumb | no | Upload/Scan; max 1MB; JPG/PNG |

### Section ② Qualifications (color: #6A1B9A)
| Field | Type | Required | Rule |
|---|---|---|---|
| Qualifications | multi-chip select | yes (at least 1) | From AdminData.Qualifications; click to toggle; reorderable via drag or up/down arrows |
| Custom qualification | text + Enter | no | Type and press Enter to add custom; appears as chip with × to remove |
| NMC Reg. Number | text | no | For doctors — National Medical Commission |
| AERB RP No. | text | no | Radiation Protection Officer licence |

### Section ③ Professional & Employment (color: #00695C)
| Field | Type | Required | Rule |
|---|---|---|---|
| Designation | text+datalist | yes | From AdminData.Designations; free entry also allowed |
| Staff Type | dropdown | yes | From AdminData.StaffTypes |
| Department | text | no | Free text |
| Employee Code | text | no | e.g. EMP-001 |
| Clinic | dropdown | yes | From master.clinics; site_admin auto-locked to their clinic |
| Joining Date | date | no | Future date shows "future date — view-only until then" hint |

### Section ④ Contact (color: #BF360C)
| Field | Type | Required | Rule |
|---|---|---|---|
| Email | email | yes | Valid format |
| Mobile | phone | yes | Country code + number; India: 10 digits starting 6–9 |
| Landline | phone | no | Country code + number; India: 6–11 digits |

### Section ⑤ System Roles (color: #1A237E)
| Group | Roles |
|---|---|
| Administrative | Director/HOD, Dept. Manager |
| Clinical | Treating Physician, Consultant, Resident/Intern |
| Nursing | Charge/Head Nurse, Staff Nurse |
| Allied Health | Medical Physicist, Dosimetrist, Radiographer/RTT, Pharmacist, Lab Technician, Radiologist |
| Support | Billing Staff, Receptionist, Data Entry |
- At least one role required
- Multiple roles allowed simultaneously

### Section ⑥ System Access (color: #006064)
| Field | Type | Required | Rule |
|---|---|---|---|
| Username | text | yes | Min 3 chars; lowercase+digits+dots+underscores+hyphens |
| Must change password on first login | checkbox | — | Default: checked |
| Password | password | yes | 8+ chars, upper+lower+digit+symbol |
| Confirm Password | password | yes | Must match |

### Qualification Reorder Rules
- Selected qualifications shown as ordered chips with ↑↓ arrows
- Drag-to-reorder also supported (via JS ondragstart/ondrop)
- Order is preserved in `extension_data.qualifications` array
- On reports/prescriptions, qualifications display in user-defined order


---

## User Registration — Two-Step Architecture (DECIDED)

### Decision
User registration is split into TWO separate steps/pages:

**Step 1 — Create User** (`/admin/users/new`)
- Saves to `lipi_master` → `master.platform_users`
- Also creates `master.clinic_memberships` record
- Covers: Personal Details, Qualifications, Professional & Employment, Contact, System Access (username + password)
- Does NOT assign roles yet
- On success → redirects to Step 2 with the new user ID

**Step 2 — Assign Roles** (`/admin/users/{id}/roles`)
- Saves to clinic DB (`lipi_training` etc.) → `identity.user_roles`
- Roles reference `platform_user_id` (cross-DB, no FK)
- Can be done immediately after Step 1, or deferred
- Can be re-done anytime via Edit User → Roles tab

### Why separate?
- Master DB and clinic DB are completely separate databases
- Roles are clinic-specific; a user can have different roles in different clinics
- Cleaner separation of concerns — identity vs access control
- HIPAA: principle of least privilege — roles assigned explicitly, not bundled with registration
- Allows admin to create a user and assign roles later when clinic context is known

### Current state
- Step 1 is implemented in `UsersNew.razor` (saves to master.platform_users)
- Step 2 is NOT yet built — `UserRoles` in UsersNew currently saves roles inline
- **TODO**: Extract role assignment into `/admin/users/{id}/roles` page
- Until Step 2 is built, role assignment remains inline in UsersNew as a temporary measure

### Role assignment page (to build)
```
/admin/users/{id}/roles
- Load user from master.platform_users
- Show user name/designation as context
- Show role checkboxes (same as Section 5 in UsersNew)
- Save to identity.user_roles in the clinic DB
- Support multiple clinic assignments (user can have different roles in each clinic)
```

---

## Role Definitions (Finalized)

### Mutually Exclusive Groups — user picks exactly ONE

| Code | Group | Roles |
|---|---|---|
| `clinician` | Clinician | Consultant, Visiting Consultant, Medical Officer, Senior Resident, Junior Resident |
| `nursing` | Nursing | Chief Nurse, Nurse Manager, Nurse Supervisor, Staff Nurse, Trainee Nurse |
| `physicist` | Physicist | Chief Physicist, Senior Physicist, Medical Physicist, Resident Physicist, Trainee Physicist |
| `dosimetrist` | Dosimetrist | Senior Dosimetrist, Dosimetrist |
| `rtt` | Radiation Therapist | Chief RTT, Senior RTT, RTT, Trainee RTT |
| `rad_tech` | Radiology Technician | Chief Rad Tech, Senior Rad Tech, Rad Tech, Trainee Rad Tech |
| `lab_tech` | Lab Technician | Chief Lab Tech, Senior Lab Tech, Lab Tech, Trainee Lab Tech |
| `nm_tech` | Nuclear Medicine Tech | Chief NM Tech, Senior NM Tech, NM Tech, Trainee NM Tech |
| `ot_tech` | OT Technician | OT In-charge, OT Tech, Trainee OT Tech |
| `cssd_tech` | CSSD Technician | CSSD In-charge, CSSD Tech, Trainee CSSD Tech |
| `billing` | Billing | Head Billing, Billing Manager, Billing Supervisor, Billing Executive, Billing Trainee |
| `pharmacy` | Pharmacy | Chief Pharmacist, Senior Pharmacist, Pharmacist, Trainee Pharmacist |
| `dietician` | Dietician | Chief Dietician, Senior Dietician, Dietician, Trainee Dietician |
| `physiotherapist` | Physiotherapist | Chief Physiotherapist, Senior Physiotherapist, Physiotherapist, Trainee Physiotherapist |
| `counsellor` | Counsellor | Senior Counsellor, Counsellor, Social Worker, Trainee Counsellor |

### Add-On Roles — combinable with any group above

| Code | Group | Roles |
|---|---|---|
| `operations` | Operations | COO, Facility Director, Administrator, MRO, Front Desk Manager, Front Desk Executive, Help Desk |
| `rso` | RSO | RSO Level I, RSO Level II, RSO Level III |

### Mutual Exclusivity Rules
- Groups 1–15 are mutually exclusive (clinical/technical groups) — a user can have exactly ONE of them
- Operations and RSO can be PRIMARY (standalone) OR ADD-ON to any clinical/technical group
- Example valid: Consultant + RSO Level II + Operations (Front Desk)
- Example invalid: Clinician + Nursing
- Visiting Consultants do NOT get Break Glass access

### Role Codes for DB (identity.roles)
All roles stored as `code` in `identity.roles` table.
Mutual exclusivity enforced at application layer (UserRoles.razor + ValidateRoles()).

---

## Table Alignment Rules (ALL tables across the system)

Apply to every table and list-grid in every page — current and future.

| Column position | Content type | Alignment |
|---|---|---|
| First column | Name, label, description | **Left aligned** |
| First column | S.No or Date | **Center aligned** |
| Second column | (when first is S.No/Date) | **Left aligned** |
| All other columns | Any content | **Center aligned** |

### Rules
- Column **headings** follow the same alignment as their content
- Status badges, pills, icons → center
- Names, usernames, descriptions → left
- Numbers, dates, codes → center
- Action buttons → right (last column only)

### Implementation pattern (ul-row grid)
```html
<!-- Header -->
<div class="ul-col-hdr">Name & Actions</div>           ← left (default)
<div class="ul-col-hdr" style="text-align:center">Role</div>
<div class="ul-col-hdr" style="text-align:center">Status</div>

<!-- Row cells -->
<div class="ul-info">...</div>                          ← left (default)
<div style="...;justify-content:center">...</div>       ← center
<div style="...;justify-content:center">...</div>       ← center
```

### ap-table (standard HTML table) pattern
```html
<th>Name</th>                                           ← left
<th style="text-align:center">Status</th>              ← center
<td>Dr. Sharma</td>                                     ← left
<td style="text-align:center">active</td>               ← center
```

---

## Form Field Rules — id and name (MANDATORY)

Every `<input>`, `<select>`, and `<textarea>` in every form MUST have BOTH:
- `id="..."` — unique within the page, used for label association and JS targeting
- `name="..."` — used by browser autofill and form accessibility

**Without both attributes, browser shows:** "A form field element has neither an `id` nor a `name` attribute."

### Naming convention
- `id`: page prefix + field name in kebab-case → `un-firstName`, `ce-email`, `oe-phone`
- `name`: camelCase field name → `firstName`, `emailAddress`, `mobileNumber`

### Prefixes by page
| Page | Prefix |
|---|---|
| UsersNew | `un-` |
| UsersEdit | `ue-` |
| ClinicsNew | `cn-` (already has) |
| ClinicsEdit | `ce-` |
| OrgsNew | `on-` (already has) |
| OrgsEdit | `oe-` |
| Any new page | short 2-3 char prefix based on page |

### Exception
Display-only checkboxes with `tabindex="-1"` and `pointer-events:none` do not need id/name (they are not user-interactive).

### Template
```html
<input id="un-firstName" name="firstName" class="af-finput" ... />
<select id="un-gender" name="gender" class="af-fselect" ...>
<textarea id="un-notes" name="notes" class="af-finput" ...>
```

---

## Form Field Accessibility Rules (MANDATORY for all pages)

Every `<input>`, `<select>`, `<textarea>` MUST have all three:

### 1. `id` and `name` attributes
```html
<input id="un-firstName" name="firstName" ... />
```

### 2. `autocomplete` attribute
Use standard HTML autocomplete values:
| Field type | autocomplete value |
|---|---|
| Title/Salutation | `honorific-prefix` |
| First name | `given-name` |
| Middle name | `additional-name` |
| Last name | `family-name` |
| Nickname | `nickname` |
| Display/Full name | `name` |
| Gender | `sex` |
| Date of birth | `bday` |
| Email | `email` |
| Phone country code | `tel-country-code` |
| Phone number | `tel-national` |
| Full phone | `tel` |
| Username | `username` |
| New password | `new-password` |
| Country | `country` |
| Job title/Designation | `off` (no standard equivalent) |
| Website | `url` |
| Any internal/custom field | `off` |

### 3. `<label for="...">` matching the field `id`
```html
<label for="un-firstName">First Name</label>
<input id="un-firstName" name="firstName" autocomplete="given-name" ... />
```
OR nest the input inside the label:
```html
<label>First Name <input id="un-firstName" name="firstName" ... /></label>
```

### Exception
Display-only elements with `tabindex="-1"` and `pointer-events:none` are exempt.

---

## Design System — Colour & Element Reference

### Form Section Card Border Strips (border-left: 4px solid)
| Section | Colour | Hex |
|---|---|---|
| ① Personal Details | Navy Blue | #1565C0 |
| ② Qualifications | Purple | #6A1B9A |
| ③ Professional & Employment | Teal | #00695C |
| ④ Contact | Deep Orange | #BF360C |
| ⑤ System Access | Dark Teal | #006064 |
| Audit success row | Green | #10B981 |
| Audit error row | Red | #EF4444 |

### Row Status Strips (ul-row border-left-color)
active=#10B981 · suspended=#F59E0B · locked=#EF4444 · invited=#4A9BD4 · provisioning=#6366F1

### Avatars (ul-avatar ul-avatar-{colour})
Shape: circle for users (border-radius:50%) · rounded square for clinics/orgs (border-radius:10px)
navy=#0B2545 · blue=#1B4DA0 · teal=#00695C · amber=#D97706 · purple=#6A1B9A · red=#C62828 · green=#2E7D32 · slate=#475569

ClinicColor: cancer_centre→red · hospital→blue · day_care→teal · diagnostic→amber · default→navy
OrgColor: hospital_chain→blue · government→navy · academic→purple · trust→green · single_clinic→teal · default→slate
Users: always ul-avatar-navy

### Status Pills (ul-role-pill ul-{colour})
green=active · amber=suspended/onboarding · red=terminated/suspended · navy=default

### Action Buttons (ul-act ul-act-{type})
edit=✏ blue hover · susp=⏸/▶ amber hover · del=🗑/⏻ red hover
org=🏢 light-blue hover · roles=🎭 violet hover · rights=🔑 orange hover · ok=▶ green hover

### Inline Badges
cl-code-badge: clinic code — indigo pill, monospace
cl-type-badge: clinic/org type — green pill
ul-mono-badge: username — indigo pill, monospace
ul-status-dot: 7px circle — same colours as row strips

---

## Session 13-14 — Patient Registration Module

### TopNav Page Title CSS (on light #F0F4F8 S-curve SVG island)
- `.tn-pt-title`: `font-size:20px; font-weight:700; color:#0B2545; letter-spacing:-0.4px`
- `.tn-pt-sub`: `font-size:10px; font-weight:400; color:#64748B`
- The centre band is `#F0F4F8` (light) — dark text is correct. Never use white here.

### KPI Strip
- 6 tiles, full page width, always visible on every module page
- Tiles vary per Role + Page — wired to `IKpiService` (TODO)
- Each tile: `pr-kpi-accent` (3px left strip colour) + label + value + sub
- Accent colours: Navy · Amber · Cobalt · Sky · Red · Green

### PatientNew.razor — Registration Rules
- **3 steps** (not 4): Step 1=Identity+Contact+Address · Step 2=Clinical · Step 3=Review
- **No `pr-header` block** — topnav title already shows "Register Patient"
- **Wizard tabs + action buttons share one row** (tabs-row with `margin-left:auto` buttons)
- **Mandatory fields**: FirstName · LastName · (DOB OR Age) · Sex · Mobile · PinCode
- DOB ⇄ Age bidirectional: DOB filled → exact age (green note); Age filled → estimated Jan 1 DOB (amber note)
- Mobile is mandatory (+91 India default), Email is optional
- Photo: `IBrowserFile` on patient card (right column), `InputFile` component
- Patient Identifiers: card below patient card on right → click opens modal
  - Modal sections: ABDM (tabs: ABHA Number / ABHA Address / Mobile OTP) · Aadhaar (XML / Last 4) · Other ID
  - Full Aadhaar number never stored (UIDAI)
- Address via `AddressBlock` component with `Required=true`
- Emergency note in card hint: "Name + DOB/Age + Sex + Mobile + PIN is minimum to save"

### Compact Form CSS (pr-* prefix, sentinel: .pr-body)
- Input height: `padding:16px 10px 5px` (was 20px 12px 7px) — saves ~6px per field
- Float label: `top:4px left:10px font-size:7.5px` (was 6px 12px 8.5px)
- Grid gaps: `7px` (was 10px), grid margin-bottom: `7px` (was 12px)
- Section header `.pr-sh` margin-bottom: `8px` (was 14px)
- Wizard tab padding: `7px 10px` (was 10px 12px), tab-num: `18px` (was 22px)
- Panel padding: `14px 16px 16px` (was 20px 24px 24px)

### Bottom Nav + FAB
- Circular bubble style kept (not flat bar)
- FAB: teal (`#0F6E56`), bottom-right, `pfab-*` prefix
- FAB actions: Register patient (Ctrl+N) · Search patient (Ctrl+F) · Add to queue (Ctrl+Q)
- FAB visible only to roles with patient registration rights
- Keyboard shortcuts registered via `lipi-nav.js` → `lipiNav.initFab(dotNetRef)`
