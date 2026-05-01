# LiPi HIS — CSS Refactoring Roadmap

> **Decision Reference**: BASE v1.0 #1 — Refactor to 00-baseline.css + per-module CSS  
> **Estimated Effort**: 4-6 hours (one-time)  
> **Risk Level**: Medium (visual regression possible — test thoroughly)

---

## CURRENT STATE

```
wwwroot/css/
├── admin.css          175KB  ← MONOLITH (everything mixed)
├── app.css             19KB  ← Some baseline + dashboard
├── dashboard.css       25KB  ← Dashboard-specific (good separation)
└── LiPi-TopNav.css      5KB  ← TopNav layout (good separation)
```

**Problem**: `admin.css` contains:
- CSS variables (--navy, --gold, --bg, --sh-sm/md/lg, --ts-* font sizes)
- Common utilities (buttons, forms, tables)
- User-specific styles (`uf-*`, `ul-*`)
- Clinic-specific styles (`cf-*`, `cl-*`)
- Org-specific styles (`of-*`, `ol-*`)
- Patient-specific styles (`reg-*`, `pn-*`)
- Section accent colors (1=#1565C0, etc.)

This will not scale to 25 modules.

---

## TARGET STATE

```
wwwroot/css/
├── 00-baseline.css                    ← Variables, resets, common (extract from admin.css)
│
├── 01-user-registration.css           ← uf-, un-, ue-, ul-
├── 02-clinic-registration.css         ← cf-, cn-, ce-, cl-
├── 03-organization-registration.css   ← of-, on-, oe-, ol-
├── 04-patient-registration.css        ← reg-* (special), pn-, pe-, ps-
├── 05-appointments.css                ← ab-, ae-, ac-
├── 06-opd.css                         ← (TBD when built)
├── ...
├── 25-pacs.css                        ← (TBD when built)
│
├── components/                        ← Shared components (extract gradually)
│   ├── confirmation-dialog.css
│   ├── inline-edit-panel.css
│   ├── status-strip.css
│   └── empty-state.css
│
└── layouts/
    ├── topnav.css                     ← (renamed from LiPi-TopNav.css)
    ├── dashboard.css                  ← (already exists)
    └── admin-layout.css               ← (extract from admin.css)
```

---

## STEP-BY-STEP REFACTORING PLAN

### Phase 1: Create `00-baseline.css` (1 hour)

Extract these from `admin.css`:

#### 1.1 CSS Variables
```css
/* 00-baseline.css */
:root {
  /* Brand colors */
  --navy: #0B2545;
  --gold: #C49A22;
  --cobalt: #1565C0;
  --bg: #F4F7FB;
  --bg-card: #FFFFFF;
  
  /* Section accents (used across modules) */
  --sec-1: #1565C0;  /* blue */
  --sec-2: #6A1B9A;  /* purple */
  --sec-3: #00695C;  /* teal */
  --sec-4: #BF360C;  /* orange */
  --sec-5: #1A237E;  /* navy */
  --sec-6: #006064;  /* cyan-dark */
  --sec-7: #E65100;  /* amber */
  
  /* Status colors */
  --status-active: #4CAF50;
  --status-suspended: #FF9800;
  --status-locked: #F44336;
  --status-invited: #2196F3;
  --status-terminated: #9E9E9E;
  
  /* Shadows */
  --sh-sm: 0 1px 2px rgba(11, 37, 69, 0.06);
  --sh-md: 0 4px 8px rgba(11, 37, 69, 0.08);
  --sh-lg: 0 12px 24px rgba(11, 37, 69, 0.12);
  
  /* Typography scale */
  --ts-page-title: 18px;
  --ts-section: 15px;
  --ts-body: 13px;
  --ts-label: 12px;
  --ts-small: 11px;
  --ts-th: 10px;
  --ts-badge: 10px;
  --ts-micro: 9px;
  --ts-mono-data: 12px;
  
  /* Typography weights */
  --fw-regular: 400;
  --fw-medium: 500;
  --fw-semi: 600;
  --fw-bold: 700;
  
  /* Spacing scale */
  --sp-xs: 4px;
  --sp-sm: 8px;
  --sp-md: 12px;
  --sp-lg: 16px;
  --sp-xl: 24px;
  --sp-2xl: 32px;
  
  /* Border radius */
  --r-sm: 4px;
  --r-md: 7px;
  --r-lg: 10px;
  --r-xl: 16px;
  
  /* Transitions */
  --tr-fast: 0.15s ease;
  --tr-med: 0.3s ease;
}
```

#### 1.2 Resets & Base
```css
/* Reset & base typography */
* { box-sizing: border-box; margin: 0; padding: 0; }
html, body { font-family: 'DM Sans', sans-serif; font-size: var(--ts-body); }
body { background: var(--bg); color: var(--navy); }

/* Mono utility */
.mono { font-family: 'DM Mono', monospace; }
```

#### 1.3 Common Components
- `.btn`, `.btn-primary`, `.btn-danger`, `.btn-secondary`
- `.card`, `.card-header`, `.card-body`
- `.form-field`, `.form-label`, `.form-error`
- `.table`, `.table th`, `.table td`
- `.status-strip`, `.status-active`, `.status-locked`, etc.
- `.confirmation-dialog`
- `.empty-state`
- `.skeleton-row`

---

### Phase 2: Extract Per-Module CSS (2-3 hours)

For each module currently in `admin.css`:

#### 2.1 User Module (01-user-registration.css)
Extract: `uf-*`, `un-*`, `ue-*`, `ul-*` classes
Move to: `01-user-registration.css`

#### 2.2 Clinic Module (02-clinic-registration.css)
Extract: `cf-*`, `cn-*`, `ce-*`, `cl-*`

#### 2.3 Org Module (03-organization-registration.css)
Extract: `of-*`, `on-*`, `oe-*`, `ol-*`

#### 2.4 Patient Module (04-patient-registration.css)
Extract: `reg-*` (special PatientNew system), `pn-*`, `pe-*`, `ps-*`

#### 2.5 Appointment Module (05-appointments.css)
Extract: `ab-*`, `ae-*`, `ac-*`

---

### Phase 3: Update App.razor (15 min)

Replace single CSS link with imports:

```html
<!-- App.razor head -->
<link rel="stylesheet" href="~/css/00-baseline.css?v=@AppVersion" />
<link rel="stylesheet" href="~/css/layouts/topnav.css?v=@AppVersion" />
<link rel="stylesheet" href="~/css/01-user-registration.css?v=@AppVersion" />
<link rel="stylesheet" href="~/css/02-clinic-registration.css?v=@AppVersion" />
<link rel="stylesheet" href="~/css/03-organization-registration.css?v=@AppVersion" />
<link rel="stylesheet" href="~/css/04-patient-registration.css?v=@AppVersion" />
<link rel="stylesheet" href="~/css/05-appointments.css?v=@AppVersion" />
<!-- Add more as modules built -->
```

**Note**: Browser caches each file independently. Changing one module CSS doesn't invalidate others.

---

### Phase 4: Visual Regression Testing (1-2 hours)

For each page in app:
1. Take "before" screenshot (with original `admin.css`)
2. Apply refactored CSS
3. Take "after" screenshot
4. Diff side-by-side
5. Fix any regressions

**Test pages**:
- `/admin/users` (list, new, edit, roles, rights)
- `/admin/clinics` (list, new, edit)
- `/admin/orgs` (list, new, edit)
- `/admin/settings`
- `/patients/new`, `/patients/search`
- `/dashboard`
- Login, Forgot Password, etc.

---

### Phase 5: Deprecate `admin.css` (15 min)

After verification:
1. Move `admin.css` → `admin.css.legacy`
2. Add comment: `/* DEPRECATED — Split into 00-baseline.css + per-module files. Do not use. */`
3. Remove from `App.razor`
4. Keep .legacy file for 1 month as safety net
5. Delete after confirmed no regressions

---

## CRITICAL RULES (DURING REFACTORING)

### ✅ DO
- Use CSS variables (--ts-body, --navy, etc.) — NEVER hardcode values
- Keep `reg-*` system isolated (PatientNew only)
- Prefix every per-module class (un-, cn-, etc.)
- Document any new variables in 00-baseline.css with comments
- Test on multiple browsers (Chrome, Edge, Firefox)
- Test responsive breakpoints

### ❌ DON'T
- Don't mix module classes (e.g., `un-` in clinic CSS)
- Don't hardcode colors (use --navy, --gold, --status-* etc.)
- Don't hardcode font sizes (use --ts-* variables)
- Don't add !important (refactor specificity instead)
- Don't use scoped styles in InteractiveServer components (broken)

---

## VERSION BUMPING (CACHE BUSTING)

Use a global version variable:

```csharp
// Program.cs
public static string AppVersion => 
  Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion 
    ?? "1.0.0";
```

```html
<link rel="stylesheet" href="~/css/00-baseline.css?v=@AppVersion" />
```

Bump version on every CSS change to force browser refresh.

---

## VALIDATION

After refactoring:

```bash
# 1. Verify total CSS size (should be similar or smaller)
du -h wwwroot/css/

# 2. Check for unused selectors
npx purgecss --css wwwroot/css/01-user-registration.css \
  --content "src/LiPi.Web/Pages/Admin/Users*.razor"

# 3. Run all pages, check F12 Network tab — only loaded CSS should be the baseline + relevant module
```

---

## TROUBLESHOOTING

### "Styles missing on a page"
- Check App.razor — is the module CSS imported?
- Check browser cache — try Ctrl+Shift+R

### "Conflicts between modules"
- Module classes should be prefixed — check for unprefixed classes in module CSS

### "Variables not working"
- 00-baseline.css must load FIRST (before module CSS)
- Use CSS variables only AFTER they're defined

---

## DEPENDENCY ORDER

```
00-baseline.css                    ← Variables, resets
  ↓
layouts/*.css                      ← Page layouts
  ↓
components/*.css                   ← Shared components
  ↓
[module]-*.css                     ← Per-module styles (last)
```

This order is critical. Variables must be defined before use.

---

## ROLLBACK PLAN

If refactoring causes issues:
1. Revert `App.razor` to use original `admin.css`
2. Keep new files as `*.css.new` for review
3. Apply changes module-by-module instead of all at once

The original `admin.css` should be kept as `admin.css.legacy` for at least 1 month after refactoring.
