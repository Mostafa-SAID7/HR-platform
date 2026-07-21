namespace HR.Attendance.Features.ApproveLeave;

using HR.Attendance.Hubs;
using Microsoft.AspNetCore.SignalR;

public record ApproveLeaveCommand(Guid LeaveRequestId, Guid ApprovedBy, Guid TenantId) : ICommand;

public class ApproveLeaveCommandHandler : IRequestHandler<ApproveLeaveCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AttendanceHub> _hubContext;
    private readonly ILogger<ApproveLeaveCommandHandler> _logger;

    public ApproveLeaveCommandHandler(
        IUnitOfWork unitOfWork,
        IHubContext<AttendanceHub> hubContext,
        ILogger<ApproveLeaveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<LeaveRequest>();

        var leaveRequest = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId && 
                lr.TenantId == request.TenantId, cancellationToken);

        if (leaveRequest is null)
            throw new NotFoundException("LeaveRequest", request.LeaveRequestId);

        leaveRequest.Approve(request.ApprovedBy);
        repo.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Broadcast approval notification
        var dto = new LeaveRequestDto
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

        await _hubContext.Clients.All.SendAsync("ReceiveLeaveApproval", dto, cancellationToken);

        _logger.LogInformation("Leave request {LeaveRequestId} approved by {ApprovedBy}", 
            request.LeaveRequestId, request.ApprovedBy);
    }
}
