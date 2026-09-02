using System.Net.Http.Json;
using RagAssignment.Api.Interfaces;

namespace RagAssignment.Api.Services;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;

    public OllamaEmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = "nomic-embed-text",
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/embeddings",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
                cancellationToken: cancellationToken);

        return result?.Embedding
            ?? throw new InvalidOperationException(
                "Ollama returned an empty embedding.");
    }

    private class EmbeddingResponse
    {
        public float[] Embedding { get; set; } = [];
    }
}