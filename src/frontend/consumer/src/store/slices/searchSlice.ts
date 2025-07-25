// Search state slice for flight filtering and search functionality
// Manages search parameters, filter state, and search history

import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import {
  FlightSearchDto,
  FlightStatus,
  FlightType,
} from "../../types/flight.types"

export interface SearchState {
  // Current search parameters
  currentSearch: FlightSearchDto

  // Search history for quick access
  searchHistory: FlightSearchDto[]

  // Filter state
  activeFilters: string[]

  // Search results metadata
  totalResults: number
  currentPage: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean

  // Search UI state
  isSearching: boolean
  searchError: string | null

  // Quick filters
  quickFilters: {
    delayed: boolean
    boarding: boolean
    departed: boolean
    cancelled: boolean
  }

  // Advanced search settings
  searchMode: "simple" | "advanced"
  sortBy: "departure" | "arrival" | "status" | "flightNumber"
  sortOrder: "asc" | "desc"
}

const initialState: SearchState = {
  currentSearch: {
    page: 1,
    pageSize: 20,
  },
  searchHistory: [],
  activeFilters: [],
  totalResults: 0,
  currentPage: 1,
  totalPages: 1,
  hasNext: false,
  hasPrevious: false,
  isSearching: false,
  searchError: null,
  quickFilters: {
    delayed: false,
    boarding: false,
    departed: false,
    cancelled: false,
  },
  searchMode: "simple",
  sortBy: "departure",
  sortOrder: "asc",
}

const searchSlice = createSlice({
  name: "search",
  initialState,
  reducers: {
    // Search parameters
    setSearchParams: (
      state,
      action: PayloadAction<Partial<FlightSearchDto>>
    ) => {
      state.currentSearch = { ...state.currentSearch, ...action.payload }
      // Reset to first page when search params change (except pagination)
      if (!action.payload.page) {
        state.currentSearch.page = 1
      }
    },

    setFlightNumber: (state, action: PayloadAction<string | undefined>) => {
      state.currentSearch.flightNumber = action.payload
      state.currentSearch.page = 1
    },

    setDestination: (state, action: PayloadAction<string | undefined>) => {
      state.currentSearch.destination = action.payload
      state.currentSearch.page = 1
    },

    setStatus: (state, action: PayloadAction<FlightStatus | undefined>) => {
      state.currentSearch.status = action.payload
      state.currentSearch.page = 1
    },

    setAirline: (state, action: PayloadAction<string | undefined>) => {
      state.currentSearch.airline = action.payload
      state.currentSearch.page = 1
    },

    setOrigin: (state, action: PayloadAction<string | undefined>) => {
      state.currentSearch.origin = action.payload
      state.currentSearch.page = 1
    },

    setFlightType: (state, action: PayloadAction<FlightType | undefined>) => {
      state.currentSearch.type = action.payload
      state.currentSearch.page = 1
    },

    setIsDelayed: (state, action: PayloadAction<boolean | undefined>) => {
      state.currentSearch.isDelayed = action.payload
      state.currentSearch.page = 1
    },

    setDateRange: (
      state,
      action: PayloadAction<{ fromDate?: string; toDate?: string }>
    ) => {
      state.currentSearch.fromDate = action.payload.fromDate
      state.currentSearch.toDate = action.payload.toDate
      state.currentSearch.page = 1
    },

    // Pagination
    setPage: (state, action: PayloadAction<number>) => {
      state.currentSearch.page = action.payload
    },

    setPageSize: (state, action: PayloadAction<number>) => {
      state.currentSearch.pageSize = action.payload
      state.currentSearch.page = 1
    },

    // Search results metadata
    setSearchResults: (
      state,
      action: PayloadAction<{
        totalResults: number
        currentPage: number
        totalPages: number
        hasNext: boolean
        hasPrevious: boolean
      }>
    ) => {
      state.totalResults = action.payload.totalResults
      state.currentPage = action.payload.currentPage
      state.totalPages = action.payload.totalPages
      state.hasNext = action.payload.hasNext
      state.hasPrevious = action.payload.hasPrevious
    },

    // Search state
    setIsSearching: (state, action: PayloadAction<boolean>) => {
      state.isSearching = action.payload
    },

    setSearchError: (state, action: PayloadAction<string | null>) => {
      state.searchError = action.payload
    },

    clearSearchError: (state) => {
      state.searchError = null
    },

    // Quick filters
    toggleQuickFilter: (
      state,
      action: PayloadAction<keyof SearchState["quickFilters"]>
    ) => {
      const filter = action.payload
      state.quickFilters[filter] = !state.quickFilters[filter]

      // Apply quick filter to search params
      switch (filter) {
        case "delayed":
          state.currentSearch.isDelayed =
            state.quickFilters.delayed || undefined
          break
        case "boarding":
          state.currentSearch.status = state.quickFilters.boarding
            ? FlightStatus.Boarding
            : undefined
          break
        case "departed":
          state.currentSearch.status = state.quickFilters.departed
            ? FlightStatus.Departed
            : undefined
          break
        case "cancelled":
          state.currentSearch.status = state.quickFilters.cancelled
            ? FlightStatus.Cancelled
            : undefined
          break
      }
      state.currentSearch.page = 1
    },

    clearQuickFilters: (state) => {
      state.quickFilters = {
        delayed: false,
        boarding: false,
        departed: false,
        cancelled: false,
      }
    },

    // Search history
    addToSearchHistory: (state, action: PayloadAction<FlightSearchDto>) => {
      const newSearch = action.payload
      // Remove duplicates and limit history to 10 items
      state.searchHistory = [
        newSearch,
        ...state.searchHistory.filter(
          (search) => JSON.stringify(search) !== JSON.stringify(newSearch)
        ),
      ].slice(0, 10)
    },

    loadFromHistory: (state, action: PayloadAction<FlightSearchDto>) => {
      state.currentSearch = { ...action.payload }
    },

    clearSearchHistory: (state) => {
      state.searchHistory = []
    },

    // Advanced search
    setSearchMode: (
      state,
      action: PayloadAction<SearchState["searchMode"]>
    ) => {
      state.searchMode = action.payload
    },

    setSortBy: (state, action: PayloadAction<SearchState["sortBy"]>) => {
      state.sortBy = action.payload
    },

    setSortOrder: (state, action: PayloadAction<SearchState["sortOrder"]>) => {
      state.sortOrder = action.payload
    },

    // Clear all filters
    clearAllFilters: (state) => {
      state.currentSearch = {
        page: 1,
        pageSize: state.currentSearch.pageSize ?? 20,
        type: state.currentSearch.type, // Preserve flight type
      }
      state.quickFilters = {
        delayed: false,
        boarding: false,
        departed: false,
        cancelled: false,
      }
      state.activeFilters = []
    },

    // Reset search state
    resetSearch: () => initialState,
  },
})

export const {
  setSearchParams,
  setFlightNumber,
  setDestination,
  setStatus,
  setAirline,
  setOrigin,
  setFlightType,
  setIsDelayed,
  setDateRange,
  setPage,
  setPageSize,
  setSearchResults,
  setIsSearching,
  setSearchError,
  clearSearchError,
  toggleQuickFilter,
  clearQuickFilters,
  addToSearchHistory,
  loadFromHistory,
  clearSearchHistory,
  setSearchMode,
  setSortBy,
  setSortOrder,
  clearAllFilters,
  resetSearch,
} = searchSlice.actions

export default searchSlice.reducer
