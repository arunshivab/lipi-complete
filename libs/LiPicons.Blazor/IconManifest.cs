using System.Reflection;
using System.Text.Json;

namespace LiPicons.Blazor;

/// <summary>
/// Loads the embedded <c>icons.json</c> manifest exactly once and serves icon path
/// fragments by name. No network, no <c>wwwroot</c>: the manifest is an embedded
/// resource inside this assembly, so icons render behind a firewall offline.
/// </summary>
internal static class IconManifest
{
    private static readonly Lazy<IReadOnlyDictionary<string, string[]>> Icons =
        new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>The manifest version string (e.g. "1.0.2").</summary>
    public static string Version { get; private set; } = "0.0.0";

    /// <summary>Number of icons available.</summary>
    public static int Count => Icons.Value.Count;

    /// <summary>True if the named icon exists in the manifest.</summary>
    public static bool Contains(string name) => Icons.Value.ContainsKey(name);

    /// <summary>
    /// Attempts to fetch the raw SVG child fragments for the named icon
    /// (each entry is a <c>&lt;path&gt;</c>/<c>&lt;circle&gt;</c>/… element string).
    /// </summary>
    public static bool TryGet(string name, out IReadOnlyList<string> paths)
    {
        if (Icons.Value.TryGetValue(name, out var p))
        {
            paths = p;
            return true;
        }

        paths = Array.Empty<string>();
        return false;
    }

    private static IReadOnlyDictionary<string, string[]> Load()
    {
        var asm = typeof(IconManifest).Assembly;
        var resourceName = Array.Find(
            asm.GetManifestResourceNames(),
            n => n.EndsWith("icons.json", StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                "LiPicons.Blazor: embedded resource 'icons.json' was not found in the assembly.");

        using var stream = asm.GetManifestResourceStream(resourceName)!;
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        if (root.TryGetProperty("version", out var v) && v.ValueKind == JsonValueKind.String)
        {
            Version = v.GetString() ?? Version;
        }

        var dict = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var icons = root.GetProperty("icons");
        foreach (var icon in icons.EnumerateObject())
        {
            var pathsEl = icon.Value.GetProperty("paths");
            var list = new string[pathsEl.GetArrayLength()];
            var i = 0;
            foreach (var el in pathsEl.EnumerateArray())
            {
                list[i++] = el.GetString() ?? string.Empty;
            }

            dict[icon.Name] = list;
        }

        return dict;
    }
}
