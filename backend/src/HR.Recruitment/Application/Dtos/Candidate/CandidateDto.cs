namespace HR.Recruitment.Application.Dtos.Candidate;

public record CandidateDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime RegisteredDate);
