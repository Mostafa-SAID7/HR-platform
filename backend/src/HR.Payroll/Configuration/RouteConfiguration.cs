namespace HR.Payroll.Configuration;

using HR.Payroll.Features.CalculatePayroll;
using HR.Payroll.Features.ApprovePayroll;
using HR.Payroll.Features.ProcessPayment;
using HR.Payroll.Features.GetPayslip;
using HR.Payroll.Features.GetPayrollReport;
using HR.Payroll.Features.AddDeduction;

/// <summary>
/// API route configuration for Payroll service
/// Organized by feature following SOLID principles
/// </summary>
public static class RouteConfiguration
{
    /// <summary>
    /// Map all API endpoints
    /// </summary>
    public static WebApplication MapPayrollRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/payroll")
            .WithTags("Payroll");

        // Payroll management endpoints
        MapPayrollManagementRoutes(apiGroup);

        // Payment processing endpoints
        MapPaymentRoutes(apiGroup);

        // Reporting endpoints
        MapReportingRoutes(apiGroup);

        return app;
    }

    /// <summary>
    /// Map payroll management endpoints
    /// </summary>
    private static void MapPayrollManagementRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/calculate", CalculatePayrollEndpoint.Handle)
            .WithName("CalculatePayroll")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/{id:guid}/approve", ApprovePayrollEndpoint.Handle)
            .WithName("ApprovePayroll")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapPost("/{id:guid}/deductions", AddDeductionEndpoint.Handle)
            .WithName("AddDeduction")
            .WithOpenApi()
            .RequireAuthorization();
    }

    /// <summary>
    /// Map payment processing endpoints
    /// </summary>
    private static void MapPaymentRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapPost("/process-payment", ProcessPaymentEndpoint.Handle)
            .WithName("ProcessPayment")
            .WithOpenApi()
            .RequireAuthorization();
    }

    /// <summary>
    /// Map reporting endpoints
    /// </summary>
    private static void MapReportingRoutes(RouteGroupBuilder apiGroup)
    {
        apiGroup.MapGet("/{id:guid}/payslip", GetPayslipEndpoint.Handle)
            .WithName("GetPayslip")
            .WithOpenApi()
            .RequireAuthorization();

        apiGroup.MapGet("/reports", GetPayrollReportEndpoint.Handle)
            .WithName("GetPayrollReport")
            .WithOpenApi()
            .RequireAuthorization();
    }
}
