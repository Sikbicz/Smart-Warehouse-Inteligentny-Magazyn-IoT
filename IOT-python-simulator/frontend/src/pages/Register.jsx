import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import './AuthForm.css';

const API_BASE_URL = 'http://localhost:7071/api/users';

const Register = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      const response = await fetch(`${API_BASE_URL}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Rejestracja nie powiodła się.');
      setSuccess('Konto zostało pomyślnie utworzone! Przekierowuję do logowania...');
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="auth-container">
      <form onSubmit={handleSubmit} className="auth-form">
        <h2>Rejestracja</h2>
        {error && <p className="message error-message">{error}</p>}
        {success && <p className="message success-message">{success}</p>}
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div className="form-group">
          <label htmlFor="password">Hasło</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <button type="submit" className="auth-button">Zarejestruj się</button>
        <div className="form-links">
          <Link to="/login">Masz już konto? Zaloguj się</Link>
        </div>
      </form>
    </div>
  );
};

export default Register;
