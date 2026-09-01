namespace RagAssignment.Api.Models;

public class IngestedDocument
{
    public string DocumentId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public List<PdfPageContent> Pages { get; set; } = [];
}