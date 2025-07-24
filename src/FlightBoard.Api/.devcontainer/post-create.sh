#!/bin/bash

echo "🚀 Setting up FlightBoard API Development Environment..."

# Ensure we're in the right directory
cd /workspace

# Create data directory if it doesn't exist
mkdir -p data

# Install/update EF Core tools
dotnet tool update --global dotnet-ef

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore src/FlightBoard.Api/FlightBoard.Api.csproj

# Check if database exists, if not create it
if [ ! -f "data/flightboard.db" ]; then
    echo "🗄️ Creating database..."
    dotnet ef database update --project src/FlightBoard.Api
else
    echo "✅ Database already exists"
fi

# Build the project
echo "🔨 Building FlightBoard API..."
dotnet build src/FlightBoard.Api/FlightBoard.Api.csproj

echo "✅ FlightBoard API development environment ready!"
echo ""
echo "📋 Available commands:"
echo "  • dotnet run --project src/FlightBoard.Api --urls http://0.0.0.0:5183"
echo "  • dotnet run --project src/FlightBoard.Api --urls https://0.0.0.0:7022"
echo "  • dotnet test"
echo "  • dotnet ef migrations add <MigrationName> --project src/FlightBoard.Api"
echo "  • dotnet ef database update --project src/FlightBoard.Api"
echo ""
echo "🌐 API will be available at:"
echo "  • HTTP: http://localhost:5183"
echo "  • HTTPS: https://localhost:7022"
echo "  • Swagger: http://localhost:5183/swagger"
