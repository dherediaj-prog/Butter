namespace App.Bootstrapping;

public static class AppBootstrap
{
    public static IServiceProvider BootstrapEngine(this IServiceProvider provider, int serverPort)
    {
        ArgumentNullException.ThrowIfNull(provider);

        // Bootstrap modular de subsistemas WinForms / Dominios
        provider.BootstrapOverlay(serverPort);
        provider.BootstrapSelection();
        provider.BootstrapPanelTrigger();
        provider.BootstrapHotkeys();

        return provider;
    }
}