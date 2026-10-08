using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Transport;
using Engine.Entities.Overlays;
using Engine.Entities.PanelTriggers;
using Engine.Entities.Selections;
using Engine.Services.ScreenCapture;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class EngineDIRegistration
{
    public static IServiceCollection AddEngineDI(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // --- Entidades ---
        services.AddSingleton<Overlay>();
        services.AddSingleton<PanelTrigger>();
        services.AddSingleton<Selection>();

        // --- Servicios Web, WebSocket y AI ---
        services.AddSingleton<AIProviderManager>();
        services.AddSingleton<WebSocketTransport>();
        services.AddSingleton<MessageDispatcher>();

        // --- Captura de Pantalla ---
        services.AddSingleton<IScreenCaptureService, Win32ScreenCaptureService>();

        return services;
    }
}