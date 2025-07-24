// filepath: d:\personal\fligh_board-rt-ms\src\frontend\flight-board-backoffice\src\components\FlightList.tsx
// Flight list component for BBS terminal interface
// Displays flights in a terminal-style table with admin actions

import React, { useState } from "react"
import { useQuery, useMutation } from "@tanstack/react-query"
import FlightApiService from "../services/flight-api.service"
import { QUERY_KEYS, invalidateFlightData } from "../config/query-client"
import { FlightDto } from "../types/flight.types"

interface FlightListProps {
  onEdit: (flight: FlightDto) => void
  onDelete: (flightId: number) => Promise<void>
}

const FlightList: React.FC<FlightListProps> = ({ onEdit, onDelete }) => {
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null)

  // Fetch flights with pagination
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: [...QUERY_KEYS.flights, { page, pageSize }],
    queryFn: () => FlightApiService.getFlights({ page, pageSize }),
  })

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: FlightApiService.deleteFlight,
    onSuccess: () => {
      invalidateFlightData()
      setDeleteConfirm(null)
    },
    onError: (error) => {
      console.error("[ADMIN] Delete failed:", error)
    },
  })

  // Handle delete confirmation
  const handleDeleteClick = (flightId: number) => {
    setDeleteConfirm(flightId)
  }

  // Handle confirmed delete
  const handleDeleteConfirm = async (flightId: number) => {
    try {
      await deleteMutation.mutateAsync(flightId)
      await onDelete(flightId)
    } catch (error) {
      console.error("[ADMIN] Delete operation failed:", error)
    }
  }

  // Handle delete cancellation
  const handleDeleteCancel = () => {
    setDeleteConfirm(null)
  }

  // Format date for terminal display
  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString("en-US", {
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    })
  }

  // Get status styling
  const getStatusClass = (status: string) => {
    switch (status.toLowerCase()) {
      case "on time":
      case "boarding":
      case "departed":
        return "on-time"
      case "delayed":
        return "delayed"
      case "cancelled":
        return "cancelled"
      default:
        return ""
    }
  }

  if (isLoading) {
    return (
      <div className="loading">
        <span className="terminal-prompt">LOADING_FLIGHT_DATABASE</span>
      </div>
    )
  }

  if (error) {
    return (
      <div className="error">
        <span className="terminal-prompt">DATABASE_ACCESS_ERROR:</span>
        <br />
        {error instanceof Error ? error.message : "Failed to load flight data"}
        <br />
        <button onClick={() => refetch()} className="mt-1">
          RETRY_CONNECTION
        </button>
      </div>
    )
  }

  if (!data || !data.data.length) {
    return (
      <div className="text-center p-2">
        <span className="terminal-prompt">NO_FLIGHT_RECORDS_FOUND</span>
        <br />
        <span>DATABASE_EMPTY - USE [2] ADD_NEW_FLIGHT TO CREATE RECORDS</span>
      </div>
    )
  }

  return (
    <div className="flight-list">
      {/* Data Summary */}
      <div className="data-summary mb-2">
        <span className="terminal-prompt">
          RECORDS_FOUND: {data.totalCount} | PAGE: {data.page}/{data.totalPages}{" "}
          | SHOWING: {data.data.length} ENTRIES
        </span>
      </div>

      {/* Flight Table */}
      <table className="flight-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>FLIGHT_NO</th>
            <th>AIRLINE</th>
            <th>ROUTE</th>
            <th>DEPARTURE</th>
            <th>STATUS</th>
            <th>GATE</th>
            <th>ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          {data.data.map((flight) => (
            <tr key={flight.id}>
              <td>{flight.id}</td>
              <td>{flight.flightNumber}</td>
              <td>{flight.airline}</td>
              <td>
                {flight.origin} → {flight.destination}
              </td>
              <td>{formatDateTime(flight.scheduledDeparture)}</td>
              <td>
                <span className={`status ${getStatusClass(flight.status)}`}>
                  {flight.status.toUpperCase()}
                </span>
              </td>
              <td>{flight.gate ?? "TBD"}</td>
              <td>
                <div className="action-buttons">
                  <button onClick={() => onEdit(flight)} title="Edit flight">
                    EDIT
                  </button>
                  <button
                    onClick={() => handleDeleteClick(flight.id)}
                    className="danger"
                    title="Delete flight"
                  >
                    DEL
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Delete Confirmation Modal */}
      {deleteConfirm && (
        <div className="delete-confirm-overlay">
          <div className="delete-confirm-modal">
            <h3>CONFIRM_DELETION</h3>
            <p className="terminal-prompt">DELETE_FLIGHT_ID: {deleteConfirm}</p>
            <p>WARNING: THIS_ACTION_CANNOT_BE_UNDONE</p>
            <div className="confirm-actions mt-2">
              <button
                onClick={() => handleDeleteConfirm(deleteConfirm)}
                className="danger"
                disabled={deleteMutation.isPending}
              >
                {deleteMutation.isPending ? "DELETING..." : "CONFIRM_DELETE"}
              </button>
              <button
                onClick={handleDeleteCancel}
                disabled={deleteMutation.isPending}
              >
                CANCEL
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Pagination */}
      {data.totalPages > 1 && (
        <div className="pagination mt-2">
          <span className="terminal-prompt">NAVIGATION:</span>
          <div className="pagination-buttons">
            <button
              onClick={() => setPage(1)}
              disabled={page === 1}
              title="Go to first page"
            >
              FIRST
            </button>
            <button
              onClick={() => setPage(page - 1)}
              disabled={!data.hasPrevious}
              title="Previous page"
            >
              PREV
            </button>
            <span className="page-info">
              PAGE {data.page} OF {data.totalPages}
            </span>
            <button 
              onClick={() => setPage(page + 1)} 
              disabled={!data.hasNext}
              title="Next page"
            >
              NEXT
            </button>
            <button
              onClick={() => setPage(data.totalPages)}
              disabled={page === data.totalPages}
              title="Go to last page"
            >
              LAST
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

export default FlightList
