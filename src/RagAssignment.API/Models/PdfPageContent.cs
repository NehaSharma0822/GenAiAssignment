namespace RagAssignment.Api.Models;

public class PdfPageContent
{
    public string DocumentId { get; set; } = string.Empty;

    public int PageNumber { get; set; }

    public string Text { get; set; } = string.Empty;
}