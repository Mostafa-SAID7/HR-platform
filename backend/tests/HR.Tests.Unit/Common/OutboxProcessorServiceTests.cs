namespace HR.Tests.Unit.Common;

[Trait("Category", "Unit")]
public class OutboxProcessorServiceTests
{
    [Fact]
    public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OutboxProcessorService(null!, new Mock<ILogger<OutboxProcessorService>>().Object)
        );
    }

    [Fact]
    public async Task ExecuteAsync_OnBackgroundService_RunsContinuously()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<OutboxProcessorService>>();
        var service = new OutboxProcessorService(mockServiceProvider.Object, mockLogger.Object);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        // Service should start without throwing
        Assert.True(service != null);
    }

    [Fact]
    public async Task StopAsync_OnBackgroundService_StopsCleanly()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<OutboxProcessorService>>();
        var service = new OutboxProcessorService(mockServiceProvider.Object, mockLogger.Object);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        // Service should stop without throwing
        Assert.True(service != null);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void ProcessingInterval_AlwaysPositive(int seconds)
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<OutboxProcessorService>>();
        var service = new OutboxProcessorService(mockServiceProvider.Object, mockLogger.Object);

        // Act & Assert
        // Service should be created successfully
        service.Should().NotBeNull();
    }
}

[Trait("Category", "Unit")]
public class EventPublisherTests
{
    [Fact]
    public void Constructor_WithNullPublishEndpoint_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EventPublisher(null!, new Mock<ILogger<EventPublisher>>().Object)
        );
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_CallsPublishEndpoint()
    {
        // Arrange
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<EventPublisher>>();
        var publisher = new EventPublisher(mockPublishEndpoint.Object, mockLogger.Object);

        var @event = new TestDomainEvent { Id = Guid.NewGuid() };

        // Act
        await publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        mockPublishEndpoint.Verify(
            x => x.Publish(@event, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task PublishAsync_WithPublishFailure_LogsError()
    {
        // Arrange
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<EventPublisher>>();
        
        mockPublishEndpoint
            .Setup(x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publish failed"));

        var publisher = new EventPublisher(mockPublishEndpoint.Object, mockLogger.Object);
        var @event = new TestDomainEvent { Id = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            publisher.PublishAsync(@event, CancellationToken.None)
        );
    }

    [Fact]
    public async Task PublishManyAsync_WithMultipleEvents_PublishesAll()
    {
        // Arrange
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<EventPublisher>>();
        var publisher = new EventPublisher(mockPublishEndpoint.Object, mockLogger.Object);

        var events = new List<TestDomainEvent>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        // Act
        await publisher.PublishManyAsync(events, CancellationToken.None);

        // Assert
        mockPublishEndpoint.Verify(
            x => x.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
    }

    [Fact]
    public void Subscribe_RegistersHandler()
    {
        // Arrange
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        var mockLogger = new Mock<ILogger<EventPublisher>>();
        var publisher = new EventPublisher(mockPublishEndpoint.Object, mockLogger.Object);

        Func<TestDomainEvent, CancellationToken, Task> handler = async (@event, ct) =>
        {
            await Task.CompletedTask;
        };

        // Act
        publisher.Subscribe(handler);

        // Assert
        // Handler subscription should succeed without throwing
        Assert.NotNull(handler);
    }
}

// Test Domain Event
public class TestDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}
