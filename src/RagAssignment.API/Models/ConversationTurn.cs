namespace RagAssignment.Api.Models;

public class ConversationTurn
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<string> DocumentIds { get; set; } = [];
}