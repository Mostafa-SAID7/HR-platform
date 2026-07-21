namespace HR.Attendance.Features.CheckIn;

using HR.Attendance.Application.Dtos;
using HR.Attendance.Hubs;
using Microsoft.AspNetCore.SignalR;

public record CheckInCommand(CheckInRequest Request, Guid TenantId) : ICommand<AttendanceRecordDto>;

public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.Request.EmployeeId).NotEmpty().WithMessage("Employee ID is required");
        RuleFor(x => x.Request.EmployeeName).NotEmpty().WithMessage("Employee name is required");
        RuleFor(x => x.Request.Location).NotEmpty().WithMessage("Location is required");
    }
}

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, AttendanceRecordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AttendanceHub> _hubContext;
    private readonly ILogger<CheckInCommandHandler> _logger;

    public CheckInCommandHandler(
        IUnitOfWork unitOfWork,
        IHubContext<AttendanceHub> hubContext,
        ILogger<CheckInCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<AttendanceRecordDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var repo = _unitOfWork.GetRepository<AttendanceRecord>();

        // Get or create today's attendance record
        var record = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(r => r.EmployeeId == req.EmployeeId && 
                r.AttendanceDate.Date == req.CheckInTime.Date && 
                r.TenantId == request.TenantId, cancellationToken);

        if (record is null)
        {
            record = AttendanceRecord.Create(req.EmployeeId, req.EmployeeName, req.CheckInTime, request.TenantId);
            await repo.AddAsync(record, cancellationToken);
        }

        record.CheckIn(req.CheckInTime, req.Location);
        repo.Update(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Broadcast real-time check-in event via SignalR
        var @event = new RealTimeAttendanceEvent
        {
            EmployeeId = req.EmployeeId,
            EmployeeName = req.EmployeeName,
            EventType = "CheckIn",
            Timestamp = req.CheckInTime,
            Location = req.Location
        };

        await _hubContext.Clients.All.SendAsync("ReceiveCheckIn", @event, cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} ({EmployeeName}) checked in at {Location}", 
            req.EmployeeId, req.EmployeeName, req.Location);

        return MapToDto(record);
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord record)
    {
        return new AttendanceRecordDto
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            EmployeeName = record.EmployeeName,
            AttendanceDate = record.AttendanceDate,
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            WorkHours = record.WorkHours,
            Status = record.Status,
            CheckInLocation = record.CheckInLocation,
            CheckOutLocation = record.CheckOutLocation
        };
    }
}
