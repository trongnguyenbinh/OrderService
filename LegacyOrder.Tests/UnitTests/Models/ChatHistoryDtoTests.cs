namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;

public class ChatHistoryDtoTests
{
    [Fact]
    public void ChatHistoryDto_DefaultInitialization_HasEmptyProperties()
    {
        // Arrange & Act
        var dto = new ChatHistoryDto();

        // Assert
        dto.SessionId.Should().Be(Guid.Empty);
        dto.UserFingerprint.Should().Be(string.Empty);
        dto.CreatedAt.Should().Be(default(DateTime));
        dto.LastActivityAt.Should().Be(default(DateTime));
        dto.Messages.Should().NotBeNull();
        dto.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ChatHistoryDto_CanSetAllProperties()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var fingerprint = "test-fingerprint-123";
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var lastActivityAt = DateTime.UtcNow;

        // Act
        var dto = new ChatHistoryDto
        {
            SessionId = sessionId,
            UserFingerprint = fingerprint,
            CreatedAt = createdAt,
            LastActivityAt = lastActivityAt
        };

        // Assert
        dto.SessionId.Should().Be(sessionId);
        dto.UserFingerprint.Should().Be(fingerprint);
        dto.CreatedAt.Should().Be(createdAt);
        dto.LastActivityAt.Should().Be(lastActivityAt);
    }

    [Fact]
    public void ChatHistoryDto_MessagesListCanBePopulated()
    {
        // Arrange
        var dto = new ChatHistoryDto();
        var message1 = new ChatMessageDto { Id = Guid.NewGuid(), Role = "user", Content = "Hello" };
        var message2 = new ChatMessageDto { Id = Guid.NewGuid(), Role = "assistant", Content = "Hi there" };

        // Act
        dto.Messages.Add(message1);
        dto.Messages.Add(message2);

        // Assert
        dto.Messages.Should().HaveCount(2);
        dto.Messages.Should().Contain(message1);
        dto.Messages.Should().Contain(message2);
    }

    [Fact]
    public void ChatHistoryDto_CanClearMessages()
    {
        // Arrange
        var dto = new ChatHistoryDto();
        dto.Messages.Add(new ChatMessageDto { Id = Guid.NewGuid() });
        dto.Messages.Add(new ChatMessageDto { Id = Guid.NewGuid() });

        // Act
        dto.Messages.Clear();

        // Assert
        dto.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ChatHistoryDto_LastActivityAtCanBeUpdated()
    {
        // Arrange
        var initialTime = DateTime.UtcNow.AddHours(-1);
        var dto = new ChatHistoryDto { LastActivityAt = initialTime };
        var newTime = DateTime.UtcNow;

        // Act
        dto.LastActivityAt = newTime;

        // Assert
        dto.LastActivityAt.Should().Be(newTime);
        dto.LastActivityAt.Should().BeAfter(initialTime);
    }

    [Fact]
    public void ChatHistoryDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();

        // Act
        var dto1 = new ChatHistoryDto { SessionId = sessionId1 };
        var dto2 = new ChatHistoryDto { SessionId = sessionId2 };

        dto1.Messages.Add(new ChatMessageDto { Id = Guid.NewGuid() });

        // Assert
        dto1.SessionId.Should().NotBe(dto2.SessionId);
        dto1.Messages.Should().HaveCount(1);
        dto2.Messages.Should().BeEmpty();
    }
}

