using Avalonia.Media;

namespace SkyUI.Core.Theming;

/// <summary>WCAG 2.x relative luminance and contrast helpers (validation / tests).</summary>
public static class SkyContrast
{
    public static double ContrastRatio(Color foreground, Color background)
    {
        var l1 = RelativeLuminance(foreground);
        var l2 = RelativeLuminance(background);
        var lighter = Math.Max(l1, l2);
        var darker = Math.Min(l1, l2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    public static bool MeetsAaText(Color foreground, Color background, bool largeText = false) =>
        ContrastRatio(foreground, background) >= (largeText ? 3.0 : 4.5);

    public static bool MeetsAaUi(Color foreground, Color background) =>
        ContrastRatio(foreground, background) >= 3.0;

    private static double RelativeLuminance(Color c)
    {
        static double Channel(byte channel)
        {
            var s = channel / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }
}
