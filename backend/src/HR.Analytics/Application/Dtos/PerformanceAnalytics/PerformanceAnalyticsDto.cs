namespace HR.Analytics.Application.Dtos.PerformanceAnalytics;

/// <summary>
/// Performance analytics DTO.
/// </summary>
public record PerformanceAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Quarter { get; set; }
    public decimal AverageRating { get; set; }
    public int GoalsCompleted { get; set; }
}
