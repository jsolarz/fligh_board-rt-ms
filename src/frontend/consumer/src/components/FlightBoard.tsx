// FlightBoard component - Cyberpunk flight matrix display with Redux integration
import React, { useCallback, useEffect } from "react"
import { useQuery } from "@tanstack/react-query"
import { FlightApiService } from "../services/flight-api.service"
import { FlightDto, FlightType, PagedResponse } from "../types/flight.types"
import LoadingSpinner from "./LoadingSpinner"
import ErrorAlert from "./ErrorAlert"
import SearchFiltersRedux from "./SearchFiltersRedux"
import Pagination from "./Pagination"
import useSignalR from "../hooks/useSignalR"
import { useAppSelector, useAppDispatch } from "../store"
import { setPage } from "../store/slices/searchSlice"
import {
  setFlights,
  setLoading,
  setError,
} from "../store/slices/flightBoardSlice"

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
  const dispatch = useAppDispatch()

  // Redux state selectors with explicit typing
  const searchParams = useAppSelector(
    (state: any) => state.search.currentSearch
  )
  const isLoading = useAppSelector((state: any) => state.flightBoard.isLoading)
  const isConnected = useAppSelector(
    (state: any) => state.flightBoard.signalRConnected
  )
  const connectionState = useAppSelector(
    (state: any) => state.flightBoard.signalRConnectionState
  )

  // Set flight type in search params if provided
  useEffect(() => {
    if (flightType && searchParams.type !== flightType) {
      // Update search params with flight type without triggering page reset
      const updatedParams = { ...searchParams, type: flightType }
      // Don't dispatch here to avoid circular updates, handle in parent component
    }
  }, [flightType, searchParams.type])

  // SignalR real-time connection with group filtering
  const { isConnected: signalRConnected, connectionState: signalRState } =
    useSignalR({
      autoConnect: true,
      joinGroups:
        flightType === FlightType.Departure
          ? ["Departures"]
          : flightType === FlightType.Arrival
          ? ["Arrivals"]
          : ["AllFlights"],
    })

  // Handle search parameter changes with useCallback to prevent unnecessary re-renders
  const handleSearchChange = useCallback((newSearchParams: any) => {
    // Search params are managed by Redux, this is just for backward compatibility
    console.log("Search params changed:", newSearchParams)
  }, [])
  const {
    data: flightsData,
    isLoading: queryLoading,
    error,
    isError,
    refetch,
  } = useQuery<PagedResponse<FlightDto>>({
    queryKey: ["flights", searchParams],
    queryFn: async () => {
      dispatch(setLoading(true))

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

      try {
        let result: PagedResponse<FlightDto>

        if (hasFilters) {
          // Use advanced search endpoint when filters are active
          result = await FlightApiService.searchFlights(searchParams)
        } else if (flightType === FlightType.Departure) {
          result = await FlightApiService.getDepartures(searchParams)
        } else if (flightType === FlightType.Arrival) {
          result = await FlightApiService.getArrivals(searchParams)
        } else {
          result = await FlightApiService.getFlights(searchParams)
        }

        dispatch(setFlights(result.data ?? []))
        dispatch(setLoading(false))
        dispatch(setError(null))

        return result
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Unknown error occurred"
        dispatch(setError(errorMessage))
        dispatch(setLoading(false))
        throw err
      }
    },
    refetchInterval: refreshInterval,
    retry: 3,
    staleTime: 5 * 60 * 1000,
  })
  const flights = flightsData?.data ?? []
  const pagination = flightsData ?? {
    page: 1,
    totalPages: 1,
    hasNext: false,
    hasPrevious: false,
    totalCount: 0,
    currentPageSize: 0,
  }

  const handlePageChange = (newPage: number) => {
    dispatch(setPage(newPage))
  }

  // Update Redux loading state
  useEffect(() => {
    dispatch(setLoading(queryLoading))
  }, [queryLoading, dispatch])

  if (queryLoading) {
    return <LoadingSpinner message="ACCESSING_FLIGHT_DATABASE..." />
  }

  if (isError) {
    return (
      <ErrorAlert
        type="error"
        message="NEURAL_LINK_COMPROMISED"
        onRetry={() => refetch()}
      />
    )
  }

  // ...existing code for rendering table and other UI elements...
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      {/* Flight Board Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h2 className="text-3xl font-cyber font-bold text-neon-cyan neon-text">
            {title}
          </h2>
          <div className="font-mono text-sm text-neon-cyan/80">
            <span className="text-neon-green">[ACTIVE]</span> {flights.length}{" "}
            OF {pagination?.totalCount ?? 0} FLIGHTS_LOADED{" "}
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

      {/* Search and Filter Interface */}
      <SearchFiltersRedux
        onSearchChange={handleSearchChange}
        isLoading={queryLoading}
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
                    <div className="text-sm text-neon-cyan/60">
                      {flight.airline}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-mono text-white">
                      <span className="text-neon-orange">{flight.origin}</span>
                      <span className="text-neon-cyan mx-2">→</span>
                      <span className="text-neon-green">
                        {flight.destination}
                      </span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-mono text-white">
                      {new Date(flight.scheduledDeparture).toLocaleString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-mono text-white">
                      {new Date(flight.scheduledArrival).toLocaleString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex px-3 py-1 text-xs font-cyber font-bold uppercase tracking-wider rounded-full ${
                        flight.status === "On Time"
                          ? "bg-neon-green/20 text-neon-green border border-neon-green/30"
                          : flight.status === "Delayed"
                          ? "bg-neon-orange/20 text-neon-orange border border-neon-orange/30"
                          : flight.status === "Boarding"
                          ? "bg-neon-cyan/20 text-neon-cyan border border-neon-cyan/30"
                          : flight.status === "Departed"
                          ? "bg-neon-purple/20 text-neon-purple border border-neon-purple/30"
                          : "bg-red-500/20 text-red-400 border border-red-400/30"
                      }`}
                    >
                      {flight.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-mono text-white">
                      {flight.gate ?? "TBD"}
                    </div>
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
      )}

      {/* Pagination */}
      {pagination && (
        <Pagination
          currentPage={pagination.page}
          totalPages={pagination.totalPages}
          hasNext={pagination.hasNext}
          hasPrevious={pagination.hasPrevious}
          totalCount={pagination.totalCount}
          currentPageSize={pagination.currentPageSize}
          onPageChange={handlePageChange}
          isLoading={queryLoading}
        />
      )}
    </div>
  )
}

export default FlightBoard
