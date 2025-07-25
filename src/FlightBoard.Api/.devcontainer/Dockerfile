# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install additional tools
RUN apt-get update && apt-get install -y \
    curl \
    git \
    wget \
    unzip \
    sqlite3 \
    && rm -rf /var/lib/apt/lists/*

# Install Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Set working directory
WORKDIR /workspace

# Copy project files
COPY . .

# Restore dependencies
RUN dotnet restore src/FlightBoard.Api/FlightBoard.Api.csproj

# Expose ports
EXPOSE 5183 7022

# Set default command
CMD ["dotnet", "run", "--project", "src/FlightBoard.Api", "--urls", "http://0.0.0.0:5183"]
