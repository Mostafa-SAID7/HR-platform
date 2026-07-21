# HR Analytics Platform - Documentation Index

**Version**: 2.0 | **Last Updated**: July 21, 2026 | **Status**: ✅ Complete

Complete documentation for the HR Analytics Platform ASP.NET Core 9 microservices backend.

## 📚 Documentation Structure

### Architecture & Design

- [Infrastructure Overview](architecture/INFRASTRUCTURE.md) - System architecture, deployment infrastructure
- [Kafka Integration](architecture/KAFKA_INTEGRATION.md) - Event-driven architecture, Kafka setup

### Services Documentation

All 10 microservices with complete API documentation, domain models, and integration examples:

#### Core Services

1. **[Identity Service](services/01-IDENTITY-SERVICE.md)** (Port 5001)
   - JWT authentication, RBAC, OAuth2
   - User management, token refresh, account lockout

2. **[Employee Service](services/02-EMPLOYEE-SERVICE.md)** (Port 5002)
   - Employee CRUD, lifecycle management
   - Skills tracking, department assignments

3. **[Performance Service](services/03-PERFORMANCE-SERVICE.md)** (Port 5003)
   - Performance reviews, feedback collection
   - Rating system, approval workflows

4. **[Recruitment Service](services/04-RECRUITMENT-SERVICE.md)** (Port 5004)
   - Job postings, application management
   - Candidate pipeline, interview tracking

5. **[Attendance Service](services/05-ATTENDANCE-SERVICE.md)** (Port 5005)
   - Real-time check-in/check-out
   - Leave requests, work hour calculations

6. **[Payroll Service](services/06-PAYROLL-SERVICE.md)** (Port 5006)
   - Complex salary calculations, tax engine
   - Approval workflows, payment processing

#### Analytics & Supporting Services

7. **[Analytics Service](services/07-ANALYTICS-SERVICE.md)** (Port 5007)
   - Employee metrics, salary analytics
   - Turnover rates, performance trends

8. **[Notification Service](services/08-NOTIFICATION-SERVICE.md)** (Port 5008)
   - Multi-channel notifications (Email, SMS, In-App)
   - Template-based messaging, preferences

9. **[Audit Service](services/09-AUDIT-SERVICE.md)** (Port 5009)
   - Centralized audit logging
   - Compliance reporting, change history

10. **[API Gateway](services/10-API-GATEWAY.md)** (Port 5000)
    - Request routing and load balancing
    - Authentication, rate limiting, health checks

### Guides

- [Development Setup](guides/DEVELOPMENT_SETUP.md) - Local environment setup, prerequisites
- [Testing Guide](guides/TESTING_GUIDE.md) - Unit and integration testing

### Operations

- [Deployment Guide](operations/DEPLOYMENT_GUIDE.md) - Production deployment, CI/CD
- [Monitoring & Logging](operations/MONITORING_AND_LOGGING.md) - Observability, alerts
- [Security Best Practices](operations/SECURITY_BEST_PRACTICES.md) - Security hardening, compliance
- [Performance Tuning](operations/PERFORMANCE_TUNING.md) - Optimization, caching strategies
- [Troubleshooting](operations/TROUBLESHOOTING.md) - Common issues and solutions
- [Scaling Strategy](operations/SCALING_STRATEGY.md) - Horizontal scaling, load balancing

### Service Overview

- [Microservices Status](services/MICROSERVICES_STATUS.md) - Current status of all services, ports, features

## 🚀 Quick Start

### Prerequisites

- .NET 9 SDK
- PostgreSQL 15+
- Docker & Docker Compose
- Node.js 18+ (frontend)
- Kafka 3.0+

### Local Development

```bash
# 1. Clone and navigate
cd backend

# 2. Setup local environment
cp .env.example .env

# 3. Start infrastructure
docker-compose up -d

# 4. Build solution
dotnet build HRAnalytics.sln

# 5. Run tests
dotnet test tests/HR.Tests.Unit/
dotnet test tests/HR.Tests.Integration/

# 6. Start services (individual terminals)
dotnet run --project src/HR.ApiGateway
dotnet run --project src/HR.Identity
# ... etc for other services
```

## 📊 Architecture Overview

```
┌─────────────────────────────────────────────┐
│  Client / Frontend (http://localhost:4200)  │
└────────────────┬────────────────────────────┘
                 │
                 ▼
         ┌───────────────┐
         │  API Gateway  │ Port 5000
         │    (YARP)     │
         └───────┬───────┘
                 │
    ┌────────────┼────────────┬──────────────┬───────────┐
    ▼            ▼            ▼              ▼           ▼
┌─────────┐ ┌────────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐
│Identity │ │ Employee   │ │Performance│ │Attendance│ │Payroll │
│Service  │ │ Service    │ │Service   │ │Service   │ │Service │
│(5001)   │ │ (5002)     │ │ (5003)   │ │ (5005)   │ │(5006)  │
└─────────┘ └────────────┘ └──────────┘ └──────────┘ └────────┘
    │            │             │             │           │
└─────────────────────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┐
         ▼           ▼           ▼
      ┌─────────┐ ┌─────────┐ ┌────────┐
      │Analytics│ │Notification│ │Audit  │
      │(5007)   │ │(5008)    │ │(5009) │
      └─────────┘ └─────────┘ └────────┘
         │           │           │
         └───────────┼───────────┘
                     │
         ┌───────────▼───────────┐
         │ Kafka Event Bus       │
         │ • Outbox Pattern      │
         │ • Event Sourcing      │
         │ • Saga Orchestration  │
         └───────────────────────┘
                     │
         ┌───────────▼───────────┐
         │ Data Stores           │
         │ • PostgreSQL          │
         │ • Elasticsearch       │
         │ • Redis Cache         │
         │ • Snowflake (BI)      │
         └───────────────────────┘
```

## 🔑 Key Technologies

- **Framework**: ASP.NET Core 9
- **Architecture**: CQRS + Clean Architecture
- **Patterns**: Domain-Driven Design, Event Sourcing, Saga
- **Messaging**: Apache Kafka, Kafkaflow
- **Database**: PostgreSQL, Elasticsearch, Snowflake
- **Testing**: xUnit, Testcontainers, Moq
- **Reverse Proxy**: YARP (Yet Another Reverse Proxy)
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT, OAuth2, RBAC
- **API Documentation**: OpenAPI/Swagger

## 📈 Service Ports

| Service | Port | Status |
|---------|------|--------|
| API Gateway | 5000 | ✅ Active |
| Identity | 5001 | ✅ Active |
| Employee | 5002 | ✅ Active |
| Performance | 5003 | ✅ Active |
| Recruitment | 5004 | ✅ Active |
| Attendance | 5005 | ✅ Active |
| Payroll | 5006 | ✅ Active |
| Analytics | 5007 | ✅ Active |
| Notification | 5008 | ✅ Active |
| Audit | 5009 | ✅ Active |

## 🧪 Testing

### Test Coverage

- **Unit Tests**: 107 tests across all services
- **Integration Tests**: 66 tests with database + Kafka
- **Total**: 173 tests (100% critical path coverage)

```bash
# Run all tests
dotnet test tests/

# Run specific service tests
dotnet test tests/HR.Tests.Unit/Identity/
dotnet test tests/HR.Tests.Integration/Employee/

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📝 Common Tasks

### Adding a New Service

1. Create service project: `src/HR.{ServiceName}`
2. Implement domain, commands, queries
3. Add endpoints in `Program.cs`
4. Create tests in `tests/HR.Tests.Unit/{ServiceName}`
5. Update Kafka topics and consumers
6. Document in `docs/services/{ServiceName}.md`

### Debugging Issues

See [Troubleshooting](operations/TROUBLESHOOTING.md) for common issues and solutions.

### Performance Optimization

See [Performance Tuning](operations/PERFORMANCE_TUNING.md) for caching, indexing, and query optimization.

### Deployment

See [Deployment Guide](operations/DEPLOYMENT_GUIDE.md) for production deployment procedures.

## 🔒 Security

- All endpoints require JWT authentication
- Rate limiting enabled (1000 req/hour default)
- CORS configured for frontend origins
- All passwords hashed with BCrypt
- Audit logging for compliance
- See [Security Best Practices](operations/SECURITY_BEST_PRACTICES.md)

## 📞 Support

- **Documentation**: See relevant guides above
- **Issues**: Check [Troubleshooting](operations/TROUBLESHOOTING.md)
- **Testing**: Run `dotnet test` to verify setup

## 📄 License

All rights reserved © 2026 HR Analytics Platform
