namespace HR.Tests.Integration.Attendance;

/// <summary>
/// Integration tests for Attendance Service (12 tests).
/// Tests cover: check-in/out, leave requests, and attendance workflows.
/// </summary>
[Collection("Database collection")]
public class AttendanceServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<AttendanceRecord> _attendanceRepository;
    private readonly IRepository<LeaveRequest> _leaveRepository;

    public AttendanceServiceIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _unitOfWork = _fixture.CreateUnitOfWork();
        _attendanceRepository = _unitOfWork.GetRepository<AttendanceRecord>();
        _leaveRepository = _unitOfWork.GetRepository<LeaveRequest>();
    }

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task CheckIn_RecordsAttendance()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var attendance = AttendanceRecord.Create(employeeId, DateTime.Now.Date, tenantId);

        // Act
        attendance.CheckIn(DateTime.Now);
        await _attendanceRepository.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _attendanceRepository.GetByIdAsync(attendance.Id);
        Assert.NotNull(retrieved.CheckInTime);
        Assert.Equal("Present", retrieved.Status);
    }

    [Fact]
    public async Task CheckOut_CalculatesWorkHours()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var attendance = AttendanceRecord.Create(employeeId, DateTime.Now.Date, tenantId);
        var checkInTime = DateTime.Now;
        var checkOutTime = checkInTime.AddHours(8);

        // Act
        attendance.CheckIn(checkInTime);
        attendance.CheckOut(checkOutTime);
        await _attendanceRepository.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _attendanceRepository.GetByIdAsync(attendance.Id);
        Assert.Equal(8.0m, retrieved.WorkHours);
    }

    [Fact]
    public async Task MarkAbsent_UpdatesStatus()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(Guid.NewGuid(), DateTime.Now.Date, Guid.NewGuid());
        await _attendanceRepository.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Act
        attendance.MarkAbsent();
        _attendanceRepository.Update(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _attendanceRepository.GetByIdAsync(attendance.Id);
        Assert.Equal("Absent", retrieved.Status);
    }

    [Fact]
    public async Task MarkLeave_RecordsLeaveType()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(Guid.NewGuid(), DateTime.Now.Date, Guid.NewGuid());
        await _attendanceRepository.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Act
        attendance.MarkLeave("Vacation");
        _attendanceRepository.Update(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _attendanceRepository.GetByIdAsync(attendance.Id);
        Assert.Equal("Leave", retrieved.Status);
        Assert.Equal("Vacation", retrieved.LeaveType);
    }

    [Fact]
    public async Task CreateLeaveRequest_PersistsToDatabase()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(4);

        var leaveRequest = LeaveRequest.Create(employeeId, startDate, endDate, "Vacation", tenantId);

        // Act
        await _leaveRepository.AddAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _leaveRepository.GetByIdAsync(leaveRequest.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Pending", retrieved.Status);
        Assert.Equal(5, retrieved.NumberOfDays);
    }

    [Fact]
    public async Task ApproveLeaveRequest_TransitionsStatus()
    {
        // Arrange
        var leaveRequest = LeaveRequest.Create(
            Guid.NewGuid(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation", Guid.NewGuid());
        await _leaveRepository.AddAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        // Act
        leaveRequest.Approve(Guid.NewGuid(), "Approved");
        _leaveRepository.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _leaveRepository.GetByIdAsync(leaveRequest.Id);
        Assert.Equal("Approved", retrieved.Status);
        Assert.NotNull(retrieved.ApprovedDate);
    }

    [Fact]
    public async Task RejectLeaveRequest_SetsRejectionReason()
    {
        // Arrange
        var leaveRequest = LeaveRequest.Create(
            Guid.NewGuid(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation", Guid.NewGuid());
        await _leaveRepository.AddAsync(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var reason = "Insufficient coverage";
        leaveRequest.Reject(Guid.NewGuid(), reason);
        _leaveRepository.Update(leaveRequest);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var retrieved = await _leaveRepository.GetByIdAsync(leaveRequest.Id);
        Assert.Equal("Rejected", retrieved.Status);
        Assert.Equal(reason, retrieved.RejectionReason);
    }

    [Fact]
    public async Task QueryTodayAttendance_FiltersByDate()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);
        var tenantId = Guid.NewGuid();

        var todayRecord = AttendanceRecord.Create(Guid.NewGuid(), today, tenantId);
        var yesterdayRecord = AttendanceRecord.Create(Guid.NewGuid(), yesterday, tenantId);

        await _attendanceRepository.AddAsync(todayRecord);
        await _attendanceRepository.AddAsync(yesterdayRecord);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _attendanceRepository.GetAsQueryable();
        var todayRecords = queryable
            .Where(a => a.AttendanceDate == today && a.TenantId == tenantId)
            .ToList();

        // Assert
        Assert.Single(todayRecords);
    }

    [Fact]
    public async Task GetEmployeeAttendanceByDateRange_FiltersCorrectly()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var startDate = DateTime.Now.Date.AddDays(-5);
        var endDate = DateTime.Now.Date;

        for (int i = 0; i < 10; i++)
        {
            var date = startDate.AddDays(i);
            var attendance = AttendanceRecord.Create(employeeId, date, tenantId);
            await _attendanceRepository.AddAsync(attendance);
        }
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _attendanceRepository.GetAsQueryable();
        var rangeRecords = queryable
            .Where(a => a.EmployeeId == employeeId && a.TenantId == tenantId &&
                       a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
            .ToList();

        // Assert
        Assert.Equal(6, rangeRecords.Count); // 6 days including start and end
    }

    [Fact]
    public async Task GetPendingLeaveRequests_FiltersByStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var pending = LeaveRequest.Create(
            Guid.NewGuid(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation", tenantId);

        var approved = LeaveRequest.Create(
            Guid.NewGuid(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation", tenantId);
        approved.Approve(Guid.NewGuid(), "Approved");

        await _leaveRepository.AddAsync(pending);
        await _leaveRepository.AddAsync(approved);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var queryable = _leaveRepository.GetAsQueryable();
        var pendingRequests = queryable
            .Where(l => l.TenantId == tenantId && l.Status == "Pending")
            .ToList();

        // Assert
        Assert.Single(pendingRequests);
    }

    [Fact]
    public async Task BulkInsertAttendance_PerformsEfficiently()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var records = Enumerable.Range(1, 100)
            .Select(i => AttendanceRecord.Create(Guid.NewGuid(), DateTime.Now.Date.AddDays(-(i % 30)), tenantId))
            .ToList();

        // Act
        foreach (var record in records)
        {
            await _attendanceRepository.AddAsync(record);
        }
        var startTime = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(duration.TotalSeconds < 5);
    }
}
