namespace RagAssignment.Api.Models;

public class ChatRequest
{
    public string ConversationId { get; set; } = "default";

    public string Message { get; set; } = string.Empty;
}