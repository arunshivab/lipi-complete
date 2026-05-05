// SPEC:     docs/00-COMPONENTS/01.2-TextInputs.md (pending — Phase 2.2 Sub-step)
//           HTML5 autofill spec: https://html.spec.whatwg.org/multipage/form-control-infrastructure.html#autofill
// DECISION: docs/00-PROJECT-BASELINE.md §12 + Phase 2.2 strategic decisions
// PHASE:    Phase 2 Sub-step 2.2 — TextInput component family
//
// Purpose: validate the `autocomplete` HTML attribute against the WHATWG-blessed list of
// autofill tokens before rendering. Catches typos at dev-time (Development environment
// throws InvalidOperationException with hint) so accessibility and password manager
// integration aren't silently broken.
//
// Why this matters: invalid autocomplete values are silently ignored by browsers, but
// they break:
//   - Password managers (1Password, Bitwarden, etc.) that match on these tokens
//   - Browser autofill (Chrome, Safari, Firefox)
//   - Screen reader hints (ARIA infers some affordances from autocomplete)
// "country-name" → silently broken; user keeps typing into a field that won't autofill.
//
// Scope: covers the realistic HIS form surface area (~30 tokens). The full WHATWG
// list has 50+ values; we accept the common ones explicitly. Add tokens here as
// new HIS modules need them rather than wholesale-listing every WHATWG value.
//
// Validation policy: env-gated.
//   Development → throw InvalidOperationException with rejected value + hint
//   Production   → caller logs + falls back to "off" (safe no-op)
// Components implement the env gate; this class only knows valid/invalid.

using System.Collections.Frozen;

namespace LiPi.Web.Components.Shared;

/// <summary>
/// Validates the HTML5 <c>autocomplete</c> attribute value against the WHATWG autofill spec
/// (subset relevant to HIS forms). Used by LipiTextBox / LipiTextArea / LipiNumberInput / LipiSelect
/// in their parameter validation passes.
/// </summary>
public static class AutocompleteValidator
{
    /// <summary>
    /// Allowed autocomplete tokens for v1.0 LiPi forms. Subset of the WHATWG spec covering
    /// the realistic HIS surface area: identity, contact, address, password, dates, demographics.
    /// FrozenSet is faster than HashSet for read-only lookups and zero-allocation enumeration.
    /// </summary>
    private static readonly FrozenSet<string> ValidTokens = new[]
    {
        // Universal off/on
        "off", "on",

        // Name (full + parts)
        "name", "given-name", "additional-name", "family-name",
        "honorific-prefix", "honorific-suffix", "nickname",

        // Account
        "username", "new-password", "current-password", "one-time-code",

        // Email + tel
        "email",
        "tel", "tel-country-code", "tel-national", "tel-area-code", "tel-local",
        "tel-extension",

        // Address (postal)
        "street-address",
        "address-line1", "address-line2", "address-line3",
        "address-level1", // state / province
        "address-level2", // city
        "address-level3", // district / sub-locality
        "address-level4", // neighborhood
        "postal-code",
        "country", "country-name",

        // Birthday
        "bday", "bday-day", "bday-month", "bday-year",

        // Demographics
        "sex",

        // Organization
        "organization", "organization-title",

        // Misc
        "language", "url", "photo",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Cached hint string for invalid-value error messages. Lists the most commonly-needed
    /// tokens in form-field order rather than alphabetical so devs can scan it quickly.
    /// </summary>
    public static readonly string ValidValuesHint =
        "off | on | " +
        "name | given-name | family-name | additional-name | honorific-prefix | honorific-suffix | nickname | " +
        "username | new-password | current-password | one-time-code | " +
        "email | tel | tel-country-code | tel-national | " +
        "street-address | address-line1 | address-line2 | address-line3 | " +
        "address-level1 (state) | address-level2 (city) | address-level3 (district) | postal-code | " +
        "country | country-name | " +
        "bday | bday-day | bday-month | bday-year | " +
        "sex | organization | organization-title | language | url | photo";

    /// <summary>
    /// Returns true if the provided value is a valid HTML5 autocomplete token.
    /// Empty/null/whitespace returns true (treated as "not specified" → no validation needed;
    /// the component will not emit the autocomplete attribute at all in that case).
    /// </summary>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return ValidTokens.Contains(value);
    }

    /// <summary>
    /// Returns a developer-friendly error message for an invalid value. Includes the
    /// rejected value and a suggestion when a likely-intended token is detectable
    /// (e.g., "country-name" was rejected by older WHATWG drafts; current spec accepts both).
    /// </summary>
    public static string FormatInvalidMessage(string componentName, string parameterName, string? rejectedValue)
    {
        var suggestion = SuggestCorrection(rejectedValue);
        var suggestionPart = suggestion is null
            ? string.Empty
            : $" Did you mean '{suggestion}'?";

        return
            $"{componentName}: '{parameterName}' value '{rejectedValue}' is not a valid HTML5 autofill token.{suggestionPart} " +
            $"Valid values: {ValidValuesHint}";
    }

    /// <summary>
    /// Levenshtein-light correction for common typos. Hand-coded for the values most
    /// likely to be miswritten by devs new to the spec. Returns null if no clear suggestion.
    /// </summary>
    private static string? SuggestCorrection(string? rejected)
    {
        if (string.IsNullOrWhiteSpace(rejected)) return null;
        var lower = rejected.ToLowerInvariant().Trim();

        // Common mistakes captured from real-world experience
        return lower switch
        {
            "city"           => "address-level2",
            "town"           => "address-level2",
            "state"          => "address-level1",
            "province"       => "address-level1",
            "district"       => "address-level3",
            "zip"            => "postal-code",
            "zipcode"        => "postal-code",
            "pincode"        => "postal-code",
            "pin"            => "postal-code",
            "postcode"       => "postal-code",
            "phone"          => "tel",
            "mobile"         => "tel",
            "phone-number"   => "tel",
            "first-name"     => "given-name",
            "last-name"      => "family-name",
            "middle-name"    => "additional-name",
            "title"          => "honorific-prefix",
            "dob"            => "bday",
            "date-of-birth"  => "bday",
            "birthdate"      => "bday",
            "birthday"       => "bday",
            "gender"         => "sex",
            "company"        => "organization",
            "designation"    => "organization-title",
            "job-title"      => "organization-title",
            "password"       => "current-password",
            "user-name"      => "username",
            "user_name"      => "username",
            "userid"         => "username",
            "user-id"        => "username",
            _                => null
        };
    }
}
