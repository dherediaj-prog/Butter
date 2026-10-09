using Engine.Commands.Payloads;
using Engine.Entities.Commands.Model;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.Provider.Extensions;

namespace Engine.Entities.Commands;

public class SendPromptCommand : AsyncAppCommand<string>
{
    private readonly ActiveAIProviderSelector _selector;

    public SendPromptCommand(ActiveAIProviderSelector selector)
        : base(
            title: "Enviar Prompt",
            description: "Envía una consulta de texto al proveedor de IA activo.",
            shortcut: Keys.Control | Keys.Enter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _selector = selector;
    }

    protected override bool CanExecuteAsync(string parameter)
    {
        return !string.IsNullOrWhiteSpace(parameter) && _selector.GetActiveConnectedProvider() != null;
    }

    public override async Task ExecuteAsync(string prompt, CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null || string.IsNullOrWhiteSpace(prompt)) return;

        var payload = new AskPayload(prompt);
        await provider.SendSuccessAsync("ASK", payload, ct: ct);
    }
}