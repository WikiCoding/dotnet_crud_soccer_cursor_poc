import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import api from '../../services/api';
import Input from '../../components/Input';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/LoadingSpinner';
import ErrorMessage from '../../components/ErrorMessage';

const TeamForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;
  const [loading, setLoading] = useState(isEditing);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm();

  useEffect(() => {
    if (isEditing) {
      fetchTeam();
    }
  }, [id]);

  const fetchTeam = async () => {
    try {
      const response = await api.get(`/teams/${id}`);
      if (response.data) {
        setValue('teamName', response.data.teamName);
        setValue('managerName', response.data.managerName);
      }
    } catch (err) {
      setError('Failed to load team');
    } finally {
      setLoading(false);
    }
  };

  const onSubmit = async (data) => {
    setSubmitting(true);
    setError('');

    try {
      if (isEditing) {
        await api.put(`/teams/${id}`, data);
      } else {
        await api.post('/teams', data);
      }
      navigate('/teams');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save team');
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
          {isEditing ? 'Edit Team' : 'Add Team'}
        </h1>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            label="Team Name"
            {...register('teamName', {
              required: 'Team name is required',
              minLength: {
                value: 1,
                message: 'Team name must be at least 1 character',
              },
              maxLength: {
                value: 200,
                message: 'Team name must be less than 200 characters',
              },
            })}
            error={errors.teamName?.message}
            placeholder="Enter team name"
          />

          <Input
            label="Manager Name"
            {...register('managerName', {
              required: 'Manager name is required',
              minLength: {
                value: 1,
                message: 'Manager name must be at least 1 character',
              },
              maxLength: {
                value: 200,
                message: 'Manager name must be less than 200 characters',
              },
            })}
            error={errors.managerName?.message}
            placeholder="Enter manager name"
          />

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
                isEditing ? 'Update Team' : 'Create Team'
              )}
            </Button>
            <Button
              type="button"
              onClick={() => navigate('/teams')}
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

export default TeamForm;