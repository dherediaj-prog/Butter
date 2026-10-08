using System.Net;
using Engine.Entities.Network.MessageHandler;
using Engine.Entities.Network.Transport;
using Engine.Presentation.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class WebBootstrap
{
    public static WebApplication ConfigureButterWeb(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseWebSockets();

        var transport = app.Services.GetRequiredService<WebSocketTransport>();
        var dispatcher = app.Services.GetRequiredService<WebSocketMessageDispatcher>();
        var hotkeyListener = app.Services.GetRequiredService<NativeHotkeyListener>();

        // Endpoint principal de WebSocket para comunicación con la extensión/clientes
        app.Map("/ws/chat", async (HttpContext context) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await transport.HandleConnectionAsync(webSocket, dispatcher);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });
        return app;
    }
}