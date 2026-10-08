using Engine.Entities.Network.Provider.Enums;

namespace Engine.Entities.Network.Provider;

public class AIProviderMetadata
{
    public string Id { get; }

    /// <summary>
    /// Proveedor o motor de la IA (ej: "openai", "claude", "ollama").
    /// </summary>
    public string Provider { get; }

    public string Url { get; }

    /// <summary>
    /// Tipo de dispositivo basado exclusivamente en su factor de forma.
    /// </summary>
    public DeviceType Device { get; }

    public AIProviderMetadata(
        string? id = null,
        string? provider = null,
        string? url = null,
        DeviceType device = DeviceType.Unknown)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Provider = string.IsNullOrWhiteSpace(provider) ? "unknown" : provider.Trim().ToLowerInvariant();
        Url = url ?? string.Empty;
        Device = device;
    }
}