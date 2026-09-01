namespace RagAssignment.Api.Interfaces;

public interface IPdfDownloadService
{
    Task<string> DownloadAsync(
        string url,
        string fileName,
        CancellationToken cancellationToken = default);
}