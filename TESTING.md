# Smart Warehouse Monitoring System - Testing Guide

This guide provides instructions for testing the various components of the Smart Warehouse Monitoring System.

## Table of Contents
1.  [Prerequisites](#1-prerequisites)
2.  [Backend API Testing](#2-backend-api-testing)
    *   [Starting the Backend](#starting-the-backend)
    *   [Testing Sensor Data Endpoint](#testing-sensor-data-endpoint)
    *   [Testing Dashboard Endpoints](#testing-dashboard-endpoints)
    *   [Testing User Registration and Login](#testing-user-registration-and-login)
3.  [IoT Simulator Testing](#3-iot-simulator-testing)
4.  [Frontend Application Testing](#4-frontend-application-testing)
    *   [Starting the Frontend](#starting-the-frontend)
    *   [Verifying Data Display](#verifying-data-display)
    *   [Testing User Authentication Flow](#testing-user-authentication-flow)

---

## 1. Prerequisites

Before you begin testing, ensure you have the following:

*   **Backend Running:** Your Azure Functions backend application should be running locally (see instructions below).
*   **Frontend Running:** Your React frontend application should be running locally (see instructions below).
*   **Postman (Optional but Recommended):** For easy API testing.
    *   [Download Postman](https://www.postman.com/downloads/)
*   **Python:** For running the IoT simulator.

## 2. Backend API Testing

### Starting the Backend

Navigate to the `Backend` directory of your project in your terminal and start the Azure Functions host:

```bash
cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Backend
func start --cors "*" # Use --cors "*" for local development to avoid CORS issues
```

Keep this terminal window open. The backend will be listening on `http://localhost:7071`.

### Testing Sensor Data Endpoint

This endpoint receives sensor data from IoT devices/simulators.

**Method:** `POST`
**URL:** `http://localhost:7071/api/SensorDataController`
**Content-Type:** `application/json`

**Using Python Script:**

Navigate to your project root and run the `send_sensor_data.py` script:

```bash
cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject
python send_sensor_data.py
```

**Expected Output:** `Status Code: 200` and `Response Text: Sensor data processed successfully.`

**Using Postman:**

1.  Import the `SmartWarehouse.postman_collection.json` file into Postman.
2.  Open the "Send Sensor Data" request.
3.  Send the request.

**Expected Output:** Status `200 OK`.

### Testing Dashboard Endpoints

These endpoints retrieve sensor data and alerts.

**2.2.1. Get All Sensor Data**

**Method:** `GET`
**URL:** `http://localhost:7071/api/dashboard/sensordata`

**Using cURL (in a new terminal):**

```bash
curl http://localhost:7071/api/dashboard/sensordata
```

**Expected Output:** A JSON array of sensor data, including the data sent in the previous step.

**Using Postman:**

1.  Open the "Get All Sensor Data" request.
2.  Send the request.

**Expected Output:** Status `200 OK` and a JSON array of sensor data.

**2.2.2. Get All Alerts**

**Method:** `GET`
**URL:** `http://localhost:7071/api/dashboard/alerts`

**Using cURL (in a new terminal):**

```bash
curl http://localhost:7071/api/dashboard/alerts
```

**Expected Output:** A JSON array of alerts. If you sent sensor data with temperature above 28.0°C, you should see an alert here.

**Using Postman:**

1.  Open the "Get All Alerts" request.
2.  Send the request.

**Expected Output:** Status `200 OK` and a JSON array of alerts.

### Testing User Registration and Login

**2.3.1. Register User**

**Method:** `POST`
**URL:** `http://localhost:7071/api/users/register`
**Content-Type:** `application/json`

**Using Python Script:**

Navigate to your project root and run the `register_user.py` script:

```bash
cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject
python register_user.py
```

**Expected Output:** `Status Code: 201` and `Response Text: {"Message":"Registration successful", ...}`

**Using Postman:**

1.  Open the "Register User" request.
2.  Modify the `username`, `email`, and `password` in the request body if you want to register a new user.
3.  Send the request.

**Expected Output:** Status `201 Created`.

**2.3.2. Login User**

**Method:** `POST`
**URL:** `http://localhost:7071/api/users/login`
**Content-Type:** `application/json`

**Using Python Script:**

Navigate to your project root and run the `login_user.py` script:

```bash
cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject
python login_user.py
```

**Expected Output:** `Status Code: 200` and `Response Text: {"Message":"Login successful", "Token":"..."}` (you should see a JWT token).

**Using Postman:**

1.  Open the "Login User" request.
2.  Modify the `email` and `password` in the request body to match a registered user.
3.  Send the request.

**Expected Output:** Status `200 OK` and a JSON object containing a `Token`.

## 3. IoT Simulator Testing

This simulates IoT devices sending data to your backend.

**Prerequisites:** Ensure your backend is running (see section 2.1).

1.  **Navigate to the `IoTSimulator` directory:**
    ```bash
    cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\IoTSimulator
    ```
2.  **Run the simulator:**
    ```bash
    python simulator.py
    ```

**Expected Behavior:**

*   The simulator will print messages to its console indicating data being sent.
*   Your backend terminal (where `func start` is running) should show logs of `SensorDataController` receiving and processing data, saving to SQL and Cosmos DB, and potentially triggering alerts.
*   You can verify data in your Azure SQL Database and Azure Cosmos DB accounts.

## 4. Frontend Application Testing

### Starting the Frontend

Navigate to the `Frontend` directory of your project in your terminal and start the development server:

```bash
cd C:\Users\Sikibicz\Desktop\NewSmartWarehouseProject\Frontend
npm install # Only if you haven't run it recently
npm run dev
```

Keep this terminal window open. The frontend will typically be listening on `http://localhost:5173`.

### Verifying Data Display

1.  Open your web browser and navigate to `http://localhost:5173/`.
2.  You should be redirected to the login page.
3.  After logging in (or registering and then logging in), you should see the dashboard.
4.  Verify that the sensor data chart is populated and the latest sensor readings are displayed.
5.  Check if any active alerts are shown.

### Testing User Authentication Flow

1.  **Registration:**
    *   Go to `http://localhost:5173/register`.
    *   Fill in the registration form and submit.
    *   Verify that you see a success message and are eventually redirected to the login page.

2.  **Login:**
    *   Go to `http://localhost:5173/login`.
    *   Enter the credentials of a registered user.
    *   Verify that upon successful login, you are redirected to the dashboard (`/`).

3.  **Protected Route:**
    *   While logged in, try navigating directly to `http://localhost:5173/`.
    *   You should remain on the dashboard.
    *   Log out (if a logout button is implemented, otherwise clear local storage).
    *   Try navigating directly to `http://localhost:5173/` again. You should be redirected back to the login page.

---
