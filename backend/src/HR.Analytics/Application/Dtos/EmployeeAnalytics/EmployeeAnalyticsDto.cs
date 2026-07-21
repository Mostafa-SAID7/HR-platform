namespace HR.Analytics.Application.Dtos.EmployeeAnalytics;

/// <summary>
/// Employee analytics DTO.
/// </summary>
public record EmployeeAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int YearsOfService { get; set; }
    public int SkillCount { get; set; }
}
