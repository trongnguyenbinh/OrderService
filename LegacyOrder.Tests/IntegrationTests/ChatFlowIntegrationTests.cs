using Domain;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

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

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires service refactoring.")]
    public async Task ChatFlow_CreateSessionAndSendMessage_Success()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires service refactoring.")]
    public async Task ChatFlow_SendMultipleMessages_SessionPersists()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires service refactoring.")]
    public async Task ChatFlow_RetrieveSessionByFingerprint_Success()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires service refactoring.")]
    public async Task ChatFlow_GetChatHistory_ReturnsAllMessages()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "ChatCompletion is sealed and cannot be mocked. This test requires service refactoring.")]
    public async Task ChatFlow_SessionActivityUpdated_OnNewMessage()
    {
        await Task.CompletedTask;
    }

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

