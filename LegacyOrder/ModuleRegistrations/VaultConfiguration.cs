using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace LegacyOrder.ModuleRegistrations;

public static class VaultConfiguration
{
    public static void LoadSecretsFromVault(this ConfigurationManager config)
    {
        var vaultAddress = config["Vault:Address"] ?? Environment.GetEnvironmentVariable("VAULT__ADDRESS");
        var vaultToken = config["Vault:Token"] ?? Environment.GetEnvironmentVariable("VAULT__TOKEN");
        var mountPoint = config["Vault:MountPoint"] ?? "secret";
        var secretPath = config["Vault:SecretPath"] ?? "legacy-order-api";

        if (string.IsNullOrWhiteSpace(vaultAddress))
        {
            Console.WriteLine("Vault configuration skipped: VAULT__ADDRESS is not set.");
            throw new Exception("VAULT__ADDRESS is not set.");
        }

        if (string.IsNullOrWhiteSpace(vaultToken))
        {
            Console.WriteLine("Please set the VAULT__TOKEN environment variable to authenticate with HashiCorp Vault.");
            throw new Exception("VAULT__TOKEN is not set.");
        }

        try
        {
            Console.WriteLine($"Connecting to Vault at: {vaultAddress}");
            Console.WriteLine($"Using token from environment variable: VAULT__TOKEN");

            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);

            var settings = new VaultClientSettings(vaultAddress, authMethod);

            var vaultClient = new VaultClient(settings);

            // KV v2 read
            Console.WriteLine($"Reading secrets from path: {secretPath} (mount point: {mountPoint})");
            var secret = vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync(path: secretPath, mountPoint: mountPoint)
                .GetAwaiter().GetResult();

            var data = secret.Data.Data;

            // Map connection string from Vault to application configuration
            if (data.TryGetValue("ConnectionStrings__Default", out var raw))
            {
                var connStr = raw?.ToString();
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    config["ConnectionStrings:Default"] = connStr;
                    Console.WriteLine("Successfully loaded connection string from Vault.");
                }
            }
            else
            {
                Console.WriteLine("Warning: ConnectionStrings__Default not found in Vault secrets.");
            }

            // Map OpenAI API Key from Vault to application configuration
            if (data.TryGetValue("OpenAI__ApiKey", out var openAIKey))
            {
                var apiKey = openAIKey?.ToString();
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    config["OpenAI:ApiKey"] = apiKey;
                    Console.WriteLine("Successfully loaded OpenAI API key from Vault.");
                }
            }
            else
            {
                Console.WriteLine("Warning: OpenAI__ApiKey not found in Vault secrets.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading secrets from Vault: {ex.Message}");
            throw;
        }
    }
}