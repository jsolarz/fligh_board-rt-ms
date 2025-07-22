#!/bin/bash
# Podman Deployment Script for FlightBoard Application
# Alternative to Docker with rootless containers and enhanced security

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[PODMAN]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if Podman is installed
check_podman() {
    print_status "Checking Podman installation..."
    
    if ! command -v podman &> /dev/null; then
        print_error "Podman is not installed!"
        echo
        echo "To install Podman:"
        echo "1. Fedora/RHEL: sudo dnf install podman"
        echo "2. Ubuntu/Debian: sudo apt install podman"
        echo "3. macOS: brew install podman"
        echo "4. Windows: https://github.com/containers/podman/releases"
        echo
        echo "After installation, restart your terminal and run this script again."
        exit 1
    fi
    
    local podman_version=$(podman --version)
    print_success "Found: $podman_version"
    
    # Check if podman-compose is available (optional but recommended)
    if command -v podman-compose &> /dev/null; then
        local compose_version=$(podman-compose --version)
        print_success "Found podman-compose: $compose_version"
        USE_COMPOSE_COMPAT=true
    else
        print_warning "podman-compose not found, using native Podman commands"
        USE_COMPOSE_COMPAT=false
    fi
    
    print_success "Podman is ready for deployment"
}

# Function to display usage
show_usage() {
    echo -e "${CYAN}FlightBoard Podman Deployment Script${NC}"
    echo
    echo "Usage: $0 [ENVIRONMENT] [ACTION]"
    echo
    echo "ENVIRONMENT:"
    echo "  dev         Development environment (hot reload, debug)"
    echo "  prod        Production environment (optimized, security)"
    echo
    echo "ACTION:"
    echo "  up          Start services with Podman"
    echo "  down        Stop services"
    echo "  build       Build containers"
    echo "  logs        Show service logs"
    echo "  status      Show service status"
    echo "  clean       Remove containers, pods, and volumes"
    echo
    echo -e "${GREEN}Podman Advantages:${NC}"
    echo "  - Rootless containers (enhanced security)"
    echo "  - Daemonless architecture"
    echo "  - Docker-compatible commands"
    echo "  - Better resource isolation"
    echo
    echo "Examples:"
    echo "  $0 dev up          # Start development environment with Podman"
    echo "  $0 prod build      # Build production containers with Podman"
    echo "  $0 dev logs        # Show development logs"
    echo "  $0 prod down       # Stop production services"
}

# Function to set environment variables
set_environment() {
    local env=$1
    
    case $env in
        "dev")
            export COMPOSE_FILE="docker-compose.dev.yml"
            export PODMAN_FILE="podman-compose.dev.yml"
            export ENV_FILE=".env.dev"
            export POD_NAME="flightboard-dev"
            print_status "Using development environment with Podman"
            ;;
        "prod")
            export COMPOSE_FILE="docker-compose.prod.yml"
            export PODMAN_FILE="podman-compose.prod.yml"
            export ENV_FILE=".env.prod"
            export POD_NAME="flightboard-prod"
            print_status "Using production environment with Podman"
            ;;
        *)
            print_error "Invalid environment: $env"
            show_usage
            exit 1
            ;;
    esac
    
    # Check if environment file exists
    if [[ ! -f "$ENV_FILE" ]]; then
        print_warning "Environment file $ENV_FILE not found"
        print_status "Copying from .env.example..."
        cp .env.example "$ENV_FILE"
        print_warning "Please edit $ENV_FILE with your configuration"
    fi
    
    # Create Podman-specific compose file if it doesn't exist
    if [[ ! -f "$PODMAN_FILE" ]]; then
        print_status "Creating Podman-specific compose file from Docker compose..."
        cp "$COMPOSE_FILE" "$PODMAN_FILE"
        
        # Podman-specific adjustments
        sed -i 's/restart: unless-stopped/restart: always/g' "$PODMAN_FILE" 2>/dev/null || \
        sed -i '' 's/restart: unless-stopped/restart: always/g' "$PODMAN_FILE" 2>/dev/null || true
        
        print_success "Created $PODMAN_FILE"
    fi
}

# Function to perform actions with Podman
perform_action() {
    local action=$1
    
    case $action in
        "up")
            print_status "Starting FlightBoard services with Podman..."
            
            if [[ "$USE_COMPOSE_COMPAT" == "true" ]]; then
                # Use podman-compose for Docker Compose compatibility
                podman-compose -f "$PODMAN_FILE" --env-file "$ENV_FILE" up -d
            else
                # Use native Podman commands
                print_status "Creating Podman pod: $POD_NAME..."
                podman pod create --name "$POD_NAME" -p 5183:8080 -p 3000:3000 -p 3001:3001 -p 6379:6379 || true
                
                # Start Redis cache first
                print_status "Starting Redis cache..."
                podman run -d --pod "$POD_NAME" --name flightboard-redis \
                    -v redis-data:/data \
                    redis:7-alpine || print_warning "Redis container may already be running"
                
                # Wait for Redis to be ready
                sleep 3
                
                # Build and run API container with Redis connection
                print_status "Building and running API container..."
                podman build -t flightboard-api:latest -f api/Dockerfile ../../../
                podman run -d --pod "$POD_NAME" --name flightboard-api-container \
                    --env-file "$ENV_FILE" \
                    -e ConnectionStrings__Redis="localhost:6379" \
                    -v flightboard-data:/app/data \
                    flightboard-api:latest || print_warning "Container may already be running"
            fi
            
            print_success "Services started successfully with Podman"
            print_status "Podman advantages: Rootless, daemonless, enhanced security"
            print_status "Services will be available at:"
            echo "  - API: http://localhost:5183"
            echo "  - Consumer App: http://localhost:3000"
            echo "  - Backoffice App: http://localhost:3001"
            ;;
        "down")
            print_status "Stopping FlightBoard services..."
            
            if [[ "$USE_COMPOSE_COMPAT" == "true" ]]; then
                podman-compose -f "$PODMAN_FILE" down
            else
                # Stop pod and remove containers
                podman pod stop "$POD_NAME" 2>/dev/null || true
                podman pod rm "$POD_NAME" 2>/dev/null || true
            fi
            
            print_success "Services stopped successfully"
            ;;
        "build")
            print_status "Building FlightBoard containers with Podman..."
            
            if [[ "$USE_COMPOSE_COMPAT" == "true" ]]; then
                podman-compose -f "$PODMAN_FILE" --env-file "$ENV_FILE" build --no-cache
            else
                # Build individual images
                podman build -t flightboard-api:latest -f api/Dockerfile ../../../ --no-cache
                print_status "Consider building frontend images as well"
            fi
            
            print_success "Containers built successfully with Podman"
            ;;
        "logs")
            print_status "Showing service logs (Ctrl+C to exit)..."
            
            if [[ "$USE_COMPOSE_COMPAT" == "true" ]]; then
                podman-compose -f "$PODMAN_FILE" logs -f
            else
                podman pod logs "$POD_NAME" -f
            fi
            ;;
        "status")
            print_status "Podman service status:"
            
            if [[ "$USE_COMPOSE_COMPAT" == "true" ]]; then
                podman-compose -f "$PODMAN_FILE" ps
            else
                podman pod ps
                echo
                podman ps --pod
            fi
            ;;
        "clean")
            print_warning "This will remove all containers, pods, volumes, and data!"
            read -p "Are you sure? (y/N): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                print_status "Cleaning up Podman resources..."
                
                if [[ "$USE_COMPOSE_COMPAT" == "true" ]]; then
                    podman-compose -f "$PODMAN_FILE" down -v
                else
                    # Stop and remove pod
                    podman pod stop "$POD_NAME" 2>/dev/null || true
                    podman pod rm "$POD_NAME" 2>/dev/null || true
                    
                    # Remove volumes
                    podman volume rm flightboard-data 2>/dev/null || true
                fi
                
                # Clean up unused resources
                podman system prune -f
                print_success "Cleanup completed"
            else
                print_status "Cleanup cancelled"
            fi
            ;;
        *)
            print_error "Invalid action: $action"
            show_usage
            exit 1
            ;;
    esac
}

# Main script logic
main() {
    # Check if running from correct directory
    if [[ ! -f "docker-compose.yml" ]]; then
        print_error "Please run this script from the infra/docker directory"
        exit 1
    fi
    
    # Check arguments
    if [[ $# -ne 2 ]]; then
        show_usage
        exit 1
    fi
    
    local environment=$1
    local action=$2
    
    # Check Podman installation
    check_podman
    
    # Set environment
    set_environment "$environment"
    
    # Perform action
    perform_action "$action"
}

# Run main function with all arguments
main "$@"
