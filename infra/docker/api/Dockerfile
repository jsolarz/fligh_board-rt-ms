# API Dockerfile for FlightBoard API
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files for dependency resolution
COPY src/FlightBoard.Api/FlightBoard.Api.csproj ./src/FlightBoard.Api/
COPY src/FlightBoard.Tests/FlightBoard.Tests.csproj ./src/FlightBoard.Tests/
COPY src/FlightBoardSystem.sln ./src/

# Restore dependencies
RUN dotnet restore ./src/FlightBoardSystem.sln

# Copy source code
COPY src/ ./src/

# Build the application
RUN dotnet build ./src/FlightBoard.Api/FlightBoard.Api.csproj -c Release --no-restore

# Test stage (optional - can be commented out for faster builds)
FROM build AS test
RUN dotnet test ./src/FlightBoard.Tests/FlightBoard.Tests.csproj -c Release --no-build --verbosity normal

# Publish stage
FROM build AS publish
RUN dotnet publish ./src/FlightBoard.Api/FlightBoard.Api.csproj -c Release --no-build -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN addgroup --system --gid 1001 flightboard \
    && adduser --system --uid 1001 --gid 1001 flightboard

# Create data directory with proper permissions
RUN mkdir -p /app/data && chown -R flightboard:flightboard /app/data

# Copy published application
COPY --from=publish --chown=flightboard:flightboard /app/publish .

# Switch to non-root user
USER flightboard

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/flightboard.db"

# Start the application
ENTRYPOINT ["dotnet", "FlightBoard.Api.dll"]
