namespace HR.Recruitment.Application.Mappings;

using Mapster;
using HR.Recruitment.Domain.Aggregates;
using HR.Recruitment.Application.Dtos.JobApplication;

/// <summary>
/// Mapping configuration for JobApplication DTOs
/// Centralizes all Mapster configurations for job application related mappings
/// </summary>
public class JobApplicationMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // JobApplication entity to summary DTO
        config.NewConfig<JobApplication, JobApplicationDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        // JobApplication entity to detailed DTO
        config.NewConfig<JobApplication, JobApplicationDetailDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
