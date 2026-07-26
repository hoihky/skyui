using Avalonia.Media;

namespace SkyUI.Icons;

/// <summary>24×24 path data for <see cref="SkyIcon"/> (scaled to 16 / 20 / 24 px).</summary>
public static class SkyIconGlyphs
{
    private static readonly IReadOnlyDictionary<SkyIconKind, string> PathData = new Dictionary<SkyIconKind, string>
    {
        [SkyIconKind.Play] = "M8,5 L19,12 L8,19 Z",
        [SkyIconKind.Pause] = "M6,4 H10 V20 H6 Z M14,4 H18 V20 H14 Z",
        [SkyIconKind.Search] =
            "M10,4 A6,6 0 1,1 9.99,4 Z M14.8,14.8 L20.5,20.5 L18.8,22.2 L13.1,16.5 Z",
        [SkyIconKind.Close] = "M6.4,6.4 L12,12 L17.6,6.4 L19,7.8 L13.4,13.4 L19,19 L17.6,20.4 L12,14.8 L6.4,20.4 L5,19 L10.6,13.4 L5,7.8 Z",
        [SkyIconKind.ChevronDown] = "M6,9 L12,15 L18,9 L16.6,7.6 L12,12.2 L7.4,7.6 Z",
        [SkyIconKind.ChevronRight] = "M9,6 L15,12 L9,18 L7.6,16.6 L12.2,12 L7.6,7.4 Z",
        [SkyIconKind.Check] = "M9,16.2 L4.8,12 L3.4,13.4 L9,19 L21,7 L19.6,5.6 Z",
        [SkyIconKind.Add] = "M11,5 H13 V19 H11 Z M5,11 H19 V13 H5 Z",
        [SkyIconKind.Remove] = "M5,11 H19 V13 H5 Z",
        [SkyIconKind.Home] = "M4,10.5 L12,4 L20,10.5 V20 H15 V14 H9 V20 H4 Z",
        [SkyIconKind.LayoutGrid] = "M4,4 H11 V11 H4 Z M13,4 H20 V11 H13 Z M4,13 H11 V20 H4 Z M13,13 H20 V20 H13 Z",
        [SkyIconKind.List] = "M4,6 H6 V8 H4 Z M4,11 H6 V13 H4 Z M4,16 H6 V18 H4 Z M9,6 H20 V8 H9 Z M9,11 H20 V13 H9 Z M9,16 H20 V18 H9 Z",
        [SkyIconKind.Settings] =
            "M12,2.5 L13.6,6.8 L18.2,5.6 L16.4,9.6 L20.5,11.2 L16.8,13.6 L17.8,18.2 L13.5,16.2 L12,20.5 L10.5,16.2 L6.2,18.2 L7.2,13.6 L3.5,11.2 L7.6,9.6 L5.8,5.6 L10.4,6.8 Z",
        [SkyIconKind.Filter] = "M4,5 H20 V7 H4 Z M6,11 H18 V13 H6 Z M9,17 H15 V19 H9 Z",
        [SkyIconKind.Table] = "M4,4 H20 V20 H4 Z M6,6 H18 V10 H6 Z M6,12 H11 V18 H6 Z M13,12 H18 V18 H13 Z",
        [SkyIconKind.Sliders] = "M4,6 H20 V8 H4 Z M7,11 H17 V13 H7 Z M10,16 H14 V18 H10 Z",
        [SkyIconKind.Image] =
            "M5,6 H19 V18 H5 Z M7,15 L10.5,12 L12.5,14 L16.5,9.5 L17.5,10.5 V15 H7 Z M8.75,8.75 A1.25,1.25 0 1,0 8.75,11.25 A1.25,1.25 0 1,0 8.75,8.75 Z",
        [SkyIconKind.Video] = "M4,6 H14 V18 H4 Z M16,9 L20,7 V17 L16,15 Z",
        [SkyIconKind.Layers] = "M12,3 L2,9 L12,15 L22,9 Z M12,11.5 L5.8,9 L12,6.5 L18.2,9 Z M4,14.5 L12,19.5 L20,14.5 V16.5 L12,21.5 L4,16.5 Z",
    };

    public static string? TryGetPathData(SkyIconKind kind) =>
        PathData.TryGetValue(kind, out var path) ? path : null;

    public static Geometry? TryGetGeometry(SkyIconKind kind)
    {
        var path = TryGetPathData(kind);
        return path is null ? null : Parse(path);
    }

    public static double ToPixels(SkyIconSize size) => (int)size;

    private static Geometry? Parse(string path)
    {
        try
        {
            return StreamGeometry.Parse(path);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
