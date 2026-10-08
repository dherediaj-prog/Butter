using System.Text.Json.Serialization;

namespace Engine.Entities.Network.Provider.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProviderStatus
{
    Loading,
    Processing,
    Completed,
    Failed
}