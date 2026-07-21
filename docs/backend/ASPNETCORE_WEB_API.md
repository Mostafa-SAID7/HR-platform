# ASP.NET Core Web API
## C# Backend for HR Analytics Platform

Complete guide to ASP.NET Core 9 REST/gRPC APIs serving Angular frontend and mobile apps.

---

## Architecture Overview

### Backend Stack

```
┌─────────────────────────────────┐
│ Clients                         │
│ • Angular Web (PWA)             │
│ • Flutter Mobile                │
│ • Electron Desktop              │
└────────────┬────────────────────┘
             │ HTTP/REST + gRPC
             ▼
┌─────────────────────────────────┐
│ API Gateway (YARP)              │
│ • Request routing               │
│ • Rate limiting                 │
│ • Authentication                │
└────────────┬────────────────────┘
             │
    ┌────────┴────────┐
    ▼                 ▼
┌─────────────┐  ┌─────────────┐
│ Microserv.  │  │ Monolithic  │
│ (gRPC)      │  │ (REST)      │
└─────────────┘  └─────────────┘
    │                 │
    └────────┬────────┘
             ▼
┌─────────────────────────────────┐
│ Data Layer                      │
│ • EF Core (ORM)                 │
│ • PostgreSQL                    │
│ • Redis Cache                   │
└─────────────────────────────────┘
```

---

## Project Structure

```
HRAnalytics/
├── HRAnalytics.API/
│   ├── Controllers/
│   │   ├── EmployeesController.cs
│   │   ├── PayrollController.cs
│   │   ├── AttendanceController.cs
│   │   ├── AnalyticsController.cs
│   │   └── HealthController.cs
│   ├── Services/
│   │   ├── EmployeeService.cs
│   │   ├── PayrollService.cs
│   │   └── AuthService.cs
│   ├── Middleware/
│   │   ├── ExceptionMiddleware.cs
│   │   ├── AuthenticationMiddleware.cs
│   │   └── LoggingMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── HRAnalytics.Domain/
│   ├── Entities/
│   │   ├── Employee.cs
│   │   ├── Payroll.cs
│   │   └── Attendance.cs
│   ├── DTOs/
│   │   ├── CreateEmployeeDto.cs
│   │   └── UpdateEmployeeDto.cs
│   └── Interfaces/
│       └── IRepository.cs
│
├── HRAnalytics.Data/
│   ├── Context/
│   │   └── HRContext.cs
│   ├── Repositories/
│   │   └── Repository.cs
│   └── Migrations/
│
└── HRAnalytics.Tests/
    ├── Unit/
    └── Integration/
```

---

## Program.cs Setup

```csharp
using HRAnalytics.API.Middleware;
using HRAnalytics.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://hranalytics.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<HRContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.hranalytics.com";
        options.Audience = "hr-api";
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// Middleware
app.UseCors("AllowAngular");
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<EmployeeGrpcService>();

app.Run();
```

---

## REST API Controllers

### Employee Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRAnalytics.Domain.DTOs;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        IEmployeeService employeeService,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees for company
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), 200)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null)
    {
        var companyId = User.FindFirst("company_id")?.Value;
        
        var result = await _employeeService.GetEmployeesAsync(
            int.Parse(companyId!),
            page,
            pageSize,
            department,
            status
        );

        return Ok(result);
    }

    /// <summary>
    /// Get single employee
    /// </summary>
    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEmployee(string employeeId)
    {
        var companyId = User.FindFirst("company_id")?.Value;
        
        var employee = await _employeeService.GetEmployeeAsync(
            employeeId,
            int.Parse(companyId!)
        );

        if (employee == null)
            return NotFound(new { error = "Employee not found" });

        return Ok(employee);
    }

    /// <summary>
    /// Create new employee
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,HRManager")]
    [ProducesResponseType(typeof(EmployeeDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var companyId = User.FindFirst("company_id")?.Value;
        var userId = User.FindFirst("sub")?.Value;

        var employee = await _employeeService.CreateEmployeeAsync(
            request,
            int.Parse(companyId!),
            userId!
        );

        return CreatedAtAction(
            nameof(GetEmployee),
            new { employeeId = employee.Id },
            employee
        );
    }

    /// <summary>
    /// Update employee
    /// </summary>
    [HttpPut("{employeeId}")]
    [Authorize(Roles = "Admin,HRManager")]
    [ProducesResponseType(typeof(EmployeeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateEmployee(
        string employeeId,
        [FromBody] UpdateEmployeeDto request)
    {
        var companyId = User.FindFirst("company_id")?.Value;
        var userId = User.FindFirst("sub")?.Value;

        var employee = await _employeeService.UpdateEmployeeAsync(
            employeeId,
            request,
            int.Parse(companyId!),
            userId!
        );

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    /// <summary>
    /// Delete employee
    /// </summary>
    [HttpDelete("{employeeId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteEmployee(string employeeId)
    {
        var companyId = User.FindFirst("company_id")?.Value;
        var userId = User.FindFirst("sub")?.Value;

        var success = await _employeeService.DeleteEmployeeAsync(
            employeeId,
            int.Parse(companyId!),
            userId!
        );

        if (!success)
            return NotFound();

        return NoContent();
    }
}
```

---

## Services Layer

```csharp
public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
        int companyId,
        int page,
        int pageSize,
        string? department = null,
        string? status = null);
    
    Task<EmployeeDto?> GetEmployeeAsync(string employeeId, int companyId);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto request, int companyId, string userId);
    Task<EmployeeDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeDto request, int companyId, string userId);
    Task<bool> DeleteEmployeeAsync(string employeeId, int companyId, string userId);
}

public class EmployeeService : IEmployeeService
{
    private readonly IRepository<Employee> _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<EmployeeService> _logger;
    private const string CacheKeyPrefix = "employee:";

    public EmployeeService(
        IRepository<Employee> repository,
        IDistributedCache cache,
        ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
        int companyId,
        int page,
        int pageSize,
        string? department = null,
        string? status = null)
    {
        var query = _repository.Query()
            .Where(e => e.CompanyId == companyId);

        if (!string.IsNullOrEmpty(department))
            query = query.Where(e => e.Department == department);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);

        var total = await query.CountAsync();
        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<EmployeeDto>
        {
            Items = employees.Select(e => MapToDto(e)).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<EmployeeDto?> GetEmployeeAsync(string employeeId, int companyId)
    {
        var cacheKey = $"{CacheKeyPrefix}{employeeId}";
        
        // Try cache first
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return JsonSerializer.Deserialize<EmployeeDto>(cached);

        // Database
        var employee = await _repository.Query()
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.CompanyId == companyId);

        if (employee == null)
            return null;

        var dto = MapToDto(employee);

        // Cache for 5 minutes
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }
        );

        return dto;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(
        CreateEmployeeDto request,
        int companyId,
        string userId)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Department = request.Department,
            JobTitle = request.JobTitle,
            HireDate = request.HireDate,
            BaseSalary = request.BaseSalary,
            Status = "ACTIVE",
            CompanyId = companyId,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();

        _logger.LogInformation($"Employee {employee.Id} created by {userId}");

        return MapToDto(employee);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Department = employee.Department,
            JobTitle = employee.JobTitle,
            HireDate = employee.HireDate,
            BaseSalary = employee.BaseSalary,
            Status = employee.Status,
            CreatedDate = employee.CreatedDate
        };
    }
}
```

---

## Data Models

```csharp
// Domain Entity
public class Employee
{
    public string Id { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Department { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public int CompanyId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
}

// DTO (Data Transfer Object)
public class EmployeeDto
{
    public string Id { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Department { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}

// Create Request
public class CreateEmployeeDto
{
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Department { get; set; } = null!;
    
    [Required]
    public string JobTitle { get; set; } = null!;
    
    [Required]
    public DateTime HireDate { get; set; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
}

// Paged Result
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}
```

---

## Entity Framework Setup

```csharp
public class HRContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Payroll> Payroll { get; set; }
    public DbSet<Attendance> Attendance { get; set; }

    public HRContext(DbContextOptions<HRContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.Status);
        });

        // Row-level security (multi-tenant)
        modelBuilder.Entity<Employee>()
            .HasQueryFilter(e => e.CompanyId == EF.Property<int>("CompanyId"));

        base.OnModelCreating(modelBuilder);
    }
}
```

---

## Middleware

```csharp
// Exception Handler
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new { error = exception.Message };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## Testing

```csharp
[TestFixture]
public class EmployeeServiceTests
{
    private Mock<IRepository<Employee>> _mockRepository;
    private Mock<IDistributedCache> _mockCache;
    private EmployeeService _service;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IRepository<Employee>>();
        _mockCache = new Mock<IDistributedCache>();
        _service = new EmployeeService(_mockRepository.Object, _mockCache.Object, new NullLogger<EmployeeService>());
    }

    [Test]
    public async Task GetEmployee_WithValidId_ReturnsEmployee()
    {
        // Arrange
        var employee = new Employee { Id = "1", FirstName = "John", LastName = "Doe" };
        _mockCache.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);

        _mockRepository.Setup(r => r.Query())
            .Returns(new[] { employee }.AsQueryable());

        // Act
        var result = await _service.GetEmployeeAsync("1", 1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("John", result.FirstName);
    }
}
```

---

**Last Updated:** July 2026
**Status:** ASP.NET Core API Complete
