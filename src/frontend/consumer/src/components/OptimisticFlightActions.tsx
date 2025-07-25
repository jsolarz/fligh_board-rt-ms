// OptimisticFlightActions - Enhanced flight actions with optimistic updates
import React, { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FlightApiService } from '../services/flight-api.service';
import { FlightDto } from '../types/flight.types';

interface OptimisticFlightActionsProps {
  flight: FlightDto;
  onUpdate?: (flight: FlightDto) => void;
}

const OptimisticFlightActions: React.FC<OptimisticFlightActionsProps> = ({
  flight,
  onUpdate
}) => {
  const [isOptimisticUpdate, setIsOptimisticUpdate] = useState(false);
  const queryClient = useQueryClient();

  // Optimistic status update mutation
  const statusUpdateMutation = useMutation({
    mutationFn: async (newStatus: string) => {
      // Create optimistic update data
      const optimisticFlight = { ...flight, status: newStatus };

      // Show immediate visual feedback
      setIsOptimisticUpdate(true);
      onUpdate?.(optimisticFlight);

      // Trigger visual effects
      triggerStatusUpdateEffect();

      // Call actual API
      return FlightApiService.updateFlightStatus(flight.id, { status: newStatus });
    },
    onSuccess: (updatedFlight) => {
      // Real update received, update cache
      queryClient.setQueryData(['flights'], (oldData: any) => {
        if (!oldData) return oldData;

        return {
          ...oldData,
          data: oldData.data.map((f: FlightDto) =>
            f.id === flight.id ? updatedFlight : f
          )
        };
      });

      setIsOptimisticUpdate(false);
      onUpdate?.(updatedFlight);

      // Show success notification
      showUpdateNotification('Status updated successfully', 'success');
    },
    onError: (error) => {
      // Revert optimistic update
      setIsOptimisticUpdate(false);
      onUpdate?.(flight); // Revert to original

      // Show error notification
      showUpdateNotification('Failed to update status', 'error');
      console.error('Status update failed:', error);
    }
  });

  // Optimistic gate update mutation
  const gateUpdateMutation = useMutation({
    mutationFn: async (newGate: string) => {
      const optimisticFlight = { ...flight, gate: newGate };
      setIsOptimisticUpdate(true);
      onUpdate?.(optimisticFlight);

      triggerGateUpdateEffect();

      return FlightApiService.updateFlight(flight.id, { gate: newGate });
    },
    onSuccess: (updatedFlight) => {
      queryClient.invalidateQueries({ queryKey: ['flights'] });
      setIsOptimisticUpdate(false);
      onUpdate?.(updatedFlight);
      showUpdateNotification('Gate updated successfully', 'success');
    },
    onError: (error) => {
      setIsOptimisticUpdate(false);
      onUpdate?.(flight);
      showUpdateNotification('Failed to update gate', 'error');
      console.error('Gate update failed:', error);
    }
  });

  // Visual effect triggers
  const triggerStatusUpdateEffect = () => {
    const element = document.querySelector(`[data-flight-id="${flight.id}"]`);
    if (element) {
      element.classList.add('holo-glitch');
      setTimeout(() => element.classList.remove('holo-glitch'), 300);
    }
  };

  const triggerGateUpdateEffect = () => {
    const element = document.querySelector(`[data-flight-id="${flight.id}"] .gate-info`);
    if (element) {
      element.classList.add('data-stream');
      setTimeout(() => element.classList.remove('data-stream'), 2000);
    }
  };

  // Show update notification
  const showUpdateNotification = (message: string, type: 'success' | 'error') => {
    const notification = document.createElement('div');
    notification.className = `update-notification ${type === 'error' ? 'error' : 'success'}`;
    notification.innerHTML = `
      <div class="flex items-center space-x-2">
        <span class="text-sm font-mono">${type === 'error' ? '✗' : '✓'}</span>
        <span class="text-sm">${message}</span>
      </div>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
      notification.remove();
    }, 4000);
  };

  // Quick status change buttons
  const statusOptions = [
    { value: 'Scheduled', label: 'SCHEDULED', color: 'status-scheduled' },
    { value: 'Boarding', label: 'BOARDING', color: 'status-boarding' },
    { value: 'Delayed', label: 'DELAYED', color: 'status-delayed' },
    { value: 'Departed', label: 'DEPARTED', color: 'status-departed' }
  ];

  return (
    <div className="optimistic-actions flex flex-col space-y-2">
      {/* Quick Status Updates */}
      <div className="status-quick-actions">
        <label className="text-xs text-neon-cyan mb-1 block font-mono">QUICK_STATUS_UPDATE:</label>
        <div className="flex space-x-1">
          {statusOptions.map((status) => (
            <button
              key={status.value}
              className={`
                px-2 py-1 text-xs font-mono border rounded
                transition-all duration-200 hover:scale-105
                ${flight.status === status.value
                  ? `${status.color} border-current`
                  : 'border-neon-cyan/30 text-neon-cyan/70 hover:text-neon-cyan'
                }
                ${isOptimisticUpdate ? 'opacity-50 cursor-wait' : 'cursor-pointer'}
              `}
              onClick={() => statusUpdateMutation.mutate(status.value)}
              disabled={isOptimisticUpdate || statusUpdateMutation.isPending}
            >
              {status.label}
            </button>
          ))}
        </div>
      </div>

      {/* Gate Update */}
      <div className="gate-quick-update">
        <label className="text-xs text-neon-cyan mb-1 block font-mono">GATE_UPDATE:</label>
        <div className="flex space-x-2">
          <input
            type="text"
            placeholder="A12"
            className="
              px-2 py-1 text-xs bg-transparent border border-neon-cyan/30
              text-neon-cyan font-mono rounded w-16
              focus:border-neon-cyan focus:outline-none
              terminal-input
            "
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                const newGate = (e.target as HTMLInputElement).value.trim();
                if (newGate) {
                  gateUpdateMutation.mutate(newGate);
                  (e.target as HTMLInputElement).value = '';
                }
              }
            }}
            disabled={isOptimisticUpdate || gateUpdateMutation.isPending}
          />
          <div className="text-xs text-neon-cyan/50 font-mono">
            {isOptimisticUpdate ? 'UPDATING...' : 'PRESS_ENTER'}
          </div>
        </div>
      </div>

      {/* Update Status Indicator */}
      {(statusUpdateMutation.isPending || gateUpdateMutation.isPending) && (
        <div className="flex items-center space-x-2 text-xs text-neon-yellow">
          <div className="quantum-loading w-3 h-3"></div>
          <span className="font-mono">TRANSMITTING_DATA...</span>
        </div>
      )}

      {/* Optimistic Update Indicator */}
      {isOptimisticUpdate && (
        <div className="flex items-center space-x-2 text-xs text-neon-cyan">
          <span className="animate-pulse">⟡</span>
          <span className="font-mono">OPTIMISTIC_UPDATE_ACTIVE</span>
        </div>
      )}
    </div>
  );
};

export default OptimisticFlightActions;
