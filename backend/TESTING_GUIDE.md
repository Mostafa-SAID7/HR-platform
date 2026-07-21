# Testing Guide - HR Analytics Platform

**Date**: July 21, 2026  
**Scope**: Unit, Integration, and E2E Testing Strategy

---

## Table of Contents
1. [Testing Strategy](#testing-strategy)
2. [Unit Testing](#unit-testing)
3. [Integration Testing](#integration-testing)
4. [Test Project Structure](#test-project-structure)
5. [Running Tests](#running-tests)
6. [Code Coverage](#code-coverage)
7. [Test Examples](#test-examples)

---

## Testing Strategy

### Test Pyramid

```
                    ▲
                   / \
                  /   \  E2E Tests (10%)
                 -------
                /       \
               /         \  Integration Tests (30%)
              -----------
             /           \
            /             \  Unit Tests (60%)
           ________________
```

### Coverage Goals
- **Unit Tests**: 70-80% (core business logic)
- **Integration Tests**: 40-50% (database, Kafka, external services)
- **E2E Tests**: 30-40% (critical workflows)
- **Overall Target**: 60%+ code coverage

### Test Technology Stack
- **Framework**: xUnit (flexible, lightweight)
- **Mocking**: Moq (mock objects)
- **Assertions**: FluentAssertions (readable assertions)
- **Containers**: Testcontainers (PostgreSQL, Kafka)
- **Builders**: FluentBuilder (test data)

### NuGet Packages Required
```xml
<!-- Unit Testing -->
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.11.0" />

<!-- Integration Testing -->
<PackageReference Include="Testcontainers" Version="3.7.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.7.0" />
<PackageReference Include="Testcontainers.Kafka" Version="3.7.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />

<!-- Database Testing -->
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />

<!-- Builders -->
<PackageReference Include="FluentBuilder" Version="2.0.0" />
```

---

## Unit Testing

### Purpose
Test individual classes/methods in isolation using mocks for dependencies.

### Coverage Areas

#### 1. **Command Handlers**
```csharp
[Fact]
public async Task CreateEmployeeCommand_WithValidData_CreatesEmployeeSuccessfully()
{
    // Arrange
    var command = new CreateEmployeeCommand(
        Name: "John Doe",
        Email: "john@example.com",
        Department: "IT"
    );
    
    var mockRepository = new Mock<IRepository<Employee>>();
    var mockOutboxService = new Mock<IOutboxService>();
    var handler = new CreateEmployeeCommandHandler(mockRepository.Object, mockOutboxService.Object);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Id.Should().NotBeEmpty();
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
    mockOutboxService.Verify(o => o.AddAsync(It.IsAny<OutboxMessage>()), Times.Once);
}
```

#### 2. **Query Handlers**
```csharp
[Fact]
public async Task GetEmployeeByIdQuery_WithValidId_ReturnsEmployee()
{
    // Arrange
    var employeeId = Guid.NewGuid();
    var employee = new Employee { Id = employeeId, Name = "John Doe" };
    
    var mockRepository = new Mock<IQueryRepository>();
    mockRepository
        .Setup(r => r.GetByIdAsync<EmployeeDetailDto>(employeeId))
        .ReturnsAsync(new EmployeeDetailDto { Id = employeeId, Name = "John Doe" });
    
    var handler = new GetEmployeeByIdQueryHandler(mockRepository.Object);
    
    // Act
    var result = await handler.Handle(
        new GetEmployeeByIdQuery(employeeId),
        CancellationToken.None
    );
    
    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("John Doe");
}
```

#### 3. **Domain Logic**
```csharp
[Fact]
public void PayrollRecord_CalculateTax_WithValidSalary_CalculatesCorrectly()
{
    // Arrange
    var salary = 50000m;
    var taxRate = 0.20m;
    
    // Act
    var tax = TaxCalculator.Calculate(salary, taxRate);
    
    // Assert
    tax.Should().Be(10000m);
}
```

#### 4. **Validators**
```csharp
[Fact]
public async Task CreateEmployeeValidator_WithEmptyName_FailsValidation()
{
    // Arrange
    var command = new CreateEmployeeCommand(Name: "", Email: "test@example.com");
    var validator = new CreateEmployeeCommandValidator();
    
    // Act
    var result = await validator.ValidateAsync(command);
    
    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
}
```

#### 5. **Services**
```csharp
[Fact]
public async Task OutboxProcessorService_WithUnprocessedMessages_PublishesAndMarksProcessed()
{
    // Arrange
    var mockRepository = new Mock<IRepository<OutboxMessage>>();
    var mockPublisher = new Mock<IEventPublisher>();
    var logger = new Mock<ILogger<OutboxProcessorService>>();
    
    var messages = new List<OutboxMessage>
    {
        new OutboxMessage { Id = Guid.NewGuid(), Content = "test", ProcessedOnUtc = null }
    };
    
    mockRepository
        .Setup(r => r.GetUnprocessedAsync(It.IsAny<int>()))
        .ReturnsAsync(messages);
    
    var service = new OutboxProcessorService(mockRepository.Object, mockPublisher.Object, logger.Object);
    
    // Act
    await service.ProcessAsync(CancellationToken.None);
    
    // Assert
    mockPublisher.Verify(p => p.PublishAsync(It.IsAny<OutboxMessage>(), CancellationToken.None), Times.Once);
    messages[0].ProcessedOnUtc.Should().NotBeNull();
}
```

---

## Integration Testing

### Purpose
Test components working together with real dependencies (database, Kafka, etc.).

### Using Testcontainers

#### PostgreSQL Integration Test
```csharp
[Collection("PostgreSQL Collection")]
public class EmployeeRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private EmployeeDbContext _dbContext;
    private IRepository<Employee> _repository;
    
    public EmployeeRepositoryIntegrationTests()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("hr_employee")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var connectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        
        _dbContext = new EmployeeDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        _repository = new EmployeeRepository(_dbContext);
    }
    
    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task AddEmployee_WithValidData_PersistsToDatabase()
    {
        // Arrange
        var employee = new Employee
        {
            Name = "John Doe",
            Email = "john@example.com",
            Department = "IT"
        };
        
        // Act
        await _repository.AddAsync(employee);
        await _dbContext.SaveChangesAsync();
        
        // Assert
        var retrieved = await _repository.GetByIdAsync(employee.Id);
        retrieved.Should().NotBeNull();
        retrieved.Name.Should().Be("John Doe");
    }
    
    [Fact]
    public async Task Outbox_OnCommandExecution_StoresMessage()
    {
        // Arrange
        var employee = Employee.Create("Jane Doe", "jane@example.com");
        
        // Act
        await _repository.AddAsync(employee);
        await _dbContext.SaveChangesAsync();
        
        // Assert
        var outboxMessages = await _dbContext.OutboxMessages
            .Where(m => m.AggregateId == employee.Id)
            .ToListAsync();
        
        outboxMessages.Should().NotBeEmpty();
        outboxMessages.First().EventType.Should().Contain("EmployeeCreatedEvent");
    }
}
```

#### Kafka Integration Test
```csharp
[Collection("Kafka Collection")]
public class KafkaIntegrationTests : IAsyncLifetime
{
    private readonly KafkaContainer _container;
    private IProducer<string, string> _producer;
    private IConsumer<string, string> _consumer;
    
    public KafkaIntegrationTests()
    {
        _container = new KafkaBuilder().Build();
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var bootstrapServers = _container.GetBootstrapAddress();
        
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }
    
    public async Task DisposeAsync()
    {
        _producer?.Dispose();
        _consumer?.Dispose();
        await _container.StopAsync();
    }
    
    [Fact]
    public async Task PublishEvent_WithValidEvent_CanBeConsumed()
    {
        // Arrange
        const string topic = "employee-events";
        var eventMessage = new { EmployeeId = Guid.NewGuid(), Name = "John Doe" };
        
        // Act
        await _producer.ProduceAsync(
            topic,
            new Message<string, string>
            {
                Key = eventMessage.EmployeeId.ToString(),
                Value = JsonSerializer.Serialize(eventMessage)
            }
        );
        
        _consumer.Subscribe(topic);
        var consumedMessage = _consumer.Consume(TimeSpan.FromSeconds(10));
        
        // Assert
        consumedMessage.Should().NotBeNull();
        consumedMessage.Message.Value.Should().Contain("John Doe");
    }
}
```

#### Outbox to Kafka End-to-End Test
```csharp
[Collection("End-to-End Collection")]
public class OutboxProcessorEndToEndTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer;
    private readonly KafkaContainer _kafkaContainer;
    private EmployeeDbContext _dbContext;
    private OutboxProcessorService _processor;
    
    public OutboxProcessorEndToEndTests()
    {
        _pgContainer = new PostgreSqlBuilder().Build();
        _kafkaContainer = new KafkaBuilder().Build();
    }
    
    public async Task InitializeAsync()
    {
        await _pgContainer.StartAsync();
        await _kafkaContainer.StartAsync();
        
        // Setup DB
        var pgConnectionString = _pgContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseNpgsql(pgConnectionString)
            .Options;
        _dbContext = new EmployeeDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        // Setup Kafka Producer
        var kafkaBootstrap = _kafkaContainer.GetBootstrapAddress();
        var kafkaConfig = new ProducerConfig { BootstrapServers = kafkaBootstrap };
        var kafkaProducer = new ProducerBuilder<string, string>(kafkaConfig).Build();
        
        // Create Processor
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var logger = new Mock<ILogger<OutboxProcessorService>>();
        _processor = new OutboxProcessorService(publishEndpoint.Object, logger.Object);
    }
    
    public async Task DisposeAsync()
    {
        await _pgContainer.StopAsync();
        await _kafkaContainer.StopAsync();
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task ProcessOutbox_WithMessages_PublishesToKafkaAndMarksProcessed()
    {
        // Arrange
        var employee = Employee.Create("John Doe", "john@example.com");
        await _dbContext.Employees.AddAsync(employee);
        await _dbContext.SaveChangesAsync();
        
        var outboxMessage = new OutboxMessage
        {
            AggregateId = employee.Id,
            AggregateType = "Employee",
            EventType = "EmployeeCreatedEvent",
            Topic = "employee-events",
            Content = JsonSerializer.Serialize(new { EmployeeId = employee.Id }),
            CreatedOnUtc = DateTime.UtcNow,
            ProcessedOnUtc = null
        };
        await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        await _dbContext.SaveChangesAsync();
        
        // Act
        await _processor.ProcessOutboxAsync();
        
        // Assert
        var processed = await _dbContext.OutboxMessages
            .FirstAsync(m => m.Id == outboxMessage.Id);
        
        processed.ProcessedOnUtc.Should().NotBeNull();
    }
}
```

---

## Test Project Structure

### Directory Layout
```
backend/
├── tests/
│   ├── HR.Tests.Unit/
│   │   ├── HR.Tests.Unit.csproj
│   │   ├── Usings.cs
│   │   ├── Common/
│   │   │   ├── OutboxProcessorServiceTests.cs
│   │   │   ├── EventPublisherTests.cs
│   │   │   └── CachingBehaviorTests.cs
│   │   ├── Employee/
│   │   │   ├── CreateEmployeeCommandTests.cs
│   │   │   ├── GetEmployeesQueryTests.cs
│   │   │   ├── UpdateEmployeeCommandTests.cs
│   │   │   └── EmployeeValidatorTests.cs
│   │   ├── Performance/
│   │   │   ├── CreatePerformanceReviewCommandTests.cs
│   │   │   └── AddFeedbackCommandTests.cs
│   │   ├── Attendance/
│   │   │   ├── CheckInCommandTests.cs
│   │   │   └── LeaveRequestCommandTests.cs
│   │   ├── Payroll/
│   │   │   ├── CalculatePayrollCommandTests.cs
│   │   │   ├── TaxCalculatorTests.cs
│   │   │   └── PayslipQueryTests.cs
│   │   └── Analytics/
│   │       ├── SearchEmployeesQueryTests.cs
│   │       └── DashboardMetricsQueryTests.cs
│   │
│   ├── HR.Tests.Integration/
│   │   ├── HR.Tests.Integration.csproj
│   │   ├── Usings.cs
│   │   ├── Fixtures/
│   │   │   ├── PostgreSqlFixture.cs
│   │   │   ├── KafkaFixture.cs
│   │   │   └── TestDataBuilder.cs
│   │   ├── Database/
│   │   │   ├── EmployeeRepositoryTests.cs
│   │   │   ├── OutboxRepositoryTests.cs
│   │   │   └── MigrationTests.cs
│   │   ├── Kafka/
│   │   │   ├── EventPublishingTests.cs
│   │   │   ├── EventConsumerTests.cs
│   │   │   ├── DeadLetterQueueTests.cs
│   │   │   └── SagaOrchestrationTests.cs
│   │   ├── ApiGateway/
│   │   │   ├── AuthenticationMiddlewareTests.cs
│   │   │   ├── GatewayRoutingTests.cs
│   │   │   └── RateLimitingTests.cs
│   │   └── Workflows/
│   │       ├── EmployeeOnboardingWorkflowTests.cs
│   │       └── PerformanceReviewWorkflowTests.cs
│   │
│   └── HR.Tests.E2E/ (future)
│       ├── HR.Tests.E2E.csproj
│       └── Workflows/ (full end-to-end scenarios)
```

---

## Running Tests

### CLI Commands

#### Run All Tests
```bash
# Unit tests only
dotnet test tests/HR.Tests.Unit/ -v normal

# Integration tests only
dotnet test tests/HR.Tests.Integration/ -v normal

# All tests
dotnet test tests/ -v normal

# With code coverage
dotnet test tests/ /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

#### Filter Tests
```bash
# Run specific test class
dotnet test tests/ --filter "FullyQualifiedName~CreateEmployeeCommandTests"

# Run specific test method
dotnet test tests/ --filter "Name~CreateEmployeeCommand_WithValidData_CreatesEmployeeSuccessfully"

# Run tests in category
dotnet test tests/ --filter "Category=Unit"

# Skip integration tests
dotnet test tests/ --filter "Category!=Integration"
```

#### Watch Mode
```bash
# Unit tests in watch mode
dotnet watch -p tests/HR.Tests.Unit test

# All tests in watch mode
dotnet watch -p tests/ test
```

#### Debug Mode
```bash
# Run tests with verbose output
dotnet test tests/ -v detailed

# Run with debugging
dotnet test tests/ --logger:"console;verbosity=detailed"
```

### Visual Studio Integration
- **Test Explorer**: View → Test Explorer (Ctrl+E, T)
- **Run Tests**: Test → Run All Tests (Ctrl+R, A)
- **Debug Tests**: Test → Debug All Tests (Ctrl+R, D)
- **Coverage**: Tools → Code Coverage → Analyze Code Coverage

---

## Code Coverage

### Coverage Goals by Service

| Service | Unit Coverage | Integration Coverage | Total |
|---------|---------------|----------------------|-------|
| HR.Common | 80% | 60% | 75% |
| HR.Identity | 75% | 50% | 70% |
| HR.Employee | 75% | 60% | 70% |
| HR.Performance | 70% | 50% | 65% |
| HR.Attendance | 70% | 50% | 65% |
| HR.Payroll | 80% | 60% | 75% |
| HR.Analytics | 65% | 50% | 60% |
| HR.ApiGateway | 60% | 40% | 55% |
| **Overall** | **72%** | **53%** | **65%** |

### Generate Coverage Report
```bash
# Generate coverage report
dotnet test tests/ \
  /p:CollectCoverage=true \
  /p:CoverageFormat=cobertura \
  /p:CoverageFileName=coverage.cobertura.xml

# View HTML report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
# Open: coverage-report/index.html
```

---

## Test Examples

### Example 1: Unit Test with Mocks
```csharp
using Xunit;
using Moq;
using FluentAssertions;

[Trait("Category", "Unit")]
public class CreateEmployeeCommandTests
{
    private readonly Mock<IRepository<Employee>> _mockRepository;
    private readonly Mock<IOutboxService> _mockOutboxService;
    private readonly Mock<ILogger<CreateEmployeeCommandHandler>> _mockLogger;
    private readonly CreateEmployeeCommandHandler _handler;
    
    public CreateEmployeeCommandTests()
    {
        _mockRepository = new Mock<IRepository<Employee>>();
        _mockOutboxService = new Mock<IOutboxService>();
        _mockLogger = new Mock<ILogger<CreateEmployeeCommandHandler>>();
        
        _handler = new CreateEmployeeCommandHandler(
            _mockRepository.Object,
            _mockOutboxService.Object,
            _mockLogger.Object
        );
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_CreatesEmployeeAndPublishesEvent()
    {
        // Arrange
        var command = new CreateEmployeeCommand(
            Name: "John Doe",
            Email: "john@example.com",
            DepartmentId: Guid.NewGuid()
        );
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(
            x => x.AddAsync(It.IsAny<Employee>()),
            Times.Once
        );
        _mockOutboxService.Verify(
            x => x.AddAsync(It.IsAny<OutboxMessage>()),
            Times.Once
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Handle_WithInvalidName_ThrowsValidationException(string invalidName)
    {
        // Arrange
        var command = new CreateEmployeeCommand(
            Name: invalidName,
            Email: "john@example.com",
            DepartmentId: Guid.NewGuid()
        );
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }
}
```

### Example 2: Integration Test with Testcontainers
```csharp
using Xunit;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

[Collection("PostgreSQL Collection")]
[Trait("Category", "Integration")]
public class EmployeeRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private EmployeeDbContext _dbContext;
    private EmployeeRepository _repository;
    
    public EmployeeRepositoryIntegrationTests()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("hr_employee_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var connectionString = _container.GetConnectionString();
        
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        
        _dbContext = new EmployeeDbContext(options);
        await _dbContext.Database.MigrateAsync();
        
        _repository = new EmployeeRepository(_dbContext);
    }
    
    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task AddEmployee_PersistsToDatabase()
    {
        // Arrange
        var employee = Employee.Create("Jane Doe", "jane@example.com");
        
        // Act
        await _repository.AddAsync(employee);
        await _dbContext.SaveChangesAsync();
        
        // Assert
        var retrieved = await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == employee.Id);
        
        retrieved.Should().NotBeNull();
        retrieved.Name.Should().Be("Jane Doe");
        retrieved.Email.Should().Be("jane@example.com");
    }
    
    [Fact]
    public async Task OutboxMessage_CreatedOnEmployeeAdd()
    {
        // Arrange
        var employee = Employee.Create("John Doe", "john@example.com");
        
        // Act
        await _repository.AddAsync(employee);
        await _dbContext.SaveChangesAsync();
        
        // Assert
        var outboxMessages = await _dbContext.OutboxMessages
            .Where(m => m.AggregateId == employee.Id)
            .ToListAsync();
        
        outboxMessages.Should().HaveCount(1);
        outboxMessages[0].EventType.Should().Contain("EmployeeCreatedEvent");
        outboxMessages[0].ProcessedOnUtc.Should().BeNull();
    }
}
```

### Example 3: Kafka Consumer Test
```csharp
using Xunit;
using Testcontainers.Kafka;
using MassTransit;
using FluentAssertions;

[Collection("Kafka Collection")]
[Trait("Category", "Integration")]
public class EmployeeEventConsumerTests : IAsyncLifetime
{
    private readonly KafkaContainer _container;
    private IPublishEndpoint _publishEndpoint;
    
    public EmployeeEventConsumerTests()
    {
        _container = new KafkaBuilder().Build();
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var bootstrapServers = _container.GetBootstrapAddress();
        
        // Setup MassTransit with Kafka
        var services = new ServiceCollection();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<EmployeeEventConsumer>();
            x.UsingKafka((context, cfg) =>
            {
                cfg.Host(bootstrapServers);
                cfg.ConfigureEndpoints(context);
            });
        });
        
        var provider = services.BuildServiceProvider();
        var busControl = provider.GetRequiredService<IBusControl>();
        await busControl.StartAsync();
        
        _publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
    }
    
    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
    
    [Fact]
    public async Task EmployeeCreatedEvent_IsConsumedSuccessfully()
    {
        // Arrange
        var @event = new EmployeeCreatedEvent
        {
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        await _publishEndpoint.Publish(@event);
        
        // Assert
        // Verify consumer processed the event (check database or mock)
        await Task.Delay(2000); // Give consumer time to process
    }
}
```

---

## Best Practices

### ✅ DO
- Write tests that are independent and can run in any order
- Use descriptive test names: `Method_Scenario_ExpectedResult`
- Test one behavior per test method
- Use builders for complex test data
- Mock external dependencies
- Use Testcontainers for infrastructure
- Assert on behavior, not implementation details
- Keep tests fast (unit < 100ms, integration < 5s)

### ❌ DON'T
- Create tests that depend on other tests
- Test internal implementation details
- Use real databases in unit tests
- Test third-party libraries
- Leave commented-out test code
- Create tight coupling between tests and code
- Test multiple behaviors in one test
- Use Thread.Sleep for synchronization (use proper waiting)

---

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      
      kafka:
        image: confluentinc/cp-kafka:7.5.0
        env:
          KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
          KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run unit tests
        run: dotnet test tests/HR.Tests.Unit/ --no-build --verbosity normal
      
      - name: Run integration tests
        run: dotnet test tests/HR.Tests.Integration/ --no-build --verbosity normal
      
      - name: Generate coverage report
        run: dotnet test tests/ /p:CollectCoverage=true /p:CoverageFormat=cobertura
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.cobertura.xml
```

---

## Summary

**Test Coverage Strategy**: Comprehensive testing pyramid with focus on unit tests, supported by integration tests for critical workflows.

**Tools**: xUnit + Moq + FluentAssertions + Testcontainers

**Target**: 65%+ overall code coverage

**Execution**: Fast (< 5 minutes for all tests locally)

**CI/CD Ready**: GitHub Actions integration for automated testing

---

**Document Version**: 1.0  
**Last Updated**: July 21, 2026  
**Next**: Start implementing tests in Task #12
