// UI state slice for cyberpunk flight board interface
// Manages cyberpunk theme preferences, sidebar state, modal dialogs

import { createSlice, PayloadAction } from "@reduxjs/toolkit"

export interface UIState {
  // Theme and visual preferences
  theme: "cyberpunk" | "dark" | "neon"
  glitchEffectsEnabled: boolean
  animationsEnabled: boolean

  // Navigation and layout
  currentView: "all" | "departures" | "arrivals"
  sidebarExpanded: boolean

  // Search interface
  searchFiltersExpanded: boolean

  // Loading and error states
  isLoading: boolean
  error: string | null

  // Modal dialogs
  modalOpen: boolean
  modalType: "info" | "error" | "warning" | null
  modalMessage: string | null

  // Connection status
  connectionStatus: "connected" | "disconnected" | "connecting" | "error"

  // User preferences
  refreshInterval: number
  autoRefreshEnabled: boolean
  soundEffectsEnabled: boolean
}

const initialState: UIState = {
  theme: "cyberpunk",
  glitchEffectsEnabled: true,
  animationsEnabled: true,
  currentView: "all",
  sidebarExpanded: false,
  searchFiltersExpanded: false,
  isLoading: false,
  error: null,
  modalOpen: false,
  modalType: null,
  modalMessage: null,
  connectionStatus: "disconnected",
  refreshInterval: 30000,
  autoRefreshEnabled: true,
  soundEffectsEnabled: false,
}

const uiSlice = createSlice({
  name: "ui",
  initialState,
  reducers: {
    // Theme and visual settings
    setTheme: (state, action: PayloadAction<UIState["theme"]>) => {
      state.theme = action.payload
    },
    toggleGlitchEffects: (state) => {
      state.glitchEffectsEnabled = !state.glitchEffectsEnabled
    },
    toggleAnimations: (state) => {
      state.animationsEnabled = !state.animationsEnabled
    },

    // Navigation
    setCurrentView: (state, action: PayloadAction<UIState["currentView"]>) => {
      state.currentView = action.payload
    },
    toggleSidebar: (state) => {
      state.sidebarExpanded = !state.sidebarExpanded
    },

    // Search interface
    setSearchFiltersExpanded: (state, action: PayloadAction<boolean>) => {
      state.searchFiltersExpanded = action.payload
    },
    toggleSearchFilters: (state) => {
      state.searchFiltersExpanded = !state.searchFiltersExpanded
    },

    // Loading and error states
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
    },
    clearError: (state) => {
      state.error = null
    },

    // Modal dialogs
    openModal: (
      state,
      action: PayloadAction<{ type: UIState["modalType"]; message: string }>
    ) => {
      state.modalOpen = true
      state.modalType = action.payload.type
      state.modalMessage = action.payload.message
    },
    closeModal: (state) => {
      state.modalOpen = false
      state.modalType = null
      state.modalMessage = null
    },

    // Connection status
    setConnectionStatus: (
      state,
      action: PayloadAction<UIState["connectionStatus"]>
    ) => {
      state.connectionStatus = action.payload
    },

    // User preferences
    setRefreshInterval: (state, action: PayloadAction<number>) => {
      state.refreshInterval = action.payload
    },
    toggleAutoRefresh: (state) => {
      state.autoRefreshEnabled = !state.autoRefreshEnabled
    },
    toggleSoundEffects: (state) => {
      state.soundEffectsEnabled = !state.soundEffectsEnabled
    },

    // Reset to defaults
    resetUI: () => initialState,
  },
})

export const {
  setTheme,
  toggleGlitchEffects,
  toggleAnimations,
  setCurrentView,
  toggleSidebar,
  setSearchFiltersExpanded,
  toggleSearchFilters,
  setLoading,
  setError,
  clearError,
  openModal,
  closeModal,
  setConnectionStatus,
  setRefreshInterval,
  toggleAutoRefresh,
  toggleSoundEffects,
  resetUI,
} = uiSlice.actions

export default uiSlice.reducer
