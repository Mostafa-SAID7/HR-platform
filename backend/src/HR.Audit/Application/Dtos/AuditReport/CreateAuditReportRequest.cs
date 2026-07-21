namespace HR.Audit.Application.Dtos.AuditReport;

public record CreateAuditReportRequest(
    string Title,
    string ReportType,
    DateTime? StartDate = null,
    DateTime? EndDate = null);
