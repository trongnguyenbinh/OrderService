using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace LegacyOrder.ModuleRegistrations;

public static class VaultConfiguration
{
    public static void LoadSecretsFromVault(this ConfigurationManager config)
    {
        var vaultAddress = config["Vault:Address"];
        var vaultToken = config["Vault:Token"];
        var mountPoint = config["Vault:MountPoint"] ?? "secret";
        var secretPath = config["Vault:SecretPath"] ?? "legacy-order-api";

        if (string.IsNullOrWhiteSpace(vaultAddress) || string.IsNullOrWhiteSpace(vaultToken))
        {
            throw new Exception("Vault address and token are required.");
        }

        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);

        var settings = new VaultClientSettings(vaultAddress, authMethod)
        {
        };

        var vaultClient = new VaultClient(settings);

        // KV v2 read
        var secret = vaultClient.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path: secretPath, mountPoint: mountPoint)
            .GetAwaiter().GetResult();

        var data = secret.Data.Data;

        // Option 1: specifically map your connection string key
        if (data.TryGetValue("ConnectionStrings__Default", out var raw))
        {
            var connStr = raw?.ToString();
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                config["ConnectionStrings:Default"] = connStr;
            }
        }
    }
}