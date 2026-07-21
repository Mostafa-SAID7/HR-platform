namespace HR.Audit.Application.Dtos.AuditReport;

public record AuditReportDto(
    Guid Id,
    string Title,
    string Description,
    DateTime GeneratedAt,
    DateTime? StartDate,
    DateTime? EndDate,
    string ReportType,
    string Status,
    int EventCount,
    Guid? GeneratedByUserId);
