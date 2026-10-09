namespace Engine.Entities.Network.MessageHandler.Payloads;

public record PromptPayload(
    string? Prompt = null,
    string? Image = null,
    string MimeType = "image/png"
);