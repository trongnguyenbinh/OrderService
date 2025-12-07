using LegacyOrder.ModuleRegistrations;
using LegacyOrder.Tests.TestFixtures;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace LegacyOrder.Tests.UnitTests.Middleware;

public class GlobalExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _mockLogger;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlingMiddlewareTests()
    {
        _mockLogger = LoggerFixture.CreateLogger<GlobalExceptionHandlingMiddleware>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    #region KeyNotFoundException Tests

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_Returns404()
    {
        // Arrange
        var exception = new KeyNotFoundException("Product not found");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var exception = new KeyNotFoundException("Product not found");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("Product not found");
        errorResponse.StatusCode.Should().Be(404);
    }

    #endregion

    #region ArgumentException Tests

    [Fact]
    public async Task InvokeAsync_WithArgumentException_Returns400()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentNullException_Returns400()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName", "Parameter cannot be null");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    #endregion

    #region InvalidOperationException Tests

    [Fact]
    public async Task InvokeAsync_WithInvalidOperationException_Returns400()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    #endregion

    #region Generic Exception Tests

    [Fact]
    public async Task InvokeAsync_WithGenericException_Returns500()
    {
        // Arrange
        var exception = new Exception("Unexpected error");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_ReturnsGenericErrorMessage()
    {
        // Arrange
        var exception = new Exception("Unexpected error");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("An error occurred while processing your request");
        errorResponse.StatusCode.Should().Be(500);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task InvokeAsync_SetsContentTypeToApplicationJson()
    {
        // Arrange
        var exception = new Exception("Test error");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_ReturnsErrorResponseWithTimestamp()
    {
        // Arrange
        var beforeExecution = DateTime.UtcNow;
        var exception = new Exception("Test error");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);
        var afterExecution = DateTime.UtcNow;

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Timestamp.Should().BeOnOrAfter(beforeExecution);
        errorResponse.Timestamp.Should().BeOnOrBefore(afterExecution);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsErrorResponseWithPath()
    {
        // Arrange
        _httpContext.Request.Path = "/api/products/123";
        var exception = new Exception("Test error");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Path.Should().Be("/api/products/123");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task InvokeAsync_WithBadRequestException_LogsWarning()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithNotFoundException_LogsWarning()
    {
        // Arrange
        var exception = new KeyNotFoundException("Not found");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_LogsError()
    {
        // Arrange
        var exception = new Exception("Unexpected error");
        RequestDelegate next = (HttpContext hc) => throw exception;
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region Success Path Tests

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext hc) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_DoesNotModifyResponse()
    {
        // Arrange
        RequestDelegate next = (HttpContext hc) =>
        {
            hc.Response.StatusCode = 200;
            return Task.CompletedTask;
        };
        var middleware = new GlobalExceptionHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(200);
    }

    #endregion
}


