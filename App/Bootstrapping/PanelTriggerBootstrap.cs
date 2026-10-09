using Engine.Entities.Commands;
using Engine.Entities.PanelTriggers;
using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class PanelTriggerBootstrap
{
    public static void BootstrapPanelTrigger(this IServiceProvider provider)
    {
        var panelTrigger = provider.GetRequiredService<PanelTrigger>();
        var floatingTrigger = provider.GetRequiredService<FloatingTriggerForm>();
        var sendPromptCommand = provider.GetRequiredService<SendPromptCommand>();
        /*var sendCancelCommand = provider.GetRequiredService<SendCancelCommand>();*/

        floatingTrigger.OnActionExecuted += async actionKey =>
        {
            switch (actionKey)
            {
                case "SEND_PROMPT":
                    await sendPromptCommand.ExecuteAsync("Procesa la selección actual");
                    break;

                case "CAPTURE_SCREEN":
                    // Disparar flujo de captura si es requerido
                    break;

                case "CANCEL":
                    /*await sendCancelCommand.ExecuteAsync();*/
                    panelTrigger.Hide();
                    break;
            }
        };

        floatingTrigger.Show();
    }
}