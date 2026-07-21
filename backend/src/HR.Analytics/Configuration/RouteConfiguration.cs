namespace HR.Analytics.Configuration;

using HR.Analytics.Features.Search;
using HR.Analytics.Features.Dashboard;

/// <summary>
/// Configuration for API route mappings in Analytics service
/// </summary>
public static class RouteConfiguration
{
    /// <summary>
    /// Map all Analytics service routes
    /// </summary>
    public static WebApplication MapAnalyticsRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/analytics")
            .WithTags("Analytics");

        // Search employees
        apiGroup.MapGet("/search", SearchEndpoint.Handle)
            .WithName("Search")
            .WithOpenApi()
            .RequireAuthorization();

        // Get dashboard metrics
        apiGroup.MapGet("/dashboard", GetDashboardEndpoint.Handle)
            .WithName("GetDashboard")
            .WithOpenApi()
            .RequireAuthorization();

        return app;
    }
}
