namespace HR.Recruitment.Application.Dtos.JobApplication;

public record SetApplicationRatingRequest(
    int Rating,
    string Notes);
