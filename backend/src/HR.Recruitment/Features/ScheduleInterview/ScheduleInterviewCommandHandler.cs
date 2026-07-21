namespace HR.Recruitment.Features.ScheduleInterview;

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
