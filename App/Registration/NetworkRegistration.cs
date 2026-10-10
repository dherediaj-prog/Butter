using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.MessageHandler.Implementations;
using Engine.Entities.Network.MessageHandler.Interfaces;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class NetworkRegistration
{
    public static IServiceCollection AddNetworkServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 1. Registrar Handlers de mensajes entrantes de WebSocket
        services.AddSingleton<IWebSocketMessageHandler, AIResponseMessageHandler>();
        services.AddSingleton<IWebSocketMessageHandler, PingMessageHandler>(); // <--- Registrado

        // 2. Registrar Servicios Nucleares de Red
        services.AddSingleton<AIProviderManager>();
        services.AddSingleton<MessageDispatcher>();
        services.AddSingleton<WebSocketTransport>();

        return services;
    }
}