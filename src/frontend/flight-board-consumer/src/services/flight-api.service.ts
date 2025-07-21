// API service for flight board backend communication
// Uses Axios for HTTP requests with proper error handling

import axios, { AxiosResponse } from 'axios';
import {
  FlightDto,
  PagedResponse,
  FlightSearchDto,
  CreateFlightDto,
  UpdateFlightDto,
  UpdateStatusRequest,
  ApiResponse
} from '../types/flight.types';

// API configuration
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5183';
const API_TIMEOUT = 10000; // 10 seconds

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  timeout: API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for logging and auth (future)
apiClient.interceptors.request.use(
  (config) => {
    console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error('[API] Request error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    console.log(`[API] Response ${response.status} for ${response.config.url}`);
    return response;
  },
  (error) => {
    console.error('[API] Response error:', error.response?.data || error.message);
    // Transform error to consistent format
    if (error.response?.data?.message) {
      throw new Error(error.response.data.message);
    } else if (error.response?.status >= 500) {
      throw new Error('Server error. Please try again later.');
    } else if (error.response?.status === 404) {
      throw new Error('Resource not found.');
    } else if (error.code === 'ECONNABORTED') {
      throw new Error('Request timeout. Please check your connection.');
    } else {
      throw new Error(error.message || 'An unexpected error occurred.');
    }
  }
);

// Flight API service class
export class FlightApiService {
  // Get paginated flights with search
  static async getFlights(searchParams: FlightSearchDto = {}): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> = await apiClient.get('/flights', {
      params: searchParams
    });
    return response.data;
  }

  // Get single flight by ID
  static async getFlight(id: number): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.get(`/flights/${id}`);
    return response.data;
  }

  // Get departure flights (paginated)
  static async getDepartures(searchParams: FlightSearchDto = {}): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> = await apiClient.get('/flights/departures', {
      params: searchParams
    });
    return response.data;
  }

  // Get arrival flights (paginated)
  static async getArrivals(searchParams: FlightSearchDto = {}): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> = await apiClient.get('/flights/arrivals', {
      params: searchParams
    });
    return response.data;
  }

  // Get active flights (currently in progress)
  static async getActiveFlights(): Promise<FlightDto[]> {
    const response: AxiosResponse<FlightDto[]> = await apiClient.get('/flights/active');
    return response.data;
  }

  // Get delayed flights
  static async getDelayedFlights(): Promise<FlightDto[]> {
    const response: AxiosResponse<FlightDto[]> = await apiClient.get('/flights/delayed');
    return response.data;
  }

  // Create new flight
  static async createFlight(flight: CreateFlightDto): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.post('/flights', flight);
    return response.data;
  }

  // Update existing flight
  static async updateFlight(id: number, flight: UpdateFlightDto): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.put(`/flights/${id}`, flight);
    return response.data;
  }

  // Update flight status
  static async updateFlightStatus(id: number, statusUpdate: UpdateStatusRequest): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.patch(`/flights/${id}/status`, statusUpdate);
    return response.data;
  }

  // Delete flight (soft delete)
  static async deleteFlight(id: number): Promise<void> {
    await apiClient.delete(`/flights/${id}`);
  }

  // Health check endpoint
  static async healthCheck(): Promise<boolean> {
    try {
      await apiClient.get('/flights?pageSize=1');
      return true;
    } catch {
      return false;
    }
  }
}

// Export default instance for convenience
export default FlightApiService;

// Utility function for error handling in components
export const handleApiError = (error: unknown): string => {
  if (error instanceof Error) {
    return error.message;
  }
  return 'An unexpected error occurred';
};
