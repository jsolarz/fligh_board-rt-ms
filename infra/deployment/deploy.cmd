@echo off
REM Windows deployment script for FlightBoard system

echo 🚀 Starting FlightBoard deployment...

REM Change to docker directory
cd /d "%~dp0"

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Check if .env file exists
if not exist .env (
    echo 📝 Creating .env file from template...
    copy .env.example .env
    echo ⚠️  Please edit .env file with your configuration before running again.
    exit /b 1
)

REM Create data directory if it doesn't exist
echo 📁 Creating data directory...
if not exist ..\..\data mkdir ..\..\data

REM Build and start services
echo 🔨 Building and starting services...
docker-compose down --remove-orphans
docker-compose pull
docker-compose build --no-cache
docker-compose up -d

REM Wait for services to be healthy
echo ⏳ Waiting for services to be healthy...
timeout /t 10 /nobreak >nul

REM Check service status
echo 🔍 Checking service status...
docker-compose ps

REM Show logs
echo 📋 Recent logs:
docker-compose logs --tail=10

echo ✅ Deployment completed!
echo.
echo 🌐 Services available at:
echo   - API: http://localhost:5183
echo   - Consumer Frontend: http://localhost:3000
echo   - Backoffice Frontend: http://localhost:3001
echo.
echo 🔧 Management commands:
echo   - View logs: docker-compose logs -f
echo   - Stop services: docker-compose down
echo   - Restart services: docker-compose restart

pause
