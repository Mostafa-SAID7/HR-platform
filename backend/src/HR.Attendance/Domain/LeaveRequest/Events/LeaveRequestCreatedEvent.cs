namespace HR.Attendance.Domain.LeaveRequest.Events;

/// <summary>
/// Domain event raised when a leave request is created
/// </summary>
public record LeaveRequestCreatedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
