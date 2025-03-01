# **Smart Warehouse – Inteligentny Magazyn IoT**  

## 📌 **Opis projektu**  
Smart Warehouse to inteligentny system monitorowania warunków w magazynie przy użyciu urządzeń IoT i chmury obliczeniowej AWS. System umożliwia:  
✅ Monitorowanie temperatury, wilgotności i jakości powietrza w czasie rzeczywistym  
✅ Wykrywanie nieautoryzowanego dostępu do magazynu  
✅ Wysyłanie powiadomień o przekroczeniu progów alarmowych  
✅ Wizualizację danych w aplikacji webowej  

## 🏗 **Architektura systemu**  
![Diagram C4](link_do_obrazka.png)  
- **Urządzenie IoT** – Czujniki IoT (lub symulator), które zbierają dane i wysyłają je do chmury  
- **Chmura (AWS)** – Amazon S3 do przechowywania danych  
- **Backend** – REST API obsługujące logikę biznesową  
- **Frontend** (opcjonalnie) – Panel do wizualizacji danych  

## 🛠 **Technologie AWS**  
🔹 **IoT:** ESP32 / Raspberry Pi (lub symulator)  
🔹 **Backend:** Python (Flask) / Node.js (Express) hostowany na AWS Lambda lub EC2  
🔹 **Baza danych:** Amazon DynamoDB / Amazon RDS  
🔹 **Chmura:** AWS IoT Core, Amazon S3, AWS API Gateway  
🔹 **Powiadomienia:** Amazon SNS  

## 📜 **User Stories**  
1. **JAKO** menedżer magazynu **CHCIAŁBYM** mieć dostęp do danych o temperaturze i wilgotności w czasie rzeczywistym **PO TO, ABY** zapobiegać uszkodzeniom przechowywanych produktów.  
2. **JAKO** właściciel firmy **CHCIAŁBYM** otrzymywać powiadomienia o wykryciu nieautoryzowanego dostępu **PO TO, ABY** zwiększyć bezpieczeństwo.  
3. **JAKO** pracownik magazynu **CHCIAŁBYM** otrzymywać ostrzeżenia o nadmiernym poziomie pyłu **PO TO, ABY** unikać zagrożenia zdrowia.  

## 🔧 **Instalacja i uruchomienie**  
### **1. Klonowanie repozytorium**  
```bash
git clone https://github.com/username/smart-warehouse.git
cd smart-warehouse
