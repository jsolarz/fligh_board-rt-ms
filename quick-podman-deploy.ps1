# Quick Podman Deploy Script for FlightBoard (PowerShell)
# Rootless, daemonless container deployment alternative to Docker

param(
    [Parameter(Position=0)]
    [ValidateSet("start", "stop", "status", "logs", "clean")]
    [string]$Action = "start"
)

# Colors for better output
$Green = "`e[32m"
$Blue = "`e[34m"
$Yellow = "`e[33m"
$Cyan = "`e[36m"
$Reset = "`e[0m"

Write-Host "${Cyan}FlightBoard - Quick Podman Deployment${Reset}"
Write-Host "${Cyan}====================================${Reset}"
Write-Host

# Check if Podman is installed
try {
    $podmanVersion = podman --version
    Write-Host "${Green}✓ Podman found: $podmanVersion${Reset}"
} catch {
    Write-Host "${Yellow}Podman is not installed!${Reset}"
    Write-Host
    Write-Host "Install Podman:"
    Write-Host "  - Windows: Download from https://github.com/containers/podman/releases"
    Write-Host "  - Or use Chocolatey: choco install podman"
    Write-Host "  - Or use Winget: winget install RedHat.Podman"
    Write-Host
    exit 1
}

# Navigate to Docker infrastructure directory
Set-Location "infra\docker"

switch ($Action) {
    "start" {
        Write-Host "${Blue}Starting FlightBoard with Podman (Development Mode)...${Reset}"
        Write-Host
        
        # Use the detailed Podman deployment script
        .\deploy-podman.ps1 -Environment dev -Action up
        
        Write-Host
        Write-Host "${Green}🚀 FlightBoard is starting with Podman!${Reset}"
        Write-Host
        Write-Host "${Cyan}Podman Advantages:${Reset}"
        Write-Host "  ✓ Rootless containers (enhanced security)"
        Write-Host "  ✓ Daemonless architecture"
        Write-Host "  ✓ Docker-compatible commands"
        Write-Host "  ✓ Better resource isolation"
        Write-Host
        Write-Host "${Cyan}Access your application:${Reset}"
        Write-Host "  🌐 Consumer App:  http://localhost:3000"
        Write-Host "  🏢 Backoffice App: http://localhost:3001" 
        Write-Host "  🔧 API:           http://localhost:5183"
        Write-Host "  ❤️  Health Check:  http://localhost:5183/health"
        Write-Host
        Write-Host "${Yellow}To stop services: .\quick-podman-deploy.ps1 stop${Reset}"
        Write-Host "${Yellow}To view logs: .\quick-podman-deploy.ps1 logs${Reset}"
    }
    
    "stop" {
        Write-Host "${Blue}Stopping FlightBoard Podman services...${Reset}"
        .\deploy-podman.ps1 -Environment dev -Action down
        Write-Host "${Green}✓ Services stopped${Reset}"
    }
    
    "status" {
        Write-Host "${Blue}FlightBoard Podman Status:${Reset}"
        .\deploy-podman.ps1 -Environment dev -Action status
    }
    
    "logs" {
        Write-Host "${Blue}FlightBoard Podman Logs:${Reset}"
        .\deploy-podman.ps1 -Environment dev -Action logs
    }
    
    "clean" {
        Write-Host "${Blue}Cleaning FlightBoard Podman resources...${Reset}"
        .\deploy-podman.ps1 -Environment dev -Action clean
        Write-Host "${Green}✓ Resources cleaned${Reset}"
    }
}
