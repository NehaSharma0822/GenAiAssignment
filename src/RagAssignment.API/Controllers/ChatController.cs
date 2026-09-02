using Microsoft.AspNetCore.Mvc;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
private readonly IOllamaService _ollamaService;
private readonly IRetrievalService _retrievalService;

public ChatController(
    IOllamaService ollamaService,
    IRetrievalService retrievalService)
{
    _ollamaService = ollamaService;
    _retrievalService = retrievalService;
}

[HttpPost]
public async Task<IActionResult> Chat(
    [FromBody] ChatRequest request,
    CancellationToken cancellationToken)
{
    var chunks = await _retrievalService.SearchAsync(
        request.Message,
        3,
        cancellationToken);

    var context = string.Join(
    "\n\n---\n\n",
    chunks.Select(chunk =>
    {
        var text = chunk.Text.Length > 2000
            ? chunk.Text[..2000]
            : chunk.Text;

        return
            $"Document: {chunk.DocumentId}, Page: {chunk.PageNumber}\n" +
            text;
    }));

    var prompt = $"""
You are a document question-answering assistant.

Your task is to answer the user's question using ONLY the information
contained in the provided context.

Rules:
1. Do not use your general knowledge.
2. Do not invent or assume information.
3. If the answer cannot be found in the context, say:
   "The information is not available in the provided documents."
4. Give a concise and accurate answer.
5. When possible, mention the relevant document or page.

Context:
{context}

Question:
{request.Message}

Answer:
""";

    var ollamaRequest = new OllamaChatRequest
    {
        Model = "llama3.2",
        Stream = false,
        Messages =
        [
            new OllamaMessage
            {
                Role = "user",
                Content = prompt
            }
        ]
    };

    var response = await _ollamaService.ChatAsync(
        ollamaRequest,
        cancellationToken);

    var chatResponse = new ChatResponse
{
    Question = request.Message,
    Answer = response.Message.Content,
    Sources = chunks
        .Select(chunk => new ChatSource
        {
            DocumentId = chunk.DocumentId,
            PageNumber = chunk.PageNumber,
            ChunkIndex = chunk.ChunkIndex
        })
        .ToList()
};

return Ok(chatResponse);
}
}