namespace HR.Attendance.Features.GetTodayAttendance;

using MediatR;
using HR.Attendance.Application.Dtos.AttendanceRecord;
using HR.Attendance.Application.Mappings;

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

        // Use centralized mapping instead of inline
        return record.ToDto();
    }
}
