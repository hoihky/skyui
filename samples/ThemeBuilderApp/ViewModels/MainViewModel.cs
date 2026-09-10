using SkyUI.Samples.Infrastructure.Mvvm;
using SkyUI.Samples.ThemeBuilderApp.Services;

namespace SkyUI.Samples.ThemeBuilderApp.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IThemeConfigurator themeConfigurator;
    private string themeVariant = "Dark";
    private string density = "Comfortable";
    private string accentHex = "#1ED760";

    public MainViewModel(IThemeConfigurator themeConfigurator)
    {
        this.themeConfigurator = themeConfigurator;
    }

    public IReadOnlyList<string> ThemeVariants { get; } = ["Dark", "Light", "HighContrast"];

    public IReadOnlyList<string> DensityOptions { get; } = ["Comfortable", "Compact"];

    public string ThemeVariant
    {
        get => themeVariant;
        set
        {
            if (!SetProperty(ref themeVariant, value))
                return;

            themeConfigurator.ApplyVariant(value);
        }
    }

    public string Density
    {
        get => density;
        set
        {
            if (!SetProperty(ref density, value))
                return;

            themeConfigurator.ApplyDensity(value);
        }
    }

    public string AccentHex
    {
        get => accentHex;
        set
        {
            if (!SetProperty(ref accentHex, value))
                return;

            themeConfigurator.ApplyAccent(value);
        }
    }
}
