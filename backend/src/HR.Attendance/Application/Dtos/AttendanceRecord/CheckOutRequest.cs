namespace HR.Attendance.Application.Dtos.AttendanceRecord;

/// <summary>
/// Check-out request DTO.
/// </summary>
public record CheckOutRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public string Location { get; set; } = string.Empty;
}
