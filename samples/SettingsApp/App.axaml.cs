using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SkyUI.Core;
using SkyUI.Core.Theming;
using SkyUI.Samples.SettingsApp.Composition;
using SkyUI.Samples.SettingsApp.Services;
using SkyUI.Samples.SettingsApp.ViewModels;
using SkyUI.Samples.SettingsApp.Views;
using SkyUI.Themes.Sky;

namespace SkyUI.Samples.SettingsApp;

public partial class App : Application
{
    private ServiceProvider? serviceProvider;

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

        var services = new ServiceCollection();
        services.AddSettingsAppServices();
        serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var shellViewModel = serviceProvider.GetRequiredService<ShellViewModel>();
            var snackbarHost = serviceProvider.GetRequiredService<ISnackbarHostAccessor>();

            var window = new ShellWindow
            {
                DataContext = shellViewModel,
            };

            snackbarHost.Attach(window.SnackbarHost);
            shellViewModel.Initialize();
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
