namespace HR.Analytics.Application.Dtos.Report;

/// <summary>
/// Report DTO.
/// </summary>
public record ReportDto
{
    public Guid ReportId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public int RecordCount { get; set; }
    public string ReportUrl { get; set; } = string.Empty; // URL to download report
}
