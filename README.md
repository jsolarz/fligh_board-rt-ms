# Flight Board System

A real-time flight information display system with cyberpunk-styled consumer interface and BBS terminal-styled backoffice management.

## 🚀 Quick Start

**One-command deployment:**
```bash
./quick-deploy.sh    # Linux/macOS
quick-deploy.cmd     # Windows
```

This will build and start all services with default configurations.

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

## 🌐 Services

- **API**: http://localhost:5183 (Swagger: /swagger)
- **Consumer App**: http://localhost:3000 (Public flight board)
- **Backoffice App**: http://localhost:3001 (Admin interface)

## 🛠️ Development Setup

### Prerequisites
- .NET 9.0 SDK
- Node.js (v18+)
- Docker & Docker Compose

### Manual Development Setup

#### Backend API
```bash
cd src
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
