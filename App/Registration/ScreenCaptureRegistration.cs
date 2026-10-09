using Engine.Services.ScreenCapture;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class ScreenCaptureRegistration
{
    public static IServiceCollection AddScreenCaptureServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IScreenCaptureService, Win32ScreenCaptureService>();

        return services;
    }
}