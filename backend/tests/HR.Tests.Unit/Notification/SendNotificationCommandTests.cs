namespace HR.Tests.Unit.Notification;

public class SendNotificationCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Notification>> _mockRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<SendNotificationCommandHandler>> _mockLogger;
    private readonly SendNotificationCommandHandler _handler;

    public SendNotificationCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IRepository<Notification>>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<SendNotificationCommandHandler>>();
        
        _handler = new SendNotificationCommandHandler(
            _mockUnitOfWork.Object,
            _mockRepository.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidEmailRequest_SendsNotificationSuccessfully()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var request = new SendNotificationRequest(
            RecipientId: recipientId,
            RecipientEmail: "user@example.com",
            RecipientPhone: "+1234567890",
            NotificationType: "SystemAlert",
            Channel: "Email",
            Title: "Test Notification",
            Content: "This is a test notification content");

        var command = new SendNotificationCommand(request, Guid.NewGuid());

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService.Setup(s => s.SendAsync(
            It.IsAny<string>(),
            It.IsAny<NotificationChannel>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Notification", result.Title);
        Assert.Equal("SystemAlert", result.Type);
        Assert.Equal("Email", result.Channel);
        Assert.Equal("Sent", result.Status);

        _mockNotificationService.Verify(s => s.SendAsync(
            "user@example.com",
            NotificationChannel.Email,
            "Test Notification",
            "This is a test notification content",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailedSend_MarksNotificationAsFailed()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var request = new SendNotificationRequest(
            RecipientId: recipientId,
            RecipientEmail: "user@example.com",
            RecipientPhone: "+1234567890",
            NotificationType: "PerformanceReviewDue",
            Channel: "SMS",
            Title: "Performance Review",
            Content: "Your performance review is due");

        var command = new SendNotificationCommand(request, Guid.NewGuid());

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService.Setup(s => s.SendAsync(
            It.IsAny<string>(),
            It.IsAny<NotificationChannel>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Failed", result.Status);
    }

    [Fact]
    public async Task Handle_WithInvalidChannel_ThrowsDomainException()
    {
        // Arrange
        var request = new SendNotificationRequest(
            RecipientId: Guid.NewGuid(),
            RecipientEmail: "user@example.com",
            RecipientPhone: "+1234567890",
            NotificationType: "SystemAlert",
            Channel: "InvalidChannel",
            Title: "Test",
            Content: "Test content");

        var command = new SendNotificationCommand(request, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMultipleMetadata_IncludesAllMetadata()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var metadata = new Dictionary<string, object>
        {
            { "correlationId", "123-456-789" },
            { "source", "PerformanceService" },
            { "priority", "High" }
        };

        var request = new SendNotificationRequest(
            RecipientId: recipientId,
            RecipientEmail: "user@example.com",
            RecipientPhone: "+1234567890",
            NotificationType: "PerformanceReviewDue",
            Channel: "InApp",
            Title: "Review Due",
            Content: "Your review is overdue",
            Metadata: metadata);

        var command = new SendNotificationCommand(request, Guid.NewGuid());

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService.Setup(s => s.SendAsync(
            It.IsAny<string>(),
            It.IsAny<NotificationChannel>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }
}
