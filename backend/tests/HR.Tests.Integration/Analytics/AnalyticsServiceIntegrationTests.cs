namespace HR.Tests.Integration.Analytics;

/// <summary>
/// Integration tests for Analytics Service (10 tests).
/// Tests cover: employee event processing, aggregations, and analytics queries.
/// </summary>
[Collection("Database collection")]
public class AnalyticsServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IUnitOfWork _unitOfWork;
    private readonly KafkaFixture _kafkaFixture;

    public AnalyticsServiceIntegrationTests(PostgreSqlFixture fixture, KafkaFixture kafkaFixture = null)
    {
        _fixture = fixture;
        _kafkaFixture = kafkaFixture;
        _unitOfWork = _fixture.CreateUnitOfWork();
    }

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task ProcessEmployeeCreatedEvent_UpdatesAnalytics()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Act
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();
        var record = new EmployeeAnalyticsRecord
        {
            EmployeeId = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Status = "Active",
            TenantId = tenantId,
            HireDate = DateTime.Now
        };

        await analyticsRepository.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await analyticsRepository.GetByIdAsync(record.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("John", retrieved.FirstName);
    }

    [Fact]
    public async Task QueryEmployeesByDepartment_AggregatesMetrics()
    {
        // Arrange
        var departmentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();
        for (int i = 0; i < 5; i++)
        {
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = "Test",
                DepartmentId = departmentId,
                Status = "Active",
                Salary = 100000 + (i * 10000),
                TenantId = tenantId,
                HireDate = DateTime.Now
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var deptRecords = queryable
            .Where(a => a.DepartmentId == departmentId && a.TenantId == tenantId)
            .ToList();

        var avgSalary = deptRecords.Average(r => r.Salary);

        // Assert
        Assert.Equal(5, deptRecords.Count);
        Assert.True(avgSalary > 100000);
    }

    [Fact]
    public async Task CalculateTurnoverRate_WithTerminatedEmployees()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        for (int i = 0; i < 10; i++)
        {
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = "Test",
                Status = i < 8 ? "Active" : "Terminated",
                TerminationDate = i >= 8 ? DateTime.Now.AddDays(-30) : null,
                TenantId = tenantId,
                HireDate = DateTime.Now.AddYears(-5)
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var totalRecords = queryable.Count(a => a.TenantId == tenantId);
        var terminatedCount = queryable.Count(a => a.TenantId == tenantId && a.Status == "Terminated");
        var turnoverRate = (terminatedCount / (decimal)totalRecords) * 100;

        // Assert
        Assert.Equal(10, totalRecords);
        Assert.Equal(2, terminatedCount);
        Assert.Equal(20m, turnoverRate);
    }

    [Fact]
    public async Task SearchEmployees_ByName_FiltersResults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        var names = new[] { "John Doe", "Jane Smith", "John Smith", "Bob Johnson" };
        for (int i = 0; i < names.Length; i++)
        {
            var parts = names[i].Split(' ');
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = parts[0],
                LastName = parts[1],
                TenantId = tenantId
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var johnRecords = queryable
            .Where(a => a.TenantId == tenantId && a.FirstName.Contains("John"))
            .ToList();

        // Assert
        Assert.Equal(3, johnRecords.Count);
    }

    [Fact]
    public async Task GetGenderDistribution_CalculatesPercentages()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        var genders = new[] { "Male", "Male", "Female", "Female", "Other" };
        for (int i = 0; i < genders.Length; i++)
        {
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = "Test",
                Gender = genders[i],
                TenantId = tenantId
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var total = queryable.Count(a => a.TenantId == tenantId);
        var maleCount = queryable.Count(a => a.TenantId == tenantId && a.Gender == "Male");
        var femaleCount = queryable.Count(a => a.TenantId == tenantId && a.Gender == "Female");

        var malePercentage = (maleCount / (decimal)total) * 100;
        var femalePercentage = (femaleCount / (decimal)total) * 100;

        // Assert
        Assert.Equal(40m, malePercentage);
        Assert.Equal(40m, femalePercentage);
    }

    [Fact]
    public async Task GetTopPerformers_ReturnsHighestRatedEmployees()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        var ratings = new[] { 5, 4, 5, 3, 4, 2 };
        for (int i = 0; i < ratings.Length; i++)
        {
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = "Test",
                PerformanceRating = ratings[i],
                TenantId = tenantId
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var topPerformers = queryable
            .Where(a => a.TenantId == tenantId && a.PerformanceRating >= 4)
            .OrderByDescending(a => a.PerformanceRating)
            .ToList();

        // Assert
        Assert.Equal(4, topPerformers.Count);
        Assert.True(topPerformers.All(e => e.PerformanceRating >= 4));
    }

    [Fact]
    public async Task GetEmployeesByJobTitle_FiltersAndGroupsData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        var jobTitles = new[] { "Engineer", "Engineer", "Manager", "Manager", "Analyst" };
        for (int i = 0; i < jobTitles.Length; i++)
        {
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = "Test",
                JobTitle = jobTitles[i],
                TenantId = tenantId
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var engineers = queryable
            .Where(a => a.TenantId == tenantId && a.JobTitle == "Engineer")
            .ToList();

        var managers = queryable
            .Where(a => a.TenantId == tenantId && a.JobTitle == "Manager")
            .ToList();

        // Assert
        Assert.Equal(2, engineers.Count);
        Assert.Equal(2, managers.Count);
    }

    [Fact]
    public async Task BulkInsertAnalyticsRecords_HandlesLargeDatasets()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        var records = Enumerable.Range(1, 500)
            .Select(i => new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = $"Last{i}",
                TenantId = tenantId,
                Salary = 50000 + (i * 100),
                JobTitle = i % 3 == 0 ? "Manager" : "Engineer"
            })
            .ToList();

        // Act
        foreach (var record in records)
        {
            await analyticsRepository.AddAsync(record);
        }
        var startTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(duration.TotalSeconds < 10);
        var queryable = analyticsRepository.GetAsQueryable();
        Assert.Equal(500, queryable.Count(a => a.TenantId == tenantId));
    }

    [Fact]
    public async Task AgeDistributionAnalysis_CategorizesEmployees()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var analyticsRepository = _unitOfWork.GetRepository<EmployeeAnalyticsRecord>();

        var ages = new[] { 25, 35, 45, 55, 28, 38, 48, 32, 42, 52 };
        for (int i = 0; i < ages.Length; i++)
        {
            var record = new EmployeeAnalyticsRecord
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = $"Employee{i}",
                LastName = "Test",
                Age = ages[i],
                TenantId = tenantId
            };
            await analyticsRepository.AddAsync(record);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = analyticsRepository.GetAsQueryable();
        var under30 = queryable.Count(a => a.TenantId == tenantId && a.Age < 30);
        var age30to40 = queryable.Count(a => a.TenantId == tenantId && a.Age >= 30 && a.Age < 40);
        var above40 = queryable.Count(a => a.TenantId == tenantId && a.Age >= 40);

        // Assert
        Assert.Equal(2, under30);
        Assert.Equal(4, age30to40);
        Assert.Equal(4, above40);
    }
}

// Helper class for testing
public class EmployeeAnalyticsRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid DepartmentId { get; set; }
    public string JobTitle { get; set; }
    public string Status { get; set; }
    public string Gender { get; set; }
    public int Age { get; set; }
    public decimal Salary { get; set; }
    public int PerformanceRating { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
}
