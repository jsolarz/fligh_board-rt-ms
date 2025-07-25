# Docker Deployment Verification Script
# Verifies Docker configuration without requiring Docker to be installed

param(
    [switch]$Detailed = $false
)

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

function Test-DockerConfiguration {
    Write-Status "Verifying Docker configuration files..."
    
    $configFiles = @(
        "docker-compose.yml",
        "docker-compose.dev.yml", 
        "docker-compose.prod.yml",
        ".env.example",
        ".env.dev",
        ".env.prod",
        "api/Dockerfile",
        "deploy.ps1",
        "deploy.sh"
    )
    
    $missingFiles = @()
    $validFiles = @()
    
    foreach ($file in $configFiles) {
        if (Test-Path $file) {
            $validFiles += $file
            Write-Success "✓ $file exists"
        } else {
            $missingFiles += $file
            Write-Error "✗ $file missing"
        }
    }
    
    if ($missingFiles.Count -eq 0) {
        Write-Success "All Docker configuration files are present"
    } else {
        Write-Warning "$($missingFiles.Count) files are missing"
    }
    
    return @{
        Valid = $validFiles
        Missing = $missingFiles
    }
}

function Test-DockerfileValidation {
    Write-Status "Validating Dockerfile syntax..."
    
    $dockerfilePath = "api/Dockerfile"
    
    if (-not (Test-Path $dockerfilePath)) {
        Write-Error "Dockerfile not found at $dockerfilePath"
        return $false
    }
    
    $content = Get-Content $dockerfilePath
    $hasFrom = $content | Where-Object { $_ -match "^FROM " }
    $hasWorkdir = $content | Where-Object { $_ -match "^WORKDIR " }
    $hasCopy = $content | Where-Object { $_ -match "^COPY " }
    $hasRun = $content | Where-Object { $_ -match "^RUN " }
    
    if ($hasFrom) {
        Write-Success "✓ FROM instruction found"
    } else {
        Write-Error "✗ No FROM instruction found"
    }
    
    if ($hasWorkdir) {
        Write-Success "✓ WORKDIR instruction found"
    } else {
        Write-Warning "△ No WORKDIR instruction found"
    }
    
    if ($hasCopy) {
        Write-Success "✓ COPY instruction found"
    } else {
        Write-Error "✗ No COPY instruction found"
    }
    
    if ($hasRun) {
        Write-Success "✓ RUN instruction found"
    } else {
        Write-Warning "△ No RUN instruction found"
    }
    
    # Check for .NET 9.0
    $dotnetVersion = $content | Where-Object { $_ -match "dotnet.*:9\.0" }
    if ($dotnetVersion) {
        Write-Success "✓ Using .NET 9.0"
    } else {
        Write-Warning "△ .NET version not clearly identified as 9.0"
    }
    
    return $true
}

function Test-ComposeFileValidation {
    Write-Status "Validating docker-compose files..."
    
    $composeFiles = @("docker-compose.yml", "docker-compose.dev.yml", "docker-compose.prod.yml")
    
    foreach ($file in $composeFiles) {
        if (Test-Path $file) {
            Write-Status "Checking $file..."
            
            $content = Get-Content $file -Raw
            
            # Check for version
            if ($content -match "version:") {
                Write-Success "  ✓ Version specified"
            } else {
                Write-Warning "  △ No version specified"
            }
            
            # Check for services
            if ($content -match "services:") {
                Write-Success "  ✓ Services section found"
            } else {
                Write-Error "  ✗ No services section found"
            }
            
            # Check for networks
            if ($content -match "networks:") {
                Write-Success "  ✓ Networks section found"
            } else {
                Write-Warning "  △ No networks section found"
            }
            
            # Check for volumes
            if ($content -match "volumes:") {
                Write-Success "  ✓ Volumes section found"
            } else {
                Write-Warning "  △ No volumes section found"
            }
        }
    }
}

function Test-EnvironmentFiles {
    Write-Status "Validating environment files..."
    
    $envFiles = @(".env.example", ".env.dev", ".env.prod")
    
    foreach ($file in $envFiles) {
        if (Test-Path $file) {
            Write-Status "Checking $file..."
            
            $content = Get-Content $file
            
            # Check for required variables
            $requiredVars = @("JWT_SECRET", "DB_CONNECTION_STRING", "API_PUBLIC_URL")
            
            foreach ($var in $requiredVars) {
                if ($content | Where-Object { $_ -match "^$var=" }) {
                    Write-Success "  ✓ $var defined"
                } else {
                    Write-Warning "  △ $var not found"
                }
            }
        }
    }
}

function Show-DeploymentInstructions {
    Write-Host ""
    Write-Host "=== DEPLOYMENT INSTRUCTIONS ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Install Docker Desktop:" -ForegroundColor Yellow
    Write-Host "   https://www.docker.com/products/docker-desktop"
    Write-Host ""
    Write-Host "2. Start Docker Desktop and ensure it's running"
    Write-Host ""
    Write-Host "3. Navigate to infra/docker directory:"
    Write-Host "   cd infra/docker"
    Write-Host ""
    Write-Host "4. Deploy development environment:" -ForegroundColor Green
    Write-Host "   .\deploy.ps1 -Environment dev -Action up"
    Write-Host ""
    Write-Host "5. Or deploy production environment:" -ForegroundColor Green
    Write-Host "   .\deploy.ps1 -Environment prod -Action up"
    Write-Host ""
    Write-Host "6. Check service status:" -ForegroundColor Green
    Write-Host "   .\deploy.ps1 -Environment dev -Action status"
    Write-Host ""
    Write-Host "7. View logs:" -ForegroundColor Green
    Write-Host "   .\deploy.ps1 -Environment dev -Action logs"
    Write-Host ""
    Write-Host "8. Stop services:" -ForegroundColor Green
    Write-Host "   .\deploy.ps1 -Environment dev -Action down"
    Write-Host ""
}

# Main execution
Write-Host "FlightBoard Docker Deployment Verification" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running from correct directory
if (-not (Test-Path "docker-compose.yml")) {
    Write-Error "Please run this script from the infra/docker directory"
    Write-Host "Current directory: $(Get-Location)"
    exit 1
}

# Run verification tests
$configTest = Test-DockerConfiguration
Test-DockerfileValidation | Out-Null
Test-ComposeFileValidation
Test-EnvironmentFiles

# Summary
Write-Host ""
Write-Host "=== VERIFICATION SUMMARY ===" -ForegroundColor Cyan
Write-Success "$($configTest.Valid.Count) configuration files found"

if ($configTest.Missing.Count -gt 0) {
    Write-Warning "$($configTest.Missing.Count) files missing"
    Write-Host "Missing files:" -ForegroundColor Yellow
    $configTest.Missing | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
}

# Check if Docker is installed
try {
    docker --version 2>$null | Out-Null
    Write-Success "Docker is installed and available"
    
    try {
        docker info 2>$null | Out-Null
        Write-Success "Docker daemon is running"
        Write-Host ""
        Write-Success "✅ Ready for deployment! Use deploy.ps1 to start services."
    }
    catch {
        Write-Warning "Docker is installed but daemon is not running"
        Write-Host "Please start Docker Desktop"
    }
}
catch {
    Write-Warning "Docker is not installed or not in PATH"
    Show-DeploymentInstructions
}

if ($Detailed) {
    Show-DeploymentInstructions
}
