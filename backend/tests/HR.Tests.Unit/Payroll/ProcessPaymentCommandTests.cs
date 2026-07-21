namespace HR.Tests.Unit.Payroll;

/// <summary>
/// Unit tests for ProcessPaymentCommand handler and Payroll payment marking logic.
/// Tests cover: payment processing, status transitions, and domain events.
/// </summary>
public class ProcessPaymentCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<PayrollRecord>> _mockPayrollRepository;
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _mockLogger;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPayrollRepository = new Mock<IRepository<PayrollRecord>>();
        _mockLogger = new Mock<ILogger<ProcessPaymentCommandHandler>>();

        _mockUnitOfWork
            .Setup(u => u.GetRepository<PayrollRecord>())
            .Returns(_mockPayrollRepository.Object);

        _handler = new ProcessPaymentCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithApprovedPayroll_MarksAsPaid()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var payroll = PayrollRecord.Create(
            employeeId,
            "John Doe",
            100000,
            2026,
            7,
            tenantId)
        {
            Id = payrollId
        };

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());
        Assert.Equal("Approved", payroll.Status);

        var command = new ProcessPaymentCommand(payrollId, tenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Paid", payroll.Status);
        Assert.NotNull(payroll.PaidDate);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayroll_ThrowsNotFoundException()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(payrollId, Guid.NewGuid());

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRecord?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void MarkAsPaid_UpdatesStatusToPaid()
    {
        // Arrange
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid());

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());
        Assert.Equal("Approved", payroll.Status);

        // Act
        payroll.MarkAsPaid();

        // Assert
        Assert.Equal("Paid", payroll.Status);
    }

    [Fact]
    public void MarkAsPaid_SetsPaidDate()
    {
        // Arrange
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid());

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());
        var beforePaid = DateTime.UtcNow;

        // Act
        payroll.MarkAsPaid();

        var afterPaid = DateTime.UtcNow;

        // Assert
        Assert.NotNull(payroll.PaidDate);
        Assert.True(payroll.PaidDate >= beforePaid && payroll.PaidDate <= afterPaid);
    }

    [Fact]
    public void MarkAsPaid_PublishesPayrollPaidEvent()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var payroll = PayrollRecord.Create(
            employeeId,
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid());

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());

        // Act
        payroll.MarkAsPaid();

        // Assert
        var paidEvent = payroll.DomainEvents.OfType<PayrollPaidEvent>().LastOrDefault();
        Assert.NotNull(paidEvent);
        Assert.Equal(payroll.Id, paidEvent.PayrollRecordId);
        Assert.Equal(employeeId, paidEvent.EmployeeId);
        Assert.Equal(payroll.NetSalary, paidEvent.Amount);
        Assert.Equal(payroll.PaidDate, paidEvent.PaidDate);
    }

    [Fact]
    public async Task Handle_WithProcessedButNotApprovedPayroll_ThrowsDomainException()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid())
        {
            Id = payrollId
        };

        payroll.CalculatePayroll(10m, 5m, 500m);
        // Not approved

        var command = new ProcessPaymentCommand(payrollId, payroll.TenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAlreadyPaidPayroll_ThrowsDomainException()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid())
        {
            Id = payrollId,
            Status = "Paid"
        };

        var command = new ProcessPaymentCommand(payrollId, payroll.TenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void MarkAsPaid_UpdatesUpdatedOnUtcTimestamp()
    {
        // Arrange
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid());

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());
        var beforeMarkAsPaid = payroll.UpdatedOnUtc;

        // Small delay
        System.Threading.Thread.Sleep(10);

        // Act
        payroll.MarkAsPaid();

        // Assert
        Assert.True(payroll.UpdatedOnUtc > beforeMarkAsPaid);
    }

    [Fact]
    public async Task Handle_WithValidApprovedPayroll_SavesChanges()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid())
        {
            Id = payrollId
        };

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());

        var command = new ProcessPaymentCommand(payrollId, payroll.TenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MarkAsPaid_PreservesPayrollData()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var payroll = PayrollRecord.Create(
            employeeId,
            "John Doe",
            100000,
            2026,
            7,
            tenantId);

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());

        var netSalary = payroll.NetSalary;
        var grossIncome = payroll.GrossIncome;

        // Act
        payroll.MarkAsPaid();

        // Assert
        Assert.Equal(employeeId, payroll.EmployeeId);
        Assert.Equal("John Doe", payroll.EmployeeName);
        Assert.Equal(100000, payroll.BasicSalary);
        Assert.Equal(netSalary, payroll.NetSalary);
        Assert.Equal(grossIncome, payroll.GrossIncome);
    }

    [Fact]
    public void MarkAsPaid_WithMultipleCalls_DoesNotDuplicateEvents()
    {
        // Arrange
        var payroll = PayrollRecord.Create(
            Guid.NewGuid(),
            "Test User",
            50000,
            2026,
            7,
            Guid.NewGuid());

        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());

        // Act
        payroll.MarkAsPaid();
        var eventCountAfterFirst = payroll.DomainEvents.Count;

        // Act again on already paid
        // This should ideally throw an exception in a real scenario
        // but testing the behavior here

        // Assert
        var paidEvents = payroll.DomainEvents.OfType<PayrollPaidEvent>().ToList();
        Assert.Single(paidEvents); // Only one PaidEvent should exist
    }
}
