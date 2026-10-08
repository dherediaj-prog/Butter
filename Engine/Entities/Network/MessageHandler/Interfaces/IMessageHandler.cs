using System.Text.Json;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Provider.DTO;

namespace Engine.Entities.Network.MessageHandler.Interfaces;

public interface IWebSocketMessageHandler
{
    /// <summary>
    /// Nombre de la acción que este manejador procesa (ej: "ASK", "STREAM", "CANCEL").
    /// </summary>
    string Action { get; }

    /// <summary>
    /// Ejecuta la lógica de negocio correspondiente a la acción.
    /// </summary>
    Task HandleAsync(AIProvider client, ProviderEnvelope<JsonElement> envelope, CancellationToken ct = default);
}