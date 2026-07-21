namespace HR.Recruitment.Application.Dtos.JobApplication;

public record ApplyJobRequest(
    string CandidateName,
    string CandidateEmail,
    string CandidatePhone,
    string Resume,
    string? CoverLetter);
