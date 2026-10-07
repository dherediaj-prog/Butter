using Engine.Entities.Overlays;
using Engine.Entities.Selections;
using Engine.Presentation.Web.Services;
using Engine.Services.ScreenCapture;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class EngineDIRegistration
{
    public static IServiceCollection AddEngineDI(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<Overlay>();
        services.AddSingleton<Selection>();
        services.AddSingleton<WebBridgeService>();
        services.AddSingleton<IScreenCaptureService, Win32ScreenCaptureService>();
        return services;
    }
}