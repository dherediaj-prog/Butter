namespace Engine.Entities.Network.MessageHandler.Payloads;

public record ResponsePayload(
    string Text,
    string? ProviderName = null
);