using Microsoft.Extensions.DependencyInjection;
using SkyUI.Samples.ThemeBuilderApp.Services;
using SkyUI.Samples.ThemeBuilderApp.ViewModels;

namespace SkyUI.Samples.ThemeBuilderApp.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddThemeBuilderServices(this IServiceCollection services)
    {
        services.AddSingleton<IThemeConfigurator, ThemeConfigurator>();
        services.AddTransient<MainViewModel>();
        return services;
    }
}
