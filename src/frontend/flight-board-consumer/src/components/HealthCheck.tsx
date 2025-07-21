// HealthCheck component - Tests API connectivity
// Simple component to verify backend connection

import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { FlightApiService } from '../services/flight-api.service';

const HealthCheck: React.FC = () => {
  const { data: isHealthy, isLoading, error } = useQuery({
    queryKey: ['health'],
    queryFn: FlightApiService.healthCheck,
    retry: 1,
    refetchInterval: 30000,
  });

  if (isLoading) {
    return (
      <div className="flex items-center space-x-2 text-sm text-gray-500">
        <div className="animate-spin rounded-full h-3 w-3 border-b-2 border-gray-500"></div>
        <span>Checking API...</span>
      </div>
    );
  }

  return (
    <div className={`flex items-center space-x-2 text-sm ${
      isHealthy ? 'text-green-600' : 'text-red-600'
    }`}>
      <div className={`w-2 h-2 rounded-full ${
        isHealthy ? 'bg-green-500' : 'bg-red-500'
      }`}></div>
      <span>
        API {isHealthy ? 'Connected' : 'Disconnected'}
        {error && ` (${error})`}
      </span>
    </div>
  );
};

export default HealthCheck;
