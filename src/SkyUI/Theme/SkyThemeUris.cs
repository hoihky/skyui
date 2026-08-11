namespace SkyUI.Theme;

/// <summary>
/// Avalonia style include URIs. Public brand is <b>Sky</b>; default visual preset is Content-first dark.
/// </summary>
public static class SkyThemeUris
{
    /// <summary>Default Sky theme include (<see cref="SkyUI.Themes.Sky.SkyPresetIds.ContentFirstDark"/>).</summary>
    public const string Theme = "avares://SkyUI.Themes.Sky/Themes/SkyTheme.axaml";

    /// <summary>Preset plus <c>SkyUI.Data</c> control themes.</summary>
    public const string ThemeWithData = "avares://SkyUI.Data/Themes/SkyTheme.WithData.axaml";

    /// <summary>Content-first dark preset styles and resources.</summary>
    public const string ContentFirstDark =
        "avares://SkyUI.Themes.Sky/Themes/SkyDark/Presets/ContentFirstDark/ContentFirstDark.axaml";

    /// <summary>Same as <see cref="Theme"/>.</summary>
    public const string PresetDark = Theme;

    /// <summary>Same as <see cref="Theme"/>.</summary>
    public const string Sky = Theme;

    /// <summary>Legacy path; redirects to <see cref="Theme"/>.</summary>
    public const string SkyTheme = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyTheme.axaml";

    /// <summary>Data controls only (grid, filter).</summary>
    public const string SkyData = "avares://SkyUI.Data/Themes/SkyDataTheme.axaml";

    /// <summary>Semantic tokens (requires a merged palette).</summary>
    public const string SkyTokens = "avares://SkyUI.Core/Themes/SkyTokens.axaml";

    /// <summary>Dark palette color slots (<c>Themes/SkyDark/</c>).</summary>
    public const string SkyPaletteDark =
        "avares://SkyUI.Themes.Sky/Themes/SkyDark/SkyPalette.Dark.axaml";

    /// <summary>Obsolete path; redirects to <see cref="SkyPaletteDark"/>.</summary>
    public const string SkyPaletteDarkLegacy =
        "avares://SkyUI.Themes.Sky/Themes/Sky/SkyPalette.Dark.axaml";
}
