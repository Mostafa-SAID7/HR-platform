# Scaling Strategy

## Horizontal Scaling

### Stateless Service Design

All services are stateless:
- No in-memory caches (use Redis)
- No session storage locally
- All state in database/external storage

### Load Balancing

```yaml
# Kubernetes service
apiVersion: v1
kind: Service
metadata:
  name: employee-service
spec:
  selector:
    app: employee
  ports:
  - port: 80
    targetPort: 5002
  type: LoadBalancer
```

### Auto-scaling Configuration

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: employee-service-hpa
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
```

## Database Scaling

### Read Replicas

```csharp
// Primary database for writes
var primaryConnection = "Server=primary.db.internal:5432;...";

// Replica for reads
var replicaConnection = "Server=replica.db.internal:5432;...";

// Read-only operations use replica
var analytics = await replicaContext.Employees.ToListAsync();

// Write operations use primary
await primaryContext.Employees.AddAsync(employee);
```

### Connection Pooling

```
Min Pool: 10 connections
Max Pool: 50 connections
Max Pool Size = (Number of CPU cores) × (Workload multiplier)
For 8 cores: 8 × (2-3) = 16-24 connections
```

### Partitioning Strategy

Partition large tables by tenant:

```sql
-- Create partitions by tenant_id
CREATE TABLE employees_partition (
  id UUID,
  tenant_id UUID,
  first_name VARCHAR(100),
  ...
) PARTITION BY LIST (tenant_id);

-- Create partition for each tenant
CREATE TABLE employees_tenant_1 PARTITION OF employees_partition
  FOR VALUES IN ('tenant-1-uuid');
```

## Message Queue Scaling

### Kafka Topics

Configure for throughput:

```bash
# Create topics with replication and partitions
kafka-topics.sh --create \
  --topic employee.created \
  --partitions 10 \
  --replication-factor 3 \
  --bootstrap-server kafka:9092
```

### Consumer Groups

```csharp
// Multiple consumer instances
// Each processes different partitions
services.AddKafka(options =>
{
    options.ConsumerGroupId = "analytics-service";
    options.Partitions = 10;  // Process 10 partitions in parallel
});
```

## Caching Strategy

### Redis Cluster

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis-cluster-node1:6379,redis-cluster-node2:6379,redis-cluster-node3:6379";
});
```

### Cache Layers

1. **API Gateway Cache** (1 minute)
   - Health checks
   - Service discovery

2. **Service Cache** (5 minutes)
   - Department lookups
   - Reference data

3. **Database Query Cache** (30 seconds)
   - Frequently accessed data
   - Aggregation results

## Load Patterns

### Peak Hours

- Payroll processing: 8-9 AM
- Performance reviews: Week 1 of month
- Reporting: End of month

**Strategy**:
- Scale up before known peaks
- Use predictive scaling
- Queue heavy operations during off-peak

### Bulk Operations

```csharp
// Queue bulk payroll processing
BackgroundJob.Enqueue(() => ProcessBulkPayroll(year, month));

// Process asynchronously
services.AddHangfire(config => 
{
    config.UsePostgreSqlStorage(connectionString);
    config.UseSimpleAssemblyNameTypeSerializer();
    config.UseRecommendedSerializationSettings();
    config.GlobalJobFilters.Add(new AutomaticRetryAttribute { Attempts = 3 });
});
```

## Monitoring Scaling

### Metrics to Track

- CPU utilization per service
- Memory usage
- Request latency (p95, p99)
- Database connection pool usage
- Kafka consumer lag
- Cache hit ratio

### Scaling Triggers

```yaml
Trigger         Value            Action
CPU Utilization > 70%            Scale up by 1 pod
Memory Usage    > 80%            Scale up by 1 pod
Request Lag     > 1 second       Scale up by 2 pods
DB Connections  > 80% of max     Alert + Scale up
Kafka Consumer  > 10 seconds lag Scale up consumers
Cache Hit Rate  < 60%            Increase cache TTL
```

## Cost Optimization

### Resource Limits

```yaml
apiVersion: v1
kind: Container
metadata:
  name: employee-service
spec:
  resources:
    requests:
      memory: "256Mi"
      cpu: "250m"
    limits:
      memory: "512Mi"
      cpu: "500m"
```

### Spot Instances

Use for non-critical services:
- Analytics service
- Report generation
- Notification service

## Disaster Recovery

### Data Backup

```bash
# Daily backups
0 2 * * * pg_dump hranalytics | gzip > /backups/hranalytics_$(date +%Y%m%d).sql.gz

# Retention: 30 days
find /backups -name "*.sql.gz" -mtime +30 -delete
```

### Replication

```sql
-- Streaming replication
wal_level = replica
max_wal_senders = 10
wal_keep_size = 1GB
```

### Failover Strategy

```yaml
Master  → Primary Database
            ↓
         Streaming Replication
            ↓
Slave   ← Standby Database

On failure: Promote Slave → New Master
```

## Regional Scaling

For global deployments:

```
Region 1 (US-East):
- Primary Database
- 3+ replicas
- Kafka cluster

Region 2 (EU-West):
- Read Replica
- Secondary Kafka cluster
- Delayed sync
```

## Testing at Scale

### Load Testing

```bash
# Generate 10,000 requests with 100 concurrent users
wrk -t12 -c100 -d30s http://localhost:5000/employee/list

# k6 script for complex scenarios
k6 run --vus 50 --duration 30s load-test.js
```

### Performance Targets

| Metric | Target | Current |
|--------|--------|---------|
| p95 latency | <200ms | - |
| p99 latency | <500ms | - |
| Error rate | <0.1% | - |
| Throughput | >1000 req/sec | - |
| Database CPU | <70% | - |

## Recommended Configuration by Scale

### Small (< 100 employees)

- 1 Kubernetes cluster
- PostgreSQL: 1 primary
- 1 Kafka broker
- 2 replicas per service

### Medium (100-1000 employees)

- 2 Kubernetes clusters (main + standby)
- PostgreSQL: 1 primary + 2 replicas
- 3 Kafka brokers
- 3 replicas per service
- Redis cluster for caching

### Large (> 1000 employees)

- 3+ Kubernetes clusters (multi-region)
- PostgreSQL: 1 primary + 4+ replicas, partitioning
- 5+ Kafka brokers
- 5+ replicas per service
- Redis cluster + memcached
- CDN for static content
