#!/bin/bash

echo "🚀 Setting up FlightBoard Consumer Frontend Development Environment..."

# Ensure we're in the right directory
cd /workspace

# Install dependencies
echo "📦 Installing npm dependencies..."
npm install

# Check if .env file exists, create one if not
if [ ! -f ".env" ]; then
    echo "📝 Creating .env file..."
    cat > .env << EOL
# FlightBoard Consumer App Environment Variables
# API Configuration - Auto-detection enabled
# REACT_APP_API_URL=http://localhost:5183
# REACT_APP_SIGNALR_URL=http://localhost:5183/flighthub

# Development Configuration
REACT_APP_ENV=development
REACT_APP_DEBUG=true

# Docker/Container specific
CHOKIDAR_USEPOLLING=true
WDS_SOCKET_HOST=0.0.0.0
EOL
    echo "✅ .env file created"
else
    echo "✅ .env file already exists"
fi

# Run linting to check for issues
echo "🔍 Running ESLint..."
npm run lint --if-present || echo "⚠️ ESLint not configured or has issues"

# Check if build works
echo "🔨 Testing build..."
npm run build

echo "✅ FlightBoard Consumer Frontend development environment ready!"
echo ""
echo "📋 Available commands:"
echo "  • npm start - Start development server"
echo "  • npm run build - Build for production"
echo "  • npm test - Run tests"
echo "  • npm run lint - Run ESLint"
echo ""
echo "🌐 App will be available at:"
echo "  • Development: http://localhost:3000"
echo "  • With auto-detection, it will connect to API at:"
echo "    - http://localhost:5183 (if accessed via HTTP)"
echo "    - https://localhost:7022 (if accessed via HTTPS)"
