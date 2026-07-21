namespace HR.Attendance.Features.ApproveLeave;

public record ApproveLeaveCommand(Guid LeaveRequestId, Guid ApprovedBy, Guid TenantId) : ICommand;
