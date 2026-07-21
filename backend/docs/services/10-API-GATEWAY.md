# API Gateway Service

**Port**: 5000 | **Status**: ✅ Production Ready | **Technology**: YARP (Yet Another Reverse Proxy) | **Pattern**: Gateway Pattern

## Overview

The API Gateway serves as the single entry point for all client requests, handling routing, authentication, rate limiting, request logging, and cross-cutting concerns for the entire microservices architecture.

## Key Features

- ✅ Request routing to backend services
- ✅ JWT authentication and authorization
- ✅ Rate limiting per client/user
- ✅ Request/response logging
- ✅ CORS policy enforcement
- ✅ Request timeout handling
- ✅ Health check aggregation
- ✅ Request tracing and tracking
- ✅ API versioning support

## Service Routes

| Path | Service | Port |
|------|---------|------|
| `/identity/*` | Identity Service | 5001 |
| `/employees/*` | Employee Service | 5002 |
| `/performance/*` | Performance Service | 5003 |
| `/recruitment/*` | Recruitment Service | 5004 |
| `/attendance/*` | Attendance Service | 5005 |
| `/payroll/*` | Payroll Service | 5006 |
| `/analytics/*` | Analytics Service | 5007 |
| `/notifications/*` | Notification Service | 5008 |
| `/audit/*` | Audit Service | 5009 |
| `/health/*` | Health checks | All |

## Architecture

```
Client Request
    ↓
[CORS Middleware]
    ↓
[Authentication Middleware]
    ↓
[Rate Limiting Middleware]
    ↓
[Request Logging Middleware]
    ↓
[YARP Router]
    ↓
[Backend Service]
```

## API Endpoints

### Gateway Health

```
GET /health
GET /health/live
GET /health/ready
GET /health/services
```

### Health Response

```json
{
  "status": "Healthy",
  "timestamp": "2026-07-21T10:30:00Z",
  "services": {
    "identity": "Healthy",
    "employee": "Healthy",
    "payroll": "Healthy",
    "analytics": "Healthy"
  }
}
```

## Middleware Components

### 1. Authentication Middleware

- Validates JWT tokens in `Authorization` header
- Extracts claims and user context
- Returns 401 for missing/invalid tokens

### 2. Rate Limiting Middleware

- Per-client rate limiting (IP-based or User ID)
- Default limit: 1000 requests/hour
- Returns 429 (Too Many Requests) when exceeded
- Rate limit info in response headers

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 975
X-RateLimit-Reset: 1626859800
```

### 3. Request Logging Middleware

- Logs all requests with:
  - Method, Path, Status Code
  - Response time
  - User ID
  - Trace ID

### 4. CORS Middleware

- Configured allowed origins:
  - `http://localhost:4200` (frontend dev)
  - `http://localhost:3000` (alternative frontend)
  - Production domain (configured in appsettings)

## Configuration

### appsettings.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  },
  "ReverseProxy": {
    "Routes": {
      "identity": {
        "ClusterId": "identity_cluster",
        "Match": {
          "Path": "/identity/{**catch-all}"
        }
      },
      "employee": {
        "ClusterId": "employee_cluster",
        "Match": {
          "Path": "/employees/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "identity_cluster": {
        "Destinations": {
          "identity": {
            "Address": "http://localhost:5001"
          }
        }
      },
      "employee_cluster": {
        "Destinations": {
          "employee": {
            "Address": "http://localhost:5002"
          }
        }
      }
    }
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerHour": 1000,
    "RequestsPerMinute": 100
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200", "http://localhost:3000"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Authorization", "Content-Type"],
    "ExposedHeaders": ["X-RateLimit-Limit", "X-RateLimit-Remaining"]
  },
  "Timeout": {
    "RequestTimeoutSeconds": 30
  }
}
```

## Integration Examples

### Login via Gateway

```bash
curl -X POST http://localhost:5000/identity/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

### Get Employee via Gateway

```bash
curl "http://localhost:5000/employees/{employeeId}" \
  -H "Authorization: Bearer {accessToken}"
```

### Rate Limit Headers

```bash
curl -X GET http://localhost:5000/employees \
  -H "Authorization: Bearer token" \
  -v

# Response headers
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 990
X-RateLimit-Reset: 1626859800
```

## Error Responses

### 401 Unauthorized

```json
{
  "error": "Unauthorized",
  "message": "Missing or invalid authorization token",
  "traceId": "trace-123"
}
```

### 429 Too Many Requests

```json
{
  "error": "TooManyRequests",
  "message": "Rate limit exceeded. Max 1000 requests per hour",
  "retryAfter": 3600
}
```

### 504 Gateway Timeout

```json
{
  "error": "GatewayTimeout",
  "message": "Request timeout after 30 seconds",
  "traceId": "trace-456"
}
```

## Testing

```bash
dotnet test tests/HR.Tests.Unit/ApiGateway/
dotnet test tests/HR.Tests.Integration/ApiGateway/
```

## Load Balancing

YARP supports round-robin load balancing for multiple backend instances:

```json
{
  "Clusters": {
    "employee_cluster": {
      "Destinations": {
        "employee1": { "Address": "http://localhost:5002" },
        "employee2": { "Address": "http://localhost:5022" },
        "employee3": { "Address": "http://localhost:5032" }
      },
      "LoadBalancingPolicy": "RoundRobin"
    }
  }
}
```

## Monitoring

Monitor gateway health:

```bash
# Check if gateway is running
curl http://localhost:5000/health

# Check all service health
curl http://localhost:5000/health/services

# Check readiness for traffic
curl http://localhost:5000/health/ready
```

## Dependencies

- YARP (reverse proxy)
- JWT Bearer Authentication
- AspNetCore.RateLimiting
- Serilog (logging)
- CorsPolicies

## Related Documentation

- [SECURITY_BEST_PRACTICES.md](../operations/SECURITY_BEST_PRACTICES.md)
- [MONITORING_AND_LOGGING.md](../operations/MONITORING_AND_LOGGING.md)
- [DEPLOYMENT_GUIDE.md](../operations/DEPLOYMENT_GUIDE.md)
