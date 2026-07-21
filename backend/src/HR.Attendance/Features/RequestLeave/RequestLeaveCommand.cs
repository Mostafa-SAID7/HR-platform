namespace HR.Attendance.Features.RequestLeave;

using HR.Attendance.Application.Dtos.LeaveRequest;

public record RequestLeaveCommand(LeaveRequestDto Request, Guid TenantId) : ICommand<LeaveRequestDto>;
