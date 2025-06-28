import React, { useState, useEffect } from 'react';
import { Line } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';
import { useAuth } from './AuthContext';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

function Dashboard() {
  const [sensorData, setSensorData] = useState([]);
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const { logout } = useAuth();

  const fetchDashboardData = async () => {
    try {
      // Fetch Sensor Data
      const sensorResponse = await fetch('http://localhost:7071/api/dashboard/sensordata');
      if (!sensorResponse.ok) {
        throw new Error(`HTTP error! status: ${sensorResponse.status}`);
      }
      const sensorJson = await sensorResponse.json();
      setSensorData(sensorJson);

      // Fetch Alerts
      const alertResponse = await fetch('http://localhost:7071/api/dashboard/alerts');
      if (!alertResponse.ok) {
        throw new Error(`HTTP error! status: ${alertResponse.status}`);
      }
      const alertJson = await alertResponse.json();
      setAlerts(alertJson);

    } catch (error) {
      setError(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const handleDismissAlert = async (alertId) => {
    try {
      const response = await fetch(`http://localhost:7071/api/dashboard/alerts/${alertId}/dismiss`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`Failed to dismiss alert: ${response.status}`);
      }

      // Refresh alerts after dismissal
      fetchDashboardData();
    } catch (err) {
      console.error('Error dismissing alert:', err);
      setError(err.message);
    }
  };

  if (loading) {
    return <div>Loading dashboard data...</div>;
  }

  if (error) {
    return <div>Error loading dashboard: {error.message}</div>;
  }

  // Prepare data for chart
  const chartData = {
    labels: sensorData.map(data => new Date(data.Timestamp).toLocaleTimeString()),
    datasets: [
      {
        label: 'Temperature (°C)',
        data: sensorData.map(data => data.Temperature),
        borderColor: 'rgb(255, 99, 132)',
        backgroundColor: 'rgba(255, 99, 132, 0.5)',
      },
      {
        label: 'Humidity (%)',
        data: sensorData.map(data => data.Humidity),
        borderColor: 'rgb(53, 162, 235)',
        backgroundColor: 'rgba(53, 162, 235, 0.5)',
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
      title: {
        display: true,
        text: 'Sensor Data Over Time',
      },
    },
  };

  return (
    <div className="app-container">
      <nav className="navbar">
        <h1>Smart Warehouse Dashboard</h1>
        <button onClick={logout}>Logout</button>
      </nav>
      <div className="main-content">
        <div className="dashboard-grid">
          <div className="card">
            <h2>Sensor Data Chart</h2>
            <Line options={chartOptions} data={chartData} />
          </div>

          <div className="card">
            <h2>Latest Sensor Readings</h2>
            {sensorData.length > 0 ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                {sensorData.slice(-5).map((data) => (
                  <div key={data.Id} style={{
                    border: '1px solid #eee',
                    borderRadius: '5px',
                    padding: '10px',
                    backgroundColor: '#f9f9f9'
                  }}>
                    <strong>Device ID:</strong> {data.DeviceId}<br/>
                    <strong>Temperature:</strong> {data.Temperature.toFixed(2)}°C<br/>
                    <strong>Humidity:</strong> {data.Humidity.toFixed(2)}%<br/>
                    <strong>Time:</strong> {new Date(data.Timestamp).toLocaleString()}
                  </div>
                ))}
              </div>
            ) : (
              <p>No sensor data available.</p>
            )}
          </div>

          <div className="card">
            <h2>Active Alerts</h2>
            {alerts.length > 0 ? (
              <ul>
                {alerts.filter(alert => alert.IsActive).map((alert) => (
                  <li key={alert.Id} className="alert-item">
                    Message: {alert.Message}, Triggered At: {new Date(alert.TriggeredAt).toLocaleString()}
                    <button 
                      onClick={() => handleDismissAlert(alert.Id)}
                      style={{
                        marginLeft: '10px',
                        padding: '5px 10px',
                        backgroundColor: '#dc3545',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer',
                      }}
                    >Dismiss</button>
                  </li>
                ))}
              </ul>
            ) : (
              <p>No active alerts.</p>
            )}
          </div>

          <div className="card">
            <h2>Weekly Summary (Placeholder)</h2>
            <p>This section will display a summary of sensor data and alerts over the last week.</p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;