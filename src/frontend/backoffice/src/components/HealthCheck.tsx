// filepath: d:\personal\fligh_board-rt-ms\src\frontend\flight-board-backoffice\src\components\HealthCheck.tsx
// Health check component for BBS terminal interface
// Monitors API connectivity and system status

import React from "react"
import { useQuery } from "@tanstack/react-query"
import FlightApiService from "../services/flight-api.service"
import { QUERY_KEYS } from "../config/query-client"

const HealthCheck: React.FC = () => {
  const { data, isLoading, error, isSuccess } = useQuery({
    queryKey: QUERY_KEYS.health,
    queryFn: FlightApiService.healthCheck,
    refetchInterval: 30000, // Check every 30 seconds
    retry: 1, // Only retry once for health checks
  })

  const getStatusDisplay = () => {
    if (isLoading) {
      return <span className="status loading">SYSTEM_CHECK_IN_PROGRESS</span>
    }

    if (error) {
      return (
        <span className="status error">
          CONNECTION_FAILED -{" "}
          {error instanceof Error ? error.message : "UNKNOWN_ERROR"}
        </span>
      )
    }

    if (isSuccess && data) {
      return (
        <span className="status on-time">
          API_CONNECTION_ACTIVE - LAST_PING:{" "}
          {new Date(data.timestamp).toLocaleTimeString()}
        </span>
      )
    }

    return <span className="status">SYSTEM_STATUS_UNKNOWN</span>
  }

  return (
    <div className="health-check">
      <span className="terminal-prompt">SYSTEM_STATUS:</span>{" "}
      {getStatusDisplay()}
      {error && (
        <div className="error mt-1">
          <span className="terminal-prompt">ERROR_DETAILS:</span>
          <br />
          {error instanceof Error
            ? error.message
            : "Unable to connect to flight board API"}
          <br />
          <span className="terminal-prompt">VERIFY:</span> Backend service
          running on localhost:5183
        </div>
      )}
    </div>
  )
}

export default HealthCheck
