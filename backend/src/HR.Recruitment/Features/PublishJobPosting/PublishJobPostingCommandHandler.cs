namespace HR.Recruitment.Features.PublishJobPosting;

/// <summary>
/// Handler for PublishJobPostingCommand
/// </summary>
public class PublishJobPostingCommandHandler : ICommandHandler<PublishJobPostingCommand, JobPostingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<JobPosting> _jobPostingRepository;

    public PublishJobPostingCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<JobPosting> jobPostingRepository)
    {
        _unitOfWork = unitOfWork;
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<JobPostingDto> Handle(PublishJobPostingCommand request, CancellationToken cancellationToken)
    {
        // Get job posting
        var jobPosting = await _jobPostingRepository.GetByIdAsync(request.JobPostingId, cancellationToken);
        if (jobPosting == null)
            throw new DomainException("Job posting not found");

        // Publish the posting
        jobPosting.Publish();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return jobPosting.Adapt<JobPostingDto>();
    }
}
