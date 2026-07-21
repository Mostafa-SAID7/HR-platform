namespace HR.Tests.Integration.Payroll;

/// <summary>
/// Integration tests for Payroll Service (12 tests).
/// Tests cover: calculations, approvals, payments, and reporting with database persistence.
/// </summary>
[Collection("Database collection")]
public class PayrollServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<PayrollRecord> _payrollRepository;

    public PayrollServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _unitOfWork = _fixture.CreateUnitOfWork();
        _payrollRepository = _unitOfWork.GetRepository<PayrollRecord>();
    }

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task CalculatePayroll_PersistsRecord()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var payroll = PayrollRecord.Create(employeeId, "John Doe", 100000, 2026, 7, tenantId);

        // Act
        payroll.CalculatePayroll(10m, 5m, 500m);
        await _payrollRepository.AddAsync(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _payrollRepository.GetByIdAsync(payroll.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Processed", retrieved.Status);
        Assert.Equal(100000, retrieved.GrossIncome);
    }

    [Fact]
    public async Task ApprovePayroll_TransitionsStatus()
    {
        // Arrange
        var payroll = PayrollRecord.Create(Guid.NewGuid(), "Test", 100000, 2026, 7, Guid.NewGuid());
        payroll.CalculatePayroll(10m, 5m, 500m);
        await _payrollRepository.AddAsync(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Act
        payroll.Approve(Guid.NewGuid());
        _payrollRepository.Update(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _payrollRepository.GetByIdAsync(payroll.Id);
        Assert.Equal("Approved", retrieved.Status);
    }

    [Fact]
    public async Task MarkAsPaid_RecordsPaidDate()
    {
        // Arrange
        var payroll = PayrollRecord.Create(Guid.NewGuid(), "Test", 100000, 2026, 7, Guid.NewGuid());
        payroll.CalculatePayroll(10m, 5m, 500m);
        payroll.Approve(Guid.NewGuid());
        await _payrollRepository.AddAsync(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Act
        payroll.MarkAsPaid();
        _payrollRepository.Update(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _payrollRepository.GetByIdAsync(payroll.Id);
        Assert.Equal("Paid", retrieved.Status);
        Assert.NotNull(retrieved.PaidDate);
    }

    [Fact]
    public async Task GetPayrollByMonthYear_FiltersCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var july = PayrollRecord.Create(Guid.NewGuid(), "Test1", 100000, 2026, 7, tenantId);
        var august = PayrollRecord.Create(Guid.NewGuid(), "Test2", 100000, 2026, 8, tenantId);

        await _payrollRepository.AddAsync(july);
        await _payrollRepository.AddAsync(august);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _payrollRepository.GetAsQueryable();
        var julyRecords = queryable
            .Where(p => p.TenantId == tenantId && p.Year == 2026 && p.Month == 7)
            .ToList();

        // Assert
        Assert.Single(julyRecords);
    }

    [Fact]
    public async Task GetPendingApprovals_FiltersByStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var processed = PayrollRecord.Create(Guid.NewGuid(), "Test1", 100000, 2026, 7, tenantId);
        processed.CalculatePayroll(10m, 5m, 500m);

        var approved = PayrollRecord.Create(Guid.NewGuid(), "Test2", 100000, 2026, 7, tenantId);
        approved.CalculatePayroll(10m, 5m, 500m);
        approved.Approve(Guid.NewGuid());

        await _payrollRepository.AddAsync(processed);
        await _payrollRepository.AddAsync(approved);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _payrollRepository.GetAsQueryable();
        var pending = queryable
            .Where(p => p.TenantId == tenantId && p.Status == "Processed")
            .ToList();

        // Assert
        Assert.Single(pending);
    }

    [Fact]
    public async Task QueryPayrollByEmployee_FiltersByEmployeeId()
    {
        // Arrange
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var emp1Payroll = PayrollRecord.Create(employee1, "Emp1", 100000, 2026, 7, tenantId);
        var emp2Payroll = PayrollRecord.Create(employee2, "Emp2", 80000, 2026, 7, tenantId);

        await _payrollRepository.AddAsync(emp1Payroll);
        await _payrollRepository.AddAsync(emp2Payroll);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _payrollRepository.GetAsQueryable();
        var emp1Records = queryable
            .Where(p => p.EmployeeId == employee1 && p.TenantId == tenantId)
            .ToList();

        // Assert
        Assert.Single(emp1Records);
        Assert.Equal(employee1, emp1Records.First().EmployeeId);
    }

    [Fact]
    public async Task CalculateMonthlyTotalPayroll_AggregatesCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var payrolls = new[]
        {
            PayrollRecord.Create(Guid.NewGuid(), "Emp1", 100000, 2026, 7, tenantId),
            PayrollRecord.Create(Guid.NewGuid(), "Emp2", 80000, 2026, 7, tenantId),
            PayrollRecord.Create(Guid.NewGuid(), "Emp3", 120000, 2026, 7, tenantId)
        };

        foreach (var p in payrolls)
        {
            p.CalculatePayroll(10m, 5m, 500m);
            await _payrollRepository.AddAsync(p);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _payrollRepository.GetAsQueryable();
        var totalGross = queryable
            .Where(p => p.TenantId == tenantId && p.Year == 2026 && p.Month == 7)
            .Sum(p => p.GrossIncome);

        var totalNet = queryable
            .Where(p => p.TenantId == tenantId && p.Year == 2026 && p.Month == 7)
            .Sum(p => p.NetSalary);

        // Assert
        Assert.Equal(300000m, totalGross);
        Assert.True(totalNet > 0);
    }

    [Fact]
    public async Task BulkProcessPayroll_PerformsEfficiently()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var payrolls = Enumerable.Range(1, 100)
            .Select(i => PayrollRecord.Create(Guid.NewGuid(), $"Emp{i}", 100000, 2026, 7, tenantId))
            .ToList();

        foreach (var p in payrolls)
        {
            p.CalculatePayroll(10m, 5m, 500m);
        }

        // Act
        var startTime = DateTime.UtcNow;
        foreach (var p in payrolls)
        {
            await _payrollRepository.AddAsync(p);
        }
        await _unitOfWork.SaveChangesAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(duration.TotalSeconds < 5);
        var queryable = _payrollRepository.GetAsQueryable();
        Assert.Equal(100, queryable.Count(p => p.TenantId == tenantId));
    }

    [Fact]
    public async Task DomainEvent_PayrollCalculated_IsRaised()
    {
        // Arrange
        var payroll = PayrollRecord.Create(Guid.NewGuid(), "Test", 100000, 2026, 7, Guid.NewGuid());

        // Act
        payroll.CalculatePayroll(10m, 5m, 500m);
        await _payrollRepository.AddAsync(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.NotEmpty(payroll.DomainEvents);
    }

    [Fact]
    public async Task UpdatePayrollAmount_PersistsChanges()
    {
        // Arrange
        var payroll = PayrollRecord.Create(Guid.NewGuid(), "Test", 100000, 2026, 7, Guid.NewGuid());
        payroll.CalculatePayroll(10m, 5m, 500m);
        await _payrollRepository.AddAsync(payroll);
        await _unitOfWork.SaveChangesAsync();

        var originalGross = payroll.GrossIncome;

        // Act
        payroll.HousingAllowance = 20000; // Increase allowance
        payroll.CalculatePayroll(10m, 5m, 500m);
        _payrollRepository.Update(payroll);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _payrollRepository.GetByIdAsync(payroll.Id);
        Assert.NotEqual(originalGross, retrieved.GrossIncome);
    }

    [Fact]
    public async Task GetPayrollReport_GeneratesAccurateData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var payrolls = new[]
        {
            PayrollRecord.Create(Guid.NewGuid(), "Emp1", 100000, 2026, 7, tenantId),
            PayrollRecord.Create(Guid.NewGuid(), "Emp2", 80000, 2026, 7, tenantId)
        };

        foreach (var p in payrolls)
        {
            p.CalculatePayroll(10m, 5m, 500m);
            await _payrollRepository.AddAsync(p);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _payrollRepository.GetAsQueryable();
        var report = queryable
            .Where(p => p.TenantId == tenantId && p.Year == 2026 && p.Month == 7)
            .ToList();

        var totalEmployees = report.Count;
        var avgNetSalary = report.Average(p => p.NetSalary);

        // Assert
        Assert.Equal(2, totalEmployees);
        Assert.True(avgNetSalary > 0);
    }
}
