namespace HR.Performance.Domain.PerformanceGoal;

using HR.Performance.Domain.PerformanceReview;
using HR.Performance.Domain.PerformanceGoal.Events;

/// <summary>
/// Performance goal aggregate root
/// </summary>
public class PerformanceGoal : AggregateRoot
{
    public Guid EmployeeId { get; set; }
    public Guid PerformanceReviewId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal ActualValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public PerformanceGoalStatus Status { get; set; }
    public decimal Weight { get; set; } // 0-100, percentage contribution to review

    public PerformanceReview? PerformanceReview { get; set; }

    private PerformanceGoal() { }

    /// <summary>
    /// Create a new performance goal
    /// </summary>
    public static PerformanceGoal Create(
        Guid employeeId,
        Guid reviewId,
        string title,
        string description,
        decimal targetValue,
        string unitOfMeasure,
        DateTime dueDate,
        decimal weight,
        Guid tenantId)
    {
        return new PerformanceGoal
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            PerformanceReviewId = reviewId,
            GoalTitle = title,
            GoalDescription = description,
            TargetValue = targetValue,
            UnitOfMeasure = unitOfMeasure,
            StartDate = DateTime.UtcNow,
            DueDate = dueDate,
            Weight = weight,
            Status = PerformanceGoalStatus.Active,
            TenantId = tenantId,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update goal progress
    /// </summary>
    public void UpdateProgress(decimal actualValue)
    {
        ActualValue = actualValue;
        UpdatedOnUtc = DateTime.UtcNow;

        if (actualValue >= TargetValue)
        {
            Status = PerformanceGoalStatus.Completed;
            
            AddDomainEvent(new PerformanceGoalCompletedEvent
            {
                GoalId = Id,
                EmployeeId = EmployeeId,
                CompletionPercentage = GetCompletionPercentage(),
                TenantId = TenantId
            });
        }
    }

    /// <summary>
    /// Get goal completion percentage
    /// </summary>
    public decimal GetCompletionPercentage()
    {
        if (TargetValue == 0) return 0;
        return (ActualValue / TargetValue) * 100m;
    }
}
