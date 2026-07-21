namespace HR.Attendance.Features.ApproveLeave;

using MediatR;
using HR.Attendance.Hubs;
using Microsoft.AspNetCore.SignalR;
using HR.Attendance.Application.Dtos.LeaveRequest;
using HR.Attendance.Application.Mappings;

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

        // Use centralized mapping instead of inline
        var dto = leaveRequest.ToDto();

        await _hubContext.Clients.All.SendAsync("ReceiveLeaveApproval", dto, cancellationToken);

        _logger.LogInformation("Leave request {LeaveRequestId} approved by {ApprovedBy}", 
            request.LeaveRequestId, request.ApprovedBy);
    }
}
