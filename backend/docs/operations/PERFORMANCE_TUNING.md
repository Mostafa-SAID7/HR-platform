# Performance Tuning Guide

## Database Optimization

### Connection Pooling

```csharp
// Optimized connection string
services.AddDbContext<HrContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
    }));
```

Configuration:
- Pool size: 20-50 connections
- Min pool size: 10
- Max pool size: 50
- Connection lifetime: 5 minutes

### Query Optimization

```csharp
// ✅ Include related entities
var employees = dbContext.Employees
    .Include(e => e.Department)
    .Include(e => e.Skills)
    .Where(e => e.IsActive)
    .ToList();

// ❌ N+1 queries
var employees = dbContext.Employees.ToList();
foreach (var emp in employees)
{
    var dept = dbContext.Departments.Find(emp.DepartmentId); // N additional queries!
}
```

### Indexing Strategy

Create indexes on frequently queried columns:

```sql
-- Filter conditions
CREATE INDEX idx_employee_tenant_active 
ON employees(tenant_id, is_active);

-- Join columns
CREATE INDEX idx_employee_department 
ON employees(department_id);

-- Sort columns
CREATE INDEX idx_payroll_employee_date 
ON payroll_records(employee_id, year, month);
```

### Query Pagination

```csharp
// Load only needed pages
var page = 1;
var pageSize = 20;

var employees = dbContext.Employees
    .Where(e => e.TenantId == tenantId)
    .OrderBy(e => e.FirstName)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToList();
```

## API Performance

### Response Caching

```csharp
// Cache dashboard data for 5 minutes
[HttpGet("dashboard")]
[ResponseCache(Duration = 300)]
public async Task<DashboardDto> GetDashboard()
{
    // ...
}
```

### Compression

```csharp
services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
});

app.UseResponseCompression();
```

### Rate Limiting

```csharp
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

## Asynchronous Processing

### Batch Operations

Process multiple items together:

```csharp
// ✅ Batch insert (faster)
var employees = Enumerable.Range(1, 1000)
    .Select(i => new Employee { ... })
    .ToList();

foreach (var batch in employees.Chunk(100))
{
    dbContext.Employees.AddRange(batch);
}
await dbContext.SaveChangesAsync();

// ❌ Individual inserts (slower)
foreach (var emp in employees)
{
    dbContext.Employees.Add(emp);
    await dbContext.SaveChangesAsync();
}
```

### Background Jobs

Use Hangfire or similar for long-running operations:

```csharp
// Queue job asynchronously
BackgroundJob.Enqueue(() => ProcessMonthlyPayroll(year, month));

// Schedule job
BackgroundJob.Schedule(() => SendNotification(userId), TimeSpan.FromHours(1));
```

## Caching Strategy

### Distributed Cache

```csharp
// Redis cache
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

// Use cache
var cachedDept = await cache.GetStringAsync($"department:{id}");
if (cachedDept == null)
{
    var dept = await dbContext.Departments.FindAsync(id);
    await cache.SetStringAsync($"department:{id}", JsonSerializer.Serialize(dept), 
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
}
```

### Cache Invalidation

```csharp
// Invalidate on update
employee.Update(...);
dbContext.Update(employee);
await dbContext.SaveChangesAsync();

// Clear cache
await cache.RemoveAsync($"employee:{employee.Id}");
```

## Memory Management

### Streaming Large Data

```csharp
// ✅ Stream results (low memory)
[HttpGet("export")]
public async IAsyncEnumerable<EmployeeExportDto> ExportEmployees()
{
    var employees = dbContext.Employees.AsAsyncEnumerable();
    await foreach (var emp in employees)
    {
        yield return MapToDto(emp);
    }
}

// ❌ Load all (high memory)
var employees = dbContext.Employees.ToList();
```

### Using Statements

```csharp
// Proper disposal
using (var connection = new NpgsqlConnection(connectionString))
{
    await connection.OpenAsync();
    // ...
} // Connection disposed automatically
```

## Monitoring Performance

### Key Metrics

- **p95 response time**: Target < 200ms
- **p99 response time**: Target < 500ms
- **Error rate**: Target < 0.1%
- **Database query time**: Target < 100ms for 95%
- **Throughput**: Target > 1000 req/sec per service

### Performance Testing

```bash
# Load testing with Apache Bench
ab -n 10000 -c 100 http://localhost:5000/health

# Load testing with k6
k6 run load-test.js
```

## Optimization Checklist

- [ ] Indexes created on filter/join columns
- [ ] N+1 queries eliminated
- [ ] Pagination implemented for large datasets
- [ ] Response caching configured
- [ ] Batch operations used where applicable
- [ ] Unused dependencies removed
- [ ] Compression enabled
- [ ] Rate limiting configured
- [ ] Monitoring alerts in place
