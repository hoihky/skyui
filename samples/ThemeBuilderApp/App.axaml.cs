using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SkyUI.Core;
using SkyUI.Core.Theming;
using SkyUI.Samples.ThemeBuilderApp.Composition;
using SkyUI.Samples.ThemeBuilderApp.ViewModels;
using SkyUI.Samples.ThemeBuilderApp.Views;
using SkyUI.Themes.Sky;

namespace SkyUI.Samples.ThemeBuilderApp;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        SkyTheme.Apply(this, new SkyThemeOptions
        {
            AccentColor = SkyThemeProperties.GetAccentOverride(this),
            Density = SkyThemeProperties.GetDensity(this),
        });

        var services = new ServiceCollection();
        services.AddThemeBuilderServices();
        serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
