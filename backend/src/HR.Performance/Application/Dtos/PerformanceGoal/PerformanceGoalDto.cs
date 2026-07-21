namespace HR.Performance.Application.Dtos.PerformanceGoal;

/// <summary>
/// Performance goal DTO.
/// </summary>
public record PerformanceGoalDto
{
    public Guid Id { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public string GoalDescription { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal ActualValue { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal CompletionPercentage { get; set; }
}
