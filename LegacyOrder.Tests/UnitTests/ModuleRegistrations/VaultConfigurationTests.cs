using LegacyOrder.ModuleRegistrations;
using Microsoft.Extensions.Configuration;

namespace LegacyOrder.Tests.UnitTests.ModuleRegistrations;

public class VaultConfigurationTests
{
    [Fact]
    public void LoadSecretsFromVault_WithoutVaultAddress_ThrowsException()
    {
        // Arrange
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", null);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", "test-token");

            var configManager = new ConfigurationManager();

            // Act & Assert
            var action = () => configManager.LoadSecretsFromVault();
            action.Should().Throw<Exception>()
                .WithMessage("VAULT__ADDRESS is not set.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void LoadSecretsFromVault_WithoutVaultToken_ThrowsException()
    {
        // Arrange
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "http://vault:8200");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", null);

            var configManager = new ConfigurationManager();

            // Act & Assert
            var action = () => configManager.LoadSecretsFromVault();
            action.Should().Throw<Exception>()
                .WithMessage("VAULT__TOKEN is not set.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void LoadSecretsFromVault_WithEmptyVaultAddress_ThrowsException()
    {
        // Arrange
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", "test-token");

            var configManager = new ConfigurationManager();

            // Act & Assert
            var action = () => configManager.LoadSecretsFromVault();
            action.Should().Throw<Exception>()
                .WithMessage("VAULT__ADDRESS is not set.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void LoadSecretsFromVault_WithEmptyVaultToken_ThrowsException()
    {
        // Arrange
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "http://vault:8200");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", "");

            var configManager = new ConfigurationManager();

            // Act & Assert
            var action = () => configManager.LoadSecretsFromVault();
            action.Should().Throw<Exception>()
                .WithMessage("VAULT__TOKEN is not set.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void LoadSecretsFromVault_WithWhitespaceVaultAddress_ThrowsException()
    {
        // Arrange
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "   ");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", "test-token");

            var configManager = new ConfigurationManager();

            // Act & Assert
            var action = () => configManager.LoadSecretsFromVault();
            action.Should().Throw<Exception>()
                .WithMessage("VAULT__ADDRESS is not set.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }

    [Fact]
    public void LoadSecretsFromVault_WithWhitespaceVaultToken_ThrowsException()
    {
        // Arrange
        var originalVaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var originalVaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");

        try
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", "http://vault:8200");
            Environment.SetEnvironmentVariable("VAULT__TOKEN", "   ");

            var configManager = new ConfigurationManager();

            // Act & Assert
            var action = () => configManager.LoadSecretsFromVault();
            action.Should().Throw<Exception>()
                .WithMessage("VAULT__TOKEN is not set.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VAULT__ADDRESS", originalVaultAddress);
            Environment.SetEnvironmentVariable("VAULT__TOKEN", originalVaultToken);
        }
    }
}

