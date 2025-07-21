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
import useSignalR from "./hooks/useSignalR"
import "./App.css"

function AppContent() {
  const dispatch = useAppDispatch()
  const currentView = useAppSelector((state) => state.ui.currentView)

  // Initialize SignalR connection at app level for global notifications
  useSignalR({ autoConnect: true })

  const handleViewChange = (view: "all" | "departures" | "arrivals") => {
    dispatch(setCurrentView(view))
  }

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-cyber-dark">
        {/* Cyberpunk Header */}
        <header className="holographic border-b border-neon-cyan/30 relative">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between items-center py-6">
              <div className="flex items-center space-x-6">
                <div className="flex items-center space-x-3">
                  <div className="text-3xl animate-glow-pulse">✈</div>
                  <div>
                    <h1 className="text-2xl font-cyber font-bold text-neon-cyan neon-text">
                      SKYNET BOARD
                    </h1>
                    <div className="text-xs text-neon-cyan/60 font-mono tracking-wider">
                      CONSUMER PORTAL // v2.077
                    </div>
                  </div>
                </div>
                <div className="hidden md:block h-8 w-px bg-neon-cyan/30"></div>
                <HealthCheck />
              </div>{" "}
              {/* Cyberpunk Navigation */}
              <nav className="flex space-x-2">
                <button
                  onClick={() => handleViewChange("all")}
                  className={`cyber-button ${
                    currentView === "all"
                      ? "text-neon-cyan border-neon-cyan"
                      : "text-neon-cyan/60 border-neon-cyan/30 hover:text-neon-cyan hover:border-neon-cyan"
                  }`}
                >
                  ALL_FLIGHTS
                </button>
                <button
                  onClick={() => handleViewChange("departures")}
                  className={`cyber-button ${
                    currentView === "departures"
                      ? "text-neon-orange border-neon-orange"
                      : "text-neon-orange/60 border-neon-orange/30 hover:text-neon-orange hover:border-neon-orange"
                  }`}
                >
                  DEPARTURES
                </button>
                <button
                  onClick={() => handleViewChange("arrivals")}
                  className={`cyber-button ${
                    currentView === "arrivals"
                      ? "text-neon-green border-neon-green"
                      : "text-neon-green/60 border-neon-green/30 hover:text-neon-green hover:border-neon-green"
                  }`}
                >
                  ARRIVALS
                </button>
              </nav>
            </div>
          </div>

          {/* Animated data stream */}
          <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
            <div className="absolute top-1/2 -left-4 w-8 h-px bg-neon-cyan opacity-60 animate-data-flow"></div>
            <div
              className="absolute top-1/3 -left-4 w-6 h-px bg-neon-purple opacity-40 animate-data-flow"
              style={{ animationDelay: "1s" }}
            ></div>
            <div
              className="absolute top-2/3 -left-4 w-4 h-px bg-neon-green opacity-30 animate-data-flow"
              style={{ animationDelay: "2s" }}
            ></div>
          </div>
        </header>

        {/* Main Content Area */}
        <main className="relative py-8">
          {/* Background grid overlay */}
          <div className="absolute inset-0 bg-grid-pattern bg-grid opacity-30 pointer-events-none"></div>

          <div className="relative z-10">
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
          </div>
        </main>

        {/* Cyberpunk Footer */}
        <footer className="holographic border-t border-neon-cyan/20 mt-12">
          <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between items-center">
              <div className="text-xs text-neon-cyan/40 font-mono">
                SKYNET_FLIGHT_BOARD © 2077 | NEURAL_LINK_ACTIVE
              </div>
              <div className="flex space-x-4 text-xs text-neon-cyan/40 font-mono">
                <span>UPLINK: STABLE</span>
                <span>LAT: 35.234ms</span>
                <span>SEC_LVL: [CLASSIFIED]</span>
              </div>
            </div>
          </div>
        </footer>

        {/* Ambient particles effect */}
        <div className="fixed inset-0 pointer-events-none overflow-hidden">
          {[...Array(5)].map((_, i) => (
            <div
              key={i}
              className="absolute w-1 h-1 bg-neon-cyan rounded-full opacity-60 animate-data-flow"
              style={{
                top: `${20 + i * 15}%`,
                left: "-10px",
                animationDelay: `${i * 0.8}s`,
                animationDuration: "4s",
              }}
            ></div>
          ))}
        </div>
      </div>
    </QueryClientProvider>
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
