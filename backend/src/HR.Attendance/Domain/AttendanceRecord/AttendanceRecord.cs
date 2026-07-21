namespace HR.Attendance.Domain.AttendanceRecord;

using HR.Attendance.Domain.AttendanceRecord.Events;

/// <summary>
/// Attendance record for employee check-in/check-out
/// </summary>
public class AttendanceRecord : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string CheckInLocation { get; set; } = string.Empty;
    public string CheckOutLocation { get; set; } = string.Empty;
    public decimal WorkHours { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? ShiftId { get; set; }

    private AttendanceRecord() { }

    /// <summary>
    /// Create a new attendance record
    /// </summary>
    public static AttendanceRecord Create(
        Guid employeeId,
        string employeeName,
        DateTime attendanceDate,
        Guid tenantId)
    {
        return new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            AttendanceDate = attendanceDate.Date,
            Status = AttendanceStatus.Absent,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Record check-in time
    /// </summary>
    public void CheckIn(DateTime checkInTime, string location)
    {
        CheckInTime = checkInTime;
        CheckInLocation = location;
        Status = AttendanceStatus.Present;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new EmployeeCheckedInEvent
        {
            AttendanceRecordId = Id,
            EmployeeId = EmployeeId,
            CheckInTime = checkInTime,
            Location = location,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Record check-out time and calculate work hours
    /// </summary>
    public void CheckOut(DateTime checkOutTime, string location)
    {
        CheckOutTime = checkOutTime;
        CheckOutLocation = location;

        if (CheckInTime.HasValue)
        {
            WorkHours = (decimal)(checkOutTime - CheckInTime.Value).TotalHours;
        }

        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new EmployeeCheckedOutEvent
        {
            AttendanceRecordId = Id,
            EmployeeId = EmployeeId,
            CheckOutTime = checkOutTime,
            WorkHours = WorkHours,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Mark as on leave
    /// </summary>
    public void MarkOnLeave(string notes)
    {
        Status = AttendanceStatus.OnLeave;
        Notes = notes;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as late
    /// </summary>
    public void MarkAsLate(string notes = null)
    {
        Status = AttendanceStatus.Late;
        Notes = notes;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as half day
    /// </summary>
    public void MarkAsHalfDay(string notes = null)
    {
        Status = AttendanceStatus.HalfDay;
        Notes = notes;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
