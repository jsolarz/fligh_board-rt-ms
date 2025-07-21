// TypeScript interfaces matching backend DTOs
// Auto-generated from C# DTOs - maintain sync manually for now

export interface FlightDto {
  id: number;
  flightNumber: string;
  airline: string;
  origin: string;
  destination: string;
  scheduledDeparture: string; // ISO string
  actualDeparture?: string;
  scheduledArrival: string;
  actualArrival?: string;
  status: string;
  gate?: string;
  terminal?: string;
  aircraftType?: string;
  remarks?: string;
  delayMinutes: number;
  type: string;
  createdAt: string;
  updatedAt: string;
  // Computed properties
  isDelayed: boolean;
  estimatedDeparture: string;
  estimatedArrival: string;
}

export interface CreateFlightDto {
  flightNumber: string;
  airline: string;
  origin: string;
  destination: string;
  scheduledDeparture: string;
  scheduledArrival: string;
  status?: string;
  gate?: string;
  terminal?: string;
  aircraftType?: string;
  remarks?: string;
  delayMinutes?: number;
  type?: string;
}

export interface UpdateFlightDto {
  flightNumber?: string;
  airline?: string;
  origin?: string;
  destination?: string;
  scheduledDeparture?: string;
  actualDeparture?: string;
  scheduledArrival?: string;
  actualArrival?: string;
  status?: string;
  gate?: string;
  terminal?: string;
  aircraftType?: string;
  remarks?: string;
  delayMinutes?: number;
  type?: string;
}

export interface FlightSearchDto {
  flightNumber?: string;
  airline?: string;
  origin?: string;
  destination?: string;
  status?: string;
  type?: string;
  fromDate?: string;
  toDate?: string;
  isDelayed?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PagedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
  hasData: boolean;
  currentPageSize: number;
}

export interface UpdateStatusRequest {
  status: string;
  remarks?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
  timestamp: string;
}

// Frontend-specific types
export interface FlightBoardState {
  flights: FlightDto[];
  loading: boolean;
  error: string | null;
  searchParams: FlightSearchDto;
}

// Flight status enum for type safety
export enum FlightStatus {
  Scheduled = 'Scheduled',
  Boarding = 'Boarding',
  Departed = 'Departed',
  InFlight = 'InFlight',
  Arrived = 'Arrived',
  Delayed = 'Delayed',
  Cancelled = 'Cancelled'
}

// Flight type enum
export enum FlightType {
  Departure = 'Departure',
  Arrival = 'Arrival'
}
