import random
import time
from azure.iot.device import IoTHubDeviceClient, Message

CONNECTION_STRING = "HostName=iothub-smartwarehouse.azure-devices.net;DeviceId=Symulator1;SharedAccessKey=pgjMkGMyZNDA2Qh6ecW7Bv/R7P9cJgOhLxsiVpdaGIc="
MSG_SND = '{{"temperature": {temperature}, "humidity": {humidity}}}'

# Wartości początkowe
temperature = 20.0
humidity = 50.0

def update_value_with_violation(current, min_value, max_value, max_delta, violation_chance=0.2, violation_amount=5.0):
    if random.random() < violation_chance:
        # Generuj wartość przekraczającą próg
        violation = max_value + random.uniform(1.0, violation_amount)
        print(f"*** CELOWE PRZEKROCZENIE PROGU: {round(violation, 2)} ***")
        return violation
    else:
        delta = random.uniform(-max_delta, max_delta)
        new_value = current + delta
        return max(min_value, min(max_value, new_value))

def main():
    global temperature, humidity
    try:
        client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING)
        print("Symulator uruchomiony. Wysyłam dane do chmury...")
        
        while True:
            # Używamy progów z Twojego kodu C# (temp: 25, humidity: 60)
            temperature = update_value_with_violation(temperature, 15.0, 25.0, 0.3)
            humidity = update_value_with_violation(humidity, 30.0, 60.0, 1.5) 
            
            msg_body = MSG_SND.format(temperature=round(temperature, 2), humidity=round(humidity, 2))
            msg = Message(msg_body)
            
            print(f"Wysyłam: {msg_body}")
            client.send_message(msg)
            time.sleep(5)
            
    except ModuleNotFoundError:
        print("\nBŁĄD: Brak biblioteki 'azure.iot.device'.")
        print("Zainstaluj ją za pomocą polecenia: pip install azure-iot-device\n")
    except Exception as e:
        print(f"Wystąpił nieoczekiwany błąd: {e}")

if __name__ == "__main__":
    main()