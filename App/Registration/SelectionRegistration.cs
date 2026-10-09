using Engine.Entities.Selections;
using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Registration;

public static class SelectionRegistration
{
    public static IServiceCollection AddSelectionServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<Selection>();
        services.AddTransient<SelectionForm>();

        return services;
    }
}