using Avalonia.Styling;

namespace SkyUI.Themes.Sky;

/// <summary>Custom <see cref="ThemeVariant"/> keys used by Sky theme dictionaries.</summary>
public static class SkyThemeVariants
{
    /// <summary>WCAG-oriented high-contrast palette (<c>x:Key="HighContrast"</c> in <c>SkyResources.Themed.axaml</c>).</summary>
    public static readonly ThemeVariant HighContrast = new("HighContrast", ThemeVariant.Dark);
}
