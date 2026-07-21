namespace HR.Analytics.Domain.PerformanceAnalytics;

/// <summary>
/// Performance analytics view - denormalized from Performance Service
/// Provides performance reviews and goal tracking data
/// </summary>
public class PerformanceAnalytics : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Quarter { get; set; }
    public decimal AverageRating { get; set; }
    public int GoalsSet { get; set; }
    public int GoalsCompleted { get; set; }
    public int FeedbackCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
}
