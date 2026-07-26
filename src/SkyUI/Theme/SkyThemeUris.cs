namespace SkyUI.Theme;

/// <summary>
/// Avalonia style include URIs. Use <see cref="Theme"/> as the single preset include.
/// </summary>
public static class SkyThemeUris
{
  /// <summary>Single include: Sky Dark preset (palette, tokens, essentials).</summary>
  public const string Theme = "avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml";

  /// <summary>Preset plus <c>SkyUI.Data</c> control themes.</summary>
  public const string ThemeWithData = "avares://SkyUI.Data/Themes/SkyTheme.WithData.axaml";

  /// <summary>Same as <see cref="Theme"/>.</summary>
  public const string PresetDark = Theme;

  /// <summary>Back-compat alias.</summary>
  public const string Sky = Theme;

  /// <summary>Back-compat alias of <see cref="Theme"/>.</summary>
  public const string SkyTheme = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyTheme.axaml";

  /// <summary>Data controls only (grid, filter).</summary>
  public const string SkyData = "avares://SkyUI.Data/Themes/SkyDataTheme.axaml";

  /// <summary>Semantic tokens (requires a merged palette).</summary>
  public const string SkyTokens = "avares://SkyUI.Core/Themes/SkyTokens.axaml";

  /// <summary>Dark palette color slots.</summary>
  public const string SkyPaletteDark = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyPalette.Dark.axaml";
}
