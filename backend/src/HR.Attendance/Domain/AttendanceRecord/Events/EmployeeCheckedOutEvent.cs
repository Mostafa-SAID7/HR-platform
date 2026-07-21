namespace HR.Attendance.Domain.AttendanceRecord.Events;

/// <summary>
/// Domain event raised when an employee checks out
/// </summary>
public record EmployeeCheckedOutEvent : DomainEvent
{
    public Guid AttendanceRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public decimal WorkHours { get; set; }
}
