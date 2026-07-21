# DevOps & Deployment for Microservices

Comprehensive guide to containerization, Kubernetes orchestration, CI/CD pipelines, and monitoring.

---

## Containerization Strategy

### Docker Images for Each Service

```dockerfile
# Dockerfile - Employee Service
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /src

# Copy project files
COPY ["HRAnalytics.EmployeeService/HRAnalytics.EmployeeService.csproj", "HRAnalytics.EmployeeService/"]
COPY ["HRAnalytics.Shared/HRAnalytics.Shared.csproj", "HRAnalytics.Shared/"]

# Restore dependencies
RUN dotnet restore "HRAnalytics.EmployeeService/HRAnalytics.EmployeeService.csproj"

# Copy all source
COPY . .

# Build
RUN dotnet build "HRAnalytics.EmployeeService/HRAnalytics.EmployeeService.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "HRAnalytics.EmployeeService/HRAnalytics.EmployeeService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Copy from builder
COPY --from=builder /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD dotnet /app/HRAnalytics.EmployeeService.dll /health

# Expose ports
EXPOSE 5001 5101

# Run
ENTRYPOINT ["dotnet", "HRAnalytics.EmployeeService.dll"]
```

### Docker Compose - Local Development

```yaml
# docker-compose.yml
version: '3.9'

services:
  # Message Broker
  rabbitmq:
    image: rabbitmq:3.12-management
    container_name: rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 30s
      timeout: 10s
      retries: 5

  # PostgreSQL - Employee Service
  postgres-employee:
    image: postgres:15-alpine
    container_name: postgres-employee
    environment:
      POSTGRES_DB: hr_employee
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5432:5432"
    volumes:
      - employee-db-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  # PostgreSQL - Payroll Service
  postgres-payroll:
    image: postgres:15-alpine
    container_name: postgres-payroll
    environment:
      POSTGRES_DB: hr_payroll
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5433:5432"
    volumes:
      - payroll-db-data:/var/lib/postgresql/data

  # PostgreSQL - Attendance Service
  postgres-attendance:
    image: postgres:15-alpine
    container_name: postgres-attendance
    environment:
      POSTGRES_DB: hr_attendance
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5434:5432"
    volumes:
      - attendance-db-data:/var/lib/postgresql/data

  # Employee Service
  employee-service:
    build:
      context: .
      dockerfile: HRAnalytics.EmployeeService/Dockerfile
    container_name: employee-service
    ports:
      - "5001:5001"
      - "5101:5101"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5001;https://+:5101
      ConnectionStrings__DefaultConnection: "Host=postgres-employee;Port=5432;Database=hr_employee;Username=postgres;Password=password;"
      RabbitMQ__Url: "amqp://guest:guest@rabbitmq:5672/"
    depends_on:
      postgres-employee:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Payroll Service
  payroll-service:
    build:
      context: .
      dockerfile: HRAnalytics.PayrollService/Dockerfile
    container_name: payroll-service
    ports:
      - "5002:5002"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5002
      ConnectionStrings__DefaultConnection: "Host=postgres-payroll;Port=5432;Database=hr_payroll;Username=postgres;Password=password;"
      RabbitMQ__Url: "amqp://guest:guest@rabbitmq:5672/"
      EmployeeServiceUrl: "http://employee-service:5001"
    depends_on:
      postgres-payroll:
        condition: service_healthy
      employee-service:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  # API Gateway
  api-gateway:
    build:
      context: .
      dockerfile: HRAnalytics.ApiGateway/Dockerfile
    container_name: api-gateway
    ports:
      - "80:80"
      - "443:443"
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:80
      EmployeeServiceUrl: "http://employee-service:5001"
      PayrollServiceUrl: "http://payroll-service:5002"
      AttendanceServiceUrl: "http://attendance-service:5003"
    depends_on:
      - employee-service
      - payroll-service

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: redis
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  employee-db-data:
  payroll-db-data:
  attendance-db-data:

networks:
  default:
    name: hr-microservices
```

---

## Kubernetes Deployment

### Service Manifests

```yaml
# k8s/employee-service-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: employee-service
  namespace: hr-analytics
  labels:
    app: employee-service
    version: v1

spec:
  replicas: 3
  
  selector:
    matchLabels:
      app: employee-service
  
  template:
    metadata:
      labels:
        app: employee-service
        version: v1
    
    spec:
      # Service account for RBAC
      serviceAccountName: employee-service
      
      containers:
      - name: employee-service
        image: registry.example.com/hr-analytics/employee-service:latest
        imagePullPolicy: IfNotPresent
        
        ports:
        - name: http
          containerPort: 5001
          protocol: TCP
        - name: grpc
          containerPort: 5101
          protocol: TCP
        
        # Environment variables
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ASPNETCORE_URLS
          value: http://+:5001;http://+:5101
        
        # Database connection from secret
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: employee-db-secret
              key: connection-string
        
        # Message broker
        - name: RabbitMQ__Url
          valueFrom:
            configMapKeyRef:
              name: rabbitmq-config
              key: url
        
        # Service discovery
        - name: ServiceRegistry__ConsulUrl
          value: http://consul:8500
        
        # Logging
        - name: Serilog__MinimumLevel
          value: Information
        - name: Serilog__ElasticsearchUrl
          value: http://elasticsearch:9200
        
        # Resource limits
        resources:
          requests:
            cpu: 250m
            memory: 512Mi
          limits:
            cpu: 500m
            memory: 1Gi
        
        # Health checks
        livenessProbe:
          httpGet:
            path: /health/live
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        readinessProbe:
          httpGet:
            path: /health/ready
            port: http
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        
        # Startup probe (slow to start)
        startupProbe:
          httpGet:
            path: /health
            port: http
          failureThreshold: 30
          periodSeconds: 2
        
        # Security context
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          runAsNonRoot: true
          runAsUser: 1000
        
        # Volume mounts
        volumeMounts:
        - name: tmp
          mountPath: /tmp
        - name: app-logs
          mountPath: /app/logs
      
      # Pod disruption budget for high availability
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - employee-service
              topologyKey: kubernetes.io/hostname
      
      volumes:
      - name: tmp
        emptyDir: {}
      - name: app-logs
        emptyDir: {}

---
# Service
apiVersion: v1
kind: Service
metadata:
  name: employee-service
  namespace: hr-analytics

spec:
  type: ClusterIP
  selector:
    app: employee-service
  
  ports:
  - name: http
    port: 5001
    targetPort: 5001
    protocol: TCP
  - name: grpc
    port: 5101
    targetPort: 5101
    protocol: TCP

---
# Horizontal Pod Autoscaler
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: employee-service-hpa
  namespace: hr-analytics

spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: employee-service
  
  minReplicas: 3
  maxReplicas: 10
  
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80

---
# Network Policy
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: employee-service-netpol
  namespace: hr-analytics

spec:
  podSelector:
    matchLabels:
      app: employee-service
  
  policyTypes:
  - Ingress
  - Egress
  
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: hr-analytics
    - podSelector:
        matchLabels:
          app: api-gateway
    ports:
    - protocol: TCP
      port: 5001
    - protocol: TCP
      port: 5101
  
  egress:
  - to:
    - namespaceSelector:
        matchLabels:
          name: hr-analytics
    - podSelector:
        matchLabels:
          app: postgres-employee
    ports:
    - protocol: TCP
      port: 5432
  
  - to:
    - podSelector:
        matchLabels:
          app: rabbitmq
    ports:
    - protocol: TCP
      port: 5672
```

---

## CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/deploy-services.yml
name: Deploy Microservices

on:
  push:
    branches: [main, develop]
    paths:
      - 'HRAnalytics.EmployeeService/**'
      - 'HRAnalytics.PayrollService/**'
      - '.github/workflows/deploy-services.yml'

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    
    permissions:
      contents: read
      packages: write
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2
    
    - name: Log in to Container Registry
      uses: docker/login-action@v2
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    # Build Employee Service
    - name: Build and push Employee Service
      uses: docker/build-push-action@v4
      with:
        context: .
        file: ./HRAnalytics.EmployeeService/Dockerfile
        push: true
        tags: |
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/employee-service:latest
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/employee-service:${{ github.sha }}
    
    # Build Payroll Service
    - name: Build and push Payroll Service
      uses: docker/build-push-action@v4
      with:
        context: .
        file: ./HRAnalytics.PayrollService/Dockerfile
        push: true
        tags: |
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/payroll-service:latest
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/payroll-service:${{ github.sha }}

  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: test
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: password
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Run tests
      run: dotnet test --configuration Release --no-build --verbosity normal

  deploy:
    needs: [build-and-push, test]
    runs-on: ubuntu-latest
    
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup kubectl
      uses: azure/setup-kubectl@v3
      with:
        version: 'v1.28.0'
    
    - name: Deploy to Kubernetes
      run: |
        mkdir -p $HOME/.kube
        echo "${{ secrets.KUBE_CONFIG }}" | base64 -d > $HOME/.kube/config
        
        # Apply manifests
        kubectl apply -f k8s/
        
        # Update image
        kubectl set image deployment/employee-service \
          employee-service=${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/employee-service:${{ github.sha }} \
          -n hr-analytics
        
        # Wait for rollout
        kubectl rollout status deployment/employee-service -n hr-analytics
    
    - name: Verify deployment
      run: |
        kubectl get pods -n hr-analytics
        kubectl get services -n hr-analytics
```

---

## Monitoring & Observability

### Prometheus + Grafana

```yaml
# k8s/prometheus-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
  namespace: hr-analytics

data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
      evaluation_interval: 15s
    
    scrape_configs:
    - job_name: 'kubernetes-pods'
      kubernetes_sd_configs:
      - role: pod
        namespaces:
          names:
          - hr-analytics
      
      relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__
```

### Application Metrics

```csharp
// Program.cs - Add Prometheus metrics
services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

app.MapPrometheusScrapingEndpoint();
```

---

**Last Updated:** July 2026
**Status:** DevOps & Deployment Complete
