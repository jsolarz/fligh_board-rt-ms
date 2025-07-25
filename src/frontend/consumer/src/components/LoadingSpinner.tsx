// LoadingSpinner component - Cyberpunk neural processing indicator
// Futuristic loading states with glitch effects

import React from "react"

interface LoadingSpinnerProps {
  message?: string
  size?: "small" | "medium" | "large"
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  message = "PROCESSING_DATA...",
  size = "medium",
}) => {
  const sizeClasses = {
    small: "h-4 w-4",
    medium: "h-8 w-8",
    large: "h-12 w-12",
  }

  return (
    <div className="flex flex-col items-center justify-center p-8 space-y-4">
      {/* Cyberpunk spinner */}
      <div className="relative">
        <div
          className={`cyber-spinner ${sizeClasses[size]} animate-spin`}
        ></div>
        {/* Additional rotating ring */}
        <div
          className={`absolute inset-0 ${sizeClasses[size]} border-2 border-transparent border-t-neon-purple animate-spin`}
          style={{ animationDirection: "reverse", animationDuration: "1.5s" }}
        ></div>
        {/* Center glow */}
        <div
          className={`absolute inset-2 bg-neon-cyan/20 rounded-full animate-pulse`}
        ></div>
      </div>

      {/* Loading message with glitch effect */}
      {message && (
        <div className="text-center">
          <p className="text-neon-cyan text-sm font-mono tracking-wider animate-pulse">
            {message}
          </p>
          {/* Progress dots */}
          <div className="flex justify-center space-x-1 mt-2">
            {[...Array(3)].map((_, i) => (
              <div
                key={i}
                className="w-1 h-1 bg-neon-cyan rounded-full animate-pulse"
                style={{ animationDelay: `${i * 0.3}s` }}
              ></div>
            ))}
          </div>
        </div>
      )}

      {/* Data stream lines */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {[...Array(4)].map((_, i) => (
          <div
            key={i}
            className="absolute w-px h-8 bg-neon-cyan/30 animate-data-flow"
            style={{
              left: `${25 + i * 15}%`,
              top: "20%",
              animationDelay: `${i * 0.5}s`,
            }}
          ></div>
        ))}
      </div>
    </div>
  )
}

export default LoadingSpinner
