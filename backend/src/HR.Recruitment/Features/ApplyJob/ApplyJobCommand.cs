namespace HR.Recruitment.Features.ApplyJob;

/// <summary>
/// Apply for a job position
/// </summary>
public record ApplyJobCommand(
    Guid JobPostingId,
    ApplyJobRequest Request,
    Guid TenantId) : ICommand<JobApplicationDto>;

/// <summary>
/// Validator for ApplyJobCommand
/// </summary>
public class ApplyJobCommandValidator : AbstractValidator<ApplyJobCommand>
{
    public ApplyJobCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty().WithMessage("Job posting ID is required");

        RuleFor(x => x.Request.CandidateName)
            .NotEmpty().WithMessage("Candidate name is required")
            .MaximumLength(200).WithMessage("Candidate name must not exceed 200 characters");

        RuleFor(x => x.Request.CandidateEmail)
            .NotEmpty().WithMessage("Candidate email is required")
            .EmailAddress().WithMessage("Valid email is required");

        RuleFor(x => x.Request.CandidatePhone)
            .NotEmpty().WithMessage("Candidate phone is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.Request.Resume)
            .NotEmpty().WithMessage("Resume is required");
    }
}

/// <summary>
/// Handler for ApplyJobCommand
/// </summary>
public class ApplyJobCommandHandler : ICommandHandler<ApplyJobCommand, JobApplicationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<JobPosting> _jobPostingRepository;
    private readonly IRepository<JobApplication> _jobApplicationRepository;

    public ApplyJobCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<JobPosting> jobPostingRepository,
        IRepository<JobApplication> jobApplicationRepository)
    {
        _unitOfWork = unitOfWork;
        _jobPostingRepository = jobPostingRepository;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationDto> Handle(ApplyJobCommand request, CancellationToken cancellationToken)
    {
        // Verify job posting exists and is open
        var jobPosting = await _jobPostingRepository.GetByIdAsync(request.JobPostingId, cancellationToken);
        if (jobPosting == null)
            throw new DomainException("Job posting not found");

        if (jobPosting.Status != JobPostingStatus.Open)
            throw new DomainException("Job posting is not open for applications");

        // Create job application
        var application = JobApplication.Create(
            request.JobPostingId,
            Guid.NewGuid(), // In real app, get from current user context
            request.Request.CandidateName,
            request.Request.CandidateEmail,
            request.Request.CandidatePhone,
            request.Request.Resume,
            request.Request.CoverLetter);

        // Add to repository
        await _jobApplicationRepository.AddAsync(application, cancellationToken);

        // Increment view count for job posting
        jobPosting.IncrementViewCount();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return application.Adapt<JobApplicationDto>();
    }
}
