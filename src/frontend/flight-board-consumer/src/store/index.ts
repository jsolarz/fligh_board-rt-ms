// Redux store configuration for Flight Board Consumer App
// Cyberpunk neural state management system

import { configureStore } from "@reduxjs/toolkit"
import { useDispatch, useSelector, TypedUseSelectorHook } from "react-redux"
import uiReducer from "./slices/uiSlice"
import searchReducer from "./slices/searchSlice"
import flightBoardReducer from "./slices/flightBoardSlice"

// Configure Redux store with slices
export const store = configureStore({
  reducer: {
    ui: uiReducer,
    search: searchReducer,
    flightBoard: flightBoardReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // Ignore specific action types for SignalR connection objects
        ignoredActions: ["flightBoard/setSignalRConnection"],
        ignoredPaths: ["flightBoard.signalRConnection"],
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
