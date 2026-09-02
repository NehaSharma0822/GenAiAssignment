using RagAssignment.Api.Models;

namespace RagAssignment.Api.Interfaces;

public interface IQdrantService
{
    Task EnsureCollectionAsync(
        CancellationToken cancellationToken = default);

    Task<bool> CollectionExistsAsync(
        CancellationToken cancellationToken = default);

    Task UpsertChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);
}