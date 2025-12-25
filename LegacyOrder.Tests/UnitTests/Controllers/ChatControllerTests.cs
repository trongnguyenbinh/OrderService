using LegacyOrder.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace LegacyOrder.Tests.UnitTests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<ILogger<ChatController>> _mockLogger;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockLogger = new Mock<ILogger<ChatController>>();
        _controller = new ChatController(_mockChatService.Object, _mockLogger.Object);
    }

    #region Ask Endpoint Tests

    [Fact]
    public async Task Ask_WithValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = "What products do you have?",
            SessionId = null
        };

        var response = new ChatResponse
        {
            SessionId = Guid.NewGuid(),
            Message = "We have various products available",
            Timestamp = DateTime.UtcNow
        };

        _mockChatService.Setup(s => s.AskAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Ask(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Ask_WithValidRequest_CallsServiceWithCorrectRequest()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = "Test message",
            SessionId = null
        };

        var response = new ChatResponse
        {
            SessionId = Guid.NewGuid(),
            Message = "Response",
            Timestamp = DateTime.UtcNow
        };

        _mockChatService.Setup(s => s.AskAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _controller.Ask(request, CancellationToken.None);

        // Assert
        _mockChatService.Verify(s => s.AskAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Ask_WithArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = new string('a', 2001),
            SessionId = null
        };

        _mockChatService.Setup(s => s.AskAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Message exceeds maximum length"));

        // Act
        var result = await _controller.Ask(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badResult = result as BadRequestObjectResult;
        badResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Ask_WithInvalidOperationException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = "Test message",
            SessionId = null
        };

        _mockChatService.Setup(s => s.AskAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("OpenAI API key not configured"));

        // Act
        var result = await _controller.Ask(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Ask_WithGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = "Test message",
            SessionId = null
        };

        _mockChatService.Setup(s => s.AskAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Ask(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetHistory Endpoint Tests

    [Fact]
    public async Task GetHistory_WhenSessionExists_ReturnsOkWithHistory()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var history = new ChatHistoryDto
        {
            SessionId = sessionId,
            UserFingerprint = "test-fingerprint",
            Messages = new List<ChatMessageDto>()
        };

        _mockChatService.Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetHistory(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(history);
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetHistory_WhenSessionNotFound_ReturnsNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockChatService.Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatHistoryDto?)null);

        // Act
        var result = await _controller.GetHistory(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetHistory_WithValidSessionId_CallsServiceWithCorrectId()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var history = new ChatHistoryDto
        {
            SessionId = sessionId,
            UserFingerprint = "test-fingerprint",
            Messages = new List<ChatMessageDto>()
        };

        _mockChatService.Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        await _controller.GetHistory(sessionId, CancellationToken.None);

        // Assert
        _mockChatService.Verify(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetHistory_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockChatService.Setup(s => s.GetHistoryAsync(sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetHistory(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion
}

