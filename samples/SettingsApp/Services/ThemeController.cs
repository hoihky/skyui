using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SkyUI.Core;
using SkyUI.Core.Theming;
using SkyUI.Themes.Sky;

namespace SkyUI.Samples.SettingsApp.Services;

public sealed class ThemeController : IThemeController
{
    public void ApplyThemeVariant(string variantName)
    {
        if (Application.Current is not Application app)
            return;

        app.RequestedThemeVariant = variantName switch
        {
            "Light" => ThemeVariant.Light,
            "HighContrast" => SkyThemeVariants.HighContrast,
            _ => ThemeVariant.Dark,
        };
    }

    public void ApplyDensity(string densityName)
    {
        if (Application.Current is not Application app)
            return;

        var density = densityName == "Compact"
            ? SkyDensity.Compact
            : SkyDensity.Comfortable;

        SkyThemeProperties.SetDensity(app, density);
        SkyTheme.Apply(app, new SkyThemeOptions
        {
            AccentColor = SkyThemeProperties.GetAccentOverride(app),
            Density = density,
        });
    }

    public void ApplyAccent(string hexColor)
    {
        if (Application.Current is not Application app)
            return;

        if (!Color.TryParse(hexColor, out var color))
            return;

        SkyThemeProperties.SetAccentOverride(app, color);
        SkyTheme.Apply(app, new SkyThemeOptions
        {
            AccentColor = color,
            Density = SkyThemeProperties.GetDensity(app),
        });
    }
}
