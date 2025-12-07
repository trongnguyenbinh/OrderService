namespace LegacyOrder.ModuleRegistrations;

using Serilog;
using System.Reflection;

public static class SerilogRegister
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        //Config serilog to write to console and file
        builder.Host.UseSerilog((_, serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .Enrich.WithProperty("EnvironmentName", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            .Enrich.WithProperty("ContentRootPath", AppContext.BaseDirectory)
            .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly()?.GetName().Name)
            .WriteTo.Console(
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u12}] [{SourceContext}] {Message:lj}{NewLine}{Exception}") // Write logs to console with a specific template.
            .WriteTo.File(
                path: "logs/log_day_.log",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10485760,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u12}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")); // Write logs to file with a specific template.
    }
}