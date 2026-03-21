import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import TeamsList from './pages/teams/TeamsList';
import TeamForm from './pages/teams/TeamForm';
import PlayersList from './pages/players/PlayersList';
import PlayerForm from './pages/players/PlayerForm';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Layout>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            {/* Protected routes */}
            <Route
              path="/teams"
              element={
                <ProtectedRoute>
                  <TeamsList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teams/new"
              element={
                <ProtectedRoute>
                  <TeamForm />
                </ProtectedRoute>
              }
            />
            <Route
              path="/teams/:id/edit"
              element={
                <ProtectedRoute>
                  <TeamForm />
                </ProtectedRoute>
              }
            />
            <Route
              path="/players/:teamId"
              element={
                <ProtectedRoute>
                  <PlayersList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/players/new"
              element={
                <ProtectedRoute>
                  <PlayerForm />
                </ProtectedRoute>
              }
            />
            <Route
              path="/players/:id/edit"
              element={
                <ProtectedRoute>
                  <PlayerForm />
                </ProtectedRoute>
              }
            />

            {/* Default redirect */}
            <Route path="/" element={<Navigate to="/teams" />} />
            <Route path="*" element={<Navigate to="/teams" />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  );
}

export default App;
