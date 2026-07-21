namespace HR.Attendance.Domain.AttendanceRecord.Events;

/// <summary>
/// Domain event raised when an employee checks in
/// </summary>
public record EmployeeCheckedInEvent : DomainEvent
{
    public Guid AttendanceRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string Location { get; set; } = string.Empty;
}
