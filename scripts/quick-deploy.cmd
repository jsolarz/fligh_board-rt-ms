@echo off
REM FlightBoard Quick Deploy
REM Run from project root: quick-deploy.cmd

echo 🚀 FlightBoard Quick Deploy
echo ==========================

REM Check if we're in the right directory
if not exist objectives.md (
    echo ❌ Error: Please run this script from the FlightBoard project root directory
    exit /b 1
)

echo 📍 Project root: %CD%

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Create necessary directories
echo 📁 Setting up directory structure...
if not exist data mkdir data
if not exist infra\docker mkdir infra\docker

REM Change to docker directory
cd infra\docker

REM Check if .env file exists
if not exist .env (
    echo 📝 Creating .env file from template...
    if exist .env.example (
        copy .env.example .env
        echo ⚠️  Created .env file. You can continue with defaults for local development.
        choice /M "Continue with default settings"
        if errorlevel 2 (
            echo Please edit infra\docker\.env and run this script again.
            exit /b 1
        )
    ) else (
        echo ❌ .env.example not found. Creating basic .env file...
        echo # Basic configuration for local development > .env
        echo JWT_SECRET=DevSecretKeyForLocalDevelopment2024! >> .env
        echo JWT_ISSUER=FlightBoard.Api >> .env
        echo JWT_AUDIENCE=FlightBoard.Frontend >> .env
        echo JWT_ACCESS_EXPIRY=15 >> .env
        echo JWT_REFRESH_EXPIRY=7 >> .env
        echo DB_VOLUME_PATH=../../data >> .env
        echo API_PUBLIC_URL=http://localhost:5183 >> .env
        echo SIGNALR_PUBLIC_URL=http://localhost:5183/flighthub >> .env
        echo LOG_LEVEL=Information >> .env
    )
)

echo 🔨 Building and starting FlightBoard services...

REM Clean up any existing containers
echo 🧹 Cleaning up existing containers...
docker-compose down --remove-orphans >nul 2>&1

REM Build and start services
echo 🏗️  Building services...
docker-compose build --no-cache

echo 🚀 Starting services...
docker-compose up -d

REM Wait for services to be ready
echo ⏳ Waiting for services to be ready...
timeout /t 15 /nobreak >nul

REM Check service health
echo 🔍 Checking service health...
docker-compose ps

REM Display logs
echo 📋 Recent service logs:
docker-compose logs --tail=5

echo.
echo ✅ FlightBoard deployment completed successfully!
echo.
echo 🌐 Access your applications:
echo    • API Documentation: http://localhost:5183/swagger
echo    • API Health Check:  http://localhost:5183/health
echo    • Consumer App:      http://localhost:3000
echo    • Backoffice App:    http://localhost:3001
echo.
echo 🔧 Useful commands:
echo    • View logs:         cd infra\docker ^&^& docker-compose logs -f
echo    • Stop services:     cd infra\docker ^&^& docker-compose down
echo    • Restart services:  cd infra\docker ^&^& docker-compose restart
echo.
echo 📁 Project structure:
echo    • Source code:       src\
echo    • Infrastructure:    infra\
echo    • Database:          data\

pause
