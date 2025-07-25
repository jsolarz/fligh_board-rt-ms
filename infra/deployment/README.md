# Deployment Guide

## Quick Deployment

### Windows
```cmd
cd infra\deployment
deploy.cmd
```

### Linux/macOS
```bash
cd infra/deployment
chmod +x deploy.sh
./deploy.sh
```

## Manual Deployment

1. **Navigate to Docker directory**
   ```bash
   cd infra/docker
   ```

2. **Configure Environment**
   ```bash
   cp .env.example .env
   # Edit .env with your settings
   ```

3. **Deploy Services**
   ```bash
   docker-compose up -d
   ```

## Production Deployment

For production environments, use the production compose file:

```bash
cd infra/docker
cp .env.example .env
# Configure production settings in .env
docker-compose -f docker-compose.prod.yml up -d
```

### Production Checklist

- [ ] Set secure JWT_SECRET in `.env`
- [ ] Configure external database connection
- [ ] Set proper API_PUBLIC_URL and SIGNALR_PUBLIC_URL
- [ ] Enable SSL certificates (nginx configuration)
- [ ] Configure monitoring and logging
- [ ] Set resource limits appropriately
- [ ] Backup strategy for data volume

## Development Deployment

For development with hot reload:

```bash
cd infra/docker
docker-compose -f docker-compose.dev.yml up -d
```

## Service Management

### View Logs
```bash
docker-compose logs -f [service-name]
```

### Stop Services
```bash
docker-compose down
```

### Restart Services
```bash
docker-compose restart [service-name]
```

### Scale Services (Production)
```bash
docker-compose -f docker-compose.prod.yml up -d --scale flightboard-api=2
```

## Troubleshooting

### Database Issues
- Check data directory permissions
- Verify volume mounting paths
- Ensure SQLite file is writable

### Network Issues
- Verify service dependencies
- Check port availability
- Review network configuration

### Build Issues
- Clear Docker cache: `docker system prune -a`
- Check Dockerfile contexts and paths
- Verify source code file structure

### Health Check Failures
- Check service startup time
- Verify health check endpoints
- Review container logs for errors
