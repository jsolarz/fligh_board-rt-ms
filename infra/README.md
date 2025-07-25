# FlightBoard Infrastructure

This directory contains all infrastructure-related files for the FlightBoard system.

## Structure

```
infra/
├── docker/                 # Docker-related files
│   ├── api/                # API container configuration
│   │   ├── Dockerfile      # Production API Dockerfile
│   │   └── Dockerfile.dev  # Development API Dockerfile
│   ├── frontend/           # Frontend containers configuration
│   │   ├── consumer.Dockerfile     # Consumer frontend Dockerfile
│   │   └── backoffice.Dockerfile   # Backoffice frontend Dockerfile
│   ├── docker-compose.yml     # Main docker-compose file
│   ├── docker-compose.prod.yml # Production docker-compose
│   ├── docker-compose.dev.yml  # Development docker-compose
│   └── .env.example           # Environment variables template
├── deployment/             # Deployment scripts and configurations
│   ├── deploy.sh          # Unix deployment script
│   ├── deploy.cmd         # Windows deployment script
│   └── README.md          # Deployment documentation
├── iac/                   # Infrastructure as Code (Future)
│   └── README.md          # IaC documentation placeholder
└── cicd/                  # CI/CD pipelines (Future)
    └── README.md          # CI/CD documentation placeholder
```

## Quick Start

### Development Environment
```bash
cd infra/docker
cp .env.example .env
# Edit .env with your settings
docker-compose -f docker-compose.dev.yml up -d
```

### Production Environment
```bash
cd infra/deployment
./deploy.sh  # Linux/Mac
# or
deploy.cmd   # Windows
```

## Environment Configuration

Copy `infra/docker/.env.example` to `infra/docker/.env` and configure:

- **Database**: SQLite by default, configure external DB for production
- **JWT**: Set secure JWT secret for production
- **URLs**: Configure public URLs for your deployment
- **Logging**: Set appropriate log levels

## Services

- **API**: .NET 8 Web API (Port 5183)
- **Consumer Frontend**: React app with cyberpunk design (Port 3000)
- **Backoffice Frontend**: React admin panel with BBS terminal design (Port 3001)

## Security Features

- Non-root containers
- Health checks for all services
- Resource limits in production
- Secure JWT configuration
- HTTPS support ready

## Data Persistence

SQLite database is persisted in `../../data` directory with proper volume mounting.
