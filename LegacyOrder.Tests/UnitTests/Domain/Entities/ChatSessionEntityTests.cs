using Domain.Contractors;
using LegacyOrder.Tests.TestFixtures;

namespace LegacyOrder.Tests.UnitTests.Domain.Entities;

public class ChatSessionEntityTests
{
    [Fact]
    public void ChatSessionEntity_ImplementsIEntity()
    {
        // Arrange & Act
        var session = new ChatSessionEntity();

        // Assert
        session.Should().BeAssignableTo<IEntity<Guid>>();
    }

    [Fact]
    public void ChatSessionEntity_CanSetAndGetId()
    {
        // Arrange
        var session = new ChatSessionEntity();
        var expectedId = Guid.NewGuid();

        // Act
        session.Id = expectedId;

        // Assert
        session.Id.Should().Be(expectedId);
    }

    [Fact]
    public void ChatSessionEntity_CanSetAndGetUserFingerprint()
    {
        // Arrange
        var session = new ChatSessionEntity();
        var expectedFingerprint = "test-fingerprint-12345";

        // Act
        session.UserFingerprint = expectedFingerprint;

        // Assert
        session.UserFingerprint.Should().Be(expectedFingerprint);
    }

    [Fact]
    public void ChatSessionEntity_CanSetAndGetCreatedAt()
    {
        // Arrange
        var session = new ChatSessionEntity();
        var expectedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        session.CreatedAt = expectedDate;

        // Assert
        session.CreatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void ChatSessionEntity_CanSetAndGetLastActivityAt()
    {
        // Arrange
        var session = new ChatSessionEntity();
        var expectedDate = DateTime.UtcNow;

        // Act
        session.LastActivityAt = expectedDate;

        // Assert
        session.LastActivityAt.Should().Be(expectedDate);
    }

    [Fact]
    public void ChatSessionEntity_CanSetAndGetIsActive()
    {
        // Arrange
        var session = new ChatSessionEntity();

        // Act
        session.IsActive = false;

        // Assert
        session.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ChatSessionEntity_DefaultIsActiveIsTrue()
    {
        // Arrange & Act
        var session = new ChatSessionEntity();

        // Assert
        session.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChatSessionEntity_MessagesCollectionIsInitialized()
    {
        // Arrange & Act
        var session = new ChatSessionEntity();

        // Assert
        session.Messages.Should().NotBeNull();
        session.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ChatSessionEntity_CanAddMessagesToCollection()
    {
        // Arrange
        var session = new ChatSessionEntity();
        var message = TestDataBuilder.CreateChatMessageEntity(sessionId: session.Id);

        // Act
        session.Messages.Add(message);

        // Assert
        session.Messages.Should().HaveCount(1);
        session.Messages.First().Should().Be(message);
    }

    [Fact]
    public void ChatSessionEntity_AllPropertiesCanBeSetViaBuilder()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fingerprint = "unique-fingerprint";
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var lastActivityAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var session = TestDataBuilder.CreateChatSessionEntity(
            id: id,
            userFingerprint: fingerprint,
            createdAt: createdAt,
            lastActivityAt: lastActivityAt,
            isActive: false);

        // Assert
        session.Id.Should().Be(id);
        session.UserFingerprint.Should().Be(fingerprint);
        session.CreatedAt.Should().Be(createdAt);
        session.LastActivityAt.Should().Be(lastActivityAt);
        session.IsActive.Should().BeFalse();
    }
}

