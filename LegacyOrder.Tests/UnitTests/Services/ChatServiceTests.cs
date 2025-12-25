using LegacyOrder.Tests.TestFixtures;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace LegacyOrder.Tests.UnitTests.Services;

public class ChatServiceTests
{
    private readonly Mock<IChatRepository> _mockChatRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IOpenAIService> _mockOpenAIService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ChatService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ChatService _chatService;

    public ChatServiceTests()
    {
        _mockChatRepository = new Mock<IChatRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockOpenAIService = new Mock<IOpenAIService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ChatService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup default configuration values
        _mockConfiguration.Setup(c => c["Chat:MaxMessagesPerSession"]).Returns("10");
        _mockConfiguration.Setup(c => c["Chat:MaxMessageLength"]).Returns("2000");

        _chatService = new ChatService(
            _mockChatRepository.Object,
            _mockProductRepository.Object,
            _mockOpenAIService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }



    #region AskAsync Tests

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires integration testing or service refactoring.")]
    public async Task AskAsync_WithValidRequest_CreatesNewSession()
    {
        // This test is skipped because ChatCompletion from the OpenAI SDK is a sealed class
        // and cannot be mocked with Moq. To properly test this, we would need to:
        // 1. Create a wrapper interface for ChatCompletion
        // 2. Refactor IOpenAIService to return the wrapper
        // 3. Or convert this to an integration test
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires integration testing or service refactoring.")]
    public async Task AskAsync_WithExistingSession_RetrievesSession()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task AskAsync_WithMessageExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = new string('a', 2001), // Exceeds default max of 2000
            SessionId = null
        };

        // Act & Assert
        var act = async () => await _chatService.AskAsync(request);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires integration testing or service refactoring.")]
    public async Task AskAsync_SavesUserMessage()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires integration testing or service refactoring.")]
    public async Task AskAsync_SavesAssistantMessage()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires integration testing or service refactoring.")]
    public async Task AskAsync_UpdatesSessionActivity()
    {
        await Task.CompletedTask;
    }

    #endregion

    #region GetHistoryAsync Tests

    [Fact]
    public async Task GetHistoryAsync_WhenSessionExists_ReturnsHistory()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = TestDataBuilder.CreateChatSessionEntity(id: sessionId);
        var history = new ChatHistoryDto { SessionId = sessionId };

        _mockChatRepository.Setup(r => r.GetSessionWithMessagesAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockMapper.Setup(m => m.Map<ChatHistoryDto>(session))
            .Returns(history);

        // Act
        var result = await _chatService.GetHistoryAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenSessionNotFound_ReturnsNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockChatRepository.Setup(r => r.GetSessionWithMessagesAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatSessionEntity?)null);

        // Act
        var result = await _chatService.GetHistoryAsync(sessionId);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}

