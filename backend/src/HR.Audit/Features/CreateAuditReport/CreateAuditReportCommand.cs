namespace HR.Audit.Features.CreateAuditReport;

/// <summary>
/// Create an audit report for compliance
/// </summary>
public record CreateAuditReportCommand(
    CreateAuditReportRequest Request,
    Guid UserId,
    Guid TenantId) : ICommand<AuditReportDto>;
