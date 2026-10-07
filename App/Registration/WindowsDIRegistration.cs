using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class WindowsDIRegistration
{
    public static IServiceCollection AddWindowsDI(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<PassiveOverlayForm>();
        services.AddSingleton<NativeHotkeyListener>();
        services.AddTransient<SelectionForm>();
        return services;
    }
}
