using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.UnitTests.Repositories;

public class ChatRepositoryTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ChatRepository _repository;
    private readonly Mock<ILogger<ChatRepository>> _mockLogger;

    public ChatRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _mockLogger = LoggerFixture.CreateLogger<ChatRepository>();
        _repository = new ChatRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenSessionExists_ReturnsSession()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.UserFingerprint.Should().Be(session.UserFingerprint);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_UsesAsNoTracking()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        _context.Entry(result!).State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenSessionsExist_ReturnsAllSessions()
    {
        // Arrange
        var session1 = TestDataBuilder.CreateChatSessionEntity();
        var session2 = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddRangeAsync(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == session1.Id);
        result.Should().Contain(s => s.Id == session2.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoSessions_ReturnsEmptyList()
    {
        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByLastActivityAtDescending()
    {
        // Arrange
        var session1 = TestDataBuilder.CreateChatSessionEntity(lastActivityAt: DateTime.UtcNow.AddDays(-2));
        var session2 = TestDataBuilder.CreateChatSessionEntity(lastActivityAt: DateTime.UtcNow.AddDays(-1));
        var session3 = TestDataBuilder.CreateChatSessionEntity(lastActivityAt: DateTime.UtcNow);
        
        await _context.ChatSessions.AddRangeAsync(session1, session2, session3);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].LastActivityAt.Should().BeOnOrAfter(result[1].LastActivityAt);
        result[1].LastActivityAt.Should().BeOnOrAfter(result[2].LastActivityAt);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidSession_SavesSessionToDatabase()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();

        // Act
        var result = await _repository.AddAsync(session);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(session.Id);

        var savedSession = await _context.ChatSessions.FindAsync(session.Id);
        savedSession.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidSession_GeneratesNewGuid()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();

        // Act
        var result = await _repository.AddAsync(session);

        // Assert
        result.Id.Should().NotBeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidSession_UpdatesSession()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        session.IsActive = false;
        var result = await _repository.UpdateAsync(session);

        // Assert
        result.IsActive.Should().BeFalse();

        var updatedSession = await _context.ChatSessions.FindAsync(session.Id);
        updatedSession!.IsActive.Should().BeFalse();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenSessionExists_DeletesSession()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(session.Id);

        // Assert
        result.Should().BeTrue();

        var deletedSession = await _context.ChatSessions.FindAsync(session.Id);
        deletedSession.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenSessionNotFound_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetSessionByIdAsync Tests

    [Fact]
    public async Task GetSessionByIdAsync_WhenActiveSessionExists_ReturnsSession()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity(isActive: true);
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionByIdAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetSessionByIdAsync_WhenSessionInactive_ReturnsNull()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity(isActive: false);
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionByIdAsync(session.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSessionByFingerprintAsync Tests

    [Fact]
    public async Task GetSessionByFingerprintAsync_WhenActiveSessionExists_ReturnsSession()
    {
        // Arrange
        var fingerprint = "unique-fingerprint-123";
        var session = TestDataBuilder.CreateChatSessionEntity(userFingerprint: fingerprint, isActive: true);
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionByFingerprintAsync(fingerprint);

        // Assert
        result.Should().NotBeNull();
        result!.UserFingerprint.Should().Be(fingerprint);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetSessionByFingerprintAsync_WhenMultipleSessionsExist_ReturnsLatestActive()
    {
        // Arrange
        var fingerprint = "shared-fingerprint";
        var session1 = TestDataBuilder.CreateChatSessionEntity(
            userFingerprint: fingerprint,
            isActive: true,
            lastActivityAt: DateTime.UtcNow.AddDays(-1));
        var session2 = TestDataBuilder.CreateChatSessionEntity(
            userFingerprint: fingerprint,
            isActive: true,
            lastActivityAt: DateTime.UtcNow);

        await _context.ChatSessions.AddRangeAsync(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionByFingerprintAsync(fingerprint);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session2.Id);
    }

    [Fact]
    public async Task GetSessionByFingerprintAsync_WhenNoActiveSession_ReturnsNull()
    {
        // Arrange
        var fingerprint = "nonexistent-fingerprint";

        // Act
        var result = await _repository.GetSessionByFingerprintAsync(fingerprint);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSessionWithMessagesAsync Tests

    [Fact]
    public async Task GetSessionWithMessagesAsync_WhenSessionExists_ReturnsSessionWithMessages()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        var message1 = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);
        var message2 = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);

        await _context.ChatSessions.AddAsync(session);
        await _context.ChatMessages.AddRangeAsync(message1, message2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionWithMessagesAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.Messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSessionWithMessagesAsync_WhenSessionNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetSessionWithMessagesAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetSessionMessagesAsync Tests

    [Fact]
    public async Task GetSessionMessagesAsync_WhenMessagesExist_ReturnsMessages()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        var message1 = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);
        var message2 = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);

        await _context.ChatSessions.AddAsync(session);
        await _context.ChatMessages.AddRangeAsync(message1, message2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionMessagesAsync(session.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSessionMessagesAsync_WithLimit_ReturnsLimitedMessages()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        var messages = new List<ChatMessageEntity>();
        for (int i = 0; i < 5; i++)
        {
            messages.Add(TestDataBuilder.CreateChatMessageEntity(
                sessionId: session.Id,
                createdAt: DateTime.UtcNow.AddMinutes(i)));
        }

        await _context.ChatSessions.AddAsync(session);
        await _context.ChatMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionMessagesAsync(session.Id, limit: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSessionMessagesAsync_WhenNoMessages_ReturnsEmptyList()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionMessagesAsync(session.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddMessageAsync Tests

    [Fact]
    public async Task AddMessageAsync_WithValidMessage_SavesMessageToDatabase()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        var message = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);

        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AddMessageAsync(message);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(message.Id);

        var savedMessage = await _context.ChatMessages.FindAsync(message.Id);
        savedMessage.Should().NotBeNull();
    }

    #endregion

    #region UpdateSessionActivityAsync Tests

    [Fact]
    public async Task UpdateSessionActivityAsync_UpdatesLastActivityAt()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity(lastActivityAt: DateTime.UtcNow.AddHours(-1));
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        var oldActivityTime = session.LastActivityAt;

        // Act
        await Task.Delay(100); // Small delay to ensure time difference
        await _repository.UpdateSessionActivityAsync(session.Id);

        // Assert
        var updatedSession = await _context.ChatSessions.FindAsync(session.Id);
        updatedSession!.LastActivityAt.Should().BeAfter(oldActivityTime);
    }

    #endregion

    #region GetMessageCountForSessionAsync Tests

    [Fact]
    public async Task GetMessageCountForSessionAsync_ReturnsCorrectCount()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        var messages = new List<ChatMessageEntity>();
        for (int i = 0; i < 5; i++)
        {
            messages.Add(TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id));
        }

        await _context.ChatSessions.AddAsync(session);
        await _context.ChatMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetMessageCountForSessionAsync(session.Id);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetMessageCountForSessionAsync_WhenNoMessages_ReturnsZero()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _context.ChatSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetMessageCountForSessionAsync(session.Id);

        // Assert
        result.Should().Be(0);
    }

    #endregion
}

