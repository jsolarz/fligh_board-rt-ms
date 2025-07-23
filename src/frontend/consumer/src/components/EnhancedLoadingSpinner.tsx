// EnhancedLoadingSpinner - Quantum-enhanced loading with matrix effects
import React from 'react';

interface EnhancedLoadingSpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  variant?: 'quantum' | 'matrix' | 'neural';
  message?: string;
}

const EnhancedLoadingSpinner: React.FC<EnhancedLoadingSpinnerProps> = ({
  size = 'md',
  variant = 'quantum',
  message = 'LOADING_DATA_STREAMS...'
}) => {
  const sizeClasses = {
    sm: 'w-8 h-8',
    md: 'w-12 h-12',
    lg: 'w-16 h-16'
  };

  const renderQuantumSpinner = () => (
    <div className={`quantum-loading ${sizeClasses[size]} relative`}>
      <div className="absolute inset-0 border-2 border-transparent border-t-neon-cyan border-r-neon-magenta rounded-full animate-spin"></div>
      <div className="absolute inset-1 border-2 border-transparent border-b-neon-green border-l-neon-yellow rounded-full animate-spin animation-direction-reverse"></div>
      <div className="absolute inset-2 border border-transparent border-t-neon-cyan rounded-full animate-ping"></div>

      {/* Quantum particles */}
      <div className="quantum-particle"></div>
      <div className="quantum-particle"></div>
      <div className="quantum-particle"></div>
      <div className="quantum-particle"></div>
    </div>
  );

  const renderMatrixSpinner = () => (
    <div className={`matrix-loading ${sizeClasses[size]} relative`}>
      <div className="matrix-rain absolute inset-0"></div>
      <div className="absolute inset-0 flex items-center justify-center">
        <div className="text-neon-green font-mono text-xs animate-pulse">
          {Array.from('01100010011').map((bit, index) => (
            <span
              key={index}
              className="inline-block animate-bounce"
              style={{ animationDelay: `${index * 0.1}s` }}
            >
              {bit}
            </span>
          ))}
        </div>
      </div>
    </div>
  );

  const renderNeuralSpinner = () => (
    <div className={`neural-loading ${sizeClasses[size]} relative`}>
      <div className="neural-connection absolute inset-0 rounded-full border border-neon-cyan/30">
        <div className="absolute top-1/2 left-1/2 w-2 h-2 -mt-1 -ml-1 bg-neon-cyan rounded-full animate-ping"></div>
      </div>
      <div className="neural-connection absolute inset-2 rounded-full border border-neon-magenta/30 rotate-45">
        <div className="absolute top-1/2 left-1/2 w-1 h-1 -mt-0.5 -ml-0.5 bg-neon-magenta rounded-full animate-pulse"></div>
      </div>
      <div className="neural-connection absolute inset-4 rounded-full border border-neon-green/30 rotate-90">
        <div className="absolute top-1/2 left-1/2 w-1 h-1 -mt-0.5 -ml-0.5 bg-neon-green rounded-full animate-bounce"></div>
      </div>
    </div>
  );

  const renderSpinner = () => {
    switch (variant) {
      case 'matrix':
        return renderMatrixSpinner();
      case 'neural':
        return renderNeuralSpinner();
      default:
        return renderQuantumSpinner();
    }
  };

  return (
    <div className="enhanced-loading-container flex flex-col items-center justify-center space-y-4 p-8">
      {/* Main Spinner */}
      <div className="relative">
        {renderSpinner()}

        {/* Energy field effect */}
        <div className="absolute inset-0 rounded-full border border-neon-cyan/20 animate-ping animation-delay-300"></div>
        <div className="absolute inset-0 rounded-full border border-neon-magenta/10 animate-ping animation-delay-600"></div>
      </div>

      {/* Loading Message */}
      <div className="loading-message text-center">
        <div className="text-neon-cyan font-mono text-sm mb-2">
          {message}
        </div>

        {/* Progress Bar */}
        <div className="loading-progress w-64 h-1 bg-gray-800 rounded-full overflow-hidden">
          <div className="h-full bg-gradient-to-r from-neon-cyan via-neon-magenta to-neon-green animate-pulse"></div>
        </div>

        {/* Status Indicators */}
        <div className="status-indicators flex justify-center space-x-4 mt-3">
          <div className="flex items-center space-x-1">
            <div className="w-2 h-2 rounded-full bg-neon-green animate-pulse"></div>
            <span className="text-xs text-neon-green font-mono">API_CONNECTED</span>
          </div>
          <div className="flex items-center space-x-1">
            <div className="w-2 h-2 rounded-full bg-neon-cyan animate-ping"></div>
            <span className="text-xs text-neon-cyan font-mono">DATA_PROCESSING</span>
          </div>
          <div className="flex items-center space-x-1">
            <div className="w-2 h-2 rounded-full bg-neon-yellow animate-bounce"></div>
            <span className="text-xs text-neon-yellow font-mono">RENDERING</span>
          </div>
        </div>
      </div>

      {/* Data Stream Visualization */}
      <div className="data-stream-viz flex space-x-1 mt-4">
        {Array.from({ length: 20 }).map((_, index) => (
          <div
            key={index}
            className="w-1 bg-neon-cyan/30 rounded-full data-stream"
            style={{
              height: `${Math.random() * 20 + 10}px`,
              animationDelay: `${index * 0.1}s`
            }}
          ></div>
        ))}
      </div>

      {/* Loading Tips */}
      <div className="loading-tips text-xs text-neon-cyan/60 font-mono text-center max-w-md">
        <div className="animate-pulse">
          TIP: Real-time updates are active. Flight data streams directly from the neural network.
        </div>
      </div>
    </div>
  );
};

export default EnhancedLoadingSpinner;
