using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", HealthCheck)
    .WithName("HealthCheck")
    .WithDescription("Health check endpoint for the Order Service API");

app.Run();

static IResult HealthCheck()
{
    return Results.Ok(new
    {
        status = "healthy",
        service = "LegacyOrder API",
        timestamp = DateTime.UtcNow,
        version = "1.0.0"
    });
}
