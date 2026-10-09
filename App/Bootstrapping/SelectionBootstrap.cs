using Engine.Entities.Commands;
using Engine.Entities.Overlays;
using Engine.Entities.PanelTriggers;
using Engine.Entities.PanelTriggers.Enums;
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
        var overlay = provider.GetRequiredService<Overlay>();
        var panelTrigger = provider.GetRequiredService<PanelTrigger>();
        var captureService = provider.GetRequiredService<IScreenCaptureService>();
        var sendImageCommand = provider.GetRequiredService<SendImageCommand>();
        var hotkeyListener = provider.GetRequiredService<NativeHotkeyListener>();

        selection.SelectionRequested += () =>
        {
            hotkeyListener.BeginInvoke((Action)(() =>
            {
                if (hotkeyListener.IsDisposed || hotkeyListener.Disposing) return;

                using var selectionForm = provider.GetRequiredService<SelectionForm>();
                selectionForm.ShowDialog(hotkeyListener);
            }));
        };

        selection.Completed += async bounds =>
        {
            try
            {
                overlay.SetPosition(bounds.X, bounds.Y);
                panelTrigger.AnchorTo(bounds, AnchorAlignment.TopRight);
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
                    hotkeyListener,
                    $"No se pudo capturar o enviar la selección: {exception.Message}",
                    "ButterKnife",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };

        selection.Cancelled += () => { panelTrigger.Hide(); };
    }
}