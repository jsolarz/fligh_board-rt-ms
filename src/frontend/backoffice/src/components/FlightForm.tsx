// filepath: d:\personal\fligh_board-rt-ms\src\frontend\flight-board-backoffice\src\components\FlightForm.tsx
// Flight form component for BBS terminal interface
// Comprehensive validation and creation/editing of flights

import React, { useState, useEffect } from "react"
import { useMutation } from "@tanstack/react-query"
import FlightApiService from "../services/flight-api.service"
import { invalidateFlightData } from "../config/query-client"
import {
  FlightDto,
  CreateFlightDto,
  UpdateFlightDto,
  FlightFormErrors,
} from "../types/flight.types"

interface FlightFormProps {
  flight?: FlightDto
  onSubmit: (flight: CreateFlightDto | UpdateFlightDto) => Promise<void>
  onCancel: () => void
  isLoading?: boolean
}

const FlightForm: React.FC<FlightFormProps> = ({
  flight,
  onSubmit,
  onCancel,
  isLoading,
}) => {
  // Form state
  const [formData, setFormData] = useState({
    flightNumber: "",
    airline: "",
    origin: "",
    destination: "",
    scheduledDeparture: "",
    scheduledArrival: "",
    status: "Scheduled",
    gate: "",
    terminal: "",
    aircraftType: "",
    remarks: "",
    delayMinutes: 0,
    type: "Departure",
  })

  const [errors, setErrors] = useState<FlightFormErrors>({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Initialize form data when editing
  useEffect(() => {
    if (flight) {
      setFormData({
        flightNumber: flight.flightNumber,
        airline: flight.airline,
        origin: flight.origin,
        destination: flight.destination,
        scheduledDeparture: flight.scheduledDeparture.slice(0, 16), // Format for datetime-local
        scheduledArrival: flight.scheduledArrival.slice(0, 16),
        status: flight.status,
        gate: flight.gate || "",
        terminal: flight.terminal || "",
        aircraftType: flight.aircraftType || "",
        remarks: flight.remarks || "",
        delayMinutes: flight.delayMinutes,
        type: flight.type,
      })
    }
  }, [flight])

  // Create/Update mutations
  const createMutation = useMutation({
    mutationFn: FlightApiService.createFlight,
    onSuccess: () => {
      invalidateFlightData()
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateFlightDto }) =>
      FlightApiService.updateFlight(id, data),
    onSuccess: () => {
      invalidateFlightData()
    },
  })

  // Validation function
  const validateForm = (): boolean => {
    const newErrors: FlightFormErrors = {}

    // Required field validations
    if (!formData.flightNumber.trim()) {
      newErrors.flightNumber = "FLIGHT_NUMBER_REQUIRED"
    } else if (
      !/^[A-Z]{2,3}\d{1,4}[A-Z]?$/i.test(formData.flightNumber.trim())
    ) {
      newErrors.flightNumber =
        "INVALID_FLIGHT_NUMBER_FORMAT (e.g., AA123, BA456A)"
    }

    if (!formData.airline.trim()) {
      newErrors.airline = "AIRLINE_REQUIRED"
    } else if (formData.airline.trim().length < 2) {
      newErrors.airline = "AIRLINE_TOO_SHORT (minimum 2 characters)"
    }

    if (!formData.origin.trim()) {
      newErrors.origin = "ORIGIN_AIRPORT_REQUIRED"
    } else if (!/^[A-Z]{3}$/i.test(formData.origin.trim())) {
      newErrors.origin = "INVALID_AIRPORT_CODE (3 letters, e.g., JFK)"
    }

    if (!formData.destination.trim()) {
      newErrors.destination = "DESTINATION_AIRPORT_REQUIRED"
    } else if (!/^[A-Z]{3}$/i.test(formData.destination.trim())) {
      newErrors.destination = "INVALID_AIRPORT_CODE (3 letters, e.g., LAX)"
    }

    if (!formData.scheduledDeparture) {
      newErrors.scheduledDeparture = "DEPARTURE_TIME_REQUIRED"
    } else {
      const depTime = new Date(formData.scheduledDeparture)
      if (depTime <= new Date()) {
        newErrors.scheduledDeparture = "DEPARTURE_MUST_BE_IN_FUTURE"
      }
    }

    if (!formData.scheduledArrival) {
      newErrors.scheduledArrival = "ARRIVAL_TIME_REQUIRED"
    } else {
      const arrTime = new Date(formData.scheduledArrival)
      const depTime = new Date(formData.scheduledDeparture)
      if (arrTime <= depTime) {
        newErrors.scheduledArrival = "ARRIVAL_MUST_BE_AFTER_DEPARTURE"
      }
    }

    // Optional field validations
    if (formData.gate && !/^[A-Z]?\d{1,3}[A-Z]?$/i.test(formData.gate.trim())) {
      newErrors.gate = "INVALID_GATE_FORMAT (e.g., A12, 23, B15A)"
    }

    if (
      formData.terminal &&
      !/^[A-Z0-9]{1,3}$/i.test(formData.terminal.trim())
    ) {
      newErrors.terminal = "INVALID_TERMINAL_FORMAT (e.g., A, B, 1, T1)"
    }

    // Same origin/destination check
    if (
      formData.origin.trim().toUpperCase() ===
      formData.destination.trim().toUpperCase()
    ) {
      newErrors.origin = "ORIGIN_AND_DESTINATION_CANNOT_BE_SAME"
      newErrors.destination = "ORIGIN_AND_DESTINATION_CANNOT_BE_SAME"
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  // Handle input changes
  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => {
    const { name, value } = e.target
    setFormData((prev) => ({
      ...prev,
      [name]: name === "delayMinutes" ? parseInt(value) || 0 : value,
    }))

    // Clear error when user starts typing
    if (errors[name as keyof FlightFormErrors]) {
      setErrors((prev) => ({
        ...prev,
        [name]: undefined,
      }))
    }
  }

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!validateForm()) {
      return
    }

    setIsSubmitting(true)

    try {
      // Prepare data for submission
      const submitData = {
        ...formData,
        flightNumber: formData.flightNumber.trim().toUpperCase(),
        airline: formData.airline.trim(),
        origin: formData.origin.trim().toUpperCase(),
        destination: formData.destination.trim().toUpperCase(),
        gate: formData.gate.trim() || undefined,
        terminal: formData.terminal.trim() || undefined,
        aircraftType: formData.aircraftType.trim() || undefined,
        remarks: formData.remarks.trim() || undefined,
        scheduledDeparture: new Date(formData.scheduledDeparture).toISOString(),
        scheduledArrival: new Date(formData.scheduledArrival).toISOString(),
      }

      if (flight) {
        // Update existing flight
        await updateMutation.mutateAsync({ id: flight.id, data: submitData })
      } else {
        // Create new flight
        await createMutation.mutateAsync(submitData as CreateFlightDto)
      }

      await onSubmit(submitData)
    } catch (error) {
      console.error("[ADMIN] Form submission failed:", error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const isFormLoading =
    isLoading ||
    isSubmitting ||
    createMutation.isPending ||
    updateMutation.isPending

  return (
    <div className="flight-form">
      <form onSubmit={handleSubmit}>
        {/* Required Fields Section */}
        <fieldset className="form-section">
          <legend className="terminal-prompt">REQUIRED_FLIGHT_DATA</legend>

          <div className="form-row">
            <div className="form-field">
              <label className="terminal-prompt">FLIGHT_NUMBER:</label>
              <input
                type="text"
                name="flightNumber"
                value={formData.flightNumber}
                onChange={handleChange}
                placeholder="AA123, BA456A"
                maxLength={10}
                disabled={isFormLoading}
              />
              {errors.flightNumber && (
                <div className="error">{errors.flightNumber}</div>
              )}
            </div>

            <div className="form-field">
              <label className="terminal-prompt">AIRLINE:</label>
              <input
                type="text"
                name="airline"
                value={formData.airline}
                onChange={handleChange}
                placeholder="American Airlines"
                maxLength={50}
                disabled={isFormLoading}
              />
              {errors.airline && <div className="error">{errors.airline}</div>}
            </div>
          </div>

          <div className="form-row">
            <div className="form-field">
              <label className="terminal-prompt">ORIGIN_AIRPORT:</label>
              <input
                type="text"
                name="origin"
                value={formData.origin}
                onChange={handleChange}
                placeholder="JFK"
                maxLength={3}
                style={{ textTransform: "uppercase" }}
                disabled={isFormLoading}
              />
              {errors.origin && <div className="error">{errors.origin}</div>}
            </div>

            <div className="form-field">
              <label className="terminal-prompt">DESTINATION_AIRPORT:</label>
              <input
                type="text"
                name="destination"
                value={formData.destination}
                onChange={handleChange}
                placeholder="LAX"
                maxLength={3}
                style={{ textTransform: "uppercase" }}
                disabled={isFormLoading}
              />
              {errors.destination && (
                <div className="error">{errors.destination}</div>
              )}
            </div>
          </div>

          <div className="form-row">
            <div className="form-field">
              <label className="terminal-prompt">SCHEDULED_DEPARTURE:</label>
              <input
                type="datetime-local"
                name="scheduledDeparture"
                value={formData.scheduledDeparture}
                onChange={handleChange}
                disabled={isFormLoading}
              />
              {errors.scheduledDeparture && (
                <div className="error">{errors.scheduledDeparture}</div>
              )}
            </div>

            <div className="form-field">
              <label className="terminal-prompt">SCHEDULED_ARRIVAL:</label>
              <input
                type="datetime-local"
                name="scheduledArrival"
                value={formData.scheduledArrival}
                onChange={handleChange}
                disabled={isFormLoading}
              />
              {errors.scheduledArrival && (
                <div className="error">{errors.scheduledArrival}</div>
              )}
            </div>
          </div>
        </fieldset>

        {/* Optional Fields Section */}
        <fieldset className="form-section">
          <legend className="terminal-prompt">OPTIONAL_FLIGHT_DATA</legend>

          <div className="form-row">
            <div className="form-field">
              <label className="terminal-prompt">STATUS:</label>
              <select
                name="status"
                value={formData.status}
                onChange={handleChange}
                disabled={isFormLoading}
              >
                <option value="Scheduled">SCHEDULED</option>
                <option value="Boarding">BOARDING</option>
                <option value="Departed">DEPARTED</option>
                <option value="Delayed">DELAYED</option>
                <option value="Cancelled">CANCELLED</option>
                <option value="On Time">ON_TIME</option>
              </select>
            </div>

            <div className="form-field">
              <label className="terminal-prompt">FLIGHT_TYPE:</label>
              <select
                name="type"
                value={formData.type}
                onChange={handleChange}
                disabled={isFormLoading}
              >
                <option value="Departure">DEPARTURE</option>
                <option value="Arrival">ARRIVAL</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-field">
              <label className="terminal-prompt">GATE:</label>
              <input
                type="text"
                name="gate"
                value={formData.gate}
                onChange={handleChange}
                placeholder="A12, 23, B15A"
                maxLength={5}
                disabled={isFormLoading}
              />
              {errors.gate && <div className="error">{errors.gate}</div>}
            </div>

            <div className="form-field">
              <label className="terminal-prompt">TERMINAL:</label>
              <input
                type="text"
                name="terminal"
                value={formData.terminal}
                onChange={handleChange}
                placeholder="A, B, 1, T1"
                maxLength={3}
                disabled={isFormLoading}
              />
              {errors.terminal && (
                <div className="error">{errors.terminal}</div>
              )}
            </div>
          </div>

          <div className="form-row">
            <div className="form-field">
              <label className="terminal-prompt">AIRCRAFT_TYPE:</label>
              <input
                type="text"
                name="aircraftType"
                value={formData.aircraftType}
                onChange={handleChange}
                placeholder="Boeing 737, Airbus A320"
                maxLength={50}
                disabled={isFormLoading}
              />
            </div>

            <div className="form-field">
              <label className="terminal-prompt">DELAY_MINUTES:</label>
              <input
                type="number"
                name="delayMinutes"
                value={formData.delayMinutes}
                onChange={handleChange}
                min="0"
                max="1440"
                disabled={isFormLoading}
              />
            </div>
          </div>

          <div className="form-field">
            <label className="terminal-prompt">REMARKS:</label>
            <textarea
              name="remarks"
              value={formData.remarks}
              onChange={handleChange}
              placeholder="Additional flight information..."
              rows={3}
              maxLength={500}
              disabled={isFormLoading}
            />
          </div>
        </fieldset>

        {/* Form Actions */}
        <div className="form-actions mt-3">
          <button type="submit" className="primary" disabled={isFormLoading}>
            {isFormLoading
              ? "PROCESSING..."
              : flight
              ? "UPDATE_FLIGHT"
              : "CREATE_FLIGHT"}
          </button>
          <button type="button" onClick={onCancel} disabled={isFormLoading}>
            CANCEL_OPERATION
          </button>
        </div>

        {/* Error Display */}
        {(createMutation.error || updateMutation.error) && (
          <div className="error mt-2">
            <span className="terminal-prompt">OPERATION_FAILED:</span>
            <br />
            {createMutation.error?.message || updateMutation.error?.message}
          </div>
        )}
      </form>
    </div>
  )
}

export default FlightForm
