using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.Provider.Extensions;

namespace Engine.Commands;

public class SendPromptCommand
{
    private readonly ActiveAIProviderSelector _selector;

    public SendPromptCommand(ActiveAIProviderSelector selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _selector = selector;
    }

    public async Task ExecuteAsync(string prompt, CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null || string.IsNullOrWhiteSpace(prompt)) return;

        await provider.SendSuccessAsync("ASK", new { prompt }, ct: ct);
    }
}