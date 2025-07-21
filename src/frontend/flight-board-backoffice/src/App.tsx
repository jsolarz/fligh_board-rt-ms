// Main App component for Flight Board Backoffice Admin Portal
// BBS Terminal styling with flight management interface

import React, { useState } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { queryClient } from './config/query-client'
import { FlightList, FlightForm, HealthCheck } from './components'
import { FlightDto, CreateFlightDto, UpdateFlightDto } from './types/flight.types'
import './App.css'

// App modes for navigation
type AppMode = 'list' | 'create' | 'edit'

function App() {
  const [mode, setMode] = useState<AppMode>('list')
  const [editingFlight, setEditingFlight] = useState<FlightDto | undefined>()

  // Handle flight creation
  const handleCreateFlight = () => {
    setEditingFlight(undefined)
    setMode('create')
  }

  // Handle flight editing
  const handleEditFlight = (flight: FlightDto) => {
    setEditingFlight(flight)
    setMode('edit')
  }

  // Handle form submission
  const handleFormSubmit = async (flightData: CreateFlightDto | UpdateFlightDto) => {
    // This will be implemented with the actual API calls
    console.log('[ADMIN] Form submitted:', flightData)
    setMode('list')
    setEditingFlight(undefined)
  }

  // Handle form cancellation
  const handleFormCancel = () => {
    setMode('list')
    setEditingFlight(undefined)
  }

  // Handle flight deletion
  const handleDeleteFlight = async (flightId: number) => {
    // This will be implemented with the actual API calls
    console.log('[ADMIN] Delete flight:', flightId)
  }

  return (
    <QueryClientProvider client={queryClient}>
      <div className="App">
        {/* Terminal Header */}
        <header className="terminal-header">
          <h1 className="flicker">
            ═══════════════════════════════════════════════════════════════
            <br />
            ███████╗██╗     ██╗ ██████╗ ██╗  ██╗████████╗    ██████╗ ██████╗ ███████╗
            ██╔════╝██║     ██║██╔════╝ ██║  ██║╚══██╔══╝   ██╔═══██╗██╔══██╗██╔════╝
            █████╗  ██║     ██║██║  ███╗███████║   ██║      ██║   ██║██████╔╝███████╗
            ██╔══╝  ██║     ██║██║   ██║██╔══██║   ██║      ██║   ██║██╔═══╝ ╚════██║
            ██║     ███████╗██║╚██████╔╝██║  ██║   ██║      ╚██████╔╝██║     ███████║
            ╚═╝     ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝       ╚═════╝ ╚═╝     ╚══════╝
            <br />
            ═══════════════════════════════════════════════════════════════
          </h1>
          <div className="system-info">
            <span className="terminal-prompt">FLIGHT_BOARD_ADMIN_TERMINAL v2.1.0</span>
            <br />
            <span className="terminal-prompt">ACCESS_LEVEL: ADMINISTRATOR</span>
            <br />
            <span className="terminal-prompt">SESSION: {new Date().toISOString().slice(0, 19)}</span>
          </div>
        </header>

        {/* System Status */}
        <section className="system-status mb-2">
          <HealthCheck />
        </section>

        {/* Navigation Menu */}
        <nav className="terminal-nav mb-3">
          <span className="terminal-prompt">MAIN_MENU:</span>
          <div className="nav-buttons mt-1">
            <button 
              className={mode === 'list' ? 'primary' : ''}
              onClick={() => setMode('list')}
            >
              [1] FLIGHT_DATABASE
            </button>
            <button 
              className={mode === 'create' ? 'primary' : ''}
              onClick={handleCreateFlight}
            >
              [2] ADD_NEW_FLIGHT
            </button>
            <button onClick={() => window.location.reload()}>
              [3] REFRESH_SYSTEM
            </button>
          </div>
        </nav>

        {/* Main Content Area */}
        <main className="terminal-content">
          {mode === 'list' && (
            <div>
              <h2 className="terminal-prompt">FLIGHT_DATABASE_ACTIVE</h2>
              <FlightList 
                onEdit={handleEditFlight}
                onDelete={handleDeleteFlight}
              />
            </div>
          )}

          {(mode === 'create' || mode === 'edit') && (
            <div>
              <h2 className="terminal-prompt">
                {mode === 'create' ? 'ADD_NEW_FLIGHT_RECORD' : 'MODIFY_FLIGHT_RECORD'}
              </h2>
              <FlightForm
                flight={editingFlight}
                onSubmit={handleFormSubmit}
                onCancel={handleFormCancel}
              />
            </div>
          )}
        </main>

        {/* Footer */}
        <footer className="terminal-footer mt-3">
          <div className="footer-info">
            <span className="terminal-prompt">
              STATUS: OPERATIONAL | UPTIME: {Math.floor(Date.now() / 1000)} | 
              MEM_USAGE: 2.4MB | CPU: 15% | DISK: 89%
            </span>
          </div>
        </footer>
      </div>

      {/* React Query DevTools (only in development) */}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  )
}

export default App
