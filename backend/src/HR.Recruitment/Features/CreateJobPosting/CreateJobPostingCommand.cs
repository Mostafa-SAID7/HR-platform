namespace HR.Recruitment.Features.CreateJobPosting;

/// <summary>
/// Create a new job posting
/// </summary>
public record CreateJobPostingCommand(
    CreateJobPostingRequest Request,
    Guid TenantId) : ICommand<JobPostingDto>;

/// <summary>
/// Validator for CreateJobPostingCommand
/// </summary>
public class CreateJobPostingCommandValidator : AbstractValidator<CreateJobPostingCommand>
{
    public CreateJobPostingCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Job description is required")
            .MinimumLength(50).WithMessage("Job description must be at least 50 characters");

        RuleFor(x => x.Request.Department)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Request.RequiredSkills)
            .NotEmpty().WithMessage("At least one required skill must be specified");

        RuleFor(x => x.Request.SalaryMin)
            .GreaterThan(0).When(x => x.Request.SalaryMin.HasValue).WithMessage("Minimum salary must be greater than 0");

        RuleFor(x => x.Request.SalaryMax)
            .GreaterThan(x => x.Request.SalaryMin).When(x => x.Request.SalaryMin.HasValue && x.Request.SalaryMax.HasValue)
            .WithMessage("Maximum salary must be greater than minimum salary");
    }
}

/// <summary>
/// Handler for CreateJobPostingCommand
/// </summary>
public class CreateJobPostingCommandHandler : ICommandHandler<CreateJobPostingCommand, JobPostingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<JobPosting> _jobPostingRepository;

    public CreateJobPostingCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<JobPosting> jobPostingRepository)
    {
        _unitOfWork = unitOfWork;
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<JobPostingDto> Handle(CreateJobPostingCommand request, CancellationToken cancellationToken)
    {
        // Create the job posting aggregate
        var jobPosting = JobPosting.Create(
            request.Request.Title,
            request.Request.Description,
            request.Request.Department,
            request.Request.RequiredSkills,
            request.Request.SalaryMin,
            request.Request.SalaryMax);

        // Add to repository
        await _jobPostingRepository.AddAsync(jobPosting, cancellationToken);

        // Save changes (including outbox messages)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return jobPosting.Adapt<JobPostingDto>();
    }
}
