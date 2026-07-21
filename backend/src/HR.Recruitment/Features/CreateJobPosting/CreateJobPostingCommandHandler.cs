namespace HR.Recruitment.Features.CreateJobPosting;

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
