namespace HR.Attendance.Features.GetTodayAttendance;

public record GetTodayAttendanceQuery(Guid EmployeeId, Guid TenantId) : IQuery<AttendanceRecordDto>;

public class GetTodayAttendanceQueryHandler : IRequestHandler<GetTodayAttendanceQuery, AttendanceRecordDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetTodayAttendanceQueryHandler> _logger;

    public GetTodayAttendanceQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetTodayAttendanceQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AttendanceRecordDto> Handle(GetTodayAttendanceQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<AttendanceRecord>();

        var record = await repo.GetAsQueryable()
            .FirstOrDefaultAsync(r => r.EmployeeId == request.EmployeeId && 
                r.AttendanceDate.Date == DateTime.UtcNow.Date && 
                r.TenantId == request.TenantId, cancellationToken);

        if (record is null)
            throw new NotFoundException("AttendanceRecord", request.EmployeeId);

        _logger.LogInformation("Retrieved today's attendance for employee {EmployeeId}", request.EmployeeId);

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
