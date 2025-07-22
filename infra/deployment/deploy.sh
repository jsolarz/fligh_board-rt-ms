#!/bin/bash
# Deployment script for FlightBoard system

set -e

echo "🚀 Starting FlightBoard deployment..."

# Change to docker directory
cd "$(dirname "$0")"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if .env file exists
if [ ! -f .env ]; then
    echo "📝 Creating .env file from template..."
    cp .env.example .env
    echo "⚠️  Please edit .env file with your configuration before running again."
    exit 1
fi

# Create data directory if it doesn't exist
echo "📁 Creating data directory..."
mkdir -p ../../data
chmod 755 ../../data

# Build and start services
echo "🔨 Building and starting services..."
docker-compose down --remove-orphans
docker-compose pull
docker-compose build --no-cache
docker-compose up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
sleep 10

# Check service status
echo "🔍 Checking service status..."
docker-compose ps

# Show logs
echo "📋 Recent logs:"
docker-compose logs --tail=10

echo "✅ Deployment completed!"
echo ""
echo "🌐 Services available at:"
echo "  - API: http://localhost:5183"
echo "  - Consumer Frontend: http://localhost:3000"
echo "  - Backoffice Frontend: http://localhost:3001"
echo ""
echo "🔧 Management commands:"
echo "  - View logs: docker-compose logs -f"
echo "  - Stop services: docker-compose down"
echo "  - Restart services: docker-compose restart"
