# **Smart Warehouse – Inteligentny Magazyn IoT**  

## 📌 Opis projektu  
Smart Warehouse to system IoT do monitorowania warunków w magazynie. Urządzenie IoT zbiera dane o temperaturze i wilgotności, wysyła je do chmury AWS i udostępnia poprzez REST API.  

## 🏗 Architektura  
- **Urządzenie IoT** – symulator czujników (Python)  
- **Chmura (AWS)** – Amazon S3 (przechowywanie danych)  
- **Backend** – REST API (Flask/Node.js na AWS Lambda/EC2)  

## 🛠 Technologie  
- **IoT:** Python (symulator)  
- **Backend:** Flask / Node.js  
- **Baza danych:** Amazon DynamoDB / RDS  
- **Chmura:** AWS IoT Core, S3, API Gateway  

## 📜 User Stories  
1. **JAKO** menedżer **CHCIAŁBYM** monitorować temperaturę **PO TO, ABY** zapobiec uszkodzeniom towaru.  
2. **JAKO** administrator **CHCIAŁBYM** otrzymywać alerty **PO TO, ABY** reagować na awarie.  

## 🔧 Instalacja  
```bash
git clone https://github.com/username/smart-warehouse.git
cd smart-warehouse

## 🔧 **Instalacja i uruchomienie**  
### **1. Klonowanie repozytorium**  
```bash
git clone https://github.com/username/smart-warehouse.git
cd smart-warehouse
