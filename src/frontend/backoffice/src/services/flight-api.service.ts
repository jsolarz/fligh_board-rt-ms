// filepath: d:\personal\fligh_board-rt-ms\src\frontend\flight-board-backoffice\src\services\flight-api.service.ts
// API service for flight board backend communication (Backoffice Admin App)
// Uses Axios for HTTP requests with comprehensive error handling

import axios, { AxiosResponse } from "axios"
import {
  FlightDto,
  PagedResponse,
  FlightSearchDto,
  CreateFlightDto,
  UpdateFlightDto,
  UpdateStatusRequest,
} from "../types/flight.types"
import { apiConfig } from "../config/app.config"

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: `${apiConfig.apiUrl}/api`,
  timeout: apiConfig.timeout,
  headers: {
    "Content-Type": "application/json",
  },
})

// Request interceptor for logging and auth (future)
apiClient.interceptors.request.use(
  (config) => {
    console.log(`[ADMIN_API] ${config.method?.toUpperCase()} ${config.url}`)
    return config
  },
  (error) => {
    console.error("[ADMIN_API] Request error:", error)
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    console.log(`[ADMIN_API] ${response.status} ${response.config.url}`)
    return response
  },
  (error) => {
    console.error(
      `[ADMIN_API] ${error.response?.status ?? "Network"} error:`,
      error.message
    )
    return Promise.reject(error)
  }
)

// Flight API service class
export class FlightApiService {
  // Get all flights with pagination and search
  static async getFlights(
    params?: FlightSearchDto
  ): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> =
      await apiClient.get("/flights", {
        params,
      })
    return response.data
  }

  // Get single flight by ID
  static async getFlight(id: number): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.get(
      `/flights/${id}`
    )
    return response.data
  }

  // Create new flight
  static async createFlight(flight: CreateFlightDto): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.post(
      "/flights",
      flight
    )
    return response.data
  }

  // Update existing flight
  static async updateFlight(
    id: number,
    flight: UpdateFlightDto
  ): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.put(
      `/flights/${id}`,
      flight
    )
    return response.data
  }

  // Delete flight (soft delete)
  static async deleteFlight(id: number): Promise<void> {
    await apiClient.delete(`/flights/${id}`)
  }

  // Update flight status
  static async updateFlightStatus(
    id: number,
    statusUpdate: UpdateStatusRequest
  ): Promise<FlightDto> {
    const response: AxiosResponse<FlightDto> = await apiClient.patch(
      `/flights/${id}/status`,
      statusUpdate
    )
    return response.data
  }

  // Search flights with advanced filtering
  static async searchFlights(
    searchParams: FlightSearchDto = {}
  ): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> =
      await apiClient.get("/flights/search", {
        params: searchParams,
      })
    return response.data
  }

  // Get departures with pagination
  static async getDepartures(
    params?: FlightSearchDto
  ): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> =
      await apiClient.get("/flights/departures", {
        params,
      })
    return response.data
  }

  // Get arrivals with pagination
  static async getArrivals(
    params?: FlightSearchDto
  ): Promise<PagedResponse<FlightDto>> {
    const response: AxiosResponse<PagedResponse<FlightDto>> =
      await apiClient.get("/flights/arrivals", {
        params,
      })
    return response.data
  }

  // Get active flights
  static async getActiveFlights(): Promise<FlightDto[]> {
    const response: AxiosResponse<FlightDto[]> = await apiClient.get(
      "/flights/active"
    )
    return response.data
  }

  // Get delayed flights
  static async getDelayedFlights(): Promise<FlightDto[]> {
    const response: AxiosResponse<FlightDto[]> = await apiClient.get(
      "/flights/delayed"
    )
    return response.data
  }

  // Health check endpoint
  static async healthCheck(): Promise<{ status: string; timestamp: string }> {
    const response = await apiClient.get("/flights/health")
    return response.data
  }
}

// Default export
export default FlightApiService
