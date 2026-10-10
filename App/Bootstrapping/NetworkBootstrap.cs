using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class NetworkBootstrap
{
    /// <summary>
    /// Habilita los WebSockets en Kestrel y mapea la ruta de escucha del transporte.
    /// </summary>
    public static WebApplication BootstrapNetwork(this WebApplication app, string routePath = "/ws/chat")
    {
        ArgumentNullException.ThrowIfNull(app);

        // 1. Forzar la instanciación del Singleton para que ejecute su constructor
        _ = app.Services.GetRequiredService<MessageDispatcher>();

        // 2. Habilita los WebSockets
        app.UseWebSockets();

        // 3. Mapea la ruta HTTP
        app.Map(routePath, async context =>
        {
            var transport = context.RequestServices.GetRequiredService<WebSocketTransport>();
            await transport.ProcessAsync(context);
        });

        return app;
    }
}