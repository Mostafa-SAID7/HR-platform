# API Gateway & Request Routing

Comprehensive guide to API Gateway implementation, routing, rate limiting, and authentication gateway.

---

## API Gateway Overview

### Purpose

```
┌──────────────────────────────────────────────┐
│  Client Applications                         │
│  • Web Browser                               │
│  • Mobile Apps                               │
│  • Third-party Integrations                  │
└─────────────────┬──────────────────────────┘
                  │
                  │ HTTP/HTTPS
                  ▼
┌──────────────────────────────────────────────┐
│  API Gateway (YARP / Ocelot)                 │
│  ┌──────────────────────────────────────────┐│
│  │ Authentication & Authorization           ││
│  │ • JWT validation                         ││
│  │ • RBAC enforcement                       ││
│  │ • OAuth2 token exchange                  ││
│  └──────────────────────────────────────────┘│
│  ┌──────────────────────────────────────────┐│
│  │ Request Routing                          ││
│  │ • /employees → Employee Service          ││
│  │ • /payroll → Payroll Service             ││
│  │ • /attendance → Attendance Service       ││
│  └──────────────────────────────────────────┘│
│  ┌──────────────────────────────────────────┐│
│  │ Rate Limiting & Throttling               ││
│  │ • Per-user limits                        ││
│  │ • Per-IP limits                          ││
│  │ • Burst protection                       ││
│  └──────────────────────────────────────────┘│
│  ┌──────────────────────────────────────────┐│
│  │ Request/Response Transformation          ││
│  │ • Header injection                       ││
│  │ • Request aggregation                    ││
│  │ • Response caching                       ││
│  └──────────────────────────────────────────┘│
└─────────────────┬──────────────────────────┘
                  │
        ┌─────────┼─────────┐
        ▼         ▼         ▼
    ┌────────┐┌────────┐┌────────┐
    │Employee││Payroll ││Attendance
    │Service ││Service ││Service
    └────────┘└────────┘└────────┘
```

---

## YARP (Yet Another Reverse Proxy) Setup

### Configuration

```csharp
// Program.cs
services.AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"))
    .AddTransforms<CustomTransforms>();

app.MapReverseProxy(proxyPipeline =>
{
    // Add authentication middleware
    proxyPipeline.UseAuthentication();
    proxyPipeline.UseAuthorization();
    
    // Add rate limiting
    proxyPipeline.UseRateLimiter();
    
    // Add custom correlation ID
    proxyPipeline.UseMiddleware<CorrelationIdMiddleware>();
    
    // Add request logging
    proxyPipeline.UseMiddleware<RequestLoggingMiddleware>();
});
```

### appsettings.json Configuration

```json
{
  "ReverseProxy": {
    "Routes": {
      "employees-route": {
        "ClusterId": "employees-cluster",
        "Match": {
          "Path": "/api/v1/employees{**catch-all}"
        },
        "AuthorizationPolicy": "AuthorizedEmployees",
        "Transforms": [
          { "PathPattern": "/api/v1/employees{**catch-all}", "PathPrefix": "/api" }
        ]
      },
      "payroll-route": {
        "ClusterId": "payroll-cluster",
        "Match": {
          "Path": "/api/v1/payroll{**catch-all}"
        },
        "AuthorizationPolicy": "AuthorizedPayroll"
      },
      "attendance-route": {
        "ClusterId": "attendance-cluster",
        "Match": {
          "Path": "/api/v1/attendance{**catch-all}"
        },
        "AuthorizationPolicy": "AuthorizedAttendance"
      },
      "analytics-route": {
        "ClusterId": "analytics-cluster",
        "Match": {
          "Path": "/api/v1/analytics{**catch-all}"
        },
        "AuthorizationPolicy": "AuthorizedAnalytics",
        "RateLimiterPolicy": "AnalyticsLimit"
      },
      "health-route": {
        "ClusterId": "health-cluster",
        "Match": {
          "Path": "/health"
        }
      }
    },
    "Clusters": {
      "employees-cluster": {
        "Destinations": {
          "employees-1": {
            "Address": "http://employee-service-1:5001"
          },
          "employees-2": {
            "Address": "http://employee-service-2:5001"
          },
          "employees-3": {
            "Address": "http://employee-service-3:5001"
          }
        },
        "SessionAffinity": {
          "Enabled": false,
          "Mode": "Cookie"
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:05",
            "Policy": "ConsecutiveFailures",
            "UnhealthyThreshold": 3
          }
        }
      },
      "payroll-cluster": {
        "Destinations": {
          "payroll-1": { "Address": "http://payroll-service-1:5002" },
          "payroll-2": { "Address": "http://payroll-service-2:5002" }
        }
      },
      "attendance-cluster": {
        "Destinations": {
          "attendance-1": { "Address": "http://attendance-service-1:5003" }
        }
      },
      "analytics-cluster": {
        "Destinations": {
          "analytics-1": { "Address": "http://analytics-service-1:5005" }
        }
      },
      "health-cluster": {
        "Destinations": {
          "health-1": { "Address": "http://api-gateway:8080" }
        }
      }
    }
  }
}
```

---

## Authentication Gateway

### JWT Validation

```csharp
// Middleware/JwtValidationMiddleware.cs
public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenService _tokenService;
    private readonly ILogger<JwtValidationMiddleware> _logger;
    
    public JwtValidationMiddleware(
        RequestDelegate next,
        ITokenService tokenService,
        ILogger<JwtValidationMiddleware> logger)
    {
        _next = next;
        _tokenService = tokenService;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractToken(context.Request.Headers);
        
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }
        
        try
        {
            var principal = await _tokenService.ValidateTokenAsync(token);
            context.User = principal;
            
            // Add user info to request headers for downstream services
            context.Request.Headers.Add("X-User-Id", principal.FindFirst("sub")?.Value ?? "");
            context.Request.Headers.Add("X-Company-Id", principal.FindFirst("company_id")?.Value ?? "");
            context.Request.Headers.Add("X-User-Roles", principal.FindFirst("roles")?.Value ?? "");
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning($"Invalid token: {ex.Message}");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
            return;
        }
        
        await _next(context);
    }
    
    private string ExtractToken(IHeaderDictionary headers)
    {
        var authHeader = headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
            return null;
        
        var parts = authHeader.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 && parts[0] == "Bearer" ? parts[1] : null;
    }
}

// Register middleware
app.UseMiddleware<JwtValidationMiddleware>();
```

### RBAC (Role-Based Access Control)

```csharp
// Policies/AuthorizationPolicies.cs
public static class AuthorizationPolicies
{
    public const string EmployeeAdmin = "EmployeeAdmin";
    public const string PayrollManager = "PayrollManager";
    public const string AnalyticsViewer = "AnalyticsViewer";
    public const string HRManager = "HRManager";
    
    public static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Employee management
            options.AddPolicy(EmployeeAdmin, policy =>
            {
                policy.RequireRole("admin", "hr_manager");
                policy.RequireClaim("scope", "employee:write");
            });
            
            // Payroll management
            options.AddPolicy(PayrollManager, policy =>
            {
                policy.RequireRole("admin", "payroll_manager");
                policy.RequireClaim("scope", "payroll:write");
            });
            
            // Analytics viewing
            options.AddPolicy(AnalyticsViewer, policy =>
            {
                policy.RequireRole("admin", "hr_manager", "analyst");
                policy.RequireClaim("scope", "analytics:read");
            });
            
            // HR management
            options.AddPolicy(HRManager, policy =>
            {
                policy.RequireRole("admin", "hr_manager");
            });
        });
        
        return services;
    }
}

// Usage in gateway
app.MapPost("/api/v1/employees", CreateEmployee)
    .RequireAuthorization(AuthorizationPolicies.EmployeeAdmin);

app.MapGet("/api/v1/payroll/reports", GetPayrollReports)
    .RequireAuthorization(AuthorizationPolicies.PayrollManager);

app.MapGet("/api/v1/analytics/dashboard", GetDashboard)
    .RequireAuthorization(AuthorizationPolicies.AnalyticsViewer);
```

---

## Rate Limiting

### Token Bucket Algorithm

```csharp
// Middleware/RateLimitingMiddleware.cs
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly IDistributedCache _cache;
    
    public RateLimitingMiddleware(
        RequestDelegate next,
        IOptions<RateLimitOptions> options,
        IDistributedCache cache)
    {
        _next = next;
        _options = options.Value;
        _cache = cache;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress.ToString();
        var endpoint = context.Request.Path.ToString();
        
        var cacheKey = $"rate-limit:{userId}:{endpoint}";
        var currentCount = await GetRequestCountAsync(cacheKey);
        
        if (currentCount >= _options.MaxRequests)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("Retry-After", "60");
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded" });
            return;
        }
        
        // Increment counter
        await _cache.SetAsync(
            cacheKey,
            BitConverter.GetBytes(currentCount + 1),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.WindowSeconds)
            }
        );
        
        // Add headers
        context.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", ((_options.MaxRequests - currentCount - 1)).ToString());
        
        await _next(context);
    }
    
    private async Task<int> GetRequestCountAsync(string cacheKey)
    {
        var cached = await _cache.GetAsync(cacheKey);
        return cached == null ? 0 : BitConverter.ToInt32(cached, 0);
    }
}

// Options
public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 1000;
    public int WindowSeconds { get; set; } = 60;
}

// Configuration
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});

services.Configure<RateLimitOptions>(configuration.GetSection("RateLimit"));
app.UseMiddleware<RateLimitingMiddleware>();
```

### Per-Endpoint Rate Limits

```json
{
  "RateLimiting": {
    "Global": {
      "MaxRequests": 1000,
      "WindowSeconds": 60
    },
    "Endpoints": {
      "/api/v1/analytics/export": {
        "MaxRequests": 10,
        "WindowSeconds": 3600,
        "Message": "Analytics export limited to 10 per hour"
      },
      "/api/v1/payroll/calculate": {
        "MaxRequests": 100,
        "WindowSeconds": 60
      }
    },
    "UserRoles": {
      "premium": {
        "Multiplier": 2
      },
      "enterprise": {
        "Multiplier": 5
      }
    }
  }
}
```

---

## Request Aggregation

### Combining Multiple Service Calls

```csharp
// Controllers/AggregatedController.cs
[ApiController]
[Route("api/v1")]
public class AggregatedController : ControllerBase
{
    private readonly IEmployeeGrpcClient _employeeClient;
    private readonly IPayrollGrpcClient _payrollClient;
    private readonly IPerformanceGrpcClient _performanceClient;
    
    public AggregatedController(
        IEmployeeGrpcClient employeeClient,
        IPayrollGrpcClient payrollClient,
        IPerformanceGrpcClient performanceClient)
    {
        _employeeClient = employeeClient;
        _payrollClient = payrollClient;
        _performanceClient = performanceClient;
    }
    
    /// <summary>
    /// Get complete employee profile (from multiple services)
    /// Single request returns data from Employee + Payroll + Performance services
    /// </summary>
    [HttpGet("employees/{id}/profile")]
    public async Task<IActionResult> GetEmployeeProfile(string id)
    {
        try
        {
            // Call all services in parallel
            var employeeTask = _employeeClient.GetEmployeeAsync(id);
            var payrollTask = _payrollClient.GetEmployeePayrollAsync(id);
            var performanceTask = _performanceClient.GetEmployeePerformanceAsync(id);
            
            await Task.WhenAll(employeeTask, payrollTask, performanceTask);
            
            var profile = new EmployeeProfileDto
            {
                Employee = employeeTask.Result,
                Payroll = payrollTask.Result,
                Performance = performanceTask.Result
            };
            
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get department summary (employee count + payroll + turnover risk)
    /// </summary>
    [HttpGet("departments/{id}/summary")]
    public async Task<IActionResult> GetDepartmentSummary(string id)
    {
        var employees = await _employeeClient.GetEmployeesByDepartmentAsync(id);
        var payrollSummary = await _payrollClient.GetDepartmentPayrollAsync(id);
        var performance = await _performanceClient.GetDepartmentPerformanceAsync(id);
        
        return Ok(new
        {
            EmployeeCount = employees.Count,
            AverageSalary = payrollSummary.AverageSalary,
            PayrollCost = payrollSummary.TotalCost,
            AveragePerformanceRating = performance.AverageRating,
            HighRiskEmployees = performance.AtRiskCount
        });
    }
}
```

---

## Response Caching

```csharp
// Middleware/ResponseCachingMiddleware.cs
public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ResponseCachingMiddleware> _logger;
    
    // Cacheable endpoints
    private static readonly string[] CacheableEndpoints = new[]
    {
        "/api/v1/employees",
        "/api/v1/departments",
        "/api/v1/analytics"
    };
    
    public ResponseCachingMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }
        
        var cacheKey = GenerateCacheKey(context.Request);
        var cachedResponse = await _cache.GetStringAsync(cacheKey);
        
        if (cachedResponse != null)
        {
            _logger.LogInformation($"Cache hit: {cacheKey}");
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(JsonDocument.Parse(cachedResponse).RootElement);
            return;
        }
        
        var originalResponse = context.Response.Body;
        using (var memoryStream = new MemoryStream())
        {
            context.Response.Body = memoryStream;
            
            await _next(context);
            
            memoryStream.Position = 0;
            var response = await new StreamReader(memoryStream).ReadToEndAsync();
            
            // Cache successful responses
            if (context.Response.StatusCode == 200 && IsCacheableEndpoint(context.Request.Path))
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    response,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    }
                );
            }
            
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalResponse);
        }
        
        context.Response.Body = originalResponse;
    }
    
    private string GenerateCacheKey(HttpRequest request)
    {
        return $"response-cache:{request.Method}:{request.Path}:{request.QueryString}";
    }
    
    private bool IsCacheableEndpoint(PathString path)
    {
        return CacheableEndpoints.Any(ep => path.StartsWithSegments(ep));
    }
}

app.UseMiddleware<ResponseCachingMiddleware>();
```

---

## Health Checks

```csharp
// HealthChecks/ServiceHealthChecks.cs
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck<EmployeeServiceHealthCheck>("employee-service")
    .AddCheck<PayrollServiceHealthCheck>("payroll-service")
    .AddCheck<AttendanceServiceHealthCheck>("attendance-service")
    .AddCheck<AnalyticsServiceHealthCheck>("analytics-service");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteResponse
});

// Custom health check
public class EmployeeServiceHealthCheck : IHealthCheck
{
    private readonly IEmployeeGrpcClient _client;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.GetHealthAsync();
            return HealthCheckResult.Healthy("Employee service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Employee service is down", ex);
        }
    }
}
```

---

**Last Updated:** July 2026
**Status:** API Gateway Complete
