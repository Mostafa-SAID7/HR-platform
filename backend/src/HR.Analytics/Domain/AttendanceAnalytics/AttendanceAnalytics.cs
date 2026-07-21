namespace HR.Analytics.Domain.AttendanceAnalytics;

/// <summary>
/// Attendance analytics view - denormalized from Attendance Service
/// Provides attendance tracking and reporting data
/// </summary>
public class AttendanceAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal AverageWorkHours { get; set; }
    public DateTime ReportDate { get; set; }
}
