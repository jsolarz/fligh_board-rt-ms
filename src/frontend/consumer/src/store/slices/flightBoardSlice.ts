// Flight board state slice for managing flight data and real-time updates
// Handles flight collections, SignalR integration, and cache management

import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import { FlightDto } from "../../types/flight.types"

export interface FlightBoardState {
  // Flight data
  flights: FlightDto[]
  selectedFlight: FlightDto | null

  // Data loading state
  isLoading: boolean
  isRefreshing: boolean
  lastUpdated: string | null

  // Error handling
  error: string | null

  // Real-time connection
  signalRConnected: boolean
  signalRConnectionState:
    | "disconnected"
    | "connecting"
    | "connected"
    | "reconnecting"
    | "error"
  signalRConnection: any // SignalR connection object

  // Cache and performance
  cacheExpiry: string | null
  lastRefreshTime: string | null

  // Flight statistics
  stats: {
    totalFlights: number
    onTimeFlights: number
    delayedFlights: number
    cancelledFlights: number
    boardingFlights: number
  }

  // View preferences
  viewMode: "table" | "grid" | "list"
  groupBy: "none" | "status" | "airline" | "destination"

  // Real-time updates tracking
  recentUpdates: {
    flightId: number
    updateType: "created" | "updated" | "deleted"
    timestamp: string
  }[]
}

const initialState: FlightBoardState = {
  flights: [],
  selectedFlight: null,
  isLoading: false,
  isRefreshing: false,
  lastUpdated: null,
  error: null,
  signalRConnected: false,
  signalRConnectionState: "disconnected",
  signalRConnection: null,
  cacheExpiry: null,
  lastRefreshTime: null,
  stats: {
    totalFlights: 0,
    onTimeFlights: 0,
    delayedFlights: 0,
    cancelledFlights: 0,
    boardingFlights: 0,
  },
  viewMode: "table",
  groupBy: "none",
  recentUpdates: [],
}

const flightBoardSlice = createSlice({
  name: "flightBoard",
  initialState,
  reducers: {
    // Flight data management
    setFlights: (state, action: PayloadAction<FlightDto[]>) => {
      state.flights = action.payload
      state.lastUpdated = new Date().toISOString()
      state.error = null

      // Update statistics
      const flights = action.payload
      state.stats = {
        totalFlights: flights.length,
        onTimeFlights: flights.filter((f) => f.status === "On Time").length,
        delayedFlights: flights.filter((f) => f.status === "Delayed").length,
        cancelledFlights: flights.filter((f) => f.status === "Cancelled")
          .length,
        boardingFlights: flights.filter((f) => f.status === "Boarding").length,
      }
    },

    addFlight: (state, action: PayloadAction<FlightDto>) => {
      const newFlight = action.payload
      const existingIndex = state.flights.findIndex(
        (f) => f.id === newFlight.id
      )

      if (existingIndex >= 0) {
        // Update existing flight
        state.flights[existingIndex] = newFlight
      } else {
        // Add new flight
        state.flights.push(newFlight)
      }

      state.lastUpdated = new Date().toISOString()

      // Track update
      state.recentUpdates.unshift({
        flightId: newFlight.id,
        updateType: existingIndex >= 0 ? "updated" : "created",
        timestamp: new Date().toISOString(),
      })

      // Keep only last 50 updates
      state.recentUpdates = state.recentUpdates.slice(0, 50)
    },

    updateFlight: (state, action: PayloadAction<FlightDto>) => {
      const updatedFlight = action.payload
      const index = state.flights.findIndex((f) => f.id === updatedFlight.id)

      if (index >= 0) {
        state.flights[index] = updatedFlight
        state.lastUpdated = new Date().toISOString()

        // Track update
        state.recentUpdates.unshift({
          flightId: updatedFlight.id,
          updateType: "updated",
          timestamp: new Date().toISOString(),
        })

        // Keep only last 50 updates
        state.recentUpdates = state.recentUpdates.slice(0, 50)
      }
    },

    removeFlight: (state, action: PayloadAction<number>) => {
      const flightId = action.payload
      state.flights = state.flights.filter((f) => f.id !== flightId)
      state.lastUpdated = new Date().toISOString()

      // Clear selected flight if it was deleted
      if (state.selectedFlight?.id === flightId) {
        state.selectedFlight = null
      }

      // Track update
      state.recentUpdates.unshift({
        flightId,
        updateType: "deleted",
        timestamp: new Date().toISOString(),
      })

      // Keep only last 50 updates
      state.recentUpdates = state.recentUpdates.slice(0, 50)
    },

    setSelectedFlight: (state, action: PayloadAction<FlightDto | null>) => {
      state.selectedFlight = action.payload
    },

    // Loading states
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },

    setRefreshing: (state, action: PayloadAction<boolean>) => {
      state.isRefreshing = action.payload
      if (action.payload) {
        state.lastRefreshTime = new Date().toISOString()
      }
    },

    // Error handling
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
    },

    clearError: (state) => {
      state.error = null
    },

    // SignalR connection management
    setSignalRConnection: (state, action: PayloadAction<any>) => {
      state.signalRConnection = action.payload
    },

    setSignalRConnected: (state, action: PayloadAction<boolean>) => {
      state.signalRConnected = action.payload
    },

    setSignalRConnectionState: (
      state,
      action: PayloadAction<FlightBoardState["signalRConnectionState"]>
    ) => {
      state.signalRConnectionState = action.payload
    },

    // Cache management
    setCacheExpiry: (state, action: PayloadAction<string>) => {
      state.cacheExpiry = action.payload
    },

    clearCache: (state) => {
      state.flights = []
      state.cacheExpiry = null
      state.lastUpdated = null
    },

    // View preferences
    setViewMode: (
      state,
      action: PayloadAction<FlightBoardState["viewMode"]>
    ) => {
      state.viewMode = action.payload
    },

    setGroupBy: (state, action: PayloadAction<FlightBoardState["groupBy"]>) => {
      state.groupBy = action.payload
    },

    // Statistics update
    updateStats: (state) => {
      const flights = state.flights
      state.stats = {
        totalFlights: flights.length,
        onTimeFlights: flights.filter((f) => f.status === "On Time").length,
        delayedFlights: flights.filter((f) => f.status === "Delayed").length,
        cancelledFlights: flights.filter((f) => f.status === "Cancelled")
          .length,
        boardingFlights: flights.filter((f) => f.status === "Boarding").length,
      }
    },

    // Clear recent updates
    clearRecentUpdates: (state) => {
      state.recentUpdates = []
    },

    // Reset flight board state
    resetFlightBoard: () => initialState,
  },
})

export const {
  setFlights,
  addFlight,
  updateFlight,
  removeFlight,
  setSelectedFlight,
  setLoading,
  setRefreshing,
  setError,
  clearError,
  setSignalRConnection,
  setSignalRConnected,
  setSignalRConnectionState,
  setCacheExpiry,
  clearCache,
  setViewMode,
  setGroupBy,
  updateStats,
  clearRecentUpdates,
  resetFlightBoard,
} = flightBoardSlice.actions

export default flightBoardSlice.reducer
