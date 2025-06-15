import random
import time
from azure.iot.device import IoTHubDeviceClient, Message

CONNECTION_STRING = "HostName=iothub-smartwarehouse.azure-devices.net;DeviceId=Symulator1;SharedAccessKey=pgjMkGMyZNDA2Qh6ecW7Bv/R7P9cJgOhLxsiVpdaGIc="
MSG_SND = '{{"temperature": {temperature}, "humidity": {humidity}}}'

temperature = 20.0
humidity = 50.0

def update_value(current, min_value, max_value, max_delta):
    delta = random.uniform(-max_delta, max_delta)
    new_value = current + delta
    return max(min_value, min(max_value, new_value))

def main():
    global temperature, humidity
    client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING)
    print("Symulator uruchomiony. Wysyłam dane do chmury")
    while True:
        temperature = update_value(temperature, 15.0, 25.0, 0.3)
        humidity = update_value(humidity, 30.0, 70.0, 0.8) 
        msg = Message(MSG_SND.format(temperature=round(temperature,2), humidity=round(humidity,2)))
        print(f"Wysyłam: {msg}")
        client.send_message(msg)
        time.sleep(5)

if __name__ == "__main__":
    main()
