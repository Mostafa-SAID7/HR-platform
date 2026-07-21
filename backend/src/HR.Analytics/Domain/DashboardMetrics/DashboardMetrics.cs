namespace HR.Analytics.Domain.DashboardMetrics;

/// <summary>
/// Dashboard metrics - aggregated metrics for quick overview
/// Provides key performance indicators and company-wide metrics
/// </summary>
public class DashboardMetrics : BaseEntity
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public decimal AverageBasicSalary { get; set; }
    public decimal AverageNetSalary { get; set; }
    public decimal AveragePerformanceRating { get; set; }
    public decimal AverageAttendancePercentage { get; set; }
    public int TotalDepartments { get; set; }
    public DateTime ComputedDate { get; set; }
}
