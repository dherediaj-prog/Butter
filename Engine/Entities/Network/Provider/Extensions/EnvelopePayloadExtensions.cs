using System.Text.Json;
using Engine.Entities.Network.Provider.DTO;

namespace Engine.Entities.Network.Provider.Extensions;

public static class EnvelopePayloadExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializa el JsonElement del Payload al DTO fuertemente tipado especificado.
    /// </summary>
    public static T? GetPayload<T>(this ProviderEnvelope<JsonElement> envelope, JsonSerializerOptions? options = null)
    {
        if (envelope.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return default;

        if (typeof(T) == typeof(string) && envelope.Payload.ValueKind == JsonValueKind.String)
        {
            return (T)(object)envelope.Payload.GetString()!;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(envelope.Payload.GetRawText(), options ?? DefaultOptions);
        }
        catch
        {
            return default;
        }
    }
}