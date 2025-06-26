import React from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

// Rejestracja niezbędnych komponentów Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

const TemperatureChart = ({ chartData }) => {
  // Sprawdzenie, czy dane są dostępne
  if (!chartData || !chartData.labels || !chartData.temperatures) {
    return <p>Brak danych do wyświetlenia wykresu.</p>;
  }

  const data = {
    labels: chartData.labels,
    datasets: [
      {
        label: 'Średnia temperatura (°C)',
        data: chartData.temperatures,
        fill: false,
        borderColor: '#ff8c00',
        backgroundColor: '#ffc107',
        tension: 0.1,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
      },
      title: {
        display: true,
        text: 'Średnia dzienna temperatura - ostatnie 7 dni',
        font: {
          size: 18,
        }
      },
    },
    scales: {
      y: {
        beginAtZero: false,
        title: {
            display: true,
            text: 'Temperatura (°C)'
        }
      }
    }
  };

  return <Line options={options} data={data} />;
};

export default TemperatureChart;