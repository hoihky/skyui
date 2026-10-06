using Avalonia;
using Avalonia.Media;
namespace SkyUI.Demo.Models;

/// <summary>Procedural demo atlas: stable cel images keyed by sprite name and frame index.</summary>
public sealed class DemoSpriteAtlasCatalog
{
    private const int CelSize = 32;
    private readonly Dictionary<string, IImage> cache = new(StringComparer.Ordinal);

    public IImage ResolveCel(string? atlasId, string? spriteName, int frameIndex)
    {
        var atlas = string.IsNullOrEmpty(atlasId) ? "default" : atlasId;
        var name = string.IsNullOrEmpty(spriteName) ? "(empty)" : spriteName;
        var key = $"{atlas}:{name}:{frameIndex}";
        if (cache.TryGetValue(key, out var existing))
            return existing;

        var image = CreateCelImage(name, frameIndex);
        cache[key] = image;
        return image;
    }

    public IBrush ResolveBrush(string? spriteName) =>
        new SolidColorBrush(ResolveBaseColor(spriteName));

    private static IImage CreateCelImage(string spriteName, int frameIndex)
    {
        var baseColor = ResolveBaseColor(spriteName);
        var accent = ShiftColor(baseColor, frameIndex * 17);
        var drawing = new DrawingImage(
            new DrawingGroup
            {
                Children =
                {
                    new GeometryDrawing
                    {
                        Brush = new SolidColorBrush(baseColor),
                        Pen = new Pen(Brushes.White, 1.5),
                        Geometry = new RectangleGeometry(new Rect(1, 1, CelSize - 2, CelSize - 2)),
                    },
                    new GeometryDrawing
                    {
                        Brush = new SolidColorBrush(accent),
                        Geometry = new EllipseGeometry(new Rect(6 + frameIndex % 4, 8, 14, 14)),
                    },
                },
            });
        return drawing;
    }

    private static Color ResolveBaseColor(string? spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return Color.Parse("#444444");
        var hash = spriteName.GetHashCode(StringComparison.Ordinal);
        var r = (byte)(80 + (hash & 0x7F));
        var g = (byte)(80 + ((hash >> 8) & 0x7F));
        var b = (byte)(80 + ((hash >> 16) & 0x7F));
        return Color.FromRgb(r, g, b);
    }

    private static Color ShiftColor(Color color, int delta)
    {
        var r = (byte)Math.Clamp(color.R + delta % 40, 0, 255);
        var g = (byte)Math.Clamp(color.G + (delta / 2) % 40, 0, 255);
        var b = (byte)Math.Clamp(color.B + (delta / 3) % 40, 0, 255);
        return Color.FromRgb(r, g, b);
    }
}
