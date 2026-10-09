using Engine.Commands.Payloads;
using Engine.Entities.Commands.Model;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.Provider.Extensions;

namespace Engine.Entities.Commands;

public record SendImageArgs(byte[] ImageBytes, string MimeType = "image/png");

public class SendImageCommand : AsyncAppCommand<SendImageArgs>
{
    private readonly ActiveAIProviderSelector _selector;

    public SendImageCommand(ActiveAIProviderSelector selector)
        : base(
            title: "Enviar Imagen",
            description: "Envía una imagen para análisis visual al proveedor de IA activo.")
    {
        ArgumentNullException.ThrowIfNull(selector);
        _selector = selector;
    }

    protected override bool CanExecuteAsync(SendImageArgs parameter)
    {
        return parameter is { ImageBytes.Length: > 0 } && _selector.GetActiveConnectedProvider() != null;
    }

    public override async Task ExecuteAsync(SendImageArgs args, CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null || args.ImageBytes.Length == 0) return;

        var payload = new IAAnalyzeImagePayload(
            Image: Convert.ToBase64String(args.ImageBytes),
            MimeType: args.MimeType
        );

        await provider.SendSuccessAsync("ANALYZE_IMAGE", payload, ct: ct);
    }
}