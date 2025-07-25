// filepath: d:\personal\fligh_board-rt-ms\src\frontend\flight-board-backoffice\src\types\flight.types.ts
// TypeScript interfaces matching backend DTOs for Backoffice App
// Shared with Consumer App - maintain sync manually

export interface FlightDto {
  id: number
  flightNumber: string
  airline: string
  origin: string
  destination: string
  scheduledDeparture: string // ISO string
  actualDeparture?: string
  scheduledArrival: string
  actualArrival?: string
  status: string
  gate?: string
  terminal?: string
  aircraftType?: string
  remarks?: string
  delayMinutes: number
  type: string
  createdAt: string
  updatedAt: string
  // Computed properties
  isDelayed: boolean
  estimatedDeparture: string
  estimatedArrival: string
}

export interface CreateFlightDto {
  flightNumber: string
  airline: string
  origin: string
  destination: string
  scheduledDeparture: string
  scheduledArrival: string
  status?: string
  gate?: string
  terminal?: string
  aircraftType?: string
  remarks?: string
  delayMinutes?: number
  type?: string
}

export interface UpdateFlightDto {
  flightNumber?: string
  airline?: string
  origin?: string
  destination?: string
  scheduledDeparture?: string
  actualDeparture?: string
  scheduledArrival?: string
  actualArrival?: string
  status?: string
  gate?: string
  terminal?: string
  aircraftType?: string
  remarks?: string
  delayMinutes?: number
  type?: string
}

export interface FlightSearchDto {
  flightNumber?: string
  airline?: string
  origin?: string
  destination?: string
  status?: string
  type?: string
  fromDate?: string
  toDate?: string
  isDelayed?: boolean
  page?: number
  pageSize?: number
}

export interface PagedResponse<T> {
  data: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean
  hasData: boolean
  currentPageSize: number
}

export interface UpdateStatusRequest {
  status: string
  remarks?: string
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
  timestamp: string
}

// Form validation types
export interface FlightFormErrors {
  flightNumber?: string
  airline?: string
  origin?: string
  destination?: string
  scheduledDeparture?: string
  scheduledArrival?: string
  gate?: string
  terminal?: string
  aircraftType?: string
}

// Component props types
export interface FlightFormProps {
  flight?: FlightDto
  onSubmit: (flight: CreateFlightDto | UpdateFlightDto) => Promise<void>
  onCancel: () => void
  isLoading?: boolean
}

export interface FlightListProps {
  flights: FlightDto[]
  onEdit: (flight: FlightDto) => void
  onDelete: (flightId: number) => Promise<void>
  isLoading?: boolean
}
