namespace HR.Attendance.Application.Dtos.AttendanceRecord;

/// <summary>
/// Real-time event DTO for SignalR.
/// </summary>
public record RealTimeAttendanceEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // CheckIn, CheckOut
    public DateTime Timestamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? WorkHours { get; set; }
}
