using RagAssignment.Api.Models;

namespace RagAssignment.Api.Configuration;

public static class PdfSources
{
    public static IReadOnlyList<PdfSource> All =>
    [
        new()
        {
            DocumentId = "attention-is-all-you-need",
            FileName = "1706.03762.pdf",
            Url = "https://arxiv.org/pdf/1706.03762.pdf"
        },

        new()
        {
            DocumentId = "bert",
            FileName = "1810.04805.pdf",
            Url = "https://arxiv.org/pdf/1810.04805.pdf"
        },

        new()
        {
            DocumentId = "gpt-3",
            FileName = "2005.14165.pdf",
            Url = "https://arxiv.org/pdf/2005.14165.pdf"
        },

        new()
        {
            DocumentId = "1907.11692",
            FileName = "1907.11692.pdf",
            Url = "https://arxiv.org/pdf/1907.11692.pdf"
        },

        new()
        {
            DocumentId = "1910.10683",
            FileName = "1910.10683.pdf",
            Url = "https://arxiv.org/pdf/1910.10683.pdf"
        }
    ];
}