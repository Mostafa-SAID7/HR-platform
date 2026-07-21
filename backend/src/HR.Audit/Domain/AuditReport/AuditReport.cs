namespace HR.Audit.Domain.AuditReport;

using HR.Audit.Domain.AuditEvent;

/// <summary>
/// Audit Report - for compliance and investigation
/// </summary>
public class AuditReport
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportType Type { get; set; }
    public ReportStatus Status { get; set; }
    public List<AuditEvent> Events { get; set; } = [];
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public Guid? GeneratedByUserId { get; set; }

    private AuditReport() { }

    /// <summary>
    /// Create a compliance audit report
    /// </summary>
    public static AuditReport CreateComplianceReport(
        string title,
        DateTime startDate,
        DateTime endDate,
        Guid generatedByUserId)
    {
        return new AuditReport
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = $"Compliance audit report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            GeneratedAt = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Type = ReportType.Compliance,
            Status = ReportStatus.Generated,
            GeneratedByUserId = generatedByUserId
        };
    }

    /// <summary>
    /// Create an incident audit report
    /// </summary>
    public static AuditReport CreateIncidentReport(
        string title,
        string description,
        Guid generatedByUserId)
    {
        return new AuditReport
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            GeneratedAt = DateTime.UtcNow,
            Type = ReportType.Incident,
            Status = ReportStatus.InProgress,
            GeneratedByUserId = generatedByUserId
        };
    }

    /// <summary>
    /// Add findings to the report
    /// </summary>
    public void AddFindings(string findings, string recommendations)
    {
        Findings = findings;
        Recommendations = recommendations;
        Status = ReportStatus.Generated;
    }

    /// <summary>
    /// Publish the report
    /// </summary>
    public void Publish()
    {
        Status = ReportStatus.Published;
    }

    /// <summary>
    /// Archive the report
    /// </summary>
    public void Archive()
    {
        Status = ReportStatus.Archived;
    }
}
