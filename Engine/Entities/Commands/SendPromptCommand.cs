using Engine.Entities.Commands.Model;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.MessageHandler.Payloads;
using Engine.Entities.Network.Provider.Extensions;

namespace Engine.Entities.Commands;

public record SendPromptArgs(
    string? Prompt = null,
    byte[]? ImageBytes = null,
    string MimeType = "image/png")
{
    public static implicit operator SendPromptArgs(string prompt) => new(Prompt: prompt);
}

public class SendPromptCommand : AsyncAppCommand<SendPromptArgs>
{
    private readonly ActiveAIProviderSelector _selector;

    public SendPromptCommand(ActiveAIProviderSelector selector)
        : base(
            title: "Enviar Prompt",
            description: "Envía una consulta con texto e/o imagen al proveedor de IA activo.",
            shortcut: Keys.Control | Keys.Enter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _selector = selector;
    }

    protected override bool CanExecuteAsync(SendPromptArgs parameter)
    {
        var hasContent = !string.IsNullOrWhiteSpace(parameter?.Prompt) || (parameter?.ImageBytes is { Length: > 0 });
        return hasContent && _selector.GetActiveConnectedProvider() != null;
    }

    public override async Task ExecuteAsync(SendPromptArgs args, CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null) return;

        var payload = new PromptPayload(
            Prompt: args.Prompt,
            Image: args.ImageBytes is { Length: > 0 } ? Convert.ToBase64String(args.ImageBytes) : null,
            MimeType: args.MimeType
        );

        await provider.SendSuccessAsync("ASK", payload, ct: ct);
    }
}