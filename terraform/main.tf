terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3.0"
    }
  }
  required_version = ">= 1.0"
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "rg" {
  name     = "${var.prefix}-rg"
  location = var.location
}

# App Service Plan (Linux)
resource "azurerm_service_plan" "asp" {
  name                = "${var.prefix}-asp"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = "S1" # Standard small tier
}

# Linux Web App (production slot)
resource "azurerm_linux_web_app" "webapp" {
  name                = "${var.prefix}-app"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.asp.id
  https_only          = true
  site_config {
    minimum_tls_version = "1.2"
  }
  identity {
    type = "SystemAssigned"
  }
}

# Deployment Slot "staging"
resource "azurerm_linux_web_app_slot" "staging_slot" {
  name           = "staging"
  location       = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id = azurerm_service_plan.asp.id
  app_service_name = azurerm_linux_web_app.webapp.name
  depends_on     = [azurerm_linux_web_app.webapp]
}

# Azure SQL Server
resource "random_password" "sql_admin_password" {
  length  = 16
  special = true
}

resource "azurerm_sql_server" "sqlsrv" {
  name                         = "${var.prefix}-sqlsrv"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = "${var.prefix}-admin"
  administrator_login_password = random_password.sql_admin_password.result
  public_network_access_enabled = true
}

# Allow Azure services (0.0.0.0)
resource "azurerm_sql_firewall_rule" "allow_azure" {
  name                = "AllowAzure"
  resource_group_name = azurerm_resource_group.rg.name
  server_name         = azurerm_sql_server.sqlsrv.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}

resource "azurerm_sql_database" "sqldb" {
  name                = "${var.prefix}-sqldb"
  resource_group_name = azurerm_resource_group.rg.name
  server_name         = azurerm_sql_server.sqlsrv.name
  requested_service_objective_name = "Basic"
}

# Azure Key Vault
resource "random_string" "kv_suffix" {
  length  = 8
  upper   = false
  number  = false
  special = false
}

resource "azurerm_key_vault" "kv" {
  name                = "${var.prefix}-kv-${random_string.kv_suffix.result}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
  soft_delete_retention_days = 7
  
  # Access policy for current user (so we can add secrets)
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id
    secret_permissions = ["get", "list", "set"]
  }
}

data "azurerm_client_config" "current" {}

# Allow Web App's managed identity to read secrets
resource "azurerm_key_vault_access_policy" "app_policy" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.webapp.identity[0].principal_id
  secret_permissions = ["get", "list"]
}

# Store SQL connection string in Key Vault
resource "azurerm_key_vault_secret" "sql_conn" {
  name         = "SqlConnectionString"
  value        = "Server=tcp:${azurerm_sql_server.sqlsrv.name}.database.windows.net,1433;Initial Catalog=${azurerm_sql_database.sqldb.name};Persist Security Info=False;User ID=${azurerm_sql_server.sqlsrv.administrator_login};Password=${random_password.sql_admin_password.result};"
  key_vault_id = azurerm_key_vault.kv.id
}

# Azure Container Registry (ACR)
resource "azurerm_container_registry" "acr" {
  name                = "${var.prefix}acr"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "Standard"
  admin_enabled       = true
}

# Application Insights
resource "azurerm_application_insights" "appi" {
  name                = "${var.prefix}-appi"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}
