using App.Bootstrapping;
using App.Registration;
using Engine.Services.Firewall;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace App;

internal static class Program
{
    private const int ServerPort = 5000;

    [STAThread]
    private static async Task Main()
    {
        ApplicationConfiguration.Initialize();
        FirewallService.EnsurePortOpen(ServerPort);

        var webRootPath = Path.Combine(AppContext.BaseDirectory, "Presentation.Web", "wwwroot");
        if (!Directory.Exists(webRootPath))
        {
            Directory.CreateDirectory(webRootPath);
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = webRootPath
        });

        builder.WebHost.UseUrls($"http://0.0.0.0:{ServerPort}");

        // Registro modular por dominios
        builder.Services.AddAppServicesDI();

        await using var app = builder.Build();

        // Mapeo del transporte WebSocket (/ws/chat)
        //app.MapWebSocketTransport();

        // Inicialización de UI y motores
        app.Services.BootstrapEngine(ServerPort);

        await app.StartAsync();

        try
        {
            // Bucle nativo de mensajes sin dependencia de ningún Form principal
            Application.Run();
        }
        finally
        {
            await app.StopAsync();
        }
    }
}