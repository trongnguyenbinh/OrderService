namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;

public class ChatMessageDtoTests
{
    [Fact]
    public void ChatMessageDto_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var role = "assistant";
        var content = "Hello, how can I help?";
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ChatMessageDto
        {
            Id = id,
            Role = role,
            Content = content,
            CreatedAt = createdAt
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Role.Should().Be(role);
        dto.Content.Should().Be(content);
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ChatMessageDto_DefaultInitialization_HasEmptyProperties()
    {
        // Arrange & Act
        var dto = new ChatMessageDto();

        // Assert
        dto.Id.Should().Be(Guid.Empty);
        dto.Role.Should().Be(string.Empty);
        dto.Content.Should().Be(string.Empty);
        dto.CreatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void ChatMessageDto_CanBeModifiedAfterCreation()
    {
        // Arrange
        var dto = new ChatMessageDto { Id = Guid.NewGuid() };
        var newRole = "assistant";
        var newContent = "Updated content";

        // Act
        dto.Role = newRole;
        dto.Content = newContent;

        // Assert
        dto.Role.Should().Be(newRole);
        dto.Content.Should().Be(newContent);
    }

    [Fact]
    public void ChatMessageDto_WithUserRole_PropertiesAreCorrect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ChatMessageDto
        {
            Id = id,
            Role = "user",
            Content = "What products do you have?",
            CreatedAt = createdAt
        };

        // Assert
        dto.Role.Should().Be("user");
        dto.Content.Should().Be("What products do you have?");
    }

    [Fact]
    public void ChatMessageDto_WithAssistantRole_PropertiesAreCorrect()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ChatMessageDto
        {
            Id = id,
            Role = "assistant",
            Content = "We have many products available.",
            CreatedAt = createdAt
        };

        // Assert
        dto.Role.Should().Be("assistant");
        dto.Content.Should().Be("We have many products available.");
    }

    [Fact]
    public void ChatMessageDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var dto1 = new ChatMessageDto { Id = id1, Role = "user" };
        var dto2 = new ChatMessageDto { Id = id2, Role = "assistant" };

        // Assert
        dto1.Id.Should().NotBe(dto2.Id);
        dto1.Role.Should().NotBe(dto2.Role);
    }
}

