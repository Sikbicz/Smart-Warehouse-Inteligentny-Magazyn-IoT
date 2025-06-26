import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { format, parseISO } from 'date-fns';
import { pl } from 'date-fns/locale';
import TemperatureChart from '../components/TemperatureChart';

const logoutButtonStyle = {
  backgroundColor: '#f44336', color: 'white', border: 'none',
  padding: '10px 20px', borderRadius: '4px', cursor: 'pointer',
  fontWeight: 'bold', marginLeft: 'auto'
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:7071/api/dashboard';

function Dashboard() {
  const { token, logout } = useAuth();
  
  const [activeAlerts, setActiveAlerts] = useState([]);
  const [latestData, setLatestData] = useState(null);
  const [historicalData, setHistoricalData] = useState(null);
  const [isLoading, setIsLoading] = useState({ alerts: true, latestData: true, historical: true });
  const [error, setError] = useState({ alerts: null, latestData: null, historical: null });

  // --- POPRAWIONE FUNKCJE Z LEPSZĄ OBSŁUGĄ BŁĘDÓW ---

  const createFetchFunction = (endpoint, successCallback, errorStateKey, errorMessage) => {
    return useCallback(async () => {
      setIsLoading(prev => ({ ...prev, [errorStateKey]: true }));
      setError(prev => ({ ...prev, [errorStateKey]: null }));
      try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) {
          const errorData = await response.json().catch(() => ({ message: `Błąd HTTP! Status: ${response.status}` }));
          throw new Error(errorData.message);
        }
        const data = await response.json();
        successCallback(data);
      } catch (err) {
        console.error(`Błąd podczas pobierania danych dla ${endpoint}:`, err);
        setError(prev => ({ ...prev, [errorStateKey]: `${errorMessage}: ${err.message}` }));
      } finally {
        setIsLoading(prev => ({ ...prev, [errorStateKey]: false }));
      }
    }, [token, successCallback, errorStateKey, errorMessage]);
  };

  const fetchHistoricalData = createFetchFunction('/historical', setHistoricalData, 'historical', 'Nie udało się pobrać danych do wykresu');
  const fetchLatestData = createFetchFunction('/latest', setLatestData, 'latestData', 'Nie udało się pobrać aktualnego stanu');
  const fetchActiveAlerts = createFetchFunction('/alerts/active', setActiveAlerts, 'alerts', 'Nie udało się pobrać alertów');
  
  const acknowledgeAlert = async (alertId) => {
    try {
      const response = await fetch(`${API_BASE_URL}/alerts/${alertId}/acknowledge`, { 
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (response.ok) {
        setActiveAlerts(prevAlerts => prevAlerts.filter(a => a.Id !== alertId));
      } else {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Nie udało się potwierdzić alertu. Status: ${response.status}`);
      }
    } catch (err) {
      console.error('Błąd podczas potwierdzania alertu:', err);
      alert(`Wystąpił błąd: ${err.message}`);
    }
  };

  // --- Efekty ---
  useEffect(() => {
    if (token) {
        fetchHistoricalData();
        fetchLatestData();
        fetchActiveAlerts();
    }
  }, [token]); // Usunięto funkcje z zależności, aby uniknąć pętli

  useEffect(() => {
    if (token) {
        const intervalId = setInterval(fetchActiveAlerts, 30000);
        return () => clearInterval(intervalId);
    }
  }, [token, fetchActiveAlerts]);

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Dashboard Magazynu</h1>
        <button onClick={logout} style={logoutButtonStyle}>Wyloguj</button>
      </header>
      <div className="dashboard-layout">
        <main className="main-content">
          <h2>Analiza Temperatury</h2>
          {isLoading.historical && <p>Ładowanie wykresu...</p>}
          {error.historical && <p className="error-message">{error.historical}</p>}
          {!isLoading.historical && !error.historical && historicalData && (
            <div className="chart-container">
              <TemperatureChart chartData={historicalData} />
            </div>
          )}
        </main>
        <aside className="alerts-sidebar">
          <h2>Aktywne Alerty</h2>
          {isLoading.alerts && <p>Ładowanie alertów...</p>}
          {error.alerts && <p className="error-message">{error.alerts}</p>}
          {!isLoading.alerts && !error.alerts && (
            <div className="alerts-list">
              {activeAlerts.length === 0 ? (
                <div className="no-alerts"><p>✅ Brak aktywnych alertów.</p></div>
              ) : (
                activeAlerts.map((alert) => (
                  <div key={alert.Id} className={`alert-card ${alert.SensorType === 0 ? 'temperature' : 'humidity'}`}>
                    <div className="alert-icon">{alert.SensorType === 0 ? '🌡️' : '💧'}</div>
                    <div className="alert-details">
                      <p className="alert-message">{alert.Message}</p>
                      <p className="alert-info">
                        <strong>Urządzenie:</strong> {alert.SensorId} |{' '}
                        <strong>Czas:</strong> {format(parseISO(alert.TriggeredAt), 'dd.MM.yyyy HH:mm:ss', { locale: pl })}
                      </p>
                    </div>
                    <button className="acknowledge-button" onClick={() => acknowledgeAlert(alert.Id)}>Potwierdź</button>
                  </div>
                ))
              )}
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}

export default Dashboard;
