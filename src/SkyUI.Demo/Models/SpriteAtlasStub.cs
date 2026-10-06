using Avalonia.Media;

namespace SkyUI.Demo.Models;

/// <summary>Demo stand-in for a sprite atlas (maps cel names to preview colors).</summary>
public sealed class SpriteAtlasStub
{
    public IBrush ResolveBrush(string? spriteName) =>
        new SolidColorBrush(ParseColor(spriteName));

    private static Color ParseColor(string? spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return Color.Parse("#444444");
        var hash = spriteName.GetHashCode(StringComparison.Ordinal);
        var r = (byte)(80 + (hash & 0x7F));
        var g = (byte)(80 + ((hash >> 8) & 0x7F));
        var b = (byte)(80 + ((hash >> 16) & 0x7F));
        return Color.FromRgb(r, g, b);
    }
}
