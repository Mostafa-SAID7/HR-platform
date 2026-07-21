namespace HR.Analytics.Features.Dashboard;

public record GetDashboardMetricsQuery(Guid TenantId) : IQuery<DashboardMetricsDto>;

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDashboardMetricsQueryHandler> _logger;

    public GetDashboardMetricsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDashboardMetricsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DashboardMetricsDto> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<DashboardMetrics>();

        // Get the most recent dashboard metrics
        var metrics = await repo.GetAsQueryable()
            .OrderByDescending(m => m.ComputedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (metrics is null)
        {
            _logger.LogWarning("No dashboard metrics found for tenant {TenantId}", request.TenantId);
            return new DashboardMetricsDto { ComputedDate = DateTime.UtcNow };
        }

        _logger.LogInformation("Dashboard metrics retrieved: {Employees} employees, Avg Rating: {Rating}",
            metrics.TotalEmployees, metrics.AveragePerformanceRating);

        return new DashboardMetricsDto
        {
            TotalEmployees = metrics.TotalEmployees,
            ActiveEmployees = metrics.ActiveEmployees,
            AverageBasicSalary = metrics.AverageBasicSalary,
            AveragePerformanceRating = metrics.AveragePerformanceRating,
            AverageAttendancePercentage = metrics.AverageAttendancePercentage,
            TotalDepartments = metrics.TotalDepartments,
            ComputedDate = metrics.ComputedDate
        };
    }
}
