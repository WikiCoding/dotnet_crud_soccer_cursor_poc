import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import api from '../../services/api';
import Input from '../../components/Input';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/LoadingSpinner';
import ErrorMessage from '../../components/ErrorMessage';

const PlayerForm = () => {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const teamId = searchParams.get('teamId');
  const isEditing = !!id;
  const [loading, setLoading] = useState(isEditing);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [teams, setTeams] = useState([]);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm();

  useEffect(() => {
    fetchTeams();
    if (isEditing) {
      fetchPlayer();
    } else if (teamId) {
      setValue('teamId', teamId);
    }
  }, [id, teamId]);

  const fetchTeams = async () => {
    try {
      const response = await api.get('/teams', {
        params: { page: 1, pageSize: 100 }, // pageSize max 100 in backend
      });
      setTeams(response.data?.items || []);
    } catch (err) {
      console.error('Error loading teams:', err);
      setError(`Failed to load teams: ${err.response?.data || err.message}`);
    }
  };

  const fetchPlayer = async () => {
    try {
      const response = await api.get(`/players/${id}`);
      if (response.data) {
        setValue('playerName', response.data.playerName);
        setValue('playerPosition', response.data.playerPosition);
        setValue('playerAge', response.data.playerAge);
        setValue('teamId', response.data.teamId);
      }
    } catch (err) {
      setError('Failed to load player');
    } finally {
      setLoading(false);
    }
  };

  const onSubmit = async (data) => {
    setSubmitting(true);
    setError('');

    try {
      if (isEditing) {
        await api.put(`/players/${id}`, data);
      } else {
        await api.post('/players', data);
      }
      navigate(`/players/${data.teamId}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save player');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-md">
      <div className="bg-white shadow-md rounded-lg p-6">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">
          {isEditing ? 'Edit Player' : 'Add Player'}
        </h1>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            label="Player Name"
            {...register('playerName', {
              required: 'Player name is required',
              minLength: {
                value: 1,
                message: 'Player name must be at least 1 character',
              },
              maxLength: {
                value: 200,
                message: 'Player name must be less than 200 characters',
              },
            })}
            error={errors.playerName?.message}
            placeholder="Enter player name"
          />

          <Input
            label="Position"
            {...register('playerPosition', {
              required: 'Position is required',
              minLength: {
                value: 1,
                message: 'Position must be at least 1 character',
              },
              maxLength: {
                value: 100,
                message: 'Position must be less than 100 characters',
              },
            })}
            error={errors.playerPosition?.message}
            placeholder="Enter player position"
          />

          <Input
            label="Age"
            type="number"
            {...register('playerAge', {
              required: 'Age is required',
              min: {
                value: 1,
                message: 'Age must be at least 1',
              },
              max: {
                value: 100,
                message: 'Age must be less than 100',
              },
            })}
            error={errors.playerAge?.message}
            placeholder="Enter player age"
          />

          <div className="mb-4">
            <label htmlFor="teamId" className="block text-sm font-medium text-gray-700 mb-1">
              Team <span className="text-red-500">*</span>
            </label>
            <select
              id="teamId"
              {...register('teamId', { required: 'Team is required' })}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${errors.teamId ? 'border-red-500' : 'border-gray-300'
                }`}
            >
              <option value="">Select a team</option>
              {teams.map((team) => (
                <option key={team.teamId} value={team.teamId}>
                  {team.teamName}
                </option>
              ))}
            </select>
            {errors.teamId && <p className="mt-1 text-sm text-red-600">{errors.teamId.message}</p>}
          </div>

          <ErrorMessage message={error} />

          <div className="flex space-x-4">
            <Button
              type="submit"
              disabled={submitting}
              className="flex-1"
            >
              {submitting ? (
                <>
                  <LoadingSpinner size="sm" className="mr-2" />
                  {isEditing ? 'Updating...' : 'Creating...'}
                </>
              ) : (
                isEditing ? 'Update Player' : 'Create Player'
              )}
            </Button>
            <Button
              type="button"
              onClick={() => navigate(teamId ? `/players/${teamId}` : '/teams')}
              variant="secondary"
              className="flex-1"
            >
              Cancel
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PlayerForm;