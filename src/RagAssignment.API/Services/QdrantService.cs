using Qdrant.Client;
using Qdrant.Client.Grpc;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Services;

public class QdrantService : IQdrantService
{
    private const string CollectionName = "rag_documents";
    private const ulong VectorSize = 768;

    private readonly QdrantClient _client;
    private readonly IEmbeddingService _embeddingService;

    public QdrantService(
        QdrantClient client,
        IEmbeddingService embeddingService)
    {
        _client = client;
        _embeddingService = embeddingService;
    }

    public async Task EnsureCollectionAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = await _client.ListCollectionsAsync(
            cancellationToken);

        if (collections.Contains(CollectionName))
        {
            return;
        }

        await _client.CreateCollectionAsync(
            CollectionName,
            new VectorParams
            {
                Size = VectorSize,
                Distance = Distance.Cosine
            },
            cancellationToken: cancellationToken);
    }

    public async Task<bool> CollectionExistsAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = await _client.ListCollectionsAsync(
            cancellationToken);

        return collections.Contains(CollectionName);
    }

    public async Task UpsertChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        await EnsureCollectionAsync(cancellationToken);

        var points = new List<PointStruct>();

        foreach (var chunk in chunks)
        {
            var embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    chunk.Text,
                    cancellationToken);

            points.Add(new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embedding,
                Payload =
                {
                    ["documentId"] = chunk.DocumentId,
                    ["pageNumber"] = chunk.PageNumber,
                    ["chunkIndex"] = chunk.ChunkIndex,
                    ["text"] = chunk.Text
                }
            });
        }

        if (points.Count == 0)
        {
            return;
        }

        await _client.UpsertAsync(
            CollectionName,
            points,
            cancellationToken: cancellationToken);
    }
}