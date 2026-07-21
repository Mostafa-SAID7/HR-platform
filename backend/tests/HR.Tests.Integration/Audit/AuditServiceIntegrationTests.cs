namespace HR.Tests.Integration.Audit;

public class AuditServiceIntegrationTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly AuditEventConsumer _consumer;

    public AuditServiceIntegrationTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        var logger = new Mock<ILogger<AuditEventConsumer>>();
        var mediator = new Mock<IMediator>();
        _consumer = new AuditEventConsumer(_mockCache.Object, logger.Object, mediator.Object);
    }

    [Fact]
    public async Task AuditEvent_CreatedFromDomainEvent_ContainsAllRequiredData()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var oldValues = new { Name = "Old Name", Department = "OldDept" };
        var newValues = new { Name = "New Name", Department = "NewDept" };

        // Act
        var auditEvent = AuditEvent.FromEvent(
            entityId: entityId,
            entityType: "Employee",
            action: "Updated",
            userId: userId,
            userEmail: "admin@example.com",
            oldValues: oldValues,
            newValues: newValues,
            reason: "Employee information updated",
            severity: AuditEventSeverity.Warning,
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0");

        // Assert
        Assert.NotEqual(Guid.Empty, auditEvent.Id);
        Assert.Equal(entityId, auditEvent.EntityId);
        Assert.Equal("Employee", auditEvent.EntityType);
        Assert.Equal("Updated", auditEvent.Action);
        Assert.Equal(userId, auditEvent.UserId);
        Assert.Equal("admin@example.com", auditEvent.UserEmail);
        Assert.Equal(AuditEventSeverity.Warning, auditEvent.Severity);
        Assert.Equal("192.168.1.1", auditEvent.IpAddress);
        Assert.NotNull(auditEvent.OldValues);
        Assert.NotNull(auditEvent.NewValues);
    }

    [Fact]
    public async Task AuditTrail_AddMultipleEvents_TracksChanges()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var trail = new AuditTrail
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = "Employee",
            Events = []
        };

        var event1 = AuditEvent.FromEvent(
            entityId, "Employee", "Created", Guid.NewGuid(), "admin@example.com",
            severity: AuditEventSeverity.Info);

        var event2 = AuditEvent.FromEvent(
            entityId, "Employee", "Updated", Guid.NewGuid(), "manager@example.com",
            oldValues: new { Salary = 50000 },
            newValues: new { Salary = 55000 },
            severity: AuditEventSeverity.Warning);

        // Act
        trail.AddEvent(event1);
        trail.AddEvent(event2);

        // Assert
        Assert.Equal(2, trail.ChangeCount);
        Assert.Equal(2, trail.Events.Count);
        Assert.Contains("admin@example.com", trail.AffectedUsers);
        Assert.Contains("manager@example.com", trail.AffectedUsers);
        Assert.Equal(event1.Timestamp, trail.FirstChangeAt);
        Assert.Equal(event2.Timestamp, trail.LastChangeAt);
    }

    [Fact]
    public async Task AuditTrailSummary_CalculatesEventCounts()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var trail = new AuditTrail
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = "Payroll"
        };

        var criticalEvent = AuditEvent.FromEvent(
            entityId, "Payroll", "ProcessPayment", Guid.NewGuid(), "payroll@example.com",
            severity: AuditEventSeverity.Critical);

        var warningEvent = AuditEvent.FromEvent(
            entityId, "Payroll", "Approve", Guid.NewGuid(), "manager@example.com",
            severity: AuditEventSeverity.Warning);

        var infoEvent = AuditEvent.FromEvent(
            entityId, "Payroll", "Calculate", Guid.NewGuid(), "system@example.com",
            severity: AuditEventSeverity.Info);

        trail.AddEvent(criticalEvent);
        trail.AddEvent(warningEvent);
        trail.AddEvent(infoEvent);

        // Act
        var summary = trail.GetSummary();

        // Assert
        Assert.Equal(1, summary.CriticalEventCount);
        Assert.Equal(1, summary.WarningEventCount);
        Assert.Equal(1, summary.InfoEventCount);
        Assert.Equal(3, summary.ChangeCount);
    }

    [Fact]
    public async Task CompliancePolicy_DeterminesCriticalActions()
    {
        // Arrange
        var policy = new CompliancePolicy
        {
            Id = Guid.NewGuid(),
            Name = "HR Compliance Policy",
            Description: "Audit all HR-related critical actions",
            AuditedEntities = ["Employee", "Payroll", "Performance"],
            CriticalActions = ["Delete", "ProcessPayment", "Approve", "Terminate"],
            RetentionDays = 2555,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Assert.True(policy.ShouldAudit("Employee", "Delete"));
        Assert.True(policy.ShouldAudit("Payroll", "ProcessPayment"));
        Assert.True(policy.IsCriticalAction("Terminate"));
        Assert.False(policy.IsCriticalAction("View"));
    }

    [Fact]
    public async Task AuditReport_Compliance_StoresReportData()
    {
        // Arrange
        var generatedBy = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        // Act
        var report = AuditReport.CreateComplianceReport(
            "Monthly Compliance Report",
            startDate,
            endDate,
            generatedBy);

        // Assert
        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal("Monthly Compliance Report", report.Title);
        Assert.Equal(ReportType.Compliance, report.Type);
        Assert.Equal(ReportStatus.Generated, report.Status);
        Assert.Equal(startDate, report.StartDate);
        Assert.Equal(endDate, report.EndDate);
        Assert.Equal(generatedBy, report.GeneratedByUserId);
    }

    [Fact]
    public async Task AuditReport_Incident_CreatesIncidentReport()
    {
        // Arrange
        var generatedBy = Guid.NewGuid();
        var description = "Unauthorized access attempt detected";

        // Act
        var report = AuditReport.CreateIncidentReport(
            "Security Incident",
            description,
            generatedBy);

        // Assert
        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal("Security Incident", report.Title);
        Assert.Equal(ReportType.Incident, report.Type);
        Assert.Equal(ReportStatus.InProgress, report.Status);
        Assert.Equal(generatedBy, report.GeneratedByUserId);
    }

    [Fact]
    public async Task ConsumeEmployeeEventAsync_WithCriticalAction_CategorizesProperly()
    {
        // Arrange
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
            "Terminate",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hr@example.com",
            null,
            new { Status = "Terminated" },
            CancellationToken.None);

        // Assert - Verify critical event was stored
        _mockCache.Verify(c => c.SetStringAsync(
            It.Is<string>(key => key.StartsWith("audit:critical:")),
            It.IsAny<string>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
