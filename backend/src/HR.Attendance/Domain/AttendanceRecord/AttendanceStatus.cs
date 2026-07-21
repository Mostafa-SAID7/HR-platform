namespace HR.Attendance.Domain.AttendanceRecord;

/// <summary>
/// Enum representing attendance status
/// </summary>
public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Early = 3,
    OnLeave = 4,
    HalfDay = 5
}
