using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.Samples.SettingsApp.Services;

/// <summary>Applies runtime theme options to the application host.</summary>
public interface IThemeController
{
    void ApplyThemeVariant(string variantName);

    void ApplyDensity(string densityName);

    void ApplyAccent(string hexColor);
}
