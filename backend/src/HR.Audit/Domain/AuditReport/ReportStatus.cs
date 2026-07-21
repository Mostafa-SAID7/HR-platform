namespace HR.Audit.Domain.AuditReport;

/// <summary>
/// Enum representing the status of an audit report
/// </summary>
public enum ReportStatus
{
    Draft = 0,
    InProgress = 1,
    Generated = 2,
    Published = 3,
    Archived = 4
}
