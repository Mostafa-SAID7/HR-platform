# Kafka Integration Guide

## Overview

This document describes the Kafka integration strategy for the HR Analytics Platform, including the Outbox pattern for guaranteed event delivery and the Saga pattern for distributed transactions.

## Architecture

### Event-Driven Architecture
```
┌─────────────────────────────────────────────────────────────┐
│ Microservice (Command Handler)                              │
├─────────────────────────────────────────────────────────────┤
│ 1. Perform business operation                               │
│ 2. Generate domain events                                   │
│ 3. Save aggregate + OutboxMessages in same transaction      │
│ 4. Commit transaction (all-or-nothing)                      │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Outbox Processor (Background Service)                       │
├─────────────────────────────────────────────────────────────┤
│ 1. Poll OutboxMessages table periodically                   │
│ 2. Publish messages to Kafka                                │
│ 3. Mark as processed (ProcessedOnUtc)                       │
│ 4. Retry failed messages with exponential backoff           │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Kafka Topic (Event Store)                                   │
├─────────────────────────────────────────────────────────────┤
│ Topic: employee-events, payroll-events, etc.                │
│ Partitions: 3 (scale as needed)                             │
│ Replication Factor: 1 (dev), 3 (prod)                       │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ Consumer Services (Event Handlers)                          │
├─────────────────────────────────────────────────────────────┤
│ 1. Subscribe to relevant topics                             │
│ 2. Process events (e.g., sync to Analytics)                │
│ 3. Handle failures with dead letter queues                  │
│ 4. Update read models / projections                         │
└─────────────────────────────────────────────────────────────┘
```

## Outbox Pattern

### Purpose
Guarantee exactly-once event delivery by writing events and business data in the same database transaction.

### Implementation Steps

#### 1. Save Data + Outbox Message (Atomic Transaction)
```csharp
public async Task<CreateEmployeeResponse> Handle(CreateEmployeeCommand command)
{
    var employee = Employee.Create(...);
    
    // Save aggregate
    var repo = _unitOfWork.GetRepository<Employee>();
    await repo.AddAsync(employee);
    
    // Save domain events to OutboxMessages
    var outboxService = _unitOfWork.GetService<IOutboxService>();
    foreach (var domainEvent in employee.DomainEvents)
    {
        await outboxService.AddAsync(domainEvent);
    }
    
    // Commit: Both succeed or both fail (ACID guarantee)
    await _unitOfWork.SaveChangesAsync();
}
```

#### 2. Outbox Processor Publishes to Kafka
```csharp
public async Task ProcessOutboxAsync()
{
    // Poll unprocessed messages
    var messages = await _repository.GetUnprocessedAsync(pageSize: 100);
    
    foreach (var message in messages)
    {
        try
        {
            // Publish to Kafka
            await _kafkaProducer.SendAsync(message.Topic, message.Content);
            
            // Mark as processed
            message.ProcessedOnUtc = DateTime.UtcNow;
            await _repository.UpdateAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process outbox message {Id}", message.Id);
            // Retry logic with exponential backoff
            message.RetryCount++;
        }
    }
}
```

### Database Schema
```sql
CREATE TABLE OutboxMessages (
    Id UUID PRIMARY KEY,
    AggregateId UUID NOT NULL,
    AggregateType VARCHAR(256) NOT NULL,
    EventType VARCHAR(256) NOT NULL,
    Content TEXT NOT NULL,
    CreatedOnUtc TIMESTAMP NOT NULL,
    ProcessedOnUtc TIMESTAMP,
    Topic VARCHAR(256) NOT NULL,
    RetryCount INT DEFAULT 0,
    IsDeleted BOOLEAN DEFAULT FALSE
);

CREATE INDEX idx_outbox_processed 
ON OutboxMessages(ProcessedOnUtc) 
WHERE ProcessedOnUtc IS NULL;
```

## Saga Pattern

### Purpose
Manage distributed transactions across multiple services (e.g., Employee Onboarding Saga).

### Example: Employee Onboarding Saga
```
Employee Service
    ↓ (EmployeeCreatedEvent)
Payroll Service: Create salary record
    ↓ (PayrollCreatedEvent)
Attendance Service: Create attendance record
    ↓ (AttendanceCreatedEvent)
Notification Service: Send welcome email
    ↓ (NotificationSentEvent)
Analytics Service: Update employee analytics
    ↓ (AnalyticsUpdatedEvent)
Saga Complete ✓
```

### Compensation (Rollback on Failure)
If any step fails:
1. Payroll fails → Delete employee (EmployeeDeletedEvent)
2. Cascade compensation through all steps
3. Log failure for manual intervention

### Implementation Patterns

#### Option 1: Orchestration (Saga Orchestrator Service)
```csharp
public class EmployeeOnboardingSaga
{
    public async Task HandleEmployeeCreatedAsync(EmployeeCreatedEvent @event)
    {
        // Step 1: Create payroll record
        var payrollCommand = new CreatePayrollRecordCommand(@event.EmployeeId);
        await _mediator.Send(payrollCommand);
        
        // Step 2: Create attendance record
        var attendanceCommand = new CreateAttendanceAsync(@event.EmployeeId);
        await _mediator.Send(attendanceCommand);
        
        // Step 3: Send notification
        var notificationCommand = new SendWelcomeEmailCommand(@event.EmployeeId);
        await _mediator.Send(notificationCommand);
    }
    
    public async Task HandlePayrollFailureAsync(PayrollCreationFailedEvent @event)
    {
        // Compensation: Delete employee
        var deleteCommand = new DeleteEmployeeCommand(@event.EmployeeId);
        await _mediator.Send(deleteCommand);
    }
}
```

#### Option 2: Choreography (Event-Driven)
Each service listens to events and triggers next step:
- Employee Service publishes EmployeeCreatedEvent
- Payroll Service listens, creates payroll, publishes PayrollCreatedEvent
- Attendance Service listens, creates record, publishes AttendanceCreatedEvent
- And so on...

### Kafka Topics for Saga

| Topic | Events |
|-------|--------|
| employee-events | EmployeeCreatedEvent, EmployeeUpdatedEvent, EmployeeDeletedEvent |
| payroll-events | PayrollRecordCreatedEvent, PayrollApprovedEvent, PayrollPaidEvent |
| attendance-events | AttendanceRecordCreatedEvent, LeaveApprovedEvent |
| saga-events | SagaStartedEvent, SagaCompletedEvent, SagaFailedEvent |

## Dead Letter Queue (DLQ)

### Purpose
Handle messages that fail processing after retries.

### Implementation
```csharp
public const string DeadLetterQueueTopic = "dlq-failed-events";

public async Task HandleFailedMessageAsync(string originalTopic, string message, Exception ex)
{
    var dlqMessage = new DeadLetterMessage
    {
        OriginalTopic = originalTopic,
        Content = message,
        Error = ex.Message,
        Timestamp = DateTime.UtcNow,
        RetryCount = retryCount
    };
    
    await _kafkaProducer.SendAsync(DeadLetterQueueTopic, dlqMessage);
    _logger.LogError($"Message sent to DLQ: {originalTopic}");
}
```

## Kafka Topics Configuration

```yaml
Topics:
  - employee-events:
      partitions: 3
      replication_factor: 3
      retention_ms: 604800000  # 7 days
      
  - payroll-events:
      partitions: 3
      replication_factor: 3
      retention_ms: 2592000000  # 30 days
      
  - performance-events:
      partitions: 2
      replication_factor: 3
      retention_ms: 604800000  # 7 days
      
  - attendance-events:
      partitions: 2
      replication_factor: 3
      retention_ms: 604800000  # 7 days
      
  - saga-events:
      partitions: 1
      replication_factor: 3
      retention_ms: 86400000  # 1 day
      
  - dlq-failed-events:
      partitions: 1
      replication_factor: 3
      retention_ms: 2592000000  # 30 days (keep for investigation)
```

## Consumer Group Strategy

Each service has a consumer group to track offsets:

```
Consumer Groups:
  - employee-service-group
  - payroll-service-group
  - attendance-service-group
  - performance-service-group
  - analytics-service-group
  - notification-service-group
```

## Monitoring

### Kafka Metrics
- Message lag per consumer group
- Topic partition distribution
- Broker disk usage
- Network I/O

### Application Metrics
- Outbox messages pending
- Event processing latency
- Kafka producer errors
- Consumer lag alerts

### Alerts
- Consumer lag > 10,000 messages
- Message processing time > 5 seconds
- DLQ messages received
- Kafka broker unavailable

## Production Checklist

- [ ] Configure Kafka cluster with 3+ brokers
- [ ] Set replication factor to 3
- [ ] Configure consumer group offsets in ZooKeeper
- [ ] Set up monitoring (Prometheus + Grafana)
- [ ] Configure log retention per topic
- [ ] Set up DLQ processing job
- [ ] Implement circuit breaker for Kafka producer
- [ ] Add health checks for Kafka connectivity
- [ ] Document topic schemas with Avro/Protobuf
- [ ] Test failover scenarios
- [ ] Set up Kafka security (SASL/SSL)
- [ ] Configure backups for ZooKeeper data

## Troubleshooting

### Consumer Lag High
- Check consumer processing time
- Increase consumer instances or partitions
- Check for processing errors in logs

### Messages in DLQ
- Check application error logs
- Review message schema changes
- Manual replay if issue is fixed

### Kafka Broker Down
- Check broker logs
- Verify disk space
- Check network connectivity
- Verify ZooKeeper health

## Example: Event Publishing

```csharp
// In domain model
public class Employee : AggregateRoot
{
    public void Create(string name)
    {
        // Business logic
        
        // Publish domain event
        AddDomainEvent(new EmployeeCreatedEvent 
        { 
            EmployeeId = this.Id,
            EmployeeName = name,
            CreatedAt = DateTime.UtcNow
        });
    }
}

// In command handler
public async Task Handle(CreateEmployeeCommand command)
{
    var employee = Employee.Create(command.Name);
    
    // Save aggregate + events to OutboxMessages (same transaction)
    await _unitOfWork.GetRepository<Employee>().AddAsync(employee);
    
    foreach (var domainEvent in employee.DomainEvents)
    {
        await _outboxService.AddAsync(new OutboxMessage
        {
            AggregateId = employee.Id,
            AggregateType = nameof(Employee),
            EventType = domainEvent.GetType().Name,
            Content = JsonSerializer.Serialize(domainEvent),
            Topic = "employee-events"
        });
    }
    
    await _unitOfWork.SaveChangesAsync();
    
    // Outbox processor will publish to Kafka asynchronously
}
```

## Example: Event Publishing

```csharp
// In domain model
public class Employee : AggregateRoot
{
    public void Create(string name)
    {
        // Business logic
        
        // Publish domain event
        AddDomainEvent(new EmployeeCreatedEvent 
        { 
            EmployeeId = this.Id,
            EmployeeName = name,
            CreatedAt = DateTime.UtcNow
        });
    }
}

// In command handler
public async Task Handle(CreateEmployeeCommand command)
{
    var employee = Employee.Create(command.Name);
    
    // Save aggregate + events to OutboxMessages (same transaction)
    await _unitOfWork.GetRepository<Employee>().AddAsync(employee);
    
    foreach (var domainEvent in employee.DomainEvents)
    {
        await _outboxService.AddAsync(new OutboxMessage
        {
            AggregateId = employee.Id,
            AggregateType = nameof(Employee),
            EventType = domainEvent.GetType().Name,
            Content = JsonSerializer.Serialize(domainEvent),
            Topic = "employee-events"
        });
    }
    
    await _unitOfWork.SaveChangesAsync();
    
    // Outbox processor will publish to Kafka asynchronously
}
```

## Consumer Implementation

### Idempotent Consumer Pattern
```csharp
public class EmployeeEventConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly IRepository<EmployeeAnalytics> _repository;
    private readonly ILogger<EmployeeEventConsumer> _logger;
    
    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var @event = context.Message;
        var correlationId = context.CorrelationId;
        
        try
        {
            // Idempotency check: see if event already processed
            var existing = await _repository.FindAsync(
                a => a.EmployeeId == @event.EmployeeId && 
                     a.CorrelationId == correlationId
            );
            
            if (existing != null)
            {
                _logger.LogInformation("Event already processed: {CorrelationId}", correlationId);
                return; // Skip duplicate
            }
            
            // Process event
            var analytics = new EmployeeAnalytics
            {
                EmployeeId = @event.EmployeeId,
                EmployeeName = @event.EmployeeName,
                CorrelationId = correlationId,
                ProcessedAt = DateTime.UtcNow
            };
            
            await _repository.AddAsync(analytics);
            await _repository.UnitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Employee analytics updated: {EmployeeId}", @event.EmployeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing employee event: {CorrelationId}", correlationId);
            throw; // MassTransit will handle retry/DLQ
        }
    }
}
```

## Saga Best Practices

### Timeout Handling
```csharp
public class EmployeeOnboardingSaga : 
    StateMachine<EmployeeOnboardingState>,
    IAmStartedBy<EmployeeCreatedEvent>,
    IAmStartedBy<EmployeeOnboardingTimeout>
{
    public Event<EmployeeCreatedEvent> EmployeeCreated { get; set; }
    public Event<PayrollCreatedEvent> PayrollCreated { get; set; }
    public Event<EmployeeOnboardingTimeout> Timeout { get; set; }
    
    public EmployeeOnboardingSaga()
    {
        // Timeout: 5 minutes
        Schedule(() => Timeout, 
            instance => instance.CorrelationId, 
            x => x.Delay = TimeSpan.FromMinutes(5));
        
        // Initial state
        Initially(
            When(EmployeeCreated)
                .Then(context => LogActivity("Employee Created", context))
                .TransitionTo(WaitingForPayroll)
        );
    }
}
```

### Circuit Breaker Pattern
```csharp
public class KafkaProducerWithCircuitBreaker
{
    private readonly IProducer<string, string> _producer;
    private readonly CircuitBreakerPolicy _policy;
    
    public KafkaProducerWithCircuitBreaker(IProducer<string, string> producer)
    {
        _producer = producer;
        
        _policy = Policy
            .Handle<ProduceException<string, string>>()
            .OrResult<DeliveryReport<string, string>>(
                r => r.Status == PersistenceStatus.Persisted == false)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogWarning($"Circuit breaker opened for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                }
            );
    }
    
    public async Task SendAsync(string topic, string message)
    {
        await _policy.ExecuteAsync(async () =>
        {
            var result = await _producer.ProduceAsync(topic, 
                new Message<string, string> { Value = message });
            
            if (result.Status != PersistenceStatus.Persisted)
                throw new Exception("Message not persisted");
        });
    }
}
```

## Error Handling & DLQ Strategy

### Retry with Exponential Backoff
```csharp
public class OutboxProcessorWithRetry
{
    private readonly IOutboxRepository _repository;
    
    public async Task ProcessWithRetryAsync(OutboxMessage message)
    {
        // Max retries: 3
        const int maxRetries = 3;
        
        if (message.RetryCount >= maxRetries)
        {
            await MoveToDeadLetterQueueAsync(message);
            return;
        }
        
        try
        {
            await _kafkaProducer.SendAsync(message.Topic, message.Content);
            message.ProcessedOnUtc = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            
            // Exponential backoff: 2^retry seconds
            var delaySeconds = Math.Pow(2, message.RetryCount);
            message.RetryAfter = DateTime.UtcNow.AddSeconds(delaySeconds);
            
            _logger.LogWarning($"Retry {message.RetryCount}/{maxRetries} for message {message.Id}");
        }
        
        await _repository.UpdateAsync(message);
    }
    
    private async Task MoveToDeadLetterQueueAsync(OutboxMessage message)
    {
        var dlqMessage = new DeadLetterMessage
        {
            OriginalTopic = message.Topic,
            MessageContent = message.Content,
            FailureReason = $"Max retries ({3}) exceeded",
            Timestamp = DateTime.UtcNow
        };
        
        await _kafkaProducer.SendAsync("dlq-failed-events", dlqMessage);
        message.IsDeleted = true;
        await _repository.UpdateAsync(message);
    }
}
```

## Monitoring & Observability

### Consumer Lag Monitoring
```csharp
public class KafkaConsumerMetrics
{
    private readonly IMetricsCollector _metrics;
    
    public async Task MonitorConsumerLagAsync()
    {
        var adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = "kafka:9092" }
        ).Build();
        
        foreach (var group in new[] { "employee-service-group", "analytics-service-group" })
        {
            var offsets = await adminClient.ListGroupOffsetsAsync(group);
            
            foreach (var partition in offsets.Offsets)
            {
                var lag = partition.Value - partition.Key.Offset;
                _metrics.RecordGauge("kafka.consumer.lag", lag);
                
                if (lag > 10000)
                    _logger.LogWarning($"High consumer lag for {group}: {lag}");
            }
        }
    }
}
```

## Testing Kafka Integration

### Unit Test Example
```csharp
[Fact]
public async Task EventPublisher_PublishesEventToKafka()
{
    // Arrange
    var mockProducer = new Mock<IProducer<string, string>>();
    var publisher = new EventPublisher(mockProducer.Object);
    var @event = new EmployeeCreatedEvent { EmployeeId = Guid.NewGuid() };
    
    // Act
    await publisher.PublishAsync(@event, "employee-events");
    
    // Assert
    mockProducer.Verify(
        p => p.ProduceAsync(
            "employee-events", 
            It.IsAny<Message<string, string>>(),
            It.IsAny<CancellationToken>()
        ),
        Times.Once
    );
}
```

### Integration Test Example
```csharp
[Collection("Kafka Collection")]
public class EventConsumerIntegrationTests : IAsyncLifetime
{
    private readonly KafkaFixture _fixture;
    private IConsumer<string, string> _consumer;
    private IProducer<string, string> _producer;
    
    public async Task InitializeAsync()
    {
        _producer = _fixture.CreateProducer();
        _consumer = _fixture.CreateConsumer("test-group", "test-topic");
    }
    
    [Fact]
    public async Task Consumer_ReceivesPublishedEvent()
    {
        // Arrange
        var @event = JsonSerializer.Serialize(
            new EmployeeCreatedEvent { EmployeeId = Guid.NewGuid() }
        );
        
        // Act
        await _producer.ProduceAsync("test-topic", 
            new Message<string, string> { Value = @event });
        
        var consumed = _consumer.Consume(TimeSpan.FromSeconds(5));
        
        // Assert
        Assert.NotNull(consumed);
        Assert.Contains("EmployeeId", consumed.Message.Value);
    }
}
```

## Resources

- [Kafka Documentation](https://kafka.apache.org/documentation/)
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)
- [MassTransit Saga](https://masstransit.io/documentation/patterns/saga)
- [Debezium CDC](https://debezium.io/)
- [Polly Circuit Breaker](https://github.com/App-vNext/Polly)
- [Kafka Consumer Lag Monitoring](https://kafka.apache.org/documentation/#consumerconfigs)

---

**Last Updated**: July 21, 2026  
**Version**: 1.1 (Complete with examples)
**Status**: Production Ready
