# LipiEmptyState Specification

**Phase:** 2.8 — Data Display
**Component:** `LipiEmptyState`
**Status:** Spec body for build — locked
**Composed by:** LipiTable §18 (body states), LipiList §10, any consuming page with empty / error / no-result surfaces
**Cross-references:** LipiTable §18.4 (Error state), §18.5 (Empty state), §18.6 (FilteredEmpty state)

---

## §0 — Cross-section reconciliation

While drafting LipiTable §18 / §26, I referenced LipiEmptyState's `Variant` and default icons. Locking the authoritative values here:

### §0.1 — `EmptyStateVariant` enum

```csharp
public enum EmptyStateVariant
{
    Default,         // generic empty — caller picks everything
    Empty,           // "no data exists yet" — default icon: inbox
    FilteredEmpty,   // "filters yield zero" — default icon: search-x
    Error,           // "load failed" — default icon: alert-octagon
    Success,         // "task complete, nothing pending" — default icon: check-circle
    Coming           // "feature coming soon" — default icon: clock
}
```

### §0.2 — Default icon per variant

| Variant | Default Icon | Default tone color |
|---|---|---|
| Default | `inbox` (generic) | neutral / muted |
| Empty | `inbox` | neutral / muted |
| FilteredEmpty | `search-x` | neutral / muted |
| Error | `alert-octagon` | danger |
| Success | `check-circle` | success |
| Coming | `clock` | info |

The variant determines:
- The default icon (when `Icon` parameter is not set)
- The default CTA color tone (Error → danger button; others → primary)
- The default title placeholder if `Title` is not set (rare — Title is required so this fallback is for dev-mode warnings only)

### §0.3 — Locked cross-references

- LipiTable §18.4 default error UI = `<LipiEmptyState Variant="Error" />` (with Retry CTA injected by LipiTable)
- LipiTable §18.5 default empty UI = `<LipiEmptyState Variant="Empty" />` (with caller's `EmptyTitle` / `EmptyBody` / Add CTA)
- LipiTable §18.6 default filtered-empty UI = `<LipiEmptyState Variant="FilteredEmpty" />` (with Clear Filters CTA injected by LipiTable)
- LipiList §10.2 = same pattern

This means LipiTable's `Empty*` parameters (EmptyTitle, EmptyIcon, EmptyShowAddCta) map directly to LipiEmptyState's parameters. LipiTable composes LipiEmptyState internally — no separate empty-state code in LipiTable.

---

## §1 — Overview and design principles

### §1.1 — What LipiEmptyState is

LipiEmptyState is the **zero-state primitive** — a centered icon + title + body + optional CTAs for surfaces that have no content to show. It consolidates a pattern that was previously ad-hoc across the codebase (custom divs with inconsistent spacing, colors, and copy patterns).

Three primary use cases:

1. **True empty** — "no data exists yet" (e.g., "Add your first user", "Invite team members to get started")
2. **Filtered empty** — "data exists but current filters yield zero" (e.g., "No matches — try clearing filters")
3. **Error empty** — "load failed" (e.g., "Couldn't load. Retry?")

Plus secondary use cases:
- **Success empty** — "task completed, nothing pending" (e.g., "All caught up!", "Inbox zero")
- **Coming soon** — "feature placeholder" (e.g., "Reports coming in v2.0")
- **Default** — caller picks everything; no variant-specific defaults

### §1.2 — Why a separate component vs ad-hoc divs

Before LipiEmptyState:
- Each page wrote its own empty div with inconsistent spacing / colors / icon sizes
- Dark mode broke randomly when consumers forgot to use tokens
- Mobile layouts varied per page
- Visual cohesion drifted as the app grew

With LipiEmptyState:
- One canonical visual treatment
- Token-driven (light / dark / high-contrast)
- Responsive built-in
- Discoverable as a primitive — devs reach for it instead of building custom

### §1.3 — Composition vs ownership

LipiEmptyState is purely presentational — no state, no service injection, no data fetching. It renders what the caller gives it. The "data is empty" determination lives in the consuming code (LipiTable's state machine, the consuming page's logic, etc.).

This stateless design makes it trivially reusable. Same component in 50 different contexts; each context provides its own copy and CTAs.

### §1.4 — Out of scope for v1.0

- **Animated empty states** (lottie-style animated illustrations) — static icons only in v1.0. Caller can compose via `IconTemplate` if they want animation.
- **Multi-step CTAs** (carousel of "what to do next" steps) — caller composes their own multi-step UI if needed.
- **Inline embedding inside text-content** (LipiEmptyState is a block component; doesn't render inline with surrounding text)
- **Built-in tutorial / tour integration** — out of scope; caller wires their own onboarding.

---

## §2 — Visual anatomy

### §2.1 — Layout

```
┌──────────────────────────────────────────────┐
│                                              │
│              [    icon    ]                  │  ← icon (default Lucide or custom)
│                                              │
│         No users yet                         │  ← title (h3 / 18px)
│                                              │
│  Add your first user to get started.         │  ← body (body / 14px, muted)
│  Users can be invited by email or            │
│  created directly.                            │
│                                              │
│         [ + Invite first user ]              │  ← primary CTA (LipiButton)
│         [ Learn more ]                       │  ← secondary CTA (link or button)
│                                              │
└──────────────────────────────────────────────┘
```

### §2.2 — Element inventory

| Element | Required? | Visual |
|---|---|---|
| Container | Yes | Centered flex column, padded |
| Icon | Optional (default per variant) | 32-64px Lucide icon, muted color |
| Title | Required | Bold, larger text |
| Body | Optional | Body text, muted color, multi-line OK |
| Primary CTA | Optional | LipiButton (primary variant by default) |
| Secondary CTA | Optional | LipiButton (tertiary / link variant) |

### §2.3 — Size variants

```csharp
public enum EmptyStateSize
{
    Inline,    // ~120px tall — fits inside table / list empty slot
    Card,      // ~200px tall — fits inside a card
    Page       // ~320px tall — full-page empty state with larger icon + spacing
}
```

Visual spec per size:

| Property | Inline | Card | Page |
|---|---|---|---|
| Container min-height | 120px | 200px | 320px |
| Icon size | 32px | 48px | 64px |
| Title size | 16px | 18px | 24px |
| Body size | 13px | 14px | 15px |
| Vertical gap (icon-title) | 8px | 12px | 16px |
| Vertical gap (title-body) | 4px | 8px | 12px |
| Vertical gap (body-cta) | 12px | 16px | 24px |
| Body max-width | 320px | 400px | 480px |
| Container padding | 16px | 24px | 40px |

Defaults to `Inline` because that's the dominant use case (LipiTable / LipiList body states).

### §2.4 — CSS class naming

All classes follow the `lipi-empty-*` prefix per the isolation contract:

- `lipi-empty` — outer container
- `lipi-empty--inline`, `lipi-empty--card`, `lipi-empty--page` — size modifiers
- `lipi-empty--default`, `lipi-empty--error`, etc. — variant modifiers
- `lipi-empty-icon` — icon wrapper
- `lipi-empty-title` — title text
- `lipi-empty-body` — body text
- `lipi-empty-actions` — CTA container
- `lipi-empty-actions-primary` — primary CTA wrapper
- `lipi-empty-actions-secondary` — secondary CTA wrapper

---

## §3 — API surface

### §3.1 — Parameters

```csharp
[Parameter] public string Title { get; set; } = "";

[Parameter] public string? Body { get; set; }

[Parameter] public string? Icon { get; set; }                  // Lucide icon name; null = use variant default

[Parameter] public RenderFragment? IconTemplate { get; set; }  // custom icon override (e.g., SVG, image)

[Parameter] public EmptyStateSize Size { get; set; } = EmptyStateSize.Inline;

[Parameter] public EmptyStateVariant Variant { get; set; } = EmptyStateVariant.Default;

[Parameter] public RenderFragment? PrimaryCta { get; set; }    // primary action slot

[Parameter] public RenderFragment? SecondaryCta { get; set; }  // secondary action slot

[Parameter] public string? Class { get; set; }                  // additional CSS classes

[Parameter] public string? Style { get; set; }                  // inline style override
```

### §3.2 — `Title` is required

`Title` is the only required parameter. In Development mode, an empty `Title` throws `InvalidOperationException`:

```
LipiEmptyState requires a non-empty Title parameter. 
Pass Title="..." to communicate what's missing or why the surface is empty.
```

In Production: empty title silently renders no title element (graceful degradation), and a warning logs.

This matches LipiTable / LipiList's dev-mode-throw, prod-fallback pattern (per A14 in user memories — env-gated validation throwing).

### §3.3 — Icon resolution order

1. If `IconTemplate` is set → use it (custom RenderFragment)
2. Else if `Icon` is set → render `<LucideIcon Name="@Icon" />`
3. Else if `Variant` has a default icon (per §0.2) → use it
4. Else → no icon rendered

When `Icon` is set explicitly to empty string `""`, no icon renders (explicit opt-out).

### §3.4 — CTA slots

Both CTA slots are `RenderFragment` — caller can pass any content. Typical patterns:

```razor
<LipiEmptyState Title="No users yet"
                Body="Add your first user to get started."
                Variant="EmptyStateVariant.Empty">
    <PrimaryCta>
        <LipiButton OnClick="@InviteUser" Icon="plus">Invite first user</LipiButton>
    </PrimaryCta>
    <SecondaryCta>
        <LipiButton Variant="ButtonVariant.Tertiary" OnClick="@OpenHelp">Learn more</LipiButton>
    </SecondaryCta>
</LipiEmptyState>
```

The slots are NOT type-constrained — caller can put anything inside (a select dropdown, a search input, a stack of buttons). LipiEmptyState just provides the centered container; what goes inside is the caller's call.

### §3.5 — When all CTAs are null

If neither `PrimaryCta` nor `SecondaryCta` are provided, the CTA container collapses (no empty space below the body). Common for pure informational empty states ("Nothing to see here").

### §3.6 — Variant-specific defaults (consolidated reference)

| Variant | Default Icon | Default CTA tone | Common Title pattern |
|---|---|---|---|
| Default | `inbox` | primary | (caller picks) |
| Empty | `inbox` | primary | "No {entity} yet" |
| FilteredEmpty | `search-x` | tertiary | "No matches found" |
| Error | `alert-octagon` | primary (danger tone for retry) | "Couldn't load {thing}" |
| Success | `check-circle` | primary | "All caught up!" |
| Coming | `clock` | tertiary | "{Feature} coming soon" |

These are **suggestions**, not hard rules. Caller can override every default. The variant primarily affects the default icon and visual tone (subtle color shift in container padding / accent).

### §3.7 — Standalone vs composed usage

LipiEmptyState works in two modes:

**Standalone usage** (consuming page provides everything):

```razor
<LipiEmptyState Title="No reports yet"
                Body="Reports will appear here after your first month of activity."
                Icon="bar-chart"
                Size="EmptyStateSize.Page">
    <PrimaryCta>
        <LipiButton OnClick="@SeeDocs">Read about reports</LipiButton>
    </PrimaryCta>
</LipiEmptyState>
```

**Composed inside LipiTable / LipiList** (parent provides everything):

```razor
<!-- LipiTable internally -->
<LipiEmptyState Variant="EmptyStateVariant.Empty"
                Title="@(EmptyTitle ?? "No data")"
                Body="@(EmptyBody ?? "There's nothing here yet.")"
                Icon="@EmptyIcon"
                Size="EmptyStateSize.Inline">
    @if (EmptyShowAddCta && ShowAddButton)
    {
        <PrimaryCta>
            <LipiButton OnClick="@HandleAddClick" Icon="plus">@AddButtonLabel</LipiButton>
        </PrimaryCta>
    }
</LipiEmptyState>
```

LipiTable parameters (`EmptyTitle`, `EmptyBody`, `EmptyIcon`, `EmptyShowAddCta`) map directly to LipiEmptyState parameters.

---

## §4 — Accessibility

### §4.1 — ARIA structure

```html
<div role="region" 
     aria-label="No users yet"
     class="lipi-empty lipi-empty--inline lipi-empty--empty">
    
    <div class="lipi-empty-icon" aria-hidden="true">
        <svg>...</svg>  <!-- Lucide icon -->
    </div>
    
    <h3 class="lipi-empty-title">No users yet</h3>
    
    <p class="lipi-empty-body">Add your first user to get started.</p>
    
    <div class="lipi-empty-actions">
        <button class="lipi-button">Invite first user</button>
        <button class="lipi-button lipi-button--tertiary">Learn more</button>
    </div>
</div>
```

Key elements:
- `role="region"` on the outer container — screen readers announce as a "region" (not "main" or other landmarks)
- `aria-label` matches the title — provides discoverability when navigating by regions
- Icon has `aria-hidden="true"` — decorative; the title conveys the meaning
- Title uses semantic `<h3>` (or `<h2>` for Page size variant)
- Body uses `<p>`
- CTAs are standard `<button>` elements

### §4.2 — Heading level

The heading element varies by size:

| Size | Heading element |
|---|---|
| Inline | `<h3>` |
| Card | `<h3>` |
| Page | `<h2>` |

Page-sized empty states represent a "main content area is empty" situation; `<h2>` matches the typical page hierarchy. Inline / Card are sub-regions; `<h3>` fits below the page's `<h1>` / `<h2>`.

For consuming pages with non-standard heading hierarchies, override via:

```csharp
[Parameter] public int? TitleHeadingLevel { get; set; }
```

Setting `2` forces `<h2>`, `3` forces `<h3>`, etc. Range: 1-6. Out-of-range values fall back to defaults.

### §4.3 — Keyboard navigation

LipiEmptyState itself doesn't trap focus. Tab order:
1. Tab into the empty state
2. First focusable element receives focus (typically primary CTA)
3. Tab moves through secondary CTA
4. Tab moves out of empty state to next focusable element on the page

When LipiEmptyState renders inside LipiTable / LipiList body during empty/error/filtered-empty states, focus management is the parent's responsibility per LipiTable §19.4.3 (focus moves to the CTA when transitioning into empty / error / filtered-empty states).

### §4.4 — Screen reader behavior

When the user navigates into the empty state region, the screen reader announces:
- "Region, No users yet" (from `aria-label`)
- The body text (when focus moves into it)
- The CTA buttons (when tabbed)

The icon doesn't announce because `aria-hidden="true"`. The semantic meaning is in the title.

For dynamically appearing empty states (e.g., the user applies a filter that yields no results), the parent component (LipiTable) announces via aria-live polite — LipiEmptyState itself doesn't have a live region.

### §4.5 — Color contrast

All text colors use tokens. Title meets WCAG AA (`--lipi-empty-text-title` → color-text-primary at 16.4:1). Body meets AA (`--lipi-empty-text-body` → color-text-secondary at 4.8:1). Icon color is decorative (no contrast requirement).

In high-contrast mode, tokens swap to AAA values per LipiTable §24.7.

### §4.6 — Reduced motion

No animations in LipiEmptyState. Renders instantly. No hover transitions on the container. Buttons inside inherit their own hover behavior (which respects reduced motion).

---

## §5 — Multi-industry tokens

LipiEmptyState uses the same foundation tokens as the rest of the library. Specific empty-namespace tokens:

```css
:root {
    /* Size-based dimensions */
    --lipi-empty-min-h-inline:           120px;
    --lipi-empty-min-h-card:             200px;
    --lipi-empty-min-h-page:             320px;
    
    --lipi-empty-pad-inline:             16px;
    --lipi-empty-pad-card:               24px;
    --lipi-empty-pad-page:               40px;
    
    /* Icon sizes */
    --lipi-empty-icon-size-inline:       32px;
    --lipi-empty-icon-size-card:         48px;
    --lipi-empty-icon-size-page:         64px;
    
    /* Title text */
    --lipi-empty-title-size-inline:      16px;
    --lipi-empty-title-size-card:        18px;
    --lipi-empty-title-size-page:        24px;
    --lipi-empty-title-weight:           600;
    --lipi-empty-title-color:            var(--color-text-primary);
    --lipi-empty-title-line-height:      1.3;
    
    /* Body text */
    --lipi-empty-body-size-inline:       13px;
    --lipi-empty-body-size-card:         14px;
    --lipi-empty-body-size-page:         15px;
    --lipi-empty-body-color:             var(--color-text-secondary);
    --lipi-empty-body-line-height:       1.5;
    --lipi-empty-body-max-w-inline:      320px;
    --lipi-empty-body-max-w-card:        400px;
    --lipi-empty-body-max-w-page:        480px;
    
    /* Spacing between elements */
    --lipi-empty-gap-icon-title:         8px;
    --lipi-empty-gap-title-body:         4px;
    --lipi-empty-gap-body-cta:           12px;
    --lipi-empty-gap-cta-cta:            8px;
    
    /* Page-size override for breathing room */
    --lipi-empty-gap-icon-title-page:    16px;
    --lipi-empty-gap-title-body-page:    12px;
    --lipi-empty-gap-body-cta-page:      24px;
    
    /* Variant-specific accent colors (icon tint) */
    --lipi-empty-icon-color-default:     var(--color-text-faint);
    --lipi-empty-icon-color-empty:       var(--color-text-faint);
    --lipi-empty-icon-color-filtered:    var(--color-text-faint);
    --lipi-empty-icon-color-error:       var(--color-danger);
    --lipi-empty-icon-color-success:     var(--color-success);
    --lipi-empty-icon-color-coming:      var(--color-info);
    
    /* Background variant (subtle accent for variants) */
    --lipi-empty-bg-default:             transparent;
    --lipi-empty-bg-error:               var(--color-danger-alpha-04);
    --lipi-empty-bg-success:             var(--color-success-alpha-04);
}
```

Lives in `wwwroot/css/lipi-empty-tokens.css`. Same mode-swap mechanism as the other components.

---

## §6 — StyleGuide additions

### §6.1 — Demo location

LipiEmptyState gets its own demo page at `/styleguide/data-display/lipi-empty-state-standalone`:

```
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiEmptyStateStandaloneDemo.razor
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiEmptyStateStandaloneDemo.razor.cs
```

(Files already counted in LipiTable §27.13.)

### §6.2 — Demo content

The demo shows all six variants side-by-side, plus the three sizes for each:

**Section 1: All six variants (default Inline size)**
- Default — "No data" / generic icon
- Empty — "No users yet" / `inbox` / Add CTA
- FilteredEmpty — "No matches" / `search-x` / Clear Filters CTA
- Error — "Couldn't load" / `alert-octagon` / Retry CTA (danger button)
- Success — "All caught up" / `check-circle` / (no CTA)
- Coming — "Reports coming soon" / `clock` / Learn More CTA

**Section 2: Three sizes for one variant (Empty)**
- Inline (~120px) — fits in a card
- Card (~200px) — fits in a panel
- Page (~320px) — full-page empty state

**Section 3: Custom icon variants**
- Lucide icon name passed via `Icon`
- Custom IconTemplate with SVG
- Custom IconTemplate with an emoji
- Custom IconTemplate with an image

**Section 4: Inside LipiCard / LipiPanel containers**

Shows LipiEmptyState composed inside larger containers — demonstrates it adapts to context.

### §6.3 — Interactive toggles

- Variant dropdown
- Size dropdown
- Title input field (live updates)
- Body input field (live updates)
- Icon input field (Lucide icon name picker)
- Toggle for primary CTA visibility
- Toggle for secondary CTA visibility

### §6.4 — A11y notes panel

- `role="region"` + `aria-label`
- Heading level varies by size (h3 / h2)
- Icon `aria-hidden="true"` (decorative)
- Body text in `<p>` for semantic structure

### §6.5 — Code snippet

The Code tab shows the canonical use cases in <30 lines each (each variant + each size).

---

## §7 — Files to create

### §7.1 — Source files

```
src/LiPi.Components/DataDisplay/LipiEmptyState/
├── LipiEmptyState.razor
├── LipiEmptyState.razor.cs
├── LipiEmptyState.razor.css
└── LipiEmptyStateTypes.cs       (EmptyStateSize, EmptyStateVariant enums)
```

**Just 4 files** — LipiEmptyState is a simple component. No internal sub-components needed.

### §7.2 — Shared CSS

```
src/LiPi.Web/wwwroot/css/lipi-empty-tokens.css
```

One new CSS file. Loaded via App.razor link tag.

### §7.3 — Demo files

```
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiEmptyStateStandaloneDemo.razor
src/LiPi.Web/Components/Pages/StyleGuide/DataDisplay/LipiEmptyStateStandaloneDemo.razor.cs
```

(Already counted in LipiTable §27.13.)

### §7.4 — Tests

```
test/LiPi.Components.Tests/DataDisplay/LipiEmptyStateTests.cs
```

One test file covers parameter validation, variant icon resolution, size variants, heading level overrides, CTA rendering. LipiEmptyState's surface is small enough that one file suffices.

### §7.5 — Total file count

| Category | Count |
|---|---|
| Source files | 4 |
| Shared CSS | 1 |
| Demo files | 2 (already counted in LipiTable) |
| Test files | 1 |
| **Total LipiEmptyState-specific** | **~6 files** |

The smallest of the four Phase 2.8 components, matching its smallest API surface.

---

## §8 — Deploy script additions

```powershell
# === Phase 2.8 — LipiEmptyState ===

# Source
@{ Source = "2.8-components-LipiEmptyState.razor"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor" },
@{ Source = "2.8-components-LipiEmptyState.razor.cs"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.cs" },
@{ Source = "2.8-components-LipiEmptyState.razor.css"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyState.razor.css" },
@{ Source = "2.8-types-LipiEmptyStateTypes.cs"; Target = "src\LiPi.Components\DataDisplay\LipiEmptyState\LipiEmptyStateTypes.cs" },

# CSS
@{ Source = "2.8-css-lipi-empty-tokens.css"; Target = "src\LiPi.Web\wwwroot\css\lipi-empty-tokens.css" },

# Tests
@{ Source = "2.8-test-LipiEmptyStateTests.cs"; Target = "test\LiPi.Components.Tests\DataDisplay\LipiEmptyStateTests.cs" },
```

Build stage: **Stage 2** of LipiTable's build order. LipiEmptyState is needed early since LipiTable's body states (LipiTable §18) compose it from the start.

Modified files: `App.razor` needs the `lipi-empty-tokens.css` `<link>` tag.

---

## §9 — Component isolation contract

Cross-reference LipiTable §25. LipiEmptyState follows identical rules:

### §9.1 — Naming invariants

- Namespace: `LiPi.Components.DataDisplay.LipiEmptyState.*`
- CSS prefix: `lipi-empty-*`
- Type names: `LipiEmptyState`, `EmptyStateVariant`, `EmptyStateSize`

No HIS-specific terminology. The component is entirely generic.

### §9.2 — Dependency invariants

LipiEmptiyState depends only on:
- `Microsoft.AspNetCore.Components.*`
- `LiPi.Components.Shared` (for `LucideIcon` rendering)
- `System.*`

It does NOT depend on:
- `LipiTable` / `LipiList` / `LipiPagination` source code (independence — these compose LipiEmptyState, not the reverse)
- Any HIS-specific types or services

### §9.3 — CSS contract

All CSS uses tokens (`--color-*`, `--sp-*`, `--r-*`, `--lipi-empty-*`). No hex literals, no px literals (other than 0, 1, 2). Same audit pattern as LipiTable §25.4.

### §9.4 — Stateless component

Like LipiPagination, LipiEmptyState is **stateless** — pure function of parameters. Same inputs → same output. No internal state, no service injection.

This makes:
- Multi-instance use trivial
- Testing parameter-input → DOM-output assertions
- Mount / unmount free of state-loss risk

### §9.5 — No JS interop

LipiEmptyState doesn't need JS interop. Purely a CSS + HTML composition. The component file has no `@inject IJSRuntime`.

---

## §10 — Worked examples

### §10.1 — Empty state for "no users yet"

```razor
<LipiEmptyState 
    Variant="EmptyStateVariant.Empty"
    Size="EmptyStateSize.Card"
    Title="No users yet"
    Body="Add your first user to start managing your team. Users can be invited by email or created directly.">
    
    <PrimaryCta>
        <LipiButton Icon="plus" OnClick="@OpenInviteModal">
            Invite first user
        </LipiButton>
    </PrimaryCta>
    
    <SecondaryCta>
        <LipiButton Variant="ButtonVariant.Tertiary" OnClick="@OpenDocs">
            Learn about user roles
        </LipiButton>
    </SecondaryCta>
</LipiEmptyState>
```

Card-sized empty state with both CTAs. Uses default `inbox` icon from Empty variant.

### §10.2 — Error state with retry

```razor
<LipiEmptyState
    Variant="EmptyStateVariant.Error"
    Size="EmptyStateSize.Card"
    Title="Couldn't load reports"
    Body="@_errorMessage">
    
    <PrimaryCta>
        <LipiButton Icon="refresh-cw" OnClick="@RetryLoad">
            Retry
        </LipiButton>
    </PrimaryCta>
    
    <SecondaryCta>
        <LipiButton Variant="ButtonVariant.Tertiary" OnClick="@ContactSupport">
            Contact support
        </LipiButton>
    </SecondaryCta>
</LipiEmptyState>
```

Error variant uses default `alert-octagon` icon with danger tint. Retry primary action; contact support secondary.

### §10.3 — Filtered-empty with clear-filters CTA

```razor
<LipiEmptyState
    Variant="EmptyStateVariant.FilteredEmpty"
    Size="EmptyStateSize.Inline"
    Title="No matches found"
    Body="@($"No results match \"{_currentSearch}\". Try different terms or clear filters.")">
    
    <PrimaryCta>
        <LipiButton Icon="x" OnClick="@ClearFilters">
            Clear filters
        </LipiButton>
    </PrimaryCta>
</LipiEmptyState>
```

Inline size for table-body composition. FilteredEmpty default `search-x` icon. Body shows the current search term.

### §10.4 — Custom IconTemplate

```razor
<LipiEmptyState
    Size="EmptyStateSize.Page"
    Title="Welcome to your dashboard"
    Body="Get started by completing your profile setup.">
    
    <IconTemplate>
        <img src="/images/dashboard-illustration.svg" 
             alt="" 
             style="width: 96px; height: 96px;" />
    </IconTemplate>
    
    <PrimaryCta>
        <LipiButton OnClick="@StartOnboarding">
            Start setup
        </LipiButton>
    </PrimaryCta>
</LipiEmptyState>
```

Page-sized empty state with custom SVG illustration (no Lucide icon). Perfect for onboarding flows.

### §10.5 — Success state (no CTA)

```razor
<LipiEmptyState
    Variant="EmptyStateVariant.Success"
    Size="EmptyStateSize.Inline"
    Title="All caught up!"
    Body="No notifications waiting." />
```

Success variant with no CTAs. Renders just icon + title + body. Container collapses tightly without empty CTA space.

### §10.6 — Composed inside LipiTable body state

This pattern lives inside LipiTable's internal code (per §0.3 cross-reference). Users of LipiTable don't write this directly — they configure via LipiTable's parameters:

```razor
<!-- User's code: -->
<LipiTable TItem="Patient"
           DataSource="@LoadPatientsAsync"
           KeySelector="@(p => p.Id)"
           EmptyTitle="No patients yet"
           EmptyBody="Get started by adding your first patient."
           EmptyShowAddCta="true"
           OnAddClick="@OpenAddModal"
           AddButtonLabel="+ Add Patient">
    <!-- columns -->
</LipiTable>

<!-- LipiTable internally renders during empty state: -->
<!--
<LipiEmptyState Variant="EmptyStateVariant.Empty"
                Size="EmptyStateSize.Inline"
                Title="No patients yet"
                Body="Get started by adding your first patient.">
    <PrimaryCta>
        <LipiButton OnClick="@OpenAddModal" Icon="plus">+ Add Patient</LipiButton>
    </PrimaryCta>
</LipiEmptyState>
-->
```

Users get a consistent visual treatment without writing LipiEmptyState markup directly — the component is composed under the hood.

---

*End of LipiEmptyState spec. End of Phase 2.8 strategic spec corpus.*
