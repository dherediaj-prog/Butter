using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class AppDIRegistration
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRouting();
        services.AddEngineDI();
        services.AddWindowsDI();
        return services;
    }
}