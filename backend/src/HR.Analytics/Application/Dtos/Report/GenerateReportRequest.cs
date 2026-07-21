namespace HR.Analytics.Application.Dtos.Report;

/// <summary>
/// Report generation request.
/// </summary>
public record GenerateReportRequest
{
    public string ReportType { get; set; } = string.Empty; // EmployeeReport, PayrollReport, etc.
    public int Year { get; set; }
    public int? Month { get; set; }
    public string? Department { get; set; }
}
