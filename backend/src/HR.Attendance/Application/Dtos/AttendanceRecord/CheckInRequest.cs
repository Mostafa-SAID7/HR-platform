namespace HR.Attendance.Application.Dtos.AttendanceRecord;

/// <summary>
/// Check-in request DTO.
/// </summary>
public record CheckInRequest
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public string Location { get; set; } = string.Empty;
}
