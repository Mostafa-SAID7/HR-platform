# Configuration Folder - Service Setup Organization

## Overview
Modular configuration for HR.Payroll service organized by concern. Each configuration class handles one specific area following the **Single Responsibility Principle**.

## Structure

```
Configuration/
├── ServiceCollectionExtensions.cs     # Main DI registration
├── AuthenticationConfiguration.cs     # JWT & security
├── CorsConfiguration.cs               # CORS policies
├── HealthCheckConfiguration.cs        # Health checks
├── SwaggerConfiguration.cs            # OpenAPI/Swagger
├── RouteConfiguration.cs              # API route mapping
└── README.md                          # This file
```

## Files

### 1. **ServiceCollectionExtensions.cs**
Main entry point for all service registration.

```csharp
// In Program.cs:
builder.Services.AddPayrollService(connectionString);
```

**Responsibilities**:
- Register database context
- Register application services (MediatR, validators)
- Register infrastructure services (caching, messaging, queries)

### 2. **AuthenticationConfiguration.cs**
JWT authentication and authorization setup.

```csharp
// In Program.cs:
builder.Services.AddPayrollAuthentication();
builder.Services.AddPayrollAuthorization();
```

### 3. **CorsConfiguration.cs**
Cross-Origin Resource Sharing policies.

```csharp
// In Program.cs:
builder.Services.AddPayrollCors();

// In middleware:
app.UseCors("AllowGateway");
```

### 4. **HealthCheckConfiguration.cs**
Application health monitoring setup.

```csharp
// In Program.cs:
builder.Services.AddPayrollHealthChecks(builder.Configuration);
```

### 5. **SwaggerConfiguration.cs**
OpenAPI specification and Swagger UI.

```csharp
// In Program.cs:
builder.Services.AddPayrollSwagger();

// In middleware:
app.UsePayrollSwagger();
```

### 6. **RouteConfiguration.cs**
API endpoint mapping organized by feature.

```csharp
// In Program.cs:
app.MapPayrollRoutes();
```

**Route Groups**:
- Payroll Management: Calculate, Approve, Add Deductions
- Payment Processing: Process Payment
- Reporting: Get Payslip, Get Payroll Report

## SOLID Principles Applied

✅ **Single Responsibility**: Each class configures ONE concern
✅ **Open/Closed**: Easy to extend without modifying
✅ **Interface Segregation**: Small, focused methods
✅ **Dependency Inversion**: Depend on abstractions

---

**Pattern**: SOLID Principles + Configuration Pattern
**Status**: ✅ Production Ready
