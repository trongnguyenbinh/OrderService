namespace LegacyOrder.Tests.UnitTests.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Service.Implementations;
using Xunit;
using FluentAssertions;

public class OpenAIServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<OpenAIService>> _mockLogger;

    public OpenAIServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<OpenAIService>>();
    }

    [Fact]
    public void Constructor_WithoutApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns((string?)null);

        // Act & Assert
        var action = () => new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("OpenAI API key is not configured. Please set OpenAI:ApiKey in configuration.");
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("");

        // Act & Assert
        var action = () => new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("OpenAI API key is not configured. Please set OpenAI:ApiKey in configuration.");
    }

    [Fact]
    public void Constructor_WithWhitespaceApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("   ");

        // Act & Assert
        var action = () => new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("OpenAI API key is not configured. Please set OpenAI:ApiKey in configuration.");
    }

    [Fact]
    public void Constructor_WithValidApiKey_UsesDefaultModel()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("test-api-key");
        _mockConfiguration
            .Setup(c => c["OpenAI:Model"])
            .Returns((string?)null);

        // Act
        var service = new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("gpt-4o-mini")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithCustomModel_UsesProvidedModel()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("test-api-key");
        _mockConfiguration
            .Setup(c => c["OpenAI:Model"])
            .Returns("gpt-4");

        // Act
        var service = new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("gpt-4")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_LogsErrorWhenApiKeyNotConfigured()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns((string?)null);

        // Act & Assert
        var action = () => new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);
        action.Should().Throw<InvalidOperationException>();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("OpenAI API key is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidApiKey_LogsInformation()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c["OpenAI:ApiKey"])
            .Returns("test-api-key");
        _mockConfiguration
            .Setup(c => c["OpenAI:Model"])
            .Returns("gpt-4o-mini");

        // Act
        var service = new OpenAIService(_mockConfiguration.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

