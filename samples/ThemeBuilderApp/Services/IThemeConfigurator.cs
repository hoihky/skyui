namespace SkyUI.Samples.ThemeBuilderApp.Services;

/// <summary>Applies Sky theme options without copying theme XAML.</summary>
public interface IThemeConfigurator
{
    void ApplyVariant(string variantName);

    void ApplyDensity(string densityName);

    void ApplyAccent(string hexColor);
}
