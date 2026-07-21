namespace HR.Tests.Unit.Audit;

public class AuditEventConsumerTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<AuditEventConsumer>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly AuditEventConsumer _consumer;

    public AuditEventConsumerTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<AuditEventConsumer>>();
        _mockMediator = new Mock<IMediator>();
        _consumer = new AuditEventConsumer(_mockCache.Object, _mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task ConsumeEmployeeEventAsync_WithValidEvent_StoresAuditEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventType = "Created";
        var userEmail = "manager@example.com";

        _mockCache.Setup(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache.Setup(c => c.GetStringAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        // Act
        await _consumer.ConsumeEmployeeEventAsync(
            eventType,
            entityId,
            userId,
            userEmail,
            oldValues: null,
            newValues: new { Name = "John Doe", Department = "Engineering" },
            CancellationToken.None);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ConsumePayrollEventAsync_WithProcessPaymentAction_SetsCriticalSeverity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventType = "ProcessPayment";
        var userEmail = "payroll@example.com";

        _mockCache.Setup(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache.Setup(c => c.GetStringAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        // Act
        await _consumer.ConsumePayrollEventAsync(
            eventType,
            entityId,
            userId,
            userEmail,
            oldValues: null,
            newValues: new { Amount = 5000, Status = "Processed" },
            CancellationToken.None);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        // Verify critical event was indexed
        _mockCache.Verify(c => c.SetStringAsync(
            It.Is<string>(key => key.StartsWith("audit:critical:")),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConsumeRecruitmentEventAsync_WithOfferExtendedAction_SetsCriticalSeverity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eventType = "OfferExtended";
        var userEmail = "recruiter@example.com";

        _mockCache.Setup(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache.Setup(c => c.GetStringAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        // Act
        await _consumer.ConsumeRecruitmentEventAsync(
            eventType,
            entityId,
            userId,
            userEmail,
            oldValues: null,
            newValues: new { Salary = 100000, Position = "Senior Engineer" },
            CancellationToken.None);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            It.Is<string>(key => key.StartsWith("audit:critical:")),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConsumeEmployeeEventAsync_WithMultipleEvents_MaintainsAuditTrail()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockCache.Setup(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache.Setup(c => c.GetStringAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        // Act - First event
        await _consumer.ConsumeEmployeeEventAsync(
            "Created", entityId, userId, "user@example.com", null,
            new { Name = "John" }, CancellationToken.None);

        // Act - Second event
        await _consumer.ConsumeEmployeeEventAsync(
            "Updated", entityId, userId, "user@example.com",
            new { Name = "John" },
            new { Name = "John", Department = "Sales" }, CancellationToken.None);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.AtLeast(4)); // Multiple sets per event
    }
}
