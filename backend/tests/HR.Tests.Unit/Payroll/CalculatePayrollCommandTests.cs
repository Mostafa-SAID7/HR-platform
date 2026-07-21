namespace HR.Tests.Unit.Payroll;

/// <summary>
/// Unit tests for CalculatePayrollCommand handler.
/// Tests cover: basic salary calculation, tax computation, deductions, allowances, and edge cases.
/// </summary>
public class CalculatePayrollCommandTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<PayrollRecord>> _mockPayrollRepository;
    private readonly Mock<ILogger<CalculatePayrollCommandHandler>> _mockLogger;
    private readonly CalculatePayrollCommandHandler _handler;

    public CalculatePayrollCommandTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPayrollRepository = new Mock<IRepository<PayrollRecord>>();
        _mockLogger = new Mock<ILogger<CalculatePayrollCommandHandler>>();

        _mockUnitOfWork
            .Setup(u => u.GetRepository<PayrollRecord>())
            .Returns(_mockPayrollRepository.Object);

        _handler = new CalculatePayrollCommandHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CalculatesPayrollCorrectly()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "John Doe",
            BasicSalary = 100000,
            HousingAllowance = 15000,
            TransportAllowance = 5000,
            OtherAllowances = 5000
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord>().AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockPayrollRepository
            .Setup(r => r.AddAsync(It.IsAny<PayrollRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("John Doe", result.EmployeeName);
        Assert.Equal(100000, result.BasicSalary);
        Assert.Equal(130000, result.GrossIncome); // 100k + 15k + 5k + 5k + 5k
        Assert.Equal("Processed", result.Status);

        // Verify calculations: Tax 10%, SSC 5%, Health Insurance 500
        decimal expectedTax = (130000 * 10m) / 100m; // 13000
        decimal expectedSSC = (130000 * 5m) / 100m;  // 6500
        Assert.Equal(expectedTax, result.IncomeTax);
        Assert.Equal(expectedSSC, result.SocialSecurityContribution);
        Assert.Equal(500, result.HealthInsurance);

        decimal expectedNetSalary = 130000 - expectedTax - expectedSSC - 500; // 110000
        Assert.Equal(expectedNetSalary, result.NetSalary);
    }

    [Fact]
    public async Task Handle_WithBasicSalaryOnly_CalculatesCorrectly()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "Jane Smith",
            BasicSalary = 50000,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord>().AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockPayrollRepository
            .Setup(r => r.AddAsync(It.IsAny<PayrollRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(50000, result.GrossIncome);
        Assert.Equal(5000, result.IncomeTax); // 50000 * 10%
        Assert.Equal(2500, result.SocialSecurityContribution); // 50000 * 5%
        Assert.Equal(500, result.HealthInsurance);
        Assert.Equal(42000, result.NetSalary); // 50000 - 5000 - 2500 - 500
    }

    [Fact]
    public async Task Handle_WithMultipleAllowances_CalculatesGrossIncome()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "Bob Johnson",
            BasicSalary = 80000,
            HousingAllowance = 20000,
            TransportAllowance = 10000,
            OtherAllowances = 15000
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord>().AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockPayrollRepository
            .Setup(r => r.AddAsync(It.IsAny<PayrollRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(80000, result.BasicSalary);
        Assert.Equal(20000, result.HousingAllowance);
        Assert.Equal(10000, result.TransportAllowance);
        Assert.Equal(15000, result.OtherAllowances);
        Assert.Equal(125000, result.GrossIncome); // 80 + 20 + 10 + 15
    }

    [Fact]
    public async Task Handle_WithZeroBasicSalary_ThrowsValidationException()
    {
        // Arrange
        var request = new CalculatePayrollRequest
        {
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "Invalid User",
            BasicSalary = 0,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, Guid.NewGuid());
        var validator = new CalculatePayrollCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName.Contains("BasicSalary"));
    }

    [Fact]
    public async Task Handle_WithInvalidMonth_ThrowsValidationException()
    {
        // Arrange
        var request = new CalculatePayrollRequest
        {
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "Test User",
            BasicSalary = 50000,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0,
            Month = 13 // Invalid month
        };

        var command = new CalculatePayrollCommand(request, Guid.NewGuid());
        var validator = new CalculatePayrollCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public async Task Handle_WithInvalidYear_ThrowsValidationException()
    {
        // Arrange
        var request = new CalculatePayrollRequest
        {
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "Test User",
            BasicSalary = 50000,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0,
            Year = 1999 // Invalid year
        };

        var command = new CalculatePayrollCommand(request, Guid.NewGuid());
        var validator = new CalculatePayrollCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public async Task Handle_WithExistingDraftPayroll_UpdatesPayroll()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var existingPayroll = PayrollRecord.Create(
            employeeId,
            "John Doe",
            50000,
            2026,
            7,
            tenantId);

        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "John Doe",
            BasicSalary = 60000, // Updated salary
            HousingAllowance = 10000,
            TransportAllowance = 5000,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord> { existingPayroll }.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(75000, result.GrossIncome); // 60 + 10 + 5
        _mockPayrollRepository.Verify(r => r.Update(It.IsAny<PayrollRecord>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithProcessedPayroll_ThrowsDomainException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var processedPayroll = PayrollRecord.Create(
            employeeId,
            "John Doe",
            50000,
            2026,
            7,
            tenantId);

        processedPayroll.CalculatePayroll(10m, 5m, 500m);
        Assert.Equal("Processed", processedPayroll.Status);

        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "John Doe",
            BasicSalary = 60000,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord> { processedPayroll }.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNegativeBasicSalary_ThrowsValidationException()
    {
        // Arrange
        var request = new CalculatePayrollRequest
        {
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "Invalid User",
            BasicSalary = -10000,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, Guid.NewGuid());
        var validator = new CalculatePayrollCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public async Task Handle_TaxCalculation_UsesCorrectRate()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "Test User",
            BasicSalary = 200000, // High salary to test tax calculation
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord>().AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockPayrollRepository
            .Setup(r => r.AddAsync(It.IsAny<PayrollRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        decimal expectedTax = (200000 * 10m) / 100m; // 20000 at 10%
        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Fact]
    public async Task Handle_SSCCalculation_UsesCorrectRate()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var request = new CalculatePayrollRequest
        {
            EmployeeId = employeeId,
            EmployeeName = "Test User",
            BasicSalary = 100000,
            HousingAllowance = 0,
            TransportAllowance = 0,
            OtherAllowances = 0
        };

        var command = new CalculatePayrollCommand(request, tenantId);

        var queryable = new List<PayrollRecord>().AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        _mockPayrollRepository
            .Setup(r => r.AddAsync(It.IsAny<PayrollRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        decimal expectedSSC = (100000 * 5m) / 100m; // 5000 at 5%
        Assert.Equal(expectedSSC, result.SocialSecurityContribution);
    }
}
