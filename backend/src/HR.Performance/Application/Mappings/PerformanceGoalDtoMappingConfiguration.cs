namespace HR.Performance.Application.Mappings;

using HR.Performance.Domain;
using HR.Performance.Application.Dtos.PerformanceGoal;

/// <summary>
/// Mapping configuration for PerformanceGoal DTOs.
/// </summary>
public static class PerformanceGoalDtoMappingConfiguration
{
    /// <summary>
    /// Maps PerformanceGoal domain entity to PerformanceGoalDto.
    /// </summary>
    public static PerformanceGoalDto ToDto(this PerformanceGoal goal)
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
            CompletionPercentage = goal.GetCompletionPercentage()
        };
    }
}
