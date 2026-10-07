using App.Bootstrapping;
using App.Registration;
using Engine.Presentation.Windows;
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
            throw new DirectoryNotFoundException($"No se encontró el contenido web: {webRootPath}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = webRootPath
        });
        builder.WebHost.UseUrls($"http://0.0.0.0:{ServerPort}");
        builder.Services.AddAppServices();

        await using var app = builder.Build();
        app.ConfigureButterWeb();
        app.Services.BootstrapEngine(ServerPort);

        await app.StartAsync();
        try
        {
            Application.Run(app.Services.GetRequiredService<NativeHotkeyListener>());
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
