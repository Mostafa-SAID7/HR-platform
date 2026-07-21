namespace HR.Recruitment.Application.Mappings;

using Mapster;
using HR.Recruitment.Domain.Aggregates;
using HR.Recruitment.Application.Dtos.JobPosting;

/// <summary>
/// Mapping configuration for JobPosting DTOs
/// Centralizes all Mapster configurations for job posting related mappings
/// </summary>
public class JobPostingMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // JobPosting aggregate to summary DTO
        config.NewConfig<JobPosting, JobPostingDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ApplicationCount, src => src.Applications.Count);

        // JobPosting aggregate to detailed DTO
        config.NewConfig<JobPosting, JobPostingDetailDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        // JobPosting aggregate to list DTO
        config.NewConfig<JobPosting, JobPostingListDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ApplicationCount, src => src.Applications.Count);
    }
}
