namespace HR.Employee.Configuration;

using HR.Employee.Features.CreateEmployee;
using HR.Employee.Features.GetEmployees;
using HR.Employee.Features.GetEmployeeById;
using HR.Employee.Features.UpdateEmployee;
using HR.Employee.Features.TerminateEmployee;

/// <summary>
/// API route configuration for Employee service
/// Organized by feature following SOLID principles
/// </summary>
public static class RouteConfiguration
{
    /// <summary>
    /// Map all API endpoints
    /// </summary>
    public static WebApplication MapEmployeeRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/employee")
            .WithTags("Employees");

        // Employee CRUD endpoints
        apiGroup.MapPost("", CreateEmployeeEndpoint.Handle)
            .WithName("CreateEmployee")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("", GetEmployeesEndpoint.Handle)
            .WithName("GetEmployees")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/{id:guid}", GetEmployeeByIdEndpoint.Handle)
            .WithName("GetEmployeeById")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPut("/{id:guid}", UpdateEmployeeEndpoint.Handle)
            .WithName("UpdateEmployee")
            .WithOpenApi()
            .RequireAuthorization();

        // Employee termination endpoint
        apiGroup.MapPost("/{id:guid}/terminate", TerminateEmployeeEndpoint.Handle)
            .WithName("TerminateEmployee")
            .WithOpenApi()
            .RequireAuthorization();

        return app;
    }
}
