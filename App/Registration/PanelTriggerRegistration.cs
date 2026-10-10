using Engine.Entities.PanelTriggers;
using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class PanelTriggerRegistration
{
    public static IServiceCollection AddPanelTriggers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<PanelTrigger>();
        services.AddTransient<SystemPromptControl>();
        services.AddTransient<FloatingTriggerForm>();

        return services;
    }
}