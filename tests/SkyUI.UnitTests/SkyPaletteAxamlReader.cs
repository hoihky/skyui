using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace SkyUI.UnitTests;

internal static partial class SkyPaletteAxamlReader
{
    private static readonly Regex ColorLine = ColorLineRegex();

    public static IReadOnlyDictionary<string, Color> LoadPalette(string axamlRelativePath)
    {
        var path = Path.Combine(RepoRoot(), axamlRelativePath);
        var colors = new Dictionary<string, Color>(StringComparer.Ordinal);
        foreach (var line in File.ReadAllLines(path))
        {
            var match = ColorLine.Match(line);
            if (!match.Success)
                continue;

            colors[match.Groups[1].Value] = ParseHexColor(match.Groups[2].Value);
        }

        return colors;
    }

    private static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "SkyUI.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root (SkyUI.slnx).");
    }

    private static Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
            hex = "FF" + hex;

        if (hex.Length != 8)
            throw new FormatException($"Invalid color hex: #{hex}");

        var a = byte.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromArgb(a, r, g, b);
    }

    [GeneratedRegex(@"<Color x:Key=""([^""]+)"">#([0-9A-Fa-f]{6,8})</Color>")]
    private static partial Regex ColorLineRegex();
}
