#!/bin/bash

echo "🛠️ Setting up FlightBoard Backoffice Frontend Development Environment..."

# Ensure we're in the right directory
cd /workspace

# Install dependencies
echo "📦 Installing npm dependencies..."
npm install

# Check if .env file exists, create one if not
if [ ! -f ".env" ]; then
    echo "📝 Creating .env file..."
    cat > .env << EOL
# FlightBoard Backoffice App Environment Variables
# API Configuration - Auto-detection enabled
# REACT_APP_API_URL=http://localhost:5183
# REACT_APP_SIGNALR_URL=http://localhost:5183/flighthub

# Development Configuration
REACT_APP_ENV=development
REACT_APP_DEBUG=true

# Backoffice specific settings
REACT_APP_ADMIN_MODE=true
REACT_APP_TERMINAL_THEME=true

# Port configuration (runs on 3001 to avoid conflicts)
PORT=3001

# Docker/Container specific
CHOKIDAR_USEPOLLING=true
WDS_SOCKET_HOST=0.0.0.0
FAST_REFRESH=true
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

echo "✅ FlightBoard Backoffice Frontend development environment ready!"
echo ""
echo "📋 Available commands:"
echo "  • npm start - Start development server on port 3001"
echo "  • npm run build - Build for production"
echo "  • npm test - Run tests"
echo "  • npm run lint - Run ESLint"
echo ""
echo "🌐 Admin app will be available at:"
echo "  • Development: http://localhost:3001"
echo "  • Terminal theme enabled for admin interface"
echo "  • Auto-connects to API at http://localhost:5183"
echo ""
echo "🔧 Admin Features:"
echo "  • Flight management interface"
echo "  • Real-time flight updates"
echo "  • Terminal-style ASCII art headers"
echo "  • Administrative controls"
