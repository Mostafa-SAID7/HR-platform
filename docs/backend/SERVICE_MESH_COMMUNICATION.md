# Service Mesh & Inter-Service Communication

Comprehensive guide to service-to-service communication patterns, resilience, and service mesh implementation.

---

## Communication Stack

### Layer Overview

```
┌─────────────────────────────────────────────────┐
│ Application Layer (Business Logic)              │
├─────────────────────────────────────────────────┤
│ Service Communication Layer                     │
│ ┌─────────────────────────────────────────────┐ │
│ │ gRPC (Sync)  │  Event Bus (Async)           │ │
│ │ • Request/   │  • RabbitMQ/Kafka            │ │
│ │   Response   │  • Eventually consistent     │ │
│ │ • Type-safe  │  • Decoupled                 │ │
│ └─────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────┤
│ Service Mesh (Istio/Linkerd)                    │
│ ┌─────────────────────────────────────────────┐ │
│ │ • Load balancing                            │ │
│ │ • Circuit breaking                          │ │
│ │ • Retries & timeouts                        │ │
│ │ • Distributed tracing                       │ │
│ │ • Mutual TLS                                │ │
│ │ • Rate limiting                             │ │
│ └─────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────┤
│ Container Network (Docker/Kubernetes)           │
├─────────────────────────────────────────────────┤
│ Service Discovery (Consul/Kubernetes DNS)       │
└─────────────────────────────────────────────────┘
```

---

## gRPC: Synchronous Communication

### When to Use gRPC

| Scenario | gRPC | HTTP REST |
|----------|------|----------|
| **Speed** | Binary (fast) | Text (slower) |
| **Real-time** | Excellent | Good |
| **Strongly typed** | Yes (Proto) | No |
| **Streaming** | Yes (bi-directional) | No |
| **Payload** | Small binary | Larger JSON |
| **Latency** | <50ms | 50-200ms |

### gRPC Service Definition

```protobuf
// protos/services/employee.proto
syntax = "proto3";

package hrplatform.services.employee;

// Employee Service - manages employee master data
service EmployeeService {
  // Get single employee
  rpc GetEmployee (GetEmployeeRequest) returns (EmployeeResponse);
  
  // Get employees by criteria
  rpc GetEmployees (GetEmployeesRequest) returns (EmployeeListResponse);
  
  // Server streaming - get large datasets
  rpc StreamEmployees (StreamEmployeesRequest) returns (stream EmployeeResponse);
  
  // Create employee
  rpc CreateEmployee (CreateEmployeeRequest) returns (EmployeeResponse);
  
  // Update employee
  rpc UpdateEmployee (UpdateEmployeeRequest) returns (EmployeeResponse);
  
  // Delete employee
  rpc DeleteEmployee (DeleteEmployeeRequest) returns (DeleteEmployeeResponse);
  
  // Check employee exists
  rpc EmployeeExists (EmployeeExistsRequest) returns (EmployeeExistsResponse);
}

// Request/Response messages
message GetEmployeeRequest {
  string employee_id = 1;
  int32 company_id = 2;
}

message EmployeeResponse {
  string id = 1;
  string full_name = 2;
  string email = 3;
  string phone = 4;
  string department_id = 5;
  string job_title = 6;
  string employment_status = 7;
  string hire_date = 8;
  decimal base_salary = 9;
  repeated EmployeeBenefit benefits = 10;
  int32 company_id = 11;
  string created_at = 12;
  string updated_at = 13;
}

message GetEmployeesRequest {
  int32 company_id = 1;
  string department_id = 2;
  string employment_status = 3;
  int32 page = 4;
  int32 page_size = 5;
  string sort_by = 6;
}

message EmployeeListResponse {
  repeated EmployeeResponse employees = 1;
  int32 total_count = 2;
  int32 page = 3;
  int32 page_size = 4;
}

message EmployeeBenefit {
  string benefit_id = 1;
  string benefit_name = 2;
  decimal amount = 3;
}
```

### gRPC Client Implementation

```csharp
// Infrastructure/GrpcClients/EmployeeGrpcClient.cs
public interface IEmployeeGrpcClient
{
    Task<Employee> GetEmployeeAsync(string employeeId, int companyId);
    Task<List<Employee>> GetEmployeesAsync(int companyId, string department);
    Task<bool> EmployeeExistsAsync(string employeeId);
}

public class EmployeeGrpcClient : IEmployeeGrpcClient
{
    private readonly EmployeeService.EmployeeServiceClient _client;
    private readonly ILogger<EmployeeGrpcClient> _logger;
    private readonly IMetricsCollector _metrics;
    
    public EmployeeGrpcClient(
        EmployeeService.EmployeeServiceClient client,
        ILogger<EmployeeGrpcClient> logger,
        IMetricsCollector metrics)
    {
        _client = client;
        _logger = logger;
        _metrics = metrics;
    }
    
    public async Task<Employee> GetEmployeeAsync(string employeeId, int companyId)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var request = new GetEmployeeRequest
            {
                EmployeeId = employeeId,
                CompanyId = companyId
            };
            
            // Call with timeout
            var response = await _client.GetEmployeeAsync(
                request,
                deadline: DateTime.UtcNow.AddSeconds(5)
            );
            
            stopwatch.Stop();
            _metrics.RecordGrpcCall("GetEmployee", stopwatch.ElapsedMilliseconds, true);
            
            _logger.LogInformation(
                $"Retrieved employee {employeeId} via gRPC in {stopwatch.ElapsedMilliseconds}ms"
            );
            
            return MapToEmployee(response);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            stopwatch.Stop();
            _metrics.RecordGrpcCall("GetEmployee", stopwatch.ElapsedMilliseconds, false);
            
            _logger.LogWarning($"Employee {employeeId} not found");
            throw new NotFoundException($"Employee {employeeId} not found");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            stopwatch.Stop();
            _metrics.RecordGrpcCall("GetEmployee", stopwatch.ElapsedMilliseconds, false);
            
            _logger.LogError($"gRPC timeout calling Employee Service");
            throw new ServiceUnavailableException("Employee Service timeout");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordGrpcCall("GetEmployee", stopwatch.ElapsedMilliseconds, false);
            
            _logger.LogError(ex, $"Error calling Employee Service: {ex.Message}");
            throw;
        }
    }
    
    public async Task<List<Employee>> GetEmployeesAsync(int companyId, string department)
    {
        var stopwatch = Stopwatch.StartNew();
        var employees = new List<Employee>();
        
        try
        {
            var request = new GetEmployeesRequest
            {
                CompanyId = companyId,
                DepartmentId = department,
                PageSize = 1000  // Stream in pages
            };
            
            using (var call = _client.StreamEmployees(
                new StreamEmployeesRequest { CompanyId = companyId, DepartmentId = department },
                deadline: DateTime.UtcNow.AddSeconds(30)
            ))
            {
                await foreach (var employee in call.ResponseStream.ReadAllAsync())
                {
                    employees.Add(MapToEmployee(employee));
                }
            }
            
            stopwatch.Stop();
            _metrics.RecordGrpcCall("StreamEmployees", stopwatch.ElapsedMilliseconds, true);
            
            _logger.LogInformation(
                $"Retrieved {employees.Count} employees via gRPC streaming in {stopwatch.ElapsedMilliseconds}ms"
            );
            
            return employees;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordGrpcCall("StreamEmployees", stopwatch.ElapsedMilliseconds, false);
            
            _logger.LogError(ex, "Error streaming employees");
            throw;
        }
    }
    
    public async Task<bool> EmployeeExistsAsync(string employeeId)
    {
        try
        {
            var response = await _client.EmployeeExistsAsync(
                new EmployeeExistsRequest { EmployeeId = employeeId },
                deadline: DateTime.UtcNow.AddSeconds(2)
            );
            
            return response.Exists;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking employee existence");
            throw;
        }
    }
    
    private Employee MapToEmployee(EmployeeResponse response)
    {
        return new Employee
        {
            Id = response.Id,
            FullName = response.FullName,
            Email = response.Email,
            Phone = response.Phone,
            DepartmentId = response.DepartmentId,
            JobTitle = response.JobTitle,
            EmploymentStatus = response.EmploymentStatus,
            HireDate = response.HireDate,
            BaseSalary = (decimal)response.BaseSalary,
            CompanyId = response.CompanyId,
            CreatedAt = DateTime.Parse(response.CreatedAt),
            UpdatedAt = DateTime.Parse(response.UpdatedAt)
        };
    }
}

// DI Registration - Program.cs
services.AddGrpcClient<EmployeeService.EmployeeServiceClient>(options =>
{
    options.Address = new Uri("https://employee-service:5001");
})
    .AddInterceptor<GrpcLoggingInterceptor>()
    .AddInterceptor<GrpcMetricsInterceptor>();

services.AddScoped<IEmployeeGrpcClient, EmployeeGrpcClient>();
```

### gRPC Server Implementation

```csharp
// Services/EmployeeGrpcService.cs
public class EmployeeGrpcService : EmployeeService.EmployeeServiceBase
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeeGrpcService> _logger;
    
    public EmployeeGrpcService(
        IEmployeeRepository repository,
        ILogger<EmployeeGrpcService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public override async Task<EmployeeResponse> GetEmployee(
        GetEmployeeRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation($"gRPC: GetEmployee {request.EmployeeId}");
        
        var employee = await _repository.GetByIdAsync(
            request.EmployeeId,
            request.CompanyId
        );
        
        if (employee == null)
            throw new RpcException(
                new Status(StatusCode.NotFound, "Employee not found")
            );
        
        return MapToProto(employee);
    }
    
    public override async Task StreamEmployees(
        StreamEmployeesRequest request,
        IServerStreamWriter<EmployeeResponse> responseStream,
        ServerCallContext context)
    {
        _logger.LogInformation($"gRPC: StreamEmployees for company {request.CompanyId}");
        
        var pageSize = 100;
        var page = 0;
        
        while (true)
        {
            var employees = await _repository.GetPagedAsync(
                request.CompanyId,
                request.DepartmentId,
                page,
                pageSize
            );
            
            if (employees.Count == 0)
                break;
            
            foreach (var employee in employees)
            {
                await responseStream.WriteAsync(MapToProto(employee));
                
                // Check if cancellation was requested
                context.CancellationToken.ThrowIfCancellationRequested();
            }
            
            page++;
        }
    }
    
    private EmployeeResponse MapToProto(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            DepartmentId = employee.DepartmentId,
            JobTitle = employee.JobTitle,
            EmploymentStatus = employee.EmploymentStatus,
            HireDate = employee.HireDate,
            BaseSalary = (double)employee.BaseSalary,
            CompanyId = employee.CompanyId,
            CreatedAt = employee.CreatedAt.ToString("O"),
            UpdatedAt = employee.UpdatedAt.ToString("O")
        };
    }
}

// Program.cs
services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 4 * 1024 * 1024;  // 4MB
    options.MaxSendMessageSize = 4 * 1024 * 1024;
});

app.MapGrpcService<EmployeeGrpcService>();
app.MapGet("/protos/employee.proto", async context =>
{
    context.Response.ContentType = "application/octet-stream";
    await context.Response.SendFileAsync("Protos/employee.proto");
});
```

---

## Event Bus: Asynchronous Communication

### RabbitMQ + MassTransit Setup

```csharp
// Program.cs
services.AddMassTransit(x =>
{
    // Register all consumers
    x.AddConsumer<EmployeeSalaryChangedConsumer>();
    x.AddConsumer<EmployeeCreatedConsumer>();
    x.AddConsumer<EmployeeTerminatedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq-broker", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        // Payroll Service - receives salary changes
        cfg.ReceiveEndpoint("payroll-salary-changes", e =>
        {
            e.ConfigureConsumer<EmployeeSalaryChangedConsumer>(context);
            e.PrefetchCount = 10;
            e.DefaultContentType = "application/json";
        });
        
        // Analytics Service - receives all employee events
        cfg.ReceiveEndpoint("analytics-employee-events", e =>
        {
            e.ConfigureConsumer<EmployeeCreatedConsumer>(context);
            e.ConfigureConsumer<EmployeeTerminatedConsumer>(context);
        });
        
        // Notification Service - receives events
        cfg.ReceiveEndpoint("notification-events", e =>
        {
            e.ConfigureConsumer<NotificationEventConsumer>(context);
        });
    });
});
```

### Event Publishing

```csharp
// Events/DomainEvents.cs
public abstract class DomainEvent
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public int CompanyId { get; set; }
    public string TriggeredBy { get; set; }
}

public class EmployeeCreatedEvent : DomainEvent
{
    public string EmployeeId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Department { get; set; }
    public DateTime HireDate { get; set; }
}

public class EmployeeSalaryChangedEvent : DomainEvent
{
    public string EmployeeId { get; set; }
    public decimal OldSalary { get; set; }
    public decimal NewSalary { get; set; }
    public string EffectiveDate { get; set; }
}

// Services/EmployeeService.cs
public class EmployeeService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEmployeeRepository _repository;
    
    public async Task CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee { /* ... */ };
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();
        
        // Publish event
        await _publishEndpoint.Publish<EmployeeCreatedEvent>(new
        {
            EmployeeId = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Department = employee.Department,
            HireDate = employee.HireDate,
            CompanyId = employee.CompanyId,
            TriggeredBy = _currentUser.Id
        });
    }
    
    public async Task UpdateSalaryAsync(string employeeId, decimal newSalary)
    {
        var employee = await _repository.GetByIdAsync(employeeId);
        var oldSalary = employee.BaseSalary;
        
        employee.BaseSalary = newSalary;
        await _repository.UpdateAsync(employee);
        await _repository.SaveChangesAsync();
        
        // Publish event
        await _publishEndpoint.Publish<EmployeeSalaryChangedEvent>(new
        {
            EmployeeId = employeeId,
            OldSalary = oldSalary,
            NewSalary = newSalary,
            EffectiveDate = DateTime.Today.ToString("yyyy-MM-dd"),
            CompanyId = employee.CompanyId,
            TriggeredBy = _currentUser.Id
        });
    }
}
```

### Event Subscribers (Consumers)

```csharp
// Consumers/EmployeeSalaryChangedConsumer.cs
public class EmployeeSalaryChangedConsumer : IConsumer<EmployeeSalaryChangedEvent>
{
    private readonly IPayrollCalculationService _payrollService;
    private readonly ILogger<EmployeeSalaryChangedConsumer> _logger;
    
    public async Task Consume(ConsumeContext<EmployeeSalaryChangedEvent> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation(
            $"Processing salary change: Employee={@event.EmployeeId}, " +
            $"OldSalary={@event.OldSalary}, NewSalary={@event.NewSalary}"
        );
        
        try
        {
            // Recalculate payroll with new salary
            await _payrollService.RecalculatePayrollAsync(
                @event.EmployeeId,
                @event.EffectiveDate
            );
            
            _logger.LogInformation($"Payroll recalculated for {@event.EmployeeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing salary change");
            throw;  // Retry or dead-letter queue
        }
    }
}

// Consumers/EmployeeCreatedConsumer.cs (Analytics Service)
public class EmployeeCreatedConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly IAnalyticsRepository _analyticsRepo;
    
    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var @event = context.Message;
        
        // Record new hire in analytics
        await _analyticsRepo.RecordNewHireAsync(
            @event.EmployeeId,
            @event.Department,
            @event.HireDate,
            @event.CompanyId
        );
        
        // Update workforce metrics
        await _analyticsRepo.UpdateWorkforceMetricsAsync(@event.CompanyId);
    }
}
```

---

## Resilience Patterns

### Circuit Breaker

```csharp
// Polly Circuit Breaker Configuration
var policyRegistry = new PolicyRegistry
{
    {
        "EmployeeServicePolicy",
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync<HttpResponseMessage>(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for 30 seconds");
                },
                onReset: () =>
                {
                    Console.WriteLine($"Circuit breaker reset");
                }
            )
    }
};

services.AddHttpClient("EmployeeService")
    .AddPolicyHandler(policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("EmployeeServicePolicy"));

// Usage
var client = httpClientFactory.CreateClient("EmployeeService");
var response = await client.GetAsync("https://employee-service:5001/api/employees/123");
```

### Retry with Exponential Backoff

```csharp
// Polly Retry Configuration
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),  // 100ms, 200ms, 400ms
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine(
                $"Retry {retryCount} after {timespan.TotalMilliseconds}ms"
            );
        }
    );

services.AddHttpClient("PayrollService")
    .AddPolicyHandler(retryPolicy);
```

### Bulkhead Isolation

```csharp
// Limit concurrent requests to external service
var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
    maxParallelization: 50,  // Max 50 concurrent
    maxQueuingActions: 100,  // Max 100 waiting
    onBulkheadRejectedAsync: context =>
    {
        Console.WriteLine("Bulkhead rejected - too many concurrent requests");
        return Task.CompletedTask;
    }
);

services.AddHttpClient("EmployeeService")
    .AddPolicyHandler(bulkheadPolicy);
```

### Timeout

```csharp
// Add timeout to prevent hanging requests
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(5)
);

services.AddHttpClient("EmployeeService")
    .AddPolicyHandler(timeoutPolicy);
```

---

## Distributed Tracing

### OpenTelemetry + Jaeger

```csharp
// Program.cs
services.AddOpenTelemetry()
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddConsoleExporter()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "localhost";
                options.AgentPort = 6831;
            });
    });

// Automatic correlation IDs
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
    {
        context.Request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());
    }
    
    await next();
});

// View traces at: http://localhost:16686
```

---

**Last Updated:** July 2026
**Status:** Service Communication Architecture Complete
