using Avalonia.Media;

namespace SkyUI.Core.Theming;

/// <summary>Runtime configuration for <see cref="SkyTheme"/> (accent, optional extensions).</summary>
public sealed class SkyThemeOptions
{
  /// <summary>Default options (preset only; accent from palette).</summary>
  public static SkyThemeOptions Default { get; } = new();

  /// <summary>When set, overrides semantic accent brushes on the target <see cref="Avalonia.Application"/>.</summary>
  public Color? AccentColor { get; init; }

  /// <summary>Comfortable (default) or compact control metrics.</summary>
  public SkyDensity Density { get; init; } = SkyDensity.Comfortable;

  /// <summary>
  /// When true, callers should also include the data-controls theme
  /// (<c>SkyThemeUris.Data</c> / <c>SkyDataTheme.axaml</c>). Not merged automatically from the preset package.
  /// </summary>
  public bool IncludeDataControlsTheme { get; init; }
}
