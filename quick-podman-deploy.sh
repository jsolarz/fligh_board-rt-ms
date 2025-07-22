#!/bin/bash
# Quick Podman Deploy Script for FlightBoard
# Rootless, daemonless container deployment alternative to Docker

set -e

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${CYAN}FlightBoard - Quick Podman Deployment${NC}"
echo -e "${CYAN}====================================${NC}"
echo

# Check if Podman is installed
if ! command -v podman &> /dev/null; then
    echo -e "${YELLOW}Podman is not installed!${NC}"
    echo
    echo "Install Podman:"
    echo "  - Fedora/RHEL: sudo dnf install podman"
    echo "  - Ubuntu/Debian: sudo apt install podman"
    echo "  - macOS: brew install podman"
    echo "  - Windows: Download from https://github.com/containers/podman/releases"
    echo
    exit 1
fi

echo -e "${GREEN}✓ Podman found: $(podman --version)${NC}"

# Navigate to Docker infrastructure directory
cd infra/docker

echo -e "${BLUE}Starting FlightBoard with Podman (Development Mode)...${NC}"
echo

# Use the detailed Podman deployment script
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    # Windows (Git Bash/MSYS2)
    powershell -ExecutionPolicy Bypass -File deploy-podman.ps1 -Environment dev -Action up
else
    # Linux/macOS
    chmod +x deploy-podman.sh
    ./deploy-podman.sh dev up
fi

echo
echo -e "${GREEN}🚀 FlightBoard is starting with Podman!${NC}"
echo
echo -e "${CYAN}Podman Advantages:${NC}"
echo "  ✓ Rootless containers (enhanced security)"
echo "  ✓ Daemonless architecture"
echo "  ✓ Docker-compatible commands"
echo "  ✓ Better resource isolation"
echo
echo -e "${CYAN}Access your application:${NC}"
echo "  🌐 Consumer App:  http://localhost:3000"
echo "  🏢 Backoffice App: http://localhost:3001" 
echo "  🔧 API:           http://localhost:5183"
echo "  ❤️  Health Check:  http://localhost:5183/health"
echo
echo -e "${YELLOW}To stop services: ./quick-podman-deploy.sh stop${NC}"
echo -e "${YELLOW}To view logs: cd infra/docker && ./deploy-podman.sh dev logs${NC}"
