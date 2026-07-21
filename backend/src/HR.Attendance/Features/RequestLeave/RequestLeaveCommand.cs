namespace HR.Attendance.Features.RequestLeave;

using HR.Attendance.Hubs;
using Microsoft.AspNetCore.SignalR;

public record RequestLeaveCommand(LeaveRequestDto Request, Guid TenantId) : ICommand<LeaveRequestDto>;

public class RequestLeaveCommandValidator : AbstractValidator<RequestLeaveCommand>
{
    public RequestLeaveCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId).NotEmpty().WithMessage("Employee ID is required");
        RuleFor(x => x.Request.LeaveType).NotEmpty().WithMessage("Leave type is required");
        RuleFor(x => x.Request.StartDate).NotEmpty().WithMessage("Start date is required");
        RuleFor(x => x.Request.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.Request.StartDate).WithMessage("End date must be greater than or equal to start date");
        RuleFor(x => x.Request.Reason).NotEmpty().WithMessage("Reason is required");
    }
}

public class RequestLeaveCommandHandler : IRequestHandler<RequestLeaveCommand, LeaveRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AttendanceHub> _hubContext;
    private readonly ILogger<RequestLeaveCommandHandler> _logger;

    public RequestLeaveCommandHandler(
        IUnitOfWork unitOfWork,
        IHubContext<AttendanceHub> hubContext,
        ILogger<RequestLeaveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<LeaveRequestDto> Handle(RequestLeaveCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var leaveRequest = LeaveRequest.Create(
            req.EmployeeId,
            req.EmployeeName,
            req.LeaveType,
            req.StartDate,
            req.EndDate,
            req.Reason,
            request.TenantId);

        var repo = _unitOfWork.GetRepository<LeaveRequest>();
        await repo.AddAsync(leaveRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Broadcast via SignalR
        var dto = MapToDto(leaveRequest);
        await _hubContext.Clients.All.SendAsync("ReceiveLeaveRequest", dto, cancellationToken);

        _logger.LogInformation("Leave request created for employee {EmployeeId}, type: {LeaveType}", 
            req.EmployeeId, req.LeaveType);

        return dto;
    }

    private static LeaveRequestDto MapToDto(LeaveRequest leaveRequest)
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
