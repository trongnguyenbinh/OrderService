using Scalar.AspNetCore;
using LegacyOrder.ModuleRegistrations;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.AddSerilogLogging(builder.Configuration);

var services = builder.Services;
services.AddControllers();
services.AddEndpointsApiExplorer();

builder.Configuration.LoadSecretsFromVault();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Connection string is not configured.");
}

services.AddRepositoryCollection(connectionString);
services.AddServiceCollection();
services.AddAutoMapper(typeof(Program));

#region Swagger/OpenAPI Configuration

services.AddSwaggerGen();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("LegacyOrder API")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
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
