# Database Performance Optimization & Indexing

Comprehensive guide to query optimization, indexing strategies, and performance tuning for HR Analytics Platform.

---

## Performance Goals

| Metric | Target | Year 1 | Year 3 |
|--------|--------|--------|--------|
| **P99 Query Latency** | <100ms | <150ms | <200ms |
| **Average Query Time** | <50ms | <75ms | <100ms |
| **Database CPU** | <70% | <60% | <50% |
| **Connection Pool** | <50% used | <40% used | <30% used |
| **Slow Query Rate** | <1% | <2% | <5% |
| **Cache Hit Ratio** | >95% | >90% | >85% |

---

## Indexing Strategy

### 1. Primary Key Indexes (Automatic)

Every table has automatic primary key index:

```sql
-- Automatically created by PRIMARY KEY constraint
PRIMARY KEY (id)

-- Performance: O(log n) lookup
-- Size: Small
-- Maintenance: Automatic
```

### 2. Foreign Key Indexes (Manually Create)

```sql
-- Create indexes on foreign keys
CREATE INDEX idx_employee_company ON employees(company_id);
CREATE INDEX idx_employee_department ON employees(department_id);
CREATE INDEX idx_employee_manager ON employees(manager_id);
CREATE INDEX idx_turnover_employee ON turnover_risks(employee_id);
CREATE INDEX idx_compensation_employee ON compensation_analysis(employee_id);

-- Why: Foreign key joins use these for lookups
-- Performance: 10-100x faster than table scans
-- Size: Moderate (per table)
```

### 3. WHERE Clause Indexes

```sql
-- Frequently filtered columns
CREATE INDEX idx_employee_status ON employees(status);
CREATE INDEX idx_employee_is_active ON employees(is_active);
CREATE INDEX idx_turnover_risk_level ON turnover_risks(risk_level);
CREATE INDEX idx_compensation_risk ON compensation_analysis(equity_risk_level);
CREATE INDEX idx_engagement_department ON engagement_scores(company_id);

-- Why: WHERE e.status = 'Active' uses these
-- Performance: 100-1000x faster than full table scan
-- Size: Small-Medium
```

### 4. Date Range Indexes

```sql
-- Date columns used in WHERE/ORDER BY
CREATE INDEX idx_employee_hire_date ON employees(hire_date);
CREATE INDEX idx_turnover_prediction_date ON turnover_risks(prediction_date);
CREATE INDEX idx_engagement_survey_date ON engagement_scores(survey_date);
CREATE INDEX idx_audit_log_date ON audit_logs(created_date);

-- Why: WHERE created_date > NOW() - INTERVAL '30 days'
-- Performance: 10-100x faster
-- Size: Small-Medium
```

### 5. Composite/Multi-Column Indexes

Used for queries filtering/joining multiple columns:

```sql
-- Common query: Get active employees by department
CREATE INDEX idx_employee_dept_status 
  ON employees(company_id, department_id, is_active);

-- Query:
-- SELECT * FROM employees 
-- WHERE company_id = 1 AND department_id = 5 AND is_active = TRUE

-- Composite indexes: 100-1000x faster for multi-condition queries
-- Order matters: Put most selective column first
-- Size: Medium-Large (one index covers multiple columns)

-- Another example: Turnover risks by company and score
CREATE INDEX idx_turnover_company_score 
  ON turnover_risks(company_id, risk_score DESC);

-- Query:
-- SELECT * FROM turnover_risks 
-- WHERE company_id = 1 
-- ORDER BY risk_score DESC LIMIT 100
```

### 6. Partial Indexes (Conditional)

```sql
-- Only index active records (filter out deleted)
CREATE INDEX idx_active_employees 
  ON employees(company_id, department_id) 
  WHERE is_active = TRUE;

-- Why: Smaller index, faster (less data to search)
-- Performance: Same as regular index but smaller
-- Size: 30-50% smaller than full index

-- Another example: Only recent risks
CREATE INDEX idx_recent_risks 
  ON turnover_risks(risk_score DESC) 
  WHERE prediction_date > CURRENT_DATE - INTERVAL '90 days';
```

### 7. JSON Indexes (For Flexible Data)

```sql
-- If using JSONB columns for event details
CREATE INDEX idx_event_details_gin 
  ON employee_events USING GIN (event_details);

-- Allows efficient JSON queries:
-- WHERE event_details->>'field_name' = 'value'

-- Type: GIN (Generalized Inverted Index)
-- Performance: 10-100x faster for JSON searches
```

---

## Query Optimization Patterns

### Pattern 1: Use Column Selection (Not SELECT *)

❌ **Slow:**
```sql
SELECT * FROM employees WHERE department_id = 1;
-- Returns all 50 columns (including large text fields)
```

✅ **Fast:**
```sql
SELECT id, first_name, last_name, job_title, salary 
FROM employees 
WHERE department_id = 1;
-- Only 5 columns, 10x smaller result set
```

### Pattern 2: Use WHERE Before JOINs

❌ **Slow:**
```sql
SELECT * FROM employees e
JOIN departments d ON e.department_id = d.id
WHERE d.company_id = 1;
-- Joins first, then filters (large intermediate result)
```

✅ **Fast:**
```sql
SELECT e.* FROM employees e
JOIN departments d ON e.department_id = d.id
WHERE e.company_id = 1 AND d.company_id = 1;
-- Filters before join (smaller intermediate result)
```

### Pattern 3: Use LIMIT for Large Results

❌ **Slow:**
```sql
SELECT * FROM employee_events;
-- Returns 10M rows (huge memory, slow network)
```

✅ **Fast:**
```sql
SELECT * FROM employee_events 
ORDER BY created_date DESC 
LIMIT 1000;
-- Returns 1000 rows (fast, efficient)
```

### Pattern 4: Avoid N+1 Queries

❌ **Slow (N+1 Problem):**
```csharp
// 1 query to get employees
var employees = context.Employees.ToList(); // 1 query

// Then N queries (one per employee)
foreach (var emp in employees)
{
    var manager = context.Employees.FirstOrDefault(e => e.Id == emp.ManagerId); // N queries!
}
// Total: 1 + N queries
```

✅ **Fast (Use Include):**
```csharp
// 1 query with eager loading
var employees = context.Employees
    .Include(e => e.Manager)
    .ToList(); // Single query with JOIN
// Total: 1 query
```

### Pattern 5: Use Proper Aggregations

❌ **Slow (Load all data):**
```csharp
var count = context.Employees.ToList().Count();
```

✅ **Fast (Count in database):**
```csharp
var count = context.Employees.Count();
```

---

## Query Execution Plan Analysis

### Using EXPLAIN (PostgreSQL)

```sql
EXPLAIN ANALYZE
SELECT e.name, d.name, tr.risk_score
FROM employees e
LEFT JOIN departments d ON e.department_id = d.id
LEFT JOIN turnover_risks tr ON e.id = tr.employee_id
WHERE e.company_id = 1 AND tr.risk_score > 0.7;
```

**Output Example:**
```
Nested Loop Left Join  (cost=0.29..50000.00 rows=1000)
  ->  Seq Scan on employees e  (cost=0.00..100.00 rows=100)
        Filter: (company_id = 1)
  ->  Index Scan using idx_turnover_employee on turnover_risks tr
        Index Cond: (employee_id = e.id)
        Filter: (risk_score > 0.7)
```

**What This Means:**
- `Seq Scan`: Full table scan (can add index)
- `Index Scan`: Using index (good)
- `Nested Loop`: Type of join algorithm
- `cost`: Estimated execution cost

**Optimization Tips:**
- ❌ Seq Scan on large table = missing index
- ✅ Index Scan = good
- ⚠️ High cost = query needs optimization

---

## Caching Strategy (Redis)

### L1: Hot Data Cache (5-min TTL)

```csharp
// Employee lookup (frequently accessed)
public async Task<Employee> GetEmployeeWithCacheAsync(int id)
{
    var cacheKey = $"employee_{id}";
    
    // Check Redis cache
    var cached = await cache.GetAsync(cacheKey);
    if (cached != null)
    {
        return JsonConvert.DeserializeObject<Employee>(cached);
    }
    
    // Not in cache, query database
    var employee = await context.Employees
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Id == id);
    
    // Store in cache (5 minutes)
    if (employee != null)
    {
        await cache.SetAsync(cacheKey, 
            JsonConvert.SerializeObject(employee),
            TimeSpan.FromMinutes(5));
    }
    
    return employee;
}
```

### L2: Turnover Risk Cache (60-min TTL)

```csharp
// Risk scores (computed, not frequently changed)
public async Task<List<TurnoverRiskDto>> GetTurnoverRisksWithCacheAsync(int companyId)
{
    var cacheKey = $"turnover_risks_company_{companyId}";
    
    // Check cache
    var cached = await cache.GetAsync(cacheKey);
    if (cached != null)
    {
        return JsonConvert.DeserializeObject<List<TurnoverRiskDto>>(cached);
    }
    
    // Query database
    var risks = await connection.QueryAsync<TurnoverRiskDto>(
        @"SELECT e.Id, e.Name, tr.RiskScore FROM Employees e
          JOIN TurnoverRisks tr ON e.Id = tr.EmployeeId
          WHERE e.CompanyId = @companyId AND tr.RiskScore > 0.7",
        new { companyId }
    );
    
    var result = risks.ToList();
    
    // Cache for 60 minutes
    await cache.SetAsync(cacheKey,
        JsonConvert.SerializeObject(result),
        TimeSpan.FromHours(1));
    
    return result;
}
```

### Cache Invalidation Pattern

```csharp
// When employee updated, invalidate caches
public async Task UpdateEmployeeAsync(Employee employee)
{
    // Update database
    context.Employees.Update(employee);
    await context.SaveChangesAsync();
    
    // Invalidate related caches
    await cache.DeleteAsync($"employee_{employee.Id}");
    await cache.DeleteAsync($"turnover_risks_company_{employee.CompanyId}");
    await cache.DeleteAsync($"compensation_analysis_company_{employee.CompanyId}");
}
```

---

## Connection Pooling

### Configuration

```csharp
services.AddDbContext<HRContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions
            .CommandTimeout(60)
            .MaxPoolSize(100)  // Max connections
    )
);

services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "HR_";
});
```

### Monitoring

```
Optimal Pool Size = (Core Count * 2) + Effective Spindle Count
Example: 8 cores → (8 * 2) + 1 = 17 connections
```

---

## Batch Operations (For Bulk Inserts)

### EF Core (Slower)

```csharp
// ❌ Slow: 1 insert per query
foreach (var score in scores)
{
    context.EmployeeScores.Add(score);
}
await context.SaveChangesAsync();
// Time: 30 seconds for 10k records
```

### Dapper Bulk (Faster)

```csharp
// ✅ Fast: Single bulk insert
using (var bulkCopy = new NpgsqlBulkCopy(connection))
{
    bulkCopy.DestinationTableName = "EmployeeScores";
    await bulkCopy.WriteToServerAsync(scores);
}
// Time: 1 second for 10k records
```

---

## Query Result Caching (Materialized Views)

```sql
-- Pre-compute expensive aggregations
CREATE MATERIALIZED VIEW employee_stats AS
SELECT 
    company_id,
    COUNT(*) as total_employees,
    COUNT(CASE WHEN status = 'Active' THEN 1 END) as active_count,
    COUNT(CASE WHEN status = 'Terminated' THEN 1 END) as terminated_count,
    AVG(EXTRACT(YEAR FROM AGE(CURRENT_DATE, hire_date))) as avg_tenure_years,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY base_salary) as median_salary
FROM employees
GROUP BY company_id;

-- Refresh periodically (nightly)
REFRESH MATERIALIZED VIEW employee_stats;

-- Query is now very fast
SELECT * FROM employee_stats WHERE company_id = 1;
```

---

## Monitoring & Alerts

### Key Metrics to Monitor

```
1. Query Duration (P50, P95, P99)
   Target: P99 < 100ms
   Alert: > 500ms

2. Connection Pool Usage
   Target: < 50% of max
   Alert: > 80% of max

3. Cache Hit Ratio
   Target: > 90%
   Alert: < 70%

4. Slow Queries
   Target: < 1%
   Alert: > 5%

5. Replication Lag
   Target: < 1 second
   Alert: > 5 seconds

6. Disk I/O
   Target: < 50%
   Alert: > 80%
```

### Monitoring Query

```sql
-- Find slow queries (PostgreSQL)
SELECT 
    query,
    calls,
    mean_exec_time,
    max_exec_time,
    min_exec_time
FROM pg_stat_statements
WHERE mean_exec_time > 100  -- > 100ms
ORDER BY mean_exec_time DESC
LIMIT 20;
```

---

## Performance Tuning Checklist

### Before Optimization
- [ ] Measure current performance (baseline)
- [ ] Identify bottleneck (query, cache, network)
- [ ] Analyze EXPLAIN plan
- [ ] Check index usage

### Query Optimization
- [ ] Add missing indexes
- [ ] Use column selection (not SELECT *)
- [ ] Filter before JOIN
- [ ] Use LIMIT for pagination
- [ ] Avoid N+1 queries
- [ ] Use proper aggregations

### Caching
- [ ] Add Redis for hot data
- [ ] Implement cache invalidation
- [ ] Monitor cache hit ratio
- [ ] Set appropriate TTL

### Connection Management
- [ ] Configure pool size correctly
- [ ] Monitor pool usage
- [ ] Set appropriate timeouts

### Bulk Operations
- [ ] Use BulkCopy for 1k+ inserts
- [ ] Use Dapper for complex queries
- [ ] Batch operations where possible

### After Optimization
- [ ] Measure new performance
- [ ] Calculate improvement
- [ ] Monitor over time
- [ ] Document changes

---

## Common Performance Issues & Solutions

| Issue | Symptom | Solution |
|-------|---------|----------|
| **Missing Index** | Seq Scan in EXPLAIN | Create index on WHERE column |
| **N+1 Queries** | 1000+ queries for 100 items | Use Include() or Dapper |
| **SELECT \*** | Large result sets | Specify columns needed |
| **Low Cache Hit** | Repeated queries | Increase cache TTL or size |
| **Pool Exhaustion** | "Connection pool timeout" | Increase pool size or find leaks |
| **Large JOINs** | Slow query | Filter before JOIN |
| **Old Statistics** | Bad EXPLAIN plans | Run ANALYZE on table |

---

## Year 1-3 Performance Roadmap

### Year 1: Foundation
- ✅ Index all foreign keys
- ✅ Index all WHERE columns
- ✅ Add Redis for hot data
- ✅ Implement caching layer
- ✅ Monitor slow queries

### Year 2: Scaling
- 🔄 Add read replicas for analytics
- 🔄 Implement query result caching
- 🔄 Optimize bulk operations
- 🔄 Advanced indexing (partial, GIN)

### Year 3: Advanced
- 📋 Shard by company_id if needed
- 📋 Separate analytics to Snowflake
- 📋 Implement query federation
- 📋 Auto-scaling based on load

---

**Last Updated:** July 2026
**Status:** Active Performance Strategy
**Monitoring:** Daily (automated alerts)
