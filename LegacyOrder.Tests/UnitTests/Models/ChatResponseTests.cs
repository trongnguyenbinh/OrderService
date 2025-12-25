namespace LegacyOrder.Tests.UnitTests.Models;

using Model.Models;

public class ChatResponseTests
{
    [Fact]
    public void ChatResponse_CanBeCreatedWithAllProperties()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var message = "Hello, how can I help you?";
        var timestamp = DateTime.UtcNow;

        // Act
        var response = new ChatResponse
        {
            SessionId = sessionId,
            Message = message,
            Timestamp = timestamp
        };

        // Assert
        response.SessionId.Should().Be(sessionId);
        response.Message.Should().Be(message);
        response.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void ChatResponse_DefaultInitialization_HasDefaultValues()
    {
        // Arrange & Act
        var response = new ChatResponse();

        // Assert
        response.SessionId.Should().Be(Guid.Empty);
        response.Message.Should().Be(string.Empty);
        response.Timestamp.Should().Be(default(DateTime));
    }

    [Fact]
    public void ChatResponse_CanBeModifiedAfterCreation()
    {
        // Arrange
        var response = new ChatResponse { SessionId = Guid.NewGuid() };
        var newMessage = "Updated message";
        var newTimestamp = DateTime.UtcNow;

        // Act
        response.Message = newMessage;
        response.Timestamp = newTimestamp;

        // Assert
        response.Message.Should().Be(newMessage);
        response.Timestamp.Should().Be(newTimestamp);
    }

    [Fact]
    public void ChatResponse_MultipleInstances_AreIndependent()
    {
        // Arrange
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();

        // Act
        var response1 = new ChatResponse { SessionId = sessionId1, Message = "Message 1" };
        var response2 = new ChatResponse { SessionId = sessionId2, Message = "Message 2" };

        // Assert
        response1.SessionId.Should().NotBe(response2.SessionId);
        response1.Message.Should().NotBe(response2.Message);
    }
}

