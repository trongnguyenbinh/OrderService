using Domain;

namespace LegacyOrder.Tests.UnitTests.Domain;

public class DataContextFactoryTests
{
    [Fact]
    public void CreateDbContext_WithoutVaultEnvironmentVariables_ThrowsException()
    {
        // Arrange
        var factory = new DataContextFactory();
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            // Clear Vault environment variables
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", null);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", null);

            // Act & Assert
            var action = () => factory.CreateDbContext(Array.Empty<string>());
            action.Should().Throw<Exception>()
                .WithMessage("Connection string is not configured.");
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void CreateDbContext_WithVaultAddressNotSet_ThrowsException()
    {
        // Arrange
        var factory = new DataContextFactory();
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");

        try
        {
            // Clear VAULT__ADDRESS
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", null);

            // Act & Assert
            var action = () => factory.CreateDbContext(Array.Empty<string>());
            action.Should().Throw<Exception>()
                .WithMessage("Connection string is not configured.");
        }
        finally
        {
            // Restore original value
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
        }
    }

    [Fact]
    public void CreateDbContext_WithVaultTokenNotSet_ThrowsException()
    {
        // Arrange
        var factory = new DataContextFactory();
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            // Set VAULT__ADDRESS but not VAULT__TOKEN
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "http://vault:8200");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", null);

            // Act & Assert
            var action = () => factory.CreateDbContext(Array.Empty<string>());
            action.Should().Throw<Exception>()
                .WithMessage("Connection string is not configured.");
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void CreateDbContext_WithEmptyVaultAddress_ThrowsException()
    {
        // Arrange
        var factory = new DataContextFactory();
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");

        try
        {
            // Set empty VAULT__ADDRESS
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "");

            // Act & Assert
            var action = () => factory.CreateDbContext(Array.Empty<string>());
            action.Should().Throw<Exception>()
                .WithMessage("Connection string is not configured.");
        }
        finally
        {
            // Restore original value
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
        }
    }

    [Fact]
    public void CreateDbContext_WithEmptyVaultToken_ThrowsException()
    {
        // Arrange
        var factory = new DataContextFactory();
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            // Set VAULT__ADDRESS but empty VAULT__TOKEN
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "http://vault:8200");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", "");

            // Act & Assert
            var action = () => factory.CreateDbContext(Array.Empty<string>());
            action.Should().Throw<Exception>()
                .WithMessage("Connection string is not configured.");
        }
        finally
        {
            // Restore original values
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void CreateDbContext_WithWhitespaceVaultAddress_ThrowsException()
    {
        // Arrange
        var factory = new DataContextFactory();
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");

        try
        {
            // Set whitespace VAULT__ADDRESS
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "   ");

            // Act & Assert
            var action = () => factory.CreateDbContext(Array.Empty<string>());
            action.Should().Throw<Exception>()
                .WithMessage("Connection string is not configured.");
        }
        finally
        {
            // Restore original value
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
        }
    }
}

