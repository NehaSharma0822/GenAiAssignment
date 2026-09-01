using RagAssignment.Api.Models;

namespace RagAssignment.Api.Interfaces;

public interface IPdfTextExtractor
{
    IReadOnlyList<PdfPageContent> Extract(string filePath);
}