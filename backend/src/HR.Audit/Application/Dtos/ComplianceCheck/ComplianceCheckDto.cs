namespace HR.Audit.Application.Dtos.ComplianceCheck;

public record ComplianceCheckDto(
    bool IsCompliant,
    int TotalAuditEvents,
    int CriticalEvents,
    int WarningEvents,
    DateTime LastCheckedAt,
    List<string> Issues);
