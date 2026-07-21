namespace HR.Tests.Integration.Notification;

public class NotificationServiceIntegrationTests : IAsyncLifetime
{
    private PostgreSqlFixture _dbFixture;
    private IServiceProvider _serviceProvider;
    private Mock<INotificationService> _mockNotificationService;

    public async Task InitializeAsync()
    {
        _dbFixture = new PostgreSqlFixture("hr_notification_test");
        await _dbFixture.InitializeAsync();

        var services = new ServiceCollection();
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(_dbFixture.ConnectionString));
        services.AddScoped<IUnitOfWork, NotificationDbContext>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        _mockNotificationService = new Mock<INotificationService>();
        services.AddScoped(_ => _mockNotificationService.Object);

        var logger = new Mock<ILogger<SendNotificationCommandHandler>>();
        services.AddScoped(_ => logger.Object);

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            await context.Database.MigrateAsync();
        }
    }

    [Fact]
    public async Task SendNotification_WithEmailChannel_StoresNotificationRecord()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var recipientId = Guid.NewGuid();
            var notification = Notification.Create(
                recipientId,
                "user@example.com",
                "+1-555-0100",
                NotificationType.EmployeeCreated,
                NotificationChannel.Email,
                "Welcome",
                "Welcome to the HR Platform",
                new Dictionary<string, object> { { "employeeId", recipientId } });

            // Act
            await repository.AddAsync(notification);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await repository.GetByIdAsync(notification.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("user@example.com", retrieved.RecipientEmail);
            Assert.Equal(NotificationStatus.Pending, retrieved.Status);
        }
    }

    [Fact]
    public async Task MarkNotificationAsRead_UpdatesStatus()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var notification = Notification.Create(
                Guid.NewGuid(),
                "user@example.com",
                "+1-555-0101",
                NotificationType.PerformanceReviewDue,
                NotificationChannel.InApp,
                "Review Due",
                "Your performance review is due");

            await repository.AddAsync(notification);
            await unitOfWork.SaveChangesAsync();

            // Act
            notification.MarkAsRead();
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await repository.GetByIdAsync(notification.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(NotificationStatus.Read, retrieved.Status);
            Assert.NotNull(retrieved.ReadAt);
        }
    }

    [Fact]
    public async Task UpdateNotificationPreference_StoresUserPreferences()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<NotificationPreference>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var userId = Guid.NewGuid();
            var preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmailEnabled = true,
                SmsEnabled = false,
                PushEnabled = true,
                InAppEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(preference);
            await unitOfWork.SaveChangesAsync();

            // Assert
            var retrieved = await repository.GetByIdAsync(preference.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(userId, retrieved.UserId);
            Assert.True(retrieved.EmailEnabled);
            Assert.False(retrieved.SmsEnabled);
            Assert.True(retrieved.PushEnabled);
        }
    }

    [Fact]
    public async Task SendMultipleNotifications_ToMultipleChannels()
    {
        // Arrange
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var recipientId = Guid.NewGuid();
            var notifications = new[]
            {
                Notification.Create(recipientId, "user@example.com", "+1-555-0102", NotificationType.LeaveApproved, NotificationChannel.Email, "Leave Approved", "Your leave request has been approved"),
                Notification.Create(recipientId, "user@example.com", "+1-555-0102", NotificationType.LeaveApproved, NotificationChannel.SMS, "Leave Approved", "Your leave approved"),
                Notification.Create(recipientId, "user@example.com", "+1-555-0102", NotificationType.LeaveApproved, NotificationChannel.InApp, "Leave Approved", "Your leave request has been approved")
            };

            // Act
            foreach (var notification in notifications)
            {
                await repository.AddAsync(notification);
            }
            await unitOfWork.SaveChangesAsync();

            // Assert
            Assert.Equal(3, notifications.Length);
            foreach (var notification in notifications)
            {
                var retrieved = await repository.GetByIdAsync(notification.Id);
                Assert.NotNull(retrieved);
            }
        }
    }

    public async Task DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
    }
}
