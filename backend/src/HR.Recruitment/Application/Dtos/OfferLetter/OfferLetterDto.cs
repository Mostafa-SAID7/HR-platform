namespace HR.Recruitment.Application.Dtos.OfferLetter;

/// <summary>
/// DTO representing an offer letter summary
/// </summary>
public record OfferLetterDto(
    Guid Id,
    Guid JobApplicationId,
    Guid CandidateId,
    DateTime CreatedDate,
    DateTime? ExpiryDate,
    decimal OfferSalary,
    string Department,
    string Position,
    DateTime ProposedStartDate,
    string Status,
    DateTime? AcceptedDate,
    DateTime? RejectedDate);
