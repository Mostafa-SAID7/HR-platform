namespace HR.Attendance.Domain.LeaveRequest;

/// <summary>
/// Enum representing types of leave
/// </summary>
public enum LeaveType
{
    Sick = 0,
    Vacation = 1,
    Personal = 2,
    Maternity = 3,
    Paternity = 4,
    Unpaid = 5,
    Casual = 6
}
