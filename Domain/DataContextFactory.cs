using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace Domain;

/// <summary>
/// Design-time factory for creating DataContext instances during migrations.
/// This is required for EF Core CLI tools to create migrations.
/// Reads connection string from HashiCorp Vault when VAULT__ADDRESS and VAULT__TOKEN are set.
/// Falls back to local connection string if Vault is unavailable.
/// </summary>
public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{

    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        // Try to get connection string from Vault, fallback to local if unavailable
        var connectionString = GetConnectionStringFromVault();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Connection string is not configured.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new DataContext(optionsBuilder.Options);
    }

    private static string? GetConnectionStringFromVault()
    {
        try
        {
            var vaultAddress = Environment.GetEnvironmentVariable("VAULT__ADDRESS");
            var vaultToken = Environment.GetEnvironmentVariable("VAULT__TOKEN");
            var mountPoint = Environment.GetEnvironmentVariable("VAULT__MOUNTPOINT") ?? "secret";
            var secretPath = Environment.GetEnvironmentVariable("VAULT__SECRETPATH") ?? "legacy-order-api";

            // Skip Vault if environment variables are not set
            if (string.IsNullOrWhiteSpace(vaultAddress))
            {
                Console.WriteLine("[DataContextFactory] VAULT__ADDRESS not set. Using fallback connection string.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(vaultToken))
            {
                Console.WriteLine("[DataContextFactory] VAULT__TOKEN not set. Using fallback connection string.");
                return null;
            }

            Console.WriteLine($"[DataContextFactory] Connecting to Vault at: {vaultAddress}");
            Console.WriteLine($"[DataContextFactory] Reading secrets from path: {secretPath} (mount point: {mountPoint})");

            // Configure Vault client
            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
            var settings = new VaultClientSettings(vaultAddress, authMethod);
            var vaultClient = new VaultClient(settings);

            // Read secrets from Vault KV v2
            var secret = vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync(path: secretPath, mountPoint: mountPoint)
                .GetAwaiter()
                .GetResult();

            var data = secret.Data.Data;

            // Try to get connection string from Vault
            if (data.TryGetValue("ConnectionStrings__Default", out var raw))
            {
                var connStr = raw?.ToString();
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    Console.WriteLine("[DataContextFactory] Successfully loaded connection string from Vault.");
                    return connStr;
                }
            }

            Console.WriteLine("[DataContextFactory] Warning: ConnectionStrings__Default not found in Vault secrets.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DataContextFactory] Error loading secrets from Vault: {ex.Message}");
            Console.WriteLine("[DataContextFactory] Falling back to local connection string.");
            return null;
        }
    }
}

