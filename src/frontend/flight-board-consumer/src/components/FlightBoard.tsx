// FlightBoard component - Cyberpunk flight matrix display
import React, { useState, useCallback } from "react"
import { useQuery } from "@tanstack/react-query"
import { FlightApiService } from "../services/flight-api.service"
import { FlightDto, FlightSearchDto, FlightType } from "../types/flight.types"
import LoadingSpinner from "./LoadingSpinner"
import ErrorAlert from "./ErrorAlert"
import SearchFilters from "./SearchFilters"
import Pagination from "./Pagination"
import useSignalR from "../hooks/useSignalR"

interface FlightBoardProps {
  title?: string
  flightType?: FlightType
  refreshInterval?: number
}

const FlightBoard: React.FC<FlightBoardProps> = ({
  title = "FLIGHT_MATRIX",
  flightType,
  refreshInterval = 30000,
}) => {
  const [searchParams, setSearchParams] = useState<FlightSearchDto>({
    page: 1,
    pageSize: 20,
    type: flightType,
  })

  // SignalR real-time connection with group filtering
  const { isConnected, connectionState } = useSignalR({
    autoConnect: true,
    joinGroups:
      flightType === FlightType.Departure
        ? ["Departures"]
        : flightType === FlightType.Arrival
        ? ["Arrivals"]
        : ["AllFlights"],
  })
  // Handle search parameter changes with useCallback to prevent unnecessary re-renders
  const handleSearchChange = useCallback((newSearchParams: FlightSearchDto) => {
    setSearchParams(newSearchParams)
  }, [])

  const {
    data: flightsData,
    isLoading,
    error,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["flights", searchParams],
    queryFn: () => {
      // Use search endpoint if any filters are active (beyond basic pagination and type)
      const hasFilters = Boolean(
        searchParams.flightNumber ||
        searchParams.destination ||
        searchParams.status ||
        searchParams.airline ||
        searchParams.origin ||
        searchParams.isDelayed ||
        searchParams.fromDate ||
        searchParams.toDate
      )

      if (hasFilters) {
        // Use advanced search endpoint when filters are active
        return FlightApiService.searchFlights(searchParams)
      } else if (flightType === FlightType.Departure) {
        return FlightApiService.getDepartures(searchParams)
      } else if (flightType === FlightType.Arrival) {
        return FlightApiService.getArrivals(searchParams)
      } else {
        return FlightApiService.getFlights(searchParams)
      }
    },
    refetchInterval: refreshInterval,
    retry: 3,
    staleTime: 5 * 60 * 1000,
  })
  const flights = flightsData?.data || []
  const pagination = flightsData

  const handlePageChange = (newPage: number) => {
    setSearchParams((prev) => ({ ...prev, page: newPage }))
  }

  if (isLoading) {
    return <LoadingSpinner message="ACCESSING_FLIGHT_DATABASE..." />
  }

  if (isError) {
    return (
      <ErrorAlert
        type="error"
        message={error instanceof Error ? error.message : "Unknown error"}
        onRetry={() => refetch()}
      />
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      {/* Cyberpunk Header */}
      <div className="mb-8 holographic rounded-lg p-6">
        <div className="flex items-center space-x-4 mb-4">
          <div className="flex items-center space-x-2">
            <div className="w-3 h-3 bg-neon-cyan rounded-full animate-pulse"></div>
            <h1 className="text-2xl font-cyber font-bold text-neon-cyan neon-text uppercase tracking-wider">
              {title}
            </h1>
          </div>
          <div className="flex-1 h-px bg-gradient-to-r from-neon-cyan/50 to-transparent"></div>
        </div>

        <div className="flex justify-between items-center">
          {" "}
          <div className="font-mono text-sm text-neon-cyan/80">
            <span className="text-neon-green">[ACTIVE]</span> {flights.length}{" "}
            OF {pagination?.totalCount || 0} FLIGHTS_LOADED
            <div className="flex items-center space-x-2 mt-1">
              <div
                className={`w-2 h-2 rounded-full ${
                  isConnected ? "bg-neon-green animate-pulse" : "bg-red-500"
                }`}
              ></div>
              <span className="text-xs">
                NEURAL_LINK: {isConnected ? "ACTIVE" : "SEVERED"} (
                {connectionState})
              </span>
            </div>
          </div>
          <div className="flex space-x-3">
            <button
              onClick={() => refetch()}
              className="cyber-button text-neon-cyan border-neon-cyan hover:shadow-neon-md"
            >
              REFRESH_DATA
            </button>
          </div>
        </div>
      </div>

      {/* Search and Filter Interface */}
      <SearchFilters
        searchParams={searchParams}
        onSearchChange={handleSearchChange}
        isLoading={isLoading}
      />

      {/* Flight Table */}
      <div className="holographic rounded-lg overflow-hidden border border-neon-cyan/30">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-neon-cyan/10 border-b border-neon-cyan/30">
              <tr>
                <th className="px-6 py-4 text-left text-sm font-cyber font-bold text-neon-cyan uppercase tracking-wider">
                  Flight
                </th>
                <th className="px-6 py-4 text-left text-sm font-cyber font-bold text-neon-cyan uppercase tracking-wider">
                  Route
                </th>
                <th className="px-6 py-4 text-left text-sm font-cyber font-bold text-neon-cyan uppercase tracking-wider">
                  Departure
                </th>
                <th className="px-6 py-4 text-left text-sm font-cyber font-bold text-neon-cyan uppercase tracking-wider">
                  Arrival
                </th>
                <th className="px-6 py-4 text-left text-sm font-cyber font-bold text-neon-cyan uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-4 text-left text-sm font-cyber font-bold text-neon-cyan uppercase tracking-wider">
                  Gate
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-neon-cyan/20">
              {flights.map((flight: FlightDto) => (
                <tr
                  key={flight.id}
                  className="hover:bg-neon-cyan/5 transition-colors duration-200 border-b border-neon-cyan/10"
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-mono font-bold text-neon-cyan">
                      {flight.flightNumber}
                    </div>
                    <div className="text-xs text-neon-cyan/60 font-mono">
                      {flight.airline}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-white font-mono">
                      {flight.origin} → {flight.destination}
                    </div>
                    <div className="text-xs text-neon-cyan/60 font-mono">
                      {flight.type}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-white font-mono">
                      {new Date(flight.scheduledDeparture).toLocaleTimeString()}
                    </div>
                    <div className="text-xs text-neon-cyan/60 font-mono">
                      {new Date(flight.scheduledDeparture).toLocaleDateString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-white font-mono">
                      {new Date(flight.scheduledArrival).toLocaleTimeString()}
                    </div>
                    <div className="text-xs text-neon-cyan/60 font-mono">
                      {new Date(flight.scheduledArrival).toLocaleDateString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex px-2 py-1 text-xs font-cyber font-bold rounded-full border ${
                        flight.status === "Delayed"
                          ? "text-neon-red border-neon-red bg-neon-red/10"
                          : flight.status === "Boarding"
                          ? "text-neon-yellow border-neon-yellow bg-neon-yellow/10"
                          : "text-neon-green border-neon-green bg-neon-green/10"
                      }`}
                    >
                      {flight.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-white font-mono">
                    {flight.gate || "TBD"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Empty State */}
      {flights.length === 0 && (
        <div className="text-center py-12">
          <div className="text-neon-cyan/60 text-lg font-cyber">
            NO_FLIGHT_DATA_AVAILABLE
          </div>
          <div className="text-neon-cyan/40 text-sm font-mono mt-2">
            NEURAL_LINK_SEARCHING...
          </div>
        </div>
      )}      {/* Pagination */}
      {pagination && (
        <Pagination
          currentPage={pagination.page}
          totalPages={pagination.totalPages}
          hasNext={pagination.hasNext}
          hasPrevious={pagination.hasPrevious}
          totalCount={pagination.totalCount}
          currentPageSize={pagination.currentPageSize}
          onPageChange={handlePageChange}
          isLoading={isLoading}
        />
      )}
    </div>
  )
}

export default FlightBoard
