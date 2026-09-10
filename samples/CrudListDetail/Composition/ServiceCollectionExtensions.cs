using Microsoft.Extensions.DependencyInjection;
using SkyUI.Samples.CrudListDetail.Data;
using SkyUI.Samples.CrudListDetail.Services;
using SkyUI.Samples.CrudListDetail.ViewModels;

namespace SkyUI.Samples.CrudListDetail.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrudListDetailServices(this IServiceCollection services)
    {
        services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
        services.AddSingleton<CustomerGridDataSource>();
        services.AddSingleton<ISnackbarNotifier, SnackbarNotifier>();
        services.AddSingleton<ISnackbarHostAccessor, SnackbarHostAccessor>();

        services.AddSingleton<CustomerDetailViewModel>();
        services.AddSingleton<ShellViewModel>();

        return services;
    }
}
