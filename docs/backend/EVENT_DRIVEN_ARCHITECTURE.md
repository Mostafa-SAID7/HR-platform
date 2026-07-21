# Event-Driven Architecture

Comprehensive guide to event-driven patterns, message queues, event sourcing, and eventual consistency.

---

## Event-Driven Overview

### Architecture

```
┌─────────────────────────────────────────────────────────┐
│ Services Publishing Events                              │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Employee Service    Payroll Service    Attendance Svc  │
│  • EmployeeCreated   • PaymentProcessed • CheckinCreated│
│  • SalaryChanged     • PaymentFailed    • LeaveRequested│
│  • EmployeeTerminated• PayslipGenerated • OvertimeLogged│
│                                                         │
└─────────────────────┬─────────────────────────────────┘
                      │
                      │ Publish Events
                      ▼
        ┌─────────────────────────────────┐
        │  Message Broker                 │
        │  (RabbitMQ / Kafka)             │
        │                                 │
        │  ✓ Event Queue 1                │
        │  ✓ Event Queue 2                │
        │  ✓ Dead Letter Queue            │
        │                                 │
        └─────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┬──────────────┐
        │             │             │              │
        ▼             ▼             ▼              ▼
    Payroll Svc  Analytics Svc  Notification  Audit Service
    (Recalc)     (Update)       Service        (Log)

Benefits:
✅ Decoupling
✅ Asynchronous processing
✅ Scalability
✅ Resilience
✅ Event sourcing capability
```

---

## Domain Events

### Event Hierarchy

```csharp
// Shared/Events/DomainEvent.cs
public abstract class DomainEvent
{
    // Unique event ID
    public Guid EventId { get; } = Guid.NewGuid();
    
    // When event occurred
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    
    // Correlation for tracing
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    
    // Which tenant (company)
    public int TenantId { get; set; }
    
    // Who triggered it
    public string TriggeredBy { get; set; }
    
    // Event version for compatibility
    public int Version { get; } = 1;
}

// Human Resources Domain Events
public class EmployeeCreatedEvent : DomainEvent
{
    public string EmployeeId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string DepartmentId { get; set; }
    public string JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public string ManagerId { get; set; }
}

public class EmployeeSalaryChangedEvent : DomainEvent
{
    public string EmployeeId { get; set; }
    public decimal OldSalary { get; set; }
    public decimal NewSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string SalaryChangeReason { get; set; }
}

public class EmployeePromotedEvent : DomainEvent
{
    public string EmployeeId { get; set; }
    public string OldJobTitle { get; set; }
    public string NewJobTitle { get; set; }
    public DateTime PromotionDate { get; set; }
    public decimal SalaryIncrease { get; set; }
}

public class EmployeeTerminatedEvent : DomainEvent
{
    public string EmployeeId { get; set; }
    public DateTime TerminationDate { get; set; }
    public string TerminationReason { get; set; }
    public string SeveranceAmount { get; set; }
}

// Payroll Domain Events
public class PaymentProcessedEvent : DomainEvent
{
    public string PaymentId { get; set; }
    public string EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; }
}

public class PayslipGeneratedEvent : DomainEvent
{
    public string PayslipId { get; set; }
    public string EmployeeId { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
}

// Attendance Domain Events
public class CheckinCreatedEvent : DomainEvent
{
    public string CheckinId { get; set; }
    public string EmployeeId { get; set; }
    public DateTime CheckinTime { get; set; }
    public string Location { get; set; }
    public string Device { get; set; }
}

public class LeaveRequestedEvent : DomainEvent
{
    public string LeaveRequestId { get; set; }
    public string EmployeeId { get; set; }
    public string LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; }
}
```

---

## Event Publishing

### Outbox Pattern (Guaranteed Delivery)

```
Problem: 
Event published but service crashes before persisting to DB
→ Event lost, subscribers miss update

Solution: Outbox Pattern
1. Save event to DB with entity
2. Publish to message broker
3. If DB fails, rollback everything
4. Separate process publishes events to broker
```

```csharp
// Domain/Outbox/OutboxEvent.cs
public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; }
    public string Payload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; }
}

// Repository - Save with Transaction
public class EmployeeRepository
{
    private readonly HRContext _context;
    private readonly IMediator _mediator;
    
    public async Task CreateEmployeeAsync(Employee employee)
    {
        // 1. Add employee
        _context.Employees.Add(employee);
        
        // 2. Add events to outbox (same transaction)
        var @event = new EmployeeCreatedEvent
        {
            EmployeeId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            HireDate = employee.HireDate,
            DepartmentId = employee.DepartmentId,
            JobTitle = employee.JobTitle
        };
        
        var outboxEvent = new OutboxEvent
        {
            EventType = nameof(EmployeeCreatedEvent),
            Payload = JsonSerializer.Serialize(@event)
        };
        
        _context.OutboxEvents.Add(outboxEvent);
        
        // 3. Single transaction - all or nothing
        await _context.SaveChangesAsync();
    }
}

// Background Service - Publish Events
public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OutboxPublisher> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HRContext>();
                
                // Get unpublished events
                var unpublishedEvents = await context.OutboxEvents
                    .Where(e => e.PublishedAt == null && e.RetryCount < 3)
                    .Take(100)
                    .ToListAsync();
                
                foreach (var outboxEvent in unpublishedEvents)
                {
                    try
                    {
                        // Deserialize event
                        var eventType = Type.GetType($"HR.Events.{outboxEvent.EventType}");
                        var @event = JsonSerializer.Deserialize(
                            outboxEvent.Payload,
                            eventType
                        );
                        
                        // Publish to message broker
                        await _publishEndpoint.Publish(@event, stoppingToken);
                        
                        // Mark as published
                        outboxEvent.PublishedAt = DateTime.UtcNow;
                        context.OutboxEvents.Update(outboxEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error publishing outbox event");
                        outboxEvent.RetryCount++;
                        context.OutboxEvents.Update(outboxEvent);
                    }
                }
                
                await context.SaveChangesAsync(stoppingToken);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

// Dependency Injection
services.AddHostedService<OutboxPublisher>();
```

---

## Event Subscribers (Consumers)

### Multi-Service Event Handling

```csharp
// Consumers/EmployeeCreatedConsumer.cs (Analytics Service)
public class EmployeeCreatedConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly IAnalyticsRepository _analyticsRepo;
    private readonly ILogger<EmployeeCreatedConsumer> _logger;
    
    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation($"Processing EmployeeCreated: {@event.EmployeeId}");
        
        try
        {
            // Record new hire in analytics database
            await _analyticsRepo.RecordNewHireAsync(
                @event.EmployeeId,
                @event.DepartmentId,
                @event.HireDate,
                @event.TenantId
            );
            
            // Update workforce metrics
            await _analyticsRepo.UpdateWorkforceMetricsAsync(@event.TenantId);
            
            _logger.LogInformation($"Analytics updated for employee {@event.EmployeeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing EmployeeCreated event");
            throw;  // Let message broker handle retry
        }
    }
}

// Consumers/EmployeeSalaryChangedConsumer.cs (Payroll Service)
public class EmployeeSalaryChangedConsumer : IConsumer<EmployeeSalaryChangedEvent>
{
    private readonly IPayrollCalculationService _payrollService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<EmployeeSalaryChangedConsumer> _logger;
    
    public async Task Consume(ConsumeContext<EmployeeSalaryChangedEvent> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation(
            $"Processing salary change: {@event.EmployeeId} " +
            $"{@event.OldSalary} → {@event.NewSalary}"
        );
        
        try
        {
            // Recalculate payroll with new salary
            var recalculation = await _payrollService.RecalculatePayrollAsync(
                @event.EmployeeId,
                @event.EffectiveDate,
                @event.NewSalary
            );
            
            // Notify relevant parties
            await _notificationService.NotifyAsync(
                recipientId: $"dept-{recalculation.DepartmentId}",
                subject: "Salary Update Processed",
                body: $"Salary updated for employee {@event.EmployeeId}"
            );
            
            _logger.LogInformation($"Payroll recalculated for {@event.EmployeeId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing salary change event");
            throw;
        }
    }
}

// Consumer Registration
services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<EmployeeCreatedConsumer>();
    x.AddConsumer<EmployeeSalaryChangedConsumer>();
    x.AddConsumer<LeaveRequestedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq-broker", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        // Analytics receives employee events
        cfg.ReceiveEndpoint("analytics-employee-events", e =>
        {
            e.ConfigureConsumer<EmployeeCreatedConsumer>(context);
            e.PrefetchCount = 10;
            e.ConcurrentMessageLimit = 5;
        });
        
        // Payroll receives salary/payment events
        cfg.ReceiveEndpoint("payroll-events", e =>
        {
            e.ConfigureConsumer<EmployeeSalaryChangedConsumer>(context);
        });
    });
});
```

---

## Saga Pattern: Distributed Transactions

### Employee Onboarding Saga

```
Scenario: New employee hired
→ Create employee, provision accounts, send notification

Problem: Distributed transaction across 4 services
Solution: Saga orchestration pattern
```

```csharp
// Sagas/EmployeeOnboardingSaga.cs
public class EmployeeOnboardingSaga : StateMachine<EmployeeOnboardingState>
{
    public State Initial { get; private set; }
    public State EmployeeCreated { get; private set; }
    public State AccountsProvisioned { get; private set; }
    public State NotificationSent { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }
    
    public Event<EmployeeCreatedEvent> EmployeeCreatedEvent { get; private set; }
    public Event<AccountsProvisionedEvent> AccountsProvisionedEvent { get; private set; }
    public Event<NotificationSentEvent> NotificationSentEvent { get; private set; }
    
    public EmployeeOnboardingSaga()
    {
        // Initial state
        InstanceState(x => x.CurrentState);
        
        // Start saga
        Initially(
            When(EmployeeCreatedEvent)
                .Then(context =>
                {
                    context.Instance.EmployeeId = context.Data.EmployeeId;
                    context.Instance.TenantId = context.Data.TenantId;
                })
                .Publish(context => new ProvisionAccountsCommand
                {
                    EmployeeId = context.Data.EmployeeId,
                    Email = context.Data.Email,
                    Department = context.Data.Department
                })
                .TransitionTo(EmployeeCreated)
        );
        
        // Accounts provisioned
        During(EmployeeCreated,
            When(AccountsProvisionedEvent)
                .Publish(context => new SendOnboardingNotificationCommand
                {
                    EmployeeId = context.Instance.EmployeeId,
                    Email = context.Data.Email
                })
                .TransitionTo(AccountsProvisioned)
        );
        
        // Notification sent - saga complete
        During(AccountsProvisioned,
            When(NotificationSentEvent)
                .TransitionTo(Completed)
        );
        
        // Handle failures
        DuringAny(
            When(EmployeeCreatedEvent.IsNotHandled())
                .TransitionTo(Failed)
        );
    }
}

// Saga state
public class EmployeeOnboardingState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string EmployeeId { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Registration
services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<EmployeeOnboardingSaga, EmployeeOnboardingState>()
        .InMemoryRepository();
});
```

---

## Eventual Consistency

### Handling Stale Data

```csharp
// Problem: Employee salary changed in Employee Service
// Analytics Service might not have updated yet (eventual consistency)

// Solution 1: Accept eventual consistency for non-critical reads
public class AnalyticsService
{
    public async Task<EmployeeAnalyticsDto> GetEmployeeAnalyticsAsync(string employeeId)
    {
        // May be a few seconds stale
        var analytics = await _analyticsCache.GetAsync(employeeId);
        return analytics;
    }
}

// Solution 2: Get real-time data for critical operations
public class PayrollService
{
    private readonly IEmployeeGrpcClient _employeeClient;
    
    public async Task<PayslipDto> GeneratePayslipAsync(string employeeId)
    {
        // Call Employee Service directly (strongly consistent)
        var currentEmployee = await _employeeClient.GetEmployeeAsync(employeeId);
        
        // Use current salary for payslip calculation
        var payslip = new Payslip
        {
            EmployeeId = employeeId,
            BaseSalary = currentEmployee.BaseSalary,
            // ... calculate deductions, taxes, net
        };
        
        return payslip;
    }
}

// Solution 3: Cache invalidation on events
public class CacheInvalidationConsumer : IConsumer<EmployeeSalaryChangedEvent>
{
    private readonly IDistributedCache _cache;
    
    public async Task Consume(ConsumeContext<EmployeeSalaryChangedEvent> context)
    {
        var @event = context.Message;
        
        // Invalidate cache immediately
        await _cache.RemoveAsync($"employee:{@event.EmployeeId}");
        await _cache.RemoveAsync($"analytics:employee:{@event.EmployeeId}");
    }
}
```

---

## Dead Letter Queue (DLQ)

```csharp
// Handle permanently failed messages
public class FailedEventConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly ILogger<FailedEventConsumer> _logger;
    private readonly IDeadLetterService _deadLetterService;
    
    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        try
        {
            // Process event
            // ...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event processing failed - moving to DLQ");
            
            await _deadLetterService.RecordFailedEventAsync(
                eventId: context.Message.EventId,
                eventType: nameof(EmployeeCreatedEvent),
                payload: context.Message,
                error: ex.Message,
                retryCount: context.GetRetryCount()
            );
            
            // Notify operations team
            throw;  // Message broker moves to DLQ
        }
    }
}
```

---

**Last Updated:** July 2026
**Status:** Event-Driven Architecture Complete
