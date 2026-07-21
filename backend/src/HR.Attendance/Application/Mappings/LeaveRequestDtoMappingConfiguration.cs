namespace HR.Attendance.Application.Mappings;

using HR.Attendance.Domain;
using HR.Attendance.Application.Dtos.LeaveRequest;

/// <summary>
/// Centralized mapping configuration for LeaveRequest DTOs.
/// </summary>
public static class LeaveRequestDtoMappingConfiguration
{
    public static LeaveRequestDto ToDto(this LeaveRequest leaveRequest)
    {
        return new LeaveRequestDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            EmployeeName = leaveRequest.EmployeeName,
            LeaveType = leaveRequest.LeaveType,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            LeaveDays = leaveRequest.LeaveDays,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            ApprovedBy = leaveRequest.ApprovedBy,
            ApprovedDate = leaveRequest.ApprovedDate
        };
    }
}
