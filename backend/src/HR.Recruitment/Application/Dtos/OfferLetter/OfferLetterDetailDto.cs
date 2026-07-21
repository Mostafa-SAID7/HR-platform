namespace HR.Recruitment.Application.Dtos.OfferLetter;

/// <summary>
/// DTO representing detailed offer letter information
/// </summary>
public record OfferLetterDetailDto(
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
    string? Terms,
    DateTime? AcceptedDate,
    DateTime? RejectedDate);
