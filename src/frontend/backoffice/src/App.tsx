// Main App component for Flight Board Backoffice Admin Portal
// BBS Terminal styling with flight management interface

import React from "react"
import { QueryClientProvider } from "@tanstack/react-query"
import { ReactQueryDevtools } from "@tanstack/react-query-devtools"
import { Provider } from "react-redux"
import { queryClient } from "./config/query-client"
import { store } from "./store"
import { FlightList, FlightForm, HealthCheck } from "./components"
import {
  FlightDto,
  CreateFlightDto,
  UpdateFlightDto,
} from "./types/flight.types"
import { useAppSelector, useAppDispatch } from "./store"
import { setCurrentMode } from "./store/slices/uiSlice"
import { setSelectedFlight } from "./store/slices/flightManagementSlice"
import "./App.css"

// Admin Content Component with Redux integration
const AdminContent: React.FC = () => {
  const dispatch = useAppDispatch()
  const mode = useAppSelector((state: any) => state.ui.currentMode)
  const editingFlight = useAppSelector(
    (state: any) => state.flightManagement.selectedFlight
  )

  // Handle flight creation
  const handleCreateFlight = () => {
    dispatch(setSelectedFlight(null))
    dispatch(setCurrentMode("create"))
  }

  // Handle flight editing
  const handleEditFlight = (flight: FlightDto) => {
    dispatch(setSelectedFlight(flight))
    dispatch(setCurrentMode("edit"))
  }

  // Handle form submission
  const handleFormSubmit = async (
    flightData: CreateFlightDto | UpdateFlightDto
  ) => {
    // This will be implemented with the actual API calls
    console.log("[ADMIN] Form submitted:", flightData)
    dispatch(setCurrentMode("list"))
    dispatch(setSelectedFlight(null))
  }

  // Handle form cancellation
  const handleFormCancel = () => {
    dispatch(setCurrentMode("list"))
    dispatch(setSelectedFlight(null))
  }

  // Handle flight deletion
  const handleDeleteFlight = async (flightId: number) => {
    // This will be implemented with the actual API calls
    console.log("[ADMIN] Delete flight:", flightId)
  }

  return (
    <div className="App">
      {/* Terminal Header */}
      <header className="terminal-header">
        <h1 className="header-flicker">
          <pre className="ascii-art-terminal">
{`┌─────────────────────────────────────────────────────────────────────────────────────┐
│ ███████╗██╗     ██╗ ██████╗ ██╗  ██╗████████╗    ██████╗  ██████╗  █████╗ ██████╗ ██████╗ │
│ ██╔════╝██║     ██║██╔════╝ ██║  ██║╚══██╔══╝    ██╔══██╗██╔═══██╗██╔══██╗██╔══██╗██╔══██╗│
│ █████╗  ██║     ██║██║  ███╗███████║   ██║       ██████╔╝██║   ██║███████║██████╔╝██║  ██║│
│ ██╔══╝  ██║     ██║██║   ██║██╔══██║   ██║       ██╔══██╗██║   ██║██╔══██║██╔══██╗██║  ██║│
│ ██║     ███████╗██║╚██████╔╝██║  ██║   ██║       ██████╔╝╚██████╔╝██║  ██║██║  ██║██████╔╝│
│ ╚═╝     ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝       ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═════╝ │
│                                                                                           │
│                        █████╗ ██████╗ ███╗   ███╗██╗███╗   ██╗                          │
│                       ██╔══██╗██╔══██╗████╗ ████║██║████╗  ██║                          │
│                       ███████║██║  ██║██╔████╔██║██║██╔██╗ ██║                          │
│                       ██╔══██║██║  ██║██║╚██╔╝██║██║██║╚██╗██║                          │
│                       ██║  ██║██████╔╝██║ ╚═╝ ██║██║██║ ╚████║                          │
│                       ╚═╝  ╚═╝╚═════╝ ╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝                          │
└─────────────────────────────────────────────────────────────────────────────────────┘`}
          </pre>
        </h1>
        <div className="system-info">
          <span className="terminal-prompt">
            FLIGHT_BOARD_ADMIN_TERMINAL v2.1.0
          </span>
          <br />
          <span className="terminal-prompt">ACCESS_LEVEL: ADMINISTRATOR</span>
          <br />
          <span className="terminal-prompt">
            SESSION: {new Date().toISOString().slice(0, 19)}
          </span>
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
              className={mode === "list" ? "primary" : ""}
              onClick={() => dispatch(setCurrentMode("list"))}
            >
              [1] FLIGHT_DATABASE
            </button>
            <button
              className={mode === "create" ? "primary" : ""}
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
          {mode === "list" && (
            <div>
              <h2 className="terminal-prompt">FLIGHT_DATABASE_ACTIVE</h2>
              <FlightList
                onEdit={handleEditFlight}
                onDelete={handleDeleteFlight}
              />
            </div>
          )}

          {(mode === "create" || mode === "edit") && (
            <div>
              <h2 className="terminal-prompt">
                {mode === "create"
                  ? "ADD_NEW_FLIGHT_RECORD"
                  : "MODIFY_FLIGHT_RECORD"}
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
    )
  }

// Main App component with providers in correct order
function App() {
  return (
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <AdminContent />
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </Provider>
  )
}

export default App
