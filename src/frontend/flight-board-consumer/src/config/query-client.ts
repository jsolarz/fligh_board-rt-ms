// React Query client configuration
// Provides global query configuration and client setup

import { QueryClient } from '@tanstack/react-query';

// Create and configure the React Query client
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Global query configuration
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes (was cacheTime in v4)
      retry: (failureCount, error) => {
        // Retry logic: don't retry for 4xx errors
        if (error instanceof Error && error.message.includes('404')) {
          return false;
        }
        return failureCount < 3;
      },
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
      refetchOnWindowFocus: false, // Disable refetch on window focus for demo
      refetchOnReconnect: true,
    },
    mutations: {
      // Global mutation configuration
      retry: 1,
    },
  },
});

export default queryClient;
