import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import api from '../../services/api';
import Table from '../../components/Table';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/LoadingSpinner';
import ErrorMessage from '../../components/ErrorMessage';

const PlayersList = () => {
  const { teamId } = useParams();
  const navigate = useNavigate();
  const [players, setPlayers] = useState([]);
  const [team, setTeam] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchPlayers = async () => {
    try {
      setLoading(true);
      const [playersResponse, teamResponse] = await Promise.all([
        api.get('/players', { params: { teamId } }),
        api.get(`/teams/${teamId}`),
      ]);
      setPlayers(playersResponse.data || []);
      setTeam(teamResponse.data);
    } catch (err) {
      setError('Failed to load players');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (teamId) {
      fetchPlayers();
    }
  }, [teamId]);

  const handleDelete = async (player) => {
    if (window.confirm(`Are you sure you want to delete ${player.playerName}?`)) {
      try {
        await api.delete(`/players/${player.playerId}`);
        fetchPlayers();
      } catch (err) {
        setError('Failed to delete player');
      }
    }
  };

  const columns = [
    { key: 'playerName', header: 'Name' },
    { key: 'playerPosition', header: 'Position' },
    { key: 'playerAge', header: 'Age' },
  ];

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (!team) {
    return (
      <div className="container mx-auto px-4 py-8">
        <ErrorMessage message="Team not found" />
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{team.teamName} Players</h1>
          <p className="text-gray-600">Manager: {team.managerName}</p>
        </div>
        <div className="space-x-4">
          <Link to={`/players/new?teamId=${teamId}`}>
            <Button>Add Player</Button>
          </Link>
          <Link to="/teams">
            <Button variant="secondary">Back to Teams</Button>
          </Link>
        </div>
      </div>

      <ErrorMessage message={error} className="mb-4" />

      {players.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No players found for this team.</p>
          <Link to={`/players/new?teamId=${teamId}`} className="text-blue-600 hover:text-blue-900">
            Add the first player
          </Link>
        </div>
      ) : (
        <div className="bg-white shadow overflow-hidden sm:rounded-md">
          <Table
            columns={columns}
            data={players}
            onView={(player) => { }}
            onEdit={(player) => navigate(`/players/${player.playerId}/edit`)}
            onDelete={handleDelete}
          />
        </div>
      )}
    </div>
  );
};

export default PlayersList;