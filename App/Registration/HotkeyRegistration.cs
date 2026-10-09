using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class HotkeyRegistration
{
    public static IServiceCollection AddHotkeyServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<NativeHotkeyListener>();

        return services;
    }
}