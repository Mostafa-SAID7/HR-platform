namespace HR.Tests.Unit.Payroll;

/// <summary>
/// Unit tests for ApprovePayrollCommand handler and Payroll aggregate approval logic.
/// Tests cover: valid approval, state transitions, and approval event publishing.
/// </summary>
public class ApprovePayrollCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<PayrollRecord>> _mockPayrollRepository;
    private readonly Mock<ILogger<ApprovePayrollCommandHandler>> _mockLogger;
    private readonly ApprovePayrollCommandHandler _handler;

    public ApprovePayrollCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPayrollRepository = new Mock<IRepository<PayrollRecord>>();
        _mockLogger = new Mock<ILogger<ApprovePayrollCommandHandler>>();

        _mockUnitOfWork
            .Setup(u => u.GetRepository<PayrollRecord>())
            .Returns(_mockPayrollRepository.Object);

        _handler = new ApprovePayrollCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidPayroll_ApprovesSuccessfully()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var approverIds = Guid.NewGuid();
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
        Assert.Equal("Processed", payroll.Status);

        var command = new ApprovePayrollCommand(payrollId, approverIds, tenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Approved", payroll.Status);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayroll_ThrowsNotFoundException()
    {
        // Arrange
        var payrollId = Guid.NewGuid();
        var command = new ApprovePayrollCommand(payrollId, Guid.NewGuid(), Guid.NewGuid());

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRecord?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void Approve_UpdatesStatus()
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
        Assert.Equal("Processed", payroll.Status);

        var approverId = Guid.NewGuid();

        // Act
        payroll.Approve(approverId);

        // Assert
        Assert.Equal("Approved", payroll.Status);
    }

    [Fact]
    public void Approve_PublishesPayrollApprovedEvent()
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

        var approverId = Guid.NewGuid();

        // Act
        payroll.Approve(approverId);

        // Assert
        Assert.NotEmpty(payroll.DomainEvents);
        var approvalEvent = payroll.DomainEvents.OfType<PayrollApprovedEvent>().FirstOrDefault();
        Assert.NotNull(approvalEvent);
        Assert.Equal(payroll.Id, approvalEvent.PayrollRecordId);
        Assert.Equal(payroll.EmployeeId, approvalEvent.EmployeeId);
        Assert.Equal(approverId, approvalEvent.ApprovedBy);
    }

    [Fact]
    public void Approve_UpdatesUpdatedOnUtcTimestamp()
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
        var beforeApproval = payroll.UpdatedOnUtc;

        // Small delay to ensure timestamp difference
        System.Threading.Thread.Sleep(10);

        // Act
        payroll.Approve(Guid.NewGuid());

        // Assert
        Assert.True(payroll.UpdatedOnUtc > beforeApproval);
    }

    [Fact]
    public void Approve_PreservesPayrollData()
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

        payroll.HousingAllowance = 15000;
        payroll.TransportAllowance = 5000;
        payroll.OtherAllowances = 5000;
        payroll.CalculatePayroll(10m, 5m, 500m);

        var grossIncome = payroll.GrossIncome;
        var netSalary = payroll.NetSalary;

        // Act
        payroll.Approve(Guid.NewGuid());

        // Assert
        Assert.Equal(employeeId, payroll.EmployeeId);
        Assert.Equal("John Doe", payroll.EmployeeName);
        Assert.Equal(100000, payroll.BasicSalary);
        Assert.Equal(grossIncome, payroll.GrossIncome);
        Assert.Equal(netSalary, payroll.NetSalary);
    }

    [Fact]
    public async Task Handle_WithAlreadyApprovedPayroll_ThrowsDomainException()
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
            Id = payrollId,
            Status = "Approved" // Already approved
        };

        var command = new ApprovePayrollCommand(payrollId, Guid.NewGuid(), tenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithPaidPayroll_ThrowsDomainException()
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
            Id = payrollId,
            Status = "Paid" // Already paid
        };

        var command = new ApprovePayrollCommand(payrollId, Guid.NewGuid(), tenantId);

        _mockPayrollRepository
            .Setup(r => r.GetByIdAsync(payrollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payroll);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void Approve_WithValidApproverId_SetsCorrectApprovalId()
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

        var approverId = Guid.NewGuid();

        // Act
        payroll.Approve(approverId);

        // Assert
        var approvalEvent = payroll.DomainEvents.OfType<PayrollApprovedEvent>().FirstOrDefault();
        Assert.NotNull(approvalEvent);
        Assert.Equal(approverId, approvalEvent.ApprovedBy);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
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

        var command = new ApprovePayrollCommand(payrollId, Guid.NewGuid(), payroll.TenantId);

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
}
