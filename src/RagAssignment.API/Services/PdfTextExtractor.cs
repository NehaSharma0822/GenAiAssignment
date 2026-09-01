using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;
using UglyToad.PdfPig;

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

        var documentId = Path.GetFileNameWithoutExtension(filePath);

        var pages = new List<PdfPageContent>();

        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            pages.Add(new PdfPageContent
            {
                DocumentId = documentId,
                PageNumber = page.Number,
                Text = page.Text
            });
        }

        return pages;
    }
}