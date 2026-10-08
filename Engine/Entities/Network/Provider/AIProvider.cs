using System.Net.WebSockets;
using System.Text.Json;
using Engine.Entities.Network.Provider.DTO;

namespace Engine.Entities.Network.Provider;

public class AIProvider : IDisposable
{
    public AIProviderMetadata? Metadata { get; private set; }
    public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;
    public WebSocket Socket { get; }

    public bool IsConnected => Socket.State == WebSocketState.Open;

    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private bool _disposed;

    // --- Eventos de datos entrantes ---
    public event Func<ProviderEnvelope<JsonElement>, Task>? OnTextMessageReceived;

    public AIProvider(
        WebSocket socket,
        AIProviderMetadata? metadata = null,
        JsonSerializerOptions? jsonOptions = null)
    {
        Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        Metadata = metadata;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task ListenAsync(CancellationToken ct = default)
    {
        var buffer = new byte[1024 * 8];

        try
        {
            while (IsConnected && !ct.IsCancellationRequested)
            {
                var result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleCloseHandshakeAsync(result.CloseStatus, result.CloseStatusDescription, ct);
                    break;
                }

                // Optimización: Si el mensaje cabe en 1 solo frame (<8KB), no se asigna MemoryStream
                if (result.EndOfMessage)
                {
                    await ProcessIncomingFrameAsync(result.MessageType, buffer.AsMemory(0, result.Count));
                }
                else
                {
                    // Si viene fragmentado, acumulamos el mensaje completo
                    using var ms = new MemoryStream();
                    ms.Write(buffer, 0, result.Count);

                    while (!result.EndOfMessage)
                    {
                        result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                        ms.Write(buffer, 0, result.Count);
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleCloseHandshakeAsync(result.CloseStatus, result.CloseStatusDescription, ct);
                        break;
                    }

                    await ProcessIncomingFrameAsync(result.MessageType, ms.ToArray());
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Salida limpia por cancelación explícita
        }
        catch (WebSocketException)
        {
            // Conexión cortada o reseteada abruptamente por el cliente/red
        }
    }

    private async Task ProcessIncomingFrameAsync(WebSocketMessageType messageType, ReadOnlyMemory<byte> payload)
    {
        try
        {
            if (messageType == WebSocketMessageType.Text && OnTextMessageReceived is not null)
            {
                var envelope = JsonSerializer.Deserialize<ProviderEnvelope<JsonElement>>(payload.Span, _jsonOptions);
                if (envelope is not null)
                {
                    await OnTextMessageReceived(envelope);
                }
            }
        }
        catch (Exception)
        {
            // Evita que un fallo en el manejador del cliente detenga el bucle de escucha del Socket
        }
    }

    private async Task HandleCloseHandshakeAsync(WebSocketCloseStatus? status, string? description, CancellationToken ct)
    {
        if (Socket.State == WebSocketState.CloseReceived)
        {
            await Socket.CloseOutputAsync(
                status ?? WebSocketCloseStatus.NormalClosure,
                description ?? "Acknowledge Close",
                ct);
        }
    }

    public void SetMetadata(AIProviderMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public async Task SendTextMessageAsync<T>(
        ProviderEnvelope<T> envelope,
        JsonSerializerOptions? jsonOpts = null,
        CancellationToken ct = default)
    {
        if (!IsConnected) return;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope, jsonOpts ?? _jsonOptions);

        await _sendLock.WaitAsync(ct);
        try
        {
            if (IsConnected)
            {
                await Socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task DisconnectAsync(
        WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
        string description = "Closed by server",
        CancellationToken ct = default)
    {
        if (IsConnected)
        {
            await _sendLock.WaitAsync(ct);
            try
            {
                if (IsConnected)
                {
                    await Socket.CloseAsync(status, description, ct);
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        OnTextMessageReceived = null;
        _sendLock.Dispose();
    }
}