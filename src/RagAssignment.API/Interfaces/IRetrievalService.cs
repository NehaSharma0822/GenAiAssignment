using RagAssignment.Api.Models;

namespace RagAssignment.Api.Interfaces;

public interface IRetrievalService
{
    Task<IReadOnlyList<DocumentChunk>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default);
}