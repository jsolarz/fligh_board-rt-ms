# Docker Deployment Guide

**Complete containerization setup for FlightBoard Real-time Management System**

This folder contains all Docker configurations for deploying the FlightBoard application in production or development environments.

## Prerequisites

- **Docker Desktop** - Install from https://docker.com/products/docker-desktop
- **Docker Compose** - Included with Docker Desktop
- **Minimum RAM**: 4GB available for containers
- **Port Requirements**: 5183 (API), 3000 (Consumer), 3001 (Backoffice), 80/443 (Nginx)

## Quick Start

### Development Environment
```bash
# Start all services with hot reload
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop services
docker-compose -f docker-compose.dev.yml down
```

### Production Environment
```bash
# Start production services
docker-compose -f docker-compose.prod.yml up -d

# Check service health
docker-compose -f docker-compose.prod.yml ps

# Stop services
docker-compose -f docker-compose.prod.yml down
```

## Service Architecture

### API Service (flightboard-api)
- **Image**: Custom .NET 9.0 application
- **Port**: 5183 → 8080 (internal)
- **Database**: SQLite volume-mounted
- **Health Check**: `/health` endpoint

### Frontend Services
- **Consumer** (flightboard-consumer): Port 3000, Cyberpunk theme
- **Backoffice** (flightboard-backoffice): Port 3001, BBS terminal theme

### Nginx Reverse Proxy
- **Port**: 80 (HTTP), 443 (HTTPS)
- **SSL**: Let's Encrypt certificates (production)
- **Load Balancing**: Round-robin for API endpoints

## Environment Configuration

### Development (.env.dev)
- Hot reload enabled
- Debug logging
- Development database
- CORS enabled for all origins

### Production (.env.prod)
- Optimized builds
- Production logging
- SSL/TLS enabled
- Security headers
- Rate limiting

## Data Persistence

### Volumes
- **flightboard-data**: SQLite database storage
- **nginx-certs**: SSL certificates
- **nginx-logs**: Access and error logs

### Backup Strategy
```bash
# Backup database
docker cp flightboard-api:/app/data/flightboard.db ./backup/

# Restore database
docker cp ./backup/flightboard.db flightboard-api:/app/data/
```

## Security Features

- **Non-root containers** - All services run as unprivileged users
- **Network isolation** - Internal Docker network
- **Secret management** - Environment variables for sensitive data
- **Health checks** - Automatic container restart on failure
- **SSL/TLS** - HTTPS encryption in production

## Monitoring & Debugging

### Container Logs
```bash
# API logs
docker logs flightboard-api -f

# Frontend logs
docker logs flightboard-consumer -f
docker logs flightboard-backoffice -f

# Nginx logs
docker logs flightboard-nginx -f
```

### Performance Monitoring
```bash
# Container resource usage
docker stats

# Service health status
curl http://localhost:5183/health
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Change ports in docker-compose.yml
2. **Build failures**: Check Dockerfile syntax and dependencies
3. **Database issues**: Verify volume permissions and SQLite file
4. **Network issues**: Ensure Docker network configuration

### Reset Environment
```bash
# Remove all containers and volumes
docker-compose down -v
docker system prune -f

# Rebuild from scratch
docker-compose build --no-cache
docker-compose up -d
```

## Production Deployment

### Cloud Deployment
- **AWS ECS**: Use provided task definitions
- **Azure Container Instances**: Use ARM templates
- **Google Cloud Run**: Use Cloud Build configurations

### CI/CD Integration
- **GitHub Actions**: `.github/workflows/docker-deploy.yml`
- **Azure DevOps**: `azure-pipelines.docker.yml`
- **GitLab CI**: `.gitlab-ci.yml`

## Files in this Directory

- **api/Dockerfile** - .NET 9.0 API container definition
- **frontend/consumer.Dockerfile** - React consumer app container
- **frontend/backoffice.Dockerfile** - React backoffice app container
- **nginx/nginx.conf** - Reverse proxy configuration
- **docker-compose.yml** - Base service definitions
- **docker-compose.dev.yml** - Development overrides
- **docker-compose.prod.yml** - Production configuration
- **.env.example** - Environment variable template
