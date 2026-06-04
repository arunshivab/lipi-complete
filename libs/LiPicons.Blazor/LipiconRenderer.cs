using System.Globalization;
using System.Text.RegularExpressions;

namespace LiPicons.Blazor;

/// <summary>
/// Produces the inner SVG markup for an icon + variant. This is a direct port of the
/// repo's build pipeline (<c>scripts/build.mjs</c>) so a runtime-rendered
/// <see cref="LipiIcon"/> is byte-faithful to the pre-built CDN sprites.
/// </summary>
/// <remarks>
/// All output uses the literal token <c>currentColor</c>. The component substitutes an
/// explicit colour afterwards when <see cref="LipiIcon.Color"/> is set, so callers get
/// CSS-tintable icons by default and a fixed colour on request.
/// </remarks>
internal static partial class LipiconRenderer
{
    private static readonly IReadOnlyDictionary<LipiconVariant, double> StrokeWidth =
        new Dictionary<LipiconVariant, double>
        {
            [LipiconVariant.Regular] = 1.75,
            [LipiconVariant.Bold] = 2.25,
            [LipiconVariant.Light] = 1.25,
            [LipiconVariant.Thin] = 1.00,
            [LipiconVariant.Duotone] = 1.5,
            // Fill has no stroke width.
        };

    private static readonly char[] CloseCommandChars = ['Z', 'z'];

    [GeneratedRegex(@"<(path|circle|rect|ellipse|polygon|polyline|line)\b([^/>]*)(/?>)")]
    private static partial Regex ShapeRegex();

    [GeneratedRegex(@"\s*fill\s*=\s*""[^""]*""")]
    private static partial Regex FillRegex();

    [GeneratedRegex(@"\s*stroke\s*=\s*""[^""]*""")]
    private static partial Regex StrokeRegex();

    [GeneratedRegex(@"\s*fill-opacity\s*=\s*""[^""]*""")]
    private static partial Regex FillOpacityRegex();

    [GeneratedRegex(@"<(rect|circle|ellipse)")]
    private static partial Regex BackgroundShapeRegex();

    /// <summary>
    /// Builds the inner SVG content for the icon, plus the stroke width to set on the
    /// root <c>&lt;svg&gt;</c> (null for the Fill variant, which carries no stroke width).
    /// </summary>
    public static (string Inner, double? StrokeWidth) Render(
        IReadOnlyList<string> paths, LipiconVariant variant)
    {
        var joined = string.Concat(paths);

        switch (variant)
        {
            case LipiconVariant.Fill:
                return (ApplyAttrs(joined, "fill=\"currentColor\" stroke=\"none\" "), null);

            case LipiconVariant.Duotone:
            {
                var index = FindBackgroundIndex(paths);
                var bg = ApplyAttrs(
                    paths[index],
                    "fill=\"currentColor\" stroke=\"none\" fill-opacity=\"0.22\" ");
                // The background shape is drawn once as a translucent fill, then the full
                // path set (including that shape) is drawn on top as strokes.
                return (bg + joined, 1.5);
            }

            default:
                return (joined, StrokeWidth[variant]);
        }
    }

    /// <summary>
    /// Picks the shape that forms the icon's "background" for duotone fill: the first
    /// closed shape (rect/circle/ellipse, or any path with a Z/z close command), else
    /// the first path.
    /// </summary>
    private static int FindBackgroundIndex(IReadOnlyList<string> paths)
    {
        for (var i = 0; i < paths.Count; i++)
        {
            if (BackgroundShapeRegex().IsMatch(paths[i]) ||
                paths[i].IndexOfAny(CloseCommandChars) >= 0)
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>
    /// Strips any baked-in fill/stroke/fill-opacity from each shape element and prepends
    /// <paramref name="newAttrs"/>. Required because ~519 icons carry decorative dots with
    /// a baked-in <c>fill="currentColor"</c>; naive concatenation would duplicate
    /// attributes and break XML parsing.
    /// </summary>
    private static string ApplyAttrs(string fragment, string newAttrs) =>
        ShapeRegex().Replace(fragment, match =>
        {
            var tag = match.Groups[1].Value;
            var attrs = match.Groups[2].Value;
            var end = match.Groups[3].Value;

            attrs = FillRegex().Replace(attrs, string.Empty);
            attrs = StrokeRegex().Replace(attrs, string.Empty);
            attrs = FillOpacityRegex().Replace(attrs, string.Empty);

            return $"<{tag} {newAttrs}{attrs}{end}";
        });

    /// <summary>Formats a stroke width invariantly (so "1.75" never becomes "1,75").</summary>
    public static string FormatStroke(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
