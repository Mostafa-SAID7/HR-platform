namespace HR.Recruitment.Application.Dtos.Candidate;

public record CandidateFilterDto(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null);
