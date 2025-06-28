# Smart Warehouse Monitoring System - Deployment Guide

This guide provides instructions for deploying the Smart Warehouse Monitoring System to Azure.

## Table of Contents
1.  [Prerequisites](#1-prerequisites)
2.  [Azure Resource Setup](#2-azure-resource-setup)
    *   [Resource Group](#21-resource-group)
    *   [Azure SQL Database](#22-azure-sql-database)
    *   [Azure Cosmos DB](#23-azure-cosmos-db)
    *   [Azure IoT Hub](#24-azure-iot-hub)
    *   [Azure Function App (for Backend)](#25-azure-function-app-for-backend)
    *   [Azure Static Web App (for Frontend)](#26-azure-static-web-app-for-frontend)
3.  [Backend Deployment (.NET Azure Functions)](#3-backend-deployment-net-azure-functions)
4.  [Frontend Deployment (Vite React App)](#4-frontend-deployment-vite-react-app)
5.  [IoT Simulator Setup](#5-iot-simulator-setup)
6.  [Testing and Verification](#6-testing-and-verification)

---

## 1. Prerequisites

Before you begin, ensure you have the following installed:

*   **Azure CLI:** For interacting with Azure resources.
    *   [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
*   **Azure Functions Core Tools:** For running and deploying Azure Functions.
    *   [Install Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash&pivots=programming-language-csharp#install-the-azure-functions-core-tools)
*   **[.NET SDK (8.0 or later)](https://dotnet.microsoft.com/download/dotnet/8.0):** For building the backend.
*   **Node.js and npm:** For building the frontend.
    *   [Install Node.js](https://nodejs.org/en/download/)
*   **Python (3.x):** For running the IoT simulator.
    *   [Install Python](https://www.python.org/downloads/)

## 2. Azure Resource Setup

Log in to your Azure account via the CLI:
```bash
az login
```
Set your default subscription (optional, but recommended):
```bash
az account set --subscription "<Your-Subscription-ID-or-Name>"
```

### 2.1. Resource Group

Create a resource group to organize your Azure resources. Replace `SmartWarehouseRG` and `eastus2` with your preferred name and region.

```bash
az group create --name "SmartWarehouseRG" --location "eastus2"
```

### 2.2. Azure SQL Database

Create an Azure SQL Server and Database. **Remember to replace `YourStrongPassword123!` with a strong, secure password.**

```bash
# Variables for SQL Database (customize these)
SQL_SERVER_NAME="sw-sqlserver-$(openssl rand -hex 4)" # Must be globally unique
SQL_ADMIN_LOGIN="sqladmin"
SQL_ADMIN_PASSWORD="YourStrongPassword123!" # IMPORTANT: Replace with a strong password!
SQL_DB_NAME="SmartWarehouseDB"
RESOURCE_GROUP="SmartWarehouseRG"
LOCATION="eastus2"

# Create SQL Server
echo "Creating Azure SQL Server: $SQL_SERVER_NAME..."
az sql server create \
    --name $SQL_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN_LOGIN \
    --admin-password $SQL_ADMIN_PASSWORD \
    --enable-public-network true # For simplicity, enable public access. Restrict in production!

# Create SQL Database
echo "Creating Azure SQL Database: $SQL_DB_NAME on $SQL_SERVER_NAME..."
az sql db create \
    --name $SQL_DB_NAME \
    --server $SQL_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --edition Basic \
    --service-objective Basic

# Configure Firewall Rule (Allow Azure services to access server)
echo "Configuring SQL Server firewall rule..."
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name "AllowAzureServices" \
    --start-ip-address "0.0.0.0" \
    --end-ip-address "0.0.0.0"

# Get SQL Connection String (for your application)
echo "Getting SQL Connection String..."
az sql db show-connection-string \
    --client ado.net \
    --name $SQL_DB_NAME \
    --server $SQL_SERVER_NAME \
    --query "connectionStrings[0].connectionString"
```
**Note down the generated SQL Connection String.** You will need it for your backend application.

### 2.3. Azure Cosmos DB

Create an Azure Cosmos DB account, SQL database, and container.

```bash
# Variables for Cosmos DB (customize these)
COSMOS_ACCOUNT_NAME="sw-cosmosdb-$(openssl rand -hex 4)" # Must be globally unique
COSMOS_DB_NAME="IoTData"
COSMOS_CONTAINER_NAME="Telemetry"
PARTITION_KEY_PATH="/deviceId"
RESOURCE_GROUP="SmartWarehouseRG"
LOCATION="eastus2"

# Create Cosmos DB Account
echo "Creating Azure Cosmos DB Account: $COSMOS_ACCOUNT_NAME..."
az cosmosdb create \
    --name $COSMOS_ACCOUNT_NAME \
    --resource-group $RESOURCE_GROUP \
    --locations RegionName=$LOCATION FailoverPriority=0 \
    --default-consistency-level Session \
    --kind GlobalDocumentDB

# Create Cosmos DB SQL Database
echo "Creating Cosmos DB SQL Database: $COSMOS_DB_NAME..."
az cosmosdb sql database create \
    --account-name $COSMOS_ACCOUNT_NAME \
    --name $COSMOS_DB_NAME \
    --resource-group $RESOURCE_GROUP

# Create Cosmos DB SQL Container
echo "Creating Cosmos DB SQL Container: $COSMOS_CONTAINER_NAME..."
az cosmosdb sql container create \
    --account-name $COSMOS_ACCOUNT_NAME \
    --database-name $COSMOS_DB_NAME \
    --name $COSMOS_CONTAINER_NAME \
    --partition-key-path $PARTITION_KEY_PATH \
    --resource-group $RESOURCE_GROUP \
    --throughput 400 # Adjust throughput as needed

# Get Cosmos DB Connection String (for your application)
echo "Getting Cosmos DB Connection String..."
az cosmosdb keys list \
    --name $COSMOS_ACCOUNT_NAME \
    --resource-group $RESOURCE_GROUP \
```
**Note down the generated Cosmos DB Connection String.** You will need it for your backend application.

### 2.4. Azure IoT Hub

Create an Azure IoT Hub. This will be your MQTT broker for IoT devices.

```bash
# Variables for IoT Hub (customize these)
IOT_HUB_NAME="sw-iothub-$(openssl rand -hex 4)" # Must be globally unique
RESOURCE_GROUP="SmartWarehouseRG"
LOCATION="eastus2"

echo "Creating Azure IoT Hub: $IOT_HUB_NAME..."
az iot hub create \
    --resource-group $RESOURCE_GROUP \
    --name $IOT_HUB_NAME \
    --sku S1 \
    --unit 1 \
    --location $LOCATION

# Get IoT Hub Connection String (for your application)
echo "Getting IoT Hub Connection String..."
az iot hub show-connection-string \
    --name $IOT_HUB_NAME \
    --resource-group $RESOURCE_GROUP \
    --query "primaryConnectionString"
```
**Note down the generated IoT Hub Connection String.** You will need it for your backend application (SensorDataFunctions) and potentially for your IoT simulator.

### 2.5. Azure Function App (for Backend)

Create an Azure Function App to host your .NET backend.

```bash
# Variables for Function App (customize these)
FUNCTION_APP_NAME="sw-backend-api-$(openssl rand -hex 4)" # Must be globally unique
STORAGE_ACCOUNT_NAME="swbackendstorage$(openssl rand -hex 4)" # Must be globally unique
RESOURCE_GROUP="SmartWarehouseRG"
LOCATION="eastus2"
APP_SERVICE_PLAN_NAME="sw-backend-plan"

# Create a Storage Account for the Function App (required)
echo "Creating Storage Account for Function App: $STORAGE_ACCOUNT_NAME..."
az storage account create \
    --name $STORAGE_ACCOUNT_NAME \
    --location $LOCATION \
    --resource-group $RESOURCE_GROUP \
    --sku Standard_LRS

# Create an App Service Plan (Linux, Consumption plan for Functions)
echo "Creating App Service Plan: $APP_SERVICE_PLAN_NAME..."
az functionapp plan create \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_PLAN_NAME \
    --location $LOCATION \
    --sku EP1 \
    --is-linux

# Create the Function App
echo "Creating Azure Function App: $FUNCTION_APP_NAME..."
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

# Configure Application Settings for the Function App
echo "Configuring application settings for Function App..."
az functionapp config appsettings set \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --settings \
        "SqlConnectionString=<Your-SQL-Connection-String>" \
        "CosmosDbConnectionString=<Your-CosmosDB-Connection-String>" \
        "IoTHubConnection=<Your-IoT-Hub-Connection-String>" \
        "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated"
```
*   **IMPORTANT:** Replace `<Your-SQL-Connection-String>`, `<Your-CosmosDB-Connection-String>`, and `<Your-IoT-Hub-Connection-String>` with the actual connection strings you noted down earlier.

### 2.6. Azure Static Web App (for Frontend)

Create an Azure Static Web App to host your React frontend.

```bash
# Variables for Static Web App (customize these)
STATIC_WEB_APP_NAME="sw-frontend-app-$(openssl rand -hex 4)" # Must be globally unique
RESOURCE_GROUP="SmartWarehouseRG"
LOCATION="eastus2"

echo "Creating Azure Static Web App: $STATIC_WEB_APP_NAME..."
az staticwebapp create \
    --name $STATIC_WEB_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --output-location "dist" \
    --app-location "frontend" \
    --api-location "backend" \
    --source "https://github.com/Sikbicz/Smart-Warehouse-Inteligentny-Magazyn-IoT" # This will be updated later for your new project
```
*   **Note:** The `--source` parameter here is a placeholder. For a real CI/CD pipeline, you would connect this to your GitHub repository. For manual deployment, you'll use the Azure Static Web Apps CLI or VS Code extension.

---

## 3. Backend Deployment (.NET Azure Functions)

1.  **Update Connection Strings in `Program.cs` (Local Development):**
    For local testing, you might want to update `local.settings.json` in `NewSmartWarehouseProject/Backend` with your connection strings. For deployment to Azure, you've already set them in the Function App's application settings.

    In `NewSmartWarehouseProject/Backend/Program.cs`, ensure your `AddDbContext` and `AddSingleton` for CosmosClient are configured to read from environment variables (which they currently are).

2.  **Build the Backend Project:**
    Navigate to the `Backend` directory and build the project.
    ```bash
    cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Backend
    dotnet build --configuration Release
    ```

3.  **Publish the Function App:**
    Use the Azure Functions Core Tools to publish your function app. Replace `<FUNCTION_APP_NAME>` with the name of your Azure Function App created in step 2.5.

    ```bash
    func azure functionapp publish <FUNCTION_APP_NAME> --csharp
    ```
    *   You might be prompted to log in to Azure.

---

## 4. Frontend Deployment (Vite React App)

1.  **Build the Frontend Project:**
    Navigate to the `Frontend` directory and build the project.
    ```bash
    cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Frontend
    npm install
    npm run build
    ```
    This will create a `dist` folder containing the production-ready static files.

2.  **Deploy to Azure Static Web App:**
    You can deploy using the Azure Static Web Apps CLI or the VS Code extension.

    **Using Azure Static Web Apps CLI:**
    ```bash
    # Install the SWA CLI if you haven't already
    npm install -g @azure/static-web-apps-cli

    # Navigate to the root of your frontend build output
    cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Frontend\dist

    # Deploy (replace <STATIC_WEB_APP_NAME> with your SWA name)
    swa deploy --app-location "C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Frontend\dist" --api-location "C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Backend" --output-location "." --env production --app-build-command "npm run build" --api-build-command "dotnet publish --configuration Release" --resource-group "SmartWarehouseRG" --name <STATIC_WEB_APP_NAME>
    ```
    *   **Note:** The `swa deploy` command is powerful and can handle building and deploying both frontend and backend. The `--app-location` should point to your *build output* directory (`dist`), and `--api-location` to your *backend project* directory.

---

## 5. IoT Simulator Setup

1.  **Install Python Dependencies:**
    If your `simulator.py` uses any external libraries (e.g., `requests`), install them:
    ```bash
    cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\IoTSimulator
    pip install -r requirements.txt # If you have a requirements.txt
    # Or install individually:
    pip install requests
    ```

2.  **Update Simulator with Backend API Endpoint:**
    Open `C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\IoTSimulator\simulator.py` and uncomment/update the `requests.post` line with the actual URL of your deployed `SensorDataController` Azure Function. You can find this URL in the Azure Portal under your Function App -> Functions -> SensorDataController -> Get Function URL.

3.  **Run the Simulator:**
    ```bash
    cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\IoTSimulator
    python simulator.py
    ```

---

## 6. Testing and Verification

1.  **Access Frontend:**
    Once the Static Web App is deployed, navigate to its URL (found in the Azure Portal). Test the user sign-up, login, and dashboard functionalities.

2.  **Monitor Azure Resources:**
    *   **Azure Function App:** Check the logs in the Azure Portal for your Function App to see if `SensorDataController` is receiving data and processing it.
    *   **Azure Cosmos DB:** Verify that sensor data is being stored in the `Telemetry` container.
    *   **Azure SQL Database:** Verify that user data and alerts are being stored.
    *   **Application Insights:** If configured, monitor application performance and logs.

3.  **Postman/API Testing:**
    Use Postman (or a similar tool) to directly test your backend API endpoints (e.g., `/api/users/register`, `/api/dashboard/sensordata`).
    *   You can get the Function App URL from the Azure Portal.

---
