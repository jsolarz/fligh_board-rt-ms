// Redux-connected SearchFilters component
// Cyberpunk search interface with centralized state management

import React, { useEffect } from "react"
import { FlightStatus } from "../types/flight.types"
import { useAppSelector, useAppDispatch } from "../store"
import {
  setFlightNumber,
  setDestination,
  setStatus,
  setAirline,
  setOrigin,
  setIsDelayed,
  setDateRange,
  clearAllFilters,
} from "../store/slices/searchSlice"
import { setSearchFiltersExpanded } from "../store/slices/uiSlice"

interface SearchFiltersProps {
  onSearchChange: (params: any) => void
  isLoading?: boolean
}

const SearchFilters: React.FC<SearchFiltersProps> = ({
  onSearchChange,
  isLoading = false,
}) => {
  const dispatch = useAppDispatch()

  // Redux state selectors
  const searchParams = useAppSelector((state) => state.search.currentSearch)
  const isExpanded = useAppSelector((state) => state.ui.searchFiltersExpanded)

  // Debounce search to avoid excessive API calls
  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      onSearchChange(searchParams)
    }, 500)

    return () => clearTimeout(debounceTimer)
  }, [searchParams, onSearchChange])

  const handleFilterChange = (action: any, value: any) => {
    dispatch(action(value))
  }

  const handleClearFilters = () => {
    dispatch(clearAllFilters())
  }

  const toggleExpanded = () => {
    dispatch(setSearchFiltersExpanded(!isExpanded))
  }

  const statusOptions = Object.values(FlightStatus)

  return (
    <div className="holographic rounded-lg border border-neon-cyan/30 mb-6 overflow-hidden">
      {/* Search Header */}
      <div
        className="flex items-center justify-between p-4 bg-neon-cyan/5 border-b border-neon-cyan/30 cursor-pointer hover:bg-neon-cyan/10 transition-colors"
        onClick={toggleExpanded}
      >
        <div className="flex items-center space-x-3">
          <div className="w-3 h-3 bg-neon-green rounded-full animate-pulse"></div>
          <span className="font-cyber text-neon-cyan font-bold uppercase tracking-wide">
            SEARCH_MATRIX
          </span>
          {(searchParams.destination ||
            searchParams.status ||
            searchParams.flightNumber) && (
            <span className="px-2 py-1 bg-neon-cyan/20 rounded text-xs text-neon-cyan">
              FILTERS_ACTIVE
            </span>
          )}
        </div>
        <div className="flex items-center space-x-3">
          <button
            onClick={(e) => {
              e.stopPropagation()
              handleClearFilters()
            }}
            className="text-xs cyber-button border-yellow-400 text-yellow-400 hover:shadow-yellow-400/50"
            disabled={isLoading}
          >
            CLEAR_ALL
          </button>
          <div
            className={`transform transition-transform ${
              isExpanded ? "rotate-180" : ""
            }`}
          >
            <svg
              className="w-5 h-5 text-neon-cyan"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </div>
        </div>
      </div>

      {/* Search Filters */}
      {isExpanded && (
        <div className="p-6 space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {/* Flight Number Search */}
            <div className="space-y-2">
              <label className="block text-sm font-cyber text-neon-cyan uppercase tracking-wide">
                Flight_Number
              </label>
              <input
                type="text"
                value={searchParams.flightNumber || ""}
                onChange={(e) =>
                  handleFilterChange(setFlightNumber, e.target.value)
                }
                placeholder="e.g., UA123"
                className="w-full cyber-input"
                disabled={isLoading}
              />
            </div>

            {/* Destination Filter */}
            <div className="space-y-2">
              <label className="block text-sm font-cyber text-neon-cyan uppercase tracking-wide">
                Destination
              </label>
              <input
                type="text"
                value={searchParams.destination || ""}
                onChange={(e) =>
                  handleFilterChange(setDestination, e.target.value)
                }
                placeholder="e.g., LAX, NYC"
                className="w-full cyber-input"
                disabled={isLoading}
              />
            </div>

            {/* Status Filter */}
            <div className="space-y-2">
              <label className="block text-sm font-cyber text-neon-cyan uppercase tracking-wide">
                Status
              </label>
              <select
                value={searchParams.status || ""}
                onChange={(e) => handleFilterChange(setStatus, e.target.value)}
                className="w-full cyber-select"
                disabled={isLoading}
              >
                <option value="">ALL_STATUS</option>
                {statusOptions.map((status) => (
                  <option key={status} value={status}>
                    {status.toUpperCase()}
                  </option>
                ))}
              </select>
            </div>

            {/* Airline Filter */}
            <div className="space-y-2">
              <label className="block text-sm font-cyber text-neon-cyan uppercase tracking-wide">
                Airline
              </label>
              <input
                type="text"
                value={searchParams.airline || ""}
                onChange={(e) => handleFilterChange(setAirline, e.target.value)}
                placeholder="e.g., United, Delta"
                className="w-full cyber-input"
                disabled={isLoading}
              />
            </div>

            {/* Origin Filter */}
            <div className="space-y-2">
              <label className="block text-sm font-cyber text-neon-cyan uppercase tracking-wide">
                Origin
              </label>
              <input
                type="text"
                value={searchParams.origin || ""}
                onChange={(e) => handleFilterChange(setOrigin, e.target.value)}
                placeholder="e.g., JFK, DFW"
                className="w-full cyber-input"
                disabled={isLoading}
              />
            </div>

            {/* Delayed Only Toggle */}
            <div className="space-y-2">
              <label className="block text-sm font-cyber text-neon-cyan uppercase tracking-wide">
                Show_Delayed_Only
              </label>
              <label className="cyber-checkbox-container">
                <input
                  type="checkbox"
                  checked={searchParams.isDelayed || false}
                  onChange={(e) =>
                    handleFilterChange(setIsDelayed, e.target.checked)
                  }
                  className="cyber-checkbox"
                  disabled={isLoading}
                />
                <span className="cyber-checkbox-checkmark"></span>
                <span className="text-sm text-gray-300 ml-2">
                  Delayed_Flights_Only
                </span>
              </label>
            </div>
          </div>

          {/* Advanced Date Filters */}
          <div className="border-t border-neon-cyan/30 pt-4">
            <h4 className="text-sm font-cyber text-neon-cyan uppercase tracking-wide mb-3">
              Date_Range_Filter
            </h4>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="block text-sm font-cyber text-neon-green uppercase tracking-wide">
                  From_Date
                </label>
                <input
                  type="datetime-local"
                  value={searchParams.fromDate || ""}
                  onChange={(e) =>
                    dispatch(setDateRange({ fromDate: e.target.value }))
                  }
                  className="w-full cyber-input"
                  disabled={isLoading}
                />
              </div>
              <div className="space-y-2">
                <label className="block text-sm font-cyber text-neon-green uppercase tracking-wide">
                  To_Date
                </label>
                <input
                  type="datetime-local"
                  value={searchParams.toDate || ""}
                  onChange={(e) =>
                    dispatch(setDateRange({ toDate: e.target.value }))
                  }
                  className="w-full cyber-input"
                  disabled={isLoading}
                />
              </div>
            </div>
          </div>

          {/* Search Status */}
          {isLoading && (
            <div className="flex items-center space-x-2 text-neon-cyan">
              <div className="animate-spin rounded-full h-4 w-4 border-2 border-neon-cyan border-t-transparent"></div>
              <span className="text-sm font-cyber">
                PROCESSING_SEARCH_QUERY...
              </span>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default SearchFilters
