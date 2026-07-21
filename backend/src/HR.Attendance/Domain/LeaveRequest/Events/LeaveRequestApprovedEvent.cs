namespace HR.Attendance.Domain.LeaveRequest.Events;

/// <summary>
/// Domain event raised when a leave request is approved
/// </summary>
public record LeaveRequestApprovedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
}
