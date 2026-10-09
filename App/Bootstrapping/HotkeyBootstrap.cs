using Engine.Entities.ResponseDisplays;
using Engine.Entities.Selections;
using Engine.Services.Hotkey;
using Engine.Services.Hotkey.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class HotkeyBootstrap
{
    public static void BootstrapHotkeys(this IServiceProvider provider)
    {
        var hotkeyService = provider.GetRequiredService<HotkeyService>();
        var selection = provider.GetRequiredService<Selection>();
        var responseDisplay = provider.GetRequiredService<ResponseDisplay>();

        // F8 -> Iniciar Selección y Captura
        hotkeyService.Register(HotkeyModifiers.None, Keys.F8, () => selection.RequestSelection());

        // F9 -> Mover Posición del ResponseDisplay al Cursor (0ms)
        hotkeyService.Register(HotkeyModifiers.None, Keys.F9, () => responseDisplay.SetPosition(Cursor.Position.X, Cursor.Position.Y));

        // Shift + F8 -> Cierre Total del Programa
        hotkeyService.Register(HotkeyModifiers.Shift, Keys.F8, () => Application.Exit());
    }
}