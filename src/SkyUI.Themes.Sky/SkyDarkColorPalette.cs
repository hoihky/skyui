namespace SkyUI.Themes.Sky;

using Avalonia.Styling;
using SkyUI.Core.Theming;

/// <summary>Dark color palette for the default <see cref="SkyPresetUris.Dark"/> theme preset.</summary>
public sealed class SkyDarkColorPalette : ISkyColorPalette
{
    public static SkyDarkColorPalette Instance { get; } = new();

    public string ResourceUri => SkyPaletteUris.Dark;
}

/// <summary>Light palette for <see cref="ThemeVariant.Light"/>.</summary>
public sealed class SkyLightColorPalette : ISkyColorPalette
{
    public static SkyLightColorPalette Instance { get; } = new();

    public string ResourceUri => SkyPaletteUris.Light;
}

/// <summary>High-contrast palette for <see cref="SkyThemeVariants.HighContrast"/>.</summary>
public sealed class SkyHighContrastColorPalette : ISkyColorPalette
{
    public static SkyHighContrastColorPalette Instance { get; } = new();

    public string ResourceUri => SkyPaletteUris.HighContrast;
}

/// <summary>Resource URIs for Sky theme palettes (merged before semantic tokens).</summary>
public static class SkyPaletteUris
{
    public const string Dark = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyPalette.Dark.axaml";
    public const string Light = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyPalette.Light.axaml";
    public const string HighContrast = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyPalette.HighContrast.axaml";
}
