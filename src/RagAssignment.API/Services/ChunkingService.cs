using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Services;

public class ChunkingService : IChunkingService
{
    private const int ChunkSize = 500;
    private const int OverlapSize = 100;

    public IReadOnlyList<DocumentChunk> Chunk(
        IReadOnlyList<PdfPageContent> pages)
    {
        var chunks = new List<DocumentChunk>();

        foreach (var page in pages)
        {
            var words = page.Text
                .Split(
                    [' ', '\n', '\r', '\t'],
                    StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
            {
                continue;
            }

            var start = 0;
            var chunkIndex = 0;

            while (start < words.Length)
            {
                var length = Math.Min(
                    ChunkSize,
                    words.Length - start);

                var chunkText = string.Join(
                    " ",
                    words.Skip(start).Take(length));

                chunks.Add(new DocumentChunk
                {
                    Id = $"{page.DocumentId}_{page.PageNumber}_{chunkIndex}",
                    DocumentId = page.DocumentId,
                    PageNumber = page.PageNumber,
                    ChunkIndex = chunkIndex,
                    Text = chunkText
                });

                chunkIndex++;

                if (length == words.Length - start)
                {
                    break;
                }

                start += ChunkSize - OverlapSize;
            }
        }

        return chunks;
    }
}