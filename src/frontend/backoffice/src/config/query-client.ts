// filepath: d:\personal\fligh_board-rt-ms\src\frontend\flight-board-backoffice\src\config\query-client.ts
// React Query client configuration for Backoffice Admin App
// Optimized for admin operations with appropriate cache settings

import { QueryClient } from "@tanstack/react-query"

// Create React Query client with admin-optimized settings
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Admin data should be fresh - shorter cache time than consumer app
      staleTime: 2 * 60 * 1000, // 2 minutes (vs 5 minutes for consumer)
      gcTime: 5 * 60 * 1000, // 5 minutes
      retry: 2, // Less aggressive retry for admin operations
      refetchOnWindowFocus: true, // Always refetch when admin returns to tab
      refetchOnMount: true, // Always refetch on component mount
    },
    mutations: {
      retry: 1, // Single retry for admin mutations
      onError: (error) => {
        console.error("[ADMIN_MUTATION_ERROR]", error)
      },
    },
  },
})

// Query keys for consistent cache management
export const QUERY_KEYS = {
  flights: ["flights"] as const,
  flight: (id: number) => ["flights", id] as const,
  departures: ["flights", "departures"] as const,
  arrivals: ["flights", "arrivals"] as const,
  active: ["flights", "active"] as const,
  delayed: ["flights", "delayed"] as const,
  health: ["health"] as const,
} as const

// Cache invalidation helpers for admin operations
export const invalidateFlightData = () => {
  queryClient.invalidateQueries({ queryKey: QUERY_KEYS.flights })
  queryClient.invalidateQueries({ queryKey: QUERY_KEYS.departures })
  queryClient.invalidateQueries({ queryKey: QUERY_KEYS.arrivals })
  queryClient.invalidateQueries({ queryKey: QUERY_KEYS.active })
  queryClient.invalidateQueries({ queryKey: QUERY_KEYS.delayed })
}

export default queryClient
