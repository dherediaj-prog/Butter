using Engine.Entities.Network.Provider.Enums;

namespace Engine.Entities.Network.Provider.DTO;

public record ProviderEnvelope<T>(
    string Action,
    T? Payload = default,
    ProviderStatus Status = ProviderStatus.Completed,
    string? Id = null,
    ProviderError? Error = null,
    long? Timestamp = null
)
{
    public string? Id { get; init; } = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id;
    public long? Timestamp { get; init; } = Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static ProviderEnvelope<T> Success(string action, T payload, string? id = null) =>
        new(action, payload, ProviderStatus.Completed, id);

    public static ProviderEnvelope<T> Processing(string action, T? payload = default, string? id = null) =>
        new(action, payload, ProviderStatus.Processing, id);

    public static ProviderEnvelope<T> Loading(string action, T? payload = default, string? id = null) =>
        new(action, payload, ProviderStatus.Loading, id);

    public static ProviderEnvelope<T>
        Fail(string action, string errorMessage, int errorCode = 500, string? id = null) =>
        new(action, default, ProviderStatus.Failed, id, new ProviderError(errorCode, errorMessage));
}