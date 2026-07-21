namespace HR.Audit.Application.Dtos.AuditReport;

using HR.Audit.Application.Dtos.AuditEvent;

public record AuditReportDetailDto(
    Guid Id,
    string Title,
    string Description,
    DateTime GeneratedAt,
    DateTime? StartDate,
    DateTime? EndDate,
    string ReportType,
    string Status,
    List<AuditEventDto> Events,
    string? Findings,
    string? Recommendations,
    Guid? GeneratedByUserId);
