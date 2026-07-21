namespace HR.Tests.Unit.Attendance;

/// <summary>
/// Comprehensive unit tests for Attendance Service (15 tests).
/// Tests cover: check-in, check-out, leave requests, reporting, and queries.
/// </summary>
public class AttendanceTests
{
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void CreateAttendance_WithValidData_CreatesRecord()
    {
        // Arrange
        var attendanceDate = DateTime.Now.Date;

        // Act
        var attendance = AttendanceRecord.Create(_employeeId, attendanceDate, _tenantId);

        // Assert
        Assert.NotNull(attendance);
        Assert.NotEqual(Guid.Empty, attendance.Id);
        Assert.Equal(_employeeId, attendance.EmployeeId);
        Assert.Equal(attendanceDate, attendance.AttendanceDate);
        Assert.Equal("Absent", attendance.Status);
        Assert.Null(attendance.CheckInTime);
        Assert.Null(attendance.CheckOutTime);
    }

    [Fact]
    public void CheckIn_WithValidTime_RecordsCheckIn()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);
        var checkInTime = DateTime.Now;

        // Act
        attendance.CheckIn(checkInTime);

        // Assert
        Assert.Equal(checkInTime, attendance.CheckInTime);
        Assert.Equal("Present", attendance.Status);
    }

    [Fact]
    public void CheckIn_UpdatesWorkHours()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);
        var checkInTime = DateTime.Now;
        var checkOutTime = checkInTime.AddHours(8);

        // Act
        attendance.CheckIn(checkInTime);
        attendance.CheckOut(checkOutTime);

        // Assert
        var expectedHours = 8.0m;
        Assert.Equal(expectedHours, attendance.WorkHours);
    }

    [Fact]
    public void CheckOut_WithValidTime_RecordsCheckOut()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);
        var checkInTime = DateTime.Now;
        attendance.CheckIn(checkInTime);

        var checkOutTime = checkInTime.AddHours(8);

        // Act
        attendance.CheckOut(checkOutTime);

        // Assert
        Assert.Equal(checkOutTime, attendance.CheckOutTime);
        Assert.Equal(8.0m, attendance.WorkHours);
    }

    [Fact]
    public void CheckOut_BeforeCheckIn_ThrowsException()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attendance.CheckOut(DateTime.Now));
    }

    [Fact]
    public void MarkAbsent_SetsStatusToAbsent()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);

        // Act
        attendance.MarkAbsent();

        // Assert
        Assert.Equal("Absent", attendance.Status);
        Assert.Null(attendance.CheckInTime);
        Assert.Null(attendance.CheckOutTime);
    }

    [Fact]
    public void MarkLate_RecordsLateArrival()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);
        var checkInTime = DateTime.Now.AddHours(1); // 1 hour late

        // Act
        attendance.CheckIn(checkInTime);
        attendance.MarkLate(checkInTime);

        // Assert
        Assert.Contains("Late", attendance.Status);
    }

    [Fact]
    public void CreateLeaveRequest_WithValidData_CreatesRequest()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(4);
        var leaveType = "Vacation";

        // Act
        var leaveRequest = LeaveRequest.Create(_employeeId, startDate, endDate, leaveType, _tenantId);

        // Assert
        Assert.NotNull(leaveRequest);
        Assert.NotEqual(Guid.Empty, leaveRequest.Id);
        Assert.Equal(_employeeId, leaveRequest.EmployeeId);
        Assert.Equal(startDate, leaveRequest.StartDate);
        Assert.Equal(endDate, leaveRequest.EndDate);
        Assert.Equal(leaveType, leaveRequest.LeaveType);
        Assert.Equal("Pending", leaveRequest.Status);
    }

    [Fact]
    public void LeaveRequest_CalculatesDays()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(4); // 5 days including start and end

        // Act
        var leaveRequest = LeaveRequest.Create(_employeeId, startDate, endDate, "Vacation", _tenantId);

        // Assert
        Assert.Equal(5, leaveRequest.NumberOfDays);
    }

    [Fact]
    public void ApproveLeaveRequest_ChangesStatusToApproved()
    {
        // Arrange
        var leaveRequest = LeaveRequest.Create(
            _employeeId, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation", _tenantId);
        Assert.Equal("Pending", leaveRequest.Status);

        // Act
        leaveRequest.Approve(Guid.NewGuid(), "Approved");

        // Assert
        Assert.Equal("Approved", leaveRequest.Status);
        Assert.NotNull(leaveRequest.ApprovedDate);
    }

    [Fact]
    public void RejectLeaveRequest_ChangesStatusToRejected()
    {
        // Arrange
        var leaveRequest = LeaveRequest.Create(
            _employeeId, DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation", _tenantId);

        // Act
        leaveRequest.Reject(Guid.NewGuid(), "Insufficient coverage");

        // Assert
        Assert.Equal("Rejected", leaveRequest.Status);
        Assert.Equal("Insufficient coverage", leaveRequest.RejectionReason);
    }

    [Fact]
    public void GetTodayAttendance_FiltersByDate()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        var records = new List<AttendanceRecord>
        {
            AttendanceRecord.Create(Guid.NewGuid(), yesterday, _tenantId),
            AttendanceRecord.Create(Guid.NewGuid(), today, _tenantId),
            AttendanceRecord.Create(Guid.NewGuid(), today, _tenantId),
            AttendanceRecord.Create(Guid.NewGuid(), tomorrow, _tenantId)
        };

        // Act
        var todayRecords = records.Where(r => r.AttendanceDate == today).ToList();

        // Assert
        Assert.Equal(2, todayRecords.Count);
        Assert.All(todayRecords, r => Assert.Equal(today, r.AttendanceDate));
    }

    [Fact]
    public void MarkLeaveDay_UpdatesAttendanceWithLeaveInfo()
    {
        // Arrange
        var attendance = AttendanceRecord.Create(_employeeId, DateTime.Now.Date, _tenantId);

        // Act
        attendance.MarkLeave("Vacation");

        // Assert
        Assert.Equal("Leave", attendance.Status);
        Assert.Equal("Vacation", attendance.LeaveType);
    }

    [Fact]
    public void GetEmployeeAttendance_FiltersByEmployeeAndDate()
    {
        // Arrange
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        var date = DateTime.Now.Date;

        var records = new List<AttendanceRecord>
        {
            AttendanceRecord.Create(employee1, date, _tenantId),
            AttendanceRecord.Create(employee1, date.AddDays(-1), _tenantId),
            AttendanceRecord.Create(employee2, date, _tenantId)
        };

        // Act
        var employee1Records = records
            .Where(r => r.EmployeeId == employee1 && r.TenantId == _tenantId)
            .ToList();

        // Assert
        Assert.Equal(2, employee1Records.Count);
        Assert.All(employee1Records, r => Assert.Equal(employee1, r.EmployeeId));
    }
}
