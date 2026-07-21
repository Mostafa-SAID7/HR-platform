namespace HR.Recruitment.Application.Mappings;

using Mapster;
using HR.Recruitment.Domain.Aggregates;
using HR.Recruitment.Application.Dtos.OfferLetter;

/// <summary>
/// Mapping configuration for OfferLetter DTOs
/// Centralizes all Mapster configurations for offer letter related mappings
/// </summary>
public class OfferLetterMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // OfferLetter entity to summary DTO
        config.NewConfig<OfferLetter, OfferLetterDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        // OfferLetter entity to detailed DTO
        config.NewConfig<OfferLetter, OfferLetterDetailDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
