// FlightBoard component - Main flight display component
// Uses React Query for data fetching and state management

import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { FlightApiService, handleApiError } from '../services/flight-api.service';
import { FlightDto, FlightSearchDto, FlightType } from '../types/flight.types';
import LoadingSpinner from './LoadingSpinner';
import ErrorAlert from './ErrorAlert';

interface FlightBoardProps {
  title?: string;
  flightType?: FlightType;
  refreshInterval?: number; // milliseconds
}

const FlightBoard: React.FC<FlightBoardProps> = ({ 
  title = 'Flight Board',
  flightType,
  refreshInterval = 30000 // 30 seconds
}) => {
  // Search state
  const [searchParams, setSearchParams] = useState<FlightSearchDto>({
    page: 1,
    pageSize: 20,
    type: flightType
  });

  // React Query for data fetching
  const {
    data: flightsData,
    isLoading,
    error,
    isError,
    refetch
  } = useQuery({
    queryKey: ['flights', searchParams],
    queryFn: () => {
      // Choose API endpoint based on flight type
      if (flightType === FlightType.Departure) {
        return FlightApiService.getDepartures(searchParams);
      } else if (flightType === FlightType.Arrival) {
        return FlightApiService.getArrivals(searchParams);
      } else {
        return FlightApiService.getFlights(searchParams);
      }
    },
    refetchInterval: refreshInterval,
    staleTime: 10000, // Data considered fresh for 10 seconds
    retry: 3,
  });

  // Format time for display
  const formatTime = (dateString: string): string => {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    });
  };

  // Format date for display
  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric'
    });
  };

  // Get status badge styling
  const getStatusBadge = (status: string, isDelayed: boolean) => {
    const baseClasses = 'px-2 py-1 rounded-full text-xs font-medium';
    
    if (isDelayed) {
      return `${baseClasses} bg-red-100 text-red-800`;
    }
    
    switch (status) {
      case 'Scheduled':
        return `${baseClasses} bg-blue-100 text-blue-800`;
      case 'Boarding':
        return `${baseClasses} bg-yellow-100 text-yellow-800`;
      case 'Departed':
        return `${baseClasses} bg-green-100 text-green-800`;
      case 'Arrived':
        return `${baseClasses} bg-green-100 text-green-800`;
      case 'Cancelled':
        return `${baseClasses} bg-red-100 text-red-800`;
      default:
        return `${baseClasses} bg-gray-100 text-gray-800`;
    }
  };

  // Handle pagination
  const handlePageChange = (newPage: number) => {
    setSearchParams(prev => ({ ...prev, page: newPage }));
  };

  // Handle search
  const handleSearch = (newSearchParams: Partial<FlightSearchDto>) => {
    setSearchParams(prev => ({ ...prev, ...newSearchParams, page: 1 }));
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading flights..." />;
  }

  if (isError) {
    return (
      <ErrorAlert 
        message={handleApiError(error)}
        onRetry={() => refetch()}
      />
    );
  }

  const flights = flightsData?.data || [];
  const pagination = flightsData;

  return (
    <div className="w-full max-w-7xl mx-auto p-4">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">{title}</h1>
        <div className="flex justify-between items-center">
          <p className="text-gray-600">
            Showing {flights.length} of {pagination?.totalCount || 0} flights
          </p>
          <div className="flex space-x-2">
            <button
              onClick={() => refetch()}
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Flight Table */}
      <div className="bg-white shadow-lg rounded-lg overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Flight
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Route
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Scheduled
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Estimated
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Gate
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Aircraft
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {flights.map((flight: FlightDto) => (
                <tr key={flight.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">
                        {flight.flightNumber}
                      </div>
                      <div className="text-sm text-gray-500">
                        {flight.airline}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {flight.origin} → {flight.destination}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {formatTime(flightType === FlightType.Arrival ? flight.scheduledArrival : flight.scheduledDeparture)}
                    </div>
                    <div className="text-sm text-gray-500">
                      {formatDate(flightType === FlightType.Arrival ? flight.scheduledArrival : flight.scheduledDeparture)}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {formatTime(flightType === FlightType.Arrival ? flight.estimatedArrival : flight.estimatedDeparture)}
                    </div>
                    {flight.isDelayed && (
                      <div className="text-sm text-red-600">
                        +{flight.delayMinutes}min
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={getStatusBadge(flight.status, flight.isDelayed)}>
                      {flight.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {flight.gate || '-'}
                    {flight.terminal && (
                      <div className="text-xs text-gray-500">
                        Terminal {flight.terminal}
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {flight.aircraftType || '-'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Empty state */}
        {flights.length === 0 && (
          <div className="text-center py-12">
            <div className="text-gray-500">
              <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
              </svg>
              <h3 className="mt-2 text-sm font-medium text-gray-900">No flights</h3>
              <p className="mt-1 text-sm text-gray-500">No flights match your criteria.</p>
            </div>
          </div>
        )}

        {/* Pagination */}
        {pagination && pagination.totalPages > 1 && (
          <div className="bg-white px-4 py-3 flex items-center justify-between border-t border-gray-200 sm:px-6">
            <div className="flex-1 flex justify-between sm:hidden">
              <button
                onClick={() => handlePageChange(pagination.page - 1)}
                disabled={!pagination.hasPrevious}
                className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
              >
                Previous
              </button>
              <button
                onClick={() => handlePageChange(pagination.page + 1)}
                disabled={!pagination.hasNext}
                className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
              >
                Next
              </button>
            </div>
            <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
              <div>
                <p className="text-sm text-gray-700">
                  Showing <span className="font-medium">{((pagination.page - 1) * pagination.pageSize) + 1}</span> to{' '}
                  <span className="font-medium">
                    {Math.min(pagination.page * pagination.pageSize, pagination.totalCount)}
                  </span>{' '}
                  of <span className="font-medium">{pagination.totalCount}</span> results
                </p>
              </div>
              <div>
                <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                  <button
                    onClick={() => handlePageChange(pagination.page - 1)}
                    disabled={!pagination.hasPrevious}
                    className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    Previous
                  </button>
                  
                  {/* Page numbers */}
                  {Array.from({ length: Math.min(5, pagination.totalPages) }, (_, i) => {
                    const pageNum = Math.max(1, pagination.page - 2) + i;
                    if (pageNum > pagination.totalPages) return null;
                    
                    return (
                      <button
                        key={pageNum}
                        onClick={() => handlePageChange(pageNum)}
                        className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium ${
                          pageNum === pagination.page
                            ? 'z-10 bg-blue-50 border-blue-500 text-blue-600'
                            : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50'
                        }`}
                      >
                        {pageNum}
                      </button>
                    );
                  })}
                  
                  <button
                    onClick={() => handlePageChange(pagination.page + 1)}
                    disabled={!pagination.hasNext}
                    className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                  >
                    Next
                  </button>
                </nav>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default FlightBoard;
