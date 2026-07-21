namespace HR.Recruitment.Application.Mappings;

using Mapster;
using HR.Recruitment.Domain.Aggregates;
using HR.Recruitment.Application.Dtos.InterviewSchedule;

/// <summary>
/// Mapping configuration for InterviewSchedule DTOs
/// Centralizes all Mapster configurations for interview schedule related mappings
/// </summary>
public class InterviewScheduleMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // InterviewSchedule entity to DTO
        config.NewConfig<InterviewSchedule, InterviewScheduleDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
