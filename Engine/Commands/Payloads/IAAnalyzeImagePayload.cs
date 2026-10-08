namespace Engine.Commands.Payloads;

public record IAAnalyzeImagePayload(string Image, string MimeType = "image/png");