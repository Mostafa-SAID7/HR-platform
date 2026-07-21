namespace HR.Attendance.Domain.EmployeeShift;

/// <summary>
/// Employee shift aggregate root
/// </summary>
public class EmployeeShift : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public string ShiftName { get; set; } = string.Empty; // Morning, Evening, Night
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int WorkHoursPerDay { get; set; }
    public string[] WorkDays { get; set; } = []; // Mon-Fri
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    private EmployeeShift() { }

    /// <summary>
    /// Create a new employee shift
    /// </summary>
    public static EmployeeShift Create(
        Guid employeeId,
        string shiftName,
        TimeSpan startTime,
        TimeSpan endTime,
        int workHoursPerDay,
        string[] workDays,
        DateTime effectiveDate,
        Guid tenantId,
        DateTime? endDate = null)
    {
        if (endTime <= startTime)
            throw new ValidationException("End time must be after start time");

        if (workHoursPerDay < 1 || workHoursPerDay > 24)
            throw new ValidationException("Work hours per day must be between 1 and 24");

        return new EmployeeShift
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ShiftName = shiftName,
            StartTime = startTime,
            EndTime = endTime,
            WorkHoursPerDay = workHoursPerDay,
            WorkDays = workDays,
            EffectiveDate = effectiveDate,
            EndDate = endDate,
            IsActive = true,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Deactivate the shift
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
        UpdatedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if employee should work on given day
    /// </summary>
    public bool IsWorkingDay(DayOfWeek dayOfWeek)
    {
        return WorkDays.Contains(dayOfWeek.ToString());
    }
}
