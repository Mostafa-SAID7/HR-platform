namespace HR.Audit.Features.CreateAuditReport;

using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using HR.Audit.Application.Mappings;

/// <summary>
/// Handler for CreateAuditReportCommand
/// </summary>
public class CreateAuditReportCommandHandler : ICommandHandler<CreateAuditReportCommand, AuditReportDto>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CreateAuditReportCommandHandler> _logger;

    public CreateAuditReportCommandHandler(
        IDistributedCache cache,
        ILogger<CreateAuditReportCommandHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<AuditReportDto> Handle(CreateAuditReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<ReportType>(request.Request.ReportType, true, out var reportType))
                throw new DomainException($"Invalid report type: {request.Request.ReportType}");

            AuditReport report;

            if (reportType == ReportType.Compliance)
            {
                var startDate = request.Request.StartDate ?? DateTime.UtcNow.AddDays(-30);
                var endDate = request.Request.EndDate ?? DateTime.UtcNow;

                report = AuditReport.CreateComplianceReport(
                    request.Request.Title,
                    startDate,
                    endDate,
                    request.UserId);

                // Retrieve events from cache for the date range
                var events = await RetrieveEventsForDateRangeAsync(startDate, endDate, cancellationToken);
                report.Events = events;
            }
            else
            {
                report = AuditReport.CreateIncidentReport(
                    request.Request.Title,
                    request.Request.Title,
                    request.UserId);
            }

            // Store report in cache
            var reportKey = $"audit:report:{report.Id}";
            var reportJson = System.Text.Json.JsonSerializer.Serialize(report);
            await _cache.SetStringAsync(reportKey, reportJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2555) // ~7 years
            }, cancellationToken);

            // Add to reports index
            var reportsIndexKey = "audit:reports:index";
            var reportsIndex = await GetReportsIndexAsync(cancellationToken);
            reportsIndex.Add(report.Id);

            var indexJson = System.Text.Json.JsonSerializer.Serialize(reportsIndex);
            await _cache.SetStringAsync(reportsIndexKey, indexJson, cancellationToken: cancellationToken);

            _logger.LogInformation("Audit report created: {ReportId} ({ReportType})", report.Id, reportType);

            // Use centralized mapping instead of inline
            return report.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit report");
            throw new DomainException($"Error creating audit report: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve audit events for a date range from cache
    /// </summary>
    private async Task<List<AuditEvent>> RetrieveEventsForDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var events = new List<AuditEvent>();

        try
        {
            // Get all events from monthly indexes
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                var monthKey = $"audit:events:{currentDate:yyyy-MM}";
                var monthJson = await _cache.GetStringAsync(monthKey, cancellationToken);

                if (!string.IsNullOrEmpty(monthJson))
                {
                    var monthEvents = System.Text.Json.JsonSerializer.Deserialize<List<AuditEvent>>(monthJson) ?? [];
                    events.AddRange(monthEvents.Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate));
                }

                currentDate = currentDate.AddMonths(1);
            }

            return events.OrderByDescending(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for date range {StartDate} - {EndDate}", startDate, endDate);
            return [];
        }
    }

    /// <summary>
    /// Get reports index from cache
    /// </summary>
    private async Task<List<Guid>> GetReportsIndexAsync(CancellationToken cancellationToken)
    {
        try
        {
            var indexKey = "audit:reports:index";
            var indexJson = await _cache.GetStringAsync(indexKey, cancellationToken);

            if (!string.IsNullOrEmpty(indexJson))
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(indexJson) ?? [];
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports index");
            return [];
        }
    }
}
