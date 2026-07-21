namespace HR.Tests.Unit.Payroll;

/// <summary>
/// Unit tests for GetPayrollReportQuery handler.
/// Tests cover: report generation, filtering, aggregation, and data retrieval.
/// </summary>
public class GetPayrollReportQueryTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<PayrollRecord>> _mockPayrollRepository;
    private readonly Mock<ILogger<GetPayrollReportQueryHandler>> _mockLogger;
    private readonly GetPayrollReportQueryHandler _handler;

    public GetPayrollReportQueryTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPayrollRepository = new Mock<IRepository<PayrollRecord>>();
        _mockLogger = new Mock<ILogger<GetPayrollReportQueryHandler>>();

        _mockUnitOfWork
            .Setup(u => u.GetRepository<PayrollRecord>())
            .Returns(_mockPayrollRepository.Object);

        _handler = new GetPayrollReportQueryHandler(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidYearAndMonth_ReturnsPayrollReport()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Jane Smith", 80000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Bob Johnson", 120000, 2026, 7, "Approved", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalRecords);
        Assert.Equal(2026, result.Year);
        Assert.Equal(7, result.Month);
    }

    [Fact]
    public async Task Handle_WithNoPayrolls_ReturnsEmptyReport()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = new List<PayrollRecord>().AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalRecords);
    }

    [Fact]
    public async Task Handle_FiltersByTenantId()
    {
        // Arrange
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();

        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 7, "Processed", tenantId1),
            CreatePayrollRecord(Guid.NewGuid(), "Jane Smith", 80000, 2026, 7, "Processed", tenantId2),
            CreatePayrollRecord(Guid.NewGuid(), "Bob Johnson", 120000, 2026, 7, "Processed", tenantId1)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId1);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalRecords);
    }

    [Fact]
    public async Task Handle_AggregatesPayrollAmounts()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Jane Smith", 80000, 2026, 7, "Processed", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        decimal expectedTotalGross = payrolls.Sum(p => p.GrossIncome);
        decimal expectedTotalNet = payrolls.Sum(p => p.NetSalary);
        
        Assert.Equal(expectedTotalGross, result.TotalGrossIncome);
        Assert.Equal(expectedTotalNet, result.TotalNetSalary);
    }

    [Fact]
    public async Task Handle_CalculatesAverageNetSalary()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Jane Smith", 80000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Bob Johnson", 120000, 2026, 7, "Processed", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        decimal expectedAverage = payrolls.Average(p => p.NetSalary);
        Assert.Equal(expectedAverage, result.AverageNetSalary);
    }

    [Fact]
    public async Task Handle_SummarizesProcessedRecords()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Jane Smith", 80000, 2026, 7, "Approved", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "Bob Johnson", 120000, 2026, 7, "Draft", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var processedCount = payrolls.Count(p => p.Status == "Processed");
        var approvedCount = payrolls.Count(p => p.Status == "Approved");
        
        Assert.Equal(processedCount, result.ProcessedCount);
        Assert.Equal(approvedCount, result.ApprovedCount);
    }

    [Fact]
    public async Task Handle_WithDifferentMonths_ReturnsCorrectData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 6, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 105000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 110000, 2026, 8, "Processed", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Records); // Only July data
    }

    [Fact]
    public async Task Handle_IncludesPayrollDetailsByEmployee()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();

        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(employee1, "John Doe", 100000, 2026, 7, "Processed", tenantId),
            CreatePayrollRecord(employee2, "Jane Smith", 80000, 2026, 7, "Processed", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Records.Count);
        Assert.True(result.Records.Any(r => r.EmployeeName == "John Doe"));
        Assert.True(result.Records.Any(r => r.EmployeeName == "Jane Smith"));
    }

    [Fact]
    public async Task Handle_SetsReportGenerationTimestamp()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var beforeQuery = DateTime.UtcNow;

        var payrolls = new List<PayrollRecord>
        {
            CreatePayrollRecord(Guid.NewGuid(), "John Doe", 100000, 2026, 7, "Processed", tenantId)
        };

        var query = new GetPayrollReportQuery(2026, 7, tenantId);

        var queryable = payrolls.AsQueryable();
        _mockPayrollRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        var afterQuery = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result.ReportGeneratedDate);
        Assert.True(result.ReportGeneratedDate >= beforeQuery);
        Assert.True(result.ReportGeneratedDate <= afterQuery);
    }

    // Helper method to create test payroll records
    private PayrollRecord CreatePayrollRecord(
        Guid employeeId,
        string employeeName,
        decimal basicSalary,
        int year,
        int month,
        string status,
        Guid tenantId)
    {
        var payroll = PayrollRecord.Create(employeeId, employeeName, basicSalary, year, month, tenantId);
        payroll.HousingAllowance = basicSalary * 0.15m;
        payroll.TransportAllowance = basicSalary * 0.05m;
        payroll.OtherAllowances = 5000;
        payroll.CalculatePayroll(10m, 5m, 500m);

        if (status == "Approved" || status == "Paid")
        {
            payroll.Approve(Guid.NewGuid());
        }

        if (status == "Paid")
        {
            payroll.MarkAsPaid();
        }

        return payroll;
    }
}
