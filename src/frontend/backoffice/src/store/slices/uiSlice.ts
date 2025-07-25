// UI state slice for BBS terminal admin interface
// Manages terminal preferences, modes, and admin interface state

import { createSlice, PayloadAction } from "@reduxjs/toolkit"

export interface UIState {
  // Terminal mode and navigation
  currentMode: "list" | "create" | "edit" | "view" | "settings"
  previousMode: "list" | "create" | "edit" | "view" | "settings" | null

  // Terminal preferences
  terminalTheme: "green" | "amber" | "cyan" | "white"
  scanlineEffectsEnabled: boolean
  typingEffectsEnabled: boolean
  soundEffectsEnabled: boolean

  // Loading and error states
  isLoading: boolean
  isSaving: boolean
  error: string | null
  successMessage: string | null

  // Modal dialogs and confirmations
  confirmDialogOpen: boolean
  confirmDialogMessage: string | null
  confirmDialogAction: string | null

  // Admin notifications
  notifications: {
    id: string
    type: "info" | "success" | "warning" | "error"
    message: string
    timestamp: string
    read: boolean
  }[]

  // System status
  systemStatus: "operational" | "maintenance" | "error" | "unknown"
  connectionStatus: "connected" | "disconnected" | "connecting" | "error"

  // Form states
  unsavedChanges: boolean
  formErrors: Record<string, string>

  // Data refresh
  autoRefreshEnabled: boolean
  refreshInterval: number
  lastRefreshTime: string | null

  // Sidebar and layout
  sidebarCollapsed: boolean
  showSystemInfo: boolean

  // Admin preferences
  recordsPerPage: number
  dateFormat: "ISO" | "US" | "EU"
  timeFormat: "12h" | "24h"
}

const initialState: UIState = {
  currentMode: "list",
  previousMode: null,
  terminalTheme: "green",
  scanlineEffectsEnabled: true,
  typingEffectsEnabled: false,
  soundEffectsEnabled: false,
  isLoading: false,
  isSaving: false,
  error: null,
  successMessage: null,
  confirmDialogOpen: false,
  confirmDialogMessage: null,
  confirmDialogAction: null,
  notifications: [],
  systemStatus: "unknown",
  connectionStatus: "disconnected",
  unsavedChanges: false,
  formErrors: {},
  autoRefreshEnabled: true,
  refreshInterval: 30000,
  lastRefreshTime: null,
  sidebarCollapsed: false,
  showSystemInfo: true,
  recordsPerPage: 10,
  dateFormat: "ISO",
  timeFormat: "24h",
}

const uiSlice = createSlice({
  name: "ui",
  initialState,
  reducers: {
    // Mode navigation
    setCurrentMode: (state, action: PayloadAction<UIState["currentMode"]>) => {
      state.previousMode = state.currentMode
      state.currentMode = action.payload
      // Clear unsaved changes when navigating away from forms
      if (action.payload === "list") {
        state.unsavedChanges = false
        state.formErrors = {}
      }
    },

    goToPreviousMode: (state) => {
      if (state.previousMode) {
        const temp = state.currentMode
        state.currentMode = state.previousMode
        state.previousMode = temp
      }
    },

    // Terminal preferences
    setTerminalTheme: (
      state,
      action: PayloadAction<UIState["terminalTheme"]>
    ) => {
      state.terminalTheme = action.payload
    },

    toggleScanlineEffects: (state) => {
      state.scanlineEffectsEnabled = !state.scanlineEffectsEnabled
    },

    toggleTypingEffects: (state) => {
      state.typingEffectsEnabled = !state.typingEffectsEnabled
    },

    toggleSoundEffects: (state) => {
      state.soundEffectsEnabled = !state.soundEffectsEnabled
    },

    // Loading and error states
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },

    setSaving: (state, action: PayloadAction<boolean>) => {
      state.isSaving = action.payload
    },

    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
      if (action.payload) {
        state.successMessage = null
      }
    },

    setSuccessMessage: (state, action: PayloadAction<string | null>) => {
      state.successMessage = action.payload
      if (action.payload) {
        state.error = null
      }
    },

    clearMessages: (state) => {
      state.error = null
      state.successMessage = null
    },

    // Confirmation dialogs
    openConfirmDialog: (
      state,
      action: PayloadAction<{ message: string; action: string }>
    ) => {
      state.confirmDialogOpen = true
      state.confirmDialogMessage = action.payload.message
      state.confirmDialogAction = action.payload.action
    },

    closeConfirmDialog: (state) => {
      state.confirmDialogOpen = false
      state.confirmDialogMessage = null
      state.confirmDialogAction = null
    },

    // Notifications
    addNotification: (
      state,
      action: PayloadAction<{
        type: UIState["notifications"][0]["type"]
        message: string
      }>
    ) => {
      const notification = {
        id: Date.now().toString(),
        type: action.payload.type,
        message: action.payload.message,
        timestamp: new Date().toISOString(),
        read: false,
      }
      state.notifications.unshift(notification)
      // Keep only last 100 notifications
      state.notifications = state.notifications.slice(0, 100)
    },

    markNotificationRead: (state, action: PayloadAction<string>) => {
      const notification = state.notifications.find(
        (n) => n.id === action.payload
      )
      if (notification) {
        notification.read = true
      }
    },

    markAllNotificationsRead: (state) => {
      state.notifications.forEach((notification) => {
        notification.read = true
      })
    },

    removeNotification: (state, action: PayloadAction<string>) => {
      state.notifications = state.notifications.filter(
        (n) => n.id !== action.payload
      )
    },

    clearNotifications: (state) => {
      state.notifications = []
    },

    // System status
    setSystemStatus: (
      state,
      action: PayloadAction<UIState["systemStatus"]>
    ) => {
      state.systemStatus = action.payload
    },

    setConnectionStatus: (
      state,
      action: PayloadAction<UIState["connectionStatus"]>
    ) => {
      state.connectionStatus = action.payload
    },

    // Form states
    setUnsavedChanges: (state, action: PayloadAction<boolean>) => {
      state.unsavedChanges = action.payload
    },

    setFormErrors: (state, action: PayloadAction<Record<string, string>>) => {
      state.formErrors = action.payload
    },

    addFormError: (
      state,
      action: PayloadAction<{ field: string; error: string }>
    ) => {
      state.formErrors[action.payload.field] = action.payload.error
    },

    clearFormError: (state, action: PayloadAction<string>) => {
      delete state.formErrors[action.payload]
    },

    clearAllFormErrors: (state) => {
      state.formErrors = {}
    },

    // Data refresh
    setAutoRefreshEnabled: (state, action: PayloadAction<boolean>) => {
      state.autoRefreshEnabled = action.payload
    },

    setRefreshInterval: (state, action: PayloadAction<number>) => {
      state.refreshInterval = action.payload
    },

    setLastRefreshTime: (state, action: PayloadAction<string>) => {
      state.lastRefreshTime = action.payload
    },

    // Layout
    toggleSidebar: (state) => {
      state.sidebarCollapsed = !state.sidebarCollapsed
    },

    toggleSystemInfo: (state) => {
      state.showSystemInfo = !state.showSystemInfo
    },

    // Admin preferences
    setRecordsPerPage: (state, action: PayloadAction<number>) => {
      state.recordsPerPage = action.payload
    },

    setDateFormat: (state, action: PayloadAction<UIState["dateFormat"]>) => {
      state.dateFormat = action.payload
    },

    setTimeFormat: (state, action: PayloadAction<UIState["timeFormat"]>) => {
      state.timeFormat = action.payload
    },

    // Reset to defaults
    resetUI: () => initialState,
  },
})

export const {
  setCurrentMode,
  goToPreviousMode,
  setTerminalTheme,
  toggleScanlineEffects,
  toggleTypingEffects,
  toggleSoundEffects,
  setLoading,
  setSaving,
  setError,
  setSuccessMessage,
  clearMessages,
  openConfirmDialog,
  closeConfirmDialog,
  addNotification,
  markNotificationRead,
  markAllNotificationsRead,
  removeNotification,
  clearNotifications,
  setSystemStatus,
  setConnectionStatus,
  setUnsavedChanges,
  setFormErrors,
  addFormError,
  clearFormError,
  clearAllFormErrors,
  setAutoRefreshEnabled,
  setRefreshInterval,
  setLastRefreshTime,
  toggleSidebar,
  toggleSystemInfo,
  setRecordsPerPage,
  setDateFormat,
  setTimeFormat,
  resetUI,
} = uiSlice.actions

export default uiSlice.reducer
