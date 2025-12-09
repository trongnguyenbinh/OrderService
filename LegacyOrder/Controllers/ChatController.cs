using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Model.RequestModels;
using Service.Interfaces;

namespace LegacyOrder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Send a question to the AI assistant
    /// </summary>
    /// <param name="request">Chat request containing user fingerprint, message, and optional session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chat response with session ID and AI-generated message</returns>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "API: Chat request received - Fingerprint: {Fingerprint}, SessionId: {SessionId}, MessageLength: {MessageLength}",
            request.UserFingerprint, request.SessionId, request.Message?.Length ?? 0);

        try
        {
            var response = await _chatService.AskAsync(request, cancellationToken);

            _logger.LogInformation(
                "API: Chat response generated successfully - SessionId: {SessionId}",
                response.SessionId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "API: Chat request validation failed");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "API: OpenAI service configuration error");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "AI service is not properly configured", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Unexpected error processing chat request");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while processing your request", details = ex.Message });
        }
    }

    /// <summary>
    /// Get chat history for a specific session
    /// </summary>
    /// <param name="sessionId">The session ID to retrieve history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chat history with all messages in the session</returns>
    [HttpGet("history/{sessionId}")]
    [ProducesResponseType(typeof(ChatHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistory(Guid sessionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Getting chat history for session: {SessionId}", sessionId);

        try
        {
            var history = await _chatService.GetHistoryAsync(sessionId, cancellationToken);

            if (history == null)
            {
                _logger.LogWarning("API: Chat session not found: {SessionId}", sessionId);
                return NotFound(new { error = $"Chat session with ID {sessionId} not found" });
            }

            _logger.LogInformation(
                "API: Successfully retrieved chat history - SessionId: {SessionId}, MessageCount: {MessageCount}",
                sessionId, history.Messages.Count);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API: Error retrieving chat history for session: {SessionId}", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving chat history", details = ex.Message });
        }
    }
}
