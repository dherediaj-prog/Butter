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

        // 1. Habilita el middleware nativo de WebSockets en ASP.NET Core
        app.UseWebSockets();

        // 2. Mapea la ruta HTTP especificada hacia el WebSocketTransport
        app.Map(routePath, async context =>
        {
            var transport = context.RequestServices.GetRequiredService<WebSocketTransport>();
            await transport.ProcessAsync(context);
        });

        return app;
    }
}