import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import './AuthForm.css';

const API_BASE_URL = 'http://localhost:7071/api/users';

const ResetPassword = () => {
  const { token } = useParams(); // Pobiera token z URL
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage('');
    setError('');
    

    const email = prompt("Ze względów bezpieczeństwa, proszę podać ponownie swój adres email:");
    if (!email) {
      setError("Email jest wymagany do zresetowania hasła.");
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/password-reset/complete`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, token, newPassword: password }),
      });
      if (!response.ok) throw new Error('Nie udało się zresetować hasła. Token może być nieprawidłowy lub wygasł.');
      setMessage('Hasło zostało pomyślnie zmienione! Przekierowuję do logowania...');
      setTimeout(() => navigate('/login'), 3000);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="auth-container">
      <form onSubmit={handleSubmit} className="auth-form">
        <h2>Ustaw nowe hasło</h2>
        {message && <p className="message success-message">{message}</p>}
        {error && <p className="message error-message">{error}</p>}
        <div className="form-group">
          <label htmlFor="password">Nowe hasło</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <button type="submit" className="auth-button">Zapisz nowe hasło</button>
      </form>
    </div>
  );
};

export default ResetPassword;
