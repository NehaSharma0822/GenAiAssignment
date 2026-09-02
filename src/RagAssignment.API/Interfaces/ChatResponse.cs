namespace RagAssignment.Api.Models;

public class ChatResponse
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<ChatSource> Sources { get; set; } = [];
}

public class ChatSource
{
    public string DocumentId { get; set; } = string.Empty;

    public int PageNumber { get; set; }

    public int ChunkIndex { get; set; }
}