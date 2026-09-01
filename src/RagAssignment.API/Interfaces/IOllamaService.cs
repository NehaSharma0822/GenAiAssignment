using RagAssignment.Api.Models;

namespace RagAssignment.Api.Interfaces;

public interface IOllamaService
{
    Task<OllamaChatResponse> ChatAsync(
        OllamaChatRequest request,
        CancellationToken cancellationToken = default);
}