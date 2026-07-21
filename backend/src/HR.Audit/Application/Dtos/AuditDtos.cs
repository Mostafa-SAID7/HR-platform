namespace HR.Audit.Application.Dtos;

// Audit Event DTOs
public record AuditEventDto(
    Guid Id,
    Guid EntityId,
    string EntityType,
    string Action,
    Guid? UserId,
    string? UserEmail,
    DateTime Timestamp,
    string? OldValues,
    string? NewValues,
    string? Reason,
    string Severity,
    string? IpAddress);

public record AuditEventDetailDto(
    Guid Id,
    Guid EntityId,
    string EntityType,
    string Action,
    Guid? UserId,
    string? UserEmail,
    DateTime Timestamp,
    string? OldValues,
    string? NewValues,
    string? Reason,
    Dictionary<string, string> Metadata,
    string Severity,
    string? IpAddress,
    string? UserAgent);

public record AuditEventFilterDto(
    Guid? EntityId = null,
    string? EntityType = null,
    string? Action = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Severity = null,
    int Page = 1,
    int PageSize = 10);

// Audit Trail DTOs
public record AuditTrailDto(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers);

public record AuditTrailDetailDto(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers,
    List<AuditEventDto> Events);

public record AuditTrailSummaryDto(
    Guid EntityId,
    string EntityType,
    DateTime FirstChangeAt,
    DateTime LastChangeAt,
    int ChangeCount,
    List<string> AffectedUsers,
    int CriticalEventCount,
    int WarningEventCount,
    int InfoEventCount);

// Compliance Policy DTOs
public record CreateCompliancePolicyRequest(
    string Name,
    string Description,
    List<string> AuditedEntities,
    List<string> CriticalActions,
    int RetentionDays);

public record CompliancePolicyDto(
    Guid Id,
    string Name,
    string Description,
    List<string> AuditedEntities,
    List<string> CriticalActions,
    int RetentionDays,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Audit Report DTOs
public record CreateAuditReportRequest(
    string Title,
    string ReportType,
    DateTime? StartDate = null,
    DateTime? EndDate = null);

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

public record PublishReportRequest(
    string? Findings,
    string? Recommendations);

// Query DTOs
public record ChangeLogDto(
    DateTime Timestamp,
    string EntityType,
    string Action,
    string? UserEmail,
    Dictionary<string, object>? Changes);

public record ComplianceCheckDto(
    bool IsCompliant,
    int TotalAuditEvents,
    int CriticalEvents,
    int WarningEvents,
    DateTime LastCheckedAt,
    List<string> Issues);
