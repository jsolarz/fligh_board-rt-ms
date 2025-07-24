import React from "react"
import { QueryClientProvider } from "@tanstack/react-query"
import { Provider } from "react-redux"
import queryClient from "./config/query-client"
import store from "./store"
import FlightBoard from "./components/FlightBoard"
import HealthCheck from "./components/HealthCheck"
import { FlightType } from "./types/flight.types"
import { useAppSelector, useAppDispatch } from "./store"
import { setCurrentView } from "./store/slices/uiSlice"
// import useSignalR from "./hooks/useSignalR"
import "./App.css"

function AppContent() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppContentWithQuery />
    </QueryClientProvider>
  )
}

function AppContentWithQuery() {
  const dispatch = useAppDispatch()
  const currentView = useAppSelector((state) => state.ui.currentView)

  // Initialize SignalR connection at app level for global notifications
  // Now this is inside QueryClientProvider context
  // useSignalR({ autoConnect: true })

  const handleViewChange = (view: "all" | "departures" | "arrivals") => {
    dispatch(setCurrentView(view))
  }

  return (
      <div className="App" style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #0f0f23 0%, #1a1a2e 50%, #16213e 100%)',
        color: '#00ff9f',
        fontFamily: "'Orbitron', 'Monaco', 'Consolas', monospace"
      }}>
        {/* Header */}
        <header style={{ 
          borderBottom: '1px solid rgba(0, 255, 159, 0.3)', 
          position: 'relative',
          padding: '2rem 1rem'
        }}>
          <div style={{ maxWidth: '1280px', margin: '0 auto', textAlign: 'center' }}>
            {/* ASCII Art Banner */}
            <pre className="ascii-art" style={{ 
              color: '#00ff9f',
              fontFamily: 'monospace',
              textShadow: '0 0 10px rgba(0, 255, 159, 0.8)',
              marginBottom: '1rem'
            }}>
{`┌─────────────────────────────────────────────────────────────────────────────────────────┐
│ ███████╗██╗     ██╗ ██████╗ ██╗  ██╗████████╗    ██████╗  ██████╗  █████╗ ██████╗ ██████╗ │
│ ██╔════╝██║     ██║██╔════╝ ██║  ██║╚══██╔══╝    ██╔══██╗██╔═══██╗██╔══██╗██╔══██╗██╔══██╗│
│ █████╗  ██║     ██║██║  ███╗███████║   ██║       ██████╔╝██║   ██║███████║██████╔╝██║  ██║│
│ ██╔══╝  ██║     ██║██║   ██║██╔══██║   ██║       ██╔══██╗██║   ██║██╔══██║██╔══██╗██║  ██║│
│ ██║     ███████╗██║╚██████╔╝██║  ██║   ██║       ██████╔╝╚██████╔╝██║  ██║██║  ██║██████╔╝│
│ ╚═╝     ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝       ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═════╝ │
└─────────────────────────────────────────────────────────────────────────────────────────┘`}
            </pre>
            
            <div style={{ 
              display: 'flex', 
              justifyContent: 'center', 
              alignItems: 'center', 
              gap: '1rem', 
              marginBottom: '2rem',
              flexWrap: 'wrap'
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <div style={{ fontSize: '1.5rem' }}>✈</div>
                <div style={{ 
                  fontSize: '0.75rem', 
                  color: 'rgba(0, 255, 159, 0.6)', 
                  fontFamily: 'monospace', 
                  letterSpacing: '0.1em' 
                }}>
                  CONSUMER PORTAL v3.077 // NEURAL_LINK_ACTIVE
                </div>
              </div>
              <HealthCheck />
            </div>

            {/* Navigation */}
            <nav style={{ display: 'flex', justifyContent: 'center', gap: '1rem', flexWrap: 'wrap' }}>
              <button
                onClick={() => handleViewChange("all")}
                style={{
                  background: currentView === "all" ? '#00ff9f' : 'transparent',
                  color: currentView === "all" ? '#000' : '#00ff9f',
                  border: '1px solid #00ff9f',
                  padding: '0.75rem 1.5rem',
                  fontFamily: 'monospace',
                  cursor: 'pointer',
                  fontSize: '0.9rem',
                  textTransform: 'uppercase',
                  transition: 'all 0.2s'
                }}
              >
                [1] ALL FLIGHTS
              </button>
              <button
                onClick={() => handleViewChange("departures")}
                style={{
                  background: currentView === "departures" ? '#00ff9f' : 'transparent',
                  color: currentView === "departures" ? '#000' : '#00ff9f',
                  border: '1px solid #00ff9f',
                  padding: '0.75rem 1.5rem',
                  fontFamily: 'monospace',
                  cursor: 'pointer',
                  fontSize: '0.9rem',
                  textTransform: 'uppercase',
                  transition: 'all 0.2s'
                }}
              >
                [2] DEPARTURES
              </button>
              <button
                onClick={() => handleViewChange("arrivals")}
                style={{
                  background: currentView === "arrivals" ? '#00ff9f' : 'transparent',
                  color: currentView === "arrivals" ? '#000' : '#00ff9f',
                  border: '1px solid #00ff9f',
                  padding: '0.75rem 1.5rem',
                  fontFamily: 'monospace',
                  cursor: 'pointer',
                  fontSize: '0.9rem',
                  textTransform: 'uppercase',
                  transition: 'all 0.2s'
                }}
              >
                [3] ARRIVALS
              </button>
            </nav>
          </div>
        </header>

        {/* Main Content */}
        <main style={{ 
          padding: '2rem 1rem', 
          position: 'relative',
          background: 'rgba(0, 255, 159, 0.02)'
        }}>
          {currentView === "all" && (
            <FlightBoard
              title="FLIGHT_MATRIX // ALL_SECTORS"
              refreshInterval={30000}
            />
          )}
          {currentView === "departures" && (
            <FlightBoard
              title="DEPARTURE_VECTOR // OUTBOUND"
              flightType={FlightType.Departure}
              refreshInterval={30000}
            />
          )}
          {currentView === "arrivals" && (
            <FlightBoard
              title="ARRIVAL_SEQUENCE // INBOUND"
              flightType={FlightType.Arrival}
              refreshInterval={30000}
            />
          )}
        </main>

        {/* Footer */}
        <footer style={{ 
          borderTop: '1px solid rgba(0, 255, 159, 0.2)', 
          padding: '1rem',
          marginTop: '2rem'
        }}>
          <div style={{ 
            maxWidth: '1280px', 
            margin: '0 auto', 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: '1rem'
          }}>
            <div style={{ 
              fontSize: '0.75rem', 
              color: 'rgba(0, 255, 159, 0.4)', 
              fontFamily: 'monospace' 
            }}>
              SKYNET_FLIGHT_BOARD © 3077 | NEURAL_LINK_ACTIVE | QUANTUM_SECURE
            </div>
            <div style={{ 
              display: 'flex', 
              gap: '1rem', 
              fontSize: '0.75rem', 
              color: 'rgba(0, 255, 159, 0.4)', 
              fontFamily: 'monospace',
              flexWrap: 'wrap'
            }}>
              <span style={{ border: '1px solid rgba(0, 255, 159, 0.3)', padding: '0.25rem 0.5rem' }}>
                UPLINK: STABLE
              </span>
              <span style={{ border: '1px solid rgba(0, 255, 159, 0.3)', padding: '0.25rem 0.5rem' }}>
                LAT: 35.234ms
              </span>
              <span style={{ border: '1px solid rgba(0, 255, 159, 0.3)', padding: '0.25rem 0.5rem' }}>
                SEC_LVL: [CLASSIFIED]
              </span>
            </div>
          </div>
        </footer>
      </div>
  )
}

function App() {
  return (
    <Provider store={store}>
      <AppContent />
    </Provider>
  )
}

export default App
