# C# ASP.NET Core Web API
## Backend REST/gRPC API for HR Analytics Platform

Complete guide to building and deploying ASP.NET Core 9 backend serving Angular frontend and mobile apps.

---

## Architecture Overview

```
┌─────────────────┐
│  Client Apps    │
├─────────────────┤
│ • Angular Web   │
│ • Flutter Mobile│
│ • Electron App  │
└────────┬────────┘
         │ HTTP/gRPC
         ▼
┌─────────────────────────────────────┐
│  ASP.NET Core 9 API Gateway         │
│  • Request routing                  │
│  • Authentication (JWT)             │
│  • Rate limiting                    │
│  • Logging                          │
└────────┬────────────────────────────┘
         │
    ┌────┴────┬──────────┬──────────┐
    ▼         ▼          ▼          ▼
┌────────┐┌────────┐┌────────┐┌────────┐
│Employee││Payroll ││Attend. ││Analyt. │
│Service ││Service ││Service ││Service │
└────────┘└────────┘└────────┘└────────┘
    │         │          │          │
    └─────────┴──────────┴──────────┘
             │
        PostgreSQL
         + Snowflake
```

---

## Project Structure

```
HRAnalytics/
├── HRAnalytics.sln
├── src/
│   ├── HRAnalytics.API/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Controllers/
│   │   │   ├── EmployeesController.cs
│   │   │   ├── PayrollController.cs
│   │   │   ├── AttendanceController.cs
│   │   │   └── AnalyticsController.cs
│   │   ├── Services/
│   │   │   ├── EmployeeService.cs
│   │   │   ├── PayrollService.cs
│   │   │   └── AuthService.cs
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── JwtMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   └── Protos/
│   │       ├── employee.proto
│   │       └── payroll.proto
│   │
│   ├── HRAnalytics.Domain/
│   │   ├── Models/
│   │   │   ├── Employee.cs
│   │   │   ├── Payroll.cs
│   │   │   └── Attendance.cs
│   │   └── Interfaces/
│   │       ├── IEmployeeRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── HRAnalytics.Infrastructure/
│   │   ├── Data/
│   │   │   ├── HRContext.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── EmployeeRepository.cs
│   │   │   └── GenericRepository.cs
│   │   └── Services/
│   │       ├── JwtService.cs
│   │       └── CacheService.cs
│   │
│   └── HRAnalytics.Application/
│       ├── DTOs/
│       │   ├── CreateEmployeeDto.cs
│       │   └── EmployeeResponseDto.cs
│       ├── UseCases/
│       │   └── CreateEmployeeUseCase.cs
│       └── Mappers/
│           └── AutoMapperProfile.cs
│
├── tests/
│   ├── HRAnalytics.Tests.Unit/
│   └── HRAnalytics.Tests.Integration/
│
└── Dockerfile
```

---

## Project Setup

### 1. Create Solution

```bash
dotnet new sln -n HRAnalytics
dotnet new webapi -n HRAnalytics.API
dotnet new classlib -n HRAnalytics.Domain
dotnet new classlib -n HRAnalytics.Infrastructure
dotnet new classlib -n HRAnalytics.Application

# Add to solution
dotnet sln add HRAnalytics.API/HRAnalytics.API.csproj
dotnet sln add HRAnalytics.Domain/HRAnalytics.Domain.csproj
dotnet sln add HRAnalytics.Infrastructure/HRAnalytics.Infrastructure.csproj
dotnet sln add HRAnalytics.Application/HRAnalytics.Application.csproj
```

### 2. NuGet Dependencies

```bash
# Core
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Data Access
dotnet add package Dapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# gRPC
dotnet add package Grpc.AspNetCore
dotnet add package Grpc.Tools

# Caching
dotnet add package StackExchange.Redis
dotnet add package LazyCache

# Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File

# Validation
dotnet add package FluentValidation.AspNetCore

# Testing
dotnet add package xunit
dotnet add package Moq
```

---

## Core Implementation

### 1. Program.cs Setup

```csharp
using HRAnalytics.Infrastructure.Data;
using HRAnalytics.Infrastructure.Repositories;
using HRAnalytics.Application.Mappers;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// CORS
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

// Database
builder.Services.AddDbContext<HRContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.hranalytics.com";
        options.Audience = "hr-api";
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));
    options.AddPolicy("HRManager", policy =>
        policy.RequireRole("admin", "hr_manager"));
});

// Services
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// gRPC
builder.Services.AddGrpc();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<EmployeeGrpcService>();

app.Run();
```

### 2. Entity Models

```csharp
// Domain/Models/Employee.cs
public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int CompanyId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Department { get; set; }
    public string JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } // Active, Inactive, Terminated
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public virtual Company Company { get; set; }
    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
```

### 3. Repository Pattern

```csharp
// Infrastructure/Repositories/EmployeeRepository.cs
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<List<Employee>> GetByCompanyAsync(int companyId);
    Task<Employee> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
}

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(HRContext context) : base(context)
    {
    }

    public async Task<List<Employee>> GetByCompanyAsync(int companyId)
    {
        return await dbSet
            .Where(e => e.CompanyId == companyId && e.Status == "Active")
            .OrderBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<Employee> GetByEmailAsync(string email)
    {
        return await dbSet.FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await dbSet.AnyAsync(e => e.Email == email);
    }
}
```

### 4. API Controller

```csharp
// Controllers/EmployeesController.cs
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeesController> _logger;
    private readonly IMemoryCache _cache;

    public EmployeesController(
        IEmployeeService employeeService,
        IMapper mapper,
        ILogger<EmployeesController> logger,
        IMemoryCache cache)
    {
        _employeeService = employeeService;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Get all employees for company
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting employees for company {CompanyId}", companyId);

        // Try cache first
        var cacheKey = $"employees:company:{companyId}:page:{page}";
        if (_cache.TryGetValue(cacheKey, out List<EmployeeResponseDto> cachedEmployees))
        {
            _logger.LogInformation("Returning cached employees");
            return Ok(cachedEmployees);
        }

        // Get from database
        var employees = await _employeeService.GetEmployeesByCompanyAsync(companyId, page, pageSize);
        var response = _mapper.Map<List<EmployeeResponseDto>>(employees);

        // Cache for 1 hour
        _cache.Set(cacheKey, response, TimeSpan.FromHours(1));

        return Ok(response);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(string id)
    {
        var employee = await _employeeService.GetEmployeeAsync(id);
        if (employee == null)
            return NotFound();

        var response = _mapper.Map<EmployeeResponseDto>(employee);
        return Ok(response);
    }

    /// <summary>
    /// Create new employee
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "HRManager")]
    [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeDto request)
    {
        _logger.LogInformation("Creating employee {Email}", request.Email);

        try
        {
            var employee = await _employeeService.CreateEmployeeAsync(request);
            var response = _mapper.Map<EmployeeResponseDto>(employee);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, response);
        }
        catch (DuplicateException ex)
        {
            _logger.LogWarning("Duplicate email: {Email}", request.Email);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update employee
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "HRManager")]
    public async Task<IActionResult> UpdateEmployee(
        string id,
        [FromBody] UpdateEmployeeDto request)
    {
        var employee = await _employeeService.UpdateEmployeeAsync(id, request);
        var response = _mapper.Map<EmployeeResponseDto>(employee);
        return Ok(response);
    }

    /// <summary>
    /// Delete employee
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteEmployee(string id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
        return NoContent();
    }
}
```

---

## gRPC Services

### 1. Proto Definition

```protobuf
// Protos/employee.proto
syntax = "proto3";

package hranalytics.employee;

service EmployeeService {
  rpc GetEmployee (GetEmployeeRequest) returns (EmployeeResponse);
  rpc GetEmployees (GetEmployeesRequest) returns (EmployeeListResponse);
  rpc CreateEmployee (CreateEmployeeRequest) returns (EmployeeResponse);
}

message GetEmployeeRequest {
  string employee_id = 1;
  int32 company_id = 2;
}

message EmployeeResponse {
  string id = 1;
  string full_name = 2;
  string email = 3;
  string department = 4;
  double base_salary = 5;
  string status = 6;
}

message GetEmployeesRequest {
  int32 company_id = 1;
  int32 page = 2;
  int32 page_size = 3;
}

message EmployeeListResponse {
  repeated EmployeeResponse employees = 1;
  int32 total_count = 2;
}

message CreateEmployeeRequest {
  int32 company_id = 1;
  string first_name = 2;
  string last_name = 3;
  string email = 4;
  string department = 5;
  double base_salary = 6;
}
```

### 2. gRPC Service Implementation

```csharp
// Services/EmployeeGrpcService.cs
public class EmployeeGrpcService : Employee.EmployeeBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeGrpcService(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public override async Task<EmployeeResponse> GetEmployee(
        GetEmployeeRequest request,
        ServerCallContext context)
    {
        var employee = await _employeeService.GetEmployeeAsync(request.EmployeeId);
        
        if (employee == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Employee not found"));

        return new EmployeeResponse
        {
            Id = employee.Id,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Email = employee.Email,
            Department = employee.Department,
            BaseSalary = (double)employee.BaseSalary,
            Status = employee.Status,
        };
    }
}
```

---

## Error Handling

```csharp
// Middleware/ErrorHandlingMiddleware.cs
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new { error = exception.Message };

        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            DuplicateException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "HRAnalytics.API.dll"]
```

---

**Last Updated:** July 2026
**Status:** ASP.NET Core Web API Complete
