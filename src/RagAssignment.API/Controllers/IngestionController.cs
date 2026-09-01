using Microsoft.AspNetCore.Mvc;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Services;
using RagAssignment.Api.Configuration;

namespace RagAssignment.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public class IngestionController : ControllerBase
{
    private readonly IPdfTextExtractor _extractor;
    private readonly PdfDownloadService _pdfDownloadService;

    public IngestionController(
        PdfDownloadService pdfDownloadService,
        IPdfTextExtractor extractor
        )
    {
        _extractor = extractor;
        _pdfDownloadService = pdfDownloadService;
    }

    [HttpPost("test-pdf")]
    public async Task<IActionResult> TestPdf(
        CancellationToken cancellationToken)
    {
        var results = new List<object>();

foreach (var source in PdfSources.All)
{
    var destinationPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "data",
        "PDFs",
        source.FileName);

    await _pdfDownloadService.DownloadAsync(
        source.Url,
        destinationPath,
        cancellationToken);

    results.Add(new
    {
        source.DocumentId,
        source.FileName,
        destinationPath
    });
}

return Ok(results);
    }
}