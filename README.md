# **Smart Warehouse – Inteligentny Magazyn IoT**

## 📌 Opis projektu
Smart Warehouse to system IoT do monitorowania warunków w magazynie. Urządzenie IoT zbiera dane o temperaturze i wilgotności, wysyła je do chmury Microsoft Azure i udostępnia poprzez REST API oparty o .NET.

## 🏗 Architektura
- **Urządzenie IoT** – symulator czujników (Python)
- **Chmura (Azure)** – Azure IoT Hub (przesyłanie danych), Azure Storage (przechowywanie danych)
- **Backend** – REST API (.NET, np. ASP.NET Core Web API hostowany na Azure App Service lub Azure Functions)

## 🛠 Technologie
- **IoT:** Python (symulator)
- **Backend:** .NET (ASP.NET Core Web API / Azure Functions)
- **Baza danych:** Azure Table Storage / Azure SQL Database
- **Chmura:** Azure IoT Hub, Azure Storage, Azure API Management

## 📜 User Stories
1. **JAKO** menedżer **CHCIAŁBYM** monitorować temperaturę **PO TO, ABY** zapobiec uszkodzeniom towaru.
2. **JAKO** administrator **CHCIAŁBYM** otrzymywać alerty **PO TO, ABY** reagować na awarie.

## 🔧 Instalacja


Azure Monitor – do monitorowania i alertowania
