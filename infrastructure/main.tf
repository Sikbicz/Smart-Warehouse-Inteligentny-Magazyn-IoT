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

# Definicja zmiennych dla unikalnych nazw i lokalizacji
variable "resource_group_name" {
  description = "Nazwa grupy zasobów."
  default     = "smartwarehouse-rg"
}

variable "location" {
  description = "Region Azure."
  default     = "East US 2"
}

variable "app_service_plan_name" {
  description = "Nazwa planu App Service."
  default     = "smartwarehouse-plan"
}

variable "backend_app_name" {
  description = "Unikalna nazwa dla aplikacji backendowej (API)."
  default     = "smartwarehouse-backend-api"
}

variable "frontend_app_name" {
  description = "Unikalna nazwa dla aplikacji frontendowej."
  default     = "smartwarehouse-frontend-app"
}

# Krok 2: Utworzenie grupy zasobów
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# Krok 3: Utworzenie App Service Plan (środowisko uruchomieniowe dla aplikacji)
resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux" # Lepsze dla .NET
  sku_name            = "B1"    # Basic tier, wystarczający na start
}

# Krok 4: Utworzenie aplikacji backendowej (.NET API)
resource "azurerm_linux_web_app" "backend" {
  name                = var.backend_app_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "8.0" # Zgodnie z Twoim projektem .NET 8[15]
    }
  }
  
  # Tutaj zdefiniujesz zmienne środowiskowe, np. connection string
  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = "Production"
    # Connection string do bazy danych zostanie dodany dynamicznie
    "DatabaseConnectionString" = azurerm_sql_database.main.connection_string[0] 
  }
}

# Krok 5: Utworzenie aplikacji frontendowej (React - Static Web App)[16]
resource "azurerm_static_site" "frontend" {
  name                = var.frontend_app_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
}

# Krok 6: Utworzenie serwera i bazy danych SQL
resource "azurerm_sql_server" "main" {
  name                         = "smartwarehouse-accounts"
  location                     = azurerm_resource_group.main.location
  resource_group_name          = azurerm_resource_group.main.name
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = "Smartwarehouse"
}

resource "azurerm_sql_database" "main" {
  name                = "smartwarehouse-db"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_sql_server.main.name
  sku_name            = "S0" # Standard tier
}

# Krok 7: Utworzenie IoT Hub do odbierania danych z urządzeń[12]
resource "azurerm_iothub" "main" {
  name                = "smartwarehouse-iothub-main"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  sku {
    name     = "S1"
    capacity = 1
  }
}

# Krok 8 (Opcjonalnie, ale zalecane): Utworzenie Azure Key Vault do przechowywania sekretów
resource "azurerm_key_vault" "main" {
  name                = "smartwarehouse-keyvault-zmienne"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"
}

