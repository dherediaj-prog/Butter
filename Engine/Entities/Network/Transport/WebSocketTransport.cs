using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace Engine.Entities.Network.Transport;

public class WebSocketTransport
{
    public event Func<WebSocket, CancellationToken, Task>? OnConnected;
    public event Func<WebSocket, Task>? OnDisconnected;

    public async Task ProcessAsync(HttpContext context, CancellationToken ct = default)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Combina el CancellationToken externo con la cancelación de la solicitud HTTP activa
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, context.RequestAborted);
        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        try
        {
            if (OnConnected is not null)
            {
                await OnConnected(socket, linkedCts.Token);
            }
        }
        finally
        {
            if (OnDisconnected is not null)
            {
                await OnDisconnected(socket);
            }
        }
    }
}