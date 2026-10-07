using Engine.Entities.Overlays;
using Engine.Entities.Selections;
using Engine.Helpers;
using Engine.Presentation.Web.Services;
using Engine.Presentation.Windows;
using Engine.Services.ScreenCapture;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class EngineBootstrap
{
    public static IServiceProvider BootstrapEngine(this IServiceProvider provider, int serverPort)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var overlay = provider.GetRequiredService<Overlay>();
        var selection = provider.GetRequiredService<Selection>();
        var webHandler = provider.GetRequiredService<WebBridgeService>();
        var captureService = provider.GetRequiredService<IScreenCaptureService>();
        var passiveOverlay = provider.GetRequiredService<PassiveOverlayForm>();
        var hotkeyListener = provider.GetRequiredService<NativeHotkeyListener>();

        var localIp = NetworkHelper.GetLocalIPAddress();
        overlay.UpdateResponseText(
            "=== CONFIGURACIÓN DE CONEXIÓN ===\n" +
            "1. Abre Kiwi Browser / Firefox (Android) y entra a Gemini con tu cuenta activa.\n" +
            $"2. Navega a: http://{localIp}:{serverPort}/index.html\n" +
            "3. Descarga e instala la extensión, luego reinicia el navegador.\n" +
            $"4. Abre la extensión, ingresa la IP ({localIp}) y Puerto ({serverPort}), y presiona 'Conectar'.\n\n" +
            "[ ATAJOS ]\n" +
            "• F8: Iniciar selección / Ubicar texto\n" +
            "• Shift + F8: Cerrar programa");
        overlay.SetVisibility(true);

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
                var imageBytes = captureService.CaptureRegion(bounds);
                if (imageBytes.Length > 0)
                    await webHandler.SendImageAsync(imageBytes);
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

        passiveOverlay.Show();
        _ = hotkeyListener.Handle;
        return provider;
    }
}
