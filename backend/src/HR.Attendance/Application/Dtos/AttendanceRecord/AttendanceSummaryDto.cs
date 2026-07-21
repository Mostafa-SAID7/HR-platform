namespace HR.Attendance.Application.Dtos.AttendanceRecord;

/// <summary>
/// Attendance summary DTO.
/// </summary>
public record AttendanceSummaryDto
{
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalLateDays { get; set; }
    public decimal TotalWorkHours { get; set; }
    public decimal AverageWorkHoursPerDay { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int ApprovedLeaveRequests { get; set; }
}
