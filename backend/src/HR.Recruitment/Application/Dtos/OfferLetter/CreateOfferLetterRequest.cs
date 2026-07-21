namespace HR.Recruitment.Application.Dtos.OfferLetter;

public record CreateOfferLetterRequest(
    Guid JobApplicationId,
    Guid CandidateId,
    decimal OfferSalary,
    string Department,
    string Position,
    DateTime ProposedStartDate,
    string? Terms);
