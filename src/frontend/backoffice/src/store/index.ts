// Redux store configuration for Flight Board Backoffice Admin Portal
// BBS terminal state management system

import { configureStore } from "@reduxjs/toolkit"
import { useDispatch, useSelector, TypedUseSelectorHook } from "react-redux"
import uiReducer from "./slices/uiSlice"
import flightManagementReducer from "./slices/flightManagementSlice"
import adminReducer from "./slices/adminSlice"

// Configure Redux store with slices
export const store = configureStore({
  reducer: {
    ui: uiReducer,
    flightManagement: flightManagementReducer,
    admin: adminReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // Ignore specific action types for file uploads and SignalR connections
        ignoredActions: ["flightManagement/setSignalRConnection"],
        ignoredPaths: ["flightManagement.signalRConnection"],
      },
    }),
  devTools: process.env.NODE_ENV !== "production",
})

// Export types for TypeScript
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

// Typed hooks for use throughout the app
export const useAppDispatch = () => useDispatch<AppDispatch>()
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector

export default store
