
namespace Engine.Services.Prompt;

public class SystemPromptService
{
    public const string DefaultSystemPrompt = 
        "Responde de forma ultracorta y directa. Usa SOLO texto plano legible en Bloc de Notas. Se permite el uso de fórmulas complejas. Prohibido usar tablas, HTML, diagramas u otros formatos visuales exóticos. NO generes ni crees imágenes bajo ninguna circunstancia.";

    private readonly string _filePath;
    public string CurrentSystemPrompt { get; private set; } = DefaultSystemPrompt;

    public SystemPromptService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EngineApp"
        );
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "system_prompt.txt");

        Load();
    }

    public void Load()
    {
        if (File.Exists(_filePath))
        {
            var content = File.ReadAllText(_filePath);
            
            CurrentSystemPrompt = string.IsNullOrWhiteSpace(content) 
                ? DefaultSystemPrompt 
                : content;
        }
        else
        {
            CurrentSystemPrompt = DefaultSystemPrompt;
            File.WriteAllText(_filePath, DefaultSystemPrompt);
        }
    }

    public async Task SetPromptAsync(string prompt, CancellationToken ct = default)
    {
        CurrentSystemPrompt = prompt ?? string.Empty;
        await File.WriteAllTextAsync(_filePath, CurrentSystemPrompt, ct);
    }
}