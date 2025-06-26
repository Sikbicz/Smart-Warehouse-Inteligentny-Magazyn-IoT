import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import './AuthForm.css';

//const API_BASE_URL = `${process.env.REACT_APP_API_URL}/users`;
const API_BASE_URL = `${import.meta.env.VITE_API_URL}/users`;

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const response = await fetch(`${API_BASE_URL}/login`, {
        method: 'POST', // Upewnij się, że metoda to POST
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Logowanie nie powiodło się.');
      
      login(data.Token); // TYLKO TO ZOSTAJE!

    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="auth-container">
      <form onSubmit={handleSubmit} className="auth-form">
        <h2>Logowanie</h2>
        {error && <p className="message error-message">{error}</p>}
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div className="form-group">
          <label htmlFor="password">Hasło</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <button type="submit" className="auth-button">Zaloguj się</button>
        <div className="form-links">
          <Link to="/register">Nie masz konta? Zarejestruj się</Link>
          <Link to="/request-password-reset">Zapomniałeś hasła?</Link>
        </div>
      </form>
    </div>
  );
};

export default Login;
