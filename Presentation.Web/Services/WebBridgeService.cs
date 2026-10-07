using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Engine.Entities.Overlays;
using Engine.Entities.Selections;
using Presentation.Web.Constants;
using Presentation.Web.Models;

namespace Presentation.Web.Services;

public partial class WebBridgeService
{
    private readonly Overlay _overlay;
    private readonly Selection _selection;
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ConcurrentDictionary<WebSocket, byte> _sockets = new();

    // Pilar 1 & 3: Dependencias inmutables inyectadas por constructor (Plano 1D)
    public WebBridgeService(Overlay overlay, Selection selection, IWebHostEnvironment env)
    {
        _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
        _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        // Pilar 2: Autoprotección de entrada
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!context.WebSockets.IsWebSocketRequest) return;

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        _sockets.TryAdd(socket, 0);

        var buffer = new byte[1024 * 4];
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var action = JsonSerializer.Deserialize<ClientAction>(buffer.AsSpan(0, result.Count), JsonOptions);
                await DispatchActionAsync(action);
            }
        }
        finally
        {
            _sockets.TryRemove(socket, out _);
        }
    }

    // Pilar 4: Desacoplamiento antisocial; orquesta estados y broadcasts según la acción recibida por WS
    private async Task DispatchActionAsync(ClientAction? action)
    {
        if (action == null) return;

        switch (action.Action)
        {
            case MessageAction.Ask:
                // Retransmite el prompt entrante a los clientes WebSocket (Extensiones)
                var promptText = action.Prompt ?? action.Text ?? string.Empty;
                await BroadcastAsync(new { action = MessageAction.Ask, prompt = promptText });
                break;

            case MessageAction.Response:
                var displayText = action.Status switch
                {
                    MessageStatus.Loading => "Cargando...",
                    MessageStatus.Processing => "Procesando en el navegador...",
                    _ => action.Text ?? string.Empty
                };
                _overlay.UpdateResponseText(displayText);
                break;

            case MessageAction.Error:
                _overlay.UpdateResponseText($"Error: {action.Error ?? "Error desconocido en el proveedor"}");
                break;

            case MessageAction.TriggerSelection:
                _selection.RequestSelection();
                break;
        }
    }

    // Pilar 1: Datos efímeros (bytes de imagen) pasados por parámetro de método
    public async Task SendImageAsync(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Los bytes de la imagen no pueden estar vacíos.", nameof(imageBytes));

        var base64Image = Convert.ToBase64String(imageBytes);
        await BroadcastAsync(new
        {
            action = MessageAction.CapturedImage,
            image = $"data:image/png;base64,{base64Image}"
        });
    }

    private async Task BroadcastAsync(object payloadObj)
    {
        if (payloadObj == null) throw new ArgumentNullException(nameof(payloadObj));

        var payload = JsonSerializer.SerializeToUtf8Bytes(payloadObj);
        var activeSockets = _sockets.Keys.Where(s => s.State == WebSocketState.Open);

        foreach (var socket in activeSockets)
        {
            await socket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}