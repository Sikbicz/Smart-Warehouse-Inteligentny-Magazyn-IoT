import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import './AuthForm.css';

const API_BASE_URL = 'http://localhost:7071/api/users';

const RequestPasswordReset = () => {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage('');
    setError('');
    try {
      const response = await fetch(`${API_BASE_URL}/password-reset/initiate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email }),
      });
      if (!response.ok) throw new Error('Nie udało się wysłać linku do resetu hasła.');
      setMessage('Jeśli konto istnieje, link do resetu hasła został wysłany na Twój adres e-mail.');
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="auth-container">
      <form onSubmit={handleSubmit} className="auth-form">
        <h2>Resetowanie hasła</h2>
        {message && <p className="message success-message">{message}</p>}
        {error && <p className="message error-message">{error}</p>}
        <div className="form-group">
          <label htmlFor="email">Podaj swój adres e-mail</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <button type="submit" className="auth-button">Wyślij link</button>
         <div className="form-links">
          <Link to="/login">Wróć do logowania</Link>
        </div>
      </form>
    </div>
  );
};

export default RequestPasswordReset;
