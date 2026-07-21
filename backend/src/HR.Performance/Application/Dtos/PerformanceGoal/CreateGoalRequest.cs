namespace HR.Performance.Application.Dtos.PerformanceGoal;

/// <summary>
/// Create goal request DTO.
/// </summary>
public record CreateGoalRequest
{
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Weight { get; set; }
}
