namespace HR.Attendance.Features.GetLeaveRequests;

using HR.Attendance.Application.Dtos.LeaveRequest;

public record GetLeaveRequestsQuery(Guid TenantId, string? Status = null) : IQuery<List<LeaveRequestDto>>;
