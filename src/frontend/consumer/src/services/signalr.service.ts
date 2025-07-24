// SignalR service for real-time flight updates
// Cyberpunk neural link communication protocol

import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr"
import { FlightDto } from "../types/flight.types"
import { apiConfig } from "../config/app.config"

export class SignalRService {
  private connection: HubConnection | null = null
  private readonly hubUrl = apiConfig.signalRUrl || `${apiConfig.apiUrl}/flighthub` // Backend SignalR hub URL

  // Event callbacks
  private onFlightCreatedCallback?: (flight: FlightDto) => void
  private onFlightUpdatedCallback?: (flight: FlightDto) => void
  private onFlightStatusChangedCallback?: (
    flight: FlightDto,
    oldStatus: string,
    newStatus: string
  ) => void
  private onConnectionStatusChangedCallback?: (isConnected: boolean) => void

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        withCredentials: false, // Adjust based on your CORS setup
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry intervals
      .build()

    this.setupEventHandlers()
  }

  // Initialize SignalR connection
  async start(): Promise<boolean> {
    try {
      if (this.connection?.state === "Disconnected") {
        await this.connection.start()
        console.log("🔗 SignalR connected - NEURAL_LINK_ACTIVE")
        this.onConnectionStatusChangedCallback?.(true)
        return true
      }
      return this.connection?.state === "Connected"
    } catch (error) {
      console.error(
        "❌ SignalR connection failed - NEURAL_LINK_SEVERED:",
        error
      )
      this.onConnectionStatusChangedCallback?.(false)
      return false
    }
  }

  // Stop SignalR connection
  async stop(): Promise<void> {
    try {
      if (this.connection && this.connection.state !== "Disconnected") {
        await this.connection.stop()
        console.log("🔌 SignalR disconnected - NEURAL_LINK_TERMINATED")
        this.onConnectionStatusChangedCallback?.(false)
      }
    } catch (error) {
      console.error("Error stopping SignalR connection:", error)
    }
  }

  // Get connection status
  get isConnected(): boolean {
    return this.connection?.state === "Connected"
  }

  get connectionState(): string {
    return this.connection?.state ?? "Disconnected"
  }

  // Join specific groups for filtered updates
  async joinGroup(
    groupName: "AllFlights" | "Departures" | "Arrivals"
  ): Promise<void> {
    if (this.isConnected) {
      try {
        await this.connection!.invoke("JoinGroup", groupName)
        console.log(`📡 Joined SignalR group: ${groupName}`)
      } catch (error) {
        console.error(`Failed to join group ${groupName}:`, error)
      }
    }
  }

  async leaveGroup(
    groupName: "AllFlights" | "Departures" | "Arrivals"
  ): Promise<void> {
    if (this.isConnected) {
      try {
        await this.connection!.invoke("LeaveGroup", groupName)
        console.log(`📡 Left SignalR group: ${groupName}`)
      } catch (error) {
        console.error(`Failed to leave group ${groupName}:`, error)
      }
    }
  }

  // Event handler registration
  onFlightCreated(callback: (flight: FlightDto) => void): void {
    this.onFlightCreatedCallback = callback
  }

  onFlightUpdated(callback: (flight: FlightDto) => void): void {
    this.onFlightUpdatedCallback = callback
  }

  onFlightStatusChanged(
    callback: (flight: FlightDto, oldStatus: string, newStatus: string) => void
  ): void {
    this.onFlightStatusChangedCallback = callback
  }

  onConnectionStatusChanged(callback: (isConnected: boolean) => void): void {
    this.onConnectionStatusChangedCallback = callback
  }

  // Setup SignalR event handlers
  private setupEventHandlers(): void {
    if (!this.connection) return

    // Handle flight created events
    this.connection.on("FlightCreated", (flight: FlightDto) => {
      console.log("🆕 Real-time: Flight created", flight.flightNumber)
      this.onFlightCreatedCallback?.(flight)
    })

    this.connection.on("FlightAdded", (flight: FlightDto) => {
      console.log("➕ Real-time: Flight added to group", flight.flightNumber)
      this.onFlightCreatedCallback?.(flight)
    })

    // Handle flight updated events
    this.connection.on("FlightUpdated", (flight: FlightDto) => {
      console.log("🔄 Real-time: Flight updated", flight.flightNumber)
      this.onFlightUpdatedCallback?.(flight)
    })

    // Handle flight status changes
    this.connection.on(
      "FlightStatusChanged",
      (flight: FlightDto, oldStatus: string, newStatus: string) => {
        console.log(
          `📊 Real-time: Flight ${flight.flightNumber} status: ${oldStatus} → ${newStatus}`
        )
        this.onFlightStatusChangedCallback?.(flight, oldStatus, newStatus)
      }
    )

    // Handle connection events
    this.connection.onreconnecting(() => {
      console.log("🔄 SignalR reconnecting - NEURAL_LINK_UNSTABLE")
      this.onConnectionStatusChangedCallback?.(false)
    })

    this.connection.onreconnected(() => {
      console.log("✅ SignalR reconnected - NEURAL_LINK_RESTORED")
      this.onConnectionStatusChangedCallback?.(true)
    })

    this.connection.onclose(() => {
      console.log("❌ SignalR connection closed - NEURAL_LINK_SEVERED")
      this.onConnectionStatusChangedCallback?.(false)
    })
  }

  // Cleanup
  dispose(): void {
    this.stop()
  }
}

// Export singleton instance
export const signalRService = new SignalRService()
