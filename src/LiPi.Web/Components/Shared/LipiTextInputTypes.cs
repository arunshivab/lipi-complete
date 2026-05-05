// SPEC:     docs/00-COMPONENTS/01.2-TextInputs.md (pending — Phase 2.2 Sub-step)
// DECISION: docs/00-PROJECT-BASELINE.md §12 (Component Library + Multi-Theme)
// PHASE:    Phase 2 Sub-step 2.2 — TextInput component family
//
// Type definitions shared by LipiTextBox / LipiTextArea / LipiNumberInput / LipiSelect.
// Pattern matches LipiButtonTypes.cs (Phase 2.1) — short enum names without Lipi prefix,
// colocated with consuming components in LiPi.Web.Components.Shared namespace.

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Size variant for input components. Heights match LipiButton for visual rhythm:
/// Small=28px, Medium=32px (default), Large=40px. Affects font size, label size,
/// helper text size, and icon size in lockstep.
/// </summary>
public enum InputSize
{
    /// <summary>28px height, 12px font, 13px label, 11px helper. Use in dense table rows, toolbars.</summary>
    Small,

    /// <summary>32px height, 14px font, 14px label, 12px helper. Default for forms.</summary>
    Medium,

    /// <summary>40px height, 16px font, 15px label, 13px helper. Use for hero forms (login, search).</summary>
    Large
}

/// <summary>
/// HTML input type for LipiTextBox. Maps to the underlying &lt;input type="..."&gt; attribute.
/// Number is intentionally excluded — use LipiNumberInput&lt;TValue&gt; for numeric input
/// (locale-aware formatting, generics over int/decimal/double, optional steppers).
/// </summary>
public enum TextInputType
{
    /// <summary>Default plain text input.</summary>
    Text,

    /// <summary>Email input — triggers email keyboard on mobile, validates @ syntax.</summary>
    Email,

    /// <summary>Telephone input — triggers numeric keypad on mobile.</summary>
    Tel,

    /// <summary>Password input — masks characters, opt out of password-manager autofill via Autocomplete="off".</summary>
    Password,

    /// <summary>Search input — renders OS-styled clear button on some browsers.</summary>
    Search,

    /// <summary>URL input — triggers URL keyboard on mobile.</summary>
    Url
}

/// <summary>
/// Resolved validation state of an input field. Computed from props (Disabled, ReadOnly,
/// Error, Warning, Success, Required, Value). Drives both visual state and ARIA attributes.
/// State precedence (highest priority wins, override required tint):
///   Error > Success > Warning > Required-tint > Default
/// Disabled and ReadOnly are independent terminal states (not part of the precedence chain).
/// </summary>
public enum InputState
{
    /// <summary>Default neutral border, no state styling.</summary>
    Default,

    /// <summary>Error state — red border, light pink bg, auto-overlay X icon, aria-invalid=true, role=alert.</summary>
    Error,

    /// <summary>Warning state — amber border, light amber bg, auto-overlay triangle icon.</summary>
    Warning,

    /// <summary>Success state — green border, auto-overlay check icon.</summary>
    Success,

    /// <summary>Empty + Required + RequiredVisualStyle.ApricotTint — apricot bg + apricot border.</summary>
    RequiredEmpty,

    /// <summary>Disabled — grey bg, faded text, no interaction. Terminal state.</summary>
    Disabled,

    /// <summary>Read-only — dashed border + faint bg, full-color text, selectable but not editable. Terminal state.</summary>
    ReadOnly
}

/// <summary>
/// How required fields are visually marked when empty. Per-component parameter (nullable);
/// null falls through to the global <see cref="LipiInputDefaults.RequiredVisualStyle"/>.
/// </summary>
public enum RequiredVisualStyle
{
    /// <summary>
    /// Apricot tint (orange-50 bg + orange-200 border) when empty + required, fading to white
    /// when filled. Pairs with red asterisk + aria-label. Default for HIS context — gives
    /// reception staff at-a-glance "what's still missing" scanning across long forms.
    /// </summary>
    ApricotTint,

    /// <summary>
    /// Industry-standard minimal marking: red asterisk after label + aria-label="required field"
    /// + title="Required" only. No background or border tint. Use this when apricot feels
    /// too heavy (e.g., dense single-row forms, embedded inputs in tables).
    /// </summary>
    AsteriskOnly
}

/// <summary>
/// Confidence tier for fields like DOB and identity verification. Renders as an inline pill
/// next to the field label. Color tokens defined in mode-light.css / mode-dark.css §Confidence pills
/// (composed from semantic + neutral tokens — see CHANGE-LOG.md A12).
/// </summary>
public enum ConfidenceLevel
{
    /// <summary>Verified against authoritative source (Aadhaar, ABHA, ID document). Green pill.</summary>
    Verified,

    /// <summary>Self-reported by patient/staff, no document verification. Blue pill.</summary>
    SelfReported,

    /// <summary>Estimated (e.g., DOB inferred from apparent age when documents unavailable). Amber pill.</summary>
    Estimated,

    /// <summary>Unknown — explicitly recorded as not provided. Grey pill. Distinguishes from "missing data".</summary>
    Unknown
}
