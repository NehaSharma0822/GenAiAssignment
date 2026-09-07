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
    private readonly IConversationMemory _conversationMemory;

    public ChatController(
        IOllamaService ollamaService,
        IRetrievalService retrievalService,
        IConversationMemory conversationMemory)
    {
        _ollamaService = ollamaService;
        _retrievalService = retrievalService;
        _conversationMemory = conversationMemory;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var conversationId =
            Request.Headers["X-Conversation-Id"].FirstOrDefault()
            ?? "default";

        // ------------------------------------------------------------
        // 1. Get previous conversation
        // ------------------------------------------------------------

        var history = _conversationMemory.GetHistory(conversationId);

        // ------------------------------------------------------------
        // 2. Build retrieval query
        //
        // For follow-up questions such as:
        //
        // Q1: What is T5?
        // Q2: What tasks can it handle?
        //
        // include the previous question and answer so that
        // semantic search understands what "it" refers to.
        // ------------------------------------------------------------

        var retrievalQuery = request.Message;

        if (history.Count > 0 && ContainsReference(request.Message))
        {
            var lastTurn = history[^1];

            retrievalQuery =
                $"{lastTurn.Question}. " +
                $"{lastTurn.Answer}. " +
                $"{request.Message}";
        }

        Console.WriteLine("===== RETRIEVAL QUERY =====");
        Console.WriteLine(retrievalQuery);
        Console.WriteLine("===========================");

        // ------------------------------------------------------------
        // 3. Retrieve relevant document chunks
        // ------------------------------------------------------------

        var chunks = await _retrievalService.SearchAsync(
            retrievalQuery,
            5,
            cancellationToken);

        // ------------------------------------------------------------
        // 4. For follow-up questions, prefer the document that
        // appeared in the previous retrieval result.
        //
        // IMPORTANT:
        // We do NOT try to extract document ID from the LLM answer.
        // The answer is generated text and may not contain metadata.
        //
        // Since conversation memory currently stores only Question
        // and Answer, we use the current retrieval results and
        // previous topic to keep the search focused.
        // ------------------------------------------------------------

        if (history.Count > 0 && ContainsReference(request.Message))
        {
            var lastTurn = history[^1];

            // Re-run retrieval using the previous question as a
            // stronger signal and combine it with the current question.
            var followUpQuery =
                $"{lastTurn.Question}. {request.Message}";

            var followUpChunks =
                await _retrievalService.SearchAsync(
                    followUpQuery,
                    10,
                    cancellationToken);

            // Prefer the document that occurs most frequently in
            // the retrieved follow-up results.
            var preferredDocumentId =
                followUpChunks
                    .GroupBy(x => x.DocumentId)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(preferredDocumentId))
            {
                var preferredChunks = followUpChunks
                    .Where(x =>
                        string.Equals(
                            x.DocumentId,
                            preferredDocumentId,
                            StringComparison.OrdinalIgnoreCase))
                    .Take(5)
                    .ToList();

                if (preferredChunks.Count > 0)
                {
                    chunks = preferredChunks;
                }
            }
        }

        // ------------------------------------------------------------
        // 5. Build compact document context
        //
        // Keeping the context small helps Ollama respond faster.
        // ------------------------------------------------------------

        var context = string.Join(
    "\n\n---\n\n",
    chunks
        .GroupBy(x => x.DocumentId)
        .OrderByDescending(g => g.Count())
        .SelectMany(g => g.Take(5))
        .Select(chunk =>
        {
            var text = chunk.Text.Length > 1800
                ? chunk.Text[..1800]
                : chunk.Text;

            return
                $"Document: {chunk.DocumentId}, " +
                $"Page: {chunk.PageNumber}\n" +
                text;
        }));

        // ------------------------------------------------------------
        // 6. Previous conversation
        //
        // Only the most recent turn is required for resolving
        // conversational references.
        // ------------------------------------------------------------

        var previousConversation = string.Empty;

        if (history.Count > 0)
        {
            var lastTurn = history[^1];

            previousConversation =
                $"""
                Previous Question:
                {lastTurn.Question}

                Previous Answer:
                {lastTurn.Answer}
                """;
        }

        // ------------------------------------------------------------
        // 7. RAG prompt
        // ------------------------------------------------------------

        var prompt =
            $"""
            You are a document question-answering assistant.

            Answer the current question using ONLY the provided
            document context.

            Rules:

            1. Use the previous conversation only to resolve
               references such as "it", "its", "they", "this",
               or "that".

            2. If the current question is a follow-up question,
               assume it refers to the same model, paper, or
               framework from the previous question unless the
               user explicitly changes the topic.

            3. Do NOT switch between T5, BERT, and Transformer
               papers unless the user explicitly asks for a
               comparison.

            4. Do NOT combine facts from different documents.

            5. Use ONLY information contained in the document
               context.

            6. Do NOT use general knowledge.

            7. Do NOT invent information.

            8. If the answer cannot be found in the document
               context, respond exactly:

            The information is not available in the provided documents.

            9. Give a concise answer.

            10. When possible, mention the document ID and page
                number containing the answer.

            Previous Conversation:
            {previousConversation}

            Document Context:
            {context}

            Current Question:
            {request.Message}

            Answer:
            """;

        Console.WriteLine("===== RAG PROMPT =====");
        Console.WriteLine(prompt);
        Console.WriteLine("=====================");

        // ------------------------------------------------------------
        // 8. Call Ollama
        // ------------------------------------------------------------

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

        // ------------------------------------------------------------
        // 9. Save conversation
        // ------------------------------------------------------------

        _conversationMemory.AddTurn(
            conversationId,
            request.Message,
            response.Message.Content);

        // ------------------------------------------------------------
        // 10. Return API response
        // ------------------------------------------------------------

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

    // ------------------------------------------------------------
    // Detect common follow-up references.
    // ------------------------------------------------------------

    private static bool ContainsReference(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return false;
        }

        var lower = question.ToLowerInvariant();

        string[] references =
        [
            "it ",
            "it?",
            "its ",
            "its?",
            "they ",
            "they?",
            "their ",
            "this ",
            "this?",
            "that ",
            "that?",
            "these ",
            "those "
        ];

        return references.Any(lower.Contains);
    }
}

