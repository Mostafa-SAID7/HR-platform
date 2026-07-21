# Monitoring and Logging Guide

## Logging Strategy

### Structured Logging

All services use Serilog for structured, JSON-formatted logging:

```csharp
logger.Information("Employee {EmployeeId} created by {UserId}", employeeId, userId);
```

### Log Levels

- **Fatal**: System shutdown required
- **Error**: Operation failed, needs investigation
- **Warning**: Unusual but recoverable condition
- **Information**: Normal operation flow
- **Debug**: Detailed diagnostic information

### Log Aggregation

Configure ELK Stack or similar:

```yaml
# Dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Serilog:MinimumLevel:Default=Information
```

## Application Monitoring

### Metrics

Prometheus-compatible metrics endpoint: `/metrics`

Key metrics:
- Request duration (p50, p95, p99)
- Error rate by endpoint
- Database query duration
- Kafka message processing time
- Thread count and memory usage

### Health Checks

```bash
GET /health           # Overall system health
GET /health/ready     # Readiness probe
GET /health/live      # Liveness probe
```

Response format:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "kafka": "Healthy",
    "cache": "Healthy"
  }
}
```

## Alerting Rules

### Critical Alerts

- Service down (liveness check fails)
- Database connection lost
- High error rate (>5% of requests)
- Kafka broker unreachable

### Warning Alerts

- Response time > 1000ms (p95)
- Error rate > 1% of requests
- Memory usage > 80%
- Connection pool exhaustion

## Distributed Tracing

Enable Jaeger for distributed tracing:

```csharp
services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        }));
```

## Performance Monitoring

### Database

- Connection pool size: 20-50
- Query execution time: Track slow queries
- Index usage: Monitor query plans

### API Gateway

- Request throughput: Monitor requests/sec
- Latency distribution: Track by endpoint
- Cache hit rate: Monitor gateway cache

## Alerting Configuration

Example Prometheus alert rules:

```yaml
alert: ServiceDown
  expr: up{job="hranalytics"} == 0
  for: 1m
  
alert: HighErrorRate
  expr: rate(requests_total{status=~"5.."}[5m]) > 0.05
  for: 5m
```

## Dashboard Setup

Create Grafana dashboards showing:
1. Request rates and latencies
2. Error rates by service
3. Database connection pool
4. Kafka lag by topic
5. Memory and CPU usage
