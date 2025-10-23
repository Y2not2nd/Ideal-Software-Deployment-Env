using System;
using System.Threading.Tasks;
using Xunit;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;

public class IntegrationTest
{
    [Fact]
    public async Task KeyVault_And_SQL_Connectivity_Works()
    {
        // Assume AZURE_CREDENTIALS via Azure Login in workflow provides DefaultAzureCredential
        string vaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
        Assert.False(string.IsNullOrEmpty(vaultName), "Environment variable KEY_VAULT_NAME not set.");

        // Connect to Key Vault and get the SQL connection string
        var kvClient = new SecretClient(new Uri($"https://{vaultName}.vault.azure.net/"), new DefaultAzureCredential());
        
        KeyVaultSecret secret = await kvClient.GetSecretAsync("SqlConnectionString");
        Assert.False(string.IsNullOrEmpty(secret.Value));
        string connString = secret.Value;

        // Test opening a connection to Azure SQL
        using (var conn = new SqlConnection(connString))
        {
            await conn.OpenAsync();
            Assert.Equal(System.Data.ConnectionState.Open, conn.State);
        }
    }
}
