using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class NetworkBootstrap
{
    public static WebApplication BootstrapNetwork(this WebApplication app, string routePath = "/ws/chat")
    {
        ArgumentNullException.ThrowIfNull(app);

        // 1. Forzar la instanciación del Singleton
        _ = app.Services.GetRequiredService<MessageDispatcher>();

        // 2. Habilitar WebSockets
        app.UseWebSockets();

        // 3. Mapeo de la página del detector
        app.MapGet("/detector", ServeDetectorPage);
        app.MapGet("/test", ServeDetectorPage);
        app.MapGet("/", ServeDetectorPage);

        // 4. Endpoint HTTP para la descarga del archivo ZIP estático
        app.MapGet("/download/extension", (IWebHostEnvironment env, HttpContext context) =>
        {
            // Evitar que el navegador use una versión en caché bloqueada anteriormente
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            context.Response.Headers.Expires = "0";

            // Apuntamos directamente al archivo ZIP que creaste
            const string relativePath = "ExtensionV2/ExtensionV2.zip";
            var fullPath = Path.Combine(env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), relativePath);

            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(AppContext.BaseDirectory, "Presentation", "Web", "wwwroot", relativePath);
            }

            if (!File.Exists(fullPath))
            {
                return Results.NotFound("El archivo 'ExtensionV2.zip' no fue encontrado en el servidor.");
            }

            // Retornamos el archivo ZIP directamente
            return Results.File(
                fullPath,
                contentType: "application/zip",
                fileDownloadName: "ExtensionV2.zip");
        });

        // 5. Mapear WebSocketTransport
        app.Map(routePath, async context =>
        {
            var transport = context.RequestServices.GetRequiredService<WebSocketTransport>();
            await transport.ProcessAsync(context);
        });

        return app;
    }

    private static IResult ServeDetectorPage(IWebHostEnvironment env)
    {
        var relativePath = Path.Combine("Test", "index.html");
        var fullPath = Path.Combine(env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), relativePath);

        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(AppContext.BaseDirectory, "Presentation", "Web", "wwwroot", relativePath);
        }

        if (!File.Exists(fullPath))
        {
            return Results.NotFound("No se encontró el archivo 'Test/index.html' en el servidor.");
        }

        return Results.File(fullPath, "text/html");
    }
}