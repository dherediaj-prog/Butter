using Engine.Commands.Payloads;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.Provider.Extensions;

namespace Engine.Commands;

public class SendImageCommand
{
    private readonly ActiveAIProviderSelector _selector;

    public SendImageCommand(ActiveAIProviderSelector selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _selector = selector;
    }

    public async Task ExecuteAsync(
        byte[] imageBytes,
        string mimeType = "image/png",
        CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null || imageBytes.Length == 0) return;

        var payload = new IAAnalyzeImagePayload(
            Image: Convert.ToBase64String(imageBytes),
            MimeType: mimeType
        );

        await provider.SendSuccessAsync("ANALYZE_IMAGE", payload, ct: ct);
    }
}