namespace SkyUI.Themes.Sky;

using SkyUI.Core.Theming;

/// <summary>Resource URIs for built-in Sky theme presets.</summary>
public static class SkyPresetUris
{
    /// <summary>
    /// Default public theme include. Loads <see cref="ContentFirstDark"/> preset.
    /// Set <see cref="Avalonia.Application.RequestedThemeVariant"/> to
    /// <c>Dark</c>, <c>Light</c>, or <c>HighContrast</c> for palette + brush sets.
    /// </summary>
    public const string Theme = SkyTheme.IncludeUri;

    /// <summary>Content-first dark control primitives and resources (<see cref="SkyPresetIds.ContentFirstDark"/>).</summary>
    public const string ContentFirstDark =
        "avares://SkyUI.Themes.Sky/Themes/SkyDark/Presets/ContentFirstDark/ContentFirstDark.axaml";

    /// <summary>Obsolete; use <see cref="Theme"/> or <see cref="ContentFirstDark"/>.</summary>
    public const string Dark = Theme;
}
