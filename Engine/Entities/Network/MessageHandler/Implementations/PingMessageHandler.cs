using System.Text.Json;
using Engine.Entities.Network.MessageHandler.Interfaces;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Provider.DTO;

namespace Engine.Entities.Network.MessageHandler.Implementations;

public class PingMessageHandler : IWebSocketMessageHandler
{
    public string Action => "PING";

    public async Task HandleAsync(AIProvider client, ProviderEnvelope<JsonElement> envelope, CancellationToken ct = default)
    {
        Console.WriteLine($"[PingMessageHandler] 📥 PING recibido [ID: {envelope.Id}]. Respondiendo PONG...");

        // Responde a la extensión con la acción "PONG" preservando el ID del mensaje
        var pongEnvelope = ProviderEnvelope<object>.Success("PONG", null, envelope.Id);
        await client.SendTextMessageAsync(pongEnvelope, ct: ct);
    }
}