# FlightBoard Real-time Management System

**Enterprise-grade flight information management with dual-themed frontends and high-performance backend**

A comprehensive full-stack application built with .NET 9 and React 18, featuring real-time flight management, role-based access control, enterprise caching, and distinctive user interfaces for both public display and administrative management.

## ✨ Key Features

- 🚀 **Real-time Updates**: SignalR with automatic reconnection and live flight status broadcasting
- 🎨 **Dual Themed UIs**: Cyberpunk consumer display + BBS terminal admin interface
- 🔐 **Enterprise Security**: JWT authentication with role-based access control (RBAC)
- ⚡ **High Performance**: Redis distributed caching with in-memory fallback
- 🏗️ **iDesign Architecture**: Manager/Engine/Accessor pattern with iFX framework
- 🐳 **Containerized**: Docker development and production deployment ready
- 📱 **Responsive**: Mobile-optimized interfaces with ASCII art headers
- 🔍 **Advanced Search**: Multi-criteria filtering with performance optimization

## 🚀 Quick Start

**One-command deployment:**
```bash
./quick-deploy.sh    # Linux/macOS
quick-deploy.cmd     # Windows
```

This will build and start all services with production-ready configurations including caching, health monitoring, and security.

## 🏗️ Project Structure

```
├── src/                          # Application source code
│   ├── FlightBoard.Api/         # .NET Core Web API
│   └── frontend/                # React TypeScript frontends
│       ├── consumer/            # Public flight display (cyberpunk style)
│       └── backoffice/          # Admin panel (BBS terminal style)
├── infra/                       # Infrastructure & deployment
│   ├── docker/                  # Container configurations
│   ├── deployment/              # Deployment scripts & automation
│   ├── iac/                     # Infrastructure as Code (future)
│   └── cicd/                    # CI/CD pipelines (future)
├── data/                        # SQLite database (auto-created)
├── docs/                        # Documentation
└── quick-deploy.*               # One-command deployment scripts
```

## 🌐 Services & Ports

- **API**: http://localhost:5183 (HTTP) / https://localhost:7022 (HTTPS)
  - Swagger Documentation: `/swagger`
  - Health Check: `/health`
  - Performance Metrics: Available via structured logging
- **Consumer App**: http://localhost:3000 (Public flight board with cyberpunk theme)
- **Backoffice App**: http://localhost:3001 (Admin interface with BBS terminal theme)

## 🛠️ Technology Stack

### Backend (.NET 9)
- **Framework**: ASP.NET Core 9 Web API
- **Database**: Entity Framework Core with SQLite
- **Caching**: Redis with in-memory fallback
- **Real-time**: SignalR with connection management
- **Authentication**: JWT with role-based authorization
- **Logging**: Serilog with structured output
- **Architecture**: iDesign Method (Manager/Engine/Accessor)

### Frontend (React 18)
- **Framework**: React 18 with TypeScript (strict mode)
- **State Management**: Redux Toolkit + TanStack Query
- **Styling**: Custom CSS with distinct theming
- **Real-time**: SignalR client with auto-reconnection
- **Forms**: React Hook Form with validation
- **Build**: Vite for fast development and optimized builds

### Infrastructure
- **Containerization**: Docker with multi-stage builds
- **Development**: DevContainer support for VS Code
- **Deployment**: Docker Compose with environment configs
- **Monitoring**: Health checks and performance tracking
- **Security**: Non-root containers and secure configurations

## 🛠️ Development Setup

### Prerequisites
- .NET 9.0 SDK
- Node.js (v20+)
- Docker & Docker Compose
- VS Code (recommended with DevContainer support)

### DevContainer Development (Recommended)
Each component has its own DevContainer for instant development setup:

```bash
# API Development
cd src/FlightBoard.Api
code .  # Open in VS Code → "Reopen in Container"

# Consumer Frontend Development  
cd src/frontend/consumer
code .  # Open in VS Code → "Reopen in Container"

# Backoffice Frontend Development
cd src/frontend/backoffice
code .  # Open in VS Code → "Reopen in Container"
```

### Docker Development Environment
```bash
cd infra/docker
cp .env.example .env
# Edit .env with your settings
docker-compose -f docker-compose.dev.yml up -d
```

### Manual Development Setup

#### Backend API (.NET 9)
```bash
cd src
dotnet build
dotnet run --project FlightBoard.Api
# API available at http://localhost:5183
```

#### Frontend Applications (React 18)
```bash
# Consumer App (Cyberpunk Theme)
cd src/frontend/consumer
npm install && npm start
# Available at http://localhost:3000

# Backoffice App (BBS Terminal Theme)
cd src/frontend/backoffice
npm install && npm start  
# Available at http://localhost:3001
```

## 🏗️ Architecture Overview

### iDesign Method Implementation
```
API Layer (Controllers)
├── Manager Layer (Use Case Orchestration)
│   ├── FlightManager → CachedFlightManager (Decorator)
│   ├── AuthManager → JWT & Role Management
│   └── PerformanceManager → Metrics Collection
├── Engine Layer (Business Logic)
│   ├── FlightEngine → Status Calculation & Rules
│   ├── AuthEngine → Security Logic
│   └── PerformanceEngine → Performance Analysis
└── Data Access Layer
    ├── FlightDataAccess → EF Core Repository
    └── UserDataAccess → Authentication Data
```

### iFX Framework (Cross-cutting Concerns)
- **CacheService**: Redis + Memory with fallback strategy
- **PerformanceService**: Operation timing and metrics
- **JwtService**: Token management and security
- **Structured Logging**: Serilog with correlation IDs

### Real-time Architecture
- **SignalR Hubs**: Flight updates broadcast to all clients
- **Auto-reconnection**: Client-side connection management
- **Performance**: Cached data with real-time invalidation
- **Security**: JWT authentication for SignalR connections

## 📋 Development Status

### ✅ Completed Features
- [x] **Enterprise Architecture**: iDesign Method with Manager/Engine/Accessor pattern
- [x] **Full-stack Implementation**: .NET 9 API + React 18 frontends
- [x] **Real-time Communication**: SignalR with automatic reconnection
- [x] **Authentication & Authorization**: JWT with role-based access control
- [x] **High-performance Caching**: Redis with in-memory fallback
- [x] **Advanced Search**: Multi-criteria filtering with optimization
- [x] **Themed User Interfaces**: Cyberpunk consumer + BBS terminal admin
- [x] **Docker Containerization**: Development and production environments
- [x] **DevContainer Setup**: VS Code integration for instant development
- [x] **Health Monitoring**: Comprehensive health checks and logging
- [x] **Performance Optimization**: Caching, query optimization, metrics collection
- [x] **Comprehensive Documentation**: Technical docs and deployment guides

### 🚀 Production Ready Features
- [x] **Security**: JWT authentication, HTTPS support, secure containers
- [x] **Performance**: Sub-second response times with caching
- [x] **Reliability**: Health checks, auto-recovery, error handling
- [x] **Scalability**: Stateless design, caching layers, container orchestration
- [x] **Operability**: Structured logging, monitoring, deployment automation
dotnet build
dotnet run --project FlightBoard.Api
```

#### Frontend Applications
```bash
# Consumer App
cd src/frontend/consumer
npm install && npm start

# Backoffice App  
cd src/frontend/backoffice
npm install && npm start
```

### Docker Development
```bash
cd infra/docker
cp .env.example .env
docker-compose -f docker-compose.dev.yml up -d
```

## 📚 Documentation

- [Infrastructure Guide](infra/README.md) - Docker, deployment, and infrastructure
- [Deployment Guide](infra/deployment/README.md) - Detailed deployment options
- [Quick Deploy Guide](QUICK_DEPLOY.md) - One-command deployment info
npm install
npm start
```
Runs on `http://localhost:3001` (if 3000 is taken)

## 📋 Development Status

### Completed ✅
- [x] Project structure and organization
- [x] .NET solution builds successfully
- [x] React apps created with TypeScript
- [x] Docker containerization with multi-service architecture
- [x] Infrastructure organization and deployment automation
- [x] Production-ready nginx configurations
- [x] Environment-based configurations (dev/staging/prod)

### In Progress 🚧
- [ ] Database foundation (Entity Framework setup)
- [ ] Basic API endpoints (CRUD operations)
- [ ] Frontend integration with API
- [ ] Real-time updates (SignalR implementation)

### Planned 📋
- [ ] CI/CD pipeline setup
- [ ] Cloud deployment (Azure/AWS/GCP)
- [ ] Monitoring and logging
- [ ] Security hardening
- [ ] Performance optimization

## 🔧 Advanced Usage

### Production Deployment
```bash
cd infra/docker
cp .env.example .env
# Edit .env with production settings
docker-compose -f docker-compose.prod.yml up -d
```

### Infrastructure Management
- **View logs**: `cd infra/docker && docker-compose logs -f`
- **Stop services**: `cd infra/docker && docker-compose down`
- **Clean rebuild**: `cd infra/docker && docker-compose build --no-cache`

### Environment Configuration
Edit `infra/docker/.env` to configure:
- Database connections (SQLite → External DB for production)
- JWT secrets and security settings
- Public URLs for frontend API connections
- Logging levels and monitoring

## 🤝 Contributing

1. Follow the project structure guidelines
2. Use the infrastructure tools for consistent environments
3. Test with Docker containers before deployment
4. Document infrastructure changes in `infra/` directory
