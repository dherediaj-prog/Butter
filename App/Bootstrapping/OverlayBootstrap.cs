using Engine.Entities.Overlays;
using Engine.Helpers;
using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class OverlayBootstrap
{
    public static void BootstrapOverlay(this IServiceProvider provider, int serverPort)
    {
        var overlay = provider.GetRequiredService<Overlay>();
        var passiveOverlay = provider.GetRequiredService<PassiveOverlayForm>();

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
        passiveOverlay.Show();
    }
}