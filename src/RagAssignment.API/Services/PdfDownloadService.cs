namespace RagAssignment.Api.Services;

public class PdfDownloadService
{
    private readonly HttpClient _httpClient;

    public PdfDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadAsync(
        string url,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            url,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var sourceStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        await using var destinationStream =
            File.Create(destinationPath);

        await sourceStream.CopyToAsync(
            destinationStream,
            cancellationToken);
    }
}