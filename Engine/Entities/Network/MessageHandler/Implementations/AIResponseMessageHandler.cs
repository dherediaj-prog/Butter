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
        Console.WriteLine("[AIResponseMessageHandler] 🛠️ Handler inicializado.");
    }

    public Task HandleAsync(AIProvider client, ProviderEnvelope<JsonElement> envelope, CancellationToken ct = default)
    {
        Console.WriteLine($"[AIResponseMessageHandler] 📥 Evento 'AI_RESPONSE' recibido [ID: {envelope.Id}] | Status: {envelope.Status}");

        switch (envelope.Status)
        {
            case ProviderStatus.Loading:
            case ProviderStatus.Processing:
                Console.WriteLine($"[AIResponseMessageHandler] ⏳ Notificación de estado activo ({envelope.Status}). Actualizando UI...");
                _responseDisplay.UpdateResponseText("⏳ Procesando consulta...");
                _responseDisplay.SetVisibility(true);
                break;

            case ProviderStatus.Completed:
                var payload = envelope.GetPayload<ResponsePayload>();
                string textResult = payload?.Text ?? string.Empty;

                Console.WriteLine($"[AIResponseMessageHandler] 🎉 Respuesta completada recibida ({textResult.Length} chars) desde {payload?.ProviderName ?? "desconocido"}.");

                _responseDisplay.UpdateResponseText(string.IsNullOrWhiteSpace(textResult) ? "͡° ͜ʖ ͡°" : textResult);
                _responseDisplay.SetVisibility(true);
                break;

            case ProviderStatus.Failed:
                string errorMessage =
                    envelope.Error?.Message ?? "Ocurrió un error inesperado al procesar la respuesta.";

                Console.WriteLine($"[AIResponseMessageHandler] ❌ Error recibido [Código: {envelope.Error?.Code}]: {errorMessage}");

                _responseDisplay.UpdateResponseText($"❌ Error: {errorMessage}");
                _responseDisplay.SetVisibility(true);
                break;

            default:
                Console.WriteLine($"[AIResponseMessageHandler] ⚠️ Estado no manejado: {envelope.Status}");
                break;
        }

        return Task.CompletedTask;
    }
}