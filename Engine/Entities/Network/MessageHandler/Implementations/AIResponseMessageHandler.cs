using System.Text.Json;
using Engine.Entities.Network.MessageHandler.Interfaces;
using Engine.Entities.Network.MessageHandler.Payloads;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Provider.DTO;
using Engine.Entities.Network.Provider.Enums;
using Engine.Entities.Network.Provider.Extensions;
using Engine.Entities.ResponseDisplays;

namespace Engine.Entities.Network.MessageHandler.Implementations;

public class AIResponseMessageHandler : IWebSocketMessageHandler
{
    private readonly ResponseDisplay _responseDisplay;

    public string Action => "AI_RESPONSE";

    public AIResponseMessageHandler(ResponseDisplay responseDisplay)
    {
        ArgumentNullException.ThrowIfNull(responseDisplay);
        _responseDisplay = responseDisplay;
    }

    public Task HandleAsync(AIProvider client, ProviderEnvelope<JsonElement> envelope, CancellationToken ct = default)
    {
        switch (envelope.Status)
        {
            case ProviderStatus.Loading:
            case ProviderStatus.Processing:
                _responseDisplay.UpdateResponseText("⏳ Procesando consulta...");
                _responseDisplay.SetVisibility(true);
                break;

            case ProviderStatus.Completed:
                var payload = envelope.GetPayload<AIResponsePayload>();
                string textResult = payload?.Text ?? string.Empty;

                _responseDisplay.UpdateResponseText(string.IsNullOrWhiteSpace(textResult) ? "͡° ͜ʖ ͡°" : textResult);
                _responseDisplay.SetVisibility(true);
                break;

            case ProviderStatus.Failed:
                string errorMessage = envelope.Error?.Message ?? "Ocurrió un error inesperado al procesar la respuesta.";
                _responseDisplay.UpdateResponseText($"❌ Error: {errorMessage}");
                _responseDisplay.SetVisibility(true);
                break;
        }

        return Task.CompletedTask;
    }
}