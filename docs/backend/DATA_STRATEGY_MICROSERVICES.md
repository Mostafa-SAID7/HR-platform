# Data Strategy for Microservices

Comprehensive guide to database per service, data consistency, distributed queries, and data synchronization.

---

## Database Per Service Pattern

### Overview

```
Monolithic (Tight Coupling)
┌──────────────────────────────┐
│ Single PostgreSQL Database   │
│ ┌────────────────────────────┤
│ │ employees                  │
│ │ departments                │
│ │ payroll                    │
│ │ attendance                 │
│ │ performance                │
│ │ compensation               │
│ └────────────────────────────┤
│                              │
│ Problems:                    │
│ ❌ Tight coupling            │
│ ❌ Hard to scale             │
│ ❌ Schema changes affect all │
│ ❌ Data access everywhere    │
└──────────────────────────────┘

Microservices (Loose Coupling)
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│  Employee    │ │   Payroll    │ │ Attendance   │
│  Service     │ │   Service    │ │ Service      │
│  ┌────────┐  │ │  ┌────────┐  │ │  ┌────────┐  │
│  │ emp_db │  │ │  │ pay_db │  │ │  │ att_db │  │
│  │        │  │ │  │        │  │ │  │        │  │
│  │ Tables:│  │ │  │ Tables:│  │ │  │ Tables:│  │
│  │ • emp  │  │ │  │ • payroll
│  │        │  │ │  │ • payment
│  └────────┘  │ │  └────────┘  │ │  └────────┘  │
└──────────────┘ └──────────────┘ └──────────────┘

Benefits:
✅ Independent scaling
✅ Technology choice
✅ Schema autonomy
✅ Data isolation
✅ Clear boundaries
```

### Service → Database Mapping

| Service | Database | Tables | Replication |
|---------|----------|--------|-------------|
| **Employee** | PostgreSQL (emp_db) | employees, departments, roles, skills | Primary in us-east-1, read replica us-west-2 |
| **Payroll** | PostgreSQL (pay_db) | employees_payroll, salary_structures, taxes, payments, payslips | Primary in us-east-1 |
| **Attendance** | PostgreSQL (att_db) | checkins, leave_requests, shifts, attendance_summary | Primary in us-east-1, hot standby |
| **Performance** | PostgreSQL (perf_db) | ratings, goals, reviews, feedback | Primary in us-east-1 |
| **Analytics** | Snowflake (dw_db) | denormalized warehouse schema, aggregated tables | Replica from all services |
| **Notification** | PostgreSQL (notif_db) | messages, templates, schedules, delivery_log | Primary in us-east-1 |
| **Auth** | PostgreSQL (auth_db) | users, roles, permissions, sessions | Primary in us-east-1, read replicas |
| **Audit** | PostgreSQL (audit_db) + Elasticsearch | audit_logs, events, changes | Long-term retention, indexed for search |

---

## Data Consistency Models

### Eventual Consistency with Event Bus

```
Timeline:
T0: Employee salary changed in Employee Service
T0: Event published to RabbitMQ
T1: Payroll Service receives event, recalculates
T2: Analytics Service receives event, updates warehouse
T3: Audit Service receives event, logs change

At T0+5ms: Eventual consistency - all services will eventually be consistent
```

```csharp
// Employee Service - immediate update
public async Task UpdateEmployeeSalaryAsync(string employeeId, decimal newSalary)
{
    var employee = await _repository.GetByIdAsync(employeeId);
    employee.BaseSalary = newSalary;
    await _repository.SaveAsync(employee);  // ACID within this service
    
    // Publish event for other services
    await _eventBus.PublishAsync(new EmployeeSalaryChangedEvent
    {
        EmployeeId = employeeId,
        OldSalary = employee.OldBaseSalary,
        NewSalary = newSalary,
        EffectiveDate = DateTime.Today
    });
    
    return Ok();  // Return immediately, don't wait for consistency
}

// Payroll Service - receives event asynchronously
public async Task Consume(ConsumeContext<EmployeeSalaryChangedEvent> context)
{
    var @event = context.Message;
    
    // After some delay (eventually consistent)
    // Update payroll calculations
    await _payrollService.RecalculateAsync(@event.EmployeeId, @event.NewSalary);
}

// Query from different services
public async Task<EmployeeAnalyticsDto> GetAnalyticsAsync(string employeeId)
{
    // These might temporarily show different values
    
    // From Employee Service (strong consistency)
    var employee = await _employeeClient.GetEmployeeAsync(employeeId);
    
    // From Analytics Service (eventual consistency, may be stale)
    var analytics = await _analyticsClient.GetAnalyticsAsync(employeeId);
    
    return new EmployeeAnalyticsDto
    {
        CurrentSalary = employee.BaseSalary,  // Latest
        AnalyticsSalary = analytics.AverageSalary  // May be from yesterday
    };
}
```

### Strong Consistency for Critical Operations

```csharp
// Payroll Service - Generate payslip
// MUST use current employee data, cannot use stale data

public async Task<PayslipDto> GeneratePayslipAsync(string employeeId)
{
    // Call Employee Service via gRPC (synchronous, strongly consistent)
    var employee = await _employeeGrpcClient.GetEmployeeAsync(employeeId);
    
    // Get current tax rates (from own database)
    var taxRates = await _taxRepository.GetCurrentRatesAsync(employee.Country);
    
    // Calculate payslip with current data
    var payslip = new Payslip
    {
        EmployeeId = employeeId,
        BaseSalary = employee.BaseSalary,  // Current
        GrossSalary = CalculateGross(employee),
        Deductions = CalculateDeductions(employee, taxRates),
        NetSalary = CalculateNet(/* ... */),
        PayPeriodStart = startDate,
        PayPeriodEnd = endDate
    };
    
    return payslip;
}
```

---

## Distributed Queries (across services)

### Query 1: Get Employee with Recent Performance Reviews

```csharp
// Endpoint: GET /api/v1/employees/123/profile
// Aggregates data from Employee Service + Performance Service

[HttpGet("employees/{id}/profile")]
public async Task<EmployeeProfileDto> GetEmployeeProfileAsync(string id)
{
    var getEmployeeTask = _employeeGrpcClient.GetEmployeeAsync(id);
    var getPerformanceTask = _performanceGrpcClient.GetRecentReviewsAsync(id);
    var getPayrollTask = _payrollGrpcClient.GetCompensationAsync(id);
    
    await Task.WhenAll(getEmployeeTask, getPerformanceTask, getPayrollTask);
    
    var employee = getEmployeeTask.Result;
    var performance = getPerformanceTask.Result;
    var payroll = getPayrollTask.Result;
    
    return new EmployeeProfileDto
    {
        Id = employee.Id,
        Name = employee.FullName,
        Email = employee.Email,
        Department = employee.Department,
        JobTitle = employee.JobTitle,
        HireDate = employee.HireDate,
        BaseSalary = payroll.BaseSalary,
        PerformanceRating = performance.CurrentRating,
        RecentReviews = performance.Reviews.Take(3)
    };
}
```

### Query 2: Department Summary

```csharp
// Endpoint: GET /api/v1/departments/eng/summary
// Aggregates from Employee + Payroll + Performance + Analytics

[HttpGet("departments/{id}/summary")]
public async Task<DepartmentSummaryDto> GetDepartmentSummaryAsync(string id)
{
    // Query 1: Get all employees in department
    var employees = await _employeeGrpcClient.GetEmployeesByDepartmentAsync(id);
    
    // Query 2: Get department payroll metrics
    var payrollMetrics = await _payrollGrpcClient
        .GetDepartmentMetricsAsync(id);
    
    // Query 3: Get performance metrics from Analytics
    var performanceMetrics = await _analyticsGrpcClient
        .GetDepartmentPerformanceAsync(id);
    
    return new DepartmentSummaryDto
    {
        DepartmentId = id,
        EmployeeCount = employees.Count,
        AverageSalary = payrollMetrics.AverageSalary,
        PayrollCost = payrollMetrics.TotalPayroll,
        AveragePerformanceRating = performanceMetrics.AverageRating,
        HighPerformersCount = performanceMetrics.HighPerformersCount,
        LowPerformersCount = performanceMetrics.LowPerformersCount,
        TurnoverRisk = performanceMetrics.AtRiskCount
    };
}
```

---

## Data Synchronization Patterns

### Pattern 1: Change Data Capture (CDC)

```
Database Replication:
Employee Service PostgreSQL → Analytics (Snowflake)
via Kafka + Debezium
```

```yaml
# docker-compose.yml - Debezium CDC setup
version: '3'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: hr_employee
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    volumes:
      - ./wal-config.sql:/docker-entrypoint-initdb.d/init.sql

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    depends_on:
      - zookeeper
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1

  debezium:
    image: debezium/connect:2.0
    environment:
      GROUP_ID: 1
      CONFIG_STORAGE_TOPIC: debezium_config
      OFFSET_STORAGE_TOPIC: debezium_offset
      STATUS_STORAGE_TOPIC: debezium_status
    depends_on:
      - kafka
      - postgres
```

```bash
# Configure Debezium connector
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d '{
    "name": "postgres-cdc",
    "config": {
      "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
      "database.hostname": "postgres",
      "database.port": 5432,
      "database.user": "postgres",
      "database.password": "password",
      "database.dbname": "hr_employee",
      "topic.prefix": "employee-service",
      "table.include.list": "public.employees,public.departments",
      "publication.name": "dbz_publication",
      "slot.name": "dbz_slot"
    }
  }'

# Kafka topics created:
# - employee-service.public.employees
# - employee-service.public.departments
```

```csharp
// Analytics Service - Consume CDC events
public class EmployeeTableChangeConsumer : IConsumer<EmployeeChanged>
{
    private readonly ISnowflakeWarehouseService _warehouse;
    
    public async Task Consume(ConsumeContext<EmployeeChanged> context)
    {
        var change = context.Message;
        
        // Sync to Snowflake
        await _warehouse.UpsertEmployeeAsync(
            employeeId: change.EmployeeId,
            firstName: change.FirstName,
            lastName: change.LastName,
            salary: change.BaseSalary,
            department: change.DepartmentId,
            changedAt: change.Timestamp
        );
    }
}
```

### Pattern 2: Event Sourcing (Audit Trail)

```csharp
// Store every change as an event in append-only log
public class EmployeeAuditLog
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EmployeeId { get; set; }
    public string EventType { get; set; }  // Created, Updated, Deleted
    public string FieldChanged { get; set; }  // "BaseSalary", "Department"
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public DateTime OccurredAt { get; set; }
    public string ChangedBy { get; set; }
    public string Reason { get; set; }
}

// SQL: Insert-only (append-only log)
INSERT INTO employee_audit_log (employee_id, event_type, field_changed, old_value, new_value, occurred_at, changed_by)
VALUES ('EMP123', 'Updated', 'BaseSalary', '80000', '85000', NOW(), 'USER456');

// Reconstruct state from events
public async Task<Employee> GetEmployeeAtTimeAsync(string employeeId, DateTime atTime)
{
    var employee = new Employee { Id = employeeId };
    
    var events = await _auditLog.GetEventsAsync(employeeId, until: atTime);
    
    foreach (var @event in events.OrderBy(e => e.OccurredAt))
    {
        switch (@event.EventType)
        {
            case "Created":
                // Parse initial values
                break;
            case "Updated":
                // Apply change
                var property = typeof(Employee).GetProperty(@event.FieldChanged);
                property.SetValue(employee, Convert.ChangeType(@event.NewValue, property.PropertyType));
                break;
        }
    }
    
    return employee;
}
```

---

## Cross-Service Transactions (Saga Pattern)

### Salary Change Transaction (across 3 services)

```
Employee Service → Payroll Service → Audit Service

Normal flow:
1. Employee Service updates salary ✓
2. Payroll Service recalculates ✓
3. Audit Service logs change ✓

Failure scenario:
1. Employee Service updates salary ✓
2. Payroll Service fails ✗
   → Rollback Employee Service
   → Audit Service logs rollback
```

```csharp
// Saga: Update Employee Salary
public class EmployeeSalaryUpdateSaga : StateMachine<EmployeeSalaryUpdateState>
{
    public State Initial { get; private set; }
    public State SalaryUpdated { get; private set; }
    public State PayrollRecalculated { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }
    
    public Event<UpdateSalaryCommand> UpdateSalaryCommand { get; private set; }
    public Event<PayrollRecalculatedEvent> PayrollRecalculatedEvent { get; private set; }
    
    public EmployeeSalaryUpdateSaga()
    {
        InstanceState(x => x.CurrentState);
        
        // Step 1: Update salary in Employee Service
        Initially(
            When(UpdateSalaryCommand)
                .Then(context =>
                {
                    context.Instance.EmployeeId = context.Data.EmployeeId;
                    context.Instance.NewSalary = context.Data.NewSalary;
                })
                .SendAsync(context => context.Init<UpdateEmployeeSalaryCommand>(
                    new UpdateEmployeeSalaryCommand
                    {
                        EmployeeId = context.Data.EmployeeId,
                        NewSalary = context.Data.NewSalary,
                        CorrelationId = context.CorrelationId
                    }
                ))
                .TransitionTo(SalaryUpdated)
        );
        
        // Step 2: Recalculate payroll
        During(SalaryUpdated,
            When(PayrollRecalculatedEvent)
                .PublishAsync(context => context.Init<AuditEmployeeSalaryChangeCommand>(
                    new AuditEmployeeSalaryChangeCommand
                    {
                        EmployeeId = context.Instance.EmployeeId,
                        OldSalary = context.Data.OldSalary,
                        NewSalary = context.Data.NewSalary
                    }
                ))
                .TransitionTo(Completed)
        );
        
        // Handle failures
        DuringAny(
            When(UpdateSalaryCommand.IsNotHandled())
                .Then(context =>
                {
                    // Compensating transaction: rollback
                    context.PublishAsync<RollbackSalaryUpdateCommand>();
                })
                .TransitionTo(Failed)
        );
    }
}
```

---

## Data Consistency Checklist

- [ ] Each microservice owns its data
- [ ] No direct database access between services (use APIs)
- [ ] Outbox pattern for guaranteed event delivery
- [ ] CDC configured for analytical data
- [ ] Saga pattern for distributed transactions
- [ ] Audit trail maintained
- [ ] Cache invalidation on data changes
- [ ] Dead letter queues for failed events
- [ ] Monitoring for data consistency issues
- [ ] Regular reconciliation jobs

---

**Last Updated:** July 2026
**Status:** Data Strategy Complete
