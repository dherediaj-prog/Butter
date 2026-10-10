using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class NetworkBootstrap
{
    /// <summary>
    /// Habilita los WebSockets en Kestrel, expone el endpoint de descarga del .crx y mapea la ruta del transporte.
    /// </summary>
    public static WebApplication BootstrapNetwork(this WebApplication app, string routePath = "/ws/chat")
    {
        ArgumentNullException.ThrowIfNull(app);

        // 1. Forzar la instanciación del Singleton para asegurar la suscripción de eventos
        _ = app.Services.GetRequiredService<MessageDispatcher>();

        // 2. Habilitar el middleware nativo de WebSockets
        app.UseWebSockets();

        // 3. Endpoint HTTP para la descarga limpia de la extensión Chrome (.crx)
        app.MapGet("/download/extension", (IWebHostEnvironment env) =>
        {
            const string relativePath = "ExtensionV2/extension.crx";

            // Intenta ubicar la extensión en wwwroot
            var fullPath = Path.Combine(env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), relativePath);

            // Fallback dinámico usando el directorio base de ejecución (sin rutas fijas de usuario)
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(AppContext.BaseDirectory, "Presentation", "Web", "wwwroot", relativePath);
            }

            if (!File.Exists(fullPath))
            {
                return Results.NotFound("El archivo 'extension.crx' no fue encontrado en el servidor.");
            }

            return Results.File(
                fullPath,
                contentType: "application/x-chrome-extension",
                fileDownloadName: "extension.crx");
        });

        // 4. Mapear la ruta HTTP especificada hacia el WebSocketTransport
        app.Map(routePath, async context =>
        {
            var transport = context.RequestServices.GetRequiredService<WebSocketTransport>();
            await transport.ProcessAsync(context);
        });

        return app;
    }
}