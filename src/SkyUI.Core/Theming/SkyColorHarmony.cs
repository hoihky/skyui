using Avalonia.Media;

namespace SkyUI.Core.Theming;

internal static class SkyColorHarmony
{
  /// <summary>Darken RGB channels toward black (matches legacy accent-pressed tuning).</summary>
  public static Color Darken(Color color, double amount)
  {
    amount = Math.Clamp(amount, 0, 1);
    var factor = 1 - amount;
    return Color.FromArgb(
        color.A,
        (byte)Math.Clamp(color.R * factor, 0, 255),
        (byte)Math.Clamp(color.G * factor, 0, 255),
        (byte)Math.Clamp(color.B * factor, 0, 255));
  }

  /// <summary>Accent selection tint (see SkyPaletteSelectedTint).</summary>
  public static Color AccentSelectionTint(Color accent) =>
        Color.FromArgb(0x33, accent.R, accent.G, accent.B);
}
