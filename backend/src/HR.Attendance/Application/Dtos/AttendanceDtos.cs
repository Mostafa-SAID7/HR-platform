namespace HR.Attendance.Application.Dtos;

/// <summary>
/// Check-in request DTO.
/// </summary>
public record CheckInRequest
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public string Location { get; set; } = string.Empty;
}

/// <summary>
/// Check-out request DTO.
/// </summary>
public record CheckOutRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public string Location { get; set; } = string.Empty;
}

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

/// <summary>
/// Leave request DTO.
/// </summary>
public record LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal LeaveDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

/// <summary>
/// Real-time event DTO for SignalR.
/// </summary>
public record RealTimeAttendanceEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // CheckIn, CheckOut
    public DateTime Timestamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? WorkHours { get; set; }
}

/// <summary>
/// Attendance summary DTO.
/// </summary>
public record AttendanceSummaryDto
{
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalLateDays { get; set; }
    public decimal TotalWorkHours { get; set; }
    public decimal AverageWorkHoursPerDay { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int ApprovedLeaveRequests { get; set; }
}
