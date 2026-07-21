# Analytics Service

**Port**: 5007 | **Status**: ✅ Production Ready | **Database**: PostgreSQL + Elasticsearch

## Overview

The Analytics Service provides comprehensive HR analytics including employee metrics, department statistics, performance trends, and business intelligence dashboards.

## Key Features

- ✅ Employee metrics aggregation
- ✅ Advanced filtering and search
- ✅ Salary analytics and trends
- ✅ Turnover rate calculation
- ✅ Department statistics
- ✅ Performance distribution
- ✅ Real-time dashboard data
- ✅ Elasticsearch integration for fast queries

## API Endpoints

```
GET    /api/analytics/dashboard
GET    /api/analytics/employees/search
GET    /api/analytics/employees/by-department
GET    /api/analytics/salary/statistics
GET    /api/analytics/turnover
GET    /api/analytics/performance
GET    /api/analytics/diversity
GET    /api/analytics/export
```

## Key Metrics

### Dashboard Metrics

```csharp
{
  "totalEmployees": 150,
  "activeEmployees": 145,
  "departmentCount": 10,
  "averageSalary": 95000,
  "turnoverRate": 2.5,
  "averagePerformanceRating": 3.8,
  "genderDiversity": {
    "male": 48,
    "female": 52,
    "other": 1
  }
}
```

### Salary Analytics

```csharp
{
  "averageSalary": 95000,
  "minSalary": 35000,
  "maxSalary": 250000,
  "medianSalary": 85000,
  "salaryByDepartment": [
    { "department": "Engineering", "average": 125000 },
    { "department": "HR", "average": 65000 }
  ]
}
```

## Kafka Topics (Consumed)

| Topic | Used For |
|-------|----------|
| `employee.created` | Add to analytics |
| `employee.updated` | Update metrics |
| `employee.terminated` | Update turnover |
| `payroll.calculated` | Salary trends |
| `performance.review.approved` | Performance data |

## Integration Examples

### Get Dashboard Metrics

```bash
curl "http://localhost:5007/api/analytics/dashboard" \
  -H "Authorization: Bearer token"

# Response
{
  "totalEmployees": 150,
  "activeEmployees": 145,
  "averageSalary": 95000,
  "turnoverRate": 2.5
}
```

### Search Employees

```bash
curl "http://localhost:5007/api/analytics/employees/search?query=john&department=Engineering&status=Active" \
  -H "Authorization: Bearer token"
```

### Get Department Statistics

```bash
curl "http://localhost:5007/api/analytics/employees/by-department?departmentId=dept-guid" \
  -H "Authorization: Bearer token"
```

### Get Turnover Metrics

```bash
curl "http://localhost:5007/api/analytics/turnover?year=2026" \
  -H "Authorization: Bearer token"

# Response
{
  "terminatedCount": 3,
  "turnoverRate": 2.0,
  "avgTenure": 4.5
}
```

## Query Examples

```csharp
// Employees by department
var deptEmployees = _employees
    .Where(e => e.DepartmentId == deptId)
    .GroupBy(e => e.JobTitle)
    .Select(g => new { Title = g.Key, Count = g.Count() })
    .ToList();

// Average salary by department
var salaryByDept = _employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new {
        Department = g.Key,
        Average = g.Average(e => e.Salary)
    })
    .ToList();

// Performance distribution
var distribution = _reviews
    .Where(r => r.ApprovedDate.HasValue)
    .GroupBy(r => r.OverallRating)
    .Select(g => new { Rating = g.Key, Count = g.Count() })
    .ToList();
```

## Performance Optimization

- Elasticsearch indexes for full-text search
- Materialized views for aggregate metrics
- Redis caching for dashboard (TTL: 1 hour)
- Batch processing for large queries

## Testing

```bash
dotnet test tests/HR.Tests.Unit/Analytics/
dotnet test tests/HR.Tests.Integration/Analytics/
```

## Dependencies

- Entity Framework Core 9.0
- Elasticsearch.Net
- StackExchange.Redis (caching)
- Kafkaflow (event consumption)

## Related Services

- [Employee Service](02-EMPLOYEE-SERVICE.md)
- [Payroll Service](06-PAYROLL-SERVICE.md)
- [Performance Service](03-PERFORMANCE-SERVICE.md)
