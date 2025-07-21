import React, { useState } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import queryClient from './config/query-client';
import FlightBoard from './components/FlightBoard';
import HealthCheck from './components/HealthCheck';
import { FlightType } from './types/flight.types';
import './App.css';

function App() {
  const [currentView, setCurrentView] = useState<'all' | 'departures' | 'arrivals'>('all');

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-gray-50">
        {/* Navigation Header */}
        <header className="bg-white shadow-sm border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between items-center py-4">
              <div className="flex items-center">
                <h1 className="text-2xl font-bold text-gray-900">
                  ✈️ Flight Board System
                </h1>
                <span className="ml-3 text-sm text-gray-500">Consumer Portal</span>
                <div className="ml-4">
                  <HealthCheck />
                </div>
              </div>
              
              {/* View Toggle */}
              <nav className="flex space-x-4">
                <button
                  onClick={() => setCurrentView('all')}
                  className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                    currentView === 'all'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  All Flights
                </button>
                <button
                  onClick={() => setCurrentView('departures')}
                  className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                    currentView === 'departures'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  Departures
                </button>
                <button
                  onClick={() => setCurrentView('arrivals')}
                  className={`px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                    currentView === 'arrivals'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  Arrivals
                </button>
              </nav>
            </div>
          </div>
        </header>

        {/* Main Content */}
        <main className="py-6">
          {currentView === 'all' && (
            <FlightBoard 
              title="All Flights" 
              refreshInterval={30000}
            />
          )}
          {currentView === 'departures' && (
            <FlightBoard 
              title="Departures" 
              flightType={FlightType.Departure}
              refreshInterval={30000}
            />
          )}
          {currentView === 'arrivals' && (
            <FlightBoard 
              title="Arrivals" 
              flightType={FlightType.Arrival}
              refreshInterval={30000}
            />
          )}
        </main>

        {/* Footer */}
        <footer className="bg-white border-t border-gray-200 mt-12">
          <div className="max-w-7xl mx-auto py-4 px-4 sm:px-6 lg:px-8">
            <p className="text-center text-sm text-gray-500">
              Flight Board System - Consumer Portal | Real-time flight information
            </p>
          </div>
        </footer>
      </div>
    </QueryClientProvider>
  );
}

export default App;
