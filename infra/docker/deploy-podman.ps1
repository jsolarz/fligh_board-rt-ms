# Podman Deployment Script for FlightBoard Application (PowerShell)
# Alternative to Docker with rootless containers and enhanced security

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("dev", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("up", "down", "build", "logs", "status", "clean")]
    [string]$Action
)

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[PODMAN] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Function to check if Podman is installed
function Test-Podman {
    Write-Status "Checking Podman installation..."
    
    try {
        $podmanVersion = podman --version 2>$null
        if (-not $podmanVersion) {
            throw "Podman not found"
        }
        Write-Success "Found: $podmanVersion"
    }
    catch {
        Write-Error "Podman is not installed or not in PATH!"
        Write-Host ""
        Write-Host "To install Podman:"
        Write-Host "1. Windows: https://github.com/containers/podman/releases"
        Write-Host "2. Or use winget: winget install -e --id RedHat.Podman"
        Write-Host "3. Linux: Use your package manager (dnf, apt, etc.)"
        Write-Host "4. macOS: brew install podman"
        Write-Host ""
        Write-Host "After installation, restart your terminal and run this script again."
        exit 1
    }
    
    # Check if podman-compose is available (optional but recommended)
    try {
        $composeVersion = podman-compose --version 2>$null
        if ($composeVersion) {
            Write-Success "Found podman-compose: $composeVersion"
            $script:UseComposeCompat = $true
        } else {
            Write-Warning "podman-compose not found, using podman play kube instead"
            $script:UseComposeCompat = $false
        }
    }
    catch {
        Write-Warning "podman-compose not available, using native Podman commands"
        $script:UseComposeCompat = $false
    }
    
    Write-Success "Podman is ready for deployment"
}

# Function to set environment variables
function Set-DeploymentEnvironment {
    param([string]$Env)
    
    switch ($Env) {
        "dev" {
            $script:ComposeFile = "docker-compose.dev.yml"
            $script:PodmanFile = "podman-compose.dev.yml"
            $script:EnvFile = ".env.dev"
            $script:PodName = "flightboard-dev"
            Write-Status "Using development environment with Podman"
        }
        "prod" {
            $script:ComposeFile = "docker-compose.prod.yml"
            $script:PodmanFile = "podman-compose.prod.yml"
            $script:EnvFile = ".env.prod"
            $script:PodName = "flightboard-prod"
            Write-Status "Using production environment with Podman"
        }
    }
    
    # Check if environment file exists
    if (-not (Test-Path $script:EnvFile)) {
        Write-Warning "Environment file $($script:EnvFile) not found"
        Write-Status "Copying from .env.example..."
        Copy-Item ".env.example" $script:EnvFile
        Write-Warning "Please edit $($script:EnvFile) with your configuration"
    }
    
    # Create Podman-specific compose file if it doesn't exist
    if (-not (Test-Path $script:PodmanFile)) {
        Write-Status "Creating Podman-specific compose file from Docker compose..."
        Copy-Item $script:ComposeFile $script:PodmanFile
        
        # Podman-specific adjustments
        $content = Get-Content $script:PodmanFile
        $content = $content -replace 'restart: unless-stopped', 'restart: always'
        $content = $content -replace 'docker-compose', 'podman-compose'
        $content | Set-Content $script:PodmanFile
        
        Write-Success "Created $($script:PodmanFile)"
    }
}

# Function to perform actions with Podman
function Invoke-PodmanAction {
    param([string]$Action)
    
    switch ($Action) {
        "up" {
            Write-Status "Starting FlightBoard services with Podman..."
            
            if ($script:UseComposeCompat) {
                # Use podman-compose for Docker Compose compatibility
                podman-compose -f $script:PodmanFile --env-file $script:EnvFile up -d
            } else {
                # Use native Podman commands
                Write-Status "Creating Podman pod: $($script:PodName)..."
                podman pod create --name $script:PodName -p 5183:8080 -p 3000:3000 -p 3001:3001 -p 6379:6379
                
                # Start Redis cache first
                Write-Status "Starting Redis cache..."
                podman run -d --pod $script:PodName --name flightboard-redis `
                    -v "redis-data:/data" `
                    redis:7-alpine
                
                # Wait for Redis to be ready
                Start-Sleep -Seconds 3
                
                # Build and run API container with Redis connection
                Write-Status "Building and running API container..."
                podman build -t flightboard-api:latest -f api/Dockerfile ../../../
                podman run -d --pod $script:PodName --name flightboard-api-container `
                    --env-file $script:EnvFile `
                    -e "ConnectionStrings__Redis=localhost:6379" `
                    -v "flightboard-data:/app/data" `
                    flightboard-api:latest
            }
            
            Write-Success "Services started successfully with Podman"
            Write-Status "Podman advantages: Rootless, daemonless, enhanced security"
            Write-Status "Services will be available at:"
            Write-Host "  - API: http://localhost:5183"
            Write-Host "  - Consumer App: http://localhost:3000"
            Write-Host "  - Backoffice App: http://localhost:3001"
        }
        "down" {
            Write-Status "Stopping FlightBoard services..."
            
            if ($script:UseComposeCompat) {
                podman-compose -f $script:PodmanFile down
            } else {
                # Stop pod and remove containers
                podman pod stop $script:PodName 2>$null
                podman pod rm $script:PodName 2>$null
            }
            
            Write-Success "Services stopped successfully"
        }
        "build" {
            Write-Status "Building FlightBoard containers with Podman..."
            
            if ($script:UseComposeCompat) {
                podman-compose -f $script:PodmanFile --env-file $script:EnvFile build --no-cache
            } else {
                # Build individual images
                podman build -t flightboard-api:latest -f api/Dockerfile ../../../ --no-cache
                Write-Status "Consider building frontend images as well"
            }
            
            Write-Success "Containers built successfully with Podman"
        }
        "logs" {
            Write-Status "Showing service logs (Ctrl+C to exit)..."
            
            if ($script:UseComposeCompat) {
                podman-compose -f $script:PodmanFile logs -f
            } else {
                podman pod logs $script:PodName -f
            }
        }
        "status" {
            Write-Status "Podman service status:"
            
            if ($script:UseComposeCompat) {
                podman-compose -f $script:PodmanFile ps
            } else {
                podman pod ps
                Write-Host ""
                podman ps --pod
            }
        }
        "clean" {
            Write-Warning "This will remove all containers, pods, volumes, and data!"
            $confirmation = Read-Host "Are you sure? (y/N)"
            if ($confirmation -eq "y" -or $confirmation -eq "Y") {
                Write-Status "Cleaning up Podman resources..."
                
                if ($script:UseComposeCompat) {
                    podman-compose -f $script:PodmanFile down -v
                } else {
                    # Stop and remove pod
                    podman pod stop $script:PodName 2>$null
                    podman pod rm $script:PodName 2>$null
                    
                    # Remove volumes
                    podman volume rm flightboard-data 2>$null
                }
                
                # Clean up unused resources
                podman system prune -f
                Write-Success "Cleanup completed"
            }
            else {
                Write-Status "Cleanup cancelled"
            }
        }
    }
}

# Main script logic
function Main {
    # Check if running from correct directory
    if (-not (Test-Path "docker-compose.yml")) {
        Write-Error "Please run this script from the infra/docker directory"
        exit 1
    }
    
    # Check Podman installation
    Test-Podman
    
    # Set environment
    Set-DeploymentEnvironment $Environment
    
    # Perform action
    Invoke-PodmanAction $Action
}

# Display usage if help is requested
if ($Environment -eq "help" -or $Action -eq "help") {
    Write-Host "FlightBoard Podman Deployment Script (PowerShell)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\deploy-podman.ps1 -Environment <env> -Action <action>"
    Write-Host ""
    Write-Host "Environment:"
    Write-Host "  dev         Development environment (hot reload, debug)"
    Write-Host "  prod        Production environment (optimized, security)"
    Write-Host ""
    Write-Host "Action:"
    Write-Host "  up          Start services with Podman"
    Write-Host "  down        Stop services"
    Write-Host "  build       Build containers"
    Write-Host "  logs        Show service logs"
    Write-Host "  status      Show service status"
    Write-Host "  clean       Remove containers, pods, and volumes"
    Write-Host ""
    Write-Host "Podman Advantages:" -ForegroundColor Green
    Write-Host "  - Rootless containers (enhanced security)"
    Write-Host "  - Daemonless architecture"
    Write-Host "  - Docker-compatible commands"
    Write-Host "  - Better resource isolation"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\deploy-podman.ps1 -Environment dev -Action up      # Start with Podman"
    Write-Host "  .\deploy-podman.ps1 -Environment prod -Action build  # Build with Podman"
    Write-Host "  .\deploy-podman.ps1 -Environment dev -Action logs    # Show logs"
    exit 0
}

# Run main function
Main
