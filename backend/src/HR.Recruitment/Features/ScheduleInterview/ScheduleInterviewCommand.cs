namespace HR.Recruitment.Features.ScheduleInterview;

/// <summary>
/// Schedule an interview for a job application
/// </summary>
public record ScheduleInterviewCommand(
    Guid JobApplicationId,
    ScheduleInterviewRequest Request,
    Guid TenantId) : ICommand<InterviewScheduleDto>;

/// <summary>
/// Validator for ScheduleInterviewCommand
/// </summary>
public class ScheduleInterviewCommandValidator : AbstractValidator<ScheduleInterviewCommand>
{
    public ScheduleInterviewCommandValidator()
    {
        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("Job application ID is required");

        RuleFor(x => x.Request.ScheduledDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Interview must be scheduled in the future");

        RuleFor(x => x.Request.ScheduledEndTime)
            .GreaterThan(x => x.Request.ScheduledDate)
            .WithMessage("End time must be after start time");

        RuleFor(x => x.Request.InterviewType)
            .NotEmpty().WithMessage("Interview type is required")
            .Must(x => new[] { "Phone", "Video", "In-Person" }.Contains(x))
            .WithMessage("Invalid interview type");

        RuleFor(x => x.Request.InterviewerName)
            .NotEmpty().WithMessage("Interviewer name is required");

        RuleFor(x => x.Request.InterviewerEmail)
            .NotEmpty().WithMessage("Interviewer email is required")
            .EmailAddress().WithMessage("Valid email is required");
    }
}

/// <summary>
/// Handler for ScheduleInterviewCommand
/// </summary>
public class ScheduleInterviewCommandHandler : ICommandHandler<ScheduleInterviewCommand, InterviewScheduleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<JobApplication> _jobApplicationRepository;
    private readonly IRepository<InterviewSchedule> _interviewRepository;

    public ScheduleInterviewCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<JobApplication> jobApplicationRepository,
        IRepository<InterviewSchedule> interviewRepository)
    {
        _unitOfWork = unitOfWork;
        _jobApplicationRepository = jobApplicationRepository;
        _interviewRepository = interviewRepository;
    }

    public async Task<InterviewScheduleDto> Handle(ScheduleInterviewCommand request, CancellationToken cancellationToken)
    {
        // Verify job application exists
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId, cancellationToken);
        if (jobApplication == null)
            throw new DomainException("Job application not found");

        // Create interview schedule
        var interview = InterviewSchedule.Create(
            request.JobApplicationId,
            request.Request.ScheduledDate,
            request.Request.ScheduledEndTime,
            request.Request.InterviewType,
            request.Request.InterviewerName,
            request.Request.InterviewerEmail,
            request.Request.MeetingLink,
            request.Request.Location);

        // Add to repository
        await _interviewRepository.AddAsync(interview, cancellationToken);

        // Update application status
        jobApplication.UpdateStatus(ApplicationStatus.Shortlisted, "Interview scheduled");

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return interview.Adapt<InterviewScheduleDto>();
    }
}
