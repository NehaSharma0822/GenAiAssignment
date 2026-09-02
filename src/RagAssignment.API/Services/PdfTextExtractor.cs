using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace RagAssignment.Api.Services;

public class PdfTextExtractor : IPdfTextExtractor
{
    public IReadOnlyList<PdfPageContent> Extract(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "PDF file was not found.",
                filePath);
        }

        var pages = new List<PdfPageContent>();

        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            pages.Add(new PdfPageContent
            {
                DocumentId = Path.GetFileNameWithoutExtension(filePath),
                PageNumber = page.Number,
                Text = ContentOrderTextExtractor.GetText(page)
            });
        }

        return pages;
    }
}