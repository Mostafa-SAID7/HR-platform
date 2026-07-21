.PHONY: help infra-up infra-down infra-logs infra-ps infra-clean \
        build run test migrate logs health backup restore

# Default target
help:
	@echo "HR Analytics Platform - Make targets"
	@echo ""
	@echo "Infrastructure:"
	@echo "  make infra-up         - Start Docker infrastructure"
	@echo "  make infra-down       - Stop Docker infrastructure"
	@echo "  make infra-restart    - Restart Docker infrastructure"
	@echo "  make infra-ps         - Show infrastructure status"
	@echo "  make infra-logs       - Show infrastructure logs"
	@echo "  make infra-clean      - Remove infrastructure (WARNING: deletes data)"
	@echo ""
	@echo "Development:"
	@echo "  make build            - Build solution"
	@echo "  make run-gateway      - Run API Gateway"
	@echo "  make run-identity     - Run Identity Service"
	@echo "  make run-employee     - Run Employee Service"
	@echo "  make run-performance  - Run Performance Service"
	@echo "  make run-attendance   - Run Attendance Service"
	@echo "  make run-payroll      - Run Payroll Service"
	@echo ""
	@echo "Database:"
	@echo "  make migrate          - Run EF Core migrations"
	@echo "  make backup           - Backup PostgreSQL database"
	@echo "  make restore          - Restore PostgreSQL database (specify BACKUP_FILE)"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  make test             - Run unit tests"
	@echo "  make test-integration - Run integration tests"
	@echo ""
	@echo "Utilities:"
	@echo "  make health           - Check all services health"
	@echo "  make logs             - Show all logs (specify SERVICE for specific)"
	@echo "  make clean            - Remove build artifacts"

# Infrastructure targets
infra-up:
	@echo "Starting infrastructure..."
	docker-compose up -d
	@echo "Waiting for services..."
	sleep 10
	@echo "Infrastructure started!"
	@echo "Visit http://localhost:8081 for Seq logs"

infra-down:
	@echo "Stopping infrastructure..."
	docker-compose down

infra-restart:
	@echo "Restarting infrastructure..."
	docker-compose restart

infra-ps:
	docker-compose ps

infra-logs:
	docker-compose logs -f $(SERVICE)

infra-clean:
	@echo "WARNING: This will delete all data!"
	@read -p "Continue? [y/N] " -n 1 -r; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		docker-compose down -v; \
		echo "Infrastructure cleaned."; \
	fi

# Build target
build:
	@echo "Building solution..."
	dotnet build HRAnalytics.sln
	@echo "Build complete!"

# Run targets
run-gateway:
	dotnet run --project src/HR.ApiGateway/HR.ApiGateway.csproj

run-identity:
	dotnet run --project src/HR.Identity/HR.Identity.csproj

run-employee:
	dotnet run --project src/HR.Employee/HR.Employee.csproj

run-performance:
	dotnet run --project src/HR.Performance/HR.Performance.csproj

run-attendance:
	dotnet run --project src/HR.Attendance/HR.Attendance.csproj

run-payroll:
	dotnet run --project src/HR.Payroll/HR.Payroll.csproj

# Database targets
migrate:
	@echo "Running migrations for all services..."
	dotnet ef database update --project src/HR.Identity/HR.Identity.csproj
	dotnet ef database update --project src/HR.Employee/HR.Employee.csproj
	dotnet ef database update --project src/HR.Performance/HR.Performance.csproj
	dotnet ef database update --project src/HR.Attendance/HR.Attendance.csproj
	dotnet ef database update --project src/HR.Payroll/HR.Payroll.csproj
	@echo "Migrations complete!"

backup:
	@echo "Backing up database..."
	docker-compose exec -T postgres pg_dump -U postgres > backup_$$(date +%Y%m%d_%H%M%S).sql
	@echo "Backup complete!"

restore:
	@echo "Restoring database from $(BACKUP_FILE)..."
	docker-compose exec -T postgres psql -U postgres < $(BACKUP_FILE)
	@echo "Restore complete!"

# Testing targets
test:
	@echo "Running unit tests..."
	dotnet test HRAnalytics.sln --filter "Category!=Integration"

test-integration:
	@echo "Running integration tests..."
	dotnet test HRAnalytics.sln --filter "Category=Integration"

# Health check
health:
	@echo "Checking infrastructure health..."
	docker-compose ps
	@echo ""
	@echo "PostgreSQL:"
	@docker-compose exec -T postgres pg_isready -U postgres || echo "Not ready"
	@echo "Redis:"
	@docker-compose exec redis redis-cli ping || echo "Not ready"
	@echo "Kafka:"
	@docker-compose exec kafka kafka-broker-api-versions --bootstrap-server localhost:9092 || echo "Not ready"
	@echo "Elasticsearch:"
	@curl -s http://localhost:9200/_cluster/health | grep -q "status" && echo "OK" || echo "Not ready"

# Log targets
logs:
	@if [ -z "$(SERVICE)" ]; then \
		docker-compose logs -f; \
	else \
		docker-compose logs -f $(SERVICE); \
	fi

# Cleanup targets
clean:
	@echo "Cleaning build artifacts..."
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true
	@echo "Clean complete!"
