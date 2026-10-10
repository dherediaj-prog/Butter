using Engine.Entities.Commands.Model;
using Engine.Entities.Network.ActiveAIProviderSelectors;
using Engine.Entities.Network.MessageHandler.Payloads;
using Engine.Entities.Network.Provider.Extensions;
using Engine.Services.Prompt;

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
    private readonly SystemPromptService _promptService;

    public SendPromptCommand(
        ActiveAIProviderSelector selector,
        SystemPromptService promptService)
        : base(
            title: "Enviar Prompt",
            description: "Envía una consulta con texto e/o imagen al proveedor de IA activo.",
            shortcut: Keys.Control | Keys.Enter)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(promptService);

        _selector = selector;
        _promptService = promptService;
    }

    protected override bool CanExecuteAsync(SendPromptArgs parameter)
    {
        var hasContent = !string.IsNullOrWhiteSpace(parameter?.Prompt)
                         || !string.IsNullOrWhiteSpace(_promptService.CurrentSystemPrompt)
                         || (parameter?.ImageBytes is { Length: > 0 });

        return hasContent && _selector.GetActiveConnectedProvider() != null;
    }

    public override async Task ExecuteAsync(SendPromptArgs args, CancellationToken ct = default)
    {
        var provider = _selector.GetActiveConnectedProvider();
        if (provider is null) return;

        // Combinar el System Prompt persistente con el prompt ingresado por el usuario
        string? finalPrompt = args.Prompt;

        if (!string.IsNullOrWhiteSpace(_promptService.CurrentSystemPrompt))
        {
            finalPrompt = string.IsNullOrWhiteSpace(args.Prompt)
                ? _promptService.CurrentSystemPrompt
                : $"{_promptService.CurrentSystemPrompt}\n\n{args.Prompt}";
        }

        var payload = new PromptPayload(
            Prompt: finalPrompt,
            Image: args.ImageBytes is { Length: > 0 } ? Convert.ToBase64String(args.ImageBytes) : null,
            MimeType: args.MimeType
        );

        await provider.SendSuccessAsync("ASK", payload, ct: ct);
    }
}