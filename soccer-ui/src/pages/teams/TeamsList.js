import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../../services/api';
import Table from '../../components/Table';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/LoadingSpinner';
import ErrorMessage from '../../components/ErrorMessage';

const TeamsList = () => {
  const navigate = useNavigate();
  const [teams, setTeams] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });
  const [sortBy, setSortBy] = useState('teamName');
  const [sortDirection, setSortDirection] = useState('asc');

  const fetchTeams = async () => {
    try {
      setLoading(true);
      const response = await api.get('/teams', {
        params: {
          page: pagination.page,
          pageSize: pagination.pageSize,
          sortBy,
          sortDirection,
        },
      });
      console.log('Teams API response:', response);
      setTeams(response.data?.items || []);
      setPagination({
        ...pagination,
        totalCount: response.data?.totalCount || 0,
        totalPages: response.data?.totalPages || 0,
      });
    } catch (err) {
      console.error('Error fetching teams:', err);
      setError('Failed to load teams');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTeams();
  }, [pagination.page, sortBy, sortDirection]);

  const handleDelete = async (team) => {
    if (window.confirm(`Are you sure you want to delete ${team.teamName}?`)) {
      try {
        await api.delete(`/teams/${team.teamId}`);
        fetchTeams();
      } catch (err) {
        setError('Failed to delete team');
      }
    }
  };

  const handlePageChange = (newPage) => {
    setPagination({ ...pagination, page: newPage });
  };

  const handleSort = (column) => {
    if (sortBy === column) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(column);
      setSortDirection('asc');
    }
  };

  const columns = [
    {
      key: 'teamName',
      header: 'Team Name',
      render: (team) => (
        <Link
          to={`/players/${team.teamId}`}
          className="text-blue-600 hover:text-blue-900"
        >
          {team.teamName}
        </Link>
      ),
    },
    { key: 'managerName', header: 'Manager' },
  ];

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Teams</h1>
        <Link to="/teams/new">
          <Button>Add Team</Button>
        </Link>
      </div>

      <ErrorMessage message={error} className="mb-4" />

      <div className="bg-white shadow overflow-hidden sm:rounded-md">
        <Table
          columns={columns}
          data={teams}
          onEdit={(team) => navigate(`/teams/${team.teamId}/edit`)}
          onDelete={handleDelete}
        />
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between mt-6">
        <div className="text-sm text-gray-700">
          Showing {teams.length > 0 ? (pagination.page - 1) * pagination.pageSize + 1 : 0} to{' '}
          {Math.min(pagination.page * pagination.pageSize, pagination.totalCount)} of{' '}
          {pagination.totalCount} results
        </div>
        <div className="flex space-x-2">
          <Button
            onClick={() => handlePageChange(pagination.page - 1)}
            disabled={pagination.page <= 1}
            variant="secondary"
          >
            Previous
          </Button>
          <Button
            onClick={() => handlePageChange(pagination.page + 1)}
            disabled={pagination.page >= pagination.totalPages}
            variant="secondary"
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  );
};

export default TeamsList;