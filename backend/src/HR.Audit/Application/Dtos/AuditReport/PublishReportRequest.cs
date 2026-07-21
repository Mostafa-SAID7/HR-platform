namespace HR.Audit.Application.Dtos.AuditReport;

public record PublishReportRequest(
    string? Findings,
    string? Recommendations);
