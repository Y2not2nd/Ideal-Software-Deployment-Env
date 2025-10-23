output "sql_connection_string_secret_id" {
  value = azurerm_key_vault_secret.sql_conn.id
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}
