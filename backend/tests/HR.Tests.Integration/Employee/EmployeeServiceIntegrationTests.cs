namespace HR.Tests.Integration.Employee;

/// <summary>
/// Integration tests for Employee Service with real database context.
/// Tests cover: CRUD operations, repository interactions, and domain events.
/// </summary>
[Collection("Database collection")]
public class EmployeeServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<HR.Employee.Domain.Employee> _employeeRepository;
    private readonly IRepository<Department> _departmentRepository;

    public EmployeeServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _unitOfWork = _fixture.CreateUnitOfWork();
        _employeeRepository = _unitOfWork.GetRepository<HR.Employee.Domain.Employee>();
        _departmentRepository = _unitOfWork.GetRepository<Department>();
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task CreateEmployee_WithValidData_PersistsToDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var department = Department.Create("Engineering", "Engineering Department", "Building A", tenantId);
        await _departmentRepository.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);

        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, department.Id, "Engineer", "Full-time", 100000m, tenantId);

        // Act
        await _employeeRepository.AddAsync(employee);
        var saved = await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.True(saved);
        var retrieved = await _employeeRepository.GetByIdAsync(employee.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("John", retrieved.FirstName);
        Assert.Equal("Doe", retrieved.LastName);
        Assert.Equal("john@example.com", retrieved.Email);
    }

    [Fact]
    public async Task UpdateEmployee_WithValidData_UpdatesInDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);

        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, departmentId, "Engineer", "Full-time", 100000m, tenantId);

        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        // Act
        employee.Update("Jane", "Smith", "9876543210", "Senior Engineer", departmentId, 120000m);
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _employeeRepository.GetByIdAsync(employee.Id);
        Assert.Equal("Jane", retrieved.FirstName);
        Assert.Equal("Smith", retrieved.LastName);
        Assert.Equal(120000m, retrieved.Salary);
    }

    [Fact]
    public async Task TerminateEmployee_WithValidDate_UpdatesStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);

        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, departmentId, "Engineer", "Full-time", 100000m, tenantId);

        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        // Act
        employee.Terminate(DateTime.Now);
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _employeeRepository.GetByIdAsync(employee.Id);
        Assert.Equal("Terminated", retrieved.Status);
        Assert.False(retrieved.IsActive);
        Assert.NotNull(retrieved.TerminationDate);
    }

    [Fact]
    public async Task AddSkill_ToEmployee_PersistsSkill()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);

        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, departmentId, "Engineer", "Full-time", 100000m, tenantId);

        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        // Act
        employee.AddSkill("C#", 5);
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _employeeRepository.GetByIdAsync(employee.Id);
        Assert.Single(retrieved.Skills);
        Assert.Equal("C#", retrieved.Skills.First().SkillName);
    }

    [Fact]
    public async Task GetEmployeesByDepartment_ReturnsCorrectRecords()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dept1 = Guid.NewGuid();
        var dept2 = Guid.NewGuid();

        var employees = new[]
        {
            CreateEmployee(Guid.NewGuid(), "Employee1", dept1, tenantId),
            CreateEmployee(Guid.NewGuid(), "Employee2", dept1, tenantId),
            CreateEmployee(Guid.NewGuid(), "Employee3", dept2, tenantId)
        };

        foreach (var emp in employees)
        {
            await _employeeRepository.AddAsync(emp);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _employeeRepository.GetAsQueryable();
        var dept1Employees = queryable.Where(e => e.DepartmentId == dept1 && e.TenantId == tenantId).ToList();

        // Assert
        Assert.Equal(2, dept1Employees.Count);
        Assert.All(dept1Employees, e => Assert.Equal(dept1, e.DepartmentId));
    }

    [Fact]
    public async Task QueryEmployees_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employees = Enumerable.Range(1, 25)
            .Select(i => CreateEmployee(Guid.NewGuid(), $"Employee{i}", departmentId, tenantId))
            .ToList();

        foreach (var emp in employees)
        {
            await _employeeRepository.AddAsync(emp);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _employeeRepository.GetAsQueryable();
        var page1 = queryable
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.FirstName)
            .Skip(0)
            .Take(10)
            .ToList();

        var page2 = queryable
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.FirstName)
            .Skip(10)
            .Take(10)
            .ToList();

        // Assert
        Assert.Equal(10, page1.Count);
        Assert.Equal(10, page2.Count);
        Assert.NotEqual(page1.First().Id, page2.First().Id);
    }

    [Fact]
    public async Task DomainEvent_EmployeeCreated_IsRaised()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employee = CreateEmployee(Guid.NewGuid(), "John", departmentId, tenantId);

        // Act
        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        Assert.NotEmpty(employee.DomainEvents);
        var createdEvent = employee.DomainEvents.OfType<EmployeeCreatedEvent>().FirstOrDefault();
        Assert.NotNull(createdEvent);
    }

    [Fact]
    public async Task DeleteEmployee_RemovesFromDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employee = CreateEmployee(Guid.NewGuid(), "John", departmentId, tenantId);
        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        var employeeId = employee.Id;

        // Act
        _employeeRepository.Delete(employee);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _employeeRepository.GetByIdAsync(employeeId);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetActiveEmployees_FiltersByStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var activeEmployee = CreateEmployee(Guid.NewGuid(), "Active Employee", departmentId, tenantId);
        var terminatedEmployee = CreateEmployee(Guid.NewGuid(), "Terminated Employee", departmentId, tenantId);
        terminatedEmployee.Terminate(DateTime.Now);

        await _employeeRepository.AddAsync(activeEmployee);
        await _employeeRepository.AddAsync(terminatedEmployee);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _employeeRepository.GetAsQueryable();
        var activeEmployees = queryable
            .Where(e => e.TenantId == tenantId && e.IsActive)
            .ToList();

        // Assert
        Assert.Single(activeEmployees);
        Assert.Equal("Active Employee", activeEmployees.First().FirstName);
    }

    [Fact]
    public async Task BulkInsertEmployees_WithMultipleRecords_PerformsEfficiently()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employees = Enumerable.Range(1, 100)
            .Select(i => CreateEmployee(Guid.NewGuid(), $"Bulk{i}", departmentId, tenantId))
            .ToList();

        // Act
        foreach (var emp in employees)
        {
            await _employeeRepository.AddAsync(emp);
        }
        var startTime = DateTime.UtcNow;
        var saved = await _unitOfWork.SaveChangesAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(saved);
        Assert.True(duration.TotalSeconds < 5); // Should complete in less than 5 seconds

        var queryable = _employeeRepository.GetAsQueryable();
        var count = queryable.Count(e => e.TenantId == tenantId);
        Assert.Equal(100, count);
    }

    // Helper method
    private HR.Employee.Domain.Employee CreateEmployee(
        Guid employeeId, string firstName, Guid departmentId, Guid tenantId)
    {
        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);

        return HR.Employee.Domain.Employee.Create(
            firstName, "TestLast", $"{firstName}@example.com", "1234567890",
            dateOfBirth, "Male", $"ID{employeeId}", hireDate, departmentId,
            "Engineer", "Full-time", 100000m, tenantId)
        {
            Id = employeeId
        };
    }
}
