using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SkyUI.Core;
using SkyUI.Core.Theming;
using SkyUI.Themes.Sky;

namespace SkyUI.Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        SkyTheme.Apply(this, new SkyThemeOptions
        {
            AccentColor = SkyThemeProperties.GetAccentOverride(this),
            Density = SkyThemeProperties.GetDensity(this),
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}