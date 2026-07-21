namespace HR.Audit.Configuration;

using HR.Audit.Features.GetAuditTrail;
using HR.Audit.Features.CreateAuditReport;

/// <summary>
/// Configuration for API route mappings in Audit service
/// </summary>
public static class RouteConfiguration
{
    /// <summary>
    /// Map all Audit service routes
    /// </summary>
    public static WebApplication MapAuditRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/audit")
            .WithTags("Audit");

        // Get audit trail for entity
        apiGroup.MapGet("/trail/{entityType}/{entityId:guid}", GetAuditTrailEndpoint.Handle)
            .WithName("GetAuditTrail")
            .WithOpenApi()
            .RequireAuthorization();

        // Create audit report
        apiGroup.MapPost("/reports", CreateAuditReportEndpoint.Handle)
            .WithName("CreateAuditReport")
            .WithOpenApi()
            .RequireAuthorization();

        // Get audit trail summary
        apiGroup.MapGet("/summary/{entityType}/{entityId:guid}", GetAuditTrailSummaryEndpoint.Handle)
            .WithName("GetAuditTrailSummary")
            .WithOpenApi()
            .RequireAuthorization();

        return app;
    }
}
