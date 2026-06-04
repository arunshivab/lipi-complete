// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md §3.2.4 (Header resolution — humanization)
// PHASE: 2.8 Data Display — Stage 2 core shell (LipiTable)
// COMPONENT: shared internal utility
//
// Turns a property/identifier name into a human-readable Title Case header:
//   "DateOfBirth" -> "Date Of Birth"
//   "firstName"   -> "First Name"
//   "mobile"      -> "Mobile"
//   "patientUHID" -> "Patient UHID"   (consecutive caps kept as an acronym group)
//   "row_count"   -> "Row Count"      (underscores/hyphens treated as word breaks)
//
// Same humanizer used by LipiList field declarations (§3.2.4). Internal — not part of
// the public API surface; lives under Shared/Internal so it's reusable across the
// component library without leaking.

using System;
using System.Globalization;
using System.Text;

namespace LiPi.Components.Shared.Internal;

/// <summary>
/// Converts camelCase / PascalCase / snake_case / kebab-case identifiers into
/// "Title Case With Spaces" for use as default column/field headers.
/// </summary>
public static class IdentifierHumanizer
{
    /// <summary>
    /// Humanize an identifier. Returns the input unchanged if null/empty/whitespace.
    /// </summary>
    public static string Humanize(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return identifier ?? string.Empty;

        // Split into words at: separators (_ - space), case boundaries (aA),
        // and acronym→word boundaries (UHIDValue -> UHID, Value).
        var words = SplitWords(identifier);

        var sb = new StringBuilder(identifier.Length + 8);
        for (int i = 0; i < words.Count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(TitleCaseWord(words[i]));
        }
        return sb.ToString();
    }

    private static System.Collections.Generic.List<string> SplitWords(string s)
    {
        var words = new System.Collections.Generic.List<string>();
        var current = new StringBuilder();

        void Flush()
        {
            if (current.Length > 0)
            {
                words.Add(current.ToString());
                current.Clear();
            }
        }

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            // Separators -> word break, char dropped.
            if (c == '_' || c == '-' || c == ' ')
            {
                Flush();
                continue;
            }

            if (current.Length == 0)
            {
                current.Append(c);
                continue;
            }

            char prev = s[i - 1];

            // lower/digit -> Upper : boundary (firstName -> first | Name)
            bool lowerToUpper = !char.IsUpper(prev) && char.IsUpper(c);

            // Upper run -> Upper+lower : acronym end (UHIDValue -> UHID | Value)
            bool acronymEnd = char.IsUpper(prev) && char.IsUpper(c)
                              && i + 1 < s.Length && char.IsLower(s[i + 1]);

            // letter <-> digit boundary (row2 -> row | 2 ; v10 -> v | 10)
            bool letterDigit = (char.IsLetter(prev) && char.IsDigit(c))
                               || (char.IsDigit(prev) && char.IsLetter(c));

            if (lowerToUpper || acronymEnd || letterDigit)
                Flush();

            current.Append(c);
        }
        Flush();
        return words;
    }

    private static string TitleCaseWord(string word)
    {
        if (word.Length == 0) return word;

        // All-caps acronym (UHID, ABHA, ID) -> keep as-is.
        bool allUpper = true;
        foreach (var ch in word)
        {
            if (char.IsLetter(ch) && !char.IsUpper(ch)) { allUpper = false; break; }
        }
        if (allUpper && word.Length > 1)
            return word;

        // Otherwise: first letter upper, rest lower.
        var lower = word.ToLower(CultureInfo.CurrentCulture);
        return char.ToUpper(lower[0], CultureInfo.CurrentCulture) + lower.Substring(1);
    }
}
