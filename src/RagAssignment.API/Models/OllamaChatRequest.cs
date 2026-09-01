namespace RagAssignment.Api.Models;

public class OllamaChatRequest
{
    public string Model { get; set; } = string.Empty;

    public List<OllamaMessage> Messages { get; set; } = [];

    public bool Stream { get; set; }
}

public class OllamaMessage
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}