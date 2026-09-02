using Qdrant.Client;
using Qdrant.Client.Grpc;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Services;

public class RetrievalService : IRetrievalService
{
    private const string CollectionName = "rag_documents";

    private readonly QdrantClient _client;
    private readonly IEmbeddingService _embeddingService;

    public RetrievalService(
        QdrantClient client,
        IEmbeddingService embeddingService)
    {
        _client = client;
        _embeddingService = embeddingService;
    }

    public async Task<IReadOnlyList<DocumentChunk>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        var results = await _client.QueryAsync(
    CollectionName,
    queryEmbedding,
    limit: (ulong)topK,
    scoreThreshold: 0.40f,
    cancellationToken: cancellationToken);

        var chunks = new List<DocumentChunk>();

        foreach (var result in results)
        {
            var payload = result.Payload;

            chunks.Add(new DocumentChunk
            {
                Id = result.Id.ToString(),

                DocumentId =
                    payload["documentId"].StringValue,

                PageNumber =
                    (int)payload["pageNumber"].IntegerValue,

                ChunkIndex =
                    (int)payload["chunkIndex"].IntegerValue,

                Text =
                    payload["text"].StringValue
            });
        }

        return chunks;
    }
}