// Admin slice for administrative functionality and system management
// Handles admin user sessions, permissions, and system-wide settings

import { createSlice, PayloadAction } from "@reduxjs/toolkit"

export interface AdminState {
  // Admin user information
  adminUser: {
    id: string
    username: string
    email: string
    role: "super_admin" | "admin" | "operator"
    permissions: string[]
    lastLogin: string | null
    sessionExpiry: string | null
  } | null

  // Authentication state
  isAuthenticated: boolean
  authToken: string | null
  tokenExpiry: string | null

  // System settings
  systemSettings: {
    maintenanceMode: boolean
    allowRegistration: boolean
    requireEmailVerification: boolean
    sessionTimeout: number
    maxLoginAttempts: number
    passwordMinLength: number
    enableAuditLog: boolean
    enableRealTimeUpdates: boolean
  }

  // Audit log
  auditLogs: {
    id: string
    userId: string
    username: string
    action: string
    resource: string
    timestamp: string
    ipAddress: string
    userAgent: string
    success: boolean
    details?: string
  }[]

  // User management
  users: {
    id: string
    username: string
    email: string
    role: string
    active: boolean
    lastLogin: string | null
    createdAt: string
  }[]

  // System statistics
  systemStats: {
    totalUsers: number
    activeUsers: number
    totalFlights: number
    flightsToday: number
    systemUptime: number
    databaseSize: number
    cacheHitRate: number
    averageResponseTime: number
  }

  // Backup and maintenance
  backupStatus: {
    lastBackup: string | null
    nextScheduledBackup: string | null
    backupInProgress: boolean
    backupSize: number
  }

  // Notifications and alerts
  systemAlerts: {
    id: string
    level: "info" | "warning" | "error" | "critical"
    message: string
    timestamp: string
    acknowledged: boolean
    category: "system" | "security" | "performance" | "database"
  }[]

  // Performance monitoring
  performanceMetrics: {
    cpuUsage: number
    memoryUsage: number
    diskUsage: number
    networkLatency: number
    activeConnections: number
    requestsPerMinute: number
    errorRate: number
  }

  // Configuration
  config: {
    apiEndpoints: Record<string, string>
    featureFlags: Record<string, boolean>
    environmentSettings: Record<string, any>
  }
}

const initialState: AdminState = {
  adminUser: null,
  isAuthenticated: false,
  authToken: null,
  tokenExpiry: null,
  systemSettings: {
    maintenanceMode: false,
    allowRegistration: false,
    requireEmailVerification: true,
    sessionTimeout: 3600, // 1 hour
    maxLoginAttempts: 5,
    passwordMinLength: 8,
    enableAuditLog: true,
    enableRealTimeUpdates: true,
  },
  auditLogs: [],
  users: [],
  systemStats: {
    totalUsers: 0,
    activeUsers: 0,
    totalFlights: 0,
    flightsToday: 0,
    systemUptime: 0,
    databaseSize: 0,
    cacheHitRate: 0,
    averageResponseTime: 0,
  },
  backupStatus: {
    lastBackup: null,
    nextScheduledBackup: null,
    backupInProgress: false,
    backupSize: 0,
  },
  systemAlerts: [],
  performanceMetrics: {
    cpuUsage: 0,
    memoryUsage: 0,
    diskUsage: 0,
    networkLatency: 0,
    activeConnections: 0,
    requestsPerMinute: 0,
    errorRate: 0,
  },
  config: {
    apiEndpoints: {},
    featureFlags: {},
    environmentSettings: {},
  },
}

const adminSlice = createSlice({
  name: "admin",
  initialState,
  reducers: {
    // Authentication
    setAdminUser: (state, action: PayloadAction<AdminState["adminUser"]>) => {
      state.adminUser = action.payload
      state.isAuthenticated = !!action.payload
    },

    setAuthToken: (
      state,
      action: PayloadAction<{ token: string; expiry: string }>
    ) => {
      state.authToken = action.payload.token
      state.tokenExpiry = action.payload.expiry
    },

    clearAuth: (state) => {
      state.adminUser = null
      state.isAuthenticated = false
      state.authToken = null
      state.tokenExpiry = null
    },

    updateLastLogin: (state) => {
      if (state.adminUser) {
        state.adminUser.lastLogin = new Date().toISOString()
      }
    },

    // System settings
    updateSystemSettings: (
      state,
      action: PayloadAction<Partial<AdminState["systemSettings"]>>
    ) => {
      state.systemSettings = { ...state.systemSettings, ...action.payload }
    },

    toggleMaintenanceMode: (state) => {
      state.systemSettings.maintenanceMode =
        !state.systemSettings.maintenanceMode
    },

    // Audit logs
    addAuditLog: (
      state,
      action: PayloadAction<Omit<AdminState["auditLogs"][0], "id">>
    ) => {
      const auditLog = {
        id: Date.now().toString(),
        ...action.payload,
      }
      state.auditLogs.unshift(auditLog)
      // Keep only last 1000 logs in memory
      state.auditLogs = state.auditLogs.slice(0, 1000)
    },

    setAuditLogs: (state, action: PayloadAction<AdminState["auditLogs"]>) => {
      state.auditLogs = action.payload
    },

    // User management
    setUsers: (state, action: PayloadAction<AdminState["users"]>) => {
      state.users = action.payload
    },

    addUser: (state, action: PayloadAction<AdminState["users"][0]>) => {
      state.users.push(action.payload)
    },

    updateUser: (state, action: PayloadAction<AdminState["users"][0]>) => {
      const index = state.users.findIndex((u) => u.id === action.payload.id)
      if (index >= 0) {
        state.users[index] = action.payload
      }
    },

    removeUser: (state, action: PayloadAction<string>) => {
      state.users = state.users.filter((u) => u.id !== action.payload)
    },

    // System statistics
    setSystemStats: (
      state,
      action: PayloadAction<AdminState["systemStats"]>
    ) => {
      state.systemStats = action.payload
    },

    updateSystemStats: (
      state,
      action: PayloadAction<Partial<AdminState["systemStats"]>>
    ) => {
      state.systemStats = { ...state.systemStats, ...action.payload }
    },

    // Backup status
    setBackupStatus: (
      state,
      action: PayloadAction<AdminState["backupStatus"]>
    ) => {
      state.backupStatus = action.payload
    },

    startBackup: (state) => {
      state.backupStatus.backupInProgress = true
    },

    completeBackup: (
      state,
      action: PayloadAction<{ timestamp: string; size: number }>
    ) => {
      state.backupStatus.backupInProgress = false
      state.backupStatus.lastBackup = action.payload.timestamp
      state.backupStatus.backupSize = action.payload.size
    },

    // System alerts
    addSystemAlert: (
      state,
      action: PayloadAction<Omit<AdminState["systemAlerts"][0], "id">>
    ) => {
      const alert = {
        id: Date.now().toString(),
        ...action.payload,
      }
      state.systemAlerts.unshift(alert)
      // Keep only last 200 alerts
      state.systemAlerts = state.systemAlerts.slice(0, 200)
    },

    acknowledgeAlert: (state, action: PayloadAction<string>) => {
      const alert = state.systemAlerts.find((a) => a.id === action.payload)
      if (alert) {
        alert.acknowledged = true
      }
    },

    clearAlert: (state, action: PayloadAction<string>) => {
      state.systemAlerts = state.systemAlerts.filter(
        (a) => a.id !== action.payload
      )
    },

    clearAllAlerts: (state) => {
      state.systemAlerts = []
    },

    // Performance metrics
    setPerformanceMetrics: (
      state,
      action: PayloadAction<AdminState["performanceMetrics"]>
    ) => {
      state.performanceMetrics = action.payload
    },

    updatePerformanceMetrics: (
      state,
      action: PayloadAction<Partial<AdminState["performanceMetrics"]>>
    ) => {
      state.performanceMetrics = {
        ...state.performanceMetrics,
        ...action.payload,
      }
    },

    // Configuration
    setConfig: (state, action: PayloadAction<AdminState["config"]>) => {
      state.config = action.payload
    },

    updateConfig: (
      state,
      action: PayloadAction<Partial<AdminState["config"]>>
    ) => {
      state.config = { ...state.config, ...action.payload }
    },

    setFeatureFlag: (
      state,
      action: PayloadAction<{ flag: string; enabled: boolean }>
    ) => {
      state.config.featureFlags[action.payload.flag] = action.payload.enabled
    },

    // Reset admin state
    resetAdmin: () => initialState,
  },
})

export const {
  setAdminUser,
  setAuthToken,
  clearAuth,
  updateLastLogin,
  updateSystemSettings,
  toggleMaintenanceMode,
  addAuditLog,
  setAuditLogs,
  setUsers,
  addUser,
  updateUser,
  removeUser,
  setSystemStats,
  updateSystemStats,
  setBackupStatus,
  startBackup,
  completeBackup,
  addSystemAlert,
  acknowledgeAlert,
  clearAlert,
  clearAllAlerts,
  setPerformanceMetrics,
  updatePerformanceMetrics,
  setConfig,
  updateConfig,
  setFeatureFlag,
  resetAdmin,
} = adminSlice.actions

export default adminSlice.reducer
