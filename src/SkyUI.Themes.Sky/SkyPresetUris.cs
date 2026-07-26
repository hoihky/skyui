namespace SkyUI.Themes.Sky;

using SkyUI.Core.Theming;

/// <summary>Resource URIs for built-in Sky theme presets.</summary>
public static class SkyPresetUris
{
  /// <summary>
  /// Public theme include. Set <see cref="Avalonia.Application.RequestedThemeVariant"/> to
  /// <c>Dark</c>, <c>Light</c>, or <c>HighContrast</c> for palette + brush sets.
  /// </summary>
  public const string Theme = SkyTheme.IncludeUri;

  /// <summary>Obsolete name; use <see cref="Theme"/>.</summary>
  public const string Dark = Theme;
}
