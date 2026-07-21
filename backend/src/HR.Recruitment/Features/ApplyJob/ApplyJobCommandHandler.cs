namespace HR.Recruitment.Features.ApplyJob;

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
