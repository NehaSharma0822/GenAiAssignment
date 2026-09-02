using RagAssignment.Api.Models;

namespace RagAssignment.Api.Interfaces;

public interface IChunkingService
{
    IReadOnlyList<DocumentChunk> Chunk(
        IReadOnlyList<PdfPageContent> pages);
}