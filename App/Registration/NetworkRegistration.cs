using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class NetworkRegistration
{
    public static IServiceCollection AddNetworkServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<AIProviderManager>();
        services.AddSingleton<MessageDispatcher>();
        services.AddSingleton<WebSocketTransport>();

        return services;
    }
}