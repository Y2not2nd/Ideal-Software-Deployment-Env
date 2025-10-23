using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from environment (App Settings will supply KeyVault name)
builder.Configuration.AddEnvironmentVariables();

// Get Key Vault name from env var
string vaultName = builder.Configuration["VaultName"] ?? throw new Exception("VaultName not set");
var kvUri = new Uri($"https://{vaultName}.vault.azure.net/");
var secretClient = new SecretClient(kvUri, new DefaultAzureCredential());

// Fetch the SQL connection string from Key Vault
KeyVaultSecret secret = await secretClient.GetSecretAsync("SqlConnectionString");
string connString = secret.Value;

// ✅ Store the connection string in configuration (for DI)
builder.Configuration["ConnectionStrings:DefaultConnection"] = connString;

// ✅ Ensure the database table exists
using (var conn = new SqlConnection(connString))
{
    await conn.OpenAsync();
    var createCmd = new SqlCommand(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clicks' AND xtype='U')
        CREATE TABLE Clicks(Id INT IDENTITY(1,1) PRIMARY KEY, Count INT);
        IF NOT EXISTS (SELECT * FROM Clicks)
        INSERT INTO Clicks(Count) VALUES (0);
    ", conn);
    await createCmd.ExecuteNo
