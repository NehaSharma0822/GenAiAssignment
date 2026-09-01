using Microsoft.AspNetCore.Mvc;
using RagAssignment.Api.Interfaces;
using RagAssignment.Api.Models;

namespace RagAssignment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOllamaService _ollamaService;

    public ChatController(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var ollamaRequest = new OllamaChatRequest
        {
            Model = "llama3.2",
            Stream = false,
            Messages =
            [
                new OllamaMessage
                {
                    Role = "user",
                    Content = request.Message
                }
            ]
        };

        var response = await _ollamaService.ChatAsync(
            ollamaRequest,
            cancellationToken);

        return Ok(response);
    }
}