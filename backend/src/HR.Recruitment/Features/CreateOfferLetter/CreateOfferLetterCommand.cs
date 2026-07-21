namespace HR.Recruitment.Features.CreateOfferLetter;

/// <summary>
/// Create an offer letter for a candidate
/// </summary>
public record CreateOfferLetterCommand(
    CreateOfferLetterRequest Request,
    Guid TenantId) : ICommand<OfferLetterDto>;
