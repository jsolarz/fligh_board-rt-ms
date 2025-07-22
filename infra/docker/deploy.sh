#!/bin/bash
# Docker Deployment Script for FlightBoard Application
# Supports both development and production environments

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
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

# Function to check if Docker is installed
check_docker() {
    print_status "Checking Docker installation..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed!"
        echo
        echo "Please install Docker Desktop from:"
        echo "https://www.docker.com/products/docker-desktop"
        echo
        echo "After installation, restart your terminal and run this script again."
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose is not installed!"
        echo
        echo "Docker Compose is usually included with Docker Desktop."
        echo "If using Linux, install it separately:"
        echo "https://docs.docker.com/compose/install/"
        exit 1
    fi
    
    # Check if Docker daemon is running
    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running!"
        echo
        echo "Please start Docker Desktop and ensure it's running."
        echo "You should see the Docker icon in your system tray."
        exit 1
    fi
    
    print_success "Docker and Docker Compose are installed and running"
}

# Function to display usage
show_usage() {
    echo "FlightBoard Docker Deployment Script"
    echo
    echo "Usage: $0 [ENVIRONMENT] [ACTION]"
    echo
    echo "ENVIRONMENT:"
    echo "  dev         Development environment (hot reload, debug)"
    echo "  prod        Production environment (optimized, SSL)"
    echo
    echo "ACTION:"
    echo "  up          Start services"
    echo "  down        Stop services"
    echo "  build       Build containers"
    echo "  logs        Show service logs"
    echo "  status      Show service status"
    echo "  clean       Remove containers and volumes"
    echo
    echo "Examples:"
    echo "  $0 dev up          # Start development environment"
    echo "  $0 prod build      # Build production containers"
    echo "  $0 dev logs        # Show development logs"
    echo "  $0 prod down       # Stop production services"
}

# Function to set environment variables
set_environment() {
    local env=$1
    
    case $env in
        "dev")
            export COMPOSE_FILE="docker-compose.dev.yml"
            export ENV_FILE=".env.dev"
            print_status "Using development environment"
            ;;
        "prod")
            export COMPOSE_FILE="docker-compose.prod.yml"
            export ENV_FILE=".env.prod"
            print_status "Using production environment"
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
}

# Function to perform actions
perform_action() {
    local action=$1
    
    case $action in
        "up")
            print_status "Starting FlightBoard services..."
            docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d
            print_success "Services started successfully"
            print_status "Services will be available at:"
            echo "  - API: http://localhost:5183"
            echo "  - Consumer App: http://localhost:3000"
            echo "  - Backoffice App: http://localhost:3001"
            ;;
        "down")
            print_status "Stopping FlightBoard services..."
            docker-compose -f "$COMPOSE_FILE" down
            print_success "Services stopped successfully"
            ;;
        "build")
            print_status "Building FlightBoard containers..."
            docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build --no-cache
            print_success "Containers built successfully"
            ;;
        "logs")
            print_status "Showing service logs (Ctrl+C to exit)..."
            docker-compose -f "$COMPOSE_FILE" logs -f
            ;;
        "status")
            print_status "Service status:"
            docker-compose -f "$COMPOSE_FILE" ps
            ;;
        "clean")
            print_warning "This will remove all containers, volumes, and data!"
            read -p "Are you sure? (y/N): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                print_status "Cleaning up..."
                docker-compose -f "$COMPOSE_FILE" down -v
                docker system prune -f
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
    
    # Check Docker installation
    check_docker
    
    # Set environment
    set_environment "$environment"
    
    # Perform action
    perform_action "$action"
}

# Run main function with all arguments
main "$@"
