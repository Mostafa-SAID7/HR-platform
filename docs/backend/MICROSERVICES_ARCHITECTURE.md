# Microservices Architecture
## HR Analytics Platform — Distributed System Design

Complete guide to transitioning from monolithic to microservices architecture for HR Analytics Platform.

---

## Executive Overview

### Current State (Monolithic)
```
┌─────────────────────────────────────────┐
│   Monolithic ASP.NET Core Application   │
│  ┌──────────────────────────────────┐   │
│  │  Controllers                     │   │
│  │  • Employees                     │   │
│  │  • Payroll                       │   │
│  │  • Attendance                    │   │
│  │  • Performance                   │   │
│  │  • Analytics                     │   │
│  └──────────────────────────────────┘   │
│  ┌──────────────────────────────────┐   │
│  │  Single PostgreSQL Database      │   │
│  │  • All tables in one DB          │   │
│  │  • Shared schema                 │   │
│  │  • ACID transactions             │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘

Issues at Scale:
❌ Cannot scale individual features
❌ Database bottleneck
❌ Difficult to deploy changes
❌ Tight coupling
❌ One failure cascades
```

### Target State (Microservices)
```
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│  Employee        │  │  Payroll         │  │  Attendance      │
│  Service         │  │  Service         │  │  Service         │
│  ┌────────────┐  │  │  ┌────────────┐  │  │  ┌────────────┐  │
│  │ Port 5001  │  │  │  │ Port 5002  │  │  │  │ Port 5003  │  │
│  │ gRPC       │  │  │  │ gRPC       │  │  │  │ gRPC       │  │
│  └────────────┘  │  │  └────────────┘  │  │  └────────────┘  │
└──────────────────┘  └──────────────────┘  └──────────────────┘
       │                      │                      │
       ▼                      ▼                      ▼
   ┌────────┐            ┌────────┐            ┌────────┐
   │ DB 1   │            │ DB 2   │            │ DB 3   │
   │ (Emp)  │            │(Pay)   │            │(Att)   │
   └────────┘            └────────┘            └────────┘

  ┌─────────────────────────────────────────┐
  │  API Gateway                            │
  │  • Request routing                      │
  │  • Rate limiting                        │
  │  • Authentication                       │
  │  • Load balancing                       │
  └─────────────────────────────────────────┘

  ┌─────────────────────────────────────────┐
  │  Event Bus (RabbitMQ / Kafka)           │
  │  • Async communication                  │
  │  • Service decoupling                   │
  │  • Event streaming                      │
  └─────────────────────────────────────────┘

Benefits:
✅ Independent scaling
✅ Technology flexibility
✅ Independent deployment
✅ Loose coupling
✅ Fault isolation
✅ Better suited for analytics
```

---

## Service Decomposition

### Core Services (Domain-Driven)

```
┌─────────────────────────────────────────────────────────────┐
│  API Gateway (Port 80/443)                                  │
│  └─ Route /employees → Employee Service                     │
│  └─ Route /payroll → Payroll Service                        │
│  └─ Route /attendance → Attendance Service                  │
│  └─ Route /performance → Performance Service                │
│  └─ Route /analytics → Analytics Service                    │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────┐  ┌──────────────────────┐
│  Employee Service    │  │  Payroll Service     │
│  (Port 5001)         │  │  (Port 5002)         │
│                      │  │                      │
│ Responsibilities:    │  │ Responsibilities:    │
│ • Employee data      │  │ • Salary calculation │
│ • Departments        │  │ • Payments           │
│ • Roles              │  │ • Tax processing     │
│ • Employee status    │  │ • Deductions         │
│                      │  │ • Payslips           │
│ Technology:          │  │                      │
│ • ASP.NET Core 9     │  │ Technology:          │
│ • PostgreSQL         │  │ • ASP.NET Core 9     │
│ • EF Core            │  │ • PostgreSQL         │
│ • gRPC               │  │ • Dapper (for speed) │
│                      │  │ • gRPC               │
└──────────────────────┘  └──────────────────────┘

┌──────────────────────┐  ┌──────────────────────┐
│ Attendance Service   │  │ Performance Service  │
│ (Port 5003)          │  │ (Port 5004)          │
│                      │  │                      │
│ Responsibilities:    │  │ Responsibilities:    │
│ • Check-in/out       │  │ • Performance ratings│
│ • Leave tracking     │  │ • Goals tracking     │
│ • Shift management   │  │ • Feedback          │
│ • Reports            │  │ • Development plans  │
│                      │  │ • Reviews            │
│ Technology:          │  │                      │
│ • ASP.NET Core 9     │  │ Technology:          │
│ • PostgreSQL         │  │ • ASP.NET Core 9     │
│ • EF Core            │  │ • PostgreSQL         │
│ • gRPC               │  │ • EF Core            │
│                      │  │ • gRPC               │
└──────────────────────┘  └──────────────────────┘

┌──────────────────────┐  ┌──────────────────────┐
│ Analytics Service    │  │ Notification Service │
│ (Port 5005)          │  │ (Port 5006)          │
│                      │  │                      │
│ Responsibilities:    │  │ Responsibilities:    │
│ • Dashboards         │  │ • Email notifications│
│ • Reports            │  │ • SMS alerts         │
│ • Trend analysis     │  │ • In-app messages    │
│ • Predictive models  │  │ • Webhook delivery   │
│ • Data warehousing   │  │ • Notification queue │
│                      │  │                      │
│ Technology:          │  │ Technology:          │
│ • ASP.NET Core 9     │  │ • ASP.NET Core 9     │
│ • Snowflake (DW)     │  │ • PostgreSQL         │
│ • Python (ML models) │  │ • Message queue      │
│ • gRPC               │  │ • SendGrid/Twilio    │
└──────────────────────┘  └──────────────────────┘

┌──────────────────────┐  ┌──────────────────────┐
│ Auth Service         │  │ Audit Service        │
│ (Port 5007)          │  │ (Port 5008)          │
│                      │  │                      │
│ Responsibilities:    │  │ Responsibilities:    │
│ • User authentication│  │ • Audit logging      │
│ • JWT token issue    │  │ • Change tracking    │
│ • OAuth2/OIDC        │  │ • Compliance reports │
│ • SSO integration    │  │ • Data access logs   │
│ • Access control     │  │ • Security events    │
│                      │  │                      │
│ Technology:          │  │ Technology:          │
│ • ASP.NET Core 9     │  │ • ASP.NET Core 9     │
│ • IdentityServer     │  │ • PostgreSQL         │
│ • PostgreSQL         │  │ • Elasticsearch      │
│ • Redis (sessions)   │  │ • gRPC               │
└──────────────────────┘  └──────────────────────┘
```

### Service Responsibilities Matrix

| Service | Owns | Communicates | Data Store |
|---------|------|--------------|-----------|
| Employee | Employee master data, departments, roles | Payroll, Performance, Attendance, Analytics | PostgreSQL (emp_db) |
| Payroll | Salary, taxes, deductions, payslips | Employee, Notification, Analytics | PostgreSQL (pay_db) |
| Attendance | Clock in/out, leave, shifts | Employee, Performance, Analytics | PostgreSQL (att_db) |
| Performance | Ratings, goals, reviews, feedback | Employee, Analytics, Notification | PostgreSQL (perf_db) |
| Analytics | Dashboards, reports, predictions | All services (read-only) | Snowflake (dw_db) |
| Notification | Email, SMS, push notifications | Event Bus (all services) | PostgreSQL (notif_db) |
| Auth | Authentication, authorization, tokens | All services | PostgreSQL (auth_db) + Redis |
| Audit | Event logging, compliance, security | Event Bus (all services) | PostgreSQL (audit_db) + Elasticsearch |

---

## Communication Patterns

### Synchronous: gRPC (Service-to-Service)

```
Scenario: Payroll Service needs Employee data
┌─────────────────────┐
│ Payroll Service     │
│                     │
│ ProcessPayroll()    │
│   │                 │
│   ├─ Get Employee   │
│   │  salary info    │
│   │  (gRPC call)    │
│   ▼                 │
│   ┌───────────────┐ │
│   │ gRPC Client   │ │
│   └───────────────┘ │
└─────────────────────┘
         │
         │ gRPC Binary
         │ (Fast, typed)
         ▼
┌─────────────────────┐
│ Employee Service    │
│                     │
│ GetEmployeeProto()  │
│  (gRPC Service)     │
│                     │
└─────────────────────┘

Benefits:
✅ Fast (binary protocol)
✅ Strongly typed
✅ Type-safe contracts
✅ Low latency

When to use:
• Real-time operations
• Within transaction
• Request-response required
```

**gRPC Service Definition:**

```protobuf
// protos/employee.proto
syntax = "proto3";

package hrplatform.employee;

service EmployeeService {
  rpc GetEmployee (GetEmployeeRequest) returns (EmployeeResponse);
  rpc GetEmployees (GetEmployeesRequest) returns (EmployeeListResponse);
  rpc UpdateEmployeeSalary (UpdateSalaryRequest) returns (UpdateSalaryResponse);
}

message GetEmployeeRequest {
  string employee_id = 1;
}

message EmployeeResponse {
  string id = 1;
  string full_name = 2;
  string email = 3;
  string department = 4;
  decimal base_salary = 5;
  string status = 6;
  int32 company_id = 7;
}

message GetEmployeesRequest {
  int32 company_id = 1;
  string department = 2;
  int32 page = 3;
  int32 page_size = 4;
}

message EmployeeListResponse {
  repeated EmployeeResponse employees = 1;
  int32 total_count = 2;
}

message UpdateSalaryRequest {
  string employee_id = 1;
  decimal new_salary = 2;
  string effective_date = 3;
}

message UpdateSalaryResponse {
  bool success = 1;
  string message = 2;
}
```

**C# Implementation:**

```csharp
// Services/EmployeeGrpcClient.cs
public class EmployeeGrpcClient
{
    private readonly EmployeeService.EmployeeServiceClient _client;
    private readonly ILogger<EmployeeGrpcClient> _logger;
    
    public EmployeeGrpcClient(
        EmployeeService.EmployeeServiceClient client,
        ILogger<EmployeeGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    public async Task<EmployeeResponse> GetEmployeeAsync(string employeeId)
    {
        try
        {
            var request = new GetEmployeeRequest { EmployeeId = employeeId };
            
            var response = await _client.GetEmployeeAsync(
                request,
                deadline: DateTime.UtcNow.AddSeconds(5)
            );
            
            _logger.LogInformation($"Retrieved employee {employeeId} via gRPC");
            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError($"gRPC error: {ex.Status.Detail}");
            throw;
        }
    }
}

// DI Registration
services.AddGrpcClient<EmployeeService.EmployeeServiceClient>(options =>
{
    options.Address = new Uri("https://employee-service:5001");
});

services.AddScoped<EmployeeGrpcClient>();
```

### Asynchronous: Event Bus (Event-Driven)

```
Scenario: Employee salary changed
┌─────────────────────┐
│ Employee Service    │
│                     │
│ UpdateEmployee()    │
│   │                 │
│   ├─ Change salary  │
│   │                 │
│   ├─ Publish event  │
│   │  "EmployeeSalary│
│   │   ChangedEvent" │
│   ▼                 │
┌─────────────────────┐
│ Event Bus           │
│ (RabbitMQ/Kafka)    │
│                     │
│ ✓ Salary changed    │
│ ✓ Employee created  │
│ ✓ Status changed    │
└─────────────────────┘
    │ (async)
    ├──► Payroll Service
    │    (update calculations)
    │
    ├──► Analytics Service
    │    (update dashboards)
    │
    ├──► Notification Service
    │    (send alerts)
    │
    └──► Audit Service
         (log event)

Benefits:
✅ Loose coupling
✅ Asynchronous
✅ Scalable
✅ Resilient (queue)

When to use:
• Non-critical operations
• Fan-out notifications
• Cross-service events
• Batch processing
```

**Event Publishing:**

```csharp
// Domain Events
public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public int CompanyId { get; init; }
}

public class EmployeeSalaryChangedEvent : DomainEvent
{
    public string EmployeeId { get; init; }
    public decimal OldSalary { get; init; }
    public decimal NewSalary { get; init; }
    public string EffectiveDate { get; init; }
    public string ChangedBy { get; init; }
}

public class EmployeeCreatedEvent : DomainEvent
{
    public string EmployeeId { get; init; }
    public string FullName { get; init; }
    public string Email { get; init; }
    public string Department { get; init; }
    public DateTime HireDate { get; init; }
}

// Publishing
public class EmployeeService
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task UpdateSalaryAsync(string employeeId, decimal newSalary)
    {
        var employee = await _repository.GetAsync(employeeId);
        var oldSalary = employee.Salary;
        
        // Update employee
        employee.Salary = newSalary;
        await _repository.UpdateAsync(employee);
        
        // Publish event to event bus (RabbitMQ/Kafka)
        var @event = new EmployeeSalaryChangedEvent
        {
            EmployeeId = employeeId,
            OldSalary = oldSalary,
            NewSalary = newSalary,
            EffectiveDate = DateTime.Today.ToString("yyyy-MM-dd"),
            ChangedBy = _currentUser.Id,
            CompanyId = _currentUser.CompanyId
        };
        
        await _publishEndpoint.Publish(@event);
        
        _logger.LogInformation(
            $"Employee {employeeId} salary updated from {oldSalary} to {newSalary}. Event published."
        );
    }
}
```

**Event Subscribers:**

```csharp
// Payroll Service - subscribes to salary change events
public class EmployeeSalaryChangedConsumer : IConsumer<EmployeeSalaryChangedEvent>
{
    private readonly IPayrollCalculationService _payrollService;
    private readonly ILogger<EmployeeSalaryChangedConsumer> _logger;
    
    public async Task Consume(ConsumeContext<EmployeeSalaryChangedEvent> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation(
            $"Processing salary change for employee {@event.EmployeeId}: " +
            $"{@event.OldSalary} → {@event.NewSalary}"
        );
        
        try
        {
            // Recalculate payroll
            await _payrollService.RecalculatePayrollAsync(
                @event.EmployeeId,
                @event.EffectiveDate
            );
            
            // Acknowledge processing
            _logger.LogInformation(
                $"Payroll recalculated for employee {@event.EmployeeId}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing salary change event");
            throw;  // Retry by message broker
        }
    }
}

// Analytics Service - subscribes for reporting
public class AnalyticsEventConsumer : 
    IConsumer<EmployeeSalaryChangedEvent>,
    IConsumer<EmployeeCreatedEvent>
{
    private readonly IAnalyticsRepository _analyticsRepo;
    
    public async Task Consume(ConsumeContext<EmployeeSalaryChangedEvent> context)
    {
        // Update analytics cache/warehouse
        await _analyticsRepo.RecordSalaryChangeAsync(
            context.Message.EmployeeId,
            context.Message.NewSalary,
            context.Message.EffectiveDate
        );
    }
    
    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        // Update workforce metrics
        await _analyticsRepo.RecordNewHireAsync(
            context.Message.EmployeeId,
            context.Message.Department,
            context.Message.HireDate
        );
    }
}

// DI Registration (MassTransit for RabbitMQ/Kafka)
services.AddMassTransit(x =>
{
    x.AddConsumer<EmployeeSalaryChangedConsumer>();
    x.AddConsumer<AnalyticsEventConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint("payroll-salary-queue", e =>
        {
            e.ConfigureConsumer<EmployeeSalaryChangedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("analytics-events-queue", e =>
        {
            e.ConfigureConsumer<AnalyticsEventConsumer>(context);
        });
    });
});
```

---

## Service Discovery

### Consul-Based Discovery

```
Scenario: Payroll Service finds Employee Service address
┌────────────────────────┐
│ Payroll Service        │
│                        │
│ Query: Where is        │
│ Employee Service?      │
│        │               │
│        ▼               │
│ ┌──────────────────┐   │
│ │ Service Registry │   │
│ │ (Consul)         │   │
│ └──────────────────┘   │
└────────────────────────┘
        │
        │ Query
        ▼
┌────────────────────────┐
│ Consul Cluster         │
│                        │
│ Service Registry:      │
│ employee-service:      │
│  - 10.0.1.10:5001      │
│  - 10.0.1.11:5001      │
│  - 10.0.1.12:5001      │
│                        │
│ payroll-service:       │
│  - 10.0.1.20:5002      │
│  - 10.0.1.21:5002      │
│                        │
│ health-checks:         │
│  ✓ Active              │
│  ✓ Active              │
│  ✗ Inactive (removed)  │
└────────────────────────┘
        │
        │ Response: 10.0.1.10:5001
        ▼
┌────────────────────────┐
│ Payroll Service        │
│                        │
│ Connect to Employee    │
│ Service at             │
│ 10.0.1.10:5001 ✓       │
└────────────────────────┘
```

**Implementation:**

```csharp
// Program.cs
services.AddConsul(options =>
{
    options.ConsulBaseUri = new Uri("http://consul:8500");
});

// Service Registration
services.AddGrpcClient<EmployeeService.EmployeeServiceClient>()
    .ConfigureChannel(options =>
    {
        options.HttpHandler = new HttpClientHandler();
    });

services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    
    // Consul-based service discovery
    http.AddServiceDiscovery()
        .AddConsulServiceEndpointProvider();
});

// Automatic heartbeat & deregistration
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await app.Services.GetRequiredService<IServiceRegistry>()
        .RegisterAsync(new ServiceRegistration
        {
            Id = Environment.MachineName,
            Name = "payroll-service",
            Address = "http://payroll-service",
            Port = 5002,
            Scheme = "http",
            Tags = new[] { "payroll", "grpc" },
            HealthCheck = new HealthCheckDefinition
            {
                HttpUri = "http://payroll-service:5002/health",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5)
            }
        });
});
```

---

## Migration Path: Monolith → Microservices

### Phase 1: Strangler Pattern (Months 1-3)
```
Old Monolith         New Microservices
┌──────────────────┐ ┌──────────────────┐
│ Monolithic App   │ │ Employee Service │
│ • Employees      │ │ (extracted)      │
│ • Payroll        │ │                  │
│ • Attendance     │◄──────────────────┤
│ • Performance    │ └──────────────────┘
│ • Analytics      │
└──────────────────┘

API Gateway routes:
- /employees → Employee Service (NEW)
- /payroll → Monolith (OLD)
- /attendance → Monolith (OLD)
```

### Phase 2: Incremental Extraction (Months 3-9)
```
Monolith            Microservices
┌──────────────┐   ┌──────────────────┐
│ Monolithic   │   │ Employee Service │
│ • Payroll    │   │ Payroll Service  │
│ • Attendance │   │ Attendance Svc   │
│ • Performance│   │ Performance Svc   │
│ • Analytics  │   │                  │
└──────────────┘   └──────────────────┘
```

### Phase 3: Fully Microservices (Month 9+)
```
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Employee Svc │  │ Payroll Svc  │  │Attendance Svc│
└──────────────┘  └──────────────┘  └──────────────┘
       │                  │                  │
       └──────────────────┼──────────────────┘
                          │
                    Event Bus
                  (RabbitMQ/Kafka)
                          │
       ┌──────────────────┼──────────────────┐
       │                  │                  │
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│Performance   │  │ Analytics    │  │ Notification │
│ Service      │  │ Service      │  │ Service      │
└──────────────┘  └──────────────┘  └──────────────┘
```

---

## Benefits & Tradeoffs

### Benefits ✅

| Benefit | Description |
|---------|-------------|
| **Independent Scaling** | Scale only high-traffic services (e.g., Analytics) |
| **Technology Flexibility** | Use Python for ML in Analytics, C# for payroll logic |
| **Faster Deployment** | Deploy Employee Service without affecting Payroll |
| **Fault Isolation** | Payroll failure doesn't crash Attendance |
| **Better for Analytics** | Dedicated Analytics service with Snowflake, not monolith |
| **Team Autonomy** | Each team owns a service end-to-end |
| **Multi-tenancy** | Easier to implement per-company isolation |

### Tradeoffs ⚠️

| Tradeoff | Mitigation |
|----------|-----------|
| **Complexity** | API Gateway, Event Bus, Service Discovery |
| **Network Latency** | gRPC (binary), local caching, optimization |
| **Distributed Debugging** | Distributed tracing (Jaeger, DataDog) |
| **Data Consistency** | Event sourcing, saga patterns, eventual consistency |
| **Operations Overhead** | Kubernetes, automation, monitoring |
| **Cost** | Infrastructure for multiple services (use containers) |

---

## Next Steps

1. **Detailed Design** → Service Mesh, API Gateway, Event Bus (see separate docs)
2. **Proof of Concept** → Extract Employee Service (smallest, self-contained)
3. **Infrastructure** → Kubernetes cluster, service discovery, event bus
4. **Team Preparation** → DevOps practices, observability, on-call rotations
5. **Gradual Migration** → Strangler pattern over 9-12 months

---

**Last Updated:** July 2026
**Status:** Architecture Design Complete
**Next:** Service Mesh & Communication Details
