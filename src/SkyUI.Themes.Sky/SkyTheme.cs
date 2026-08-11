using Avalonia;
using Avalonia.Controls;
using SkyUI.Core;
using SkyUI.Core.Theming;

namespace SkyUI.Themes.Sky;

/// <summary>Sky theme preset API: include URI and optional <see cref="SkyThemeOptions"/> application.</summary>
public static class SkyTheme
{
  /// <summary>Single XAML include for the default Sky theme (<see cref="SkyPresetIds.ContentFirstDark"/> preset).</summary>
  public const string IncludeUri = "avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml";

  /// <summary>
  /// Applies <paramref name="options"/> to <paramref name="application"/> (e.g. accent override, density).
  /// Styles must still be loaded via <see cref="IncludeUri"/> in XAML or <see cref="SkyThemeUris.Theme"/>.
  /// Call <c>SkyUI.Fonts.SkyFonts.Configure</c> on <c>AppBuilder</c> before startup to load Inter + Noto Sans SC.
  /// </summary>
  public static void Apply(Application application, SkyThemeOptions? options = null)
  {
    options ??= SkyThemeOptions.Default;

    if (options.AccentColor is { } accent)
      SkyThemeProperties.SetAccentOverride(application, accent);
    else
      SkyThemeProperties.SetAccentOverride(application, null);

    SkyThemeProperties.SetDensity(application, options.Density);
  }
}
