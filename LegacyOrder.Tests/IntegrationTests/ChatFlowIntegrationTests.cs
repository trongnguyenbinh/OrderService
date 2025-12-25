using Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrder.Tests.IntegrationTests;

public class ChatFlowIntegrationTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ChatRepository _chatRepository;

    public ChatFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);

        var chatRepositoryLogger = LoggerFixture.CreateNullLogger<ChatRepository>();
        _chatRepository = new ChatRepository(_context, chatRepositoryLogger);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Chat Session Creation and Message Flow Tests

    [Fact]
    public async Task ChatFlow_MessageCountIncreases_WithEachMessage()
    {
        // Arrange
        var session = TestDataBuilder.CreateChatSessionEntity();
        await _chatRepository.AddAsync(session);

        var initialCount = await _chatRepository.GetMessageCountForSessionAsync(session.Id);

        var message = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);

        // Act
        await _chatRepository.AddMessageAsync(message);

        // Assert
        var finalCount = await _chatRepository.GetMessageCountForSessionAsync(session.Id);
        finalCount.Should().Be(initialCount + 1);
    }

    #endregion
}

