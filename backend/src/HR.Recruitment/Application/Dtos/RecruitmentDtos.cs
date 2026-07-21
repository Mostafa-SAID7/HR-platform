namespace HR.Recruitment.Application.Dtos;

// Job Posting DTOs
public record CreateJobPostingRequest(
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax);

public record UpdateJobPostingRequest(
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax);

public record JobPostingDto(
    Guid Id,
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string Status,
    DateTime PostedDate,
    DateTime? ClosedDate,
    int ViewCount,
    int ApplicationCount) : IMapFrom<JobPosting>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<JobPosting, JobPostingDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.ApplicationCount, src => src.Applications.Count);
}

public record JobPostingDetailDto(
    Guid Id,
    string Title,
    string Description,
    string Department,
    List<string> RequiredSkills,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string Status,
    DateTime PostedDate,
    DateTime? ClosedDate,
    int ViewCount,
    List<JobApplicationDto> Applications) : IMapFrom<JobPosting>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<JobPosting, JobPostingDetailDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
}

public record JobPostingListDto(
    Guid Id,
    string Title,
    string Department,
    string Status,
    DateTime PostedDate,
    int ApplicationCount);

public record JobPostingFilterDto(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Department = null,
    string? Status = null);

// Job Application DTOs
public record ApplyJobRequest(
    string CandidateName,
    string CandidateEmail,
    string CandidatePhone,
    string Resume,
    string? CoverLetter);

public record JobApplicationDto(
    Guid Id,
    Guid JobPostingId,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string CandidatePhone,
    DateTime AppliedDate,
    string Status,
    int? Rating,
    string? FeedbackNotes) : IMapFrom<JobApplication>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<JobApplication, JobApplicationDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
}

public record JobApplicationDetailDto(
    Guid Id,
    Guid JobPostingId,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string CandidatePhone,
    string Resume,
    string CoverLetter,
    DateTime AppliedDate,
    string Status,
    int? Rating,
    string? FeedbackNotes,
    List<InterviewScheduleDto> InterviewSchedules) : IMapFrom<JobApplication>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<JobApplication, JobApplicationDetailDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
}

public record UpdateApplicationStatusRequest(
    string Status,
    string? Notes);

public record SetApplicationRatingRequest(
    int Rating,
    string Notes);

public record JobApplicationFilterDto(
    Guid JobPostingId,
    int Page = 1,
    int PageSize = 10,
    string? Status = null);

// Interview Schedule DTOs
public record ScheduleInterviewRequest(
    DateTime ScheduledDate,
    DateTime ScheduledEndTime,
    string InterviewType,
    string InterviewerName,
    string InterviewerEmail,
    string? MeetingLink,
    string? Location);

public record CompleteInterviewRequest(
    string Feedback);

public record InterviewScheduleDto(
    Guid Id,
    Guid JobApplicationId,
    DateTime ScheduledDate,
    DateTime ScheduledEndTime,
    string InterviewType,
    string InterviewerName,
    string InterviewerEmail,
    string Status,
    string? MeetingLink,
    string? Location,
    string? Feedback) : IMapFrom<InterviewSchedule>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<InterviewSchedule, InterviewScheduleDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
}

// Offer Letter DTOs
public record CreateOfferLetterRequest(
    Guid JobApplicationId,
    Guid CandidateId,
    decimal OfferSalary,
    string Department,
    string Position,
    DateTime ProposedStartDate,
    string? Terms);

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
    DateTime? RejectedDate) : IMapFrom<OfferLetter>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<OfferLetter, OfferLetterDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
}

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
    DateTime? RejectedDate) : IMapFrom<OfferLetter>
{
    public void Mapping(TypeAdapterConfig config) =>
        config.NewConfig<OfferLetter, OfferLetterDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
}

public record AcceptOfferRequest;
public record RejectOfferRequest(string Reason);

// Candidate DTOs
public record CandidateDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime RegisteredDate);

public record CandidateFilterDto(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null);
