namespace HR.Attendance.Domain;

/// <summary>
/// Attendance record for employee check-in/check-out.
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
    public string Status { get; set; } = "Present"; // Present, Absent, Late, Early, OnLeave
    public string? Notes { get; set; }
    public Guid? ShiftId { get; set; }

    public static AttendanceRecord Create(Guid employeeId, string employeeName, DateTime attendanceDate, Guid tenantId)
    {
        return new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            AttendanceDate = attendanceDate.Date,
            Status = "Absent",
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Record check-in time.
    /// </summary>
    public void CheckIn(DateTime checkInTime, string location)
    {
        CheckInTime = checkInTime;
        CheckInLocation = location;
        Status = "Present";
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
    /// Record check-out time and calculate work hours.
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
}

/// <summary>
/// Leave request entity.
/// </summary>
public class LeaveRequest : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty; // Sick, Vacation, Personal, etc.
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal LeaveDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public static LeaveRequest Create(
        Guid employeeId,
        string employeeName,
        string leaveType,
        DateTime startDate,
        DateTime endDate,
        string reason,
        Guid tenantId)
    {
        var leaveDays = (decimal)(endDate.Date - startDate.Date).TotalDays + 1;

        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            LeaveType = leaveType,
            StartDate = startDate,
            EndDate = endDate,
            LeaveDays = leaveDays,
            Reason = reason,
            Status = "Pending",
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };

        request.AddDomainEvent(new LeaveRequestCreatedEvent
        {
            LeaveRequestId = request.Id,
            EmployeeId = employeeId,
            LeaveType = leaveType,
            StartDate = startDate,
            EndDate = endDate,
            TenantId = tenantId
        });

        return request;
    }

    /// <summary>
    /// Approve the leave request.
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        Status = "Approved";
        ApprovedBy = approvedBy;
        ApprovedDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new LeaveRequestApprovedEvent
        {
            LeaveRequestId = Id,
            EmployeeId = EmployeeId,
            LeaveType = LeaveType,
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Reject the leave request.
    /// </summary>
    public void Reject(Guid rejectedBy)
    {
        Status = "Rejected";
        ApprovedBy = rejectedBy;
        ApprovedDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new LeaveRequestRejectedEvent
        {
            LeaveRequestId = Id,
            EmployeeId = EmployeeId,
            TenantId = TenantId
        });
    }
}

/// <summary>
/// Employee shift entity.
/// </summary>
public class EmployeeShift : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string ShiftName { get; set; } = string.Empty; // Morning, Evening, Night
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int WorkHoursPerDay { get; set; }
    public string[] WorkDays { get; set; } = []; // Mon-Fri
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

// ===== DOMAIN EVENTS =====

public record EmployeeCheckedInEvent : DomainEvent
{
    public Guid AttendanceRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string Location { get; set; } = string.Empty;
}

public record EmployeeCheckedOutEvent : DomainEvent
{
    public Guid AttendanceRecordId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public decimal WorkHours { get; set; }
}

public record LeaveRequestCreatedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public record LeaveRequestApprovedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
}

public record LeaveRequestRejectedEvent : DomainEvent
{
    public Guid LeaveRequestId { get; set; }
    public Guid EmployeeId { get; set; }
}
