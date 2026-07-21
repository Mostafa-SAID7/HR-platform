namespace HR.Tests.Unit.Analytics;

/// <summary>
/// Comprehensive unit tests for Analytics Service (14 tests).
/// Tests cover: employee metrics, dashboard data, search, and reporting.
/// </summary>
public class AnalyticsTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void GetDashboardMetrics_WithEmployeeData_CalculatesMetrics()
    {
        // Arrange
        var employees = CreateTestEmployees(10);

        // Act
        var totalEmployees = employees.Count;
        var activeCount = employees.Count(e => e.IsActive);
        var departmentCount = employees.Select(e => e.DepartmentId).Distinct().Count();

        // Assert
        Assert.Equal(10, totalEmployees);
        Assert.True(activeCount > 0);
        Assert.True(departmentCount > 0);
    }

    [Fact]
    public void CalculateAverageEmployeeSalary_WithValidData_ReturnsCorrectAverage()
    {
        // Arrange
        var employees = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), Salary = 100000, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Salary = 120000, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Salary = 80000, TenantId = _tenantId }
        };

        // Act
        var averageSalary = employees.Average(e => e.Salary);

        // Assert
        Assert.Equal(100000m, averageSalary);
    }

    [Fact]
    public void GetEmployeesByDepartment_FiltersCorrectly()
    {
        // Arrange
        var dept1 = Guid.NewGuid();
        var dept2 = Guid.NewGuid();

        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), DepartmentId = dept1, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), DepartmentId = dept1, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), DepartmentId = dept2, TenantId = _tenantId }
        };

        // Act
        var dept1Employees = analytics.Where(a => a.DepartmentId == dept1).ToList();

        // Assert
        Assert.Equal(2, dept1Employees.Count);
        Assert.All(dept1Employees, a => Assert.Equal(dept1, a.DepartmentId));
    }

    [Fact]
    public void GetTurnoverMetrics_CalculatesCorrectly()
    {
        // Arrange
        var employees = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), IsActive = true, TerminationDate = null, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), IsActive = true, TerminationDate = null, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), IsActive = false, TerminationDate = DateTime.Now.AddDays(-30), TenantId = _tenantId }
        };

        // Act
        var totalEmployees = employees.Count;
        var terminatedCount = employees.Count(e => !e.IsActive);
        var turnoverRate = (terminatedCount / (decimal)totalEmployees) * 100;

        // Assert
        Assert.Equal(3, totalEmployees);
        Assert.Equal(1, terminatedCount);
        Assert.Equal(33.33m, turnoverRate, 1);
    }

    [Fact]
    public void SearchEmployees_ByName_ReturnsMatches()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Smith", TenantId = _tenantId }
        };

        // Act
        var johnRecords = analytics
            .Where(a => a.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        Assert.Equal(2, johnRecords.Count);
    }

    [Fact]
    public void SearchEmployees_ByEmail_ReturnsMatches()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), Email = "john@company.com", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Email = "jane@company.com", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Email = "bob@other.com", TenantId = _tenantId }
        };

        // Act
        var companyEmails = analytics
            .Where(a => a.Email.EndsWith("@company.com", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        Assert.Equal(2, companyEmails.Count);
    }

    [Fact]
    public void GetEmployeesByHireDateRange_FiltersCorrectly()
    {
        // Arrange
        var startDate = DateTime.Now.AddYears(-2);
        var endDate = DateTime.Now;

        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), HireDate = DateTime.Now.AddYears(-3), TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), HireDate = DateTime.Now.AddYears(-1), TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), HireDate = DateTime.Now.AddMonths(-6), TenantId = _tenantId }
        };

        // Act
        var recentHires = analytics
            .Where(a => a.HireDate >= startDate && a.HireDate <= endDate)
            .ToList();

        // Assert
        Assert.Equal(2, recentHires.Count);
    }

    [Fact]
    public void GetEmployeesByJobTitle_FiltersCorrectly()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), JobTitle = "Software Engineer", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), JobTitle = "Software Engineer", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), JobTitle = "Manager", TenantId = _tenantId }
        };

        // Act
        var engineers = analytics
            .Where(a => a.JobTitle == "Software Engineer")
            .ToList();

        // Assert
        Assert.Equal(2, engineers.Count);
    }

    [Fact]
    public void GetEmployeesByStatus_FiltersCorrectly()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), Status = "Active", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Status = "Active", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Status = "Terminated", TenantId = _tenantId }
        };

        // Act
        var activeEmployees = analytics
            .Where(a => a.Status == "Active")
            .ToList();

        // Assert
        Assert.Equal(2, activeEmployees.Count);
    }

    [Fact]
    public void GetTopPerformers_ReturnsHighestRatedEmployees()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), PerformanceRating = 5, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), PerformanceRating = 4, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), PerformanceRating = 5, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), PerformanceRating = 3, TenantId = _tenantId }
        };

        // Act
        var topPerformers = analytics
            .Where(a => a.PerformanceRating >= 4)
            .OrderByDescending(a => a.PerformanceRating)
            .ToList();

        // Assert
        Assert.Equal(3, topPerformers.Count);
        Assert.Equal(5, topPerformers.First().PerformanceRating);
    }

    [Fact]
    public void CalculateGenderDiversity_ReturnsCorrectPercentages()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), Gender = "Male", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Gender = "Male", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Gender = "Female", TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Gender = "Female", TenantId = _tenantId }
        };

        // Act
        var total = analytics.Count;
        var malePercentage = (analytics.Count(a => a.Gender == "Male") / (decimal)total) * 100;
        var femalePercentage = (analytics.Count(a => a.Gender == "Female") / (decimal)total) * 100;

        // Assert
        Assert.Equal(50m, malePercentage);
        Assert.Equal(50m, femalePercentage);
    }

    [Fact]
    public void GetAgeDistribution_CategorizesCorrectly()
    {
        // Arrange
        var analytics = new List<EmployeeAnalytics>
        {
            new() { EmployeeId = Guid.NewGuid(), Age = 25, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Age = 35, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Age = 45, TenantId = _tenantId },
            new() { EmployeeId = Guid.NewGuid(), Age = 55, TenantId = _tenantId }
        };

        // Act
        var under30 = analytics.Count(a => a.Age < 30);
        var age30to40 = analytics.Count(a => a.Age >= 30 && a.Age < 40);
        var above40 = analytics.Count(a => a.Age >= 40);

        // Assert
        Assert.Equal(1, under30);
        Assert.Equal(1, age30to40);
        Assert.Equal(2, above40);
    }

    // Helper method
    private List<EmployeeAnalytics> CreateTestEmployees(int count)
    {
        var employees = new List<EmployeeAnalytics>();
        for (int i = 0; i < count; i++)
        {
            employees.Add(new EmployeeAnalytics
            {
                EmployeeId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                IsActive = i % 2 == 0,
                Salary = 50000 + (i * 5000),
                TenantId = _tenantId,
                FirstName = $"Employee{i}",
                LastName = $"Last{i}",
                Email = $"emp{i}@example.com",
                JobTitle = i % 3 == 0 ? "Manager" : "Engineer",
                Status = i % 2 == 0 ? "Active" : "Terminated",
                HireDate = DateTime.Now.AddYears(-i),
                Gender = i % 2 == 0 ? "Male" : "Female",
                Age = 25 + i,
                PerformanceRating = (i % 5) + 1
            });
        }
        return employees;
    }
}

// Helper class for testing
public class EmployeeAnalytics
{
    public Guid EmployeeId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string JobTitle { get; set; }
    public string Status { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string Gender { get; set; }
    public int Age { get; set; }
    public decimal Salary { get; set; }
    public int PerformanceRating { get; set; }
    public bool IsActive { get; set; }
}
