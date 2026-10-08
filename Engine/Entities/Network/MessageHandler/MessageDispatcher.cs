using System.Collections.Concurrent;
using System.Text.Json;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.MessageHandler.Interfaces;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Provider.DTO;

namespace Engine.Entities.Network.MessageHandler;

public class WebSocketMessageDispatcher
{
    private readonly ConcurrentDictionary<string, IWebSocketMessageHandler> _handlers =
        new(StringComparer.OrdinalIgnoreCase);

    private AIProvider? _activeProvider;
    private Func<ProviderEnvelope<JsonElement>, Task>? _activeCallback;

    public WebSocketMessageDispatcher(
        ActiveAIProviderSelector activeProviderSelector,
        IEnumerable<IWebSocketMessageHandler>? handlers = null)
    {
        ArgumentNullException.ThrowIfNull(activeProviderSelector);

        if (handlers is not null)
        {
            foreach (var handler in handlers)
            {
                RegisterHandler(handler);
            }
        }

        // Suscribirse a los cambios de selección
        activeProviderSelector.OnActiveProviderChanged += HandleActiveProviderChanged;

        if (activeProviderSelector.ActiveProvider is not null)
        {
            HandleActiveProviderChanged(activeProviderSelector.ActiveProvider);
        }
    }

    public WebSocketMessageDispatcher RegisterHandler(IWebSocketMessageHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers[handler.Action] = handler;
        return this;
    }

    private void HandleActiveProviderChanged(AIProvider? newActiveProvider)
    {
        // 1. Desuscribir del proveedor activo previo si existía
        if (_activeProvider is not null && _activeCallback is not null)
        {
            _activeProvider.OnTextMessageReceived -= _activeCallback;
            _activeProvider = null;
            _activeCallback = null;
        }

        // 2. Suscribir únicamente al nuevo proveedor activo
        if (newActiveProvider is not null)
        {
            _activeProvider = newActiveProvider;
            _activeCallback = envelope => DispatchAsync(newActiveProvider, envelope);
            _activeProvider.OnTextMessageReceived += _activeCallback;
        }
    }

    public async Task DispatchAsync(
        AIProvider client,
        ProviderEnvelope<JsonElement> envelope,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(envelope.Action))
        {
            await client.SendTextMessageAsync(
                ProviderEnvelope<object>.Fail("UNKNOWN", "La acción recibida no puede estar vacía", 400, envelope.Id),
                ct: ct);
            return;
        }

        if (_handlers.TryGetValue(envelope.Action, out var handler))
        {
            try
            {
                await handler.HandleAsync(client, envelope, ct);
            }
            catch (Exception ex)
            {
                await client.SendTextMessageAsync(
                    ProviderEnvelope<object>.Fail(
                        envelope.Action,
                        $"Error interno al procesar la acción: {ex.Message}",
                        500,
                        envelope.Id),
                    ct: ct);
            }
        }
        else
        {
            await client.SendTextMessageAsync(
                ProviderEnvelope<object>.Fail(
                    envelope.Action,
                    $"La acción '{envelope.Action}' no está registrada en el servidor",
                    404,
                    envelope.Id),
                ct: ct);
        }
    }
}