// ErrorAlert component - Cyberpunk neural error interface
// System failure notification with retro-futuristic styling

import React from "react"

interface ErrorAlertProps {
  message: string
  onRetry?: () => void
  type?: "error" | "warning" | "info"
}

const ErrorAlert: React.FC<ErrorAlertProps> = ({
  message,
  onRetry,
  type = "error",
}) => {
  const typeClasses = {
    error: "border-neon-red bg-neon-red/10",
    warning: "border-neon-yellow bg-neon-yellow/10",
    info: "border-neon-cyan bg-neon-cyan/10",
  }

  const textClasses = {
    error: "text-neon-red",
    warning: "text-neon-yellow",
    info: "text-neon-cyan",
  }

  const iconClasses = {
    error: "text-neon-red",
    warning: "text-neon-yellow",
    info: "text-neon-cyan",
  }

  return (
    <div
      className={`holographic border rounded-lg p-6 ${typeClasses[type]} animate-slide-in`}
    >
      <div className="flex items-start space-x-4">
        <div className="flex-shrink-0">
          {type === "error" && (
            <div className="relative">
              <svg
                className={`h-8 w-8 ${iconClasses[type]} animate-pulse`}
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clipRule="evenodd"
                />
              </svg>
              <div className="absolute inset-0 h-8 w-8 bg-neon-red rounded-full animate-ping opacity-20"></div>
            </div>
          )}
          {type === "warning" && (
            <div className="relative">
              <svg
                className={`h-8 w-8 ${iconClasses[type]} animate-pulse`}
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                  clipRule="evenodd"
                />
              </svg>
              <div className="absolute inset-0 h-8 w-8 bg-neon-yellow rounded-full animate-ping opacity-20"></div>
            </div>
          )}
          {type === "info" && (
            <div className="relative">
              <svg
                className={`h-8 w-8 ${iconClasses[type]} animate-pulse`}
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                  clipRule="evenodd"
                />
              </svg>
              <div className="absolute inset-0 h-8 w-8 bg-neon-cyan rounded-full animate-ping opacity-20"></div>
            </div>
          )}
        </div>

        <div className="flex-1 space-y-3">
          <div className="flex items-center space-x-2">
            <div
              className={`w-2 h-2 rounded-full ${textClasses[type]} animate-pulse`}
            ></div>
            <h3
              className={`text-lg font-mono font-bold uppercase tracking-wider ${textClasses[type]} neon-text`}
            >
              {type === "error" && "SYSTEM_ERROR_DETECTED"}
              {type === "warning" && "WARNING_PROTOCOL_ACTIVE"}
              {type === "info" && "NEURAL_NOTIFICATION"}
            </h3>
          </div>

          <div
            className={`font-mono text-sm ${textClasses[type]} leading-relaxed`}
          >
            <div className="mb-2 text-xs uppercase tracking-wider opacity-80">
              ERROR_MSG:
            </div>
            <div className="pl-4 border-l-2 border-current/30">{message}</div>
          </div>

          {onRetry && (
            <div className="flex space-x-3 pt-2">
              <button
                onClick={onRetry}
                className={`cyber-button text-sm ${textClasses[type]} border-current hover:bg-current/10 hover:shadow-neon-sm`}
              >
                RETRY_CONNECTION
              </button>
              <div className="flex items-center space-x-2 text-xs font-mono opacity-60">
                <div className="w-1 h-1 bg-current rounded-full animate-ping"></div>
                <span>ATTEMPTING_RECOVERY...</span>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Scanline effect */}
      <div className="absolute inset-0 bg-gradient-to-b from-transparent via-current/5 to-transparent opacity-30 animate-scanlines pointer-events-none"></div>
    </div>
  )
}

export default ErrorAlert
