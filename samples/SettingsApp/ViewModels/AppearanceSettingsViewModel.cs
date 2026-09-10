using SkyUI.Samples.Infrastructure.Mvvm;
using SkyUI.Samples.Infrastructure.Navigation;
using SkyUI.Samples.SettingsApp.Models;
using SkyUI.Samples.SettingsApp.Services;

namespace SkyUI.Samples.SettingsApp.ViewModels;

public sealed class AppearanceSettingsViewModel : ViewModelBase, INavigationPage
{
    private readonly IThemeController themeController;
    private string themeVariant = "Dark";
    private string density = "Comfortable";
    private string accentHex = "#1ED760";

    public AppearanceSettingsViewModel(IThemeController themeController)
    {
        this.themeController = themeController;
    }

    public string Title => "Appearance";

    public string ThemeVariant
    {
        get => themeVariant;
        set
        {
            if (!SetProperty(ref themeVariant, value))
                return;

            themeController.ApplyThemeVariant(value);
        }
    }

    public string Density
    {
        get => density;
        set
        {
            if (!SetProperty(ref density, value))
                return;

            themeController.ApplyDensity(value);
        }
    }

    public string AccentHex
    {
        get => accentHex;
        set
        {
            if (!SetProperty(ref accentHex, value))
                return;

            themeController.ApplyAccent(value);
        }
    }

    public void LoadFrom(AppearanceSettings settings)
    {
        themeVariant = settings.ThemeVariant;
        density = settings.Density;
        accentHex = settings.AccentHex;
        RaisePropertyChanged(nameof(ThemeVariant));
        RaisePropertyChanged(nameof(Density));
        RaisePropertyChanged(nameof(AccentHex));

        themeController.ApplyThemeVariant(themeVariant);
        themeController.ApplyDensity(density);
        themeController.ApplyAccent(accentHex);
    }

    public void ApplyTo(AppearanceSettings settings)
    {
        settings.ThemeVariant = ThemeVariant;
        settings.Density = Density;
        settings.AccentHex = AccentHex;
    }
}
