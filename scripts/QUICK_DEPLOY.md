# 🚀 Quick Deploy

Run this script from the project root to deploy FlightBoard with one command.

## Usage

### Linux/macOS
```bash
./quick-deploy.sh
```

### Windows
```cmd
quick-deploy.cmd
```

## What it does

1. ✅ Validates Docker is running
2. 📁 Sets up directory structure
3. ⚙️ Creates environment configuration
4. 🏗️ Builds all services
5. 🚀 Starts the application
6. 📋 Shows service status

## Services Started

- **API**: http://localhost:5183 (with Swagger at /swagger)
- **Consumer App**: http://localhost:3000 (Public flight display)
- **Backoffice App**: http://localhost:3001 (Admin panel)

## Advanced Options

For more deployment options and configurations:
- See `infra/deployment/` for detailed scripts
- See `infra/docker/` for Docker configurations
- Edit `infra/docker/.env` for custom settings
