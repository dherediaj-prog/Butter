/*using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.Provider.Extensions;

namespace Engine.Commands;

public class SendCancelCommand
{
    private readonly ActiveAIProviderSelector _selector;

    public SendCancelCommand(ActiveAIProviderSelector selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _selector = selector;
    }

    public async Task ExecuteAsync(string? targetRequestId = null, CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null) return;

        await provider.SendSuccessAsync("CANCEL", new { requestId = targetRequestId }, id: targetRequestId, ct: ct);
    }
}*/