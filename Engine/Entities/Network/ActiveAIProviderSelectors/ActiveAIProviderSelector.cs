using Engine.Entities.Network.Provider;

namespace Engine.Entities.Network.ActiveAIProviderSelectors;

public class ActiveAIProviderSelector
{
    private readonly AIProviderManager _providerManager;
    private readonly object _lock = new();

    public AIProvider? ActiveProvider { get; private set; }

    /// <summary>
    /// Evento que se dispara cuando cambia el proveedor activo.
    /// </summary>
    public event Action<AIProvider?>? OnActiveProviderChanged;

    public ActiveAIProviderSelector(AIProviderManager providerManager)
    {
        ArgumentNullException.ThrowIfNull(providerManager);
        _providerManager = providerManager;

        _providerManager.OnProviderRegistered += HandleProviderRegistered;
        _providerManager.OnProviderUnregistered += HandleProviderUnregistered;
    }

    public bool Select(AIProvider? provider)
    {
        lock (_lock)
        {
            if (ActiveProvider == provider) return false;

            ActiveProvider = provider;
            OnActiveProviderChanged?.Invoke(ActiveProvider);
            return true;
        }
    }

    public bool SelectById(string providerId)
    {
        var provider = _providerManager.GetById(providerId);
        return provider is not null && Select(provider);
    }

    public void Clear() => Select(null);

    /// <summary>
    /// Obtiene el proveedor activo garantizando que esté instanciado y conectado.
    /// </summary>
    public AIProvider? GetActiveConnectedProvider()
    {
        lock (_lock)
        {
            return ActiveProvider is { IsConnected: true } ? ActiveProvider : null;
        }
    }

    private void HandleProviderRegistered(AIProvider provider)
    {
        lock (_lock)
        {
            if (ActiveProvider is null)
            {
                Select(provider);
            }
        }
    }

    private void HandleProviderUnregistered(AIProvider provider)
    {
        lock (_lock)
        {
            if (ActiveProvider != provider) return;

            var fallback = _providerManager.GetConnected().FirstOrDefault(p => p != provider);
            Select(fallback);
        }
    }
}