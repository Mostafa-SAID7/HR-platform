namespace HR.Analytics.Domain.EmployeeAnalytics;

/// <summary>
/// Employee analytics view - denormalized from Employee Service
/// Provides analytics and reporting data about employees
/// </summary>
public class EmployeeAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Inactive, Terminated
    public int YearsOfService { get; set; }
    public int SkillCount { get; set; }
    public decimal? CurrentSalary { get; set; }
    public DateTime LastUpdated { get; set; }
}
