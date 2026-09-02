using Microsoft.AspNetCore.Mvc;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Services;
using RagAssignment.Api.Configuration;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public class IngestionController : ControllerBase
{
    private readonly PdfDownloadService _pdfDownloadService;
private readonly IPdfTextExtractor _extractor;
private readonly ITextPreprocessor _preprocessor;
private readonly IChunkingService _chunkingService;
private readonly IEmbeddingService _embeddingService;
private readonly IQdrantService _qdrantService;
private readonly IRetrievalService _retrievalService;

public IngestionController(
    PdfDownloadService pdfDownloadService,
    IPdfTextExtractor extractor,
    ITextPreprocessor preprocessor,
    IChunkingService chunkingService,
    IEmbeddingService embeddingService,
    IQdrantService qdrantService,
    IRetrievalService retrievalService)
{
    _pdfDownloadService = pdfDownloadService;
    _extractor = extractor;
    _preprocessor = preprocessor;
    _chunkingService = chunkingService;
    _embeddingService = embeddingService;
    _qdrantService = qdrantService;
    _retrievalService = retrievalService;
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

    [HttpPost("extract-pdfs")]
public IActionResult ExtractPdfs()
{
    var pdfDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "data",
        "PDFs");

    var pdfFiles = Directory.GetFiles(
        pdfDirectory,
        "*.pdf");

    var results = new List<object>();

    foreach (var pdfFile in pdfFiles)
    {
        var pages = _extractor.Extract(pdfFile);
        var cleanedPages = pages
    .Select(page => new PdfPageContent
    {
        DocumentId = page.DocumentId,
        PageNumber = page.PageNumber,
        Text = _preprocessor.Clean(page.Text)
    })
    .ToList();

         results.Add(new
        {
            documentId = Path.GetFileNameWithoutExtension(pdfFile),
            pageCount = cleanedPages.Count,
            firstPagePreview = cleanedPages.Count > 0
                ? cleanedPages[0].Text[
                    ..Math.Min(500, cleanedPages[0].Text.Length)]
                : string.Empty
        });
    }

    return Ok(results);
}
[HttpPost("create-chunks")]
public IActionResult CreateChunks()
{
    var pdfDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "data",
        "PDFs");

    var pdfFiles = Directory.GetFiles(
        pdfDirectory,
        "*.pdf");

    var results = new List<object>();

    foreach (var pdfFile in pdfFiles)
    {
        var pages = _extractor.Extract(pdfFile);

        var cleanedPages = pages
            .Select(page => new PdfPageContent
            {
                DocumentId = page.DocumentId,
                PageNumber = page.PageNumber,
                Text = _preprocessor.Clean(page.Text)
            })
            .ToList();

        var chunks = _chunkingService.Chunk(cleanedPages);

        results.Add(new
        {
            documentId = Path.GetFileNameWithoutExtension(pdfFile),
            pageCount = cleanedPages.Count,
            chunkCount = chunks.Count,
            firstChunk = chunks.Count > 0
                ? chunks[0]
                : null
        });
    }

    return Ok(results);
}

[HttpPost("test-embedding")]
public async Task<IActionResult> TestEmbedding(
    CancellationToken cancellationToken)
{
    var text = "Generative AI can create new content such as text, images, and code.";

    var embedding = await _embeddingService.GenerateEmbeddingAsync(
        text,
        cancellationToken);

    return Ok(new
    {
        text,
        dimensions = embedding.Length,
        firstValues = embedding.Take(10)
    });
}

[HttpPost("test-qdrant")]
public async Task<IActionResult> TestQdrant(
    CancellationToken cancellationToken)
{
    await _qdrantService.EnsureCollectionAsync(
        cancellationToken);

    var exists = await _qdrantService.CollectionExistsAsync(
        cancellationToken);

    return Ok(new
    {
        collection = "rag_documents",
        exists
    });
}

[HttpPost("embed-and-store")]
public async Task<IActionResult> EmbedAndStore(
    CancellationToken cancellationToken)
{
    var pdfDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "data",
        "PDFs");

    var pdfFiles = Directory.GetFiles(
        pdfDirectory,
        "*.pdf");

    var results = new List<object>();

    foreach (var pdfFile in pdfFiles)
    {
        var pages = _extractor.Extract(pdfFile);

        var cleanedPages = pages
            .Select(page => new PdfPageContent
            {
                DocumentId = page.DocumentId,
                PageNumber = page.PageNumber,
                Text = _preprocessor.Clean(page.Text)
            })
            .ToList();

        var chunks = _chunkingService.Chunk(cleanedPages);

        await _qdrantService.UpsertChunksAsync(
            chunks,
            cancellationToken);

        results.Add(new
        {
            documentId = Path.GetFileNameWithoutExtension(pdfFile),
            pageCount = cleanedPages.Count,
            chunkCount = chunks.Count
        });
    }

    return Ok(results);
}

[HttpPost("test-search")]
public async Task<IActionResult> TestSearch(
    [FromBody] ChatRequest request,
    CancellationToken cancellationToken)
{
    var results = await _retrievalService.SearchAsync(
    request.Message,
    5,
    cancellationToken);

    return Ok(results);
}

}