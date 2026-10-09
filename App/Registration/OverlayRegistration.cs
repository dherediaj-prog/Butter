using Engine.Entities.ResponseDisplays;
using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class ResponseDisplayRegistration
{
    public static IServiceCollection AddResponseDisplayServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ResponseDisplay>();
        services.AddSingleton<ResponseDisplayForm>();

        return services;
    }
}