#!/bin/bash
# FlightBoard Quick Deploy
# Run from project root: ./quick-deploy.sh

set -e

echo "🚀 FlightBoard Quick Deploy"
echo "=========================="

# Get script directory and project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR" && pwd)"

# Check if we're in the right directory
if [ ! -f "$PROJECT_ROOT/objectives.md" ]; then
    echo "❌ Error: Please run this script from the FlightBoard project root directory"
    exit 1
fi

echo "📍 Project root: $PROJECT_ROOT"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Create necessary directories
echo "📁 Setting up directory structure..."
mkdir -p data
mkdir -p infra/docker
chmod 755 data

# Change to docker directory
cd "$PROJECT_ROOT/infra/docker"

# Check if .env file exists
if [ ! -f .env ]; then
    echo "📝 Creating .env file from template..."
    if [ -f .env.example ]; then
        cp .env.example .env
        echo "⚠️  Created .env file. Please edit it with your configuration."
        echo "   You can continue with defaults for local development."
        read -p "Continue with default settings? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "Please edit infra/docker/.env and run this script again."
            exit 1
        fi
    else
        echo "❌ .env.example not found. Creating basic .env file..."
        cat > .env << 'EOF'
# Basic configuration for local development
JWT_SECRET=DevSecretKeyForLocalDevelopment2024!
JWT_ISSUER=FlightBoard.Api
JWT_AUDIENCE=FlightBoard.Frontend
JWT_ACCESS_EXPIRY=15
JWT_REFRESH_EXPIRY=7
DB_VOLUME_PATH=../../data
API_PUBLIC_URL=http://localhost:5183
SIGNALR_PUBLIC_URL=http://localhost:5183/flighthub
LOG_LEVEL=Information
EOF
    fi
fi

echo "🔨 Building and starting FlightBoard services..."

# Clean up any existing containers
echo "🧹 Cleaning up existing containers..."
docker-compose down --remove-orphans 2>/dev/null || true

# Build and start services
echo "🏗️  Building services..."
docker-compose build --no-cache

echo "🚀 Starting services..."
docker-compose up -d

# Wait for services to be ready
echo "⏳ Waiting for services to be ready..."
sleep 15

# Check service health
echo "🔍 Checking service health..."
docker-compose ps

# Display logs
echo "📋 Recent service logs:"
docker-compose logs --tail=5

# Final status
echo ""
echo "✅ FlightBoard deployment completed successfully!"
echo ""
echo "🌐 Access your applications:"
echo "   • API Documentation: http://localhost:5183/swagger"
echo "   • API Health Check:  http://localhost:5183/health"
echo "   • Consumer App:      http://localhost:3000"
echo "   • Backoffice App:    http://localhost:3001"
echo ""
echo "🔧 Useful commands:"
echo "   • View logs:         cd infra/docker && docker-compose logs -f"
echo "   • Stop services:     cd infra/docker && docker-compose down"
echo "   • Restart services:  cd infra/docker && docker-compose restart"
echo ""
echo "📁 Project structure:"
echo "   • Source code:       src/"
echo "   • Infrastructure:    infra/"
echo "   • Database:          data/"
