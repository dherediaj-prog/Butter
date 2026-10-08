using System.Collections.Concurrent;
using System.Net.WebSockets;
using Engine.Entities.Network.Transport;

namespace Engine.Entities.Network.Provider;

public class AIProviderManager
{
    private readonly ConcurrentDictionary<WebSocket, AIProvider> _providers = new();

    public event Action<AIProvider>? OnProviderRegistered;
    public event Action<AIProvider>? OnProviderUnregistered;

    public AIProviderManager(WebSocketTransport webSocketTransport)
    {
        webSocketTransport.OnConnected += HandleConnectedAsync;
        webSocketTransport.OnDisconnected += HandleDisconnectedAsync;
    }

    private async Task HandleConnectedAsync(WebSocket socket, CancellationToken ct)
    {
        using var provider = new AIProvider(socket);

        if (_providers.TryAdd(socket, provider))
        {
            OnProviderRegistered?.Invoke(provider);

            try
            {
                await provider.ListenAsync(ct);
            }
            finally
            {
                if (_providers.TryRemove(socket, out _))
                {
                    OnProviderUnregistered?.Invoke(provider);
                }
            }
        }
    }

    private Task HandleDisconnectedAsync(WebSocket socket)
    {
        // En caso de que se haya eliminado previamente en HandleConnectedAsync, garantiza limpieza
        if (_providers.TryRemove(socket, out var provider))
        {
            OnProviderUnregistered?.Invoke(provider);
            provider.Dispose();
        }

        return Task.CompletedTask;
    }

    public AIProvider? GetBySocket(WebSocket socket) => _providers.GetValueOrDefault(socket);
    public AIProvider? GetById(string id) => _providers.Values.FirstOrDefault(p => p.Id == id);
    public IEnumerable<AIProvider> GetConnected() => _providers.Values.Where(p => p.IsConnected);
}