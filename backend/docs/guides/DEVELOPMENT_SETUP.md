# Development Setup Guide

## Prerequisites

- .NET 9.0 SDK or later
- PostgreSQL 15+
- Docker & Docker Compose (optional, for containerized development)
- Visual Studio 2022 or VS Code
- Git

## Environment Variables

Create `.env.development` in the backend directory:

```env
# Database
POSTGRES_CONNECTION_STRING=Server=localhost;Port=5432;Database=hranalytics_dev;User Id=postgres;Password=postgres;

# JWT
JWT_SECRET_KEY=your-very-long-secret-key-at-least-256-bits-long-for-HS256-algorithm
JWT_ISSUER=https://identityservice:5001
JWT_AUDIENCE=hranalytics

# Kafka
KAFKA_BROKERS=localhost:9092
KAFKA_TOPIC_PREFIX=hranalytics

# Logging
LOG_LEVEL=Information
```

## Setup Steps

### 1. Clone Repository
```bash
git clone <repository-url>
cd HR-platform/backend
```

### 2. Install Dependencies
```bash
dotnet restore HRAnalytics.sln
```

### 3. Database Setup
```bash
# Using Docker Compose
docker-compose up -d postgres kafka

# Or manually with PostgreSQL
createdb hranalytics_dev
```

### 4. Run Migrations
```bash
dotnet ef database update --project src/HR.Identity/HR.Identity.csproj --startup-project src/HR.Identity/HR.Identity.csproj
```

### 5. Build Solution
```bash
dotnet build HRAnalytics.sln
```

### 6. Run Tests
```bash
# Unit tests
dotnet test tests/HR.Tests.Unit/HR.Tests.Unit.csproj

# Integration tests
dotnet test tests/HR.Tests.Integration/HR.Tests.Integration.csproj
```

### 7. Start Services

```bash
# Terminal 1: API Gateway
cd src/HR.ApiGateway
dotnet run --urls "http://localhost:5000"

# Terminal 2: Identity Service
cd src/HR.Identity
dotnet run --urls "http://localhost:5001"

# Terminal 3: Employee Service
cd src/HR.Employee
dotnet run --urls "http://localhost:5002"

# Additional services on ports 5003-5009
```

## Running with Docker Compose

```bash
docker-compose up --build
```

Services will be available at:
- API Gateway: http://localhost:5000
- Identity: http://localhost:5001
- Employee: http://localhost:5002
- ... (5003-5009 for other services)

## Useful Commands

```bash
# Format code
dotnet format HRAnalytics.sln

# Run specific test
dotnet test --filter "ClassName=TestName"

# Generate EF migrations
dotnet ef migrations add MigrationName --project src/HR.Employee/HR.Employee.csproj

# View EF migrations
dotnet ef migrations list --project src/HR.Employee/HR.Employee.csproj
```

## Troubleshooting

**Port already in use**: Change ports in `appsettings.json` or use `--urls` flag

**Database connection failed**: Verify PostgreSQL is running and connection string is correct

**Kafka connection errors**: Ensure Kafka broker is accessible on configured port
