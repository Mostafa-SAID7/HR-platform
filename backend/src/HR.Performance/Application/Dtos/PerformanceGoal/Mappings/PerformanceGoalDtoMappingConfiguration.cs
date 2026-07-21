namespace HR.Performance.Application.Dtos.PerformanceGoal.Mappings;

using HR.Performance.Domain;

/// <summary>
/// Centralized mapping configuration for PerformanceGoal DTOs.
/// Organized by aggregate to follow SOLID principles.
/// </summary>
public static class PerformanceGoalDtoMappingConfiguration
{
    /// <summary>
    /// Maps PerformanceGoal domain entity to PerformanceGoalDto.
    /// </summary>
    public static PerformanceGoalDto ToDto(this Goal goal)
    {
        return new PerformanceGoalDto
        {
            Id = goal.Id,
            GoalTitle = goal.GoalTitle,
            GoalDescription = goal.GoalDescription,
            TargetValue = goal.TargetValue,
            ActualValue = goal.ActualValue,
            UnitOfMeasure = goal.UnitOfMeasure,
            StartDate = goal.StartDate,
            DueDate = goal.DueDate,
            Status = goal.Status,
            Weight = goal.Weight,
            CompletionPercentage = goal.CompletionPercentage
        };
    }
}
