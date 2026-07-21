namespace HR.Attendance.Domain.LeaveRequest;

using HR.Attendance.Domain.LeaveRequest.Events;

/// <summary>
/// Leave request aggregate root
/// </summary>
public class LeaveRequest : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal LeaveDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveRequestStatus Status { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    private LeaveRequest() { }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    public static LeaveRequest Create(
        Guid employeeId,
        string employeeName,
        LeaveType leaveType,
        DateTime startDate,
        DateTime endDate,
        string reason,
        Guid tenantId)
    {
        if (endDate < startDate)
            throw new ValidationException("End date must be after start date");

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
            Status = LeaveRequestStatus.Pending,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };

        request.AddDomainEvent(new LeaveRequestCreatedEvent
        {
            LeaveRequestId = request.Id,
            EmployeeId = employeeId,
            LeaveType = leaveType.ToString(),
            StartDate = startDate,
            EndDate = endDate,
            TenantId = tenantId
        });

        return request;
    }

    /// <summary>
    /// Approve the leave request
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new ValidationException("Only pending leave requests can be approved");

        Status = LeaveRequestStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;

        AddDomainEvent(new LeaveRequestApprovedEvent
        {
            LeaveRequestId = Id,
            EmployeeId = EmployeeId,
            LeaveType = LeaveType.ToString(),
            TenantId = TenantId
        });
    }

    /// <summary>
    /// Reject the leave request
    /// </summary>
    public void Reject(Guid rejectedBy)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new ValidationException("Only pending leave requests can be rejected");

        Status = LeaveRequestStatus.Rejected;
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

    /// <summary>
    /// Cancel the leave request
    /// </summary>
    public void Cancel()
    {
        if (Status != LeaveRequestStatus.Pending && Status != LeaveRequestStatus.Approved)
            throw new ValidationException("Only pending or approved leave requests can be cancelled");

        Status = LeaveRequestStatus.Cancelled;
        UpdatedOnUtc = DateTime.UtcNow;
    }
}
