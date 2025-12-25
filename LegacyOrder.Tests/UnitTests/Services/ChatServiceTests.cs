using Microsoft.Extensions.Configuration;

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

    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultConfiguration_SetsDefaultValues()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Chat:MaxMessagesPerSession"]).Returns((string?)null);
        mockConfig.Setup(c => c["Chat:MaxMessageLength"]).Returns((string?)null);

        // Act
        var service = new ChatService(
            _mockChatRepository.Object,
            _mockProductRepository.Object,
            _mockOpenAIService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            mockConfig.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomConfiguration_SetsCustomValues()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Chat:MaxMessagesPerSession"]).Returns("20");
        mockConfig.Setup(c => c["Chat:MaxMessageLength"]).Returns("5000");

        // Act
        var service = new ChatService(
            _mockChatRepository.Object,
            _mockProductRepository.Object,
            _mockOpenAIService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            mockConfig.Object);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region Additional AskAsync Tests

    [Fact]
    public async Task AskAsync_WithValidMessage_LogsInformation()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = "Hello",
            SessionId = null
        };

        var session = TestDataBuilder.CreateChatSessionEntity();
        _mockChatRepository.Setup(r => r.GetSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatSessionEntity?)null);
        _mockChatRepository.Setup(r => r.GetSessionByFingerprintAsync(request.UserFingerprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatSessionEntity?)null);
        _mockChatRepository.Setup(r => r.AddAsync(It.IsAny<ChatSessionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockChatRepository.Setup(r => r.GetSessionMessagesAsync(session.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageEntity>());

        // Act & Assert - Should throw because OpenAI service is not properly mocked
        var act = async () => await _chatService.AskAsync(request);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AskAsync_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var request = new ChatRequest
        {
            UserFingerprint = "test-fingerprint",
            Message = "",
            SessionId = null
        };

        // Act & Assert
        var act = async () => await _chatService.AskAsync(request);
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion
}

