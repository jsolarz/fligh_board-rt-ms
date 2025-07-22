// Custom React hook for SignalR real-time updates
// Cyberpunk neural interface integration

import { useEffect, useState, useCallback } from "react"
import { useQueryClient } from "@tanstack/react-query"
import { signalRService } from "../services/signalr.service"
import { FlightDto } from "../types/flight.types"

interface UseSignalROptions {
  autoConnect?: boolean
  joinGroups?: ("AllFlights" | "Departures" | "Arrivals")[]
}

interface SignalRState {
  isConnected: boolean
  connectionState: string
  error: string | null
}

export function useSignalR(options: UseSignalROptions = {}) {
  const { autoConnect = true, joinGroups = ["AllFlights"] } = options
  const queryClient = useQueryClient()

  const [state, setState] = useState<SignalRState>({
    isConnected: false,
    connectionState: "Disconnected",
    error: null,
  })

  // Update React Query cache when flights change
  const invalidateFlightQueries = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["flights"] })
  }, [queryClient])

  // Show cyberpunk-style notifications
  const showNotification = useCallback(
    (message: string, type: "info" | "success" | "warning" = "info") => {
      // This could be enhanced with a proper notification system
      console.log(`🔔 ${type.toUpperCase()}: ${message}`)
    },
    []
  )

  // Connect to SignalR
  const connect = useCallback(async () => {
    try {
      setState((prev) => ({ ...prev, error: null }))
      const connected = await signalRService.start()

      if (connected && joinGroups.length > 0) {
        // Join specified groups
        for (const group of joinGroups) {
          await signalRService.joinGroup(group)
        }
      }

      return connected
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Unknown connection error"
      setState((prev) => ({ ...prev, error: errorMessage }))
      return false
    }
  }, [joinGroups])

  // Disconnect from SignalR
  const disconnect = useCallback(async () => {
    await signalRService.stop()
  }, [])

  // Setup event handlers
  useEffect(() => {
    // Connection status handler
    signalRService.onConnectionStatusChanged((isConnected: boolean) => {
      setState((prev) => ({
        ...prev,
        isConnected,
        connectionState: signalRService.connectionState,
        error: isConnected ? null : prev.error,
      }))
    })

    // Flight created handler
    signalRService.onFlightCreated((flight: FlightDto) => {
      showNotification(
        `NEW FLIGHT: ${flight.flightNumber} (${flight.origin} → ${flight.destination})`,
        "success"
      )
      invalidateFlightQueries()
    })

    // Flight updated handler
    signalRService.onFlightUpdated((flight: FlightDto) => {
      showNotification(`FLIGHT UPDATED: ${flight.flightNumber}`, "info")
      invalidateFlightQueries()
    })

    // Flight status changed handler
    signalRService.onFlightStatusChanged(
      (flight: FlightDto, oldStatus: string, newStatus: string) => {
        const statusColor =
          newStatus === "Delayed"
            ? "warning"
            : newStatus === "Cancelled"
            ? "warning"
            : "info"
        showNotification(
          `STATUS CHANGE: ${flight.flightNumber} → ${newStatus}`,
          statusColor
        )
        invalidateFlightQueries()
      }
    )

    // Auto-connect if enabled
    if (autoConnect) {
      connect()
    }

    // Cleanup on unmount
    return () => {
      signalRService.dispose()
    }
  }, [autoConnect, connect, invalidateFlightQueries, showNotification])

  return {
    // State
    isConnected: state.isConnected,
    connectionState: state.connectionState,
    error: state.error,

    // Actions
    connect,
    disconnect,

    // SignalR service access for advanced usage
    signalRService,
  }
}

export default useSignalR
