using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SkyUI.Core.Theming;

internal static class SkyAccentOverrideApplicator
{
  private static readonly string[] PaletteKeys =
  [
      SkyPaletteKeys.Accent,
      SkyPaletteKeys.AccentPressed,
      SkyPaletteKeys.SelectedTint,
  ];

  private static readonly string[] BrushKeys =
  [
      SkyTokenKeys.Brush.Accent,
      SkyTokenKeys.Brush.AccentPressed,
      SkyTokenKeys.Brush.SelectedTint,
      SkyResourceKeys.SpotifyAccentGreen,
      "SpotifyAccentGreenDarkBrush",
  ];

  public static void Apply(IResourceHost host, Color? accent)
  {
    var dictionary = ResolveDictionary(host);
    if (dictionary is null)
      return;

    if (accent is null)
    {
      ClearOverrides(dictionary);
      return;
    }

    var pressed = SkyColorHarmony.Darken(accent.Value, 0.06);
    var selection = SkyColorHarmony.AccentSelectionTint(accent.Value);

    dictionary[SkyPaletteKeys.Accent] = accent.Value;
    dictionary[SkyPaletteKeys.AccentPressed] = pressed;
    dictionary[SkyPaletteKeys.SelectedTint] = selection;

    dictionary[SkyTokenKeys.Brush.Accent] = new SolidColorBrush(accent.Value);
    dictionary[SkyTokenKeys.Brush.AccentPressed] = new SolidColorBrush(pressed);
    dictionary[SkyTokenKeys.Brush.SelectedTint] = new SolidColorBrush(selection);

    dictionary[SkyResourceKeys.SpotifyAccentGreen] = new SolidColorBrush(accent.Value);
    dictionary["SpotifyAccentGreenDarkBrush"] = new SolidColorBrush(pressed);
    dictionary["SpotifySelectedTintBrush"] = new SolidColorBrush(selection);
  }

  private static IResourceDictionary? ResolveDictionary(IResourceHost host) =>
      host is Application app ? app.Resources : null;

  private static void ClearOverrides(IResourceDictionary dictionary)
  {
    foreach (var key in PaletteKeys)
      dictionary.Remove(key);

    foreach (var key in BrushKeys)
      dictionary.Remove(key);

    dictionary.Remove("SpotifySelectedTintBrush");
  }
}
