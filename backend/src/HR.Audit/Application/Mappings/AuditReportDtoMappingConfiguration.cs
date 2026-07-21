namespace HR.Audit.Application.Mappings;

using HR.Audit.Domain;
using HR.Audit.Application.Dtos.AuditReport;

/// <summary>
/// Centralized mapping configuration for AuditReport DTOs.
/// </summary>
public static class AuditReportDtoMappingConfiguration
{
    public static AuditReportDto ToDto(this AuditReport auditReport)
    {
        return new AuditReportDto(
            auditReport.Id,
            auditReport.Title,
            auditReport.Description,
            auditReport.GeneratedAt,
            auditReport.StartDate,
            auditReport.EndDate,
            auditReport.Type.ToString(),
            auditReport.Status.ToString(),
            auditReport.Events.Count,
            auditReport.GeneratedByUserId);
    }

    public static AuditReportDetailDto ToDetailDto(this AuditReport auditReport)
    {
        return new AuditReportDetailDto
        {
            Id = auditReport.Id,
            Title = auditReport.Title,
            Description = auditReport.Description,
            GeneratedAt = auditReport.GeneratedAt,
            StartDate = auditReport.StartDate,
            EndDate = auditReport.EndDate,
            Type = auditReport.Type.ToString(),
            Status = auditReport.Status.ToString(),
            EventCount = auditReport.Events.Count,
            GeneratedByUserId = auditReport.GeneratedByUserId,
            Events = auditReport.Events.Select(e => e.ToDto()).ToList()
        };
    }
}
