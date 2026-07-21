# Deployment Guide

## Overview

HR Analytics Platform uses containerized microservices deployed on Kubernetes or Docker Swarm.

## Pre-Deployment Checklist

- [ ] All tests passing (unit + integration)
- [ ] Docker images built and tested
- [ ] Database migrations prepared
- [ ] Environment variables configured
- [ ] SSL certificates available
- [ ] Backup strategy in place

## Docker Build

```bash
# Build all services
docker-compose build

# Build specific service
docker build -f src/HR.Identity/Dockerfile -t hranalytics-identity:latest .
```

## Kubernetes Deployment

### 1. Create Namespace
```bash
kubectl create namespace hranalytics
```

### 2. Configure Secrets
```bash
kubectl create secret generic hranalytics-secrets \
  --from-literal=postgres-password=<password> \
  --from-literal=jwt-secret=<secret-key> \
  -n hranalytics
```

### 3. Deploy PostgreSQL
```bash
kubectl apply -f k8s/postgres-statefulset.yaml -n hranalytics
kubectl apply -f k8s/postgres-service.yaml -n hranalytics
```

### 4. Deploy Services
```bash
# Apply all service manifests
kubectl apply -f k8s/services/ -n hranalytics

# Monitor deployment
kubectl get pods -n hranalytics
kubectl logs -f <pod-name> -n hranalytics
```

### 5. Configure Ingress
```bash
kubectl apply -f k8s/ingress.yaml -n hranalytics
```

## Health Checks

```bash
# Check all services
curl http://localhost:5000/health

# Check specific service
curl http://localhost:5001/health
```

## Database Migration

```bash
# Create backup
pg_dump hranalytics > backup_$(date +%Y%m%d_%H%M%S).sql

# Apply migrations
dotnet ef database update --project src/HR.Identity/HR.Identity.csproj

# Rollback if needed
dotnet ef database update <previous-migration-name>
```

## Monitoring

- **Logs**: Aggregate via ELK stack or Datadog
- **Metrics**: Prometheus endpoints at `/metrics`
- **Traces**: Distributed tracing via Jaeger (optional)

## Rollback Strategy

```bash
# Revert to previous image version
kubectl set image deployment/hranalytics-identity \
  identity=hranalytics-identity:v1.0.0 \
  -n hranalytics
```

## Production Configuration

See `appsettings.Production.json` for production-specific settings including:
- Connection string pooling
- Logging levels
- Kafka configuration
- Cache settings
- Rate limiting
