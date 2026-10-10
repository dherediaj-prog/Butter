using Engine.Entities.Commands.Model;
using Engine.Services.Prompt;

namespace Engine.Entities.Commands;

public class SetSystemPromptCommand : AsyncAppCommand<string>
{
    private readonly SystemPromptService _promptService;

    public SetSystemPromptCommand(SystemPromptService promptService)
        : base(
            title: "Configurar System Prompt",
            description: "Guarda un prompt base de manera persistente para incluirlo en cada envío.")
    {
        ArgumentNullException.ThrowIfNull(promptService);
        _promptService = promptService;
    }

    protected override bool CanExecuteAsync(string parameter)
    {
        return !string.IsNullOrWhiteSpace(parameter);
    }

    public override async Task ExecuteAsync(string newPrompt, CancellationToken ct = default)
    {
        await _promptService.SetPromptAsync(newPrompt, ct);
    }
}