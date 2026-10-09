using Engine.Entities.Commands;
using Engine.Entities.PanelTriggers;
using Engine.Entities.ResponseDisplays;
using Engine.Entities.Selections;
using Engine.Presentation.Windows;
using Engine.Services.ScreenCapture;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class SelectionBootstrap
{
    public static void BootstrapSelection(this IServiceProvider provider)
    {
        var selection = provider.GetRequiredService<Selection>();
        var responseDisplay = provider.GetRequiredService<ResponseDisplay>();
        var panelTrigger = provider.GetRequiredService<PanelTrigger>();
        var captureService = provider.GetRequiredService<IScreenCaptureService>();
        var sendImageCommand = provider.GetRequiredService<SendImageCommand>();

        selection.SelectionRequested += () =>
        {
            using var selectionForm = provider.GetRequiredService<SelectionForm>();
            selectionForm.ShowDialog();
        };

        selection.Completed += async bounds =>
        {
            try
            {
                responseDisplay.SetPosition(bounds.X, bounds.Y);

                // Ubicación en la esquina superior derecha del área seleccionada
                int triggerX = bounds.Right - panelTrigger.Size.Width;
                int triggerY = bounds.Top - panelTrigger.Size.Height - 6;

                panelTrigger.SetPosition(triggerX, triggerY);
                panelTrigger.Show();

                var imageBytes = captureService.CaptureRegion(bounds);
                if (imageBytes.Length > 0)
                {
                    await sendImageCommand.ExecuteAsync(new SendImageArgs(imageBytes));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"No se pudo capturar o enviar la selección: {exception.Message}",
                    "ButterKnife",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };

        selection.Cancelled += () => { panelTrigger.Hide(); };
    }
}