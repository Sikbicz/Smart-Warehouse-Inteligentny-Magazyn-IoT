# Smart Warehouse Monitoring System - Azure Infrastructure as Code (Azure CLI)

This guide provides the Azure CLI commands to provision the necessary Azure resources for your Smart Warehouse Monitoring System. These commands are derived from your `main.tf` Terraform configuration.

**Important:**
*   Replace placeholder values (e.g., `<YOUR_SUBSCRIPTION_ID>`, `<YOUR_SQL_PASSWORD>`) with your actual, secure values.
*   Ensure you are logged into your Azure account via the Azure CLI (`az login`).
*   Set your default subscription (`az account set --subscription "<Your-Subscription-ID-or-Name>"`).
*   Resource names with `$(openssl rand -hex 4)` are designed to be globally unique. You can replace this with a fixed unique string if preferred.

---

## 1. Resource Group

```bash
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"

az group create --name $RESOURCE_GROUP --location $LOCATION
```

## 2. App Service Plan

```bash
BASE_NAME="sw-iot-2506" # Matches base_name from main.tf
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"

az functionapp plan create \
    --resource-group $RESOURCE_GROUP \
    --name "${BASE_NAME}-plan" \
    --location $LOCATION \
    --sku B1 \
    --is-linux
```

## 3. MS SQL Server and Database

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"
SQL_ADMIN_LOGIN="sqladmin"
SQL_ADMIN_PASSWORD="StrongPassword!12345" # IMPORTANT: Change this to a secure password!

# Create SQL Server
az sql server create \
    --name "${BASE_NAME}-sqlserver" \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN_LOGIN \
    --admin-password $SQL_ADMIN_PASSWORD \
    --enable-public-network true # For simplicity, enable public access. Restrict in production!

# Create SQL Database
az sql db create \
    --name "${BASE_NAME}-db" \
    --server "${BASE_NAME}-sqlserver" \
    --resource-group $RESOURCE_GROUP \
    --edition Basic \
    --service-objective Basic

# Configure Firewall Rule (Allow Azure services to access server)
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server "${BASE_NAME}-sqlserver" \
    --name "AllowAzureServices" \
    --start-ip-address "0.0.0.0" \
    --end-ip-address "0.0.0.0"

# Get SQL Connection String (for your application)
az sql db show-connection-string \
    --client ado.net \
    --name "${BASE_NAME}-db" \
    --server "${BASE_NAME}-sqlserver" \
    --query "connectionStrings[0].connectionString"
```

## 4. Cosmos DB Account, SQL Database, and Container

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"

# Create Cosmos DB Account
az cosmosdb create \
    --name "${BASE_NAME}-cosmo" \
    --resource-group $RESOURCE_GROUP \
    --locations RegionName=$LOCATION FailoverPriority=0 \
    --default-consistency-level Session \
    --kind GlobalDocumentDB

# Create Cosmos DB SQL Database
az cosmosdb sql database create \
    --account-name "${BASE_NAME}-cosmo" \
    --name "IoTData" \
    --resource-group $RESOURCE_GROUP

# Create Cosmos DB SQL Container
az cosmosdb sql container create \
    --account-name "${BASE_NAME}-cosmo" \
    --database-name "IoTData" \
    --name "Telemetry" \
    --partition-key-path "/deviceId" \
    --resource-group $RESOURCE_GROUP \
    --throughput 400

# Get Cosmos DB Connection String (for your application)
az cosmosdb keys list \
    --name "${BASE_NAME}-cosmo" \
    --resource-group $RESOURCE_GROUP \
    --query "connectionStrings[0].connectionString"
```

## 5. IoT Hub

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"

az iot hub create \
    --resource-group $RESOURCE_GROUP \
    --name "${BASE_NAME}-iothub" \
    --sku S1 \
    --unit 1 \
    --location $LOCATION

# Get IoT Hub Connection String (for your application)
az iot hub connection-string show \
    --hub-name "${BASE_NAME}-iothub" \
    --resource-group $RESOURCE_GROUP \
    --query "properties.primaryConnectionString"
```

## 6. Storage Account

```bash
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"
STORAGE_ACCOUNT_NAME="siot2506storage" # Matches name from main.tf

az storage account create \
    --name $STORAGE_ACCOUNT_NAME \
    --location $LOCATION \
    --resource-group $RESOURCE_GROUP \
    --sku Standard_LRS
```

## 7. Application Insights

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"

az monitor app-insights create \
    --name "${BASE_NAME}-appinsights" \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --kind web \
    --application-type web

# Get Application Insights Connection String
az monitor app-insights show \
    --name "${BASE_NAME}-appinsights" \
    --resource-group $RESOURCE_GROUP \
    --query "connectionString"
```

## 8. Azure Key Vault

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"
TENANT_ID=$(az account show --query tenantId -o tsv)

az keyvault create \
    --name "${BASE_NAME}-keysvault" \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --tenant-id $TENANT_ID \
    --sku standard
```

## 9. Azure Function App (Backend Host)

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"
FUNCTION_APP_NAME="${BASE_NAME}-backend-api"
APP_SERVICE_PLAN_NAME="${BASE_NAME}-plan"
STORAGE_ACCOUNT_NAME="siot2506storage"

az functionapp create \
    --resource-group $RESOURCE_GROUP \
    --consumption-plan-location $LOCATION \
    --runtime dotnet-isolated \
    --runtime-version 8.0 \
    --functions-version 4 \
    --name $FUNCTION_APP_NAME \
    --storage-account $STORAGE_ACCOUNT_NAME \
    --os-type Linux \
    --plan $APP_SERVICE_PLAN_NAME

# Configure Application Settings (replace with actual connection strings)
az functionapp config appsettings set \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --settings \
        "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated" \
        "AzureWebJobsStorage=@Microsoft.Azure.Storage.Blob" \
        "SqlConnectionString=<Your-SQL-Connection-String>" \
        "CosmosDbConnectionString=<Your-CosmosDB-Connection-String>" \
        "IoTHubConnection=<Your-IoT-Hub-Connection-String>" \
        "APPLICATIONINSIGHTS_CONNECTION_STRING=<Your-Application-Insights-Connection-String>" \
        "ASPNETCORE_ENVIRONMENT=Production" \
        "WEBSITE_WEBDEPLOY_USE_SCM=true"
```

## 10. Azure Static Web App (Frontend Host)

```bash
BASE_NAME="sw-iot-2506"
RESOURCE_GROUP="smartwarehouse-rg"
LOCATION="East US 2"
STATIC_WEB_APP_NAME="${BASE_NAME}-frontend-app"

az staticwebapp create \
    --name $STATIC_WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --output-location "dist" \
    --app-location "frontend" \
    --api-location "backend" \
    --source "https://github.com/Sikbicz/Smart-Warehouse-Inteligentny-Magazyn-IoT" # This will be updated by GitHub Actions
```

---
