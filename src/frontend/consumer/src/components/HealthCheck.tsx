// HealthCheck component - Cyberpunk API connectivity monitor
// Real-time neural link status with glitch effects

import React from "react"
import { useQuery } from "@tanstack/react-query"
import { FlightApiService } from "../services/flight-api.service"

const HealthCheck: React.FC = () => {
  const {
    data: isHealthy,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["health"],
    queryFn: FlightApiService.healthCheck,
    retry: 1,
    refetchInterval: 30000,
  })

  if (isLoading) {
    return (
      <div className="flex items-center space-x-3 text-sm font-mono">
        <div className="cyber-spinner w-3 h-3 animate-spin"></div>
        <span className="text-neon-cyan/60 tracking-wider">
          CONNECTING_TO_MAINFRAME...
        </span>
      </div>
    )
  }

  return (
    <div
      className={`flex items-center space-x-3 text-sm font-mono tracking-wider ${
        isHealthy ? "health-connected" : "health-disconnected"
      }`}
    >
      {/* Status indicator with glow effect */}
      <div className="relative">
        <div
          className={`w-3 h-3 rounded-full ${
            isHealthy
              ? "bg-neon-green shadow-neon-sm"
              : "bg-red-500 shadow-neon-sm"
          }`}
        ></div>
        {/* Pulsing ring effect */}
        <div
          className={`absolute inset-0 w-3 h-3 rounded-full animate-ping ${
            isHealthy ? "bg-neon-green/40" : "bg-red-500/40"
          }`}
        ></div>
      </div>

      {/* Status text */}
      <div className="flex flex-col">
        <span className="text-xs">
          NEURAL_LINK: {isHealthy ? "ACTIVE" : "SEVERED"}
        </span>
        {error && (
          <span className="text-xs text-red-400 font-mono">
            ERR_CODE: {error ? String(error).slice(0, 10) : "UNKNOWN"}
          </span>
        )}
      </div>

      {/* Data flow indicators */}
      <div className="flex space-x-1">
        {isHealthy &&
          [...Array(3)].map((_, i) => (
            <div
              key={i}
              className="w-px h-4 bg-neon-cyan/60"
              style={{
                animation: `dataFlow 1.5s ease-in-out infinite ${i * 0.2}s`,
              }}
            ></div>
          ))}
      </div>
    </div>
  )
}

export default HealthCheck
