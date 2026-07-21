namespace HR.Analytics.Features.Dashboard;

/// <summary>
/// Query definition for retrieving dashboard metrics.
/// SOLID: Query record separated from handler.
/// </summary>
public record GetDashboardMetricsQuery(Guid TenantId) : IQuery<DashboardMetricsDto>;
