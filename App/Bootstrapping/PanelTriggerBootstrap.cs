using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class PanelTriggerBootstrap
{
    public static void BootstrapPanelTrigger(this IServiceProvider provider)
    {
        // Resuelto automáticamente con sus comandos mediante DI
        var floatingTrigger = provider.GetRequiredService<FloatingTriggerForm>();
        floatingTrigger.Show();
    }
}