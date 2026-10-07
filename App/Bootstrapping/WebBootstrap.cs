using System.Net;
using Engine.Presentation.Web.Services;
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

        var webHandler = app.Services.GetRequiredService<WebBridgeService>();
        var hotkeyListener = app.Services.GetRequiredService<NativeHotkeyListener>();

        app.Map("/ws/chat", webHandler.HandleWebSocketAsync);
        app.MapGet("/api/extension/download", webHandler.HandleExtensionDownload);
        app.MapPost("/api/shutdown", (HttpContext context) =>
        {
            if (context.Connection.RemoteIpAddress is not { } address || !IPAddress.IsLoopback(address))
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            hotkeyListener.BeginInvoke((Action)Application.Exit);
            return Results.Ok(new { message = "Shutting down..." });
        });

        return app;
    }
}
