namespace HR.Attendance.Domain.LeaveRequest.Events;

/// <summary>
/// Domain event raised when a leave request is rejected
/// </summary>
public record LeaveRequestRejectedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public Guid EmployeeId { get; set; }
}
