namespace HR.Recruitment.Application.Dtos.JobApplication;

public record UpdateApplicationStatusRequest(
    string Status,
    string? Notes);
