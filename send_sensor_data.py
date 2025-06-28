import requests
import json

data = {
    "DeviceId": "test_device_001",
    "Temperature": 29.5,
    "Humidity": 55.2,
    "Timestamp": "2025-06-27T18:00:00Z"
}

url = "http://localhost:7071/api/SensorDataController"

try:
    response = requests.post(url, json=data)
    print(f"Status Code: {response.status_code}")
    print(f"Response Text: {response.text}")
except requests.exceptions.ConnectionError as e:
    print(f"Connection Error: {e}")
    print("Please ensure your Azure Functions backend is running locally.")
except Exception as e:
    print(f"An unexpected error occurred: {e}")
