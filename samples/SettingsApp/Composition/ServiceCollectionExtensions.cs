using Microsoft.Extensions.DependencyInjection;
using SkyUI.Samples.SettingsApp.Services;
using SkyUI.Samples.SettingsApp.ViewModels;

namespace SkyUI.Samples.SettingsApp.Composition;

/// <summary>Composition root for the settings sample (dependency inversion).</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsAppServices(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsStore, JsonFileSettingsStore>();
        services.AddSingleton<IThemeController, ThemeController>();
        services.AddSingleton<ISnackbarNotifier, SnackbarNotifier>();
        services.AddSingleton<ISnackbarHostAccessor, SnackbarHostAccessor>();

        services.AddSingleton<ProfileSettingsViewModel>();
        services.AddSingleton<AppearanceSettingsViewModel>();
        services.AddSingleton<NotificationSettingsViewModel>();
        services.AddSingleton<ShellViewModel>();

        return services;
    }
}
