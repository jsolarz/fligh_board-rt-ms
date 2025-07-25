# Quick Start Scripts

This directory contains quick deployment and development scripts.

## 🚀 Production Deployment

### One-Command Deploy
```bash
./deploy.sh    # Linux/macOS
deploy.cmd     # Windows
```

These scripts will:
1. Validate environment and Docker
2. Set up directory structure
3. Create `.env` from template if needed
4. Build and start all services
5. Display service status and URLs

## 🛠️ Development

### Development Environment
```bash
./dev-start.sh    # Linux/macOS
dev-start.cmd     # Windows
```

### Development Commands
```bash
./dev-stop.sh     # Stop all services
./dev-logs.sh     # View logs
./dev-clean.sh    # Clean containers and images
```

## 📁 Script Location Strategy

- **Root level**: Quick access scripts for common operations
- **infra/deployment/**: Detailed deployment scripts with options
- **infra/docker/**: Docker-specific configurations and compose files

## 🔧 Advanced Options

For more detailed deployment options and configurations, see:
- `infra/deployment/` - Advanced deployment scripts
- `infra/docker/` - Docker configurations and environment files
- `docs/` - Detailed documentation
