using Scalar.AspNetCore;
using LegacyOrder.ModuleRegistrations;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.AddSerilogLogging(builder.Configuration);

var service = builder.Services;

service.AddControllers();
service.AddEndpointsApiExplorer();

builder.Configuration.LoadSecretsFromVault();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Connection string is not configured.");
}

service.AddRepositoryCollection(connectionString);

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
