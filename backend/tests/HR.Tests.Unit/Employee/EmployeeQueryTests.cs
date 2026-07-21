namespace HR.Tests.Unit.Employee;

/// <summary>
/// Unit tests for Employee query handlers.
/// Tests cover: getting by ID, list retrieval with pagination, and filtering.
/// </summary>
public class EmployeeQueryTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<HR.Employee.Domain.Employee>> _mockEmployeeRepository;
    private readonly Mock<IRepository<Department>> _mockDepartmentRepository;
    private readonly Mock<ILogger<GetEmployeeByIdQueryHandler>> _mockLogger;

    public EmployeeQueryTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmployeeRepository = new Mock<IRepository<HR.Employee.Domain.Employee>>();
        _mockDepartmentRepository = new Mock<IRepository<Department>>();
        _mockLogger = new Mock<ILogger<GetEmployeeByIdQueryHandler>>();

        _mockUnitOfWork
            .Setup(u => u.GetRepository<HR.Employee.Domain.Employee>())
            .Returns(_mockEmployeeRepository.Object);

        _mockUnitOfWork
            .Setup(u => u.GetRepository<Department>())
            .Returns(_mockDepartmentRepository.Object);
    }

    [Fact]
    public async Task Handle_GetEmployeeById_WithValidId_ReturnsEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var department = Department.Create("Engineering", "Engineering Department", "Building A", tenantId)
        {
            Id = departmentId
        };

        var dateOfBirth = DateTime.Now.AddYears(-30);
        var hireDate = DateTime.Now.AddYears(-5);
        var employee = HR.Employee.Domain.Employee.Create(
            "John", "Doe", "john@example.com", "1234567890", dateOfBirth,
            "Male", "12345678", hireDate, departmentId, "Engineer", "Full-time", 100000m, tenantId)
        {
            Id = employeeId
        };

        var query = new GetEmployeeByIdQuery(employeeId, tenantId);
        var handler = new GetEmployeeByIdQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _mockDepartmentRepository
            .Setup(r => r.GetByIdAsync(departmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task Handle_GetEmployeeById_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var query = new GetEmployeeByIdQuery(employeeId, tenantId);
        var handler = new GetEmployeeByIdQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HR.Employee.Domain.Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GetEmployees_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employees = new List<HR.Employee.Domain.Employee>();
        for (int i = 0; i < 25; i++)
        {
            var dateOfBirth = DateTime.Now.AddYears(-30);
            var hireDate = DateTime.Now.AddYears(-5);
            employees.Add(HR.Employee.Domain.Employee.Create(
                $"Employee{i}", $"Last{i}", $"emp{i}@example.com", "1234567890",
                dateOfBirth, "Male", $"ID{i}", hireDate, departmentId,
                "Engineer", "Full-time", 100000m, tenantId));
        }

        var query = new GetEmployeesQuery(pageNumber: 1, pageSize: 10, tenantId);
        var handler = new GetEmployeesQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        var queryable = employees.AsQueryable();
        _mockEmployeeRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(10, result.Data.Count);
    }

    [Fact]
    public async Task Handle_GetEmployees_SecondPage_ReturnsCorrectData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employees = new List<HR.Employee.Domain.Employee>();
        for (int i = 0; i < 25; i++)
        {
            var dateOfBirth = DateTime.Now.AddYears(-30);
            var hireDate = DateTime.Now.AddYears(-5);
            var emp = HR.Employee.Domain.Employee.Create(
                $"Employee{i}", $"Last{i}", $"emp{i}@example.com", "1234567890",
                dateOfBirth, "Male", $"ID{i}", hireDate, departmentId,
                "Engineer", "Full-time", 100000m, tenantId);
            employees.Add(emp);
        }

        var query = new GetEmployeesQuery(pageNumber: 2, pageSize: 10, tenantId);
        var handler = new GetEmployeesQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        var queryable = employees.AsQueryable();
        _mockEmployeeRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(10, result.Data.Count);
    }

    [Fact]
    public async Task Handle_GetEmployees_FiltersByTenantId()
    {
        // Arrange
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var employees = new List<HR.Employee.Domain.Employee>();
        
        // Add 5 employees for tenant1
        for (int i = 0; i < 5; i++)
        {
            var dateOfBirth = DateTime.Now.AddYears(-30);
            var hireDate = DateTime.Now.AddYears(-5);
            employees.Add(HR.Employee.Domain.Employee.Create(
                $"Tenant1Employee{i}", $"Last{i}", $"t1emp{i}@example.com", "1234567890",
                dateOfBirth, "Male", $"T1ID{i}", hireDate, departmentId,
                "Engineer", "Full-time", 100000m, tenantId1));
        }

        // Add 3 employees for tenant2
        for (int i = 0; i < 3; i++)
        {
            var dateOfBirth = DateTime.Now.AddYears(-30);
            var hireDate = DateTime.Now.AddYears(-5);
            employees.Add(HR.Employee.Domain.Employee.Create(
                $"Tenant2Employee{i}", $"Last{i}", $"t2emp{i}@example.com", "1234567890",
                dateOfBirth, "Male", $"T2ID{i}", hireDate, departmentId,
                "Engineer", "Full-time", 100000m, tenantId2));
        }

        var query = new GetEmployeesQuery(pageNumber: 1, pageSize: 100, tenantId1);
        var handler = new GetEmployeesQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        var queryable = employees.AsQueryable();
        _mockEmployeeRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.All(result.Data, e => Assert.Equal(tenantId1, e.TenantId));
    }

    [Fact]
    public async Task Handle_GetEmployees_WithNoEmployees_ReturnsEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetEmployeesQuery(pageNumber: 1, pageSize: 10, tenantId);
        var handler = new GetEmployeesQueryHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        var queryable = new List<HR.Employee.Domain.Employee>().AsQueryable();
        _mockEmployeeRepository
            .Setup(r => r.GetAsQueryable())
            .Returns(queryable);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void Department_Create_WithValidData_CreatesDepartment()
    {
        // Arrange
        var name = "Engineering";
        var description = "Engineering Department";
        var location = "Building A";
        var tenantId = Guid.NewGuid();

        // Act
        var department = Department.Create(name, description, location, tenantId);

        // Assert
        Assert.NotNull(department);
        Assert.NotEqual(Guid.Empty, department.Id);
        Assert.Equal(name, department.Name);
        Assert.Equal(description, department.Description);
        Assert.Equal(location, department.Location);
        Assert.Equal(tenantId, department.TenantId);
    }
}
