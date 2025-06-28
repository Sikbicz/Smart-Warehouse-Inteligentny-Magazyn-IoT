import requests
import json

data = {
    "Username": "testuser",
    "Email": "test@example.com",
    "Password": "Password123!"
}

url = "http://localhost:7071/api/users/register"

try:
    response = requests.post(url, json=data)
    print(f"Status Code: {response.status_code}")
    print(f"Response Text: {response.text}")
except requests.exceptions.ConnectionError as e:
    print(f"Connection Error: {e}")
    print("Please ensure your Azure Functions backend is running locally.")
except Exception as e:
    print(f"An unexpected error occurred: {e}")
