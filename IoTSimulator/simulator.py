import time
import random
import requests
import json

# Placeholder for IoT Device Simulator
def simulate_sensor_data():
    while True:
        # Simulate 10 different devices (sensor_001 to sensor_010)
        device_id_num = random.randint(1, 10)
        device_id = f"sensor_{device_id_num:03d}" # Format with leading zeros
        
        temperature = round(random.uniform(15.0, 35.0), 2)
        humidity = round(random.uniform(30.0, 70.0), 2)
        timestamp = time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())

        data = {
            "DeviceId": device_id,
            "Temperature": temperature,
            "Humidity": humidity,
            "Timestamp": timestamp
        }
        print(f"Sending data: {data}")
        
        try:
            response = requests.post("http://localhost:7071/api/SensorDataController", json=data)
            print(f"Response Status Code: {response.status_code}")
            print(f"Response Text: {response.text}")
        except requests.exceptions.ConnectionError as e:
            print(f"Connection Error: {e}")
            print("Please ensure your Azure Functions backend is running locally.")
        except Exception as e:
            print(f"An unexpected error occurred: {e}")

        time.sleep(5) # Send data every 5 seconds

if __name__ == "__main__":
    simulate_sensor_data()
