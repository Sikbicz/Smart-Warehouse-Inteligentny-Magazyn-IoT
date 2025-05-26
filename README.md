Smart Warehouse – Inteligentny Magazyn IoT
📌 Opis projektu
Smart Warehouse to system IoT do monitorowania warunków w magazynie. Urządzenie IoT zbiera dane o temperaturze i wilgotności, wysyła je do chmury Microsoft Azure i udostępnia poprzez REST API oparty o .NET.

🏗 Architektura
Urządzenie IoT – symulator czujników (Python)

Chmura (Azure) – Azure IoT Hub (przesyłanie danych), Azure Storage (przechowywanie danych)

Backend – REST API (.NET, np. ASP.NET Core Web API hostowany na Azure App Service lub Azure Functions)

🛠 Technologie
IoT: Python (symulator)

Backend: .NET (ASP.NET Core Web API / Azure Functions)

Baza danych: Azure Table Storage / Azure SQL Database

Chmura: Azure IoT Hub, Azure Storage, Azure API Management

📜 User Stories
JAKO menedżer CHCIAŁBYM monitorować temperaturę PO TO, ABY zapobiec uszkodzeniom towaru.

JAKO administrator CHCIAŁBYM otrzymywać alerty PO TO, ABY reagować na awarie.

🔧 Instalacja
bash
git clone https://github.com/username/smart-warehouse.git
cd smart-warehouse
🔧 Instalacja i uruchomienie
1. Klonowanie repozytorium
bash
git clone https://github.com/username/smart-warehouse.git
cd smart-warehouse
2. Konfiguracja Azure
Utwórz zasoby: Azure IoT Hub, Azure Storage (Table lub SQL Database), Azure App Service lub Azure Functions.

Skonfiguruj połączenia i klucze w plikach konfiguracyjnych backendu.

3. Uruchomienie symulatora IoT
Przejdź do katalogu z symulatorem.

Uruchom skrypt Python, aby generować i wysyłać dane do Azure IoT Hub.

4. Uruchomienie backendu (.NET)
Przejdź do katalogu backendu.

Zbuduj i uruchom aplikację:

bash
dotnet build
dotnet run
Backend odbiera dane z Azure i udostępnia je przez REST API.

📦 Przykładowe technologie i SDK
Azure IoT Hub Device SDK for Python – do symulacji urządzenia

Azure IoT Hub Service SDK for .NET – do odbioru i zarządzania wiadomościami

Azure Storage SDK for .NET – do zapisu danych telemetrycznych

Azure Monitor – do monitorowania i alertowania
