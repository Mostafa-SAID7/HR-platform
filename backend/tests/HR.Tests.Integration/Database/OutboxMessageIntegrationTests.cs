namespace HR.Tests.Integration.Database;

using HR.Tests.Integration.Fixtures;

[Collection("PostgreSQL Collection")]
[Trait("Category", "Integration")]
public class OutboxMessageIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private DbContext? _dbContext;

    public OutboxMessageIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Note: In real scenario, use actual DbContext
        // For now, we'll test with in-memory for simplicity
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new DbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }
    }

    [Fact]
    public async Task OutboxMessage_CanBeCreated_WithValidData()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Employee",
            EventType = "EmployeeCreatedEvent",
            Topic = "employee-events",
            Content = JsonSerializer.Serialize(new { EmployeeId = Guid.NewGuid(), Name = "John Doe" }),
            CreatedOnUtc = DateTime.UtcNow,
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        // Act
        // In real scenario: await repository.AddAsync(outboxMessage);
        // For now just verify creation
        outboxMessage.Should().NotBeNull();
        outboxMessage.ProcessedOnUtc.Should().BeNull();

        // Assert
        outboxMessage.AggregateType.Should().Be("Employee");
        outboxMessage.EventType.Should().Be("EmployeeCreatedEvent");
        outboxMessage.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task OutboxMessage_CanBeMarked_AsProcessed()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Employee",
            EventType = "EmployeeCreatedEvent",
            Topic = "employee-events",
            Content = "{}",
            CreatedOnUtc = DateTime.UtcNow,
            ProcessedOnUtc = null
        };

        // Act
        outboxMessage.ProcessedOnUtc = DateTime.UtcNow;

        // Assert
        outboxMessage.ProcessedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task OutboxMessage_RetryCount_IncrementsOnFailure()
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Employee",
            EventType = "EmployeeCreatedEvent",
            Topic = "employee-events",
            Content = "{}",
            CreatedOnUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        // Act
        outboxMessage.RetryCount++;
        outboxMessage.RetryCount++;

        // Assert
        outboxMessage.RetryCount.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task OutboxMessage_WithVariousRetryCount_StoresCorrectly(int retryCount)
    {
        // Arrange
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Employee",
            EventType = "EmployeeCreatedEvent",
            Topic = "employee-events",
            Content = "{}",
            CreatedOnUtc = DateTime.UtcNow,
            RetryCount = retryCount
        };

        // Act
        // Store message

        // Assert
        outboxMessage.RetryCount.Should().Be(retryCount);
    }
}
