using System.Net;
using System.Text.Json;

namespace LegacyOrder.ModuleRegistrations;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    // Cache JsonSerializerOptions to avoid creating new instances for every request
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = GetStatusCodeAndMessage(exception);

        // Log the exception with appropriate level
        LogException(exception, statusCode);

        // Create standardized error response
        var errorResponse = new ErrorResponse
        {
            Error = message,
            StatusCode = (int)statusCode,
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path
        };

        // Set response properties
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Serialize and write response using cached options
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }

    private static (HttpStatusCode statusCode, string message) GetStatusCodeAndMessage(Exception exception)
    {
        return exception switch
        {
            // Not Found exceptions
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),

            // Validation exceptions 
            ArgumentNullException => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),

            // Business rule violations
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),

            // Default to Internal Server Error
            _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
        };
    }

    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.BadRequest => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Warning,
            HttpStatusCode.Unauthorized => LogLevel.Warning,
            HttpStatusCode.Forbidden => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(logLevel, exception, "Global exception handler caught: {ExceptionType} - {Message}",
            exception.GetType().Name, exception.Message);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}

