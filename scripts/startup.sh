#!/bin/bash

# HR Analytics Platform - Infrastructure Startup Script
# This script starts all Docker infrastructure and optionally runs microservices

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
COMPOSE_FILE="$PROJECT_ROOT/docker-compose.yml"

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}HR Analytics Platform - Infrastructure${NC}"
echo -e "${YELLOW}========================================${NC}"

# Function to print messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

# Check Docker installation
if ! command -v docker &> /dev/null; then
    log_error "Docker is not installed. Please install Docker first."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    log_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

log_info "Docker version: $(docker --version)"
log_info "Docker Compose version: $(docker-compose --version)"

# Parse command line arguments
COMMAND=${1:-start}

case $COMMAND in
    start)
        log_info "Starting infrastructure services..."
        cd "$PROJECT_ROOT"
        docker-compose up -d
        
        log_info "Waiting for services to be ready..."
        sleep 10
        
        log_info "Checking service health..."
        docker-compose ps
        
        log_info "Infrastructure started successfully!"
        log_info ""
        log_info "Available UIs:"
        log_info "  Seq (Logs):         http://localhost:8081"
        log_info "  Adminer (Database): http://localhost:8080"
        log_info "  Kibana:             http://localhost:5601"
        log_info "  Kafka UI:           http://localhost:8888"
        log_info ""
        log_info "Database Connection:"
        log_info "  Host:     localhost"
        log_info "  Port:     5432"
        log_info "  User:     postgres"
        log_info "  Password: postgres"
        log_info ""
        log_info "Next: Run microservices with 'dotnet run' in each service directory"
        ;;
        
    stop)
        log_info "Stopping infrastructure services..."
        cd "$PROJECT_ROOT"
        docker-compose down
        log_info "Infrastructure stopped."
        ;;
        
    restart)
        log_info "Restarting infrastructure services..."
        cd "$PROJECT_ROOT"
        docker-compose restart
        log_info "Infrastructure restarted."
        ;;
        
    logs)
        SERVICE=${2:-}
        cd "$PROJECT_ROOT"
        if [ -z "$SERVICE" ]; then
            docker-compose logs -f
        else
            docker-compose logs -f "$SERVICE"
        fi
        ;;
        
    ps)
        log_info "Service Status:"
        cd "$PROJECT_ROOT"
        docker-compose ps
        ;;
        
    clean)
        log_warn "This will remove all containers, networks, and volumes (database data will be deleted)!"
        read -p "Are you sure? (y/N) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            cd "$PROJECT_ROOT"
            docker-compose down -v
            log_info "Infrastructure cleaned."
        else
            log_info "Clean cancelled."
        fi
        ;;
        
    backup)
        log_info "Backing up PostgreSQL database..."
        BACKUP_FILE="$PROJECT_ROOT/backup_$(date +%Y%m%d_%H%M%S).sql"
        cd "$PROJECT_ROOT"
        docker-compose exec -T postgres pg_dump -U postgres > "$BACKUP_FILE"
        log_info "Backup created: $BACKUP_FILE"
        ;;
        
    help|--help|-h)
        echo "Usage: $0 [command]"
        echo ""
        echo "Commands:"
        echo "  start       - Start all infrastructure services (default)"
        echo "  stop        - Stop all infrastructure services"
        echo "  restart     - Restart all infrastructure services"
        echo "  ps          - Show service status"
        echo "  logs [svc]  - Show service logs (optional: service name)"
        echo "  clean       - Remove all containers and volumes (WARNING: deletes data)"
        echo "  backup      - Backup PostgreSQL database"
        echo "  help        - Show this help message"
        echo ""
        echo "Examples:"
        echo "  $0 start"
        echo "  $0 logs postgres"
        echo "  $0 logs kafka"
        ;;
        
    *)
        log_error "Unknown command: $COMMAND"
        echo "Run '$0 help' for usage information."
        exit 1
        ;;
esac

exit 0
