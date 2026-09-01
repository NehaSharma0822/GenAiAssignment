namespace RagAssignment.Api.Models;

public class OllamaChatResponse
{
    public string Model { get; set; } = string.Empty;

    public OllamaMessage Message { get; set; } = new();

    public bool Done { get; set; }
}