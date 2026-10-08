using System.Text.Json;
using Engine.Entities.Network.MessageHandler.Interfaces;
using Engine.Entities.Network.MessageHandler.Payloads;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Provider.DTO;
using Engine.Entities.Network.Provider.Enums;
using Engine.Entities.Network.Provider.Extensions;
using Engine.Entities.Overlays;

namespace Engine.Entities.Network.MessageHandler.Implementations;

public class AIResponseMessageHandler : IWebSocketMessageHandler
{
    private readonly Overlay _overlay;

    public string Action => "AI_RESPONSE";

    public AIResponseMessageHandler(Overlay overlay)
    {
        ArgumentNullException.ThrowIfNull(overlay);
        _overlay = overlay;
    }

    public Task HandleAsync(AIProvider client, ProviderEnvelope<JsonElement> envelope, CancellationToken ct = default)
    {
        switch (envelope.Status)
        {
            case ProviderStatus.Loading:
            case ProviderStatus.Processing:
                _overlay.UpdateResponseText("⏳ Procesando consulta...");
                _overlay.SetVisibility(true);
                break;

            case ProviderStatus.Completed:
                var payload = envelope.GetPayload<AIResponsePayload>();
                string textResult = payload?.Text ?? string.Empty;

                _overlay.UpdateResponseText(string.IsNullOrWhiteSpace(textResult) ? "͡° ͜ʖ ͡°" : textResult);
                _overlay.SetVisibility(true);
                break;

            case ProviderStatus.Failed:
                string errorMessage =
                    envelope.Error?.Message ?? "Ocurrió un error inesperado al procesar la respuesta.";
                _overlay.UpdateResponseText($"❌ Error: {errorMessage}");
                _overlay.SetVisibility(true);
                break;
        }

        return Task.CompletedTask;
    }
}