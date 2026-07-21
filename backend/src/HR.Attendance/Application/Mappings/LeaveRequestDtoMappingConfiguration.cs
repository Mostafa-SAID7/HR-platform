namespace HR.Attendance.Application.Mappings;

using HR.Attendance.Domain;
using HR.Attendance.Application.Dtos.LeaveRequest;

/// <summary>
/// Centralized mapping configuration for LeaveRequest DTOs.
/// </summary>
public static class LeaveRequestDtoMappingConfiguration
{
    public static LeaveRequestDto ToDto(this LeaveRequest leave)
    {
        return new LeaveRequestDto
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            EmployeeName = leave.EmployeeName,
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            LeaveDays = leave.LeaveDays,
            Reason = leave.Reason,
            Status = leave.Status,
            ApprovedBy = leave.ApprovedBy,
            ApprovedDate = leave.ApprovedDate
        };
    }
}
