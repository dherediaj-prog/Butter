using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class AppDIRegistration
{
    public static IServiceCollection AddAppServicesDI(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRouting();

        // Módulos agrupados por dominio
        services.AddPanelTriggers();
        services.AddOverlayServices();
        services.AddSelectionServices();
        services.AddHotkeyServices();
        services.AddScreenCaptureServices();
        services.AddNetworkServices();
        services.AddCommandServices(); // <-- Registro de Comandos y Selector de IA

        return services;
    }
}