using Domain.Contractors;
using LegacyOrder.Tests.TestFixtures;

namespace LegacyOrder.Tests.UnitTests.Domain.Entities;

public class ChatMessageEntityTests
{
    [Fact]
    public void ChatMessageEntity_ImplementsIEntity()
    {
        // Arrange & Act
        var message = new ChatMessageEntity();

        // Assert
        message.Should().BeAssignableTo<IEntity<Guid>>();
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetId()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedId = Guid.NewGuid();

        // Act
        message.Id = expectedId;

        // Assert
        message.Id.Should().Be(expectedId);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetSessionId()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedSessionId = Guid.NewGuid();

        // Act
        message.SessionId = expectedSessionId;

        // Assert
        message.SessionId.Should().Be(expectedSessionId);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetRole()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedRole = "assistant";

        // Act
        message.Role = expectedRole;

        // Assert
        message.Role.Should().Be(expectedRole);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetContent()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedContent = "This is a test message";

        // Act
        message.Content = expectedContent;

        // Assert
        message.Content.Should().Be(expectedContent);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetCreatedAt()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedDate = DateTime.UtcNow.AddHours(-1);

        // Act
        message.CreatedAt = expectedDate;

        // Assert
        message.CreatedAt.Should().Be(expectedDate);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetTokenMetadata()
    {
        // Arrange
        var message = new ChatMessageEntity();

        // Act
        message.PromptTokens = 50;
        message.CompletionTokens = 100;
        message.TotalTokens = 150;

        // Assert
        message.PromptTokens.Should().Be(50);
        message.CompletionTokens.Should().Be(100);
        message.TotalTokens.Should().Be(150);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetModelUsed()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedModel = "gpt-4o-mini";

        // Act
        message.ModelUsed = expectedModel;

        // Assert
        message.ModelUsed.Should().Be(expectedModel);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetToolCalled()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedTool = "search_products";

        // Act
        message.ToolCalled = expectedTool;

        // Assert
        message.ToolCalled.Should().Be(expectedTool);
    }

    [Fact]
    public void ChatMessageEntity_CanSetAndGetResponseTimeMs()
    {
        // Arrange
        var message = new ChatMessageEntity();
        var expectedTime = 1234;

        // Act
        message.ResponseTimeMs = expectedTime;

        // Assert
        message.ResponseTimeMs.Should().Be(expectedTime);
    }

    [Fact]
    public void ChatMessageEntity_AllPropertiesCanBeSetViaBuilder()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddHours(-2);

        // Act
        var message = TestDataBuilder.CreateChatMessageEntity(
            id: id,
            sessionId: sessionId,
            role: "assistant",
            content: "Test response",
            createdAt: createdAt,
            promptTokens: 100,
            completionTokens: 200,
            totalTokens: 300,
            modelUsed: "gpt-4o-mini",
            toolCalled: "search_products",
            responseTimeMs: 2500);

        // Assert
        message.Id.Should().Be(id);
        message.SessionId.Should().Be(sessionId);
        message.Role.Should().Be("assistant");
        message.Content.Should().Be("Test response");
        message.CreatedAt.Should().Be(createdAt);
        message.PromptTokens.Should().Be(100);
        message.CompletionTokens.Should().Be(200);
        message.TotalTokens.Should().Be(300);
        message.ModelUsed.Should().Be("gpt-4o-mini");
        message.ToolCalled.Should().Be("search_products");
        message.ResponseTimeMs.Should().Be(2500);
    }
}

