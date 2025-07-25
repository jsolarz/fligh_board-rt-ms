# Docker Deployment Script for FlightBoard Application (PowerShell)
# Supports both development and production environments

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
    Write-Host "[INFO] $Message" -ForegroundColor Blue
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

# Function to check if Docker is installed
function Test-Docker {
    Write-Status "Checking Docker installation..."
    
    try {
        $dockerVersion = docker --version 2>$null
        if (-not $dockerVersion) {
            throw "Docker not found"
        }
    }
    catch {
        Write-Error "Docker is not installed or not in PATH!"
        Write-Host ""
        Write-Host "Please install Docker Desktop from:"
        Write-Host "https://www.docker.com/products/docker-desktop"
        Write-Host ""
        Write-Host "After installation, restart PowerShell and run this script again."
        exit 1
    }
    
    try {
        $composeVersion = docker-compose --version 2>$null
        if (-not $composeVersion) {
            throw "Docker Compose not found"
        }
    }
    catch {
        Write-Error "Docker Compose is not installed!"
        Write-Host ""
        Write-Host "Docker Compose is usually included with Docker Desktop."
        Write-Host "Please ensure Docker Desktop is properly installed."
        exit 1
    }
    
    # Check if Docker daemon is running
    try {
        docker info 2>$null | Out-Null
    }
    catch {
        Write-Error "Docker daemon is not running!"
        Write-Host ""
        Write-Host "Please start Docker Desktop and ensure it's running."
        Write-Host "You should see the Docker icon in your system tray."
        exit 1
    }
    
    Write-Success "Docker and Docker Compose are installed and running"
}

# Function to set environment variables
function Set-DeploymentEnvironment {
    param([string]$Env)
    
    switch ($Env) {
        "dev" {
            $script:ComposeFile = "docker-compose.dev.yml"
            $script:EnvFile = ".env.dev"
            Write-Status "Using development environment"
        }
        "prod" {
            $script:ComposeFile = "docker-compose.prod.yml"
            $script:EnvFile = ".env.prod"
            Write-Status "Using production environment"
        }
    }
    
    # Check if environment file exists
    if (-not (Test-Path $script:EnvFile)) {
        Write-Warning "Environment file $($script:EnvFile) not found"
        Write-Status "Copying from .env.example..."
        Copy-Item ".env.example" $script:EnvFile
        Write-Warning "Please edit $($script:EnvFile) with your configuration"
    }
}

# Function to perform actions
function Invoke-DeploymentAction {
    param([string]$Action)
    
    switch ($Action) {
        "up" {
            Write-Status "Starting FlightBoard services..."
            docker-compose -f $script:ComposeFile --env-file $script:EnvFile up -d
            Write-Success "Services started successfully"
            Write-Status "Services will be available at:"
            Write-Host "  - API: http://localhost:5183"
            Write-Host "  - Consumer App: http://localhost:3000"
            Write-Host "  - Backoffice App: http://localhost:3001"
        }
        "down" {
            Write-Status "Stopping FlightBoard services..."
            docker-compose -f $script:ComposeFile down
            Write-Success "Services stopped successfully"
        }
        "build" {
            Write-Status "Building FlightBoard containers..."
            docker-compose -f $script:ComposeFile --env-file $script:EnvFile build --no-cache
            Write-Success "Containers built successfully"
        }
        "logs" {
            Write-Status "Showing service logs (Ctrl+C to exit)..."
            docker-compose -f $script:ComposeFile logs -f
        }
        "status" {
            Write-Status "Service status:"
            docker-compose -f $script:ComposeFile ps
        }
        "clean" {
            Write-Warning "This will remove all containers, volumes, and data!"
            $confirmation = Read-Host "Are you sure? (y/N)"
            if ($confirmation -eq "y" -or $confirmation -eq "Y") {
                Write-Status "Cleaning up..."
                docker-compose -f $script:ComposeFile down -v
                docker system prune -f
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
    
    # Check Docker installation
    Test-Docker
    
    # Set environment
    Set-DeploymentEnvironment $Environment
    
    # Perform action
    Invoke-DeploymentAction $Action
}

# Display usage if help is requested
if ($Environment -eq "help" -or $Action -eq "help") {
    Write-Host "FlightBoard Docker Deployment Script (PowerShell)"
    Write-Host ""
    Write-Host "Usage: .\deploy.ps1 -Environment <env> -Action <action>"
    Write-Host ""
    Write-Host "Environment:"
    Write-Host "  dev         Development environment (hot reload, debug)"
    Write-Host "  prod        Production environment (optimized, SSL)"
    Write-Host ""
    Write-Host "Action:"
    Write-Host "  up          Start services"
    Write-Host "  down        Stop services"
    Write-Host "  build       Build containers"
    Write-Host "  logs        Show service logs"
    Write-Host "  status      Show service status"
    Write-Host "  clean       Remove containers and volumes"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\deploy.ps1 -Environment dev -Action up       # Start development environment"
    Write-Host "  .\deploy.ps1 -Environment prod -Action build   # Build production containers"
    Write-Host "  .\deploy.ps1 -Environment dev -Action logs     # Show development logs"
    Write-Host "  .\deploy.ps1 -Environment prod -Action down    # Stop production services"
    exit 0
}

# Run main function
Main
