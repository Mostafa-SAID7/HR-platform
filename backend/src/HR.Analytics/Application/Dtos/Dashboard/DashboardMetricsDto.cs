namespace HR.Analytics.Application.Dtos.Dashboard;

/// <summary>
/// Dashboard metrics DTO.
/// </summary>
public record DashboardMetricsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public decimal AverageBasicSalary { get; set; }
    public decimal AveragePerformanceRating { get; set; }
    public decimal AverageAttendancePercentage { get; set; }
    public int TotalDepartments { get; set; }
    public DateTime ComputedDate { get; set; }
}
