# Ideal Software Deployment Project

This project demonstrates a complete Azure-based software deployment pipeline using Terraform for infrastructure-as-code, ASP.NET Core applications, and GitHub Actions for CI/CD.

## Project Structure

```
root/
├── terraform/                         # All infrastructure-as-code files
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   └── provider.tf
├── src/
│   ├── v1/                            # First version of the Click Counter app
│   │   ├── Controllers/
│   │   │   └── HomeController.cs
│   │   ├── Views/
│   │   │   └── Home/
│   │   │       └── Index.cshtml
│   │   ├── Program.cs
│   │   ├── ClickCounterApp.csproj
│   │   └── Dockerfile
│   └── v2/                            # Second version with Dark Mode Feature
│       ├── Controllers/
│       │   └── HomeController.cs
│       ├── Views/
│       │   └── Home/
│       │       └── Index.cshtml
│       ├── Program.cs
│       ├── ClickCounterApp.csproj
│       └── Dockerfile
├── tests/
│   └── integration/
│       ├── IntegrationTest.cs
│       └── IntegrationTests.csproj
├── .github/
│   └── workflows/
│       ├── initial-deployment.yml
│       └── update-with-feature.yml
└── README.md
```

## Infrastructure Components

The Terraform configuration creates the following Azure resources in the UK South region:

- **Resource Group**: `yassin-matek-rg`
- **App Service Plan**: Linux-based S1 tier
- **Linux Web App**: With HTTPS only and TLS 1.2 minimum
- **Deployment Slot**: Named "staging" for blue/green deployments
- **Azure SQL Server & Database**: With firewall rules for Azure services
- **Azure Key Vault**: Stores secrets with managed identity access
- **Azure Container Registry (ACR)**: For Docker image storage
- **Application Insights**: For monitoring and telemetry

## Application Features

### Version 1 (v1)
- Simple click counter application
- Persists count to Azure SQL Database
- Uses Azure Key Vault for secure connection string storage
- Managed Identity authentication

### Version 2 (v2)
- All v1 features plus dark mode UI
- Feature flag controlled via App Settings
- Demonstrates feature toggle capabilities

## CI/CD Pipeline

### Initial Deployment Workflow
- **CodeQL Analysis**: Static security analysis
- **Dockerfile Linting**: Hadolint security checks
- **Terraform Security Scan**: tfsec security validation
- **Build & Deploy**: Docker image build, push to ACR, deploy to staging slot
- **Production Swap**: Blue/green deployment to production

### Feature Update Workflow
- **v2 Deployment**: Builds and deploys v2 with dark mode
- **Feature Flag Toggle**: Enables dark mode via App Settings
- **Health Verification**: Post-deployment log checks

## Security Features

- **HTTPS Only**: All traffic encrypted with TLS 1.2+
- **Managed Identity**: No hardcoded credentials
- **Key Vault Integration**: Secure secret management
- **Static Analysis**: CodeQL, Hadolint, and tfsec scans
- **Firewall Rules**: Restricted database access

## Getting Started

1. **Configure Azure Credentials**: Set up service principal and GitHub secrets
2. **Deploy Infrastructure**: Run `terraform init` and `terraform apply`
3. **Deploy Application**: Push to main branch triggers initial deployment
4. **Enable Features**: Merge v2 code to activate dark mode feature

## Prerequisites

- Azure subscription with appropriate permissions
- GitHub repository with Actions enabled
- Terraform CLI installed
- Docker CLI installed
- .NET 6.0 SDK

## GitHub Secrets Required

- `AZURE_CREDENTIALS`: Service principal credentials
- `ACR_LOGIN_SERVER`: ACR login server URL (yassindevacr.azurecr.io)
- `KEY_VAULT_NAME`: Azure Key Vault name (yassindev-kv-tleoqsfk)

## Monitoring & Logs

- Application Insights provides telemetry and performance monitoring
- Azure App Service logs available via Azure Portal or CLI
- GitHub Actions logs show deployment status and any failures

## Rollback Strategy

While not automated in this demo, rollback can be performed by:
- Swapping deployment slots back to previous version
- Reverting App Settings to disable features
- Using Azure CLI commands in GitHub Actions

This project demonstrates modern DevOps practices with infrastructure-as-code, containerization, secure deployments, and feature flag management.
