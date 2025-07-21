// Flight management slice for admin operations
// Handles CRUD operations, form state, and flight data management

import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import {
  FlightDto,
  CreateFlightDto,
  UpdateFlightDto,
  FlightFormErrors,
} from "../../types/flight.types"

export interface FlightManagementState {
  // Flight data
  flights: FlightDto[]
  totalFlights: number
  currentPage: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean

  // Selected flight for editing/viewing
  selectedFlight: FlightDto | null

  // Form state
  formData: Partial<CreateFlightDto>
  formErrors: FlightFormErrors
  isFormDirty: boolean
  formMode: "create" | "edit" | "view"

  // Loading states
  isLoading: boolean
  isSaving: boolean
  isDeleting: boolean

  // Error handling
  error: string | null
  validationErrors: string[]

  // Operations tracking
  lastOperation: {
    type: "create" | "update" | "delete" | null
    flightId?: number
    timestamp?: string
    success?: boolean
  }

  // Real-time connection for admin updates
  signalRConnected: boolean
  signalRConnection: any

  // Bulk operations
  selectedFlightIds: number[]
  bulkOperation: "none" | "delete" | "update-status" | "export"

  // Search and filtering for admin
  searchTerm: string
  filterBy: {
    status?: string
    airline?: string
    dateRange?: {
      from: string
      to: string
    }
  }

  // Import/Export
  importProgress: {
    isImporting: boolean
    progress: number
    errors: string[]
    imported: number
    total: number
  }

  exportProgress: {
    isExporting: boolean
    progress: number
    format: "csv" | "json" | "xlsx"
  }

  // Statistics
  stats: {
    totalFlights: number
    flightsToday: number
    onTimePercentage: number
    averageDelay: number
    mostActiveAirline: string
    mostPopularDestination: string
  }
}

const initialState: FlightManagementState = {
  flights: [],
  totalFlights: 0,
  currentPage: 1,
  totalPages: 1,
  hasNext: false,
  hasPrevious: false,
  selectedFlight: null,
  formData: {},
  formErrors: {},
  isFormDirty: false,
  formMode: "create",
  isLoading: false,
  isSaving: false,
  isDeleting: false,
  error: null,
  validationErrors: [],
  lastOperation: { type: null },
  signalRConnected: false,
  signalRConnection: null,
  selectedFlightIds: [],
  bulkOperation: "none",
  searchTerm: "",
  filterBy: {},
  importProgress: {
    isImporting: false,
    progress: 0,
    errors: [],
    imported: 0,
    total: 0,
  },
  exportProgress: {
    isExporting: false,
    progress: 0,
    format: "csv",
  },
  stats: {
    totalFlights: 0,
    flightsToday: 0,
    onTimePercentage: 0,
    averageDelay: 0,
    mostActiveAirline: "",
    mostPopularDestination: "",
  },
}

const flightManagementSlice = createSlice({
  name: "flightManagement",
  initialState,
  reducers: {
    // Flight data management
    setFlights: (
      state,
      action: PayloadAction<{
        flights: FlightDto[]
        totalFlights: number
        currentPage: number
        totalPages: number
        hasNext: boolean
        hasPrevious: boolean
      }>
    ) => {
      state.flights = action.payload.flights
      state.totalFlights = action.payload.totalFlights
      state.currentPage = action.payload.currentPage
      state.totalPages = action.payload.totalPages
      state.hasNext = action.payload.hasNext
      state.hasPrevious = action.payload.hasPrevious
      state.error = null
    },

    addFlight: (state, action: PayloadAction<FlightDto>) => {
      state.flights.unshift(action.payload)
      state.totalFlights += 1
      state.lastOperation = {
        type: "create",
        flightId: action.payload.id,
        timestamp: new Date().toISOString(),
        success: true,
      }
    },

    updateFlight: (state, action: PayloadAction<FlightDto>) => {
      const index = state.flights.findIndex((f) => f.id === action.payload.id)
      if (index >= 0) {
        state.flights[index] = action.payload
        if (state.selectedFlight?.id === action.payload.id) {
          state.selectedFlight = action.payload
        }
        state.lastOperation = {
          type: "update",
          flightId: action.payload.id,
          timestamp: new Date().toISOString(),
          success: true,
        }
      }
    },

    removeFlight: (state, action: PayloadAction<number>) => {
      const flightId = action.payload
      state.flights = state.flights.filter((f) => f.id !== flightId)
      state.totalFlights -= 1
      if (state.selectedFlight?.id === flightId) {
        state.selectedFlight = null
      }
      state.selectedFlightIds = state.selectedFlightIds.filter(
        (id) => id !== flightId
      )
      state.lastOperation = {
        type: "delete",
        flightId,
        timestamp: new Date().toISOString(),
        success: true,
      }
    },

    setSelectedFlight: (state, action: PayloadAction<FlightDto | null>) => {
      state.selectedFlight = action.payload
      if (action.payload) {
        // Initialize form data when selecting flight for editing
        state.formData = {
          flightNumber: action.payload.flightNumber,
          airline: action.payload.airline,
          origin: action.payload.origin,
          destination: action.payload.destination,
          scheduledDeparture: action.payload.scheduledDeparture,
          scheduledArrival: action.payload.scheduledArrival,
          status: action.payload.status,
          gate: action.payload.gate,
          terminal: action.payload.terminal,
          aircraftType: action.payload.aircraftType,
          remarks: action.payload.remarks,
          delayMinutes: action.payload.delayMinutes,
          type: action.payload.type,
        }
        state.formMode = "edit"
      }
    },

    // Form management
    setFormData: (state, action: PayloadAction<Partial<CreateFlightDto>>) => {
      state.formData = { ...state.formData, ...action.payload }
      state.isFormDirty = true
    },

    setFormField: (
      state,
      action: PayloadAction<{ field: string; value: any }>
    ) => {
      ;(state.formData as any)[action.payload.field] = action.payload.value
      state.isFormDirty = true
      // Clear field error when user starts typing
      if (state.formErrors[action.payload.field as keyof FlightFormErrors]) {
        delete state.formErrors[action.payload.field as keyof FlightFormErrors]
      }
    },

    setFormErrors: (state, action: PayloadAction<FlightFormErrors>) => {
      state.formErrors = action.payload
    },

    clearFormErrors: (state) => {
      state.formErrors = {}
    },

    setFormMode: (
      state,
      action: PayloadAction<FlightManagementState["formMode"]>
    ) => {
      state.formMode = action.payload
      if (action.payload === "create") {
        state.formData = {}
        state.selectedFlight = null
      }
      state.isFormDirty = false
      state.formErrors = {}
    },

    resetForm: (state) => {
      state.formData = {}
      state.formErrors = {}
      state.isFormDirty = false
      state.selectedFlight = null
    },

    // Loading states
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },

    setSaving: (state, action: PayloadAction<boolean>) => {
      state.isSaving = action.payload
    },

    setDeleting: (state, action: PayloadAction<boolean>) => {
      state.isDeleting = action.payload
    },

    // Error handling
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
    },

    setValidationErrors: (state, action: PayloadAction<string[]>) => {
      state.validationErrors = action.payload
    },

    clearErrors: (state) => {
      state.error = null
      state.validationErrors = []
    },

    // SignalR connection
    setSignalRConnection: (state, action: PayloadAction<any>) => {
      state.signalRConnection = action.payload
    },

    setSignalRConnected: (state, action: PayloadAction<boolean>) => {
      state.signalRConnected = action.payload
    },

    // Bulk operations
    toggleFlightSelection: (state, action: PayloadAction<number>) => {
      const flightId = action.payload
      const index = state.selectedFlightIds.indexOf(flightId)
      if (index >= 0) {
        state.selectedFlightIds.splice(index, 1)
      } else {
        state.selectedFlightIds.push(flightId)
      }
    },

    selectAllFlights: (state) => {
      state.selectedFlightIds = state.flights.map((f) => f.id)
    },

    clearFlightSelection: (state) => {
      state.selectedFlightIds = []
    },

    setBulkOperation: (
      state,
      action: PayloadAction<FlightManagementState["bulkOperation"]>
    ) => {
      state.bulkOperation = action.payload
    },

    // Search and filter
    setSearchTerm: (state, action: PayloadAction<string>) => {
      state.searchTerm = action.payload
    },

    setFilterBy: (
      state,
      action: PayloadAction<Partial<FlightManagementState["filterBy"]>>
    ) => {
      state.filterBy = { ...state.filterBy, ...action.payload }
    },

    clearFilters: (state) => {
      state.searchTerm = ""
      state.filterBy = {}
    },

    // Import/Export
    setImportProgress: (
      state,
      action: PayloadAction<Partial<FlightManagementState["importProgress"]>>
    ) => {
      state.importProgress = { ...state.importProgress, ...action.payload }
    },

    setExportProgress: (
      state,
      action: PayloadAction<Partial<FlightManagementState["exportProgress"]>>
    ) => {
      state.exportProgress = { ...state.exportProgress, ...action.payload }
    },

    // Statistics
    setStats: (
      state,
      action: PayloadAction<FlightManagementState["stats"]>
    ) => {
      state.stats = action.payload
    },

    // Reset state
    resetFlightManagement: () => initialState,
  },
})

export const {
  setFlights,
  addFlight,
  updateFlight,
  removeFlight,
  setSelectedFlight,
  setFormData,
  setFormField,
  setFormErrors,
  clearFormErrors,
  setFormMode,
  resetForm,
  setLoading,
  setSaving,
  setDeleting,
  setError,
  setValidationErrors,
  clearErrors,
  setSignalRConnection,
  setSignalRConnected,
  toggleFlightSelection,
  selectAllFlights,
  clearFlightSelection,
  setBulkOperation,
  setSearchTerm,
  setFilterBy,
  clearFilters,
  setImportProgress,
  setExportProgress,
  setStats,
  resetFlightManagement,
} = flightManagementSlice.actions

export default flightManagementSlice.reducer
