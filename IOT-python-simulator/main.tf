# main.tf

# Krok 1: Konfiguracja dostawcy Azure
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

# Definicja zmiennych
variable "resource_group_name" {
  description = "Nazwa grupy zasobów."
  default     = "smartwarehouse-rg"
}

variable "location" {
  description = "Region Azure."
  default     = "East US 2"
}

variable "base_name" {
  description = "Podstawowa, unikalna nazwa dla zasobów."
  default     = "sw-iot-2506"
}

# Krok 2: Utworzenie grupy zasobów
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# Krok 3: Utworzenie App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "${var.base_name}-plan"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "B1"
}

# Krok 4: Utworzenie serwera i bazy danych MS SQL
resource "azurerm_mssql_server" "main" {
  name                         = "${var.base_name}-sqlserver"
  location                     = azurerm_resource_group.main.location
  resource_group_name          = azurerm_resource_group.main.name
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = "StrongPassword!12345"
}

resource "azurerm_mssql_database" "main" {
  name      = "${var.base_name}-db"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S0"
}

# Krok 5: Utworzenie konta, bazy i kontenera Cosmos DB
resource "azurerm_cosmosdb_account" "main" {
  name                = "${var.base_name}-cosmos"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = azurerm_resource_group.main.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "main" {
  name                = "IoTData"
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
}

resource "azurerm_cosmosdb_sql_container" "main" {
  name                 = "Telemetry"
  resource_group_name  = azurerm_resource_group.main.name
  account_name         = azurerm_cosmosdb_account.main.name
  database_name        = azurerm_cosmosdb_sql_database.main.name
  partition_key_paths  = ["/deviceId"]
  # POPRAWKA: Usunięto 'analytical_storage_ttl', ponieważ nie jest on wspierany dla konta bez włączonego Analytical Store.
}

# Krok 6: Utworzenie IoT Hub
resource "azurerm_iothub" "main" {
  name                = "${var.base_name}-iothub"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  sku {
    name     = "S1"
    capacity = 1
  }
}

data "azurerm_iothub_shared_access_policy" "iothub_policy" {
  name                = "iothubowner"
  resource_group_name = azurerm_resource_group.main.name
  iothub_name         = azurerm_iothub.main.name
  
  depends_on = [azurerm_iothub.main]
}

# Krok 7: Utworzenie Storage Account
resource "azurerm_storage_account" "main" {
  name                     = "swiot2506storage"
  location                 = azurerm_resource_group.main.location
  resource_group_name      = azurerm_resource_group.main.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Krok 8: Utworzenie Application Insights
resource "azurerm_application_insights" "main" {
  name                = "${var.base_name}-appinsights"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  application_type    = "web"

  lifecycle {
    ignore_changes = [
      workspace_id, # Ignoruj zmiany tego atrybutu
    ]
  }
}

# Krok 9: Utworzenie APLIKACJI FUNKCJI (zamiast Web App)
# To jest kluczowa zmiana, aby dopasować infrastrukturę do Twojego kodu.
resource "azurerm_linux_function_app" "backend" {
  name                = "${var.base_name}-backend-api" # Nazwa URL pozostaje ta sama
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id
  
  # Aplikacje Funkcji wymagają konta storage do zarządzania triggerami i logami
  storage_account_name       = azurerm_storage_account.main.name
  storage_account_access_key = azurerm_storage_account.main.primary_access_key

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  # Konfiguracja dla Azure Functions
  app_settings = {
    # Podstawowe ustawienia dla aplikacji funkcji
    "FUNCTIONS_WORKER_RUNTIME" = "dotnet-isolated"
    "AzureWebJobsStorage"      = azurerm_storage_account.main.primary_connection_string

    # Twoje dotychczasowe zmienne, które są nadal potrzebne
    "SqlConnectionString"                   = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};User ID=${azurerm_mssql_server.main.administrator_login};Password=${azurerm_mssql_server.main.administrator_login_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    "CosmosDbConnectionString"              = azurerm_cosmosdb_account.main.connection_strings[0]
    "IoTHubConnection"                      = data.azurerm_iothub_shared_access_policy.iothub_policy.primary_connection_string
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.main.connection_string
    "ASPNETCORE_ENVIRONMENT"                = "Production"
    "WEBSITE_WEBDEPLOY_USE_SCM" = "true"
  }
}

# Krok 10: Utworzenie aplikacji frontendowej
resource "azurerm_static_web_app" "frontend" {
  name                = "${var.base_name}-frontend-app"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

# Krok 11: Utworzenie Azure Key Vault
resource "azurerm_key_vault" "main" {
  name                = "${var.base_name}-keyvault"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
}
