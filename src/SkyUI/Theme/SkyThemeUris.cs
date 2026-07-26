namespace SkyUI.Theme;

/// <summary>
/// Default Avalonia style include URIs. Reference <see cref="SkyUI.Themes.Sky"/> and optional extension packages in the app.
/// </summary>
public static class SkyThemeUris
{
    /// <summary>Essentials theme: tokens, primitives, and SkyUI control templates.</summary>
    public const string Sky = "avares://SkyUI.Themes.Sky/Themes/Sky/SkyTheme.axaml";

    /// <summary>Data controls theme (grid, filter). Requires <c>SkyUI.Data</c>.</summary>
    public const string SkyData = "avares://SkyUI.Data/Themes/SkyDataTheme.axaml";
}
