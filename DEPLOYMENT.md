# Smart Warehouse Monitoring System - Deployment Guide

This guide provides instructions for deploying the Smart Warehouse Monitoring System to Azure using GitHub Actions for Continuous Integration/Continuous Deployment (CI/CD).

## Table of Contents
1.  [Prerequisites](#1-prerequisites)
2.  [Azure Resource Setup](#2-azure-resource-setup)
3.  [GitHub Setup for CI/CD](#3-github-setup-for-cicd)
4.  [Deployment via GitHub Actions](#4-deployment-via-github-actions)
5.  [Verification](#5-verification)

---

## 1. Prerequisites

Before you begin, ensure you have the following installed and configured:

*   **Azure CLI:** For interacting with Azure resources.
    *   [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
*   **GitHub Account:** With access to your repository (`https://github.com/Sikbicz/Smart-Warehouse-Inteligentny-Magazyn-IoT`).
*   **Azure Subscription:** With permissions to create resources.

## 2. Azure Resource Setup

First, provision the necessary Azure resources. You can find the Azure CLI commands for this in the `INFRASTRUCTURE.md` file in the root of this repository. Follow the instructions there to create your:

*   Resource Group
*   App Service Plan
*   MS SQL Server and Database
*   Cosmos DB Account, SQL Database, and Container
*   IoT Hub
*   Storage Account
*   Application Insights
*   Azure Key Vault
*   Azure Function App (Backend Host)
*   Azure Static Web App (Frontend Host)

**Important:** Note down all connection strings and names of the resources you create, as you will need them for configuring your Function App settings and GitHub Secrets.

## 3. GitHub Setup for CI/CD

To allow GitHub Actions to deploy to your Azure subscription, you need to set up a Service Principal and add its credentials as GitHub Secrets.

### 3.1. Create an Azure Service Principal

Open your terminal and run the following Azure CLI command. Replace `<YOUR_SUBSCRIPTION_ID>` and `<YOUR_RESOURCE_GROUP_NAME>` with your actual values.

```bash
az ad sp create-for-rbac --name "github-actions-sp" --role contributor --scopes /subscriptions/<YOUR_SUBSCRIPTION_ID>/resourceGroups/<YOUR_RESOURCE_GROUP_NAME> --json
```

**Copy the entire JSON output.** You will need the `appId`, `password`, and `tenant` values.

### 3.2. Add Credentials as GitHub Secrets

1.  Go to your GitHub repository: `https://github.com/Sikbicz/Smart-Warehouse-Inteligentny-Magazyn-IoT`
2.  Navigate to **Settings** > **Secrets and variables** > **Actions**.
3.  Click **"New repository secret"** and add the following secrets:
    *   **Name:** `AZURE_CLIENT_ID`
        **Value:** The `appId` from the JSON output.
    *   **Name:** `AZURE_TENANT_ID`
        **Value:** The `tenant` from the JSON output.
    *   **Name:** `AZURE_SUBSCRIPTION_ID`
        **Value:** Your Azure Subscription ID.
    *   **Name:** `AZURE_CREDENTIALS`
        **Value:** The entire JSON output from `az ad sp create-for-rbac` (including `appId`, `displayName`, `password`, `tenant`).

## 4. Deployment via GitHub Actions

Deployment is automated using GitHub Actions workflows located in the `.github/workflows/` directory.

### 4.1. Configure Workflow Files

Open the following files in your repository and **replace the placeholder values** (`<YOUR_FUNCTION_APP_NAME>`, `<YOUR_STATIC_WEB_APP_NAME>`, `<YOUR_RESOURCE_GROUP_NAME>`) with the actual names of your Azure resources:

*   `.github/workflows/backend-deploy.yml`
*   `.github/workflows/frontend-deploy.yml`

### 4.2. Trigger Deployment

Deployment workflows are configured to run automatically on `push` to the `main` branch. You can also trigger them manually:

1.  Go to your GitHub repository.
2.  Click on the **"Actions"** tab.
3.  In the left sidebar, select the workflow you want to run (e.g., "Deploy Backend to Azure Functions" or "Deploy Frontend to Azure Static Web Apps").
4.  Click **"Run workflow"** on the right side.

### 4.3. Configure Azure Function App Settings

After the backend deployment workflow runs for the first time, you **MUST** configure the application settings for your Azure Function App. These settings provide your backend with the necessary connection strings and runtime configurations.

Replace the placeholder values with your actual connection strings and ensure the `FUNCTIONS_WORKER_RUNTIME` and `Functions:Worker:HostEndpoint` are set correctly.

```bash
az functionapp config appsettings set \
    --name <YOUR_FUNCTION_APP_NAME> \
    --resource-group <YOUR_RESOURCE_GROUP_NAME> \
    --settings \
        "SqlConnectionString=<Your-Azure-SQL-Connection-String>" \
        "CosmosDbConnectionString=<Your-Azure-CosmosDB-Connection-String>" \
        "IoTHubConnection=<Your-Azure-IoT-Hub-Connection-String>" \
        "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated" \
        "Functions:Worker:HostEndpoint=http://127.0.0.1:9000" \
        "APPLICATIONINSIGHTS_CONNECTION_STRING=<Your-Application-Insights-Connection-String>"
```

## 5. Verification

Once both deployments are complete and Function App settings are configured:

1.  **Get your Static Web App URL:** You can find this in the Azure Portal under your Static Web App's "Overview" page.
2.  **Open the URL in your browser.**
3.  **Test the application:**
    *   Try registering a new user.
    *   Log in and access the dashboard.
    *   Verify sensor data and alerts are displayed.
    *   Test the "Dismiss" alert button.

---

