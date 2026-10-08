using Engine.Entities.Network.Provider.DTO;

namespace Engine.Entities.Network.Provider.Extensions;

public static class AIProviderEnvelopeExtensions
{
    /// <summary>
    /// Envía un mensaje exitoso envolviéndolo en un WebSocketEnvelope.
    /// </summary>
    public static Task SendSuccessAsync<T>(
        this AIProvider provider,
        string action,
        T payload,
        string? id = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var envelope = ProviderEnvelope<T>.Success(action, payload, id);
        return provider.SendTextMessageAsync(envelope, ct: ct);
    }

    /// <summary>
    /// Envía una notificación de estado "En proceso".
    /// </summary>
    public static Task SendProcessingAsync<T>(
        this AIProvider provider,
        string action,
        T? payload = default,
        string? id = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var envelope = ProviderEnvelope<T>.Processing(action, payload, id);
        return provider.SendTextMessageAsync(envelope, ct: ct);
    }

    /// <summary>
    /// Envía una notificación de estado "Cargando".
    /// </summary>
    public static Task SendLoadingAsync<T>(
        this AIProvider provider,
        string action,
        T? payload = default,
        string? id = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var envelope = ProviderEnvelope<T>.Loading(action, payload, id);
        return provider.SendTextMessageAsync(envelope, ct: ct);
    }

    /// <summary>
    /// Envía una respuesta de error estructurada.
    /// </summary>
    public static Task SendErrorAsync(
        this AIProvider provider,
        string action,
        string errorMessage,
        int errorCode = 500,
        string? id = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var envelope = ProviderEnvelope<object>.Fail(action, errorMessage, errorCode, id);
        return provider.SendTextMessageAsync(envelope, ct: ct);
    }
}