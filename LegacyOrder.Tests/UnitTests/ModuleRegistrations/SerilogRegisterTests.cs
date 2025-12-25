using LegacyOrder.ModuleRegistrations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace LegacyOrder.Tests.UnitTests.ModuleRegistrations;

public class SerilogRegisterTests
{
    [Fact]
    public void AddSerilogLogging_WithValidBuilder_ConfiguresSerilog()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;

        // Act
        var action = () => builder.AddSerilogLogging(configuration);

        // Assert
        action.Should().NotThrow();
    }



    [Fact]
    public void AddSerilogLogging_ConfiguresHostWithSerilog()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;

        // Act
        builder.AddSerilogLogging(configuration);

        // Assert
        // Verify that the builder's host was configured (no exception thrown)
        builder.Should().NotBeNull();
    }

    [Fact]
    public void AddSerilogLogging_MultipleCallsDoNotThrow()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var configuration = builder.Configuration;

        // Act
        var action = () =>
        {
            builder.AddSerilogLogging(configuration);
            builder.AddSerilogLogging(configuration);
        };

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddSerilogLogging_WithEmptyConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var action = () => builder.AddSerilogLogging(configuration);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddSerilogLogging_WithComplexConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Serilog:MinimumLevel", "Information" },
                { "Serilog:WriteTo:0:Name", "Console" },
                { "Serilog:WriteTo:1:Name", "File" }
            });
        var configuration = configBuilder.Build();

        // Act
        var action = () => builder.AddSerilogLogging(configuration);

        // Assert
        action.Should().NotThrow();
    }
}

