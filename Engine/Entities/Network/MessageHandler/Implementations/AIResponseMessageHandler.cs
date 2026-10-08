using System.Text.Json;
using Engine.Entities.Network.MessageHandler.Interfaces;
using Engine.Entities.Network.Provider;
using Engine.Entities.Network.Provider.DTO;
using Engine.Entities.Network.Provider.Enums;
using Engine.Entities.Overlays;

namespace Engine.Entities.Network.MessageHandler.Implementations;

public class AIResponseMessageHandler : IWebSocketMessageHandler
{
    private readonly Overlay _overlay;

    /// <summary>
    /// Acción de la envoltura WebSocket que procesa este handler.
    /// </summary>
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
                string textResult = ExtractResponseText(envelope.Payload);
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

    /// <summary>
    /// Extrae el texto plano del Payload JSON recibido.
    /// </summary>
    private static string ExtractResponseText(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.String)
        {
            return payload.GetString() ?? string.Empty;
        }

        if (payload.ValueKind == JsonValueKind.Object)
        {
            if (payload.TryGetProperty("text", out var textProp) ||
                payload.TryGetProperty("response", out textProp) ||
                payload.TryGetProperty("message", out textProp))
            {
                return textProp.GetString() ?? string.Empty;
            }
        }

        return payload.ValueKind != JsonValueKind.Undefined && payload.ValueKind != JsonValueKind.Null
            ? payload.ToString()
            : string.Empty;
    }
}