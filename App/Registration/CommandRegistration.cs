using Engine.Entities.Commands;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Services.Prompt;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class CommandRegistration
{
    public static IServiceCollection AddCommandServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Selector de Proveedor Activo
        services.AddSingleton<ActiveAIProviderSelector>();

        // Comandos de IA
        services.AddSingleton<SystemPromptService>();
        services.AddSingleton<SetSystemPromptCommand>();
        services.AddSingleton<SendPromptCommand>();

        return services;
    }
}