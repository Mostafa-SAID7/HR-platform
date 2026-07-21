namespace HR.Attendance.Domain.LeaveRequest;

/// <summary>
/// Enum representing the status of a leave request
/// </summary>
public enum LeaveRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Expired = 4
}
