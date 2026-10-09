using Engine.Entities.Overlays;
using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class OverlayRegistration
{
    public static IServiceCollection AddOverlayServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<Overlay>();
        services.AddSingleton<PassiveOverlayForm>();

        return services;
    }
}