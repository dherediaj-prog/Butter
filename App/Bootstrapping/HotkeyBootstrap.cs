using Engine.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace App.Bootstrapping;

public static class HotkeyBootstrap
{
    public static void BootstrapHotkeys(this IServiceProvider provider)
    {
        var hotkeyListener = provider.GetRequiredService<NativeHotkeyListener>();
        _ = hotkeyListener.Handle;
    }
}