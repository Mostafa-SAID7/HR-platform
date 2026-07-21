namespace HR.Attendance.Application.Dtos.AttendanceRecord;

/// <summary>
/// Attendance record DTO.
/// </summary>
public record AttendanceRecordDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal WorkHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CheckInLocation { get; set; } = string.Empty;
    public string CheckOutLocation { get; set; } = string.Empty;
}
